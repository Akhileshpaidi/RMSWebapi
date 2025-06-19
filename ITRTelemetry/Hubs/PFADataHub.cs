using System;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITRTelemetry.Models.SignalRData;
using DomainModel;
using ITR_TelementaryAPI.Models.SignalRTickData;

namespace ITRTelemetry.Hubs
{
    public class PFADataHub : Hub
    {
        private readonly PFADataService _service;

        public PFADataHub(PFADataService service)
        {
            _service = service;
        }

        public IEnumerable<SystemParamData> SendMessage()
        {
            return _service.GetAllData();

        }
    }
}
