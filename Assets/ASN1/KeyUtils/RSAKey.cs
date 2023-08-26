using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Security.Cryptography;

namespace ASN1
{
    public static class Extensions
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

        internal static bool ReadRSAParameters(RSAParameters toRead, byte[] der)
        {
            AsnReader reader = new AsnReader(der, AsnEncodingRules.DER);

            bool expectedPrivate = false;
            AsnReader RSAKeySequence = reader.ReadSequence();

            RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.Modulus);
            // Public Key: Modulus & Exponent,
            // Private Key: All eight, prepended with the version integer (=0)
            if(toRead.Modulus.Length == 1 && toRead.Modulus[0] == 0)
            {
                expectedPrivate = true;
                // Discard the version and try to read the Modulus, again.
                RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.Modulus);
            }

            RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.Exponent);

            // Private key material as following...
            if(expectedPrivate)
            {
                RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.D);
                RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.P);
                RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.Q);
                RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.DP);
                RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.DQ);
                RSAKeySequence.ReadIntegerBytes().CopyTo(toRead.InverseQ);
            }

            return expectedPrivate;
        }

        internal static byte[] WriteRSAParameters(RSAParameters toWrite, bool includePrivate)
        {
            AsnWriter writer = new(AsnEncodingRules.DER);

            writer.PushSequence();      // SEQUENCE { ...
            {
                if(includePrivate)
                    writer.WriteInteger(0); // Private only: Version 0

                writer.WriteIntegerUnsigned(toWrite.Modulus);
                writer.WriteIntegerUnsigned(toWrite.Exponent);

                if(includePrivate)
                {
                    writer.WriteIntegerUnsigned(toWrite.D);
                    writer.WriteIntegerUnsigned(toWrite.P);
                    writer.WriteIntegerUnsigned(toWrite.Q);
                    writer.WriteIntegerUnsigned(toWrite.DP);
                    writer.WriteIntegerUnsigned(toWrite.DQ);
                    writer.WriteIntegerUnsigned(toWrite.InverseQ);
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

        public static bool ReadDER(this RSAParameters toRead, byte[] der)
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

                bool priv2 = ReadRSAParameters(toRead, encapsulated);

                // Mismatch of the envelope with the embedded key material
                Debug.Assert(expectedPrivate == priv2);
            }

            return expectedPrivate;
        }

        public static byte[] WriteDER(this RSAParameters toWrite, bool includePrivate = false)
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
