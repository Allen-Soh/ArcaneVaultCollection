using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcaneVaultAPI.Data;

[Route("api/[controller]")]
[ApiController]
public class CollectionItemCategoryController : ControllerBase
{
    private readonly ArcaneDbContext _context;
    public CollectionItemCategoryController(ArcaneDbContext context)
    {
        _context = context;
    }

    // GET: api/CollectionItemCategory
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CollectionItemCategory>>> GetCollectionItemCategory()
    {
        return await _context.CollectionItemCategories.ToListAsync();
    }

    // GET: api/CollectionItemCategory/5
    [HttpGet("{ItemId}/{CategoryCode}")]
    public async Task<ActionResult<CollectionItemCategory>> GetCollectionItemCategory(string ItemId, string CategoryCode)
    {
        var collectionitemcategory = await _context.CollectionItemCategories.FindAsync(ItemId, CategoryCode);

        if (collectionitemcategory == null)
        {
            return NotFound();
        }

        return collectionitemcategory;
    }

    // POST: api/CollectionItemCategory
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<CollectionItemCategory>> PostCollectionItemCategory(CollectionItemCategory collectionitemcategory)
    {
        _context.CollectionItemCategories.Add(collectionitemcategory);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCollectionItemCategory", new { ItemId = collectionitemcategory.ItemId, CategoryCode = collectionitemcategory.CategoryCode }, collectionitemcategory);
    }

    // DELETE: api/CollectionItemCategory/5
    [HttpDelete("{ItemId}/{CategoryCode}")]
    public async Task<IActionResult> DeleteCollectionItemCategory(string? ItemId, string? CategoryCode)
    {
        var collectionitemcategory = await _context.CollectionItemCategories.FindAsync(ItemId, CategoryCode);
        if (collectionitemcategory == null)
        {
            return NotFound();
        }

        _context.CollectionItemCategories.Remove(collectionitemcategory);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}