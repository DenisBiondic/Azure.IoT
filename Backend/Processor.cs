using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public static class Processor
    {
        public static async Task ReceiveMessagesFromDeviceAsync(string eventHubConnectionString, string eventHubName)
        {
            await using var consumer = new EventHubConsumerClient(
                EventHubConsumerClient.DefaultConsumerGroupName,
                eventHubConnectionString,
                eventHubName);

            Console.WriteLine("Listening for messages on all partitions.");

            // Begin reading events for all partitions, starting with the first event in each partition and waiting indefinitely for
            // events to become available.
            //
            // The "ReadEventsAsync" method on the consumer is a good starting point for consuming events for prototypes
            // and samples. For real-world production scenarios, it is strongly recommended that you consider using the
            // "EventProcessorClient" from the "Azure.Messaging.EventHubs.Processor" package.
            //
            // More information on the "EventProcessorClient" and its benefits can be found here:
            //   https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/eventhub/Azure.Messaging.EventHubs.Processor/README.md
            await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync())
            {
                Console.WriteLine($"\nMessage received on partition {partitionEvent.Partition.PartitionId}:");

                string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                Console.WriteLine($"\tMessage body: {data}");

                Console.WriteLine("\tApplication properties (set by device):");
                foreach (KeyValuePair<string, object> prop in partitionEvent.Data.Properties)
                {
                    Console.WriteLine($"\t\t{prop.Key}: {prop.Value}");
                }

                Console.WriteLine("\tSystem properties (set by IoT Hub):");
                foreach (KeyValuePair<string, object> prop in partitionEvent.Data.SystemProperties)
                {
                    Console.WriteLine($"\t\t{prop.Key}: {prop.Value}");
                }
            }
        }
    }
}
