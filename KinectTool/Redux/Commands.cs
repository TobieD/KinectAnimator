using System.Collections.Generic;
using KinectTool.Drawing.Prefabs;
using KinectTool.Model.Kinect;
using Meshy;
using Microsoft.Kinect;
using SharpDX.Collections;

namespace KinectTool.Redux
{
    public interface ICommand
    {
        void Execute();
        void UnExecute();

    }

    public class AddNewAnimation:ICommand
    {
        private readonly CustomMesh _mesh;
        private readonly AnimationClip _clip;

        public AddNewAnimation(CustomMesh mesh, AnimationClip clip)
        {
            _mesh = mesh;
            _clip = clip;
            Execute();
        }

        public void Execute()
        {
            _mesh.AddAnimation(_clip);
        }

        public void UnExecute()
        {
            _mesh.RemoveAnimation(_clip);
        }
    }

    public class RemoveAnimation : ICommand
    {
        private readonly CustomMesh _mesh;
        private readonly AnimationClip _clip;

        public RemoveAnimation(CustomMesh mesh, AnimationClip clip)
        {
            _mesh = mesh;
            _clip = clip;
            Execute();
        }

        public void Execute()
        {
            _mesh.RemoveAnimation(_clip);
        }

        public void UnExecute()
        {
            _mesh.AddAnimation(_clip);
        }
    }

    public class RemoveSingleFrame:ICommand
    {

        private readonly KinectRecording _recording;
        private readonly KinectFrame _frame;
        private readonly int _index = 0;

        public RemoveSingleFrame(KinectRecording rec, int index)
        {
            _recording = rec;
            _index = index;
            _frame = _recording.GetFrame(_index);
            Execute();
        }

        public void Execute()
        {
            _recording.RemoveFrame(_index);
        }

        public void UnExecute()
        {
            _recording.AddFrame(_frame,_index);
        }
    }

    public class CleanUpFrames:ICommand
    {

        private readonly KinectRecording _recording;
        private List<KinectFrame> _originalKinectFrames;
        private List<KinectFrame> _updatedKinectFrames; 

        public CleanUpFrames(KinectRecording rec)
        {
            _recording = rec;
            _originalKinectFrames = rec.FrameList;
            Execute();
        }

        public void Execute()
        {
           _updatedKinectFrames = _recording.CleanFrames();
            _recording.SetFrames(_updatedKinectFrames);
        }

        public void UnExecute()
        {
            _recording.SetFrames(_originalKinectFrames);
        }
    }

    public class SetJoint : ICommand
    {

        private AnimationJoint _joint;
        private AnimationJoint _prevValue;
        private readonly ObservableDictionary<JointType, AnimationJoint> _jointList;
        private readonly JointType _type;
        public SetJoint(ObservableDictionary<JointType, AnimationJoint> list, AnimationJoint j, JointType t)
        {
            _joint = j;
            _type = t;
            _jointList = list;
            Execute();
        }

        public void Execute()
        {
            _prevValue = _jointList[_type];
            _jointList[_type] = _joint;
        }

        public void UnExecute()
        {
            _jointList[_type] = _prevValue;
        }
    }
}
