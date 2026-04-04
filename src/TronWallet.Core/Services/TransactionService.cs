using TronWallet.Core.Interfaces.Services;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITronGridClient _tronGridClient;
    private readonly IEncryptionService _encryptionService;
    private readonly ITronAddressService _tronAdressService;
    private readonly ITransactionSigner _transactionSigner;
    private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITronGridClient tronGridClient, IWalletRepository walletRepository, IEncryptionService encryptionService, ITronAddressService tronAdressService, ITransactionSigner transactionSigner, ITransactionRepository transactionRepository)
    {
        _tronGridClient = tronGridClient;
        _walletRepository = walletRepository;
        _encryptionService = encryptionService;
        _tronAdressService = tronAdressService;
        _transactionSigner = transactionSigner;
        _transactionRepository = transactionRepository;
    }

    public async Task<List<WalletTransaction>> GetWalletsTransactionsAsync(Guid walletId)
    {
        return await _transactionRepository.GetWalletsTransactionsAsync(walletId);
    }

    public async Task SendTransactionAsync(Guid fromUserId, string toAddress, decimal amountTrx)
    {
        var wallet = await _walletRepository.GetWalletByUserIdAsync(fromUserId);

        if (wallet == null)
        {
            throw new Exception("Cannot find the wallet");
        }
        var balanceTRX = (await _tronGridClient.GetAccountAsync(wallet.TronAddress))?.Account?.GetBalanceInTRX() ?? 0m;            

        if(balanceTRX < amountTrx)
        {
            throw new Exception("Not enough TRX on the balance");
        }


        var privateKeyHex = _encryptionService.Decrypt(wallet.PrivateKeyEnc);

        var toAddressHex = _tronAdressService.Base58ToHex(toAddress);
        var fromAddressHex = _tronAdressService.Base58ToHex(wallet.TronAddress);

        if(toAddressHex == fromAddressHex)
        {
            throw new Exception("You cannot send to yourself");
        }

        long amountSun = (long)(amountTrx * 1000000);

        var unsignedTx = await _tronGridClient.CreateTransactionAsync(fromAddressHex, toAddressHex, amountSun);

        var txSign = _transactionSigner.Sign(unsignedTx.RawDataHex, privateKeyHex);
        privateKeyHex = null;
        var broadcastResponse = await _tronGridClient.BroadcastTransactionAsync(new
        {
            txID = unsignedTx.TxID,
            raw_data = unsignedTx.RawData,
            raw_data_hex = unsignedTx.RawDataHex,
            signature = new List<string> { txSign }
        });
        if(!broadcastResponse.Result)
        {
            throw new Exception("Oops something went wrong");
        }

        var txHash = unsignedTx.GetTxHash();
        
        await _transactionRepository.InsertAsync(new WalletTransaction
        {
            WalletId = wallet.Id,
            TxHash = txHash,
            FromAddress = wallet.TronAddress,
            ToAddress = toAddress,
            AmountSun = amountSun,
            RawData = unsignedTx.RawDataStr
        });

    }
}