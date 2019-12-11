using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Abioka.Function {
    public static class PngWebHook {
        private static readonly string rawUrl = "https://raw.githubusercontent.com/";

        [FunctionName ("PngWebHook")]
        public static async Task<IActionResult> Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, 
            [CosmosDB (databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<PngFile> pngFiles,
            ILogger log) {
            log.LogInformation ("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader (req.Body).ReadToEndAsync ();
            dynamic data = JsonConvert.DeserializeObject (requestBody);
            if (data == null)
                return new BadRequestObjectResult ("body is empty");

            if (data.commits == null)
                return new EmptyResult ();

            var url = $"{rawUrl}{data.repository?.full_name}/{data.repository?.master_branch}";

            foreach (var commitItem in data.commits) {
                if (commitItem.added == null)
                    return new EmptyResult ();

                foreach (string fileItem in commitItem.added) {
                    if (Path.GetExtension (fileItem).ToLower () != ".png")
                        continue;

                    var fileUrl = $"{url}/{fileItem}";
                    log.LogInformation (fileUrl);
                    await pngFiles.AddAsync (new PngFile { Id = Guid.NewGuid (), Url = fileUrl });
                }
            }

            return new EmptyResult ();
        }
    }

    public class PngFile {
        public Guid Id { get; set; }

        public string Url { get; set; }
    }
}
