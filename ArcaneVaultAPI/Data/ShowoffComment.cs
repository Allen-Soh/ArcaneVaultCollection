//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ShowoffComment
    {
        [Key]
        public int CommentId { get; set; }

        public int ShowoffId { get; set; }
        public string UserName { get; set; }

        [MaxLength(500)]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("ShowoffId")]
        public Showoff? Showoff { get; set; }

        [ForeignKey("UserName")]
        public ArcaneVaultUser? User { get; set; }
    }
}
