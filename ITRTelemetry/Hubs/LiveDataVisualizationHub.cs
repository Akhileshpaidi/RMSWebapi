using DomainModel;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Hubs
{
    public class LiveDataVisualizationHub : Hub
    {
        private readonly LiveDataVisualizationService _service;
        public LiveDataVisualizationHub(LiveDataVisualizationService service)
        {
            _service = service;
        }
        public IEnumerable<StationParametersData> SendMessage(int EquipmentID,int MissionID,int FlightID)
        {
            return _service.GetAllData(EquipmentID, MissionID,FlightID);

        }

        public void SQLConnection(bool con)
        {
            if (con == true)
            {
                _service.MakeSQLConnection();
            }
            else
            {
                _service.SQLDisConnect();
            }

        }

        public void StartMission()
        {
            _service.StartMission();
        }
        public void EndMission()
        {
            _service.EndMission();
        }
    }
}
