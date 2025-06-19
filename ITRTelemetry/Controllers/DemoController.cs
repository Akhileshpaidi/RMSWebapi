using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITRTelemetry.DataStorage;
using ITRTelemetry.HubConfig;
using ITRTelemetry.TimerFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ITRTelemetry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private IHubContext<ChartHub> _hub;
        public DemoController(IHubContext<ChartHub> hub)
        {
            _hub = hub;
        }
        public IActionResult Get()
        {
            var timerManager = new TimerManager(() => _hub.Clients.All.SendAsync("transferdemochartdata", DataManager.GetDemoData()));
            return Ok(new { Message = "Request Completed" });
        }
    }
}
