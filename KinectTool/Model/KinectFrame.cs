using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Kinect;

namespace KinectTool.Model.Kinect
{
    /// <summary>
    /// Contains information about a frame recorded from the KinectSensor
    /// </summary>
    [Serializable]
    public class KinectFrame:IDeserializationCallback
    {

        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// List of all the joints in this Frame
        /// </summary>
        public Dictionary<JointType, KinectJoint> Joints { get; private set; }

       // public List<BoneOrientation> BoneOrientations { get; set; }
        private List<KinectBone> _bones;

        public List<KinectBone> Bones
        {
            get { return _bones; }
            private set { _bones = value; }
        }

        public KinectFrame(Skeleton skeleton)
        {

            Skeleton = skeleton;

            //Keep the joints
            Joints = new Dictionary<JointType, KinectJoint>();
            foreach (Joint j in skeleton.Joints)
            {
                var kJoint = new KinectJoint(j, Skeleton.BoneOrientations[j.JointType]);
                Joints.Add(kJoint.Type, kJoint);
            }

            //Import Joints
             SetupJoints();
            //SetUpBones();
        }

        private void SetupJoints()
        {
            Joints.Clear();

            Joints.Add(JointType.HipCenter, CreateJoint(JointType.HipCenter));

            Joints.Add(JointType.HipLeft, CreateJoint(JointType.HipLeft));
            Joints.Add(JointType.KneeLeft, CreateJoint(JointType.KneeLeft));
            Joints.Add(JointType.AnkleLeft, CreateJoint(JointType.AnkleLeft));
            Joints.Add(JointType.FootLeft, CreateJoint(JointType.FootLeft));

            Joints.Add(JointType.HipRight, CreateJoint(JointType.HipRight));
            Joints.Add(JointType.KneeRight, CreateJoint(JointType.KneeRight));
            Joints.Add(JointType.AnkleRight, CreateJoint(JointType.AnkleRight));
            Joints.Add(JointType.FootRight, CreateJoint(JointType.FootRight));

            
            Joints.Add(JointType.Spine, CreateJoint(JointType.Spine));
            Joints.Add(JointType.ShoulderCenter, CreateJoint(JointType.ShoulderCenter));
            

            Joints.Add(JointType.ShoulderLeft, CreateJoint(JointType.ShoulderLeft));
            Joints.Add(JointType.ElbowLeft, CreateJoint(JointType.ElbowLeft));
            Joints.Add(JointType.WristLeft, CreateJoint(JointType.WristLeft));
            Joints.Add(JointType.HandLeft, CreateJoint(JointType.HandLeft));

            Joints.Add(JointType.ShoulderRight, CreateJoint(JointType.ShoulderRight));
            Joints.Add(JointType.ElbowRight, CreateJoint(JointType.ElbowRight));
            Joints.Add(JointType.WristRight, CreateJoint(JointType.WristRight));
            Joints.Add(JointType.HandRight, CreateJoint(JointType.HandRight));

            Joints.Add(JointType.Head, CreateJoint(JointType.Head));
           
            

        }

        private KinectJoint CreateJoint(JointType type)
        {
            var joints = Skeleton.Joints;
            var joint = new KinectJoint(joints[type], Skeleton.BoneOrientations[type]);

            return joint;

        }

        public void SetUpBones()
        {
            if(Joints.Count == 0)
                return;

            Bones = new List<KinectBone>
            {
                //Torso
                new KinectBone(Joints[JointType.Head], Joints[JointType.ShoulderCenter]),
                new KinectBone(Joints[JointType.ShoulderCenter], Joints[JointType.ShoulderLeft]),
                new KinectBone(Joints[JointType.ShoulderCenter], Joints[JointType.ShoulderRight]),
                new KinectBone(Joints[JointType.ShoulderCenter], Joints[JointType.Spine]),
                new KinectBone(Joints[JointType.Spine], Joints[JointType.HipCenter]),
                new KinectBone(Joints[JointType.HipCenter], Joints[JointType.HipLeft]),
                new KinectBone(Joints[JointType.HipCenter], Joints[JointType.HipRight]),

                //Left Arm
                new KinectBone(Joints[JointType.ShoulderLeft], Joints[JointType.ElbowLeft]),
                new KinectBone(Joints[JointType.ElbowLeft], Joints[JointType.WristLeft]),
                new KinectBone(Joints[JointType.WristLeft], Joints[JointType.HandLeft]),

                //Right Arm
                new KinectBone(Joints[JointType.ShoulderRight], Joints[JointType.ElbowRight]),
                new KinectBone(Joints[JointType.ElbowRight], Joints[JointType.WristRight]),
                new KinectBone(Joints[JointType.WristRight], Joints[JointType.HandRight]),

                //Left Leg
                new KinectBone(Joints[JointType.HipLeft], Joints[JointType.KneeLeft]),
                new KinectBone(Joints[JointType.KneeLeft], Joints[JointType.AnkleLeft]),
                new KinectBone(Joints[JointType.AnkleLeft], Joints[JointType.FootLeft]),

                //Right Leg
                new KinectBone(Joints[JointType.HipRight], Joints[JointType.KneeRight]),
                new KinectBone(Joints[JointType.KneeRight], Joints[JointType.AnkleRight]),
                new KinectBone(Joints[JointType.AnkleRight], Joints[JointType.FootRight])
            };
        }

        public void OnDeserialization(object sender)
        {
            //SetUpBones();
        }

        public bool IsBadFrame()
        {
            if (Joints.Values.Any(joint => joint.TrackState != JointTrackingState.Tracked))
                return true;


            return false;
        }
    }
}
