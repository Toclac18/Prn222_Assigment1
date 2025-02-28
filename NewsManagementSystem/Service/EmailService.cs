namespace NewsManagementSystem.Service
{
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using NewsManagementSystem.Models;
    using System.Threading.Tasks;

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("News System", _emailSettings.SenderEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task TestSendEmail()
        {
            string adminEmail = "admin@example.com";
            string subject = "🛠 Test Email";
            string body = "<h3>Đây là email test</h3>";

            try
            {
                Console.WriteLine("📧 Đang gửi email test...");
                await SendEmailAsync(adminEmail, subject, body);
                Console.WriteLine("✅ Email test đã gửi thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi khi gửi email: " + ex.Message);
            }
        }
    }
}
