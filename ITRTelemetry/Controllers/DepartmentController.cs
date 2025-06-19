using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using Org.BouncyCastle.Bcpg;
using MySqlConnector;
using System.Data;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Bibliography;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class DepartmentController : ControllerBase
    {
         
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        public DepartmentController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }
        //getting DepartmentMaster Detail by Departmentid
        [Route("api/DepartmentMaster/GetDepartmentMasterDetails")]
        [HttpGet]

        public IEnumerable<DepartmentModel> GetDepartmentMasterDetails()
        {
            return this.mySqlDBContext.DepartmentModels.Where(x => x.Department_Master_Status == "Active").ToList();
        }


        [Route("api/userDetails/GetuserDetails")]
        [HttpGet]
        public IEnumerable<tableuser> GetuserDetails()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
select emailid,USR_ID,Department_Master_id,firstname from
 risk.tbluser ", con);




            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<tableuser>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new tableuser
                    {


                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        emailid = dt.Rows[i]["emailid"].ToString(),
                        Department_Master_id = Convert.ToInt32(dt.Rows[i]["Department_Master_id"].ToString())
                    





                    });
                }

            }
            return pdata;
        }





        [Route("api/DepartmentMaster/GetDepartmentMasterDetailsById/{unit_location_Master_id}")]
        [HttpGet]

        public IEnumerable<object> GetDepartmentMasterDetails(int unit_location_Master_id)
        {
            // return this.mySqlDBContext.DepartmentModels.Where(x => x.Department_Master_Status == "Active").ToList();

            var departments = (from
                         Department in mySqlDBContext.DepartmentModels

                               join tbluser in mySqlDBContext.usermodels on Department.Department_Master_id equals tbluser.Department_Master_id


                               where tbluser.Unit_location_Master_id == unit_location_Master_id
                               select new
                               {

                                   Department.Department_Master_id,
                                   Department.Department_Master_name

                               })

                     .Distinct().ToList();

            return departments;

        }



        [Route("api/DepartmentMaster/GetUserDetailsById")]
        [HttpGet]

 public async Task<List<usermodel>> GetUserDetailsById( int[] USR_ID)
        {
            List<usermodel> usersList = new List<usermodel>();

            foreach (var user in USR_ID)
            {
                var usersForId = (from tbluser in mySqlDBContext.usermodels
                                  where tbluser.Department_Master_id == user && tbluser.USR_STATUS == "Active"
                                  select new usermodel
                                  {
                                      USR_ID = tbluser.USR_ID,
                                      firstname = tbluser.firstname,
                                      Department_Master_id=tbluser.Department_Master_id,
                                  })
                                 .ToList();

                usersList.AddRange(usersForId);
            }

            return usersList.Distinct().ToList();
        }

        //getting users by entity ,unit location and departments
        [Route("api/DepartmentMaster/GetUserDetailsByentityUntitDepartmentsIds")]
        [HttpGet]

        public async Task<List<usermodel>> GetUserDetailsByentityUntitDepartmentsIds(int[] USR_ID,int Entity_Master_id,int Unit_location_Master_id)
        {
            List<usermodel> usersList = new List<usermodel>();

            foreach (var user in USR_ID)
            {
                var usersForId = (from tbluser in mySqlDBContext.usermodels
                                  where tbluser.Department_Master_id == user && tbluser.USR_STATUS == "Active" && tbluser.Entity_Master_id == Entity_Master_id && tbluser.Unit_location_Master_id== Unit_location_Master_id
                                  select new usermodel
                                  {
                                      USR_ID = tbluser.USR_ID,
                                      firstname = tbluser.firstname,
                                      Department_Master_id = tbluser.Department_Master_id,
                                  })
                                 .ToList();

                usersList.AddRange(usersForId);
            }

            return usersList.Distinct().ToList();
        }



        //getting excluded users

        [Route("api/DepartmentMaster/GetExcludedUserDetailsById")]
        [HttpGet]
        public async Task<List<usermodel>> GetExcludedUserDetailsById([FromQuery] int[] USR_ID, [FromQuery] string uniqId)
        {
            List<usermodel> usersList = new List<usermodel>();
            List<int> excludedUserIds = new List<int>();
            int Entity_Master_id=0;
            int Unit_location_Master_id=0;

            // Step 1: Retrieve mapped_user IDs for exclusion
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                await con.OpenAsync();
                string query = "SELECT mapped_user,Entity_Master_id,Unit_location_Master_id FROM schedule_assessment WHERE uq_ass_schid = @uq_ass_schid AND status=@status";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@uq_ass_schid", uniqId);
                    cmd.Parameters.AddWithValue("@status", "Active");

                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {  
                        while (await reader.ReadAsync())
                        {
                            string[] mappedUsers = reader["mapped_user"].ToString().Split(',');
                            excludedUserIds.AddRange(mappedUsers.Select(id => Convert.ToInt32(id.Trim())));
                            Entity_Master_id = Convert.ToInt32(reader["Entity_Master_id"].ToString());
                            Unit_location_Master_id = Convert.ToInt32( reader["Unit_location_Master_id"].ToString());
                        }
                    }
                }
            }

            // Step 2: Retrieve users excluding the mapped_user IDs
            usersList = await mySqlDBContext.usermodels
                .Where(tbluser => USR_ID.Contains(tbluser.Department_Master_id)
                                  && tbluser.USR_STATUS == "Active" && tbluser.Entity_Master_id == Entity_Master_id && tbluser.Unit_location_Master_id == Unit_location_Master_id
                                  && !excludedUserIds.Contains(tbluser.USR_ID))
                .Select(tbluser => new usermodel
                {
                    USR_ID = tbluser.USR_ID,
                    firstname = tbluser.firstname,
                    Department_Master_id = tbluser.Department_Master_id,
                    // Include any other necessary properties here
                })
                .Distinct() // Use Distinct after projection to ensure unique results
                .ToListAsync();

            return usersList;
        }




        //getting included userids by uniqId

        [Route("api/DepartmentMaster/GetUserDetailsByuniqId/{uniqId}")]
        [HttpGet]
        public async Task<List<usermodel>> GetUserDetailsByuniqId(string uniqId)
        {
            List<int> userIds = new List<int>();
            List<usermodel> userDetails = new List<usermodel>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                await con.OpenAsync();

                // Query to get the mapped_user values for the specified uniqId
                string query = "SELECT mapped_user FROM schedule_assessment WHERE uq_ass_schid = @uq_ass_schid AND status=@status";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@uq_ass_schid", uniqId);
                    cmd.Parameters.AddWithValue("@status", "Active");


                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Retrieve comma-separated user IDs and add them to userIds list
                            string[] mappedUsers = reader["mapped_user"].ToString().Split(',');
                            userIds.AddRange(mappedUsers.Select(id => Convert.ToInt32(id.Trim())));
                        }
                    }
                }

                // Fetch user details for each userId
                foreach (var userId in userIds)
                {
                    string userQuery = "SELECT * FROM tbluser WHERE USR_ID = @user_id";

                    using (MySqlCommand userCmd = new MySqlCommand(userQuery, con))
                    {
                        userCmd.Parameters.AddWithValue("@user_id", userId);

                        using (MySqlDataReader userReader = await userCmd.ExecuteReaderAsync())
                        {
                            while (await userReader.ReadAsync())
                            {
                                usermodel user = new usermodel
                                {
                                    USR_ID = Convert.ToInt32(userReader["USR_ID"]),
                                    firstname = userReader["firstname"].ToString(),
                                   
                                };
                                userDetails.Add(user);
                            }
                        }
                    }
                }
            }

            return userDetails;
        }




        //insert DepartmentMaster Details
        [Route("api/DepartmentMaster/InsertDepartmentMasterDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] DepartmentModel DepartmentModels)
        {
            try
            {

                DepartmentModels.Department_Master_name = DepartmentModels.Department_Master_name?.Trim();

                var existingDepartment = this.mySqlDBContext.DepartmentModels
                    .FirstOrDefault(d => d.Department_Master_name == DepartmentModels.Department_Master_name.Trim() && d.Department_Master_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Department with the same name already exists.");
                }

                // Proceed with the insertion
                var DepartmentModel = this.mySqlDBContext.DepartmentModels;
                DepartmentModel.Add(DepartmentModels);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                DepartmentModels.Department_Master_CreatedDate = dt1;
                DepartmentModels.Department_Master_Status = "Active";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Department with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/DepartmentMaster/UpdateDepartmentMasterDetails")]
        [HttpPut]

        public IActionResult UpdateDepartmentModel([FromBody] DepartmentModel DepartmentModels)
        {
            try
            {
                if (DepartmentModels.Department_Master_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    DepartmentModels.Department_Master_name = DepartmentModels.Department_Master_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.DepartmentModels
                        .FirstOrDefault(d => d.Department_Master_name == DepartmentModels.Department_Master_name && d.Department_Master_id != DepartmentModels.Department_Master_id && d.Department_Master_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Department with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(DepartmentModels);
                    this.mySqlDBContext.Entry(DepartmentModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(DepartmentModels);

                    Type type = typeof(DepartmentModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        // Check if the property is null or has a default value (e.g., 0 for numeric types)
                        //if (property.GetValue(DepartmentModels) == null || Equals(property.GetValue(DepartmentModels), Activator.CreateInstance(property.PropertyType)))
                        //{
                        //    // Mark the property as not modified to exclude it from the update
                        //    entry.Property(property.Name).IsModified = false;
                        //}
                        if (property.GetValue(DepartmentModels, null) == null || property.GetValue(DepartmentModels, null).Equals(0))
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
                    return BadRequest("Error: Department with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/DepartmentMaster/DepartmentMasterDetails")]
        [HttpDelete]

        public void DeleteDepartmentMaster(int id)
        {
            var currentClass = new DepartmentModel { Department_Master_id = id };
            currentClass.Department_Master_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Department_Master_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    
}
}
