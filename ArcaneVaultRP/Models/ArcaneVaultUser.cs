namespace ArcaneVaultRP.Models
{
    public class ArcaneVaultUser
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
    }
}
