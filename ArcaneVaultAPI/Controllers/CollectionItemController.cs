using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.DataTransferObject;

[Route("api/[controller]")]
[ApiController]
public class CollectionItemController : ControllerBase
{
    private readonly ArcaneDbContext _context;
    public CollectionItemController(ArcaneDbContext context)
    {
        _context = context;
    }

    // GET: api/CollectionItem
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CollectionItem>>> GetCollectionItem()
    {
        return await _context.CollectionItems.ToListAsync();
    }

    // GET: api/CollectionItem/5
    [HttpGet("{itemid}")]
    public async Task<ActionResult<CollectionItem>> GetCollectionItem(int itemid)
    {
        var collectionitem = await _context.CollectionItems.FindAsync(itemid);

        if (collectionitem == null)
        {
            return NotFound();
        }

        return collectionitem;
    }

    // PUT: api/CollectionItem/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{itemid}")]
    public async Task<IActionResult> PutCollectionItem(int? itemid, CollectionItemDTO dto)
    {
        if (itemid != dto.ItemId)
        {
            return BadRequest();
        }

        var collectionItem = new CollectionItem
        {
            ItemId = dto.ItemId,
            ItemName = dto.ItemName,
            IsDeleted = dto.IsDeleted,
            StartingQuantity = dto.StartingQuantity,
            CurrentQuantity = dto.CurrentQuantity,
            UserName = dto.UserName,
            User = null
        };

        _context.Entry(collectionItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CollectionItemExists(itemid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/CollectionItem
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<CollectionItem>> PostCollectionItem(CollectionItemDTO dto)
    {
        var collectionItem = new CollectionItem
        {
            ItemName = dto.ItemName,
            IsDeleted = dto.IsDeleted,
            StartingQuantity = dto.StartingQuantity,
            CurrentQuantity = dto.CurrentQuantity,
            UserName = dto.UserName,
            User = null
        };

        _context.CollectionItems.Add(collectionItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCollectionItem", new { itemid = collectionItem.ItemId }, collectionItem);
    }

    // DELETE: api/CollectionItem/5
    [HttpDelete("{itemid}")]
    public async Task<IActionResult> DeleteCollectionItem(int? itemid)
    {
        var collectionitem = await _context.CollectionItems.FindAsync(itemid);
        if (collectionitem == null)
        {
            return NotFound();
        }

        _context.CollectionItems.Remove(collectionitem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CollectionItemExists(int? itemid)
    {
        return _context.CollectionItems.Any(e => e.ItemId == itemid);
    }
}
