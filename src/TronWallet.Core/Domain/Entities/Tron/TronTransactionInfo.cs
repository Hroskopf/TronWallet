

namespace TronWallet.Core.Domain.Entities.Tron;

using System;
using System.Text.Json.Serialization;

public class TronTransactionInfo
{
    // Transaction ID
    [JsonPropertyName("id")]
    public string TxID { get; set; }

    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; set; }

    [JsonPropertyName("blockTimeStamp")]
    public long BlockTimeStamp { get; set; }

    [JsonPropertyName("receipt")]
    public TronTransactionReceipt Receipt { get; set; }

    [JsonPropertyName("raw_data")]
    public object RawData { get; set; } 
}

public class TronTransactionReceipt
{
    [JsonPropertyName("result")]
    public string Result { get; set; }
}