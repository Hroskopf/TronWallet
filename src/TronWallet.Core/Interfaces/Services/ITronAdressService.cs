namespace TronWallet.Core.Interfaces.Services;

public interface ITronAdressService
{
    public (string PrivateKeyHex, string PublicKeyHex, string Base58Address) GenerateWallet();
}