//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcaneVaultAPI.Data;

[Route("api/[controller]")]
[ApiController]
public class ArcaneVaultUserRolesController : ControllerBase
{
    private readonly ArcaneDbContext _context;
    public ArcaneVaultUserRolesController(ArcaneDbContext context)
    {
        _context = context;
    }

    // GET: api/ArcaneVaultUserRoles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArcaneVaultUserRoles>>> GetArcaneVaultUserRoles()
    {
        return await _context.ArcaneVaultUserRoles.ToListAsync();
    }

    // GET: api/ArcaneVaultUserRoles/5
    [HttpGet("{roleid}")]
    public async Task<ActionResult<ArcaneVaultUserRoles>> GetArcaneVaultUserRoles(int roleid)
    {
        var arcanevaultuserroles = await _context.ArcaneVaultUserRoles.FindAsync(roleid);

        if (arcanevaultuserroles == null)
        {
            return NotFound();
        }

        return arcanevaultuserroles;
    }

    // PUT: api/ArcaneVaultUserRoles/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{roleid}")]
    public async Task<IActionResult> PutArcaneVaultUserRoles(int? roleid, ArcaneVaultUserRoles arcanevaultuserroles)
    {
        if (roleid != arcanevaultuserroles.RoleId)
        {
            return BadRequest();
        }

        _context.Entry(arcanevaultuserroles).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ArcaneVaultUserRolesExists(roleid))
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

    // POST: api/ArcaneVaultUserRoles
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<ArcaneVaultUserRoles>> PostArcaneVaultUserRoles(ArcaneVaultUserRoles arcanevaultuserroles)
    {
        _context.ArcaneVaultUserRoles.Add(arcanevaultuserroles);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetArcaneVaultUserRoles", new { roleid = arcanevaultuserroles.RoleId }, arcanevaultuserroles);
    }

    // DELETE: api/ArcaneVaultUserRoles/5
    [HttpDelete("{roleid}")]
    public async Task<IActionResult> DeleteArcaneVaultUserRoles(int? roleid)
    {
        var arcanevaultuserroles = await _context.ArcaneVaultUserRoles.FindAsync(roleid);
        if (arcanevaultuserroles == null)
        {
            return NotFound();
        }

        _context.ArcaneVaultUserRoles.Remove(arcanevaultuserroles);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ArcaneVaultUserRolesExists(int? roleid)
    {
        return _context.ArcaneVaultUserRoles.Any(e => e.RoleId == roleid);
    }
}
