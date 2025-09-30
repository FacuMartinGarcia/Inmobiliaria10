using System.Net;
using System.Net.Mail;

namespace Inmobiliaria10.Services
{
    public interface IEmailService
    {
        Task Enviar(string destinatario, string asunto, string cuerpoHtml);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task Enviar(string destinatario, string asunto, string cuerpoHtml)
        {
            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException("El destinatario no puede ser nulo o vacío.", nameof(destinatario));

            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
            var smtpFrom = Environment.GetEnvironmentVariable("SMTP_FROM");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(smtpFrom ?? throw new InvalidOperationException("SMTP_FROM no configurado")),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mail.To.Add(destinatario);

            await client.SendMailAsync(mail);
        }       
    }
}
