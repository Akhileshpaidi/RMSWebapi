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
    public class BusinesssectorController : Controller
    {
        private CommonDBContext commonDBContext;
        private MySqlDBContext mySqlDBContext;

        public BusinesssectorController(MySqlDBContext mySqlDBContext, CommonDBContext commonDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
            this.commonDBContext = commonDBContext;
        }

        [Route("api/Businesssector/GetBusinesssectorDetails")]
        [HttpGet]

        public IEnumerable<Businesssectormodel> GetBusinesssectorDetails()
        {
            return this.mySqlDBContext.Businesssectormodels.Where(x => x.status == "Active").ToList();
        }



        [Route("api/Businesssector/InsertBusinesssectorDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Businesssectormodel Businesssectormodels)
        {
            try
            {
                Businesssectormodels.businesssectorname = Businesssectormodels.businesssectorname?.Trim();
                var existingDepartment = this.mySqlDBContext.Businesssectormodels
                    .FirstOrDefault(d => d.businesssectorname == Businesssectormodels.businesssectorname && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion
                var maxBusinessId = this.mySqlDBContext.Businesssectormodels
            .Where(d => d.source == "No")
           .Max(d => (int?)d.businesssectorid) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                Businesssectormodels.businesssectorid = maxBusinessId + 1;
                var Businesssectormodel = this.mySqlDBContext.Businesssectormodels;
              

                Businesssectormodel.Add(Businesssectormodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Businesssectormodels.createddate = dt1;
                Businesssectormodels.status = "Active";
                Businesssectormodels.source = "No";

                Businesssectormodels.businesssectortable = "Businesssector";
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

        [Route("api/Businesssector/UpdateBusinesssectorDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] Businesssectormodel Businesssectormodels)
        {
            try
            {
                if (Businesssectormodels.businesssectorid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Businesssectormodels.businesssectorname = Businesssectormodels.businesssectorname?.Trim();
                    var existingDepartment = this.mySqlDBContext.Businesssectormodels
                  .FirstOrDefault(d => d.businesssectorname == Businesssectormodels.businesssectorname && d.businesssectorid != Businesssectormodels.businesssectorid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Businesssectormodels);
                    this.mySqlDBContext.Entry(Businesssectormodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Businesssectormodels);

                    Type type = typeof(Businesssectormodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Businesssectormodels, null) == null || property.GetValue(Businesssectormodels, null).Equals(0))
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



        [Route("api/businesssector/DeletebusinesssectorDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new Businesssectormodel{ businesssectorid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



    }
}
