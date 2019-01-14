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

        public async Task SendMatchEmailAsync(Profile profile)
        {
            await SendEmailAsync(
                _gmailUserName,
                _gmailAppPassword,
                _gmailUserName,
                _gmailUserName,
                CreateMatchEmailSubject(profile),
                CreateMatchEmailBody(profile)
            );
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

        public async Task SendTokenExpirationEmailAsync(string subject, string body)
        {
            await SendEmailAsync(
                _gmailUserName,
                _gmailAppPassword,
                _gmailUserName,
                _gmailUserName,
                subject,
                body
            );
        }

        private string CreateMessageEmailSubject(Person person)
        {
            return $"[Tinder function] You got a new message from {person.name} ({Utils.GetAge(person.birth_date)})";
        }

        private string CreateMessageEmailBody(Person person, Message message)
        {
            return $"{person.name}: {message.message}";
        }

        private static string CreateMatchEmailSubject(Profile profile)
        {
            return $"[Tinder function] Match with {profile.results.name} ({Utils.GetAge(profile.results.birth_date)})! {profile.results.name} has {profile.results.photos.Count} photo(s)";
        }

        private string CreateMatchEmailSubject(Person person)
        {
            return $"[Tinder function] Match with {person.name} ({Utils.GetAge(person.birth_date)})! {person.name} has {person.photos.Count} photo(s)";
        }

        private static string CreateMatchEmailBody(Profile profile)
        {
            var age = Utils.GetAge(profile.results.birth_date);
            var birthDate = Utils.GetBirthDate(profile.results.birth_date);
            var body = $"{profile.results.name} is {age} years old. " +
                       $"Born on {birthDate.ToString("MMMM", CultureInfo.InvariantCulture)} {birthDate.Day}, {birthDate.Year} " +
                       $"and is {Utils.GetKmDistance(profile.results.distance_mi)} away. ";
            if (profile.results.schools.Count > 0)
            {
                body += "Studies or has studied at ";
                foreach (var school in profile.results.schools)
                {
                    body += $"{school.name}, ";
                }
                body = body.Remove(body.Length - 2) + ". ";
            }
            else if (profile.results.jobs.Count > 0)
            {
                body += "Works ";
                foreach (var job in profile.results.jobs)
                {
                    if (!string.IsNullOrWhiteSpace(job.title?.name) && !string.IsNullOrWhiteSpace(job.company?.name))
                    {
                        body += $"as a {job.title.name} at {job.company.name}, ";
                    }
                    else if (string.IsNullOrWhiteSpace(job.title?.name))
                    {
                        body += $"at {job.company?.name}, ";
                    }
                    else if (string.IsNullOrWhiteSpace(job.company?.name))
                    {
                        body += $"as a {job.title.name}, ";
                    }
                }
                body = body.Remove(body.Length - 2) + ". ";
            }
            foreach (var photo in profile.results.photos)
            {
                var url = photo.processedFiles.First().url;
                body += $"<br/><br/><img src=\"{url}\">";
            }
            return body;
        }

        private static string CreateMatchEmailBody(Person person)
        {
            var age = Utils.GetAge(person.birth_date);
            var birthDate = Utils.GetBirthDate(person.birth_date);
            var body = $"{person.name} is {age} years old. Born on {birthDate.ToString("MMMM", CultureInfo.InvariantCulture)} {birthDate.Day}, {birthDate.Year}.";
            foreach (var photo in person.photos)
            {
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
