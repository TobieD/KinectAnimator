using System;

namespace KinectTool.Model.Kinect
{

    /// <summary>
    /// Stores data about the bone between 2 KinectJoints
    /// </summary>
    [Serializable]
    public class KinectBone
    {
        public KinectJoint Joint0 { get; private set; }
        public KinectJoint Joint1 { get; private set; }

        public KinectBone(KinectJoint j0, KinectJoint j1)
        {
            Joint0 = j0;
            Joint1 = j1;
        }

    }
}
