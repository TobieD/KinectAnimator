using System;
using System.Collections.Generic;
using System.Linq;
using Debugify;
using Meshy;
using Meshy.Interfaces;

namespace KinectTool.Model.Kinect
{
    /// <summary>
    /// Holds data about a series of frames and can be saved to a file
    /// Rewrite to Editable Recording for editing and keep this purely data
    /// </summary>
    [Serializable]
    public class KinectRecording
    {
        #region NonSerialized

        //total frames
        public int Frames
        {
            get { return _Frames.Count; }
        }
        #endregion

        #region Serialized

        //Name of the recording
        public string FileName { get; set; }

        //All the frames recorded
        private List<KinectFrame> _Frames;
        public List<KinectFrame> FrameList
        {
            get { return _Frames; }
            private set { _Frames = value.ToList(); }
        }

        #endregion

        [NonSerialized] 
        private Action _onFramesUpdated;
        public Action OnFramesUpdated {
            get { return _onFramesUpdated; }
            set { _onFramesUpdated = value; }}

        public KinectRecording(string filename)
        {
            FileName = filename;
            _Frames = new List<KinectFrame>();
        }

        public void SetupBones()
        {
            foreach (var frame in _Frames)
            {
                frame.SetUpBones();
            }
        }

        #region Editing
        /// <summary>
        //Add a new Frame to the list
        /// </summary>
        public void AddFrame(KinectFrame frame)
        {
            if (frame == null) return;

            _Frames.Add(frame);
            if(OnFramesUpdated != null)
                OnFramesUpdated();
        }

        public void AddFrame(KinectFrame frame, int index)
        {
            if (frame == null) return;

            _Frames.Insert(index, frame);
            if (OnFramesUpdated != null)
                OnFramesUpdated();
               
        }

        /// <summary>
        /// Remove Frame at given index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveFrame(int index)
        {
            _Frames.RemoveAt(index);

            if (OnFramesUpdated != null)
                OnFramesUpdated();
        }

        /// <summary>
        /// Remove Frame
        /// </summary>
        public void RemoveFrame(KinectFrame frame)
        {
            _Frames.Remove(frame);

            if (OnFramesUpdated != null)
                OnFramesUpdated();
        }

        /// <summary>
        /// Remove frames that aren't tracked well
        /// </summary>
        public List<KinectFrame> CleanFrames()
        {
            var newFrames = (from f in _Frames where !f.IsBadFrame() select new KinectFrame(f.Skeleton)).ToList();
            return newFrames.ToList();
        }

        public void SetFrames(List<KinectFrame> list)
        {
            FrameList = list;
            SetupBones();
            if (OnFramesUpdated != null)
                OnFramesUpdated();
        }
            
        #endregion

        /// <summary>
        /// Get Frame at given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public KinectFrame GetFrame(int index)
        {
            return index >= Frames ? null : _Frames[index];
        }

        /// <summary>
        /// Save Copy
        /// </summary>
        /// <returns></returns>
        public KinectRecording Copy()
        {
            var copiedRec = new KinectRecording(FileName);
            copiedRec._Frames = _Frames.ToList();
            return copiedRec;
        }

        public void RecreateFrames()
        {
            var tempFrames = _Frames.ToList();

            _Frames.Clear();

            foreach (var f in tempFrames)
            {
                _Frames.Add(new KinectFrame(f.Skeleton));
            }
        }
      
    }
}
