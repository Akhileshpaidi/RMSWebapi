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
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class SupAdmin_ActRegulatoryController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        private MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_ActRegulatoryController(CommonDBContext commonDBContext, MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/SupAdmin_ActRegulatory/GetActregulatory")]
        [HttpGet]

        public IEnumerable<object> GetActregulatory()
        {
            //return this.mySqlDBContext.Actregulatorymodels.Where(x => x.status == "Active").ToList();

            var act = from Actregulatory in commonDBContext.SupAdmin_ActRegulatoryModels
                      where Actregulatory.status == "Active"
                      select new
                      {
                          Actregulatory.actregulatoryid,
                          Actregulatory.actregulatoryname,
                          Actregulatory.global_actId,
                          actname = $"{Actregulatory.global_actId}-{Actregulatory.actregulatoryname}"
                      };
            var result = act.ToList();


            return result;
        }

        [Route("api/SupAdmin_ActRegulatory/InsertActRegulatory")]
        [HttpPost]
        public async Task<IActionResult> InsertActRegulatoryfile()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:commondb"]);
            con.Open();

            try
            {
                var Maxactid = this.commonDBContext.SupAdmin_ActRegulatoryModels.Max(d => (int?)d.actregulatoryid) ?? 50000;

                var newActRegulatoryId = Maxactid + 1;

                var form = HttpContext.Request.Form;
                var actregulatoryname = form["actregulatoryname"].FirstOrDefault(); // Get the first value
                var actrequlatorydescription = form["actrequlatorydescription"].FirstOrDefault(); // Get the first value
                var mainFile = form.Files; // This should contain the files
                var createdBy = form["userId"].FirstOrDefault();
                var weblinks = form["weblink"].ToString();
                // Get the current HTTP request
                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);
                // Generate global_act_id
                string globalactId = GenerateGlobalActId(con);

                var globalactIdFloder = Path.Combine("Reports", "GlobalActID", globalactId);

                DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(globalactIdFloder);


                // Insert data into act-regulatorymaster table using insertMasterQuery
                var insertMasterQuery = "INSERT INTO act_regulatorymaster (actregulatoryid,actregulatoryname, actrequlatorydescription, createddate,createdBy,status, global_actId) " +
                                        "VALUES (@actregulatoryid,@actregulatoryname, @actrequlatorydescription, @createddate,@createdBy,@status, @globalActId); " +
                                        "SELECT LAST_INSERT_ID();";

                MySqlCommand masterCommand = new MySqlCommand(insertMasterQuery, con);
                masterCommand.Parameters.AddWithValue("@actregulatoryid", newActRegulatoryId);
                masterCommand.Parameters.AddWithValue("@actregulatoryname", actregulatoryname);
                masterCommand.Parameters.AddWithValue("@actrequlatorydescription", actrequlatorydescription);
                masterCommand.Parameters.AddWithValue("@createddate", DateTime.Now);
                masterCommand.Parameters.AddWithValue("@createdBy", createdBy);
                masterCommand.Parameters.AddWithValue("@globalActId", globalactId);
                masterCommand.Parameters.AddWithValue("@status", "Active");
                int insertedActRegulatoryId = Convert.ToInt32(masterCommand.ExecuteScalar());

                List<string> fileList = new List<string>();

                foreach (var file in mainFile)
                {

                    // Save the file to the directory
                    var filePath = Path.Combine(globalactIdFloder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add the file path to the list
                    fileList.Add(filePath);

                    // Insert file attachment for each record
                    InsertFile("FileAttach", $"{baseUrl}/Reports/GlobalActID/{globalactId}/{file.FileName}", file, newActRegulatoryId, globalactId, file.FileName);
                }

                if (!string.IsNullOrEmpty(weblinks))
                {

                    string[] webLinksArray = weblinks.Split(';');

                    foreach (var weblink in webLinksArray)
                    {
                        // Insert each web link as a separate record
                        InsertFile("Weblink", weblink.Trim(), null, newActRegulatoryId, globalactId, null);
                    }
                }

                // Function to insert file or web link
                void InsertFile(string filecategory, string filepath, IFormFile file, int newActRegulatoryId, string globalactId, string fileName)
                {
                    var fileUploadModel = new Actregulatoryfilemodel
                    {
                        file_name = filecategory == "FileAttach" ? file?.Name : null, // Check if file is null before accessing its properties
                        filepath = filepath,
                        filecategory = filecategory,
                        status = "Active",
                        created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        actregulatoryid = newActRegulatoryId,
                        global_act_id = globalactId,
                    };

                    var Maxactfileid = this.commonDBContext.SupAdmin_Actregulatoryfilemodels.Max(d => (int?)d.bare_act_id) ?? 50000;

                    var newActfileRegulatoryId = Maxactfileid + 1;




                    string insertSubTableQuery = "INSERT INTO act_regulatory (bare_act_id,actregulatoryid, global_act_id, filecategory, filepath, status, created_date,file_name) " +
                                                    "VALUES (@bare_act_id,@actregulatoryid, @globalActId, @fileCategory, @filePath, @status, @createddate, @file_name)";

                    MySqlCommand subTableCommand = new MySqlCommand(insertSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@bare_act_id", newActfileRegulatoryId);
                    subTableCommand.Parameters.AddWithValue("@actregulatoryid", newActRegulatoryId);
                    subTableCommand.Parameters.AddWithValue("@globalActId", globalactId);
                    subTableCommand.Parameters.AddWithValue("@fileCategory", fileUploadModel.filecategory);
                    subTableCommand.Parameters.AddWithValue("@filePath", fileUploadModel.filepath);
                    subTableCommand.Parameters.AddWithValue("@status", fileUploadModel.status);
                    subTableCommand.Parameters.AddWithValue("@createddate", fileUploadModel.created_date);
                    subTableCommand.Parameters.AddWithValue("@file_name", fileName); // Pass the filename here

                    subTableCommand.ExecuteNonQuery();
                }


                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex}");
            }
        }

        private string GenerateGlobalActId(MySqlConnection con)
        {
            string gActId;

            // Retrieve the highest existing gActId from the database
            string getMaxIdQuery = "SELECT MAX(CAST(SUBSTRING(global_actId, LENGTH(global_actId) - 2) AS UNSIGNED)) FROM commondb.act_regulatorymaster";
            using (MySqlCommand getMaxIdCommand = new MySqlCommand(getMaxIdQuery, con))
            {
                object maxIdObj = getMaxIdCommand.ExecuteScalar();
                if (maxIdObj != DBNull.Value && maxIdObj != null)
                {
                    int maxSerialNo = Convert.ToInt32(maxIdObj);
                    maxSerialNo++; // Increment the last number by 1
                    gActId = $"GC{maxSerialNo:D4}"; // Ensure it's formatted with leading zeros
                }
                else
                {
                    // If no existing records, generate the default ID
                    gActId = "GC0001"; // Start the series from 001 onwards
                }
            }

            return gActId;
        }

        [Route("api/superadminactregulations/superadminGetactregulationsDetailsnyid/{actid}")]
        [HttpGet]

        public IEnumerable<object> superadminGetactregulationsDetailsnyid(int actid)
        {
            var result = (from actregulatory in commonDBContext.SupAdmin_ActRegulatoryModels
                          //join tbluser in mySqlDBContext.usermodels on actregulatory.createdBy equals tbluser.USR_ID
                          // user in mySqlDBContext.usermodels on actregulatory.updatedby equals user.USR_ID into userJoin from user in userJoin.DefaultIfEmpty()
                          where actregulatory.actregulatoryid == actid
                          select new
                          {
                              actregulatoryid = actregulatory.actregulatoryid,
                              actregulatoryname = actregulatory.actregulatoryname,
                              
                           //   createdby = actregulatory.createdBy,
                             // createddate = actregulatory.createddate,
                              globalactId = actregulatory.global_actId,
                              actname = $"{actregulatory.global_actId}-{actregulatory.actregulatoryname}",
                              actrequlatorydescription = actregulatory.actrequlatorydescription,
                            //  create = $"{tbluser.firstname}-{actregulatory.createddate}",
//update = $"{user.firstname}-{actregulatory.updateddate}"
                          })
                         .Distinct()
                         .ToList();

            var actFiles = commonDBContext.SupAdmin_Actregulatoryfilemodels
                .Where(af => result.Select(r => r.actregulatoryid).Contains(af.actregulatoryid) && af.status == "Active")
                .Select(af => new
                {
                    af.bare_act_id,
                    af.actregulatoryid,
                    af.filepath,
                    af.status,
                    filecategory = af.filecategory
                })
                .ToList();

            var modifiedResult = result.Select(item => new
            {
                item.actregulatoryid,
                item.actregulatoryname,
                item.actname,
               // item.create,
                item.actrequlatorydescription,
              
                item.globalactId,
//item.update,
//item.createddate,
                //item.createdby,

                actfiles = actFiles.Where(af => af.actregulatoryid == item.actregulatoryid && af.status == "Active")
                                   .Select(af => new
                                   {
                                       af.bare_act_id,
                                       af.filecategory,
                                       af.filepath,
                                       filename = af.filepath != null ?
                                                  af.filepath.Substring(af.filepath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList()
            })
            .ToList();

            return modifiedResult;
        }



        [Route("api/superadminactdownload/superadminactDownLoadFiles")]
        [HttpGet]
        public async Task<IActionResult> superadminDownloadFile(string filePath)
        {
            try
            {
                // Extract the file name from the URL
                //string[] segments = filePath.Split('/');
                //string extractedFileName = segments.LastOrDefault();

                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Invalid file name provided.");
                    return BadRequest("Invalid file name provided.");
                }

                // var filePath = Path.Combine("Reports", "GlobalAct", extractedFileName);

                // Debugging: Print file path
                Console.WriteLine($"File Path: {filePath}");

                Uri uri = new Uri(filePath);
                string relativePath = uri.LocalPath.TrimStart('/');

                // Assuming filePath is the local file path on the server
                // string localFilePath = Path.Combine("YourLocalFolderPath", relativePath);

                if (System.IO.File.Exists(relativePath))
                {
                    // Read the file content
                    byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(relativePath);

                    // Determine the file name from the local file path
                    string extractedFileName = Path.GetFileName(relativePath);

                    // Determine the file type based on the file extension or fileType parameter
                    string contentType = GetContentType(extractedFileName);

                    // Return the file content as a FileResult
                    return File(fileBytes, contentType, extractedFileName);
                }
                else
                {
                    Console.WriteLine("File does not exist at the specified path.");
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Internal Server Error: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        private string GetContentType(string fileName)
        {
            // Implement logic to determine the content type based on the file extension or fileType parameter
            // For simplicity, assume the content type based on the file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                case ".docx":
                    return "application/msword";
                case ".xls":
                case ".xlsx":
                    return "application/vnd.ms-excel";
                default:
                    return "application/octet-stream"; // Default content type for binary data
            }
        }




        [Route("Api/superadminactregulations/superadminUpdateActRegulatory")]
        [HttpPost]

        public async Task<IActionResult> superadminUpdateActRegulatory([FromQuery] int actregulatoryid)
        {
            try
            {
                var Maxactfileid = commonDBContext.SupAdmin_Actregulatoryfilemodels.Max(d => (int?)d.bare_act_id) ?? 50000;
                var newActfileRegulatoryId = Maxactfileid + 1;
                var act = await commonDBContext.SupAdmin_ActRegulatoryModels.FirstOrDefaultAsync(a => a.actregulatoryid == actregulatoryid);

                if (act == null)
                {
                    return NotFound();
                }

                var formCollection = await Request.ReadFormAsync();

                // Update actregulatory details
                act.actregulatoryname = formCollection["actregulatoryname"];
                act.actrequlatorydescription = formCollection["actrequlatorydescription"];
                act.updateddate = DateTime.Now.ToString("yyyy-MM-dd");
                act.updatedby = int.Parse(formCollection["updatedby"]);





                // Handle Weblinks
                var existingWeblinks = await commonDBContext.SupAdmin_Actregulatoryfilemodels
                    .Where(f => f.actregulatoryid == actregulatoryid && f.filecategory == "Weblink" && f.status == "Active")
                    .ToListAsync();

                //          var actFiles = await mySqlDBContext.Actregulatoryfilemodels
                //    .Where(f => f.actregulatoryid == actregulatoryid)
                //    .ToListAsync();


                var newWeblinks = formCollection["Weblink"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);

                var newWeblinkSet = new HashSet<string>(newWeblinks);

                // Remove existing weblinks that are not in the new weblinks
                foreach (var existingLink in existingWeblinks)
                {
                    if (!newWeblinkSet.Contains(existingLink.file_name))
                    {
                        existingLink.status = "Inactive";
                        existingLink.updateddate = DateTime.Now.ToString("yyyy-MM-dd");
                        existingLink.updatedby = int.Parse(formCollection["updatedby"]);
                        // mySqlDBContext.Actregulatoryfilemodels.Remove(existingLink);
                    }
                }
                // Add new weblinks that are not in the existing weblinks
                foreach (var link in newWeblinks)
                {
                    if (!existingWeblinks.Any(f => f.file_name == link))
                    {
                        commonDBContext.SupAdmin_Actregulatoryfilemodels.Add(new SupAdmin_Actregulatoryfilemodel
                        {
                            bare_act_id = newActfileRegulatoryId++,
                            actregulatoryid = actregulatoryid,
                            file_name = link,
                            filecategory = "Weblink",
                            filepath = link,
                            global_act_id = formCollection["global_act_id"],
                            created_date = DateTime.Now.ToString("yyyy-MM-dd"),
                            updateddate = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedby = int.Parse(formCollection["updatedby"]),
                            status = "Active",
                          
                        });
                    }
                }
                // Handle File Attachments
                var existingFiles = await commonDBContext.SupAdmin_Actregulatoryfilemodels
                    .Where(f => f.actregulatoryid == actregulatoryid && f.filecategory == "FileAttach" && f.status == "Active")
                    .ToListAsync();


                var files = Request.Form.Files;



                var global_act_id = formCollection["global_act_id"].ToString(); // Retrieve global_act_id from the form data

                // Ensure the directory exists
                var globalactIdFolder = Path.Combine("Reports", "GlobalActID", global_act_id);
                Directory.CreateDirectory(globalactIdFolder);
                // Remove existing file records that are not in the new files
                //foreach (var existingFile in existingFiles)
                //{
                //    if (!files.Any(f => f.FileName == existingFile.file_name) )
                //    {
                //        existingFile.status = "Inactive";
                //        existingFile.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                //        existingFile.updatedby = int.Parse(formCollection["updatedby"]);
                //        //  mySqlDBContext.ActRuleregulatoryfilemodels.Remove(existingFile);
                //    }
                //}

                // Add new files and update existing files
                foreach (var file in files)
                {
                    var fileName = file.FileName;
                    var filePath = Path.Combine(globalactIdFolder, fileName);
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    var fileUrl = new Uri(new Uri(baseUrl), $"Reports/GlobalActID/{global_act_id}/{fileName}").ToString();

                    var existingFile = existingFiles.FirstOrDefault(f => f.file_name == fileName);
                    if (existingFile != null)
                    {
                        // Update existing file properties
                        existingFile.filecategory = "FileAttach";
                        existingFile.filepath = fileUrl;
                        existingFile.created_date = DateTime.Now.ToString("yyyy-MM-dd");

                        // Overwrite the existing file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        // Save the new file to the directory
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Add new file entry to the database
                        commonDBContext.SupAdmin_Actregulatoryfilemodels.Add(new SupAdmin_Actregulatoryfilemodel
                        {
                            bare_act_id = newActfileRegulatoryId++,
                            actregulatoryid = actregulatoryid,
                            file_name = fileName,
                            filecategory = "FileAttach",
                            filepath = fileUrl,
                            global_act_id = global_act_id,
                            updateddate = DateTime.Now.ToString("yyyy-MM-dd"),
                            created_date = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedby = int.Parse(formCollection["updatedby"]),
                            status = "Active",
                          
                        });
                    }
                }

                await commonDBContext.SaveChangesAsync(); // Save changes for new files and weblinks

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while updating the Act Regulatory record.");
            }
        }


        [Route("api/superadminactregulations/removesupActregulatoryDetails/{bare_act_id}")]
        [HttpPost]
        public void removesupActregulatoryDetails(int bare_act_id)
        {
            try
            {
                var currentClass = new SupAdmin_Actregulatoryfilemodel { bare_act_id = bare_act_id };
                currentClass.status = "Inactive";
                currentClass.updateddate = DateTime.Now.ToString("yyyy-MM-dd");
                this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
                this.commonDBContext.SaveChanges();
            }
            catch
            {
                return;
            }
        }

    }

}
