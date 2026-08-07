using ArcaneVaultAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Staff")]
public class CosmeticAuditLogController : ControllerBase
{
    private readonly string _connectionString = "Data Source=ArcaneVault.db";

    // GET: api/CosmeticAuditLog
    // Recomputes and verifies the hash chain on every read — once a row's own
    // hash or its link to the previous row's stored hash fails to check out,
    // that row and everything after it is reported as Tampered.
    [HttpGet]
    public ActionResult<IEnumerable<CosmeticAuditLogEntry>> GetLog()
    {
        var entries = new List<CosmeticAuditLogEntry>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = new SqliteCommand(
            @"SELECT LogId, AdminId, AdminName, Timestamp, CosmeticId, CosmeticName, CosmeticType, Action, EntryHash, PreviousEntryHash
              FROM CosmeticAuditLog ORDER BY LogId ASC",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            entries.Add(new CosmeticAuditLogEntry
            {
                LogId = Convert.ToInt32(reader["LogId"]),
                AdminId = reader["AdminId"].ToString(),
                AdminName = reader["AdminName"].ToString(),
                Timestamp = reader["Timestamp"].ToString(),
                CosmeticId = reader["CosmeticId"].ToString(),
                CosmeticName = reader["CosmeticName"].ToString(),
                CosmeticType = reader["CosmeticType"].ToString(),
                Action = reader["Action"].ToString(),
                EntryHash = reader["EntryHash"].ToString(),
                PreviousEntryHash = reader["PreviousEntryHash"].ToString()
            });
        }

        var chainBroken = false;
        var actualPreviousHash = "0";
        foreach (var entry in entries)
        {
            var expectedHash = CosmeticAuditLogger.ComputeHash(
                entry.AdminId, entry.AdminName, entry.Timestamp,
                entry.CosmeticId, entry.CosmeticName, entry.CosmeticType,
                entry.Action, actualPreviousHash);

            if (entry.PreviousEntryHash != actualPreviousHash || entry.EntryHash != expectedHash)
            {
                chainBroken = true;
            }

            entry.Status = chainBroken ? "Tampered" : "Verified";
            actualPreviousHash = entry.EntryHash;
        }

        entries.Reverse(); // newest first for display
        return entries;
    }
}
