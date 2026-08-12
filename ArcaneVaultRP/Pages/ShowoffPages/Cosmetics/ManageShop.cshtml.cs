//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.ShowoffPages;

public class ManageShopModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ManageShopModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<Frame> AllFrames { get; set; } = new();
    public List<Sticker> AllStickers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
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

        // Unfiltered — Manage Shop shows delisted items too, unlike the public Shop tab.
        AllFrames = (await httpClient.GetFromJsonAsync<List<Frame>>("Frame") ?? new List<Frame>())
            .OrderBy(f => f.FrameName)
            .ToList();

        AllStickers = (await httpClient.GetFromJsonAsync<List<Sticker>>("Sticker") ?? new List<Sticker>())
            .OrderBy(s => s.StickerName)
            .ToList();

        return Page();
    }
}
