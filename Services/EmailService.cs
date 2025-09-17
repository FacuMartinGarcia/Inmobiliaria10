using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var smtpUser = _config["Smtp:User"];
            var smtpPass = _config["Smtp:Pass"];
            var smtpFrom = _config["Smtp:From"];

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
