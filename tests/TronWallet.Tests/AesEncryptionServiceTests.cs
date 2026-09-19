using System.Security.Cryptography;
using TronWallet.Infrastructure.Security;

namespace TronWallet.Tests;

public class AesEncryptionServiceTests
{
    private static string NewKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    [Fact]
    public void Encrypt_Then_Decrypt_ReturnsOriginal()
    {
        var service = new AesEncryptionService(NewKey());
        const string privateKeyHex = "af3b1c9d5e7f00112233445566778899aabbccddeeff00112233445566778899";

        var decrypted = service.Decrypt(service.Encrypt(privateKeyHex));

        Assert.Equal(privateKeyHex, decrypted);
    }

    [Fact]
    public void Encrypt_SamePlaintextTwice_ProducesDifferentCiphertexts()
    {
        var service = new AesEncryptionService(NewKey());

        var first = service.Encrypt("deadbeef");
        var second = service.Encrypt("deadbeef");

        Assert.NotEqual(first, second); // random nonce per encryption
    }

    [Theory]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(64)]
    public void Constructor_RejectsKeys_NotExactly32Bytes(int keyLength)
    {
        var badKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(keyLength));

        Assert.Throws<ArgumentException>(() => new AesEncryptionService(badKey));
    }

    [Fact]
    public void Decrypt_WithDifferentKey_Throws()
    {
        var ciphertext = new AesEncryptionService(NewKey()).Encrypt("deadbeef");
        var otherService = new AesEncryptionService(NewKey());

        Assert.ThrowsAny<CryptographicException>(() => otherService.Decrypt(ciphertext));
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_Throws()
    {
        var service = new AesEncryptionService(NewKey());
        var bytes = Convert.FromBase64String(service.Encrypt("deadbeef"));
        bytes[^1] ^= 0xFF; // flip bits in the GCM tag
        var tampered = Convert.ToBase64String(bytes);

        Assert.ThrowsAny<CryptographicException>(() => service.Decrypt(tampered));
    }
}
