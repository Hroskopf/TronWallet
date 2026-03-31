namespace TronWallet.Core.Interfaces.Services;

public interface IEncryptionService
{
    public string Encrypt(string strHex);
    public string Decrypt(string strEnc);
}