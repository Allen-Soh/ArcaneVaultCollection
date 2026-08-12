//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using ArcaneVaultAPI.DataTransferObject;

[Route("api/[controller]")]
[ApiController]
public class ShowoffReportController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/ShowoffReport — all reports (staff use), grouped by showoff
    [HttpGet]
    public ActionResult GetAllReports()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var reports = ReadReports(connection, null);
        return Ok(reports);
    }

    // GET: api/ShowoffReport/staff/{username} — reports assigned to a specific staff
    [HttpGet("staff/{username}")]
    public ActionResult GetStaffReports(string username)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var reports = ReadReports(connection, username);
        return Ok(reports);
    }

    // POST: api/ShowoffReport — submit a report with Sequential Looping Assignment
    [HttpPost]
    public IActionResult PostReport([FromBody] CreateShowoffReportDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Category))
            return BadRequest("Report category is required.");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // 1. Get all staff users (RoleId = 1) ordered by UserName
        var staffCommand = new SqliteCommand(
            "SELECT UserName FROM ArcaneVaultUsers WHERE RoleId = 1 AND IsDeleted = 0 AND IsActive = 1 ORDER BY UserName",
            connection);
        var staffList = new List<string>();
        using (var reader = staffCommand.ExecuteReader())
        {
            while (reader.Read())
                staffList.Add(reader["UserName"].ToString());
        }

        if (staffList.Count == 0)
            return BadRequest("No staff available to handle reports.");

        // 2. Get or create the assignment counter
        var counterCommand = new SqliteCommand(
            "SELECT Counter FROM ReportAssignmentCounters WHERE Id = 1",
            connection);
        var counterResult = counterCommand.ExecuteScalar();
        int counter;

        if (counterResult == null)
        {
            var createCounter = new SqliteCommand(
                "INSERT INTO ReportAssignmentCounters (Id, Counter) VALUES (1, 0)",
                connection);
            createCounter.ExecuteNonQuery();
            counter = 0;
        }
        else
        {
            counter = Convert.ToInt32(counterResult);
        }

        // 3. Assign staff via round-robin
        string assignedStaff = staffList[counter % staffList.Count];

        // 4. Increment counter
        var updateCounter = new SqliteCommand(
            "UPDATE ReportAssignmentCounters SET Counter = @Counter WHERE Id = 1",
            connection);
        updateCounter.Parameters.AddWithValue("@Counter", counter + 1);
        updateCounter.ExecuteNonQuery();

        // 5. Insert the report
        var insertCommand = new SqliteCommand(
            @"INSERT INTO ShowoffReports (ShowoffId, ReporterUserName, AssignedStaffUserName, Category, Reason, CreatedAt, IsResolved)
              VALUES (@ShowoffId, @ReporterUserName, @AssignedStaffUserName, @Category, @Reason, @CreatedAt, 0)",
            connection);
        insertCommand.Parameters.AddWithValue("@ShowoffId", dto.ShowoffId);
        insertCommand.Parameters.AddWithValue("@ReporterUserName", dto.ReporterUserName);
        insertCommand.Parameters.AddWithValue("@AssignedStaffUserName", assignedStaff);
        insertCommand.Parameters.AddWithValue("@Category", dto.Category);
        insertCommand.Parameters.AddWithValue("@Reason", dto.Reason ?? (object)DBNull.Value);
        insertCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        insertCommand.ExecuteNonQuery();

        return Ok(new { AssignedStaff = assignedStaff });
    }

    // PATCH: api/ShowoffReport/{showoffId}/resolve — marks ALL reports for this showoff as resolved
    [HttpPatch("{showoffId}/resolve")]
    public IActionResult ResolveReports(int showoffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "UPDATE ShowoffReports SET IsResolved = 1 WHERE ShowoffId = @ShowoffId",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", showoffId);
        command.ExecuteNonQuery();

        return Ok();
    }

    // PATCH: api/ShowoffReport/{showoffId}/unresolve — reopens ALL reports for this showoff
    [HttpPatch("{showoffId}/unresolve")]
    public IActionResult UnresolveReports(int showoffId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            "UPDATE ShowoffReports SET IsResolved = 0 WHERE ShowoffId = @ShowoffId",
            connection);
        command.Parameters.AddWithValue("@ShowoffId", showoffId);
        command.ExecuteNonQuery();

        return Ok();
    }

    // Helper — read reports, optionally filtered by assigned staff
    private List<ShowoffReportDTO> ReadReports(SqliteConnection connection, string? staffUsername)
    {
        var sql = @"SELECT sr.ReportId, sr.ShowoffId, sr.ReporterUserName, sr.AssignedStaffUserName,
                           sr.Category, sr.Reason, sr.CreatedAt, sr.IsResolved,
                           s.ShowoffName, s.UserName as OwnerUserName, s.IsDisabled,
                           (SELECT COUNT(*) FROM ShowoffLikes sl WHERE sl.ShowoffId = s.ShowoffId) as LikeCount,
                           (SELECT COUNT(*) FROM ShowoffViews sv WHERE sv.ShowoffId = s.ShowoffId) as ViewCount
                    FROM ShowoffReports sr
                    JOIN Showoffs s ON sr.ShowoffId = s.ShowoffId
                    WHERE s.IsDeleted = 0";

        if (!string.IsNullOrEmpty(staffUsername))
            sql += " AND sr.AssignedStaffUserName = @StaffUserName";

        sql += " ORDER BY sr.CreatedAt DESC";

        var command = new SqliteCommand(sql, connection);
        if (!string.IsNullOrEmpty(staffUsername))
            command.Parameters.AddWithValue("@StaffUserName", staffUsername);

        var reports = new List<ShowoffReportDTO>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            reports.Add(new ShowoffReportDTO
            {
                ReportId = Convert.ToInt32(reader["ReportId"]),
                ShowoffId = Convert.ToInt32(reader["ShowoffId"]),
                ShowoffName = reader["ShowoffName"].ToString(),
                OwnerUserName = reader["OwnerUserName"].ToString(),
                ReporterUserName = reader["ReporterUserName"].ToString(),
                AssignedStaffUserName = reader["AssignedStaffUserName"].ToString(),
                Category = reader["Category"].ToString(),
                Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                IsDisabled = Convert.ToBoolean(reader["IsDisabled"]),
                IsResolved = Convert.ToBoolean(reader["IsResolved"]),
                LikeCount = Convert.ToInt32(reader["LikeCount"]),
                ViewCount = Convert.ToInt32(reader["ViewCount"])
            });
        }

        return reports;
    }
}