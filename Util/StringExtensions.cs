using System;
using System.Collections.Generic;
using System.Text;

namespace WinCron.Util
{
    public static class StringExtensions
    {
        public static string Fmt(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static string StringJoin(this IEnumerable<string> strings, string separator = ",")
        {
            var sBuilder = new StringBuilder();
            foreach (var s in strings)
            {
                sBuilder.Append(s).Append(separator);
            }

            if (sBuilder.Length == 0)
            {
                return string.Empty;
            }

            return sBuilder.ToString(0, sBuilder.Length - 1);
        }

        public static string[] SplitOn(this string s, char c = ',')
        {
            return s.Split(new[] { c }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}