using System;

namespace Debugify
{
    public enum InfoLevel
    {
        Simple,
        Extended
    }

    /// <summary>
    /// contains extra info about the DebugLog
    /// </summary>
    public class DebugInfo
    {
        /// <summary>
        /// the message send to the log
        /// </summary>
        public string _Message { get; set; }

        /// <summary>
        /// the level of the log
        /// </summary>
        public LogLevel Level { get; private set; }
        
        /// <summary>
        /// Time the Log happened
        /// </summary>
        private readonly DateTime _Time;

        public bool Delete { get; set; }

        public int Count { get; set; }

        public DebugInfo(LogLevel level, string message)
        {
            _Message = message;
            Level = level;
            _Time = DateTime.Now;
            Delete = false;

        }

        /// <summary>
        /// Log level and message
        /// </summary>
        /// <returns></returns>

        public string SimpleToString()
        {
            return string.Format("[{0}] {1}", Level,_Message);
        }

        /// <summary>
        /// Log Extra function
        /// </summary>
        /// <returns></returns>
        public string ExtendedToString()
        {
            if(Count > 0)
                return string.Format("[{0}][{1}]{2} [{3}]", _Time, Level, _Message, Count);

            return string.Format("[{0}][{1}]{2}", _Time, Level,_Message);
            
        }

       
        public override string ToString()
        {
            return SimpleToString();
        }
    }
}
