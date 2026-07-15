using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
public class ShowoffViewController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // POST: api/ShowoffView — record a view
    [HttpPost]
    public IActionResult RecordView([FromBody] ShowoffViewRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"INSERT INTO ShowoffViews (ShowoffId, ViewerUserName, ViewedAt)
              VALUES (@ShowoffId, @ViewerUserName, @ViewedAt)",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", request.ShowoffId);
        command.Parameters.AddWithValue("@ViewerUserName", request.ViewerUserName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ViewedAt", DateTime.UtcNow);
        command.ExecuteNonQuery();

        return Ok();
    }

    // GET: api/ShowoffView/{showoffId} — get view count
    [HttpGet("{showoffId}")]
    public ActionResult GetViewCount(int showoffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT COUNT(*) FROM ShowoffViews WHERE ShowoffId = @ShowoffId",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", showoffId);
        var count = Convert.ToInt32(command.ExecuteScalar());

        return Ok(new { ViewCount = count });
    }
}

public class ShowoffViewRequest
{
    public int ShowoffId { get; set; }
    public string? ViewerUserName { get; set; }
}