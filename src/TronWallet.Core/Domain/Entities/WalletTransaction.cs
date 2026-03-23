using System;
using System.Numerics;
using TronWallet.Core.Domain.Enums;

namespace TronWallet.Core.Domain.Entities;
public class WalletTransaction
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string TxHash { get; set; }
    public TransactionDirection Direction  { get; set; }
    public string FromAdress { get; set; }
    public string ToAdress { get; set; }
    public BigInteger AmountSun { get; set; }
    public BigInteger FeeSun { get; set; }
    public TransactionStatus Status { get; set; }
    public BigInteger BlockNumber { get; set; }
    public DateTime BlockTime { get; set; }
    public string RawData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ConfirmedAt { get; set; }

}