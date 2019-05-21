using System;
using System.Linq;
using System.Text;

namespace BlockChain
{
  class Program
  {
    static void Main(string[] args)
    {
      var sender = new Signature();
      var receiver = new Signature();

      var blockChain = new BlockChain(difficulty: 2);

      var transaction = new Transaction(sender, receiver.PublicKey, 42.42m);
      blockChain.AddContent(transaction.Serialize());
      var smallTransaction = new Transaction(sender, receiver.PublicKey, 0.42m);
      blockChain.AddContent(smallTransaction.Serialize());

      System.Console.WriteLine($"Valide Chain? {blockChain.IsChainValid()}");

      foreach (var block in blockChain)
      {
        Console.WriteLine($"Block:\n{block}\n---\n");

        var blockTransaktion = new Transaction(block.Content);
        Console.WriteLine($"Transaktion:\n{blockTransaktion}\n");
        Console.WriteLine($"valide? {blockTransaktion.Verify()}");
        Console.WriteLine("=================\n\n");
      }
    }
  }
}
