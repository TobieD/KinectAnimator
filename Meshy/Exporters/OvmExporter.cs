using Meshy.Interfaces;
using OVCLib;
using Encoding = System.Text.Encoding;

namespace Meshy.Exporters
{
    /// <summary>
    /// Export meshData to an ovm file
    /// </summary>
    public class OvmExporter:IMeshExporter
    {
        private const int VerMajor = 1;
        private const int VerMinor = 1;
        internal enum MeshDataType : int
        {
            END = 0,
            HEADER = 1,
            POSITIONS = 2,
            INDICES = 3,
            NORMALS = 4,
            BINORMALS = 5,
            TANGENTS = 6,
            COLORS = 7,
            TEXCOORDS = 8,
            BLENDINDICES = 9,
            BLENDWEIGHTS = 10,
            ANIMATIONCLIPS = 11,
            SKELETON = 12
        };

        public MeshData MeshData { get; private set; }
        public AnimationData AnimData { get; private set; }
        public void WriteToFile(string filename, IMesh mesh)
        {
            MeshData = mesh.MeshData;
            AnimData = mesh.AnimationData;

            using (var writer = new BinaryBlockWriter(filename, Encoding.Default))
            {
                writer.Write((byte)VerMajor);
                writer.Write((byte)VerMinor);
                writer.ForceFlush();

                //Header
                WriteHeader(writer);


                if (MeshData != null)
                {
                    //Mesh data
                    WritePositonData(writer);
                    WriteIndexData(writer);
                    WriteNormalData(writer);
                    WriteTextcoordData(writer);
                }

                //Animation Data
                if (AnimData.HasAnimations)
                {
                    
                    WriteBlendIndices(writer);
                    WriteBlendWeights(writer);
                    WriteAnimationClips(writer);
                    WriteSkeletonData(writer);
                }

                writer.Write((byte)MeshDataType.END);
                writer.ForceFlush();
            }
        }

        private void WriteHeader(BinaryBlockWriter w)
        {
            w.StartBlock((byte)MeshDataType.HEADER);
            w.Write(MeshData.Name);
            w.Write(MeshData.VertexCount);
            w.Write(MeshData.IndexCount);
            w.EndBlock();
        }

        private void WritePositonData(BinaryBlockWriter w)
        {

            var positions = MeshData.VertexPositions;
            if(positions == null || positions.Count == 0)
                return;

            w.StartBlock((byte)MeshDataType.POSITIONS);
            foreach (var pos in positions)
                w.Write(pos);
            
            w.EndBlock();
        }

        private void WriteIndexData(BinaryBlockWriter w)
        {

            var indices = MeshData.Indices;
            if (indices == null || indices.Count == 0)
                return;

            w.StartBlock((byte)MeshDataType.INDICES);
            foreach (var index in indices)
                w.Write(index);

            w.EndBlock();
        }

        private void WriteNormalData(BinaryBlockWriter w)
        {

            var normals = MeshData.VertexNormals;
            if (normals == null || normals.Count == 0)
                return;

            w.StartBlock((byte)MeshDataType.NORMALS);
            foreach (var normal in normals)
                w.Write(normal);

            w.EndBlock();
        }

        private void WriteTextcoordData(BinaryBlockWriter w)
        {

            var coords = MeshData.VertexTextureCoordinates;
            if (coords == null || coords.Count == 0)
                return;

            w.StartBlock((byte)MeshDataType.TEXCOORDS);
            foreach (var uv in coords)
                w.Write(uv);

            w.EndBlock();
        }

        private void WriteBlendIndices(BinaryBlockWriter w)
        {
            var blendIndices = AnimData.BlendIndices;
            if (blendIndices == null || blendIndices.Count == 0)
                return;

            w.StartBlock((byte)MeshDataType.BLENDINDICES);
            foreach (var indices in blendIndices)
                w.Write(indices);

            w.EndBlock();
        }

        private void WriteBlendWeights(BinaryBlockWriter w)
        {
            var blendWeights = AnimData.BlendWeights;
            if (blendWeights == null || blendWeights.Count == 0)
                return;

            w.StartBlock((byte)MeshDataType.BLENDWEIGHTS);
            foreach (var weight in blendWeights)
                w.Write(weight);

            w.EndBlock();
        }

        private void WriteAnimationClips(BinaryBlockWriter w)
        {
            var animClips = AnimData.Animations;
            if (animClips == null || animClips.Count == 0)
                return;

            w.StartBlock((byte)MeshDataType.ANIMATIONCLIPS);

            w.Write((ushort)animClips.Count);
            foreach (var clip in animClips)
            {
                w.Write(clip.Name);
                w.Write(clip.Duration);
                w.Write(clip.TicksPerSecond);
                w.Write((ushort)clip.Keys.Count);
                foreach (var key in clip.Keys)
                {
                    w.Write(key.Tick);
                    w.Write((ushort)key.BoneTransforms.Count);
                    foreach (var m in key.BoneTransforms)
                        w.Write(m);
                }
            }

            w.EndBlock();
        }

        private void WriteSkeletonData(BinaryBlockWriter w)
        {
            w.StartBlock((byte)MeshDataType.SKELETON);
            w.Write((ushort)AnimData.BoneCount);

            foreach (var bone in AnimData.Bones)
            {
                w.Write((ushort)bone.Index);
                w.Write(bone.Name);
                w.Write((short)bone.ParentIndex);
                w.Write(bone.OffsetMatrix);
                w.Write(bone.LocalMatrix);
                w.Write(bone.GlobalMatrix);
            }

            w.EndBlock();
        }


    }
}
