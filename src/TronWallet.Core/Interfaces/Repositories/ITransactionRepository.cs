using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Guid> InsertAsync(WalletTransaction transaction);
    Task<List<WalletTransaction>> GetWalletsTransactionsAsync(Guid walletId);
    Task<List<WalletTransaction>> GetPendingAsync();
    Task UpdateStatusAsync(Guid id, string status, long BlockNumber, DateTime BlockTime);
}