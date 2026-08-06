using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class Sticker
    {
        [Key]
        public string StickerCode { get; set; }
        public string StickerName { get; set; }
        public string? ImagePath { get; set; }
    }
}