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
        //Replace stars with your hostname and shared access key
        static string connectionString = "HostName=**********************.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=*****************************";
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
