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
    public class DocumentCategoryMasterController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public DocumentCategoryMasterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting DocCategoryMaster Details by DocTypeID

        [Route("api/DocCategoryMaster/GetDocCategoryMasterModelDetails")]
        [HttpGet]

        public  IEnumerable<DocCategoryMasterModel> GetDocCategoryMasterModelDetails()
        {
            
            return this.mySqlDBContext.DocCategoryMasterModels.Where(x => x.doccategory_status == "Active").ToList();
        }


        [Route("api/DocCategoryMaster/GetDocCategoryMasterModelDetailsByDocTypeID/{DocTypeID}")]
        [HttpGet]

        public IEnumerable<DocCategoryMasterModel> GetDocCategoryMasterModelDetails(int DocTypeID)
        {

            return this.mySqlDBContext.DocCategoryMasterModels.Where(x => x.doccategory_status == "Active" && x.DocTypeID== DocTypeID).ToList();
        }

        [Route("api/DocCategoryMaster/GetDocCategoryMasterModelDetailsByDocTypeID")]
        [HttpGet]

        public IEnumerable<DocCategoryMasterModel> GetDocCategoryMasterModelDetailsnew(int DocTypeID)
        {

            return this.mySqlDBContext.DocCategoryMasterModels.Where(x => x.doccategory_status == "Active" && x.DocTypeID == DocTypeID).ToList();
        }

        //Insert Document Category Details


        [Route("api/DocCategoryMaster/InsertDocCategoryMasterModelDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] DocCategoryMasterModel DocCategoryMasterModels)
        {
            try
            {
                DocCategoryMasterModels.Doc_CategoryName = DocCategoryMasterModels.Doc_CategoryName?.Trim();

                var existingDepartment = this.mySqlDBContext.DocCategoryMasterModels
                    .FirstOrDefault(d => d.Doc_CategoryName == DocCategoryMasterModels.Doc_CategoryName && d.DocTypeID == DocCategoryMasterModels.DocTypeID && d.doccategory_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Document category name with the same name already exists.");
                }
                // Proceed with the insertion
                var DocCategoryMasterModel = this.mySqlDBContext.DocCategoryMasterModels;
                DocCategoryMasterModel.Add(DocCategoryMasterModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                DocCategoryMasterModels.doccategory_createdDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                DocCategoryMasterModels.doccategory_status = "Active";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Document category name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/DocCategoryMaster/UpdateDocCategoryMasterModelDetails")]
        [HttpPut]
        public IActionResult UpdateDocumentCategory([FromBody] DocCategoryMasterModel DocCategoryMasterModels)
        {
            try
            {
                if (DocCategoryMasterModels.Doc_CategoryID == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim the specified property before checking or using its value
                    DocCategoryMasterModels.Doc_CategoryName = DocCategoryMasterModels.Doc_CategoryName?.Trim();

                    var existingDepartment = this.mySqlDBContext.DocCategoryMasterModels
                       .FirstOrDefault(d => d.Doc_CategoryName == DocCategoryMasterModels.Doc_CategoryName && d.Doc_CategoryID != DocCategoryMasterModels.Doc_CategoryID && d.DocTypeID != DocCategoryMasterModels.DocTypeID && d.doccategory_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Document category name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(DocCategoryMasterModels);
                    this.mySqlDBContext.Entry(DocCategoryMasterModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(DocCategoryMasterModels);

                    Type type = typeof(DocCategoryMasterModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(DocCategoryMasterModels, null) == null || property.GetValue(DocCategoryMasterModels, null).Equals(0))
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
                    return BadRequest("Error: Document category name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/DocCategoryMaster/DeleteDocCategoryMasterModelDetails")]
        [HttpDelete]
        public void DeleteDocCategory(int id)
        {
            var currentClass = new DocCategoryMasterModel { Doc_CategoryID = id };
            currentClass.doccategory_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("doccategory_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
