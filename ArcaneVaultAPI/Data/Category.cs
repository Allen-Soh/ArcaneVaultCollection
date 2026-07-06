using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class Category
    {
        [Key]
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
    }
}
