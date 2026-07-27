namespace ArcaneVaultAPI.DataTransferObject
{
    public class ArcaneVaultUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class CollectionItemDTO
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public bool IsDeleted { get; set; }
        public int StartingQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string UserName { get; set; }
    }
}
