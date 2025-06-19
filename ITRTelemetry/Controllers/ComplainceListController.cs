using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
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
    public class ComplainceListController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public ComplainceListController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/ComplainceListController/GetComplainceListUserWise")]
        [HttpGet]
        public List<complaincelistmodel> GetComplainceListUserWise(int userid)
       
         {
            try
            {
                MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
                con.Open();
                MySqlCommand cmd = new MySqlCommand(@"


 SELECT Distinct entity_master.Entity_Master_Name,unit_location_master.Unit_location_Master_name,create_company_compliance.create_company_compliance_id,batch_compliance.compliance_id,batch_compliance.location_department_mapping_id,create_company_compliance.compliance_name,actregulatoryname,act_rule_name,
section_rule_regulation_ref,compliance_type_name,law_Categoryname,extended_due_date_required,Effective_StartDate,compliance_due_date_created,CONCAT(DATE_FORMAT(effective_StartDate, '%b %Y'), ' - ', DATE_FORMAT(compliance_due_date_created, '%b %Y')) AS ComplaincePeriod1,
batch_compliance.compliance_period as ComplaincePeriod,
CASE 
        WHEN remediation_plan.compliance_id IS NOT NULL THEN remediation_plan.compliance_status
                WHEN compliance_due_date_created < CURDATE() THEN 'Overdue'  -- Original complianceStage logic
        ELSE 'Due'
    END AS complianceStage,compliance_risk_criteria_name,compliance_type.compliance_type_name,`department-locationmapping`.entityid,
`department-locationmapping`.unitlocationid,No_of_Attachements,Make_Attachement_Mandatory,CASE 
        -- If due, show how many days are left
        WHEN compliance_due_date_created >= CURDATE() THEN CONCAT(DATEDIFF(compliance_due_date_created, CURDATE()), ' Days Left')  
        -- If overdue, show how many days have passed since the due date
        WHEN compliance_due_date_created < CURDATE() THEN CONCAT(DATEDIFF(CURDATE(), compliance_due_date_created), ' Days Overdue')  
    END AS days_status,Include_Audit_Workflow,compliance_status,Include_Authorization_Workflow_required,compliance_description,proposed_remedial_comply_date,Include_Review_Activity_Workflow,Include_Approve_Activity_Workflow
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
left join compliance_risk_classification_criteria on compliance_risk_classification_criteria.compliance_risk_criteria_id=create_company_compliance.risk_classification_criteria_id
join batch_compliance on batch_compliance.create_company_compliance_id=create_company_compliance.create_company_compliance_id
join generate_current_batch_compliance on batch_compliance.generate_current_batch_compliance_id = generate_current_batch_compliance.id  and 
generate_current_batch_compliance.updateUser=complaince_user_activity_mapping.UpdateActivity
left join remediation_plan on remediation_plan.compliance_id=batch_compliance.compliance_id 
where UpdateActivity=@userid and batch_compliance.status = 'Active'  AND (
        -- Fetch past compliances
        compliance_due_date_created < CURDATE()
        -- Fetch upcoming compliances in the next 90 days
        or compliance_due_date_created BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL (SELECT CAST(parameters AS SIGNED) 
         FROM risk.app_specific_settings 
         WHERE configuration = 'Set Compliance Period' 
         LIMIT 1) DAY)
    ) Order by compliance_due_date_created;
 ", con);


                cmd.CommandType = CommandType.Text;
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                cmd.Parameters.AddWithValue("@userid", userid);
                DataTable dt = new DataTable();
                da.Fill(dt);
                con.Close();

                List<complaincelistmodel> pdata = new List<complaincelistmodel>();

                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
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
                        pdata.Add(new complaincelistmodel
                        {
                           compliance_id =dt.Rows[i]["compliance_id"].ToString(),
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
                            Effective_EndDate = Convert.ToDateTime(dt.Rows[i]["compliance_due_date_created"]),
                            due_date = Convert.ToDateTime(dt.Rows[i]["compliance_due_date_created"]),
                            proposed_remedial_comply_date = dt.Rows[i]["proposed_remedial_comply_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dt.Rows[i]["proposed_remedial_comply_date"]),
                            location_department_mapping_id = Convert.ToInt32(dt.Rows[i]["location_department_mapping_id"]),
                            Approve_Workflow = approve,
                            Review_Workflow=review,
                            compliance_status = dt.Rows[i]["compliance_status"].ToString(),
                            compliance_description = dt.Rows[i]["compliance_description"].ToString(),
                            AuditWorkflow = audit,
                            AuthWorkflow = auth,

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


        [Route("api/ComplainceListController/GetRemidationComplainceListUserWise")]
        [HttpGet]
        public List<complaincelistmodel> GetRemidationComplainceListUserWise(int userid)

        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"   SELECT Distinct entity_master.Entity_Master_Name,unit_location_master.Unit_location_Master_name,create_company_compliance.create_company_compliance_id,batch_compliance.location_department_mapping_id,batch_compliance.compliance_id,create_company_compliance.compliance_name,actregulatoryname,act_rule_name,
section_rule_regulation_ref,compliance_type_name,law_Categoryname,extended_due_date_required,Effective_StartDate,compliance_due_date_created,
Effective_EndDate,CONCAT(DATE_FORMAT(effective_StartDate, '%b %Y'), ' - ',
 DATE_FORMAT(effective_EndDate, '%b %Y')) AS ComplaincePeriod,
CASE 
        WHEN effective_EndDate < CURDATE() THEN 'Overdue'
        ELSE 'Due'
    END AS complianceStage,compliance_risk_criteria_name,compliance_type.compliance_type_name,`department-locationmapping`.entityid,
`department-locationmapping`.unitlocationid,No_of_Attachements,CASE 
        -- If due, show how many days are left
        WHEN effective_EndDate >= CURDATE() THEN CONCAT(DATEDIFF(effective_EndDate, CURDATE()), ' Days Left')  
        
        -- If overdue, show how many days have passed since the due date
        WHEN effective_EndDate < CURDATE() THEN CONCAT(DATEDIFF(CURDATE(), effective_EndDate), ' Days Overdue')  
    END AS days_status,compliance_stage_progress,proposed_remedial_comply_date,rpa_request_date,remediation_request_remarks,
remediation_plan_id,compliance_status,Include_Audit_Workflow,Include_Authorization_Workflow_required,compliance_description

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
left join compliance_risk_classification_criteria on compliance_risk_classification_criteria.compliance_risk_criteria_id=create_company_compliance.risk_classification_criteria_id
join batch_compliance on batch_compliance.create_company_compliance_id=create_company_compliance.create_company_compliance_id
join generate_current_batch_compliance on batch_compliance.generate_current_batch_compliance_id = generate_current_batch_compliance.id  and 
generate_current_batch_compliance.remediationUser=complaince_user_activity_mapping.RemediationActivity
join remediation_plan on remediation_plan.compliance_id=batch_compliance.compliance_id 
where RemediationActivity=@userid and batch_compliance.status = 'Active' and( compliance_stage_progress ='Extension Applied' or compliance_stage_progress= 'Remediation Applied' ) 
order by create_company_compliance.create_company_compliance_id


  ", con);


            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@userid", userid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            List<complaincelistmodel> pdata = new List<complaincelistmodel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string audit = "";
                    string auth = "";

                    if (dt.Rows[i]["Include_Audit_Workflow"].ToString() == "1")
                        audit = "Yes";
                    else
                        audit = "No";
                    if (dt.Rows[i]["Include_Authorization_Workflow_required"].ToString() == "1")
                        auth = "Yes";
                    else
                        auth = "No";


                    pdata.Add(new complaincelistmodel
                    {
                        create_company_compliance_id = Convert.ToInt32(dt.Rows[i]["create_company_compliance_id"]),
                        
                        compliance_id = dt.Rows[i]["compliance_id"].ToString(),
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
                        compliance_stage_progress = dt.Rows[i]["compliance_stage_progress"].ToString(),
                        entityid = Convert.ToInt32(dt.Rows[i]["entityid"]),
                        unitlocationid = Convert.ToInt32(dt.Rows[i]["unitlocationid"]),
                        //Department_Master_name = dt.Rows[i]["Department_Master_name"].ToString(),
                        //departmentid = Convert.ToInt32(dt.Rows[i]["departmentid"]),
                        No_of_Attachements = Convert.ToInt32(dt.Rows[i]["No_of_Attachements"]),
                        remediation_plan_id = Convert.ToInt32(dt.Rows[i]["remediation_plan_id"]),
                        location_department_mapping_id = Convert.ToInt32(dt.Rows[i]["location_department_mapping_id"]),
                        due_date = Convert.ToDateTime(dt.Rows[i]["compliance_due_date_created"]),
                        Effective_StartDate = Convert.ToDateTime(dt.Rows[i]["Effective_StartDate"]),
                        Effective_EndDate = Convert.ToDateTime(dt.Rows[i]["Effective_EndDate"]),
                        proposed_remedial_comply_date = dt.Rows[i]["proposed_remedial_comply_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dt.Rows[i]["proposed_remedial_comply_date"]),
                        rpa_request_date = dt.Rows[i]["rpa_request_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dt.Rows[i]["rpa_request_date"]),
                        remediation_request_remarks = dt.Rows[i]["remediation_request_remarks"].ToString(),
                        compliance_status = dt.Rows[i]["compliance_status"].ToString(),
                        compliance_description = dt.Rows[i]["compliance_description"].ToString(),
                     AuditWorkflow=audit,
                     AuthWorkflow=auth,



                    });
                }
            }
            return pdata;
        }



    }
}
