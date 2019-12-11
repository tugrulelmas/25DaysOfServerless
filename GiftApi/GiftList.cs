using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Abioka.GiftApi
{
    public static class GiftList {
        [FunctionName ("GiftList")]
        public static IActionResult Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "get", Route = "gifts")] HttpRequest req, 
            [CosmosDB (databaseName: "ToDoList",
                collectionName: "Gift",
                ConnectionStringSetting = "CosmosDBConnection",
                CreateIfNotExists = true)] IEnumerable<Gift> gifts,
            ILogger log) {
            return new OkObjectResult(gifts);
        }
    }
}