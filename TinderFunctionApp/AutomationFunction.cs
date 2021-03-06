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
using Match = TinderFunctionApp.TableEntities.Match;

namespace TinderFunctionApp
{
    public static class AutomationFunction
    {
        private static bool _unAuthorizedEmailSent;
        private static string _facebookToken;
        private static string _tinderToken;

        [FunctionName("AutomationFunction")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var config = new ConfigurationBuilder()
                     .SetBasePath(context.FunctionAppDirectory)
                     .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables()
                     .Build();
                    var tableStorageService = new TableStorageService(config["StorageAccountName"], config["StorageAccountKey"]);
                    var gmailService = new GmailService(config["GmailUserName"], config["GmailAppPassword"]);
                    client.DefaultRequestHeaders.Add("X-Auth-Token", _tinderToken);
                    client.DefaultRequestHeaders.Add("User-Agent", "Tinder/7.5.3 (iPhone; iOS 10.3.2; Scale/2.00)");
                    await AuthenticateAndAutomateAsync(log, client, tableStorageService, config, gmailService);
                }
                catch (HttpRequestException e)
                {
                    log.Error($"Exception caught! Message :{e.Message}.");
                }
            }
        }

        private static async Task AuthenticateAndAutomateAsync(TraceWriter log, HttpClient client, TableStorageService tableStorageService, IConfigurationRoot config, GmailService gmailService)
        {
            var updates = await client.PostAsJsonAsync(Utils.GetUpdatesUrl(), new Time { last_activity_date = DateTime.UtcNow.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ") });
            switch (updates.StatusCode)
            {
                case HttpStatusCode.OK:
                    log.Info($"Successful authentication with Tinder API using Tinder token. {(int)updates.StatusCode} {updates.ReasonPhrase}.");
                    var updatesBody = await updates.Content.ReadAsStringAsync();
                    var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(updatesBody)) { Position = 0 };
                    var ser = new DataContractJsonSerializer(typeof(Updates));
                    var updatesJson = (Updates)ser.ReadObject(ms);
                    var messagesTable = tableStorageService.GetCloudTable(Utils.GetMessagesTableName());
                    foreach (var match in updatesJson.matches)
                    {
                        foreach (var message in match.messages)
                        {
                            var messageEntity = tableStorageService.GetMessage(messagesTable, message._id, match.person._id);
                            if (message.from == config["TinderUserId"] || messageEntity != null) continue;
                            log.Info($"New message from {match.person.name} ({Utils.GetAge(match.person.birth_date)}). Notifying user by e-mail...");
                            await gmailService.SendMessageEmailAsync(match.person, message);
                            log.Info("Saving new message in table storage...");
                            tableStorageService.Insert(messagesTable, new TableEntities.Message(message._id, match.person._id));
                        }
                        var matchesTable = tableStorageService.GetCloudTable(Utils.GetMatchesTableName());
                        var matchEntity = tableStorageService.GetMatch(matchesTable, match._id);
                        if (matchEntity != null) continue;
                        log.Info($"Match with {match.person.name} ({Utils.GetAge(match.person.birth_date)})! Attempting to get match profile...");
                        var user = await client.GetAsync(Utils.GetUserUrl(match.person._id));
                        switch (user.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                var userBody = await user.Content.ReadAsStringAsync();
                                var userJson = JToken.Parse(userBody).Last().ToString();
                                var encloseUserJson = $"{{{userJson}}}";
                                ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(encloseUserJson)) { Position = 0 };
                                ser = new DataContractJsonSerializer(typeof(Profile));
                                var profile = (Profile)ser.ReadObject(ms);
                                log.Info("Notifying user by e-mail including all match profile data...");
                                await gmailService.SendMatchEmailAsync(profile);
                                break;
                            default:
                                log.Info("Did not succeed to get match profile. Notifying user by e-mail including limited match profile data...");
                                await gmailService.SendMatchEmailAsync(match.person);
                                break;
                        }
                        log.Info("Saving new match in table storage...");
                        tableStorageService.Insert(matchesTable, new Match(match._id, match.person.name));
                    }
                    var recs = await client.GetAsync(Utils.GetRecsUrl());
                    var recsBody = await recs.Content.ReadAsStringAsync();
                    if (!(recsBody.Contains("recs timeout") || recsBody.Contains("recs exhausted")))
                    {
                        var resultsJson = JToken.Parse(recsBody).Last().ToString();
                        var encloseResultsJson = $"{{{resultsJson}}}";
                        ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(encloseResultsJson)) { Position = 0 };
                        ser = new DataContractJsonSerializer(typeof(Results));
                        var results = (Results)ser.ReadObject(ms);
                        var counter = 1;
                        foreach (var result in results.results)
                        {
                            if (counter % 2 == 0)
                            {
                                var superLike = await client.PostAsync(Utils.GetSuperLikeUrl(result._id), null);
                                if (superLike.StatusCode == HttpStatusCode.OK)
                                {
                                    log.Info($"Successfully super liked {result.name} ({Utils.GetGender(result.gender)} age {Utils.GetAge(result.birth_date)}) who is {Utils.GetKmDistance(result.distance_mi)} km away from my current location. {result.name} has {result.photos.Count} photo(s).");
                                }
                            }
                            else
                            {
                                var like = await client.GetAsync(Utils.GetLikeUrl(result._id));
                                if (like.StatusCode == HttpStatusCode.OK)
                                {
                                    log.Info($"Successfully liked {result.name} ({Utils.GetGender(result.gender)} age {Utils.GetAge(result.birth_date)}) who is {Utils.GetKmDistance(result.distance_mi)} km away from my current location. {result.name} has {result.photos.Count} photo(s).");
                                }
                            }
                            counter++;
                        }
                    }
                    else
                    {
                        log.Info($"Too many queries for new users in a too short period of time. Pausing function for {Convert.ToInt32(config["FunctionPauseMilliseconds"])}ms...");
                        Thread.Sleep(Convert.ToInt32(config["FunctionPauseMilliseconds"]));
                    }
                    break;
                case HttpStatusCode.Unauthorized:
                    log.Info($"Unsuccessful authentication with Tinder API using Tinder token. {(int)updates.StatusCode} {updates.ReasonPhrase}. Generating new Tinder token using Facebook token...");
                    _facebookToken = FacebookHelper.GetFbToken(config["FacebookUserName"], config["FacebookUserPassword"]);
                    var response = await client.PostAsJsonAsync(Utils.GetAuthUrl(), new Auth { facebook_id = config["FacebookId"], facebook_token = _facebookToken });
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            _unAuthorizedEmailSent = false;
                            log.Info($"Successful authentication with Tinder API using Facebook token. {(int)response.StatusCode} {response.ReasonPhrase}.");
                            var responseBody = await response.Content.ReadAsStringAsync();
                            _tinderToken = JObject.Parse(responseBody).GetValue("token").ToString();
                            client.DefaultRequestHeaders.Remove("X-Auth-Token");
                            client.DefaultRequestHeaders.Add("X-Auth-Token", _tinderToken);
                            await AuthenticateAndAutomateAsync(log, client, tableStorageService, config, gmailService);
                            break;
                        case HttpStatusCode.Unauthorized:
                            log.Info($"Unsuccessful authentication with Tinder API using Facebook token. {(int)response.StatusCode} {response.ReasonPhrase}. Facebook token may have expired and must be regenerated.");
                            if (!_unAuthorizedEmailSent)
                            {
                                await gmailService.SendTokenExpirationEmailAsync(
                                    "[Tinder function] Unauthorized connection to Tinder API.",
                                    "Tinder automation has stopped working. Check the Tinder function code at: https://github.com/sirarsalih/tinder-function-app");
                                _unAuthorizedEmailSent = true;
                            }
                            break;
                    }
                    break;
            }
        }
    }
}
