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
    public static class GiftInsert {
        [FunctionName ("GiftInsert")]
        public static async Task<IActionResult> Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "post", Route = "gifts")] HttpRequest req, 
            [CosmosDB (databaseName: "ToDoList",
                collectionName: "Gift",
                ConnectionStringSetting = "CosmosDBConnection",
                CreateIfNotExists = true)] IAsyncCollector<Gift> gifts,
            ILogger log) {
            
            string requestBody = await new StreamReader (req.Body).ReadToEndAsync ();
            dynamic data = JsonConvert.DeserializeObject (requestBody);
            string name = data?.name ?? string.Empty;
            if(string.IsNullOrWhiteSpace(name))
                return new BadRequestObjectResult("Please send a name for your gift");

            var gift = new Gift{ Id = Guid.NewGuid(), Name = name};
            await gifts.AddAsync(gift);

            return new CreatedResult($"/{gift.Id}", gift);
        }
    }
}