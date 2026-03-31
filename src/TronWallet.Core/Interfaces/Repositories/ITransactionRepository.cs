using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Guid> InsertAsync(WalletTransaction transaction);
}