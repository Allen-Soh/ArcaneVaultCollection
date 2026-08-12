//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

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
