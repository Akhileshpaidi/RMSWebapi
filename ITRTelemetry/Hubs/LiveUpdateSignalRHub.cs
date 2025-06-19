using System;
using System.Collections.Generic;
using ITR_TelementaryAPI.Models.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace ITR_TelementaryAPI.Hubs {
    public class LiveUpdateSignalRHub : Hub {
        private readonly ParameterDataService _paramData;

        public LiveUpdateSignalRHub(ParameterDataService paramData) {
            _paramData = paramData;
        }

        public IEnumerable<ParametersModel> GetParamData() {
            return _paramData.GetParamData();
        }
    }
}
