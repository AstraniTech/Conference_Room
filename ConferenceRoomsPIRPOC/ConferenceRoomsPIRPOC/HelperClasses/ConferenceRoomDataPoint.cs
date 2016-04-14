using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConferenceRoomsPIRPOC
{
    public class ConferenceRoomDataPoint
    {
        public string DeviceId { get; set; }
        public string RoomStatus { get; set; }

        public string RoomTemp { get; set; }

        public string RoomPressure { get; set; }

        public string RoomAlt { get; set; }
        public string Color { get; set; }
        public string Time { get; set; }
        public string LightStatus { get; internal set; }
        public string LightCDSValue { get; internal set; }
        public string LightCDSVoltageValue { get; internal set; }
    }
}
