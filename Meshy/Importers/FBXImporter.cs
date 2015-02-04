using System;
using System.IO;
using System.Linq;
using ManagedFbx;
using Meshy.Interfaces;

namespace Meshy.Importers
{
    /// <summary>
    /// Load in a single mesh from an fbx file using ManagedFbx
    /// </summary>
    public class FbxImporter : IMeshImporter
    {
        public MeshData MeshData { get; private set; }

        public void ReadFromFile(string filename)
        {
            //Check if file exists
            if (!File.Exists(filename))
                return;
           
            //Open the File
            var fbxScene = Scene.Import(filename);

            MeshData = new MeshData();

            //Node can be a mesh or a skeleton
            foreach (var node in fbxScene.RootNode.ChildNodes)
            {
                foreach (var attr in node.Attributes)
                {
                    switch (attr.Type)
                    {
                        //For Bones
                        case NodeAttributeType.Skeleton:
                            var skeleton = node.Skeleton;

                            break;

                        //for mesh data
                        case NodeAttributeType.Mesh:
                            var mesh = node.Mesh;
                            MeshData.Name = node.Name;

                            //Triangulate mesh
                            if (!mesh.Triangulated)
                                mesh.Triangulate();

                            //Indices
                            foreach (var index in mesh.Polygons.SelectMany(t => t.Indices))
                                MeshData.Indices.Add(index);

                            MeshData.IndexCount = MeshData.Indices.Count;
                            MeshData.VertexCount = mesh.Vertices.Count();

                            //Vertices
                            MeshData.VertexPositions.AddRange(
                                mesh.Vertices.Select(
                                    vertex => new SharpDX.Vector3((float)vertex.X, (float)vertex.Y, (float)vertex.Z)));

                            //Normal
                            MeshData.VertexNormals.AddRange(
                                mesh.Normals.Select(
                                    n => new SharpDX.Vector3((float)n.X, (float)n.Y, (float)n.Z)));

                            //Texcoords
                            MeshData.VertexTextureCoordinates.AddRange(mesh.TextureCoords.Select(t => new SharpDX.Vector2((float)t.X, (float)t.Y)));

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

        }
    }
}
