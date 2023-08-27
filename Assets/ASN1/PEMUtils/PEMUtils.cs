using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ASN1
{
    internal static class PEMUtils
    {
        public static byte[] ExtractPEM(string pem)
        {
            var match = Regex.Match(pem, @"^(?<header>\-+\s?BEGIN[^-]+\-+)\s*(?<body>[^-]+)\s*(?<footer>\-+\s?END[^-]+\-+)\s*$");

            string value = match.Groups["body"].Value;
            return Convert.FromBase64String(Regex.Replace(value, @"\s+", string.Empty));
        }

        public static string AssemblePEM(ArraySegment<byte> der, string identifier, int maxlinelength = 64)
        {
            string base64 = Convert.ToBase64String(der);

            StringBuilder pem = new();

            pem.Append($"-----BEGIN {identifier}-----\n");
            for(int i = 0; i < base64.Length; i += maxlinelength)
            {
                pem.Append(base64.Substring(i, Math.Min(maxlinelength, base64.Length - i)));
                pem.Append("\n");
            }
            pem.Append($"-----END {identifier}-----\n");

            return pem.ToString();
        }
    }
}
