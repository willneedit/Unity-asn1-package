using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DerConverter;
using DerConverter.Asn;
using DerConverter.Asn.KnownTypes;

namespace PemUtils
{
    public class PemReader : IDisposable
    {
        private static readonly int[] RsaIdentifier = new[] { 1, 2, 840, 113549, 1, 1, 1 };  //  TODO move to csv file
        private readonly Stream _stream;
        private readonly bool _disposeStream;
        private readonly Encoding _encoding;

        protected string lastOID = "";


        public PemReader(Stream stream, bool disposeStream = false, Encoding encoding = null)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _disposeStream = disposeStream;
            _encoding = encoding ?? Encoding.UTF8;
        }

        public void Dispose()
        {
            if(_disposeStream) _stream.Dispose();
        }

        public RSAParameters ReadRsaKey()
        {
            PemParts parts = ReadPemParts();
            byte[] derData = Convert.FromBase64String(parts.Body);
            DerAsnType der = DerConvert.Decode(derData);

            if(parts.Format.Equals(PemFormat.Public._type)) return ReadPublicKey(der);
            _ = PemFormat.Rsa._type; ;
            if(parts.Format.Equals(PemFormat.Rsa._type)) return ReadRSAPrivateKey(der);
            _ = parts.Format;
            _ = PemFormat.Private._type;
            if(parts.Format.Equals(PemFormat.Private._type)) return ReadPrivateKey(der);
            throw new NotImplementedException($"The format {parts.Format} is not yet implemented");
        }

        private PemParts ReadPemParts()
        {
            using StreamReader reader = new(_stream, _encoding, true, 4096, true);
            PemParts parts = ExtractPemParts(reader.ReadToEnd());
            PemFormat headerFormat = ExtractFormat(parts.Header, isFooter: false);
            PemFormat footerFormat = ExtractFormat(parts.Footer, isFooter: true);
            string format = headerFormat._type;
            parts.Format = format;
            if(!headerFormat.Equals(footerFormat))
                throw new InvalidOperationException($"Header/footer format mismatch: {headerFormat}/{footerFormat}");
            return parts;

        }

        private static PemParts ExtractPemParts(string pem)
        {
            Match match = Regex.Match(pem, @"^(?<header>\-+\s?BEGIN[^-]+\-+)\s*(?<body>[^-]+)\s*(?<footer>\-+\s?END[^-]+\-+)\s*$");
            if(!match.Success)
                throw new InvalidOperationException("Data on the stream doesn't match the required PEM format");
            return new PemParts
            {
                Header = match.Groups["header"].Value,
                Body = match.Groups["body"].Value.RemoveWhitespace(),
                Footer = match.Groups["footer"].Value
            };
        }

        private static PemFormat ExtractFormat(string headerOrFooter, bool isFooter)
        {
            string beginOrEnd = isFooter ? "END" : "BEGIN";
            Match match = Regex.Match(headerOrFooter, $@"({beginOrEnd})\s+(?<format>[^-]+)", RegexOptions.IgnoreCase);
            if(!match.Success)
                throw new InvalidOperationException($"Unrecognized {beginOrEnd}: {headerOrFooter}");
            return PemFormat.Parse(match.Groups["format"].Value.Trim());
        }

        private static RSAParameters ReadPublicKey(DerAsnType der)
        {
            if(der == null) throw new ArgumentNullException(nameof(der));
            if(der is not DerAsnSequence outerSequence) throw new ArgumentException($"{nameof(der)} is not a sequence");
            if(outerSequence.Length != 2) throw new InvalidOperationException("Outer sequence must contain 2 parts");

            if(outerSequence[0] is not DerAsnSequence headerSequence) throw new InvalidOperationException("First part of outer sequence must be another sequence (the header sequence)");
            if(headerSequence.Length != 2) throw new InvalidOperationException("The header sequence must contain 2 parts");
            if(headerSequence[0] is not DerAsnObjectIdentifier objectIdentifier) throw new InvalidOperationException("First part of header sequence must be an object-identifier");
            if(objectIdentifier != RsaIdentifier) throw new InvalidOperationException($"RSA object-identifier expected 1.2.840.113549.1.1.1, got: {string.Join(".", objectIdentifier.Value.Select(x => x.ToString()))}");
            if(headerSequence[1] is not DerAsnNull) throw new InvalidOperationException("Second part of header sequence must be a null");

            if(outerSequence[1] is not DerAsnBitString innerSequenceBitString) throw new InvalidOperationException("Second part of outer sequence must be a bit-string");

            byte[] innerSequenceData = innerSequenceBitString.ToByteArray();
            if(DerConvert.Decode(innerSequenceData) is not DerAsnSequence innerSequence) throw new InvalidOperationException("Could not decode the bit-string as a sequence");
            if(innerSequence.Length < 2) throw new InvalidOperationException("Inner sequence must at least contain 2 parts (modulus and exponent)");

            return new RSAParameters
            {
                Modulus = GetIntegerData(innerSequence[0]),
                Exponent = GetIntegerData(innerSequence[1])
            };
        }

        private static RSAParameters ReadPrivateKey(DerAsnType der)
        {
            if(der == null) throw new ArgumentNullException(nameof(der));
            if(der is not DerAsnSequence sequence) throw new ArgumentException($"{nameof(der)} is not a sequence");
            if(sequence.Length != 9) throw new InvalidOperationException("Sequence must contain 9 parts");
            return new RSAParameters
            {
                // Version = GetIndegerData(sequence[0]); 
                Modulus = GetIntegerData(sequence[1]),
                Exponent = GetIntegerData(sequence[2]),
                D = GetIntegerData(sequence[3]),
                P = GetIntegerData(sequence[4]),
                Q = GetIntegerData(sequence[5]),
                DP = GetIntegerData(sequence[6]),
                DQ = GetIntegerData(sequence[7]),
                InverseQ = GetIntegerData(sequence[8]),
            };
        }
        private static RSAParameters ReadRSAPrivateKey(DerAsnType der)
        {
            if(der == null) throw new ArgumentNullException(nameof(der));
            if(der is not DerAsnSequence sequence) throw new ArgumentException($"{nameof(der)} is not a sequence");
            if(sequence.Length != 9) throw new InvalidOperationException("Sequence must contain 9 parts");
            return new RSAParameters
            {
                // Version = GetIndegerData(sequence[0]); 
                Modulus = GetIntegerData(sequence[1]),
                Exponent = GetIntegerData(sequence[2]),
                D = GetIntegerData(sequence[3]),
                P = GetIntegerData(sequence[4]),
                Q = GetIntegerData(sequence[5]),
                DP = GetIntegerData(sequence[6]),
                DQ = GetIntegerData(sequence[7]),
                InverseQ = GetIntegerData(sequence[8]),
            };
        }

        private static byte[] GetIntegerData(DerAsnType der)
        {
            byte[] data = (der as DerAsnInteger)?.Encode(null);
            if(data == null) throw new InvalidOperationException("Part does not contain integer data");
            if(data[0] == 0x00) data = data.Skip(1).ToArray();
            return data;
        }

        private class PemParts
        {
            public string Header { get; set; }
            public string Body { get; set; }
            public string Footer { get; set; }
            public string Format { get; set; }   //tcj
        }
    }
}
