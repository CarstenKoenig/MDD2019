using System;
using System.Linq;
using System.Text;

namespace BlockChain
{
    class Program
    {
        static void Main(string[] args)
        {
            var sender = new Account();
            var receiver = new Account();

            var blockChain = new BlockChain(schwierigkeit: 2);

            var transaktion = new Transaction(sender, receiver.PublicKey, 42.42m);
            blockChain.AddBlock(transaktion.Serialize());

            System.Console.WriteLine($"Valide Chain? {blockChain.ValidiereChain()}");

            foreach (var block in blockChain.Skip(1))
            {
                Console.WriteLine($"Block:\n{block}\n---\n");

                var blockTransaktion = new Transaction(block.BlockInhalt);
                Console.WriteLine($"Transaktion:\n{blockTransaktion}\n");
                Console.WriteLine($"valide? {blockTransaktion.Verify()}");
                Console.WriteLine("=================\n\n");
            }
        }
    }
}
