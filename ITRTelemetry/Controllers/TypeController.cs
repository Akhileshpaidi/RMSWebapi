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
    public class TypeController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public TypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting Type Details by TypeID

        [Route("api/Type/GetTypeDetails")]
        [HttpGet]

        public IEnumerable<TypeModel> GetTypeDetails()
        {

            return this.mySqlDBContext.TypeModels.Where(x => x.Type_Status == "Active").ToList();
        }


        //Insert TypeMaster  Details

        [Route("api/Type/InsertTypeDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] TypeModel TypeModels)
        {
            try
            {
                TypeModels.Type_Name = TypeModels.Type_Name?.Trim();
                var existingDepartment = this.mySqlDBContext.TypeModels
                    .FirstOrDefault(d => d.Type_Name == TypeModels.Type_Name && d.Type_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion
                var TypeModel = this.mySqlDBContext.TypeModels;
            TypeModel.Add(TypeModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            TypeModels.Type_CreatedDate = dt1;
            TypeModels.Type_Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/Type/UpdateTypeDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] TypeModel TypeModels)
        {
            try
            {
                if (TypeModels.Type_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    TypeModels.Type_Name = TypeModels.Type_Name?.Trim();
                    var existingDepartment = this.mySqlDBContext.TypeModels
                  .FirstOrDefault(d => d.Type_Name == TypeModels.Type_Name && d.Type_id != TypeModels.Type_id && d.Type_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: TypeName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(TypeModels);
                    this.mySqlDBContext.Entry(TypeModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(TypeModels);

                    Type type = typeof(TypeModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(TypeModels, null) == null || property.GetValue(TypeModels, null).Equals(0))
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
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/Type/DeleteTypeDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new TypeModel { Type_id = id };
            currentClass.Type_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Type_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



    }
}
