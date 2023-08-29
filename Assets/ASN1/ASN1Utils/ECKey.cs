using System;
using System.Formats.Asn1;
using System.Security.Cryptography;

namespace ASN1Utils
{
    public static partial class Extensions
    {
        public static byte[] ExportDER(this ECDsa key, bool includePrivateParameters)
            => KeyExport.WriteEC(key.ExportParameters(includePrivateParameters), includePrivateParameters);

        public static byte[] ExportDER(this ECDiffieHellman key, bool includePrivateParameters)
            => KeyExport.WriteEC(key.ExportParameters(includePrivateParameters), includePrivateParameters);
    }

    public static partial class KeyImport
    {
        public static void ImportDER(ArraySegment<byte> data, out ECDsa key)
            => key = ECDsa.Create(KeyExport.ReadEC(data));

        public static void ImportDER(ArraySegment<byte> data, out ECDiffieHellman key)
            => key = ECDiffieHellman.Create(KeyExport.ReadEC(data));

        public static ECParameters ReadEC(ArraySegment<byte> data)
            => KeyExport.ReadEC(data);
    }

    internal static partial class KeyExport
    {
        private const string oid_ecPublicKey = "1.2.840.10045.2.1";

        // As seen in:
        // https://github.com/dotnet/designs/blob/main/accepted/2020/asnreader/asnreader.md
        internal static ECParameters ReadEC(ArraySegment<byte> ecPrivateKey)
        {
            AsnReader reader = new AsnReader(ecPrivateKey, AsnEncodingRules.BER);
            AsnReader sequenceReader = reader.ReadSequence();
            reader.ThrowIfNotEmpty();

            Asn1Tag integer = new(UniversalTagNumber.Integer);
            if(!sequenceReader.PeekTag().HasSameClassAndValue(integer))
                return ReadECPublicKey(sequenceReader);
            else
                return ReadECPrivateKey(sequenceReader);
        }

        private static ECParameters ReadECPublicKey(AsnReader sequenceReader)
        {
            AsnReader keyAlgo = sequenceReader.ReadSequence();
            string oid = keyAlgo.ReadObjectIdentifier();
            if(oid != oid_ecPublicKey) throw new ArgumentException("Key is not an EC key");

            ECParameters ecParameters = new ECParameters();
            ecParameters.Curve = ECCurve.CreateFromValue(keyAlgo.ReadObjectIdentifier());

            ReadECPKPart(ref ecParameters, sequenceReader);

            return ecParameters;
        }

        private static ECParameters ReadECPrivateKey(AsnReader sequenceReader)
        {
            sequenceReader.TryReadInt32(out int _);
            ECParameters ecParameters = new ECParameters()
            {
                D = sequenceReader.ReadOctetString(),
            };

            Asn1Tag context0 = new Asn1Tag(TagClass.ContextSpecific, 0);
            Asn1Tag context1 = new Asn1Tag(TagClass.ContextSpecific, 1);

            // Don't test for the ECParameters, since we didn't accept external parameters,
            // just assert it's there and read it.
            //if (sequenceReader.HasData && sequenceReader.PeekTag().HasSameClassAndValue(context0))
            {
                AsnReader ecParamsReader = sequenceReader.ReadSequence(context0);
                ecParameters.Curve = ECCurve.CreateFromValue(ecParamsReader.ReadObjectIdentifier());
                ecParamsReader.ThrowIfNotEmpty();
            }

            // Don't test for the presence of public key, we require it,
            // so just assert it's there and read it.
            //if (sequenceReader.HasData && sequenceReader.PeekTag().HasSameClassAndValue(context1))
            {
                AsnReader publicKeyReader = sequenceReader.ReadSequence(context1);
                ReadECPKPart(ref ecParameters, publicKeyReader);
            }

            sequenceReader.ThrowIfNotEmpty();
            return ecParameters;
        }

        private static void ReadECPKPart(ref ECParameters ecParameters, AsnReader publicKeyReader)
        {
            byte[] encodedKey = publicKeyReader.ReadBitString(out int unused);
            publicKeyReader.ThrowIfNotEmpty();

            if(unused != 0 || encodedKey.Length % 2 != 1 || encodedKey[0] != 0x04)
            {
                throw new NotSupportedException();
            }

            ecParameters.Q.X = encodedKey.AsSpan(1, encodedKey.Length / 2).ToArray();
            ecParameters.Q.Y = encodedKey.AsSpan(1 + encodedKey.Length / 2).ToArray();
        }

        internal static byte[] WriteEC(ECParameters eCParameters, bool includePrivateParameters)
        {
            if(includePrivateParameters)
                return WriteECPrivateKey(eCParameters);
            else
                return WriteECPublicKey(eCParameters);
        }

        private static byte[] WriteECPrivateKey(ECParameters eCParameters)
        {
            AsnWriter writer = new(AsnEncodingRules.DER);
            Asn1Tag context0 = new Asn1Tag(TagClass.ContextSpecific, 0);
            Asn1Tag context1 = new Asn1Tag(TagClass.ContextSpecific, 1);

            writer.PushSequence();
            {
                writer.WriteInteger(1); // Version
                writer.WriteOctetString(eCParameters.D); // Private Key

                writer.PushSequence(context0); // Curve. Only named curves
                {
                    writer.WriteObjectIdentifier(eCParameters.Curve.Oid.ToString());
                }
                writer.PopSequence();

                writer.PushSequence(context1); // Public Key as bit string. For us, it's mandatory
                {
                    WriteECPKPart(writer, eCParameters);
                }
                writer.PopSequence();
            }
            writer.PopSequence();


            return writer.Encode();
        }

        private static byte[] WriteECPublicKey(ECParameters eCParameters)
        {
            AsnWriter writer = new(AsnEncodingRules.DER);

            writer.PushSequence();
            {
                writer.PushSequence();
                {
                    writer.WriteObjectIdentifier(oid_ecPublicKey);
                    writer.WriteObjectIdentifier(eCParameters.Curve.Oid.ToString());
                }
                writer.PopSequence();
            }
            WriteECPKPart(writer, eCParameters);
            writer.PopSequence();

            return writer.Encode();
        }

        private static void WriteECPKPart(AsnWriter writer, ECParameters eCParameters)
        {
            byte[] publicKeyStream = new byte[1 + eCParameters.Q.X.Length + eCParameters.Q.Y.Length];

            publicKeyStream[0] = 0x04; // Uncompressed
            eCParameters.Q.X.CopyTo(publicKeyStream, 1);
            eCParameters.Q.Y.CopyTo(publicKeyStream, 1 + eCParameters.Q.X.Length);

            writer.WriteBitString(publicKeyStream);
        }
    }
}
