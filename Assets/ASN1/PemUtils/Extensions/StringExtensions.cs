﻿using System.Text.RegularExpressions;

namespace PemUtils
{
    internal static class StringExtensions
    {
        public static string RemoveWhitespace(this string input) 
            => Regex.Replace(input, @"\s+", string.Empty);

    }
}
