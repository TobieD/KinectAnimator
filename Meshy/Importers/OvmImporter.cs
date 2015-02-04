using System;
using System.IO;
using Meshy.Interfaces;
using SharpDX;

namespace Meshy.Importers
{
    /// <summary>
    /// Load in a single mesh from an ovm file using a binary reader
    /// </summary>
    public class OvmImporter :IMeshImporter,IAnimationImporter
    {
        private enum MeshDataType : int
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

        public AnimationData AnimationData { get; private set; }

        public MeshData MeshData { get; private set; }

        public void ReadFromFile(string filename)
        {
            //Check if file exists
            if (!File.Exists(filename))
                return;
            
            MeshData = new MeshData();
            AnimationData = new AnimationData();

            using (var reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                //Read version info
                var verMajor = reader.ReadByte();
                var verMinor = reader.ReadByte();
                //Console.WriteLine("Importing OVM Version {0}.{1}",verMajor,verMinor);

                while (true)
                {
                    //For each block
                    var id = (MeshDataType)reader.ReadByte();
                    if (id == MeshDataType.END)
                        break;

                    var length = reader.ReadUInt32();

                    //Console.WriteLine("Length of Id:{0} = {1}", ((MeshDataType)id).ToString(), length);

                    //Do something based on Id
                    switch (id)
                    {
                        case MeshDataType.HEADER:
                            {
                                MeshData.Name = reader.ReadString();
                                MeshData.VertexCount = (int)reader.ReadUInt32();
                                MeshData.IndexCount = (int)reader.ReadUInt32();
                            }
                            break;

                        case MeshDataType.POSITIONS:
                            for (var i = 0; i < MeshData.VertexCount; ++i)
                            {
                                var x = reader.ReadSingle();
                                var y = reader.ReadSingle();
                                var z = reader.ReadSingle();
                                MeshData.VertexPositions.Add(new Vector3(x, y, z));
                            }
                            break;

                        case MeshDataType.INDICES:

                            for (var i = 0; i < MeshData.IndexCount; ++i)
                            {
                                var x = (int)reader.ReadInt32();
                                MeshData.Indices.Add(x);
                            }
                            break;

                        case MeshDataType.NORMALS:
                            for (var i = 0; i < MeshData.VertexCount; ++i)
                            {
                                var x = reader.ReadSingle();
                                var y = reader.ReadSingle();
                                var z = reader.ReadSingle();
                                MeshData.VertexNormals.Add(new Vector3(x, y, z));
                            }
                            break;

                            //Not used
                        case MeshDataType.COLORS:
                            for (var i = 0; i < MeshData.VertexCount; ++i)
                            {
                                var x = reader.ReadSingle();
                                var y = reader.ReadSingle();
                                var z = reader.ReadSingle();
                                var w = reader.ReadSingle();
                            }
                            break;

                        case MeshDataType.BLENDINDICES:
                            for (var i = 0; i < MeshData.VertexCount; ++i)
                            {
                                var blendIndex = new Vector4
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle(),
                                    Z = reader.ReadSingle(),
                                    W = reader.ReadSingle()
                                };
                                AnimationData.BlendIndices.Add(blendIndex);
                            }
                            break;

                        case MeshDataType.BLENDWEIGHTS:
                            for (var i = 0; i < MeshData.VertexCount; ++i)
                            {
                                var blendWeight = new Vector4
                                {
                                    X = reader.ReadSingle(),
                                    Y = reader.ReadSingle(),
                                    Z = reader.ReadSingle(),
                                    W = reader.ReadSingle()
                                };
                                AnimationData.BlendWeights.Add(blendWeight);
                            }
                            break;

                        case MeshDataType.ANIMATIONCLIPS:

                            
                            // lips in the data
                            var clipcount = reader.ReadInt16();
                            //for every clip
                            for (var i = 0; i < clipcount; ++i)
                            {
                                var clip = new AnimationClip
                                {
                                    Name = reader.ReadString(),
                                    Duration = reader.ReadSingle(),
                                    TicksPerSecond = reader.ReadSingle()
                                };

                                //Amount of keys in the clip
                                var keyCount = reader.ReadInt16();

                                for (var j = 0; j < keyCount; j++)
                                {
                                    var key = new AnimationKey();
                                    key.Tick = reader.ReadSingle();

                                    //Transforms in the key
                                    var transformCount = reader.ReadInt16();
                                    for (var k = 0; k < transformCount; k++)
                                    {
                                        var floatArr = new float[16];
                                        for (var l = 0; l < 16; l++)
                                            floatArr[l] = reader.ReadSingle();

                                        var boneTransforms = new Matrix(floatArr);
                                        key.BoneTransforms.Add(boneTransforms);
                                    }

                                    //Add key to the clip
                                    clip.Keys.Add(key);
                                }

                                //Add clip to the animationList
                                AnimationData.Animations.Add(clip);
                            }

                            
                            break;
                        case MeshDataType.SKELETON:
                            AnimationData.BoneCount = reader.ReadInt16();
                            

                            for (var i = 0; i < AnimationData.BoneCount; ++i)
                            {
                                var bone = new AnimationJoint
                                {
                                    Index = reader.ReadInt16(),
                                    Name = reader.ReadString(),
                                    ParentIndex = reader.ReadUInt16()
                                };

                                //Offset
                                var floatArr = new float[16];
                                for (var l = 0; l < 16; l++)
                                    floatArr[l] = reader.ReadSingle();

                                bone.OffsetMatrix = new Matrix(floatArr);

                                //Local
                                for (var l = 0; l < 16; l++)
                                    floatArr[l] = reader.ReadSingle();

                                bone.LocalMatrix = new Matrix(floatArr);

                                //Global
                                for (var l = 0; l < 16; l++)
                                    floatArr[l] = reader.ReadSingle();

                                bone.GlobalMatrix = new Matrix(floatArr);

                                bone.IsValid = true;

                                AnimationData.Bones.Add(bone);

                                AnimationData.HasAnimations = true;
                            }
                             break;

                        default:
                            reader.ReadBytes((int)length);
                            break;
                    }
                }
            }

        }


        
    }
}
