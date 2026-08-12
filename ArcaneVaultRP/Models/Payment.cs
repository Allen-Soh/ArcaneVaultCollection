//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultRP.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public string UserName { get; set; }
        public string CosmeticId { get; set; }
        public string CosmeticName { get; set; }
        public string CosmeticType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string StripeSessionId { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
    }
}
