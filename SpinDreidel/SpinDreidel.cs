using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Abioka.Function
{
    public static class SpinDreidel {

        private static char[] messages = new char[] { 'נ', 'ג', 'ה', 'ש' };

        [FunctionName ("SpinDreidel")]
        public static IActionResult Run (
            [HttpTrigger (AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log) {
            var index = new Random ().Next (4);
            return new OkObjectResult (messages[index]);
        }
    }
}