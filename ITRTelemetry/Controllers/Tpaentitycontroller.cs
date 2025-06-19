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
    public class Tpaentitycontroller:ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public Tpaentitycontroller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/tpaentity/GettpaentityMasterDetails")]
        [HttpGet]

        public IEnumerable<TPAEntitymodel> GettpaentityMasterDetails()
        {
            return this.mySqlDBContext.TPAEntitymodels.Where(x => x.status == "Active").ToList();
        }


        [Route("api/tpaentity/GettpaentityMasterbyid/{tpaenityid}")]
        [HttpGet]
        public IEnumerable<object> GetTpaEntityMasterById(int tpaenityid)
        {
            var details = from tpaentity in mySqlDBContext.TPAEntitymodels
                          where tpaentity.status == "Active" && tpaentity.tpaenityid == tpaenityid
                          select new 
                          {
                              tpaenityid = tpaentity.tpaenityid,
                              tpaenityname = tpaentity.tpaenityname,
                              tpadescription = tpaentity.tpadescription
                          };

            return details.ToList();
        }



        [Route("api/tpaentity/InserttpaentityDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] TPAEntitymodel TPAEntitymodels)
        {
            try
            {
                TPAEntitymodels.tpaenityname = TPAEntitymodels.tpaenityname?.Trim();
                var existingDepartment = this.mySqlDBContext.TPAEntitymodels
                    .FirstOrDefault(d => d.tpaenityname == TPAEntitymodels.tpaenityname && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion
                var TPAEntitymodel = this.mySqlDBContext.TPAEntitymodels;
                TPAEntitymodel.Add(TPAEntitymodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                TPAEntitymodels.createddate  = dt1;
                TPAEntitymodels.status = "Active";
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

        [Route("api/tpaentity/UpdatetpaentityDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] TPAEntitymodel TPAEntitymodels)
        {
            try
            {
                if (TPAEntitymodels.tpaenityid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    TPAEntitymodels. tpaenityname= TPAEntitymodels.tpaenityname?.Trim();
                    var existingDepartment = this.mySqlDBContext.TPAEntitymodels
                  .FirstOrDefault(d => d.tpaenityname == TPAEntitymodels.tpaenityname && d.tpaenityid != TPAEntitymodels.tpaenityid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: TypeName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(TPAEntitymodels);
                    this.mySqlDBContext.Entry(TPAEntitymodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(TPAEntitymodels);

                    Type type = typeof(TPAEntitymodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(TPAEntitymodels, null) == null || property.GetValue(TPAEntitymodels, null).Equals(0))
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



        [Route("api/tpaentity/DeletetpaentityDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new TPAEntitymodel { tpaenityid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}

