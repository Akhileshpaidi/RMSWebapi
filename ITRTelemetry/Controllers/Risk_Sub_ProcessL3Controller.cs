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
    public class Risk_Sub_ProcessL3Controller : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Risk_Sub_ProcessL3Controller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/BusinessSubProcessL3/GetBusinessSubProcesssL3")]
        [HttpGet]

        public IEnumerable<Risk_Sub_ProcessL3> GetBusinessSubProcesssL3()
        {
            return this.mySqlDBContext.Risk_Sub_ProcessL3s.Where(x => x.ProcessL3Status == "Active").ToList();
        }
        [Route("api/BusinessSubProcessL3/InsertBusinessSubProcessL3")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Risk_Sub_ProcessL3 Risk_Sub_ProcessL3s)
        {
            try
            {
                Risk_Sub_ProcessL3s.BusinessSubProcessL3Name = Risk_Sub_ProcessL3s.BusinessSubProcessL3Name?.Trim();

                var existingDepartment = this.mySqlDBContext.Risk_Sub_ProcessL3s
                    .FirstOrDefault(d => d.BusinessSubProcessL3Name == Risk_Sub_ProcessL3s.BusinessSubProcessL3Name && d.BusinessProcessL3ID == Risk_Sub_ProcessL3s.BusinessProcessL3ID && d.ProcessL3Status == "Active");

                if (existingDepartment != null)
                {
                    return BadRequest("Error:  Business Sub-ProcessL3 with the same name already exists.");
                }

                var Risk_Sub_ProcessL3 = this.mySqlDBContext.Risk_Sub_ProcessL3s;
                Risk_Sub_ProcessL3.Add(Risk_Sub_ProcessL3s);
                //DateTime dt = DateTime.Now;
                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                //AuthorityNameModels.Authority_CreatedDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Risk_Sub_ProcessL3s.ProcessL3Status = "Active";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:  Business Sub-ProcessL3 with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
        [Route("api/BusinessSubProcessL3/UpdateBusinessSubProcessL3")]
        [HttpPut]
        public IActionResult UpdateBusiness([FromBody] Risk_Sub_ProcessL3 Risk_Sub_ProcessL3s)
        {
            try
            {
                if (Risk_Sub_ProcessL3s.BusinessProcessL3ID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Risk_Sub_ProcessL3s.BusinessSubProcessL3Name = Risk_Sub_ProcessL3s.BusinessSubProcessL3Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.Risk_Sub_ProcessL3s
                     .FirstOrDefault(d => d.BusinessSubProcessL3Name == Risk_Sub_ProcessL3s.BusinessSubProcessL3Name && d.BusinessProcessL3ID != Risk_Sub_ProcessL3s.BusinessProcessL3ID && d.ProcessL3Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Sub-ProcessL3 with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Risk_Sub_ProcessL3s);
                    this.mySqlDBContext.Entry(Risk_Sub_ProcessL3s).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Risk_Sub_ProcessL3s);

                    Type type = typeof(Risk_Sub_ProcessL3);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Risk_Sub_ProcessL3s, null) == null || property.GetValue(Risk_Sub_ProcessL3s, null).Equals(0))
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
                    return BadRequest("Error:  Business Sub-ProcessL3 with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/BusinessSubProcessL3/DeleteBusinessSubProcessL3")]
        [HttpDelete]
        public void DeleteBusinessSubProcessL3(int id)
        {
            var currentClass = new Risk_Sub_ProcessL3 { BusinessProcessL3ID = id };
            currentClass.ProcessL3Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("ProcessL3Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
