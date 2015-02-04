
using SharpDX;

namespace KinectTool.Drawing.Components
{
    /// <summary>
    /// Allow Scaling, Rotating and Translating
    /// </summary>
    public interface ITransformable
    {
        Matrix Translation { get; set; }
        Matrix Scale { get; set; }
        Matrix Rotating { get; set; }

    }
}
