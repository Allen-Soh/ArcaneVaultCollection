//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultRP.Models
{
    public class InventoryItem
    {
        public string Uid { get; set; }
        public string CosmeticId { get; set; }
        public string CosmeticName { get; set; }
        public string CosmeticType { get; set; }
        public string? ImagePath { get; set; }
        public bool IsLimited { get; set; }
        public int? CopyNumber { get; set; }
        public int? LimitedQuantity { get; set; }
        public string AcquiredAt { get; set; }
        public string? Orientation { get; set; }
    }
}
