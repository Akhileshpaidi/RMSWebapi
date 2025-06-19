using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using MySqlConnector;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class DocTypeMasterController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public DocTypeMasterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting DocTypeMaster Details

        [Route("api/DocTypeMaster/GetDocTypeMasterModelDetails")]
        [HttpGet]

        public IEnumerable<DocTypeMasterModel> GetDocTypeMasterModelDetails()
        {
            return this.mySqlDBContext.DocTypeMasterModels.Where(x => x.DocType_Status == "Active").ToList();
        }




        [Route("api/DocTypeMaster/InsertDocTypeMasterModelDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] DocTypeMasterModel DocTypeMasterModels)
        {
            try
            {
                DocTypeMasterModels.docTypeName = DocTypeMasterModels.docTypeName?.Trim();

                var existingDepartment = this.mySqlDBContext.DocTypeMasterModels
                    .FirstOrDefault(d => d.docTypeName == DocTypeMasterModels.docTypeName && d.task_id == DocTypeMasterModels.task_id && d.DocType_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Doctypename with the same name already exists.");
                }
                // Proceed with the insertion
                var docTypeMasterModel = this.mySqlDBContext.DocTypeMasterModels;
                docTypeMasterModel.Add(DocTypeMasterModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                DocTypeMasterModels.Doctype_CreatedDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                DocTypeMasterModels.DocType_Status = "Active";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Doctypename with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/DocTypeMaster/UpdateDocTypeMasterModelDetails")]
        [HttpPut]
        public IActionResult UpdateDocumentType([FromBody] DocTypeMasterModel DocTypeMasterModels)
        {
            try
            {
                if (DocTypeMasterModels.docTypeID == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    DocTypeMasterModels.docTypeName = DocTypeMasterModels.docTypeName?.Trim();

                    var existingDepartment = this.mySqlDBContext.DocTypeMasterModels
                        .FirstOrDefault(d => d.docTypeName == DocTypeMasterModels.docTypeName && d.docTypeID != DocTypeMasterModels.docTypeID && d.task_id != DocTypeMasterModels.task_id && d.DocType_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Doctypename with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(DocTypeMasterModels);
                    this.mySqlDBContext.Entry(DocTypeMasterModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(DocTypeMasterModels);

                    Type type = typeof(DocTypeMasterModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(DocTypeMasterModels, null) == null || property.GetValue(DocTypeMasterModels, null).Equals(0))
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
                    return BadRequest("Error: Doctypename with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        
    }



        [Route("api/DocTypeMaster/DeleteDocTypeMasterModelDetails")]
        [HttpDelete]
        public void DeleteDocType(int id)
        {
            var currentClass = new DocTypeMasterModel { docTypeID = id };
            currentClass.DocType_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("DocType_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
