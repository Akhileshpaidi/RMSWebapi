using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;

namespace ITR_TelementaryAPI.Controllers
{
    [Produces("application/json")]
    public class GetFlightController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public GetFlightController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/GetFlight/GetFlightDetails")]
        [HttpGet]
        public IEnumerable<FlightModel> Get()
        {
            return this.mySqlDBContext.FlightModels.Where(x => x.Status == "Active").ToList();
        }



        [Route("api/GetFlight/GetFlightByMission")]
        [HttpGet]
        public IEnumerable<FlightModel> GetByMission(int ID)
        {
            return this.mySqlDBContext.FlightModels.Where(x => x.MissionID == ID).ToList();
        }

        [Route("api/GetFlight/InsertFlight")]
        [HttpPost]
        public IActionResult InsertFlight([FromBody] FlightModel FlightModels)
        {
            var flightModels = this.mySqlDBContext.FlightModels;
            flightModels.Add(FlightModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            FlightModels.DateTime = dt1;
            FlightModels.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/GetFlight/UpdateFlight")]
        [HttpPut]
        public void UpdateFlight([FromBody] FlightModel FlightModels)
        {
            if (FlightModels.FlightID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(FlightModels);
                this.mySqlDBContext.Entry(FlightModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(FlightModels);

                Type type = typeof(FlightModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(FlightModels, null) == null || property.GetValue(FlightModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
           
        }

        [Route("api/GetFlight/DeleteFlight")]
        [HttpDelete]
        public void DeleteFlight(int id)
        {
            var currentClass = new FlightModel { FlightID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        
        }
}
