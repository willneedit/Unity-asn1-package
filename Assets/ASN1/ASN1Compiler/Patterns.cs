using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASN1Compiler
{
    internal class Patterns
    {
        public static string Optional(string pattern) => $"({pattern}|)";

        public const string ws = @"\s+";

        public const string comment = @"(-- .*? --|-- .*$)";

        public const string type = @"(?<type>[_a-zA-Z][_a-zA-Z0-9]*)";

        public const string identifier = @"(?<identifier>[_a-zA-Z][\-_a-zA-Z0-9]*)";

        public const string oiditem = @"((?<oidref>[_a-zA-Z][\-_a-zA-Z0-9]*)|(?<oiditem>[0-9]*))";

        public const string universalType = @"(?<type>(INTEGER|BIT STRING|OCTET STRING|NULL|OBJECT IDENTIFIER|UTF8String|SEQUENCE OF|SET OF|UTCTime))";

        public static readonly string tagdecl = @"\["+ Optional(@"(?<app>APPLICATION)" + ws) + @"(?<tagdecl>[0-9]+)\]\s*";

        private const string declaration = identifier + ws + @"::=" + ws;

        public const string complexdef = declaration + @"(?<blocklead>(INTEGER|SEQUENCE|SET|OBJECT IDENTIFIER))" + ws + @"\{(?<body>.*?)\}";


        // type name
        public const string typedef = declaration + universalType;

        // enumitem(enumvalue)
        public const string enumitemdef = identifier + @"\((?<value>[0-9]+)\)";

        //struct field: type name; // tag usage description, or nullable?
        public static readonly string fielddef = identifier + ws 
            + Optional(tagdecl + ws) 
            + Optional(@"(?<expl>(EXPLICIT|IMPLICIT))" + ws)
            + type
            + Optional(ws + @"(?<optional>OPTIONAL)");

        public static string FilterComments(string raw)
        {
            string[] lines = raw.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None);

            StringBuilder result = new();

            foreach(string line in lines)
            {
                string value = Regex.Replace(line, comment, "");

                result.Append(value);
                result.Append("\n");
            }

            return result.ToString();
        }


        public static (string, string, string[]) EmitBlockDefinition(string block)
        {
            string filtered = FilterComments(block);

            Match match = Regex.Match(filtered, complexdef, RegexOptions.Singleline);

            string ident = match.Groups["identifier"].Value;
            string type = match.Groups["blocklead"].Value;

            type = EmitStructDefinition(match, type, out string[] lines);

            return (ident, type, lines);
        }

        private static string EmitStructDefinition(Match match, string type, out string[] lines)
        {
            static string enumline(string x)
            {
                Match match = Regex.Match(x, enumitemdef);
                GroupCollection gr = match.Groups;
                if(gr["identifier"].Value == string.Empty) return null;

                return $"    {gr["identifier"]} = {gr["value"]};\n";
            }

            static string structline(string x)
            {
                Match match = Regex.Match(x, fielddef);
                GroupCollection gr = match.Groups;
                if(gr["identifier"].Value == string.Empty) return null;

                string nullable = gr["optional"].Value != string.Empty ? "?" : "";

                return $"    {gr["type"]}{nullable} {gr["identifier"]};\n";
            }

            Func<string, string> linefunc;

            switch(type)
            {
                case "SEQUENCE":
                case "SET":
                    linefunc = structline;
                    type = "struct";
                    break;
                case "INTEGER":
                    linefunc = enumline;
                    type = "enum";
                    break;
                default:
                    throw new NotImplementedException();
            }
            lines = match.Groups["body"].Value.Split(new string[] { ", ", ",\n", "\n" }, StringSplitOptions.None);
            for(int i = 0; i < lines.Length; i++)
                lines[i] = linefunc(lines[i]);
            return type;
        }
    }
}
