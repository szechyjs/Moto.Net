using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Moto.Net.Mototrbo.XNL.XCMP;
using acryptohashnet;

namespace Moto.Net.Mototrbo.FXP;

public class FXPClient : IDisposable
{
  private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

  private UdpClient client;
  private IPEndPoint endpoint = IPEndPoint.Parse("192.168.10.1:4070");
  private XCMPClient xcmpClient = null;

  private ProtocolVersion protocolVersion = ProtocolVersion.eAes256;
  private Random rand = new Random();
  private ushort SequenceNumberCounter = 0;
  private uint prevRecvSeqNum = 0;
  private bool isRemote = false;
  private int ReceiveTimeout = 1000;
  private int CommRetryTimes = 3;

  public FXPClient() : this(4070) { }

  public FXPClient(int port)
  {
    client = new UdpClient(port);
    client.Client.ReceiveTimeout = ReceiveTimeout;
    client.Connect(endpoint);
  }

  private bool IsOpen => client != null;

  public void Dispose()
  {
    client.Dispose();
    client = null;
  }

  public void Enable(string fwVersion, byte[] hashInput, byte[] featureCodeID)
  {
    protocolVersion = VersionMapping.GetProtocolVersionByFWVersion(fwVersion);
    if (!VersionMapping.IsProtocolVersionSupport(protocolVersion))
      throw new Exception("Protocol version not supported");
    this.FeatureExchange(hashInput, featureCodeID, true);
  }

  public void Disable(byte[] hashInput, byte[] featureCodeID)
  {
    FeatureExchange(hashInput, featureCodeID, false);
  }

  private void FeatureExchange(byte[] hashInput, byte[] featureCodeID, bool bEnable)
  {
    if (featureCodeID == null || hashInput == null || hashInput.Length != 128)
      throw new ArgumentException("Invalid argument");
    ResetPrevRecvSeqNum();
    byte[] encryptKey = null;
    byte[] hashedProductId = null;
    try
    {
      using (HashAlgorithm hashAlgorithm = new Haval256(HavalPassCount.Pass4))
        hashedProductId = hashAlgorithm.ComputeHash(hashInput);
      RandomSendDiscardMessage(hashedProductId);
      encryptKey = FeatureSessionRequest(hashedProductId);
      byte keyPosition = FeatureCodeTransfer(encryptKey, featureCodeID);
      RandomSendDiscardMessage(encryptKey);
      FeatureLockUnlockRequest(encryptKey, keyPosition, !bEnable);
    }
    catch (Exception ex)
    {
      log.Error("Feature exchange failed", ex);
      throw new Exception("Feature exchange failed", ex);
    }
    finally
    {
      FXPMessage.ZeroBuff(encryptKey);
      FXPMessage.ZeroBuff(hashedProductId);
    }
  }

  private byte[] FeatureSessionRequest(byte[] hashedProductID)
  {
    SymmetricAlgorithm byProtocolVersion = VersionMapping.GetCryptoByProtocolVersion(protocolVersion);
    byProtocolVersion.GenerateKey();
    byte[] decryptKey = (byte[])byProtocolVersion.Key.Clone();
    using (FXPMessage sesReq = CreateFXPMessage(Operation.eFeatureSessionRequest, decryptKey))
    {
      using (FXPMessage sesReply = TransferMessage(sesReq, hashedProductID, decryptKey))
      {
        int length = 32;
        if (sesReply.Operation != Operation.eFeatureSessionRequestAck || sesReply.PayloadLength != length + 1 || sesReply.Payload[0] != 1) {
          log.Error("Feature session request failed");
          throw new Exception("Feature session request failed");
        }
        byte[] encryptKey = new byte[length];
        Array.Copy(sesReply.Payload, 1, encryptKey, 0, encryptKey.Length);
        return encryptKey;
      }
    }
  }

  private byte FeatureCodeTransfer(byte[] encryptKey, byte[] featureCode)
  {
    if (featureCode == null)
      throw new ArgumentException("Missing feature code");

    // create a random array of bytes
    int randLength = rand.Next(1, 9);
    byte[] randBytes = new byte[randLength];
    for (int i = 0; i < randLength; ++i)
      randBytes[i] = (byte)rand.Next(0, 6);

    // select a random byte from the array and its index
    byte randByte = randBytes[rand.Next(0, randLength)];
    int randByteIndex = 0;
    for (int index = randLength - 1; index >= 0; --index)
    {
      if (randByte == randBytes[index])
      {
        randByteIndex = index;
        break;
      }
    }

    byte[] buff = null;
    try
    {
      byte[] featCopy = featureCode;
      buff = featCopy != null ? new byte[1 + featCopy.Length] : throw new Exception("Invalid feature");
      buff[0] = randByte;
      featCopy.CopyTo(buff, 1); // [randByte, fe, at]
      for (int i = 0; i < randLength; ++i)
      {
        byte[] msgData;
        if (i == randByteIndex)
        {
          // use the selected random byte as the first byte of the message data
          msgData = buff; // [randByte, fe, at]
        }
        else
        {
          // use corresponding byte from randBytes, and fill the rest with random bytes
          msgData = new byte[1 + featCopy.Length];
          rand.NextBytes(msgData);
          msgData[0] = randBytes[i];
        }
        RandomSendDiscardMessage(encryptKey);
        using FXPMessage msg = CreateFXPMessage(Operation.eFeatureCodeTransfer, msgData);
        using FXPMessage reply = TransferMessage(msg, encryptKey, encryptKey);
        if (reply.Operation != Operation.eFeatureCodeTransferAck || reply.PayloadLength != 1 || reply.Payload[0] != 1)
            throw new Exception("Feature code transfer failed");
      }
      return randByte;
    }
    finally
    {
      FXPMessage.ZeroBuff(buff);
    }
  }

  private void FeatureLockUnlockRequest(byte[] encryptKey, byte keyPosition, bool bLock)
  {
    byte[] payload;
    Operation Operation;
    if (bLock)
    {
      payload = new byte[1];
      Operation = Operation.eFeatureLockRequest;
    }
    else
    {
      payload = new byte[129];
      Operation = Operation.eFeatureUnlockRequest;
    }
    payload[0] = keyPosition;
    using FXPMessage msg = CreateFXPMessage(Operation, payload);
    using FXPMessage reply = TransferMessage(msg, encryptKey, encryptKey);
    Operation operationEnum = bLock ? Operation.eFeatureLockRequestAck : Operation.eFeatureUnlockRequestAck;
    if (reply.Operation != operationEnum || reply.PayloadLength != 1)
    {
      log.Error("Unexpected unlock reply: " + reply.Operation);
      throw new Exception("Feature lock/unlock request failed");
    }
    if (reply.Payload[0] != 1)
    {
      log.Error("Unlock status: " + reply.Payload[0]);
      throw new Exception("Feature failed");
    }
  }

  private void RandomSendDiscardMessage(byte[] key, int nMinToSend, int nMaxToSend)
  {
    int num = rand.Next(nMinToSend, nMaxToSend);
    for (int index = 0; index < num; ++index)
    {
      byte[] res = new byte[16];
      rand.NextBytes(res);
      using (FXPMessage fxpMessage1 = CreateFXPMessage(Operation.eDiscard, res))
      {
        using (FXPMessage fxpMessage2 = TransferMessage(fxpMessage1, key, key))
        {
          if (fxpMessage2.Operation != Operation.eDiscardAck || fxpMessage2.PayloadLength < 1)
            throw new Exception("Discard message failed");
        }
      }
    }
  }

  private FXPMessage TransferMessage(FXPMessage message, byte[] encryptKey, byte[] decryptKey)
  {
    byte[] txPayload = null;
    byte[] rxPayload = null;
    byte[] rxBuffer  = null;
    FXPMessage fxpMessage = null;
    try
    {
      txPayload = message.ToMessageBytes(encryptKey, rand);
      for (int txRetry = 0; txRetry < CommRetryTimes; ++txRetry)
      {
        try
        {
          if (isRemote)
          {
            XferDataRequest msg = new XferDataRequest(0, DataType.Fxp, (ushort)txPayload.Length, txPayload);
            XferDataReply reply = (XferDataReply)xcmpClient.SendPacketAndWaitForSameType(msg);
            if (reply.ErrorCode != XCMPErrorCode.Success)
              throw new Exception("TransferMessage failed");
            rxPayload = new byte[reply.Length - 2];
            Array.Copy(reply.Payload, 2, rxPayload, 0, reply.Length - 2);
            // TODO: is length - 2 correct?
          }
          else
            SendPayload(txPayload);
          for (int rxRetry = 0; rxRetry < 5; ++rxRetry)
          {
            try
            {
              if (!isRemote)
              {
                rxBuffer = ReceivePayload();
                ushort count = FXPMessage.FromBigEndianBytes(rxBuffer, 0);
                rxPayload = new byte[count];
                Buffer.BlockCopy(rxBuffer, 2, rxPayload, 0, count);
              }
              fxpMessage = FXPMessage.ParseMessageBytes(decryptKey, rxPayload, prevRecvSeqNum);
              prevRecvSeqNum = fxpMessage.SequenceNumber;
              break;
            }
            catch (Exception)
            {
              if (rxRetry >= 4)
                throw;
            }
          }
          break;
        }
        catch (Exception ex)
        {
          if (txRetry >= CommRetryTimes - 1)
            log.Error("TransferMessage failed", ex);
            throw new Exception("TransferMessage failed", ex);
        }
      }
      return fxpMessage;
    }
    finally
    {
      FXPMessage.ZeroBuff(rxBuffer);
      FXPMessage.ZeroBuff(rxPayload);
      FXPMessage.ZeroBuff(txPayload);
    }
  }

  private void SendPayload(byte[] payload)
  {
    client.Send(payload, payload.Length);
  }

  private byte[] ReceivePayload()
  {
    return client.Receive(ref endpoint);
  }

  private FXPMessage CreateFXPMessage(Operation Operation, byte[] payload)
  {
    FXPMessage fxpMessage1 = null;
    FXPMessage fxpMessage2 = null;
    try
    {
      fxpMessage2 = new FXPMessage();
      Randomization[] values = (Randomization[])Enum.GetValues(typeof(Randomization));
      int index = rand.Next(0, values.Length);
      fxpMessage2.Randomization = values[index];
      fxpMessage2.ProtocolVersion = protocolVersion;
      fxpMessage2.SequenceNumber = GetNextSequenceNumber();
      fxpMessage2.Operation = Operation;
      fxpMessage2.Payload = payload;
      fxpMessage1 = fxpMessage2;
      fxpMessage2 = null;
    }
    finally
    {
      fxpMessage2?.Dispose();
    }
    return fxpMessage1;
  }


  private void RandomSendDiscardMessage(byte[] key) => RandomSendDiscardMessage(key, 0, 2);

  private void ResetPrevRecvSeqNum() => prevRecvSeqNum = uint.MaxValue;

  private ushort GetNextSequenceNumber()
  {
    if (SequenceNumberCounter == ushort.MaxValue)
      SequenceNumberCounter = 0;
    return SequenceNumberCounter++;
  }
}
