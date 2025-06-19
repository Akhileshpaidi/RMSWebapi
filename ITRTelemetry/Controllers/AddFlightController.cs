using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;

namespace ITR_TelementaryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddFlightController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public AddFlightController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        // GET: api/<AddFlightController>
        // GET: api/AddFlight
        [HttpPost]
        public IActionResult InsertFlightDetails([FromBody] FlightModel FlightModels)
        {
            var flightModels = this.mySqlDBContext.FlightModels;
            flightModels.Add(FlightModels);
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }
    }
}
