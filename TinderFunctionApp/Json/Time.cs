namespace TinderFunctionApp.Json
{
    public class Time
    {
        // Format ISO 8601: yyyy-MM-ddTHH:mm:ssZ
        // System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        public string last_activity_date { get; set; }
    }
}
