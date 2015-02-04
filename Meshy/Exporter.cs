using System.Collections;
using System.Text;
using Meshy.Exporters;
using Meshy.Interfaces;

namespace Meshy
{
    public enum ExportType
    {
        NONE,
        OVM
    }

    /// <summary>
    /// Stores all available Importers
    /// </summary>
    public static class Exporter
    {

        /// <summary>
        /// Stores the importers, all are unique, using the ImporterType as key
        /// </summary>
        private static readonly Hashtable AvailableExporters = new Hashtable();

        /// <summary>
        /// conversion from string to ImporterType
        /// </summary>
        private static readonly Hashtable ExporterTypes = new Hashtable(); 

        /// <summary>
        /// Setup the default Importers
        /// </summary>
        public static void SetupExporters()
        {
            AddExporter(ExportType.OVM, new OvmExporter());
        }

        /// <summary>
        /// Add a new Importer
        /// </summary>
        private static void AddExporter(ExportType key, IMeshExporter exporter)
        {
            AvailableExporters.Add(key, exporter);

            ExporterTypes.Add(key.ToString(), key);
        }

        /// <summary>
        /// Returns an importer by type
        /// </summary>
        /// <param name="type"> File format to load</param>
        /// <returns> Importer for that type</returns>
        public static IMeshExporter GetImporter(ExportType type)
        {
            if (AvailableExporters.ContainsKey(type))
                return AvailableExporters[type] as IMeshExporter;

            return AvailableExporters[ExportType.NONE] as IMeshExporter; ;
        }

        /// <summary>
        /// Returns an importer by the extension of the given filename
        /// </summary>
        /// <param name="filename"> Filename </param>
        /// <returns> Importer for that filename</returns>
        public static IMeshExporter GetImporterFromExtension(string filename)
        {
            var fileExtPos = filename.LastIndexOf(".", System.StringComparison.Ordinal);

            var extension = "";

            if (fileExtPos >= 0)
                extension = filename.Substring(fileExtPos + 1);

            return GetImporter(TypeFromString(extension.ToUpper()));
        }

        /// <summary>
        /// Returns an importerType based on a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static ExportType TypeFromString(string s)
        {
            if (ExporterTypes.ContainsKey(s))
                return (ExportType)ExporterTypes[s];

            return ExportType.NONE;
        }

        /// <summary>
        /// Get all types as a string for use in the OpenDialog or SaveDialog Filter
        /// </summary>
        public static string AvailableExtensions()
        {
            var sb = new StringBuilder();
            var i = 0;

            //Start of new
            foreach (string importKey in ExporterTypes.Keys)
            {
                //Skip first
                if (i != 0)
                    sb.Append(";");

                sb.Append("*.");
                sb.Append(importKey);
                i++;
            }

            return sb.ToString();
        }

    }
}
