#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Review.Core.Utility;

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

    public static string NormalizeDiacritics(this string str)
    {
        var sb = new StringBuilder();
        foreach (var b in str.Normalize(NormalizationForm.FormD).Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            sb.Append(b);
        }

        return sb.ToString();
    }
}