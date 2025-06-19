using DomainModel;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Hubs
{
    public class LiveChartSignalRHub : Hub
    {
        private readonly LiveChartSignalRService _service;
        public LiveChartSignalRHub(LiveChartSignalRService service)
        {
            _service = service;
        }
        public IEnumerable<XYValues> SendMessage(int EquipmentID,int XParameterID,int YParameterID,int FlightID)
        {
            return _service.GetAllData(EquipmentID,  XParameterID, YParameterID,FlightID);

        }
    }
}
