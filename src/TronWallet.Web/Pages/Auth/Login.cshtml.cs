using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TronWallet.Core.Interfaces.Services;
using System.ComponentModel.DataAnnotations;

namespace TronWallet.Web.Pages.Auth;
public class LoginModel : PageModel
{


    private readonly IAuthService _authService;

    public string? ErrorMessage { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the email")]
    public string? Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the password")]
    public string? Password { get; set; }

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {

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

