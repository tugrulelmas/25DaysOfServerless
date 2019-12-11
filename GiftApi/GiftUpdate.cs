using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Abioka.GiftApi {
    public static class GiftUpdate {
        [FunctionName ("GiftUpdate")]
        public static async Task<IActionResult> Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "put", Route = "gifts/{id}")] HttpRequest req, 
            [CosmosDB (databaseName: "ToDoList",
                collectionName: "Gift",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}",
                PartitionKey= "{id}")] Gift gift,
            ILogger log) {
            
            if(gift == null)
                return new BadRequestObjectResult("gift cannot be found");

            string requestBody = await new StreamReader (req.Body).ReadToEndAsync ();
            dynamic data = JsonConvert.DeserializeObject (requestBody);
            string name = data?.name ?? string.Empty;
            if(string.IsNullOrWhiteSpace(name))
                return new BadRequestObjectResult("Please send a name for your gift");

            gift.Name = name;

            return new OkResult();
        }
    }
}