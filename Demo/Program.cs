using System;
using System.Linq;
using System.Text;

namespace BlockChain
{

  class Program
  {
    static void Main(string[] args)
    {
      var blockChain = new BlockChain(schwierigkeit: 2);
      blockChain.AddBlock(Encoding.UTF8.GetBytes("Hello World"));

      System.Console.WriteLine($"Valide? {blockChain.ValidiereChain()}");

      foreach (var block in blockChain)
        Console.WriteLine($"Block:\n{block}\n---\n");
    }
  }
}
