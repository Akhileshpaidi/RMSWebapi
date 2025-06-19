
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using iText.Commons.Utils;
using Newtonsoft.Json;
//using GroupDocs.Viewer.Options;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using DocumentFormat.OpenXml.Bibliography;
using MySqlX.XDevAPI.Common;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class Risk_AddRegisterController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public Risk_AddRegisterController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }
        [Route("api/RiskDeparment/GetRiskDeparment/{unitid}")]
        [HttpGet]
        public IEnumerable<object> GetdeptKeyFocusArea(int unitid)
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           where businessfunction.status == "Active" && businessfunction.unitlocationid == unitid.ToString()
                           select new
                           {
                               businessfunction.entityid,
                               businessfunction.unitlocationid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                           })
                             .Distinct()
                 .ToList();

            return details;
        }

        [Route("api/getRiskDeparment/RiskDeparment")]
        [HttpGet]
        public IEnumerable<object> RiskDeparment()
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           where businessfunction.status == "Active" 
                           select new
                           {
                               businessfunction.entityid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                           })
                             .Distinct()
                 .ToList();

            return details;
        }

        [Route("api/RiskBusinessFunction/GetRiskBusinessFunction/{depid}")]
        [HttpGet]
        public IEnumerable<object> GetbusiKeyFocusArea(int depid)
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           where businessfunction.status == "Active" && businessfunction.departmentid == depid
                           select new
                           {

                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,

                           })
                             .Distinct()
                 .ToList();

            return details;
        }

        [Route("api/BusinessProcess/GetBusinessProcess/{processID}")]
        [HttpGet]
        public IEnumerable<object> GetBusintKeyFocusArea(int processID)
        {
            var details = (from risk_businessprocess_l1 in mySqlDBContext.Risk_Sub_ProcessL1s
                           where risk_businessprocess_l1.ProcessL1Status == "Active" && risk_businessprocess_l1.BusinessprocessID == processID
                           select new
                           {

                               risk_businessprocess_l1.BusinessProcessL1ID,
                               risk_businessprocess_l1.BusinessSubProcessL1Name,
                               risk_businessprocess_l1.SubProcessObjestive,

                           })
                             .Distinct()
                 .ToList();

            return details;
        }
        [Route("api/BusinessProcessL1/GetBusinessProcessL1/{processL1}")]
        [HttpGet]
        public IEnumerable<object> GetBusinLItKeyFocusArea(int processL1)
        {
            var details = (from risk_businessprocess_l2 in mySqlDBContext.Risk_Sub_ProcessL2s
                           where risk_businessprocess_l2.ProcessL2Status == "Active" && risk_businessprocess_l2.BusinessProcessL1ID == processL1
                           select new
                           {

                               risk_businessprocess_l2.BusinessProcessL2ID,
                               risk_businessprocess_l2.BusinessSubProcessL2Name,

                           })
                             .Distinct()
                 .ToList();

            return details;
        }

        [Route("api/BusinessProcessL2/GetBusinessProcessL2/{processL2}")]
        [HttpGet]
        public IEnumerable<object> GetBusinL2tKeyFocusArea(int processL2)
        {
            var details = (from risk_businessprocess_l3 in mySqlDBContext.Risk_Sub_ProcessL3s
                           where risk_businessprocess_l3.ProcessL3Status == "Active" && risk_businessprocess_l3.BusinessProcessL2ID == processL2
                           select new
                           {

                               risk_businessprocess_l3.BusinessProcessL3ID,
                               risk_businessprocess_l3.BusinessSubProcessL3Name,

                           })
                             .Distinct()
                 .ToList();

            return details;
        }


        [Route("api/RiskDeparment/GetLossEventThreatCategoryL1/{riskid}")]
        [HttpGet]
        public IEnumerable<object> GetLossEventThreatCategoryL1(int riskid)
        {
            var details = (from risk_admin_letc_l1 in mySqlDBContext.risk_admin_letc_l1
                           where risk_admin_letc_l1.status == "Active"
                                 && risk_admin_letc_l1.risk_admin_LETC_L1_id == riskid

                           select new
                           {
                               //risk_admin_letc_l1.risk_admin_LETC_L1_id,
                               risk_admin_letc_l1.risk_admin_LETC_L1_show_desc,
                           })
                    .Distinct()
                    .ToList();

            return details;
        }

        [Route("api/RiskDeparment/GetLossEventThreatCategoryL2/{ThreatCategoryid}")]
        [HttpGet]
        public IEnumerable<object> GetLossEventThreatCategoryL2(int ThreatCategoryid)
        {
            var details = (from risk_admin_letc_l2 in mySqlDBContext.risk_Admin_Letc_L2s
                           where risk_admin_letc_l2.status == "Active"
                           && risk_admin_letc_l2.risk_admin_letc_l2_id == ThreatCategoryid

                           select new
                           {
                               //risk_admin_letc_l1.risk_admin_LETC_L1_id,
                               risk_admin_letc_l2.risk_admin_letc_l2_show_desc,
                           })
                    .Distinct()
                    .ToList();

            return details;
        }

        [Route("api/RiskDeparment/GetLossEventThreatCategoryL3/{Categoryid}")]
        [HttpGet]
        public IEnumerable<object> GetLossEventThreatCategoryL3(int Categoryid)
        {
            var details = (from risk_admin_letc_l3 in mySqlDBContext.risk_admin_letc_l3
                           where risk_admin_letc_l3.status == "Active"
                           && risk_admin_letc_l3.risk_admin_LETC_l3_id == Categoryid

                           select new
                           {
                               risk_admin_letc_l3.risk_admin_LETC_l3_desc,
                           })
                    .Distinct()
                    .ToList();

            return details;
        }

        [Route("api/RiskDeparment/Getriskpotenbussimpact/{potentialid}")]
        [HttpGet]
        public IEnumerable<object> Getriskpotenbussimpact(int potentialid)
        {
            var details = (from risk_admin_potenbussimpact in mySqlDBContext.risk_admin_potenbussimpact
                           where risk_admin_potenbussimpact.risk_admin_potenBussImpactstatus == "Active"
                           && risk_admin_potenbussimpact.risk_admin_potenBussImpactid == potentialid

                           select new
                           {
                               risk_admin_potenbussimpact.risk_admin_potenBussImpactdesc,
                           })
                    .Distinct()
                    .ToList();

            return details;

        }






        [Route("api/RiskDeparment/GetRiskDeparment")]
        [HttpGet]
        public IEnumerable<object> GetRiskDeparment()
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           where businessfunction.status == "Active"
                           select new
                           {
                               businessfunction.entityid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                           })
                             .Distinct()
                 .ToList();

            return details;
        }
        // range rating
        //[Route("api/RiskInhervRange/GetRiskInhervRangelev")]
        //[HttpGet]
        //public IEnumerable<risk_admin_inherriskratinglevl> GetRiskInhervRange(int range)
        //{
        //    var pdata = new List<risk_admin_inherriskratinglevl>();

        //    // Use a using statement to ensure the connection is closed properly
        //    using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
        //    {
        //        con.Open();
        //        using (MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM risk.risk_admin_inherriskratinglevl 
        //                                            WHERE risk_level_range_min <= @range AND risk_level_range_max >= @range", con))
        //        {
        //            // Add parameter to avoid SQL injection
        //            cmd.Parameters.AddWithValue("@range", range);
        //            cmd.CommandType = CommandType.Text;

        //            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        //            {
        //                DataTable dt = new DataTable();
        //                da.Fill(dt);

        //                // Check if dt is filled and if it contains rows
        //                if (dt.Rows.Count > 0)
        //                {
        //                    for (var i = 0; i < dt.Rows.Count; i++)
        //                    {
        //                        pdata.Add(new risk_admin_inherriskratinglevl
        //                        {
        //                            risk_admin_inherRiskRatingLevlname = dt.Rows[i]["risk_admin_inherRiskRatingLevlname"]?.ToString(),
        //                            colour_reference = dt.Rows[i]["colour_reference"]?.ToString(),
        //                            risk_admin_inherRiskRatingLevlid = Convert.ToInt32(dt.Rows[i]["risk_admin_inherRiskRatingLevlid"] ?? "0")
        //                        });
        //                    }

        //                }
        //                else
        //                {
        //                    pdata.Add(new risk_admin_inherriskratinglevl
        //                    {
        //                        colour_reference = ""
        //                    });

        //                    }
        //            }
        //        }
        //    }

        //    return pdata;
        //}

        [Route("api/RiskInhervRange/GetRiskInhervRangelev")]
        [HttpGet]
        public ActionResult<IEnumerable<risk_admin_inherriskratinglevl>> GetRiskInhervRange(int range)
        {
            var pdata = new List<risk_admin_inherriskratinglevl>();

            // Use a using statement to ensure the connection is closed properly
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM risk.risk_admin_inherriskratinglevl 
                                         WHERE risk_level_range_min <= @range AND risk_level_range_max >= @range", con))
                {
                    // Add parameter to avoid SQL injection
                    cmd.Parameters.AddWithValue("@range", range);
                    cmd.CommandType = CommandType.Text;

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Check if dt is filled and if it contains rows
                        if (dt.Rows.Count > 0)
                        {
                            for (var i = 0; i < dt.Rows.Count; i++)
                            {
                                pdata.Add(new risk_admin_inherriskratinglevl
                                {
                                    risk_admin_inherRiskRatingLevlname = dt.Rows[i]["risk_admin_inherRiskRatingLevlname"]?.ToString(),
                                    colour_reference = dt.Rows[i]["colour_reference"]?.ToString(),
                                    risk_admin_inherRiskRatingLevlid = Convert.ToInt32(dt.Rows[i]["risk_admin_inherRiskRatingLevlid"] ?? "0")
                                });
                            }
                            return Ok(pdata); // Return 200 OK with data
                        }
                        else
                        {
                            // Return a NotFound result with a validation message
                            return NotFound("No acceptable values found for the specified range.");
                        }
                    }
                }
            }
        }


        [Route("api/RiskPriority/GetRiskPriorityRange")]
        [HttpGet]
        public ActionResult<IEnumerable<risk_admin_riskpriority>> GetRiskPriorityRange(int rangevalue)
        {
            var pdata = new List<risk_admin_riskpriority>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM risk.risk_admin_riskpriority 
                             WHERE rating_level_min <= @rangevalue AND rating_level_max >= @rangevalue", con))
                {
                    // Add parameter to avoid SQL injection
                    cmd.Parameters.AddWithValue("@rangevalue", rangevalue);
                    cmd.CommandType = CommandType.Text;

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Check if dt is filled and if it contains rows
                        if (dt.Rows.Count > 0)
                        {
                            for (var i = 0; i < dt.Rows.Count; i++)
                            {
                                pdata.Add(new risk_admin_riskpriority
                                {
                                    risk_admin_riskPriorityName = dt.Rows[i]["risk_admin_riskPriorityName"]?.ToString(),
                                    color_code = dt.Rows[i]["color_code"]?.ToString(),
                                    risk_admin_riskPriorityId = Convert.ToInt32(dt.Rows[i]["risk_admin_riskPriorityId"] ?? "0")
                                });
                            }
                            return Ok(pdata); // Return 200 OK with data
                        }
                        else
                        {
                            // Return a NotFound result with a validation message
                            return BadRequest();
                        }
                    }
                }
            }



        }


        [Route("api/RiskIntensity/GetRiskIntensityRange")]
        [HttpGet]
        public ActionResult<IEnumerable<risk_admin_riskintensity>> GetRiskIntensityRange(int rangevalue)
        {
            var pdata = new List<risk_admin_riskintensity>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM risk.risk_admin_riskintensity 
                                                    WHERE risk_level_range_min <= @rangevalue AND risk_level_range_max >= @rangevalue", con))
                {
                    // Add parameter to avoid SQL injection
                    cmd.Parameters.AddWithValue("@rangevalue", rangevalue);
                    cmd.CommandType = CommandType.Text;

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Check if dt is filled and if it contains rows
                        if (dt.Rows.Count > 0)
                        {
                            for (var i = 0; i < dt.Rows.Count; i++)
                            {

                                pdata.Add(new risk_admin_riskintensity
                                {
                                    risk_admin_riskIntensityname = dt.Rows[i]["risk_admin_riskIntensityname"]?.ToString(),
                                    colour_reference = dt.Rows[i]["colour_reference"]?.ToString(),
                                    risk_admin_riskIntensityid = Convert.ToInt32(dt.Rows[i]["risk_admin_riskIntensityid"] ?? "0")
                                });
                            }
                            return Ok(pdata); // Return 200 OK with data
                        }
                        else
                        {
                            // Return a NotFound result with a validation message
                            return BadRequest();
                        }
                    }
                }
            }


        }

        [Route("api/SearchDocuments/GetSearchDocuments")]
        [HttpGet]
        public IEnumerable<Risk_RiskStatement> GetSearchDocuments(int RiskStatementID)
        {
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string query = @"SELECT riskStatementName,riskDescription,FileCategory,FileName,FilePath FROM risk.risk_risk_statement
                   LEFT JOIN risk.risk_statementfilemaster ON risk_risk_statement.RiskStatementID = risk_statementfilemaster.RiskStatementID
              WHERE risk_risk_statement.Status = 'Active' and risk_risk_statement.RiskStatementID ='" + RiskStatementID + "';";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RiskStatementID", RiskStatementID);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    System.Data.DataTable dt = new System.Data.DataTable();
                    da.Fill(dt);

                    var riskStatement = new Risk_RiskStatement();

                    foreach (DataRow row in dt.Rows)
                    {
                        //riskStatements.Add(new Risk_RiskStatement
                        //{
                        //    RiskStatementName = row["riskStatementName"].ToString(),
                        //    RiskDescription = row["riskDescription"].ToString(),
                        //});
                        if (riskStatement.RiskStatementName == null) // First row, set the statement details
                        {
                            riskStatement.RiskStatementName = row["riskStatementName"].ToString();
                            riskStatement.RiskDescription = row["riskDescription"].ToString();
                        }

                        if (row["FileCategory"] != DBNull.Value && row["FileName"] != DBNull.Value)
                        {
                            var attachment = new Risk_StatementFileMaster
                            {
                                FileCategory = row["FileCategory"].ToString(),
                                FileName = row["FileName"].ToString(),
                            };
                            riskStatement.Attachments.Add(attachment);
                        }
                        else if (row["FileCategory"] != DBNull.Value && row["FilePath"] != DBNull.Value)
                        {
                            var attachment = new Risk_StatementFileMaster
                            {
                                FileCategory = row["FileCategory"].ToString(),
                                FilePath = row["FilePath"].ToString(),
                            };
                            riskStatement.Attachments.Add(attachment);
                        }
                    }

                    return new List<Risk_RiskStatement> { riskStatement };
                }
            }
        }

        [Route("api/GetDocumentsLists/GetDocumentsListsdetails/{riskMasterID}")]
        [HttpGet]
        public IEnumerable<Risk_RiskStatement> GetDocumentsListsdetails(int riskMasterID)
        {
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string query = @"select risk_risk_statement.RiskStatementID,
    risk_risk_statement.RiskStatementName,
    risk_risk_statement.RiskDescription,
    risk_statementfilemaster.FileCategory,
    risk_statementfilemaster.FileName,
    risk_statementfilemaster.FilePath, 
     
    risk_riskregistermaster.RiskDefinition,
    risk_riskregistermaster.RiskRegisterMasterID AS RiskRegisterStatementID FROM 
    risk.risk_risk_statement
LEFT JOIN 
    risk.risk_statementfilemaster 
    ON risk_risk_statement.RiskStatementID = risk_statementfilemaster.RiskStatementID
LEFT JOIN 
    risk.risk_riskregistermaster 
    ON risk_risk_statement.RiskStatementID = risk_riskregistermaster.RiskStatementID
WHERE 
    risk_risk_statement.Status = 'Active' 
    AND risk_riskregistermaster.RiskRegisterMasterID  = '" + riskMasterID + "' ";
 

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RiskRegisterMasterID", riskMasterID);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    System.Data.DataTable dt = new System.Data.DataTable();
                    da.Fill(dt);

                    var riskStatement = new Risk_RiskStatement();

                    foreach (DataRow row in dt.Rows)
                    {
                       
                        if (riskStatement.RiskStatementName == null) // First row, set the statement details
                        {
                            riskStatement.RiskStatementID = Convert.ToInt32(row["RiskStatementID"]);  
                            riskStatement.RiskStatementName = row["riskStatementName"].ToString();
                            riskStatement.RiskDescription = row["riskDescription"].ToString();
                           
                        }
                        if (row["RiskRegisterStatementID"] != DBNull.Value)
                        {
                            riskStatement.RiskRegisterStatementID = Convert.ToInt32(row["RiskRegisterStatementID"]);
                            riskStatement.RiskDefinition = row["RiskDefinition"].ToString();
                        }

                        if (row["FileCategory"] != DBNull.Value && row["FileName"] != DBNull.Value)
                        {
                            var attachment = new Risk_StatementFileMaster
                            {
                                FileCategory = row["FileCategory"].ToString(),
                                FileName = row["FileName"].ToString(),
                               
                            };
                            riskStatement.Attachments.Add(attachment);
                        }
                        else if (row["FileCategory"] != DBNull.Value && row["FilePath"] != DBNull.Value)
                        {
                            var attachment = new Risk_StatementFileMaster
                            {
                                FileCategory = row["FileCategory"].ToString(),
                                FilePath = row["FilePath"].ToString(),
                              
                            };
                            riskStatement.Attachments.Add(attachment);
                        }
                    }

                    return new List<Risk_RiskStatement> { riskStatement };
                }
            }
        }


        [Route("api/RiskIntensity/GetRiskIntensityRange1")]
        [HttpGet]
        public IEnumerable<risk_admin_riskintensity> GetRiskIntensityRange1(int rangevalue)
        {
            var pdata = new List<risk_admin_riskintensity>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM risk.risk_admin_riskintensity 
                                                    WHERE risk_level_range_min <= @rangevalue AND risk_level_range_max >= @rangevalue", con))
                {
                    // Add parameter to avoid SQL injection
                    cmd.Parameters.AddWithValue("@rangevalue", rangevalue);
                    cmd.CommandType = CommandType.Text;

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Check if dt is filled and if it contains rows
                        if (dt.Rows.Count > 0)
                        {
                            for (var i = 0; i < dt.Rows.Count; i++)
                            {

                                pdata.Add(new risk_admin_riskintensity
                                {
                                    risk_admin_riskIntensityname = dt.Rows[i]["risk_admin_riskIntensityname"]?.ToString(),
                                    colour_reference = dt.Rows[i]["colour_reference"]?.ToString(),
                                    risk_admin_riskIntensityid = Convert.ToInt32(dt.Rows[i]["risk_admin_riskIntensityid"] ?? "0")
                                });
                            }
                        }
                        else
                        {
                            pdata.Add(new risk_admin_riskintensity
                            {
                                colour_reference = ""
                            });

                        }
                    }
                }
            }

            return pdata;
        }

        [Route("api/SearchDocuments/GetSearchDocuments/{RiskStatementID}")]
        [HttpGet]
        public IEnumerable<Risk_RiskStatement> GetSearchDocuments1(int RiskStatementID)
        {
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();

                string query = @"SELECT riskStatementName,riskDescription,FileCategory,FileName FROM risk.risk_risk_statement
                   LEFT JOIN risk.risk_statementfilemaster ON risk_risk_statement.RiskStatementID = risk_statementfilemaster.RiskStatementID
              WHERE risk_risk_statement.Status = 'Active' and risk_risk_statement.RiskStatementID ='" + RiskStatementID + "';";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RiskStatementID", RiskStatementID);
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    System.Data.DataTable dt = new System.Data.DataTable();
                    da.Fill(dt);

                    var riskStatement = new Risk_RiskStatement();

                    foreach (DataRow row in dt.Rows)
                    {
                        //riskStatements.Add(new Risk_RiskStatement
                        //{
                        //    RiskStatementName = row["riskStatementName"].ToString(),
                        //    RiskDescription = row["riskDescription"].ToString(),
                        //});
                        if (riskStatement.RiskStatementName == null) // First row, set the statement details
                        {
                            riskStatement.RiskStatementName = row["riskStatementName"].ToString();
                            riskStatement.RiskDescription = row["riskDescription"].ToString();
                        }

                        if (row["FileCategory"] != DBNull.Value && row["FileName"] != DBNull.Value)
                        {
                            var attachment = new Risk_StatementFileMaster
                            {
                                FileCategory = row["FileCategory"].ToString(),
                                FileName = row["FileName"].ToString(),
                            };
                            riskStatement.Attachments.Add(attachment);
                        }
                    }

                    return new List<Risk_RiskStatement> { riskStatement };
                }
            }
        }

        [Route("api/RiskRegisterDoc/AddRiskRegisterDoc")]
        [HttpPost]
        public IActionResult InsertRiskRegister()
        // public async Task<IActionResult> InsertRiskRegister()

        {
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
             int   riskStatmentscount = 0;

                try
                {
                    con.Open();
                    var form = HttpContext.Request.Form;

                    // Retrieve and log the JSON payload
                    var jsonPayload = form["jsonPayload"];
                    if (string.IsNullOrEmpty(jsonPayload))
                    {
                        return BadRequest("jsonPayload is missing.");
                    }

                    Console.WriteLine("jsonPayload received: " + jsonPayload);

                    // Deserialize the JSON payload into the model
                    var riskModel = JsonConvert.DeserializeObject<Risk_AddRegisterModel>(jsonPayload);

                    // Process the file if available
                    var mainFile = form.Files.FirstOrDefault();
                    if (mainFile != null)
                    {
                        //string RiskfileID = $"{new Random().Next(10):00}-{new Random().Next(10):00}-{new Random().Next(1000):000}-{new Random().Next(10):00}-R{new Random().Next(10000):0000}";
                        string basePath = Path.Combine("Reports", "UniqueDocumentID");

                        string uniqueFolder = Path.Combine(basePath, $"A{123:D5}");
                        Directory.CreateDirectory(uniqueFolder);

                        var filePath = Path.Combine(uniqueFolder, mainFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            mainFile.CopyTo(stream);
                        }

                        // Set the file attachment path in the model
                        riskModel.FileAttachement = filePath;
                        riskModel.FileName = mainFile.FileName;
                        //riskModel.UniqueDocumentID = RiskfileID;
                    }

                    // Log final model values
                    Console.WriteLine("Deserialized Model Values: " + JsonConvert.SerializeObject(riskModel));

                    // Save `riskModel` to the database...

                    int RiskRegisterMasterID;
                    string uniqueDocumentID;
                    string query = @"
                        INSERT INTO risk_riskregistermaster (
                            Entity_Master_id, 
                            Unit_location_Master_id, 
                            Department_Master_id, 
                            riskBusinessfunctionid, 
                            businessprocessID,
                            BusinessProcessL1ID, 
                            BusinessProcessL2ID, 
                            BusinessProcessL3ID, 
                            BusinessSubProcessObjective, 
                            NameofRiskDocumentRegister, 
                            ObjectiveofRiskDocument, 
                            RiskRootCause, 
                            Risk_Admin_typeOfRisk_id, 
                            risk_admin_classification_id, 
                            risk_admin_risk_categorization_id, 
                            risk_admin_causeList_id, 
                            AMLComplianceRisk,ModelRisk,ConductRisk,ITCyberSecurity,ThirdPartyOutsourcing,FraudRisk,
                            LegalRisk,OperationalRisk,ReputationalRisk,FinancialRiskReporting,RiskCostImpact,risk_admin_riskImpactRating_id,
                            risk_admin_likeoccfact_id,InherentRiskRating,activityvalue,RiskPriority,Slidervalue,RiskIntensity,
                            risk_admin_LETC_L1_id,CategoryL1Description,risk_admin_letc_l2_id,CategoryL2Description,risk_admin_LETC_l3_id,
                           CategoryL3Description,risk_admin_potenBussImpactid,PotentialImpactDescription,SuggestivePriventive,
                           RepeatReviewFrequency,EnterValueforrepeat,Selectfrequencyperiod,StartDateNextReview,ReviewfrequencyCheck,UniqueRiskID,
                        UniqueDocumentID,InherentRatingColor,RiskPirorityColor,RiskIntensityColor,BusinessProcessHead,RiskOwnership
                          ,ProcessOwner,RiskStatementID,activityid,CreatedDate,RegisterStatus,RiskDefinition
                        ) 
                        VALUES (
                            @EntityMasterId, 
                            @UnitLocationMasterId, 
                            @Department_Master_id, 
                            @RiskBusinessFunctionId,
                            @businessprocessID,
                            @BusinessProcessL1Id, 
                            @BusinessProcessL2Id, 
                            @BusinessProcessL3Id, 
                            @BusinessSubProcessObjective, 
                            @NameofRiskDocumentRegister, 
                            @ObjectiveofRiskDocument, 
                            @RiskRootCause, 
                            @RiskAdminTypeOfRiskId, 
                            @RiskAdminClassificationId, 
                            @RiskAdminRiskCategorizationId, 
                            @RiskAdminCauseListId, 
                              @AMLComplianceRisk,@ModelRisk,@ConductRisk,@ITCyberSecurity,@ThirdPartyOutsourcing,@FraudRisk,
                            @LegalRisk,@OperationalRisk,@ReputationalRisk,@FinancialRiskReporting,@RiskCostImpact,@risk_admin_riskImpactRating_id,
                            @risk_admin_likeoccfact_id,@InherentRiskRating,@activityvalue,@RiskPriority,@Slidervalue,@RiskIntensity,
                            @risk_admin_LETC_L1_id,@CategoryL1Description,@risk_admin_letc_l2_id,@CategoryL2Description,@risk_admin_LETC_l3_id,
                           @CategoryL3Description,@risk_admin_potenBussImpactid,@PotentialImpactDescription,@SuggestivePriventive,
                           @RepeatReviewFrequency,@EnterValueforrepeat,@Selectfrequencyperiod,@StartDateNextReview,@ReviewfrequencyCheck,@UniqueRiskID,
                        @UniqueDocumentID,@InherentRatingColor,@RiskPirorityColor,@RiskIntensityColor,@BusinessProcessHead,@RiskOwnership
                          ,@ProcessOwner,@RiskStatementID,@activityid,@CreatedDate,@RegisterStatus,@RiskDefinition
                        ); SELECT LAST_INSERT_ID();";

                    using (MySqlCommand myCommand = new MySqlCommand(query, con))
                    {
                        myCommand.Parameters.AddWithValue("@EntityMasterId", riskModel.Entity_Master_id);
                        myCommand.Parameters.AddWithValue("@UnitLocationMasterId", riskModel.Unit_location_Master_id);
                        myCommand.Parameters.AddWithValue("@Department_Master_id", riskModel.department_Master_id);
                        myCommand.Parameters.AddWithValue("@RiskBusinessFunctionId", riskModel.riskBusinessfunctionid);
                        myCommand.Parameters.AddWithValue("@businessprocessID", riskModel.businessprocessID);
                        myCommand.Parameters.AddWithValue("@BusinessProcessL1Id", riskModel.BusinessProcessL1ID);
                        myCommand.Parameters.AddWithValue("@BusinessProcessL2Id", riskModel.BusinessProcessL2ID);
                        myCommand.Parameters.AddWithValue("@BusinessProcessL3Id", riskModel.BusinessProcessL3ID);
                        myCommand.Parameters.AddWithValue("@BusinessSubProcessObjective", riskModel.BusinessSubProcessObjective);
                        myCommand.Parameters.AddWithValue("@NameofRiskDocumentRegister", riskModel.NameofRiskDocumentRegister);
                        myCommand.Parameters.AddWithValue("@ObjectiveofRiskDocument", riskModel.ObjectiveofRiskDocument);
                        myCommand.Parameters.AddWithValue("@RiskRootCause", riskModel.RiskRootCause);
                        myCommand.Parameters.AddWithValue("@RiskAdminTypeOfRiskId", riskModel.Risk_Admin_typeOfRisk_id);
                        myCommand.Parameters.AddWithValue("@RiskAdminClassificationId", riskModel.risk_admin_classification_id);
                        myCommand.Parameters.AddWithValue("@RiskAdminRiskCategorizationId", riskModel.risk_admin_risk_categorization_id);
                        myCommand.Parameters.AddWithValue("@RiskAdminCauseListId", riskModel.risk_admin_causeList_id);
                        myCommand.Parameters.AddWithValue("@AMLComplianceRisk", riskModel.AMLComplianceRisk);
                        myCommand.Parameters.AddWithValue("@ModelRisk", riskModel.ModelRisk);
                        myCommand.Parameters.AddWithValue("@ConductRisk", riskModel.ConductRisk);
                        myCommand.Parameters.AddWithValue("@ITCyberSecurity", riskModel.ITCyberSecurity);
                        myCommand.Parameters.AddWithValue("@ThirdPartyOutsourcing", riskModel.ThirdPartyOutsourcing);
                        myCommand.Parameters.AddWithValue("@FraudRisk", riskModel.FraudRisk);
                        myCommand.Parameters.AddWithValue("@LegalRisk", riskModel.LegalRisk);
                        myCommand.Parameters.AddWithValue("@OperationalRisk", riskModel.OperationalRisk);
                        myCommand.Parameters.AddWithValue("@ReputationalRisk", riskModel.ReputationalRisk);
                        myCommand.Parameters.AddWithValue("@FinancialRiskReporting", riskModel.FinancialRiskReporting);
                        myCommand.Parameters.AddWithValue("@RiskCostImpact", riskModel.RiskCostImpact);
                        myCommand.Parameters.AddWithValue("@risk_admin_riskImpactRating_id", riskModel.risk_admin_riskImpactRating_id);
                        myCommand.Parameters.AddWithValue("@risk_admin_likeoccfact_id", riskModel.risk_admin_likeoccfact_id);
                        myCommand.Parameters.AddWithValue("@InherentRiskRating", riskModel.InherentRiskRating);
                        myCommand.Parameters.AddWithValue("@activityvalue", riskModel.activityvalue);
                        myCommand.Parameters.AddWithValue("@RiskPriority", riskModel.RiskPriority);
                        myCommand.Parameters.AddWithValue("@Slidervalue", riskModel.Slidervalue);
                        myCommand.Parameters.AddWithValue("@RiskIntensity", riskModel.RiskIntensity);
                        myCommand.Parameters.AddWithValue("@risk_admin_LETC_L1_id", riskModel.risk_admin_LETC_L1_id);
                        myCommand.Parameters.AddWithValue("@CategoryL1Description", riskModel.CategoryL1Description);
                        myCommand.Parameters.AddWithValue("@risk_admin_letc_l2_id", riskModel.risk_admin_letc_l2_id);
                        myCommand.Parameters.AddWithValue("@CategoryL2Description", riskModel.CategoryL2Description);
                        myCommand.Parameters.AddWithValue("@risk_admin_LETC_l3_id", riskModel.risk_admin_LETC_l3_id);
                        myCommand.Parameters.AddWithValue("@CategoryL3Description", riskModel.CategoryL3Description);
                        myCommand.Parameters.AddWithValue("@risk_admin_potenBussImpactid", riskModel.risk_admin_potenBussImpactid);
                        myCommand.Parameters.AddWithValue("@PotentialImpactDescription", riskModel.PotentialImpactDescription);
                        myCommand.Parameters.AddWithValue("@SuggestivePriventive", riskModel.SuggestivePriventive);
                        myCommand.Parameters.AddWithValue("@RepeatReviewFrequency", riskModel.RepeatReviewFrequency);
                        myCommand.Parameters.AddWithValue("@EnterValueforrepeat", riskModel.EnterValueforrepeat);
                        myCommand.Parameters.AddWithValue("@Selectfrequencyperiod", riskModel.Selectfrequencyperiod);
                        myCommand.Parameters.AddWithValue("@StartDateNextReview", riskModel.StartDateNextReview);

                        myCommand.Parameters.AddWithValue("@ReviewfrequencyCheck", riskModel.ReviewfrequencyCheck);
                        myCommand.Parameters.AddWithValue("@InherentRatingColor", riskModel.InherentRatingColor);
                        myCommand.Parameters.AddWithValue("@RiskPirorityColor", riskModel.RiskPirorityColor);
                        myCommand.Parameters.AddWithValue("@RiskIntensityColor", riskModel.RiskIntensityColor);


                        myCommand.Parameters.AddWithValue("@RiskOwnership", riskModel.RiskOwnership);
                        myCommand.Parameters.AddWithValue("@ProcessOwner", riskModel.ProcessOwner);
                        myCommand.Parameters.AddWithValue("@BusinessProcessHead", riskModel.BusinessProcessHead);
                        myCommand.Parameters.AddWithValue("@RiskStatementID", riskModel.RiskStatementID);
                        myCommand.Parameters.AddWithValue("@activityid", riskModel.activityid);
                        myCommand.Parameters.AddWithValue("@RiskDefinition", riskModel.RiskDefinition);
                        myCommand.Parameters.AddWithValue("@CreatedDate", System.DateTime.Now);
                        myCommand.Parameters.AddWithValue("@RegisterStatus", "Active");

                        //myCommand.Parameters.AddWithValue("@RiskStatementID", riskStatementId);

                        //int RiskRegisterMasterID = Convert.ToInt32(myCommand.ExecuteScalar());
                        //string UniqueRiskID1 = $"{RiskRegisterMasterID:D20}R";
                        //var StatementIdFloder = Path.Combine("Reports", "RiskStatementDocument", UniqueRiskID1);

                        //DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(StatementIdFloder);
                        //myCommand.Parameters.AddWithValue("@UniqueRiskID", UniqueRiskID1);
                        //List<string> fileList = new List<string>();
                        // Generate Unique Risk ID in the format "00-00-000-00-R9999"

                        //string uniqueRiskId = $"{new Random().Next(10):00}-{new Random().Next(10):00}-{new Random().Next(1000):000}-{new Random().Next(10):00}-R{new Random().Next(10000):0000}";
                        string uniqueRiskId = $"{DateTime.Now:yyMMdd}-UR{new Random().Next(1000, 10000):D4}";
                        myCommand.Parameters.AddWithValue("@UniqueRiskID", uniqueRiskId);

                        myCommand.Parameters.AddWithValue("@FileAttachement", riskModel.FileAttachement);
                        myCommand.Parameters.AddWithValue("@FileName", riskModel.FileName);

                        //uniqueDocumentID = $"{new Random().Next(10):00}-{new Random().Next(10):00}-{new Random().Next(1000):000}-{new Random().Next(10):00}-R{new Random().Next(10000):0000}";
                         uniqueDocumentID = $"{DateTime.Now:yyMMdd}-RD{new Random().Next(1000, 10000):D4}";
                        myCommand.Parameters.AddWithValue("@UniqueDocumentID", uniqueDocumentID);

                        //int UniqueDocumentID = Convert.ToInt32(myCommand.ExecuteScalar());
                        //string statementfileID = $"R{UniqueDocumentID:D5}";
                        //var StatementIdFloder = Path.Combine("Reports", "UniqueDocumentID", statementfileID);

                        //DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(StatementIdFloder);



                        //myCommand.ExecuteNonQuery();
                        RiskRegisterMasterID = Convert.ToInt32(myCommand.ExecuteScalar());
                        string selectQuery1 = @"SELECT * FROM risk_riskregistermaster WHERE NameofRiskDocumentRegister = @NameofRiskDocumentRegister";

                        using (MySqlCommand selectCommand1 = new MySqlCommand(selectQuery1, con))
                        {
                            selectCommand1.Parameters.AddWithValue("@NameofRiskDocumentRegister", riskModel.NameofRiskDocumentRegister);

                            MySqlDataAdapter da1 = new MySqlDataAdapter(selectCommand1);
                            DataTable dt1 = new DataTable();
                            da1.Fill(dt1);
                            if (dt1.Rows.Count > 0)
                            {
                                riskStatmentscount = dt1.Rows.Count;

                            }
                        }
                        int Finish = Convert.ToInt32(riskModel.Finish.ToString());
                        if (Finish == 1)
                        {

                            string selectQuery = @"SELECT * FROM risk_riskregistermaster WHERE NameofRiskDocumentRegister = @NameofRiskDocumentRegister";

                            using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, con))
                            {
                                selectCommand.Parameters.AddWithValue("@NameofRiskDocumentRegister", riskModel.NameofRiskDocumentRegister);

                                MySqlDataAdapter da1 = new MySqlDataAdapter(selectCommand);
                                DataTable dt1 = new DataTable();
                                da1.Fill(dt1);
                                if (dt1.Rows.Count > 0)
                                {
                                    riskStatmentscount = dt1.Rows.Count;
                                    for (var i = 0; i < dt1.Rows.Count; i++)
                                    {
                                        int RiskRegisterMasterID1 = Convert.ToInt32(dt1.Rows[i]["RiskRegisterMasterID"].ToString());
                                        string updatestatus = @"
            UPDATE risk_riskregistermaster 
            SET 
                docTypeID = @docTypeID,
                doc_CategoryID = @doc_CategoryID,
                doc_SubCategoryID = @doc_SubCategoryID,
                DocumentEffectiveDate = @DocumentEffectiveDate,
                DocumentConfidentiality = @DocumentConfidentiality,
                OtpMethod = @OtpMethod,
                natureOf_Doc_id = @natureOf_Doc_id,
                InternalReferenceNo = @InternalReferenceNo,
                PhysicalVaultLocation = @PhysicalVaultLocation,
                risk_admin_RiskAppetiteId = @risk_admin_RiskAppetiteId,
                AppetiteStatement = @AppetiteStatement,
                PublishingRemarks = @PublishingRemarks,
                Keywords = @Keywords,
                FileAttachement = @FileAttachement,
                FileName = @FileName,
                DocumentApprover = @DocumentApprover,
                BusinessFunctionHead=@BusinessFunctionHead ,
                   NoOfRiskStatements=@NoOfRiskStatements

            WHERE RiskRegisterMasterID = @RiskRegisterMasterID";

                                        using (MySqlCommand updateCommand = new MySqlCommand(updatestatus, con))
                                        {

                                            updateCommand.Parameters.AddWithValue("@docTypeID", riskModel.docTypeID);
                                            updateCommand.Parameters.AddWithValue("@doc_CategoryID", riskModel.doc_CategoryID);
                                            updateCommand.Parameters.AddWithValue("@doc_SubCategoryID", riskModel.doc_SubCategoryID);
                                            updateCommand.Parameters.AddWithValue("@DocumentEffectiveDate", riskModel.DocumentEffectiveDate);
                                            updateCommand.Parameters.AddWithValue("@DocumentConfidentiality", riskModel.DocumentConfidentiality);
                                            updateCommand.Parameters.AddWithValue("@OtpMethod", riskModel.OtpMethod);
                                            updateCommand.Parameters.AddWithValue("@natureOf_Doc_id", riskModel.natureOf_Doc_id);
                                            updateCommand.Parameters.AddWithValue("@InternalReferenceNo", riskModel.InternalReferenceNo);
                                            updateCommand.Parameters.AddWithValue("@PhysicalVaultLocation", riskModel.PhysicalVaultLocation);
                                            updateCommand.Parameters.AddWithValue("@risk_admin_RiskAppetiteId", riskModel.risk_admin_RiskAppetiteId);
                                            updateCommand.Parameters.AddWithValue("@AppetiteStatement", riskModel.AppetiteStatement);
                                            updateCommand.Parameters.AddWithValue("@PublishingRemarks", riskModel.PublishingRemarks);
                                            updateCommand.Parameters.AddWithValue("@Keywords", riskModel.Keywords);
                                            updateCommand.Parameters.AddWithValue("@FileAttachement", riskModel.FileAttachement);
                                            updateCommand.Parameters.AddWithValue("@FileName", riskModel.FileName);
                                            updateCommand.Parameters.AddWithValue("@DocumentApprover", riskModel.DocumentApprover);
                                            updateCommand.Parameters.AddWithValue("@BusinessFunctionHead", riskModel.BusinessFunctionHead);
                                            updateCommand.Parameters.AddWithValue("@NoOfRiskStatements", riskModel.NoOfRiskStatements);
                                            updateCommand.Parameters.AddWithValue("@RiskRegisterMasterID", RiskRegisterMasterID1);  // The same ID for updating

                                            // Execute the update query
                                            updateCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                {
                                    // Return a response if no rows are found in the select query
                                    return Ok("No matching record found for the provided RiskRegisterMasterID.");
                                }
                            }
                        }

                        //return Ok("Inserted successfully.");

                        return Ok(new { message = "Inserted successfully", RiskRegisterMasterID = RiskRegisterMasterID, UniqueDocumentID = uniqueDocumentID, StartDateNextReview = riskModel.StartDateNextReview, riskStatmentscount = riskStatmentscount });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        public IActionResult InsertRiskRegister1([FromForm] Risk_AddRegisterModel riskModel)
        {
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {

                try
                {

                    var form = HttpContext.Request.Form;
                    var jsonPayload = form["jsonPayload"];
                    var mainFile = form.Files.FirstOrDefault();

                    if (!string.IsNullOrEmpty(jsonPayload))
                    {
                        riskModel = JsonConvert.DeserializeObject<Risk_AddRegisterModel>(jsonPayload);
                    }

                    con.Open();

                    var request = HttpContext.Request;


                    if (mainFile != null)
                    {
                        string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
                        string RiskfileID = $"{new Random().Next(10):00}-{new Random().Next(10):00}-{new Random().Next(1000):000}-{new Random().Next(10):00}-R{new Random().Next(10000):0000}";
                        var UniqueIdFolder = Path.Combine("Reports", "UniqueDocumentID", RiskfileID);
                        Directory.CreateDirectory(UniqueIdFolder);

                        // Define file path and URL for saving and referencing
                        var filePath = Path.Combine(UniqueIdFolder, mainFile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            mainFile.CopyTo(stream);
                        }

                        // Construct the file URL
                        string fileUrl = $"{baseUrl}/Reports/UniqueDocumentID/{RiskfileID}/{mainFile.FileName}";



                        string query = @"
            INSERT INTO risk_riskregistermaster (
                Entity_Master_id, 
                Unit_location_Master_id, 
                Department_Master_id, 
                riskBusinessfunctionid, 
                BusinessProcessL1ID, 
                BusinessProcessL2ID, 
                BusinessProcessL3ID, 
                BusinessSubProcessObjective, 
                NameofRiskDocumentRegister, 
                ObjectiveofRiskDocument, 
                RiskRootCause, 
                Risk_Admin_typeOfRisk_id, 
                risk_admin_classification_id, 
                risk_admin_risk_categorization_id, 
                risk_admin_causeList_id, 
                AMLComplianceRisk,ModelRisk,ConductRisk,ITCyberSecurity,ThirdPartyOutsourcing,FraudRisk,
                LegalRisk,OperationalRisk,ReputationalRisk,FinancialRiskReporting,RiskCostImpact,risk_admin_riskImpactRating_id,
                risk_admin_likeoccfact_id,InherentRiskRating,activityvalue,RiskPriority,Slidervalue,RiskIntensity,
                risk_admin_LETC_L1_id,CategoryL1Description,risk_admin_letc_l2_id,CategoryL2Description,risk_admin_LETC_l3_id,
               CategoryL3Description,risk_admin_potenBussImpactid,PotentialImpactDescription,SuggestivePriventive,
               RepeatReviewFrequency,EnterValueforrepeat,Selectfrequencyperiod,StartDateNextReview,
               NameOfRisRegister,NoOfRiskStatements,docTypeID,doc_CategoryID,doc_SubCategoryID,DocumentEffectiveDate,
               DocumentConfidentiality,OtpMethod,natureOf_Doc_id,InternalReferenceNo,PhysicalVaultLocation,
               risk_admin_RiskAppetiteId,AppetiteStatement,PublishingRemarks,Keywords,ReviewfrequencyCheck,UniqueRiskID,activityid
            ) 
            VALUES (
                @EntityMasterId, 
                @UnitLocationMasterId, 
                @Department_Master_id, 
                @RiskBusinessFunctionId, 
                @BusinessProcessL1Id, 
                @BusinessProcessL2Id, 
                @BusinessProcessL3Id, 
                @BusinessSubProcessObjective, 
                @NameofRiskDocumentRegister, 
                @ObjectiveofRiskDocument, 
                @RiskRootCause, 
                @RiskAdminTypeOfRiskId, 
                @RiskAdminClassificationId, 
                @RiskAdminRiskCategorizationId, 
                @RiskAdminCauseListId, 
                 @AMLComplianceRisk,@ModelRisk,ConductRisk,@ITCyberSecurity,@ThirdPartyOutsourcing,@FraudRisk,
                @LegalRisk,@OperationalRisk,@ReputationalRisk,@FinancialRiskReporting,@RiskCostImpact,@risk_admin_riskImpactRating_id,
                @risk_admin_likeoccfact_id,@InherentRiskRating,@activityvalue,@RiskPriority,@Slidervalue,@RiskIntensity,
                @risk_admin_LETC_L1_id,@CategoryL1Description,@risk_admin_letc_l2_id,@CategoryL2Description,@risk_admin_LETC_l3_id,
               @CategoryL3Description,@risk_admin_potenBussImpactid,@PotentialImpactDescription,@SuggestivePriventive,
               @RepeatReviewFrequency,@EnterValueforrepeat,@Selectfrequencyperiod,@StartDateNextReview,
               @NameOfRisRegister,@NoOfRiskStatements,@docTypeID,@doc_CategoryID,@doc_SubCategoryID,@DocumentEffectiveDate,
               @DocumentConfidentiality,@OtpMethod,@natureOf_Doc_id,@InternalReferenceNo,@PhysicalVaultLocation,
               @risk_admin_RiskAppetiteId,@AppetiteStatement,@PublishingRemarks,@Keywords,@ReviewfrequencyCheck,@UniqueRiskID,@activityid
            )";

                        using (MySqlCommand myCommand = new MySqlCommand(query, con))
                        {
                            myCommand.Parameters.AddWithValue("@EntityMasterId", riskModel.Entity_Master_id);
                            myCommand.Parameters.AddWithValue("@UnitLocationMasterId", riskModel.Unit_location_Master_id);
                            myCommand.Parameters.AddWithValue("@Department_Master_id", riskModel.department_Master_id);
                            myCommand.Parameters.AddWithValue("@RiskBusinessFunctionId", riskModel.riskBusinessfunctionid);
                            myCommand.Parameters.AddWithValue("@BusinessProcessL1Id", riskModel.BusinessProcessL1ID);
                            myCommand.Parameters.AddWithValue("@BusinessProcessL2Id", riskModel.BusinessProcessL2ID);
                            myCommand.Parameters.AddWithValue("@BusinessProcessL3Id", riskModel.BusinessProcessL3ID);
                            myCommand.Parameters.AddWithValue("@BusinessSubProcessObjective", riskModel.BusinessSubProcessObjective);
                            myCommand.Parameters.AddWithValue("@NameofRiskDocumentRegister", riskModel.NameofRiskDocumentRegister);
                            myCommand.Parameters.AddWithValue("@ObjectiveofRiskDocument", riskModel.ObjectiveofRiskDocument);
                            myCommand.Parameters.AddWithValue("@RiskRootCause", riskModel.RiskRootCause);
                            myCommand.Parameters.AddWithValue("@RiskAdminTypeOfRiskId", riskModel.Risk_Admin_typeOfRisk_id);
                            myCommand.Parameters.AddWithValue("@RiskAdminClassificationId", riskModel.risk_admin_classification_id);
                            myCommand.Parameters.AddWithValue("@RiskAdminRiskCategorizationId", riskModel.risk_admin_risk_categorization_id);
                            myCommand.Parameters.AddWithValue("@RiskAdminCauseListId", riskModel.risk_admin_causeList_id);
                            myCommand.Parameters.AddWithValue("@AMLComplianceRisk", riskModel.AMLComplianceRisk);
                            myCommand.Parameters.AddWithValue("@ModelRisk", riskModel.ModelRisk);
                            myCommand.Parameters.AddWithValue("@ConductRisk", riskModel.ConductRisk);
                            myCommand.Parameters.AddWithValue("@ITCyberSecurity", riskModel.ITCyberSecurity);
                            myCommand.Parameters.AddWithValue("@ThirdPartyOutsourcing", riskModel.ThirdPartyOutsourcing);
                            myCommand.Parameters.AddWithValue("@FraudRisk", riskModel.FraudRisk);
                            myCommand.Parameters.AddWithValue("@LegalRisk", riskModel.LegalRisk);
                            myCommand.Parameters.AddWithValue("@OperationalRisk", riskModel.OperationalRisk);
                            myCommand.Parameters.AddWithValue("@ReputationalRisk", riskModel.ReputationalRisk);
                            myCommand.Parameters.AddWithValue("@FinancialRiskReporting", riskModel.FinancialRiskReporting);
                            myCommand.Parameters.AddWithValue("@RiskCostImpact", riskModel.RiskCostImpact);
                            myCommand.Parameters.AddWithValue("@risk_admin_riskImpactRating_id", riskModel.risk_admin_riskImpactRating_id);
                            myCommand.Parameters.AddWithValue("@risk_admin_likeoccfact_id", riskModel.risk_admin_likeoccfact_id);
                            myCommand.Parameters.AddWithValue("@InherentRiskRating", riskModel.InherentRiskRating);
                            myCommand.Parameters.AddWithValue("@activityvalue", riskModel.activityvalue);
                            myCommand.Parameters.AddWithValue("@RiskPriority", riskModel.RiskPriority);
                            myCommand.Parameters.AddWithValue("@Slidervalue", riskModel.Slidervalue);
                            myCommand.Parameters.AddWithValue("@RiskIntensity", riskModel.RiskIntensity);
                            myCommand.Parameters.AddWithValue("@risk_admin_LETC_L1_id", riskModel.risk_admin_LETC_L1_id);
                            myCommand.Parameters.AddWithValue("@CategoryL1Description", riskModel.CategoryL1Description);
                            myCommand.Parameters.AddWithValue("@risk_admin_letc_l2_id", riskModel.risk_admin_letc_l2_id);
                            myCommand.Parameters.AddWithValue("@CategoryL2Description", riskModel.CategoryL2Description);
                            myCommand.Parameters.AddWithValue("@risk_admin_LETC_l3_id", riskModel.risk_admin_LETC_l3_id);
                            myCommand.Parameters.AddWithValue("@CategoryL3Description", riskModel.CategoryL3Description);
                            myCommand.Parameters.AddWithValue("@risk_admin_potenBussImpactid", riskModel.risk_admin_potenBussImpactid);
                            myCommand.Parameters.AddWithValue("@PotentialImpactDescription", riskModel.PotentialImpactDescription);
                            myCommand.Parameters.AddWithValue("@SuggestivePriventive", riskModel.SuggestivePriventive);
                            myCommand.Parameters.AddWithValue("@RepeatReviewFrequency", riskModel.RepeatReviewFrequency);
                            myCommand.Parameters.AddWithValue("@EnterValueforrepeat", riskModel.EnterValueforrepeat);
                            myCommand.Parameters.AddWithValue("@Selectfrequencyperiod", riskModel.Selectfrequencyperiod);
                            myCommand.Parameters.AddWithValue("@StartDateNextReview", riskModel.StartDateNextReview);
                            myCommand.Parameters.AddWithValue("@NameOfRisRegister", riskModel.NameOfRisRegister);
                            myCommand.Parameters.AddWithValue("@NoOfRiskStatements", riskModel.NoOfRiskStatements);
                            myCommand.Parameters.AddWithValue("@docTypeID", riskModel.docTypeID);
                            myCommand.Parameters.AddWithValue("@doc_CategoryID", riskModel.doc_CategoryID);
                            myCommand.Parameters.AddWithValue("@doc_SubCategoryID", riskModel.doc_SubCategoryID);
                            myCommand.Parameters.AddWithValue("@DocumentEffectiveDate", riskModel.DocumentEffectiveDate);
                            myCommand.Parameters.AddWithValue("@DocumentConfidentiality", riskModel.DocumentConfidentiality);
                            myCommand.Parameters.AddWithValue("@OtpMethod", riskModel.OtpMethod);
                            myCommand.Parameters.AddWithValue("@natureOf_Doc_id", riskModel.natureOf_Doc_id);
                            myCommand.Parameters.AddWithValue("@InternalReferenceNo", riskModel.InternalReferenceNo);
                            myCommand.Parameters.AddWithValue("@PhysicalVaultLocation", riskModel.PhysicalVaultLocation);
                            myCommand.Parameters.AddWithValue("@risk_admin_RiskAppetiteId", riskModel.risk_admin_RiskAppetiteId);
                            myCommand.Parameters.AddWithValue("@AppetiteStatement", riskModel.AppetiteStatement);
                            myCommand.Parameters.AddWithValue("@PublishingRemarks", riskModel.PublishingRemarks);
                            myCommand.Parameters.AddWithValue("@Keywords", riskModel.Keywords);
                            myCommand.Parameters.AddWithValue("@ReviewfrequencyCheck", riskModel.ReviewfrequencyCheck);
                            myCommand.Parameters.AddWithValue("@activityid", riskModel.activityid);

                            //myCommand.Parameters.AddWithValue("@RiskStatementID", riskStatementId);

                            //int RiskRegisterMasterID = Convert.ToInt32(myCommand.ExecuteScalar());
                            //string UniqueRiskID1 = $"{RiskRegisterMasterID:D20}R";
                            //var StatementIdFloder = Path.Combine("Reports", "RiskStatementDocument", UniqueRiskID1);

                            //DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(StatementIdFloder);
                            //myCommand.Parameters.AddWithValue("@UniqueRiskID", UniqueRiskID1);
                            //List<string> fileList = new List<string>();
                            // Generate Unique Risk ID in the format "00-00-000-00-R9999"

                            string uniqueRiskId = $"{new Random().Next(10):00}-{new Random().Next(10):00}-{new Random().Next(1000):000}-{new Random().Next(10):00}-R{new Random().Next(10000):0000}";
                            myCommand.Parameters.AddWithValue("@UniqueRiskID", uniqueRiskId);

                            myCommand.Parameters.AddWithValue("@FileAttachment", fileUrl);





                            myCommand.ExecuteNonQuery();

                        }
                    }

                    return Ok("Inserted successfully.");
                }

                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/RiskRegister/UpdateRiskRegisterDetails")]
        [HttpPost]
        public IActionResult UpdateRiskRegisterDetails([FromBody] Risk_AddRegisterModel Risk_AddRegisterModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
          
            try
            {
                con.Open();
                string UpdateQuery = @"
            UPDATE risk_riskregistermaster SET
                Entity_Master_id = @Entity_Master_id,
                Unit_location_Master_id = @Unit_location_Master_id,
                Department_Master_id = @Department_Master_id,
                riskBusinessfunctionid = @riskBusinessfunctionid,
                businessprocessID = @businessprocessID,
                BusinessProcessL1ID = @BusinessProcessL1ID,
                BusinessProcessL2ID = @BusinessProcessL2ID,
                BusinessProcessL3ID = @BusinessProcessL3ID,
                BusinessSubProcessObjective = @BusinessSubProcessObjective,
                NameofRiskDocumentRegister = @NameofRiskDocumentRegister,
                ObjectiveofRiskDocument = @ObjectiveofRiskDocument,
                RiskRootCause = @RiskRootCause,
                Risk_Admin_typeOfRisk_id = @Risk_Admin_typeOfRisk_id,
                risk_admin_classification_id = @risk_admin_classification_id,
                risk_admin_risk_categorization_id = @risk_admin_risk_categorization_id,
                risk_admin_causeList_id = @risk_admin_causeList_id,
                AMLComplianceRisk = @AMLComplianceRisk,
                ModelRisk = @ModelRisk,
                ConductRisk = @ConductRisk,
                ITCyberSecurity = @ITCyberSecurity,
                ThirdPartyOutsourcing = @ThirdPartyOutsourcing,
                FraudRisk = @FraudRisk,
                LegalRisk = @LegalRisk,
                OperationalRisk = @OperationalRisk,
                ReputationalRisk = @ReputationalRisk,
                FinancialRiskReporting = @FinancialRiskReporting,
                RiskCostImpact = @RiskCostImpact,
                risk_admin_riskImpactRating_id=@risk_admin_riskImpactRating_id,
                risk_admin_likeoccfact_id=@risk_admin_likeoccfact_id,
                InherentRiskRating =@InherentRiskRating,
activityid=@activityid,
RiskPriority=@RiskPriority,
Slidervalue=@Slidervalue,
RiskIntensity=@RiskIntensity,
RiskOwnership=@RiskOwnership,
ProcessOwner=@ProcessOwner,
BusinessFunctionHead=@BusinessFunctionHead,
ReviewfrequencyCheck=@ReviewfrequencyCheck,
RepeatReviewFrequency=@RepeatReviewFrequency,
EnterValueforrepeat=@EnterValueforrepeat,
Selectfrequencyperiod=@Selectfrequencyperiod,
StartDateNextReview=@StartDateNextReview,
                risk_admin_LETC_L1_id = @risk_admin_LETC_L1_id,
                CategoryL1Description = @CategoryL1Description,
                risk_admin_letc_l2_id = @risk_admin_letc_l2_id,
                CategoryL2Description = @CategoryL2Description,
                risk_admin_LETC_l3_id = @risk_admin_LETC_l3_id,
                CategoryL3Description = @CategoryL3Description,
                risk_admin_potenBussImpactid = @risk_admin_potenBussImpactid,
                PotentialImpactDescription = @PotentialImpactDescription,
                SuggestivePriventive = @SuggestivePriventive,
                docTypeID = @docTypeID,
                doc_CategoryID = @doc_CategoryID,
                doc_SubCategoryID = @doc_SubCategoryID,
DocumentEffectiveDate=@DocumentEffectiveDate,
DocumentConfidentiality=@DocumentConfidentiality,
OtpMethod=@OtpMethod,
natureOf_Doc_id=@natureOf_Doc_id,
InternalReferenceNo=@InternalReferenceNo,
PhysicalVaultLocation=@PhysicalVaultLocation,
BusinessProcessHead=@BusinessProcessHead,
risk_admin_RiskAppetiteId=@risk_admin_RiskAppetiteId,
AppetiteStatement=@AppetiteStatement,
PublishingRemarks=@PublishingRemarks,
Keywords=@Keywords,
                LastEditedon=@LastEditedon
                
            WHERE RiskRegisterMasterID = @RiskRegisterMasterID";

                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@RiskRegisterMasterID", Risk_AddRegisterModels.RiskRegisterMasterID);
                    myCommand.Parameters.AddWithValue("@Entity_Master_id", Risk_AddRegisterModels.Entity_Master_id);
                    myCommand.Parameters.AddWithValue("@Unit_location_Master_id", Risk_AddRegisterModels.Unit_location_Master_id);
                    myCommand.Parameters.AddWithValue("@Department_Master_id", Risk_AddRegisterModels.department_Master_id);
                    myCommand.Parameters.AddWithValue("@riskBusinessfunctionid", Risk_AddRegisterModels.riskBusinessfunctionid);
                    myCommand.Parameters.AddWithValue("@businessprocessID", Risk_AddRegisterModels.businessprocessID);
                    myCommand.Parameters.AddWithValue("@BusinessProcessL1ID", Risk_AddRegisterModels.BusinessProcessL1ID);
                    myCommand.Parameters.AddWithValue("@BusinessProcessL2ID", Risk_AddRegisterModels.BusinessProcessL2ID);
                    myCommand.Parameters.AddWithValue("@BusinessProcessL3ID", Risk_AddRegisterModels.BusinessProcessL3ID);
                    myCommand.Parameters.AddWithValue("@BusinessSubProcessObjective", Risk_AddRegisterModels.BusinessSubProcessObjective);
                    myCommand.Parameters.AddWithValue("@NameofRiskDocumentRegister", Risk_AddRegisterModels.NameofRiskDocumentRegister);
                     myCommand.Parameters.AddWithValue("@ObjectiveofRiskDocument", Risk_AddRegisterModels.ObjectiveofRiskDocument);
                    //Attributes Stepper
                    myCommand.Parameters.AddWithValue("@RiskRootCause", Risk_AddRegisterModels.RiskRootCause);
                    myCommand.Parameters.AddWithValue("@Risk_Admin_typeOfRisk_id", Risk_AddRegisterModels.Risk_Admin_typeOfRisk_id);
                    myCommand.Parameters.AddWithValue("@risk_admin_classification_id", Risk_AddRegisterModels.risk_admin_classification_id);
                    myCommand.Parameters.AddWithValue("@risk_admin_risk_categorization_id", Risk_AddRegisterModels.risk_admin_risk_categorization_id);
                    myCommand.Parameters.AddWithValue("@risk_admin_causeList_id", Risk_AddRegisterModels.risk_admin_causeList_id);
                    myCommand.Parameters.AddWithValue("@AMLComplianceRisk", Risk_AddRegisterModels.AMLComplianceRisk);
                    myCommand.Parameters.AddWithValue("@ModelRisk", Risk_AddRegisterModels.ModelRisk);
                    myCommand.Parameters.AddWithValue("@ConductRisk", Risk_AddRegisterModels.ConductRisk);
                    myCommand.Parameters.AddWithValue("@ITCyberSecurity", Risk_AddRegisterModels.ITCyberSecurity);
                    myCommand.Parameters.AddWithValue("@ThirdPartyOutsourcing", Risk_AddRegisterModels.ThirdPartyOutsourcing);
                    myCommand.Parameters.AddWithValue("@FraudRisk", Risk_AddRegisterModels.FraudRisk);
                    myCommand.Parameters.AddWithValue("@LegalRisk", Risk_AddRegisterModels.LegalRisk);
                    myCommand.Parameters.AddWithValue("@OperationalRisk", Risk_AddRegisterModels.OperationalRisk);
                    myCommand.Parameters.AddWithValue("@ReputationalRisk", Risk_AddRegisterModels.ReputationalRisk);
                    myCommand.Parameters.AddWithValue("@FinancialRiskReporting", Risk_AddRegisterModels.FinancialRiskReporting);
                    myCommand.Parameters.AddWithValue("@RiskCostImpact", Risk_AddRegisterModels.RiskCostImpact);
                    myCommand.Parameters.AddWithValue("@risk_admin_riskImpactRating_id", Risk_AddRegisterModels.risk_admin_riskImpactRating_id);
                    myCommand.Parameters.AddWithValue("@risk_admin_likeoccfact_id", Risk_AddRegisterModels.risk_admin_likeoccfact_id);
                    myCommand.Parameters.AddWithValue("@InherentRiskRating", Risk_AddRegisterModels.InherentRiskRating);
                    //myCommand.Parameters.AddWithValue("@InherentRatingColor", Risk_AddRegisterModels.InherentRatingColor);
                    myCommand.Parameters.AddWithValue("@activityid", Risk_AddRegisterModels.activityid);
                    myCommand.Parameters.AddWithValue("@RiskPriority", Risk_AddRegisterModels.RiskPriority);
                    myCommand.Parameters.AddWithValue("@Slidervalue", Risk_AddRegisterModels.Slidervalue);
                    myCommand.Parameters.AddWithValue("@RiskIntensity", Risk_AddRegisterModels.RiskIntensity);

                    //Consequences
                    myCommand.Parameters.AddWithValue("@risk_admin_LETC_L1_id", Risk_AddRegisterModels.risk_admin_LETC_L1_id);
                    myCommand.Parameters.AddWithValue("@CategoryL1Description", Risk_AddRegisterModels.CategoryL1Description);
                    myCommand.Parameters.AddWithValue("@risk_admin_letc_l2_id", Risk_AddRegisterModels.risk_admin_letc_l2_id);
                    myCommand.Parameters.AddWithValue("@CategoryL2Description", Risk_AddRegisterModels.CategoryL2Description);
                    myCommand.Parameters.AddWithValue("@risk_admin_LETC_l3_id", Risk_AddRegisterModels.risk_admin_LETC_l3_id);
                    myCommand.Parameters.AddWithValue("@CategoryL3Description", Risk_AddRegisterModels.CategoryL3Description);
                    myCommand.Parameters.AddWithValue("@risk_admin_potenBussImpactid", Risk_AddRegisterModels.risk_admin_potenBussImpactid);
                    myCommand.Parameters.AddWithValue("@PotentialImpactDescription", Risk_AddRegisterModels.PotentialImpactDescription);
                    myCommand.Parameters.AddWithValue("@SuggestivePriventive", Risk_AddRegisterModels.SuggestivePriventive);
                    //Ownership
                    myCommand.Parameters.AddWithValue("@RiskOwnership", Risk_AddRegisterModels.RiskOwnership);
                    myCommand.Parameters.AddWithValue("@ProcessOwner", Risk_AddRegisterModels.ProcessOwner);
                    myCommand.Parameters.AddWithValue("@BusinessFunctionHead", Risk_AddRegisterModels.BusinessFunctionHead);
                    //Review
                    myCommand.Parameters.AddWithValue("@ReviewfrequencyCheck", Risk_AddRegisterModels.ReviewfrequencyCheck);
                    myCommand.Parameters.AddWithValue("@RepeatReviewFrequency", Risk_AddRegisterModels.RepeatReviewFrequency);
                    myCommand.Parameters.AddWithValue("@EnterValueforrepeat", Risk_AddRegisterModels.EnterValueforrepeat);
                    myCommand.Parameters.AddWithValue("@Selectfrequencyperiod", Risk_AddRegisterModels.Selectfrequencyperiod);
                    myCommand.Parameters.AddWithValue("@StartDateNextReview", Risk_AddRegisterModels.StartDateNextReview);
                    //Statement
                    myCommand.Parameters.AddWithValue("@docTypeID", Risk_AddRegisterModels.docTypeID);
                    myCommand.Parameters.AddWithValue("@doc_CategoryID", Risk_AddRegisterModels.doc_CategoryID);
                    myCommand.Parameters.AddWithValue("@doc_SubCategoryID", Risk_AddRegisterModels.doc_SubCategoryID);
                    myCommand.Parameters.AddWithValue("@DocumentEffectiveDate", Risk_AddRegisterModels.DocumentEffectiveDate);
                    myCommand.Parameters.AddWithValue("@DocumentConfidentiality", Risk_AddRegisterModels.DocumentConfidentiality);
                    myCommand.Parameters.AddWithValue("@OtpMethod", Risk_AddRegisterModels.OtpMethod);
                    myCommand.Parameters.AddWithValue("@natureOf_Doc_id", Risk_AddRegisterModels.natureOf_Doc_id);
                    myCommand.Parameters.AddWithValue("@InternalReferenceNo", Risk_AddRegisterModels.InternalReferenceNo);
                    myCommand.Parameters.AddWithValue("@PhysicalVaultLocation", Risk_AddRegisterModels.PhysicalVaultLocation);
                    myCommand.Parameters.AddWithValue("@BusinessProcessHead", Risk_AddRegisterModels.BusinessProcessHead);
                    myCommand.Parameters.AddWithValue("@risk_admin_RiskAppetiteId", Risk_AddRegisterModels.risk_admin_RiskAppetiteId);
                    myCommand.Parameters.AddWithValue("@AppetiteStatement", Risk_AddRegisterModels.AppetiteStatement);
                    myCommand.Parameters.AddWithValue("@PublishingRemarks", Risk_AddRegisterModels.PublishingRemarks);
                    myCommand.Parameters.AddWithValue("@Keywords", Risk_AddRegisterModels.Keywords);
                    myCommand.Parameters.AddWithValue("@LastEditedon", System.DateTime.Now);
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

        [Route("api/EditRiskStatement/UpdateRiskStatement")]
        [HttpPost]
        public async Task<IActionResult> UpdateRiskStatement()
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
                //var statementid = form["RiskStatementID"].FirstOrDefault();
                var weblinks = form["weblink"].ToString();
                var TextField = form["Text"].ToString();

                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                string UpdateQuery = @"UPDATE risk_risk_statement SET RiskStatementName=@RiskStatementName, RiskDescription=@RiskDescription WHERE RiskStatementID = @RiskStatementID";

                MySqlCommand masterCommand = new MySqlCommand(UpdateQuery, con);
                masterCommand.Parameters.AddWithValue("@RiskStatementName", RiskStatementName);
                masterCommand.Parameters.AddWithValue("@RiskDescription", RiskDescription);
                //masterCommand.Parameters.AddWithValue("@RiskStatementID", statementid);

                //int RiskStatementID = Convert.ToInt32(masterCommand.ExecuteScalar());
                //string statementfileID = $"R{RiskStatementID:D5}";
                //var StatementIdFloder = Path.Combine("Reports", "RiskDocument", statementfileID);

                //DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(StatementIdFloder);
                //List<string> fileList = new List<string>();

                //foreach (var file in mainFile)
                //{
                //    var filePath = Path.Combine(StatementIdFloder, file.FileName);
                //    using (var stream = new FileStream(filePath, FileMode.Create))
                //    {
                //        await file.CopyToAsync(stream);
                //    }
                //    fileList.Add(filePath);

                //    InsertFile("FileAttach", $"{baseUrl}/Reports/RiskDocument/{statementfileID}/{file.FileName}", file, RiskStatementID, file.FileName);
                //}

                //if (!string.IsNullOrEmpty(weblinks))
                //{

                //    string[] webLinksArray = weblinks.Split(';');

                //    foreach (var weblink in webLinksArray)
                //    {

                //        InsertFile("Weblink", weblink.Trim(), null, RiskStatementID, null);
                //    }
                //}
                //if (!string.IsNullOrEmpty(TextField))
                //{

                //    string[] TextArray = TextField.Split(';');

                //    foreach (var Text in TextArray)
                //    {

                //        InsertFile("Text", TextField.Trim(), null, RiskStatementID, null);
                //    }
                //}


                void InsertFile(string filecategory, string filepath, IFormFile file, int RiskStatementID, string FileName)
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

                    string updateSubTableQuery = @"UPDATE risk_risk_statement SET FileCategory=@FileCategory, FilePath=@FilePath,FileName=@FileName WHERE RiskStatementID = @RiskStatementID";

                    MySqlCommand subTableCommand = new MySqlCommand(updateSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@FileCategory", fileUploadModel.FileCategory);
                    subTableCommand.Parameters.AddWithValue("@FilePath", fileUploadModel.FilePath);
                    subTableCommand.Parameters.AddWithValue("@FileName", fileUploadModel.FileName);
                    
                    


                    subTableCommand.ExecuteNonQuery();

                }

                // return Ok();
                return Ok(new { message = "Inserted successfully"});

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex}");
            }
        }

        //public IActionResult UpdateRiskStatement([FromBody] Risk_RiskStatement Risk_RiskStatements)
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

        //    try
        //    {
        //        con.Open();
        //        string UpdateQuery = @" UPDATE risk_risk_statement SET
        //       RiskStatementName=@RiskStatementName,
        //        RiskDescription=@RiskDescription


        //    WHERE RiskStatementID = @RiskStatementID";

        //        using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
        //        {
        //            myCommand.Parameters.AddWithValue("@RiskStatementName", Risk_RiskStatements.RiskStatementName);
        //            myCommand.Parameters.AddWithValue("@RiskDescription", Risk_RiskStatements.RiskDescription);
        //            myCommand.Parameters.AddWithValue("@LastEditedon", System.DateTime.Now);
        //            myCommand.ExecuteNonQuery();
        //        }
        //        return Ok("Updated Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //    finally
        //    {
        //        con.Close();
        //    }
        //}

        [Route("api/Riskuserlocationmapping/GetRiskuserlocationmapping")]
        [HttpGet]
        public IEnumerable<object> GetRiskuserlocationmapping(int EntityMasterid, int Unitid)
        {
            var details = (from user_location_mapping in mySqlDBContext.userlocationmappingModels
                           join tbluser in mySqlDBContext.usermodels on user_location_mapping.USR_ID equals tbluser.USR_ID
                           where user_location_mapping.user_location_mapping_status == "Active" && user_location_mapping.Entity_Master_id == EntityMasterid
                           && user_location_mapping.Unit_location_Master_id == Unitid
                           select new
                           {
                               user_location_mapping.user_location_mapping_id,
                               user_location_mapping.Entity_Master_id,
                               user_location_mapping.Unit_location_Master_id,
                               user_location_mapping.USR_ID,
                               tbluser.firstname,

                           })
                            .Distinct()
                .ToList();


            return details;
        }
        [Route("api/AddRiskRegister/GetAddRiskRegisterDetails")]
        [HttpGet]
        public IEnumerable<object> GetAddRiskRegisterDetails()
        {
            //    var details = (from riskregister in mySqlDBContext.Risk_AddRegisterModels
            //                       //join Business in mySqlDBContext.RiskBusinessFunctionModels on riskregister.riskBusinessfunctionid equals Business.riskBusinessfunctionid
            //                       //join classification in mySqlDBContext.riskAdminClassifications on riskregister.risk_admin_classification_id equals classification.risk_admin_classification_id
            //                       //join entitymaster in mySqlDBContext.UnitMasterModels on riskregister.Entity_Master_id equals entitymaster.Entity_Master_id
            //                       //join unitlocation in mySqlDBContext.UnitLocationMasterModels on riskregister.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
            //                       //join departmentmaster in mySqlDBContext.DepartmentModels on riskregister.Department_Master_id equals departmentmaster.Department_Master_id
            //                       //join inherentrating in mySqlDBContext.risk_admin_inherriskratinglevl on riskregister.InherentRiskRating equals inherentrating.risk_admin_inherRiskRatingLevlid
            //                       //join tbluser in mySqlDBContext.usermodels on riskregister.BusinessProcessHead equals tbluser.USR_ID
            //                       // join Classification in mySqlDBContext.NatureOf_DocumentMasterModels on riskregister.natureOf_Doc_id equals Classification.NatureOf_Doc_id

            //                   join Business in mySqlDBContext.RiskBusinessFunctionModels on riskregister.riskBusinessfunctionid equals (int?)Business.riskBusinessfunctionid into Businessgroup
            //                   from Businessfun in Businessgroup.DefaultIfEmpty()
            //                   join classification in mySqlDBContext.riskAdminClassifications on riskregister.risk_admin_classification_id equals (int?)classification.risk_admin_classification_id into classificationgroup
            //                   from classi in classificationgroup.DefaultIfEmpty()
            //                   join entitymaster in mySqlDBContext.UnitMasterModels on riskregister.Entity_Master_id equals (int?)entitymaster.Entity_Master_id into entitygroup
            //                   from entity in entitygroup.DefaultIfEmpty()
            //                   join unitlocation in mySqlDBContext.UnitLocationMasterModels on riskregister.Unit_location_Master_id equals (int?)unitlocation.Unit_location_Master_id into locationgroup
            //                   from location in locationgroup.DefaultIfEmpty()
            //                   join departmentmaster in mySqlDBContext.DepartmentModels on riskregister.Department_Master_id equals (int?)departmentmaster.Department_Master_id into deparmentgroup
            //                   from Department in deparmentgroup.DefaultIfEmpty()
            //                   join inherentrating in mySqlDBContext.risk_admin_inherriskratinglevl on riskregister.InherentRiskRating equals (int?)inherentrating.risk_admin_inherRiskRatingLevlid into inhergroup
            //                   from inherent in inhergroup.DefaultIfEmpty()
            //                   join tbluser in mySqlDBContext.usermodels on riskregister.BusinessProcessHead equals tbluser.USR_ID into iusergroup
            //                   from user in iusergroup.DefaultIfEmpty()




            //                   select new
            //                   {
            //                       RiskRegisterMasterID = riskregister.RiskRegisterMasterID ?? 0, // Default to 0 if null
            //                       UniqueDocumentID = riskregister.UniqueDocumentID ?? string.Empty, // Default to empty string if null
            //                       NameofRiskDocumentRegister = riskregister.NameofRiskDocumentRegister ?? string.Empty,
            //                       InherentRiskRating = riskregister.InherentRiskRating ?? 0,
            //                       AppetiteStatement = riskregister.AppetiteStatement ?? string.Empty,
            //                       NoOfRiskStatements = riskregister.NoOfRiskStatements ?? 0,
            //                       RiskBusinessName = Businessfun.riskbusinessname ?? "", // Default to empty string
            //                       DocumentEffectiveDate = riskregister.DocumentEffectiveDate ?? DateTime.MinValue, // Default to minimum date
            //                       ClassificationName = classi.risk_admin_classification_name ?? string.Empty,
            //                       BusinessProcessHead = riskregister.BusinessProcessHead ?? 0,
            //                       InherentRiskRatingLevelName = inherent.risk_admin_inherRiskRatingLevlname ?? string.Empty,
            //                       FirstName = user.firstname ?? string.Empty,
            //                       CombinedFields =
            //(entity.Entity_Master_Name == null && location.Unit_location_Master_name == null && Department.Department_Master_name == null)
            //? string.Empty // Default to empty string
            //: (entity.Entity_Master_Name + " | " + location.Unit_location_Master_name + " | " + Department.Department_Master_name)
            //                   })
            //                      .GroupBy(x => x.NameofRiskDocumentRegister) // Group by NameofRiskDocumentRegister
            //       .Select(g => g.FirstOrDefault())           // Take the first record from each group
            //       .ToList();
            //          //.Distinct()
            //          //.ToList();

            //    return details;
            var details = (from riskregister in mySqlDBContext.Risk_AddRegisterModels

                           join Business in mySqlDBContext.RiskBusinessFunctionModels on riskregister.riskBusinessfunctionid equals (int?)Business.riskBusinessfunctionid into Businessgroup
                           from Businessfun in Businessgroup.DefaultIfEmpty()

                           join classification in mySqlDBContext.riskAdminClassifications on riskregister.risk_admin_classification_id equals (int?)classification.risk_admin_classification_id into classificationgroup
                           from classi in classificationgroup.DefaultIfEmpty()

                           join entitymaster in mySqlDBContext.UnitMasterModels on riskregister.Entity_Master_id equals (int?)entitymaster.Entity_Master_id into entitygroup
                           from entity in entitygroup.DefaultIfEmpty()

                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on riskregister.Unit_location_Master_id equals (int?)unitlocation.Unit_location_Master_id into locationgroup
                           from location in locationgroup.DefaultIfEmpty()

                           join departmentmaster in mySqlDBContext.DepartmentModels on riskregister.department_Master_id equals (int?)departmentmaster.Department_Master_id into deparmentgroup
                           from Department in deparmentgroup.DefaultIfEmpty()

                           join inherentrating in mySqlDBContext.risk_admin_inherriskratinglevl on riskregister.InherentRiskRating equals (int?)inherentrating.risk_admin_inherRiskRatingLevlid into inhergroup
                           from inherent in inhergroup.DefaultIfEmpty()

                           join tbluser in mySqlDBContext.usermodels on riskregister.BusinessProcessHead equals tbluser.USR_ID into iusergroup
                           from user in iusergroup.DefaultIfEmpty()

                           select new
                           {
                               riskregister.RiskRegisterMasterID,
                               riskregister.UniqueDocumentID,
                               riskregister.NameofRiskDocumentRegister,
                               riskregister.InherentRiskRating,
                               riskregister.AppetiteStatement,
                               riskregister.NoOfRiskStatements,
                               Businessfun.riskbusinessname,
                               riskregister.DocumentEffectiveDate,
                               classi.risk_admin_classification_name,
                               riskregister.BusinessProcessHead,
                               inherent.risk_admin_inherRiskRatingLevlname,
                               FirstName = user.firstname ?? "",
                               CombinedFields = entity.Entity_Master_Name + " | " + location.Unit_location_Master_name + " | " + Department.Department_Master_name
                           })
                   .GroupBy(x => x.NameofRiskDocumentRegister) // Group by NameofRiskDocumentRegister
                   .Select(g => g.FirstOrDefault())           // Take the first record from each group
                   .ToList();

            return details;

        }
        [Route("api/viewRiskRegister/GetviewRiskRegisterDetailsByID")]
        [HttpGet]
        public IEnumerable<object> GetviewRiskRegisterDetailsByID(int riskRegisterMasterID)
        {
            var details = (from riskregister in mySqlDBContext.Risk_AddRegisterModels
                           join Business in mySqlDBContext.RiskBusinessFunctionModels on riskregister.riskBusinessfunctionid equals Business.riskBusinessfunctionid
                           join classification in mySqlDBContext.riskAdminClassifications on riskregister.risk_admin_classification_id equals classification.risk_admin_classification_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on riskregister.Entity_Master_id equals entitymaster.Entity_Master_id
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on riskregister.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on riskregister.department_Master_id equals departmentmaster.Department_Master_id
                           join inherentrating in mySqlDBContext.risk_admin_inherriskratinglevl on riskregister.InherentRiskRating equals inherentrating.risk_admin_inherRiskRatingLevlid
                           join tbluser in mySqlDBContext.usermodels on riskregister.BusinessProcessHead equals tbluser.USR_ID
                         
                           join Process in mySqlDBContext.Risk_BusinessProcesss on riskregister.businessprocessID equals (int?)Process.businessprocessID into Processgroup
                           from Pros in Processgroup.DefaultIfEmpty()
                           join ProcessSub1 in mySqlDBContext.Risk_Sub_ProcessL1s on riskregister.BusinessProcessL1ID equals (int?)ProcessSub1.BusinessProcessL1ID into Sub1group
                           from Sub1 in Sub1group.DefaultIfEmpty()
                           join ProcessSub2 in mySqlDBContext.Risk_Sub_ProcessL2s on riskregister.BusinessProcessL2ID equals (int?)ProcessSub2.BusinessProcessL2ID into Sub2group
                           from Sub2 in Sub2group.DefaultIfEmpty()
                           join ProcessSub3 in mySqlDBContext.Risk_Sub_ProcessL3s on riskregister.BusinessProcessL3ID equals (int?)ProcessSub3.BusinessProcessL3ID into Sub3group
                           from Sub3 in Sub3group.DefaultIfEmpty()
                           join typerisk in mySqlDBContext.riskAdminModels on riskregister.Risk_Admin_typeOfRisk_id equals (int?)typerisk.Risk_Admin_typeOfRisk_id into Riskgroup
                           from Type in Riskgroup.DefaultIfEmpty()
                           join categorization in mySqlDBContext.risk_admin_risk_categorization on riskregister.risk_admin_risk_categorization_id equals (int?)categorization.risk_admin_risk_categorization_id into categorizgroup
                           from categoriz in categorizgroup.DefaultIfEmpty()
                           join causelist in mySqlDBContext.risk_admin_causelist on riskregister.risk_admin_causeList_id equals (int?)causelist.risk_admin_causeList_id into causegroup
                           from cause in causegroup.DefaultIfEmpty()

                         
                           join riskimpact in mySqlDBContext.risk_admin_riskimpactrating on riskregister.risk_admin_riskImpactRating_id equals (int?)riskimpact.risk_admin_riskImpactRating_id into impactgroup
                           from impact in impactgroup.DefaultIfEmpty()

                           join likelihood in mySqlDBContext.risk_admin_likeoccfact on riskregister.risk_admin_likeoccfact_id equals (int?)likelihood.risk_admin_likeoccfact_id into likelihoodgroup
                           from likelihood in likelihoodgroup.DefaultIfEmpty()
                            
                           join Inherent in mySqlDBContext.risk_admin_inherriskratinglevl
                              on riskregister.InherentRatingColor equals Inherent.colour_reference into InherentGroup
                           from Inherent in InherentGroup.DefaultIfEmpty()

                           join riskactivity in mySqlDBContext.riskadminactivityfrequencymodels
                           on riskregister.activityid equals riskactivity.activityid into activityGroup
                           from riskactivity in activityGroup.DefaultIfEmpty()

                           join Priority in mySqlDBContext.risk_admin_riskpriority
                            on riskregister.RiskPirorityColor equals Priority.color_code into PriorityGroup
                           from Priority in PriorityGroup.DefaultIfEmpty()

                           join RiskInsensity in mySqlDBContext.risk_admin_riskintensity
                           on riskregister.RiskIntensityColor equals RiskInsensity.colour_reference into InsensityGroup
                           from RiskInsensity in InsensityGroup.DefaultIfEmpty()

                           
                           join Notifier in mySqlDBContext.Risk_NotificationSetUps
                            on riskregister.RiskRegisterMasterID equals Notifier.RiskRegisterMasterID into NotifierGroup
                           from Notifier in NotifierGroup.DefaultIfEmpty()

                           join FileSatement in mySqlDBContext.Risk_StatementFileMasters on riskregister.RiskStatementID 
                           equals FileSatement.RiskStatementID into fileGroup from  Statement  in fileGroup.DefaultIfEmpty()

                                                                                  // join Notifier in mySqlDBContext.Risk_NotificationSetUps on riskregister.RiskRegisterMasterID equals Notifier.RiskRegisterMasterID
                                                                                  // join Classification in mySqlDBContext.NatureOf_DocumentMasterModels on riskregister.natureOf_Doc_id equals Classification.NatureOf_Doc_id
                           where riskregister.RiskRegisterMasterID == riskRegisterMasterID
                           select new
                           {
                               riskregister.RiskRegisterMasterID,
                               riskregister.ObjectiveofRiskDocument,
                               riskregister.Entity_Master_id,
                               riskregister.Unit_location_Master_id,
                               riskregister.Risk_Admin_typeOfRisk_id,
                               riskregister.risk_admin_classification_id,
                               riskregister.risk_admin_risk_categorization_id,
                               riskregister.risk_admin_causeList_id,
                               riskregister.risk_admin_riskImpactRating_id,
                               riskregister.risk_admin_likeoccfact_id,
                               riskregister.risk_admin_LETC_L1_id,


                               riskregister.UniqueRiskID,
                               riskregister.UniqueDocumentID,
                               riskregister.NameofRiskDocumentRegister,
                               riskregister.InherentRiskRating,
                               riskregister.AppetiteStatement,
                               riskregister.NoOfRiskStatements,
                               Business.riskbusinessname,
                               riskregister.DocumentEffectiveDate,
                               classification.risk_admin_classification_name,
                               riskregister.BusinessProcessHead,
                               inherentrating.risk_admin_inherRiskRatingLevlname,
                               tbluser.firstname,
                               BusinessProcessName = (Pros != null) ? Pros.BusinessProcessName : "N/A",
                               BusinessSubProcessL1Name = (Sub1 != null) ? Sub1.BusinessSubProcessL1Name : "N/A",
                               BusinessSubProcessL2Name = (Sub2 != null) ? Sub2.BusinessSubProcessL2Name : "N/A",
                               BusinessSubProcessL3Name = (Sub3 != null) ? Sub3.BusinessSubProcessL3Name : "N/A",
                               Risk_Admin_typeOfRisk_name = (Type != null) ? Type.Risk_Admin_typeOfRisk_name : "N/A",
                               risk_admin_risk_categorizationName = (categoriz != null) ? categoriz.risk_admin_risk_categorizationName : "N/A",
                               risk_admin_causeListName = (cause != null) ? cause.risk_admin_causeListName : "N/A",

                               riskregister.BusinessSubProcessObjective,
                               riskregister.RiskRootCause,
                              
                               riskregister.Selectfrequencyperiod,
                               riskregister.AMLComplianceRisk,
                               riskregister.ModelRisk,
                               riskregister.ConductRisk,
                               riskregister.ITCyberSecurity,
                               riskregister.ThirdPartyOutsourcing,
                               riskregister.FraudRisk,
                               riskregister.LegalRisk,
                               riskregister.OperationalRisk,
                               riskregister.ReputationalRisk,
                               riskregister.FinancialRiskReporting,
                               riskregister.RiskCostImpact,
                               riskregister.CategoryL1Description,
                               riskregister.CategoryL2Description,
                               riskregister.CategoryL3Description,
                               riskregister.PotentialImpactDescription,
                               riskregister.SuggestivePriventive,
                               riskregister.RepeatReviewFrequency,
                               riskregister.EnterValueforrepeat,
                               riskregister.StartDateNextReview,
                               riskregister.InherentRatingColor,
                               CombinedFieldsNotifierStatus1 = (from Notifier in mySqlDBContext.Risk_NotificationSetUps
                                                                where Notifier.RiskRegisterMasterID == riskregister.RiskRegisterMasterID
                                                                      && Notifier.EscalationStatus == 1
                                                                select Notifier.EnterDays + " | " + Notifier.EnterComb)
                                                    .FirstOrDefault(),
                               CombinedFieldsNotifierStatus2 = (from Notifier in mySqlDBContext.Risk_NotificationSetUps
                                                                where Notifier.RiskRegisterMasterID == riskregister.RiskRegisterMasterID
                                                                      && Notifier.EscalationStatus == 2
                                                                select Notifier.EnterDays + " | " + Notifier.EnterComb)
                                                    .FirstOrDefault(),
                               CombinedFieldsNotifierStatus3 = (from Notifier in mySqlDBContext.Risk_NotificationSetUps
                                                                where Notifier.RiskRegisterMasterID == riskregister.RiskRegisterMasterID
                                                                      && Notifier.EscalationStatus == 3
                                                                select Notifier.EnterDays + " | " + Notifier.EnterComb)
                                                    .FirstOrDefault(),
                               CombinedFieldsNotifierStatus4 = (from Notifier in mySqlDBContext.Risk_NotificationSetUps
                                                                where Notifier.RiskRegisterMasterID == riskregister.RiskRegisterMasterID
                                                                      && Notifier.EscalationStatus == 4
                                                                select Notifier.EnterDays + " | " + Notifier.EnterComb)
                                                    .FirstOrDefault(),
                             
                               CombinedFields = entitymaster.Entity_Master_Name + " | " + unitlocation.Unit_location_Master_name + " | " + departmentmaster.Department_Master_name,

                               risk_admin_riskImpactRating_name = (impact != null) ? impact.risk_admin_riskImpactRating_name : "N/A",
                               color_reference = (impact != null) ? impact.color_reference : "N/A",
                               risk_admin_likeoccfact_name = (likelihood != null) ? likelihood.risk_admin_likeoccfact_name : "N/A",
                               //colorreference = (likeli != null) ? likeli.colorreference : "N/A",
                               InherentRiskRatingLevelName = (Inherent != null) ? Inherent.risk_admin_inherRiskRatingLevlname : "N/A",
                               activityname = (riskactivity != null) ? riskactivity.activityname : "N/A",
                               riskregister.RiskPirorityColor,
                               PriorityName = (Priority != null) ? Priority.risk_admin_riskPriorityName : "N/A",
                               riskregister.RiskIntensityColor,
                               RiskInsensityName = (RiskInsensity != null) ? RiskInsensity.risk_admin_riskIntensityname : "N/A",

                               likeoccfact = (likelihood != null) ? likelihood.color_reference : "N/A",

                               FileCategory = (Statement != null) ? Statement.FileCategory : "N/A",
                               FilePath = (Statement != null) ? Statement.FilePath : "N/A",
                               Statement.CreatedBy,
                               Statement.CreatedDate,
                               FileName = (Statement != null) ? Statement.FileName : "N/A",
                               riskregister.RiskStatementID,


                           })
                  .Distinct().ToList();

            var actFiles = mySqlDBContext.Risk_StatementFileMasters
                .Where(af => details.Select(r => r.RiskStatementID).Contains(af.RiskStatementID) && af.Status == "Active")
                .Select(af => new
                {
                    af.StatementFileID,
                    //af.RiskRegisterMasterID,
                    af.RiskStatementID,
                    af.FilePath,
                    af.FileName,
                    af.Status,
                    FileCategory = af.FileCategory
                })
               .Distinct()
            .ToList();

            var modifiedResult = details.Select(item => new
            {
                item.RiskRegisterMasterID,
                item.CreatedBy,
                item.CreatedDate,
                item.ObjectiveofRiskDocument,
                item.Entity_Master_id,
                item.Unit_location_Master_id,
                item.Risk_Admin_typeOfRisk_id,
                item.risk_admin_classification_id,
                item.risk_admin_risk_categorization_id,
                item.risk_admin_causeList_id,
                item.risk_admin_riskImpactRating_id,
                item.risk_admin_likeoccfact_id,
                item.risk_admin_LETC_L1_id,
                item.UniqueRiskID,
                item.UniqueDocumentID,
                item.NameofRiskDocumentRegister,
                item.InherentRiskRating,
                item.AppetiteStatement,
                item.NoOfRiskStatements,
                item.riskbusinessname,
                item.DocumentEffectiveDate,
                item.risk_admin_classification_name,
                item.BusinessProcessHead,
                item.risk_admin_inherRiskRatingLevlname,
                item.firstname,
                item.BusinessProcessName ,
                item.BusinessSubProcessL1Name,
                item.BusinessSubProcessL2Name,
                item.BusinessSubProcessL3Name ,
                item.Risk_Admin_typeOfRisk_name ,
                item.risk_admin_risk_categorizationName ,
                item.risk_admin_causeListName ,

                item.BusinessSubProcessObjective,
                item.RiskRootCause,

                item.Selectfrequencyperiod,
                item.AMLComplianceRisk,
                item.ModelRisk,
                item.ConductRisk,
                item.ITCyberSecurity,
                item.ThirdPartyOutsourcing,
                item.FraudRisk,
                item.LegalRisk,
                item.OperationalRisk,
                item.ReputationalRisk,
                item.FinancialRiskReporting,
                item.RiskCostImpact,
                item.CategoryL1Description,
                item.CategoryL2Description,
                item.CategoryL3Description,
                item.PotentialImpactDescription,
                item.SuggestivePriventive,
                item.RepeatReviewFrequency,
                item.EnterValueforrepeat,
                item.StartDateNextReview,
                item.InherentRatingColor,
                item.CombinedFieldsNotifierStatus1 ,
                item.CombinedFieldsNotifierStatus2 ,
                item.CombinedFieldsNotifierStatus3 ,
               item. CombinedFieldsNotifierStatus4 ,

                item.CombinedFields ,

                item.risk_admin_riskImpactRating_name ,
                item.color_reference ,
                item.risk_admin_likeoccfact_name ,
                item.InherentRiskRatingLevelName ,
                item.activityname ,
                item.RiskPirorityColor,
                item.PriorityName ,
                item.RiskIntensityColor,
                item.RiskInsensityName ,
                item.likeoccfact ,

                actfiles = actFiles.Where(af => af.RiskStatementID == item.RiskStatementID && af.Status == "Active")
                                   .Select(af => new
                                   {
                                       af.StatementFileID,
                                       af.FileCategory,
                                       af.FilePath,
                                       FileName = af.FilePath != null ?
                                                  af.FilePath.Substring(af.FilePath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList()
            })
            .ToList();
            var distinctDepartmentDetails = modifiedResult
          .GroupBy(d => d.RiskRegisterMasterID)
          .Select(g => g.First()) // Keep only the first occurrence per departmentid
          .ToList();
            return distinctDepartmentDetails;
        }

        [Route("api/EditRiskRegister/GetEditRiskRegisterDetailsByID/{riskRegisterMasterID}")]
        [HttpGet]
        public IEnumerable<object> GetEditRiskRegisterDetailsByID(int riskRegisterMasterID)
                   
        {
            var details = (from riskregister in mySqlDBContext.Risk_AddRegisterModels
                           join Business in mySqlDBContext.RiskBusinessFunctionModels on riskregister.riskBusinessfunctionid equals Business.riskBusinessfunctionid
                           join classification in mySqlDBContext.riskAdminClassifications on riskregister.risk_admin_classification_id equals classification.risk_admin_classification_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on riskregister.Entity_Master_id equals entitymaster.Entity_Master_id
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on riskregister.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on riskregister.department_Master_id equals departmentmaster.Department_Master_id
                           join inherentrating in mySqlDBContext.risk_admin_inherriskratinglevl on riskregister.InherentRiskRating equals inherentrating.risk_admin_inherRiskRatingLevlid
                           join tbluser in mySqlDBContext.usermodels on riskregister.BusinessProcessHead equals tbluser.USR_ID

                           join Process in mySqlDBContext.Risk_BusinessProcesss on riskregister.businessprocessID equals (int?)Process.businessprocessID into Processgroup
                           from Pros in Processgroup.DefaultIfEmpty()
                           join ProcessSub1 in mySqlDBContext.Risk_Sub_ProcessL1s on riskregister.BusinessProcessL1ID equals (int?)ProcessSub1.BusinessProcessL1ID into Sub1group
                           from Sub1 in Sub1group.DefaultIfEmpty()
                           join ProcessSub2 in mySqlDBContext.Risk_Sub_ProcessL2s on riskregister.BusinessProcessL2ID equals (int?)ProcessSub2.BusinessProcessL2ID into Sub2group
                           from Sub2 in Sub2group.DefaultIfEmpty()
                           join ProcessSub3 in mySqlDBContext.Risk_Sub_ProcessL3s on riskregister.BusinessProcessL3ID equals (int?)ProcessSub3.BusinessProcessL3ID into Sub3group
                           from Sub3 in Sub3group.DefaultIfEmpty()
                           join typerisk in mySqlDBContext.riskAdminModels on riskregister.Risk_Admin_typeOfRisk_id equals (int?)typerisk.Risk_Admin_typeOfRisk_id into Riskgroup
                           from Type in Riskgroup.DefaultIfEmpty()
                           join categorization in mySqlDBContext.risk_admin_risk_categorization on riskregister.risk_admin_risk_categorization_id equals (int?)categorization.risk_admin_risk_categorization_id into categorizgroup
                           from categoriz in categorizgroup.DefaultIfEmpty()
                           join causelist in mySqlDBContext.risk_admin_causelist on riskregister.risk_admin_causeList_id equals (int?)causelist.risk_admin_causeList_id into causegroup
                           from cause in causegroup.DefaultIfEmpty()


                           join riskimpact in mySqlDBContext.risk_admin_riskimpactrating on riskregister.risk_admin_riskImpactRating_id equals (int?)riskimpact.risk_admin_riskImpactRating_id into impactgroup
                           from impact in impactgroup.DefaultIfEmpty()

                           join likelihood in mySqlDBContext.risk_admin_likeoccfact on riskregister.risk_admin_likeoccfact_id equals (int?)likelihood.risk_admin_likeoccfact_id into likelihoodgroup
                           from likelihood in likelihoodgroup.DefaultIfEmpty()

                           join Inherent in mySqlDBContext.risk_admin_inherriskratinglevl
                              on riskregister.InherentRatingColor equals Inherent.colour_reference into InherentGroup
                           from Inherent in InherentGroup.DefaultIfEmpty()

                           join riskactivity in mySqlDBContext.riskadminactivityfrequencymodels
                           on riskregister.activityid equals riskactivity.activityid into activityGroup
                           from riskactivity in activityGroup.DefaultIfEmpty()

                           join Priority in mySqlDBContext.risk_admin_riskpriority
                            on riskregister.RiskPirorityColor equals Priority.color_code into PriorityGroup
                           from Priority in PriorityGroup.DefaultIfEmpty()

                           join RiskInsensity in mySqlDBContext.risk_admin_riskintensity
                           on riskregister.RiskIntensityColor equals RiskInsensity.colour_reference into InsensityGroup
                           from RiskInsensity in InsensityGroup.DefaultIfEmpty()

                           join riskappetite in mySqlDBContext.risk_admin_riskappetite
                           on riskregister.risk_admin_RiskAppetiteId equals riskappetite.risk_admin_RiskAppetiteId into appetiteGroup
                           from riskappetite in appetiteGroup.DefaultIfEmpty()

                           join naturedoc in mySqlDBContext.NatureOf_DocumentMasterModels
                          on riskregister.natureOf_Doc_id equals naturedoc.NatureOf_Doc_id into natureGroup
                           from naturedoc in natureGroup.DefaultIfEmpty()


                           where riskregister.RiskRegisterMasterID == riskRegisterMasterID
                           select new
                           {
                               riskregister.RiskRegisterMasterID,
                               riskregister.ObjectiveofRiskDocument,
                               riskregister.Entity_Master_id,
                               riskregister.Unit_location_Master_id,
                               riskregister.department_Master_id,
                               riskregister.riskBusinessfunctionid,
                               riskregister.businessprocessID,
                               riskregister.BusinessProcessL1ID,
                               riskregister.BusinessProcessL2ID,
                               riskregister.BusinessProcessL3ID,
                               riskregister.BusinessSubProcessObjective,
                               riskregister.Risk_Admin_typeOfRisk_id,
                               riskregister.risk_admin_classification_id,
                               riskregister.risk_admin_risk_categorization_id,
                               riskregister.risk_admin_causeList_id,
                               riskregister.risk_admin_riskImpactRating_id,
                               riskregister.risk_admin_likeoccfact_id,
                               riskregister.RiskPriority,
                               riskregister.activityid,
                               riskregister.Slidervalue,
                               riskregister.RiskIntensity,
                               riskregister.risk_admin_LETC_L1_id,
                               riskregister.UniqueRiskID,
                               riskregister.UniqueDocumentID,
                               riskregister.NameofRiskDocumentRegister,
                               riskregister.InherentRiskRating ,

            riskregister.AppetiteStatement,
                               riskregister.NoOfRiskStatements,
                               riskregister.DocumentEffectiveDate,
                               classification.risk_admin_classification_name,
                               riskregister.BusinessProcessHead,
                               inherentrating.risk_admin_inherRiskRatingLevlname,
                               tbluser.firstname,
                               riskregister.RiskOwnership,
                               riskregister.ReviewfrequencyCheck,
                               riskregister.RepeatReviewFrequency,
                               riskregister.EnterValueforrepeat,
                               riskregister.Selectfrequencyperiod,
                               Risk_Admin_typeOfRisk_name = (Type != null) ? Type.Risk_Admin_typeOfRisk_name : "N/A",
                               risk_admin_risk_categorizationName = (categoriz != null) ? categoriz.risk_admin_risk_categorizationName : "N/A",
                               risk_admin_causeListName = (cause != null) ? cause.risk_admin_causeListName : "N/A",
                               riskregister.RiskRootCause,
                               riskregister.AMLComplianceRisk,
                               riskregister.ModelRisk,
                               riskregister.ConductRisk,
                               riskregister.ITCyberSecurity,
                               riskregister.ThirdPartyOutsourcing,
                               riskregister.FraudRisk,
                               riskregister.LegalRisk,
                               riskregister.OperationalRisk,
                               riskregister.ReputationalRisk,
                               riskregister.FinancialRiskReporting,
                               riskregister.RiskCostImpact,
                               riskregister.risk_admin_letc_l2_id,
                               riskregister.risk_admin_LETC_l3_id,
                               riskregister.CategoryL1Description,
                               riskregister.CategoryL2Description,
                               riskregister.CategoryL3Description,
                               riskregister.risk_admin_potenBussImpactid,
                               riskregister.PotentialImpactDescription,
                               riskregister.SuggestivePriventive,
                               riskregister.StartDateNextReview,

                               riskregister.InherentRatingColor,

                               CombinedFields = entitymaster.Entity_Master_Name + " | " + unitlocation.Unit_location_Master_name ,
                               risk_admin_riskImpactRating_name = (impact != null) ? impact.risk_admin_riskImpactRating_name : "N/A",
                               color_reference = (impact != null) ? impact.color_reference : "N/A",
                               risk_admin_likeoccfact_name = (likelihood != null) ? likelihood.risk_admin_likeoccfact_name : "N/A",
                             
                               InherentRiskRatingLevelName = (Inherent != null) ? Inherent.risk_admin_inherRiskRatingLevlname : "N/A",
                              
                               riskregister.RiskPirorityColor,
                               PriorityName = (Priority != null) ? Priority.risk_admin_riskPriorityName : "N/A",
                               riskregister.RiskIntensityColor,
                               RiskInsensityName = (RiskInsensity != null) ? RiskInsensity.risk_admin_riskIntensityname : "N/A",

                               likeoccfact = (likelihood != null) ? likelihood.color_reference : "N/A",
                               riskregister.docTypeID,
                               riskregister.doc_CategoryID,
                               riskregister.doc_SubCategoryID,
                             
                               riskregister.DocumentConfidentiality,
                               riskregister.InternalReferenceNo,
                               riskregister.PhysicalVaultLocation,
                               riskregister.Keywords,
                               riskregister.PublishingRemarks,
                               riskregister.risk_admin_RiskAppetiteId,
                               riskregister.natureOf_Doc_id,
                               riskregister.OtpMethod,
                               riskregister.ProcessOwner,
                               riskregister.BusinessFunctionHead,








                           })
                  .Distinct()
                  .ToList();

            return details;
        }

        [Route("api/risk_admininherriskratinglevl/Getrisk_admininherriskratinglevl")]
        [HttpGet]
        public IEnumerable<risk_admin_inherriskratinglevl> Getrisk_admininherriskratinglevl(int inherRiskRatingID)
        {
            return this.mySqlDBContext.risk_admin_inherriskratinglevl.Where(x => x.risk_admin_inherRiskRatingstatus == "Active" && x.risk_admin_inherRiskRatingLevlid == inherRiskRatingID)
                .OrderBy(r => r.risk_level_range_min)
                .ToList();
        }
        [Route("api/ViewListRiskStatement/GetViewListRiskStatement")]
        [HttpGet]
        public IEnumerable<object> GetViewListRiskStatement(string nameofRiskDocumentRegister)
        {
            var details = (from riskstatment in mySqlDBContext.Risk_AddRegisterModels
                           join statement in mySqlDBContext.Risk_RiskStatements on riskstatment.RiskStatementID equals statement.RiskStatementID
                           join Process in mySqlDBContext.Risk_BusinessProcesss on riskstatment.businessprocessID equals (int?)Process.businessprocessID into Processgroup
                           from Pros in Processgroup.DefaultIfEmpty()
                           join riskimpact in mySqlDBContext.risk_admin_riskimpactrating on riskstatment.risk_admin_riskImpactRating_id equals (int?)riskimpact.risk_admin_riskImpactRating_id into impactgroup
                           from impact in impactgroup.DefaultIfEmpty()

                           join likelihood in mySqlDBContext.risk_admin_likeoccfact on riskstatment.risk_admin_likeoccfact_id equals (int?)likelihood.risk_admin_likeoccfact_id into likelihoodgroup
                           from likelihood in likelihoodgroup.DefaultIfEmpty()
                           join Priority in mySqlDBContext.risk_admin_riskpriority on riskstatment.RiskPirorityColor equals Priority.color_code into PriorityGroup
                           from Priority in PriorityGroup.DefaultIfEmpty()
                           join tbluser in mySqlDBContext.usermodels on riskstatment.BusinessProcessHead equals tbluser.USR_ID
                           join entitymaster in mySqlDBContext.UnitMasterModels on riskstatment.Entity_Master_id equals entitymaster.Entity_Master_id
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on riskstatment.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on riskstatment.department_Master_id equals departmentmaster.Department_Master_id
                           join Business in mySqlDBContext.RiskBusinessFunctionModels on riskstatment.riskBusinessfunctionid equals Business.riskBusinessfunctionid


                           where riskstatment.NameofRiskDocumentRegister == nameofRiskDocumentRegister

                           select new
                           {
                               riskstatment.RiskRegisterMasterID,
                               riskstatment.UniqueRiskID,
                               entitymaster.Entity_Master_Name,
                               unitlocation.Unit_location_Master_name,
                               departmentmaster.Department_Master_name,
                               Business.riskbusinessname,
                               riskstatment.NameofRiskDocumentRegister,
                               statement.RiskStatementID,
                               statement.RiskStatementName,
                               statement.RiskDescription,
                               BusinessProcessName = (Pros != null) ? Pros.BusinessProcessName : "N/A",
                               risk_admin_riskImpactRating_name = (impact != null) ? impact.risk_admin_riskImpactRating_name : "N/A",
                               risk_admin_likeoccfact_name = (likelihood != null) ? likelihood.risk_admin_likeoccfact_name : "N/A",
                               PriorityName = (Priority != null) ? Priority.risk_admin_riskPriorityName : "N/A",
                               riskstatment.RepeatReviewFrequency,
                               tbluser.firstname,

                           })
                            .Distinct()
                .ToList();


            return details;
        }


        [Route("api/ViewListName/GetViewListName")]
        [HttpGet]
        public IEnumerable<object> GetViewListName()
        {
            var details = (from riskstatment in mySqlDBContext.Risk_AddRegisterModels
                           join statement in mySqlDBContext.Risk_RiskStatements on riskstatment.RiskStatementID equals statement.RiskStatementID
                           join Process in mySqlDBContext.Risk_BusinessProcesss on riskstatment.businessprocessID equals (int?)Process.businessprocessID into Processgroup
                           from Pros in Processgroup.DefaultIfEmpty()
                           join riskimpact in mySqlDBContext.risk_admin_riskimpactrating on riskstatment.risk_admin_riskImpactRating_id equals (int?)riskimpact.risk_admin_riskImpactRating_id into impactgroup
                           from impact in impactgroup.DefaultIfEmpty()

                           join likelihood in mySqlDBContext.risk_admin_likeoccfact on riskstatment.risk_admin_likeoccfact_id equals (int?)likelihood.risk_admin_likeoccfact_id into likelihoodgroup
                           from likelihood in likelihoodgroup.DefaultIfEmpty()
                           join Priority in mySqlDBContext.risk_admin_riskpriority on riskstatment.RiskPirorityColor equals Priority.color_code into PriorityGroup
                           from Priority in PriorityGroup.DefaultIfEmpty()
                           join tbluser in mySqlDBContext.usermodels on riskstatment.BusinessProcessHead equals tbluser.USR_ID
                           join entitymaster in mySqlDBContext.UnitMasterModels on riskstatment.Entity_Master_id equals entitymaster.Entity_Master_id
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on riskstatment.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on riskstatment.department_Master_id equals departmentmaster.Department_Master_id
                           join Business in mySqlDBContext.RiskBusinessFunctionModels on riskstatment.riskBusinessfunctionid equals Business.riskBusinessfunctionid



                           select new
                           {
                               riskstatment.RiskRegisterMasterID,
                               riskstatment.UniqueRiskID,
                               entitymaster.Entity_Master_Name,
                               unitlocation.Unit_location_Master_name,
                               departmentmaster.Department_Master_name,
                               Business.riskbusinessname,
                               riskstatment.NameofRiskDocumentRegister,
                               statement.RiskStatementID,
                               statement.RiskStatementName,
                               statement.RiskDescription,
                               BusinessProcessName = (Pros != null) ? Pros.BusinessProcessName : "N/A",
                               risk_admin_riskImpactRating_name = (impact != null) ? impact.risk_admin_riskImpactRating_name : "N/A",
                               risk_admin_likeoccfact_name = (likelihood != null) ? likelihood.risk_admin_likeoccfact_name : "N/A",
                               PriorityName = (Priority != null) ? Priority.risk_admin_riskPriorityName : "N/A",
                               riskstatment.RepeatReviewFrequency,
                               tbluser.firstname,

                           })
                            .Distinct()
                .ToList();


            return details;
        }



        [Route("api/Risk_AddRegisterController/checkregistername")]
        [HttpGet]

        public bool? CheckRegisterName(string nameofriskreg)
        {
            if (string.IsNullOrEmpty(nameofriskreg) || nameofriskreg == "undefined")
            {
                return null; // Invalid input
            }

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                string query = @"SELECT COUNT(*) FROM risk_riskregistermaster 
                         WHERE NameofRiskDocumentRegister = @NameofRiskDocumentRegister";

                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@NameofRiskDocumentRegister", nameofriskreg);

                    // Execute the scalar query to get the count
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    // Return true if the name exists, false otherwise
                    return count > 0;
                }
            }
        }


        [Route("api/FilesGetting/FilesGettingDetails/{fileid}")]
        [HttpGet]

        public IEnumerable<object> FilesGettingDetails(int fileid)
        {
            var result = (from RiskStatement in mySqlDBContext.Risk_RiskStatements
                          join tbluser in mySqlDBContext.usermodels on RiskStatement.CreatedBy equals tbluser.USR_ID
                          //join user in mySqlDBContext.usermodels on actregulatory.updatedby equals user.USR_ID into userJoin
                          //from user in userJoin.DefaultIfEmpty()
                          where RiskStatement.RiskStatementID == fileid
                          select new
                          {
                              RiskStatementID = RiskStatement.RiskStatementID,
                              CreatedBy = RiskStatement.CreatedBy,
                              CreatedDate = RiskStatement.CreatedDate,
                              
                          })
                         .Distinct()
                         .ToList();

            var actFiles = mySqlDBContext.Risk_StatementFileMasters
                .Where(af => result.Select(r => r.RiskStatementID).Contains(af.RiskStatementID) && af.Status == "Active")
                .Select(af => new
                {
                    af.StatementFileID,
                    af.RiskStatementID,
                    af.FilePath,
                    af.FileName,
                    af.Status,
                    FileCategory = af.FileCategory
                })
                .ToList();

            var modifiedResult = result.Select(item => new
            {
                item.RiskStatementID,
                
                item.CreatedBy,
                item.CreatedDate,

                actfiles = actFiles.Where(af => af.RiskStatementID == item.RiskStatementID && af.Status == "Active")
                                   .Select(af => new
                                   {
                                       af.StatementFileID,
                                       af.FileCategory,
                                       af.FilePath,
                                       FileName = af.FilePath != null ?
                                                  af.FilePath.Substring(af.FilePath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList()
            })
            .ToList();

            return modifiedResult;
        }



    }
}

