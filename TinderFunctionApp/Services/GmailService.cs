using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TinderFunctionApp.Helpers;
using TinderFunctionApp.Json;

namespace TinderFunctionApp.Services
{
    public class GmailService
    {
        private static string _gmailUserName;
        private static string _gmailAppPassword;

        public GmailService(string gmailUserName, string gmailAppPassword)
        {
            _gmailUserName = gmailUserName;
            _gmailAppPassword = gmailAppPassword;
        }

        public async Task SendMatchEmailAsync(Person person)
        {
            await SendEmailAsync(
                _gmailUserName,
                _gmailAppPassword,
                _gmailUserName,
                _gmailUserName,
                CreateMatchEmailSubject(person),
                CreateMatchEmailBody(person)
            );
        }

        public async Task SendMessageEmailAsync(Person person, Message message)
        {
            await SendEmailAsync(
                _gmailUserName,
                _gmailAppPassword,
                _gmailUserName,
                _gmailUserName,
                CreateMessageEmailSubject(person),
                CreateMessageEmailBody(person, message)
            );
        }

        private string CreateMessageEmailSubject(Person person)
        {
            return $"You got a new message from {person.name} ({Utils.GetAge(person.birth_date)})!";
        }

        private string CreateMessageEmailBody(Person person, Message message)
        {
            return $"{person.name} says \"{message.message}\"";
        }

        private static string CreateMatchEmailSubject(Person person)
        {
            return $"Tinder match with {person.name} ({Utils.GetAge(person.birth_date)})! {person.name} has {person.photos.Count} photo(s)";
        }

        private static string CreateMatchEmailBody(Person person)
        {
            var age = Utils.GetAge(person.birth_date);
            var birthDate = Utils.GetBirthDate(person.birth_date);
            var body = $"{person.name} is {age} years old. Born on {birthDate.ToString("MMMM", CultureInfo.InvariantCulture)} {birthDate.Day}, {birthDate.Year}.";
            foreach (var photo in person.photos) {
                var url = photo.processedFiles.First().url;
                body += $"<br/><br/><img src=\"{url}\">";
            }
            return body;
        }

        private async Task SendEmailAsync(string userName, string appPassword, string from, string to, string subject, string body)
        {
            var mail = new MailMessage(from, to);
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
