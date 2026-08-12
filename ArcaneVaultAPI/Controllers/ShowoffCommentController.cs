//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ArcaneVaultAPI.DataTransferObject;

[Route("api/[controller]")]
[ApiController]
public class ShowoffCommentController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/ShowoffComment/{showoffId} — paginated comments for a showoff
    [HttpGet("{showoffId}")]
    public ActionResult GetComments(int showoffId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Get total count
        var countCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM ShowoffComments WHERE ShowoffId = @ShowoffId",
            connection);
        countCommand.Parameters.AddWithValue("@ShowoffId", showoffId);
        var totalCount = Convert.ToInt32(countCommand.ExecuteScalar());

        // Get paginated comments with user avatar
        var command = new SqliteCommand(
            @"SELECT sc.CommentId, sc.ShowoffId, sc.UserName, sc.Text, sc.CreatedAt,
                     u.AvatarUrl
              FROM ShowoffComments sc
              LEFT JOIN ArcaneVaultUsers u ON sc.UserName = u.UserName
              WHERE sc.ShowoffId = @ShowoffId
              ORDER BY sc.CreatedAt DESC
              LIMIT @PageSize OFFSET @Offset",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", showoffId);
        command.Parameters.AddWithValue("@PageSize", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        var comments = new List<ShowoffCommentDTO>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            comments.Add(new ShowoffCommentDTO
            {
                CommentId = Convert.ToInt32(reader["CommentId"]),
                ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                UserName = reader["UserName"].ToString(),
                AvatarUrl = reader["AvatarUrl"] == DBNull.Value ? null : reader["AvatarUrl"].ToString(),
                Text = reader["Text"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            });
        }

        return Ok(new
        {
            Comments = comments,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    // POST: api/ShowoffComment — add a comment
    [HttpPost]
    public IActionResult PostComment([FromBody] CreateShowoffCommentDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest("Comment text cannot be empty.");

        if (dto.Text.Length > 500)
            return BadRequest("Comment text cannot exceed 500 characters.");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"INSERT INTO ShowoffComments (ShowoffId, UserName, Text, CreatedAt)
              VALUES (@ShowoffId, @UserName, @Text, @CreatedAt)",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", dto.ShowoffId);
        command.Parameters.AddWithValue("@UserName", dto.UserName);
        command.Parameters.AddWithValue("@Text", dto.Text);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.ExecuteNonQuery();

        return Ok();
    }
}
