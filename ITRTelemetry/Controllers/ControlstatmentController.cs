using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using iText.Kernel.Pdf;
using System.Diagnostics;
using MySqlX.XDevAPI.Common;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]

    public class ControlstatmentController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;



        public ControlstatmentController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

        }



        [Route("api/controlstatment/Getcontrolstatment")]
        [HttpGet]

        public IEnumerable<object> Getcontrolstatment()
        {
            //return this.mySqlDBContext.Actregulatorymodels.Where(x => x.status == "Active").ToList();

            var act = from control in mySqlDBContext.controlstatementmodels
                      where control.status == "Active"
                      select new
                      {
                          control.control_statement_id,
                          control.control_objective_heading,
                          control.control_detailed_description,
                        
                      };
            var result = act.ToList();


            return result;
        }


        [Route("api/controlstatment/Getcontrolstatmentbyid/{cntid}")]
        [HttpGet]

        public IEnumerable<object> Getcontrolstatmentbyid(int cntid)
        {
            var result = (from control in mySqlDBContext.controlstatementmodels
                          join tbluser in mySqlDBContext.usermodels on control.createdBy equals tbluser.USR_ID
                          join user in mySqlDBContext.usermodels on control.createdBy equals user.USR_ID into userJoin
                          from user in userJoin.DefaultIfEmpty()
                          where control.control_statement_id == cntid
                          select new
                          {
                              controlid = control.control_statement_id,
                              controlname = control.control_objective_heading,
                              
                              createdby = control.createdBy,
                              createddate = control.created_date,
                              globalcontrolId = control.globalcontrolId,
                             
                              controldescription = control.control_brief_description,
                              controldetaildesc = control.control_detailed_description,
                              create = $"{tbluser.firstname}-{control.created_date}",
                              //update = $"{user.firstname}-{actregulatory.updatedDate}"
                          })
                         .Distinct()
                         .ToList();

            var actFiles = mySqlDBContext.controlstatementfilemodels
                .Where(af => result.Select(r => r.controlid).Contains(af.control_statement_id) && af.status == "Active")
                .Select(af => new
                {
                    af.control_statement_attach_doc_id,
                    af.control_statement_id,
                    af.filepath,
                    af.status,
                    filecategory = af.filecategory
                })
                .ToList();

            var modifiedResult = result.Select(item => new
            {
                item.controlid,
                item.controlname,
                item.controldescription,
                item.create,
                item.controldetaildesc,
                //item.IsImportedData,
                item.globalcontrolId,
                //item.update,
                item.createddate,
                item.createdby,

                actfiles = actFiles.Where(af => af.control_statement_id == item.controlid && af.status == "Active")
                                   .Select(af => new
                                   {
                                       af.control_statement_attach_doc_id,
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

        [Route("api/controlstatment/Insertcontrolstatment")]
        [HttpPost]
        public async Task<IActionResult> Insertcontrolstatment()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            try
            {
                var Maxactid = this.mySqlDBContext.controlstatementmodels.Max(d => (int?)d.control_statement_id) ?? 0;

                var newActRegulatoryId = Maxactid + 1;


                var form = HttpContext.Request.Form;
                var control_objective_heading = form["control_objective_heading"].FirstOrDefault(); // Get the first value
                var control_brief_description = form["control_brief_description"].FirstOrDefault(); // Get the first value 
                var control_detailed_description = form["control_detailed_description"].FirstOrDefault();
                var addsubcontrolcheckboxField = form["addsubcontrolcheckboxField"].FirstOrDefault();
                var add_sub_control = form["additionalsub1"].FirstOrDefault();
                var controlmeasureid= form["controlmeasureid"].FirstOrDefault();
                var mainFile = form.Files; // This should contain the files weblink
                var createdBy = form["userId"].FirstOrDefault();
                var weblinks = form["weblink"].ToString();
                var fieldsArray = form["fields"].ToString();
                // Get the current HTTP request
                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);
                // Generate global_act_id
                string globalcontrolId = GenerateGlobalActId(con);

                var globalactIdFloder = Path.Combine("Reports", "RiskControlstatment", globalcontrolId);

                DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(globalactIdFloder);



                if (!string.IsNullOrEmpty(fieldsArray))
                {
                    // Fetch the max control_statement_sub_id or default to 0
                    var maxSubControlId = this.mySqlDBContext.controlsubstatementmodels
                                          .Max(d => (int?)d.control_statement_sub_id) ?? 0;

                    var fields = System.Text.Json.JsonSerializer
                                .Deserialize<List<Dictionary<string, object>>>(fieldsArray);

                    foreach (var field in fields)
                    {
                        // Retrieve values from the field dictionary
                        var subControlName = field.ContainsKey("additionalsub") ? field["additionalsub"]?.ToString() : null;
                        var checkboxField2 = field.ContainsKey("checkboxField2") && field["checkboxField2"]?.ToString().ToLower() == "true" ? "true" : "false";
                        var dependentSubControlId = field.ContainsKey("dependentid") ? field["dependentid"]?.ToString() : null;

                        // Increment control_statement_sub_id for each new record
                        maxSubControlId++;

                        // Insert each field record into the database
                        string insertFieldQuery = @"INSERT INTO risk_control_statement_sub 
            (control_statement_sub_id, control_statement_id, sub_control_name, checkboxField2, dependent_sub_control_id, created_date, status) 
            VALUES (@control_statement_sub_id, @control_statement_id, @sub_control_name, @checkboxField2, @dependent_sub_control_id, @created_date, @status)";

                        using (MySqlCommand fieldCommand = new MySqlCommand(insertFieldQuery, con))
                        {
                            // Add parameters to the SQL command
                            fieldCommand.Parameters.AddWithValue("@control_statement_sub_id", maxSubControlId);
                            fieldCommand.Parameters.AddWithValue("@control_statement_id", newActRegulatoryId);
                            fieldCommand.Parameters.AddWithValue("@sub_control_name", subControlName);
                            fieldCommand.Parameters.AddWithValue("@checkboxField2", checkboxField2);
                            fieldCommand.Parameters.AddWithValue("@dependent_sub_control_id", dependentSubControlId);
                            fieldCommand.Parameters.AddWithValue("@created_date", DateTime.Now);
                            fieldCommand.Parameters.AddWithValue("@status", "Active");

                            // Execute the query
                            fieldCommand.ExecuteNonQuery();
                        }
                    }
                }



                // Insert data into act-regulatorymaster table using insertMasterQuery
                var insertMasterQuery = "INSERT INTO risk_control_statement (control_statement_id,control_objective_heading,control_brief_description, control_detailed_description,addsubcontrolcheckboxField,add_sub_control,controlmeasureid,globalcontrolId, created_date,status) " +
                                        "VALUES (@control_statement_id,@control_objective_heading,@control_brief_description, @control_detailed_description,@addsubcontrolcheckboxField,@add_sub_control,@controlmeasureid,@globalcontrolId, @created_date,@status); " +
                                        "SELECT LAST_INSERT_ID();";

                MySqlCommand masterCommand = new MySqlCommand(insertMasterQuery, con);
                masterCommand.Parameters.AddWithValue("@control_statement_id", newActRegulatoryId);
                masterCommand.Parameters.AddWithValue("@control_objective_heading", control_objective_heading);
                masterCommand.Parameters.AddWithValue("@control_brief_description", control_brief_description);
                masterCommand.Parameters.AddWithValue("@control_detailed_description", control_detailed_description);
                masterCommand.Parameters.AddWithValue("@addsubcontrolcheckboxField", addsubcontrolcheckboxField);
                masterCommand.Parameters.AddWithValue("@add_sub_control", add_sub_control);
                masterCommand.Parameters.AddWithValue("@controlmeasureid", controlmeasureid);
                masterCommand.Parameters.AddWithValue("@globalcontrolId", globalcontrolId);
                masterCommand.Parameters.AddWithValue("@created_date", DateTime.Now);
                masterCommand.Parameters.AddWithValue("@createdBy", createdBy);
            
                masterCommand.Parameters.AddWithValue("@status", "Active");
              

                int insertedactRegulatoryId = Convert.ToInt32(masterCommand.ExecuteScalar());

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
                    InsertFile("FileAttach", $"{baseUrl}/Reports/RiskControlstatment/{globalcontrolId}/{file.FileName}", file, newActRegulatoryId, globalcontrolId, file.FileName);
                }

                if (!string.IsNullOrEmpty(weblinks))
                {

                    string[] webLinksArray = weblinks.Split(';');

                    foreach (var weblink in webLinksArray)
                    {
                        // Insert each web link as a separate record
                        InsertFile("Weblink", weblink.Trim(), null, newActRegulatoryId, globalcontrolId, null);
                    }
                }

                // Function to insert file or web link
                void InsertFile(string filecategory, string filepath, IFormFile file, int newActRegulatoryId, string globalcontrolId, string fileName)
                {
                    var fileUploadModel = new controlstatementfilemodel
                    {
                        file_name = filecategory == "FileAttach" ? file?.Name : null, // Check if file is null before accessing its properties
                        filepath = filepath,
                        filecategory = filecategory,
                        status = "Active",
                        created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        control_statement_id = newActRegulatoryId,
                        globalcontrolId = globalcontrolId,
                    };

                    var Maxactfileid = this.mySqlDBContext.controlstatementfilemodels.Max(d => (int?)d.control_statement_attach_doc_id) ?? 0;

                    var newActfileRegulatoryId = Maxactfileid + 1;


                    string insertSubTableQuery = "INSERT INTO risk_control_statement_attach_doc (control_statement_attach_doc_id,control_statement_id, globalcontrolId, filecategory, filepath, status, created_date,file_name) " +
                                                    "VALUES (@control_statement_attach_doc_id,@control_statement_id, @globalcontrolId, @fileCategory, @filePath, @status, @createddate, @file_name)";

                    MySqlCommand subTableCommand = new MySqlCommand(insertSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@control_statement_attach_doc_id", newActfileRegulatoryId);
                    subTableCommand.Parameters.AddWithValue("@control_statement_id", newActRegulatoryId);
                    subTableCommand.Parameters.AddWithValue("@globalcontrolId", globalcontrolId);
                    subTableCommand.Parameters.AddWithValue("@fileCategory", fileUploadModel.filecategory);
                    subTableCommand.Parameters.AddWithValue("@filePath", fileUploadModel.filepath);
                    subTableCommand.Parameters.AddWithValue("@status", fileUploadModel.status);
                    subTableCommand.Parameters.AddWithValue("@createddate", fileUploadModel.created_date);
                    subTableCommand.Parameters.AddWithValue("@file_name", fileName);
                   


                    subTableCommand.ExecuteNonQuery();
                }
                // Extract additionalsub value


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
            string getMaxIdQuery = "SELECT MAX(CAST(SUBSTRING(globalcontrolId, LENGTH(globalcontrolId) - 2) AS UNSIGNED)) FROM risk.risk_control_statement";
            using (MySqlCommand getMaxIdCommand = new MySqlCommand(getMaxIdQuery, con))
            {
                object maxIdObj = getMaxIdCommand.ExecuteScalar();
                if (maxIdObj != DBNull.Value && maxIdObj != null)
                {
                    int maxSerialNo = Convert.ToInt32(maxIdObj);
                    maxSerialNo++; // Increment the last number by 1
                    gActId = $"C{maxSerialNo:D4}"; // Ensure it's formatted with leading zeros
                }
                else
                {
                    // If no existing records, generate the default ID
                    gActId = "C00001"; // Start the series from 001 onwards
                }
            }

            return gActId;
        }



    }
}
