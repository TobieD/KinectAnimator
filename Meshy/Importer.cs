using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Meshy.Importers;
using Meshy.Interfaces;

namespace Meshy
{
    public enum ImporterType
    {
        NONE,
        OVM,
        FBX,
        ASSIMP
    }

    /// <summary>
    /// Stores all available Importers
    /// </summary>
    public static class Importer
    {

        /// <summary>
        /// Stores the importers, all are unique, using the ImporterType as key
        /// </summary>
        private static readonly Hashtable AvailableImporters = new Hashtable();

        /// <summary>
        /// conversion from string to ImporterType
        /// </summary>
        private static readonly Hashtable ImporterTypes = new Hashtable(); 

        /// <summary>
        /// Setup the default Importers
        /// </summary>
        public static void SetupImporters()
        {
            AddImporter(ImporterType.FBX, new FbxImporter());
            AddImporter(ImporterType.OVM, new OvmImporter());
            //AddImporter(ImporterType.ASSIMP, new AssimpImporter());
        }

        /// <summary>
        /// Add a new Importer
        /// </summary>
        private static void AddImporter(ImporterType key, IMeshImporter importer)
        {
            AvailableImporters.Add(key, importer);

            ImporterTypes.Add(key.ToString(), key);
        }

        /// <summary>
        /// Returns an importer by type
        /// </summary>
        /// <param name="type"> File format to load</param>
        /// <returns> Importer for that type</returns>
        public static IMeshImporter GetImporter(ImporterType type)
        {
            if (AvailableImporters.ContainsKey(type))
                return AvailableImporters[type] as IMeshImporter;

            return AvailableImporters[ImporterType.NONE] as IMeshImporter; ;
        }

        /// <summary>
        /// Returns an importer by the extension of the given filename
        /// </summary>
        /// <param name="filename"> Filename </param>
        /// <returns> Importer for that filename</returns>
        public static IMeshImporter GetImporterFromExtension(string filename)
        {
            var extension = Path.GetExtension(filename).Replace(".","").ToUpper();

            var importer = GetImporter(TypeFromString(extension));

            //Check for assimp types
            if (importer == null && AssimpImporter.AvaliableFormats.Contains("."  + extension.ToLower()))
            {   
                return GetImporter(ImporterType.ASSIMP);
            }
            return importer;
        }

        /// <summary>
        /// Returns an importerType based on a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static ImporterType TypeFromString(string s)
        {
            if (ImporterTypes.ContainsKey(s))
                return (ImporterType)ImporterTypes[s];

            return ImporterType.NONE;
        }

        /// <summary>
        /// Get all types as a string for use in the OpenDialog or SaveDialog Filter
        /// </summary>
        public static string AvailableExtensions()
        {
            var sb = new StringBuilder();
            var i = 0;

            //Start of new
            foreach (string importKey in ImporterTypes.Keys)
            {
                //Skip first
                if (i != 0)
                    sb.Append(";");
                
                if (importKey == ImporterType.ASSIMP.ToString())
                {
                    foreach (string avaliableFormat in AssimpImporter.AvaliableFormats)
                    {
                         sb.Append("*.");
                        sb.Append(avaliableFormat);
                    }
                }
                else
                {
                     sb.Append("*.");
                     sb.Append(importKey);
                }

                    i++;
            }

            return sb.ToString();
        }

    }
}
