using Dapper;
using System.Data;
using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Domain.Entities;

namespace TronWallet.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DbConnectionFactory _factory;

    public RefreshTokenRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task InsertAsync(RefreshToken token)
    {
        var sql = @"
            INSERT INTO refresh_tokens
            (id, user_id, token_hash, expires_at, revoked_at, created_at, ip_address, user_agent)
            VALUES
            (@Id, @UserId, @TokenHash, @ExpiresAt, @RevokedAt, @CreatedAt, @IpAddress, @UserAgent);
        ";
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(sql, token);
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        var sql = @"
            SELECT *
            FROM refresh_tokens
            WHERE token_hash = @TokenHash
            LIMIT 1;
        ";
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<RefreshToken>(
            sql,
            new { TokenHash = tokenHash }
        );
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        var sql = @"
            UPDATE refresh_tokens
            SET
                expires_at = @ExpiresAt,
                revoked_at = @RevokedAt,
                ip_address = @IpAddress,
                user_agent = @UserAgent
            WHERE id = @Id;
        ";
        using var conn = _factory.CreateConnection();

        await conn.ExecuteAsync(sql, token);
    }

    public async Task RevokeAsync(string tokenHash, DateTime revokedAt)
    {
        var sql = @"
            UPDATE refresh_tokens
            SET revoked_at = @RevokedAt
            WHERE token_hash = @TokenHash AND revoked_at IS NULL;
        ";
        using var conn = _factory.CreateConnection();

        await conn.ExecuteAsync(sql, new
        {
            TokenHash = tokenHash,
            RevokedAt = revokedAt
        });
    }

    public async Task RevokeAllForUserAsync(Guid userId, DateTime revokedAt)
    {
        var sql = @"
            UPDATE refresh_tokens
            SET revoked_at = @RevokedAt
            WHERE user_id = @UserId AND revoked_at IS NULL;
        ";
        using var conn = _factory.CreateConnection();

        await conn.ExecuteAsync(sql, new
        {
            UserId = userId,
            RevokedAt = revokedAt
        });
    }
}