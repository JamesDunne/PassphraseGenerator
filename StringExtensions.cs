using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class StringExtensions
    {
        /// <summary>
        /// Filters out a set of characters from the string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string Filter(this string value, params char[] filter)
        {
            if (value == null) return null;

            StringBuilder sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (filter.Contains(c)) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Formats a string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string F(this string format, params object[] args)
        {
            if (args == null || args.Length == 0) return format;
            return String.Format(format, args);
        }
    }
}
