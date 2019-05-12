using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockChain
{
    public class BlockChain : IEnumerable<Block>
    {
        readonly Dictionary<string, Block> _blocks;
        readonly int _difficulty;


        public BlockChain(int difficulty)
        {
            _blocks = new Dictionary<string, Block>();
            _difficulty = difficulty;

            // Genisis-Block
            AddBlock(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        }

        public Block AddBlock(byte[] content)
        {
            var previousHash = _blocks.Values.LastOrDefault()?.BlockHashBytes ?? new byte[] { };
            var newBlock = new Block(content, DateTime.Now, previousHash);
            newBlock.MineHash(_difficulty);
            _blocks.Add(newBlock.BlockHash, newBlock);
            return newBlock;
        }

        public Block this[string hash]
        {
            get
            {
                if (_blocks.TryGetValue(hash, out var block))
                    return block;
                throw new KeyNotFoundException($"no block with hash {hash} found");
            }
        }

        public bool IsChainValid()
        {
            var previousHash = new byte[] { };
            foreach (var block in this)
            {
                if (!previousHash.IsSameAs(block.PreviousHash) || !block.ValidateHash())
                    return false;
                previousHash = block.BlockHashBytes;
            }

            return true;

        }

        public IEnumerator<Block> GetEnumerator()
        {
            return ((IEnumerable<Block>)_blocks.Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Block>)_blocks.Values).GetEnumerator();
        }
    }
}
