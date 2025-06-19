using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class ScoreIndicatorController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public ScoreIndicatorController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting ScoreIndicator Details

        [Route("api/ScoreIndicator/GetScoreIndicatorDetails")]
        [HttpGet]

        public IEnumerable<ScoreIndicatorModel> GetScoreIndicatorDetails()
        {

            return this.mySqlDBContext.ScoreIndicatorModels.Where(x => x.score_status == "Active").ToList();
        }


        //Insert ScoreIndicator Master Details

        [Route("api/ScoreIndicator/InsertScoreIndicatorDetails")]
        [HttpPost]
        public IActionResult InsertScoreIndicator([FromBody] ScoreIndicatorModel ScoreIndicatorModels)
        {
            try
            {
                ScoreIndicatorModels.Score_Name = ScoreIndicatorModels.Score_Name?.Trim();

                if (ScoreIndicatorModels.scoremaxrange <= ScoreIndicatorModels.scoreminrange)
                {
                    return BadRequest("Error: Max score should be greater than min score.");
                }

                var existingDepartment = this.mySqlDBContext.ScoreIndicatorModels
                    .FirstOrDefault(d => d.Score_Name == ScoreIndicatorModels.Score_Name &&  d.score_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: ScoreIndicator with the same name already exists.");
                }
                // Proceed with the insertion
                var ScoreIndicatorModel = this.mySqlDBContext.ScoreIndicatorModels;
                ScoreIndicatorModel.Add(ScoreIndicatorModels);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ScoreIndicatorModels.score_createdDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                ScoreIndicatorModels.score_status = "Active";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: ScoreIndicator with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


            [Route("api/ScoreIndicator/UpdateScoreIndicatorDetails")]
        [HttpPut]
        public IActionResult UpdateScoreIndicator([FromBody] ScoreIndicatorModel ScoreIndicatorModels)
        {
            try
            {
                if (ScoreIndicatorModels.Score_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    ScoreIndicatorModels.Score_Name = ScoreIndicatorModels.Score_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.ScoreIndicatorModels
                  .FirstOrDefault(d => d.Score_Name == ScoreIndicatorModels.Score_Name && d.Score_id != ScoreIndicatorModels.Score_id && d.score_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Score_Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(ScoreIndicatorModels);
                    this.mySqlDBContext.Entry(ScoreIndicatorModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(ScoreIndicatorModels);

                    Type type = typeof(ScoreIndicatorModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(ScoreIndicatorModels, null) == null || property.GetValue(ScoreIndicatorModels, null).Equals(0))
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
                    return BadRequest("Error: Score_Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/ScoreIndicator/DeleteScoreIndicatorDetails")]
        [HttpDelete]
        public void DeleteScoreIndicator(int id)
        {
            var currentClass = new ScoreIndicatorModel { Score_id = id };
            currentClass.score_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("score_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
