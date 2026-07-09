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
    }
}
