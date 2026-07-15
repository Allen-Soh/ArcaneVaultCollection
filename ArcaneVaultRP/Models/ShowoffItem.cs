namespace ArcaneVaultRP.Models
{
    public class ShowoffItem
    {
        public int ShowoffId { get; set; }
        public int ItemId { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public int DisplayOrder { get; set; }
        public string? ItemName { get; set; }
        public string? CategoryName { get; set; }
    }
}