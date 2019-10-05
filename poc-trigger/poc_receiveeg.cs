using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.EventGrid.Models;

namespace poc_trigger
{
    public static class poc_receiveeg
    {
        [FunctionName("poc_receiveeg")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //var messages = await req.Content.ReadAsAsync<JArray>();
            MemoryStream ms = new MemoryStream();
            await req.Body.CopyToAsync(ms);
            byte[] messageBytes = ms.ToArray();
            var str = System.Text.Encoding.Default.GetString(messageBytes);
            var messages = JArray.Parse(str);

            // If the request is for subscription validation, send back the validation code.
            if (messages.Count > 0 && string.Equals((string)messages[0]["eventType"],
                "Microsoft.EventGrid.SubscriptionValidationEvent",
                System.StringComparison.OrdinalIgnoreCase))
            {
                log.LogInformation("Validate request received");
                return new OkObjectResult(new
                {
                    validationResponse = messages[0]["data"]["validationCode"]
                });                
            }

            // The request is not for subscription validation, so it's for one or more events.
            foreach (JObject message in messages)
            {
                // Handle one event.
                EventGridEvent eventGridEvent = message.ToObject<EventGridEvent>();
                log.LogInformation($"Subject: {eventGridEvent.Subject}");
                log.LogInformation($"Time: {eventGridEvent.EventTime}");
                log.LogInformation($"Event data: {eventGridEvent.Data.ToString()}");
            }

            return new OkResult();
        }
    }
}
