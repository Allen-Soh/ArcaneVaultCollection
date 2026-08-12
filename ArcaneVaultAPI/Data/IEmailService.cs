//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultAPI.Services
{
    public interface IEmailService
    {
        Task SendTicketConfirmationAsync(string toEmail, string name, string ticketUid, string subject);
        Task SendPasswordResetCodeAsync(string toEmail, string userName, string code);
        Task SendShowoffAppealResponseAsync(string toEmail, string userName, string showoffName, bool approved, string? staffNote);
        Task SendShowoffAppealSubmittedAsync(string toEmail, string ownerUserName, string showoffName, string appealMessage, int showoffId);
        Task SendShowoffAppealConfirmationAsync(string toEmail, string userName, string showoffName);
        Task SendTicketReplyAsync(string toEmail, string name, string ticketUid, string subject, string replyMessage);
        Task SendPaymentReceiptAsync(string toEmail, string userName, string cosmeticName, decimal amount, string currency, int paymentId, string cosmeticId, int? copyNumber);
    }
}