using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class Frame
    {
        [Key]
        public string FrameCode { get; set; }
        public string FrameName { get; set; }
        public string? ImagePath { get; set; }
    }
}
