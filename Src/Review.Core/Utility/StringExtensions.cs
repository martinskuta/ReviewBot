#region using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Review.Core.Utility
{
    public static class StringExtensions
    {
        public static bool ContainsAnyOrdinal(this string str, params string[] values)
        {
            return values.Any(value => str.Contains(value, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ContainsAnyOrdinal(this IList<string> listOfStrings, params string[] values)
        {
            return values.Any(value => listOfStrings.Contains(value, StringComparer.OrdinalIgnoreCase));
        }
    }
}