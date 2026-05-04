using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TronWallet.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(254, ErrorMessage = "Email is too long")]
    public string? Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Username is required")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be 3–30 characters")]
    public string? Username { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be 6–100 characters")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        Email = Email?.Trim().ToLowerInvariant();
        Username = Username?.Trim();

        ValidateInput();

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _authService.Register(Email!, Username!, Password!);

            Password = null;

            return RedirectToPage("/Cabinet/Dashboard");
        }
        catch (Exception ex)
        {
            if (ex.Message == "Email already exists")
                ModelState.AddModelError(nameof(Email), "Email already exists");
            else if (ex.Message == "User already exists")
                ModelState.AddModelError(nameof(Username), "Username already exists");
            else
                ModelState.AddModelError(string.Empty, "Registration failed");

            return Page();
        }
    }

    private void ValidateInput()
    {
        // Email sanity check (optional)
        if (Email != null &&
            !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ModelState.AddModelError(nameof(Email), "Invalid email format");
        }

        // Username rules (IMPORTANT)
        if (Username != null &&
            !Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$"))
        {
            ModelState.AddModelError(nameof(Username),
                "Username can contain only letters, numbers, and underscore");
        }

        // Prevent weird password input
        if (Password != null && Password.Any(char.IsControl))
        {
            ModelState.AddModelError(nameof(Password),
                "Password contains invalid characters");
        }
    }
}