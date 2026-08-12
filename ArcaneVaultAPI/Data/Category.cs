//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

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
