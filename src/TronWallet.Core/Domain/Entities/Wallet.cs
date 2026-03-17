using System;

namespace TronWallet.Core.Domain.Entities;

class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TronAdress { get; set; }
    public string PrivateKeyEnc { get; set; }
    public string PublicKey { get; set; }
    public string Network { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
}