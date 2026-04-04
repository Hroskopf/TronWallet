namespace TronWallet.Core.Interfaces.Services;

public interface ITransactionSigner
{
    string Sign(string unsignedTxHex, string privateKeyHex);
}