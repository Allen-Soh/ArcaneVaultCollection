//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ShowoffLike
    {
        public int ShowoffId { get; set; }
        public string UserName { get; set; }

        [ForeignKey("ShowoffId")]
        public Showoff? Showoff { get; set; }

        [ForeignKey("UserName")]
        public ArcaneVaultUser? User { get; set; }
    }
}