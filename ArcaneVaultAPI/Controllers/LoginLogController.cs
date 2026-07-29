using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LoginLogController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // POST: api/LoginLog
    [HttpPost]
    public IActionResult RecordLogin([FromBody] string userName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "INSERT INTO LoginLogs (UserName, LoggedInAt) VALUES (@UserName, @LoggedInAt)",
            connection);
        command.Parameters.AddWithValue("@UserName", userName);
        command.Parameters.AddWithValue("@LoggedInAt", DateTime.UtcNow);
        command.ExecuteNonQuery();

        return Ok();
    }

    // GET: api/LoginLog/dau
    [HttpGet("dau")]
    public ActionResult GetDAU()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT COUNT(DISTINCT UserName) as Count
              FROM LoginLogs
              WHERE LoggedInAt >= @Since",
            connection);
        command.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-1));
        var count = Convert.ToInt32(command.ExecuteScalar());
        return Ok(new { Count = count });
    }

    // GET: api/LoginLog/wau
    [HttpGet("wau")]
    public ActionResult GetWAU()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT COUNT(DISTINCT UserName) as Count
              FROM LoginLogs
              WHERE LoggedInAt >= @Since",
            connection);
        command.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-7));
        var count = Convert.ToInt32(command.ExecuteScalar());
        return Ok(new { Count = count });
    }

    // GET: api/LoginLog/mau
    [HttpGet("mau")]
    public ActionResult GetMAU()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT COUNT(DISTINCT UserName) as Count
              FROM LoginLogs
              WHERE LoggedInAt >= @Since",
            connection);
        command.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-30));
        var count = Convert.ToInt32(command.ExecuteScalar());
        return Ok(new { Count = count });
    }

    // GET: api/LoginLog/activity — daily login counts for last 30 days
    [HttpGet("activity")]
    public ActionResult GetActivity()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT DATE(LoggedInAt) as Date, COUNT(DISTINCT UserName) as Count
              FROM LoginLogs
              WHERE LoggedInAt >= @Since
              GROUP BY DATE(LoggedInAt)
              ORDER BY Date ASC",
            connection);
        command.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-30));

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new
            {
                Date = reader["Date"].ToString(),
                Count = Convert.ToInt32(reader["Count"])
            });
        }

        return Ok(results);
    }
}