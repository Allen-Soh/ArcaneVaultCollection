//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ShowoffAppeal
    {
        [Key]
        public int AppealId { get; set; }
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string UserName { get; set; }
        public string AppealMessage { get; set; }
        public string Status { get; set; } // "Pending", "Approved", "Denied"
        public string? StaffNote { get; set; }
        public string SubmittedAt { get; set; }
        public string? DecidedAt { get; set; }

        [ForeignKey("ShowoffId")]
        public Showoff? Showoff { get; set; }
    }
}
