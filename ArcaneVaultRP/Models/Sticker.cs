namespace ArcaneVaultRP.Models
{
    public class Sticker
    {
        public string StickerCode { get; set; }
        public string StickerName { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public decimal Price { get; set; }
        public bool IsLimited { get; set; }
        public int? LimitedQuantity { get; set; }
        public bool IsDelisted { get; set; }
    }
}
