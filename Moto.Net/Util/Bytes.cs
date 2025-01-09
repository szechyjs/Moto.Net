using System;

namespace Moto.Net.Util;

public class Bytes
{
  public static byte[] ConcatArrays(params byte[][] byteArrays)
  {
    int length = 0;
    foreach (byte[] byteArray in byteArrays)
    {
      if (byteArray != null)
        length += byteArray.Length;
    }
    if (length == 0)
      return null;
    byte[] res = new byte[length];
    int index = 0;
    foreach (byte[] byteArray in byteArrays)
    {
      if (byteArray != null)
      {
        byteArray.CopyTo(res, index);
        index += byteArray.Length;
      }
    }
    return res;
  }
}
