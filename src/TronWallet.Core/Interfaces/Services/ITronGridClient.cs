using TronWallet.Core.Domain.Entities.Tron;

namespace TronWallet.Core.Interfaces.Services;

public interface ITronGridClient
{
    Task<TronAccountResponse?>          GetAccountAsync(string base58Address);
    Task<TronUnsignedTx>                CreateTransactionAsync(string fromHex, string toHex, long amountSun);
    Task<TronBroadcastResponse>         BroadcastTransactionAsync(object signedTx);
    Task<TronTransactionInfo?>          GetTransactionInfoAsync(string txHashHex);
    // Task<TronTransactionsListResponse>  GetTransactionsAsync(string base58Address, int limit = 20, string? fingerprint = null);
}