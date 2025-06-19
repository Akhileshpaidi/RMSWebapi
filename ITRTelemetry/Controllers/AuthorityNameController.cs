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
    public class AuthorityNameController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public AuthorityNameController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting AuthorityName Details by AuthorityTypeID

        [Route("api/AuthorityName/GetAuthorityNameDetails")]
        [HttpGet]

        public IEnumerable<AuthorityNameModel> GetAuthorityNameDetails()
        {

            return this.mySqlDBContext.AuthorityNameModels.Where(x => x.Authority_Status == "Active").ToList();
        }


      


        [Route("api/AuthorityName/GetAuthorityNameModelDetailsByAuthorityTypeID/{AuthorityTypeID}")]
        [HttpGet]

        public IEnumerable<AuthorityNameModel> GetAuthorityNameModelDetails(int AuthorityTypeID)
        {

            return this.mySqlDBContext.AuthorityNameModels.Where(x => x.Authority_Status == "Active" && x.AuthorityTypeID == AuthorityTypeID).ToList();
        }


        //Insert AuthorityName Master Details

        [Route("api/AuthorityName/InsertAuthorityNameModelDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] AuthorityNameModel AuthorityNameModels)
        {
            try
            {
                AuthorityNameModels.AuthorityName = AuthorityNameModels.AuthorityName?.Trim();

                var existingDepartment = this.mySqlDBContext.AuthorityNameModels
                    .FirstOrDefault(d => d.AuthorityName == AuthorityNameModels.AuthorityName && d.AuthorityTypeID == AuthorityNameModels.AuthorityTypeID && d.Authority_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: AuthorityName with the same name already exists.");
                }
                // Proceed with the insertion
                var AuthorityNameModel = this.mySqlDBContext.AuthorityNameModels;
                AuthorityNameModel.Add(AuthorityNameModels);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                AuthorityNameModels.Authority_CreatedDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                AuthorityNameModels.Authority_Status = "Active";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: AuthorityName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/AuthorityName/UpdateAuthorityNameModelDetails")]
        [HttpPut]
        public IActionResult UpdateAuthorityName([FromBody] AuthorityNameModel AuthorityNameModels)
        {
            try { 
            if (AuthorityNameModels.AuthoritynameID == 0)
            {
                // Logic for handling new department (insertion) goes here
                // You may want to return some success response
                return Ok("Insertion successful");
            }
            else
                {
                    AuthorityNameModels.AuthorityName = AuthorityNameModels.AuthorityName?.Trim();

                    var existingDepartment = this.mySqlDBContext.AuthorityNameModels
                     .FirstOrDefault(d => d.AuthorityName == AuthorityNameModels.AuthorityName && d.AuthoritynameID != AuthorityNameModels.AuthoritynameID && d.AuthorityTypeID != AuthorityNameModels.AuthorityTypeID && d.Authority_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: AuthorityName with the same name already exists.");
                }

                // Existing department, update logic
                this.mySqlDBContext.Attach(AuthorityNameModels);
                this.mySqlDBContext.Entry(AuthorityNameModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(AuthorityNameModels);

                Type type = typeof(AuthorityNameModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(AuthorityNameModels, null) == null || property.GetValue(AuthorityNameModels, null).Equals(0))
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
                    return BadRequest("Error: AuthorityName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }



        [Route("api/AuthorityName/DeleteAuthorityNameModelDetails")]
        [HttpDelete]
        public void DeleteAuthorityName(int id)
        {
            var currentClass = new AuthorityNameModel { AuthoritynameID = id };
            currentClass.Authority_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Authority_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




    }
}
