using System;
using System.Windows;
using System.Windows.Input;
using Debugify;
using KinectTool.Drawing.Components;
using SharpDX;
using Matrix = SharpDX.Matrix;
using Point = System.Windows.Point;

namespace KinectTool.Drawing.Prefabs
{
    public class FlyingCamera:ICamera,ITransformable
    {
        //Movement
        private const float MOVE_SPEED = 140.0f;
        private const float ROTATION_SPEED = 0.3f;
        private float _totalYaw;
        private float _totalPitch;

        private DateTime _prevFrameTime;

        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }

        private Point _prevMousePos;


        #region Matrices
        public Matrix View
        {
            get { return Matrix.LookAtLH(EyePosition, TargetPosition, UpDirection); }
        }
        public Matrix Projection
        {
            get { return Matrix.PerspectiveFovLH(FieldOfView, AspectRatio, NearPlane, FarPlane); }
        }

        private Matrix _world = Matrix.Identity;
        public Matrix World
        {
            get { return _world; }
            set { _world = value; }
        }

        public Matrix Translation { get; set; }
        public Matrix Scale { get; set; }
        public Matrix Rotating { get; set; }


        #endregion 

       
        #region View Variables
        public Vector3 EyePosition { get; set; }

        public Vector3 TargetPosition { get; set; }
        public Vector3 UpDirection { get; set; }

        #endregion


        #region Projection Variables
        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }
        public float NearPlane { get; set; }
        public float FarPlane { get; set; }
        #endregion

        public FlyingCamera()
        {
            AspectRatio = 1920.0f/1080.0f;
            NearPlane = 0.01f;
            FarPlane = 900000.0f;
            FieldOfView = MathUtil.PiOverFour;

            Forward = Vector3.ForwardLH;
            Right = Vector3.Right;
            UpDirection = Vector3.Up;

            EyePosition = new Vector3(0, 250, -750);
        }

        public void Update()
        {
            //Calculate deltaTime
            var passedTime = (float)(DateTime.Now - _prevFrameTime).Milliseconds / 1000;

            //Determine direction of travel
            var moveDirection = Vector2.Zero;
            moveDirection.X = Keyboard.IsKeyDown(Key.D) ? 1.0f : 0.0f;
            if (moveDirection.X.Equals(0.0f)) moveDirection.X = -(Keyboard.IsKeyDown(Key.A) ? 1.0f : 0.0f);

            moveDirection.Y = Keyboard.IsKeyDown(Key.W) ? 1.0f : 0.0f;
            if (moveDirection.Y.Equals(0.0f)) moveDirection.Y = -(Keyboard.IsKeyDown(Key.S) ? 1.0f : 0.0f);

            var currPos = Mouse.GetPosition(Application.Current.MainWindow);

            //Mouse Rotation
            var mouseLook = Vector2.Zero;
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
               
                var mouseMovement = currPos - _prevMousePos;

                //Debug.Log(LogLevel.Info,mouseMovement.ToString());
                //mouseLook.X = (float) (mouseMovement.X/Math.Abs(mouseMovement.X));
                //mouseLook.Y = (float)(mouseMovement.Y / Math.Abs(mouseMovement.Y));

                mouseLook.X = (float)(mouseMovement.X);
                mouseLook.Y = (float)(mouseMovement.Y);
            }
            
            //Calculate rotation
            _totalYaw += mouseLook.X*ROTATION_SPEED* passedTime;
            _totalPitch += mouseLook.Y * ROTATION_SPEED * passedTime;
            Rotating = Matrix.RotationYawPitchRoll(_totalYaw,_totalPitch ,0 );

            Translation = Matrix.Translation(EyePosition); 
            Scale = Matrix.Scaling(1.0f);

            //Update Forward,Right and up based on the current rotation of the Camera
            Forward = Vector3.TransformCoordinate(Vector3.ForwardLH, Rotating);
            Right = Vector3.TransformCoordinate(Vector3.Right, Rotating);
            UpDirection = Vector3.Cross(Forward, Right);

            EyePosition += moveDirection.X*Right * MOVE_SPEED * passedTime;
            EyePosition += moveDirection.Y * Forward * MOVE_SPEED * passedTime;


            var viewDirection = (Vector3)Vector3.Transform(Vector3.ForwardLH, Rotating);
            viewDirection.Normalize();
            TargetPosition = EyePosition + viewDirection;

            _prevFrameTime = DateTime.Now;
            _prevMousePos = currPos;

            //Debug.Log(LogLevel.Info,EyePosition.ToString());
        }
    }
}
