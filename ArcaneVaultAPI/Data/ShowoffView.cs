using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ShowoffView
    {
        [Key]
        public int ViewId { get; set; }
        public int ShowoffId { get; set; }
        public string? ViewerUserName { get; set; }
        public DateTime ViewedAt { get; set; }

        [ForeignKey("ShowoffId")]
        public Showoff? Showoff { get; set; }
    }
}