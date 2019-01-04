using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TinderFunctionApp.Services
{
    public class GmailService
    {
        public async Task SendEmailAsync(string userName, string appPassword, string from, string to, string subject, string body)
        {
            var mail = new MailMessage(userName, userName);
            var mailClient = new SmtpClient
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Credentials = new NetworkCredential(userName, appPassword)
            };
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            await mailClient.SendMailAsync(mail);
        }
    }
}
