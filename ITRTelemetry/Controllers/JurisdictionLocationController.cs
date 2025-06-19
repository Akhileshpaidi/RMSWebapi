using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class JurisdictionLocationController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public JurisdictionLocationController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        [Route("api/JurisdictionLocation/GetJurisdictionLocationDetails")]
        [HttpGet]

        public IEnumerable<JurisdictionLocationModel> GetJurisdictionLocationDetails()
        {
            return this.mySqlDBContext.JurisdictionLocationModels.Where(x => x.status == "Active").ToList();
        }



        [Route("api/JurisdictionLocation/InsertJurisdictionLocationDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] JurisdictionLocationModel JurisdictionLocationModels)
        {
            try
            {
                //JurisdictionLocationModels.penalty_category_name = JurisdictionLocationModels.penalty_category_name.Trim();

                //var existingPenaltyCategory = this.mySqlDBContext.JurisdictionLocationModels
                //    .FirstOrDefault(d => d.penalty_category_name == JurisdictionLocationModels.penalty_category_name && d.penalty_category_status == "Active");

                //if (existingPenaltyCategory != null)
                //{
                //    // Department with the same name already exists, return an error message
                //    return BadRequest("Error: Penalty Category with the same name already exists.");
                //}
                var maxjuridicationId = this.mySqlDBContext.JurisdictionLocationModels
            .Where(d => d.IsImportedData == "No")
           .Max(d => (int?)d.jurisdiction_location_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                JurisdictionLocationModels.jurisdiction_location_id = maxjuridicationId + 1;

                var TypeModel = this.mySqlDBContext.JurisdictionLocationModels;
              
                TypeModel.Add(JurisdictionLocationModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                JurisdictionLocationModels.jurisdiction_location_create_date = dt1;
                JurisdictionLocationModels.status = "Active";
                JurisdictionLocationModels.IsImportedData = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/JurisdictionLocation/UpdateJurisdictionLocationDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] JurisdictionLocationModel JurisdictionLocationModels)
        {

            try
            {
                if (JurisdictionLocationModels.jurisdiction_location_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    //JurisdictionLocationModels.penalty_category_name = JurisdictionLocationModels..Trim();
                    //var existingCompliancenotifiedstatus = this.mySqlDBContext.JurisdictionLocationModels
                    //   .FirstOrDefault(d => d.penalty_category_name == JurisdictionLocationModels.penalty_category_name && d.penalty_category_id != JurisdictionLocationModels.penalty_category_id && d.penalty_category_status == "Active");

                    //if (existingCompliancenotifiedstatus != null)
                    //{
                    //    return BadRequest("Error: Penalty Category with the same name already exists.");
                    //}
                    this.mySqlDBContext.Attach(JurisdictionLocationModels);
                    this.mySqlDBContext.Entry(JurisdictionLocationModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(JurisdictionLocationModels);

                    Type type = typeof(JurisdictionLocationModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(JurisdictionLocationModels, null) == null || property.GetValue(JurisdictionLocationModels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok();
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/JurisdictionLocation/DeleteJurisdictionLocationDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new JurisdictionLocationModel { jurisdiction_location_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
