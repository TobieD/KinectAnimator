using KinectTool.Model.Kinect;

namespace KinectTool.Drawing.Components
{
    public interface IFrameVisual
    {
        void SetFrame(KinectFrame frame);
        void Draw();

    }
}
