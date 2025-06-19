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
    public class SupAdmin_CategoryPenaltyController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        private MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_CategoryPenaltyController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/SupAdmin_penaltycategory/GetpenaltyDetails")]
        [HttpGet]

        public IEnumerable<object> GetpenaltyDetails()
        {
            //  return this.commonDBContext.PenaltyCategoryModels.Where(x => x.penalty_category_status == "Active").ToList();

            var result = (from penalty in commonDBContext.SupAdmin_compliancepenatlymasterModels
                          join actregulatory in commonDBContext.SupAdmin_ActRegulatoryModels on penalty.actid equals actregulatory.actregulatoryid
                          join actrulerepos in commonDBContext.SupAdmin_RulesandRegulatoryModels on penalty.ruleid equals actrulerepos.act_rule_regulatory_id
                          join penaltycat in commonDBContext.SupAdmin_PenaltyCategoryModels on penalty.penalty equals penaltycat.penalty_category_id
                          where penalty.status == "Active"
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
        [Route("api/SupAdmin_penaltycategory/GetpenaltycomplianceByID/{ruleid}")]
        [HttpGet]

        public IEnumerable<object> GetpenaltycomplianceByID(int ruleid)
        {
            try
            {
                var penaltyIds = commonDBContext.SupAdmin_compliancepenatlymasterModels
        .Where(penalty => penalty.status == "Active" && penalty.ruleid == ruleid)
        .Select(penalty => penalty.compliancepenaltyid)
        .ToList();

                var combinedResults = new List<object>();

                foreach (var penaltyId in penaltyIds)
                {
                    var penaltyDetails = commonDBContext.SupAdmin_compliancepenatlymasterModels
                        .Where(penalty => penalty.compliancepenaltyid == penaltyId)
                        .Join(commonDBContext.SupAdmin_PenaltyCategoryModels,
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
                        var filesForcompliancepenaltyId = commonDBContext.SupAdmin_PenaltyCategoryfileModels
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
                            categoryDetails = links.Concat(files).ToList()
                            //links,
                            // files
                        };

                        combinedResults.Add(combinedResult);
                    }
                }

                return combinedResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
                return null;

            }

        }

        [Route("api/SupAdmin_penaltycategory/InsertCompliancepenaltycategorydetails")]
        [HttpPost]
        public async Task<IActionResult> Insertpenaltycategorydetails()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:commondb"]);
            con.Open();

            try
            {
                var maxpenaltycompliances = this.commonDBContext.SupAdmin_compliancepenatlymasterModels.Max(d => (int?)d.compliancepenaltyid) ?? 50000;

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
                var insertMasterQuery = "INSERT INTO compliancepenatlymaster ( compliancepenaltyid ,ruleid, actid,applicationselectionrule,penaltydesc,maxpenalty,minpenalty,additionalrefernce,penalty,createddate,status,createdBy) " +
                                        "VALUES (@compliancepenaltyid,@ruleid, @actid,@applicationselectionrule,@penaltydesc,@maxpenalty,@minpenalty,@additionalrefernce,@penalty, @createddate,@status,@createdBy); " +
                                        "SELECT LAST_INSERT_ID();";

                MySqlCommand masterCommand = new MySqlCommand(insertMasterQuery, con);
                masterCommand.Parameters.AddWithValue("@compliancepenaltyid",newpenaltycomplianceid);
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

                    var maxpenaltyfile = this.commonDBContext.SupAdmin_PenaltyCategoryfileModels.Max(d => (int?)d.compliance_filepenalty_id) ?? 50000;
                    var newpenaltyfileid = maxpenaltyfile + 1;

                    string insertSubTableQuery = "INSERT INTO compliance_filepenalty (compliance_filepenalty_id,compliancepenaltyid, filecategory, filepath, status, created_date,file_name) " +
                                                "VALUES (@compliance_filepenalty_id,@compliancepenaltyid, @fileCategory, @filePath, @status, @createddate,@file_name)";

                    MySqlCommand subTableCommand = new MySqlCommand(insertSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@compliance_filepenalty_id", newpenaltyfileid);
                    subTableCommand.Parameters.AddWithValue("@compliancepenaltyid", newpenaltycomplianceid);
                    subTableCommand.Parameters.AddWithValue("@fileCategory", fileUploadModel.filecategory);
                    subTableCommand.Parameters.AddWithValue("@filePath", fileUploadModel.filepath);
                    subTableCommand.Parameters.AddWithValue("@status", fileUploadModel.status);
                    subTableCommand.Parameters.AddWithValue("@createddate", fileUploadModel.created_date);
                    subTableCommand.Parameters.AddWithValue("@file_name", fileName);
                    subTableCommand.ExecuteNonQuery();
                }





                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex}");
            }
        }



        [Route("api/SupAdmin_penaltycategory/removesuppenaltycategory/{compliance_filepenalty_id}")]
        [HttpPost]
        public void removesuppenaltycategory(int compliance_filepenalty_id)
        {
            try
            {
                var currentClass = new SupAdmin_PenaltyCategoryfileModel { compliance_filepenalty_id = compliance_filepenalty_id };
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

        [Route("api/SupAdmin_penaltycategory/GetpenaltysupcomplianceByID/{ruleid}")]
        [HttpGet]

        public IEnumerable<object> GetpenaltysupcomplianceByID(int ruleid)
        {
            try
            {
                var penaltyIds = commonDBContext.SupAdmin_compliancepenatlymasterModels
        .Where(penalty => penalty.status == "Active" && penalty.ruleid == ruleid)
        .Select(penalty => penalty.compliancepenaltyid)
        .ToList();

                var combinedResults = new List<object>();

                foreach (var penaltyId in penaltyIds)
                {
                    var penaltyDetails = commonDBContext.SupAdmin_compliancepenatlymasterModels
                        .Where(penalty => penalty.compliancepenaltyid == penaltyId)
                        .Join(commonDBContext.SupAdmin_PenaltyCategoryModels,
                              penalty => penalty.penalty,
                              penaltyCategory => penaltyCategory.penalty_category_id,
                              (penalty, penaltyCategory) => new
                              {
                                  penalty.compliancepenaltyid,
                                  penalty.applicationselectionrule,
                                  penaltyCategory.penalty_category_name,
                                  penalty.penaltydesc,
                                  penalty.minpenalty,
                                  penalty.maxpenalty,
                                  penalty.additionalrefernce
                              })
                        .FirstOrDefault();

                    if (penaltyDetails != null)
                    {
                        var filesForcompliancepenaltyId = commonDBContext.SupAdmin_PenaltyCategoryfileModels
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
                            applicable_section = penaltyDetails.applicationselectionrule,
                            description = penaltyDetails.penaltydesc,
                            minimum = penaltyDetails.minpenalty,
                            maximum = penaltyDetails.maxpenalty,
                            additionalreference = penaltyDetails.additionalrefernce,
                            compliancename = $"{penaltyDetails.penalty_category_name}    /    {penaltyDetails.applicationselectionrule}",
                            categoryDetails = links.Concat(files).ToList()
                            //links,
                            // files
                        };

                        combinedResults.Add(combinedResult);
                    }
                }

                return combinedResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured: {ex.Message}");
                return null;

            }

        }

        [Route("api/SupAdmin_penaltycategory/GetpenaltysupcomplianceDetailsnyid/{compliancespenatlyid}")]
        [HttpGet]

        public IEnumerable<object> GetpenaltysupcomplianceDetailsnyid(int compliancespenatlyid)
        {

            var result = (from penalty in commonDBContext.SupAdmin_compliancepenatlymasterModels
                          join actregulatory in commonDBContext.SupAdmin_ActRegulatoryModels on penalty.actid equals actregulatory.actregulatoryid
                          join actrulerepos in commonDBContext.SupAdmin_RulesandRegulatoryModels on penalty.ruleid equals actrulerepos.act_rule_regulatory_id
                          join penaltycat in commonDBContext.SupAdmin_PenaltyCategoryModels on penalty.penalty equals penaltycat.penalty_category_id
                         // join tbluser in mySqlDBContext.usermodels on penalty.createdBy equals tbluser.USR_ID
                          //join user in mySqlDBContext.usermodels on penalty.updatedby equals user.USR_ID into userJoin
                          //from user in userJoin.DefaultIfEmpty()
                          where penalty.compliancepenaltyid == compliancespenatlyid && penalty.status == "Active"
                          select new
                          {
                              penaltyid = penalty.compliancepenaltyid,
                              actid = penalty.actid,
                              ruleid = penalty.ruleid,
                              maxpen = penalty.maxpenalty,
                              minpen = penalty.minpenalty,
                              addref = penalty.additionalrefernce,
                       
                              penaltysectrule = penalty.applicationselectionrule,
                              desc = penalty.penaltydesc,
                              actname = actregulatory.actregulatoryname,
                              rulename = actrulerepos.act_rule_name,
                              penalty = penalty.penalty,
                              pencatname = penaltycat.penalty_category_name,
                              name = $"{actregulatory.actregulatoryname}-{actrulerepos.act_rule_name}-{penaltycat.penalty_category_name}",
                            //  create = $"{tbluser.firstname}-{penalty.createddate}",
                              //update = $"{user.firstname}-{penalty.updateddate}"
                          })

                        .Distinct()
                          .ToList();

            //var penaltyFiles = mySqlDBContext.PenaltyCategoryfileModels
            //                           .Where(pf => result.Select(r => r.penaltyid).Contains(pf.compliancepenaltyid))
            //                           .Select(pf => new { pf.compliancepenaltyid, pf.filecategory, pf.filepath })
            //                           .ToList();

            var penaltyFiles = commonDBContext.SupAdmin_PenaltyCategoryfileModels
                    .Where(pf => result.Select(r => r.penaltyid).Contains(pf.compliancepenaltyid) && pf.status == "Active")
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
                //item.create,
                //item.update,
                
                item.actid,
                item.ruleid,
                item.penalty,

                //penaltyFiles = penaltyFiles.Where(pf => pf.compliancepenaltyid == item.penaltyid)
                //                   .Select(pf => new { pf.filecategory, pf.filepath })
                //                   .ToList()

                penaltyFiles = penaltyFiles.Where(pf => pf.compliancepenaltyid == item.penaltyid && pf.status == "Active")
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


        [Route("api/SupAdmin_penaltycategory/compliancesuppenaltydownloadFiles")]
        [HttpGet]
        public async Task<IActionResult> compliancesuppenaltydownloadFiles(string filePath)
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

        [Route("Api/SupAdmin_penaltycategory/Updatesuppenaltycompliance")]
        [HttpPost]

        public async Task<IActionResult> Updatesuppenaltycompliance([FromQuery] int compliancespenatlyid)
        {
            try
            {
                var maxpenaltyfile = this.commonDBContext.SupAdmin_PenaltyCategoryfileModels.Max(d => (int?)d.compliance_filepenalty_id) ?? 50000;
                var newpenaltyfileid = maxpenaltyfile + 1;
                var compliancepenalty = await commonDBContext.SupAdmin_compliancepenatlymasterModels.FirstOrDefaultAsync(a => a.compliancepenaltyid == compliancespenatlyid);

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
                compliancepenalty.updateddate = DateTime.Now.ToString("yyyy-MM-dd");
                compliancepenalty.updatedby = int.Parse(formCollection["updatedby"]);





                // Handle Weblinks
                var existingWeblinks = await commonDBContext.SupAdmin_PenaltyCategoryfileModels
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
                        commonDBContext.SupAdmin_PenaltyCategoryfileModels.Add(new SupAdmin_PenaltyCategoryfileModel
                        {
                            compliance_filepenalty_id = newpenaltyfileid++,
                            compliancepenaltyid = compliancespenatlyid,
                            file_name = link,
                            filecategory = "Weblink",
                            filepath = link,
                            created_date = DateTime.Now.ToString("yyyy-MM-dd"),
                            updateddate = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedby = int.Parse(formCollection["updatedby"]),
                            status = "Active",
                      
                        });
                    }
                }
                // Handle File Attachments
                var existingFiles = await commonDBContext.SupAdmin_PenaltyCategoryfileModels
                    .Where(f => f.compliancepenaltyid == compliancespenatlyid && f.filecategory == "FileAttach" && f.status == "Active")
                    .ToListAsync();

                var files = formCollection.Files;


                // Ensure the directory exists
                var globalactIdFolder = Path.Combine("Reports", "compliancepenalty");
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
                        commonDBContext.SupAdmin_PenaltyCategoryfileModels.Add(new SupAdmin_PenaltyCategoryfileModel
                        {
                            compliance_filepenalty_id = newpenaltyfileid++,
                            compliancepenaltyid = compliancespenatlyid,
                            file_name = fileName,
                            filecategory = "FileAttach",
                            filepath = fileUrl,
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


    }
}
