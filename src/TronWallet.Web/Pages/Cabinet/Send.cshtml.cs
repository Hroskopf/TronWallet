using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;


namespace MyApp.Namespace
{
    [Authorize]
    public class SendModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
