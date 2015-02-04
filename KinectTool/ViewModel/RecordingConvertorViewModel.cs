using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Debugify;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using KinectTool.Model.Kinect;
using KinectTool.Redux;
using Meshy;
using Microsoft.Kinect;
using Microsoft.Win32;
using SharpDX;
using SharpDX.Collections;

namespace KinectTool.ViewModel
{

    internal delegate AnimationKey KinectFrameToKeyFrameConversionMethod(KinectFrame f);

    public class RecordingConvertorViewModel:ViewModelBase
    {
        #region Commands

        private RelayCommand<AnimationJoint> _setJointCommand;
        public RelayCommand<AnimationJoint> SetJointCommand
        {
            get
            {
                return (_setJointCommand ?? (_setJointCommand = new RelayCommand<AnimationJoint>(SetJoint)));
            }
        }
        
        private RelayCommand _saveToXmlCommand;
        public RelayCommand SaveToXmlCommand
        {
            get
            {
                return (_saveToXmlCommand ?? (_saveToXmlCommand = new RelayCommand(SaveToXml)));
            }
        }

        private RelayCommand _loadFromXmlCommand;
        public RelayCommand LoadXmlCommand
        {
            get
            {
                return (_loadFromXmlCommand ?? (_loadFromXmlCommand = new RelayCommand(LoadFromXml)));
            }
        }

        #endregion

        #region General Properties
        private AnimationClip _clip;
        public AnimationClip Clip {
            get { return _clip; }
            set
            {
                _clip = value;
                RaisePropertyChanged();
            }
        }

        public bool Hierarchial { get; set; }

        private int _frameInterval = 2;

        public int FrameInterval
        {
            get { return _frameInterval; }
            set
            {
                _frameInterval = Math.Abs(value);
                RaisePropertyChanged();
            }
        }


       
        #endregion

        #region Joint Setup Properties

        private ObservableDictionary<string,AnimationJoint> _meshJoints = new ObservableDictionary<string,AnimationJoint>();
        public ObservableDictionary<string, AnimationJoint> MeshJoints
        {
            get { return _meshJoints; }
            set { _meshJoints = value; }
        }

        private ObservableDictionary<JointType, AnimationJoint> _linkedJoints = new ObservableDictionary<JointType, AnimationJoint>();
        public ObservableDictionary<JointType, AnimationJoint> LinkedJoints {
            get { return _linkedJoints; }
            set {_linkedJoints = value;}
        }

        public AnimationJoint SelectedJoint
        {
            get { return LinkedJoints[SelectedType]; }
        }

        private JointType _selectedType;
        public JointType SelectedType
        {
            get { return _selectedType; }
            set
            {
                _selectedType = value;
                RaisePropertyChanged();
                RaisePropertyChanged("SelectedJoint");
            }
        }
        #endregion

        private AnimationData _meshAnimationData;
        private MeshData _meshData;
        private KinectRecording _recording;

        private readonly KinectFrameToKeyFrameConversionMethod _frameToKeyFrameMethod;

        public RecordingConvertorViewModel()
        {
            Clip = new AnimationClip();

            _frameToKeyFrameMethod += KinectFrameToKeyFrame;
        }

        public void SetRecording(KinectRecording r)
        {
            _recording = r;
        }
        public void SetMeshAnimationData(MeshData m,AnimationData d)
        {
            _meshData = m;
            _meshAnimationData = d;

            MeshJoints.Clear();
            foreach (var joint in d.Bones)
            {
                MeshJoints.Add(joint.Name,joint);
            }

            LinkedJoints.Clear();

            var jointOrder = new List<JointType>
            {
                JointType.HipCenter,
                JointType.HipLeft,
                JointType.KneeLeft,
                JointType.AnkleLeft,
                JointType.FootLeft,
                JointType.HipRight,
                JointType.KneeRight,
                JointType.AnkleRight,
                JointType.FootRight,
                JointType.Spine,
                JointType.ShoulderCenter,
                JointType.Head,
                JointType.ShoulderLeft,
                JointType.ElbowLeft,
                JointType.WristLeft,
                JointType.HandLeft,
                JointType.ShoulderRight,
                JointType.ElbowRight,
                JointType.WristRight,
                JointType.HandRight
            };

            foreach (var j in jointOrder)
                LinkedJoints.Add(j, new AnimationJoint());

            RaisePropertyChanged("LinkedJoints");
            RaisePropertyChanged("MeshJoints");


            #if (DEBUG)
            {
                Load("Resources/Links/KnightLinks.xml");
            }
            #endif
        }
        private void SetJoint(AnimationJoint joint)
        {
            ReduxManager.InsertInUndoRedo(new SetJoint(LinkedJoints,joint,SelectedType));
            RaisePropertyChanged("SelectedJoint");
        }

        #region Conversion
        public AnimationClip ConvertRecording()
        {
            //Check if a converison method is set
            if (_frameToKeyFrameMethod == null)
            {
                Debug.Log(LogLevel.Warning, "No conversion method set");
                return null;
            }

            //make a copy of the active clip
            var newClip = Clip.Copy();

            //Clamp Interval
            if (FrameInterval >= (_recording.Frames))
                FrameInterval = _recording.Frames - 1;

            var div = _recording.Frames/FrameInterval;
            var tickInterval = newClip.Duration / div;

            //In the end Tick == Duration
            var tick = -tickInterval;

            //Go through all the frames
            for (var i = 0; i < _recording.Frames; ++i)
            {
                if (i % FrameInterval != 0) continue;
                
                //Create a keyFrame
                var frame = _recording.GetFrame(i);
                var keyFrame = _frameToKeyFrameMethod(frame);

                tick += tickInterval;
                keyFrame.Tick = tick;

                newClip.Keys.Add(keyFrame);
            }

            //New animation clip is created
            return newClip;
        }

        private AnimationKey KinectFrameToKeyFrame_Old(KinectFrame frame)
        {
            var keyFrame = new AnimationKey();

            //Go through all the joints in a single frame 
            //and add the rotation in that frame to a key frame for the animation
            foreach (var kinectjoint in frame.Joints)
            {
                LinkedJoints[kinectjoint.Key].LocalMatrix = kinectjoint.Value.HierarchicalRotation;
            }

            var mat = Matrix.Identity;
            foreach (var animationJoint in LinkedJoints.Values)
            {
                if (animationJoint.IsValid)
                {
                    mat = animationJoint.LocalMatrix;
                    keyFrame.BoneTransforms.Add(mat);
                }
                else
                {
                    keyFrame.BoneTransforms.Add(mat);

                }

            }

            return keyFrame;
        }

        private AnimationKey KinectFrameToKeyFrame(KinectFrame frame)
        {
            var f = new AnimationKey();

            var mat = Matrix.Identity;
            var prev = Matrix.Identity;

            foreach (var j in frame.Joints.Where(j => LinkedJoints[j.Key].IsValid))
            {
                LinkedJoints[j.Key].LocalMatrix = j.Value.HierarchicalRotation;
            }

            foreach (var joint in LinkedJoints.Values)
            {
                if (!joint.IsValid)
                {
                    f.BoneTransforms.Add(prev);
                    break;
                }

                mat = joint.LocalMatrix;

                f.BoneTransforms.Add(mat);
                prev = mat;
            }
            return f;
        }
       
        #endregion

        /// <summary>
        /// Export the KinectJoint - MeshJoint Link to an xml file
        /// </summary>
        private void SaveToXml()
        {
            //Choose file location
            var dlg = new SaveFileDialog()
            {
                FileName = "",
                DefaultExt = ".xml",
                Filter = "Xml documents (.xml)|*.xml"
            };

            var r = dlg.ShowDialog();
            if (r != true)
                return;


            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));


            //doc.Add(new XElement("Mesh",_meshData.Name));

            var links = new XElement("Links");

            foreach (KeyValuePair<JointType, AnimationJoint> joint in LinkedJoints)
            {
                links.Add(new XElement("Link",
                    new XElement("KinectJoint",joint.Key),
                    new XElement("MeshJoint",joint.Value.Name)));
            }

            doc.Add(links);
            doc.Save(dlg.FileName);
        }

        /// <summary>
        /// Import the KinectJoint - MeshJoint Link from an xml file
        /// </summary>
        private void LoadFromXml()
        {
            //Choose file
            var dlg = new OpenFileDialog()
            {
                FileName = "",
                DefaultExt = ".xml",
                Filter = "Xml documents (.xml)|*.xml"
            };

            var r = dlg.ShowDialog();
            if (r != true)
                return;

            Load(dlg.FileName);
        }

        private void Load(string file)
        {
            var root = XElement.Load(file);

            foreach (var x in root.Elements())
            {
                JointType kiJoint;
                Enum.TryParse(x.Element("KinectJoint").Value, out kiJoint);
                var meshJoint = x.Element("MeshJoint").Value;

                if (!meshJoint.Equals("Not Set"))
                {
                    if (MeshJoints.ContainsKey(meshJoint))
                        LinkedJoints[kiJoint] = MeshJoints[meshJoint];
                }
                else
                {
                    LinkedJoints[kiJoint] = new AnimationJoint();
                }
            }
        }
      
        #region CanApplyCheck
        private RelayCommand<Window> _ApplyChangesCommand;

        public RelayCommand<Window> ApplyChangesCommmand
        {
            get
            {
                return _ApplyChangesCommand ??
                       (_ApplyChangesCommand = new RelayCommand<Window>(ApplyChanges, CanApplyChanges));
            }
        }

        private void ApplyChanges(Window w)
        {
            w.DialogResult = true;
        }

        private bool CanApplyChanges(Window w)
        {
            if (Clip == null)
                return false;

            if (string.IsNullOrWhiteSpace(Clip.Name))
                return false;

            if (float.IsNaN(Clip.Duration))
                return false;

            if (float.IsNaN(Clip.TicksPerSecond))
                return false;

            if (FrameInterval == 0)
                return false;
            return true;
        }
        #endregion

    }

}
