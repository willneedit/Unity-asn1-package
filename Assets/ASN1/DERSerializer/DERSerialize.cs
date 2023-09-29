using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Numerics;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;


namespace DERSerializer
{
    public partial class Serializer
    {
        private static (Type type, bool optional) GetOptionalType(Type t)
        {
            Type t2 = Nullable.GetUnderlyingType(t);
            return t2 == null
                ? (t, false)
                : (t2, true);
        }


        private static void EmitOneItem(AsnWriter writer, object item, ASN1TagAttribute attrs = null)
        {
            if(item == null) return;

            Asn1Tag? tag = attrs;

            if(item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(List<>))
                EmitCollection(writer, item, tag);
            else if(item is Enum e) writer.WriteEnumeratedValue(e, tag);
            else if(item is bool b) writer.WriteBoolean(b);
            else if(item is uint ui) writer.WriteInteger(ui, tag);
            else if(item is ulong ul) writer.WriteInteger(ul, tag);
            else if(item is int i) writer.WriteInteger(i, tag);
            else if(item is long l) writer.WriteInteger(l, tag);
            else if(item is BigInteger bi) writer.WriteInteger(bi, tag);
            else if(item is byte[] ba) writer.WriteOctetString(ba, tag);
            else if(item is string s) writer.WriteCharacterString(UniversalTagNumber.UTF8String, s, tag);
            else if(item.GetType().IsStruct()) EmitStruct(writer, item, tag);
            else throw new NotImplementedException($"{item.GetType().Name} is unsupported");
        }

        private static void EmitCollection(AsnWriter writer, object collection, Asn1Tag? tag = null)
        {
            writer.PushSequence(tag);

            ForEachObj(collection, (o) => EmitOneItem(writer, o));

            writer.PopSequence(tag);
        }

        private static void EmitStruct(AsnWriter writer, object @struct, Asn1Tag? tag = null)
        {
            writer.PushSequence(tag);

            ForEachStructFields(@struct, (sfv, ta) => EmitOneItem(writer, sfv, ta));

            writer.PopSequence(tag);

        }

        /// <summary>
        /// Recreation of foreach, being type agnostic
        /// </summary>
        /// <param name="collection">Collection, implementing GetEnumerator()</param>
        /// <param name="action">The action to act on for the elements</param>
        private static void ForEachObj(object collection, Action<object> action)
        {
            MethodInfo mi = collection.GetType().GetRuntimeMethod("GetEnumerator", new Type[0]);
            object enumerator = mi.Invoke(collection, new object[0]);

            Type type = enumerator.GetType();
            PropertyInfo e_current = type.GetRuntimeProperty("Current");
            MethodInfo e_movenext = type.GetRuntimeMethod("MoveNext", new Type[0]);

            while((e_movenext.Invoke(enumerator, new object[0]) as bool?) ?? false)
                action(e_current.GetValue(enumerator));
        }

        /// <summary>
        /// Do something over all public structure fields
        /// </summary>
        /// <param name="struct">The struct to act on for the fields</param>
        /// <param name="action">The action to act</param>
        /// 
        private static void ForEachStructFields(object @struct, Action<object, ASN1TagAttribute> action)
        {
            foreach(FieldInfo field in @struct.GetType()
                .GetFields(BindingFlags.Instance |
                // BindingFlags.NonPublic |
                BindingFlags.Public))
            {
                action(field.GetValue(@struct), field.GetCustomAttribute<ASN1TagAttribute>());
            }
        }

        public static byte[] Serialize(object obj)
        {
            AsnWriter writer = new(AsnEncodingRules.DER);
            EmitOneItem(writer, obj);

            return writer.Encode();
        }

        public static string Hexdump(byte[] output)
        {
            StringBuilder sb = new();
            int i = 0;
            foreach(byte v in output)
            {
                sb.Append(v.ToString("X2"));
                i++;
                if((i % 16)== 0) sb.Append("  ");
                if((i % 32) == 0) sb.Append("\n");
                else sb.Append(" ");
            }
            return sb.ToString();
        }
    }
}
