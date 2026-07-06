using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultRP.Models
{
    public class Category
    {
        [Key]
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
    }
}
