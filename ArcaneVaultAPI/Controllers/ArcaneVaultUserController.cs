//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.DataTransferObject;
using ArcaneVaultAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
public class ArcaneVaultUserController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";
    private readonly IEmailService _emailService;

    public ArcaneVaultUserController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    // GET: api/ArcaneVaultUser
    [HttpGet]
    public ActionResult<IEnumerable<ArcaneVaultUser>> GetArcaneVaultUser()
    {
        var users = new List<ArcaneVaultUser>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT UserName, Email, IsDeleted, RoleId, Country, AvatarUrl, IsActive FROM ArcaneVaultUsers",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new ArcaneVaultUser
            {
                UserName = reader["UserName"].ToString(),
                Email = reader["Email"].ToString(),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                Country = reader["Country"] == DBNull.Value ? null : reader["Country"].ToString(),
                AvatarUrl = reader["AvatarUrl"] == DBNull.Value ? null : reader["AvatarUrl"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            });
        }

        // LINQ — order by UserName
        var result = users.OrderBy(u => u.UserName).ToList();

        return result;
    }

    // GET: api/ArcaneVaultUser/5
    [HttpGet("{username}")]
    public ActionResult<ArcaneVaultUser> GetArcaneVaultUser(string username)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT UserName, Email, IsDeleted, RoleId, Country, AvatarUrl, IsActive FROM ArcaneVaultUsers WHERE UserName = @UserName",
            connection);
        command.Parameters.AddWithValue("@UserName", username);

        using var reader = command.ExecuteReader();

        ArcaneVaultUser? user = null;
        while (reader.Read())
        {
            user = new ArcaneVaultUser
            {
                UserName = reader["UserName"].ToString(),
                Email = reader["Email"].ToString(),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                Country = reader["Country"] == DBNull.Value ? null : reader["Country"].ToString(),
                AvatarUrl = reader["AvatarUrl"] == DBNull.Value ? null : reader["AvatarUrl"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // PUT: api/ArcaneVaultUser/5
    [HttpPut("{username}")]
    public IActionResult PutArcaneVaultUser(string? username, ArcaneVaultUser arcaneVaultUser)
    {
        if (username != arcaneVaultUser.UserName)
        {
            return BadRequest();
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Check if exists
        var checkCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM ArcaneVaultUsers WHERE UserName = @UserName",
            connection);
        checkCommand.Parameters.AddWithValue("@UserName", username);
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count == 0)
        {
            return NotFound();
        }

        var command = new SqliteCommand(
            @"UPDATE ArcaneVaultUsers 
              SET Email = @Email, 
                  IsDeleted = @IsDeleted, 
                  RoleId = @RoleId, 
                  Password = @Password 
              WHERE UserName = @UserName",
            connection);

        command.Parameters.AddWithValue("@UserName", arcaneVaultUser.UserName);
        command.Parameters.AddWithValue("@Email", arcaneVaultUser.Email);
        command.Parameters.AddWithValue("@IsDeleted", arcaneVaultUser.IsDeleted);
        command.Parameters.AddWithValue("@RoleId", arcaneVaultUser.RoleId);
        command.Parameters.AddWithValue("@Password", arcaneVaultUser.Password);

        command.ExecuteNonQuery();

        return NoContent();
    }

    // POST: api/ArcaneVaultUser
    [HttpPost]
    public ActionResult<ArcaneVaultUser> PostArcaneVaultUser(ArcaneVaultUserDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Country))
        {
            return BadRequest("Country is required.");
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"INSERT INTO ArcaneVaultUsers (UserName, Email, IsDeleted, RoleId, Password, Country, IsActive) 
          VALUES (@UserName, @Email, @IsDeleted, @RoleId, @Password, @Country, @IsActive)",
            connection);

        command.Parameters.AddWithValue("@UserName", dto.UserName);
        command.Parameters.AddWithValue("@Email", dto.Email);
        command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);
        command.Parameters.AddWithValue("@RoleId", dto.RoleId);
        command.Parameters.AddWithValue("@Password", dto.Password);
        command.Parameters.AddWithValue("@Country", dto.Country);
        command.Parameters.AddWithValue("@IsActive", true);

        command.ExecuteNonQuery();

        var newUser = new ArcaneVaultUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            IsDeleted = dto.IsDeleted,
            RoleId = dto.RoleId,
            Password = dto.Password,
            Country = dto.Country
        };

        return CreatedAtAction("GetArcaneVaultUser",
            new { username = newUser.UserName },
            newUser);
    }

    // POST: api/ArcaneVaultUser/request-reset-code
    [HttpPost("request-reset-code")]
    public async Task<IActionResult> RequestResetCode(RequestResetCodeDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var userCommand = new SqliteCommand(
            "SELECT Email FROM ArcaneVaultUsers WHERE UserName = @UserName AND IsDeleted = 0",
            connection);
        userCommand.Parameters.AddWithValue("@UserName", dto.UserName);
        var email = userCommand.ExecuteScalar() as string;

        if (email == null) return NotFound("User not found.");

        return await GenerateAndSendResetCodeAsync(connection, dto.UserName, email);
    }

    // Shared logic: rate-limit check, code generation, storage, email send.
    // Returns { userName } so email-initiated callers (Login page) can carry
    // the resolved username forward into verify-reset-code / reset-password,
    // which remain keyed by UserName exactly as before.
    private async Task<IActionResult> GenerateAndSendResetCodeAsync(SqliteConnection connection, string userName, string email)
    {
        var lastRequestCommand = new SqliteCommand(
            @"SELECT CreatedAt FROM PasswordResetCodes
          WHERE UserName = @UserName
          ORDER BY CreatedAt DESC LIMIT 1",
            connection);
        lastRequestCommand.Parameters.AddWithValue("@UserName", userName);
        var lastRequestResult = lastRequestCommand.ExecuteScalar();

        if (lastRequestResult != null)
        {
            var lastRequestedAt = Convert.ToDateTime(lastRequestResult);
            var secondsSinceLastRequest = (DateTime.UtcNow - lastRequestedAt).TotalSeconds;

            if (secondsSinceLastRequest < 120)
            {
                var secondsRemaining = (int)Math.Ceiling(120 - secondsSinceLastRequest);
                return new ObjectResult($"Please wait {secondsRemaining} seconds before requesting another code.")
                { StatusCode = 429 };
            }
        }

        var code = Random.Shared.Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        var invalidateCommand = new SqliteCommand(
            "UPDATE PasswordResetCodes SET IsUsed = 1 WHERE UserName = @UserName AND IsUsed = 0",
            connection);
        invalidateCommand.Parameters.AddWithValue("@UserName", userName);
        invalidateCommand.ExecuteNonQuery();

        var insertCommand = new SqliteCommand(
            @"INSERT INTO PasswordResetCodes (UserName, Code, ExpiresAt, IsUsed, CreatedAt)
          VALUES (@UserName, @Code, @ExpiresAt, 0, @CreatedAt)",
            connection);
        insertCommand.Parameters.AddWithValue("@UserName", userName);
        insertCommand.Parameters.AddWithValue("@Code", code);
        insertCommand.Parameters.AddWithValue("@ExpiresAt", expiresAt);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        insertCommand.ExecuteNonQuery();

        try
        {
            await _emailService.SendPasswordResetCodeAsync(email, userName, code);
        }
        catch
        {
            return StatusCode(500, "Failed to send email.");
        }

        return Ok(new { userName });
    }

    // POST: api/ArcaneVaultUser/verify-reset-code
    [HttpPost("verify-reset-code")]
    public IActionResult VerifyResetCode(VerifyResetCodeDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT ExpiresAt, IsUsed FROM PasswordResetCodes
          WHERE UserName = @UserName AND Code = @Code
          ORDER BY CreatedAt DESC LIMIT 1",
            connection);
        command.Parameters.AddWithValue("@UserName", dto.UserName);
        command.Parameters.AddWithValue("@Code", dto.Code);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return BadRequest("Invalid code.");

        var expiresAt = Convert.ToDateTime(reader["ExpiresAt"]);
        var isUsed = Convert.ToBoolean(reader["IsUsed"]);

        if (isUsed) return BadRequest("This code has already been used.");
        if (DateTime.UtcNow > expiresAt) return BadRequest("This code has expired.");

        return Ok();
    }

    // PATCH: api/ArcaneVaultUser/reset-password
    [HttpPatch("reset-password")]
    public IActionResult ResetPasswordWithCode(ResetPasswordWithCodeDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var codeCommand = new SqliteCommand(
            @"SELECT ResetId, ExpiresAt, IsUsed FROM PasswordResetCodes
          WHERE UserName = @UserName AND Code = @Code
          ORDER BY CreatedAt DESC LIMIT 1",
            connection);
        codeCommand.Parameters.AddWithValue("@UserName", dto.UserName);
        codeCommand.Parameters.AddWithValue("@Code", dto.Code);

        int resetId;
        using (var reader = codeCommand.ExecuteReader())
        {
            if (!reader.Read()) return BadRequest("Invalid code.");

            var isUsed = Convert.ToBoolean(reader["IsUsed"]);
            var expiresAt = Convert.ToDateTime(reader["ExpiresAt"]);

            if (isUsed) return BadRequest("This code has already been used.");
            if (DateTime.UtcNow > expiresAt) return BadRequest("This code has expired.");

            resetId = Convert.ToInt32(reader["ResetId"]);
        }

        // Mark code as used
        var markUsedCommand = new SqliteCommand(
            "UPDATE PasswordResetCodes SET IsUsed = 1 WHERE ResetId = @ResetId",
            connection);
        markUsedCommand.Parameters.AddWithValue("@ResetId", resetId);
        markUsedCommand.ExecuteNonQuery();

        // Update password
        var updatePwdCommand = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET Password = @Password WHERE UserName = @UserName",
            connection);
        updatePwdCommand.Parameters.AddWithValue("@Password", dto.NewPassword);
        updatePwdCommand.Parameters.AddWithValue("@UserName", dto.UserName);
        var rows = updatePwdCommand.ExecuteNonQuery();

        if (rows == 0) return NotFound("User not found.");
        return NoContent();
    }

    // PATCH: api/ArcaneVaultUser/{username}/username — change username
    [HttpPatch("{username}/username")]
    public async Task<IActionResult> ChangeUsername(string username, [FromBody] ChangeUsernameRequest req)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();

        // Check duplicate
        var checkCmd = new SqliteCommand(
            "SELECT COUNT(*) FROM ArcaneVaultUsers WHERE UserName = @NewUserName",
            connection);
        checkCmd.Parameters.AddWithValue("@NewUserName", req.NewUserName);
        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            return Conflict("Username already taken.");

        // Since UserName is PK, insert new + delete old
        using var tx = connection.BeginTransaction();
        try
        {
            // Copy user with new username
            var copyCmd = new SqliteCommand(
                @"INSERT INTO ArcaneVaultUsers 
              (UserName, Email, IsDeleted, RoleId, Password, Country, AvatarUrl, IsActive)
              SELECT @NewUserName, Email, IsDeleted, RoleId, Password, Country, AvatarUrl, IsActive
              FROM ArcaneVaultUsers WHERE UserName = @OldUserName",
                connection, tx);
            copyCmd.Parameters.AddWithValue("@NewUserName", req.NewUserName);
            copyCmd.Parameters.AddWithValue("@OldUserName", username);
            copyCmd.ExecuteNonQuery();

            // Update FKs
            var updateItems = new SqliteCommand(
                "UPDATE CollectionItems SET UserName = @NewUserName WHERE UserName = @OldUserName",
                connection, tx);
            updateItems.Parameters.AddWithValue("@NewUserName", req.NewUserName);
            updateItems.Parameters.AddWithValue("@OldUserName", username);
            updateItems.ExecuteNonQuery();

            var updateShowoffs = new SqliteCommand(
                "UPDATE Showoffs SET UserName = @NewUserName WHERE UserName = @OldUserName",
                connection, tx);
            updateShowoffs.Parameters.AddWithValue("@NewUserName", req.NewUserName);
            updateShowoffs.Parameters.AddWithValue("@OldUserName", username);
            updateShowoffs.ExecuteNonQuery();

            var updateLikes = new SqliteCommand(
                "UPDATE ShowoffLikes SET UserName = @NewUserName WHERE UserName = @OldUserName",
                connection, tx);
            updateLikes.Parameters.AddWithValue("@NewUserName", req.NewUserName);
            updateLikes.Parameters.AddWithValue("@OldUserName", username);
            updateLikes.ExecuteNonQuery();

            var updateLogs = new SqliteCommand(
                "UPDATE LoginLogs SET UserName = @NewUserName WHERE UserName = @OldUserName",
                connection, tx);
            updateLogs.Parameters.AddWithValue("@NewUserName", req.NewUserName);
            updateLogs.Parameters.AddWithValue("@OldUserName", username);
            updateLogs.ExecuteNonQuery();

            // Delete old user
            var deleteCmd = new SqliteCommand(
                "DELETE FROM ArcaneVaultUsers WHERE UserName = @OldUserName",
                connection, tx);
            deleteCmd.Parameters.AddWithValue("@OldUserName", username);
            deleteCmd.ExecuteNonQuery();

            tx.Commit();
            return Ok(new { NewUserName = req.NewUserName });
        }
        catch
        {
            tx.Rollback();
            return StatusCode(500, "Failed to change username.");
        }
    }

    // PATCH: api/ArcaneVaultUser/{username}/email
    [HttpPatch("{username}/email")]
    public IActionResult ChangeEmail(string username, [FromBody] ChangeEmailRequest req)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();

        // Verify password first
        var pwdCheckCmd = new SqliteCommand(
            "SELECT COUNT(*) FROM ArcaneVaultUsers WHERE UserName = @UserName AND Password = @Password",
            connection);
        pwdCheckCmd.Parameters.AddWithValue("@UserName", username);
        pwdCheckCmd.Parameters.AddWithValue("@Password", req.Password);
        if (Convert.ToInt32(pwdCheckCmd.ExecuteScalar()) == 0)
            return BadRequest("Incorrect password.");

        // Check duplicate email
        var checkCmd = new SqliteCommand(
            "SELECT COUNT(*) FROM ArcaneVaultUsers WHERE Email = @Email AND UserName != @UserName",
            connection);
        checkCmd.Parameters.AddWithValue("@Email", req.NewEmail);
        checkCmd.Parameters.AddWithValue("@UserName", username);
        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            return Conflict("Email already in use.");

        var updateCmd = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET Email = @Email WHERE UserName = @UserName",
            connection);
        updateCmd.Parameters.AddWithValue("@Email", req.NewEmail);
        updateCmd.Parameters.AddWithValue("@UserName", username);
        updateCmd.ExecuteNonQuery();

        return Ok();
    }

    // PATCH: api/ArcaneVaultUser/{username}/password
    [HttpPatch("{username}/password")]
    public IActionResult ChangePassword(string username, [FromBody] ChangePasswordRequest req)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();

        // Verify current password
        var checkCmd = new SqliteCommand(
            "SELECT COUNT(*) FROM ArcaneVaultUsers WHERE UserName = @UserName AND Password = @Password",
            connection);
        checkCmd.Parameters.AddWithValue("@UserName", username);
        checkCmd.Parameters.AddWithValue("@Password", req.CurrentPassword);
        if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0)
            return BadRequest("Current password is incorrect.");

        var updateCmd = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET Password = @Password WHERE UserName = @UserName",
            connection);
        updateCmd.Parameters.AddWithValue("@Password", req.NewPassword);
        updateCmd.Parameters.AddWithValue("@UserName", username);
        updateCmd.ExecuteNonQuery();

        return Ok();
    }

    // PATCH: api/ArcaneVaultUser/{username}/avatar
    [HttpPatch("{username}/avatar")]
    public IActionResult UpdateAvatar(string username, [FromBody] UpdateAvatarRequest req)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();

        var updateCmd = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET AvatarUrl = @AvatarUrl WHERE UserName = @UserName",
            connection);
        updateCmd.Parameters.AddWithValue("@AvatarUrl", req.AvatarUrl);
        updateCmd.Parameters.AddWithValue("@UserName", username);
        updateCmd.ExecuteNonQuery();

        return Ok();
    }

    // PATCH: api/ArcaneVaultUser/{username}/country
    [HttpPatch("{username}/country")]
    public IActionResult UpdateCountry(string username, [FromBody] UpdateCountryRequest req)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();

        var updateCmd = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET Country = @Country WHERE UserName = @UserName",
            connection);
        updateCmd.Parameters.AddWithValue("@Country", req.Country);
        updateCmd.Parameters.AddWithValue("@UserName", username);
        updateCmd.ExecuteNonQuery();

        return Ok();
    }

    // PATCH: api/ArcaneVaultUser/{username}/disable
    [HttpPatch("{username}/disable")]
    public IActionResult DisableAccount(string username)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();

        var updateCmd = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET IsDeleted = 1 WHERE UserName = @UserName",
            connection);
        updateCmd.Parameters.AddWithValue("@UserName", username);
        updateCmd.ExecuteNonQuery();

        return Ok();
    }

    // PATCH: api/ArcaneVaultUser/{username}/enable
    [HttpPatch("{username}/enable")]
    public IActionResult EnableAccount(string username)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();

        var updateCmd = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET IsDeleted = 0 WHERE UserName = @UserName",
            connection);
        updateCmd.Parameters.AddWithValue("@UserName", username);
        updateCmd.ExecuteNonQuery();

        return Ok();
    }

    // PATCH: api/ArcaneVaultUser/{username}/active-status — staff only
    [Authorize(Roles = "Staff")]
    [HttpPatch("{username}/active-status")]
    public IActionResult UpdateActiveStatus(string username, UpdateActiveStatusDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "UPDATE ArcaneVaultUsers SET IsActive = @IsActive WHERE UserName = @UserName",
            connection);
        command.Parameters.AddWithValue("@IsActive", dto.IsActive);
        command.Parameters.AddWithValue("@UserName", username);
        var rows = command.ExecuteNonQuery();

        if (rows == 0) return NotFound();

        // Coming active picks up whatever reports piled up in the "loading bay" while no
        // staff were available — see ShowoffReportController.PostReport/AssignPendingReports.
        if (dto.IsActive)
            ShowoffReportController.AssignPendingReports(_connectionString);

        return NoContent();
    }

    // POST: api/ArcaneVaultUser/request-reset-code-by-email
    [HttpPost("request-reset-code-by-email")]
    public async Task<IActionResult> RequestResetCodeByEmail(RequestResetCodeByEmailDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var userCommand = new SqliteCommand(
            "SELECT UserName FROM ArcaneVaultUsers WHERE Email = @Email AND IsDeleted = 0",
            connection);
        userCommand.Parameters.AddWithValue("@Email", dto.Email);
        var userName = userCommand.ExecuteScalar() as string;

        if (userName == null) return NotFound("No account found with that email address.");

        return await GenerateAndSendResetCodeAsync(connection, userName, dto.Email);
    }

    // DELETE: api/ArcaneVaultUser/{username}/delete — hard delete
    [HttpDelete("{username}/delete")]
    public async Task<IActionResult> DeleteAccountPermanent(string username)
    {
        using var connection = new SqliteConnection("Data Source=ArcaneVault.db");
        connection.Open();
        using var tx = connection.BeginTransaction();

        try
        {
            // Delete showoff images from Cloudinary handled on RP side before calling this

            // Delete all related records
            foreach (var sql in new[]
            {
            "DELETE FROM ShowoffLikes    WHERE UserName  = @UserName",
            "DELETE FROM ShowoffViews    WHERE ViewerUserName = @UserName",
            "DELETE FROM LoginLogs       WHERE UserName  = @UserName",
            "DELETE FROM SearchLogs      WHERE UserName  = @UserName",
            "DELETE FROM ShowoffItems    WHERE ShowoffId IN (SELECT ShowoffId FROM Showoffs WHERE UserName = @UserName)",
            "DELETE FROM Showoffs        WHERE UserName  = @UserName",
            "DELETE FROM CollectionItemCategories WHERE ItemId IN (SELECT ItemId FROM CollectionItems WHERE UserName = @UserName)",
            "DELETE FROM CollectionItems WHERE UserName  = @UserName",
            "DELETE FROM ArcaneVaultUsers WHERE UserName = @UserName"
        })
            {
                var cmd = new SqliteCommand(sql, connection, tx);
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return Ok();
        }
        catch
        {
            tx.Rollback();
            return StatusCode(500, "Failed to delete account.");
        }
    }

    public class ChangeUsernameRequest { public string NewUserName { get; set; } }
    public class ChangeEmailRequest { public string NewEmail { get; set; } public string Password { get; set; } }
    public class ChangePasswordRequest { public string CurrentPassword { get; set; } public string NewPassword { get; set; } }
    public class UpdateAvatarRequest { public string AvatarUrl { get; set; } }
    public class UpdateCountryRequest { public string Country { get; set; } }
    public class RequestResetCodeByEmailDTO { public string Email { get; set; } }
}