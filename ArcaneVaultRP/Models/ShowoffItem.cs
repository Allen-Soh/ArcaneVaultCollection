//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

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