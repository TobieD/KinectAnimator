using System.Collections.Generic;
using System.IO;
using KinectTool.Drawing.Prefabs;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;

namespace KinectTool.Drawing.Effect
{
    public class PosColorNormSkinnedEffect:IEffect
    {
        public EffectTechnique Technique { get; set; }
        public SharpDX.Direct3D10.Effect Effect { get; set; }
        public InputLayout InputLayout { get; set; }
        public void Create(Device1 device)
        {
            //Create effect
            var shaderSource = File.ReadAllText("Resources/Effects/PosColNorm3DSkinned.fx");
            var shaderByteCode = ShaderBytecode.Compile(shaderSource, "fx_4_0", ShaderFlags.None, EffectFlags.None);
            Effect = new SharpDX.Direct3D10.Effect(device,shaderByteCode);
            Technique = Effect.GetTechniqueByIndex(0);

            //Create inputLayout
            var pass = Technique.GetPassByIndex(0);
            InputLayout = new InputLayout(device, pass.Description.Signature, InputLayouts.SkinnedVertex);

        }

        public void SetBoneTransforms(List<Matrix> boneTransforms)
        {
            if(this.Effect == null)
                return;

            Effect.GetVariableByName("_Bones").AsMatrix().SetMatrix(boneTransforms.ToArray());
        }

        public void SetWorld(Matrix world)
        {
            if (Effect == null)
                return;

            Effect.GetVariableBySemantic("WORLD").AsMatrix().SetMatrix(world);

            
            
        }   

        public void SetWorldViewProjection(Matrix wvp)
        {
            if (Effect == null)
                return;

            Effect.GetVariableBySemantic("WORLDVIEWPROJECTION").AsMatrix().SetMatrix(wvp);
        }

        public void SetLightDirection(Vector3 dir)
        {
            if (Effect == null)
                return;

            Effect.GetVariableByName("_Light").AsVector().Set(dir);
        }
    }
}
