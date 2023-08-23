﻿using System;
using System.Collections.Generic;
using System.Linq;
using DerConverter.Asn.KnownTypes;

namespace DerConverter.Asn
{
    /// <summary>
    /// A default implementation of <see cref="IDerAsnDecoder"/> that registers known ASN.1 types.
    /// </summary>
    public class DefaultDerAsnDecoder : IDerAsnDecoder
    {
        private readonly Dictionary<TypeKey, TypeConstructor> _registeredTypes = new();
        private readonly Dictionary<DerAsnIdentifier, TypeConstructor> _registeredClassSpecificTypes = new();

        /// <summary>
        /// Delegate that describes a function to construct a type during decoding.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="identifier">The tag identifier (contains tag class, encoding type and tag).</param>
        /// <param name="data">The data to decode for the specific type.</param>
        /// <returns>The decoded ASN.1 type (primitive or constructed set/sequence).</returns>
        public delegate DerAsnType TypeConstructor(IDerAsnDecoder decoder, DerAsnIdentifier identifier, Queue<byte> data);

        /// <summary>
        /// Constructs a new <see cref="DefaultDerAsnDecoder"/> instance.
        /// </summary>
        public DefaultDerAsnDecoder()
        {
            RegisterKnownTypes();
        }

        /// <summary>
        /// Cleanup used resources.
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// Decode the ASN.1 structure encoded in the given byte array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The decoded ASN.1 type (primitive or constructed set/sequence).</returns>
        public DerAsnType Decode(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (!data.Any()) throw new ArgumentException("Data cannot be empty", nameof(data));
            return Decode(new Queue<byte>(data));
        }

        /// <summary>
        /// Decode a single ASN.1 type encoded in the given byte queue.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The decoded ASN.1 type (primitive or constructed set/sequence).</returns>
        public DerAsnType Decode(Queue<byte> data)
        {
            DerAsnIdentifier identifier = DerAsnIdentifier.Decode(data);

            TypeConstructor typeConstructor = FindTypeConstructor(identifier)
                ?? throw new InvalidOperationException($"No type registered for identifier {identifier}");

            DerAsnLength length = DerAsnLength.Decode(data);
            if (data.Count < length)
                throw new InvalidOperationException($"Expected {length} bytes to decode type with identifier {identifier} but got {data.Count} bytes");

            Queue<byte> dataForType = new(data.Dequeue(length.Length));
            return typeConstructor(this, identifier, dataForType);
        }

        /// <summary>
        /// Registers a generic, class independent type.
        /// </summary>
        /// <param name="encodingType">The encoding type.</param>
        /// <param name="tag">The tag number.</param>
        /// <param name="typeConstructor">The type constructor.</param>
        /// <param name="replace">True to allow replacing an existing registration, otherwise false</param>
        /// <returns>The <see cref="DefaultDerAsnDecoder"/> instance.</returns>
        public DefaultDerAsnDecoder RegisterGenericType(DerAsnEncodingType encodingType, long tag, TypeConstructor typeConstructor, bool replace = false)
        {
            TypeKey typeKey = new() { EncodingType = encodingType, Tag = tag };

            if (!replace && _registeredTypes.ContainsKey(typeKey))
                throw new InvalidOperationException($"Type with encoding type {encodingType} and tag number {tag} already registered");

            _registeredTypes[typeKey] = typeConstructor ?? throw new ArgumentNullException(nameof(typeConstructor));

            return this;
        }

        /// <summary>
        /// Registers a class dependent type.
        /// </summary>
        /// <param name="identifier">The exact tag identifier of the type.</param>
        /// <param name="typeConstructor">The type constructor.</param>
        /// <param name="replace">True to allow replacing an existing registration, otherwise false</param>
        /// <returns>The <see cref="DefaultDerAsnDecoder"/> instance.</returns>
        public DefaultDerAsnDecoder RegisterType(DerAsnIdentifier identifier, TypeConstructor typeConstructor, bool replace = false)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
            if(!replace && _registeredClassSpecificTypes.ContainsKey(identifier))
                throw new InvalidOperationException($"Type with class {identifier.TagClass}, encoding type {identifier.EncodingType} and tag number {identifier.Tag} already registered");

            _registeredClassSpecificTypes[identifier] = typeConstructor ?? throw new ArgumentNullException(nameof(typeConstructor));

            return this;
        }

        protected virtual void RegisterKnownTypes()
        {
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Boolean, (decoder, identifier, data) => new DerAsnBoolean(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Integer, (decoder, identifier, data) => new DerAsnInteger(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.BitString, (decoder, identifier, data) => new DerAsnBitString(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.OctetString, (decoder, identifier, data) => new DerAsnOctetString(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Null, (decoder, identifier, data) => new DerAsnNull(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.ObjectIdentifier, (decoder, identifier, data) => new DerAsnObjectIdentifier(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Utf8String, (decoder, identifier, data) => new DerAsnUtf8String(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.PrintableString, (decoder, identifier, data) => new DerAsnPrintableString(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.UtcTime, (decoder, identifier, data) => new DerAsnUtcTime(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Ia5String, (decoder, identifier, data) => new DerAsnIa5String(decoder, identifier, data));

            RegisterGenericType(DerAsnEncodingType.Constructed, DerAsnKnownTypeTags.Constructed.Sequence, (decoder, identifier, data) => new DerAsnContext(decoder, identifier, data));
            RegisterGenericType(DerAsnEncodingType.Constructed, DerAsnKnownTypeTags.Constructed.Set, (decoder, identifier, data) => new DerAsnSet(decoder, identifier, data));
        }

        protected virtual TypeConstructor FindTypeConstructor(DerAsnIdentifier identifier)
        {
            if (_registeredClassSpecificTypes.TryGetValue(identifier, out TypeConstructor classSpecificTypeConstructor))
                return classSpecificTypeConstructor;

            TypeKey typeKey = new() { EncodingType = identifier.EncodingType, Tag = identifier.Tag };
            if (_registeredTypes.TryGetValue(typeKey, out classSpecificTypeConstructor))
                return classSpecificTypeConstructor;

            return null;
        }

        private class TypeKey
        {
            public DerAsnEncodingType EncodingType { get; set; }

            public long Tag { get; set; }

            public override bool Equals(object obj)
            {
                return obj is TypeKey key &&
                       EncodingType == key.EncodingType &&
                       Tag == key.Tag;
            }

            public override int GetHashCode()
            {
                int hashCode = -1011251413;
                hashCode = hashCode * -1521134295 + EncodingType.GetHashCode();
                hashCode = hashCode * -1521134295 + Tag.GetHashCode();
                return hashCode;
            }
        }
    }
}
