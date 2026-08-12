//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    // Minimal ownership record — a successful Payment writes one row here.
    // The full Inventory UI (thumbnails, copy-number display) is Phase 9 work;
    // this table just needs to exist so Phase 8's webhook has somewhere to record ownership.
    public class CosmeticInventory
    {
        [Key]
        public string Uid { get; set; }
        public string UserName { get; set; }
        public string CosmeticId { get; set; }
        public string CosmeticType { get; set; }
        public string AcquiredAt { get; set; }
        public int PaymentId { get; set; }
    }
}
