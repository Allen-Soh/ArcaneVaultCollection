using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcaneVaultAPI.Data;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ArcaneDbContext _context;
    public CategoryController(ArcaneDbContext context)
    {
        _context = context;
    }

    // GET: api/Category
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
    {
        return await _context.Categories.ToListAsync();
    }

    // GET: api/Category/5
    [HttpGet("{categorycode}")]
    public async Task<ActionResult<Category>> GetCategory(string categorycode)
    {
        var category = await _context.Categories.FindAsync(categorycode);

        if (category == null)
        {
            return NotFound();
        }

        return category;
    }

    // PUT: api/Category/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{categorycode}")]
    public async Task<IActionResult> PutCategory(string? categorycode, Category category)
    {
        if (categorycode != category.CategoryCode)
        {
            return BadRequest();
        }

        _context.Entry(category).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryExists(categorycode))
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

    // POST: api/Category
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Category>> PostCategory(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCategory", new { categorycode = category.CategoryCode }, category);
    }

    // DELETE: api/Category/5
    [HttpDelete("{categorycode}")]
    public async Task<IActionResult> DeleteCategory(string? categorycode)
    {
        var category = await _context.Categories.FindAsync(categorycode);
        if (category == null)
        {
            return NotFound();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryExists(string? categorycode)
    {
        return _context.Categories.Any(e => e.CategoryCode == categorycode);
    }
}
