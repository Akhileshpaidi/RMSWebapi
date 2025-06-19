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
using Microsoft.Extensions.Configuration;
using System.Data;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class UnitMasterController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        public UnitMasterController (MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }


        [Route("api/UnitMaster/GetEntityNames")]
        [HttpGet]

        public IEnumerable<UnitMasterModel> GetUnitMasterModels()
        {
            return this.mySqlDBContext.UnitMasterModels.Where(x => x.Entity_Master_Status == "Active").ToList();
        }


        // Getting Entity Names by Entity Id
        [Route("api/UnitMaster/GetEntityNames/{id}")]
        [HttpGet]

        public IEnumerable<UnitMasterModel> GetUnitMasterModels(int id)
        {
            var pdata = new List<UnitMasterModel>();

            // Check if the connection string is properly set
            var connectionString = Configuration["ConnectionStrings:myDb1"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'myDb1' is missing or null.");
            }

            using (MySqlConnection con = new MySqlConnection(connectionString)) // Use the connection string from Configuration
            {
                string query = "SELECT * FROM risk.entity_master WHERE Entity_Master_id IN (SELECT DISTINCT Entity_Master_id FROM risk.user_location_mapping WHERE USR_ID=@userId)";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@userId", id);
                    con.Open();
                    cmd.CommandType = CommandType.Text;

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            for (var i = 0; i < dt.Rows.Count; i++)
                            {
                                pdata.Add(new UnitMasterModel
                                {
                                    Entity_Master_id = Convert.ToInt32(dt.Rows[i]["Entity_Master_id"]),
                                    Entity_Master_Name = dt.Rows[i]["Entity_Master_Name"].ToString()
                                });
                            }
                        }
                    }
                }
            }

            return pdata; // Return the list of UnitMasterModel objects
        }
    


        //Insert EntityNames  Details

        [Route("api/UnitMaster/InsertEntityNames")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] UnitMasterModel UnitMasterModels)
        {
            try
            {
                UnitMasterModels.Entity_Master_Name = UnitMasterModels.Entity_Master_Name.Trim();

                var existingDepartment = this.mySqlDBContext.UnitMasterModels
                    .FirstOrDefault(d => d.Entity_Master_Name == UnitMasterModels.Entity_Master_Name && d.Entity_Master_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: entity with the same name already exists.");
                }
                var TypeModel = this.mySqlDBContext.UnitMasterModels;
                TypeModel.Add(UnitMasterModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                UnitMasterModels.Entity_Master_createdDate = dt1;
                UnitMasterModels.Entity_Master_Status = "Active";
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
        [Route("api/UnitMaster/UpdateEntityNames")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] UnitMasterModel UnitMasterModels)
        {

            try
            {
                if (UnitMasterModels.Entity_Master_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    UnitMasterModels.Entity_Master_Name = UnitMasterModels.Entity_Master_Name?.Trim();
                    var existingDepartment = this.mySqlDBContext.UnitMasterModels
                       .FirstOrDefault(d => d.Entity_Master_Name == UnitMasterModels.Entity_Master_Name && d.Entity_Master_id != UnitMasterModels.Entity_Master_id && d.Entity_Master_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Entity with the same name already exists.");
                    }
                    // Existing department, update logic
                    this.mySqlDBContext.Attach(UnitMasterModels);
                    this.mySqlDBContext.Entry(UnitMasterModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(UnitMasterModels);

                    Type type = typeof(UnitMasterModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(UnitMasterModels, null) == null || property.GetValue(UnitMasterModels, null).Equals(0))
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

        [Route("api/UnitMaster/DeleteEntityNames")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            var currentClass = new UnitMasterModel { Entity_Master_id = id };
            currentClass.Entity_Master_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Entity_Master_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
