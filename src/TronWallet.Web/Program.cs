using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Services;
using TronWallet.Infrastructure.Persistence.Repositories;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Infrastructure.Tron;
using TronWallet.Infrastructure.Security;
using TronWallet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using TronNet;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// ======================
// CORE SERVICES
// ======================
builder.Services.AddRazorPages();
builder.Services.AddHostedService<TransactionSyncService>();

// ======================
// TRON CONFIGURATION
// ======================
builder.Services.AddTronNet(options =>
{
    options.Network = TronNetwork.MainNet;
});

builder.Services.AddHttpContextAccessor();

var tronConfig = builder.Configuration.GetSection("TronGrid");
builder.Services.AddHttpClient("TronGrid", client =>
{
    client.BaseAddress = new Uri(tronConfig["BaseUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("TRON-PRO-API-KEY", tronConfig["ApiKey"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ======================
// DATABASE
// ======================
builder.Services.AddSingleton<DbConnectionFactory>(sp =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>()
        .GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection string is missing");

    return new DbConnectionFactory(connectionString);
});

// ======================
// REPOSITORIES
// ======================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// ======================
// SERVICES
// ======================
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITronAddressService, TronAddressService>();
builder.Services.AddScoped<ITransactionSigner, TronTransactionSigner>();
builder.Services.AddScoped<ITronGridClient, TronGridClient>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ======================
// SECURITY
// ======================
builder.Services.AddSingleton<IEncryptionService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var key = config["Encryption:Key"];

    if (string.IsNullOrWhiteSpace(key))
        throw new Exception("Encryption key is missing");

    return new AesEncryptionService(key);
});

// ======================
// AUTHENTICATION
// ======================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

// ======================
// RATE LIMITING (LOGIN PROTECTION)
// ======================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("login-limit", opt =>
    {
        opt.PermitLimit = 5; // max 5 attempts
        opt.Window = TimeSpan.FromMinutes(1); // per 1 minute
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });
});

// ======================
// BUILD APP
// ======================
var app = builder.Build();

// ======================
// PIPELINE
// ======================
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: rate limiter must be before auth
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// ======================
// ENDPOINTS
// ======================
app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();