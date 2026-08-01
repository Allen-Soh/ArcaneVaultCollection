using ArcaneVaultRP.Helpers;
using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace ArcaneVaultRP.Pages.ShowoffPages;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<CollectionItemCategory> UserCollectionItems { get; set; } = new();
    public List<Category> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.Get(SessionKeys.LoggedInUser) == null)
            return RedirectToPage("/Login");

        var userJson = HttpContext.Session.GetString(SessionKeys.LoggedInUser);
        var loggedInUser = JsonSerializer.Deserialize<ArcaneVaultUser>(userJson);

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var token = HttpContext.Session.GetString(SessionKeys.JwtToken);
        HttpClientHelper.AttachJwtToken(httpClient, token);

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
