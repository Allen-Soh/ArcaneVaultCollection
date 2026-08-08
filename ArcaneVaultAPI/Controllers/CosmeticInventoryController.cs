using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace ArcaneVaultAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CosmeticInventoryController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=ArcaneVault.db";

        public class InventoryItemResult
        {
            public string Uid { get; set; }
            public string CosmeticId { get; set; }
            public string CosmeticName { get; set; }
            public string CosmeticType { get; set; }
            public string ImagePath { get; set; }
            public bool IsLimited { get; set; }
            public int? CopyNumber { get; set; }
            public int? LimitedQuantity { get; set; }
            public string AcquiredAt { get; set; }
        }

        // GET: api/CosmeticInventory/user/{username}
        [HttpGet("user/{username}")]
        public ActionResult<IEnumerable<InventoryItemResult>> GetForUser(string username)
        {
            var callerUserName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.Equals(callerUserName, username, StringComparison.OrdinalIgnoreCase))
                return Forbid();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand(
                "SELECT Uid, CosmeticId, CosmeticType, AcquiredAt FROM CosmeticInventory WHERE UserName = @UserName ORDER BY AcquiredAt DESC",
                connection);
            command.Parameters.AddWithValue("@UserName", username);

            var rows = new List<(string Uid, string CosmeticId, string CosmeticType, string AcquiredAt)>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add((
                        reader["Uid"].ToString(),
                        reader["CosmeticId"].ToString(),
                        reader["CosmeticType"].ToString(),
                        reader["AcquiredAt"].ToString()
                    ));
                }
            }

            var results = new List<InventoryItemResult>();
            foreach (var row in rows)
            {
                string name, imagePath;
                bool isLimited;
                int? limitedQuantity;

                if (row.CosmeticType == "Frame")
                {
                    var cmd = new SqliteCommand(
                        "SELECT FrameName, ImagePath, IsLimited, LimitedQuantity FROM Frames WHERE FrameCode = @Code",
                        connection);
                    cmd.Parameters.AddWithValue("@Code", row.CosmeticId);
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read()) continue; // Cosmetic was hard-deleted since purchase — skip
                    name = reader["FrameName"].ToString();
                    imagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString();
                    isLimited = Convert.ToBoolean(Convert.ToInt32(reader["IsLimited"]));
                    limitedQuantity = reader["LimitedQuantity"] == DBNull.Value ? null : Convert.ToInt32(reader["LimitedQuantity"]);
                }
                else
                {
                    var cmd = new SqliteCommand(
                        "SELECT StickerName, ImagePath, IsLimited, LimitedQuantity FROM Stickers WHERE StickerCode = @Code",
                        connection);
                    cmd.Parameters.AddWithValue("@Code", row.CosmeticId);
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read()) continue;
                    name = reader["StickerName"].ToString();
                    imagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString();
                    isLimited = Convert.ToBoolean(Convert.ToInt32(reader["IsLimited"]));
                    limitedQuantity = reader["LimitedQuantity"] == DBNull.Value ? null : Convert.ToInt32(reader["LimitedQuantity"]);
                }

                int? copyNumber = null;
                if (isLimited)
                {
                    // 1-based rank of this copy's AcquiredAt among all copies of the same cosmetic ever sold.
                    var rankCmd = new SqliteCommand(
                        @"SELECT COUNT(*) FROM CosmeticInventory
                          WHERE CosmeticId = @CosmeticId AND CosmeticType = @CosmeticType AND AcquiredAt <= @AcquiredAt",
                        connection);
                    rankCmd.Parameters.AddWithValue("@CosmeticId", row.CosmeticId);
                    rankCmd.Parameters.AddWithValue("@CosmeticType", row.CosmeticType);
                    rankCmd.Parameters.AddWithValue("@AcquiredAt", row.AcquiredAt);
                    copyNumber = Convert.ToInt32(rankCmd.ExecuteScalar());
                }

                results.Add(new InventoryItemResult
                {
                    Uid = row.Uid,
                    CosmeticId = row.CosmeticId,
                    CosmeticName = name,
                    CosmeticType = row.CosmeticType,
                    ImagePath = imagePath,
                    IsLimited = isLimited,
                    CopyNumber = copyNumber,
                    LimitedQuantity = limitedQuantity,
                    AcquiredAt = row.AcquiredAt
                });
            }

            return results;
        }
    }
}
