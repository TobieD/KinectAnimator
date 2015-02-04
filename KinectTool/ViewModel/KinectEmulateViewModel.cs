using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Debugify;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KinectTool.Drawing;
using KinectTool.Drawing.Prefabs;
using KinectTool.Model.Kinect;
using KinectTool.Redux;
using KinectTool.View;
using Meshy;
using Meshy.Interfaces;
using Microsoft.Win32;
using SharpDX.Collections;

namespace KinectTool.ViewModel
{
    public class KinectEmulateViewModel : ViewModelBase
    {
        #region Commands

        /// <summary>
        /// Load a 3D Model
        /// </summary>
        private RelayCommand _removeAnimationCommand;

        public RelayCommand RemoveAnimationCommand
        {
            get { return (_loadModelCommand ?? (_loadModelCommand = new RelayCommand(RemoveAnimation))); }
        }


        /// <summary>
        /// Load a 3D Model
        /// </summary>
        private RelayCommand _loadModelCommand;

        public RelayCommand LoadModelCommand
        {
            get { return (_removeAnimationCommand ?? (_removeAnimationCommand = new RelayCommand(LoadModel))); }
        }

        /// <summary>
        /// Save a 3D Model
        /// </summary>
        private RelayCommand _saveModelCommand;

        public RelayCommand SaveModelCommand
        {
            get { return (_saveModelCommand ?? (_saveModelCommand = new RelayCommand(SaveModel))); }
        }

        /// <summary>
        /// Open a new Recording
        /// </summary>
        private RelayCommand _openRecordingCommand;

        public RelayCommand OpenRecordingCommand
        {
            get { return (_openRecordingCommand ?? (_openRecordingCommand = new RelayCommand(LoadRecording))); }
        }

        /// <summary>
        /// Save a Recording
        /// </summary>
        private RelayCommand _saveRecordingCommand;

        public RelayCommand SaveRecordingCommand
        {
            get { return (_saveRecordingCommand ?? 
                (_saveRecordingCommand = new RelayCommand(SaveRecording))); }
        }

        /// <summary>
        /// start playing the recording from frame 0
        /// </summary>
        private RelayCommand _startEmulationCommand;

        public RelayCommand StartEmulationCommand
        {
            get
            {
                return (_startEmulationCommand ??
                        (_startEmulationCommand = new RelayCommand(() => StartEmulation(false))));
            }
        }

        /// <summary>
        /// stop playing the recording and set to frame 0
        /// </summary>
        private RelayCommand _stopEmulationCommand;

        public RelayCommand StopEmulationCommand
        {
            get
            {
                return (_stopEmulationCommand ?? 
                    (_stopEmulationCommand = new RelayCommand(() => StopEmulation(true))));
            }
        }

        /// <summary>
        /// Convert current Recording to an animation
        /// </summary>
        private RelayCommand _recordingToAnimationCommand;

        public RelayCommand RecordingToAnimationCommand
        {
            get
            {
                return (_recordingToAnimationCommand ?? (_recordingToAnimationCommand = new RelayCommand(ConvertRecordingToAnimation)));
            }
        }

        /// <summary>
        /// Remove a single frame
        /// </summary>
        private RelayCommand _removeFrameCommand;

        public RelayCommand RemoveFrameCommand
        {
            get
            {
                return (_removeFrameCommand ?? (_removeFrameCommand = new RelayCommand(RemoveFrame)));
            }
        }

        private RelayCommand _cleanupFramesCommand;

        /// <summary>
        /// Remove all frames where joints aren't correcty tracked
        /// </summary>
        public RelayCommand CleanUpFramesCommand
        {
            get
            {
                return (_cleanupFramesCommand ?? (_cleanupFramesCommand = new RelayCommand(CleanUpFrames)));
            }
        }

       

        #endregion

        #region Bindable properties
        //Used for drawing
        public DrawingImage Display { get; set; }

        private Dx10Viewport _viewport;
        public Dx10Viewport Viewport {
            get { return _viewport; }
            set
            {
                _viewport = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Location of the current frame in the recording
        /// </summary>
        private int _currentIndex = 0;

        public int CurrentFrameIndex
        {
            get
            {
                //Return index or reset to 0 when end is reached
                return _currentIndex >= TotalFrames ? 0 : _currentIndex;
            }
            set
            {
                _currentIndex = value;
                _onFrameIndexChange();
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Total frames in the recording
        /// </summary>
        public int TotalFrames
        {
            get { return _recording != null ? _recording.Frames : 0; }
        }

        /// <summary>
        /// Is the Emulator playing a recording
        /// </summary>
        public bool IsEmulating { get; set; }

        /// <summary>
        /// Is the emulation paused
        /// </summary>
        public bool Paused { get; set; }

        public bool HasRecording
        {
            get { return _recording != null; }
        }

        public bool HasMesh
        {
            get { return _mesh != null; }
        }

        #region Animation
        
        /// <summary>
        /// When a new model is loaded in, save all the animations in a dictionary
        /// </summary>
        private ObservableDictionary<string, AnimationClip> _animationClips = new ObservableDictionary<string, AnimationClip>();

        public ObservableDictionary<string, AnimationClip> AnimationClips 
        {
            get { return _animationClips; }
            set
            {
                _animationClips = value;
                RaisePropertyChanged();
            }
            
        }

        public AnimationClip CurrentAnimationClip
        {
            get { return (_mesh != null && _mesh.Animated) ? _mesh.Animator.CurrentClip : null; }
            set
            {
                if (_mesh.Animated)
                {
                    _mesh.Animator.Reset();
                    _mesh.Animator.SetAnimation(value);
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        
        #endregion

        #region Fields
        private bool _stopped;

        //Current Open Recording
        private string _currentFile;
        private KinectRecording _recording;

        //Used for visualizing the frame
        private readonly FrameVisualizer _frameDrawer;
        private readonly Action _onFrameIndexChange;

        private CustomMesh _mesh;

        #endregion

        public KinectEmulateViewModel()
        {
            //Create a drawinggroup for the 2D image in the viewport
            var dwGroup = new DrawingGroup();
            Display = new DrawingImage(dwGroup);

            //Create a viewPort for 3D
            Viewport = new Dx10Viewport();
            //Viewport.OnDrop += OnDrop;

            //init visualizer
            _frameDrawer = new FrameVisualizer(dwGroup);
            //Default 3D DrawMode

            //When Frame index is changed => Draw the new frame
            _onFrameIndexChange += DrawFrame;
            IsEmulating = false;

            #if DEBUG
            Viewport.OnLoad += () =>
            {
                DeserializeRecordingFile("Resources/Recording/Wave.kr");
                OpenModelFile("Resources/Mesh/Knight.ovm");
            };
            #endif
        }

        #region Event

        public void OnDrop(DragEventArgs e)
        {
            //Is there data present
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            //Check what the first file is 
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var extension = Path.GetExtension(files[0]);

            if (extension.ToLower() == ".kr")
                DeserializeRecordingFile(files[0]);
            else
                OpenModelFile(files[0]);
        }

        private void UpdateAnimationList()
        {
            //Reset 
            _animationClips = null;
            CurrentAnimationClip = null;
            _animationClips = new ObservableDictionary<string, AnimationClip>();

            //Add all the animations of the model to the ComboBox
            if (_mesh.Animated)
            {
                foreach (var anim in _mesh.AnimationData.Animations)
                    _animationClips.Add(anim.Name, anim);

                if (_mesh.AnimationData.Animations.Count != 0)
                {
                    CurrentAnimationClip = _mesh.AnimationData.Animations[0];
                    RaisePropertyChanged("CurrentAnimationClip");
                }
            }

            
            RaisePropertyChanged("AnimationClips");
            RaisePropertyChanged("HasMesh");
        }

        public void DrawFrame()
        {
            //Don't do anything if no recording
            if (_recording == null)
                return;

            //get the current frame from the recording
            var currFrame = _recording.GetFrame(CurrentFrameIndex);

            //draw it
            _frameDrawer.Draw(currFrame);
        }

        #endregion

        #region Actions
        private void RemoveFrame()
        {
            if(_recording == null)
                return;

            var temp = CurrentFrameIndex;
            ReduxManager.InsertInUndoRedo(new RemoveSingleFrame(_recording,CurrentFrameIndex));
            CurrentFrameIndex = temp;

        }
        private void CleanUpFrames()
        {
            if (_recording == null)
                return;

            var temp = CurrentFrameIndex;
            ReduxManager.InsertInUndoRedo(new CleanUpFrames(_recording));
            CurrentFrameIndex = temp;
        }

        private void ConvertRecordingToAnimation()
        {
            if (_recording == null || _mesh == null || _mesh.AnimationData == null)
                return;

            //Create window
            var settings = new RecordingConvertorView();
            var temp = settings.DataContext as ViewModelLocator;
            var convertor = temp.RecordingConvertor;

            if (convertor == null)
                return;

            convertor.SetMeshAnimationData(_mesh.MeshData,_mesh.AnimationData);
            convertor.SetRecording(_recording);

            var result =  settings.ShowDialog();
            if(!result.Value)
               return;

            settings.Close();
            
            //Add to the current Mesh
            var newAnimation = convertor.ConvertRecording();
            if (newAnimation != null)
            {
                ReduxManager.InsertInUndoRedo(new AddNewAnimation(_mesh, newAnimation));
                CurrentAnimationClip = newAnimation;
                RaisePropertyChanged("CurrentAnimationClip");
            }
            else
                Debug.Log(LogLevel.Warning, "Animation not found");
        }

        private void RemoveAnimation()
        {
            if(_mesh.AnimationData == null)
                return;

            ReduxManager.InsertInUndoRedo(new RemoveAnimation(_mesh,CurrentAnimationClip));
            
        }

        #endregion

        #region Loading Data
        public void LoadRecording()
        {
            //Opens dialog for selecting the file
            var dlg = new OpenFileDialog
            {
                FileName = "",
                DefaultExt = ".kr",
                Filter = "Kinect Recordings (.kr)|*.kr"
            };

            //Show the file open dialog
            var result = dlg.ShowDialog();
            if (result != true) return;

            //Deserialize the opened recording
            DeserializeRecordingFile(dlg.FileName);

            
        }
        public void SaveRecording()
        {
            //open save dialog
            var dlg = new SaveFileDialog()
            {
                FileName = "",
                DefaultExt = ".kr",
                Filter = "Kinect Recordings (.kr)|*.kr"
            };

            //Show the file open dialog
            var result = dlg.ShowDialog();
            if (result != true) return;

            if (_recording == null)
                return;

            var filePath = dlg.FileName;

            //Serialize data
            using (var output = new FileStream(@filePath, FileMode.OpenOrCreate))
            {
                try
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(output, _recording);
                }
                catch (Exception e)
                {
                    throw e;
                }
               
            }
        }

        //Actual opening and saving
        private void DeserializeRecordingFile(string filename)
        {
            //Load in the file
            _currentFile = filename;

            using (var file = new FileStream(_currentFile, FileMode.Open))
            {
                //Loads the serialized data
                try
                {
                    _recording = null;
                    var bf = new BinaryFormatter();
                    _recording = bf.Deserialize(file) as KinectRecording;

                    //_recording.RecreateFrames();
                    _recording.SetupBones();
                    _recording.OnFramesUpdated += (() =>
                    {
                        RaisePropertyChanged("TotalFrames");
                        RaisePropertyChanged("CurrentFrameIndex");
                    });
                }
                catch (Exception)
                {
                    Debug.Log(LogLevel.Warning, "Failed to load recording" + filename);
                    RaisePropertyChanged("TotalFrames");
                    RaisePropertyChanged("IsLoaded");
                    return;
                }
            }

            Debug.Log(LogLevel.Info, String.Format("Succesfully loaded file: {0}", _recording.FileName));

            CurrentFrameIndex = 0;
            _recording.SetupBones();

            //Need to let WPF know the Recording got changed
            RaisePropertyChanged("TotalFrames");
            RaisePropertyChanged("CurrentFrameIndex");
            RaisePropertyChanged("HasRecording");
        }
        private void LoadModel()
        {
            //Opens dialog for selecting the file
            //Filters are all available Importer formats
            var dlg = new OpenFileDialog
            {
                FileName = "",
                DefaultExt = ".FBX",
            };

            //Show the file open dialog
            var result = dlg.ShowDialog();
            if (result != true) return;

            OpenModelFile(dlg.FileName);

        }

        private void SaveModel()
        {
            var filters = Exporter.AvailableExtensions();

            //Opens dialog for selecting the file
            //Filters are all available Importer formats
            var dlg = new SaveFileDialog()
            {
                FileName = "",
                DefaultExt = ".OVM",
                Filter = "Model file (" + filters + ") | " + filters
            };

            //Show the file open dialog
            var result = dlg.ShowDialog();
            if (result != true) return;

            SaveModelFile(dlg.FileName, _mesh);
        }
        private void OpenModelFile(string file)
        {
            _mesh = new CustomMesh(file);

            _mesh.OnAnimationChanged += UpdateAnimationList;

            Viewport.AddMesh(_mesh);

            UpdateAnimationList();

        }

        private void SaveModelFile(string filename, IMesh model)
        {
            var exporter = Exporter.GetImporterFromExtension(filename);

            if (exporter == null)
                return;

            exporter.WriteToFile(filename, model);
        }
        
        #endregion

        #region Emulate The recording
        public void StartEmulation(bool resume)
        {
            //Don't play when already playing
            if (IsEmulating)
                return;

            //don't reset to 0 if we press resume
            if (!resume)
                CurrentFrameIndex = 0;

            IsEmulating = true;
            var loopThread = new Thread(RunEmulation);

            //Start from the current Frame Index
            loopThread.Start(CurrentFrameIndex);
            Paused = false;
        }

        public void StopEmulation(bool pause)
        {
            _stopped = !pause;
            IsEmulating = false;
            Paused = true;

            CurrentFrameIndex = 0;
        }

        //When passing through a variable to a new thread, it needs to be an object type
        private void RunEmulation(object i)
        {
            //Convert object
            var index = i is int ? (int) i : 0;
            var sleepTime = 40;

            while (index < TotalFrames)
            {
                if (!IsEmulating)
                    break;

                ++index;
                //Set the index to the CurrentFrame of the UI thread

                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.CurrentFrameIndex = index;
                });

                //Some delay or it will run too fast
                Thread.Sleep(sleepTime);
            }

            if (_stopped)
            {
                //Doesn't stop instantly
                //At the end of the Loop, set it back to the start
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.CurrentFrameIndex = 0;
                });

            }

            IsEmulating = false;
        }
        #endregion

    }
}
