#region using

using System.Text.RegularExpressions;

#endregion

namespace ReviewBot.Utility
{
    public static class StringExtensions
    {
        public static string StripNewLineAndTrim(this string str)
        {
            return str == null ? null : Regex.Replace(str.Trim(), @"\t|\n|\r", "");
        }
    }
}