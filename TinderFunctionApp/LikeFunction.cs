using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace TinderFunctionApp
{
    public static class LikeFunction
    {
        [FunctionName("LikeFunction")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            var authentication = new Authentication()
            {
                facebook_token = "EAAGm0PX4ZCpsBANKZAmf7o3WvmWVeLRKBP4HoR431kIZCZCau78FvwMLVy8rPKfPLIH2YwZAa3hya2KgmldcZCdizG4ZB3Gdyjo1TuVxxdCOqxG68ggdP5FFrfZAscxZAMbZCYh8jSKTHMLkjHDEcAyV4z8WqSrgoPub3edTzHGqUWeNVN3kgZB9G2mUwU9B4ickv78s8XzUqTWzWcK4p6RLHFPNapPsRs1RvCqR00UEaiZBg9WZBDPYBQjyZAzJ60um2EIJ0ZD",
                //facebook_id = "506473474"
            };

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsJsonAsync("https://api.gotinder.com/auth", authentication);
                    client.DefaultRequestHeaders.Add("Anonymous", authentication.facebook_token);
                    var recs = await client.GetAsync("https://api.gotinder.com/user/recs");

                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.gotinder.com/user/recs");
                    requestMessage.Headers.Add("x-auth-token", authentication.facebook_token);
                    var reso = await client.SendAsync(requestMessage);
                }
                catch (HttpRequestException e)
                {
                    log.Error($"\nException caught! Message :{e.Message}");
                }
            }
        }
    }

    public class Authentication
    {
        public string facebook_token { get; set; }
        public string facebook_id { get; set; }
    }

    public class Updates
    {
        public string last_activity_date { get; set; }
    }
}
