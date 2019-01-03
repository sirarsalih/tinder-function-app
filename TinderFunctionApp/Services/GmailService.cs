using System.Net;
using System.Net.Mail;

namespace TinderFunctionApp.Services
{
    public class GmailService
    {
        public void SendEmail(string userName, string appPassword, string from, string to, string subject, string body)
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
            mailClient.Send(mail);
        }
    }
}
