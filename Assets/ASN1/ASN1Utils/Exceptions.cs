using System;

namespace ASN1Utils
{
    public class KeyMismatchException : Exception
    {
        public KeyMismatchException() : base("Key type doesn't match to the imported data") { }
    }
}
