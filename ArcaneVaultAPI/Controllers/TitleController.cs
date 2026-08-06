using ArcaneVaultAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TitleController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/Title
    [HttpGet]
    public ActionResult<IEnumerable<Title>> GetTitle()
    {
        var titles = new List<Title>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT TitleCode, TitleName, TitleDescription, ImagePath FROM Titles",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            titles.Add(new Title
            {
                TitleCode = reader["TitleCode"].ToString(),
                TitleName = reader["TitleName"].ToString(),
                TitleDescription = reader["TitleDescription"] == DBNull.Value ? null : reader["TitleDescription"].ToString(),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString()
            });
        }

        // LINQ — order by TitleName
        var result = titles.OrderBy(t => t.TitleName).ToList();

        return result;
    }

    // GET: api/Title/5
    [HttpGet("{titlecode}")]
    public ActionResult<Title> GetTitle(string titlecode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT TitleCode, TitleName, TitleDescription, ImagePath FROM Titles WHERE TitleCode = @TitleCode",
            connection);
        command.Parameters.AddWithValue("@TitleCode", titlecode);

        using var reader = command.ExecuteReader();

        Title? title = null;
        while (reader.Read())
        {
            title = new Title
            {
                TitleCode = reader["TitleCode"].ToString(),
                TitleName = reader["TitleName"].ToString(),
                TitleDescription = reader["TitleDescription"] == DBNull.Value ? null : reader["TitleDescription"].ToString(),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString()
            };
        }

        if (title == null)
        {
            return NotFound();
        }

        return title;
    }

    // PUT: api/Title/5
    [HttpPut("{titlecode}")]
    public IActionResult PutTitle(string? titlecode, Title title)
    {
        if (titlecode != title.TitleCode)
        {
            return BadRequest();
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Titles WHERE TitleCode = @TitleCode",
            connection);
        checkCommand.Parameters.AddWithValue("@TitleCode", titlecode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            @"UPDATE Titles 
              SET TitleName = @TitleName, TitleDescription = @TitleDescription, ImagePath = @ImagePath 
              WHERE TitleCode = @TitleCode",
            connection);

        command.Parameters.AddWithValue("@TitleCode", title.TitleCode);
        command.Parameters.AddWithValue("@TitleName", title.TitleName);
        command.Parameters.AddWithValue("@TitleDescription", (object?)title.TitleDescription ?? DBNull.Value);
        command.Parameters.AddWithValue("@ImagePath", (object?)title.ImagePath ?? DBNull.Value);

        command.ExecuteNonQuery();

        return NoContent();
    }

    // POST: api/Title
    [HttpPost]
    public ActionResult<Title> PostTitle(Title title)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "INSERT INTO Titles (TitleCode, TitleName, TitleDescription, ImagePath) VALUES (@TitleCode, @TitleName, @TitleDescription, @ImagePath)",
            connection);

        command.Parameters.AddWithValue("@TitleCode", title.TitleCode);
        command.Parameters.AddWithValue("@TitleName", title.TitleName);
        command.Parameters.AddWithValue("@TitleDescription", (object?)title.TitleDescription ?? DBNull.Value);
        command.Parameters.AddWithValue("@ImagePath", (object?)title.ImagePath ?? DBNull.Value);

        command.ExecuteNonQuery();

        return CreatedAtAction("GetTitle", new { titlecode = title.TitleCode }, title);
    }

    // DELETE: api/Title/5
    [HttpDelete("{titlecode}")]
    public IActionResult DeleteTitle(string? titlecode)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Titles WHERE TitleCode = @TitleCode",
            connection);
        checkCommand.Parameters.AddWithValue("@TitleCode", titlecode);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            "DELETE FROM Titles WHERE TitleCode = @TitleCode",
            connection);
        command.Parameters.AddWithValue("@TitleCode", titlecode);
        command.ExecuteNonQuery();

        return NoContent();
    }
}