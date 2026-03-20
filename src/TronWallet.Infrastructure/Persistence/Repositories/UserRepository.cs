using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Domain.Entities;
using TronWallet.Infrastructure.Persistence;
using Dapper;

namespace TronWallet.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _factory;

    public UserRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var conn = _factory.CreateConnection();
        var sql = "SELECT EXISTS (SELECT 1 FROM users WHERE email = @Email)";
        Console.WriteLine($"EMAIL SEARCH: {email}");
        return await conn.ExecuteScalarAsync<bool>(sql, new { Email = email });
    }
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        using var conn = _factory.CreateConnection();
        var sql = "SELECT EXISTS (SELECT 1 FROM users WHERE username = @Username)";
        Console.WriteLine($"USERNAME SEARCH: {username}");
        return await conn.ExecuteScalarAsync<bool>(sql, new { Username = username });
    }

    public async Task InsertAsync(User user)
    {
        using var conn = _factory.CreateConnection();
        var sql = "INSERT INTO users (username, email, password_hash) VALUES (@Username, @Email, @PasswordHash)";
        Console.WriteLine($"USER INSERT: {user}");
        await conn.ExecuteAsync(sql, user);
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        using var conn = _factory.CreateConnection(); 
        const string sql = "SELECT * FROM users WHERE email = @Email LIMIT 1";

        var user = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });

        return user;

    }
}