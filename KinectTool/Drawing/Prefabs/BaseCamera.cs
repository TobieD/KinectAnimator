using KinectTool.Drawing.Components;
using SharpDX;

namespace KinectTool.Drawing.Prefabs
{
    /// <summary>
    /// Simple Camera
    /// </summary>
    public class BaseCamera :ICamera
    {
        public Matrix View {
            get { return Matrix.LookAtLH(EyePosition, TargetPosition, UpDirection); }
        }

        private Matrix _World = Matrix.Identity;
        public Matrix World
        {
            get { return _World; }
            set { _World = value; }
        }
        
        public Matrix Projection
        {
            get { return Matrix.PerspectiveFovLH(FieldOfView, AspectRatio, NearPlane, FarPlane); }
        }

        private Vector3 _eyePosition = new Vector3(0,50,-100);
        public Vector3 EyePosition
        {
            get { return _eyePosition; }
            set { _eyePosition = value; }
        }

        private Vector3 _targetPosition = Vector3.One;
        public Vector3 TargetPosition
        {
            get { return _targetPosition; }
            set { _targetPosition = value; }
        }

        private Vector3 _upDirection = Vector3.Up;
        public Vector3 UpDirection
        {
            get { return _upDirection; }
            set { _upDirection = value; }
        }


        private float _fov = MathUtil.PiOverFour;
        public float FieldOfView {
            get { return _fov; }
            set { _fov = value; } 
        }

        private float _aspect = 1920.0f/1080.0f;

        public float AspectRatio {
            get { return _aspect; }
            set { _aspect = value; } }


        private float _near = 0.1f;
        public float NearPlane 
        {
            get { return _near; }
            set { _near = value; }
        }

        private float _far = 1000.0f;
        public float FarPlane
        {
            get { return _far; }
            set { _far = value; }
        }


        public BaseCamera()
        {
            EyePosition = new Vector3(0,-150,-300);
        }

        public void Update()
        {
            //nothing
        }


     
    }
}
