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
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class HelpController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HelpController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("api/ViewHelpDesk/GetHelpDeskDetails")]
        [HttpGet]
        //return this.mySqlDBContext.HelpModels.Where(x => x.Status == "Active").ToList();

       public IEnumerable<dynamic> GetHelpDeskDetails()
{
    var list = (from helpdesk in mySqlDBContext.HelpModels
                join modules in mySqlDBContext.TaskModels on helpdesk.task_id equals modules.task_id
                join rolename in mySqlDBContext.RoleModels on helpdesk.ROLE_ID equals rolename.ROLE_ID
                where (helpdesk.Status == "Active")
                select new
                {
                    helpdesk.helpdeskID,
                    modules.task_name,
                    rolename.ROLE_ID,
                    rolename.ROLE_NAME,
                    helpdesk.FileName,
                    helpdesk.FilePath,
                    helpdesk.Status,

                }).ToList();

    return list;
}

        [Route("api/RoleName/GetAddRoleNameDetails/{moduleID}")]
        [HttpGet]
         public IEnumerable<object> GetAddRoleNameDetails(int moduleID)
        {
            var role_list = (from role_names in mySqlDBContext.RoleModels
                             join modules in mySqlDBContext.TaskModels on role_names.task_id equals modules.task_id
                             where (role_names.task_id == moduleID)
                             select new
                             {
                                 role_names.ROLE_ID,
                                 role_names.ROLE_NAME

                             })
                                 .ToList();
            return role_list;
        }

        [Route("api/GettingRoleName/GettingRoleNameDetails/{userID}")]
        [HttpGet]
        //public async Task<IActionResult> GettingRoleNameDetails(int userID)
        //{
        //    var getroles = await mySqlDBContext.userlocationmappingModels
        // .Where(x => x.USR_ID == userID && x.user_location_mapping_status == "Active")
        // .Select(x => x.ROLE_ID)  
        // .ToListAsync();  


        //    List<string> allFilePaths = new List<string>();
        //    foreach (var roleIdsString in getroles)
        //    {

        //        var roleIds = roleIdsString.Split(',');


        //        foreach (var roleId in roleIds)
        //        {
        //            var filenames = await mySqlDBContext.HelpModels
        //                .Where(helpdesk => helpdesk.ROLE_ID== int.Parse(roleId) ) 
        //                .Select(helpdesk =>
        //                helpdesk.FilePath)  
        //                .ToListAsync(); 

        //            allFilePaths.AddRange(filenames); 
        //        }
        //    }

        //    // Return all file paths as a JSON response
        //    return Ok(allFilePaths);
        //}
        public async Task<IActionResult> GettingRoleNameDetails(int userID)
        {
            // Get the role IDs based on the user
            var getroles = await mySqlDBContext.userlocationmappingModels
                .Where(x => x.USR_ID == userID && x.user_location_mapping_status == "Active")
                .Select(x => x.ROLE_ID)
                .ToListAsync();

            List<object> allFileDetails = new List<object>(); // List to hold all file details

            foreach (var roleIdsString in getroles)
            {
                var roleIds = roleIdsString.Split(',');

                foreach (var roleId in roleIds)
                {

                    var helpdeskDetails = await (from helpdesk in mySqlDBContext.HelpModels
                                                 join modules in mySqlDBContext.TaskModels on helpdesk.task_id equals modules.task_id
                                                 join rolename in mySqlDBContext.RoleModels on helpdesk.ROLE_ID equals rolename.ROLE_ID
                                                 where helpdesk.ROLE_ID == int.Parse(roleId)
                                                 select new
                                                 {
                                                     helpdesk.helpdeskID,
                                                     helpdesk.FilePath,
                                                     modules.task_name,
                                                     rolename.ROLE_NAME,
                                                     helpdesk.FileName
                                                 }).ToListAsync();



                    foreach (var helpdesk in helpdeskDetails)
                    {
                        allFileDetails.Add(new
                        {
                            helpdesk.helpdeskID,
                            helpdesk.task_name,
                            helpdesk.ROLE_NAME,
                            helpdesk.FilePath,
                            helpdesk.FileName,
                        });
                    }
                }
            }
            var distinctHelpdeskDetails = allFileDetails
             .Distinct()
               .ToList();

            // Return all file details as a JSON response
            return Ok(distinctHelpdeskDetails);
        }


        [Route("api/HelpDesk/AddHelpDesk")]
        [HttpPost]
        public IActionResult AddHelpDesk([FromForm] HelpModel helpDesk, [FromForm] IFormFile file)
        {
            // Ensure that the file is uploaded
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Define the local directory where files will be saved
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "HelpDeskFiles");

            // Create the directory if it doesn't exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Generate a unique file name
            var fileName = Path.GetFileNameWithoutExtension(file.FileName)
                             + "_" + DateTime.Now.Ticks
                             + Path.GetExtension(file.FileName);

            // Construct the file path for saving the file
            var filePath = Path.Combine(directoryPath, fileName);

            // Save the file to the local server directory
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Build the base URL for serving the file over HTTP (optional)
            var request = HttpContext.Request;
            string baseUrl = $"{request.Scheme}://{request.Host}";

            // Construct the URL for accessing the file (optional, for use in the database or response)
            string fileUrl = $"{baseUrl}/HelpDeskFiles/{fileName}";

            // Create a connection to the database
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
                {
                    con.Open();
                    // Step 1: Check if the combination of task_name and rolename already exists
                    var checkQuery = @"SELECT COUNT(*) FROM helpdesk WHERE task_id = @task_id AND ROLE_ID = @ROLE_ID";
                    var checkCommand = new MySqlCommand(checkQuery, con);
                    checkCommand.Parameters.AddWithValue("@task_id", helpDesk.task_id);
                    checkCommand.Parameters.AddWithValue("@ROLE_ID", helpDesk.ROLE_ID);

                    var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (count > 0)
                    {
                        // If a record exists with the same task_name and rolename, return an error

                        return Ok("This Task Name and Role Name combination already exists.");
                    }
                    else
                    {


                        // Insert the file information into the database
                        var command = new MySqlCommand(@"INSERT INTO helpdesk (task_id, ROLE_ID, FilePath, FileName,CreatedDate, Status) 
                                             VALUES (@task_id, @ROLE_ID, @FilePath, @FileName,@CreatedDate, @Status)", con);

                        command.Parameters.AddWithValue("@task_id", helpDesk.task_id);
                        command.Parameters.AddWithValue("@ROLE_ID", helpDesk.ROLE_ID);
                        command.Parameters.AddWithValue("@FilePath", fileUrl);  // Use the file URL here for serving the file
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.Parameters.AddWithValue("@CreatedDate", System.DateTime.Now);
                        command.Parameters.AddWithValue("@Status", "Active");

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("An error occurred: " + ex.Message);
                }
                return Ok(" Data added Successfully");
            }

            

        }

       


        
        [Route("api/HeplDesk/DeActivateHeplDesk")]
        [HttpPost]
        public IActionResult DeActivateHeplDesk([FromBody] HelpModel HelpModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string UpdateQuery = "UPDATE helpdesk SET Status=@Status,CreatedDate=@CreatedDate WHERE helpdeskID = @helpdeskID";

            try
            {
                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@helpdeskID", HelpModels.helpdeskID);
                    myCommand.Parameters.AddWithValue("@Status", "InActive");
                    myCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    myCommand.ExecuteNonQuery();
                }

                return Ok("Updated Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            finally
            {
                con.Close();
            }
        }

    }
}
