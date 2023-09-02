using UnityEngine;

using ASN1Compiler;
using System.Text.RegularExpressions;

public class ASN1CompilerTests : MonoBehaviour
{
    public const string ref_tbsCertificate
= "TBSCertificate  ::=  SEQUENCE  {\n"
+ "     version         [0]  Version DEFAULT v1,\n"
+ "     serialNumber         CertificateSerialNumber,\n"
+ "     signature            AlgorithmIdentifier,\n"
+ "     issuer               Name,\n"
+ "     validity             Validity,\n"
+ "     subject              Name,\n"
+ "     subjectPublicKeyInfo SubjectPublicKeyInfo,\n"
+ "     issuerUniqueID  [1]  IMPLICIT UniqueIdentifier OPTIONAL,\n"
+ "                          -- If present, version MUST be v2 or v3\n"
+ "     subjectUniqueID [2]  IMPLICIT UniqueIdentifier OPTIONAL,\n"
+ "                          -- If present, version MUST be v2 or v3\n"
+ "     extensions      [3]  Extensions OPTIONAL\n"
+ "                          -- If present, version MUST be v3 --  }\n";

    public const string ref_Version
= @"Version ::= INTEGER { v1(0), v2(1), v3(2) }\n";

    public const string ref_pkcs1
= "   pkcs-1    OBJECT IDENTIFIER ::= {\n"
+ "     iso(1) member-body(2) us(840) rsadsi(113549) pkcs(1) 1\n"
+ "   }\n";

    // Start is called before the first frame update
    void Start()
    {
        DebugFielddecl("x INTEGER");
        DebugFielddecl("y INTEGER OPTIONAL");
        DebugFielddecl("z [2] INTEGER OPTIONAL");
        DebugFielddecl("w IMPLICIT INTEGER OPTIONAL");

        DebugFielddecl("subjectUniqueID [2]  IMPLICIT UniqueIdentifier OPTIONAL");

        DebugBlock(ref_tbsCertificate);

        // DebugBlock(ref_Version);

        // DebugOID(ref_pkcs1);
    }

    public void DebugFielddecl(string test)
    {
        Match match = Regex.Match(test, Patterns.fielddef);

        GroupCollection gr = match.Groups;

        Debug.Log($"Identifier = {gr["identifier"]}\n"
            + $"Type = {gr["type"]}\n"
            + $"Tag = {gr["tagdecl"]}\n" 
            + $"Context = {gr["app"]}\n" 
            + $"Explicit = {gr["expl"]}\n" 
            + $"Optional = {gr["optional"]}");
    }

    public void DebugBlock(string test)
    {
        SequenceParser result = new();
        result.Parse(test);

        Debug.Log(result);
    }

    public void DebugOID(string test)
    {
        (string head, string body, string tail) = Patterns.EmitOIDDefinition(test);

        Debug.Log($"{head}\n{body}{tail}");
    }

}
