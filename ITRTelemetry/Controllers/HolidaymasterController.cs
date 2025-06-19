using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;


namespace ITRTelemetry.Controllers
{

    [Produces("application/json")]
    public class HolidaymasterController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public HolidaymasterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        [Route("api/holidaymaster/GetholidaymasterDetails")]
        [HttpGet]

        public IEnumerable<Holidaymaster> GetholidaymasterDetails()

        {
            return this.mySqlDBContext.Holidaymasters.Where(x => x.status == "Active").ToList();
        }



        [Route("api/holidaymaster/InsertholidaymasterDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Holidaymaster Holidaymasters)
        {
            try
            {
                Holidaymasters.holidayname = Holidaymasters.holidayname?.Trim();
                var existingDepartment = this.mySqlDBContext.Holidaymasters
                    .FirstOrDefault(d => d.holidayname == Holidaymasters. holidayname && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                var maxholidayId = this.mySqlDBContext.Holidaymasters
        .Where(d => d.source == "No")
       .Max(d => (int?)d.holidayid) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                Holidaymasters.holidayid = maxholidayId + 1;

                var Holidaymaster = this.mySqlDBContext.Holidaymasters;
                            Holidaymaster.Add(Holidaymasters);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Holidaymasters.createddate = dt1;
                Holidaymasters.status = "Active";
                Holidaymasters.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/holidaymaster/UpdateholidaymasterDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] Holidaymaster Holidaymasters)
        {
            try
            {
                if (Holidaymasters.holidayid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Holidaymasters.holidayname = Holidaymasters.holidayname?.Trim();
                    var existingDepartment = this.mySqlDBContext.Holidaymasters
                  .FirstOrDefault(d => d.holidayname == Holidaymasters.holidayname && d.holidayid != Holidaymasters.holidayid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Holidaymasters);
                    this.mySqlDBContext.Entry(Holidaymasters).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Holidaymasters);

                    Type type = typeof(Holidaymaster);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Holidaymasters, null) == null || property.GetValue(Holidaymasters, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/holidaymaster/DeleteholidaymasterDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new Holidaymaster { holidayid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
