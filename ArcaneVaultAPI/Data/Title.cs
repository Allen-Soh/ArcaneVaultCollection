using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class Title
    {
        [Key]
        public string TitleCode { get; set; }
        public string TitleName { get; set; }
        public string? TitleDescription { get; set; }
        public string? ImagePath { get; set; }
    }
}
