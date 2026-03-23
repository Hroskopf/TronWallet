using TronNet.Protocol;
using TronWallet.Core.Domain.Entities.Tron;
using TronWallet.Core.Domain.Entities;

using TronNet;
using Microsoft.Extensions.Options;


namespace TronWallet.Infrastructure.Tron;

class TronTransactionSigner
{
    // private readonly ITransactionClient _transactionClient;
    // private readonly IOptions<TronNetOptions> _options;
    public WalletTransaction Sign(TronUnsignedTx unsignedTx, string privateKeyHex)
    {
        // var ecKey = new TronECKey(privateKeyHex, _options.Value.Network);
        // var transactionExtension = await _transactionClient.CreateTransactionAsync(from, to, amount);

        // var transactionSigned = _transactionClient.GetTransactionSign(transactionExtension.Transaction, privateKey);
        
        // var result = await _transactionClient.BroadcastTransactionAsync(transactionSigned);
        return new WalletTransaction();

    }
}