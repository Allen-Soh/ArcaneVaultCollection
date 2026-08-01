using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ShowoffReport
    {
        [Key]
        public int ReportId { get; set; }

        public int ShowoffId { get; set; }
        public string ReporterUserName { get; set; }
        public string AssignedStaffUserName { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("ShowoffId")]
        public Showoff? Showoff { get; set; }

        [ForeignKey("ReporterUserName")]
        public ArcaneVaultUser? Reporter { get; set; }
    }
}
