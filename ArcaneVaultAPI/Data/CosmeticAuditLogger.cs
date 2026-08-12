//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07


using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace ArcaneVaultAPI.Data
{
    // Appends a hash-chained row to CosmeticAuditLog on every Manage Shop
    // Create/Edit/Delist/Relist action, so tampering with any prior entry
    // breaks the chain and is detectable on read.
    public static class CosmeticAuditLogger
    {
        public static void Log(
            string connectionString,
            string adminId,
            string adminName,
            string cosmeticId,
            string cosmeticName,
            string cosmeticType,
            string action)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var lastHashCommand = new SqliteCommand(
                "SELECT EntryHash FROM CosmeticAuditLog ORDER BY LogId DESC LIMIT 1",
                connection);
            var previousEntryHash = lastHashCommand.ExecuteScalar() as string ?? "0";

            var timestamp = DateTime.UtcNow.ToString("o");
            var entryHash = ComputeHash(adminId, adminName, timestamp, cosmeticId, cosmeticName, cosmeticType, action, previousEntryHash);

            var insertCommand = new SqliteCommand(
                @"INSERT INTO CosmeticAuditLog
                  (AdminId, AdminName, Timestamp, CosmeticId, CosmeticName, CosmeticType, Action, EntryHash, PreviousEntryHash)
                  VALUES (@AdminId, @AdminName, @Timestamp, @CosmeticId, @CosmeticName, @CosmeticType, @Action, @EntryHash, @PreviousEntryHash)",
                connection);
            insertCommand.Parameters.AddWithValue("@AdminId", adminId);
            insertCommand.Parameters.AddWithValue("@AdminName", adminName);
            insertCommand.Parameters.AddWithValue("@Timestamp", timestamp);
            insertCommand.Parameters.AddWithValue("@CosmeticId", cosmeticId);
            insertCommand.Parameters.AddWithValue("@CosmeticName", cosmeticName);
            insertCommand.Parameters.AddWithValue("@CosmeticType", cosmeticType);
            insertCommand.Parameters.AddWithValue("@Action", action);
            insertCommand.Parameters.AddWithValue("@EntryHash", entryHash);
            insertCommand.Parameters.AddWithValue("@PreviousEntryHash", previousEntryHash);
            insertCommand.ExecuteNonQuery();
        }

        public static string ComputeHash(
            string adminId, string adminName, string timestamp,
            string cosmeticId, string cosmeticName, string cosmeticType,
            string action, string previousEntryHash)
        {
            var raw = string.Join('|', adminId, adminName, timestamp, cosmeticId, cosmeticName, cosmeticType, action, previousEntryHash);
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
