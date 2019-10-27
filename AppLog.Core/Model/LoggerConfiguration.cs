﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppLog.Core.Model
{
    public class LoggerConfiguration
    {
        public string LoggerName { get; set; }
        public string GrayLogHost { get; set; }
        public int GrayLogPort { get; set; }
        public string ApplicationId { get; set; }
        public LogLevel MinimumLogLevel { get; set; }
    }  
}
