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
using Match = TinderFunctionApp.Entities.Match;

namespace TinderFunctionApp
{
    public static class AutomationFunction
    {
        [FunctionName("AutomationFunction")]
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
                            var updates = await client.PostAsJsonAsync(Utils.GetUpdatesUrl(), new Time { last_activity_date = DateTime.UtcNow.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ") });
                            var updatesBody = await updates.Content.ReadAsStringAsync();
                            var ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(updatesBody)) { Position = 0 };
                            var ser = new DataContractJsonSerializer(typeof(Updates));
                            var updatesJson = (Updates)ser.ReadObject(ms);
                            var matchesTable = tableStorageService.GetCloudTable(Utils.GetMatchesTableName());
                            foreach (var match in updatesJson.matches) {
                                var matchEntity = tableStorageService.GetMatch(matchesTable, match._id);
                                if (matchEntity != null) continue;
                                log.Info("New match! Notifying user by e-mail...");
                                await SendEmailAsync(config["GmailUserName"], config["GmailAppPassword"], match.person);
                                log.Info("Saving new match in table storage...");
                                tableStorageService.InsertAsync(matchesTable, new Match(match._id, match.created_date));
                            }
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
                                    var superLike = await client.PostAsync(Utils.GetSuperLikeUrl(result._id), null);
                                    if (superLike.StatusCode == HttpStatusCode.OK) {
                                        log.Info($"Successfully super liked {result.name} ({Utils.GetGender(result.gender)} age {Utils.GetAge(result.birth_date)}) who is {result.distance_mi} Miles away from my current location. {result.name} has {result.photos.Count} photo(s).");
                                    }
                                    var like = await client.GetAsync(Utils.GetLikeUrl(result._id));
                                    if (like.StatusCode == HttpStatusCode.OK) {
                                        log.Info($"Successfully liked {result.name} ({Utils.GetGender(result.gender)} age {Utils.GetAge(result.birth_date)}) who is {result.distance_mi} Miles away from my current location. {result.name} has {result.photos.Count} photo(s).");
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

        private static async Task SendEmailAsync(string gmailUserName, string gmailAppPassword, Person person)
        {
            var gmailService = new GmailService();
            await gmailService.SendEmailAsync(
                gmailUserName,
                gmailAppPassword,
                gmailUserName,
                gmailUserName,
                Utils.CreateEmailSubject(person),
                Utils.CreateEmailBody(person)
            );
        }
    }
}
