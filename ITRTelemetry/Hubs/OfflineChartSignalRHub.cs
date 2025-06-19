using DomainModel;
using ITRTelemetry.Models.SignalRData;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace ITRTelemetry.Hubs
{
    public class OfflineChartSignalRHub :Hub
    {
        private readonly OfflineChartSignalRService _service;
        public OfflineChartSignalRHub(OfflineChartSignalRService service)
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
