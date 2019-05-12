using System;
using System.IO;
using System.Security.Cryptography;

namespace BlockChain
{
    public class Block
    {
        public Block(byte[] inhalt, DateTime zeitstempel, byte[] vorhergehenderHash)
        {
            Content = inhalt;
            Timestamp = zeitstempel;
            PreviousHash = vorhergehenderHash;

            Nonce = -1;
            BlockHashBytes = null;
        }

        public Block(byte[] inhalt, byte[] vorhergehenderHash)
          : this(inhalt, DateTime.Now, vorhergehenderHash)
        {
        }

        public byte[] Content { get; }
        public DateTime Timestamp { get; }
        public byte[] BlockHashBytes { get; private set; }
        public string BlockHash => Convert.ToBase64String(BlockHashBytes);
        public int Nonce { get; private set; }
        public byte[] PreviousHash { get; }

        public override string ToString()
        {
            if (BlockHashBytes == null)
                return $"{{not yet 'MINED'}}\ntime: {Timestamp.ToShortTimeString()}";

            string BytesToString(byte[] bytes)
            {
                var completeHash = BitConverter.ToString(bytes).Replace("-", "");
                if (completeHash.Length > 8)
                    return completeHash.Substring(completeHash.Length - 8);
                return completeHash;
            };

            return $"{{{BytesToString(BlockHashBytes)}}}\nprev: {{{BytesToString(PreviousHash)}}}\nnonce:{Nonce}\ttime: {Timestamp.ToShortTimeString()}";
        }

        public void MineHash(int difficulty)
        {
            System.Console.Write($"start MINING with difficulty={difficulty}...");
            for (var nonce = 0; nonce <= Int32.MaxValue; nonce++)
            {
                var hash = CalculateHash(nonce);
                if (IsHashValid(hash, difficulty))
                {
                    BlockHashBytes = hash;
                    Nonce = nonce;
                    System.Console.WriteLine($"found NONCE {nonce}");
                    return;
                }
            }

            throw new Exception("failed to mine block - no valid nounce found");
        }

        public bool ValidateHash()
        {
            if (BlockHashBytes == null)
                return false;

            return BlockHashBytes.IsSameAs(CalculateHash(Nonce));
        }

        bool IsHashValid(byte[] hash, int difficulty)
        {
            if (difficulty > hash.Length)
                throw new Exception("difficulty exceeds hash length");

            for (var i = 0; i < difficulty; i++)
                if (hash[i] != 0) return false;

            return true;
        }

        byte[] CalculateHash(int nonce)
        {
            var bytes = GetBlockBytes(nonce);
            using (var hashAlgorithm = SHA512.Create())
                return hashAlgorithm.ComputeHash(bytes);
        }

        byte[] GetBlockBytes(int nonce)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            using (var binaryStream = new BinaryWriter(memoryStream))
            {
                binaryStream.Write(Content);
                binaryStream.Write(BitConverter.GetBytes(nonce));
                binaryStream.Write(PreviousHash);
                binaryStream.Write(Timestamp.ToBinary());

                return memoryStream.ToArray();
            }
        }
    }
}
