using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Cryptography;

namespace ASN1
{
    public static class Extensions
    {
        public static byte[] ExportDER(this RSA key, bool includePrivateParameters)
            => DERExporter.WriteDER(key.ExportParameters(includePrivateParameters), includePrivateParameters);

        public static void ImportDER(this RSA key, ArraySegment<byte> data)
            => key.ImportParameters(DERExporter.ReadDER(data));
    }

    internal static class DERExporter
    {
        private const string oid_rsaEncryption = "1.2.840.113549.1.1.1";

        //RSA PRIVATE KEY (RFC 8017, A.1.2.)

        //         RSAPrivateKey ::= SEQUENCE {
        //             version           Version,
        //             modulus           INTEGER,  -- n
        //             publicExponent    INTEGER,  -- e
        //             privateExponent   INTEGER,  -- d
        //             prime1            INTEGER,  -- p
        //             prime2            INTEGER,  -- q
        //             exponent1         INTEGER,  -- d mod (p-1)
        //             exponent2         INTEGER,  -- d mod (q-1)
        //             coefficient       INTEGER,  -- (inverse of q) mod p
        //             otherPrimeInfos   OtherPrimeInfos OPTIONAL
        //         }

        //RSA PUBLIC KEY (RFC 8017, A.1.1)

        //         RSAPublicKey ::= SEQUENCE {
        //             modulus           INTEGER,  -- n
        //             publicExponent    INTEGER   -- e
        //         }

        internal static bool ReadRSAParameters(byte[] der, out RSAParameters toRead)
        {
            toRead = new();

            AsnReader reader = new AsnReader(der, AsnEncodingRules.DER);

            bool expectedPrivate = false;
            AsnReader RSAKeySequence = reader.ReadSequence();

            toRead.Modulus = RSAKeySequence.ReadIntegerBytes().ToArray();
            // Public Key: Modulus & Exponent,
            // Private Key: All eight, prepended with the version integer (=0)
            if(toRead.Modulus.Length == 1 && toRead.Modulus[0] == 0)
            {
                expectedPrivate = true;
                // Discard the version and try to read the Modulus, again.
                toRead.Modulus = RSAKeySequence.ReadIntegerBytes().ToArray();
            }

            toRead.Exponent = RSAKeySequence.ReadIntegerBytes().ToArray();

            // Private key material as following...
            if(expectedPrivate)
            {
                toRead.D = RSAKeySequence.ReadIntegerBytes().ToArray();
                toRead.P = RSAKeySequence.ReadIntegerBytes().ToArray();
                toRead.Q = RSAKeySequence.ReadIntegerBytes().ToArray();
                toRead.DP = RSAKeySequence.ReadIntegerBytes().ToArray();
                toRead.DQ = RSAKeySequence.ReadIntegerBytes().ToArray();
                toRead.InverseQ = RSAKeySequence.ReadIntegerBytes().ToArray();
            }

            return expectedPrivate;
        }

        internal static byte[] PruneUnsignedInteger(byte[] data)
        {
            // We've declared the INTEGER in this sense unsigned, and it's big endian,
            // so we cut off the leading zeroes, regardless the leading nonzero byte
            // being above 0x80 or not.
            //
            // And, leave it at least one byte. :)
            int i = 0;
            while(i < data.Length - 1 && data[i] == 0x00) i++;
            if(i != 0) data = data.Skip(i).ToArray();
            return data;
        }

        internal static byte[] WriteRSAParameters(RSAParameters toWrite, bool includePrivate)
        {
            AsnWriter writer = new(AsnEncodingRules.DER);

            writer.PushSequence();      // SEQUENCE { ...
            {
                if(includePrivate)
                    writer.WriteInteger(0); // Private only: Version 0

                writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.Modulus));
                writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.Exponent));

                if(includePrivate)
                {
                    writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.D));
                    writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.P));
                    writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.Q));
                    writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.DP));
                    writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.DQ));
                    writer.WriteIntegerUnsigned(PruneUnsignedInteger(toWrite.InverseQ));
                }
            }
            writer.PopSequence();       // ... }

            return writer.Encode();
        }


       // 0:d=0  hl=4 l=1213 cons: SEQUENCE
       // 4:d=1  hl=2 l=   1 prim:  INTEGER           :00
       // 7:d=1  hl=2 l=  13 cons:  SEQUENCE
       // 9:d=2  hl=2 l=   9 prim:   OBJECT            :rsaEncryption
       //20:d=2  hl=2 l=   0 prim:   NULL
       //22:d=1  hl=4 l=1191 prim:  OCTET STRING		-- DER coded[RSA] private key


       // 0:d=0  hl=4 l= 290 cons: SEQUENCE
       // 4:d=1  hl=2 l=  13 cons:  SEQUENCE
       // 6:d=2  hl=2 l=   9 prim:   OBJECT            :rsaEncryption
       //17:d=2  hl=2 l=   0 prim:   NULL
       //19:d=1  hl=4 l= 271 prim:  BIT STRING		-- DER coded[RSA] public key

        public static RSAParameters ReadDER(ArraySegment<byte> der)
        {
            bool expectedPrivate = false;

            AsnReader reader = new(der, AsnEncodingRules.DER);

            {
                AsnReader sequence = reader.ReadSequence();

                // Private: INTEGER Version(0)
                Asn1Tag integer = new(UniversalTagNumber.Integer);
                if(sequence.PeekTag().HasSameClassAndValue(integer))
                {
                    expectedPrivate = true;
                    // Read the version for real, and discard.
                    _ = sequence.ReadInteger();
                }

                {
                    AsnReader keyAlgo = sequence.ReadSequence();

                    // RSA: rsaEncryption ( 1.2.840.113549.1.1.1 )
                    string oid = keyAlgo.ReadObjectIdentifier();
                    if(oid != oid_rsaEncryption) throw new ArgumentException("Key is not a RSA key");

                    // AlgorithmParameter SHALL be NULL.
                }

                byte[] encapsulated = null;
                if(expectedPrivate)
                    encapsulated = sequence.ReadOctetString();
                else
                    encapsulated = sequence.ReadBitString(out int _);

                bool priv2 = ReadRSAParameters(encapsulated, out RSAParameters toRead);

                // Mismatch of the envelope with the embedded key material
                Debug.Assert(expectedPrivate == priv2);

                return toRead;
            }
        }

        public static byte[] WriteDER(RSAParameters toWrite, bool includePrivate = false)
        {
            AsnWriter writer = new(AsnEncodingRules.DER);

            writer.PushSequence();
            {
                // Private; Prepend Version(0)
                if(includePrivate) writer.WriteInteger(0);

                writer.PushSequence();
                {
                    writer.WriteObjectIdentifier(oid_rsaEncryption);
                    writer.WriteNull();
                }
                writer.PopSequence();

                byte[] encapsulated = WriteRSAParameters(toWrite, includePrivate);
                if(includePrivate)
                    writer.WriteOctetString(encapsulated);
                else
                    writer.WriteBitString(encapsulated);
            }
            writer.PopSequence();

            return writer.Encode();
        }
    }

}
