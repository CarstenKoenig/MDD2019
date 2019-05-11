namespace BlockChain
{
  public static class ByteHelpers
  {
    public static bool IsSameAs(this byte[] bytesA, byte[] bytesB)
    {
      if (bytesA.Length != bytesB.Length)
        return false;

      for (var i = 0; i < bytesA.Length; i++)
        if (bytesA[i] != bytesB[i])
          return false;

      return true;
    }
  }
}