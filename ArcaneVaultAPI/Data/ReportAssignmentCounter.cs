//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

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
