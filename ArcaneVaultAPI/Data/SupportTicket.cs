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
    }

    public class TicketAssignmentCounter
    {
        public int Id { get; set; }
        public int Counter { get; set; }
    }
}