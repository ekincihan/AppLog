using System;

namespace AppLog.Logging.GrayLog
{
    [Serializable]
    internal enum SyslogLevel
    {
        Emergency = 0,
        Alert = 1,
        Critical = 2,
        Error = 3,
        Warning = 4,
        Notice = 5,
        Informational = 6,
        Debug = 7,
    }
}
