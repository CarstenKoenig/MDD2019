using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockChain
{
    public class BlockChain : IEnumerable<Block>
    {
        readonly Dictionary<string, Block> _blockLookup;
        readonly Block _genisisBlock;
        Block _lastBlock;
        readonly int _difficulty;


        public BlockChain(int difficulty)
        {
            _blockLookup = new Dictionary<string, Block>();
            _difficulty = difficulty;
            _genisisBlock = new Block(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), DateTime.Now, new byte[] { });
            _genisisBlock.MineHash(difficulty);
            _blockLookup.Add(_genisisBlock.BlockHash, _genisisBlock);
            _lastBlock = _genisisBlock;
        }

        public bool AddBlock(Block newBlock)
        {
            if (!newBlock.ValidateHash())
                return false;
            if (!newBlock.PreviousHashBytes.IsSameAs(_lastBlock.BlockHashBytes))
                return false;

            _lastBlock = newBlock;
            _blockLookup.Add(newBlock.BlockHash, newBlock);
            return true;
        }

        public Block AddContent(byte[] content)
        {
            var previousHash = _lastBlock.BlockHashBytes;
            var newBlock = new Block(content, DateTime.Now, previousHash);
            newBlock.MineHash(_difficulty);
            if (!AddBlock(newBlock)) return null;
            return newBlock;
        }

        public Block this[string hash]
        {
            get
            {
                if (_blockLookup.TryGetValue(hash, out var block))
                    return block;
                return null;
            }
        }

        public bool IsChainValid()
        {
            var previous = _genisisBlock;
            foreach (var block in this)
            {
                if (!previous.BlockHashBytes.IsSameAs(block.PreviousHashBytes) || !block.ValidateHash())
                    return false;
                previous = block;
            }

            return true;

        }

        public IEnumerator<Block> GetEnumerator()
        {
            var stack = new Stack<Block>();
            var block = _lastBlock;
            while (block != null && block != _genisisBlock)
            {
                stack.Push(block);
                block = this[block.PreviousHash];
            }
            return ((IEnumerable<Block>)stack).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
