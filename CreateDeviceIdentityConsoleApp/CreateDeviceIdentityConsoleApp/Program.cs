using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace CreateDeviceIdentityConsoleApp
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=IoTHubForConnectingDots.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=sYLZ+KzhNS11tpn9jmq5ghucGZxzlrUp/MI9gc1jvlA=";
        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();
            Console.ReadLine();
        }
        private async static Task AddDeviceAsync()
        {
            string deviceId = "ConferenceRoom1";        
            Device device;       
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
