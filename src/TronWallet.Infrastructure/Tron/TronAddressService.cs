using TronWallet.Core.Interfaces.Services;
using TronNet;
using TronNet.Crypto;


namespace TronWallet.Infrastructure.Tron;

public class TronAddressService : ITronAddressService
{

    public TronAddressService()
    {
        
    }
    public (string PrivateKeyHex, string PublicKeyHex, string Base58Address) GenerateWallet()
    {
        var key = TronECKey.GenerateKey(TronNetwork.MainNet);

        var privateKeyHex = key.GetPrivateKey();

        var publicKeyHex = key.GetPubKey().ToHex();

        var address = key.GetPublicAddress();

        return (privateKeyHex, publicKeyHex, address);
    }

    public string Base58ToHex(string base58Address)
    {
        byte[] decoded = Base58Encoder.Decode(base58Address);
        byte[] addressBytes = decoded.Take(21).ToArray();
        string hex = BitConverter.ToString(addressBytes)
                               .Replace("-", "")
                               .ToLower();
        
        return hex;

    }

}