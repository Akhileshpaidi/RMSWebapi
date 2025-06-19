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
    public class frequencymasterController : Controller
    {
        private MySqlDBContext mySqlDBContext;

        public frequencymasterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/frequencymaster/GetfrequencymasterDetails")]
        [HttpGet]

        public IEnumerable<Frequencymodel> GetfrequencymasterDetails()
        {
            return this.mySqlDBContext.Frequencymodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/frequencymaster/GetnofrequencyIntervals/{frequencyid}")]
        [HttpGet]
        public IEnumerable<int> GetnofrequencyIntervals(int frequencyid)
        {
            var details = (from frequence in mySqlDBContext.Frequencymodels
                           where frequence.status == "Active" && frequence.frequencyid == frequencyid
                           select frequence.nooffrequencyintervals)
                         .ToList();
            return details;
        }

        [Route("api/frequencymaster/GetfrequencyDetails")]
        [HttpGet]

        public IEnumerable<object> GetfrequencyDetails()
        {
           var details=(from  frequence  in mySqlDBContext.Frequencymodels
                        where frequence.status =="Active"
                        select new
                        {
                            frequence.frequencyid,
                            frequence.frequencyperiod
                        })
                        .ToList();
            return details;
        }


        [Route("api/frequencymaster/InsertfrequencymasterDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Frequencymodel Frequencymodels)
        {
            try
            {
                Frequencymodels.frequencyperiod = Frequencymodels.frequencyperiod?.Trim();
                var existingDepartment = this.mySqlDBContext.Frequencymodels
                    .FirstOrDefault(d => d.frequencyperiod == Frequencymodels.frequencyperiod && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }

                var maxfrequencyId = this.mySqlDBContext.Frequencymodels
            .Where(d => d.source == "No")
           .Max(d => (int?)d.frequencyid) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                Frequencymodels.frequencyid = maxfrequencyId + 1;
                var Frequencymodel = this.mySqlDBContext.Frequencymodels;
             
                Frequencymodel.Add(Frequencymodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Frequencymodels.createddate = dt1;
                Frequencymodels.status = "Active";
                Frequencymodels.source = "No";

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

        [Route("api/frequencymaster/UpdatefrequencymasterDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] Frequencymodel Frequencymodels)
        {
            try
            {
                if (Frequencymodels.frequencyid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Frequencymodels.frequencyperiod = Frequencymodels.frequencyperiod?.Trim();
                    var existingDepartment = this.mySqlDBContext.Frequencymodels
                  .FirstOrDefault(d => d.frequencyperiod == Frequencymodels.frequencyperiod && d.frequencyid != Frequencymodels.frequencyid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Frequencymodels);
                    this.mySqlDBContext.Entry(Frequencymodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Frequencymodels);

                    Type type = typeof(Frequencymodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Frequencymodels, null) == null || property.GetValue(Frequencymodels, null).Equals(0))
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



        [Route("api/frequencymaster/DeletefrequencymasterDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new Frequencymodel { frequencyid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
