using System.Net;
using System.Net.Mail;

namespace ArcaneVaultAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public Task SendTicketConfirmationAsync(string toEmail, string name, string ticketUid, string subject)
        {
            var (emailSubject, body) = EmailTemplates.TicketConfirmation(name, ticketUid, subject);
            return SendAsync(toEmail, emailSubject, body);
        }

        public Task SendTicketReplyAsync(string toEmail, string name, string ticketUid, string subject, string replyMessage)
        {
            var (emailSubject, body) = EmailTemplates.TicketReply(name, ticketUid, subject, replyMessage);
            return SendAsync(toEmail, emailSubject, body);
        }

        public Task SendPasswordResetCodeAsync(string toEmail, string userName, string code)
        {
            var (emailSubject, body) = EmailTemplates.PasswordResetCode(userName, code);
            return SendAsync(toEmail, emailSubject, body);
        }

        public Task SendShowoffAppealSubmittedAsync(string toEmail, string ownerUserName, string showoffName, string appealMessage, int showoffId)
        {
            var (emailSubject, body) = EmailTemplates.ShowoffAppealSubmitted(ownerUserName, showoffName, appealMessage, showoffId);
            return SendAsync(toEmail, emailSubject, body);
        }

        public Task SendShowoffAppealResponseAsync(string toEmail, string userName, string showoffName, bool approved, string? staffNote)
        {
            var (emailSubject, body) = EmailTemplates.ShowoffAppealResponse(userName, showoffName, approved, staffNote);
            return SendAsync(toEmail, emailSubject, body);
        }

        public Task SendShowoffAppealConfirmationAsync(string toEmail, string userName, string showoffName)
        {
            var (emailSubject, body) = EmailTemplates.ShowoffAppealConfirmation(userName, showoffName);
            return SendAsync(toEmail, emailSubject, body);
        }

        // Single low-level sender — every email type funnels through here
        private async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var username = _config["Smtp:Username"];
            var password = _config["Smtp:Password"];
            var fromEmail = _config["Smtp:From"];

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage(fromEmail, toEmail, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}