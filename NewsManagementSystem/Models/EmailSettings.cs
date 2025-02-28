namespace NewsManagementSystem.Models
{
    public class EmailSettings
    {
        public required string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public required string SenderEmail { get; set; } = string.Empty;
        public required string SenderPassword { get; set; } = string.Empty;
        public bool UseSSL { get; set; }
        public bool UseStartTls { get; set; }
    }
}
