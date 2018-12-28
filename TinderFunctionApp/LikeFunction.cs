using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using TinderFunctionApp.Helpers;
using TinderFunctionApp.Json;

namespace TinderFunctionApp
{
    public static class LikeFunction
    {
        [FunctionName("LikeFunction")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            var authentication = new Auth()
            {
                facebook_token = FacebookHelper.GetFacebookToken("", ""),
                facebook_id = "506473474"
            };

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsJsonAsync("https://api.gotinder.com/auth", authentication);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var tinderToken = JObject.Parse(responseBody).GetValue("token").ToString();
                        client.DefaultRequestHeaders.Add("X-Auth-Token", tinderToken);
                        var recommendations = await client.GetAsync("https://api.gotinder.com/user/recs");
                    }
                }
                catch (HttpRequestException e)
                {
                    log.Error($"\nException caught! Message :{e.Message}");
                }
            }
        }
    }
}
