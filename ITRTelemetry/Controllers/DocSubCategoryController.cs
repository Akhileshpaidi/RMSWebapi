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
    //[Route("api/[controller]")]
    //[ApiController]
    public class DocSubCategoryController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public DocSubCategoryController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting DocSubCategoryMaster Details by Doc_CategoryID

        [Route("api/DocSubCategory/GetDocSubCategoryModelDetails")]
        [HttpGet]

        public IEnumerable<DocSubCategoryModel> GetDocSubCategoryModelDetails()
        {

            return this.mySqlDBContext.DocSubCategoryModels.Where(x => x.Doc_Status == "Active").ToList();
        }



        [Route("api/DocSubCategory/GetDocSubCategoryModelDetailsbyId/{Doc_CategoryID}")]
        [HttpGet]

        public IEnumerable<DocSubCategoryModel> GetDocSubCategoryModelDetails(int Doc_CategoryID)
        {

            return this.mySqlDBContext.DocSubCategoryModels.Where(x => x.Doc_Status == "Active" && x.Doc_CategoryID == Doc_CategoryID).ToList();
        }
        // for multiple select of Doc id to get subcatid in Schedule assessment
        [Route("api/DocSubCategory/GetDocSubCategoryModelDetailsbyIdnewforarray")]
        [HttpGet]
        public IEnumerable<DocSubCategoryModel> GetDocSubCategoryModelDetailsbyIdnewforarray([FromQuery] List<int> Doc_CategoryID)
        {
            return this.mySqlDBContext.DocSubCategoryModels
                .Where(x => x.Doc_Status == "Active" && Doc_CategoryID.Contains(x.Doc_CategoryID))
                .ToList();
        }

        [Route("api/DocSubCategory/GetDocSubCategoryModelDetailsbyId")]
        [HttpGet]

        public IEnumerable<DocSubCategoryModel> GetDocSubCategoryModelDetailsnew(int Doc_CategoryID)
        {

            return this.mySqlDBContext.DocSubCategoryModels.Where(x => x.Doc_Status == "Active" && x.Doc_CategoryID == Doc_CategoryID).ToList();
        }



        //Insert DocumentSub Category Master Details

        [Route("api/DocSubCategory/InsertDocSubCategoryModelDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] DocSubCategoryModel DocSubCategoryModels)
        {
            try
            {
                DocSubCategoryModels.Doc_SubCategoryName = DocSubCategoryModels.Doc_SubCategoryName?.Trim();

                var existingDepartment = this.mySqlDBContext.DocSubCategoryModels
                    .FirstOrDefault(d => d.Doc_SubCategoryName == DocSubCategoryModels.Doc_SubCategoryName && d.DocTypeID == DocSubCategoryModels.DocTypeID && d.Doc_CategoryID == DocSubCategoryModels.Doc_CategoryID && d.Doc_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Document subcategory name with the same name already exists.");
                }
                // Proceed with the insertion
                var DocSubCategoryModel = this.mySqlDBContext.DocSubCategoryModels;
                DocSubCategoryModel.Add(DocSubCategoryModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                DocSubCategoryModels.Doc_createdDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                DocSubCategoryModels.Doc_Status = "Active";



                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Document subcategory name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }


        }



            [Route("api/DocSubCategoryMaster/UpdateDocSubCategoryMasterModelDetails")]
        [HttpPut]
        public IActionResult UpdateDocumentSubCategory([FromBody] DocSubCategoryModel DocSubCategoryModels)
        {
            try
            {
                if (DocSubCategoryModels.Doc_SubCategoryID == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim the specified property before checking or using its value
                    DocSubCategoryModels.Doc_SubCategoryName = DocSubCategoryModels.Doc_SubCategoryName?.Trim();

                    var existingDepartment = this.mySqlDBContext.DocSubCategoryModels
                      .FirstOrDefault(d => d.Doc_SubCategoryName == DocSubCategoryModels.Doc_SubCategoryName && d.DocTypeID != DocSubCategoryModels.DocTypeID && d.Doc_CategoryID != DocSubCategoryModels.Doc_CategoryID && d.Doc_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Document subcategory name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(DocSubCategoryModels);
                    this.mySqlDBContext.Entry(DocSubCategoryModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(DocSubCategoryModels);

                    Type type = typeof(DocSubCategoryModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(DocSubCategoryModels, null) == null || property.GetValue(DocSubCategoryModels, null).Equals(0))
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
                    return BadRequest("Error: Document subcategory name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }


        }



        [Route("api/DocSubCategoryMaster/DeleteDocSubCategoryMasterModelDetails")]
        [HttpDelete]
        public void DeleteDocType(int id)
        {
            var currentClass = new DocSubCategoryModel { Doc_SubCategoryID = id };
            currentClass.Doc_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Doc_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
