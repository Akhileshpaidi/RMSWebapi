using ITR_TelementaryAPI.Models.SignalRTickData;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Hubs
{
    public class SignalRParamsGroupHub : Hub
    {
        private readonly SignalRParamsGroupService _service;
        public SignalRParamsGroupHub(SignalRParamsGroupService service)
        {
            _service = service;
        }
        public IEnumerable<ParameterData> SendMessage(int ParamTypeID)
        {
            return _service.GetAllData(ParamTypeID);

        }

    }
}
