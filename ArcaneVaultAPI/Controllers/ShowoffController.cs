using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.DataTransferObject;

[Route("api/[controller]")]
[ApiController]
public class ShowoffController : ControllerBase
{
    private readonly ArcaneDbContext _context;
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    public ShowoffController(ArcaneDbContext context)
    {
        _context = context;
    }

    // GET: api/Showoff — all public non-deleted showoffs with items and like count
    [HttpGet]
    public ActionResult<IEnumerable<ShowoffDTO>> GetShowoffs()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // ADO.NET — get all public non-deleted showoffs
        var showoffCommand = new SqliteCommand(
            @"SELECT s.ShowoffId, s.ShowoffName, s.BackgroundColor, s.IsPrivate,
                     s.IsDeleted, s.UserName, s.CreatedAt,
                     COUNT(sl.UserName) as LikeCount
              FROM Showoffs s
              LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
              WHERE s.IsDeleted = 0 AND s.IsPrivate = 0
              GROUP BY s.ShowoffId
              ORDER BY s.CreatedAt DESC",
            connection);

        var showoffs = new List<ShowoffDTO>();
        using var reader = showoffCommand.ExecuteReader();
        while (reader.Read())
        {
            showoffs.Add(new ShowoffDTO
            {
                ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                ShowoffName = reader["ShowoffName"].ToString(),
                BackgroundColor = reader["BackgroundColor"].ToString(),
                IsPrivate = Convert.ToBoolean(reader["IsPrivate"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                UserName = reader["UserName"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                LikeCount = Convert.ToInt32(reader["LikeCount"])
            });
        }

        // LINQ — attach items to each showoff
        foreach (var showoff in showoffs)
        {
            showoff.Items = GetShowoffItems(connection, showoff.ShowoffId);
        }

        return Ok(showoffs);
    }

    // GET: api/Showoff/user/{username} — all non-deleted showoffs for a user
    [HttpGet("user/{username}")]
    public ActionResult<IEnumerable<ShowoffDTO>> GetUserShowoffs(string username)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT s.ShowoffId, s.ShowoffName, s.BackgroundColor, s.IsPrivate,
                     s.IsDeleted, s.UserName, s.CreatedAt,
                     COUNT(sl.UserName) as LikeCount
              FROM Showoffs s
              LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
              WHERE s.IsDeleted = 0 AND s.UserName = @UserName
              GROUP BY s.ShowoffId
              ORDER BY s.CreatedAt DESC",
            connection);
        command.Parameters.AddWithValue("@UserName", username);

        var showoffs = new List<ShowoffDTO>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            showoffs.Add(new ShowoffDTO
            {
                ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                ShowoffName = reader["ShowoffName"].ToString(),
                BackgroundColor = reader["BackgroundColor"].ToString(),
                IsPrivate = Convert.ToBoolean(reader["IsPrivate"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                UserName = reader["UserName"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                LikeCount = Convert.ToInt32(reader["LikeCount"])
            });
        }

        foreach (var showoff in showoffs)
        {
            showoff.Items = GetShowoffItems(connection, showoff.ShowoffId);
        }

        return Ok(showoffs);
    }

    // GET: api/Showoff/{id}
    [HttpGet("{id}")]
    public ActionResult<ShowoffDTO> GetShowoff(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT s.ShowoffId, s.ShowoffName, s.BackgroundColor, s.IsPrivate,
                     s.IsDeleted, s.UserName, s.CreatedAt,
                     COUNT(sl.UserName) as LikeCount
              FROM Showoffs s
              LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
              WHERE s.ShowoffId = @ShowoffId AND s.IsDeleted = 0
              GROUP BY s.ShowoffId",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", id);

        ShowoffDTO? showoff = null;
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            showoff = new ShowoffDTO
            {
                ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                ShowoffName = reader["ShowoffName"].ToString(),
                BackgroundColor = reader["BackgroundColor"].ToString(),
                IsPrivate = Convert.ToBoolean(reader["IsPrivate"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                UserName = reader["UserName"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                LikeCount = Convert.ToInt32(reader["LikeCount"])
            };
            showoff.Items = GetShowoffItems(connection, showoff.ShowoffId);
        }

        if (showoff == null) return NotFound();
        return Ok(showoff);
    }

    // POST: api/Showoff
    [HttpPost]
    public async Task<ActionResult<ShowoffDTO>> PostShowoff(CreateShowoffDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check max 3 showoffs per user (non-deleted)
        var countCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Showoffs WHERE UserName = @UserName AND IsDeleted = 0",
            connection);
        countCommand.Parameters.AddWithValue("@UserName", dto.UserName);
        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        if (count >= 3)
            return BadRequest("Maximum of 3 showoffs per user reached.");

        // Insert showoff
        var insertCommand = new SqliteCommand(
            @"INSERT INTO Showoffs (ShowoffName, BackgroundColor, IsPrivate, IsDeleted, UserName, CreatedAt)
              VALUES (@ShowoffName, @BackgroundColor, @IsPrivate, 0, @UserName, @CreatedAt);
              SELECT last_insert_rowid();",
            connection);
        insertCommand.Parameters.AddWithValue("@ShowoffName", dto.ShowoffName);
        insertCommand.Parameters.AddWithValue("@BackgroundColor", dto.BackgroundColor);
        insertCommand.Parameters.AddWithValue("@IsPrivate", dto.IsPrivate);
        insertCommand.Parameters.AddWithValue("@UserName", dto.UserName);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

        var newId = Convert.ToInt32(insertCommand.ExecuteScalar());

        // Insert showoff items
        foreach (var item in dto.Items)
        {
            var itemCommand = new SqliteCommand(
                @"INSERT INTO ShowoffItems (ShowoffId, ItemId, ImagePath, ImagePublicId, DisplayOrder)
                  VALUES (@ShowoffId, @ItemId, @ImagePath, @ImagePublicId, @DisplayOrder)",
                connection);
            itemCommand.Parameters.AddWithValue("@ShowoffId", newId);
            itemCommand.Parameters.AddWithValue("@ItemId", item.ItemId);
            itemCommand.Parameters.AddWithValue("@ImagePath", item.ImagePath ?? (object)DBNull.Value);
            itemCommand.Parameters.AddWithValue("@ImagePublicId", item.ImagePublicId ?? (object)DBNull.Value);
            itemCommand.Parameters.AddWithValue("@DisplayOrder", item.DisplayOrder);
            itemCommand.ExecuteNonQuery();
        }

        return CreatedAtAction("GetShowoff", new { id = newId }, new { ShowoffId = newId });
    }

    // PUT: api/Showoff/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutShowoff(int id, UpdateShowoffDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Showoffs WHERE ShowoffId = @ShowoffId AND IsDeleted = 0",
            connection);
        checkCommand.Parameters.AddWithValue("@ShowoffId", id);
        if (Convert.ToInt32(checkCommand.ExecuteScalar()) == 0)
            return NotFound();

        // Update showoff
        var updateCommand = new SqliteCommand(
            @"UPDATE Showoffs
              SET ShowoffName = @ShowoffName,
                  BackgroundColor = @BackgroundColor,
                  IsPrivate = @IsPrivate
              WHERE ShowoffId = @ShowoffId",
            connection);
        updateCommand.Parameters.AddWithValue("@ShowoffName", dto.ShowoffName);
        updateCommand.Parameters.AddWithValue("@BackgroundColor", dto.BackgroundColor);
        updateCommand.Parameters.AddWithValue("@IsPrivate", dto.IsPrivate);
        updateCommand.Parameters.AddWithValue("@ShowoffId", id);
        updateCommand.ExecuteNonQuery();

        // Delete old showoff items
        var deleteItemsCommand = new SqliteCommand(
            "DELETE FROM ShowoffItems WHERE ShowoffId = @ShowoffId",
            connection);
        deleteItemsCommand.Parameters.AddWithValue("@ShowoffId", id);
        deleteItemsCommand.ExecuteNonQuery();

        // Insert new showoff items
        foreach (var item in dto.Items)
        {
            var itemCommand = new SqliteCommand(
                @"INSERT INTO ShowoffItems (ShowoffId, ItemId, ImagePath, ImagePublicId, DisplayOrder)
                  VALUES (@ShowoffId, @ItemId, @ImagePath, @ImagePublicId, @DisplayOrder)",
                connection);
            itemCommand.Parameters.AddWithValue("@ShowoffId", id);
            itemCommand.Parameters.AddWithValue("@ItemId", item.ItemId);
            itemCommand.Parameters.AddWithValue("@ImagePath", item.ImagePath ?? (object)DBNull.Value);
            itemCommand.Parameters.AddWithValue("@ImagePublicId", item.ImagePublicId ?? (object)DBNull.Value);
            itemCommand.Parameters.AddWithValue("@DisplayOrder", item.DisplayOrder);
            itemCommand.ExecuteNonQuery();
        }

        return NoContent();
    }

    // DELETE: api/Showoff/{id} — soft delete
    [HttpDelete("{id}")]
    public IActionResult DeleteShowoff(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Showoffs WHERE ShowoffId = @ShowoffId AND IsDeleted = 0",
            connection);
        checkCommand.Parameters.AddWithValue("@ShowoffId", id);
        if (Convert.ToInt32(checkCommand.ExecuteScalar()) == 0)
            return NotFound();

        var deleteCommand = new SqliteCommand(
            "UPDATE Showoffs SET IsDeleted = 1 WHERE ShowoffId = @ShowoffId",
            connection);
        deleteCommand.Parameters.AddWithValue("@ShowoffId", id);
        deleteCommand.ExecuteNonQuery();

        return NoContent();
    }

    // Helper — get showoff items with item name and category
    private List<ShowoffItemDTO> GetShowoffItems(SqliteConnection connection, int showoffId)
    {
        var items = new List<ShowoffItemDTO>();

        var command = new SqliteCommand(
            @"SELECT si.ShowoffId, si.ItemId, si.ImagePath, si.ImagePublicId, si.DisplayOrder,
                     ci.ItemName, c.CategoryName
              FROM ShowoffItems si
              JOIN CollectionItems ci ON si.ItemId = ci.ItemId
              LEFT JOIN CollectionItemCategories cic ON ci.ItemId = cic.ItemId
              LEFT JOIN Categories c ON cic.CategoryCode = c.CategoryCode
              WHERE si.ShowoffId = @ShowoffId
              ORDER BY si.DisplayOrder",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", showoffId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new ShowoffItemDTO
            {
                ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                ItemId = Convert.ToInt32(reader["ItemId"]),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString(),
                ImagePublicId = reader["ImagePublicId"] == DBNull.Value ? null : reader["ImagePublicId"].ToString(),
                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                ItemName = reader["ItemName"].ToString(),
                CategoryName = reader["CategoryName"] == DBNull.Value ? null : reader["CategoryName"].ToString()
            });
        }

        return items;
    }
}