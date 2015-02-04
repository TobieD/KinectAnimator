using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using SharpDX;

namespace KinectTool.Helpers
{
    public static class KinectMath
    {
        public static Vector2 SkeletonPointToScreen(SkeletonPoint point)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            if (KinectManager.Instance != null)
            {
                var depthPoint = KinectManager.Instance.CoordinateMapper.MapSkeletonPointToDepthPoint(point,
                    DepthImageFormat.Resolution640x480Fps30);
                return new Vector2(depthPoint.X, depthPoint.Y);

            }

            //The MapSkeletontoDepth only works when connected to a kinect, so trying to have the same calculation

            //scale the point
            var pX = point.X*(-640/2.0f);
            var pY = point.Y*(-480/2.0f);

            //Center the point
            pX += (640/2.0f);
            pY += (480/2.0f);
            return new Vector2(pX, pY);
        }

        public static Vector3 SkeletonPointToVector3(SkeletonPoint point)
        {
            var vec3 = new Vector3(point.X, point.Y, point.Z);
            
            vec3 *= new Vector3(50,50, 5);
            return vec3;
        }
    }

    public static class Utilities
    {
        public static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof (T)).Cast<T>();
        }

        public static Matrix ToMatrix(this Matrix4 mat)
        {

            return new Matrix(
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
                );
        }
    }

    

}

