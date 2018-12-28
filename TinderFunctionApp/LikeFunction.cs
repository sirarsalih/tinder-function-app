using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using TinderFunctionApp.Json;

namespace TinderFunctionApp
{
    public static class LikeFunction
    {
        [FunctionName("LikeFunction")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var authentication = new Auth()
                    {
                        facebook_token = "",
                        facebook_id = "506473474"
                    };
                    var auth = await client.PostAsJsonAsync("https://api.gotinder.com/auth", authentication);
                    if (auth.StatusCode == HttpStatusCode.OK)
                    {
                        var authBody = await auth.Content.ReadAsStringAsync();
                        var tinderToken = JObject.Parse(authBody).GetValue("token").ToString();
                        client.DefaultRequestHeaders.Add("X-Auth-Token", tinderToken);
                        var recs = await client.GetAsync("https://api.gotinder.com/user/recs");
                        var recsBody = await recs.Content.ReadAsStringAsync();
                        var resultsJson = JToken.Parse(recsBody).Last().ToString();
                        var encloseResultsJson = "{" + resultsJson + "}";
                        var ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(encloseResultsJson)) { Position = 0 };
                        var ser = new DataContractJsonSerializer(typeof(Results));
                        var results = (Results)ser.ReadObject(ms);
                        foreach (var result in results.results)
                        {
                            var like = await client.GetAsync("https://api.gotinder.com/like/"+result._id);
                            if (like.StatusCode == HttpStatusCode.OK)
                            {
                                log.Info($"Successfully liked {result.name} who is {result.distance_mi} Miles away from my current location.");
                            }
                        }
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
