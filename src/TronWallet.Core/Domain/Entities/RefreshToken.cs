using System;

namespace TronWallet.Core.Domain.Entities;

class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? IpAdress { get; set; }
    public string? UserAgent { get; set; }
}