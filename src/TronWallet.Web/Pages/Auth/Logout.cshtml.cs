using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Web.Pages.Auth;

public class LogoutModel : PageModel
{

    private readonly IAuthService _authService;
    public LogoutModel(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _authService.Logout();
        
        return RedirectToPage("/Index");

    }
}
