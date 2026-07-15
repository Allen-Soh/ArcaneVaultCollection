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
        public int LikeCount { get; set; }
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
        public List<ShowoffItemInputDTO> Items { get; set; } = new();
        public List<string> ImagePublicIdsToDelete { get; set; } = new();
    }
}