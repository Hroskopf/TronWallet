using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TronWallet.Core.Domain.Entities.Tron;


public class TronTransactionInfo
{
    [JsonPropertyName("txID")]
    public string? TxID { get; set; }

    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; set; }

    [JsonPropertyName("blockTimeStamp")]
    public long BlockTimeStamp { get; set; }

    [JsonPropertyName("contractResult")]
    public List<string>? ContractResult { get; set; }

    [JsonPropertyName("receipt")]
    public TronReceipt? Receipt { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("resMessage")]
    public string? ResMessage { get; set; }

    [JsonPropertyName("raw_data")]
    public TronRawData? RawData { get; set; }

    [JsonIgnore]
    public string? FromAddressHex
    {
        get
        {
            if (RawData?.Contract != null && RawData.Contract.Count > 0)
                return RawData.Contract[0].Parameter?.Value?.OwnerAddress;
            return null;
        }
    }

    [JsonIgnore]
    public string? ToAddressHex
    {
        get
        {
            if (RawData?.Contract != null && RawData.Contract.Count > 0)
                return RawData.Contract[0].Parameter?.Value?.ToAddress;
            return null;
        }
    }
}

public class TronReceipt
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }
}

public class TronRawData
{
    [JsonPropertyName("contract")]
    public List<TronContract>? Contract { get; set; }
}

public class TronContract
{
    [JsonPropertyName("parameter")]
    public TronParameter? Parameter { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class TronParameter
{
    [JsonPropertyName("value")]
    public TronContractValue? Value { get; set; }

    [JsonPropertyName("type_url")]
    public string? TypeUrl { get; set; }
}

public class TronContractValue
{
    [JsonPropertyName("owner_address")]
    public string? OwnerAddress { get; set; }

    [JsonPropertyName("to_address")]
    public string? ToAddress { get; set; }

    [JsonPropertyName("amount")]
    public long? Amount { get; set; }
}
