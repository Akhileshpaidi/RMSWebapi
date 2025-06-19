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
using MySqlX.XDevAPI.Common;


namespace ITRTelemetry.Controllers
{

    [Produces("application/json")]
    public class industrytypeController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public industrytypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        [Route("api/industrytype/GetindustrytypeDetails")]
        [HttpGet]

        public IEnumerable<industrytypemodel> GetindustrytypeDetails()
        {
            return this.mySqlDBContext.industrytypemodels.Where(x => x.status == "Active").ToList();
        }


        [Route("api/industrytype/GetindustryByBusinessID/{businessIDs}")]
        [HttpGet]
        public IActionResult GetIndustryByBusinessIDs(string businessIDs)
        {
            var businessIDsList = businessIDs.Split(',').Select(int.Parse).ToList();

            var industries = mySqlDBContext.industrytypemodels
                .Where(industry => businessIDsList.Contains(industry.businesssectorid) && industry.status == "Active")
                .ToList();

            return Ok(industries);
        }

        [Route("api/industrytype/InsertindustrytypeDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] industrytypemodel industrytypemodels)
        {
            try
            {
                industrytypemodels.industrytypename = industrytypemodels.industrytypename?.Trim();
                var existingDepartment = this.mySqlDBContext.industrytypemodels
                    .FirstOrDefault(d => d.industrytypename== industrytypemodels.industrytypename && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                var maxindustryId = this.mySqlDBContext.industrytypemodels
                  .Where(d => d.source == "No")
                 .Max(d => (int?)d.industrytypeid) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                industrytypemodels.industrytypeid = maxindustryId + 1;

                var industrytypemodel = this.mySqlDBContext.industrytypemodels;
              
                industrytypemodel.Add(industrytypemodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                industrytypemodels. createddate = dt1;
                industrytypemodels.status = "Active";
                industrytypemodels.source = "No";
                industrytypemodels.industrytyptable = "industrytype";
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

        [Route("api/industrytype/UpdateindustrytypeyDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] industrytypemodel industrytypemodels)
        {
            try
            {
                if (industrytypemodels.industrytypeid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    industrytypemodels.industrytypename = industrytypemodels.industrytypename?.Trim();
                    var existingDepartment = this.mySqlDBContext.industrytypemodels
                  .FirstOrDefault(d => d.industrytypename == industrytypemodels.industrytypename && d.industrytypeid != industrytypemodels.industrytypeid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: TypeName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(industrytypemodels);
                    this.mySqlDBContext.Entry(industrytypemodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(industrytypemodels);

                    Type type = typeof(industrytypemodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(industrytypemodels, null) == null || property.GetValue(industrytypemodels, null).Equals(0))
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



        [Route("api/industrytype/DeleteindustrytypeDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new industrytypemodel { industrytypeid = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




    }
}
