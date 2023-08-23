using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace PemUtils
{
    public class PemFormat
    {
        private static List<PemFormat> KnownFormatsCache = null;

        public readonly string _type;

        public string Header => $"-----BEGIN {_type}-----";

        public string Footer => $"-----END {_type}-----";

        public PemFormat(string type)
        {
            _type = type;
        }

        public override bool Equals(object obj)
        {
            if(obj is not PemFormat other) return false;
            return string.Equals(_type, other._type, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => _type;

        public static PemFormat Parse(string typeString)
        {
            PemFormat format = new(typeString);
            PemFormat knownFormat = GetKnownFormats().FirstOrDefault(x => x.Equals(format));
            if (knownFormat == null) throw new InvalidOperationException($"Unknown format: {typeString}");
            return knownFormat;
        }

        private static List<PemFormat> GetKnownFormats()
        {
            if (KnownFormatsCache != null) return KnownFormatsCache;
            return KnownFormatsCache = typeof(PemFormat).GetTypeInfo()
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.PropertyType == typeof(PemFormat))
                .Select(x => (PemFormat)x.GetValue(null))
                .ToList();
        }


        #region Static formats

        public static PemFormat X509CertificateOld => new("X509 CERTIFICATE");
        public static PemFormat X509Certificate => new("CERTIFICATE");
        public static PemFormat X509Pair => new("CERTIFICATE PAIR");
        public static PemFormat X509Trusted => new("TRUSTED CERTIFICATE");
        public static PemFormat X509RequestOld => new("NEW CERTIFICATE REQUEST");
        public static PemFormat X509Request => new("CERTIFICATE REQUEST");
        public static PemFormat X509Crl => new("X509 CRL");
        public static PemFormat EvpPkey => new("ANY PRIVATE KEY");
        public static PemFormat Public => new("PUBLIC KEY");
        public static PemFormat Private => new("PRIVATE KEY");   //duplicate of Pkcs8Inf - is that a problem?
        public static PemFormat Rsa => new("RSA PRIVATE KEY");
        public static PemFormat RsaPublic => new("RSA PUBLIC KEY");
        public static PemFormat Dsa => new("DSA PRIVATE KEY");
        public static PemFormat DsaPublic => new("DSA PUBLIC KEY");
        public static PemFormat Pkcs7 => new("PKCS7");
        public static PemFormat Pkcs7Signed => new("PKCS #7 SIGNED DATA");
        public static PemFormat Pkcs8 => new("ENCRYPTED PRIVATE KEY");
        public static PemFormat Pkcs8Inf => new("PRIVATE KEY");
        public static PemFormat DhParameters => new("DH PARAMETERS");
        public static PemFormat SslSession => new("SSL SESSION PARAMETERS");
        public static PemFormat DsaParameters => new("DSA PARAMETERS");
        public static PemFormat EcdsaPublic => new("ECDSA PUBLIC KEY");
        public static PemFormat EcParameters => new("EC PARAMETERS");
        public static PemFormat EcPrivateKey => new("EC PRIVATE KEY");
        public static PemFormat Parameters => new("PARAMETERS");
        public static PemFormat Cms => new("CMS");

        #endregion
    }
}
