
using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel;
using ITR_TelementaryAPI.Models;
using ITRTelemetry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using MySQLProvider;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RemediationPlanController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public RemediationPlanController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }


        //[Route("api/RemediationPlan/GetRemediationPlanRequestDetails")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<RemediationPlanModel>>> GetRemediationPlanRequestDetails()
        //{
        //    var plans = await this.mydbContext.RemediationPlanModels.ToListAsync();
        //    return Ok(plans);
        //}

        //[Route("api/RemediationPlan/InsertRemediationPlanRequestDetails")]
        //[HttpPost]
        //public async Task<IActionResult> InsertRemediationPlanDetails([FromBody] RemediationPlanModel remediationPlanModel)
        //{
        //    if (remediationPlanModel == null)
        //    {
        //        return BadRequest("Request cannot be null");
        //    }

        //    try
        //    {
        //       // remediationPlanModel.rpa_request_date = DateTime.Now;

        //     //   this.mydbContext.RemediationPlanModels.Add(remediationPlanModel);
        //     ////   await this.mydbContext.SaveChangesAsync();

        //        return Ok("Data inserted successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error occurred: {ex.Message}");
        //    }
        //}





        [Route("api/RemediationPlan/InsertRemediationPlanRequestDetails")]
        [HttpPost]

        public IActionResult InsertRemediationPlanRequestDetails([FromBody] RemediationPlanModels remediationPlanModel)

        {
            Console.WriteLine("Received Payload: " + JsonConvert.SerializeObject(remediationPlanModel));

            // Validate the incoming model
            if (remediationPlanModel == null || remediationPlanModel.originalData == null)
            {
                return BadRequest("Invalid or missing data.");
            }

            // Database connection string
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    foreach (var compliance in remediationPlanModel.originalData)
                    {
                        // Prepare SQL Insert Query
                        string insertQuery = @"INSERT INTO remediation_plan 
                            (compliance_id, entity_id, unit_location_id, department_id, requester_id, 
                             remediation_request_remarks, compliance_stage_progress, compliance_status, 
                             proposed_remedial_comply_date, rpa_request_date, Status, CreatedDate) 
                            VALUES 
                            (@compliance_id, @entity_id, @unit_location_id, @department_id, @requester_id, 
                             @remediation_request_remarks, @compliance_stage_progress, @compliance_status, 
                             @proposed_remedial_comply_date, @rpa_request_date, @Status, @CreatedDate)";

                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, con))
                        {
                            // Map the parameters from the model
                            cmd.Parameters.AddWithValue("@compliance_id", compliance.compliance_id);
                            cmd.Parameters.AddWithValue("@entity_id", compliance.entity_id);
                            cmd.Parameters.AddWithValue("@unit_location_id", compliance.unit_location_id);
                            cmd.Parameters.AddWithValue("@department_id", compliance.department_id);
                            cmd.Parameters.AddWithValue("@requester_id", remediationPlanModel.requester_id);
                            cmd.Parameters.AddWithValue("@remediation_request_remarks", remediationPlanModel.remediation_request_remarks);
                            if (compliance.complianceStage == "Due")
                            {
                                cmd.Parameters.AddWithValue("@compliance_stage_progress", "Extension Applied");
                                cmd.Parameters.AddWithValue("@compliance_status", "Remediation In Progress");
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@compliance_stage_progress", "Remediation Applied");
                                cmd.Parameters.AddWithValue("@compliance_status", "Remediation In Progress");
                            }
                            cmd.Parameters.AddWithValue("@proposed_remedial_comply_date", DateTime.Parse(remediationPlanModel.proposed_remedial_comply_date)); // Ensure proper date conversion
                            cmd.Parameters.AddWithValue("@rpa_request_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Status", "Active");
                            cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                            // Execute the query
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return Ok("Data successfully inserted into the remediation_plan table.");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine("Error while inserting data: " + ex.Message);
                    return BadRequest($"Error: {ex.Message}");
                }
                finally
                {
                    // Ensure the connection is closed even if an error occurs
                    con.Close();
                }
            }

        }


        [Route("api/RemediationPlan/ApproveRemediationPlanRequest")]
        [HttpPost]
        public IActionResult ApproveRemediationPlan([FromBody] RemediationPlanModell remediationPlanModel)
        {
            Console.WriteLine("Received Payload: " + JsonConvert.SerializeObject(remediationPlanModel));

            // Validate the incoming model
            if (remediationPlanModel == null || remediationPlanModel.originalData == null)
            {
                return BadRequest("Invalid or missing data.");
            }

            // Database connection string
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    

                    foreach (var compliance in remediationPlanModel.originalData)
                    {
                       
                        
                        string insertQuery = (@"update remediation_plan set remediation_approval_remarks=@remediation_approval_remarks,approved_remedial_comply_date=@approved_remedial_comply_date,
compliance_stage_progress=@compliance_stage_progress,compliance_status=@compliance_status,rpa_approval_date=@rpa_approval_date,approver_id=@approver_id,rejecter_id=@rejecter_id,
Status=@Status where remediation_plan_id=@remediation_plan_id ");


                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, con))
                        {
                            // Map the parameters from the model
                            cmd.Parameters.AddWithValue("@remediation_plan_id", compliance.remediation_plan_id);
                            cmd.Parameters.AddWithValue("@remediation_approval_remarks", remediationPlanModel.remediation_approval_remarks);
                            cmd.Parameters.AddWithValue("@approver_id", remediationPlanModel.approver_id);
                            cmd.Parameters.AddWithValue("@rejecter_id", 0);
                            if (compliance.complianceStage == "Extension Applied")
                            {
                                cmd.Parameters.AddWithValue("@compliance_stage_progress", "Under Extension");
                                cmd.Parameters.AddWithValue("@compliance_status", "Due(RPA)");
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@compliance_stage_progress", "Under Remediation");
                                cmd.Parameters.AddWithValue("@compliance_status", "Overdue(RPA)");
                            }
                            cmd.Parameters.AddWithValue("@approved_remedial_comply_date", DateTime.Parse(remediationPlanModel.approved_remedial_comply_date)); // Ensure proper date conversion
                            cmd.Parameters.AddWithValue("@rpa_approval_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Status", "Active");
                            

                            // Execute the query
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return Ok("Data successfully inserted into the remediation_plan table.");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine("Error while inserting data: " + ex.Message);
                    return BadRequest($"Error: {ex.Message}");
                }
                finally
                {
                    // Ensure the connection is closed even if an error occurs
                    con.Close();
                }
            }
        }


        [Route("api/RemediationPlan/RejectRemediationPlanRequest")]
        [HttpPost]
        public IActionResult RejectRemediationPlanRequest([FromBody] RemediationPlanModell remediationPlanModel)
        {
            Console.WriteLine("Received Payload: " + JsonConvert.SerializeObject(remediationPlanModel));

            // Validate the incoming model
            if (remediationPlanModel == null || remediationPlanModel.originalData == null)
            {
                return BadRequest("Invalid or missing data.");
            }

            // Database connection string
            string connectionString = Configuration["ConnectionStrings:myDb1"];
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();



                    foreach (var compliance in remediationPlanModel.originalData)
                    {


                        string insertQuery = (@"update remediation_plan set remediation_rejection_remarks=@remediation_rejection_remarks,
compliance_stage_progress=@compliance_stage_progress,compliance_status=@compliance_status,rpa_reject_date=@rpa_reject_date,rejecter_id=@rejecter_id,approver_id=@approver_id,
Status=@Status where remediation_plan_id=@remediation_plan_id ");


                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, con))
                        {
                            // Map the parameters from the model
                            cmd.Parameters.AddWithValue("@remediation_plan_id", compliance.remediation_plan_id);
                            cmd.Parameters.AddWithValue("@remediation_rejection_remarks", remediationPlanModel.remediation_rejection_remarks);
                            cmd.Parameters.AddWithValue("@rejecter_id", remediationPlanModel.rejecter_id);
                            cmd.Parameters.AddWithValue("@approver_id", 0);
                            if (compliance.complianceStage == "Extension Applied")
                            {
                                cmd.Parameters.AddWithValue("@compliance_stage_progress", "Due");
                                cmd.Parameters.AddWithValue("@compliance_status", "Due");
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@compliance_stage_progress", "Overdue");
                                cmd.Parameters.AddWithValue("@compliance_status", "Overdue");
                            }
                            
                            cmd.Parameters.AddWithValue("@rpa_reject_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Status", "Active");


                            // Execute the query
                            cmd.ExecuteNonQuery();
                        }
                    }

                    return Ok("Data successfully inserted into the remediation_plan table.");
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine("Error while inserting data: " + ex.Message);
                    return BadRequest($"Error: {ex.Message}");
                }
                finally
                {
                    // Ensure the connection is closed even if an error occurs
                    con.Close();
                }
            }
        }

    }
}
