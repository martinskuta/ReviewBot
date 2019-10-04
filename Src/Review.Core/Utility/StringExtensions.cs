#region copyright

// Copyright 2007 - 2019 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

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

        public static IList<string> SplitWords(this string str)
        {
            return str.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static IList<string> GetWordsBeforePhrase(this string str, string phrase)
        {
            return str.Substring(0, str.IndexOf(phrase, StringComparison.OrdinalIgnoreCase)).SplitWords();
        }
    }
}