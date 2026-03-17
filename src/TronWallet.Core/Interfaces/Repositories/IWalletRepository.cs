using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface IWalletRepository
{
    Task InsertAsync(string privateKeyEnc);
}