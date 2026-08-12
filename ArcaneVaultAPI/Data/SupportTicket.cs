//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class SupportTicket
    {
        [Key]
        public int TicketId { get; set; }
        public string TicketUid { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Issue { get; set; }
        public bool IsResolved { get; set; }
        public string? AssignedStaffUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserName { get; set; }
        public string? RepliedAt { get; set; }
    }

    public class TicketAssignmentCounter
    {
        public int Id { get; set; }
        public int Counter { get; set; }
    }
}