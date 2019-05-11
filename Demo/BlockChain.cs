using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockChain
{
  public class BlockChain : IEnumerable<Block>
  {
    readonly List<Block> _blocks;
    readonly int _schwierigkeit;


    public BlockChain(int schwierigkeit)
    {
      _blocks = new List<Block>();
      _schwierigkeit = schwierigkeit;

      // Genisis-Block
      AddBlock(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
    }

    public Block AddBlock(byte[] daten)
    {
      var vorhergehenderHash = _blocks.LastOrDefault()?.BlockHash ?? new byte[] { };
      var neuerBlock = new Block(daten, DateTime.Now, vorhergehenderHash);
      neuerBlock.MineHash(_schwierigkeit);
      _blocks.Add(neuerBlock);
      return neuerBlock;
    }

    public bool ValidiereChain()
    {
      var vorhergehenderHash = new byte[] { };
      foreach (var block in this)
      {
        if (!vorhergehenderHash.IsSameAs(block.VorhergehenderBlockHash) || !block.ValidiereHash())
          return false;
        vorhergehenderHash = block.BlockHash;
      }

      return true;

    }

    public IEnumerator<Block> GetEnumerator()
    {
      return ((IEnumerable<Block>)_blocks).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<Block>)_blocks).GetEnumerator();
    }
  }
}
