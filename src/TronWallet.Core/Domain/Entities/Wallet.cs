using System;

namespace TronWallet.Core.Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TronAddress { get; set; }
    public string PrivateKeyEnc { get; set; }
    public string PublicKey { get; set; }
    public string Network { get; set; } = "shasta";
    public bool IsPrimary { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}