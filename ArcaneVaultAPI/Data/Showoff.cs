using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class Showoff
    {
        [Key]
        public int ShowoffId { get; set; }

        [MaxLength(40)]
        public string ShowoffName { get; set; }

        public string BackgroundColor { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; }

        [ForeignKey("UserName")]
        public ArcaneVaultUser? User { get; set; }
    }
}