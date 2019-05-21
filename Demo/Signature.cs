using System;
using System.IO;
using System.Security.Cryptography;

namespace BlockChain
{
  public class Signature
  {
    readonly RSAOpenSsl _signAlgorithm;
    public Signature()
    {
      _signAlgorithm = new RSAOpenSsl();
    }

    public string PublicKey
    {
      get
      {
        var parameters = _signAlgorithm.ExportParameters(false);
        using (var mem = new MemoryStream())
        using (var writer = new BinaryWriter(mem))
        {
          writer.Write(parameters.Exponent.Length);
          writer.Write(parameters.Exponent);
          writer.Write(parameters.Modulus.Length);
          writer.Write(parameters.Modulus);
          return Convert.ToBase64String(mem.ToArray());
        }
      }
    }

    public string SignHash(byte[] data)
    {
      return Convert.ToBase64String(_signAlgorithm.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    public static bool VerifyHash(string publicKey, byte[] data, string signature)
    {
      using (var rsa = new RSAOpenSsl())
      {
        rsa.ImportParameters(GetParameters(publicKey));
        return rsa.VerifyData(data, Convert.FromBase64String(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
      }
    }

    private static RSAParameters GetParameters(string publicKey)
    {
      using (var mem = new MemoryStream(Convert.FromBase64String(publicKey)))
      using (var reader = new BinaryReader(mem))
      {
        var expLenth = reader.ReadInt32();
        var exponent = reader.ReadBytes(expLenth);
        var modLength = reader.ReadInt32();
        var modulus = reader.ReadBytes(modLength);

        return new RSAParameters { Exponent = exponent, Modulus = modulus };
      }
    }
  }
}
