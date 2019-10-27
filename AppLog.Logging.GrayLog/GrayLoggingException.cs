using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace AppLog.Logging.GrayLog
{
    [Serializable]
    public class GrayLoggingException : Exception
    {
        public GrayLoggingException()
            : this("An exception occurred.")
        { }

        public GrayLoggingException(string message)
            : this(message, null)
        { }

        public GrayLoggingException(string message, Exception innerException)
            : base(message, innerException)
        { }
        protected GrayLoggingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
