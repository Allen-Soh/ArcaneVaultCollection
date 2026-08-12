//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class JwtTokenController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    public JwtTokenController(IConfiguration config)
    {
        _config = config;
    }

    // POST: api/JwtToken — generate JWT for valid credentials
    [HttpPost]
    public IActionResult GenerateToken([FromBody] JwtLoginRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Validate credentials
        var command = new SqliteCommand(
            @"SELECT UserName, Email, RoleId, IsDeleted
              FROM ArcaneVaultUsers
              WHERE UserName = @UserName AND Password = @Password",
            connection);
        command.Parameters.AddWithValue("@UserName", request.UserName);
        command.Parameters.AddWithValue("@Password", request.Password);

        string? userName = null;
        string? email = null;
        int roleId = 0;
        bool isDeleted = false;

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            userName = reader["UserName"].ToString();
            email = reader["Email"].ToString();
            roleId = Convert.ToInt32(reader["RoleId"]);
            isDeleted = Convert.ToBoolean(reader["IsDeleted"]);
        }

        if (userName == null)
            return Unauthorized("Invalid username or password.");

        if (isDeleted)
            return Unauthorized("Account is disabled.");

        // Map RoleId to role name
        var roleName = roleId == 1 ? "Staff" : "User";

        // Build claims
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name,  userName),
            new Claim(ClaimTypes.Email, email ?? ""),
            new Claim(ClaimTypes.Role,  roleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Generate token
        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var tokenExpiration = DateTime.UtcNow.AddHours(3);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            expires: tokenExpiration,
            claims: authClaims,
            signingCredentials: new SigningCredentials(
                authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new JwtTokenResponse
        {
            Token = tokenString,
            TokenExpiration = tokenExpiration,
            UserName = userName,
            Email = email,
            RoleName = roleName
        });
    }
}

public class JwtLoginRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class JwtTokenResponse
{
    public string Token { get; set; }
    public DateTime TokenExpiration { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? RoleName { get; set; }
}