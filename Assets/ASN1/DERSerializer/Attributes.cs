using System;
using System.Formats.Asn1;

namespace DERSerializer
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class DERSerializableAttribute : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ASN1TagAttribute : Attribute
    {
        public TagClass tagClass;
        public int number = -1;
        public bool optional;

        // Tag Class, Tag Number
        public ASN1TagAttribute(TagClass tagClass, int number, bool optional = false)
        {
            this.tagClass = tagClass;
            this.number = number;
            this.optional = optional;
        }

        // Tag Number only
        public ASN1TagAttribute(int number, bool optional = false)
        {
            tagClass = TagClass.ContextSpecific;
            this.number = number; 
            this.optional = optional;
        }

        public ASN1TagAttribute(bool optional = false)
        {
            this.optional = optional;
        }

        public static implicit operator Asn1Tag?(ASN1TagAttribute attrs)
        {
            return attrs != null && attrs.number < 0
                    ? new Asn1Tag(attrs.tagClass, attrs.number)
                    : null;
        }
    }
}
