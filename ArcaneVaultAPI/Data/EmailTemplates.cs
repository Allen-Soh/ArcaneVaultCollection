//Name: Allen Soh Jun Hong
//AdminNo: 241309M
//Tut Group: IT2814 07

namespace ArcaneVaultAPI.Services
{
    public static class EmailTemplates
    {
        // ── Shared visual wrapper — change colors/branding here ONCE, affects every email ──
        private static string Wrap(string title, string bodyHtml)
        {
            return $@"
            <div style=""font-family: 'Segoe UI', Arial, sans-serif; background:#0d1117; padding:32px 16px;"">
              <div style=""max-width:480px; margin:0 auto; background:#161b22; border:1px solid #30363d; border-radius:12px; overflow:hidden;"">
                <div style=""background:linear-gradient(135deg,#2a3f4a,#1a3a5c); padding:24px; text-align:center;"">
                  <h2 style=""color:#e6edf3; margin:0; font-size:20px;"">Arcane Vault</h2>
                </div>
                <div style=""padding:28px 24px; color:#c9d1d9;"">
                  <h3 style=""color:#e6edf3; margin-top:0;"">{title}</h3>
                  {bodyHtml}
                </div>
                <div style=""padding:16px 24px; border-top:1px solid #21262d; text-align:center;"">
                  <p style=""color:#7d8590; font-size:12px; margin:0;"">This is an automated message from Arcane Vault Support.</p>
                </div>
              </div>
            </div>";
        }

        // ── Support Ticket confirmation ──
        public static (string Subject, string Body) TicketConfirmation(string name, string ticketUid, string subject)
        {
            var emailSubject = $"We've received your ticket ({ticketUid})";
            var body = Wrap("Ticket received", $@"
                <p>Hi {name},</p>
                <p>Thanks for reaching out to Arcane Vault Support. We've received your ticket and a member of our team will get back to you soon.</p>
                <p style=""background:#21262d; border-radius:8px; padding:12px 16px; font-size:18px; font-weight:700; text-align:center;"">{ticketUid}</p>
                <p style=""font-size:14px; color:#7d8590;"">Subject: {subject}</p>
                <p>We appreciate your patience.</p>");
            return (emailSubject, body);
        }

        // ── Support ticket reply (staff → customer) ──
        public static (string Subject, string Body) TicketReply(string name, string ticketUid, string subject, string replyMessage)
        {
            var emailSubject = $"Re: {subject} ({ticketUid})";
            var body = Wrap("Response to your ticket", $@"
        <p>Hi {name},</p>
        <p>Thank you for reaching out to Arcane Vault Support. Here's our response to your inquiry:</p>
        <p style=""background:#21262d; border-radius:8px; padding:12px 16px; font-size:14px; white-space:pre-wrap;"">{replyMessage}</p>
        <p style=""font-size:13px; color:#7d8590;"">Ticket: {ticketUid} · Subject: {subject}</p>
        <p>If you have further questions, feel free to reply to this email.</p>");
            return (emailSubject, body);
        }

        // ── Password reset code ──
        public static (string Subject, string Body) PasswordResetCode(string userName, string code)
        {
            var emailSubject = "Your Arcane Vault password reset code";
            var body = Wrap("Reset your password", $@"
                <p>Hi {userName},</p>
                <p>Your password reset code is:</p>
                <p style=""background:#21262d; border-radius:8px; padding:16px; font-size:28px; font-weight:700; letter-spacing:6px; text-align:center;"">{code}</p>
                <p style=""font-size:13px; color:#7d8590;"">This code expires in 5 minutes. If you didn't request this, you can safely ignore this email.</p>");
            return (emailSubject, body);
        }

        // ── Showoff appeal submitted (owner → staff) ──
        public static (string Subject, string Body) ShowoffAppealSubmitted(string ownerUserName, string showoffName, string appealMessage, int showoffId)
        {
            var emailSubject = $"Appeal submitted: \"{showoffName}\" (#{showoffId})";
            var body = Wrap("New showoff appeal", $@"
        <p><strong>{ownerUserName}</strong> has submitted an appeal for their disabled showoff <strong>{showoffName}</strong> (ID: {showoffId}).</p>
        <p style=""background:#21262d; border-radius:8px; padding:12px 16px; font-size:14px; white-space:pre-wrap;"">{appealMessage}</p>
        <p>Please review this in the Showoff Reports dashboard.</p>");
            return (emailSubject, body);
        }

        // ── Showoff appeal received (confirmation to the user who appealed) ──
        public static (string Subject, string Body) ShowoffAppealConfirmation(string userName, string showoffName)
        {
            var emailSubject = $"We've received your appeal for \"{showoffName}\"";
            var body = Wrap("Appeal received", $@"
        <p>Hi {userName},</p>
        <p>We've received your appeal regarding the disabled showoff <strong>{showoffName}</strong>.</p>
        <p>Our staff will review it and follow up once a decision has been made.</p>
        <p>Thank you for your patience.</p>");
            return (emailSubject, body);
        }

        // ── Payment receipt ──
        public static (string Subject, string Body) PaymentReceipt(string userName, string cosmeticName, decimal amount, string currency, int paymentId, string cosmeticId, int? copyNumber)
        {
            var emailSubject = $"Receipt for your purchase of \"{cosmeticName}\"";
            var copyNumberRow = copyNumber.HasValue
                ? $@"<tr><td style=""padding:6px 0; color:#7d8590;"">Limited copy</td><td style=""padding:6px 0; text-align:right;"">#{copyNumber}</td></tr>"
                : "";
            var body = Wrap("Payment receipt", $@"
                <p>Hi {userName},</p>
                <p>Thanks for your purchase! Here's your receipt:</p>
                <table style=""width:100%; border-collapse:collapse; font-size:14px;"">
                    <tr><td style=""padding:6px 0; color:#7d8590;"">Item</td><td style=""padding:6px 0; text-align:right;"">{cosmeticName}</td></tr>
                    <tr><td style=""padding:6px 0; color:#7d8590;"">Item ID</td><td style=""padding:6px 0; text-align:right;"">{cosmeticId}</td></tr>
                    <tr><td style=""padding:6px 0; color:#7d8590;"">Amount</td><td style=""padding:6px 0; text-align:right;"">{currency.ToUpper()} {amount:0.00}</td></tr>
                    <tr><td style=""padding:6px 0; color:#7d8590;"">Payment ID</td><td style=""padding:6px 0; text-align:right;"">{paymentId}</td></tr>
                    {copyNumberRow}
                </table>
                <p>Your new cosmetic is now in your inventory.</p>");
            return (emailSubject, body);
        }

        // ── Showoff appeal response ──
        public static (string Subject, string Body) ShowoffAppealResponse(string userName, string showoffName, bool approved, string? staffNote)
        {
            var emailSubject = approved
                ? $"Your appeal for \"{showoffName}\" was approved"
                : $"Update on your appeal for \"{showoffName}\"";

            var statusHtml = approved
                ? "<p style=\"color:#3fb950; font-weight:600;\">Your showoff has been re-enabled.</p>"
                : "<p style=\"color:#f85149; font-weight:600;\">Your appeal was reviewed and the showoff remains disabled.</p>";

            var noteHtml = !string.IsNullOrEmpty(staffNote)
                ? $"<p style=\"background:#21262d; border-radius:8px; padding:12px 16px; font-size:14px;\">{staffNote}</p>"
                : "";

            var body = Wrap("Appeal update", $@"
                <p>Hi {userName},</p>
                {statusHtml}
                {noteHtml}
                <p>If you have further questions, feel free to reach out to Arcane Vault Support.</p>");

            return (emailSubject, body);
        }
    }
}