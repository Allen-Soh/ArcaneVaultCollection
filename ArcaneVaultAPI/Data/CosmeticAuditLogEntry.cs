using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class CosmeticAuditLogEntry
    {
        [Key]
        public int LogId { get; set; }
        public string AdminId { get; set; }
        public string AdminName { get; set; }
        public string Timestamp { get; set; }
        public string CosmeticId { get; set; }
        public string CosmeticName { get; set; }
        public string CosmeticType { get; set; }
        public string Action { get; set; }
        public string EntryHash { get; set; }
        public string PreviousEntryHash { get; set; }

        // Computed on read by CosmeticAuditLogController — not a DB column.
        [NotMapped]
        public string Status { get; set; }
    }
}
