using System.Net;
using System.Net.Mail;

namespace PodcastAppProcject.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string body, string subject)
        {
            var smtphost = _configuration["SmtpSettings:Host"];
            var smtpport = int.Parse(_configuration["SmtpSettings:Port"]);
            var smtpuser = _configuration["SmtpSettings:UserName"];
            var smtppass = _configuration["SmtpSettings:Password"];
            var from = _configuration["SmtpSettings:From"];

            var mail = new MailMessage();
            mail.From = new MailAddress(from);
            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient(smtphost,smtpport)
            {
                Credentials = new NetworkCredential(smtpuser, smtppass),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
