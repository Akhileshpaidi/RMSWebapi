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
    public class NatureOfDocumentMasterController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public NatureOfDocumentMasterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting NatureOfDocument Details

        [Route("api/NatureOfDocument/GetNatureOfDocumentDetails")]
        [HttpGet]

        public IEnumerable<NatureOf_DocumentMasterModel> GetNatureOfDocumentDetails()
        {

            return this.mySqlDBContext.NatureOf_DocumentMasterModels.Where(x => x.natureof_Status == "Active").ToList();
        }


        //Insert NatureOfDocument Master Details

        [Route("api/NatureOfDocument/InsertNatureOfDocumentDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] NatureOf_DocumentMasterModel NatureOf_DocumentMasterModels)
        {
            try
            {
                NatureOf_DocumentMasterModels.NatureOf_Doc_Name = NatureOf_DocumentMasterModels.NatureOf_Doc_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.NatureOf_DocumentMasterModels
                    .FirstOrDefault(d => d.NatureOf_Doc_Name == NatureOf_DocumentMasterModels.NatureOf_Doc_Name && d.natureof_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Nature of document with the same name already exists.");
                }
                // Proceed with the insertion
                var NatureOf_DocumentMasterModel = this.mySqlDBContext.NatureOf_DocumentMasterModels;
                NatureOf_DocumentMasterModel.Add(NatureOf_DocumentMasterModels);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                NatureOf_DocumentMasterModels.natureof_createdDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                NatureOf_DocumentMasterModels.natureof_Status = "Active";



                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Nature of document with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

            [Route("api/NatureOfDocument/UpdateNatureOfDocumentDetails")]
        [HttpPut]
        public IActionResult UpdateNatureofDoc([FromBody] NatureOf_DocumentMasterModel NatureOf_DocumentMasterModels)
        {
            try
            {
                if (NatureOf_DocumentMasterModels.NatureOf_Doc_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    NatureOf_DocumentMasterModels.NatureOf_Doc_Name = NatureOf_DocumentMasterModels.NatureOf_Doc_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.NatureOf_DocumentMasterModels
                       .FirstOrDefault(d => d.NatureOf_Doc_Name == NatureOf_DocumentMasterModels.NatureOf_Doc_Name && d.NatureOf_Doc_id != NatureOf_DocumentMasterModels.NatureOf_Doc_id && d.natureof_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Nature of document with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(NatureOf_DocumentMasterModels);
                    this.mySqlDBContext.Entry(NatureOf_DocumentMasterModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(NatureOf_DocumentMasterModels);

                    Type type = typeof(NatureOf_DocumentMasterModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(NatureOf_DocumentMasterModels, null) == null || property.GetValue(NatureOf_DocumentMasterModels, null).Equals(0))
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
                    return BadRequest("Error: Nature of document with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/NatureOfDocument/DeleteNatureOfDocumentDetails")]
        [HttpDelete]
        public void DeleteNatureOfDoc(int id)
        {
            var currentClass = new NatureOf_DocumentMasterModel { NatureOf_Doc_id = id };
            currentClass.natureof_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("natureof_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



    }
}
