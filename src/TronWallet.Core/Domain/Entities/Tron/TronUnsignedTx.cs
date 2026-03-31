using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace TronWallet.Core.Domain.Entities.Tron;
public class TransferContract
{
    [JsonPropertyName("owner_address")]
    public string OwnerAddress { get; set; }

    [JsonPropertyName("to_address")]
    public string ToAddress { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }
}

public class Parameter
{
    [JsonPropertyName("value")]
    public TransferContract Value { get; set; }

    [JsonPropertyName("type_url")]
    public string TypeUrl { get; set; }
}

public class Contract
{
    [JsonPropertyName("parameter")]
    public Parameter Parameter { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class RawData
{
    [JsonPropertyName("contract")]
    public List<Contract> Contract { get; set; }

    [JsonPropertyName("ref_block_bytes")]
    public string RefBlockBytes { get; set; }

    [JsonPropertyName("ref_block_hash")]
    public string RefBlockHash { get; set; }

    [JsonPropertyName("expiration")]
    public long Expiration { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

public class TronUnsignedTx
{
    [JsonPropertyName("txID")]
    public string TxID { get; set; }

    [JsonPropertyName("raw_data")]
    public RawData RawData { get; set; }

    [JsonPropertyName("raw_data_str")]
    public string RawDataStr { get; set; }

    [JsonPropertyName("raw_data_hex")]
    public string RawDataHex { get; set; }

    public string GetTxHash()
    {
        if (string.IsNullOrEmpty(RawDataHex))
            throw new InvalidOperationException("RawDataHex is null or empty");

        byte[] bytes = Convert.FromHexString(RawDataHex);
        byte[] hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLower();
    }
}
