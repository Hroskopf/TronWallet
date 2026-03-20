using TronWallet.Core.Interfaces.Services;
using System;
using System.Text;
using System.Security.Cryptography;


namespace TronWallet.Infrastructure.Security;

public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key; // 32 байти, Base64 з конфігу

    // Формат: Base64( nonce[12] | ciphertext | tag[16] )

    public AesEncryptionService(string base64Key)
    {
        _key = Convert.FromBase64String(base64Key);
        if (_key.Length != 32)
            throw new ArgumentException("Key must be 32 bytes.");
    }

    public string Encrypt(string plaintext)
    {
        var nonce      = new byte[12];
        var plaintextB = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextB.Length];
        var tag        = new byte[16];

        RandomNumberGenerator.Fill(nonce);
        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plaintextB, ciphertext, tag);

        var result = new byte[12 + ciphertext.Length + 16];
        nonce.CopyTo(result, 0);
        ciphertext.CopyTo(result, 12);
        tag.CopyTo(result, 12 + ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encrypted)
    {
        var data       = Convert.FromBase64String(encrypted);
        var nonce      = data[..12];
        var tag        = data[^16..];
        var ciphertext = data[12..^16];
        var plaintext  = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}