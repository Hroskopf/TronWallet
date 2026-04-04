using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Services;

public interface ITransactionService
{
    Task<List<WalletTransaction>> GetWalletsTransactionsAsync(Guid walletId);
    Task SendTransactionAsync(Guid fromUserId, string toAddress, decimal amountTrx);
}