//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultRP.Models
{
    public class CosmeticAuditLogEntry
    {
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
        public string Status { get; set; }
    }
}
