using System;
using System.Security.Cryptography;
using Moto.Net.Util;

namespace Moto.Net.Mototrbo.FXP;

public enum Randomization {
  eNoRandom = 80,
  exKxK = 105,
  eKxKx = 209,
  eKxxKxx = 703,
  exxKxxK = 901,
  eKxxxKxxx = 1403,
  exxxKxxxK = 10782,
  e128xK = 56016,
}

public enum ProtocolVersion : ushort
{
  eAes256 = 1,
}

public enum Operation : ushort
{
  eDiscard = 1,
  eDiscardAck = 2,
  eFeatureSessionRequest = 3,
  eFeatureSessionRequestAck = 4,
  eFeatureCodeTransfer = 5,
  eFeatureCodeTransferAck = 6,
  eFeatureLockRequest = 7,
  eFeatureLockRequestAck = 8,
  eFeatureUnlockRequest = 9,
  eFeatureUnlockRequestAck = 10, // 0x000A
}

public sealed class FXPMessage : IDisposable
{
  private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

  private Randomization randomization = Randomization.eNoRandom;
  private ProtocolVersion protocolVersion = 0;
  private ushort sequenceNumber = 0;
  private Operation operation = Operation.eDiscard;
  private byte[] payload = (byte[])null;

  public void Dispose()
  {
    protocolVersion = 0;
    randomization = Randomization.eNoRandom;
    operation = Operation.eDiscard;
    ZeroBuff(payload);
    Payload = null;
  }

  public ProtocolVersion ProtocolVersion
  {
    private get => protocolVersion;
    set => protocolVersion = value;
  }

  public Randomization Randomization
  {
    private get => randomization;
    set => randomization = value;
  }

  public ushort SequenceNumber
  {
    get => sequenceNumber;
    set => sequenceNumber = value;
  }

  public Operation Operation
  {
    get => operation;
    set => operation = value;
  }

  public byte[] Payload
  {
    get => payload;
    set => payload = value;
  }

  public int PayloadLength => Payload == null ? 0 : Payload.Length;

  public byte[] ToMessageBytes(byte[] key, Random randGenerator)
  {
    byte[] encLayer = null;
    byte[] buff = null;
    try
    {
      encLayer = EncryptionLayer(VersionMapping.GetCryptoByProtocolVersion(ProtocolVersion), key, randGenerator);
      buff = RandomizationLayer(encLayer, randGenerator);
      return Bytes.ConcatArrays(
        ToBigEndianBytes((ushort)(2 + buff.Length)),
        ToBigEndianBytes((ushort)Randomization),
        buff
      );
    }
    catch (Exception ex)
    {
      throw new Exception("Error creating message", ex);
    }
    finally
    {
      ZeroBuff(encLayer);
      ZeroBuff(buff);
    }
  }

  public static FXPMessage ParseMessageBytes(byte[] key, byte[] rawMessage, uint prevSeqNum)
  {
    if (rawMessage == null || rawMessage.Length < 10)
      throw new ArgumentException("Invalid message length");
    byte[] deRandomized = null;
    byte[] decrypted = null;
    FXPMessage messageBytes = null;
    try
    {
      messageBytes = newFxpMessage();
      messageBytes.Randomization = (Randomization)FromBigEndianBytes(rawMessage, 0);
      deRandomized = DeRandomize(messageBytes.Randomization, rawMessage, 2);
      messageBytes.ProtocolVersion = (ProtocolVersion)FromBigEndianBytes(deRandomized, 0);
      messageBytes.SequenceNumber = FromBigEndianBytes(deRandomized, 2);
      if (messageBytes.SequenceNumber == (int)prevSeqNum)
        throw new ArgumentException("Duplicate sequence number");
      decrypted = Decrypt(VersionMapping.GetCryptoByProtocolVersion(messageBytes.ProtocolVersion), key, deRandomized, 4);
      messageBytes.Operation = (Operation)FromBigEndianBytes(decrypted, 0);
      ushort length = FromBigEndianBytes(decrypted, 2);
      messageBytes.Payload = new byte[length];
      Array.Copy(decrypted, 4, messageBytes.Payload, 0, messageBytes.Payload.Length);
    }
    catch (Exception ex)
    {
      log.Error("Error parsing message", ex);
      throw new Exception("Error parsing message", ex);
    }
    finally
    {
      ZeroBuff(deRandomized);
      ZeroBuff(decrypted);
    }
    return messageBytes;
  }

  private byte[] EncryptionLayer(SymmetricAlgorithm iCryptoProvider, byte[] key, Random randGenerator)
  {
    byte[] res = null;
    try
    {
      res = Bytes.ConcatArrays(
        ToBigEndianBytes((ushort)Operation),
        ToBigEndianBytes((ushort)PayloadLength),
        Payload
      );
      return Encrypt(iCryptoProvider, key, res, randGenerator);
    }
    finally
    {
      ZeroBuff(res);
    }
  }

  private byte[] RandomizationLayer(byte[] encryptLayer, Random randGenerator)
  {
    byte[] res = null;
    try
    {
      res = Bytes.ConcatArrays(
        ToBigEndianBytes((ushort)ProtocolVersion),
        ToBigEndianBytes(SequenceNumber),
        encryptLayer
      );
      return Randomize(Randomization, res, randGenerator);
    }
    finally
    {
      ZeroBuff(res);
    }
  }

  private static byte[] Randomize(Randomization randType, byte[] data, Random random)
  {
    switch (randType)
    {
      case Randomization.eNoRandom:
        return Randomize(0, true, data, random);
      case Randomization.exKxK:
        return Randomize(1, false, data, random);
      case Randomization.eKxKx:
        return Randomize(1, true, data, random);
      case Randomization.exxKxxK:
        return Randomize(2, false, data, random);
      case Randomization.eKxxKxx:
        return Randomize(2, true, data, random);
      case Randomization.exxxKxxxK:
        return Randomize(3, false, data, random);
      case Randomization.eKxxxKxxx:
        return Randomize(3, true, data, random);
      case Randomization.e128xK:
        byte[] res = new byte[data.Length + 128];
        random.NextBytes(res);
        Array.Copy(data, 0, res, 128, data.Length);
        return res;
        // return Randomize(0, true, data, random);
      default:
        throw new NotImplementedException("Randomization type not implemented");
    }
  }

  private static byte[] Randomize(int interval, bool startWithK, byte[] data, Random random)
  {
    byte[] res = new byte[data.Length * (interval + 1)];
    random.NextBytes(res);
    int offset = startWithK ? 0 : interval;
    for (int i = 0; i < data.Length; ++i)
    {
      int j = i * (interval + 1) + offset;
      res[j] = data[i];
    }
    return res;
  }
  private static byte[] DeRandomize(Randomization randType, byte[] data, int start)
  {
    switch (randType)
    {
      case Randomization.eNoRandom:
        return DeRandomize(0, true, data, start);
      case Randomization.exKxK:
        return DeRandomize(1, false, data, start);
      case Randomization.eKxKx:
        return DeRandomize(1, true, data, start);
      case Randomization.exxKxxK:
        return DeRandomize(2, false, data, start);
      case Randomization.eKxxKxx:
        return DeRandomize(2, true, data, start);
      case Randomization.exxxKxxxK:
        return DeRandomize(3, false, data, start);
      case Randomization.eKxxxKxxx:
        return DeRandomize(3, true, data, start);
      case Randomization.e128xK:
        return DeRandomize(0, true, data, start + 128);
      default:
        throw new NotImplementedException("Randomization type not implemented");
    }
  }

  private static byte[] DeRandomize(int interval, bool startWithK, byte[] data, int start)
  {
    var length = data.Length - start;
    if (length < 0 || length % (interval + 1) != 0) {
      throw new ArgumentException("Invalid data length");
    }

    var outLen = length / (interval + 1);
    var res = new byte[outLen];
    int offset = startWithK ? start : start + interval;
    for (int i = 0; i < outLen; ++i)
    {
      int j = (interval + 1) * i + offset;
      res[i] = data[j];
    }
    return res;
  }

  private static byte[] Encrypt(SymmetricAlgorithm iCryptoProvider, byte[] key, byte[] input, Random randGenerator)
  {
    try {
      int inputCount = iCryptoProvider.BlockSize / 8;
      int num = input.Length % inputCount;
      byte[] res = new byte[num == 0 ? input.Length : input.Length + (inputCount - num)];
      randGenerator.NextBytes(res);
      input.CopyTo(res, 0);
      for (int index = 0; index < res.Length; index += inputCount)
      {
        using ICryptoTransform enc = iCryptoProvider.CreateEncryptor(key, new byte[inputCount]);
        byte[] buff = enc.TransformFinalBlock(res, index, inputCount);
        buff.CopyTo(res, index);
        ZeroBuff(buff);
      }
      return res;
    } catch (Exception ex) {
      throw new Exception("Error encrypting data", ex);
    }
  }

  private static byte[] Decrypt(SymmetricAlgorithm iCryptoProvider, byte[] key, byte[] input, int start)
  {
    try {
      int inputCount = iCryptoProvider.BlockSize / 8;
      byte[] res = new byte[input.Length - start];
      for (int i = 0; i < res.Length; i += inputCount)
      {
        using ICryptoTransform dec = iCryptoProvider.CreateDecryptor(key, new byte[inputCount]);
        byte[] buff = dec.TransformFinalBlock(input, i + start, inputCount);
        buff.CopyTo(res, i);
        ZeroBuff(buff);
      }
      return res;
    } catch (Exception ex) {
      throw new Exception("Error decrypting data", ex);
    }
  }

  public static ushort FromBigEndianBytes(byte[] data, int index)
  {
    int length = 2;
    if (data == null || data.Length < index + length)
      return 0;
    byte[] destinationArray = new byte[length];
    Array.Copy(data, index, destinationArray, 0, 2);
    if (BitConverter.IsLittleEndian)
      Array.Reverse((Array)destinationArray);
    return BitConverter.ToUInt16(destinationArray, 0);
  }

  private static byte[] ToBigEndianBytes(ushort val)
  {
    byte[] bytes = BitConverter.GetBytes(val);
    if (BitConverter.IsLittleEndian)
      Array.Reverse((Array)bytes);
    return bytes;
  }

  public static void ZeroBuff(byte[] buff)
  {
    if (buff == null)
      return;

    Array.Clear(buff, 0, buff.Length);
  }

  private static FXPMessage newFxpMessage() => new FXPMessage();
}
