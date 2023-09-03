using System;
using System.Formats.Asn1;
using System.Numerics;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;

using Debug = UnityEngine.Debug;

namespace DERSerializer
{
    public class Serializer
    {
        private static (Type type, bool optional) GetOptionalType(Type t)
        {
            Type t2 = Nullable.GetUnderlyingType(t);
            return t2 == null
                ? (t, false)
                : (t2, true);
        }


        public static void EmitOneItem(AsnWriter writer, object item, ASN1TagAttribute attrs = null)
        {
            if(item == null) return;

            Asn1Tag? tag = attrs != null
                    ? new Asn1Tag(attrs.tagClass, attrs.number)
                    : null;

            if(item is int i) writer.WriteInteger(i, tag);
            else if(item is long l) writer.WriteInteger(l, tag);
            else if(item is BigInteger bi) writer.WriteInteger(bi, tag);
            else if(item is byte[] ba) writer.WriteOctetString(ba, tag);
            else if(item is string s) writer.WriteCharacterString(UniversalTagNumber.UTF8String, s, tag);
            else
            {
                Type t = item.GetType();
                if(t.IsStruct())
                    EmitStruct(writer, item, t, tag);
            }
        }

        public static void EmitStruct<T>(AsnWriter writer, T @struct, Type type = null, Asn1Tag? tag = null)
        {
            writer.PushSequence(tag);

            if(type == null) type = typeof(T);

            foreach(FieldInfo field in type.GetFields(BindingFlags.Instance |
                                                 BindingFlags.NonPublic |
                                                 BindingFlags.Public))
            {

                EmitOneItem(writer, field.GetValue(@struct), field.GetCustomAttribute<ASN1TagAttribute>());
            }
                
            writer.PopSequence(tag);
        }
        public static void DebugWriter(AsnWriter writer)
        {
            byte[] output = writer.Encode();
            StringBuilder sb = new();
            foreach(byte v in output)
            {
                sb.Append(v.ToString("X2"));
                sb.Append(" ");
            }
            Debug.Log(sb.ToString());
        }
    }


    [DERSerializable]
    public struct Foo
    {
        [ASN1Tag(0)]
        int bar;
        int? baz;
    }
}
