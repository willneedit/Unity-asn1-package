using System.Collections.Generic;
using System.Formats.Asn1;
using System.Numerics;
using System;
using Unity.VisualScripting;
using System.Reflection;

namespace DERSerializer
{
    public partial class Serializer
    {
#if false
        private static bool CheckOptional(AsnReader reader, ASN1TagAttribute attrs, UniversalTagNumber def)
        {
            bool optional = attrs?.optional ?? false;
            Asn1Tag tag = ((Asn1Tag?) attrs ?? new Asn1Tag(def));

            if(optional && !reader.PeekTag().HasSameClassAndValue(tag)) return false;

            return true;
        }
#endif

        private static object ReadOneItem(AsnReader reader, Type type, ASN1TagAttribute attrs = null)
        {
            bool optional;
            (type, optional) = GetOptionalType(type);
            optional |= attrs?.optional ?? false;
            Asn1Tag? tag = attrs;

            if(optional)
            {
                // If the struct field attribute is absent, infer the universal tag number
                // from the field type.
                Asn1Tag inferred = tag ?? Asn1Tag.Null;
                if(attrs == null || attrs.number < 0)
                {
                    if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) inferred = new(UniversalTagNumber.Sequence);
                    else if(type.IsEnum) inferred = new(UniversalTagNumber.Enumerated);
                    else if(type == typeof(bool)) inferred = new(UniversalTagNumber.Boolean);
                    else if(type == typeof(uint) || type == typeof(ulong)) inferred = new(UniversalTagNumber.Integer);
                    else if(type == typeof(int) || type == typeof(long) || type == typeof(BigInteger)) inferred = new(UniversalTagNumber.Integer);
                    else if(type == typeof(byte[])) inferred = new(UniversalTagNumber.OctetString);
                    else if(type == typeof(string)) inferred = new(UniversalTagNumber.UTF8String);
                    else if(type.IsStruct()) inferred = new(UniversalTagNumber.Sequence);
                }

                // Move to the next field as it seems to be missing because it's optional.
                if(!reader.HasData || !reader.PeekTag().HasSameClassAndValue(inferred)) return null;
            }

            object item;
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                item = ReadCollection(reader, type, tag);
            }
            else if(type.IsEnum)
            {
                item = reader.ReadEnumeratedValue(type, tag);
            }
            else if(type == typeof(bool))
            {
                item = reader.ReadBoolean(tag);
            }
            else if(type == typeof(ulong))
            { 
                reader.TryReadUInt64(out ulong l, tag); item = l; 
            }
            else if(type == typeof(uint))
            {
                reader.TryReadUInt32(out uint i, tag); item = i; 
            }
            else if (type == typeof(long))
            {
                reader.TryReadInt64(out long l, tag); item = l;
            }
            else if (type == typeof(int))
            {
                reader.TryReadInt32(out int i, tag); item = i;
            }
            else if (type == typeof(BigInteger))
            {
                item = reader.ReadInteger(tag);
            }
            else if(type == typeof(byte[]))
            {
                item = reader.ReadOctetString(tag);
            }
            else if(type == typeof(string))
            {
                item = reader.ReadCharacterString(UniversalTagNumber.UTF8String, tag);
            }
            else if(type.IsStruct())
            {
                item = ReadStruct(reader, type, tag);
            }
            else
            {
                throw new NotImplementedException($"{type.Name} is unsupported");
            }

            return item;
        }

        private static object ReadStruct(AsnReader reader, Type type, Asn1Tag? tag)
        {
            AsnReader seqreader = reader.ReadSequence(tag);

            object item = Activator.CreateInstance(type);
            foreach(FieldInfo field in type
                .GetFields(BindingFlags.Instance |
                // BindingFlags.NonPublic |
                BindingFlags.Public))
            {
                object fielditem = ReadOneItem(seqreader, field.FieldType, field.GetCustomAttribute<ASN1TagAttribute>());
                field.SetValue(item, fielditem);
            }

            return item;
        }

        private static object ReadCollection(AsnReader reader, Type type, Asn1Tag? tag = null)
        {
            AsnReader seqreader = reader.ReadSequence(tag);

            object item = Activator.CreateInstance(type);
            Type elemType = type.GetGenericArguments()[0];
            MethodInfo l_Add = type.GetRuntimeMethod("Add", new Type[] { elemType });

            while(seqreader.HasData)
            {
                object element = ReadOneItem(seqreader, elemType);
                l_Add.Invoke(item, new object[] { element });
            }

            return item;
        }

        public static T Deserialize<T>(ReadOnlyMemory<byte> der)
        {
            AsnReader reader = new(der, AsnEncodingRules.DER);
            object item = ReadOneItem(reader, typeof(T));
            return (T) item;
        }
    }
}
