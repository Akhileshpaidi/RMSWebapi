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
    public class riskQuestionbankattributeSubkeyareaController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;

        public riskQuestionbankattributeSubkeyareaController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }




        //Getting Type Details by TypeID

        [Route("api/Questionbanksubkeyarea/GetQuestionbanksubkeyareaDetails")]
        [HttpGet]

        public IEnumerable<riskquestionbankattributeSubkeyarea> GetSubTypeDetails()
        {

            return this.mySqlDBContext.riskquestionbankattributeSubkeyareas.Where(x => x.status == "Active").ToList();
        }


        [Route("api/Questionbanksubkeyarea/QuestionbanksubkeyareaDetailsget")]
        [HttpDelete]
        public IActionResult QuestionbanksubkeyareaDetailsget(int id)
        {


            var typeid = id;
            var documents = this.mySqlDBContext.riskquestionbankattributeSubkeyareas
                         .Where(x => x.keyareaID == typeid).ToList();

            if (documents.Count > 0)
            {
                foreach (var document in documents)
                {
                    document.status = "Inactive";
                }
                this.mySqlDBContext.SaveChanges();
                return Ok("All matching records updated to inactive successfully");
            }

            var regionDocument = this.mySqlDBContext.riskquestionbankattributekeyareas.FirstOrDefault(x => x.keyareaID == typeid);
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

        [Route("api/Questionbanksubkeyarea/InsertQuestionbanksubkeyareaDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] riskquestionbankattributeSubkeyarea riskquestionbankattributeSubkeyareas)
        {
            try
            {
                riskquestionbankattributeSubkeyareas.subkeyname = riskquestionbankattributeSubkeyareas.subkeyname?.Trim();
                var existingDepartment = this.mySqlDBContext.riskquestionbankattributeSubkeyareas
                    .FirstOrDefault(d => d.subkeyname == riskquestionbankattributeSubkeyareas.subkeyname && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Sub key Name with the same name already exists.");
                }


                var riskquestionbankattributeSubkeyarea = this.mySqlDBContext.riskquestionbankattributeSubkeyareas;


                riskquestionbankattributeSubkeyarea.Add(riskquestionbankattributeSubkeyareas);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskquestionbankattributeSubkeyareas.createddate = dt1;
                riskquestionbankattributeSubkeyareas.status = "Active";
                riskquestionbankattributeSubkeyareas.source = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();

            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: SUb key Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/Questionbanksubkeyarea/UpdateQuestionbanksubkeyareaDetails")]
        [HttpPut]

        public IActionResult UpdateType([FromBody] riskquestionbankattributeSubkeyarea riskquestionbankattributeSubkeyareas)
        {

            try
            {
                if (riskquestionbankattributeSubkeyareas.subkeyID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    riskquestionbankattributeSubkeyareas.subkeyname = riskquestionbankattributeSubkeyareas.subkeyname?.Trim();
                    var existingDepartment = this.mySqlDBContext.riskquestionbankattributeSubkeyareas
                  .FirstOrDefault(d => d.subkeyname == riskquestionbankattributeSubkeyareas.subkeyname && d.    subkeyID != riskquestionbankattributeSubkeyareas.subkeyID && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: sub Key Area Name with the same name already exists.");
                    }

                    this.mySqlDBContext.Attach(riskquestionbankattributeSubkeyareas);
                    this.mySqlDBContext.Entry(riskquestionbankattributeSubkeyareas).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskquestionbankattributeSubkeyareas);

                    Type type = typeof(riskquestionbankattributeSubkeyarea);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskquestionbankattributeSubkeyareas, null) == null || property.GetValue(riskquestionbankattributeSubkeyareas, null).Equals(0))
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
        [Route("api/Questionbanksubkeyarea/DeleteQuestionbanksubkeyareaDetails")]
        [HttpDelete]

        public void DeleteassessmentsubtypeDetails(int id)
        {
            var currentClass = new riskquestionbankattributeSubkeyarea { subkeyID = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
