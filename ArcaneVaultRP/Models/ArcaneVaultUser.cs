//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultRP.Models
{
    public class ArcaneVaultUser
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ArcaneVaultUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
