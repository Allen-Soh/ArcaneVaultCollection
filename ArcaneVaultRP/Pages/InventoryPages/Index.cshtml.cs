//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.InventoryPages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<InventoryItem> Items { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        if (userJson == null)
            return RedirectToPage("/Login");

        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        Items = await httpClient.GetFromJsonAsync<List<InventoryItem>>($"CosmeticInventory/user/{loggedInUser.UserName}")
                ?? new List<InventoryItem>();

        return Page();
    }
}
