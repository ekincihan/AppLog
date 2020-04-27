using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AppLog.Core.Model.Helper
{
    public static class ExceptionHelper
    {
        public static string ToLogString(this Exception exception, string environmentStackTrace)
        {
            List<string> environmentStackTraceLines = ExceptionHelper.GetUserStackTraceLines(environmentStackTrace);
            environmentStackTraceLines.RemoveAt(0);

            List<string> stackTraceLines = ExceptionHelper.GetStackTraceLines(exception.StackTrace);
            stackTraceLines.AddRange(environmentStackTraceLines);

            string fullStackTrace = String.Join(System.Environment.NewLine, stackTraceLines);

            return $"{exception.Message}{System.Environment.NewLine}{fullStackTrace}";
        }
        private static List<string> GetStackTraceLines(string stackTrace)
        {
            return stackTrace.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None).ToList();
        }

        private static List<string> GetUserStackTraceLines(string fullStackTrace)
        {
            List<string> outputList = new List<string>();
            Regex regex = new Regex(@"([^\)]*\)) in (.*):line (\d)*$");

            List<string> stackTraceLines = ExceptionHelper.GetStackTraceLines(fullStackTrace);
            foreach (string stackTraceLine in stackTraceLines)
            {
                if (!regex.IsMatch(stackTraceLine))
                {
                    continue;
                }

                outputList.Add(stackTraceLine);
            }

            return outputList;
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
        this TSource source,
        Func<TSource, TSource> nextItem,
        Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        public static string GetaAllMessages(this Exception exception)
        {
            var messages = exception.FromHierarchy(ex => ex.InnerException)
                .Select(ex => ex.Message);
            return String.Join(System.Environment.NewLine, messages);
        }
    }
}
