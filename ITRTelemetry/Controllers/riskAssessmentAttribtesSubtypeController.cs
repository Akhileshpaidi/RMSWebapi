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
    public class riskAssessmentAttribtesSubtypeController : Controller
    {

        private readonly MySqlDBContext mySqlDBContext;

        public riskAssessmentAttribtesSubtypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        //Getting Type Details by TypeID

        [Route("api/assessmentsubtype/GetAssessmenttemplateSubTypeDetails")]
        [HttpGet]

        public IEnumerable<riskAssessmenttemplatesubtypemodel> GetSubTypeDetails()
        {

            return this.mySqlDBContext.riskAssessmenttemplatesubtypemodels.Where(x => x.status == "Active").ToList();
        }


        [Route("api/assessmentsubtype/assessmentsubtypeDetailsget")]
        [HttpDelete]
        public IActionResult assessmentsubtypeDetailsget(int id)
        {
            

            var typeid = id;
            var documents = this.mySqlDBContext.riskAssessmenttemplatesubtypemodels
                         .Where(x => x.typeID == typeid).ToList();

            if (documents.Count > 0)
            {
                foreach (var document in documents)
                {
                    document.status = "Inactive";
                }
                this.mySqlDBContext.SaveChanges();
                return Ok("All matching records updated to inactive successfully");
            }
            
                var regionDocument = this.mySqlDBContext.assessmentAttributesmodels.FirstOrDefault(x => x.typeID == typeid);
                if (regionDocument != null)
                {
                    regionDocument.status = "Inactive";
                    this.mySqlDBContext.SaveChanges();
                    return Ok("Region inactivated as no sub-region was found");
                }
                else
                {
                    return NotFound("Record not found");
                }
            
        }

        [Route("api/assessmentsubtype/InsertassessmentsubtypeDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] riskAssessmenttemplatesubtypemodel riskAssessmenttemplatesubtypemodels)
        {
            try
            {
                riskAssessmenttemplatesubtypemodels.subtypename = riskAssessmenttemplatesubtypemodels.subtypename?.Trim();
                var existingDepartment = this.mySqlDBContext.riskAssessmenttemplatesubtypemodels
                    .FirstOrDefault(d => d.subtypename == riskAssessmenttemplatesubtypemodels.subtypename && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
   

                var riskAssessmenttemplatesubtypemodel = this.mySqlDBContext.riskAssessmenttemplatesubtypemodels;


                riskAssessmenttemplatesubtypemodel.Add(riskAssessmenttemplatesubtypemodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskAssessmenttemplatesubtypemodels.createddate= dt1;
                riskAssessmenttemplatesubtypemodels.status = "Active";
                riskAssessmenttemplatesubtypemodels.source = "No";
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



        [Route("api/assessmentsubtype/UpdateassessmentsubtypeDetails")]
        [HttpPut]

        public IActionResult UpdateType([FromBody] riskAssessmenttemplatesubtypemodel riskAssessmenttemplatesubtypemodels)
        {

            try
            {
                if (riskAssessmenttemplatesubtypemodels.subtypeID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    riskAssessmenttemplatesubtypemodels.subtypename = riskAssessmenttemplatesubtypemodels.subtypename?.Trim();
                    var existingDepartment = this.mySqlDBContext.riskAssessmenttemplatesubtypemodels
                  .FirstOrDefault(d => d.subtypename == riskAssessmenttemplatesubtypemodels.subtypename && d.subtypeID != riskAssessmenttemplatesubtypemodels.subtypeID && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: sub region Name with the same name already exists.");
                    }

                    this.mySqlDBContext.Attach(riskAssessmenttemplatesubtypemodels);
                    this.mySqlDBContext.Entry(riskAssessmenttemplatesubtypemodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskAssessmenttemplatesubtypemodels);

                    Type type = typeof(riskAssessmenttemplatesubtypemodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskAssessmenttemplatesubtypemodels, null) == null || property.GetValue(riskAssessmenttemplatesubtypemodels, null).Equals(0))
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
                    return BadRequest("Error: sub region  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }
        [Route("api/assessmentsubtype/DeleteassessmentsubtypeDetails")]
        [HttpDelete]

        public void DeleteassessmentsubtypeDetails(int id)
        {
            var currentClass = new riskAssessmenttemplatesubtypemodel { subtypeID = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("SubRegionStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}

    
