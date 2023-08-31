using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ASN1Compiler;
using System.Text.RegularExpressions;
using System.Linq;

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

    // Start is called before the first frame update
    void Start()
    {
        DebugFielddecl("x INTEGER");
        DebugFielddecl("y INTEGER OPTIONAL");
        DebugFielddecl("z [2] INTEGER OPTIONAL");
        DebugFielddecl("w IMPLICIT INTEGER OPTIONAL");

        DebugFielddecl("subjectUniqueID [2]  IMPLICIT UniqueIdentifier OPTIONAL");

        DebugBlock(ref_tbsCertificate);

        DebugBlock(ref_Version);
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
        (string ident, string type, string[] lines) = Patterns.EmitBlockDefinition(test);

        string block = string.Join("", lines);

        Debug.Log($"{type} {ident} = {{\n{block}}};");
    }

}
