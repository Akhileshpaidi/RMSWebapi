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
//using GroupDocs.Viewer.Options;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class Risk_RiskStatementController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        public Risk_RiskStatementController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("api/RiskStatement/GetRiskStatement")]
        [HttpGet]
        public IEnumerable<object> GetRiskStatement()
        {
            var details = (from risk_risk_statement in mySqlDBContext.Risk_RiskStatements
                           join risk_statementfilemaster in mySqlDBContext.Risk_StatementFileMasters on risk_risk_statement.RiskStatementID equals risk_statementfilemaster.RiskStatementID
                           where risk_risk_statement.Status == "Active" 
                           select new
                           {

                               risk_risk_statement.RiskStatementID,
                               risk_statementfilemaster.RiskLibrarySeq,
                               risk_risk_statement.RiskStatementName,
                               risk_risk_statement.RiskDescription,
                               risk_risk_statement.CreatedBy,
                               risk_risk_statement.CreatedOn,

                           })
                             .Distinct()
                 .ToList();


            return details;
        }
        [Route("api/RiskStatement/InsertRiskStatement")]
        [HttpPost]
        public async Task<IActionResult> InsertRiskStatement()
        {
           MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            try
            {
               
                var form = HttpContext.Request.Form;
                var RiskStatementName = form["riskstatement"].FirstOrDefault(); 
                var RiskDescription = form["riskdescription"].FirstOrDefault(); 
                var mainFile = form.Files;
                var createdBy = form["userid"].FirstOrDefault();
                var weblinks = form["weblink"].ToString();
                var TextField = form["Text"].ToString();

                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);
               
                var insertMasterQuery = "INSERT INTO risk_risk_statement (RiskStatementName, RiskDescription, CreatedDate,Status,CreatedBy) " +
                                        "VALUES (@RiskStatementName, @RiskDescription, @CreatedDate,@Status,@CreatedBy); " +
                                        "SELECT LAST_INSERT_ID();";

                MySqlCommand masterCommand = new MySqlCommand(insertMasterQuery, con);
                masterCommand.Parameters.AddWithValue("@RiskStatementName", RiskStatementName);
                masterCommand.Parameters.AddWithValue("@RiskDescription", RiskDescription);
                masterCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                masterCommand.Parameters.AddWithValue("@CreatedBy", createdBy);
                masterCommand.Parameters.AddWithValue("@Status", "Active");
                int RiskStatementID = Convert.ToInt32(masterCommand.ExecuteScalar());
                string statementfileID = $"R{RiskStatementID:D5}";
                var StatementIdFloder = Path.Combine("Reports", "RiskDocument", statementfileID);
                
                DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(StatementIdFloder);
                List<string> fileList = new List<string>();

                foreach (var file in mainFile)
                {
                     var filePath = Path.Combine(StatementIdFloder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    fileList.Add(filePath);

                  InsertFile("FileAttach", $"{baseUrl}/Reports/RiskDocument/{statementfileID}/{file.FileName}", file, RiskStatementID, file.FileName);
                }

                if (!string.IsNullOrEmpty(weblinks))
                {

                    string[] webLinksArray = weblinks.Split(';');

                    foreach (var weblink in webLinksArray)
                    {
                      
                        InsertFile("Weblink", weblink.Trim(), null, RiskStatementID, null);
                    }
                }
                if (!string.IsNullOrEmpty(TextField))
                {

                    string[] TextArray = TextField.Split(';');

                    foreach (var Text in TextArray)
                    {

                        InsertFile("Text", TextField.Trim(), null, RiskStatementID, null);
                    }
                }


                void InsertFile(string filecategory, string filepath, IFormFile file, int RiskStatementID,  string FileName)
                {
                    var fileUploadModel = new Risk_StatementFileMaster
                    {
                        FileName = filecategory == "FileAttach" ? file?.FileName : null, // Check if file is null before accessing its properties
                        FilePath = filepath,
                        FileCategory = filecategory,
                        Status = "Active",
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        RiskStatementID = RiskStatementID,
                                           };

                    string insertSubTableQuery = "INSERT INTO risk_statementfilemaster (RiskStatementID, FileCategory, FilePath, Status, createddate,FileName,RiskLibrarySeq,CreatedBy) " +
                                                    "VALUES (@RiskStatementID, @FileCategory, @FilePath, @Status, @createddate, @FileName,@RiskLibrarySeq,@CreatedBy)";

                    MySqlCommand subTableCommand = new MySqlCommand(insertSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@RiskStatementID", RiskStatementID);
                    subTableCommand.Parameters.AddWithValue("@FileCategory", fileUploadModel.FileCategory);
                    subTableCommand.Parameters.AddWithValue("@FilePath", fileUploadModel.FilePath);
                    subTableCommand.Parameters.AddWithValue("@Status", fileUploadModel.Status);
                    subTableCommand.Parameters.AddWithValue("@createddate", fileUploadModel.CreatedDate);
                    subTableCommand.Parameters.AddWithValue("@FileName", fileUploadModel.FileName); 
                    subTableCommand.Parameters.AddWithValue("@RiskLibrarySeq", statementfileID);
                    subTableCommand.Parameters.AddWithValue("@CreatedBy", createdBy);


                    subTableCommand.ExecuteNonQuery();

                }

                // return Ok();
                return Ok(new { message = "Inserted successfully", RiskStatementID = RiskStatementID , statementfileID = statementfileID });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex}");
            }
        }


        [Route("api/RiskStatement/GetRiskStatementByID/{riskStatementID}")]
        [HttpGet]

        public IEnumerable<object> GetRiskStatementByID(int riskStatementID)
        {
            var result = (from e in mySqlDBContext.Risk_RiskStatements.DefaultIfEmpty()
                          where e.RiskStatementID == riskStatementID
                          select new
                          {

                              RiskStatementID= e.RiskStatementID,
                              RiskStatementName= e.RiskStatementName,
                              RiskDescription= e.RiskDescription,
                              CreatedBy = e.CreatedBy,
                              CreatedOn = e.CreatedOn,

                          })
                         .Distinct()
                         .ToList();

            var Files = mySqlDBContext.Risk_StatementFileMasters
                .Where(af => result.Select(r => r.RiskStatementID).Contains(af.RiskStatementID) && af.Status == "Active")
                .Select(af => new
                {
                    af.StatementFileID,
                    af.RiskStatementID,
                    af.FilePath,
                    af.Status,
                    af.FileName,
                    filecategory = af.FileCategory,
                    af.CreatedOn,
                    af.RiskLibrarySeq

                })
                .ToList();

            var modifiedResult = result.Select(item => new
            {
                RiskStatementID = item.RiskStatementID,
                RiskStatementName = item.RiskStatementName,
                RiskDescription = item.RiskDescription,
                CreatedBy = item.CreatedBy,
                CreatedOn = item.CreatedOn,

                actfiles = Files
            })
            .ToList();

            return modifiedResult;
        }

        [Route("api/RiskStatement/removeRiskStatement/{statementFileID}")]
        [HttpPost]
        public void removeRiskStatement(int statementFileID)
        {
            try
            {
                var currentClass = new Risk_StatementFileMaster { StatementFileID = statementFileID };
                currentClass.Status = "Inactive";
               // currentClass.updatedDate = DateTime.Now.ToString("yyyy-MM-dd");
                this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
                this.mySqlDBContext.SaveChanges();
            }
            catch
            {
                return;
            }
        }


        [Route("Api/RiskStatement/UpdateRiskStatement")]
        [HttpPost]

        public async Task<IActionResult> UpdateRiskStatement([FromQuery] int riskStatementID)
        {
            try
            {
                var act = await mySqlDBContext.Risk_RiskStatements.FirstOrDefaultAsync(a => a.RiskStatementID == riskStatementID);

                if (act == null)
                {
                    return NotFound();
                }

                var formCollection = await Request.ReadFormAsync();

                // Update actregulatory details
                act.RiskStatementName = formCollection["riskstatement"];
                act.RiskDescription = formCollection["riskdescription"];
                act.CreatedOn = DateTime.Now.ToString("yyyy-MM-dd");
                //act.updatedby = int.Parse(formCollection["updatedby"]);

                // Weblinks
                var existingWeblinks = await mySqlDBContext.Risk_StatementFileMasters
                    .Where(f => f.RiskStatementID == riskStatementID && f.FileCategory == "Weblink" && f.Status == "Active")
                    .ToListAsync();

                var newWeblinks = formCollection["Weblink"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);

                var newWeblinkSet = new HashSet<string>(newWeblinks);

                foreach (var existingLink in existingWeblinks)
                {
                    if (!newWeblinkSet.Contains(existingLink.FileName))
                    {
                        existingLink.Status = "Inactive";
                        //existingLink.CreatedOn = DateTime.Now.ToString("yyyy-MM-dd");
                        //existingLink.updatedby = int.Parse(formCollection["updatedby"]);
                        // mySqlDBContext.Actregulatoryfilemodels.Remove(existingLink);
                    }
                }
                // Add new weblinks that are not in the existing weblinks
                foreach (var link in newWeblinks)
                {
                    if (!existingWeblinks.Any(f => f.FileName == link))
                    {
                        mySqlDBContext.Add(new Risk_StatementFileMaster
                        {
                            RiskStatementID =riskStatementID ,
                            FilePath = link,
                            FileCategory = "Weblink",
                            CreatedOn = DateTime.Now.ToString("yyyy-MM-dd"),
                            Status = "Active"
                        });
                    }
                }
                // TextFields  Attachments
                var existingTextfields = await mySqlDBContext.Risk_StatementFileMasters
                   .Where(f => f.RiskStatementID == riskStatementID && f.FileCategory == "Text" && f.Status == "Active")
                   .ToListAsync();
                var newTextfields = formCollection["Text"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);

                var newTextfieldset = new HashSet<string>(newTextfields);
                foreach (var existingLink in existingTextfields)
                {
                    if (!newTextfieldset.Contains(existingLink.FileName))
                    {
                        existingLink.Status = "Inactive";
                        //existingLink.CreatedOn = DateTime.Now.ToString("yyyy-MM-dd");
                        //existingLink.updatedby = int.Parse(formCollection["updatedby"]);
                        // mySqlDBContext.Actregulatoryfilemodels.Remove(existingLink);
                    }
                }
                // Add new TextFields that are not in the existing weblinks
                foreach (var link in newTextfields)
                {
                    if (!existingTextfields.Any(f => f.FileName == link))
                    {
                        mySqlDBContext.Add(new Risk_StatementFileMaster
                        {
                            RiskStatementID = riskStatementID,
                            FilePath = link,
                            FileCategory = "Text",
                            CreatedOn = DateTime.Now.ToString("yyyy-MM-dd"),
                            Status = "Active"
                        });
                    }
                }
                // Handle File Attachments
                var existingFiles = await mySqlDBContext.Risk_StatementFileMasters
                    .Where(f => f.RiskStatementID == riskStatementID && f.FileCategory == "FileAttach" && f.Status == "Active")
                    .ToListAsync();


                var files = Request.Form.Files;



                var statementfileID = $"R{riskStatementID:D5}"; // Retrieve global_act_id from the form data

                // Ensure the directory exists
                var globalactIdFolder = Path.Combine("Reports", "RiskDocument", statementfileID);
                Directory.CreateDirectory(globalactIdFolder);
               
                foreach (var file in files)
                {
                    var fileName = file.FileName;
                    var filePath = Path.Combine(globalactIdFolder, fileName);
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    var fileUrl = new Uri(new Uri(baseUrl), $"Reports/RiskDocument/{statementfileID}/{fileName}").ToString();

                    var existingFile = existingFiles.FirstOrDefault(f => f.FileName == fileName);
                    if (existingFile != null)
                    {
                        // Update existing file properties
                        existingFile.FileCategory = "FileAttach";
                        existingFile.FilePath = fileUrl;
                        existingFile.CreatedOn = DateTime.Now.ToString("yyyy-MM-dd");

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
                        mySqlDBContext.Risk_StatementFileMasters.Add(new Risk_StatementFileMaster
                        {
                            RiskStatementID = riskStatementID,
                            FileName = fileName,
                            FileCategory = "FileAttach",
                            FilePath = fileUrl,
                            CreatedOn = DateTime.Now.ToString("yyyy-MM-dd"),
                            // updatedby = int.Parse(formCollection["updatedby"]),
                            Status = "Active"
                            //IsImportedData = "No"
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

        [Route("api/RiskSatement/DeActivateRiskSatement")]
        [HttpPost]
        public IActionResult DeActivateRiskSatement([FromBody] Risk_RiskStatement Risk_RiskStatements)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string UpdateQuery = "UPDATE risk_risk_statement SET DisableReason=@DisableReason,Status=@Status,CreatedBy=@CreatedBy,CreatedOn=@CreatedOn WHERE RiskStatementID = @RiskStatementID";

            try
            {
                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@RiskStatementID", Risk_RiskStatements.RiskStatementID);
                    myCommand.Parameters.AddWithValue("@DisableReason", Risk_RiskStatements.DisableReason);
                    myCommand.Parameters.AddWithValue("@Status", "InActive");
                    myCommand.Parameters.AddWithValue("@CreatedBy", Risk_RiskStatements.CreatedBy);
                    myCommand.Parameters.AddWithValue("@CreatedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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


        [Route("api/RiskSatement/ReactivateRiskSatement")]
        [HttpPut]
        public IActionResult ReactivateRiskSatement([FromBody] Risk_RiskStatement Risk_RiskStatements)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string UpdateQuery = "UPDATE risk_risk_statement SET " + "Status=@Status " + " WHERE RiskStatementID = @RiskStatementID";

            try
            {
                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@RiskStatementID", Risk_RiskStatements.RiskStatementID);
                    myCommand.Parameters.AddWithValue("@Status", "InActive");

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
        [Route("api/RiskStatement/GetReactivateRiskSatement")]
        [HttpGet]
        public IEnumerable<object> GetReactivateRiskSatement()
        {
            var details = (from risk_risk_statement in mySqlDBContext.Risk_RiskStatements
                           join risk_statementfilemaster in mySqlDBContext.Risk_StatementFileMasters on risk_risk_statement.RiskStatementID equals risk_statementfilemaster.RiskStatementID
                           where risk_risk_statement.Status == "InActive"
                           select new
                           {

                               risk_risk_statement.RiskStatementID,
                               risk_statementfilemaster.RiskLibrarySeq,
                               risk_risk_statement.RiskStatementName,
                               risk_risk_statement.RiskDescription,
                               risk_risk_statement.CreatedBy,
                               risk_risk_statement.CreatedOn,

                           })
                             .Distinct()
                 .ToList();


            return details;
        }
    }

}
