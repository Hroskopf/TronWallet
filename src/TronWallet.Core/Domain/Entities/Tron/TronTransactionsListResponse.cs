
namespace TronWallet.Core.Domain.Entities.Tron;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class TronTransactionsListResponse
{

    public TronTransactionsListResponse()
    {
        Data = new List<TronTransactionInfo>();
        Success = false;
        Meta = new TronMeta();
    }

    [JsonPropertyName("data")]
    public List<TronTransactionInfo>? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("meta")]
    public TronMeta? Meta { get; set; }
}

public class TronMeta
{
    [JsonPropertyName("at")]
    public long At { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    // Optional fields (sometimes present)
    [JsonPropertyName("fingerprint")]
    public string? Fingerprint { get; set; }

    [JsonPropertyName("links")]
    public TronLinks? Links { get; set; }
}

public class TronLinks
{
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}