using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class ArcaneVaultUserRoles
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
