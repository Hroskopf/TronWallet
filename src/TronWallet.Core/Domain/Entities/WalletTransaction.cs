using System;
using System.Numerics;
using TronWallet.Core.Domain.Enums;

namespace TronWallet.Core.Domain.Entities;
public class WalletTransaction
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string TxHash { get; set; }
    public string Direction  { get; set; } = "OUT";
    public string FromAddress { get; set; }
    public string ToAddress { get; set; }
    public long AmountSun { get; set; }
    public long FeeSun { get; set; } = 0;
    public string Status { get; set; } = "PENDING";
    public long BlockNumber { get; set; }
    public DateTime BlockTime { get; set; }
    public string RawData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ConfirmedAt { get; set; }

    public decimal GetAmountTrx()
    {
        return (decimal)AmountSun / 1_000_000m;
    }

}