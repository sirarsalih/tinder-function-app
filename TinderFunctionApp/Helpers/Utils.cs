using System;

namespace TinderFunctionApp.Helpers
{
    public static class Utils
    {
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
    }
}
