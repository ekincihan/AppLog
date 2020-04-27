using AppLog.Core.Model;
using AppLog.Core.Model.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AppLog.Logging.GrayLog
{
    public class GrayLogLogger<T> : Core.Abstract.ILogger<T> 
    {
        #region Fields
        private readonly LoggerConfiguration _loggerConfiguration = new LoggerConfiguration
        {
            ApplicationId = ApplicationHelper.GetAppConfigValue<string>("AppLog-ApplicationId"),
            LoggerName = ApplicationHelper.GetAppConfigValue<string>("AppLog-LoggerName"),
            GrayLogHost = ApplicationHelper.GetAppConfigValue<string>("AppLog-GrayLogHost"),
            GrayLogPort = ApplicationHelper.GetAppConfigValue<int>("AppLog-GrayLogPort"),
            MinimumLogLevel = ApplicationHelper.GetAppConfigValue<LogLevel>("AppLog-MinimumLogLevel")
        };

        public string CallerMemberName { get; set; }
        public string Namespace { get; set; }
        #endregion Fields
        public GrayLogLogger()
        {
            Namespace = typeof(T).FullName;
        }
        public Core.Abstract.ILogger<T> Current([CallerMemberName]string name = "")
        {
            CallerMemberName = $"{Namespace}.{name}";
            return this;
        }
        public void Debug(params object[] logItems)
        {
            DoLog(logItems, LogLevel.Debug, CallerMemberName);
        }

        public void Info(params object[] logItems)
        {
            DoLog(logItems, LogLevel.Information, CallerMemberName);
        }
        public void Warning(params object[] logItems)
        {
            DoLog(logItems, LogLevel.Warning, CallerMemberName);
        }
        public void Error(params object[] logItems)
        {
            DoLog(logItems, LogLevel.Error, CallerMemberName);
        }
        public void Critical(params object[] logItems)
        {
            DoLog(logItems, LogLevel.Critical, CallerMemberName);
        }

        private bool IsLevelLoggingEnabled(LogLevel logLevel)
        {
            return logLevel >= _loggerConfiguration.MinimumLogLevel;
        }
       
        public async Task DebugAsync(params object[] logItems)
        {
            await DoLogAsync(logItems, LogLevel.Debug, CallerMemberName);
        }
        public async Task InfoAsync(params object[] logItems)
        {
            await DoLogAsync(logItems, LogLevel.Information, CallerMemberName);
        }
        public async Task WarningAsync(params object[] logItems)
        {
            await DoLogAsync(logItems, LogLevel.Warning, CallerMemberName);
        }
        public async Task ErrorAsync(params object[] logItems)
        {
            await DoLogAsync(logItems, LogLevel.Error, CallerMemberName);
        }
        public async Task CriticalAsync(params object[] logItems)
        {
            await DoLogAsync(logItems, LogLevel.Critical, CallerMemberName);
        }
        private async Task DoLogAsync(LogEntry entry, LogLevel logLevel)
        {
            if (!IsLevelLoggingEnabled(logLevel))
            {
                return;
            }

            entry.LogLevel = logLevel;
            entry.Environment = new Core.Model.Environment();
            entry.ApplicationId = _loggerConfiguration.ApplicationId;
            entry.Environment.ApplicationPath = ApplicationHelper.GetExecutingPath();
            entry.Environment.IpAddress = ApplicationHelper.GetLocalIpAddresses();
            entry.Environment.MachineName = ApplicationHelper.GetHostname();
            entry.LoggerName = _loggerConfiguration.LoggerName;
            entry.StackTrace = logLevel >= LogLevel.Error ? System.Environment.StackTrace : string.Empty;

            await this.PostLogAsync(entry) ;
        }
        private async Task DoLogAsync(object obj, LogLevel logLevel, string shortMessage = "")
        {
            try
            {
                var entry = new LogEntry();
                if (obj is Exception exception)
                {

                    entry.ShortMessage = shortMessage.ConvertTurkishCharToEnglishChar();

                    entry.Message = exception.GetaAllMessages().ConvertTurkishCharToEnglishChar();
                }
                else
                {
                    if (shortMessage != string.Empty)
                    {
                        entry.ShortMessage = shortMessage.ConvertTurkishCharToEnglishChar();
                    }

                    var logItems = new List<object>();

                    if (obj is object[] objectArray)
                    {
                        foreach (var logItem in objectArray)
                        {
                            try
                            {
                                if (logItem is Exception logItemException)
                                {
                                    logItems.Add(logItemException.GetaAllMessages());
                                }
                                else
                                {
                                    logItems.Add(logItem);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    entry.Message = Newtonsoft.Json.JsonConvert.SerializeObject(logItems);
                }

                await DoLogAsync(entry, logLevel);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        private void DoLog(object obj, LogLevel logLevel, string shortMessage = "")
        {
            try
            {
                var entry = new LogEntry();
                if (obj is Exception exception)
                {
                    
                    entry.ShortMessage = shortMessage.ConvertTurkishCharToEnglishChar();

                    entry.Message = exception.GetaAllMessages().ConvertTurkishCharToEnglishChar();
                }
                else
                {
                    if (shortMessage != string.Empty)
                    {
                        entry.ShortMessage = shortMessage.ConvertTurkishCharToEnglishChar();
                    }

                    var logItems = new List<object>();

                    if (obj is object[] objectArray)
                    {
                        foreach (var logItem in objectArray)
                        {
                            try
                            {
                                if (logItem is Exception logItemException)
                                {
                                    logItems.Add(logItemException.GetaAllMessages());
                                }
                                else
                                {
                                    logItems.Add(logItem);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    entry.Message = Newtonsoft.Json.JsonConvert.SerializeObject(logItems);
                }

                DoLog(entry, logLevel);
            }
            catch (Exception ex)
            {
               
                // ignored
            }
        }

        private void DoLog(LogEntry entry, LogLevel logLevel)
        {
            if (!IsLevelLoggingEnabled(logLevel))
            {
                return;
            }

            entry.LogLevel = logLevel;
            entry.Environment = new Core.Model.Environment();
            entry.ApplicationId = _loggerConfiguration.ApplicationId;
            entry.Environment.ApplicationPath = ApplicationHelper.GetExecutingPath();
            entry.Environment.IpAddress = ApplicationHelper.GetLocalIpAddresses();
            entry.Environment.MachineName = ApplicationHelper.GetHostname();
            entry.LoggerName = _loggerConfiguration.LoggerName;
            entry.StackTrace = logLevel >= LogLevel.Error ? System.Environment.StackTrace : string.Empty;

            this.PostLog(entry);
        }

        private void PostLog(LogEntry logEntry)
        {
            using (var grayLogUdpClient = new GrayLogUdpClient(_loggerConfiguration.ApplicationId, _loggerConfiguration.GrayLogHost, _loggerConfiguration.GrayLogPort))
            {
                grayLogUdpClient.Send(logEntry.ShortMessage.ConvertTurkishCharToEnglishChar(), logEntry.Message.ConvertTurkishCharToEnglishChar(), logEntry);
            }
        }
        private async Task PostLogAsync(LogEntry logEntry)
        {
            using (var grayLogUdpClient = new GrayLogUdpClient(_loggerConfiguration.ApplicationId, _loggerConfiguration.GrayLogHost, _loggerConfiguration.GrayLogPort))
            {
                await grayLogUdpClient.SendAsync(logEntry.ShortMessage.ConvertTurkishCharToEnglishChar(), logEntry.Message.ConvertTurkishCharToEnglishChar(), logEntry);
            }
        }

        
    }
}