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
    public class Risk_Sub_ProcessL1Controller : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Risk_Sub_ProcessL1Controller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/BusinessSubProcessL1/GetBusinessSubProcesssL1")]
        [HttpGet]

        public IEnumerable<Risk_Sub_ProcessL1> GetBusinessSubProcesssL1()
        {
            return this.mySqlDBContext.Risk_Sub_ProcessL1s.Where(x => x.ProcessL1Status == "Active").ToList();
        }
        [Route("api/BusinessSubProcessL1/InsertBusinessSubProcessL1")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Risk_Sub_ProcessL1 Risk_Sub_ProcessL1s)
        {
            try
            {
                Risk_Sub_ProcessL1s.BusinessSubProcessL1Name = Risk_Sub_ProcessL1s.BusinessSubProcessL1Name?.Trim();

                var existingDepartment = this.mySqlDBContext.Risk_Sub_ProcessL1s
                    .FirstOrDefault(d => d.BusinessSubProcessL1Name == Risk_Sub_ProcessL1s.BusinessSubProcessL1Name && d.BusinessProcessL1ID == Risk_Sub_ProcessL1s.BusinessProcessL1ID && d.ProcessL1Status == "Active");

                if (existingDepartment != null)
                {
                    return BadRequest("Error:  Business Sub-ProcessL1 with the same name already exists.");
                }

                var Risk_Sub_ProcessL1 = this.mySqlDBContext.Risk_Sub_ProcessL1s;
                Risk_Sub_ProcessL1.Add(Risk_Sub_ProcessL1s);
                //DateTime dt = DateTime.Now;
                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                //AuthorityNameModels.Authority_CreatedDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Risk_Sub_ProcessL1s.ProcessL1Status = "Active";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:  Business Sub-ProcessL1 with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
        [Route("api/BusinessSubProcessL1/UpdateBusinessSubProcessL1")]
        [HttpPut]
        public IActionResult UpdateBusiness([FromBody] Risk_Sub_ProcessL1 Risk_Sub_ProcessL1s)
        {
            try
            {
                if (Risk_Sub_ProcessL1s.BusinessProcessL1ID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Risk_Sub_ProcessL1s.BusinessSubProcessL1Name = Risk_Sub_ProcessL1s.BusinessSubProcessL1Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.Risk_Sub_ProcessL1s
                     .FirstOrDefault(d => d.BusinessSubProcessL1Name == Risk_Sub_ProcessL1s.BusinessSubProcessL1Name && d.BusinessProcessL1ID != Risk_Sub_ProcessL1s.BusinessProcessL1ID && d.ProcessL1Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Sub-ProcessL1 with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Risk_Sub_ProcessL1s);
                    this.mySqlDBContext.Entry(Risk_Sub_ProcessL1s).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Risk_Sub_ProcessL1s);

                    Type type = typeof(Risk_Sub_ProcessL1);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Risk_Sub_ProcessL1s, null) == null || property.GetValue(Risk_Sub_ProcessL1s, null).Equals(0))
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
                    return BadRequest("Error:  Business Sub-ProcessL1 with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/BusinessSubProcessL1/DeleteBusinessSubProcessL1")]
        [HttpDelete]
        public void DeleteBusinessSubProcessL1(int id)
        {
            var currentClass = new Risk_Sub_ProcessL1 { BusinessProcessL1ID = id};
            currentClass.ProcessL1Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("ProcessL1Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }

}
