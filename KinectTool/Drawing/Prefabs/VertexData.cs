using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;

namespace KinectTool.Drawing.Prefabs
{
    public struct VertexPosColNorm
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;

        public VertexPosColNorm(Vector3 pos, Color col, Vector3 norm)
        {
            Position = pos;
            Color = col.ToVector4();
            Normal = norm;
        }
    }

    public struct SkinnedVertex
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;

        public Vector4 BlendIndices;
        public Vector4 BlendWeights;

        public SkinnedVertex(Vector3 pos, Color col, Vector3 norm, Vector4 blendIndices,Vector4 blendWeights)
        {
            Position = pos;
            Color = col.ToVector4();
            Normal = norm;
            BlendIndices = blendIndices;
            BlendWeights = blendWeights;
        }
    }

    public static class InputLayouts
    {
        public static readonly InputElement[] PosColNorm = {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
        };

        public static readonly InputElement[] SkinnedVertex = {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElement("BLENDINDICES", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElement("BLENDWEIGHTS", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
        };
    }
}
