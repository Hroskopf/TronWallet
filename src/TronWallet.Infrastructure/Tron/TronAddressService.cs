using TronWallet.Core.Interfaces.Services;
using TronNet;
using TronNet.Crypto;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;

namespace TronWallet.Infrastructure.Tron;

public class TronAddressService : ITronAddressService
{
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    public TronAddressService() { }


    public (string PrivateKeyHex, string PublicKeyHex, string Base58Address) GenerateWallet()
    {
        var key = TronECKey.GenerateKey(TronNetwork.MainNet);

        var privateKeyHex = key.GetPrivateKey();
        var publicKeyHex = key.GetPubKey().ToHex();
        var address = key.GetPublicAddress(); // Already Base58 encoded

        return (privateKeyHex, publicKeyHex, address);
    }


    public string Base58ToHex(string base58Address)
    {
        if (string.IsNullOrWhiteSpace(base58Address))
            throw new ArgumentException("Base58 address is null or empty");

        byte[] decoded = Base58Decode(base58Address);
        if (decoded.Length < 4)
            throw new ArgumentException("Invalid Base58 address length");

        // Remove last 4 checksum bytes
        var addressBytes = decoded.Take(decoded.Length - 4).ToArray();

        string hex = BitConverter.ToString(addressBytes)
                               .Replace("-", "")
                               .ToLower();

        return hex;
    }

    public static string HexToBase58(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex string is null or empty");

        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex[2..];

        var bytes = Convert.FromHexString(hex);

        // Double SHA256 for checksum
        using var sha256 = SHA256.Create();
        var hash0 = sha256.ComputeHash(bytes);
        var hash1 = sha256.ComputeHash(hash0);
        var checksum = hash1.Take(4).ToArray();

        var addressBytes = bytes.Concat(checksum).ToArray();

        return Base58Encode(addressBytes);
    }

    private static byte[] Base58Decode(string base58)
    {
        BigInteger intData = 0;
        for (int i = 0; i < base58.Length; i++)
        {
            int digit = Alphabet.IndexOf(base58[i]);
            if (digit < 0) throw new FormatException($"Invalid Base58 character `{base58[i]}` at position {i}");
            intData = intData * 58 + digit;
        }

        // Convert BigInteger to byte array (big-endian)
        var bytes = intData.ToByteArray(isUnsigned: true, isBigEndian: true);

        // Add leading zeros
        int leadingZeros = base58.TakeWhile(c => c == '1').Count();
        var result = new byte[leadingZeros + bytes.Length];
        Array.Copy(bytes, 0, result, leadingZeros, bytes.Length);

        return result;
    }


    private static string Base58Encode(byte[] bytes)
    {
        BigInteger intData = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        var sb = new StringBuilder();

        while (intData > 0)
        {
            int remainder = (int)(intData % 58);
            intData /= 58;
            sb.Insert(0, Alphabet[remainder]);
        }

        // Add leading '1's for leading zeros
        foreach (var b in bytes)
        {
            if (b == 0x00) sb.Insert(0, '1');
            else break;
        }

        return sb.ToString();
    }
}