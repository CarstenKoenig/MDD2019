using System;
using System.IO;
using System.Security.Cryptography;

namespace BlockChain
{
  public class Block
  {
    public Block(byte[] inhalt, DateTime zeitstempel, byte[] vorhergehenderHash)
    {
      BlockInhalt = inhalt;
      Zeitstempel = zeitstempel;
      VorhergehenderBlockHash = vorhergehenderHash;

      BlockNonce = -1;
      BlockHash = null;
    }

    public Block(byte[] inhalt, byte[] vorhergehenderHash)
      : this(inhalt, DateTime.Now, vorhergehenderHash)
    {
    }

    public byte[] BlockInhalt { get; }
    public DateTime Zeitstempel { get; }
    public byte[] BlockHash { get; private set; }
    public int BlockNonce { get; private set; }
    public byte[] VorhergehenderBlockHash { get; }

    public override string ToString()
    {
      if (BlockHash == null)
        return $"{{noch nicht 'MINED'}}\ntime: {Zeitstempel.ToShortTimeString()}";

      string BytesToString(byte[] bytes)
      {
        var completeHash = BitConverter.ToString(bytes).Replace("-", "");
        if (completeHash.Length > 8)
          return completeHash.Substring(completeHash.Length - 8);
        return completeHash;
      };

      return $"{{{BytesToString(BlockHash)}}}\nprev: {{{BytesToString(VorhergehenderBlockHash)}}}\nnonce:{BlockNonce}\ttime: {Zeitstempel.ToShortTimeString()}";
    }

    public void MineHash(int difficulty)
    {
      System.Console.Write($"start MINING with difficulty={difficulty}...");
      for (var nonce = 0; nonce <= Int32.MaxValue; nonce++)
      {
        var hash = BerechneHash(nonce);
        if (ValiderHash(hash, difficulty))
        {
          BlockHash = hash;
          BlockNonce = nonce;
          System.Console.WriteLine($"found NONCE {nonce}");
          return;
        }
      }

      throw new Exception($"Konne Block nich minen - kein Nonce hat einen validen Hash der Schwierigkeit {difficulty} erzeugt");
    }

    public bool ValidiereHash()
    {
      if (BlockHash == null)
        return false;

      return BlockHash.IsSameAs(BerechneHash(BlockNonce));
    }

    bool ValiderHash(byte[] hash, int difficulty)
    {
      if (difficulty > hash.Length)
        throw new Exception("Schwierigkeit übersteigt die Hash-Block Länge");

      for (var i = 0; i < difficulty; i++)
        if (hash[i] != 0) return false;

      return true;
    }

    byte[] BerechneHash(int nonce)
    {
      var bytes = BerechneBlockBytes(nonce);
      using (var hashAlgorithm = SHA512.Create())
        return hashAlgorithm.ComputeHash(bytes);
    }

    byte[] BerechneBlockBytes(int nonce)
    {
      using (var memoryStream = new System.IO.MemoryStream())
      using (var binaryStream = new BinaryWriter(memoryStream))
      {
        binaryStream.Write(BlockInhalt);
        binaryStream.Write(BitConverter.GetBytes(nonce));
        binaryStream.Write(VorhergehenderBlockHash);
        binaryStream.Write(Zeitstempel.ToBinary());

        return memoryStream.ToArray();
      }
    }
  }
}
