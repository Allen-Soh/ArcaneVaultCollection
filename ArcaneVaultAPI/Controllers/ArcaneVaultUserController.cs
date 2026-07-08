using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.DataTransferObject;

[Route("api/[controller]")]
[ApiController]
public class ArcaneVaultUserController : ControllerBase
{
    private readonly ArcaneDbContext _context;
    public ArcaneVaultUserController(ArcaneDbContext context)
    {
        _context = context;
    }

    // GET: api/ArcaneVaultUser
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArcaneVaultUser>>> GetArcaneVaultUser()
    {
        return await _context.ArcaneVaultUsers.ToListAsync();
    }

    // GET: api/ArcaneVaultUser/5
    [HttpGet("{username}")]
    public async Task<ActionResult<ArcaneVaultUser>> GetArcaneVaultUser(string username)
    {
        var arcanevaultuser = await _context.ArcaneVaultUsers.FindAsync(username);

        if (arcanevaultuser == null)
        {
            return NotFound();
        }

        return arcanevaultuser;
    }

    // PUT: api/ArcaneVaultUser/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{username}")]
    public async Task<IActionResult> PutArcaneVaultUser(string? username, ArcaneVaultUser arcanevaultuser)
    {
        if (username != arcanevaultuser.UserName)
        {
            return BadRequest();
        }

        arcanevaultuser.Role = null;

        _context.Entry(arcanevaultuser).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ArcaneVaultUserExists(username))
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

    // POST: api/ArcaneVaultUser
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<ArcaneVaultUser>> PostArcaneVaultUser(ArcaneVaultUserDTO dto)
    {
        var arcanevaultuser = new ArcaneVaultUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            IsDeleted = dto.IsDeleted,
            RoleId = dto.RoleId,
            Password = dto.Password
        };

        _context.ArcaneVaultUsers.Add(arcanevaultuser);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetArcaneVaultUser", new { username = arcanevaultuser.UserName }, arcanevaultuser);
    }

    // DELETE: api/ArcaneVaultUser/5
    [HttpDelete("{username}")]
    public async Task<IActionResult> DeleteArcaneVaultUser(string? username)
    {
        var arcanevaultuser = await _context.ArcaneVaultUsers.FindAsync(username);
        if (arcanevaultuser == null)
        {
            return NotFound();
        }

        _context.ArcaneVaultUsers.Remove(arcanevaultuser);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ArcaneVaultUserExists(string? username)
    {
        return _context.ArcaneVaultUsers.Any(e => e.UserName == username);
    }
}
