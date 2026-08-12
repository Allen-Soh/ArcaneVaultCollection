//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultRP.Models
{
    public class ShowoffStickerPlacement
    {
        public int PlacementId { get; set; }
        public int ShowoffId { get; set; }
        public string StickerCode { get; set; }
        public string? ImagePath { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Scale { get; set; } = 1.0;
        public double Rotation { get; set; } = 0;
    }
}
