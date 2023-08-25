using System;
using System.Collections.Generic;
using System.Linq;

namespace DerConverter.Asn.KnownTypes
{
    public class DerAsnObjectIdentifier : DerAsnType<int[]>, IEquatable<DerAsnObjectIdentifier>
    {
        internal DerAsnObjectIdentifier(IDerAsnDecoder decoder, DerAsnIdentifier identifier, Queue<byte> rawData)
            : base(decoder, identifier, rawData)
        {
        }

        public DerAsnObjectIdentifier(DerAsnIdentifier identifier, int[] value)
            : base(identifier, value)
        {
        }

        public DerAsnObjectIdentifier(params int[] value)
            : this(DerAsnIdentifiers.Primitive.ObjectIdentifier, value)
        {
        }

        protected override int[] DecodeValue(IDerAsnDecoder decoder, Queue<byte> rawData)
        {
            var nodes = new List<int>();
            var firstTwoNodes = rawData.Dequeue();
            nodes.Add(firstTwoNodes / 40);
            nodes.Add(firstTwoNodes % 40);
            while (rawData.Any()) nodes.Add(DequeueNode(rawData));
            return nodes.ToArray();
        }

        private static int DequeueNode(Queue<byte> queue)
        {
            int result = 0;
            byte data;
            do
            {
                result <<= 7;
                data = queue.Dequeue();
                result += data & 0x7F;
            } while (data >= 0x80);
            return result;
        }

        protected override IEnumerable<byte> EncodeValue(IDerAsnEncoder encoder, int[] value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length < 2) throw new ArgumentException("At least two values are required", nameof(value));
            if (value[0] > 5) throw new ArgumentOutOfRangeException(nameof(value), "First value should not be greater than 5");
            if (value[1] > 39) throw new ArgumentOutOfRangeException(nameof(value), "Second value should not be greater than 39");

            yield return (byte)(value[0] * 40 + value[1]);

            foreach (var node in value.Skip(2))
                foreach (var b in EnqueueNode(node))
                    yield return b;
        }

        private static byte[] EnqueueNode(int node)
        {
            var result = new List<byte>();
            result.Add((byte)(node & 0x7F));
            node >>= 7;
            while (node > 0)
            {
                result.Add((byte)(0x80 | (node & 0x7F)));
                node >>= 7;
            }
            result.Reverse();
            return result.ToArray();
        }

        public bool Equals(DerAsnObjectIdentifier other) => other is not null && EqualityComparer<DerAsnIdentifier>.Default.Equals(Identifier, other.Identifier) && EqualityComparer<object>.Default.Equals(Value, other.Value) && EqualityComparer<int[]>.Default.Equals(Value, other.Value);
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(DerAsnObjectIdentifier left, DerAsnObjectIdentifier right) => EqualityComparer<DerAsnObjectIdentifier>.Default.Equals(left, right);
        public static bool operator !=(DerAsnObjectIdentifier left, DerAsnObjectIdentifier right) => !(left == right);
        public static bool operator ==(DerAsnObjectIdentifier left, int[] right) => left.Equals(right);
        public static bool operator !=(DerAsnObjectIdentifier left, int[] right) => !(left == right);


        public bool Equals(int[] clearOID)
        {
            int[] myOID = Value;
            return myOID.SequenceEqual(clearOID);
        }

        public override bool Equals(object obj)
        {
            if(!(obj is int[] clearOID)) return false;
            return Equals(clearOID);
        }
    }
}
