using System.Collections.Generic;
using ITR_TelementaryAPI.Models.SignalRTickData;
using Microsoft.AspNetCore.SignalR;

namespace ITR_TelementaryAPI.Hubs
{
    public class SignalRDataHub : Hub {

        private readonly SignalRDataService _service;

        public SignalRDataHub(SignalRDataService service) {
            _service = service;
        }

        public IEnumerable<ParameterData> GetAllData() {
            return _service.GetAllData();
        }
    }
}
