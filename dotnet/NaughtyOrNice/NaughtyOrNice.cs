using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Abioka.Function {
    public static class NaughtyOrNice {
        private readonly static string subscriptionKey = Environment.GetEnvironmentVariable("TextAnalyticsSubscriptionKey");
        private readonly static string endpoint = Environment.GetEnvironmentVariable("TextAnalyticsEndpoint");

        [FunctionName ("NaughtyOrNice")]
        public static async Task<IActionResult> Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log) {

            string requestBody = await new StreamReader (req.Body).ReadToEndAsync ();
            var message = JsonConvert.DeserializeObject<MessageItem> (requestBody);

            var client = HttpClientFactory.Create ();
            var body = new { documents = new [] { new { id = message.Who, text = message.Message } } };

            client.DefaultRequestHeaders.Add ("Ocp-Apim-Subscription-Key", subscriptionKey);
            var response = await client.PostAsJsonAsync ($"{endpoint}/languages", body);
            if (!response.IsSuccessStatusCode)
                throw new Exception (await response.Content.ReadAsStringAsync ());

            var result = JsonConvert.DeserializeObject<LanguageResponse> (await response.Content.ReadAsStringAsync ());
            var language = result.Documents.First ().DetectedLanguages.First ().Iso6391Name;

            var sentimentBody = new { documents = new [] { new { language = language, id = message.Who, text = message.Message } } };

            var sentimentResponse = await client.PostAsJsonAsync ($"{endpoint}/sentiment", sentimentBody);
            if (!sentimentResponse.IsSuccessStatusCode)
                throw new Exception (await sentimentResponse.Content.ReadAsStringAsync ());

            var sentimentResult = JsonConvert.DeserializeObject<SentimentResponse> (await sentimentResponse.Content.ReadAsStringAsync ());

            var document = sentimentResult.Documents.First ();
            return new OkObjectResult (new {
                Who = message.Who,
                Language = language,
                Sentiment = document.Score < 0.5 ? "Naugthy" : "Nice"
            });
        }

        private class MessageItem {
            public string Who { get; set; }

            public string Message { get; set; }
        }

        private class LanguageResponse {
            public IEnumerable<Document> Documents { get; set; }

            public class Document {
                public string Id { get; set; }

                public IEnumerable<DetectedLanguage> DetectedLanguages { get; set; }

                public class DetectedLanguage {
                    public string Iso6391Name { get; set; }
                }
            }
        }

        private class SentimentResponse {
            public IEnumerable<Document> Documents { get; set; }

            public class Document {
                public string Id { get; set; }

                public double Score { get; set; }
            }
        }
    }
}