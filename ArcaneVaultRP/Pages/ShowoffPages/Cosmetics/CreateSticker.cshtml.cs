//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.ShowoffPages;

public class CreateStickerModel : PageModel
{
    public IActionResult OnGet()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        // ASSUMPTION: ArcaneVaultUser has a RoleId property where 1 == Staff.
        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);
        if (loggedInUser?.RoleId != 1)
            return RedirectToPage("/ShowoffPages/Index", new { tab = "shop" });

        return Page();
    }
}