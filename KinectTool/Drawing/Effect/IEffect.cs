using SharpDX;
using SharpDX.Direct3D10;

namespace KinectTool.Drawing.Effect
{
    public interface IEffect
    {
        EffectTechnique Technique { get; set; }
        SharpDX.Direct3D10.Effect Effect { get; set; }
        InputLayout InputLayout { get; set; }

        void Create(Device1 device);
        void SetWorld(Matrix world);
        void SetWorldViewProjection(Matrix wvp);
        void SetLightDirection(Vector3 dir);
    }
}
