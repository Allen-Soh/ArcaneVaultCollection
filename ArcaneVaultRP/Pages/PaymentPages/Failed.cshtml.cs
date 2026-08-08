using ArcaneVaultRP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.PaymentPages;

public class FailedModel : PageModel
{
    public IActionResult OnGet()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        return Page();
    }
}
