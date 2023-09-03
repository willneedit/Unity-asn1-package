using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int number;

        // Tag Class, Tag Number
        public ASN1TagAttribute(TagClass tagClass, int number)
        {
            this.tagClass = tagClass;
            this.number = number;

            if(tagClass == TagClass.Universal) throw new ArgumentException("Use Tag class other than Universal");
        }

        // Tag Number only
        public ASN1TagAttribute(int number) { tagClass = TagClass.ContextSpecific; this.number = number; }
    }

}
