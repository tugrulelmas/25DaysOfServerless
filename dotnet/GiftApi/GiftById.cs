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
    public static class GiftById {
        [FunctionName ("GiftById")]
        public static IActionResult Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "get", Route = "gifts/{id}")] HttpRequest req, 
            [CosmosDB (databaseName: "ToDoList",
                collectionName: "Gift",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{id}",
                PartitionKey= "{id}")] Gift gift,
            ILogger log) {
            
            if(gift == null)
                return new NotFoundResult();

            return new OkObjectResult(gift);
        }
    }
}