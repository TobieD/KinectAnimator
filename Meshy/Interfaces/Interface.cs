namespace Meshy.Interfaces
{

    public interface IMesh
    {
        MeshData MeshData { get; }
        AnimationData AnimationData { get; }
    }


    public interface IMeshImporter
    {
        MeshData MeshData { get; }

        void ReadFromFile(string filename);
    }

    public interface IAnimationImporter
    {
        
        AnimationData AnimationData { get; }
    }

    public interface IMeshExporter
    {
        MeshData MeshData { get; }

        void WriteToFile(string filename,IMesh mesh);

    }


}
