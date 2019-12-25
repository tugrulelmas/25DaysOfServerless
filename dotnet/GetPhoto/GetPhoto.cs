using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace Abioka.Function
{
    public static class GetPhoto
    {
        [FunctionName("GetPhoto")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="Photos/{keywords}")] HttpRequest req,
            ILogger log,
            string keywords)
        {
            var client = HttpClientFactory.Create();
            var imageResponse = await client.GetAsync($"https://source.unsplash.com/1600x900/?{keywords}");

            return new FileContentResult(await imageResponse.Content.ReadAsByteArrayAsync(), imageResponse.Content.Headers.ContentType.MediaType);
        }
    }
}
