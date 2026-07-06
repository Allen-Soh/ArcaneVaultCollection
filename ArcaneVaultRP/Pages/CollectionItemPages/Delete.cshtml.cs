using ArcaneVaultRP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVaultRP.Pages.CollectionItemPages;

public class DeleteModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DeleteModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int? itemid)
    {
        if (itemid is null)
        {
            return NotFound();
        }

        HttpClient httpClient = _httpClientFactory.CreateClient("ArcaneVaultAPI");
        var response = await httpClient.DeleteAsync($"CollectionItems/{itemid}");

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("./Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "An error occurred while deleting the collection item.");
            return Page();
        }
    }
}