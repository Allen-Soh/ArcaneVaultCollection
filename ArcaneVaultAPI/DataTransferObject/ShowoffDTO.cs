namespace ArcaneVaultAPI.DataTransferObject
{
    public class ShowoffDTO
    {
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsDeleted { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime? DisabledAt { get; set; }
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
        public int ReportCount { get; set; }
        public List<ShowoffItemDTO> Items { get; set; } = new();
    }

    public class ShowoffItemDTO
    {
        public int ShowoffId { get; set; }
        public int ItemId { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public int DisplayOrder { get; set; }
        public string? ItemName { get; set; }
        public string? CategoryName { get; set; }
    }

    public class CreateShowoffDTO
    {
        public string ShowoffName { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsPrivate { get; set; }
        public string UserName { get; set; }
        public string? Description { get; set; }
        public List<ShowoffItemInputDTO> Items { get; set; } = new();
    }

    public class ShowoffItemInputDTO
    {
        public int ItemId { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateShowoffDTO
    {
        public string ShowoffName { get; set; }
        public string BackgroundColor { get; set; }
        public bool IsPrivate { get; set; }
        public string? Description { get; set; }
        public List<ShowoffItemInputDTO> Items { get; set; } = new();
        public List<string> ImagePublicIdsToDelete { get; set; } = new();
    }

    public class ShowoffCommentDTO
    {
        public int CommentId { get; set; }
        public int ShowoffId { get; set; }
        public string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateShowoffCommentDTO
    {
        public int ShowoffId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
    }

    public class ShowoffReportDTO
    {
        public int ReportId { get; set; }
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string OwnerUserName { get; set; }
        public string ReporterUserName { get; set; }
        public string AssignedStaffUserName { get; set; }
        public string Category { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDisabled { get; set; }
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }
    }

    public class CreateShowoffReportDTO
    {
        public int ShowoffId { get; set; }
        public string ReporterUserName { get; set; }
        public string Category { get; set; }
        public string? Reason { get; set; }
    }
}