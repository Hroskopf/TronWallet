using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Services;

public interface ITransactionService
{
    Task SendTransactionAsync(Guid fromUserId, string toAddress, decimal amountTrx);
    Task<List<WalletTransaction>> GetAccountsTransactionsAsync(string address, int limit = 50, int offset = 0);
    Task<int> GetWalletTransactionsCountAsync(Guid walletId);
}