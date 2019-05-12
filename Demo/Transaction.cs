using System.IO;

namespace BlockChain
{
    public class Transaction
    {
        readonly string _sender;
        readonly string _receiver;
        readonly decimal _amount;
        readonly string _signature;

        public Transaction(Account sender, string receiver, decimal amount)
        {
            _sender = sender.PublicKey;
            _receiver = receiver;
            _amount = amount;
            var data = GetTransactionData();
            _signature = sender.SignHash(data);
        }

        public Transaction(byte[] transactionData)
        {
            using (var mem = new MemoryStream(transactionData))
            using (var reader = new BinaryReader(mem))
            {
                _sender = reader.ReadString();
                _receiver = reader.ReadString();
                _amount = reader.ReadDecimal();
                _signature = reader.ReadString();
            }
        }

        public bool Verify()
        {
            var data = GetTransactionData();
            return Account.VerifyHash(_sender, data, _signature);
        }

        byte[] GetTransactionData()
        {
            using (var mem = new MemoryStream())
            using (var writer = new BinaryWriter(mem))
            {
                writer.Write(_sender);
                writer.Write(_receiver);
                writer.Write(_amount);
                return mem.ToArray();
            }
        }

        public byte[] Serialize()
        {
            using (var mem = new MemoryStream())
            using (var writer = new BinaryWriter(mem))
            {
                writer.Write(_sender);
                writer.Write(_receiver);
                writer.Write(_amount);
                writer.Write(_signature);

                return mem.ToArray();
            }
        }

        public override string ToString()
        {
            string Last(string input)
            {
                if (input.Length > 8)
                    return input.Substring(input.Length - 8);
                return input;
            }

            return $"Sender: {Last(_sender)}\nReceiver: {Last(_receiver)}\nAmount: {_amount}";
        }
    }
}