using System;
using System.Threading.Tasks;

namespace Backend
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Parse application parameters
            var iotHubConnectionString = System.Environment.GetEnvironmentVariable("BACKEND_IOT_HUB_CONNECTION_STRING");
            var iotHubEventHubEndpointName = System.Environment.GetEnvironmentVariable("BACKEND_IOT_HUB_EVENT_HUB_ENDPOINT_NAME");

            Console.WriteLine("Starting the backend processing...");
            Console.WriteLine(iotHubConnectionString);
            Console.WriteLine(iotHubEventHubEndpointName);
            await Processor.ReceiveMessagesFromDeviceAsync(iotHubConnectionString, iotHubEventHubEndpointName);

            return 0;
        }
    }
}
