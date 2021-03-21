﻿using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Security.Cryptography;
using System.Text;
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
            DeviceRegistrationResult result = await provClient.RegisterAsync();

            Console.WriteLine($"Registration status: {result.Status}.");
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                Console.WriteLine($"Registration status did not assign a hub, so exiting this sample.");
                return;
            }

            Console.WriteLine($"Device {result.DeviceId} registered to {result.AssignedHub}.");

            Console.WriteLine("Creating symmetric key authentication for IoT Hub...");
            IAuthenticationMethod auth = new DeviceAuthenticationWithRegistrySymmetricKey(
                result.DeviceId,
                security.GetPrimaryKey());

            // Console.WriteLine($"Testing the provisioned device with IoT Hub...");
            // using DeviceClient iotClient = DeviceClient.Create(result.AssignedHub, auth, _parameters.TransportType);

            // Console.WriteLine("Sending a telemetry message...");
            // using var message = new Message(Encoding.UTF8.GetBytes("TestMessage"));
            // await iotClient.SendEventAsync(message);

            Console.WriteLine("Finished.");
        }
    }
}