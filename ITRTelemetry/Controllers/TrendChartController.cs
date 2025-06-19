using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITR_TelementaryAPI.Controllers;
using ITRTelemetry.DataStorage;
using ITRTelemetry.HubConfig;
using ITRTelemetry.TimerFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MySQLProvider;

namespace ITRTelemetry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrendChartController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;


        private IHubContext<TrendChartHub> _hub;
        public TrendChartController(MySqlDBContext mySqlDBContext, IHubContext<TrendChartHub> hub)
        {
            _hub = hub;

            this.mySqlDBContext = mySqlDBContext;
        }
        public IActionResult Get()
        {
            var timerManager = new TimerManager(() => _hub.Clients.All.SendAsync("transfertrendchartdata", GetPacketData()));
            return Ok(new { Message = "Request Completed" });
        }

        public object GetPacketData()
        {
            var result = (from e in this.mySqlDBContext.EquipmentParameterDetailModels
                          join d
                          in this.mySqlDBContext.EquipmentParameterModels on e.ParameterID equals d.ParameterID
                          join f
                          in this.mySqlDBContext.PacketModels on e.PacketMasterID equals f.PacketMasterID
                          select new
                          {
                              PacketID = f.PacketID,
                              ParameterID = d.ParameterID,
                              ParamData = e.ParamData,
                              ParameterName = d.ParameterName,
                              FlightID = e.FlightID,
                              Time = f.Hours.ToString() + ":" + f.Minutes.ToString() + ":" + f.Seconds.ToString() + ":" + f.MilliSeconds.ToString(),
                          }).ToList();
            return result;
        }
    }
}


    