//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class LoginLog
    {
        [Key]
        public int LogId { get; set; }
        public string UserName { get; set; }
        public DateTime LoggedInAt { get; set; }

        [ForeignKey("UserName")]
        public ArcaneVaultUser? User { get; set; }
    }
}