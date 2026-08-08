namespace ArcaneVaultAPI.DataTransferObject
{
    public class ArcaneVaultUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class UpdateActiveStatusDTO
    {
        public bool IsActive { get; set; }
    }

    public class CollectionItemDTO
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public bool IsDeleted { get; set; }
        public int StartingQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string UserName { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
    }

    public class CreateSupportTicketDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Issue { get; set; }
        public string? UserName { get; set; }
    }

    public class SupportTicketDTO
    {
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

    public class AppealDecisionDTO
    {
        public string? StaffNote { get; set; }
    }

    public class RequestResetCodeDTO
    {
        public string UserName { get; set; }
    }

    public class VerifyResetCodeDTO
    {
        public string UserName { get; set; }
        public string Code { get; set; }
    }

    public class ResetPasswordWithCodeDTO
    {
        public string UserName { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
