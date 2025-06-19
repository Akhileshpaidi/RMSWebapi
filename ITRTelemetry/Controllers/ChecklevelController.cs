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
    public class ChecklevelController : ControllerBase
    {


        private readonly MySqlDBContext mySqlDBContext;

        public ChecklevelController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

    



    [Route("api/CompetencyCheckLevel/GetCompetencyCheckLevelDetails")]
    [HttpGet]

    public IEnumerable<CheckLevelModel> GetSCheckLevelDetails()
    {

        return this.mySqlDBContext.CheckLevelModels.Where(x => x.Check_Level_Status == "Active").ToList();
    }


        [Route("api/CompetencyCheckLevel/GetCompetencyCheckLevelDetailsByID")]
        [HttpGet]

        public IEnumerable<CheckLevelModel> GetSCheckLevelDetails(int check_level_id)
        {

            return this.mySqlDBContext.CheckLevelModels.Where(x => x.Check_Level_Status == "Active" && x.check_level_id==check_level_id).ToList();
        }


        //Insert CompetencySkill Master Details

        [Route("api/CompetencyCheckLevel/InsertCompetencyCheckLevelDetails")]
    [HttpPost]
    public IActionResult InsertCheckLevelDetails([FromBody] CheckLevelModel CheckLevelModels)
    {
            try
            {
                CheckLevelModels.Skill_Level_Name = CheckLevelModels.Skill_Level_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.CheckLevelModels
                    .FirstOrDefault(d => d.Skill_Level_Name == CheckLevelModels.Skill_Level_Name &&  d.Check_Level_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Checklevel name with the same name already exists.");
                }
                var CheckLevelModel = this.mySqlDBContext.CheckLevelModels;
            CheckLevelModel.Add(CheckLevelModels);

        DateTime dt = DateTime.Now;
        string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            CheckLevelModels.Created_Date = dt1;

            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            CheckLevelModels.Check_Level_Status = "Active";


        this.mySqlDBContext.SaveChanges();
        return Ok();
    }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Checklevel with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/CompetencyCheckLevel/UpdateCompetencyCheckLevelDetails")]
        [HttpPut]
        public IActionResult UpdateCheckLevelDetails([FromBody] CheckLevelModel CheckLevelModels)
        {
            try
            {
                if (CheckLevelModels.check_level_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    CheckLevelModels.Skill_Level_Name = CheckLevelModels.Skill_Level_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.CheckLevelModels
                  .FirstOrDefault(d => d.Skill_Level_Name == CheckLevelModels.Skill_Level_Name && d.Check_Level_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Checklevel with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(CheckLevelModels);
                    this.mySqlDBContext.Entry(CheckLevelModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(CheckLevelModels);

                    Type type = typeof(CheckLevelModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(CheckLevelModels, null) == null || property.GetValue(CheckLevelModels, null).Equals(0))
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
                    return BadRequest("Error: Checklevel with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



            [Route("api/CompetencyCheckLevel/DeleteCompetencyCheckLevelDetails")]
    [HttpDelete]
    public void DeleteCheckLevelDetails(int id)
    {
        var currentClass = new CheckLevelModel { check_level_id = id };
        currentClass.Check_Level_Status = "Inactive";
        this.mySqlDBContext.Entry(currentClass).Property("Check_Level_Status").IsModified = true;
        this.mySqlDBContext.SaveChanges();
    }


}

}

