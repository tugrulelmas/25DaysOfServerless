using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Abioka.Function
{
    public static class ScheduleOrchestration
    {
        [FunctionName("ScheduleOrchestration")]
        public static async Task<IActionResult> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var message = context.GetInput<string>();
            var messageContent = message.Split(",", StringSplitOptions.RemoveEmptyEntries);
            message = messageContent[0];

            DateTime startAt = new DateTime();
            if(messageContent.Length == 2){
                int seconds;
                if(Int32.TryParse(messageContent[1], out seconds)){
                    startAt = DateTime.Now.AddSeconds(seconds);
                }
            }
            await context.CreateTimer(startAt, CancellationToken.None);
            await context.CallActivityAsync<string>("SendSlackMessage", $"{message} to happen now.");

            return new OkObjectResult(message);
        }

        [FunctionName("SendSlackMessage")]
        public static async Task<IActionResult> SayHello([ActivityTrigger] IDurableActivityContext context, ILogger log) {
            var message = context.GetInput<string>();
            var client = HttpClientFactory.Create ();
            var response = await client.PostAsJsonAsync(Environment.GetEnvironmentVariable("SlackWebhookUrl"), new {text = message});
            if (!response.IsSuccessStatusCode)
                throw new Exception (await response.Content.ReadAsStringAsync ());

            return new OkObjectResult(message);
        }

        [FunctionName("Scheduler")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log) {

            string message = req.Form["text"];

            string instanceId = await starter.StartNewAsync<string>("ScheduleOrchestration", null, message);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return new OkObjectResult($"{message.Split(',')[0]} has been scheduled.");
        }
    }
}