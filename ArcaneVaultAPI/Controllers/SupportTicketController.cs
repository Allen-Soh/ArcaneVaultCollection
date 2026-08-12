//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ArcaneVaultAPI.DataTransferObject;
using ArcaneVaultAPI.Services;

[Route("api/[controller]")]
[ApiController]
public class SupportTicketController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";
    private readonly IEmailService _emailService;

    public SupportTicketController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    // GET: api/SupportTicket — ALL tickets, unfiltered by staff assignment/active status
    [HttpGet]
    public ActionResult<IEnumerable<SupportTicketDTO>> GetTickets()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT TicketId, TicketUid, Name, Email, Subject, Issue, IsResolved, AssignedStaffUserName, CreatedAt
              FROM SupportTickets
              ORDER BY CreatedAt DESC",
            connection);

        return Ok(ReadTicketDTOs(command));
    }

    // GET: api/SupportTicket/{id} — for the Review modal
    [HttpGet("{id}")]
    public ActionResult<SupportTicketDTO> GetTicket(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT TicketId, TicketUid, Name, Email, Subject, Issue, IsResolved, AssignedStaffUserName, CreatedAt
              FROM SupportTickets WHERE TicketId = @TicketId",
            connection);
        command.Parameters.AddWithValue("@TicketId", id);

        var ticket = ReadTicketDTOs(command).FirstOrDefault();
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    // POST: api/SupportTicket — user submits a ticket
    [HttpPost]
    public async Task<ActionResult<SupportTicketDTO>> PostTicket(CreateSupportTicketDTO dto)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Round-robin assign among active, non-deleted staff (RoleId = 1)
        string? assignedStaff = GetNextStaffAssignee(connection);

        var insertCommand = new SqliteCommand(
            @"INSERT INTO SupportTickets (TicketUid, Name, Email, Subject, Issue, IsResolved, AssignedStaffUserName, CreatedAt, UserName)
              VALUES ('', @Name, @Email, @Subject, @Issue, 0, @AssignedStaff, @CreatedAt, @UserName);
              SELECT last_insert_rowid();",
            connection);
        insertCommand.Parameters.AddWithValue("@Name", dto.Name);
        insertCommand.Parameters.AddWithValue("@Email", dto.Email);
        insertCommand.Parameters.AddWithValue("@Subject", dto.Subject);
        insertCommand.Parameters.AddWithValue("@Issue", dto.Issue);
        insertCommand.Parameters.AddWithValue("@AssignedStaff", (object?)assignedStaff ?? DBNull.Value);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        insertCommand.Parameters.AddWithValue("@UserName", (object?)dto.UserName ?? DBNull.Value);

        var newId = Convert.ToInt32(insertCommand.ExecuteScalar());
        var ticketUid = $"CS-{newId:D6}";

        var updateUidCommand = new SqliteCommand(
            "UPDATE SupportTickets SET TicketUid = @Uid WHERE TicketId = @TicketId",
            connection);
        updateUidCommand.Parameters.AddWithValue("@Uid", ticketUid);
        updateUidCommand.Parameters.AddWithValue("@TicketId", newId);
        updateUidCommand.ExecuteNonQuery();

        // Send confirmation email — failure here shouldn't block ticket creation
        try
        {
            await _emailService.SendTicketConfirmationAsync(dto.Email, dto.Name, ticketUid, dto.Subject);
        }
        catch
        {
            // Log this in a real system — email failure shouldn't fail the whole request
        }

        return Ok(new { TicketId = newId, TicketUid = ticketUid });
    }

    // PATCH: api/SupportTicket/{id}/resolve
    [HttpPatch("{id}/resolve")]
    public IActionResult ResolveTicket(int id)
    {
        return SetResolved(id, true);
    }

    // PATCH: api/SupportTicket/{id}/unresolve
    [HttpPatch("{id}/unresolve")]
    public IActionResult UnresolveTicket(int id)
    {
        return SetResolved(id, false);
    }

    private IActionResult SetResolved(int id, bool resolved)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "UPDATE SupportTickets SET IsResolved = @IsResolved WHERE TicketId = @TicketId",
            connection);
        command.Parameters.AddWithValue("@IsResolved", resolved);
        command.Parameters.AddWithValue("@TicketId", id);
        var rows = command.ExecuteNonQuery();

        if (rows == 0) return NotFound();
        return NoContent();
    }

    // Round-robin assignment among active staff — mirrors the Reports assignment pattern
    private string? GetNextStaffAssignee(SqliteConnection connection)
    {
        var staffCommand = new SqliteCommand(
            @"SELECT UserName FROM ArcaneVaultUsers
              WHERE RoleId = 1 AND IsDeleted = 0 AND IsActive = 1
              ORDER BY UserName",
            connection);

        var staffList = new List<string>();
        using (var reader = staffCommand.ExecuteReader())
        {
            while (reader.Read())
                staffList.Add(reader["UserName"].ToString());
        }

        if (staffList.Count == 0) return null; // no active staff — leave unassigned

        var counterCommand = new SqliteCommand(
            "SELECT Counter FROM TicketAssignmentCounters WHERE Id = 1", connection);
        var counter = Convert.ToInt32(counterCommand.ExecuteScalar() ?? 0);

        var assignee = staffList[counter % staffList.Count];

        var updateCounterCommand = new SqliteCommand(
            "UPDATE TicketAssignmentCounters SET Counter = @Counter WHERE Id = 1", connection);
        updateCounterCommand.Parameters.AddWithValue("@Counter", counter + 1);
        updateCounterCommand.ExecuteNonQuery();

        return assignee;
    }

    private List<SupportTicketDTO> ReadTicketDTOs(SqliteCommand command)
    {
        var tickets = new List<SupportTicketDTO>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tickets.Add(new SupportTicketDTO
            {
                TicketId = Convert.ToInt32(reader["TicketId"]),
                TicketUid = reader["TicketUid"].ToString(),
                Name = reader["Name"].ToString(),
                Email = reader["Email"].ToString(),
                Subject = reader["Subject"].ToString(),
                Issue = reader["Issue"].ToString(),
                IsResolved = Convert.ToBoolean(reader["IsResolved"]),
                AssignedStaffUserName = reader["AssignedStaffUserName"] == DBNull.Value ? null : reader["AssignedStaffUserName"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            });
        }
        return tickets;
    }

    // GET: api/SupportTicket/staff/{username} — tickets assigned to a specific staff member
    [HttpGet("staff/{username}")]
    public ActionResult<IEnumerable<SupportTicketDTO>> GetStaffTickets(string username)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT TicketId, TicketUid, Name, Email, Subject, Issue, IsResolved, AssignedStaffUserName, CreatedAt
          FROM SupportTickets
          WHERE AssignedStaffUserName = @StaffUserName
          ORDER BY CreatedAt DESC",
            connection);
        command.Parameters.AddWithValue("@StaffUserName", username);

        return Ok(ReadTicketDTOs(command));
    }

    // POST: api/SupportTicket/{id}/reply — staff replies via centralized email, marks resolved
    [HttpPost("{id}/reply")]
    public async Task<IActionResult> ReplyToTicket(int id, [FromBody] ReplyToTicketDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest("Reply message is required.");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "SELECT TicketUid, Name, Email, Subject FROM SupportTickets WHERE TicketId = @TicketId",
            connection);
        command.Parameters.AddWithValue("@TicketId", id);

        string? ticketUid = null, name = null, email = null, subject = null;
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read()) return NotFound();
            ticketUid = reader["TicketUid"].ToString();
            name = reader["Name"].ToString();
            email = reader["Email"].ToString();
            subject = reader["Subject"].ToString();
        }

        try
        {
            await _emailService.SendTicketReplyAsync(email, name, ticketUid, subject, dto.Message);
        }
        catch
        {
            return StatusCode(500, "Failed to send reply email.");
        }

        var resolveCommand = new SqliteCommand(
            "UPDATE SupportTickets SET IsResolved = 1, RepliedAt = @RepliedAt WHERE TicketId = @TicketId",
            connection);
        resolveCommand.Parameters.AddWithValue("@RepliedAt", DateTime.UtcNow);
        resolveCommand.Parameters.AddWithValue("@TicketId", id);
        resolveCommand.ExecuteNonQuery();

        return Ok();
    }

    public class ReplyToTicketDTO { public string Message { get; set; } }
}