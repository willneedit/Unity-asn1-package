using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Formats.Asn1;
using DERSerializer;

public class DERSerializerTest : MonoBehaviour
{
    public struct s0
    {
        public int i01;
        public string s01;
    };

    public struct s
    {
        public int i1;
        [ASN1Tag(number: 0)]
        public long? l1;
        public string s1;
        public byte[] b1;

        [ASN1Tag(tagClass: TagClass.Application,  number: 1)]
        public s0? s0;
    }
    // Start is called before the first frame update
    void Start()
    {
        byte[] bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
        s struc = new()
        {
            i1 = 1234,
            l1 = 56,
            s1 = "foo bar baz",
            b1 = bytes,
            s0 = new()
            {
                i01 = -1,
                s01 = "illegal"
            }
        };

        AsnWriter writer = new AsnWriter(AsnEncodingRules.DER);
        Serializer.EmitOneItem(writer, struc);

        Serializer.DebugWriter(writer);

    }
}
 
