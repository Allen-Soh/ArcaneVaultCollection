//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ShowoffItem
    {
        public int ShowoffId { get; set; }
        public int ItemId { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public int DisplayOrder { get; set; }

        [ForeignKey("ShowoffId")]
        public Showoff? Showoff { get; set; }

        [ForeignKey("ItemId")]
        public CollectionItem? CollectionItem { get; set; }
    }
}