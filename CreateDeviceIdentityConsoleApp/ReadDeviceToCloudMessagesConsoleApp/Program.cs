using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace ReadDeviceToCloudMessagesConsoleApp
{
    class Program
    {
        static string connectionString = "HostName=IoTHubForConnectingDots.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=sYLZ+KzhNS11tpn9jmq5ghucGZxzlrUp/MI9gc1jvlA=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;
        static void Main(string[] args)
        {
            Console.WriteLine("Receive messages\n");
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            foreach (string partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition);
            }
            Console.ReadLine();
        }

        private async static Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine(string.Format("Message received. Partition: {0} Data: '{1}'", partition, data));
            }
        }
    }
}
