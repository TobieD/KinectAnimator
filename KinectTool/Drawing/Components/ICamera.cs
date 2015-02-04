using SharpDX;

namespace KinectTool.Drawing.Components
{
    public interface ICamera
    {
        Matrix View { get; }

        //View settings
        Vector3 EyePosition { get; set; }
        Vector3 TargetPosition { get; set; }

        Vector3 UpDirection { get; set; }


        Matrix Projection { get; }

        //Projection settings
        float FieldOfView { get; set; }
        float AspectRatio { get; set; }

        float NearPlane { get; set; }

        float FarPlane { get; set; }

        Matrix World { get; }

        void Update();
    }
}
