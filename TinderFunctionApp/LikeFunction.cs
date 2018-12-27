using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace TinderFunctionApp
{
    public static class LikeFunction
    {
        [FunctionName("LikeFunction")]
        public static async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var responseBody = await client.GetStringAsync("http://www.contoso.com/");
                    log.Info(responseBody);
                }
                catch (HttpRequestException e)
                {
                    log.Error($"\nException caught! Message :{e.Message}");
                }
            }
        }
    }
}
