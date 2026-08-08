using System.ComponentModel.DataAnnotations;

namespace ArcaneVaultAPI.Data
{
    public class Payment
    {
        [Key]
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
