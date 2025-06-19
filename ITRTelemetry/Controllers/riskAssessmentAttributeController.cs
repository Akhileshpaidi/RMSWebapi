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
    public class riskAssessmentAttributeController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public riskAssessmentAttributeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting Type Details by TypeID

        [Route("api/riskAssessmentAttribute/GetriskAssessmentAttributeDetails")]
        [HttpGet]

        public IEnumerable<assessmentAttributesmodel> GetriskAssessmentAttributeDetails()
        {

            return this.mySqlDBContext.assessmentAttributesmodels.Where(x => x.status == "Active").ToList();
        }


        [Route("api/riskAssessmentAttribute/InsertriskAssessmentAttributeDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] assessmentAttributesmodel assessmentAttributesmodel)
        {
            try
            {
                assessmentAttributesmodel.typeName = assessmentAttributesmodel.typeName?.Trim();
                var existingtype = this.mySqlDBContext.assessmentAttributesmodels
                    .FirstOrDefault(d => d.typeName == assessmentAttributesmodel.typeName && d.status == "Active");

                if (existingtype != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
       
               
                var assessmentAttributesmodels = this.mySqlDBContext.assessmentAttributesmodels;

                assessmentAttributesmodels.Add(assessmentAttributesmodel);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                assessmentAttributesmodel.createddate = dt1;
                assessmentAttributesmodel.status = "Active";
                assessmentAttributesmodel.source = "No";

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

        [Route("api/riskAssessmentAttribute/UpdateriskAssessmentAttributeDetails")]
        [HttpPut]

        public IActionResult UpdateType([FromBody] assessmentAttributesmodel assessmentAttributesmodels)
        {

            try
            {
                if (assessmentAttributesmodels.typeID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    assessmentAttributesmodels.typeName = assessmentAttributesmodels.typeName?.Trim();
                    var existingDepartment = this.mySqlDBContext.assessmentAttributesmodels
                  .FirstOrDefault(d => d.typeName == assessmentAttributesmodels.typeName && d.typeID != assessmentAttributesmodels.typeID && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Type Name with the same name already exists.");
                    }

                    this.mySqlDBContext.Attach(assessmentAttributesmodels);
                    this.mySqlDBContext.Entry(assessmentAttributesmodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(assessmentAttributesmodels);

                    Type type = typeof(assessmentAttributesmodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(assessmentAttributesmodels, null) == null || property.GetValue(assessmentAttributesmodels, null).Equals(0))
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


        [Route("api/riskAssessmentAttribute/DeleteriskAssessmentAttributeDetails")]
        [HttpDelete]

        public void DeleteriskAssessmentAttributeDetails(int id)
        {
            try
            {

                //  var existingDepartment = this.mySqlDBContext.SubRegionModels
                //.FirstOrDefault(d => d.RegionMasterID != RegionModels.RegionMasterID );

                //  if (existingDepartment != null)
                //  {
                //     return BadRequest("Error:Region Name with the same name already exists.");
                //  }
                var currentClass = new assessmentAttributesmodel { typeID = id };
                currentClass.status = "Inactive";
                this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
                this.mySqlDBContext.SaveChanges();


            }
            catch
            {
                return;
            }


        }
    }

}