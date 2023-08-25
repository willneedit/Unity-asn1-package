public class SR
{
    public const string Argument_DestinationTooShort = "Destination is too short.";
    public const string Argument_EnumeratedValueRequiresNonFlagsEnum = "ASN.1 Enumerated values only apply to enum types without the [Flags] attribute.";
    public const string Argument_EnumeratedValueBackingTypeNotSupported = "Enumerations with a backing type of '{0}' are not supported for ReadEnumeratedValue.";
    public const string Argument_InvalidOidValue = "The OID value is invalid.";
    public const string Argument_NamedBitListRequiresFlagsEnum = "Named bit list operations require an enum with the [Flags] attribute.";
    public const string Argument_SourceOverlapsDestination = "The destination buffer overlaps the source buffer.";
    public const string Argument_Tag_NotCharacterString = "The specified tag has the Universal TagClass, but the TagValue does not correspond with a known character string type.";
    public const string Argument_IntegerCannotBeEmpty = "An integer value cannot be empty.";
    public const string Argument_IntegerRedundantByte = "The first 9 bits of the integer value all have the same value. Ensure the input is in big-endian byte order and that all redundant leading bytes have been removed.";
    public const string Argument_UniversalValueIsFixed = "Tags with TagClass Universal must have the appropriate TagValue value for the data type being read or written.";
    public const string Argument_UnusedBitCountMustBeZero = "Unused bit count must be 0 when the bit string is empty.";
    public const string Argument_UnusedBitCountRange = "Unused bit count must be between 0 and 7, inclusive.";
    public const string Argument_UnusedBitWasSet = "One or more of the bits covered by the provided unusedBitCount value is set. All unused bits must be cleared.";
    public const string Argument_WriteEncodedValue_OneValueAtATime = "The input to WriteEncodedValue must represent a single encoded value with no trailing data.";
    public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
    public const string AsnWriter_EncodeUnbalancedStack = "Encode cannot be called while a Sequence, Set-Of, or Octet String is still open.";
    public const string AsnWriter_PopWrongTag = "Cannot pop the requested tag as it is not currently in progress.";
    public const string ContentException_CerRequiresIndefiniteLength = "A constructed tag used a definite length encoding, which is invalid for CER data. The input may be encoded with BER or DER.";
    public const string ContentException_ConstructedEncodingRequired = "The encoded value uses a primitive encoding, which is invalid for '{0}' values.";
    public const string ContentException_DefaultMessage = "The ASN.1 value is invalid.";
    public const string ContentException_EnumeratedValueTooBig = "The encoded enumerated value is larger than the value size of the '{0}' enum.";
    public const string ContentException_InvalidUnderCer_TryBerOrDer = "The encoded value is not valid under the selected encoding, but it may be valid under the BER or DER encoding.";
    public const string ContentException_InvalidUnderCerOrDer_TryBer = "The encoded value is not valid under the selected encoding, but it may be valid under the BER encoding.";
    public const string ContentException_InvalidUnderDer_TryBerOrCer = "The encoded value is not valid under the selected encoding, but it may be valid under the BER or CER encoding.";
    public const string ContentException_InvalidTag = "The provided data does not represent a valid tag.";
    public const string ContentException_LengthExceedsPayload = "The encoded length exceeds the number of bytes remaining in the input buffer.";
    public const string ContentException_LengthRuleSetConstraint = "The encoded length is not valid under the requested encoding rules, the value may be valid under the BER encoding.";
    public const string ContentException_LengthTooBig = "The encoded length exceeds the maximum supported by this library (Int32.MaxValue).";
    public const string ContentException_NamedBitListValueTooBig = "The encoded named bit list value is larger than the value size of the '{0}' enum.";
    public const string ContentException_PrimitiveEncodingRequired = "The encoded value uses a constructed encoding, which is invalid for '{0}' values.";
    public const string ContentException_SetOfNotSorted = "The encoded set is not sorted as required by the current encoding rules. The value may be valid under the BER encoding, or you can ignore the sort validation by specifying skipSortValidation=true.";
    public const string ContentException_TooMuchData = "The last expected value has been read, but the reader still has pending data. This value may be from a newer schema, or is corrupt.";
    public const string ContentException_WrongTag = "The provided data is tagged with '{0}' class value '{1}', but it should have been '{2}' class value '{3}'.";

    public static string Format(string fmt, object obj1) => string.Format(fmt, obj1);
    public static string Format(string fmt, params object[] objs) => string.Format(fmt, objs);
}
