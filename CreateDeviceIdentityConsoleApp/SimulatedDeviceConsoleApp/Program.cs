using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace SimulatedDeviceConsoleApp
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "IoTHubForConnectingDots.azure-devices.net";
        static string deviceKey = "efG89i9ThXrSLubo9EVV3BROgrByS0yyHIY1YdWtvLw=";
        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("astraniDevice", deviceKey));

            SendDeviceToCloudMessagesAsync();
            //ReceiveC2dAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();

            while (true)
            {
                double currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

                var telemetryDataPoint = new
                {
                    deviceId = "astraniDevice",
                    windSpeed = currentWindSpeed
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Thread.Sleep(1000);
            }
        }

        //private static async void ReceiveC2dAsync()
        //{
        //    Console.WriteLine("\nReceiving cloud to device messages from service");
        //    while (true)
        //    {
        //        Message receivedMessage = await deviceClient.ReceiveAsync();
        //        if (receivedMessage == null) continue;

        //        Console.ForegroundColor = ConsoleColor.Yellow;
        //        Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
        //        Console.ResetColor();

        //        await deviceClient.CompleteAsync(receivedMessage);
        //    }
        //}
    }
}
