---
author: Carsten König
title: BlockChain selbst gebastelt
date: 23. Mai 2019
---

# Bitcoin

##

![](../Images/Sparschwein.jpg)

##

![](../Images/Sparbuch.jpg)

## Ledger

```
...

Carsten zahlt am `07.01.` `50 DM` ein
Carsten zahlt am `13.03.` `10 DM` ein
...
Carsten hebt am `09.09` `20 DM` ab
...
```

## zentralisiert

![](../Images/CentralNetwork.png)

## öffentlicher Ledger


```
...

Carsten zahlt am `23.04.` `50 €` an Anne
Anne zahlt am `13.05.` `10 €` an Karl-Heinz
...
```

## 

![](../Images/P2PwithData.png)

##

![neue Zahlung](../Images/NodeCreatesNewTransaction.png)

##

![wird im Netz verteilt](../Images/NodeSendsNewTransaction.png)

## ...

![](../Images/NodesValidateAndRelayNewTransaction.png)


## Problem

```
...

Carsten zahlt am `23.04.` `50 €` an Anne
Anne zahlt am `13.05.` `10 €` an Karl-Heinz
...
```

## *Umverteilung*

```
...

Carsten zahlt am `23.04.` `50 €` an Anne
Anne zahlt am `13.05.` `10 €` an Karl-Heinz
...
Karl-Heinz zahlt am `14.05.` `10 €` an Carsten
```

## Unterschrift

![](../Images/SparbuchUnterschrift.jpg)

##

![](../Images/ECDSA.png)

##

`sender` *zahlt* `42 €` an `receiver`

![Transaktion](../Images/Transaction.png)

##

![Transaktion unterschreiben...](../Images/Hashing.png)

##

![Transaktion unterschreiben](../Images/Signing.png)

##

![Transaktion verifizieren](../Images/Verify.png)


## doppelte Einträge

```
...

Carsten zahlt am `23.04.` `50 €` an Anne (Unterschrift)
Anne zahlt am `13.05.` `10 €` an Karl-Heinz (Unterschrift)
Anne zahlt am `13.05.` `10 €` an Karl-Heinz (Unterschrift)
...
```

## nicht weitergeben

```
...

Carsten zahlt am `23.04.` `50 €` an Anne (Unterschrift)
...
```

## Konsens?

![](../Images/P2PwithData.png)

# Block-Chain und Konsens

##

![BlockChain](../Images/BlockChain.png)

## Mining

##

![einige Nodes beteiligen sich am "minen"](../Images/Mining1.png)

##

![Transaktionen in öffentliche Liste](../Images/Mining2.png)

##

![Miner wählen Transaktionen](../Images/Mining3.png)

##

![Gewinner verbreitet neuen Block](../Images/Mining4.png)

## Mining?

![Nounce](../Images/BlockChain.png)

##

**Hash** beginnt mit `x` Nullen (*Schwierigkeit*)

nonce: 69782 - hash: A0B32524..

nonce: 69783 - hash: 4953887B..

nonce: 69784 - hash: 5004D7E5..

nonce: 69785 - hash: **0000**F9EF...

## Konsens

![2 Knoten finden neuen Block](../Images/Consensus1.png)

##

![wird im Netzwerk verbreitet](../Images/Consensus2.png)

##

![kein Konsens](../Images/Consensus3.png)

##

![Knoten finded neuen Block](../Images/Consensus4.png)

##

![Knoten wählen die längste Kette](../Images/Consensus5.png)

##

![Konsens wieder hergestellt](../Images/Consensus6.png)

# Code

##

![Transaction](../Images/TransactionUML.png)

## 

![Account](../Images/AccountUML.png)

##

```csharp
class Account
{
    readonly RSAOpenSsl _signAlgorithm;

    public string PublicKey { .. }
}
```

## Unterschreiben

```csharp
class Account
{
    string SignData(byte[] data)
    {
        return Convert.ToBase64String(
            _signAlgorithm.SignData(
                data, 
                HashAlgorithmName.SHA256, 
                RSASignaturePadding.Pkcs1));
    }
}
```

## Unterschrift verifiziern

```csharp
static bool VerifySignatrue (
    string publicKey, 
    byte[] data, 
    string signature ) {
    using (var rsa = new RSAOpenSsl())
    {
        rsa.ImportParameters(GetParameters(publicKey));
        return rsa.VerifyData(
            data, 
            Convert.FromBase64String(signature), 
            HashAlgorithmName.SHA256, 
            RSASignaturePadding.Pkcs1);
    }
}
```

## Transaktion validieren

```csharp
class Transaction
{
    readonly string _sender;
    readonly string _receiver;
    readonly decimal _amount;
    readonly string _signature;

    bool Verify()
    {
        var data = GetTransactionData();
        return Account.VerifyHash(_sender, data, _signature);
    }

}
```

## 

![Block](../Images/BlockUML.png)

## Block

```csharp
class Block
{
    DateTime Timestamp { get; }
    byte[] PreviousHash { get; }
    byte[] Content { get; }

    string BlockHash { get; private set; }
    int Nonce { get; private set; }


    void MineHash(int difficulty) {..}
    bool ValidateHash() {..}
    byte[] CalculateHash() {..}
}
```

## Block-Hash berechnen

```csharp
byte[] CalculateHash()
{
    var bytes = GetBlockBytes();
    using (var hashAlgorithm = SHA256.Create())
        return hashAlgorithm.ComputeHash(
                   hashAlgorithm.ComputeHash(
                        bytes));
}
```

## Block-Bytes

```csharp
byte[] GetBlockBytes()
{
    using (var memoryStream = new System.IO.MemoryStream())
    using (var binaryStream = new BinaryWriter(memoryStream))
    {
        binaryStream.Write(Content);
        binaryStream.Write(BitConverter.GetBytes(Nonce));
        binaryStream.Write(PreviousHash);
        binaryStream.Write(Timestamp.ToBinary());

        return memoryStream.ToArray();
    }
}
```

## Block validieren

```csharp
bool ValidateHash()
{
    if (BlockHashBytes == null)
        return false;

    return BlockHashBytes
        .IsSameAs(CalculateHash(Nonce));
}
```

## 

![BlockChain](../Images/BlockChainUML.png)

## Blockchain

```csharp
class BlockChain : IEnumerable<Block>
{
    readonly int _difficulty;
    readonly Block _genisisBlock;
    Block _lastBlock;

    public BlockChain(int difficulty)
    {
        _difficulty = difficulty;
        _genisisBlock = ...;
        _genisisBlock.MineHash(difficulty);
        _lastBlock = _genisisBlock;
    }
}
```

## Block hinzufügen

```csharp
bool AddBlock(Block newBlock)
{
    if (!newBlock.Validate())
        return false;
    if (newBlock.PreviousHash != lastBlock.BlockHash)
        return false;

    _lastBlock = newBlock;
    return true;
}
```

## Chain validieren

```csharp
bool IsChainValid()
{
    var previous = _genisisBlock;
    foreach (var block in this)
    {
        if (previous.BlockHash != block.PreviousHash ||
            !block.ValidateHash())
            return false;
        previous = block;
    }

    return true;
}
```

## Mininig

##

```csharp
void MineHash(int difficulty)
{
    for (var nonce = 0; nonce <= Int32.MaxValue; nonce++)
    {
        var hash = CalculateHash(nonce);
        if (IsHashValid(hash, difficulty))
        {
            BlockHashBytes = hash;
            Nonce = nonce;
            return;
        }
    }
    throw new Exception("failed to mine block");
}
```

##

```csharp

bool IsHashValid(byte[] hash, int difficulty)
{
    if (difficulty > hash.Length)
        throw new Exception("difficulty exceeds hash length");

    for (var i = 0; i < difficulty; i++)
        if (hash[i] != 0) return false;

    return true;
}
```
