using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Abioka.GiftApi {
    public static class GiftDelete {
        [FunctionName ("GiftDelete")]
        public static async Task<IActionResult> Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "delete", Route = "gifts/{id}")] HttpRequest req, [CosmosDB (databaseName: "ToDoList",
                collectionName: "Gift",
                ConnectionStringSetting = "CosmosDBConnection",
                PartitionKey = "{id}")] DocumentClient client,
            ILogger log,
            Guid id) {

            Uri documentUri = UriFactory.CreateDocumentUri ("ToDoList", "Gift", id.ToString ());
            await client.DeleteDocumentAsync (documentUri, new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey (id.ToString()) });

            return new OkResult ();
        }
    }
}