namespace ArcaneVaultRP.Models
{
    public class ShowoffReport
    {
        public int ReportId { get; set; }
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string OwnerUserName { get; set; }
        public string ReporterUserName { get; set; }
        public string AssignedStaffUserName { get; set; }
        public string Category { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsResolved { get; set; }
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }
    }
}