using ArcaneVaultAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FrameController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    private void LogAudit(string frameCode, string frameName, string action)
    {
        var adminName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        CosmeticAuditLogger.Log(_connectionString, adminName, adminName, frameCode, frameName, "Frame", action);
    }

    private string GenerateUniqueFrameCode(SqliteConnection connection)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            var code = "FRM_" + Guid.NewGuid().ToString("N")[..6].ToUpper();
            var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM Frames WHERE FrameCode = @Code", connection);
            checkCommand.Parameters.AddWithValue("@Code", code);
            if (Convert.ToInt32(checkCommand.ExecuteScalar()) == 0)
                return code;
        }
        throw new InvalidOperationException("Failed to generate a unique frame code after 5 attempts.");
    }

    private static Frame ReadFrame(SqliteDataReader reader)
    {
        return new Frame
        {
            FrameCode = reader["FrameCode"].ToString(),
            FrameName = reader["FrameName"].ToString(),
            ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString(),
            ImagePublicId = reader["ImagePublicId"] == DBNull.Value ? null : reader["ImagePublicId"].ToString(),
            Orientation = reader["Orientation"] == DBNull.Value ? "Landscape" : reader["Orientation"].ToString(),
            Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
            IsLimited = reader["IsLimited"] != DBNull.Value && Convert.ToBoolean(Convert.ToInt32(reader["IsLimited"])),
            LimitedQuantity = reader["LimitedQuantity"] == DBNull.Value ? null : Convert.ToInt32(reader["LimitedQuantity"]),
            IsDelisted = reader["IsDelisted"] != DBNull.Value && Convert.ToBoolean(Convert.ToInt32(reader["IsDelisted"]))
        };
    }

    // GET: api/Frame
    [HttpGet]
    public ActionResult<IEnumerable<Frame>> GetFrame()
    {
        var frames = new List<Frame>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT FrameCode, FrameName, ImagePath, ImagePublicId, Orientation, Price, IsLimited, LimitedQuantity, IsDelisted FROM Frames",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            frames.Add(ReadFrame(reader));
        }

        // LINQ — order by FrameName
        var result = frames.OrderBy(f => f.FrameName).ToList();

        return result;
    }

    // GET: api/Frame/5
    [HttpGet("{framecode}")]
    public ActionResult<Frame> GetFrame(string framecode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT FrameCode, FrameName, ImagePath, ImagePublicId, Orientation, Price, IsLimited, LimitedQuantity, IsDelisted FROM Frames WHERE FrameCode = @FrameCode",
            connection);
        command.Parameters.AddWithValue("@FrameCode", framecode);

        using var reader = command.ExecuteReader();

        Frame? frame = null;
        while (reader.Read())
        {
            frame = ReadFrame(reader);
        }

        if (frame == null)
        {
            return NotFound();
        }

        return frame;
    }

    // PUT: api/Frame/5
    [HttpPut("{framecode}")]
    public IActionResult PutFrame(string? framecode, Frame frame)
    {
        if (framecode != frame.FrameCode)
        {
            return BadRequest();
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Frames WHERE FrameCode = @FrameCode",
            connection);
        checkCommand.Parameters.AddWithValue("@FrameCode", framecode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            @"UPDATE Frames
              SET FrameName = @FrameName, ImagePath = @ImagePath, ImagePublicId = @ImagePublicId,
                  Orientation = @Orientation, Price = @Price, IsLimited = @IsLimited,
                  LimitedQuantity = @LimitedQuantity, IsDelisted = @IsDelisted
              WHERE FrameCode = @FrameCode",
            connection);

        command.Parameters.AddWithValue("@FrameCode", frame.FrameCode);
        command.Parameters.AddWithValue("@FrameName", frame.FrameName);
        command.Parameters.AddWithValue("@ImagePath", (object?)frame.ImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@ImagePublicId", (object?)frame.ImagePublicId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Orientation", frame.Orientation);
        command.Parameters.AddWithValue("@Price", frame.Price);
        command.Parameters.AddWithValue("@IsLimited", frame.IsLimited ? 1 : 0);
        command.Parameters.AddWithValue("@LimitedQuantity", (object?)frame.LimitedQuantity ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsDelisted", frame.IsDelisted ? 1 : 0);

        command.ExecuteNonQuery();
        LogAudit(frame.FrameCode, frame.FrameName, "Edit");

        return NoContent();
    }

    // POST: api/Frame
    [HttpPost]
    public ActionResult<Frame> PostFrame(Frame frame)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        frame.FrameCode = GenerateUniqueFrameCode(connection);

        var command = new SqliteCommand(
            @"INSERT INTO Frames (FrameCode, FrameName, ImagePath, ImagePublicId, Orientation, Price, IsLimited, LimitedQuantity, IsDelisted)
              VALUES (@FrameCode, @FrameName, @ImagePath, @ImagePublicId, @Orientation, @Price, @IsLimited, @LimitedQuantity, @IsDelisted)",
            connection);

        command.Parameters.AddWithValue("@FrameCode", frame.FrameCode);
        command.Parameters.AddWithValue("@FrameName", frame.FrameName);
        command.Parameters.AddWithValue("@ImagePath", (object?)frame.ImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@ImagePublicId", (object?)frame.ImagePublicId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Orientation", frame.Orientation);
        command.Parameters.AddWithValue("@Price", frame.Price);
        command.Parameters.AddWithValue("@IsLimited", frame.IsLimited ? 1 : 0);
        command.Parameters.AddWithValue("@LimitedQuantity", (object?)frame.LimitedQuantity ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsDelisted", frame.IsDelisted ? 1 : 0);

        command.ExecuteNonQuery();
        LogAudit(frame.FrameCode, frame.FrameName, "Create");

        return CreatedAtAction("GetFrame", new { framecode = frame.FrameCode }, frame);
    }

    // PUT: api/Frame/5/delist
    [HttpPut("{framecode}/delist")]
    public IActionResult DelistFrame(string framecode)
    {
        return SetDelisted(framecode, true);
    }

    // PUT: api/Frame/5/relist
    [HttpPut("{framecode}/relist")]
    public IActionResult RelistFrame(string framecode)
    {
        return SetDelisted(framecode, false);
    }

    private IActionResult SetDelisted(string framecode, bool isDelisted)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var checkCommand = new SqliteCommand(
            "SELECT FrameName FROM Frames WHERE FrameCode = @FrameCode",
            connection);
        checkCommand.Parameters.AddWithValue("@FrameCode", framecode);
        var frameName = checkCommand.ExecuteScalar() as string;

        if (frameName == null)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            "UPDATE Frames SET IsDelisted = @IsDelisted WHERE FrameCode = @FrameCode",
            connection);
        command.Parameters.AddWithValue("@FrameCode", framecode);
        command.Parameters.AddWithValue("@IsDelisted", isDelisted ? 1 : 0);
        command.ExecuteNonQuery();
        LogAudit(framecode, frameName, isDelisted ? "Delist" : "Relist");

        return NoContent();
    }

    // DELETE: api/Frame/5
    [HttpDelete("{framecode}")]
    public IActionResult DeleteFrame(string? framecode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Frames WHERE FrameCode = @FrameCode",
            connection);
        checkCommand.Parameters.AddWithValue("@FrameCode", framecode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            "DELETE FROM Frames WHERE FrameCode = @FrameCode",
            connection);
        command.Parameters.AddWithValue("@FrameCode", framecode);
        command.ExecuteNonQuery();

        return NoContent();
    }
}
