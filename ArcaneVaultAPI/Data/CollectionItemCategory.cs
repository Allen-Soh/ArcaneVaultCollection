using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class CollectionItemCategory
    {
        public int ItemId { get; set; }
        public string CategoryCode { get; set; }

        [ForeignKey("ItemId")]
        public CollectionItem? ColItem { get; set; }

        [ForeignKey("CategoryCode")]
        public Category? CatCode { get; set; }
    }
}
