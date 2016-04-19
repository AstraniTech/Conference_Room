using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using Microsoft.Azure.Devices.Client;
using System.Net;
using ppatierno.AzureSBLite;
using ppatierno.AzureSBLite.Messaging;
using ConferenceRoomsClientPOC.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ConferenceRoomsClientPOC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;
        private ObservableCollection<ConferenceRoomDataPoint> conferenceRoomDataPoint = new ObservableCollection<ConferenceRoomDataPoint>();
        ConferenceRoomDataPointList conferenceRoomDataPointList;
        
        static string ConnectionString = "Endpoint=sb://ihsuprodhkres029dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=+I95mfRw0jBMsbiYGQFGo4NF46gdUbOOyAK+T9MxQIA=";
        static string eventHubEntity = "iothub-ehub-conference-29104-8890dfe8d6";
        static string partitionId = "0";
        static DateTime startingDateTimeUtc;
        EventHubConsumerGroup group;
        EventHubClient client;
        MessagingFactory factory;
        EventHubReceiver receiver;
       
        public MainPage()
        {
            this.InitializeComponent();

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            conferenceRoomDataPointList = new ConferenceRoomDataPointList();
            roomsListView.ItemsSource = conferenceRoomDataPointList;
            roomsGridView.ItemsSource = conferenceRoomDataPointList;
            progressring.Visibility = Visibility.Visible;
            progressring.IsIndeterminate = true;
            await Task.Run(ReceiveDataFromCloud);
            progressring.Visibility = Visibility.Collapsed;
            progressring.IsIndeterminate = false;

        }

        //For demo purpose we have taken two devices/rooms and 
        //updating the collection based on the data received       
        public async Task ReceiveDataFromCloud()
        {
            startingDateTimeUtc = DateTime.UtcNow;
            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(ConnectionString);
            builder.TransportType = ppatierno.AzureSBLite.Messaging.TransportType.Amqp;

            factory = MessagingFactory.CreateFromConnectionString(ConnectionString);

            client = factory.CreateEventHubClient(eventHubEntity);
            group = client.GetDefaultConsumerGroup();
            receiver = group.CreateReceiver(partitionId.ToString(), startingDateTimeUtc);//startingDateTimeUtc
            while (true)
            {
                EventData data = receiver.Receive();

                if (data != null)
                {
                    var receiveddata = Encoding.UTF8.GetString(data.GetBytes());

                    var messageString = JsonConvert.DeserializeObject<ConferenceRoomDataPoint>(receiveddata);

                   
                    
                    if (messageString.DeviceId == "ConferenceRoom1")
                    {
                        conferenceRoomDataPointList[0].DeviceId = messageString.DeviceId;
                        conferenceRoomDataPointList[0].RoomTemp = messageString.RoomTemp;
                        conferenceRoomDataPointList[0].RoomStatus = messageString.RoomStatus;
                        conferenceRoomDataPointList[0].Color = messageString.Color;
                        conferenceRoomDataPointList[0].LightStatus = messageString.LightStatus;
                        DateTime localDateTime = DateTime.Parse(messageString.Time);
                        DateTime utcDateTime = localDateTime.ToLocalTime();
                        conferenceRoomDataPointList[0].Time = utcDateTime.ToString();
                        conferenceRoomDataPointList[0].RoomPressure = messageString.RoomPressure;
                        conferenceRoomDataPointList[0].RoomAlt = messageString.RoomAlt;

                    }
                    if (messageString.DeviceId == "ConferenceRoom2")
                    {
                        conferenceRoomDataPointList[1].DeviceId = messageString.DeviceId;
                        conferenceRoomDataPointList[1].RoomTemp = messageString.RoomTemp;
                        conferenceRoomDataPointList[1].RoomStatus = messageString.RoomStatus;
                        conferenceRoomDataPointList[1].Color = messageString.Color;
                        conferenceRoomDataPointList[1].LightStatus = messageString.LightStatus;
                        DateTime localDateTime = DateTime.Parse(messageString.Time);
                        DateTime utcDateTime = localDateTime.ToLocalTime();
                        conferenceRoomDataPointList[1].Time = utcDateTime.ToString();
                        conferenceRoomDataPointList[1].RoomPressure = messageString.RoomPressure;
                        conferenceRoomDataPointList[1].RoomAlt = messageString.RoomAlt;
                    }


                    //var message = new ConferenceRooms(Encoding.ASCII.GetBytes(receiveddata));

                    Debug.WriteLine("{0} {1} {2}", data.SequenceNumber, data.EnqueuedTimeUtc.ToLocalTime(), Encoding.UTF8.GetString(data.GetBytes()));

                }
                else
                {
                    break;
                }
                
                await Task.Delay(2000);

            }

            receiver.Close();
            client.Close();
            factory.Close();
            
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            progressring.Visibility = Visibility.Visible;
            progressring.IsIndeterminate = true;
            await Task.Run(ReceiveDataFromCloud);
            progressring.Visibility = Visibility.Collapsed;
            progressring.IsIndeterminate = false;
        }

        private void adaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
           
        }

        private void NotificationToggleSwtich_Toggled(object sender, RoutedEventArgs e)
        {
            //Code for registering to receive notifications when a Conference room is available
        }

        
    }
}
