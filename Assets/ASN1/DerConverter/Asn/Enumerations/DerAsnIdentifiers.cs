namespace DerConverter.Asn
{
    public static class DerAsnIdentifiers
    {
        public static class Primitive
        {
            public static readonly DerAsnIdentifier Boolean = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Boolean);
            public static readonly DerAsnIdentifier Integer = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Integer);
            public static readonly DerAsnIdentifier BitString = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.BitString);
            public static readonly DerAsnIdentifier OctetString = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.OctetString);
            public static readonly DerAsnIdentifier Null = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Null);
            public static readonly DerAsnIdentifier ObjectIdentifier = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.ObjectIdentifier);
            public static readonly DerAsnIdentifier Utf8String = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Utf8String);
            public static readonly DerAsnIdentifier PrintableString = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.PrintableString);
            public static readonly DerAsnIdentifier TeletexString = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.TeletexString);
            public static readonly DerAsnIdentifier Ia5String = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.Ia5String);
            public static readonly DerAsnIdentifier UtcTime = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.UtcTime);
            public static readonly DerAsnIdentifier BmpString = new(DerAsnTagClass.Universal, DerAsnEncodingType.Primitive, DerAsnKnownTypeTags.Primitive.BmpString);
        }

        public static class Constructed
        {
            public static readonly DerAsnIdentifier Sequence = new(DerAsnTagClass.Universal, DerAsnEncodingType.Constructed, DerAsnKnownTypeTags.Constructed.Sequence);
            public static readonly DerAsnIdentifier Set = new(DerAsnTagClass.Universal, DerAsnEncodingType.Constructed, DerAsnKnownTypeTags.Constructed.Set);
        }
    }
}
