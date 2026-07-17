using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class SearchLog
    {
        [Key]
        public int LogId { get; set; }
        public string SearchTerm { get; set; }
        public string SearchType { get; set; } // "User", "Item", "Category"
        public string? UserName { get; set; }
        public DateTime SearchedAt { get; set; }

        [ForeignKey("UserName")]
        public ArcaneVaultUser? User { get; set; }
    }
}