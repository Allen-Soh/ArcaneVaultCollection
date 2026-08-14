//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StickerController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    private void LogAudit(string stickerCode, string stickerName, string action)
    {
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        CosmeticAuditLogger.Log(_connectionString, adminName, adminName, stickerCode, stickerName, "Sticker", action);
    }

    private string GenerateUniqueStickerCode(SqliteConnection connection)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            var code = "STK_" + Guid.NewGuid().ToString("N")[..6].ToUpper();
            var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM Stickers WHERE StickerCode = @Code", connection);
            checkCommand.Parameters.AddWithValue("@Code", code);
            if (Convert.ToInt32(checkCommand.ExecuteScalar()) == 0)
                return code;
        }
        throw new InvalidOperationException("Failed to generate a unique sticker code after 5 attempts.");
    }

    private static Sticker ReadSticker(SqliteDataReader reader)
    {
        return new Sticker
        {
            StickerCode = reader["StickerCode"].ToString(),
            StickerName = reader["StickerName"].ToString(),
            ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString(),
            ImagePublicId = reader["ImagePublicId"] == DBNull.Value ? null : reader["ImagePublicId"].ToString(),
            Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
            IsLimited = reader["IsLimited"] != DBNull.Value && Convert.ToBoolean(Convert.ToInt32(reader["IsLimited"])),
            LimitedQuantity = reader["LimitedQuantity"] == DBNull.Value ? null : Convert.ToInt32(reader["LimitedQuantity"]),
            IsDelisted = reader["IsDelisted"] != DBNull.Value && Convert.ToBoolean(Convert.ToInt32(reader["IsDelisted"]))
        };
    }

    // GET: api/Sticker
    [HttpGet]
    public ActionResult<IEnumerable<Sticker>> GetSticker()
    {
        var stickers = new List<Sticker>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT StickerCode, StickerName, ImagePath, ImagePublicId, Price, IsLimited, LimitedQuantity, IsDelisted FROM Stickers",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            stickers.Add(ReadSticker(reader));
        }

        // LINQ — order by StickerName
        var result = stickers.OrderBy(s => s.StickerName).ToList();

        return result;
    }

    // GET: api/Sticker/5
    [HttpGet("{stickercode}")]
    public ActionResult<Sticker> GetSticker(string stickercode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT StickerCode, StickerName, ImagePath, ImagePublicId, Price, IsLimited, LimitedQuantity, IsDelisted FROM Stickers WHERE StickerCode = @StickerCode",
            connection);
        command.Parameters.AddWithValue("@StickerCode", stickercode);

        using var reader = command.ExecuteReader();

        Sticker? sticker = null;
        while (reader.Read())
        {
            sticker = ReadSticker(reader);
        }

        if (sticker == null)
        {
            return NotFound();
        }

        return sticker;
    }

    // PUT: api/Sticker/5
    [Authorize(Roles = "Staff")]
    [HttpPut("{stickercode}")]
    public IActionResult PutSticker(string? stickercode, Sticker sticker)
    {
        if (stickercode != sticker.StickerCode)
        {
            return BadRequest();
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Stickers WHERE StickerCode = @StickerCode",
            connection);
        checkCommand.Parameters.AddWithValue("@StickerCode", stickercode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            @"UPDATE Stickers
              SET StickerName = @StickerName, ImagePath = @ImagePath, ImagePublicId = @ImagePublicId,
                  Price = @Price, IsLimited = @IsLimited, LimitedQuantity = @LimitedQuantity, IsDelisted = @IsDelisted
              WHERE StickerCode = @StickerCode",
            connection);

        command.Parameters.AddWithValue("@StickerCode", sticker.StickerCode);
        command.Parameters.AddWithValue("@StickerName", sticker.StickerName);
        command.Parameters.AddWithValue("@ImagePath", (object?)sticker.ImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@ImagePublicId", (object?)sticker.ImagePublicId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", sticker.Price);
        command.Parameters.AddWithValue("@IsLimited", sticker.IsLimited ? 1 : 0);
        command.Parameters.AddWithValue("@LimitedQuantity", (object?)sticker.LimitedQuantity ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsDelisted", sticker.IsDelisted ? 1 : 0);

        command.ExecuteNonQuery();
        LogAudit(sticker.StickerCode, sticker.StickerName, "Edit");

        return NoContent();
    }

    // POST: api/Sticker
    [Authorize(Roles = "Staff")]
    [HttpPost]
    public ActionResult<Sticker> PostSticker(Sticker sticker)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        sticker.StickerCode = GenerateUniqueStickerCode(connection);

        var command = new SqliteCommand(
            @"INSERT INTO Stickers (StickerCode, StickerName, ImagePath, ImagePublicId, Price, IsLimited, LimitedQuantity, IsDelisted)
              VALUES (@StickerCode, @StickerName, @ImagePath, @ImagePublicId, @Price, @IsLimited, @LimitedQuantity, @IsDelisted)",
            connection);

        command.Parameters.AddWithValue("@StickerCode", sticker.StickerCode);
        command.Parameters.AddWithValue("@StickerName", sticker.StickerName);
        command.Parameters.AddWithValue("@ImagePath", (object?)sticker.ImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@ImagePublicId", (object?)sticker.ImagePublicId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", sticker.Price);
        command.Parameters.AddWithValue("@IsLimited", sticker.IsLimited ? 1 : 0);
        command.Parameters.AddWithValue("@LimitedQuantity", (object?)sticker.LimitedQuantity ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsDelisted", sticker.IsDelisted ? 1 : 0);

        command.ExecuteNonQuery();
        LogAudit(sticker.StickerCode, sticker.StickerName, "Create");

        return CreatedAtAction("GetSticker", new { stickercode = sticker.StickerCode }, sticker);
    }

    // PUT: api/Sticker/5/delist
    [Authorize(Roles = "Staff")]
    [HttpPut("{stickercode}/delist")]
    public IActionResult DelistSticker(string stickercode)
    {
        return SetDelisted(stickercode, true);
    }

    // PUT: api/Sticker/5/relist
    [Authorize(Roles = "Staff")]
    [HttpPut("{stickercode}/relist")]
    public IActionResult RelistSticker(string stickercode)
    {
        return SetDelisted(stickercode, false);
    }

    private IActionResult SetDelisted(string stickercode, bool isDelisted)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var checkCommand = new SqliteCommand(
            "SELECT StickerName FROM Stickers WHERE StickerCode = @StickerCode",
            connection);
        checkCommand.Parameters.AddWithValue("@StickerCode", stickercode);
        var stickerName = checkCommand.ExecuteScalar() as string;

        if (stickerName == null)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            "UPDATE Stickers SET IsDelisted = @IsDelisted WHERE StickerCode = @StickerCode",
            connection);
        command.Parameters.AddWithValue("@StickerCode", stickercode);
        command.Parameters.AddWithValue("@IsDelisted", isDelisted ? 1 : 0);
        command.ExecuteNonQuery();
        LogAudit(stickercode, stickerName, isDelisted ? "Delist" : "Relist");

        return NoContent();
    }

    // DELETE: api/Sticker/5
    [Authorize(Roles = "Staff")]
    [HttpDelete("{stickercode}")]
    public IActionResult DeleteSticker(string? stickercode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Stickers WHERE StickerCode = @StickerCode",
            connection);
        checkCommand.Parameters.AddWithValue("@StickerCode", stickercode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            "DELETE FROM Stickers WHERE StickerCode = @StickerCode",
            connection);
        command.Parameters.AddWithValue("@StickerCode", stickercode);
        command.ExecuteNonQuery();

        return NoContent();
    }
}
