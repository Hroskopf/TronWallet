using TronWallet.Core.Interfaces.Services;
using TronNet;

namespace TronWallet.Infrastructure.Tron;

public class TronAdressService : ITronAdressService
{

    public TronAdressService()
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
}