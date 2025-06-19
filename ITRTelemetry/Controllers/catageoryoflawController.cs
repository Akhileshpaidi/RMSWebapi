using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class catageoryoflawController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public catageoryoflawController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

       
        [Route("api/catageorylaw/Getcatageorylaws")]
        [HttpGet]

        public IEnumerable<catageoryoflawmodel> Getcatageorylaws()
        {
            return this.mySqlDBContext.catageoryoflawmodels.Where(x => x.status == "Active").ToList();
        }


    

        [Route("api/catageorylaw/Insertcatageorylaw")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] catageoryoflawmodel catageoryoflawmodels)
        {
            try
            {
                catageoryoflawmodels.law_Categoryname = catageoryoflawmodels.law_Categoryname.Trim();

                var existinglaw = this.mySqlDBContext.catageoryoflawmodels
                    .FirstOrDefault(d => d.law_Categoryname == catageoryoflawmodels.law_Categoryname && d.status == "Active");

                if (existinglaw != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: catageory law with the same name already exists.");
                }
                var maxcategoryId = this.mySqlDBContext.catageoryoflawmodels
                .Where(d => d.source == "No")
               .Max(d => (int?)d.category_of_law_ID) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                catageoryoflawmodels.category_of_law_ID = maxcategoryId + 1;

                var TypeModel = this.mySqlDBContext.catageoryoflawmodels;
               
                TypeModel.Add(catageoryoflawmodels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                catageoryoflawmodels.category_of_Law_Create_Date = dt1;
                catageoryoflawmodels.status = "Active";
                catageoryoflawmodels.source = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Entity with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
        [Route("api/catageorylaw/Updatecatageorylaw")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] catageoryoflawmodel catageoryoflawmodels)
        {

            try
            {
                if (catageoryoflawmodels.category_of_law_ID == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    catageoryoflawmodels.law_Categoryname = catageoryoflawmodels.law_Categoryname?.Trim();
                    var existinglaw = this.mySqlDBContext.catageoryoflawmodels
                       .FirstOrDefault(d => d.law_Categoryname == catageoryoflawmodels.law_Categoryname && d.category_of_law_ID != catageoryoflawmodels.category_of_law_ID && d.status == "Active");

                    if (existinglaw != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Entity with the same name already exists.");
                    }
                    // Existing department, update logic
                    this.mySqlDBContext.Attach(catageoryoflawmodels);
                    this.mySqlDBContext.Entry(catageoryoflawmodels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(catageoryoflawmodels);

                    Type type = typeof(catageoryoflawmodel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(catageoryoflawmodels, null) == null || property.GetValue(catageoryoflawmodels, null).Equals(0))
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
                    return BadRequest("Error: Entity with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/catageorylaw/Deletecatageorylaw")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new catageoryoflawmodel { category_of_law_ID = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
