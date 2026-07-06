using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DetailsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public CollectionItem CollectionItem { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? itemid)
    {

        if (itemid is null)
        {
            return NotFound();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var collectionitem = await httpClient.GetFromJsonAsync<CollectionItem>($"CollectionItems/{itemid}");

        if (collectionitem is null)
        {
            return NotFound();
        }

        CollectionItem = collectionitem;
        return Page();
    }
}