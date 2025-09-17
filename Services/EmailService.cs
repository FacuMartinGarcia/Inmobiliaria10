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
            var smtpHost = _config["Smtp:Host"] ?? throw new InvalidOperationException("Falta configurar Smtp:Host en appsettings.json");
            var smtpUser = _config["Smtp:User"] ?? throw new InvalidOperationException("Falta configurar Smtp:User en appsettings.json");
            var smtpPass = _config["Smtp:Pass"] ?? throw new InvalidOperationException("Falta configurar Smtp:Pass en appsettings.json");
            var smtpFrom = _config["Smtp:From"] ?? throw new InvalidOperationException("Falta configurar Smtp:From en appsettings.json");

            // Puerto con valor por defecto
            int.TryParse(_config["Smtp:Port"], out var smtpPort);
            if (smtpPort == 0) smtpPort = 587;

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(smtpFrom),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mail.To.Add(destinatario);

            await client.SendMailAsync(mail);
        }
    }
}
