using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using MySQLProvider;
using System.Linq;
using Microsoft.VisualBasic;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ITRTelemetry.Controllers
{
    [Produces("Application/json")]
    public class UpdateComplianceController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        private static readonly HashSet<DayOfWeek> NonWorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };

        public UpdateComplianceController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }


        [Route("api/UpdateComplianceController/UpdateComplianceDetails")]
        [HttpPost]
        public async Task<IActionResult> UpdateComplianceDetails([FromForm] List<UpdateComplianceModel> updateComplianceModels)
        {
            if (updateComplianceModels == null || !updateComplianceModels.Any())
            {
                return BadRequest("No data provided.");
            }
            DateTime update_date = DateTime.Now;

            try
            {
                foreach (var model in updateComplianceModels)
                {
                    if (model == null) continue;
                    var newPlan = new UpdateComplianceModel
                    {
                        compliance_id = model.compliance_id,
                        amount_paid = model.amount_paid,
                        penalty_paid = model.penalty_paid,
                        applicability_status = model.applicability_status,
                        updation_remarks = model.updation_remarks,
                        due_date = model.due_date,
                        authWorkflow = model.authWorkflow,
                        review_Workflow = model.review_Workflow,
                        approve_Workflow = model.approve_Workflow,
                        auditWorkflow = model.auditWorkflow,
                        CreatedDate = DateTime.Now,
                        compliance_update_date = update_date,
                        created_by = model.created_by
                    };
                    // If you need to update another related model, ensure you fetch and update it
                    var batchModelCompliance = mySqlDBContext.BatchComplianceModels.FirstOrDefault(x => x.compliance_id == model.compliance_id);
                    //if (batchModelCompliance != null)
                    //{
                    //    batchModelCompliance.status = "Active";  // Update related model's status
                    //}

                    //int noOfDaysDefined = 5; // Example value from workflow configuration
                    List<DateTime> holidays = new List<DateTime> { new DateTime(2025, 1, 1), new DateTime(2025, 1, 2) }; // Example public holidays
                    int daysTakenToUpdate = GetDaysTakenToUpdate(update_date, model.actual_complied_date, holidays);
                    string updationActivityResult = GetUpdationActivityResult(daysTakenToUpdate);
                    newPlan.updation_activity_result = updationActivityResult;
                    newPlan.no_of_days_taken_to_update = daysTakenToUpdate.ToString();

                    bool isBeforeOrOnDueDate = model.actual_complied_date <= model.due_date;

                        if (model.authWorkflow == "No" && model.auditWorkflow == "No")
                        {
                            // No workflows required
                            newPlan.compliance_stage_progress = "Completed";
                            newPlan.compliance_status = isBeforeOrOnDueDate ? "Complied" : "Late Complied";
                        }
                        else if (model.authWorkflow == "Yes" && model.auditWorkflow == "No")
                        {
                            // Authorization Workflow Only
                            newPlan.compliance_stage_progress = "Review Pending";
                            newPlan.compliance_status = "Authorization in Progress";

                            // Check for review and approval workflow requirements
                            //if (model.review_Workflow == "Yes") 
                            //{
                            //    // Set compliance review due date and notify the reviewer
                            //  //  newPlan.compliance_review_due_date = CalculateReviewDueDate();
                            //    //SendNotification("Reviewer", model.compliance_id);
                            //}

                            //if (model.approve_Workflow == "Yes") 
                            //{
                            //    // Set approval due date after review completion
                            //  //  newPlan.compliance_approval_due_date = CalculateApprovalDueDate();
                            //   // SendNotification("Approver", model.compliance_id);
                            //}
                        }
                        else if (model.authWorkflow == "No" && model.auditWorkflow == "Yes")
                        {
                            // Audit Workflow Only
                            newPlan.compliance_stage_progress = "Audit Pending";
                            newPlan.compliance_status = "Audit in Progress";

                            // Set compliance audit due date and notify the auditor
                           // newPlan.compliance_audit_due_date = CalculateAuditDueDate();
                           // SendNotification("Auditor", model.compliance_id);
                        }
                        else if (model.authWorkflow == "Yes" && model.auditWorkflow == "Yes")
                        {
                            // Both Authorization and Audit Workflow
                            newPlan.compliance_stage_progress = "Review Pending";
                            newPlan.compliance_status = "Authorization in Progress";

                        //    if (model.review_Workflow == "Yes")
                        //{
                        //    newPlan.compliance_stage_progress = "Approval Pending";
                        //    newPlan.compliance_status = "Authorization in Progress";
                        //    // newPlan.compliance_review_due_date = CalculateReviewDueDate();
                        //    // SendNotification("Reviewer", model.compliance_id);
                        //}

                        //    if (model.approve_Workflow == "Yes")
                        //{
                        //    newPlan.compliance_stage_progress = "Review Pending";
                        //    newPlan.compliance_status = "Authorization in Progress";
                        //    // newPlan.compliance_approval_due_date = CalculateApprovalDueDate();
                        //    // SendNotification("Approver", model.compliance_id);
                        //}

                        ////if (model.approve_Workflow == "Yes" && model.auditWorkflow == "Yes") // Ensure both are "Yes"
                        //    if (model.auditWorkflow == "Yes") // Ensure both are "Yes"
                        //    {
                        //        //newPlan.compliance_audit_due_date = CalculateAuditDueDate();
                        //        newPlan.compliance_stage_progress = "Audit Pending";
                        //        newPlan.compliance_status = "Audit in Progress";
                        //       // SendNotification("Auditor", model.compliance_id);
                        //    }
                        }

                        // Validate and set actual_complied_date
                        if (model.applicability_status == "1" && model.actual_complied_date.HasValue)
                        {
                            if (model.actual_complied_date > DateTime.Now)
                            {
                                return BadRequest("Actual Compliance Date cannot be greater than today's date.");
                            }
                            newPlan.actual_complied_date = model.actual_complied_date;
                        }
                        else if (model.applicability_status == "2")
                        {
                            newPlan.actual_complied_date = DateTime.Now < model.due_date
                                ? DateTime.Now
                                : model.due_date;
                        }

                        // Save compliance details
                    mySqlDBContext.UpdateComplianceModels.Add(newPlan);
                    batchModelCompliance.status = newPlan.compliance_stage_progress;
                    mySqlDBContext.Update(batchModelCompliance);
                    mySqlDBContext.SaveChanges();

                        int updateComplianceID = newPlan.update_compliance_id;

                        // Handle file attachments if present
                        if (model.attachments != null && model.attachments.Any())
                        {
                            foreach (var attachment in model.attachments)
                            {
                                if (attachment?.file == null)
                                {
                                    continue; // Skip if attachment or file is null
                                }

                                var fileName = Path.GetFileNameWithoutExtension(attachment.file.FileName)
                                               + "_" + DateTime.Now.Ticks
                                               + Path.GetExtension(attachment.file.FileName);
                                var filePath = Path.Combine("UpdateComplianceFiles", fileName);

                                // Save file to server
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    attachment.file.CopyTo(stream);
                                }

                                var newAttachment = new UpdateComplianceFilesModel
                                {
                                    file_name = fileName,
                                    file_type = Path.GetExtension(fileName),
                                    file_path = filePath,
                                    nature_of_attachment = attachment.nature_of_attachment,
                                    update_compliance_id = updateComplianceID,
                                    status = "Active"
                                };
                            mySqlDBContext.UpdateComplianceFilesModels.Add(newAttachment);
                            }
                        //mySqlDBContext.Update(batchModelCompliance);
                         mySqlDBContext.SaveChanges();
                        }
                    }
                return Ok(new { message = "Compliance details and attachments inserted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in saving data" });
            }
        }
        public static int GetDaysTakenToUpdate(DateTime updationDate, DateTime? actualComplianceDate, List<DateTime> holidays)
        {
            if (actualComplianceDate == null)
            {
                throw new ArgumentNullException(nameof(actualComplianceDate), "Actual Compliance Date cannot be null");
            }

            DateTime calculatedDate = actualComplianceDate.Value.AddDays(1);

            // Adjust for non-working days
            while (NonWorkingDays.Contains(calculatedDate.DayOfWeek) || holidays.Contains(calculatedDate))
            {
                calculatedDate = calculatedDate.AddDays(1);
            }

            return (updationDate - calculatedDate).Days;
        }
        public static string GetUpdationActivityResult(int daysTakenToUpdate)
        {
            return daysTakenToUpdate == 0 ? "Within Due Time" : "Delayed";
        }
    }
    //private DateTime CalculateReviewDueDate() => DateTime.Now.AddDays(5);  // Customize logic here
    // private DateTime CalculateApprovalDueDate() => DateTime.Now.AddDays(10); // Customize logic here
    // private DateTime CalculateAuditDueDate() => DateTime.Now.AddDays(15); // Customize logic here

    // private void SendNotification(string role, int complianceId)
    // {
    // Logic to send notifications to specific roles (Reviewer, Approver, Auditor) based on workflow requirements
    // }
}
