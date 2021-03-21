using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Device
{
    /// <summary>
    /// Demonstrates how to register a device with the device provisioning service using a symmetric key, and then
    /// use the registration information to authenticate to IoT Hub.
    /// </summary>
    public class DeviceSimulator
    {
        private readonly string id;
        private readonly string key;
        private readonly string idScope;

        public DeviceSimulator(string id, string key, string idScope)
        {
            this.id = id;
            this.key = key;
            this.idScope = idScope;
        }

        public async Task RunSampleAsync()
        {
            Console.WriteLine($"Initializing the device provisioning client...");

            using var security = new SecurityProviderSymmetricKey(
                this.id,
                this.key,
                null);

            using var transportHandler = new ProvisioningTransportHandlerAmqp();

            ProvisioningDeviceClient provClient = ProvisioningDeviceClient.Create(
                "global.azure-devices-provisioning.net",
                this.idScope,
                security,
                transportHandler);

            Console.WriteLine($"Initialized for registration Id {security.GetRegistrationID()}.");

            Console.WriteLine("Registering with the device provisioning service...");
            DeviceRegistrationResult device = await provClient.RegisterAsync();

            Console.WriteLine($"Registration status: {device.Status}.");
            if (device.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                Console.WriteLine($"Registration status did not assign a hub, so exiting this sample.");
                return;
            }

            Console.WriteLine($"Device {device.DeviceId} registered to {device.AssignedHub}.");

            Console.WriteLine("Creating symmetric key authentication for IoT Hub...");
            IAuthenticationMethod auth = new DeviceAuthenticationWithRegistrySymmetricKey(
                device.DeviceId,
                security.GetPrimaryKey());

            await SendDeviceToCloudMessagesAsync(device, auth);

            Console.WriteLine("Finished.");
        }

        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync(DeviceRegistrationResult device, IAuthenticationMethod auth)
        {
            Console.WriteLine($"Testing the provisioned device with IoT Hub...");
            using DeviceClient iotClient = DeviceClient.Create(device.AssignedHub, auth);

            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            var rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message
                string messageBody = JsonSerializer.Serialize(
                    new
                    {
                        temperature = currentTemperature,
                        humidity = currentHumidity,
                    });

                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
                await iotClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

                await Task.Delay(5000);
            }
        }
    }
}