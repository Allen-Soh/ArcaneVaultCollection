namespace ArcaneVaultRP.Models
{
    public class CollectionItemCategory
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public bool IsDeleted { get; set; }
        public int StartingQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string UserName { get; set; }
        public string? CategoryCode { get; set; }
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImagePublicId { get; set; }
    }
}