using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Web.Pages.Auth;

public class LogoutModel : PageModel
{

    private readonly ILogger<LogoutModel> _logger;
    private readonly IAuthService _authService;
    public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _authService.Logout();
        
        return RedirectToPage("/Auth/Login");

    }
}
