//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
public class SearchLogController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // POST: api/SearchLog
    [HttpPost]
    public IActionResult RecordSearch([FromBody] SearchLogRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"INSERT INTO SearchLogs (SearchTerm, SearchType, UserName, SearchedAt)
              VALUES (@SearchTerm, @SearchType, @UserName, @SearchedAt)",
            connection);
        command.Parameters.AddWithValue("@SearchTerm", request.SearchTerm);
        command.Parameters.AddWithValue("@SearchType", request.SearchType);
        command.Parameters.AddWithValue("@UserName", request.UserName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SearchedAt", DateTime.UtcNow);
        command.ExecuteNonQuery();

        return Ok();
    }

    // GET: api/SearchLog/trending — top searched terms
    [HttpGet("trending")]
    public ActionResult GetTrending()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT SearchTerm, SearchType, COUNT(*) as Count
              FROM SearchLogs
              GROUP BY SearchTerm, SearchType
              ORDER BY Count DESC
              LIMIT 20",
            connection);

        var results = new List<object>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new
            {
                SearchTerm = reader["SearchTerm"].ToString(),
                SearchType = reader["SearchType"].ToString(),
                Count = Convert.ToInt32(reader["Count"])
            });
        }

        return Ok(results);
    }

    // GET: api/SearchLog/trending/overtime — search trends over time
    [HttpGet("trending/overtime")]
    public ActionResult GetTrendingOverTime([FromQuery] int days = 30)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT DATE(SearchedAt) as Date, COUNT(*) as Count
              FROM SearchLogs
              WHERE SearchedAt >= @Since
              GROUP BY DATE(SearchedAt)
              ORDER BY Date ASC",
            connection);
        command.Parameters.AddWithValue("@Since", DateTime.UtcNow.AddDays(-days));

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

public class SearchLogRequest
{
    public string SearchTerm { get; set; }
    public string SearchType { get; set; }
    public string? UserName { get; set; }
}