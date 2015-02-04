using System;
using System.Runtime.Serialization;
using System.Windows;
using KinectTool.Helpers;
using Microsoft.Kinect;
using SharpDX;
using Point = System.Windows.Point;

namespace KinectTool.Model.Kinect
{
    /// <summary>
    /// Store data about 1 Joint of a kinect skeleton
    /// </summary>
    [Serializable]

    public class KinectJoint:IDeserializationCallback
    {
        /// <summary>
        /// Type of the joint
        /// </summary>
        public JointType Type { get; private set; }

        /// <summary>
        /// Joint is tracked in this frame
        /// </summary>
        public JointTrackingState TrackState { get;private set;}

        /// <summary>
        /// 3D Position in skeleton space
        /// </summary>
        public SkeletonPoint PositionSkeleton { get; private set; }

        private float[] _hierArr;
        private float[] _absArr;


        //recreate on deserialization
        #region Non Serialized
        [NonSerialized]
        private Matrix _absoluteRotation;
        public Matrix AbsoluteRotation 
        {
                get { return _absoluteRotation; }
                set { _absoluteRotation = value; }
        }

         [NonSerialized]
        private Matrix _hierarchicalRotation;
        public Matrix HierarchicalRotation
        {
            get { return _hierarchicalRotation; }
            set { _hierarchicalRotation = value; }
        }
        
        /// <summary>
        /// Position of the joint in 2D Space
        /// </summary>
        [NonSerialized]
        private Vector2 _pos2D;
        public Vector2 Position2D
        {
            get { return _pos2D; }
            private set { _pos2D = value; }
        }

        /// <summary>
        /// Position of the joint in 3D Space
        /// </summary>
        [NonSerialized]
        private Vector3 _pos3D;
        public Vector3 Position3D {
            get { return _pos3D; }
            private set { _pos3D = value; }
        }

        /// <summary>
        /// Hitbox of the joint to check when selected in editor
        /// </summary>

        [NonSerialized]
        private Rect _hitbox;

        public Rect HitBox
        {
            get { return _hitbox; }
            set { _hitbox = value; }
        }

        [NonSerialized]
        private BoneOrientation _orientation;
        public BoneOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
            }
        }

        /// <summary>
        /// Bounds of the HitBox
        /// </summary>
        [NonSerialized]
        private const double BOUNDS = 25.0f;

        #endregion

        public KinectJoint(Joint joint, BoneOrientation boneOrientation)
        {
            Type = joint.JointType;
            TrackState = joint.TrackingState;
            PositionSkeleton = joint.Position;

            Orientation = boneOrientation;

            SetUpMatrices();

            Position2D = KinectMath.SkeletonPointToScreen(joint.Position);
            Position3D = KinectMath.SkeletonPointToVector3(joint.Position);
            HitBox = new Rect(new Point(Position2D.X-(BOUNDS/2),Position2D.Y -(BOUNDS/2)), new Size(BOUNDS, BOUNDS));
        }

        public void OnDeserialization(object sender)
        {
            Position2D = KinectMath.SkeletonPointToScreen(PositionSkeleton);
            _pos3D = KinectMath.SkeletonPointToVector3(PositionSkeleton);
            HitBox = new Rect(new Point(Position2D.X - (BOUNDS / 2), Position2D.Y - (BOUNDS / 2)), new Size(BOUNDS, BOUNDS));

           HierarchicalRotation = new Matrix(_hierArr);
           AbsoluteRotation = new Matrix(_absArr);
        }

        private void SetUpMatrices()
        {
            var matRot = Orientation.AbsoluteRotation.Matrix;

            _absArr = new float[]
            {
                matRot.M11, matRot.M12, matRot.M13, matRot.M14,
                matRot.M21, matRot.M22, matRot.M23, matRot.M24,
                matRot.M31, matRot.M32, matRot.M33, matRot.M34,
                matRot.M41, matRot.M42, matRot.M43, matRot.M44
            };


            matRot = Orientation.HierarchicalRotation.Matrix;
            _hierArr = new float[]
            {
                matRot.M11, matRot.M12, matRot.M13, matRot.M14,
                matRot.M21, matRot.M22, matRot.M23, matRot.M24,
                matRot.M31, matRot.M32, matRot.M33, matRot.M34,
                matRot.M41, matRot.M42, matRot.M43, matRot.M44
            };


            HierarchicalRotation = new Matrix(_hierArr);
            AbsoluteRotation = new Matrix(_absArr);

        }
    }
}
