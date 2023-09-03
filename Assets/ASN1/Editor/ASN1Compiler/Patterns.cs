using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ASN1Compiler
{
    internal struct Fielddef
    {
        public string name;
        public string type;
        public TagClass tagClass;
        public int tagNumber;
        public int realTagNumber;
        public bool? expl;
        public bool optional;
        public bool listed;
    }

    internal struct GenericType
    {
        public string asn1name;
        public UniversalTagNumber tagNumber;
        public string csharptype;
        public string readFunc;
        public string writeFunc;
    }

    internal class Patterns
    {
        public static List<GenericType> All = new()
        {
            new()
            {
                asn1name = "INTEGER",
                csharptype = "BigInteger",
                tagNumber = UniversalTagNumber.Integer,
                readFunc = "{0} = {1}.ReadInteger(",
                writeFunc = "{1}.WriteInteger({0}, "
            },
            new()
            {
                asn1name = "INT32",
                csharptype = "int",
                tagNumber = UniversalTagNumber.Integer,
                readFunc = "_ = {1}.TryReadInt32(out int {0}, ",
                writeFunc = "{1}.WriteInteger({0}, "
            },
            new()
            {
                asn1name = "INT64",
                csharptype = "long",
                tagNumber = UniversalTagNumber.Integer,
                readFunc = "_ = {1}.TryReadInt64(out int {0}, ",
                writeFunc = "{1}.WriteInteger({0}, "
            },
            new()
            {
                asn1name = "UINT32",
                csharptype = "uint",
                tagNumber = UniversalTagNumber.Integer,
                readFunc = "_ = {1}.TryReadUInt32(out int {0}, ",
                writeFunc = "{1}.WriteInteger({0}, "
            },
            new()
            {
                asn1name = "UINT64",
                csharptype = "ulong",
                tagNumber = UniversalTagNumber.Integer,
                readFunc = "_ = {1}.TryReadUInt64(out int {0}, ",
                writeFunc = "{1}.WriteInteger({0}, "
            },


            new()
            {
                asn1name = "BIT STRING",
                csharptype = "byte[]",
                tagNumber = UniversalTagNumber.BitString,
                readFunc = "{0} = {1}.ReadBitString(out int _, ",
                writeFunc = "{1}.WriteBitString({0}, 0, "
            },
            new()
            {
                asn1name = "OCTET STRING",
                csharptype = "byte[]",
                tagNumber = UniversalTagNumber.OctetString,
                readFunc = "{0} = {1}.ReadOctetString(",
                writeFunc = "{1}.WriteOctetString({0}, "
            },

            new()
            {
                asn1name = "OBJECT IDENTIFIER",
                csharptype = "string",
                tagNumber = UniversalTagNumber.ObjectIdentifier,
                readFunc = "{0} = {1}.ReadObjectIdentifier(",
                writeFunc = "{1}.WriteObjectIdentifier({0}, "
            },

            new()
            {
                asn1name = "UTCTime",
                csharptype = "DateTimeOffset",
                tagNumber = UniversalTagNumber.UtcTime,
                readFunc = "{0} = {1}.ReadUtcTime(",
                writeFunc = "{1}.WriteUtcTime({0}, "
            },

            new()
            {
                asn1name = "UTF8String",
                csharptype = "string",
                tagNumber = UniversalTagNumber.UTF8String,
                readFunc = "{0} = {1}.ReadUTF8String(",
                writeFunc = "{1}.WriteUTF8String({0}, "
            },

            new()
            {
                asn1name = "NULL",
                csharptype = null,
                tagNumber = UniversalTagNumber.Null,
                readFunc = "{1}.ReadNull(",
                writeFunc = "{1}.WriteNull("
            },

            // 0 - Desired variable name
            // 1 - Asn1Reader or Asn1Writer, in the frame
            // 2 - Type name for complex types
            //new()
            //{
            //    asn1name = "SEQUENCE",
            //    csharptype = "{2}",
            //    tagNumber = UniversalTagNumber.Sequence,
            //    readFunc = "{0} = {2}.ReadDER({1}, ",
            //    writeFunc = "{1}.ReadDER({0}, "
            //},
        };

        public static string Optional(string pattern) => $"({pattern}|)";

        public const string ws = @"\s+";

        public const string comment = @"(-- .*? --|-- .*$)";

        public const string type = @"(?<type>[_a-zA-Z][_a-zA-Z0-9]*)";

        public const string identifier = @"(?<identifier>[_a-zA-Z][\-_a-zA-Z0-9]*)";

        public const string oiditem = @"((?<oidref>[_a-zA-Z][\-_a-zA-Z0-9]*)|(?<oiditem>[0-9]*))";

        public const string universalType = @"(?<type>(INTEGER|BIT STRING|OCTET STRING|NULL|OBJECT IDENTIFIER|UTF8String|UTCTime))";

        public static readonly string tagdecl = @"\["+ Optional(@"(?<app>APPLICATION)" + ws) + @"(?<tagdecl>[0-9]+)\]\s*";

        private const string declaration = identifier + ws + @"::=" + ws;

        private const string blockbody = @"\{(?<body>.*?)\}";

        public const string structdef = declaration + @"(?<blocklead>(INTEGER|SEQUENCE|SET))" + ws + blockbody;

        public const string oiddef = identifier + ws + "OBJECT IDENTIFIER" + ws + "::=" + ws + blockbody;
        // type name
        public const string typedef = declaration + universalType;

        // enumitem(enumvalue)
        public const string enumitemdef = identifier + @"\((?<value>[0-9]+)\)";

        //struct field: type name; // tag usage description, or nullable?
        public static readonly string fielddef = identifier + ws 
            + Optional(tagdecl + ws) 
            + Optional(@"(?<expl>(EXPLICIT|IMPLICIT))" + ws)
            + Optional(@"(?<listed>(SEQUENCE OF|SET OF))" + ws)
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

        public static (string head, string body, string tail) EmitBlockDefinition(string block)
        {
            string filtered = FilterComments(block);

            Match match = Regex.Match(filtered, structdef, RegexOptions.Singleline);

            string ident = match.Groups["identifier"].Value;
            string type = match.Groups["blocklead"].Value;

            type = EmitStructDefinition(match, type, out block);
            return ($"{type} {ident} = {{", block, "};");
        }

        public static (string head, string body, string tail) EmitOIDDefinition(string block)
        {
            string filtered = FilterComments(block);

            Match match = Regex.Match(filtered, oiddef, RegexOptions.Singleline);

            string ident = match.Groups["identifier"].Value;
            EmitOIDPartDefinition(match, out block);
            return ($"partial class OIDDefs {{\n    public const string oid_{ident} = ", block, "};");
        }

        private static string EmitStructDefinition(Match match, string type, out string block)
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
            string[] lines = match.Groups["body"].Value.Split(new string[] { ", ", ",\n", "\n" }, StringSplitOptions.None);
            for(int i = 0; i < lines.Length; i++)
                lines[i] = linefunc(lines[i]);

            block = string.Join("", lines);
            return type;
        }

        private static void EmitOIDPartDefinition(Match match, out string block)
        {

            const string oidident = @"(?<ident>[\-_0-9a-zA-Z]+)";
            const string oidnumber = @"(?<number>[0-9]+)";
            const string oiddef = oidident + @"\(" + oidnumber + @"\)";
            const string oidpart = "(" + oiddef + "|" + oidnumber + "|" + oidident + ")";

            MatchCollection coll = Regex.Matches(match.Groups["body"].Value, oidpart);
            List<string> parts = new();

            foreach(Match match1 in coll.Cast<Match>())
            {
                GroupCollection gr = match1.Groups;
                string str = (string.IsNullOrEmpty(gr["number"].Value))
                    ? gr["ident"].Value
                    : "\"" + gr["number"].Value + "\"";
                parts.Add(str);
            }
            block = string.Join("+ \".\" +", parts.ToArray());
        }
    }
}
