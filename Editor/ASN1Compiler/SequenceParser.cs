using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;
using System.Text.RegularExpressions;

namespace ASN1Compiler
{
    internal class SequenceParser
    {
        string name;
        readonly List<Fielddef> fields = new();

        public bool ParseFieldDefLine(string fieldline, out Fielddef field)
        {
            field = default;
            Match fd = Regex.Match(fieldline, Patterns.fielddef);
            GroupCollection gr = fd.Groups;

            if(!fd.Success) return false;

            field = new()
            {
                name = gr["identifier"].Value,
                type = gr["type"].Value,
                listed = !string.IsNullOrEmpty(gr["listed"].Value),
                optional = !string.IsNullOrEmpty(gr["optional"].Value),
                tagClass = TagClass.Universal
            };

            if(!string.IsNullOrEmpty(gr["expl"].Value))
                field.expl = gr["expl"].Value == "EXPLICIT";

            if(!string.IsNullOrEmpty(gr["tagdecl"].Value))
                field.tagClass = gr["app"].Value == "APPLICATION" ? TagClass.Application : TagClass.ContextSpecific;

            field.realTagNumber = (int) (field.type switch
            {
                "INTEGER" => UniversalTagNumber.Integer,
                "BIT STRING" => UniversalTagNumber.BitString,
                "OCTET STRING" => UniversalTagNumber.OctetString,
                "OBJECT IDENTIFIER" => UniversalTagNumber.RelativeObjectIdentifier,
                "UTF8String" => UniversalTagNumber.UTF8String,
                "UTCTime" => UniversalTagNumber.UtcTime,
                "NULL" => UniversalTagNumber.Null,
                _ => (UniversalTagNumber) (-1)
            });

            if(field.tagClass != TagClass.Universal)
                field.tagNumber = int.Parse(gr["tagdecl"].Value);
            else
                field.tagNumber = field.realTagNumber;

            return true;
        }

        public bool Parse(string text)
        {
            string filtered = Patterns.FilterComments(text);

            Match match = Regex.Match(filtered, Patterns.structdef, RegexOptions.Singleline);

            if(!match.Success) return false;

            name = match.Groups["identifier"].Value;

            string[] lines = match.Groups["body"].Value.Split(new string[] { ", ", ",\n", "\n" }, StringSplitOptions.None);

            foreach(string line in lines)
            {
                if(ParseFieldDefLine(line, out Fielddef result))
                    fields.Add(result);
            }

            return true;
        }

        public string EmitType(Fielddef field)
        {
            string nullable = field.optional ? "?" : string.Empty;
            string type;

            if(!field.listed)
                type = $"{field.type}{nullable}";
            else
                type = $"List<{field.type}>";

            return type;
        }

        public string EmitFieldDeclBlock()
        {
            StringBuilder sb = new();

            foreach(Fielddef field in fields)
                sb.Append($"    {EmitType(field)} {field.name};\n");

            return sb.ToString();
        }

        public string EmitCustomTagBlock()
        {
            StringBuilder sb = new();

            foreach(Fielddef field in fields)
            {
                if(field.tagClass == TagClass.Universal) continue;

                sb.Append($"    public static readonly Asn1Tag {GetCustomTagName(field)} = new({field.tagClass.GetType().Name}.{field.tagClass}, {field.tagNumber});\n");
            }

            return sb.ToString();
        }



        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append($"internal struct {name ?? "_"} = {{\n");

            sb.Append(EmitCustomTagBlock());
            sb.Append("\n");
            sb.Append(EmitFieldDeclBlock());
            sb.Append("\n");

            sb.Append("}\n");

            return sb.ToString();
        }

        private static string GetCustomTagName(Fielddef field) => $"{(field.tagClass == TagClass.ContextSpecific ? "context" : "application")}{field.tagNumber}";
    }
}
