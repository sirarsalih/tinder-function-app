using System;
using System.Globalization;
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

        public static string GetMatchMessageUrl(string id)
        {
            return "https://api.gotinder.com/user/matches/_id".Replace("_id", id);
        }

        public static string GetUserUrl(string id)
        {
            return "https://api.gotinder.com/user/_id}".Replace("_id", id);
        }

        public static string GetMatchesTableName()
        {
            return "Matches";
        }

        public static int GetAge(string birthDate)
        {
            var dob = GetBirthDate(birthDate);
            var today = DateTime.Today;

            var months = today.Month - dob.Month;
            var years = today.Year - dob.Year;

            if (today.Day < dob.Day) {
                months--;
            }
            if (months < 0) {
                years--;
                months += 12;
            }

            var days = (today - dob.AddMonths((years * 12) + months)).Days;

            return Convert.ToInt32(years);
        }

        public static DateTime GetBirthDate(string birthDate)
        {
            return DateTime.Parse(birthDate, null, DateTimeStyles.RoundtripKind);
        }

        public static string GetGender(int gender)
        {
            // Male: 0
            // Female: 1
            return gender == 1 ? "female" : "male";
        }

        public static double GetKmDistance(string miles)
        {
            return Convert.ToInt32(miles) * 1.6;
        }

        public static bool IsRecent(string createdDateTime, DateTime timeToCompare, int aliveDurationMinutes)
        {
            var elapsedTime = timeToCompare - DateTime.Parse(createdDateTime, null, DateTimeStyles.RoundtripKind);
            return (elapsedTime.TotalSeconds > -1 && elapsedTime.TotalSeconds < 0) // up to 1 second before
                   || (elapsedTime.TotalSeconds >= 0 && Math.Floor(elapsedTime.TotalSeconds) <= aliveDurationMinutes * 60);
        }
    }
}
