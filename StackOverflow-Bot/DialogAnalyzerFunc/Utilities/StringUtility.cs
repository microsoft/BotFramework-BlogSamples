using System;
using System.Collections.Generic;
using System.Linq;

namespace DialogAnalyzerFunc.Utilities
{
    public static class StringUtility
    {
        /// <summary>
        /// Get combined text with delimeter or default value
        /// </summary>
        public static string GetTextOrDefault(IEnumerable<string> values, string defaultValue, string delim = ", ")
        {
            if (values?.Count() > 0 == false || values.All(v => string.IsNullOrEmpty(v) == true))
            {
                return defaultValue;
            }

            values = values.Where(v => string.IsNullOrEmpty(v) == false);

            return string.Join(delim, values).Trim();
        }
    }
}
