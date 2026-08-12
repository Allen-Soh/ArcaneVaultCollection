//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultRP.Models
{
    public class Category
    {
        [Key]
        public string CategoryCode { get; set; }
        [Required]
        [StringLength(40)]
        public string CategoryName { get; set; }
    }
}
