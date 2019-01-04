using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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

        public static string GetMatchesTableName()
        {
            return "Matches";
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

        public static string CreateEmailSubject(Person person)
        {
            return $"Tinder match with {person.name} ({GetAge(person.birth_date)})! {person.name} has {person.photos.Count} photo(s)";
        }

        public static string CreateEmailBody(Person person)
        {
            var body = $"{person.name} is {GetAge(person.birth_date)} years old.";
            foreach (var photo in person.photos) {
                var url = photo.processedFiles.First().url;
                body += $"<br/><br/><img src=\"{url}\">";
            }
            return body;
        }
    }
}
