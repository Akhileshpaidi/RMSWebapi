using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Hubs
{
    public class SignalRPriorityHub : Hub
    {
        private readonly SignalRPriorityService _service;

        public SignalRPriorityHub(SignalRPriorityService service)
        {
            _service = service;
        }

        public bool GetAllData(int EID,int MID)
        {
            return _service.GetAllData(EID,MID);
        }
    }
}
