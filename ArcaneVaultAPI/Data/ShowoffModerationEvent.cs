//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ShowoffModerationEvent
    {
        [Key]
        public int EventId { get; set; }
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string EventType { get; set; } // "Disabled" or "Enabled"
        public string CreatedAt { get; set; }

        [ForeignKey("ShowoffId")]
        public Showoff? Showoff { get; set; }
    }
}
