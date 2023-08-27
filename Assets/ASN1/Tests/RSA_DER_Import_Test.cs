using System.Security.Cryptography;
using UnityEngine;

using ASN1;
using System.Linq;
using System;

public class RSA_DER_Import_Test : MonoBehaviour
{
    // Expected data has been derived from https://superdry.apphb.com/tools/online-rsa-key-converter
    const string ref_pem 
= "-----BEGIN PUBLIC KEY-----\n"
+ "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsiLoIxmXaZAFRBKtHYZh\n"
+ "iF8m+pYR+xGIpupvsdDEvKO92D6fIccgVLIW6p6sSNkoXx5J6KDSMbA/chy5M6pR\n"
+ "vJkaCXCI4zlCPMYvPhI8OxN3RYPfdQTLpgPywrlfdn2CAum7o4D8nR4NJacB3NfP\n"
+ "nS9tsJ2L3p5iHviuTB4xm03IKmPPqsaJy+nXUFC1XS9E/PseVHRuNvKa7WmlwSZn\n"
+ "gQzKAVSIwqpgCc+oP1pKEeJ0M3LHFo8ao5SuzhfXUIGrPnkUKEE3m7B0b8xXZfP1\n"
+ "N6ELoonWDK+RMgYIBaZdgBhPfHxF8KfTHvSzcUzWZojuR+ynaFL9AJK+8RiXnB4C\n"
+ "JwIDAQAB\n"
+ "-----END PUBLIC KEY-----\n";

    byte[] ref_modulus = Convert.FromBase64String("siLoIxmXaZAFRBKtHYZhiF8m+pYR+xGIpupvsdDEvKO92D6fIccgVLIW6p6sSNkoXx5J6KDSMbA/chy5M6pRvJkaCXCI4zlCPMYvPhI8OxN3RYPfdQTLpgPywrlfdn2CAum7o4D8nR4NJacB3NfPnS9tsJ2L3p5iHviuTB4xm03IKmPPqsaJy+nXUFC1XS9E/PseVHRuNvKa7WmlwSZngQzKAVSIwqpgCc+oP1pKEeJ0M3LHFo8ao5SuzhfXUIGrPnkUKEE3m7B0b8xXZfP1N6ELoonWDK+RMgYIBaZdgBhPfHxF8KfTHvSzcUzWZojuR+ynaFL9AJK+8RiXnB4CJw==");
    byte[] ref_Exponent = Convert.FromBase64String("AQAB");


    const string ref_privkey
= "-----BEGIN PRIVATE KEY-----\n"
+ "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCWJVRlWJv6C3YT\n"
+ "tO7HaVyEHoO6hPysOQWisAf2VlGbMiSeRucaOKWalienu7Z3YUWdCCQt/7JQmApI\n"
+ "ZQnuQOVd06cB0eMM9yMdmzL0pMfWbvuPMDynAQczYzH48LTuGVKiHizVCEtVdEgt\n"
+ "ngisku/PRoozhDZemZFdsJOgSHpUU0JfnDhBqE9TfB83CwkNglXU5AoJeeepDq7N\n"
+ "CPAvsTMYDB2Ep4LZG724+C85cfB5s48zeI8vOrtV9XgxEf86RHvmH0qMzCs3ALQG\n"
+ "ncaEcSDp/bTseui0QQkuBhBKYhDZ4qU4FAbSsddZXOhPeGw53JqjkxfY34V/4xpc\n"
+ "s0BpKOGhAgMBAAECgf8VCypEZXiL42l26piPbfBgZ6phDjQqFmOKq6p8eDLcaaoS\n"
+ "U0oUm47Ty21UYqu/Qo6fLdFXQHLCE7wo97xlZOYEtrMb+z/DGzuhC/PBe/Y1QNTl\n"
+ "ttcnaciUAhEqtCQqQq9iiB1hHuXdzQ5oNwAEJYGscJ5fsD72YAaC/GRwh27XOg1z\n"
+ "uirESXjegDP6uIlUR3faLY3mZ/Syjh+MGIKyrlyJqg4SMBaMLdT//NhEYMT9jaLQ\n"
+ "4aiGmAUxemGzyjl83/2jGEOBVMbtfBFnpaJjnWRVFOrHwtXGbvt73ENfCOLn8G4X\n"
+ "8CsgXHd8keLDsqGwe/F9J7mPVt09KX2U4pNZaSECgYEAy+O7Mnj7q2MAHr6FSh73\n"
+ "T92ViWwp0jgfB5zLqCIASd1p7hOpLAfynzGy9+OFGWG4/ZooGhB5uItwg52jsF6w\n"
+ "Kf/E1sbDH6wWppwqez6j7o20yfWF2gBTNWegJ0u+CMkbA6K+OwalMdDZ0M+EWkLZ\n"
+ "EZH9uXKbiFpdm8BycViqshECgYEAvIU0PnzB3pRzsljZbmqgMvMwJ325qOorKG66\n"
+ "IT76SvSFT9puCHlPCTGSu4iJXUPe39nNyvIJXBza8wjE8JPrxBAUNjtWu/oKSALc\n"
+ "uhjLyspV4YUTNnNmifCeSFFsjgM1dkk6zy1E0we2w+/p0YAZNnIOl77nGnTkMkPh\n"
+ "xO3TppECgYEAijMUggOrYuI6BJzTMAiJTeM+JuXf+xP7RGetS4uwcmDYGn3NH4FL\n"
+ "nUhMrOXVI/0vLQa+w9wDBWnOnAfQGg40jmNFguc6/07gE5Kq4Nr2tw3qSzJWxguO\n"
+ "WxagYcJfTwkxfGdlVhENDBUqbvUaGyxQgi5YssjST7wg0x/A8r9NBGECgYEArYrJ\n"
+ "30Qli+qI3wMflZ+ePYjVKWV5hd+bPys/ON+qtVmHZ00iwbY6ZbI261/zY+HYx6TO\n"
+ "5yYMK7l8bQIDmZvyC5jpokrZu02gLU5FNyMgZ1v/1w0T9KojGJRigRxDnC+kBXHA\n"
+ "K3v2wXV2b8TpL6yGiTJR8KsSP66faw5GRWzRy6ECgYEArUJlPivjhNQB1W45FV9p\n"
+ "xg7k3OUIhgLFoMPB7DQHuz3muRhIS12VSIfs5MnlzNw4P/o7Sag8/0U45LDqCkeh\n"
+ "wmzsZTWq45KD3M09eD8ok81xKC/pe++mgbBybn+9PCyKUUJFUTJtPzgH7QkKKNAC\n"
+ "GpnZ9FK8yQnrucPqj1sigVs=\n"
+ "-----END PRIVATE KEY-----\n";


    // -------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        PEMConversionTest();

        RSAImportKeyTest();

        RSAExportTest();
    }

    void PEMConversionTest()
    {

        byte[] der = PEMUtils.ExtractPEM(ref_pem);

        string mypem = PEMUtils.AssemblePEM(der, "PUBLIC KEY");

        if(mypem != ref_pem)
            Debug.LogError("PEM Extract and assemble mismatch");

        if(!der.SequenceEqual(Convert.FromBase64String(
  "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsiLoIxmXaZAFRBKtHYZh"
+ "iF8m+pYR+xGIpupvsdDEvKO92D6fIccgVLIW6p6sSNkoXx5J6KDSMbA/chy5M6pR"
+ "vJkaCXCI4zlCPMYvPhI8OxN3RYPfdQTLpgPywrlfdn2CAum7o4D8nR4NJacB3NfP"
+ "nS9tsJ2L3p5iHviuTB4xm03IKmPPqsaJy+nXUFC1XS9E/PseVHRuNvKa7WmlwSZn"
+ "gQzKAVSIwqpgCc+oP1pKEeJ0M3LHFo8ao5SuzhfXUIGrPnkUKEE3m7B0b8xXZfP1"
+ "N6ELoonWDK+RMgYIBaZdgBhPfHxF8KfTHvSzcUzWZojuR+ynaFL9AJK+8RiXnB4C"
+ "JwIDAQAB")))
            Debug.LogError("PEM payload garbled");

        Debug.Log("PEM conversion tests finished.");
    }

    void RSAImportKeyTest()
    {
        {
            RSA key = RSA.Create();

            key.ImportDER(PEMUtils.ExtractPEM(ref_pem));

            RSAParameters pars = key.ExportParameters(false);

            if(!pars.Modulus.SequenceEqual(ref_modulus) || !pars.Exponent.SequenceEqual(ref_Exponent))
                Debug.Log("ImportDER: Modulus, Exponent mismatch");
        }

        {
            RSA key = RSA.Create();

            key.ImportDER(PEMUtils.ExtractPEM(ref_privkey));

            RSAParameters pars = key.ExportParameters(true);
            if(pars.P == null || pars.P.Length == 0)
                Debug.Log("ImportDER: Not the private key");
        }

        Debug.Log("DER import tests finished.");
    }

    void RSAExportTest()
    {
        {
            RSAParameters pars = new()
            {
                Modulus = ref_modulus,
                Exponent = ref_Exponent
            };

            RSA key = RSA.Create();

            key.ImportParameters(pars);

            byte[] der = key.ExportDER(false);

            byte[] ref_der = PEMUtils.ExtractPEM(ref_pem);

            if(!der.SequenceEqual(ref_der))
                Debug.LogError("Constructed public key doesn't match the reference");
        }

        {
            RSA blueprint = RSA.Create();
            blueprint.ImportDER(PEMUtils.ExtractPEM(ref_privkey));

            byte[] der = blueprint.ExportDER(true);

            byte[] ref_der = PEMUtils.ExtractPEM(ref_privkey);

            if(!der.SequenceEqual(ref_der))
                Debug.LogError("Constructed private key doesn't match the reference");
        }

        Debug.Log("DER export tests finished.");
    }
}
