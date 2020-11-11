using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulator
{
    public static class AzureIoTHub
    {
        /// <summary>
        /// Please replace with correct connection string value
        /// The connection string could be got from Azure IoT Hub -> Shared access policies -> iothubowner -> Connection String:
        /// </summary>
        private const string iotHubConnectionString = "";

        /// <summary>
        /// Please replace with correct device connection string
        /// The device connect string could be got from Azure IoT Hub -> Devices -> {your device name } -> Connection string
        /// </summary>

        ////EnvironmentSensor
        //private const string deviceConnectionString = "HostName=Failover-Test.azure-devices.net;DeviceId=EnvironmentSensor;SharedAccessKey=NgXTQqSCyJw67LGNVyxWMMe8o9hPp16cqik+/PgOglM=";

        ////Smoke sensor
        //private const string deviceConnectionString = "HostName=Failover-Test.azure-devices.net;DeviceId=SmokeSensor;SharedAccessKey=G4kzhYAO9CBeGBhm0HsQpHNMw2CWpbMxK4ZXtQ0djec=";

        //Flame Sensor
        private const string deviceConnectionString = "HostName=ADTAsiaDemo.azure-devices.net;DeviceId=GW_Floor_Two;SharedAccessKey=hJxaiSviaEuiFhVlJHXfgCU1ZT2sa+n5odn+gpD4+z8=";

        public static async Task<string> CreateDeviceIdentityAsync(string deviceName)
        {
            var registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            var device = new Device(deviceName);
            try
            {
                device = await registryManager.AddDeviceAsync(device);
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceName);
            }

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        public static async Task SendDeviceToCloudMessageAsync(CancellationToken cancelToken)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);

            double avgTemperature = 20.0D;
            var rand = new Random();

            while (!cancelToken.IsCancellationRequested)
            {
                double currentTemperature = avgTemperature + rand.Next() %10;
                double currentHumidity = 50 + rand.Next() % 20;
                int currentSmoke = 600 + rand.Next() % 40;
                int currentFlame = 800 + rand.Next() % 200;
                bool currentPIRState = rand.Next() % 2 == 0 ? false : true;
                double currentDistance = 300.00 + rand.Next() % 100;
                int currentLight = 200 + rand.Next() % 100;

                ////Environment Sensor
                //var telemetryDataPoint = new
                //{
                //    Temperature = currentTemperature,
                //    Humidity = currentHumidity
                //};

                //Smoke Sensor
                var telemetryDataPoint = new
                {
                    Temperature = currentTemperature,
                    Humidity = currentHumidity,
                    Smoke = currentSmoke,
                    Flame = currentFlame,
                    PIRState = currentPIRState,
                    Distance = currentDistance,
                    Light = currentLight
                };

                ////Flame Sensor
                //var telemetryDataPoint = new
                //{
                //    Flame = currentFlame
                //};

                var messageString = JsonSerializer.Serialize(telemetryDataPoint);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(messageString));
                message.ContentType = "application/json";
                message.ContentEncoding = "utf-8";
                await deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageString}");
                
                //Keep this value above 1000 to keep a safe buffer above the ADT service limits
                //See https://aka.ms/adt-limits for more info
                await Task.Delay(20000);
            }
        }

        public static async Task<string> ReceiveCloudToDeviceMessageAsync()
        {
            var oneSecond = TimeSpan.FromSeconds(1);
            var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);

            while (true)
            {
                var receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null)
                {
                    await Task.Delay(oneSecond);
                    continue;
                }

                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }
        }

        public static async Task ReceiveMessagesFromDeviceAsync(CancellationToken cancelToken)
        {
            try
            {
                string eventHubConnectionString = await IotHubConnection.GetEventHubsConnectionStringAsync(iotHubConnectionString);
                await using var consumerClient = new EventHubConsumerClient(
                    EventHubConsumerClient.DefaultConsumerGroupName,
                    eventHubConnectionString);

                await foreach (PartitionEvent partitionEvent in consumerClient.ReadEventsAsync(cancelToken))
                {
                    if (partitionEvent.Data == null) continue;

                    string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                    Console.WriteLine($"Message received. Partition: {partitionEvent.Partition.PartitionId} Data: '{data}'");
                }
            }
            catch (TaskCanceledException) { } // do nothing
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading event: {ex}");
            }
        }
    }
}
