using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.RateLimiting;

namespace TronWallet.Web.Pages.Auth;

[EnableRateLimiting("login-limit")]
public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    public string? ErrorMessage { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the email")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the password")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _authService.Login(Email, Password);

        Password = null;

        if (user == null)
        {
            ErrorMessage = "Invalid email or password";
            return Page();
        }

        return RedirectToPage("/Cabinet/Dashboard");
    }
}