//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.ShowoffPages;

public class EditStickerModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditStickerModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Sticker? StickerData { get; set; }

    public async Task<IActionResult> OnGetAsync(string code)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);
        if (loggedInUser?.RoleId != 1)
            return RedirectToPage("/ShowoffPages/Index", new { tab = "shop" });

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        StickerData = await httpClient.GetFromJsonAsync<Sticker>($"Sticker/{code}");

        if (StickerData == null)
            return RedirectToPage("./ManageShop");

        return Page();
    }
}
