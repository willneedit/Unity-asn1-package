using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1Compiler
{
    internal class Patterns
    {
        public static string optional(string pattern) => $"({pattern}|)";

        public const string ws = @"\s+";

        public const string type = @"(?<type>[_a-zA-Z][_a-zA-Z0-9]*)";

        public const string identifier = @"(?<identifier>[_a-zA-Z][\-_a-zA-Z0-9]*)";

        public const string oid_decl = @"(?<oiddecl>[_a-zA-Z][\-_a-zA-Z0-9]*)";

        public const string universalType = @"(?<type>(INTEGER|BIT STRING|OCTET STRING|NULL|OBJECT IDENTIFIER|UTF8String|SEQUENCE OF|SET OF|UTCTime))";

        public static readonly string tagdecl = @"\["+ optional(@"(?<app>APPLICATION)" + ws) + @"(?<tagdecl>[0-9]+)\]\s*";

        private const string declaration = identifier + ws + @"::=" + ws;

        public const string enumdef = @"(?<blocklead>INTEGER)" + ws + @"\{(?<enumbody>.*?)\}";
        
        public const string structdef = @"(?<blocklead>(SEQUENCE|SET))" + ws + @"\{(?<structbody>.*?)\}";

        public const string complexdef = declaration + @"(" + enumdef + @"|" + structdef+ @")";

        // type name
        public const string typedef = declaration + universalType;

        // enumitem(enumvalue)
        public const string enumitemdef = identifier + @"\((?<value>[0-9]+)\)";

        //struct field: type name; // tag usage description, or nullable?
        public static readonly string fielddef = identifier + ws 
            + optional(tagdecl + ws) 
            + optional(@"(?<expl>(EXPLICIT|IMPLICIT))" + ws)
            + type
            + optional(ws + @"(?<optional>OPTIONAL)");
    }
}
