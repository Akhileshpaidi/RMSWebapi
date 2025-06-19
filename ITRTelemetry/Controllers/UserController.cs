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
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using Newtonsoft.Json;
using ITR_TelementaryAPI;
using OpenXmlPowerTools;
using Microsoft.Extensions.Configuration;

namespace ITR_TelementaryAPI.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly ClsEmail _emailService; // Declare ClsEmail instance

        private ClsEmail obj_Clsmail = new ClsEmail();

        public UserController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
            this._emailService = new ClsEmail();

        }

        [Route("api/UserMaster/GetuserMasterDetails")]
        [HttpGet]

        public IEnumerable<usermodel> GetUserMasterDetails()
        {
            return this.mySqlDBContext.usermodels.Where(x => x.USR_STATUS == "Active" && x.typeofuser == 1).ToList();
        }

        [Route("api/UserMaster/GetUserMasterDetailsModuleWise")]
        [HttpGet]

        public IEnumerable<object> GetUserMasterDetailsModuleWise(string module_id, int locationid, int userid)
        {
            //    return this.mySqlDBContext.usermodels
            //.Where(x => x.USR_STATUS == "Active"
            //            && (x.taskids == module_id) // Apply filter only if moduleid is non-zero
            //            && (x.Unit_location_Master_id == locationid) // Apply filter only if locationid is non-zero
            //            && (x.USR_ID != userid)) // Apply filter only if userid is non-zero
            //.ToList();


            var users = from userdetails in mySqlDBContext.usermodels
                        join user in mySqlDBContext.userlocationmappingModels on userdetails.USR_ID equals user.USR_ID
                        join Dept in mySqlDBContext.DepartmentModels on userdetails.Department_Master_id equals Dept.Department_Master_id
                        join unitlocation in mySqlDBContext.UnitLocationMasterModels on userdetails.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                        join entitymaster in mySqlDBContext.UnitMasterModels on userdetails.Entity_Master_id equals entitymaster.Entity_Master_id
                        where user.user_location_mapping_status == "Active" && user.taskID == Int32.Parse(module_id) && user.Unit_location_Master_id == locationid && user.USR_ID != userid
                        select new
                        {
                            userdetails.USR_ID,
                            firstname = $"{userdetails.firstname} / {userdetails.Designation} / {Dept.Department_Master_name} / {entitymaster.Entity_Master_Name} / {unitlocation.Unit_location_Master_name}"
                        };
            var result = users.ToList();
            return result;
        }

        [Route("api/UserMaster/GetusertpaMasterDetails")]
        [HttpGet]

        public IEnumerable<object> GetusernontpaMasterDetails(int userid)
        {
            // return this.mySqlDBContext.usermodels.Where(x => x.USR_STATUS == "Active" && x.typeofuser == 1).ToList();
            var userDetails = (from user in mySqlDBContext.usermodels
                               join tpauser in mySqlDBContext.tpausermodels on user.tpauserid equals tpauser.tpauserid
                               join tpaentity in mySqlDBContext.TPAEntitymodels on tpauser.tpaenityid equals tpaentity.tpaenityid
                               where user.USR_STATUS == "Active" && user.USR_ID == userid && user.typeofuser == 2
                               select new
                               {
                                   USR_ID = user.USR_ID,
                                   firstname = user.firstname,
                                   emailid = user.emailid,
                                   mobilenumber = user.mobilenumber,
                                   tpaentity = tpaentity.tpaenityname,
                                   designation = tpauser.designation,

                               })
                               .ToList();
            return userDetails;
        }

        [Route("api/UserMaster/GetuserDetails")]
        [HttpGet]

        public IEnumerable<usermodel> GetuserDetails()
        {
            return this.mySqlDBContext.usermodels.Where(x => x.USR_STATUS == "Active" && x.defaultrole != 7).ToList();
        }

        [Route("api/UserMaster/GettpauserDetails/{tpaentity}")]
        [HttpGet]

        public IEnumerable<object> GettpauserDetails(int tpaentity)
        {
            // return this.mySqlDBContext.usermodels.Where(x => x.USR_STATUS == "Active" && x.typeofuser == 2 && ).ToList();


            var tpauser = (from users in mySqlDBContext.usermodels
                           where users.tpaenityid == tpaentity && users.USR_STATUS == "Active" && users.typeofuser == 2
                           select new
                           {
                               usrid = users.USR_ID,
                               firstname = users.firstname,
                           })
                             .ToList();
            return tpauser;
        }

        [HttpGet]
        [Route("api/UserMaster/GetUsersWithDefaultRole")]
        public IActionResult GetUsersWithDefaultRole(int defaultroleid)
        {
            try
            {
                var usersWithDefaultRole = this.mySqlDBContext.usermodels
                    .Where(x => x.USR_STATUS == "Active" && x.defaultrole == defaultroleid)
                    .ToList();

                return Ok(usersWithDefaultRole);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }


        [Route("api/DepartmentName/GetDepartmentNameDetails/{USR_ID}")]
        [HttpGet]
        public IEnumerable<object> GetDepartmentDetails(int USR_ID)
        {

            //return this.mySqlDBContext.userlocationmappingModels.Where(x => x.user_location_mapping_status == "Active" && x.user_location_mapping_id == user_location_mapping_id).ToList();


            var deatils = (from tblusermaster in mySqlDBContext.usermodels
                           join usermappingtmaster in mySqlDBContext.userlocationmappingModels on tblusermaster.USR_ID equals usermappingtmaster.USR_ID
                           join departmentmaster in mySqlDBContext.DepartmentModels on tblusermaster.Department_Master_id equals departmentmaster.Department_Master_id
                           where usermappingtmaster.user_location_mapping_status == "Active" && departmentmaster.Department_Master_Status == "Active"
                           select new
                           {
                               tblusermaster.firstname,
                               tblusermaster.lastname,
                               usermappingtmaster.USR_ID,
                               departmentmaster.Department_Master_name,
                               //usermappingtmaster.user_location_mapping_id,
                               //usermappingtmaster.ROLE_ID,

                           })
                            .Distinct();
            return deatils;
        }



        [Route("api/UserMaster/GetuserMasterDetailsbyUnitId/{Unit_location_Master_id}")]
        [HttpGet]

        public IEnumerable<usermodel> GetUserMasterDetails(int Unit_location_Master_id)
        {
            return this.mySqlDBContext.usermodels.Where(x => x.USR_STATUS == "Active" && x.Unit_location_Master_id == Unit_location_Master_id).ToList();
        }

        //insert DepartmentMaster Details
        [Route("api/UserMaster/InsertUserMasterDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] usermodel usermodels)
        {
            var usermodel = this.mySqlDBContext.usermodels;
            usermodel.Add(usermodels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            usermodels.CREATED_DATE = dt1;
            usermodels.USR_STATUS = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        //insert RaiseQuery Details
        [Route("api/UserMaster/InsertRaiseQueryDetails")]
        [HttpPost]

        public async Task<IActionResult> InsertRaiseQueryParameter([FromBody] raisequeryModel raisequery)
        {
            var raisequerymodel = this.mySqlDBContext.raisequery;
            raisequerymodel.Add(raisequery);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            raisequery.createdDate = dt1;
            raisequery.status = "Open";
            //raisequery.queryImportance=raisequerymodel.
            this.mySqlDBContext.SaveChanges();
            // Send Notification to Reporting Person
            // SendNotification(raisequery.reportingPersonEmail, "New Issue Raised");

            var reportingEmail = await mySqlDBContext.usermodels
                                .Where(x => x.USR_ID == raisequery.reportingpersonid)
                                .Select(x => x.emailid)
                                .FirstOrDefaultAsync();
            var request = HttpContext.Request;
            string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

            _emailService.NotifyReportingPerson(reportingEmail, raisequery.reportingpersonid, raisequery.userid ,raisequery.subjectTitle,raisequery.issueDetails, baseUrl);


            return Ok();
        }


        [Route("api/UserMaster/UploadFile")]
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] List<IFormFile> file, [FromForm] string uploadedBy, [FromForm] string userid, [FromForm] string trackingNo)
        {
            if (file == null || file.Count == 0)
                return BadRequest("No files uploaded");

            foreach (var f in file)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await f.CopyToAsync(memoryStream);
                    var fileData = memoryStream.ToArray();

                    var uploadedFile = new raisequeryfilesModel
                    {
                        fileName = "raise_" + f.FileName,
                        fileType = f.ContentType,
                        fileData = fileData,
                        uploadedBy = uploadedBy,
                        userid = userid,
                        trackingNo = trackingNo,
                        uploadedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    mySqlDBContext.raisequeryfiles.Add(uploadedFile);
                }
            }

            await mySqlDBContext.SaveChangesAsync();
            return Ok("Files uploaded successfully");

        }
        //[HttpGet("GetFiles")]
        [Route("api/UserMaster/GetFiles")]
        [HttpGet]
        public IActionResult GetFiles(string trackingNo)
        {
            var files = mySqlDBContext.raisequeryfiles
        .Where(f => f.trackingNo == trackingNo) // Filter by trackingNo
        .Select(f => new { f.raisequeryfilesid, f.fileName, f.fileType })
        .ToList();

            if (files.Count == 0)
            {
                return NotFound("No files found for the given tracking number.");
            }

            return Ok(files);
        }
        //[HttpGet("DownloadFile/{id}")]
        [Route("api/UserMaster/DownloadFile/{id}")]
        [HttpGet]
        public IActionResult DownloadFile(int id)
        {
            var file = mySqlDBContext.raisequeryfiles.Find(id);
            if (file == null)
                return NotFound("File not found");

            return File(file.fileData, file.fileType, file.fileName);
        }
        //Review File Attachments Upload
        [Route("api/UserMaster/ReviewUploadFile")]
        [HttpPost]
        public async Task<IActionResult> ReviewUploadFile([FromForm] IFormFile file, [FromForm] string uploadedBy, [FromForm] string userid, [FromForm] string trackingNo)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                var uploadedFile = new reviewqueryfilesModel
                {
                    fileName = "review_" + file.FileName,
                    fileType = file.ContentType,
                    fileData = fileData,
                    uploadedBy = uploadedBy,
                    reviewuserid = userid,
                    trackingNo = trackingNo,
                    uploadedDate = DateTime.Now.ToString()
                };

                mySqlDBContext.reviewqueryfiles.Add(uploadedFile);
                await mySqlDBContext.SaveChangesAsync();
            }

            return Ok("File uploaded successfully");
        }
        [Route("api/UserMaster/GetReviewFiles")]
        [HttpGet]
        public IActionResult GetReviewFiles(string trackingNo)
        {
            var files = mySqlDBContext.reviewqueryfiles
        .Where(f => f.trackingNo == trackingNo) // Filter by trackingNo
        .Select(f => new { f.reviewqueryfilesid, f.fileName, f.fileType })
        .ToList();

            if (files.Count == 0)
            {
                return NotFound("No files found for the given tracking number.");
            }

            return Ok(files);
        }
        //[HttpGet("DownloadFile/{id}")]
        [Route("api/UserMaster/DownloadReviewFile/{id}")]
        [HttpGet]
        public IActionResult DownloadReviewFile(int id)
        {
            var file = mySqlDBContext.reviewqueryfiles.Find(id);
            if (file == null)
                return NotFound("File not found");

            return File(file.fileData, file.fileType, file.fileName);
        }

        [Route("api/UserMaster/UpdateRaiseQueryFileDetails")]
        [HttpPut]
        public async Task<IActionResult> UpdateRaiseQueryFileDetails(
   [FromForm] string trackingNo,
   [FromForm] List<IFormFile> filesdata)
        {
            if (string.IsNullOrEmpty(trackingNo))
            {
                return BadRequest("Tracking number is required.");
            }

            var existingQuery = this.mySqlDBContext.raisequery.FirstOrDefault(q => q.trackingNo == trackingNo);
            if (existingQuery == null)
            {
                return NotFound("Query not found.");
            }

            // Update fields
            //existingQuery.resolutionCategory = resolutionCategory;
            //existingQuery.resolutionDetails = resolutionDetails;
            //existingQuery.resolutionQueryDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //existingQuery.status = status ?? existingQuery.status;

            // Save uploaded files
            List<string> filePaths = new List<string>();
            if (filesdata != null && filesdata.Count > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in filesdata)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    filePaths.Add(filePath);
                }
            }

            // Save file paths to the database
            existingQuery.filesdata = string.Join(";", filePaths);

            this.mySqlDBContext.SaveChanges();

            return Ok(new { message = "Query updated successfully", filePaths });
        }

        //Update RaiseQuery Details
        //    [Route("api/UserMaster/UpdateRaiseQueryDetails")]
        //    [HttpPut]
        //    public IActionResult UpdateRaiseQueryDetails(
        //[FromQuery] string trackingNo,
        //[FromQuery] string resolutionCategory,
        //[FromQuery] string resolutionDetails,
        //[FromQuery] string status, [FromQuery] string resolutionFiles)
        //    {
        //        if (string.IsNullOrEmpty(trackingNo))
        //        {
        //            return BadRequest("Tracking number is required.");
        //        }

        //        // Find the existing query by tracking number
        //        var existingQuery = this.mySqlDBContext.raisequery.FirstOrDefault(q => q.trackingNo == trackingNo);

        //        if (existingQuery == null)
        //        {
        //            return NotFound("Query not found.");
        //        }

        //        // Update fields
        //        existingQuery.resolutionCategory = resolutionCategory;
        //        existingQuery.resolutionDetails = resolutionDetails;
        //       // existingQuery.resolutionfiles = resolutionfiles;  // Ensure proper file handling
        //        existingQuery.resolutionQueryDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //        existingQuery.status = status ?? existingQuery.status;  // Maintain status if not provided

        //        // Decode Base64 files and save them
        //        List<string> filePaths = new List<string>();
        //        if (!string.IsNullOrEmpty(resolutionFiles))
        //        {
        //            try
        //            {
        //                List<string> base64Files = JsonConvert.DeserializeObject<List<string>>(resolutionFiles);
        //                foreach (var base64File in base64Files)
        //                {
        //                    string fileName = $"file_{Guid.NewGuid()}.pdf";  // Change extension if needed
        //                    string filePath = Path.Combine("wwwroot/uploads", fileName);

        //                    byte[] fileBytes = Convert.FromBase64String(base64File.Split(',')[1]); // Remove metadata
        //                    System.IO.File.WriteAllBytes(filePath, fileBytes);
        //                    filePaths.Add(filePath);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                return BadRequest("Error processing files: " + ex.Message);
        //            }
        //        }

        //        // Save file paths to database
        //        existingQuery.resolutionfiles = string.Join(";", filePaths);

        //        // Save changes
        //        this.mySqlDBContext.SaveChanges();

        //        return Ok("Query updated successfully.");
        //    }

        [Route("api/UserMaster/UpdateRaiseQueryDetails")]
        [HttpPut]
        public async Task<IActionResult> UpdateRaiseQueryDetails(
    [FromForm] string trackingNo,
    [FromForm] string resolutionCategory,
    [FromForm] string resolutionDetails,
    [FromForm] string status,
    [FromForm] List<IFormFile> resolutionFiles)
        {
            if (string.IsNullOrEmpty(trackingNo))
            {
                return BadRequest("Tracking number is required.");
            }

            var existingQuery = this.mySqlDBContext.raisequery.FirstOrDefault(q => q.trackingNo == trackingNo);
            if (existingQuery == null)
            {
                return NotFound("Query not found.");
            }

            // Update fields
            existingQuery.resolutionCategory = resolutionCategory;
            existingQuery.resolutionDetails = resolutionDetails;
            existingQuery.resolutionQueryDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            existingQuery.status = status ?? existingQuery.status;

            // Save uploaded files
            List<string> filePaths = new List<string>();
            if (resolutionFiles != null && resolutionFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in resolutionFiles)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    filePaths.Add(filePath);
                }
            }

            // Save file paths to the database
            existingQuery.resolutionfiles = string.Join(";", filePaths);

            this.mySqlDBContext.SaveChanges();

            return Ok(new { message = "Query updated successfully", filePaths });
        }


        [Route("api/UserMaster/GetRaiseQueryDetails")]
        [HttpGet]
        public IEnumerable<object> GetRaiseQueryDetails()
        {
            var details = (from tblusermaster in mySqlDBContext.raisequery.AsNoTracking()
                           join taskmaster in mySqlDBContext.TaskModels.AsNoTracking()
                               on tblusermaster.taskid equals taskmaster.task_id
                           join appcomponent in mySqlDBContext.Componentmodels.AsNoTracking()
                               on tblusermaster.componentid equals appcomponent.id into menuGroup
                           from appcomponent in menuGroup.DefaultIfEmpty()
                           join reportingUser in mySqlDBContext.usermodels.AsNoTracking()
                               on tblusermaster.reportingpersonid equals reportingUser.USR_ID into reportingGroup
                           from reportingUser in reportingGroup.DefaultIfEmpty()
                           join raisedUser in mySqlDBContext.usermodels.AsNoTracking()
                               on tblusermaster.userid equals raisedUser.USR_ID into raisedGroup
                           from raisedUser in raisedGroup.DefaultIfEmpty()
                           where (tblusermaster.status == "Open" || tblusermaster.status == "Closed") && taskmaster.task_status == "Active"
                           select new
                           {
                               tblusermaster.queryImportance,
                               tblusermaster.taskid,
                               tblusermaster.menuItem,
                               tblusermaster.componentid,
                               tblusermaster.reportingpersonid,
                               tblusermaster.userid,
                               tblusermaster.subjectTitle,
                               tblusermaster.issueDetails,
                               tblusermaster.filesdata,
                               tblusermaster.trackingNo,
                               tblusermaster.createdDate,
                               tblusermaster.status,
                               tblusermaster.resolutionCategory,
                               tblusermaster.resolutionDetails,
                               tblusermaster.resolutionfiles,
                               tblusermaster.resolutionQueryDate,
                               taskmaster.task_name,
                               appcomponent.menu_item,
                               appcomponent.name,
                               ReportingPersonName = reportingUser.firstname, // Reporting Person Name
                               RaisedPersonName = raisedUser.firstname // Raised Person Name
                           })
              .Distinct()
              .ToList();

            var processedDetails = details.Select(d => new
            {
                d.queryImportance,
                d.taskid,
                d.menuItem,
                d.componentid,
                d.reportingpersonid,
                d.userid,
                d.subjectTitle,
                d.issueDetails,
                d.filesdata,
                d.trackingNo,
                d.createdDate,
                d.status,
                d.resolutionCategory,
                d.resolutionDetails,
                resolutionfiles = !string.IsNullOrEmpty(d.resolutionfiles)
        ? d.resolutionfiles.Split(',').Select(f => f.Trim()).ToList()
        : new List<string>(), // Convert to List here
                d.resolutionQueryDate,
                d.task_name,
                d.menu_item,
                d.name,
                d.ReportingPersonName,
                d.RaisedPersonName
            }).ToList();

            return processedDetails;

            //return details;

            //var details = (from tblusermaster in mySqlDBContext.raisequery.AsNoTracking()
            //               join taskmaster in mySqlDBContext.TaskModels.AsNoTracking() on tblusermaster.taskid equals taskmaster.task_id
            //               join appcomponent in mySqlDBContext.Componentmodels.AsNoTracking() on tblusermaster.componentid equals appcomponent.id
            //               into menuGroup from appcomponent in menuGroup.DefaultIfEmpty()
            //               join usermaster in mySqlDBContext.usermodels.AsNoTracking() on tblusermaster.reportingpersonid equals usermaster.USR_ID
            //               where tblusermaster.status == "Active" && taskmaster.task_status == "Active"
            //               select new
            //               {
            //                   tblusermaster.queryImportance,
            //                   tblusermaster.taskid,
            //                   tblusermaster.menuItem,
            //                   tblusermaster.componentid,
            //                   tblusermaster.reportingpersonid,
            //                   tblusermaster.subjectTitle,
            //                   tblusermaster.issueDetails,
            //                   tblusermaster.filesdata,
            //                   tblusermaster.trackingNo,
            //                   tblusermaster.createdDate,
            //                   tblusermaster.status,
            //                   taskmaster.task_name,
            //                   appcomponent.menu_item,
            //                   appcomponent.name,
            //                   usermaster.firstname
            //               })
            //              .Distinct()
            //              .ToList(); // Ensure query execution here

            //return details;
        }
        [Route("api/UserMaster/GetReviewQueryDetails")]
        [HttpGet]
        public IEnumerable<object> GetReviewQueryDetails(int userid)
        {
            var details = (from tblusermaster in mySqlDBContext.raisequery.AsNoTracking()
                           join taskmaster in mySqlDBContext.TaskModels.AsNoTracking()
                               on tblusermaster.taskid equals taskmaster.task_id
                           join appcomponent in mySqlDBContext.Componentmodels.AsNoTracking()
                               on tblusermaster.componentid equals appcomponent.id into menuGroup
                           from appcomponent in menuGroup.DefaultIfEmpty()
                           join reportingUser in mySqlDBContext.usermodels.AsNoTracking()
                               on tblusermaster.reportingpersonid equals reportingUser.USR_ID into reportingGroup
                           from reportingUser in reportingGroup.DefaultIfEmpty()
                           join raisedUser in mySqlDBContext.usermodels.AsNoTracking()
                               on tblusermaster.userid equals raisedUser.USR_ID into raisedGroup
                           from raisedUser in raisedGroup.DefaultIfEmpty()
                           where (tblusermaster.status == "Open" || tblusermaster.status == "Closed") && taskmaster.task_status == "Active" && tblusermaster.reportingpersonid == userid
                           select new
                           {
                               tblusermaster.queryImportance,
                               tblusermaster.taskid,
                               tblusermaster.menuItem,
                               tblusermaster.componentid,
                               tblusermaster.reportingpersonid,
                               tblusermaster.userid,
                               tblusermaster.subjectTitle,
                               tblusermaster.issueDetails,
                               tblusermaster.filesdata,
                               tblusermaster.trackingNo,
                               tblusermaster.createdDate,
                               tblusermaster.status,
                               tblusermaster.resolutionCategory,
                               tblusermaster.resolutionDetails,
                               tblusermaster.resolutionfiles,
                               tblusermaster.resolutionQueryDate,
                               tblusermaster.userEmail,
                               tblusermaster.reportingPersonEmail,
                               taskmaster.task_name,
                               appcomponent.menu_item,
                               appcomponent.name,
                               ReportingPersonName = reportingUser.firstname, // Reporting Person Name
                               RaisedPersonName = raisedUser.firstname // Raised Person Name
                           })
              .Distinct()
              .ToList();

            return details;

        }


        [Route("api/UserMaster/GetRaiseQueryDetailsbyid/{id}")]
        [HttpGet]
        public IEnumerable<object> GetRaiseQueryDetailsbyid(int id)
        {
            var details = (from tblusermaster in mySqlDBContext.raisequery.AsNoTracking()
                           join taskmaster in mySqlDBContext.TaskModels.AsNoTracking()
                               on tblusermaster.taskid equals taskmaster.task_id
                           join appcomponent in mySqlDBContext.Componentmodels.AsNoTracking()
                               on tblusermaster.componentid equals appcomponent.id into menuGroup
                           from appcomponent in menuGroup.DefaultIfEmpty()
                           join reportingUser in mySqlDBContext.usermodels.AsNoTracking()
                               on tblusermaster.reportingpersonid equals reportingUser.USR_ID into reportingGroup
                           from reportingUser in reportingGroup.DefaultIfEmpty()
                           join raisedUser in mySqlDBContext.usermodels.AsNoTracking()
                               on tblusermaster.userid equals raisedUser.USR_ID into raisedGroup
                           from raisedUser in raisedGroup.DefaultIfEmpty()
                           where (tblusermaster.status == "Open" || tblusermaster.status == "Closed") && taskmaster.task_status == "Active" && tblusermaster.userid == id
                           select new
                           {
                               tblusermaster.queryImportance,
                               tblusermaster.taskid,
                               tblusermaster.menuItem,
                               tblusermaster.componentid,
                               tblusermaster.reportingpersonid,
                               tblusermaster.userid,
                               tblusermaster.subjectTitle,
                               tblusermaster.issueDetails,
                               tblusermaster.filesdata,
                               tblusermaster.trackingNo,
                               tblusermaster.createdDate,
                               tblusermaster.status,
                               tblusermaster.resolutionCategory,
                               tblusermaster.resolutionDetails,
                               tblusermaster.resolutionfiles,
                               tblusermaster.resolutionQueryDate,
                               taskmaster.task_name,
                               appcomponent.menu_item,
                               appcomponent.name,
                               ReportingPersonName = reportingUser.firstname, // Reporting Person Name
                               RaisedPersonName = raisedUser.firstname // Raised Person Name
                           })
              .Distinct()
              .ToList();

            var processedDetails = details.Select(d => new
            {
                d.queryImportance,
                d.taskid,
                d.menuItem,
                d.componentid,
                d.reportingpersonid,
                d.userid,
                d.subjectTitle,
                d.issueDetails,
                d.filesdata,
                d.trackingNo,
                d.createdDate,
                d.status,
                d.resolutionCategory,
                d.resolutionDetails,
                resolutionfiles = !string.IsNullOrEmpty(d.resolutionfiles)
        ? d.resolutionfiles.Split(',').Select(f => f.Trim()).ToList()
        : new List<string>(), // Convert to List here
                d.resolutionQueryDate,
                d.task_name,
                d.menu_item,
                d.name,
                d.ReportingPersonName,
                d.RaisedPersonName
            }).ToList();

            return processedDetails;


        }







        [Route("api/UserMaster/download")]
        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine("C:\\Uploads", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var mimeType = "application/octet-stream";
            return PhysicalFile(filePath, mimeType, fileName);
        }


        [Route("api/UserMaster/UpdateUserMasterDetails")]
        [HttpPut]

        public void Updateusermodel([FromBody] usermodel usermodels)
        {
            if (usermodels.USR_ID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(usermodels);
                this.mySqlDBContext.Entry(usermodels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(usermodels);

                Type type = typeof(usermodel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(usermodels, null) == null || property.GetValue(usermodels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }
        [Route("api/UserMaster/UserMasterDetails")]
        [HttpDelete]

        public void DeleteUserMaster(int id)
        {
            var currentClass = new usermodel { USR_ID = id };
            currentClass.USR_STATUS = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("USR_STATUS").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        [Route("api/UserMaster/GetActiveUserDetails")]
        [HttpGet]
        public IEnumerable<object> GetActiveUserDetails()
        {



            var deatils = (from tblusermaster in mySqlDBContext.usermodels

                           join departmentmaster in mySqlDBContext.DepartmentModels on tblusermaster.Department_Master_id equals departmentmaster.Department_Master_id
                           where tblusermaster.USR_STATUS == "Active" && departmentmaster.Department_Master_Status == "Active"
                           select new
                           {
                               tblusermaster.firstname,
                               tblusermaster.lastname,
                               tblusermaster.USR_ID,
                               departmentmaster.Department_Master_name,
                               //usermappingtmaster.user_location_mapping_id,
                               //usermappingtmaster.ROLE_ID,

                           })
                            .Distinct();
            return deatils;
        }

        [Route("api/UserMaster/SendAccessMail")]
        [HttpPost]
        public async Task<IActionResult> SendAccessMail([FromBody] mailnotification request)
        {
            try
            {
                var reportingEmail = await mySqlDBContext.usermodels
                                       .Where(x => x.USR_ID == request.ReportingPersonId)
                                       .Select(x => x.emailid)
                                       .FirstOrDefaultAsync();

                // Call QueryProvideAccessMail method from ClsEmail
               // _emailService.NotifyReportingPerson(reportingEmail, request.IssueTitle, request.IssueDescription, request.SenderId, request.ReportingPersonId, request.BaseUrl);
                //_emailService.ProvideAccessMail(request.ReportingPersonEmail, request.DocumentNames, request.SenderId, request.ReportingPersonId, request.BaseUrl);
                return Ok(new { message = "Mail sent successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }




        [Route("api/User/ForgotPasswordMail")]
        [HttpPost]
        public IActionResult ForgotPasswordMail([FromBody] usermodel request)
        {
            if (string.IsNullOrWhiteSpace(request.USR_LOGIN))
            {
                return BadRequest(new Response
                {
                    ResponseCode = "1",
                    ResponseDesc = "EUser Login ID required.",
                    ResponseData = ""
                });
            }

            try
            {

                var user = mySqlDBContext.usermodels.FirstOrDefault(u => u.USR_LOGIN == request.USR_LOGIN && u.USR_STATUS == "Active");

                if (user == null)
                {
                    return NotFound(new Response
                    {
                        ResponseCode = "0",
                        ResponseDesc = "User Logid not found or user is inactive.",
                        ResponseData = ""
                    });
                }

                // Generate reset token and expiry
                string resetToken = Guid.NewGuid().ToString("N").Substring(0, 10);

                DateTime expiry = System.DateTime.Now.AddMinutes(20);

                // Save token
                var tokenEntry = new forgetpasswordtoken
                {
                    UserId = user.USR_ID,
                    Token = resetToken,
                    ExpiryDate = expiry
                };

                mySqlDBContext.forgetpasswordtokens.Add(tokenEntry);
                mySqlDBContext.SaveChanges();

                // Build reset link
                string resetLink = $"{resetToken}";

                // Email body
                string subject = "Reset Your Password";
                string body = $"This is token  to reset your password:<br>{resetLink}<br>This link is valid for 20 Minutes.";




                obj_Clsmail.SendEmaillink(user.emailid.ToLower(), subject, body);

                return Ok(new Response
                {
                    ResponseCode = "1",
                    ResponseDesc = "Password reset Token has been sent to your registered email.",
                    ResponseData = resetLink
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new Response
                {
                    ResponseCode = "3",
                    ResponseDesc = "Server error: " + ex.Message,
                    ResponseData = ""
                });
            }
        }



        public class Response
        {
            public string ResponseCode { get; set; }
            public string ResponseDesc { get; set; }
            public string ResponseData { get; set; }
        }


        // token verfication
        [Route("api/user/tokenverfication")]
        [HttpPost]

        public async Task<IActionResult> tokenverfication([FromBody] forgetpasswordtoken request)
        {

            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new
                {
                    ResponseCode = "0",
                    ResponseDesc = "Email and Token are required"
                });
            }

            var tokenEntry = await mySqlDBContext.forgetpasswordtokens
                .FirstOrDefaultAsync(t => t.Token == request.Token);

            if (tokenEntry == null)
            {
                return Ok(new
                {
                    ResponseCode = "0",
                    ResponseDesc = "Invalid token ."
                });
            }

            if (tokenEntry.ExpiryDate > System.DateTime.Now)
            {
                return Ok(new
                {
                    ResponseCode = "1",
                    ResponseDesc = "Token verified successfully.",
                    ResponseData = tokenEntry.UserId
                });

            }
            return Ok(new
            {
                ResponseCode = "0",
                ResponseDesc = "Token has expired."

            });

        }
        ///



        [Route("api/Authenticate/ForgotPasswordReset")]
        [HttpPost]

        public IActionResult ForgotPasswordReset([FromBody] usermodel usermodels)
        {


            if (usermodels.USR_ID == 0)
            {
                // Handle the case when USR_ID is 0 (e.g., insert logic)
                // You may want to return an appropriate response or redirect
                return BadRequest(new
                {
                    ResponseCode = "0",
                    ResponseDesc = "user id could not fould "
                });
            }
            else
            {
                // Attach the entity to the context
                this.mySqlDBContext.Attach(usermodels);

                // Update only the Password property
                this.mySqlDBContext.Entry(usermodels).Property("password").IsModified = true;

                try
                {
                    // Save changes
                    this.mySqlDBContext.SaveChanges();
                    return Ok(new
                    {
                        ResponseCode = "1",
                        ResponseDesc = "password updated  successfully.",

                    });
                }
                catch (DbUpdateException ex)
                {
                    // Handle exceptions (e.g., unique constraint violation)
                    return BadRequest($"Error updating password: {ex.Message}");
                }
            }
        }


        [Route("api/Authenticate/ForcePasswordReset")]
        [HttpPost]

        public IActionResult ForcePasswordReset([FromBody] usermodel usermodels)
        {


            if (usermodels.USR_ID == 0)
            {
                // Handle the case when USR_ID is 0 (e.g., insert logic)
                // You may want to return an appropriate response or redirect
                return BadRequest(new
                {
                    ResponseCode = "0",
                    ResponseDesc = "user id could not fould "
                });
            }
            else
            {
                // Attach the entity to the context
                this.mySqlDBContext.Attach(usermodels);

                // Update only the Password property
                this.mySqlDBContext.Entry(usermodels).Property("password").IsModified = true;
                this.mySqlDBContext.Entry(usermodels).Property("isFirstLogin").IsModified = true;

                // Set the new value for isfirttime
                usermodels.isFirstLogin = 1;
                try
                {
                    // Save changes
                    this.mySqlDBContext.SaveChanges();
                    return Ok(new
                    {
                        ResponseCode = "1",
                        ResponseDesc = "password updated  successfully.",

                    });
                }
                catch (DbUpdateException ex)
                {
                    // Handle exceptions (e.g., unique constraint violation)
                    return BadRequest($"Error updating password: {ex.Message}");
                }
            }
        }




    }



}
