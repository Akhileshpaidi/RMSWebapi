using System.Collections.Generic;
using DomainModel;
using ITR_TelementaryAPI.Models.SignalRTickData;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
namespace ITRTelemetry.Hubs
{
    public class OfflineSignalRDataHub : Hub
    {
        private readonly OfflineSignalRDataService _service;
        public OfflineSignalRDataHub(OfflineSignalRDataService service)
        {
            _service = service;
        }
        public void SendMessage()
        {
             _service.GetAllData();

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
