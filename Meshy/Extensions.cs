using Assimp;
using SharpDX;

namespace Meshy
{
    public static class Extensions
    {
        public static Vector3 ToVector3(this Vector3D vec)
        {
            return new Vector3(vec.X,vec.Y,vec.Z);
        }
    }
}
