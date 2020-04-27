using System.Collections.Generic;

namespace AppLog.Core.Model
{
    public sealed class LogEntry 
    {
        public LogEntry()
        {
        }
        public LogEntry(string shortMessage)
        : this()
        {
            ShortMessage = shortMessage;
        }
        public LogEntry(string shortMessage, string errorCode)
        : this(shortMessage)
        {
            ErrorCode = errorCode;
        }
        public string Version { get; set; }
        public string ApplicationId { get; set; }
        public string ErrorCode { get; set; }
        public string ShortMessage { get; set; }
        public string Message { get; set; }
        public KeyValuePair<string, object> AdditionalData { get; set; }
        public Environment Environment { get; set; }
        public byte[] Screenshot { get; set; }
        public LogLevel LogLevel { get; set; }
        public string LoggerName { get; set; }
        public string StackTrace { get; set; }
    }
}