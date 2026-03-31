using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;


namespace TronWallet.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;


    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }
    public void OnGet()
    {
        ErrorMessage = "";
    }

    public async Task<IActionResult> OnPostAsync()
    {


        // Server-side validation
        if (!ModelState.IsValid)
        {
            return Page(); // Return page with validation errors
        }

        try
        {
            await _authService.Register(Email, Username, Password);
            Password = null;
            return RedirectToPage("/Cabinet/Dashboard");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if(ex.Message == "Email already exists")
                ModelState.AddModelError("Email", ex.Message);
            else if(ex.Message == "User already exists") 
                ModelState.AddModelError("Username", ex.Message);
            else 
                // ErrorMessage = "Unknown error";
                ErrorMessage = ex.Message;
            return Page();
        }
        

    }
}
