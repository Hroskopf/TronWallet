using System.Text.Json.Serialization;

namespace TronWallet.Core.Domain.Entities.Tron;

public class TronAccountResponse
{
    public Account? Account { get; set; }
    public bool Success { get; set; }
}

public class TronAccountResponseDto
{
    public List<Account>? Data { get; set; }
    public bool Success { get; set; }
}

public class Account
{
    public string Address { get; set; } = "";
    public decimal Balance { get; set; } = 0; // in SUN

    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

}