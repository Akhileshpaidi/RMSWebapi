using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class riskQuestionbankkeyareaController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;

        public riskQuestionbankkeyareaController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        //Getting AuthorityTypeMaster Details 

        [Route("api/Questionbankkeyarea/GetQuestionbankkeyareaDetails")]
        [HttpGet]

        public IEnumerable<riskquestionbankattributekeyarea> GetQuestionbankkeyareaDetails()
        {

            return this.mySqlDBContext.riskquestionbankattributekeyareas.Where(x => x.status == "Active").ToList();
        }


     

        [Route("api/Questionbankkeyarea/InsertQuestionbankkeyareaDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] riskquestionbankattributekeyarea riskquestionbankattributekeyareas)
        {
            try
            {
                riskquestionbankattributekeyareas.keyName = riskquestionbankattributekeyareas.keyName?.Trim();

                var existingarea = this.mySqlDBContext.riskquestionbankattributekeyareas
                    .FirstOrDefault(d => d.keyName == riskquestionbankattributekeyareas.keyName && d.status == "Active");

                if (existingarea != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Key name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskquestionbankattributekeyarea = this.mySqlDBContext.riskquestionbankattributekeyareas;
                riskquestionbankattributekeyarea.Add(riskquestionbankattributekeyareas);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskquestionbankattributekeyareas.createdDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskquestionbankattributekeyareas.status = "Active";
                riskquestionbankattributekeyareas.source = "No";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }

            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Authority type name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/Questionbankkeyarea/UpdateQuestionbankkeyareaDetails")]
        [HttpPut]
        public IActionResult UpdateAuthorityType([FromBody] riskquestionbankattributekeyarea riskquestionbankattributekeyareas)
        {
            try
            {
                if (riskquestionbankattributekeyareas.keyareaID == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    riskquestionbankattributekeyareas.keyName = riskquestionbankattributekeyareas.keyName?.Trim();

                    var existingname = this.mySqlDBContext.riskquestionbankattributekeyareas
                        .FirstOrDefault(d => d.keyName == riskquestionbankattributekeyareas.keyName && d.keyareaID != riskquestionbankattributekeyareas.keyareaID && d.status == "Active");

                    if (existingname != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Authority type name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(riskquestionbankattributekeyareas);
                    this.mySqlDBContext.Entry(riskquestionbankattributekeyareas).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskquestionbankattributekeyareas);

                    Type type = typeof(riskquestionbankattributekeyarea);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskquestionbankattributekeyareas, null) == null || property.GetValue(riskquestionbankattributekeyareas, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Authority type name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/Questionbankkeyarea/DeleteQuestionbankkeyareaDetails")]
        [HttpDelete]
        public void DeleteAuthorityType(int id)
        {
            var currentClass = new riskquestionbankattributekeyarea { keyareaID = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
