﻿namespace DerConverter.Asn
{
    public enum DerAsnTagClass : byte
    {
        Universal = 0x00,
        Application = 0x01,
        ContextSpecific = 0x02,
        Private = 0x03
    }
}
