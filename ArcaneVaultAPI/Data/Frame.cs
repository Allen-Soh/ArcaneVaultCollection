using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class Frame
    {
        [Key]
        public string? FrameCode { get; set; }
        public string FrameName { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public string Orientation { get; set; } = "Landscape";
        public decimal Price { get; set; }
        public bool IsLimited { get; set; }
        public int? LimitedQuantity { get; set; }
        public bool IsDelisted { get; set; }
    }
}
