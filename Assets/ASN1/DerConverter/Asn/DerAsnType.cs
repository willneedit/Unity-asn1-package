using System.Collections.Generic;
using System.Linq;

namespace DerConverter.Asn
{
    public abstract class DerAsnType
    {
        public DerAsnIdentifier Identifier { get; private set; }

        public virtual object Value { get; protected set; }

        protected DerAsnType(DerAsnIdentifier identifier)
        {
            Identifier = identifier;
        }

        public abstract byte[] Encode(IDerAsnEncoder encoder);
    }

    public abstract class DerAsnType<T> : DerAsnType
    {
        protected DerAsnType(IDerAsnDecoder decoder, DerAsnIdentifier identifier, Queue<byte> rawData)
            : base(identifier)
        {
            Value = DecodeValue(decoder, rawData);
        }

        protected DerAsnType(DerAsnIdentifier identifier, T value)
            : base(identifier)
        {
            Value = value;
        }

        public new T Value
        {
            get { return (T) base.Value; }
            set { base.Value = value; }
        }

        public override byte[] Encode(IDerAsnEncoder encoder)
            => EncodeValue(encoder, Value).ToArray();

        protected abstract T DecodeValue(IDerAsnDecoder decoder, Queue<byte> rawData);

        protected abstract IEnumerable<byte> EncodeValue(IDerAsnEncoder encoder, T value);
    }
}
