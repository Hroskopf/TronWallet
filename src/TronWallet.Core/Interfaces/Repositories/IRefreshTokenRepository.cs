using TronWallet.Core.Domain.Entities;

namespace TronWallet.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task InsertAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task UpdateAsync(RefreshToken token);
    Task RevokeAsync(string tokenHash, DateTime revokedAt);
    Task RevokeAllForUserAsync(Guid userId, DateTime revokedAt);
}