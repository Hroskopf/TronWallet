using TronWallet.Core.Interfaces.Services;
using TronNet;
using TronNet.Crypto;
using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace TronWallet.Infrastructure.Tron;

public class TronAddressService : ITronAddressService
{
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    private readonly TronNetwork _network;

    public TronAddressService(TronNetworkConfig config)
    {
        _network = config.Network;
    }

    // =========================
    // WALLET GENERATION
    // =========================
    public (string PrivateKeyHex, string PublicKeyHex, string Base58Address) GenerateWallet()
    {
        var key = TronECKey.GenerateKey(TronNetwork.MainNet);

        var privateKeyHex = key.GetPrivateKey();

        var publicKeyBytes = key.GetPubKey();
        var publicKeyHex = publicKeyBytes.ToHex();

        var address = key.GetPublicAddress();

        return (privateKeyHex, publicKeyHex, address);
    }
    // =========================
    // HEX -> BASE58
    // =========================
    public static string HexToBase58(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex is null or empty");

        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex[2..];

        var bytes = Convert.FromHexString(hex);

        ValidateTronAddressBytes(bytes);

        return Base58CheckEncode(bytes);
    }

    // =========================
    // BASE58 -> HEX
    // =========================
    public string Base58ToHex(string base58Address)
    {
        if (string.IsNullOrWhiteSpace(base58Address))
            throw new ArgumentException("Base58 address is null or empty");

        var decoded = Base58Decode(base58Address);

        if (decoded.Length != 25)
            throw new ArgumentException("Invalid TRON address length");

        var addressBytes = decoded.Take(21).ToArray();
        var checksum = decoded.Skip(21).ToArray();

        var hash = DoubleSha256(addressBytes).Take(4).ToArray();

        if (!checksum.SequenceEqual(hash))
            throw new ArgumentException("Invalid checksum");

        if (addressBytes[0] != 0x41)
            throw new ArgumentException("Invalid TRON prefix");

        return Convert.ToHexString(addressBytes).ToLower();
    }

    // =========================
    // BASE58CHECK ENCODE
    // =========================
    private static string Base58CheckEncode(byte[] data)
    {
        var checksum = DoubleSha256(data).Take(4).ToArray();
        var full = data.Concat(checksum).ToArray();

        return Base58Encode(full);
    }

    // =========================
    // BASE58 ENCODE
    // =========================
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

        foreach (var b in bytes)
        {
            if (b == 0x00) sb.Insert(0, '1');
            else break;
        }

        return sb.ToString();
    }

    // =========================
    // BASE58 DECODE
    // =========================
    private static byte[] Base58Decode(string base58)
    {
        BigInteger intData = 0;

        for (int i = 0; i < base58.Length; i++)
        {
            int digit = Alphabet.IndexOf(base58[i]);
            if (digit < 0)
                throw new FormatException($"Invalid Base58 character: {base58[i]}");

            intData = intData * 58 + digit;
        }

        var bytes = intData.ToByteArray(isUnsigned: true, isBigEndian: true);

        int leadingZeros = base58.TakeWhile(c => c == '1').Count();

        return new byte[leadingZeros].Concat(bytes).ToArray();
    }

    // =========================
    // HELPERS
    // =========================
    private static byte[] DoubleSha256(byte[] data)
    {
        using var sha = SHA256.Create();
        var first = sha.ComputeHash(data);
        return sha.ComputeHash(first);
    }

    private static void ValidateTronAddressBytes(byte[] bytes)
    {
        if (bytes.Length != 21)
            throw new ArgumentException("TRON address must be 21 bytes");

        if (bytes[0] != 0x41)
            throw new ArgumentException("Invalid TRON prefix (expected 0x41)");
    }
}
