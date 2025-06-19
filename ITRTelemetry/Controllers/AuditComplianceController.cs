using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System.Configuration;
using System.Collections;
using MySql.Data.MySqlClient;
using DomainModel;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    // [ApiController]
    [Produces("application/json")]
    public class AuditCompliance : ControllerBase
    {

        private MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public AuditCompliance(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/GetAuditComplianceListUserWise/GetAuditComplianceListUserWise")]
        [HttpGet]
        public List<Auditcomplaincelistmodel> GetAuditComplianceListUserWise(int userid)
        {
            try
            {

                List<Auditcomplaincelistmodel> pdata = new List<Auditcomplaincelistmodel>();
                MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
                con.Open();
                MySqlCommand cmd = new MySqlCommand(@"SELECT Distinct entity_master.Entity_Master_Name,unit_location_master.Unit_location_Master_name,create_company_compliance.create_company_compliance_id,batch_compliance.location_department_mapping_id,batch_compliance.compliance_id,create_company_compliance.compliance_name,actregulatoryname,act_rule_name,
section_rule_regulation_ref,compliance_type_name,law_Categoryname,extended_due_date_required,Effective_StartDate,compliance_due_date_created,compliance_stage_progress,CONCAT(DATE_FORMAT(effective_StartDate, '%b %Y'), ' - ', DATE_FORMAT(compliance_due_date_created, '%b %Y')) AS ComplaincePeriod1,
batch_compliance.compliance_period as ComplaincePeriod,
CASE 
        WHEN update_compliance.compliance_id IS NOT NULL and Include_Review_Activity_Workflow=1  THEN update_compliance.compliance_stage_progress
                WHEN compliance_due_date_created < CURDATE() THEN 'Overdue'  -- Original complianceStage logic
        ELSE 'Due'
    END AS complianceStage,compliance_risk_criteria_name,compliance_type.compliance_type_name,`department-locationmapping`.entityid,
`department-locationmapping`.unitlocationid,No_of_Attachements,Make_Attachement_Mandatory,CASE 
        -- If due, show how many days are left
        WHEN compliance_due_date_created >= CURDATE() THEN CONCAT(DATEDIFF(compliance_due_date_created, CURDATE()), ' Days Left')  
        
        -- If overdue, show how many days have passed since the due date
        WHEN compliance_due_date_created < CURDATE() THEN CONCAT(DATEDIFF(CURDATE(), compliance_due_date_created), ' Days Overdue')  
    END AS days_status,Include_Audit_Workflow,Include_Authorization_Workflow_required,compliance_status,compliance_description,Include_Review_Activity_Workflow,Include_Approve_Activity_Workflow,
update_compliance.CreatedDate,applicability_status,
    actual_complied_date,amount_paid,penalty_paid,updation_remarks,update_compliance_id
FROM complaince_user_activity_mapping
JOIN complaince_user_mapping 
    ON complaince_user_mapping.Complaince_User_Mapping_id = complaince_user_activity_mapping.Complaince_User_Mapping_id
JOIN create_company_compliance 
    ON FIND_IN_SET(create_company_compliance.create_company_compliance_id, complaince_user_mapping.company_compliance_ids)
join compliance_location_mapping on compliance_location_mapping.companycomplianceid=create_company_compliance.create_company_compliance_id
join `department-locationmapping` on `department-locationmapping`.locationdepartmentmappingid=compliance_location_mapping.locationdepartmentmappingid
join entity_master on entity_master.Entity_Master_id=`department-locationmapping`.entityid
join unit_location_master on unit_location_master.Unit_location_Master_id=`department-locationmapping`.unitlocationid
join act_regulatorymaster on act_regulatorymaster.actregulatoryid=create_company_compliance.act_id
join act_rule_regulatory on act_rule_regulatory.act_rule_regulatory_id=create_company_compliance.rule_id
join compliance_type on compliance_type.compliance_type_id=create_company_compliance.compliance_type_id
join categoey_of_law on categoey_of_law.category_of_law_ID=create_company_compliance.category_of_law_id
join batch_compliance on batch_compliance.create_company_compliance_id=create_company_compliance.create_company_compliance_id
left join compliance_risk_classification_criteria on compliance_risk_classification_criteria.compliance_risk_criteria_id=create_company_compliance.risk_classification_criteria_id
join generate_current_batch_compliance on batch_compliance.generate_current_batch_compliance_id = generate_current_batch_compliance.id  and 
generate_current_batch_compliance.auditUser=complaince_user_activity_mapping.AuditActivity
inner join update_compliance on update_compliance.compliance_id=batch_compliance.compliance_id 
where AuditActivity=@userid and update_compliance.compliance_stage_progress='Audit Pending'
 ", con);


                cmd.CommandType = CommandType.Text;
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                cmd.Parameters.AddWithValue("@userid", userid);
                DataTable dt = new DataTable();
                da.Fill(dt);
                con.Close();


                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)

                    {


                        int update_compliance_ids = Convert.ToInt32(dt.Rows[i]["update_compliance_id"].ToString());

                        MySqlCommand cmd1 = new MySqlCommand(@"select * from update_compliance_attachments where update_compliance_id=@update_compliance_id and status=@status", con);


                        cmd1.CommandType = CommandType.Text;
                        MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                        cmd1.Parameters.AddWithValue("@update_compliance_id", update_compliance_ids);
                        cmd1.Parameters.AddWithValue("@status", "Active");
                        DataTable dt1 = new DataTable();
                        da1.Fill(dt1);

                        List<updatefiledocuments> documents = new List<updatefiledocuments>();
                        foreach (DataRow attachmentRow in dt1.Rows)
                        {
                            documents.Add(new updatefiledocuments
                            {
                                id = Convert.ToInt32(attachmentRow["id"]),
                                update_compliance_id = Convert.ToInt32(attachmentRow["update_compliance_id"]),
                                file_name = attachmentRow["file_name"].ToString(),
                                file_type = attachmentRow["file_type"].ToString(),
                                file_path = attachmentRow["file_path"].ToString(),
                                nature_of_attachment = attachmentRow["nature_of_attachment"].ToString()
                            });
                        }
                        string audit = "";
                        string auth = "";
                        string review = "";
                        string approve = "";

                        if (dt.Rows[i]["Include_Audit_Workflow"].ToString() == "1")
                            audit = "Yes";
                        else
                            audit = "No";
                        if (dt.Rows[i]["Include_Authorization_Workflow_required"].ToString() == "1")
                            auth = "Yes";
                        else
                            auth = "No";

                        if (dt.Rows[i]["Include_Review_Activity_Workflow"].ToString() == "1")
                            review = "Yes";
                        else
                            review = "No";
                        if (dt.Rows[i]["Include_Approve_Activity_Workflow"].ToString() == "1")
                            approve = "Yes";
                        else
                            approve = "No";
                        pdata.Add(new Auditcomplaincelistmodel
                        {
                            compliance_id = dt.Rows[i]["compliance_id"].ToString(),
                            create_company_compliance_id = Convert.ToInt32(dt.Rows[i]["create_company_compliance_id"]),
                            Entity_Master_Name = dt.Rows[i]["Entity_Master_Name"].ToString(),
                            Unit_location_Master_name = dt.Rows[i]["Unit_location_Master_name"].ToString(),
                            compliance_name = dt.Rows[i]["compliance_name"].ToString(),
                            act_rule_name = dt.Rows[i]["act_rule_name"].ToString(),
                            actregulatoryname = dt.Rows[i]["actregulatoryname"].ToString(),
                            section_rule_regulation_ref = dt.Rows[i]["section_rule_regulation_ref"].ToString(),
                            compliance_type_name = dt.Rows[i]["compliance_type_name"].ToString(),
                            law_Categoryname = dt.Rows[i]["law_Categoryname"].ToString(),
                            ComplaincePeriod = dt.Rows[i]["ComplaincePeriod"].ToString(),
                            complianceStage = dt.Rows[i]["complianceStage"].ToString(),
                            riskClassification = dt.Rows[i]["compliance_risk_criteria_name"].ToString(),
                            days_status = dt.Rows[i]["days_status"].ToString(),
                            entityid = Convert.ToInt32(dt.Rows[i]["entityid"]),
                            unitlocationid = Convert.ToInt32(dt.Rows[i]["unitlocationid"]),
                            //Department_Master_name = dt.Rows[i]["Department_Master_name"].ToString(),
                            //departmentid = Convert.ToInt32(dt.Rows[i]["departmentid"]),
                            No_of_Attachements = Convert.ToInt32(dt.Rows[i]["No_of_Attachements"]),
                            mandatory = Convert.ToBoolean(dt.Rows[i]["Make_Attachement_Mandatory"]),
                            Effective_StartDate = Convert.ToDateTime(dt.Rows[i]["Effective_StartDate"]),
                            due_date = Convert.ToDateTime(dt.Rows[i]["compliance_due_date_created"]),
                            compliance_stage_progress = dt.Rows[i]["compliance_stage_progress"].ToString(),
                            //approved_remedial_comply_date = dt.Rows[i]["approved_remedial_comply_date"].ToString() != "" ? Convert.ToDateTime(dt.Rows[i]["approved_remedial_comply_date"]).ToString() : "N/A",
                            Approve_Workflow = approve,
                            Review_Workflow = review,
                            compliance_description = dt.Rows[i]["compliance_description"].ToString(),
                            applicability_status = dt.Rows[i]["applicability_status"].ToString(),
                            actual_complied_date = Convert.ToDateTime(dt.Rows[i]["actual_complied_date"]),
                            ComplainceUpdatedDate = Convert.ToDateTime(dt.Rows[i]["CreatedDate"]),
                            amount_paid = dt.Rows[i]["amount_paid"].ToString(),
                            penalty_paid = dt.Rows[i]["penalty_paid"].ToString(),
                            updation_remarks = dt.Rows[i]["updation_remarks"].ToString(),
                            audit_status = "auditDue",
                            location_department_mapping_id = Convert.ToInt32(dt.Rows[i]["location_department_mapping_id"]),
                            AuditWorkflow = audit,
                            compliance_status = dt.Rows[i]["compliance_status"].ToString(),
                            AuthWorkflow = auth,
                            update_compliance_id = Convert.ToInt32(dt.Rows[i]["update_compliance_id"].ToString()),
                            updatefiledocuments = documents

                        });

                    }
                }
                return pdata;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; // Or any other appropriate action
            }

        }

        [Route("api/AuditCompliance/AuditUpdateComplianceDetails")]
        [HttpPut]
        public async Task<IActionResult> AuditUpdateComplianceDetails([FromBody] List<UpdateComplianceModel> updateComplianceModel)
        {
            if (updateComplianceModel == null || !updateComplianceModel.Any())
            {
                return StatusCode(500, new { message = "Error in saving: No compliance data provided." });
            }

            try
            {
                foreach (var update in updateComplianceModel)
                {
                    var existingCompliance = await mySqlDBContext.UpdateComplianceModels
                        .FirstOrDefaultAsync(c => c.update_compliance_id == update.update_compliance_id);

                    if (existingCompliance == null)
                    {
                        return StatusCode(500, new { message = "Error in saving: Compliance record not found." });
                    }

                        if (existingCompliance.actual_complied_date <= existingCompliance.due_date)
                    {
                        existingCompliance.compliance_stage_progress = "Completed";
                        existingCompliance.compliance_status = "Complied";
                    }

                    else if (existingCompliance.actual_complied_date > existingCompliance.due_date)
                    {
                        existingCompliance.compliance_stage_progress = "Completed";
                        existingCompliance.compliance_status = "Late Complied";
                    }

                    existingCompliance.audit_remarks = update.approval_remarks;
                    existingCompliance.audit_date = DateTime.Now;
                    existingCompliance.updated_by = update.updated_by;

                    mySqlDBContext.UpdateComplianceModels.Update(existingCompliance);

                }

                await mySqlDBContext.SaveChangesAsync();

                return Ok(new { message = "Compliance updated successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error in saving data" });
            }
        }

        [Route("api/AuditCompliance/AuditRejectCompliance")]
        [HttpDelete]
        public async Task<IActionResult> AuditRejectCompliance([FromBody] List<UpdateComplianceModel> bulkrejectComplianceModel)
        {
            try
            {
                foreach (var rejectCompliance in bulkrejectComplianceModel)
                {

                    if (bulkrejectComplianceModel == null)
                    {
                        return BadRequest("No data provided.");
                    }

                    var existingCompliance = mySqlDBContext.UpdateComplianceModels
                        .FirstOrDefault(c => c.update_compliance_id == rejectCompliance.update_compliance_id);

                    if (existingCompliance == null)
                    {
                        return NotFound("Compliance not found.");
                    }
                    if (DateTime.Now.Date < existingCompliance.due_date)
                    {
                        existingCompliance.compliance_status = "Due";
                    }
                    else
                    {
                        existingCompliance.compliance_status = "Over Due";
                    }

                    existingCompliance.compliance_stage_progress = "Update Pending";
                    existingCompliance.reject_remarks = rejectCompliance.approval_remarks;
                    existingCompliance.reject_date = DateTime.Now;
                    existingCompliance.updated_by = rejectCompliance.updated_by;

                    var batchModelCompliance = mySqlDBContext.BatchComplianceModels.FirstOrDefault(x => x.compliance_id == rejectCompliance.compliance_id);
                    batchModelCompliance.status = "Active";
                    mySqlDBContext.Update(batchModelCompliance);

                    mySqlDBContext.SaveChanges();
                }
                return Ok(new { message = "Compliance Reject successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in saving data" });
            }
        }

    }
}
