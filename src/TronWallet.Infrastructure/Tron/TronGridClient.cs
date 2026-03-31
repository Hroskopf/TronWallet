using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Domain.Entities.Tron;
using System.Net.Http.Json;
using System.Text.Json;

namespace TronWallet.Infrastructure.Tron;
public sealed class TronGridClient : ITronGridClient
{
    private readonly HttpClient _http;

    public TronGridClient(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("TronGrid");
    }

    public async Task<TronAccountResponse?> GetAccountAsync(string address)
    {
        var response = await _http.GetAsync($"v1/accounts/{address}");
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<TronAccountResponseDto>();

        return new TronAccountResponse
        {
            Success = dto?.Success ?? false,
            Account = dto?.Data?.FirstOrDefault()
        };
    }

    public async Task<TronUnsignedTx> CreateTransactionAsync(
        string fromHex, string toHex, decimal amountSun)
    {
        var body = new { owner_address = fromHex, to_address = toHex, amount = amountSun };
        var response = await _http.PostAsJsonAsync("/wallet/createtransaction", body);
        response.EnsureSuccessStatusCode();


        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var tx = JsonSerializer.Deserialize<TronUnsignedTx>(json, options)
                ?? throw new InvalidOperationException("Empty response from TronGrid");
        

        using (var doc = JsonDocument.Parse(json))
        {
            if (doc.RootElement.TryGetProperty("raw_data", out var rawDataElement))
            {
                tx.RawDataStr = rawDataElement.GetRawText();
            }
        }


        return tx;

    }

    public async Task<TronBroadcastResponse> BroadcastTransactionAsync(object signedTx)
    {
        var response = await _http.PostAsJsonAsync("/wallet/broadcasttransaction", signedTx);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TronBroadcastResponse>()
               ?? throw new InvalidOperationException("Empty broadcast response");
    }

    public async Task<TronTransactionInfo?> GetTransactionInfoAsync(string txHashHex)
    {
        var body = new { value = txHashHex };
        var response = await _http.PostAsJsonAsync("/wallet/gettransactioninfobyid", body);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TronTransactionInfo>();
    }

    // public async Task<TronTransactionsListResponse> GetTransactionsAsync(
    //     string base58Address, int limit = 20, string? fingerprint = null)
    // {
    //     var url = $"/v1/accounts/{base58Address}/transactions?limit={limit}";
    //     if (fingerprint is not null) url += $"&fingerprint={fingerprint}";
    //     var response = await _http.GetAsync(url);
    //     response.EnsureSuccessStatusCode();
    //     return await response.Content.ReadFromJsonAsync<TronTransactionsListResponse>()
    //            ?? new TronTransactionsListResponse([]);
    // }
}