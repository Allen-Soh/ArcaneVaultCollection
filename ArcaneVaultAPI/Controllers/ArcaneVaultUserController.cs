using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ArcaneVaultAPI.Data;
using ArcaneVaultAPI.DataTransferObject;

[Route("api/[controller]")]
[ApiController]
public class ArcaneVaultUserController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/ArcaneVaultUser
    [HttpGet]
    public ActionResult<IEnumerable<ArcaneVaultUser>> GetArcaneVaultUser()
    {
        var users = new List<ArcaneVaultUser>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT UserName, Email, IsDeleted, RoleId, Password, Country, AvatarUrl FROM ArcaneVaultUsers",
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
                Password = reader["Password"].ToString(),
                Country = reader["Country"] == DBNull.Value ? null : reader["Country"].ToString(),
                AvatarUrl = reader["AvatarUrl"] == DBNull.Value ? null : reader["AvatarUrl"].ToString()
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
            "SELECT UserName, Email, IsDeleted, RoleId, Password, Country, AvatarUrl FROM ArcaneVaultUsers WHERE UserName = @UserName",
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
                Password = reader["Password"].ToString(),
                Country = reader["Country"] == DBNull.Value ? null : reader["Country"].ToString(),
                AvatarUrl = reader["AvatarUrl"] == DBNull.Value ? null : reader["AvatarUrl"].ToString()
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
            @"INSERT INTO ArcaneVaultUsers (UserName, Email, IsDeleted, RoleId, Password, Country) 
          VALUES (@UserName, @Email, @IsDeleted, @RoleId, @Password, @Country)",
            connection);

        command.Parameters.AddWithValue("@UserName", dto.UserName);
        command.Parameters.AddWithValue("@Email", dto.Email);
        command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);
        command.Parameters.AddWithValue("@RoleId", dto.RoleId);
        command.Parameters.AddWithValue("@Password", dto.Password);
        command.Parameters.AddWithValue("@Country", dto.Country);

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
              (UserName, Email, IsDeleted, RoleId, Password, Country, AvatarUrl)
              SELECT @NewUserName, Email, IsDeleted, RoleId, Password, Country, AvatarUrl
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
    public class ChangeEmailRequest { public string NewEmail { get; set; } }
    public class ChangePasswordRequest { public string CurrentPassword { get; set; } public string NewPassword { get; set; } }
    public class UpdateAvatarRequest { public string AvatarUrl { get; set; } }
    public class UpdateCountryRequest { public string Country { get; set; } }
}