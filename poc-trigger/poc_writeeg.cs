using Microsoft.Azure.Documents;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using System;
using System.Collections.Generic;

namespace poc_trigger
{
    public static class poc_writeeg
    {
        /// <summary>
        /// this function is triggered by a batch of writes to cosmosdb
        /// </summary>
        /// <param name="input"></param>
        /// <param name="log"></param>
        [FunctionName("poc_writeeg")]
        public static void Run([CosmosDBTrigger(
            databaseName: "%COSMOS-DB-COLLECTION-NAME%",
            collectionName: "ChangeEventStore",
            ConnectionStringSetting = "COSMOS-DB-CONNECTION",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            string TopicKey = System.Environment.GetEnvironmentVariable("EVENT-GRID-TOPIC-KEY");

            // for example:
            // EG_NOTIFY_ENDPOINT=https://eg-notify-aa-greg-29.uksouth-1.eventgrid.azure.net/api/events
            string TopicEndPoint = System.Environment.GetEnvironmentVariable("EG_NOTIFY_ENDPOINT");
            //string TopicHostName = TopicEndPoint.Split("/api")[0];
            string TopicHostName = new Uri(TopicEndPoint).Host;

            Console.WriteLine($"Topic host name = {TopicHostName}");

            if (input != null && input.Count > 0)
            {
                var events = new List<EventGridEvent>();

                foreach (var item in input)
                {
                    events.Add(
                        new EventGridEvent()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Subject = "SoRF-Raw-Module1",
                            Data = item,
                            EventType = "NewDocInLanding",
                            DataVersion = "1.0"
                        }
                    );
                }

                ServiceClientCredentials credentials = new TopicCredentials(TopicKey);

                using (var client = new EventGridClient(credentials))
                {
                    try
                    {
                        client.PublishEventsAsync(TopicHostName, events).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in PublishEventsAsync: {ex.Message}");
                    }
                };


            }
        }
    }
}
