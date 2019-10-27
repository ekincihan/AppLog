using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
