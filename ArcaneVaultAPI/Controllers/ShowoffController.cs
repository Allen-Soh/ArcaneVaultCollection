using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.DataTransferObject;
using ArcaneVaultAPI.Services;

[Route("api/[controller]")]
[ApiController]
public class ShowoffController : ControllerBase
{
    private readonly ArcaneDbContext _context;
    private readonly IEmailService _emailService;
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    public ShowoffController(ArcaneDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // GET: api/Showoff — all public non-deleted non-disabled showoffs with items and like count
    [HttpGet]
    public ActionResult<IEnumerable<ShowoffDTO>> GetShowoffs()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // ADO.NET — get all public non-deleted non-disabled showoffs
        var showoffCommand = new SqliteCommand(
            @"SELECT s.ShowoffId, s.ShowoffName, s.BackgroundColor, s.BackgroundImagePath, s.BackgroundImagePublicId, s.IsPrivate,
                     s.IsDeleted, s.UserName, s.CreatedAt, s.Description,
                     s.IsDisabled, s.DisabledAt,
                     COUNT(sl.UserName) as LikeCount,
                     (SELECT COUNT(*) FROM ShowoffViews sv WHERE sv.ShowoffId = s.ShowoffId) as ViewCount,
                     (SELECT COUNT(*) FROM ShowoffComments sc WHERE sc.ShowoffId = s.ShowoffId) as CommentCount,
                     (SELECT COUNT(*) FROM ShowoffReports sr WHERE sr.ShowoffId = s.ShowoffId) as ReportCount
              FROM Showoffs s
              LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
              WHERE s.IsDeleted = 0 AND s.IsPrivate = 0 AND s.IsDisabled = 0
              GROUP BY s.ShowoffId
              ORDER BY s.CreatedAt DESC",
            connection);

        var showoffs = ReadShowoffDTOs(showoffCommand);

        // LINQ — attach items to each showoff
        foreach (var showoff in showoffs)
        {
            showoff.Items = GetShowoffItems(connection, showoff.ShowoffId);
        }

        return Ok(showoffs);
    }

    // GET: api/Showoff/user/{username} — all non-deleted showoffs for a user
    // Query param ?requester= controls disabled visibility
    [HttpGet("user/{username}")]
    public ActionResult<IEnumerable<ShowoffDTO>> GetUserShowoffs(string username, [FromQuery] string? requester = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // If requester is the owner, show all (including disabled). Otherwise hide disabled.
        bool isOwner = !string.IsNullOrEmpty(requester) &&
                       requester.Equals(username, StringComparison.OrdinalIgnoreCase);

        var sql = @"SELECT s.ShowoffId, s.ShowoffName, s.BackgroundColor, s.BackgroundImagePath, s.BackgroundImagePublicId, s.IsPrivate,
                     s.IsDeleted, s.UserName, s.CreatedAt, s.Description,
                     s.IsDisabled, s.DisabledAt,
                     COUNT(sl.UserName) as LikeCount,
                     (SELECT COUNT(*) FROM ShowoffViews sv WHERE sv.ShowoffId = s.ShowoffId) as ViewCount,
                     (SELECT COUNT(*) FROM ShowoffComments sc WHERE sc.ShowoffId = s.ShowoffId) as CommentCount,
                     (SELECT COUNT(*) FROM ShowoffReports sr WHERE sr.ShowoffId = s.ShowoffId) as ReportCount
              FROM Showoffs s
              LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
              WHERE s.IsDeleted = 0 AND s.UserName = @UserName";

        if (!isOwner)
            sql += " AND s.IsDisabled = 0";

        sql += @" GROUP BY s.ShowoffId
                  ORDER BY s.CreatedAt DESC";

        var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@UserName", username);

        var showoffs = ReadShowoffDTOs(command);

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
            @"SELECT s.ShowoffId, s.ShowoffName, s.BackgroundColor, s.BackgroundImagePath, s.BackgroundImagePublicId, s.IsPrivate,
                     s.IsDeleted, s.UserName, s.CreatedAt, s.Description,
                     s.IsDisabled, s.DisabledAt,
                     COUNT(sl.UserName) as LikeCount,
                     (SELECT COUNT(*) FROM ShowoffViews sv WHERE sv.ShowoffId = s.ShowoffId) as ViewCount,
                     (SELECT COUNT(*) FROM ShowoffComments sc WHERE sc.ShowoffId = s.ShowoffId) as CommentCount,
                     (SELECT COUNT(*) FROM ShowoffReports sr WHERE sr.ShowoffId = s.ShowoffId) as ReportCount
              FROM Showoffs s
              LEFT JOIN ShowoffLikes sl ON s.ShowoffId = sl.ShowoffId
              WHERE s.ShowoffId = @ShowoffId AND s.IsDeleted = 0
              GROUP BY s.ShowoffId",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", id);

        var showoffs = ReadShowoffDTOs(command);
        var showoff = showoffs.FirstOrDefault();

        if (showoff == null) return NotFound();

        showoff.Items = GetShowoffItems(connection, showoff.ShowoffId);
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

        // Insert showoff
        var insertCommand = new SqliteCommand(
            @"INSERT INTO Showoffs (ShowoffName, BackgroundColor, BackgroundImagePath, BackgroundImagePublicId, IsPrivate, IsDeleted, UserName, CreatedAt, Description, IsDisabled)
                VALUES (@ShowoffName, @BackgroundColor, @BackgroundImagePath, @BackgroundImagePublicId, @IsPrivate, 0, @UserName, @CreatedAt, @Description, 0);
                SELECT last_insert_rowid();",
            connection);
        insertCommand.Parameters.AddWithValue("@ShowoffName", dto.ShowoffName);
        insertCommand.Parameters.AddWithValue("@BackgroundColor", dto.BackgroundColor);
        insertCommand.Parameters.AddWithValue("@BackgroundImagePath", dto.BackgroundImagePath ?? (object)DBNull.Value);
        insertCommand.Parameters.AddWithValue("@BackgroundImagePublicId", dto.BackgroundImagePublicId ?? (object)DBNull.Value);
        insertCommand.Parameters.AddWithValue("@IsPrivate", dto.IsPrivate);
        insertCommand.Parameters.AddWithValue("@UserName", dto.UserName);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        insertCommand.Parameters.AddWithValue("@Description", dto.Description ?? (object)DBNull.Value);

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
                  BackgroundImagePath = @BackgroundImagePath,
                  BackgroundImagePublicId = @BackgroundImagePublicId,
                  IsPrivate = @IsPrivate,
                  Description = @Description
              WHERE ShowoffId = @ShowoffId",
            connection);
        updateCommand.Parameters.AddWithValue("@ShowoffName", dto.ShowoffName);
        updateCommand.Parameters.AddWithValue("@BackgroundColor", dto.BackgroundColor);
        updateCommand.Parameters.AddWithValue("@BackgroundImagePath", dto.BackgroundImagePath ?? (object)DBNull.Value);
        updateCommand.Parameters.AddWithValue("@BackgroundImagePublicId", dto.BackgroundImagePublicId ?? (object)DBNull.Value);
        updateCommand.Parameters.AddWithValue("@IsPrivate", dto.IsPrivate);
        updateCommand.Parameters.AddWithValue("@Description", dto.Description ?? (object)DBNull.Value);
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

    // PUT: api/Showoff/{id}/disable — staff disables a showoff
    [HttpPut("{id}/disable")]
    public IActionResult DisableShowoff(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"UPDATE Showoffs SET IsDisabled = 1, DisabledAt = @DisabledAt
              WHERE ShowoffId = @ShowoffId AND IsDeleted = 0",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", id);
        command.Parameters.AddWithValue("@DisabledAt", DateTime.UtcNow);
        var rows = command.ExecuteNonQuery();

        if (rows == 0) return NotFound();
        return NoContent();
    }

    // PUT: api/Showoff/{id}/enable — staff re-enables a showoff (also serves as "approve appeal")
    [HttpPut("{id}/enable")]
    public async Task<IActionResult> EnableShowoff(int id, [FromBody] AppealDecisionDTO? dto = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"UPDATE Showoffs SET IsDisabled = 0, DisabledAt = NULL
              WHERE ShowoffId = @ShowoffId AND IsDeleted = 0",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", id);
        var rows = command.ExecuteNonQuery();

        if (rows == 0) return NotFound();

        // Best-effort notification — re-enabling always counts as an "approved" outcome for the owner
        await NotifyAppealDecision(connection, id, approved: true, dto?.StaffNote);

        return NoContent();
    }

    // PUT: api/Showoff/{id}/deny-appeal — staff keeps a showoff disabled, but formally responds to the appeal
    [HttpPut("{id}/deny-appeal")]
    public async Task<IActionResult> DenyAppeal(int id, [FromBody] AppealDecisionDTO? dto = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Showoffs WHERE ShowoffId = @ShowoffId AND IsDeleted = 0 AND IsDisabled = 1",
            connection);
        checkCommand.Parameters.AddWithValue("@ShowoffId", id);
        if (Convert.ToInt32(checkCommand.ExecuteScalar()) == 0)
            return NotFound();

        await NotifyAppealDecision(connection, id, approved: false, dto?.StaffNote);

        return NoContent();
    }

    // POST: api/Showoff/{id}/appeal — owner appeals a disabled showoff
    [HttpPost("{id}/appeal")]
    public async Task<IActionResult> SubmitAppeal(int id, [FromBody] SubmitAppealDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AppealMessage))
            return BadRequest("Appeal message is required.");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var showoffCommand = new SqliteCommand(
            "SELECT ShowoffName, UserName, IsDisabled FROM Showoffs WHERE ShowoffId = @ShowoffId AND IsDeleted = 0",
            connection);
        showoffCommand.Parameters.AddWithValue("@ShowoffId", id);

        string? showoffName = null;
        string? ownerUserName = null;
        bool isDisabled = false;
        using (var reader = showoffCommand.ExecuteReader())
        {
            if (!reader.Read()) return NotFound();
            showoffName = reader["ShowoffName"].ToString();
            ownerUserName = reader["UserName"].ToString();
            isDisabled = Convert.ToBoolean(reader["IsDisabled"]);
        }

        if (!isDisabled)
            return BadRequest("This showoff is not currently disabled.");

        // Look up the owner's email — needed for the confirmation email
        var ownerEmailCommand = new SqliteCommand(
            "SELECT Email FROM ArcaneVaultUsers WHERE UserName = @UserName", connection);
        ownerEmailCommand.Parameters.AddWithValue("@UserName", ownerUserName);
        var ownerEmail = ownerEmailCommand.ExecuteScalar() as string;

        // Prefer the staff member already assigned to this showoff's reports
        var staffCommand = new SqliteCommand(
            @"SELECT AssignedStaffUserName FROM ShowoffReports
              WHERE ShowoffId = @ShowoffId
              ORDER BY CreatedAt DESC LIMIT 1",
            connection);
        staffCommand.Parameters.AddWithValue("@ShowoffId", id);
        var assignedStaff = staffCommand.ExecuteScalar() as string;

        string? staffEmail = null;
        if (!string.IsNullOrEmpty(assignedStaff))
        {
            var emailCommand = new SqliteCommand(
                "SELECT Email FROM ArcaneVaultUsers WHERE UserName = @UserName", connection);
            emailCommand.Parameters.AddWithValue("@UserName", assignedStaff);
            staffEmail = emailCommand.ExecuteScalar() as string;
        }

        // Fallback — any active staff member, if no assignment found
        if (string.IsNullOrEmpty(staffEmail))
        {
            var fallbackCommand = new SqliteCommand(
                "SELECT Email FROM ArcaneVaultUsers WHERE RoleId = 1 AND IsDeleted = 0 AND IsActive = 1 ORDER BY UserName LIMIT 1",
                connection);
            staffEmail = fallbackCommand.ExecuteScalar() as string;
        }

        if (string.IsNullOrEmpty(staffEmail))
            return StatusCode(500, "No staff available to receive this appeal.");

        try
        {
            await _emailService.SendShowoffAppealSubmittedAsync(staffEmail, ownerUserName, showoffName, dto.AppealMessage, id);
        }
        catch
        {
            return StatusCode(500, "Failed to send appeal email.");
        }

        // Confirmation to the appealing user — best-effort, doesn't fail the request if it errors,
        // since the staff notification (the critical part) already succeeded above
        if (!string.IsNullOrEmpty(ownerEmail))
        {
            try
            {
                await _emailService.SendShowoffAppealConfirmationAsync(ownerEmail, ownerUserName, showoffName);
            }
            catch
            {
                // Log this in a real system — confirmation failure shouldn't fail the whole request
            }
        }

        return Ok();
    }

    // Helper — looks up the owner's email/name/showoff name and sends the appeal decision email.
    // Failure here never fails the request — the state change (or lack thereof) already succeeded.
    private async Task NotifyAppealDecision(SqliteConnection connection, int showoffId, bool approved, string? staffNote)
    {
        var lookupCommand = new SqliteCommand(
            @"SELECT s.ShowoffName, s.UserName, u.Email
              FROM Showoffs s
              JOIN ArcaneVaultUsers u ON s.UserName = u.UserName
              WHERE s.ShowoffId = @ShowoffId",
            connection);
        lookupCommand.Parameters.AddWithValue("@ShowoffId", showoffId);

        string? showoffName = null, ownerUserName = null, ownerEmail = null;
        using (var reader = lookupCommand.ExecuteReader())
        {
            if (reader.Read())
            {
                showoffName = reader["ShowoffName"].ToString();
                ownerUserName = reader["UserName"].ToString();
                ownerEmail = reader["Email"].ToString();
            }
        }

        if (string.IsNullOrEmpty(ownerEmail)) return;

        try
        {
            await _emailService.SendShowoffAppealResponseAsync(ownerEmail, ownerUserName, showoffName, approved, staffNote);
        }
        catch
        {
            // Log this in a real system — notification failure shouldn't fail the whole request
        }
    }

    // Helper — read ShowoffDTOs from a command
    private List<ShowoffDTO> ReadShowoffDTOs(SqliteCommand command)
    {
        var showoffs = new List<ShowoffDTO>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            showoffs.Add(new ShowoffDTO
            {
                ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                ShowoffName = reader["ShowoffName"].ToString(),
                BackgroundColor = reader["BackgroundColor"].ToString(),
                BackgroundImagePath = reader["BackgroundImagePath"] == DBNull.Value ? null : reader["BackgroundImagePath"].ToString(),
                BackgroundImagePublicId = reader["BackgroundImagePublicId"] == DBNull.Value ? null : reader["BackgroundImagePublicId"].ToString(),
                IsPrivate = Convert.ToBoolean(reader["IsPrivate"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                UserName = reader["UserName"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                IsDisabled = Convert.ToBoolean(reader["IsDisabled"]),
                DisabledAt = reader["DisabledAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["DisabledAt"]),
                LikeCount = Convert.ToInt32(reader["LikeCount"]),
                ViewCount = Convert.ToInt32(reader["ViewCount"]),
                CommentCount = Convert.ToInt32(reader["CommentCount"]),
                ReportCount = Convert.ToInt32(reader["ReportCount"])
            });
        }
        return showoffs;
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