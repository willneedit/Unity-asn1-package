using System.Collections.Generic;

namespace DerConverter.Asn.KnownTypes
{
    public class DerAsnContext : DerAsnType<DerAsnType>
    {
        public DerAsnContext(IDerAsnDecoder decoder, DerAsnIdentifier identifier, Queue<byte> rawData)  // to public 210707
            : base(decoder, identifier, rawData)
        {
        }

        public DerAsnContext(DerAsnIdentifier identifier, DerAsnType value)
            : base(identifier, value)
        {
        }

        public DerAsnContext(long tag, DerAsnType value)
            : base(new DerAsnIdentifier(
                DerAsnTagClass.ContextSpecific, 
                DerAsnEncodingType.Constructed, tag), value)
        {
        }

        protected override DerAsnType DecodeValue(IDerAsnDecoder decoder, Queue<byte> rawData) 
            => decoder.Decode(rawData);

        protected override IEnumerable<byte> EncodeValue(IDerAsnEncoder encoder, DerAsnType value) 
            => encoder.Encode(value);
    }
}
