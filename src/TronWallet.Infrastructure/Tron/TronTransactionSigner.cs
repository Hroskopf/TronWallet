using TronNet.Protocol;
using TronWallet.Core.Domain.Entities.Tron;
using TronWallet.Core.Domain.Entities;

using TronNet;
using TronNet.Crypto;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using TronWallet.Core.Interfaces.Services;



namespace TronWallet.Infrastructure.Tron;

public class TronTransactionSigner : ITransactionSigner
{

    private static string BytesToHex(byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        int b;

        for (int i = 0; i < bytes.Length; i++)
        {
            b = bytes[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = bytes[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }

        return new string(c).ToLower();
    }

    private static byte[] HexToBytes(string hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    public static string ToTronSignatureHex(ECDSASignature signature)
    {
        if (signature == null)
            throw new ArgumentNullException(nameof(signature));

        // 1. Convert R and S to unsigned byte arrays
        byte[] rBytes = signature.R.ToByteArrayUnsigned();
        byte[] sBytes = signature.S.ToByteArrayUnsigned();

        // 2. Pad to 32 bytes
        byte[] rPadded = new byte[32];
        byte[] sPadded = new byte[32];

        Array.Copy(rBytes, 0, rPadded, 32 - rBytes.Length, rBytes.Length);
        Array.Copy(sBytes, 0, sPadded, 32 - sBytes.Length, sBytes.Length);

        // 3. Get recovery ID (V)
        if (signature.V == null)
            throw new Exception("Signature does not contain recovery id (V)");

        byte v = signature.V[0];

        // 4. Build 65-byte signature
        byte[] finalSig = new byte[65];
        Array.Copy(rPadded, 0, finalSig, 0, 32);
        Array.Copy(sPadded, 0, finalSig, 32, 32);
        finalSig[64] = v;

        // 5. Convert to hex
        return BytesToHex(finalSig);
    }
    public string Sign(string unsignedTxHex, string privateKeyHex)
    {
        byte[] privateKeyBytes = HexToBytes(privateKeyHex);
    var ecKey = new ECKey(privateKeyBytes, true);

    byte[] txBytes = HexToBytes(unsignedTxHex);

    byte[] txHash = SHA256.HashData(txBytes);

    var signature = ecKey.Sign(txHash);

    return ToTronSignatureHex(signature);

    }
}