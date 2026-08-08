using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVaultAPI.Data
{
    public class ArcaneVaultUser
    {
        [Key]
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        [Range(1, 2)]
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public string? LastNotificationCheck { get; set; }

        [ForeignKey("RoleId")]
        public ArcaneVaultUserRoles Role { get; set; }
    }
}
