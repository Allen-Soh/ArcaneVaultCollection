//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

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
