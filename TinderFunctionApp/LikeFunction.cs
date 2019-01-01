using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using TinderFunctionApp.Json;

namespace TinderFunctionApp
{
    public static class LikeFunction
    {
        [FunctionName("LikeFunction")]
        public static async Task Run([TimerTrigger("0 */10 * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            using (var client = new HttpClient()) {
                try {
                    var config = new ConfigurationBuilder()
                     .SetBasePath(context.FunctionAppDirectory)
                     .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables()
                     .Build();
                    var response = await client.PostAsJsonAsync("https://api.gotinder.com/auth", new Auth() { facebook_id = config["FacebookId"], facebook_token = config["FacebookToken"]});
                    switch (response.StatusCode) {
                        case HttpStatusCode.OK:
                            log.Info($"Successful authentication. {(int)response.StatusCode} {response.ReasonPhrase}.");
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var tinderToken = JObject.Parse(responseBody).GetValue("token").ToString();
                            client.DefaultRequestHeaders.Add("X-Auth-Token", tinderToken);
                            var recs = await client.GetAsync("https://api.gotinder.com/user/recs");
                            var recsBody = await recs.Content.ReadAsStringAsync();
                            if(recsBody.Contains("recs timeout")) {
                                log.Info($"Too many queries for new users in a too short period of time. Try again later.");
                            } else {
                                var resultsJson = JToken.Parse(recsBody).Last().ToString();
                                var encloseResultsJson = "{" + resultsJson + "}";
                                var ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(encloseResultsJson)) { Position = 0 };
                                var ser = new DataContractJsonSerializer(typeof(Results));
                                var results = (Results)ser.ReadObject(ms);
                                foreach (var result in results.results) {
                                    if(new System.Random().NextDouble() >= 0.5) {
                                        var superLike = await client.PostAsync("https://api.gotinder.com/like/" + result._id + "/super", null);
                                        if (superLike.StatusCode == HttpStatusCode.OK) {
                                            log.Info($"Successfully super liked {result.name} who is {result.distance_mi} Miles away from my current location.");
                                        }
                                    } else {
                                        var like = await client.GetAsync("https://api.gotinder.com/like/" + result._id);
                                        if (like.StatusCode == HttpStatusCode.OK) {
                                            log.Info($"Successfully liked {result.name} who is {result.distance_mi} Miles away from my current location.");
                                        }
                                    }
                                }
                            }                            
                            break;
                        case HttpStatusCode.Unauthorized:
                            log.Info($"Unsuccessful authentication, check Facebook token. {(int)response.StatusCode} {response.ReasonPhrase}.");
                            break;                       
                    }
                } catch (HttpRequestException e) {
                    log.Error($"Exception caught! Message :{e.Message}.");
                }
            }
        }
    }
}
