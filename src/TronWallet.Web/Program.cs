using TronWallet.Core.Interfaces.Repositories;
using TronWallet.Core.Services;
using TronWallet.Infrastructure.Persistence.Repositories;
using TronWallet.Core.Interfaces.Services;
using TronWallet.Infrastructure.Tron;
using TronWallet.Infrastructure.Security;
using TronWallet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using TronNet;


Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<TronTransactionSigner>();

builder.Services.AddHostedService<TransactionSyncService>();


builder.Services.AddTronNet(options =>
{
    options.Network = TronNetwork.MainNet;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("TronGrid", client =>
{
    client.BaseAddress = new Uri("https://api.shasta.trongrid.io/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<DbConnectionFactory>(sp =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>()
                             .GetConnectionString("DefaultConnection")
                             ?? throw new InvalidOperationException("DefaultConnection string is missing");

    return new DbConnectionFactory(connectionString);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();


builder.Services.AddScoped<ITronAdressService, TronAdressService>();
builder.Services.AddSingleton<IEncryptionService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var key = config["Encryption:Key"];
    if (string.IsNullOrWhiteSpace(key))
        throw new Exception("Encryption key is missing");

    return new AesEncryptionService(key);
});


builder.Services.AddScoped<ITronGridClient, TronGridClient>();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout"; 
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
