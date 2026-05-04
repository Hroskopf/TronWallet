using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.RegularExpressions;

namespace TronWallet.Web.Pages.Auth;

[EnableRateLimiting("login-limit")]
public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Enter the email")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(254, ErrorMessage = "Email is too long")]
    public string? Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the password")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be 6–100 characters")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // Normalize input
        Email = Email?.Trim().ToLowerInvariant();
        Password = Password?.Trim();

        ValidateInput();

        if (!ModelState.IsValid)
            return Page();

        var user = await _authService.Login(Email!, Password!);

        Password = null;

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return Page();
        }

        return RedirectToPage("/Cabinet/Dashboard");
    }

    private void ValidateInput()
    {
        if (Email != null &&
            !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ModelState.AddModelError(nameof(Email), "Invalid email format");
        }

        if (Password != null && Password.Any(char.IsControl))
        {
            ModelState.AddModelError(nameof(Password), "Password contains invalid characters");
        }
    }
}