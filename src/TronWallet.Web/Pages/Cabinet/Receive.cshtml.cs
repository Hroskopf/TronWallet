using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

using TronWallet.Infrastructure.Tron;

namespace TronWallet.Web.Pages.Cabinet;

[Authorize]
public class ReceiveModel : PageModel
{
    public void OnGet()
    {
        
    }
}

