namespace NewsManagementSystem.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task TestSendEmail(); // ✅ Thêm phương thức này
    }

}
