//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultRP.Models
{
    public class CollectionItem
    {
        [Key]
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public bool IsDeleted { get; set; }
        public int StartingQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string UserName { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
    }
}