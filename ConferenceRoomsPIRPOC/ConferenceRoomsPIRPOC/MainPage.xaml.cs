
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ConferenceRoomsPIRPOC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static DeviceClient deviceClient;
        //static string iotHubUri = "IoTHubForConnectingDots.azure-devices.net";
        static string iotHubUri = "ConferenceRooms.azure-devices.net";
        //static string deviceName = "astraniDevice";
        //static string deviceKey = "efG89i9ThXrSLubo9EVV3BROgrByS0yyHIY1YdWtvLw=";
        //static string deviceName = "ConferenceRoom1";
        //static string deviceKey = "PCjEwRjfA8g+g5Eq+Ok7pAVMZqqB7z99+rCxJ8UiBg4=";
        //static string deviceName = "ConferenceRoom2";
        //static string deviceKey = "2V1fE1z1paziQXTSAe/ejuuhMV6B9KbDYaXcfX9Gj/o=";

        //static string deviceName = "ConferenceRoom2";
        //static string deviceKey = "l1jJcd8TOZvUpJshkdI6WwLsesLHK2mpTGibnfIV08k=";

        static string deviceName = "ConferenceRoom1";
        static string deviceKey = "27GxYIaiFD/z6g/pPxr/V/1cqt5lAmsZ8riFHIAqDwI=";

        private const int ledPin = 5;
        private const int pirPin = 6;

        private GpioPin led;
        private GpioPin pir;

        private DispatcherTimer timer;
        public static bool  sendstatus;
        
        //Temp code

        //A class which wraps the barometric sensor
        BMP280 BMP280;

        // Use for configuration of the MCP3008 class voltage formula
        const float ReferenceVoltage = 5.0F;

        // Values for which channels we will be using from the ADC chip
        const byte LowPotentiometerADCChannel = 0;
        const byte HighPotentiometerADCChannel = 1;
        const byte CDSADCChannel = 2;

        // Some strings to let us know the current state.
        const string JustRightLightString = "Bright";//Ah, just right
        const string LowLightString = "Dark";//I need a light
        const string HighLightString = "Too Bright";

        // Some internal state information
        enum eState { unknown, JustRight, TooBright, TooDark };
        eState CurrentState = eState.unknown;

        // Our ADC Chip class
        MCP3008 mcp3008 = new MCP3008(ReferenceVoltage);       

        public MainPage()
        {
            this.InitializeComponent();          
            
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey), TransportType.Http1);

            //var deviceClient = DeviceClient.Create(iotHubUri,
            //            AuthenticationMethodFactory.
            //            CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
            //        TransportType.Http1);

            InitGPIO();

            if (led != null)
            {
                timer.Start();
            }
            Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation eas = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            Debug.WriteLine(eas.FriendlyName);

            //Create a new object for our barometric sensor class 
            BMP280 = new BMP280();
            //Initialize the sensor
            await BMP280.Initialize();

            // Initialize the ADC chip for use
            mcp3008.Initialize();
            // var devices= NetworkInformation.GetHostNames();
        }
        //private async static Task AddDeviceAsync()
        //{
        //    string deviceId = "astraniDevice";
        //    Device device;
        //    //var device1 = HttpContext.Current.Server.MachineName;           
        //    try
        //    {
        //        device = await registryManager.AddDeviceAsync(new Device(deviceId));
        //    }
        //    catch (DeviceAlreadyExistsException)
        //    {
        //        device = await registryManager.GetDeviceAsync(deviceId);
        //    }
        //    Debug.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        //}
        private void InitGPIO()
        {
            // get the GPIO controller
            var gpio = GpioController.GetDefault();

            // return an error if there is no gpio controller
            if (gpio == null)
            {
                led = null;
                Debug.WriteLine("There is no GPIO controller.");
                return;
            }

            // set up the LED on the defined GPIO pin
            // and set it to High to turn off the LED
            led = gpio.OpenPin(ledPin);
            led.Write(GpioPinValue.High);
            led.SetDriveMode(GpioPinDriveMode.Output);

            // set up the PIR sensor's signal on the defined GPIO pin
            // and set it's initial value to Low
            pir = gpio.OpenPin(pirPin);
            pir.SetDriveMode(GpioPinDriveMode.Input);

            Debug.WriteLine("GPIO pins initialized correctly.");
        }

        private async void Timer_Tick(object sender, object e)
        {

            //Reading Temperature/Pressure/Altitude Information from BMP280 Sensor        
            var BMP280SensorData = await ReadBMP280SensorData();

            Debug.WriteLine("BMP280 Sensor data\n Temperature:{0}, \nPressure:{1}, \nAltitude:{2}",
                BMP280SensorData.Temperature, BMP280SensorData.Pressure, BMP280SensorData.Altitude);

            //Reading Lighting Information from Sensor  
            var MCP3008SensorData = ReadLightStatusInRoom();

            Debug.WriteLine("Light Status in Room: " + MCP3008SensorData.lightStatus);

            // read the signal from the PIR sensor
            // if it is high, then motion was detected
            if (pir.Read() == GpioPinValue.High)
            {
                // turn on the LED
                led.Write(GpioPinValue.Low);

                // update the sensor status in the UI
                Debug.WriteLine("Motion detected!");


                //Sending Message to IoT Hub
                SendDeviceToCloudMessagesAsync("Occupied", BMP280SensorData, MCP3008SensorData);               
                
            }
            else
            {
                // turn off the LED
                led.Write(GpioPinValue.High);

                // update the sensor status in the UI
                Debug.WriteLine("No motion detected.");

                //Sending Message to IoT Hub
                SendDeviceToCloudMessagesAsync("Not Occupied", BMP280SensorData, MCP3008SensorData);               
               
            }
        }
        // sendstatus=false;
        private async void SendDeviceToCloudMessagesAsync(string status,BMP280SensorData BMP280SensorData, MCP3008SensorData MCP3008SensorData)
        {     

            ConferenceRoomDataPoint conferenceRoomDataPoint = new ConferenceRoomDataPoint()
            {
                DeviceId = deviceName,
                Time = DateTime.UtcNow.ToString("o"),
                RoomTemp = BMP280SensorData.Temperature.ToString(),
                RoomPressure = BMP280SensorData.Pressure.ToString(),
                RoomAlt = BMP280SensorData.Altitude.ToString(),
                LightStatus = MCP3008SensorData.lightStatus,
                LightCDSValue= MCP3008SensorData.cdsReadVal.ToString(),
                LightCDSVoltageValue= MCP3008SensorData.cdsVoltage.ToString(),
                RoomStatus = status
            };

            if (status == "Occupied")
            {
                conferenceRoomDataPoint.Color = "Red";
            }
            else
            {
                conferenceRoomDataPoint.Color = "Green";
            }

            var jsonString = JsonConvert.SerializeObject(conferenceRoomDataPoint);
            var jsonStringInBytes = new Message(Encoding.ASCII.GetBytes(jsonString));

            await deviceClient.SendEventAsync(jsonStringInBytes);
            Debug.WriteLine("{0} > Sending message: {1}", DateTime.UtcNow, jsonString);
        }

        private async Task<BMP280SensorData> ReadBMP280SensorData()
        {
            var sensorData = new BMP280SensorData();
            try
            {
                //Create a constant for pressure at sea level.
                //This is based on your local sea level pressure (Unit: Hectopascal)
                const float seaLevelPressure = 1013.25f;

                sensorData.Temperature = await BMP280.ReadTemperature();
                sensorData.Pressure = await BMP280.ReadPreasure();
                sensorData.Altitude = await BMP280.ReadAltitude(seaLevelPressure);


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

            }

            return sensorData;
        }

        private MCP3008SensorData ReadLightStatusInRoom()
        {
            var MCP3008SensorData = new MCP3008SensorData();
            try
            {
                if (mcp3008 == null)
                {
                    Debug.WriteLine("Light Sensor data is not ready");
                    MCP3008SensorData.lightStatus = "N/A";
                    return MCP3008SensorData;
                }

                // The new light state, assume it's just right to start.
                eState newState = eState.JustRight;

                // Read from the ADC chip the current values of the two pots and the photo cell.
                MCP3008SensorData.lowPotReadVal = mcp3008.ReadADC(LowPotentiometerADCChannel);
                MCP3008SensorData.highPotReadVal = mcp3008.ReadADC(HighPotentiometerADCChannel);
                MCP3008SensorData.cdsReadVal = mcp3008.ReadADC(CDSADCChannel);

                // convert the ADC readings to voltages to make them more friendly.
                MCP3008SensorData.lowPotVoltage = mcp3008.ADCToVoltage(MCP3008SensorData.lowPotReadVal);
                MCP3008SensorData.highPotVoltage = mcp3008.ADCToVoltage(MCP3008SensorData.highPotReadVal);
                MCP3008SensorData.cdsVoltage = mcp3008.ADCToVoltage(MCP3008SensorData.cdsReadVal);

                // Let us know what was read in.
                Debug.WriteLine(String.Format("Read values {0}, {1}, {2} ", MCP3008SensorData.lowPotReadVal,
                    MCP3008SensorData.highPotReadVal, MCP3008SensorData.cdsReadVal));
                Debug.WriteLine(String.Format("Voltages {0}, {1}, {2} ", MCP3008SensorData.lowPotVoltage,
                    MCP3008SensorData.highPotVoltage, MCP3008SensorData.cdsVoltage));

                // Compute the new state by first checking if the light level is too low
                if (MCP3008SensorData.cdsVoltage < MCP3008SensorData.lowPotVoltage)
                {
                    newState = eState.TooDark;
                }

                // And now check if it too high.
                if (MCP3008SensorData.cdsVoltage > MCP3008SensorData.highPotVoltage)
                {
                    newState = eState.TooBright;

                }

                // Use another method to determine what to do with the state.
                MCP3008SensorData.lightStatus = CheckForStateValue(newState);
                return MCP3008SensorData;
            }
            catch (Exception)
            {
                MCP3008SensorData.lightStatus = "N/A";
                return MCP3008SensorData;
            }

        }

        private string CheckForStateValue(eState newState)
        {
            String lightStatus;

            switch (newState)
            {
                case eState.JustRight:
                    {
                        lightStatus = JustRightLightString;
                    }
                    break;

                case eState.TooBright:
                    {
                        lightStatus = HighLightString;
                    }
                    break;

                case eState.TooDark:
                    {
                        lightStatus = LowLightString;
                    }
                    break;

                default:
                    {
                        lightStatus = "N/A";
                    }
                    break;
            }
            
            return lightStatus;
        }


     
    }
}
