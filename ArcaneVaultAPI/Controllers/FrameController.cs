using ArcaneVaultAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FrameController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/Frame
    [HttpGet]
    public ActionResult<IEnumerable<Frame>> GetFrame()
    {
        var frames = new List<Frame>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT FrameCode, FrameName, ImagePath FROM Frames",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            frames.Add(new Frame
            {
                FrameCode = reader["FrameCode"].ToString(),
                FrameName = reader["FrameName"].ToString(),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString()
            });
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
            "SELECT FrameCode, FrameName, ImagePath FROM Frames WHERE FrameCode = @FrameCode",
            connection);
        command.Parameters.AddWithValue("@FrameCode", framecode);

        using var reader = command.ExecuteReader();

        Frame? frame = null;
        while (reader.Read())
        {
            frame = new Frame
            {
                FrameCode = reader["FrameCode"].ToString(),
                FrameName = reader["FrameName"].ToString(),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString()
            };
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
              SET FrameName = @FrameName, ImagePath = @ImagePath 
              WHERE FrameCode = @FrameCode",
            connection);

        command.Parameters.AddWithValue("@FrameCode", frame.FrameCode);
        command.Parameters.AddWithValue("@FrameName", frame.FrameName);
        command.Parameters.AddWithValue("@ImagePath", (object?)frame.ImagePath ?? DBNull.Value);

        command.ExecuteNonQuery();

        return NoContent();
    }

    // POST: api/Frame
    [HttpPost]
    public ActionResult<Frame> PostFrame(Frame frame)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "INSERT INTO Frames (FrameCode, FrameName, ImagePath) VALUES (@FrameCode, @FrameName, @ImagePath)",
            connection);

        command.Parameters.AddWithValue("@FrameCode", frame.FrameCode);
        command.Parameters.AddWithValue("@FrameName", frame.FrameName);
        command.Parameters.AddWithValue("@ImagePath", (object?)frame.ImagePath ?? DBNull.Value);

        command.ExecuteNonQuery();

        return CreatedAtAction("GetFrame", new { framecode = frame.FrameCode }, frame);
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