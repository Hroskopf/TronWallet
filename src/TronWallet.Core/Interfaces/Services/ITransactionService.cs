using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Services;

public interface ITransactionService
{
    Task<List<WalletTransaction>> GetWalletTransactionsAsync(Guid walletId);
    Task<WalletTransaction> SendTransactionAsync(Guid fromUserId, string toAddress, decimal amount);
}