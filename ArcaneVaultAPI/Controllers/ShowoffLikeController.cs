//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
public class ShowoffLikeController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/ShowoffLike/{showoffId} — get like count and check if user liked
    [HttpGet("{showoffId}")]
    public ActionResult GetLikes(int showoffId, [FromQuery] string? userName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var countCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM ShowoffLikes WHERE ShowoffId = @ShowoffId",
            connection);
        countCommand.Parameters.AddWithValue("@ShowoffId", showoffId);
        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        bool userLiked = false;
        if (!string.IsNullOrEmpty(userName))
        {
            var likedCommand = new SqliteCommand(
                "SELECT COUNT(*) FROM ShowoffLikes WHERE ShowoffId = @ShowoffId AND UserName = @UserName",
                connection);
            likedCommand.Parameters.AddWithValue("@ShowoffId", showoffId);
            likedCommand.Parameters.AddWithValue("@UserName", userName);
            userLiked = Convert.ToInt32(likedCommand.ExecuteScalar()) > 0;
        }

        return Ok(new { LikeCount = count, UserLiked = userLiked });
    }

    // POST: api/ShowoffLike — like a showoff
    [HttpPost]
    public IActionResult LikeShowoff([FromBody] ShowoffLikeRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if already liked
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM ShowoffLikes WHERE ShowoffId = @ShowoffId AND UserName = @UserName",
            connection);
        checkCommand.Parameters.AddWithValue("@ShowoffId", request.ShowoffId);
        checkCommand.Parameters.AddWithValue("@UserName", request.UserName);

        if (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0)
            return Conflict("Already liked.");

        var insertCommand = new SqliteCommand(
            "INSERT INTO ShowoffLikes (ShowoffId, UserName) VALUES (@ShowoffId, @UserName)",
            connection);
        insertCommand.Parameters.AddWithValue("@ShowoffId", request.ShowoffId);
        insertCommand.Parameters.AddWithValue("@UserName", request.UserName);
        insertCommand.ExecuteNonQuery();

        return Ok();
    }

    // DELETE: api/ShowoffLike/{showoffId}/{userName} — unlike
    [HttpDelete("{showoffId}/{userName}")]
    public IActionResult UnlikeShowoff(int showoffId, string userName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var deleteCommand = new SqliteCommand(
            "DELETE FROM ShowoffLikes WHERE ShowoffId = @ShowoffId AND UserName = @UserName",
            connection);
        deleteCommand.Parameters.AddWithValue("@ShowoffId", showoffId);
        deleteCommand.Parameters.AddWithValue("@UserName", userName);
        deleteCommand.ExecuteNonQuery();

        return NoContent();
    }
}

public class ShowoffLikeRequest
{
    public int ShowoffId { get; set; }
    public string UserName { get; set; }
}