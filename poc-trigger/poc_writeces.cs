using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace poc_trigger
{
    /// <summary>
    /// This function writes a new CES record when triggered by a call from ADF.
    /// </summary>
    public static class poc_writeces
    {
        [FunctionName("poc_writeces")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                "%COSMOS-DB-COLLECTION-NAME%",
                "ChangeEventStore",
                ConnectionStringSetting = "COSMOS-DB-CONNECTION",
                CreateIfNotExists = true)] out dynamic document,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            document = new {
                data?.id,
                data?.subject,
                data?.eventTime,
                data?.folderName,
                data?.fileName,
                data?.copyActivity
            };

            return new OkObjectResult(document);
        }
    }
}
