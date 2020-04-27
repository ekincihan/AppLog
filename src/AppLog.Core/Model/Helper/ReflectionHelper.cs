using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AppLog.Core.Model.Helper
{
    public static class ReflectionHelper
    {
        public static string WhoseThere([CallerFilePath] string filePath = "")
        {
            var mth = new StackTrace().GetFrame(2).GetMethod();
            var np = mth.ReflectedType.FullName;
            return $"{np}";
        }
    }
}
