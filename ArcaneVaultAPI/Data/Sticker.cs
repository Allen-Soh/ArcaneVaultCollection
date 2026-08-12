//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class Sticker
    {
        [Key]
        public string? StickerCode { get; set; }
        public string StickerName { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public decimal Price { get; set; }
        public bool IsLimited { get; set; }
        public int? LimitedQuantity { get; set; }
        public bool IsDelisted { get; set; }
    }
}