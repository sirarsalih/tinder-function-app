using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using TinderFunctionApp.Helpers;
using TinderFunctionApp.Json;
using TinderFunctionApp.Services;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace TinderFunctionApp
{
    public static class LikeFunction
    {
        [FunctionName("LikeFunction")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            using (var client = new HttpClient()) {
                try {
                    var config = new ConfigurationBuilder()
                     .SetBasePath(context.FunctionAppDirectory)
                     .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables()
                     .Build();
                    var tableStorageService = new TableStorageService(config["StorageAccountName"], config["StorageAccountKey"]);
                    var response = await client.PostAsJsonAsync(Utils.GetAuthUrl(), new Auth { facebook_id = config["FacebookId"], facebook_token = config["FacebookToken"]});
                    switch (response.StatusCode) {
                        case HttpStatusCode.OK:
                            log.Info($"Successful authentication. {(int)response.StatusCode} {response.ReasonPhrase}.");
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var tinderToken = JObject.Parse(responseBody).GetValue("token").ToString();
                            client.DefaultRequestHeaders.Add("X-Auth-Token", tinderToken);
                            client.DefaultRequestHeaders.Add("User-Agent", "Tinder/7.5.3 (iPhone; iOS 10.3.2; Scale/2.00)");
                            var updates = await client.PostAsJsonAsync(Utils.GetUpdatesUrl(), new Time { last_activity_date = "" });
                            var updatesBody = await updates.Content.ReadAsStringAsync();
                            var ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(updatesBody)) { Position = 0 };
                            var ser = new DataContractJsonSerializer(typeof(Updates));
                            var updatesJson = (Updates)ser.ReadObject(ms);

                            var matchesTable = tableStorageService.GetCloudTable(Utils.GetMatchesTableName());

                            var recs = await client.GetAsync(Utils.GetRecsUrl());
                            var recsBody = await recs.Content.ReadAsStringAsync();
                            if(recsBody.Contains("recs timeout") || recsBody.Contains("recs exhausted")) {
                                log.Info($"Too many queries for new users in a too short period of time. Pausing function for {Convert.ToInt32(config["FunctionPauseMilliseconds"])}ms...");
                                Thread.Sleep(Convert.ToInt32(config["FunctionPauseMilliseconds"]));
                            } else {
                                var resultsJson = JToken.Parse(recsBody).Last().ToString();
                                var encloseResultsJson = $"{{{resultsJson}}}";
                                ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(encloseResultsJson)) { Position = 0 };
                                ser = new DataContractJsonSerializer(typeof(Results));
                                var results = (Results)ser.ReadObject(ms);
                                foreach (var result in results.results) {
                                    if(new Random().NextDouble() >= 0.5) {
                                        var superLike = await client.PostAsync(Utils.GetSuperLikeUrl(result._id), null);
                                        if (superLike.StatusCode != HttpStatusCode.OK) continue;
                                        log.Info($"Successfully super liked {result.name} ({Utils.GetGender(result.gender)} age {Utils.GetAge(result.birth_date)}) who is {result.distance_mi} Miles away from my current location. {result.name} has {result.photos.Count} photo(s).");
                                        //SendEmailAsync(client, log, result, config["GmailUserName"], config["GmailAppPassword"]);
                                    } else {
                                        var like = await client.GetAsync(Utils.GetLikeUrl(result._id));
                                        if (like.StatusCode != HttpStatusCode.OK) continue;
                                        log.Info($"Successfully liked {result.name} ({Utils.GetGender(result.gender)} age {Utils.GetAge(result.birth_date)}) who is {result.distance_mi} Miles away from my current location. {result.name} has {result.photos.Count} photo(s).");
                                        //SendEmailAsync(client, log, result, config["GmailUserName"], config["GmailAppPassword"]);
                                    }
                                }
                            }                            
                            break;
                        case HttpStatusCode.Unauthorized:
                            log.Info($"Unsuccessful authentication. {(int)response.StatusCode} {response.ReasonPhrase}. Check Facebook token, it may be invalid or has expired and must be renewed.");
                            break;                       
                    }
                } catch (HttpRequestException e) {
                    log.Error($"Exception caught! Message :{e.Message}.");
                }
            }
        }

        private static async void SendEmailAsync(HttpClient client, TraceWriter log, Person person, string gmailUserName, string gmailAppPassword)
        {
            //var match = await client.GetAsync(Utils.GetMatchUrl(person._id));
            //if (match.StatusCode != HttpStatusCode.OK) return;
            //log.Info($"Match with {person.name}!");
            //log.Info($"Notifying {gmailUserName} by e-mail...");
            //var gmailService = new GmailService();
            //gmailService.SendEmail(
            //    gmailUserName,
            //    gmailAppPassword,
            //    gmailUserName,
            //    gmailUserName,
            //    Utils.CreateEmailSubject(person),
            //    Utils.CreateEmailBody(person)
            //);
        }
    }
}
