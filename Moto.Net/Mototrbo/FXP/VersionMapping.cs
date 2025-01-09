using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Moto.Net.Mototrbo.FXP;

internal class VersionMapping
{
  private static Dictionary<ProtocolVersion, SymmetricAlgorithm> CryptoVersionMappingTable = InitCryptoVersionMappingTable();

  private static Dictionary<ProtocolVersion, SymmetricAlgorithm> InitCryptoVersionMappingTable()
  {
    Dictionary<ProtocolVersion, SymmetricAlgorithm> dictionary = new Dictionary<ProtocolVersion, SymmetricAlgorithm>();
    SymmetricAlgorithm symmetricAlgorithm1 = null;
    try
    {
      symmetricAlgorithm1 = new AesCryptoServiceProvider
      {
          BlockSize = 128,
          KeySize = 256,
          Padding = PaddingMode.None
      };
      dictionary.Add(ProtocolVersion.eAes256, symmetricAlgorithm1);
      symmetricAlgorithm1 = null;
    }
    finally
    {
      symmetricAlgorithm1?.Dispose();
    }
    return dictionary;
  }

  public static bool IsProtocolVersionSupport(ProtocolVersion protocolVersion)
  {
    return CryptoVersionMappingTable.ContainsKey(protocolVersion);
  }

  public static SymmetricAlgorithm GetCryptoByProtocolVersion(ProtocolVersion protocolVersion)
  {
    return CryptoVersionMappingTable[protocolVersion];
  }

  public static ProtocolVersion GetProtocolVersionByFWVersion(string FWVersion)
  {
    return FWVersion.StartsWith("R", StringComparison.OrdinalIgnoreCase) ? ProtocolVersion.eAes256 : ProtocolVersion.eAes256;
  }
}
