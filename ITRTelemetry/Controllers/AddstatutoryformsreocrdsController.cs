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
using DocumentFormat.OpenXml.Spreadsheet;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AddstatutoryformsreocrdsController : ControllerBase
    {

        private MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;


        public AddstatutoryformsreocrdsController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("api/statutoryfrom/GetstatutoryfromDetails")]
        [HttpGet]

        public IEnumerable<statutoryformsrecordsmodel> GetstatutoryfromDetails()
        {
            return this.mySqlDBContext.statutoryformsrecordsmodels.Where(x => x.status == "Active").ToList();
        }


        [Route("api/statutoryfrom/Getstatutoryfrom")]
        [HttpGet]

        public IEnumerable<object> Getstatutoryfrom()
        {
            //return this.mySqlDBContext.Actregulatorymodels.Where(x => x.status == "Active").ToList();

            var act = from statutoryform in mySqlDBContext.statutoryformsrecordsmodels
                      join actregulatory in mySqlDBContext.Actregulatorymodels on statutoryform.actregulatoryid equals actregulatory.actregulatoryid
                      join actrulerepo in mySqlDBContext.Rulesandregulatorymodels on statutoryform.act_rule_regulatory_id equals actrulerepo.act_rule_regulatory_id
                      where statutoryform.status == "Active"
                      select new
                      {
                          statutoryform.statutoryformsid,
                          statutoryform.recordformsname,
                          actregulatory.actregulatoryname,
                          actrulerepo.act_rule_name,
                        name = $"{actregulatory.actregulatoryname}-{actrulerepo.act_rule_name}-{statutoryform.recordformsname}"

                      };
            var result = act.ToList();


            return result;
        }

        [Route("api/statutoryfrom/removestatutoryfrom/{statutory_forms_filemaster_id}")]
        [HttpPost]
        public void removestatutoryfrom(int statutory_forms_filemaster_id)
        {
            try
            {
                var currentClass = new statutoryformsrecordsfilemodel { statutory_forms_filemaster_id = statutory_forms_filemaster_id };
                currentClass.status = "Inactive";
                currentClass.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
                this.mySqlDBContext.SaveChanges();
            }
            catch {
                return;
            }   
        }

        [Route("api/statutoryfrom/GetformrecordByID/{ruleid}")]
        [HttpGet]

        public IEnumerable<object> GetformrecordByID(int ruleid)
        {
            try
            {
                var statutoryIds = mySqlDBContext.statutoryformsrecordsmodels
                    .Where(statutory => statutory.status == "Active" && statutory.act_rule_regulatory_id == ruleid)
                    .Select(statutory => statutory.statutoryformsid)
                    .ToList();

                var combinedResults = new List<object>();

                foreach (var statutoryId in statutoryIds)
                {
                    var statutoryDetails = mySqlDBContext.statutoryformsrecordsmodels
                        .Where(statutory => statutory.statutoryformsid == statutoryId)
                        .Select(statutory => new
                        {
                            statutoryid = statutory.statutoryformsid,
                            statutoryName = statutory.recordformsname,
                            applicable_section = statutory.applicationrefernce,
                            description = statutory.recordformsdesc,
                        })
                        .FirstOrDefault(); // Retrieve statutory details for the current statutoryId

                    if (statutoryDetails != null)
                    {
                        var filesForStatutoryId = mySqlDBContext.statutoryformsrecordsfilemodels
                            .Where(files => files.statutoryformsid == statutoryId)
                            .ToList();

                        var links = filesForStatutoryId
                            .Where(file => file.filecategory == "Weblink")
                            .Select(file => new
                            {
                                category = file.filecategory,
                                filepath = file.filepath
                            })
                            .ToList();

                        var files = filesForStatutoryId
                            .Where(file => file.filecategory != "Weblink")
                            .Select(file => new
                            {
                                category = file.filecategory,
                                filepath = file.filepath
                            })
                            .ToList();

                        var combinedResult = new
                        {
                            statutoryid = statutoryDetails.statutoryid,
                            statutoryName = statutoryDetails.statutoryName,
                            applicable_section = statutoryDetails.applicable_section,
                            description = statutoryDetails.description,
                            categoryDetails = links.Concat(files).ToList()
                        };

                        combinedResults.Add(combinedResult);
                    }
                }

                return combinedResults;
            }
            catch (Exception ex)
            {
                // Log or handle the exception here
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; // Or any other appropriate action
            }
        }



        [Route("api/statutoryfrom/Insertstatutoryfromdetails")]
        [HttpPost]
        public async Task<IActionResult> Insertstatutoryfromdetails()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            try
            {
                var maxstatutoryid = this.mySqlDBContext.statutoryformsrecordsmodels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.statutoryformsid) ?? 0;

                var newstatutoryid = maxstatutoryid + 1; 
                var form = HttpContext.Request.Form;
                var actregulatoryid = form["actregulatoryid"].FirstOrDefault();
                var act_rule_regulatory_id = form["act_rule_regulatory_id"].FirstOrDefault();
                var recordformsname = form["recordformsname"].FirstOrDefault(); // Get the first value
                var recordformsdesc = form["recordformsdesc"].FirstOrDefault(); // Get the first value
                var applicationrefernce = form["applicationrefernce"].FirstOrDefault();
                var createdby = form["userId"].FirstOrDefault();
                var files = form.Files; // This should contain the files
                var weblinks = form["weblink"].ToString();

                // Get the current HTTP request
                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                var statutoryFloder = Path.Combine("Reports", "statutoryid");

                DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(statutoryFloder);


                // Insert data into act-regulatorymaster table using insertMasterQuery
                var insertMasterQuery = "INSERT INTO statutoryforms_recordsmaster (statutoryformsid,actregulatoryid, act_rule_regulatory_id,recordformsname,recordformsdesc,applicationrefernce,createddate,status,createdby,IsImportedData) " +
                                        "VALUES (@statutoryformsid,@actregulatoryid, @act_rule_regulatory_id,@recordformsname,@recordformsdesc,@applicationrefernce, @createddate,@status,@createdby,@IsImportedData); " +
                                        "SELECT LAST_INSERT_ID();";

                MySqlCommand masterCommand = new MySqlCommand(insertMasterQuery, con);
                masterCommand.Parameters.AddWithValue("@statutoryformsid", newstatutoryid);
                masterCommand.Parameters.AddWithValue("@actregulatoryid", actregulatoryid);
                masterCommand.Parameters.AddWithValue("@act_rule_regulatory_id", act_rule_regulatory_id);
                masterCommand.Parameters.AddWithValue("@recordformsname", recordformsname);
                masterCommand.Parameters.AddWithValue("@recordformsdesc", recordformsdesc);
                masterCommand.Parameters.AddWithValue("@applicationrefernce", applicationrefernce);
                masterCommand.Parameters.AddWithValue("@createddate", DateTime.Now);
                masterCommand.Parameters.AddWithValue("@createdby", createdby);
                 masterCommand.Parameters.AddWithValue("@status", "Active");
                masterCommand.Parameters.AddWithValue("@IsImportedData", "No");
                int insertedstatutoryformsid = Convert.ToInt32(masterCommand.ExecuteScalar());


                List<string> fileList = new List<string>();

                foreach (var file in files)
                {
                    // Save the file to the directory
                    var filePath = Path.Combine(statutoryFloder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add the file path to the list
                    fileList.Add(filePath);
                    // Insert file attachment for each record without passing globalactId
                    InsertFile("FileAttach", $"{baseUrl}/Reports/statutoryid/{file.FileName}", file, newstatutoryid, null, file.FileName);
                }

                if (!string.IsNullOrEmpty(weblinks))
                {

                    string[] webLinksArray = weblinks.Split(';');


                    foreach (var weblink in webLinksArray)
                    {
                        // Insert each web link as a separate record without passing globalactId
                        InsertFile("Weblink", weblink.Trim(), null, newstatutoryid, null, null);
                    }
                }


                // Function to insert file or web link
                void InsertFile(string filecategory, string filepath, IFormFile file, int act_rule_regulatory_id, string globalactId, string fileName)
                {
                    var fileUploadModel = new statutoryformsrecordsfilemodel
                    {
                        recordformsname = filecategory == "FileAttach" ? file?.Name : null, // Check if file is null before accessing its properties
                        filepath = filepath,
                        filecategory = filecategory,
                        status = "Active",
                        created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        statutoryformsid = newstatutoryid,
                        
                    };
                    var maxststutoryfileid = this.mySqlDBContext.statutoryformsrecordsfilemodels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.statutory_forms_filemaster_id) ?? 0;
                    var newstatutoryfileid = maxststutoryfileid + 1;
                    string insertSubTableQuery = "INSERT INTO statutory_forms_filemaster (statutory_forms_filemaster_id,statutoryformsid, filecategory, filepath, status, created_date,file_name,IsImportedData) " +
                                                "VALUES (@statutory_forms_filemaster_id,@statutoryformsid, @fileCategory, @filePath, @status, @createddate,@file_name,@IsImportedData)";

                    MySqlCommand subTableCommand = new MySqlCommand(insertSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@statutory_forms_filemaster_id", newstatutoryfileid);
                    subTableCommand.Parameters.AddWithValue("@statutoryformsid", newstatutoryid);
                    subTableCommand.Parameters.AddWithValue("@fileCategory", fileUploadModel.filecategory);
                    subTableCommand.Parameters.AddWithValue("@filePath", fileUploadModel.filepath);
                    subTableCommand.Parameters.AddWithValue("@status", fileUploadModel.status);
                    subTableCommand.Parameters.AddWithValue("@createddate", fileUploadModel.created_date);
                    subTableCommand.Parameters.AddWithValue("@file_name", fileName);
                    subTableCommand.Parameters.AddWithValue("@IsImportedData", "No");
                    subTableCommand.ExecuteNonQuery();

                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex}");
            }
        }

     






        [Route("api/statutory/GetstatutoryDetailsnyid/{statutoryid}")]
        [HttpGet]

        public IEnumerable<object> GetstatutoryDetailsnyid(int statutoryid)
        {

            var result = (from statutoryfrom in mySqlDBContext.statutoryformsrecordsmodels
                          join actregulatory in mySqlDBContext.Actregulatorymodels on statutoryfrom.actregulatoryid equals actregulatory.actregulatoryid
                          join actrulerepos in mySqlDBContext.Rulesandregulatorymodels on statutoryfrom.act_rule_regulatory_id equals actrulerepos.act_rule_regulatory_id
                           join tbluser in mySqlDBContext.usermodels on statutoryfrom.createdby equals tbluser.USR_ID
                          join user in mySqlDBContext.usermodels on statutoryfrom.updatedby equals user.USR_ID into userJoin
                          from user in userJoin.DefaultIfEmpty()
                          where statutoryfrom.statutoryformsid == statutoryid && statutoryfrom.status == "Active"
                          select new
                          {
                              statutoryid = statutoryfrom.statutoryformsid,
                             statutoryname = statutoryfrom.recordformsname,
                             statutoryDesc = statutoryfrom.recordformsdesc,
                             applicationrefer = statutoryfrom.applicationrefernce,
                              actregulatoryid = actregulatory.actregulatoryid,
                              actid =statutoryfrom.actregulatoryid,
                              ruleid = statutoryfrom.act_rule_regulatory_id,
                              actregulatoryname = actregulatory.actregulatoryname,
                              rulename1 =actrulerepos.act_rule_name,
                              actname = $"{actregulatory.global_actId}-{actregulatory.actregulatoryname}",
                              rulename = $"{actrulerepos.global_rule_id}-{actrulerepos.act_rule_name}",
                              actrequlatorydescription = actregulatory.actrequlatorydescription,
                              create = $"{tbluser.firstname}-{statutoryfrom.createddate}",
                              isImportedData = statutoryfrom.IsImportedData,
                              update = $"{user.firstname}-{statutoryfrom.updatedDate}"
                          })
                         .Distinct()
                         .ToList();
            //var statutoryFiles = mySqlDBContext.statutoryformsrecordsfilemodels
            //                           .Where(sf => result.Select(r => r.statutoryid).Contains(sf.statutoryformsid))
            //                           .Select(actregulatoryidsf => new { sf.statutoryformsid, sf.filecategory, sf.filepath })
            //                           .ToList();


            var statutoryFiles = mySqlDBContext.statutoryformsrecordsfilemodels
                .Where(sf => result.Select(r => r.statutoryid).Contains(sf.statutoryformsid) && sf.status=="Active")
                .Select(sf => new
                {
                    sf.statutoryformsid,
                    sf.statutory_forms_filemaster_id,
                    sf.filepath,
                    sf.status,
                    filecategory = sf.filecategory
                })
                .ToList();

            var modifiedResult = result.Select(item => new
            {
                item.statutoryid,
                item.statutoryname,
                item.actid,
                item.ruleid,
                item.actname,
                item.rulename,
                item.actregulatoryname,
                item.rulename1,
                item.statutoryDesc,
                item.create,
                item.actrequlatorydescription,
                item.update,
                item.isImportedData,
                item.applicationrefer,

                //statutoryFiles = statutoryFiles.Where(sf => sf.statutoryformsid == item.actregulatoryid)
                //                   .Select(sf => new { sf.filecategory, sf.filepath })
                //                   .ToList()

                statutoryFiles = statutoryFiles.Where(sf => sf.statutoryformsid == item.statutoryid && sf.status == "Active")
                                   .Select(sf => new
                                   {
                                       sf.statutory_forms_filemaster_id,
                                       sf.filecategory,
                                       sf.filepath,
                                       sf.status,
                                       filename = sf.filepath != null ?
                                                  sf.filepath.Substring(sf.filepath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList()
            })
            .ToList();

            return modifiedResult;

        }

        [Route("api/statutorydownload/statutorydownloadFiles")]
        [HttpGet]
        public async Task<IActionResult> statutorydownloadFiles(string filePath)
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


        [Route("Api/statutory/Updatestatutory")]
        [HttpPost]

        public async Task<IActionResult> Updatestatutory([FromQuery] int statutoryformsid)
        {
            try
            {
                var maxststutoryfileid = this.mySqlDBContext.statutoryformsrecordsfilemodels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.statutory_forms_filemaster_id) ?? 0;
                var newstatutoryfileid = maxststutoryfileid + 1;
                var statutory = await mySqlDBContext.statutoryformsrecordsmodels.FirstOrDefaultAsync(a => a.statutoryformsid == statutoryformsid);

                if (statutory == null)
                {
                    return NotFound();
                }

                var formCollection = await Request.ReadFormAsync();

                // Update actregulatory details
                statutory.actregulatoryid = int.Parse(formCollection["actregulatoryid"]);
                statutory.act_rule_regulatory_id = int.Parse(formCollection["act_rule_regulatory_id"]);
                    statutory.recordformsname = formCollection["recordformsname"];
                statutory.recordformsdesc = formCollection["recordformsdesc"];
                statutory.applicationrefernce = formCollection["applicationrefernce"];
                statutory.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                statutory.updatedby = int.Parse(formCollection["updatedby"]);





                // Handle Weblinks
                var existingWeblinks = await mySqlDBContext.statutoryformsrecordsfilemodels
                    .Where(f => f.statutoryformsid == statutoryformsid && f.filecategory == "Weblink" && f.status == "Active")
                    .ToListAsync();


                var newWeblinks = formCollection["Weblink"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);

                var newWeblinkSet = new HashSet<string>(newWeblinks);

                // Remove existing weblinks that are not in the new weblinks
                foreach (var existingLink in existingWeblinks)
                {
                    if (!newWeblinkSet.Contains(existingLink.file_name))
                    {
                        existingLink.status = "Inactive";
                        existingLink.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                        existingLink.updatedby = int.Parse(formCollection["updatedby"]);
                        // mySqlDBContext.Actregulatoryfilemodels.Remove(existingLink);
                    }
                }
                // Add new weblinks that are not in the existing weblinks
                foreach (var link in newWeblinks)
                {
                    if (!existingWeblinks.Any(f => f.file_name == link))
                    {
                        mySqlDBContext.statutoryformsrecordsfilemodels.Add(new statutoryformsrecordsfilemodel
                        {
                            statutory_forms_filemaster_id = newstatutoryfileid++,
                            statutoryformsid = statutoryformsid,
                            file_name = link,
                            filecategory = "Weblink",
                            filepath = link,
                            created_date = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedby = int.Parse(formCollection["updatedby"]),
                            status = "Active",
                            IsImportedData = "No"
                        });
                    }
                }
                // Handle File Attachments
                var existingFiles = await mySqlDBContext.statutoryformsrecordsfilemodels
                    .Where(f => f.statutoryformsid == statutoryformsid && f.filecategory == "FileAttach" && f.status == "Active")
                    .ToListAsync();

                var files = formCollection.Files;
  

                // Ensure the directory exists
                var globalactIdFolder = Path.Combine("Reports", "statutoryid");
                Directory.CreateDirectory(globalactIdFolder);
                // Remove existing file records that are not in the new files
                //foreach (var existingFile in existingFiles)
                //{
                //    if (!files.Any(f => f.FileName == existingFile.file_name))
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
                    var fileUrl = new Uri(new Uri(baseUrl), $"Reports/statutoryid/{fileName}").ToString();

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
                        mySqlDBContext.statutoryformsrecordsfilemodels.Add(new statutoryformsrecordsfilemodel
                        {
                            statutory_forms_filemaster_id = newstatutoryfileid++,
                            statutoryformsid = statutoryformsid,
                            file_name = fileName,
                            filecategory = "FileAttach",
                            filepath = fileUrl,
                            updatedDate = DateTime.Now.ToString("yyyy-MM-dd"),
                            created_date = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedby = int.Parse(formCollection["updatedby"]),
                            status = "Active",
                            IsImportedData = "No"
                        });
                    }
                }

                await mySqlDBContext.SaveChangesAsync(); // Save changes for new files and weblinks

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while updating the Act Regulatory record.");
            }
        }

    }
}
