namespace TronWallet.Core.Interfaces.Services;

public interface ITronAddressService
{
    public (string PrivateKeyHex, string PublicKeyHex, string Base58Address) GenerateWallet();
    public string Base58ToHex(string base58Address);
}