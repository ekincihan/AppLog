using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AppLog.Core.Abstract
{
    public interface ILogger
    {
        ILogger Current([CallerMemberName]string name = "");
        string CallerMemberName { get; set; }
        void Debug(params object[] logItems);
        void Info(params object[] logItems);
        void Warning(params object[] logItems);
        void Error(params object[] logItems);
        void Critical(params object[] logItems);
        Task DebugAsync(params object[] logItems);
        Task InfoAsync(params object[] logItems);
        Task WarningAsync(params object[] logItems);
        Task ErrorAsync(params object[] logItems);
        Task CriticalAsync(params object[] logItems);
    }
}
