using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Domain.Entities.Tron;
using System.Net.Http.Json;

namespace TronWallet.Infrastructure.Tron;
public sealed class TronGridClient : ITronGridClient
{
    private readonly HttpClient _http;

    public TronGridClient(HttpClient http) => _http = http;

    public async Task<TronAccountResponse?> GetAccountAsync(string base58Address)
    {
        var response = await _http.GetAsync($"/v1/accounts/{base58Address}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TronAccountResponse>();
    }

    public async Task<TronUnsignedTx> CreateTransactionAsync(
        string fromHex, string toHex, long amountSun)
    {
        var body = new { owner_address = fromHex, to_address = toHex, amount = amountSun };
        var response = await _http.PostAsJsonAsync("/wallet/createtransaction", body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TronUnsignedTx>()
               ?? throw new InvalidOperationException("Empty response from TronGrid");
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