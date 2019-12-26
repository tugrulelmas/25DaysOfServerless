using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Abioka.Function
{
    public static class ScheduleOrchestration
    {
        [FunctionName("ScheduleOrchestration")]
        public static async Task<IActionResult> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var message = context.GetInput<string>();
            var startAt = await context.CallActivityAsync<DateTime>("GetDateFromMessage", message);
            await context.CallActivityAsync("SendSlackMessage", $"*{message}* has been scheduled at *{startAt}*");
            await context.CreateTimer(startAt, CancellationToken.None);
            await context.CallActivityAsync("SendSlackMessage", $"*{message}* to happen now.");

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

        [FunctionName("GetDateFromMessage")]
        public static async Task<DateTime> GetDateFromMessage([ActivityTrigger] IDurableActivityContext context, ILogger log) {
            var message = context.GetInput<string>();
            var client = HttpClientFactory.Create ();
            var response = await client.GetAsync(string.Concat(Environment.GetEnvironmentVariable("LUISEndpoint"), message));
            if (!response.IsSuccessStatusCode)
                throw new Exception (await response.Content.ReadAsStringAsync ());

            var luisResponse = await response.Content.ReadAsAsync<LuisResponse>();
            var dateEntity = luisResponse.Entities.FirstOrDefault(x=>x.Type == "builtin.datetimeV2.date" || x.Type == "builtin.datetimeV2.datetime");
            if(dateEntity != null && DateTime.TryParse(dateEntity.Resolution.Values?.First()?.Value, out DateTime date))
            {
                return new DateTime(date.Ticks, DateTimeKind.Utc);;
            }
            
            dateEntity = luisResponse.Entities.FirstOrDefault(x=>x.Type == "builtin.datetimeV2.duration");
            if(dateEntity != null && Int32.TryParse(dateEntity.Resolution.Values?.First()?.Value, out int seconds)){
                return DateTime.UtcNow.AddSeconds(seconds);
            }

            dateEntity = luisResponse.Entities.FirstOrDefault(x=>x.Type == "builtin.datetimeV2.datetimerange");
            if(dateEntity != null && DateTime.TryParse(dateEntity.Resolution.Values?.First()?.Start, out DateTime start)
                && DateTime.TryParse(dateEntity.Resolution.Values?.First()?.End, out DateTime end)){
                return new DateTime(start.AddSeconds(((end - start).TotalSeconds / 2)).Ticks, DateTimeKind.Utc);
            }

            return DateTime.UtcNow;
        }

        [FunctionName("Scheduler")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log) {

            string message = req.Form["text"];

            string instanceId = await starter.StartNewAsync<string>("ScheduleOrchestration", null, message);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        private class LuisResponse{
            public IEnumerable<Entity> Entities { get; set; }

            public class Entity{
                public string Type { get; set; }

                public Resolution Resolution { get; set; }
            }

            public class Resolution{
                public IEnumerable<ResolutionValue> Values { get; set; }
            }

            public class ResolutionValue{
                public string Value { get; set; }

                public string End { get; set; }

                public string Start { get; set; }
            }
        }
    }
}