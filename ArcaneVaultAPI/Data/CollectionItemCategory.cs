//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

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
