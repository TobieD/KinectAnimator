using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Assimp;
using Assimp.Configs;
using Meshy.Interfaces;
using SharpDX;


namespace Meshy.Importers
{
    public class AssimpImporter:IMeshImporter,IAnimationImporter
    {
        public MeshData MeshData { get; private set; }
        public AnimationData AnimationData { get; private set; }

        public static string[] AvaliableFormats { get; private set; }

        public AssimpImporter()
        {
            var importer = new AssimpContext();
            AvaliableFormats = importer.GetSupportedImportFormats();
            importer.Dispose();
        }

        public void ReadFromFile(string filename)
        {
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
            
            //Create new importer
            var importer = new AssimpContext();

            //Check if model is supported
            if (!importer.IsImportFormatSupported(Path.GetExtension(file)))
                throw new ArgumentException("Model format " + Path.GetExtension(file) + " is not supported!  Cannot load {1}", "filename");

            //Configs
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));

            var model = importer.ImportFile(file, PostProcessPreset.TargetRealTimeMaximumQuality);
            importer.Dispose();


            MeshData = new MeshData();
            AnimationData = new AnimationData();
            foreach (var mesh in model.Meshes)
            {
                MeshData.Indices = mesh.GetIndices().ToList();
                MeshData.IndexCount = MeshData.Indices.Count;
                MeshData.Name = mesh.Name;
                MeshData.VertexCount = mesh.VertexCount;

                foreach (var v in mesh.Vertices)
                {
                    MeshData.VertexPositions.Add(new Vector3(v.X, v.Y, v.Z));
                }

                foreach (var v in mesh.Normals)
                {
                    MeshData.VertexNormals.Add(new Vector3(v.X, v.Y, v.Z));
                }

                foreach (var v in mesh.TextureCoordinateChannels[0])
                {
                    MeshData.VertexTextureCoordinates.Add(new Vector2(v.X, v.Y));
                }

                AnimationData.BoneCount = mesh.BoneCount;

                foreach (var b in mesh.Bones)
                {
                    var j = new AnimationJoint();

                    j.Name = b.Name;
                    //j.OffsetMatrix = b.OffsetMatrix;

                    AnimationData.Bones.Add(j);
                }
            }

            return;

            //TO DO STILL BONES ARE DONE IN A DIFFERENT WAY

            AnimationData.HasAnimations = model.HasAnimations;
           

            foreach (var a in model.Animations)
            {
                //Clip
                var clip = new AnimationClip()
                {
                    Name = a.Name,
                    Duration = (float) a.DurationInTicks,
                    TicksPerSecond = (float) a.TicksPerSecond
                };

                //KeyFrames
                foreach (var m in a.NodeAnimationChannels)
                {
                    var key = new AnimationKey();

                    for (var i = 0; i < m.PositionKeyCount;++i)
                    {

                        var time = m.PositionKeys[i].Time;
                        var pos = m.PositionKeys[i].Value.ToVector3();
                        var rot = m.RotationKeys[i].Value;
                        var s = m.ScalingKeys[i].Value.ToVector3();


                        

                    }
                   





                    //clip.Keys.Add(key);
                    }
                }
               // AnimationData.Animations.Add(clip);
            //}
        }

        
    }
}
