using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Debugify
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
    
    /// <summary>
    /// Handle Debugging
    /// </summary>
    public static class Debug
    {
        public static event Action<DebugInfo> OnLog;
        public static event Action<List<DebugInfo>> OnSave;

        private static readonly List<DebugInfo> _LogHistory = new List<DebugInfo>();

        /// <summary>
        /// Log the level and message to the console
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public static void Log(LogLevel level, string message)
        {
            if (OnLog == null)
            {
                Console.WriteLine("No Debuglogger set");
                return;
            }

            //Gt new log
            var newInfo = new DebugInfo(level, message);

            OnLog(newInfo);
            _LogHistory.Add(newInfo);
        }

        public static void Save()
        {
            if (OnSave == null)
            {
                Log(LogLevel.Error,"No save set");
                return;
            }

            //Save the history to the given path
            if (_LogHistory.Count >0)
                OnSave(_LogHistory);
        }

        public static void LogException(Exception e)
        {
            var error = e.InnerException ?? e;

            //message of the exception
            var message = error.Message;

            var st = new StackTrace(error, true);
            var errorLine = st.GetFrame(0).GetFileLineNumber();
            var errorFile = st.GetFrame(0).GetFileName();

            if (errorFile != null)
            {

                var sourceProject = error.Source;
                var index = errorFile.LastIndexOf(sourceProject, System.StringComparison.Ordinal);
                errorFile = errorFile.Substring(index);
            }


            Debug.Log(LogLevel.Error, string.Format("[{0}::{1}] {2}", errorFile, errorLine, message));
        }
    }
}
