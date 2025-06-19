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
    public class AuthorityTypeMasterController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public AuthorityTypeMasterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting AuthorityTypeMaster Details 

        [Route("api/AuthorityTypeMaster/GetAuthorityTypeMasterDetails")]
        [HttpGet]

        public IEnumerable<AuthorityTypeMaster> GetAuthorityTypeMasterDetails()
        {

            return this.mySqlDBContext.AuthorityTypeMasters.Where(x => x.Authoritytype_status == "Active").ToList();
        }


        //Insert AuthorityTypeMaster Details

        [Route("api/AuthorityTypeMaster/InsertAuthorityTypeMasterDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] AuthorityTypeMaster AuthorityTypeMasters)
        {
            try
            {
                AuthorityTypeMasters.AuthorityTypeName = AuthorityTypeMasters.AuthorityTypeName?.Trim();

                var existingDepartment = this.mySqlDBContext.AuthorityTypeMasters
                    .FirstOrDefault(d => d.AuthorityTypeName == AuthorityTypeMasters.AuthorityTypeName && d.Authoritytype_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Authority type name with the same name already exists.");
                }
                // Proceed with the insertion
                var AuthorityTypeMaster = this.mySqlDBContext.AuthorityTypeMasters;
                AuthorityTypeMaster.Add(AuthorityTypeMasters);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                AuthorityTypeMasters.Authoritytype_createdDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                AuthorityTypeMasters.Authoritytype_status = "Active";


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

            [Route("api/AuthorityTypeMaster/UpdateAuthorityTypeMasterDetails")]
        [HttpPut]
        public IActionResult UpdateAuthorityType([FromBody] AuthorityTypeMaster AuthorityTypeMasters)
        {
            try
            {
                if (AuthorityTypeMasters.AuthorityTypeID == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    AuthorityTypeMasters.AuthorityTypeName = AuthorityTypeMasters.AuthorityTypeName?.Trim();

                    var existingDepartment = this.mySqlDBContext.AuthorityTypeMasters
                        .FirstOrDefault(d => d.AuthorityTypeName == AuthorityTypeMasters.AuthorityTypeName && d.AuthorityTypeID != AuthorityTypeMasters.AuthorityTypeID && d.Authoritytype_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Authority type name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(AuthorityTypeMasters);
                    this.mySqlDBContext.Entry(AuthorityTypeMasters).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(AuthorityTypeMasters);

                    Type type = typeof(AuthorityTypeMaster);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(AuthorityTypeMasters, null) == null || property.GetValue(AuthorityTypeMasters, null).Equals(0))
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



        [Route("api/AuthorityTypeMaster/DeleteAuthorityTypeMasterDetails")]
        [HttpDelete]
        public void DeleteAuthorityType(int id)
        {
            var currentClass = new AuthorityTypeMaster { AuthorityTypeID = id };
            currentClass.Authoritytype_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Authoritytype_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
