using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Guid> InsertAsync(WalletTransaction transaction);
    Task<List<WalletTransaction>> GetAccountsTransactionsAsync(string address, int limit = 50, int offset = 0);
    Task<List<WalletTransaction>> GetPendingAsync();
    Task UpdateStatusAsync(Guid id, string status, long BlockNumber, DateTime BlockTime);
    Task<bool> ExistsTxByHashAsync(string txHash);
    Task<int> GetWalletTransactionsCountAsync(Guid walletId);
}