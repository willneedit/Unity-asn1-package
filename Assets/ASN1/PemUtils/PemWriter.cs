using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using DerConverter;
using DerConverter.Asn;
using DerConverter.Asn.KnownTypes;

namespace PemUtils
{
    public class PemWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly int _maximumLineLength;
        private readonly bool _disposeStream;
        private readonly Encoding _encoding;

        public PemWriter(Stream stream, int maximumLineLength = 64, bool disposeStream = false, Encoding encoding = null)
        {
            if (maximumLineLength < 32) throw new ArgumentOutOfRangeException(nameof(maximumLineLength), "Length should be bigger than or equal to 32");
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _maximumLineLength = maximumLineLength;
            _disposeStream = disposeStream;
            _encoding = encoding ?? Encoding.UTF8;
        }

        public void Dispose()
        {
            if (_disposeStream) _stream.Dispose();
        }

        public void WritePublicKey(RSA rsa) => WritePublicKey(rsa.ExportParameters(false));

        public void WritePublicKey(RSAParameters parameters)
        {
            DerAsnContext innerSequence = new(new DerAsnType[]
            {
                new DerAsnInteger(ToBigInteger(parameters.Modulus)),
                new DerAsnInteger(ToBigInteger(parameters.Exponent))
            });

            byte[] innerSequenceData = DerConvert.Encode(innerSequence);

            DerAsnContext outerSequence = new(new DerAsnType[]
            {
                new DerAsnContext(new DerAsnType[]
                {
                    new DerAsnObjectIdentifier(1, 2, 840, 113549, 1, 1, 1), // rsaEncryption http://www.oid-info.com/get/1.2.840.113549.1.1.1
                    new DerAsnNull()
                }),
                new DerAsnBitString(ToBitArray(innerSequenceData))
            });

            Write(outerSequence, PemFormat.Public);
        }

        public void WritePrivateKey(RSA rsa) => WritePrivateKey(rsa.ExportParameters(true));

        public void WritePrivateKey(RSAParameters parameters)
        {
            DerAsnContext sequence = new(new DerAsnType[]
            {
                new DerAsnInteger(ToBigInteger(new byte[] { 0x00 })),   // Version
                new DerAsnInteger(ToBigInteger(parameters.Modulus)),
                new DerAsnInteger(ToBigInteger(parameters.Exponent)),
                new DerAsnInteger(ToBigInteger(parameters.D)),
                new DerAsnInteger(ToBigInteger(parameters.P)),
                new DerAsnInteger(ToBigInteger(parameters.Q)),
                new DerAsnInteger(ToBigInteger(parameters.DP)),
                new DerAsnInteger(ToBigInteger(parameters.DQ)),
                new DerAsnInteger(ToBigInteger(parameters.InverseQ))
            });

            Write(sequence, PemFormat.Rsa);
        }

        private void Write(DerAsnType der, PemFormat format)
        {
            byte[] derBytes = DerConvert.Encode(der);

            string derBase64 = Convert.ToBase64String(derBytes);

            StringBuilder pem = new();
            pem.Append(format.Header + "\n");
            for (int i = 0; i < derBase64.Length; i += _maximumLineLength)
            {
                pem.Append(derBase64.Substring(i, Math.Min(_maximumLineLength, derBase64.Length - i)));
                pem.Append("\n");
            }
            pem.Append(format.Footer + "\n");

            using StreamWriter writer = new(_stream, _encoding, 4096, true);
            writer.Write(pem.ToString());
        }

        private static BigInteger ToBigInteger(byte[] data)
        {
            // BigInteger needs to be little endian, the RSA parameters _are_ big endian.
            if((data?.Length ?? 0) > 0 && data[0] >= 0x80)
            {
                // Add a leading zero byte to denote the value as unsigned if needed
                byte[] extendedData = new byte[data.Length + 1];
                data.CopyTo(extendedData, 1);
                data = extendedData;
            }

            return new BigInteger(data.Reverse().ToArray());
        }

        private static BitArray ToBitArray(byte[] data)
            => new(data.Reverse().ToArray());
    }
}
