using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class ReportAssignmentCounter
    {
        [Key]
        public int Id { get; set; } // always 1
        public int Counter { get; set; }
    }
}
