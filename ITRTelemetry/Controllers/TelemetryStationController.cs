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
    public class TelemetryStationController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public TelemetryStationController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        // GET: api/<TelemetryStationController>
        // GET: api/TelemetryStation
        [Route("api/TelemetryStation/GetTelemetryDetails")]
        [HttpGet]
        public IEnumerable<TelemetryStationModel> Get()
        {
            return this.mySqlDBContext.TelemetryStationModels.Where(x => x.Status == "Active").ToList();
        }

        [Route("api/TelemetryStation/InsertTelemetry")]
        [HttpPost]
        public IActionResult InsertTelemetry([FromBody] TelemetryStationModel TelemetryStationModels)
        {
            var telemetryStationModels = this.mySqlDBContext.TelemetryStationModels;
            telemetryStationModels.Add(TelemetryStationModels);

            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            TelemetryStationModels.DateTime = dt1;
            TelemetryStationModels.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();

        }

        [Route("api/TelemetryStation/UpdateTelemetry")]
        [HttpPut]
        public void UpdateTelemetry([FromBody] TelemetryStationModel TelemetryStationModels)
        {
            if (TelemetryStationModels.TSID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(TelemetryStationModels);
                this.mySqlDBContext.Entry(TelemetryStationModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(TelemetryStationModels);

                Type type = typeof(TelemetryStationModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(TelemetryStationModels, null) == null)
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
            
        }

        [Route("api/TelemetryStation/DeleteTelemetry")]
        [HttpDelete]
        public void DeleteTelemetry(int id)
        {
            var currentClass = new TelemetryStationModel { TSID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
