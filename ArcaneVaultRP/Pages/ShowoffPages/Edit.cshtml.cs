//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.ShowoffPages;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EditModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Showoff? ShowoffData { get; set; }
    public List<CollectionItemCategory> UserCollectionItems { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

        ShowoffData = await httpClient.GetFromJsonAsync<Showoff>($"Showoff/{id}");

        if (ShowoffData == null)
            return RedirectToPage("./Index", new { tab = "profile" });

        UserCollectionItems = await httpClient
            .GetFromJsonAsync<List<CollectionItemCategory>>("CollectionItem/withcategory")
            ?? new List<CollectionItemCategory>();

        UserCollectionItems = UserCollectionItems
            .Where(c => c.UserName == loggedInUser.UserName && !c.IsDeleted)
            .OrderBy(c => c.ItemName)
            .ToList();

        Categories = await httpClient.GetFromJsonAsync<List<Category>>("Category")
                     ?? new List<Category>();

        return Page();
    }
}
