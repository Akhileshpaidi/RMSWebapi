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
    public class CompetencySkillController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public CompetencySkillController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting CompetencySkill Details

        [Route("api/CompetencySkill/GetCompetencySkillDetails")]
        [HttpGet]

        public IEnumerable<CompetencySkillModel> GetScoreIndicatorDetails()
        {

            return this.mySqlDBContext.CompetencySkillModels.Where(x => x.Competency_status == "Active").ToList();
        }


        //Insert CompetencySkill Master Details

        [Route("api/CompetencySkill/InsertCompetencySkillDetails")]
        [HttpPost]
        public IActionResult InsertCompetencySkill([FromBody] CompetencySkillModel CompetencySkillModels)
        {
            try
            {
                CompetencySkillModels.Competency_Name = CompetencySkillModels.Competency_Name?.Trim();
                var existingDepartment = this.mySqlDBContext.CompetencySkillModels
                    .FirstOrDefault(d => d.Competency_Name == CompetencySkillModels.Competency_Name && d.Competency_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: competency skillLevel with the same name already exists.");
                }
                // Proceed with the insertion
                var CompetencySkillModel = this.mySqlDBContext.CompetencySkillModels;
            CompetencySkillModel.Add(CompetencySkillModels);

            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            CompetencySkillModels.competency_createdDate = dt1;

            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            CompetencySkillModels.Competency_status = "Active";


            this.mySqlDBContext.SaveChanges();
            return Ok();
        }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: competency skillLevel with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/CompetencySkill/UpdateCompetencySkillDetails")]
        [HttpPut]
        public IActionResult UpdateCompetencySkill([FromBody] CompetencySkillModel CompetencySkillModels)
        {
            try
            {
                if (CompetencySkillModels.Competency_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    CompetencySkillModels.Competency_Name = CompetencySkillModels.Competency_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.CompetencySkillModels
                  .FirstOrDefault(d => d.Competency_Name == CompetencySkillModels.Competency_Name && d.Competency_id != CompetencySkillModels.Competency_id && d.Competency_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: competency skillLevel with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(CompetencySkillModels);
                    this.mySqlDBContext.Entry(CompetencySkillModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(CompetencySkillModels);

                    Type type = typeof(CompetencySkillModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(CompetencySkillModels, null) == null || property.GetValue(CompetencySkillModels, null).Equals(0))
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
                    return BadRequest("Error: competency skillLevel with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }
    



        [Route("api/CompetencySkill/DeleteCompetencySkillDetails")]
        [HttpDelete]
        public void DeleteCompetencySkill(int id)
        {
            var currentClass = new CompetencySkillModel { Competency_id = id };
            currentClass.Competency_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Competency_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
