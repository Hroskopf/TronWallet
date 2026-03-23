using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;


namespace TronWallet.Web.Pages;

public class IndexModel : PageModel
{
    public bool IsAuthenticated { get; private set;}
    public IActionResult OnGet()
    {
        IsAuthenticated = User.Identity != null && User.Identity.IsAuthenticated;

        if(IsAuthenticated)
        {
            return Redirect("/Cabinet/Dashboard");
        }
        return Page();
    }
}
