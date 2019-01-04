using System;
using System.Linq;
using TinderFunctionApp.Json;

namespace TinderFunctionApp.Helpers
{
    public static class Utils
    {
        public static string GetAuthUrl()
        {
            return "https://api.gotinder.com/auth";
        }

        public static string GetUpdatesUrl()
        {
            return "https://api.gotinder.com/updates";
        }

        public static string GetRecsUrl()
        {
            return "https://api.gotinder.com/user/recs";
        }

        public static string GetSuperLikeUrl(string id)
        {
            return "https://api.gotinder.com/like/_id/super".Replace("_id", id);
        }

        public static string GetLikeUrl(string id)
        {
            return "https://api.gotinder.com/like/_id".Replace("_id", id);
        }

        public static string GetMatchUrl(string id)
        {
            return "https://api.gotinder.com/matches/_id".Replace("_id", id);
        }

        public static int GetAge(string birthDate)
        {
            return DateTime.Now.Year - DateTime.Parse(birthDate, null, System.Globalization.DateTimeStyles.RoundtripKind).Year;
        }

        public static string GetGender(int gender)
        {
            // Male: 0
            // Female: 1
            return gender == 1 ? "female" : "male";
        }

        public static string CreateEmailSubject(Result result)
        {
            return $"Tinder match with {result.name} ({GetAge(result.birth_date)})! {result.name} has {result.photos.Count} photo(s)";
        }

        public static string CreateEmailBody(Result result)
        {
            var body = $"{result.name} is {GetAge(result.birth_date)} years old. ";
            if (result.schools.Count > 0) {
                body += "Studies or has studied at ";
                foreach (var school in result.schools) {
                    body += $"{school.name}, ";
                }
                body = body.Remove(body.Length - 2) + ". ";
            } else if (result.jobs.Count > 0) {
                body += "Works ";
                foreach (var job in result.jobs) {
                    if (!string.IsNullOrWhiteSpace(job.title?.name) && !string.IsNullOrWhiteSpace(job.company?.name)) {
                        body += $"as a {job.title.name} at {job.company.name}, ";
                    } else if (string.IsNullOrWhiteSpace(job.title?.name)) {
                        body += $"at {job.company?.name}, ";
                    } else if (string.IsNullOrWhiteSpace(job.company?.name)) {
                        body += $"as a {job.title.name}, ";
                    }
                }
                body = body.Remove(body.Length - 2) + ". ";
            }
            foreach (var photo in result.photos) {
                var url = photo.processedFiles.First().url;
                body += $"<br/><br/><img src=\"{url}\">";
            }
            return body;
        }
    }
}
