using System.Collections.Generic;
using SharpDX;

namespace Meshy
{
    /// <summary>
    /// KeyFrame of an animation
    /// </summary>
    public class AnimationKey
    {
        public AnimationKey()
        {
            BoneTransforms = new List<Matrix>();
        }

        public float Tick { get; set; }
        public List<Matrix> BoneTransforms { get; set; }
    }

    /// <summary>
    /// Complete animation build up from animation keys
    /// </summary>
    public class AnimationClip
    {
        public AnimationClip()
        {
            Keys = new List<AnimationKey>();
            Name = "Unnamed Animation";
            Duration = 9600.0f;
            TicksPerSecond = 4800.0f;
        }

        public string Name { get; set; }
        public float Duration { get; set; }
        public float TicksPerSecond { get; set; }

        public List<AnimationKey> Keys { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public AnimationClip Copy()
        {
            var clip = new AnimationClip();
            clip.Duration = this.Duration;
            clip.Name = this.Name;
            clip.TicksPerSecond = this.TicksPerSecond;

            return clip;
        }
    }

    /// <summary>
    /// Represents a joint in the skeleton hierarchy
    /// </summary>
    public class AnimationJoint
    {
        public int Index { get; set; }

        public string Name { get; set; }

        public int ParentIndex { get; set; }

        public Matrix OffsetMatrix { get; set; }

        public Matrix LocalMatrix { get; set; }
        public Matrix GlobalMatrix { get; set; }

        public bool IsRoot { get; set; }
        public bool IsValid { get; set; }

        public AnimationJoint()
        {
            Name = "Not Set";
            IsValid = false;
            IsRoot = false;
        }
        public override string ToString()
        {
            return string.Format(Name);
        }

    }

    /// <summary>
    /// Data about the mesh
    /// </summary>
    public class MeshData
    {
        public int VertexCount { get; set; }
        public List<Vector3> VertexPositions { get; set; }
        public List<Vector3> VertexNormals { get; set; }
        public List<Vector2> VertexTextureCoordinates { get; set; }
        public int IndexCount { get; set; }
        public List<int> Indices { get; set; }

        public string Name { get; set; }

        public MeshData()
        {
            Name = string.Empty;
            VertexPositions = new List<Vector3>();
            VertexNormals = new List<Vector3>();
            VertexTextureCoordinates = new List<Vector2>();
            IndexCount = 0;
            Indices = new List<int>();
        }
    }

    /// <summary>
    /// Data about animations
    /// </summary>
    public class AnimationData
    {
        //Animations
        public List<Vector4> BlendIndices { get; set; }

        public List<Vector4> BlendWeights { get; set; }

         public List<AnimationClip> Animations { get; set; }

        public bool HasAnimations { get; set; }
        public int BoneCount { get; set; }

        public List<AnimationJoint> Bones { get; set; } 

        public AnimationData()
        {
            BlendIndices = new List<Vector4>();
            BlendWeights = new List<Vector4>();
            Bones = new List<AnimationJoint>();

            Animations = new List<AnimationClip>();
            BoneCount = 0;
            HasAnimations = false;
        }
    }
}
