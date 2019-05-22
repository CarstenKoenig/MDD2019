---
author: Carsten König
title: BlockChain selbst gebastelt
---

# Blockchain

## Fragen

in dezentralen Systemen:

- Wie kann *Konsens* erreicht werden?
- Wie können *Fälschungen* und *Manipulationen* verhindert werden?
- Können Daten *transparent* und trotzdem *vertraulich* gespeichert werden?

:::notes
- wollen Daten dezentral Speichern
- Knoten im Netz ist ein Teilnehmer
- Übertragung nicht sicher
- Netz kann partitioniert sein
:::

# Krypto-Währung

##

![](../Images/Sparschwein.jpg)

:::notes
- erinnert sich noch jemand daran
- lange gespart und dann zur Bank gebracht
:::

##

![](../Images/Sparbuch.jpg)

:::notes
- gezählt und ins Sparbuch eingetragen
- dort konnte man nachsehen, was man hat
:::

## Ledger

```
...

Einzahlung: 07.01. 50 DM - Guthaben 220 DM
Einzahlung: 13.03. 10 DM - Guthaben 230 DM
Zinsen      31.12.  2 DM - Guthaben 232 DM
Abhebung:   09.09. 20 DM - Guthaben 212 DM
...
```

:::notes
- jede Transaktion aufs Konto aufgeführt
- Guthaben = Summe Einzahlungen - Summe Abhebungen
- Vielleicht gab es noch Zinsen
- Mit dem Sparbuch konnte man Guthaben *beweisen*
:::

##

![zentralisiert](../Images/CentralNetwork.png)

:::notes
- Vertrauen in Bank nötig
:::

## öffentlicher Ledger

```
...

Max  zahlt 50 € an Anne
Anne zahlt 10 € an Charles
...
```

:::notes
- machen Transaktionen öffentlich
- jeder kann Guthaben nachrechnen
:::

## 

![](../Images/P2PwithData.png)

:::notes
- jeder hat den gesamten Ledger
:::

##

![neue Zahlung](../Images/NodeCreatesNewTransaction.png)

:::notes
- eine neue Zahlung wird im Netz propagiert
:::

##

![wird im Netz verteilt](../Images/NodeSendsNewTransaction.png)

## Privatsphäre?

```
...

Max  zahlt 50 € an Anne
Anne zahlt 10 € an Charles
Charles zahlt 10.000 € an Max
...
```

:::notes
- Privatsphäre?
- warum zahlt Charles 10k an Max?
:::

## *Vertrauen*

```
...

Max  zahlt 50 € an Anne
Anne zahlt 10 € an Charles
...
Charles zahlt 10 € an Carsten :)
```

:::notes
- gefälschte Zahlungen
- Double-Spent (durch Verzögerungen im Netzwerk)
:::

## Vertrauen

![](../Images/P2PwithData.png)

:::notes
- rote Knoten in der Mitte könnte
- Datenfluss manipuliert
- gehen wir die Probleme an
:::

## 

![Unterschrift](../Images/SparbuchUnterschrift.jpg)

:::notes
- gefälschte Zahlungen
- wie war das beim Sparbuch?
- dort hat der Mitarbeiter (Vertrauen) unterschrieben / gestempelt
- wie für unseren öffentlichen Ledger
:::

##

![](../Images/ECDSA.png)

:::notes
- public-key Kryptographie
- public-Key ist die Konto-Nummer
- kann jeder selbst erzeugen
- private muss selber aufgehoben werden
:::

##

`sender` *zahlt* `42 €` an `receiver`

![Transaktion](../Images/Transaction.png)

:::notes
- soll unterschrieben werden
:::

##

![Transaktion unterschreiben...](../Images/Hashing.png)

:::notes
- Sender / Receiver / Amount wird gehasht
- Krypto-Hash: relativ einfach zu Berechnen
- Ändert sich stark wenn sich die Nachricht leicht ändert
- Unvorhersehbar
:::

##

![Transaktion unterschreiben](../Images/Signing.png)

:::notes
- Hash wird mit Private-Key des Senders unterschrieben
- Hash wird mit angehängt
:::

##

![Transaktion verifizieren](../Images/Verify.png)

:::notes
- aus Sender, Receiver, Amount erneut hashen
- Signatur mit public-Key verifizieren
- ECDSA ein wenig anders (Hash wird nicht direkt verglichen)
:::

## 

```csharp
public class Signature
{
  readonly RSAOpenSsl _signAlgorithm;

  private string PrivateKey { get; }
  public string PublicKey { get; }

  string SignData(byte[] data)

  static bool VerifyHash(
     string publicKey, 
     byte[] data, 
     string signature)
}

```

:::notes
- Hash als String
- PubK/PrivK auch als String
:::

## Unterschreiben

```csharp
class Signature
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

:::notes
- mit SHA256 hashen und mit RSA Unterschreiben
- Bitcoin nutzt ECDSA
:::

## verifizieren

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

##

```csharp
class Transaction
{
  string SenderPublicKey { get; }
  string ReceiverPublicKey { get; }
  decimal Amount { get; }
  string Signature { get; }

  bool Verify()
}
```

:::notes
- Keys + Amount werden mit private Key unterschrieben 
:::

## Erreicht

- Privatsphären (?)
- Vertrauenswürdige Transaktionen

:::notes
- keine Namen nur selber-erzeugte Public Keys sichtbar
- könnten bei jeder Transaktion neu erzeugt werden
- Transaktionen sind schwer zu fälschen
:::

## doppelte Einträge

```
...

Max  zahlt 50 € an Anne
Anne zahlt 10 € an Charles
Anne zahlt 10 € an Charles
...
```

:::notes
- immer noch möglich
- in Bitcoin wird mit Transaktionen bezahlt
- Double-Spent
:::

## Konsens?

![](../Images/P2PwithData.png)


:::notes
- Blockchain muss Konsens erreichen
- eventuelle Consistency
:::

# BlockChain und Konsens

##

![Block](../Images/BlockDemo.png)

:::notes
- Block wird über seinen Hash identifiziert
- zeigt auf den Hash von vorherigen Block
- dadurch Graph / Baum
:::

## 

```csharp
class Block
{
    string BlockHash { get; private set; }

    byte[] PreviousHash { get; }
    int Nonce { get; private set; }

    byte[] Content { get; }

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

        return memoryStream.ToArray();
    }
}
```

## Block validieren

```csharp
bool ValidateBlock()
{
    return BlockHashBytes
        .IsSameAs(CalculateHash())
        && ValidateContent(Content);
}
```

:::notes
- Content sollte auch verifiziert werden
- z.B. sind alle Transaktionen korrekt?
:::

##

![BlockChain](../Images/BlockChainDemo.png)

:::notes
- Blockchain beginnt mit *Genisis*-Block
- Blockchain ist der längste Pfad in diesem Baum (meiste Arbeit)
:::


## Blockchain

```csharp
class BlockChain : IEnumerable<Block>
{
    int _difficulty;
    readonly Dictionary<String, Block> _blocks;
    readonly Block _genisisBlock;
    Block _lastBlock;
    ...
}
```

## Block hinzufügen

```csharp
bool AddBlock(Block newBlock)
{
    if (!newBlock.ValidateBlock())
        return false;
    if (!_blocks.Contains(newBlock.PreviousHash))
        return false;
        
    if (PathLen(_genisisBlock, newBlock) > 
        PathLen(_genisisBlock, _lastBlock)))
        _lastBlock = newBlock;

    return true;
}
```

:::notes
- falls der Block nicht auf den letzten Block zeigt könnte ein Fork vorliegen
- Pseudo-Code - habe ich so nicht beachtet
:::

## Chain validieren

```csharp
bool IsChainValid()
{
    var previous = _genisisBlock;
    foreach (var block in PathTo(_lastBlock))
    {
        if (previous.BlockHash != block.PreviousHash ||
            !block.ValidateBlock())
            return false;
        previous = block;
    }

    return true;
}
```

## Mining?

![Nonce](../Images/BlockChainDemo.png)

:::notes
- Nonce kommt ins Spiel
- Kryptographisches Puzzle: finde Nonce so, dass der Hash mit 0en beginnt
:::

##

**Hash** beginnt mit `x` Nullen (*Schwierigkeit*)

nonce: 69782 - hash: A0B32524..

nonce: 69783 - hash: 4953887B..

nonce: 69784 - hash: 5004D7E5..

nonce: 69785 - hash: **0000**F9EF...

:::notes
- nicht vorhersehbar
- Glücksache
- WS zu finden steigt mit Rechenpower
- dadurch ist die Chain praktisch unveränderbar!
:::

## Demo

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

## Miner

![einige Nodes beteiligen sich am "minen"](../Images/Mining1.png)

##

![Transaktionen in öffentliche Liste](../Images/Mining2.png)

##

![Miner wählen Transaktionen](../Images/Mining3.png)

##

![Gewinner verbreitet neuen Block](../Images/Mining4.png)

## Konsens

![2 Knoten finden neuen Block](../Images/Consensus1.png)

##

![wird im Netzwerk verbreitet](../Images/Consensus2.png)

##

![kein Konsens](../Images/Consensus3.png)

##

![Knoten findet neuen Block](../Images/Consensus4.png)

##

![Knoten wählen die längste Kette](../Images/Consensus5.png)

##

![Konsens wieder hergestellt](../Images/Consensus6.png)

## Vertrauen(?)

![](../Images/P2PwithData.png)

:::notes
- zur Erinnerung: roter Knoten könnte manipulieren
- dann müsste er aber ständig eine eigene Blockchain minen
- Netz versucht alle 10min. einen Block zu finden
- geht also nur, wenn er min. 50% Leistung hat
:::

# Fragen vom Anfang

## Wie kann Konsens erreicht werden?

über Blockchain-Eigenschaft / Algorithmus

## Wie können Fälschungen und Manipulationen verhindert werden?

- über verlinkte Hashes und *Proof of Work*
- über kryptographische Signaturen

## Transparent und trotzdem vertraulich

- Daten öffentlich
- Identitäten über selbst erstellte Schlüssel

# Probleme

## extrem hoher Energiebedarf

## zu hoher Rechenaufwand für gemeinschaftliches Minen

:::notes
- Alternativen zu ProofOfWork (ProofOfStake,...)
:::

# Fragen?

## Vielen Dank
