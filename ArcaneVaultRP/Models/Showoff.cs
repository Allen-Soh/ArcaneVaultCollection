namespace ArcaneVaultRP.Models
{
    public class Showoff
    {
        public int ShowoffId { get; set; }
        public string ShowoffName { get; set; }
        public string BackgroundColor { get; set; }
        public string? BackgroundImagePath { get; set; }
        public string? BackgroundImagePublicId { get; set; }
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
        public string Orientation { get; set; } = "Landscape";
        public string? EquippedFrameCode { get; set; }
        public string? EquippedFrameImagePath { get; set; }
        public List<ShowoffItem> Items { get; set; } = new();
        public List<ShowoffStickerPlacement> StickerPlacements { get; set; } = new();
    }
}