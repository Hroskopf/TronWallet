using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace TronWallet.Core.Domain.Entities.Tron;
public class TronTransactionInfo
{
    [JsonPropertyName("id")]
    public string? TxID { get; set; }

    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; set; }

    [JsonPropertyName("blockTimeStamp")]
    public long BlockTimeStamp { get; set; }

    [JsonPropertyName("contractResult")]
    public List<string>? ContractResult { get; set; }

    [JsonPropertyName("receipt")]
    public TronReceipt? Receipt { get; set; }

    // IMPORTANT
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("resMessage")]
    public string? ResMessage { get; set; }
}

public class TronReceipt
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }
}