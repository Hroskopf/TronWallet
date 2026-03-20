using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;


namespace TronWallet.Web.Pages;

public class IndexModel : PageModel
{
    public bool IsAuthenticated { get; private set;}
    public void OnGet()
    {
        IsAuthenticated = (User.Identity != null && User.Identity.IsAuthenticated);
    }
}
