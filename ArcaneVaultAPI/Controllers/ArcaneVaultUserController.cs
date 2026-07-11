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
            "SELECT UserName, Email, IsDeleted, RoleId, Password FROM ArcaneVaultUsers",
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
                Password = reader["Password"].ToString()
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
            "SELECT UserName, Email, IsDeleted, RoleId, Password FROM ArcaneVaultUsers WHERE UserName = @UserName",
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
                Password = reader["Password"].ToString()
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
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"INSERT INTO ArcaneVaultUsers (UserName, Email, IsDeleted, RoleId, Password) 
              VALUES (@UserName, @Email, @IsDeleted, @RoleId, @Password)",
            connection);

        command.Parameters.AddWithValue("@UserName", dto.UserName);
        command.Parameters.AddWithValue("@Email", dto.Email);
        command.Parameters.AddWithValue("@IsDeleted", dto.IsDeleted);
        command.Parameters.AddWithValue("@RoleId", dto.RoleId);
        command.Parameters.AddWithValue("@Password", dto.Password);

        command.ExecuteNonQuery();

        var newUser = new ArcaneVaultUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            IsDeleted = dto.IsDeleted,
            RoleId = dto.RoleId,
            Password = dto.Password
        };

        return CreatedAtAction("GetArcaneVaultUser",
            new { username = newUser.UserName },
            newUser);
    }

    // DELETE: api/ArcaneVaultUser/5
    [HttpDelete("{username}")]
    public IActionResult DeleteArcaneVaultUser(string? username)
    {
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
            "DELETE FROM ArcaneVaultUsers WHERE UserName = @UserName",
            connection);
        command.Parameters.AddWithValue("@UserName", username);
        command.ExecuteNonQuery();

        return NoContent();
    }
}