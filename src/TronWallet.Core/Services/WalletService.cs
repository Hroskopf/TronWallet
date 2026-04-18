using TronWallet.Core.Domain.Entities;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Core.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITronGridClient _tronGridClient;

    public WalletService(IWalletRepository walletRepository, ITronGridClient tronGridClient)
    {
        _walletRepository = walletRepository;
        _tronGridClient = tronGridClient;
    }

    public async Task<Wallet> GetWalletByUserIdAsync(Guid id)
    {
        var wallet = await _walletRepository.GetWalletByUserIdAsync(id);
        if (wallet == null)
        {
            throw new Exception("Wallet not found for the user.");
        }

        return wallet;
    }

    public async Task<decimal> GetBalanceTrxAsync(string tronAddress)
    {
        var response =  await _tronGridClient.GetAccountAsync(tronAddress);
        if(response == null || response.Account == null)
        {
            return 0;
        }

        return response.Account.Balance / 1000000;
    }
}