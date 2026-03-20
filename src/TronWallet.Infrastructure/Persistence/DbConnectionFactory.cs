namespace TronWallet.Infrastructure.Persistence;

using System.Data;
using Npgsql;
using Microsoft.Extensions.Configuration;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

}