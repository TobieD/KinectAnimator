using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;
using Debug = Debugify.Debug;

namespace Debugify
{
    /// <summary>
    /// Different functions to hook into the Debuggger
    /// </summary>
    public static class DebugTask
    {

        /// <summary>
        /// Just some customization
        /// </summary>
        private static string _extension = "txt";

        public static string DebugFileExtension
        {
            get { return _extension; }
            set { _extension = value; }
        }

        private static string _folder = "DebugLog/";

        public static string DebugFolder
        {
            //only send back when filled in
            get { return _folder != string.Empty ? string.Format("{0}/", _folder) : string.Empty;
            }
            set { _folder = value; }
        }


        //Log Handlers
        public static void SimpleLog(DebugInfo info)
        {
            SetConsoleColor(info.Level);
            Console.WriteLine(info.SimpleToString());
            ResetConsoleColor();
        }

        public static void ExtendedLog(DebugInfo info)
        {
            SetConsoleColor(info.Level);
            Console.WriteLine(info.ExtendedToString());
            ResetConsoleColor();
            
        }

        public static void SimpleLogNoColor(DebugInfo info)
        {
            Console.WriteLine(info.SimpleToString());
        }

        public static void ExtendedLogNoColor(DebugInfo info)
        {
            Console.WriteLine(info.ExtendedToString());

        }

        //Save handlers

        /// <summary>
        /// Save to a file
        /// </summary>
        /// <param name="logHistory"></param>
        public static void SaveToFile(List<DebugInfo> logHistory)
        {
            
            //Create the file, all files will be unique
            CreateDirectory(DebugFolder);
            var file = new FileInfo(GenerateFilename());

            logHistory.Add(new DebugInfo(LogLevel.Info,"Saving log to file"));

            //Check for duplicate lines
            CheckForDuplicates(ref logHistory);

            using (var writer = file.OpenWrite())
            {
                foreach (var log in logHistory)
                {
                    var bytes = Encoding.UTF8.GetBytes(log.ExtendedToString() + Environment.NewLine);
                    writer.Write(bytes,0,bytes.Length);
                }
            }
        }

        /// <summary>
        /// when a crash occurs log the exception and save th know history
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnCrash(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.LogException(e.Exception);
            //Save the loghistory
            Debug.Save();

        }

        /// <summary>
        /// Gets info out of an exception and logs it to the debugger
        /// </summary>
        /// <param name="e"></param>
      

        private static void SetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }
        }
        private static void ResetConsoleColor()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        private static string GenerateFilename()
        {
            var currentTime = DateTime.Now;
            var builder = new StringBuilder();

            //Directory
            builder.Append(DebugFolder);

            //FileName
            builder.Append(string.Format("{0}{1}",currentTime.DayOfWeek.ToString()[0],currentTime.DayOfWeek.ToString()[1]));
            builder.Append("-");
            builder.Append(currentTime.Month);
            builder.Append("-");
            builder.Append(currentTime.Year);
            builder.Append("-");
            builder.Append(currentTime.Hour);
            builder.Append("-");
            builder.Append(currentTime.Minute);
            builder.Append("-");
            builder.Append(currentTime.Second);

            //Extension
            builder.Append("." + DebugFileExtension);

            return builder.ToString();

        }
        private static void CreateDirectory(string folder)
        {
            if (Directory.Exists(folder))
                return;

            Directory.CreateDirectory(folder);
        }

        private static void CheckForDuplicates(ref List<DebugInfo> log)
        {
            var prevInfo = log[0];
            var duplicateCount = 0;

            for (var i = 1; i < log.Count; ++i)
            {
                //Compare previous message with current message
                var line = log[i]._Message;
                var prevLine = prevInfo._Message;

                if (line == prevLine)
                {
                    //mark previous for delete if they are the same
                    duplicateCount++;
                    prevInfo.Delete = true;
                    prevInfo = log[i];
                }
                else
                {
                    prevInfo.Count = duplicateCount;
                    duplicateCount = 0;
                    prevInfo = log[i];
                }

            }

            log.RemoveAll(i => i.Delete == true);

           
        }
    }
}
