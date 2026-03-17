using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;

namespace TronWallet.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Password { get; set; }


    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        
        Console.WriteLine($"Trying to POST: {Email}, {Password}");

        return RedirectToPage("/Cabinet/Dashboard");
    }
}
