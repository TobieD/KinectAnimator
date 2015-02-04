using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Debugify;
using KinectTool.Drawing.Animation;
using KinectTool.Drawing.Components;
using Meshy;
using Meshy.Interfaces;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using Buffer = SharpDX.Direct3D10.Buffer;

namespace KinectTool.Drawing.Prefabs
{
    public class CustomMesh :IMesh,ITransformable
    {
        #region Basic Mesh Data

        public PrimitiveTopology PrimitiveTopology { get; private set; }

        public int VertexStride { get; private set; }
      
        public List<SkinnedVertex> SkinnedVertices { get; private set; }

        public List<VertexPosColNorm> Vertices { get; private set; }

        #endregion

        public MeshData MeshData { get; private set; }
        public AnimationData AnimationData { get; private set; }

        #region Creation
        public Buffer IndexBuffer { get; set; }
        public Buffer VertexBuffer { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsCreated { get; set; }
        #endregion

        public Animator Animator { get; private set; }

        public Action OnAnimationChanged;

        public bool Animated
        {
            get { return AnimationData != null && AnimationData.HasAnimations; }
        }

        #region Transformable
        public Matrix Translation { get; set; }
        public Matrix Scale { get; set; }
        public Matrix Rotating { get; set; }

        public Vector3 Position { get; set; }

        public Matrix World
        {
            get { return Scale*Rotating*Translation; }
        }

        #endregion

        #region Creating
        public CustomMesh(string filename)
        {
            //Might happen data doesn't get overwritten when creating a new mesh
            ResetMesh();

            //Choose the importer based on the extension of the file
            var importer = Importer.GetImporterFromExtension(filename);
            //importer = Importer.GetImporter(ImporterType.ASSIMP);

            if (importer == null)
                return;

            //Import the data
            importer.ReadFromFile(filename);
            SetData(importer);

            
        }

        public void Update()
        {
            Translation = Matrix.Translation(Position);
            Scale = Matrix.Scaling(1.0f);
            Rotating = Matrix.RotationYawPitchRoll(0, 0, 0);
        }

        private void SetData(IMeshImporter importer)
        {

            if(importer.MeshData == null)
                return;

            //Base Mesh info ( All meshes need this)
            MeshData = importer.MeshData;

           

            //Check if the imported data contains animations
            if (importer is IAnimationImporter)
            {
                var animImporter = importer as IAnimationImporter;
                AnimationData = animImporter.AnimationData;
            }

            //Create a skinned Vertex if the loaded data has animations
            if (Animated)
            {
                VertexStride = Marshal.SizeOf(typeof (SkinnedVertex));
                SkinnedVertices = new List<SkinnedVertex>();
                for (var i = 0; i < MeshData.VertexCount; ++i)
                {
                    var pos = MeshData.VertexPositions[i];
                    var norm = MeshData.VertexNormals[i];
                    var blendWeight = AnimationData.BlendWeights[i];
                    var blendIndex = AnimationData.BlendIndices[i];
                    SkinnedVertices.Add(new SkinnedVertex(pos, Color.Gray, norm, blendIndex, blendWeight));
                }

                //Set up the Animator
                Animator = new Animator(this);
                if (AnimationData.Animations.Count != 0)
                {
                    Animator.SetAnimation(AnimationData.Animations[0].Name);
                    Animator.Play();
                }
        }

            //Create a normal vertex
            else
            {
                VertexStride = Marshal.SizeOf(typeof(VertexPosColNorm));
                Vertices = new List<VertexPosColNorm>();
                for (var i = 0; i < MeshData.VertexCount; ++i)
                {
                    var pos = MeshData.VertexPositions[i];
                    var norm = MeshData.VertexNormals[i];
                    Vertices.Add(new VertexPosColNorm(pos, Color.Gray, norm));
                }
            }

            IsLoaded = true;
        }

        /// <summary>
        /// Clear all the things
        /// </summary>
        private void ResetMesh()
        {
            IsCreated = false;
            IsLoaded = false;

            if (Animator != null)
            {
                Animator.Reset();
                Animator = null;
            }

            if (SkinnedVertices != null)
                SkinnedVertices.Clear();

            if (Vertices != null)
                Vertices.Clear();

        }

        
        public void Create(Device1 device)
        {
            PrimitiveTopology = PrimitiveTopology.TriangleList;
            CreateVertexBuffer(device);
            CreateIndexBuffer(device);

            IsCreated = true;
        }


        public void CreateVertexBuffer(Device device)
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();

            var count = 0;
            count = Animated ? SkinnedVertices.Count : Vertices.Count;

            var desc = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Immutable,
                SizeInBytes = VertexStride * count
            };

            //Create Vertex buffer
            VertexBuffer =Animated ? 
                new Buffer(device, DataStream.Create(SkinnedVertices.ToArray(), false, false), desc) : 
                new Buffer(device, DataStream.Create(Vertices.ToArray(), false, false), desc);
        }

        private void CreateIndexBuffer(Device device)
        {

            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            var desc = new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Immutable,
                SizeInBytes = sizeof(uint) * MeshData.IndexCount
            };

            IndexBuffer = new Buffer(device, DataStream.Create(MeshData.Indices.ToArray(), false, false), desc);

        }

        #endregion

        public void AddAnimation(AnimationClip clip)
        {

            if(AnimationData.Animations.Contains(clip) || (AnimationData.Animations.Any(c => c.Name == clip.Name)))
                return;

          

            Debug.Log(LogLevel.Info, "Added Animation");


            AnimationData.Animations.Add(clip);

            if (OnAnimationChanged != null)
                OnAnimationChanged();
        }

        public void RemoveAnimation(AnimationClip clip)
        {
            if (!AnimationData.Animations.Contains(clip))
                return;

            AnimationData.Animations.Remove(clip);

            if (OnAnimationChanged != null)
                OnAnimationChanged();
        }
    }
}
