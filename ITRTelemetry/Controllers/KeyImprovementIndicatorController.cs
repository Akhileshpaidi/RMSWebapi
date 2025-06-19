using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class KeyImprovementIndicatorController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public KeyImprovementIndicatorController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting KeyImprovementIndicator Details by KeyImprovementIndicatorID

        [Route("api/KeyImprovementIndicator/GetKeyImprovementIndicatorDetails")]
        [HttpGet]

        public IEnumerable<KeyImprovementIndicatorsModel> GettopicDetails()
        {


            return this.mySqlDBContext.KeyImprovementIndicatorsModels.Where(x => x.Key_Impr_Indicator_Status == "Active").ToList();
        }


        //Insert KeyImprovementIndicator  Details

        [Route("api/KeyImprovementIndicator/InsertKeyImprovementIndicatorDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] KeyImprovementIndicatorsModel KeyImprovementIndicatorsModels)
        {
            try
            {
                KeyImprovementIndicatorsModels.Key_Impr_Indicator_Name = KeyImprovementIndicatorsModels.Key_Impr_Indicator_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.KeyImprovementIndicatorsModels
        .FirstOrDefault(d =>
            d.Key_Impr_Indicator_Name == KeyImprovementIndicatorsModels.Key_Impr_Indicator_Name &&
            d.competency_id == KeyImprovementIndicatorsModels.competency_id &&
            d.Score_id == KeyImprovementIndicatorsModels.Score_id &&
            d.Key_Impr_Indicator_Status == "Active"
        );

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: KeyImprovement name with the same name already exists.");
                }


                // Proceed with the insertion
                var KeyImprovementIndicatorsModel = this.mySqlDBContext.KeyImprovementIndicatorsModels;
            KeyImprovementIndicatorsModel.Add(KeyImprovementIndicatorsModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            KeyImprovementIndicatorsModels.Key_Impr_Indicator_CreatedDate = dt1;
            KeyImprovementIndicatorsModels.Key_Impr_Indicator_Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: KeyImprovement name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/KeyImprovementIndicator/UpdateKeyImprovementIndicatorDetails")]
        [HttpPut]
        public IActionResult Updatetopic([FromBody] KeyImprovementIndicatorsModel KeyImprovementIndicatorsModels)
        {
            try
            {
                if (KeyImprovementIndicatorsModels.Key_Impr_Indicator_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    KeyImprovementIndicatorsModels.Key_Impr_Indicator_Name = KeyImprovementIndicatorsModels.Key_Impr_Indicator_Name?.Trim();
                    var existingDepartment = this.mySqlDBContext.KeyImprovementIndicatorsModels
                        .FirstOrDefault(d => d.Key_Impr_Indicator_Name == KeyImprovementIndicatorsModels.Key_Impr_Indicator_Name &&
                        d.Key_Impr_Indicator_id != KeyImprovementIndicatorsModels.Key_Impr_Indicator_id &&
                d.competency_id == KeyImprovementIndicatorsModels.competency_id &&
                d.Score_id == KeyImprovementIndicatorsModels.Score_id &&
                d.Key_Impr_Indicator_Status == "Active" );


                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: KeyImprovement name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(KeyImprovementIndicatorsModels);
                    this.mySqlDBContext.Entry(KeyImprovementIndicatorsModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(KeyImprovementIndicatorsModels);

                    Type type = typeof(KeyImprovementIndicatorsModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(KeyImprovementIndicatorsModels, null) == null || property.GetValue(KeyImprovementIndicatorsModels, null).Equals(0))
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
                    return BadRequest("Error: KeyImprovement name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

    



        [Route("api/KeyImprovementIndicator/DeleteKeyImprovementIndicatorDetails")]
        [HttpDelete]
        public void Deletetopic(int id)
        {
            var currentClass = new KeyImprovementIndicatorsModel { Key_Impr_Indicator_id = id };
            currentClass.Key_Impr_Indicator_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Key_Impr_Indicator_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }







    }
}
