using DomainModel;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Hubs
{
    public class MultiLineSignalRData : Hub
    {
        private readonly MultilineSignalRService _service;

        public MultiLineSignalRData(MultilineSignalRService service)
        {
            _service = service;
        }

        public IEnumerable<PlotingData> GetAllData()
        {
            return _service.GetAllData();
        }

    }
}
