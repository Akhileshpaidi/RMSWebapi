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
    public class SubTypeController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public SubTypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting Type Details by TypeID

        [Route("api/SubType/GetSubTypeDetails")]
        [HttpGet]

        public IEnumerable<SubTypeModel> GetSubTypeDetails()
        {

            return this.mySqlDBContext.SubTypeModels.Where(x => x.SubType_Status == "Active").ToList();
        }

        [Route("api/SubType/GetSubTypeDetails/{Type_id}")]
        [HttpGet]

        public IEnumerable<SubTypeModel> GetSubTypeDetails(int Type_id)
        {

            return this.mySqlDBContext.SubTypeModels.Where(x => x.SubType_Status == "Active" && x.Type_id== Type_id).ToList();
        }



        //Insert TypeMaster  Details

        [Route("api/SubType/InsertSubTypeDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SubTypeModel SubTypeModels)
        {
            try
            {
                SubTypeModels.SubType_Name = SubTypeModels.SubType_Name?.Trim();
                var existingDepartment = this.mySqlDBContext.SubTypeModels
                    .FirstOrDefault(d => d.SubType_Name == SubTypeModels.SubType_Name && d.Type_id == SubTypeModels.Type_id  && d.SubType_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: SubType name with the same name already exists.");
                }
                var SubTypeModel = this.mySqlDBContext.SubTypeModels;
            SubTypeModel.Add(SubTypeModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            SubTypeModels.SubType_CreatedDate = dt1;
            SubTypeModels.SubType_Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: SubType name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }


        }

        [Route("api/SubType/UpdateSubTypeDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SubTypeModel SubTypeModels)
        {
            try { 
            if (SubTypeModels.SubType_id == 0)
            {
                return Ok("Insertion successful");
            }
            else
            {
                    SubTypeModels.SubType_Name = SubTypeModels.SubType_Name?.Trim();
                    var existingDepartment = this.mySqlDBContext.SubTypeModels
                  .FirstOrDefault(d => d.SubType_Name == SubTypeModels.SubType_Name && d.Type_id != SubTypeModels.Type_id  && d.SubType_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Document subcategory name with the same name already exists.");
                }

                // Existing department, update logic
                this.mySqlDBContext.Attach(SubTypeModels);
                this.mySqlDBContext.Entry(SubTypeModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(SubTypeModels);

                Type type = typeof(SubTypeModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(SubTypeModels, null) == null || property.GetValue(SubTypeModels, null).Equals(0))
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
                    return BadRequest("Error: Document subcategory name with the same name already exists.");
    }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
}
            }

        }



        [Route("api/SubType/DeleteSubTypeDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new SubTypeModel { SubType_id = id };
            currentClass.SubType_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("SubType_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




    }
}
