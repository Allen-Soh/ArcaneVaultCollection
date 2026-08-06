using ArcaneVaultAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StickerController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/Sticker
    [HttpGet]
    public ActionResult<IEnumerable<Sticker>> GetSticker()
    {
        var stickers = new List<Sticker>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT StickerCode, StickerName, ImagePath FROM Stickers",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            stickers.Add(new Sticker
            {
                StickerCode = reader["StickerCode"].ToString(),
                StickerName = reader["StickerName"].ToString(),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString()
            });
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
            "SELECT StickerCode, StickerName, ImagePath FROM Stickers WHERE StickerCode = @StickerCode",
            connection);
        command.Parameters.AddWithValue("@StickerCode", stickercode);

        using var reader = command.ExecuteReader();

        Sticker? sticker = null;
        while (reader.Read())
        {
            sticker = new Sticker
            {
                StickerCode = reader["StickerCode"].ToString(),
                StickerName = reader["StickerName"].ToString(),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString()
            };
        }

        if (sticker == null)
        {
            return NotFound();
        }

        return sticker;
    }

    // PUT: api/Sticker/5
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
              SET StickerName = @StickerName, ImagePath = @ImagePath 
              WHERE StickerCode = @StickerCode",
            connection);

        command.Parameters.AddWithValue("@StickerCode", sticker.StickerCode);
        command.Parameters.AddWithValue("@StickerName", sticker.StickerName);
        command.Parameters.AddWithValue("@ImagePath", (object?)sticker.ImagePath ?? DBNull.Value);

        command.ExecuteNonQuery();

        return NoContent();
    }

    // POST: api/Sticker
    [HttpPost]
    public ActionResult<Sticker> PostSticker(Sticker sticker)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "INSERT INTO Stickers (StickerCode, StickerName, ImagePath) VALUES (@StickerCode, @StickerName, @ImagePath)",
            connection);

        command.Parameters.AddWithValue("@StickerCode", sticker.StickerCode);
        command.Parameters.AddWithValue("@StickerName", sticker.StickerName);
        command.Parameters.AddWithValue("@ImagePath", (object?)sticker.ImagePath ?? DBNull.Value);

        command.ExecuteNonQuery();

        return CreatedAtAction("GetSticker", new { stickercode = sticker.StickerCode }, sticker);
    }

    // DELETE: api/Sticker/5
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