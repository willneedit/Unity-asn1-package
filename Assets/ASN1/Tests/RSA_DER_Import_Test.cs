using System.Security.Cryptography;
using UnityEngine;

using ASN1Utils;
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

    // openssl genrsa |openssl rsa -pubout
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

    // openssl ecparam -name secp521r1 -genkey |openssl ec -pubout
    const string ref_ec_pubkey
= "-----BEGIN PUBLIC KEY-----\n"
+ "MIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQBq3ZO3vLXpl83T0SGLhFTlmedLV86\n"
+ "Tq5HOyjraIq6nqtnc/HvazSqM64c1qt5zRJGsx1S2gTqJeNscceyIG0HjLEBOtXc\n"
+ "xtJaZ/x+izWZDkY0Lg6m0trSLBNMT78lFN+OBaE1HwAfwnIv2nsQhCwoMZ+8lciW\n"
+ "szBGYeedT1r4yDAqkeI=\n"
+ "-----END PUBLIC KEY-----\n";

    // openssl ecparam -name secp521r1 -genkey
    const string ref_ec_privkey
= "-----BEGIN EC PRIVATE KEY-----\n"
+ "MIHcAgEBBEIBVWtu7NmzzlDrOwF40RV8T3FSE03SVDMIKCFv5nxw0CefkH5QG+MN\n"
+ "sagOYZE41EFBVca9RmmBlUjZ5WIEH2htrL2gBwYFK4EEACOhgYkDgYYABAATZ9HO\n"
+ "x5NTcT7VQizWZai7Hg0zXlh+Fon2lPrMhA2zVx7i98La6aEQUgYJLxKzCfIISG0d\n"
+ "Vi7qcIxdbBxL/4pe+AE86+SoNpkWewUQD6L1vWjR/I6towA670aw53W8tiJntW1W\n"
+ "tiRhNYgUBANplhy59Yin3KGZE6T13Yc4TVyDwWDsHw==\n"
+ "-----END EC PRIVATE KEY-----\n";

    // -------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        PEMConversionTest();

        RSAImportKeyTest();

        RSAExportKeyTest();

        // Elliptic Curve Cryptography is not included in the Unity's Mono .NET runtimes.
        // We can import or export EC keys, but unless you include or reimplement ECC, you cannot work
        // with them.

        // ECImportKeyTest();
    }

    void PEMConversionTest()
    {

        byte[] der = PEM.ExtractPEM(ref_pem);

        string mypem = PEM.AssemblePEM(der, "PUBLIC KEY");

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
            KeyImport.ImportDER(PEM.ExtractPEM(ref_pem), out RSA key);

            RSAParameters pars = key.ExportParameters(false);

            if(!pars.Modulus.SequenceEqual(ref_modulus) || !pars.Exponent.SequenceEqual(ref_Exponent))
                Debug.Log("ImportDER: Modulus, Exponent mismatch");
        }

        {
            KeyImport.ImportDER(PEM.ExtractPEM(ref_privkey), out RSA key);

            RSAParameters pars = key.ExportParameters(true);
            if(pars.P == null || pars.P.Length == 0)
                Debug.Log("ImportDER: Not the private key");
        }

        Debug.Log("RSA import tests finished.");
    }

    void RSAExportKeyTest()
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

            byte[] ref_der = PEM.ExtractPEM(ref_pem);

            if(!der.SequenceEqual(ref_der))
                Debug.LogError("Constructed public key doesn't match the reference");
        }

        {
            KeyImport.ImportDER(PEM.ExtractPEM(ref_privkey), out RSA blueprint);

            byte[] der = blueprint.ExportDER(true);

            byte[] ref_der = PEM.ExtractPEM(ref_privkey);

            if(!der.SequenceEqual(ref_der))
                Debug.LogError("Constructed private key doesn't match the reference");
        }

        Debug.Log("RSA export tests finished.");
    }


    private void ECImportKeyTest()
    {
        {
            ECParameters key = KeyImport.ReadEC(PEM.ExtractPEM(ref_ec_pubkey));
        }


        {
            ECParameters key = KeyImport.ReadEC(PEM.ExtractPEM(ref_ec_privkey));
        }

        Debug.Log("EC import tests finished.");
    }
}
