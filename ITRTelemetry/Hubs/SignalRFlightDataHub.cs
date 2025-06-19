using ITR_TelementaryAPI.Models.SignalRTickData;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Hubs
{
    public class SignalRFlightDataHub : Hub
    {
        private readonly SignalRFlightService _service;
        public SignalRFlightDataHub(SignalRFlightService service)
        {
            _service = service;
        }
        public IEnumerable<ParameterData> SendMessage(int ParameterTypeID,string FlightID,int frequency)
        {
            return _service.GetAllData(ParameterTypeID, FlightID, frequency);

        }

    }
}
