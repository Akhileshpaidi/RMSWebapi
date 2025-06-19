using System.Collections.Generic;
using System.Threading.Tasks;
using ITR_TelementaryAPI.Models.SignalR;
using ITR_TelementaryAPI.Models.SignalRTickData;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;

namespace ITRTelemetry.Hubs
{
    public class SignalRParametersHub : Hub
    {
        private readonly SignalRParameterDataService _service;
        public SignalRParametersHub(SignalRParameterDataService service)
        {
            _service = service;
        }
        public IEnumerable<ParameterData> SendMessage(int ParameterID)
        {
            return _service.GetAllData(ParameterID);
            
        }


    }

}
