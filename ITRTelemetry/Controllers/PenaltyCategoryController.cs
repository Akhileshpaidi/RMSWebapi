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
using System.Globalization;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class PenaltyCategoryController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PenaltyCategoryController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("api/penaltycategory/GetpenaltycategoryDetails")]
        [HttpGet]

        public IEnumerable<PenaltyCategoryModel> GetComplianceNotifiedStatus()
        {
            return this.mySqlDBContext.PenaltyCategoryModels.Where(x => x.penalty_category_status == "Active").ToList();
        }


        [Route("api/penaltycategory/InsertpenaltycategoryDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] PenaltyCategoryModel PenaltyCategoryModels)
        {
            try
            {
                PenaltyCategoryModels.penalty_category_name = PenaltyCategoryModels.penalty_category_name.Trim();

                var existingPenaltyCategory = this.mySqlDBContext.PenaltyCategoryModels
                    .FirstOrDefault(d => d.penalty_category_name == PenaltyCategoryModels.penalty_category_name && d.penalty_category_status == "Active");

                if (existingPenaltyCategory != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }

                var maxpenaltyId = this.mySqlDBContext.PenaltyCategoryModels
          .Where(d => d.source == "No")
          .Max(d => (int?)d.penalty_category_id) ?? 0;

                PenaltyCategoryModels.penalty_category_id = maxpenaltyId + 1;

                var TypeModel = this.mySqlDBContext.PenaltyCategoryModels;

              

                TypeModel.Add(PenaltyCategoryModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                PenaltyCategoryModels.penalty_category_date = dt1;
                PenaltyCategoryModels.penalty_category_status = "Active";
                PenaltyCategoryModels.source = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/penaltycategory/GetpenaltyDetails")]
        [HttpGet]

        public IEnumerable<object> GetpenaltyDetails()
        {
          //  return this.mySqlDBContext.PenaltyCategoryModels.Where(x => x.penalty_category_status == "Active").ToList();

            var result =( from penalty in mySqlDBContext.CompliancepenatlymasterModels
                          join actregulatory in mySqlDBContext.Actregulatorymodels on penalty.actid equals actregulatory.actregulatoryid
                          join actrulerepos in mySqlDBContext.Rulesandregulatorymodels on penalty.ruleid equals actrulerepos.act_rule_regulatory_id
                          join penaltycat in mySqlDBContext.PenaltyCategoryModels on penalty.penalty equals penaltycat.penalty_category_id
                          where penalty.status =="Active"
                          select new
                          {
                            penalty.compliancepenaltyid,
                            actregulatory.actregulatoryname,
                            actrulerepos.act_rule_name,
                            penaltycat.penalty_category_name,
                            penalty.applicationselectionrule,
                            name = $"{actregulatory.actregulatoryname}-{actrulerepos.act_rule_name}-{penaltycat.penalty_category_name}-{penalty.applicationselectionrule}",
                          })
                          .ToList();
            return result;
        }

        [Route("api/penaltycategory/removepenaltycategory/{compliance_filepenalty_id}")]
        [HttpPost]
        public void removepenaltycategory(int compliance_filepenalty_id)
        {
            try
            {
                var currentClass = new PenaltyCategoryfileModel { compliance_filepenalty_id = compliance_filepenalty_id };
                currentClass.status = "Inactive";
                currentClass.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
                this.mySqlDBContext.SaveChanges();
            }
            catch {
                return;
            }
        }

        [Route("api/penaltycategory/GetpenaltycomplianceByID/{ruleid}")]
        [HttpGet]

        public IEnumerable<object> GetpenaltycomplianceByID(int ruleid)
        {
            try
            {
                var penaltyIds = mySqlDBContext.CompliancepenatlymasterModels
        .Where(penalty => penalty.status == "Active" && penalty.ruleid == ruleid)
        .Select(penalty => penalty.compliancepenaltyid)
        .ToList();

                var combinedResults = new List<object>();

                foreach (var penaltyId in penaltyIds)
                {
                    var penaltyDetails = mySqlDBContext.CompliancepenatlymasterModels
                        .Where(penalty => penalty.compliancepenaltyid == penaltyId)
                        .Join(mySqlDBContext.PenaltyCategoryModels,
                              penalty => penalty.penalty,
                              penaltyCategory => penaltyCategory.penalty_category_id,
                              (penalty, penaltyCategory) => new
                              {
                                  penalty.compliancepenaltyid,
                                  penaltyCategory.penalty_category_name,
                                  penalty.applicationselectionrule,
                                  penalty.penaltydesc,
                                  penalty.minpenalty,
                                  penalty.maxpenalty,
                                  penalty.additionalrefernce
                              })
                        .FirstOrDefault();

                    if (penaltyDetails != null)
                    {
                        var filesForcompliancepenaltyId = mySqlDBContext.PenaltyCategoryfileModels
                            .Where(files => files.compliancepenaltyid == penaltyId)
                            .ToList();

                        var links = filesForcompliancepenaltyId
                            .Where(file => file.filecategory == "Weblink")
                            .Select(file => new
                            {
                                category = file.filecategory,
                                filepath = file.filepath
                            })
                            .ToList();

                        var files = filesForcompliancepenaltyId
                            .Where(file => file.filecategory != "Weblink")
                            .Select(file => new
                            {
                                category = file.filecategory,
                                filepath = file.filepath
                            })
                            .ToList();

                        var combinedResult = new
                        {
                            compliancepenaltyid = penaltyDetails.compliancepenaltyid,
                            compliancepenaltyName = penaltyDetails.penalty_category_name,
                            applicable_section = penaltyDetails.applicationselectionrule,
                            description = penaltyDetails.penaltydesc,
                            minimum = penaltyDetails.minpenalty,
                            maximum = penaltyDetails.maxpenalty,
                            additionalreference = penaltyDetails.additionalrefernce,
                            compliancename =$"{penaltyDetails.penalty_category_name}    /    {penaltyDetails.applicationselectionrule}",
                            categoryDetails = links.Concat(files).ToList()
                            //links,
                            // files
                        };

                        combinedResults.Add(combinedResult);
                    }
                }

                return combinedResults;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
                return null;

            }

        }





        [Route("api/penaltycategory/InsertCompliancepenaltycategorydetails")]
        [HttpPost]
        public async Task<IActionResult> Insertpenaltycategorydetails()
        { 
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            try
            {
                var maxpenaltycompliances = this.mySqlDBContext.CompliancepenatlymasterModels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.compliancepenaltyid) ?? 0;

                var newpenaltycomplianceid = maxpenaltycompliances + 1;
                var form = HttpContext.Request.Form;
                var ruleid = form["ruleid"].FirstOrDefault();
                var actid = form["actid"].FirstOrDefault();
                var penality = form["penalty"].FirstOrDefault();
                var applicationselectionrule = form["applicationselectionrule"].FirstOrDefault(); // Get the first value
                var penaltydesc = form["penaltydesc"].FirstOrDefault(); // Get the first value
                var maxpenalty = form["maxpenalty"].FirstOrDefault();
                var minpenalty = form["minpenalty"].FirstOrDefault(); // Get the first value
                var additionalrefernce = form["additionalrefernce"].FirstOrDefault();
                var createdBy = form["userId"].FirstOrDefault();
                var files = form.Files; // This should contain the files
                var weblinks = form["weblink"].ToString();


                // Get the current HTTP request
                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                var complianxcepenaltyFloder = Path.Combine("Reports", "compliancepenalty");

                DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(complianxcepenaltyFloder);


                // Insert data into act-regulatorymaster table using insertMasterQuery
                var insertMasterQuery = "INSERT INTO compliancepenatlymaster (compliancepenaltyid,ruleid, actid,applicationselectionrule,penaltydesc,maxpenalty,minpenalty,additionalrefernce,penalty,createddate,status,createdBy,IsImportedData) " +
                                        "VALUES (@compliancepenaltyid,@ruleid, @actid,@applicationselectionrule,@penaltydesc,@maxpenalty,@minpenalty,@additionalrefernce,@penalty, @createddate,@status,@createdBy,@IsImportedData); " +
                                        "SELECT LAST_INSERT_ID();";

                MySqlCommand masterCommand = new MySqlCommand(insertMasterQuery, con);
                masterCommand.Parameters.AddWithValue("@compliancepenaltyid", newpenaltycomplianceid);
                masterCommand.Parameters.AddWithValue("@ruleid", ruleid);
                masterCommand.Parameters.AddWithValue("@actid", actid);
                masterCommand.Parameters.AddWithValue("@penalty", penality);
                masterCommand.Parameters.AddWithValue("@applicationselectionrule", applicationselectionrule);
                masterCommand.Parameters.AddWithValue("@penaltydesc", penaltydesc);
                masterCommand.Parameters.AddWithValue("@maxpenalty", maxpenalty);
                masterCommand.Parameters.AddWithValue("@minpenalty", minpenalty);
                masterCommand.Parameters.AddWithValue("@additionalrefernce", additionalrefernce);
                masterCommand.Parameters.AddWithValue("@createdBy", createdBy);
                masterCommand.Parameters.AddWithValue("@createddate", DateTime.Now);
                masterCommand.Parameters.AddWithValue("@status", "Active");
                masterCommand.Parameters.AddWithValue("@IsImportedData", "No");

                int insertedcompliancepenaltyid = Convert.ToInt32(masterCommand.ExecuteScalar());

                List<string> fileList = new List<string>();

                foreach (var file in files)
                {
                    // Save the file to the directory
                    var filePath = Path.Combine(complianxcepenaltyFloder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add the file path to the list
                    fileList.Add(filePath);
                    // Insert file attachment for each record without passing globalactId
                    InsertFile("FileAttach", $"{baseUrl}/Reports/compliancepenalty/{file.FileName}", file, newpenaltycomplianceid, null, file.FileName);
                }
                if (!string.IsNullOrEmpty(weblinks))
                {

                    string[] webLinksArray = weblinks.Split(';');


                    foreach (var weblink in webLinksArray)
                    {
                        // Insert each web link as a separate record without passing globalactId
                        InsertFile("Weblink", weblink.Trim(), null, newpenaltycomplianceid, null, null);
                    }
                }


                // Function to insert file or web link
                void InsertFile(string filecategory, string filepath, IFormFile file, int act_rule_regulatory_id, string globalactId, string fileName)
                {
                    var fileUploadModel = new PenaltyCategoryfileModel
                    {
                        penalty_category_name = filecategory == "FileAttach" ? file?.Name : null, // Check if file is null before accessing its properties
                        filepath = filepath,
                        filecategory = filecategory,
                        status = "Active",
                        created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        compliancepenaltyid = newpenaltycomplianceid,

                    };
                    var maxpenaltyfile = this.mySqlDBContext.PenaltyCategoryfileModels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.compliance_filepenalty_id) ?? 0;
                    var newpenaltyfileid = maxpenaltyfile + 1;

                    string insertSubTableQuery = "INSERT INTO compliance_filepenalty (compliance_filepenalty_id,compliancepenaltyid, filecategory, filepath, status, created_date,file_name,IsImportedData) " +
                                                "VALUES (@compliance_filepenalty_id,@compliancepenaltyid, @fileCategory, @filePath, @status, @createddate,@file_name,@IsImportedData)";

                    MySqlCommand subTableCommand = new MySqlCommand(insertSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@compliance_filepenalty_id", newpenaltyfileid);
                    subTableCommand.Parameters.AddWithValue("@compliancepenaltyid", newpenaltycomplianceid);
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



        [Route("api/penaltycompliance/GetpenaltycomplianceDetailsnyid/{compliancespenatlyid}")]
        [HttpGet]

        public IEnumerable<object> GetpenaltycomplianceDetailsnyid(int compliancespenatlyid)
        {

            var result = (from penalty in mySqlDBContext.CompliancepenatlymasterModels
                          join actregulatory in mySqlDBContext.Actregulatorymodels on penalty.actid equals actregulatory.actregulatoryid
                          join actrulerepos in mySqlDBContext.Rulesandregulatorymodels on penalty.ruleid equals actrulerepos.act_rule_regulatory_id
                          join penaltycat in mySqlDBContext.PenaltyCategoryModels on penalty.penalty equals penaltycat.penalty_category_id
                          join tbluser in mySqlDBContext.usermodels on penalty.createdBy equals tbluser.USR_ID
                          join user in mySqlDBContext.usermodels on penalty.updatedby equals user.USR_ID into userJoin
                          from user in userJoin.DefaultIfEmpty()
                          where penalty.compliancepenaltyid == compliancespenatlyid  && penalty.status == "Active"
                          select new
                          {
                            penaltyid = penalty.compliancepenaltyid,
                            actid = penalty.actid,
                            ruleid = penalty.ruleid,    
                             maxpen = penalty.maxpenalty,
                             minpen = penalty.minpenalty,
                             addref = penalty.additionalrefernce,
                              isImportedData = penalty.IsImportedData,
                             penaltysectrule = penalty.applicationselectionrule,
                              desc = penalty.penaltydesc,
                            actname =  actregulatory.actregulatoryname,
                              rulename = actrulerepos.act_rule_name,
                              penalty = penalty.penalty,
                            pencatname =  penaltycat.penalty_category_name,
                              name = $"{actregulatory.actregulatoryname}-{actrulerepos.act_rule_name}-{penaltycat.penalty_category_name}",
                              create = $"{tbluser.firstname}-{penalty.createddate}",
                              update = $"{user.firstname}-{penalty.updatedDate}"
                          })

                        .Distinct()
                          .ToList();

            //var penaltyFiles = mySqlDBContext.PenaltyCategoryfileModels
            //                           .Where(pf => result.Select(r => r.penaltyid).Contains(pf.compliancepenaltyid))
            //                           .Select(pf => new { pf.compliancepenaltyid, pf.filecategory, pf.filepath })
            //                           .ToList();

            var penaltyFiles = mySqlDBContext.PenaltyCategoryfileModels
                    .Where(pf => result.Select(r => r.penaltyid).Contains(pf.compliancepenaltyid) && pf.status =="Active")
                   .Select(pf => new
                   {
                       pf.compliancepenaltyid,
                       pf.compliance_filepenalty_id,
                       pf.filepath,
                       pf.status,
                       filecategory = pf.filecategory
                   })
                                    .ToList();

            var modifiedResult = result.Select(item => new
            {
                item.penaltyid,
                item.maxpen,
                item.actname,
                item.rulename,
                item.minpen,
                item.addref,
                item.penaltysectrule,
                item.desc,
                item.pencatname,
                item.create,
                item.update,
                item.isImportedData,
                item.actid,
                item.ruleid,
                item.penalty,

                //penaltyFiles = penaltyFiles.Where(pf => pf.compliancepenaltyid == item.penaltyid)
                //                   .Select(pf => new { pf.filecategory, pf.filepath })
                //                   .ToList()

                penaltyFiles = penaltyFiles.Where(pf => pf.compliancepenaltyid == item.penaltyid && pf.status =="Active")
                                   .Select(pf => new
                                   {
                                       pf.compliance_filepenalty_id,
                                       pf.filecategory,
                                       pf.filepath,
                                       filename = pf.filepath != null ?
                                                  pf.filepath.Substring(pf.filepath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList()
            })
            .ToList();

            return modifiedResult;

        }


        [Route("api/compliancepenaltydownload/compliancepenaltydownloadFiles")]
        [HttpGet]
        public async Task<IActionResult> compliancepenaltydownloadFiles(string filePath)
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

        [Route("Api/penaltycompliance/Updatepenaltycompliance")]
        [HttpPost]

        public async Task<IActionResult> Updatepenaltycompliance([FromQuery] int compliancespenatlyid)
        {
            try
            {
                var maxpenaltyfile = this.mySqlDBContext.PenaltyCategoryfileModels.Where(d => d.IsImportedData == "No").Max(d => (int?)d.compliance_filepenalty_id) ?? 0;
                var newpenaltyfileid = maxpenaltyfile + 1;
                var compliancepenalty = await mySqlDBContext.CompliancepenatlymasterModels.FirstOrDefaultAsync(a => a.compliancepenaltyid == compliancespenatlyid);

                if (compliancepenalty == null)
                {
                    return NotFound();
                }

                var formCollection = await Request.ReadFormAsync();

                // Update actregulatory details
                compliancepenalty.actid = int.Parse(formCollection["actid"]);
                compliancepenalty.ruleid = int.Parse(formCollection["ruleid"]);
                compliancepenalty.penalty = int.Parse(formCollection["penalty"]);
                compliancepenalty.penaltydesc = formCollection["penaltydesc"];
                compliancepenalty.applicationselectionrule = formCollection["applicationselectionrule"];
                compliancepenalty.additionalrefernce = formCollection["additionalrefernce"];
                compliancepenalty.maxpenalty = formCollection["maxpenalty"];
                compliancepenalty.minpenalty = formCollection["minpenalty"];
                compliancepenalty.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                compliancepenalty.updatedby = int.Parse(formCollection["updatedby"]);





                // Handle Weblinks
                var existingWeblinks = await mySqlDBContext.PenaltyCategoryfileModels
                    .Where(f => f.compliancepenaltyid == compliancespenatlyid && f.filecategory == "Weblink" && f.status == "Active")
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
                        mySqlDBContext.PenaltyCategoryfileModels.Add(new PenaltyCategoryfileModel
                        {
                            compliance_filepenalty_id = newpenaltyfileid++,
                            compliancepenaltyid = compliancespenatlyid,
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
                var existingFiles = await mySqlDBContext.PenaltyCategoryfileModels
                    .Where(f => f.compliancepenaltyid == compliancespenatlyid && f.filecategory == "FileAttach" && f.status == "Active")
                    .ToListAsync();

                var files = formCollection.Files;


                // Ensure the directory exists
                var globalactIdFolder = Path.Combine("Reports", "compliancepenalty");
                Directory.CreateDirectory(globalactIdFolder);
                // Remove existing file records that are not in the new files
                foreach (var existingFile in existingFiles)
                {
                    if (!files.Any(f => f.FileName == existingFile.file_name))
                    {
                        existingFile.status = "Inactive";
                        existingFile.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                        existingFile.updatedby = int.Parse(formCollection["updatedby"]);
                        //  mySqlDBContext.ActRuleregulatoryfilemodels.Remove(existingFile);
                    }
                }

                // Add new files and update existing files
                foreach (var file in files)
                {
                    var fileName = file.FileName;
                    var filePath = Path.Combine(globalactIdFolder, fileName);
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    var fileUrl = new Uri(new Uri(baseUrl), $"Reports/compliancepenalty/{fileName}").ToString();

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
                        mySqlDBContext.PenaltyCategoryfileModels.Add(new PenaltyCategoryfileModel
                        {
                            compliance_filepenalty_id = newpenaltyfileid++,
                            compliancepenaltyid = compliancespenatlyid,
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






        [Route("api/penaltycategory/UpdatepenaltycategoryDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] PenaltyCategoryModel PenaltyCategoryModels)
        {

            try
            {
                if (PenaltyCategoryModels.penalty_category_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    PenaltyCategoryModels.penalty_category_name = PenaltyCategoryModels.penalty_category_name?.Trim();
                    var existingCompliancenotifiedstatus = this.mySqlDBContext.PenaltyCategoryModels
                       .FirstOrDefault(d => d.penalty_category_name == PenaltyCategoryModels.penalty_category_name && d.penalty_category_id != PenaltyCategoryModels.penalty_category_id && d.penalty_category_status == "Active");

                    if (existingCompliancenotifiedstatus != null)
                    {
                        return BadRequest("Error: Penalty Category with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(PenaltyCategoryModels);
                    this.mySqlDBContext.Entry(PenaltyCategoryModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(PenaltyCategoryModels);

                    Type type = typeof(PenaltyCategoryModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(PenaltyCategoryModels, null) == null || property.GetValue(PenaltyCategoryModels, null).Equals(0))
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
                    return BadRequest("Error: Penalty Category with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/penaltycategory/DeletepenaltycategoryDetails")]
        [HttpDelete]
        public void DeleteType(int id)
        {
            try
            {
                var currentClass = new PenaltyCategoryModel { penalty_category_id = id };
                currentClass.penalty_category_status = "Inactive";
                this.mySqlDBContext.Entry(currentClass).Property("penalty_category_status").IsModified = true;
                this.mySqlDBContext.SaveChanges();
            }
            catch
            {
                return;
            }
        }
    }
}

