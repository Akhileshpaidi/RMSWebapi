using DomainModel;
using DomainModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System.IO;
using MySql.Data.MySqlClient;
using iText.StyledXmlParser.Jsoup.Select;
using System.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Components.Forms;


namespace ITRTelemetry.Controllers
{

    [Produces("application/json")]
    public class ViewMitigationController :ControllerBase
    {
   private readonly MySqlDBContext mySqlDBContext;
            private object random;

            public IConfiguration Configuration { get; }


            public ViewMitigationController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
            {
                this.mySqlDBContext = mySqlDBContext;
                Configuration = configuration;

            }


        //Get method for view mitigation first dropdown 




        [Route("api/ViewMitigationController/GetViewAssessmentActionList")]
        [HttpGet]
        public IEnumerable<ViewMitigationModelGet> GetAssessmentList()
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"  
select distinct m.mitigations_id,m.ass_template_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
    ANY_VALUE(stbl.status) AS sug_status,
    ANY_VALUE(ab.assessment_name) AS assessment_name,
    ANY_VALUE(ab.assessment_description) AS assessment_description,
    DATE(ANY_VALUE(ab.created_date)) AS created_date,
    ANY_VALUE(ab.status) AS status,
    ANY_VALUE(ab.keywords) AS keywords,
    ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
    ANY_VALUE(tn.Type_Name) AS Type_Name,
    ANY_VALUE(sn.SubType_Name) AS SubType_Name,
    ANY_VALUE(cn.Competency_Name) AS Competency_Name,
    DATE(ANY_VALUE(startDate)) AS startDate,
    DATE(ANY_VALUE(endDate)) AS endDate,
    ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
    sa.uq_ass_schid,
    ANY_VALUE(mapped_user) AS mapped_user,
ANY_VALUE(ab.Competency_id) as Competency_id,
ANY_VALUE(sa.Schedule_Assessment_id) as Schedule_Assessment_id ,
        ANY_VALUE(sas.StartDateTime) as StartDateTime,
       ANY_VALUE(sas.EndDateTime) as EndDateTime,
        stbl.TrackerID,(select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no
from assessment_builder_versions ab
   JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN 
    risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id
JOIN	
   scheduled_ass_status sas on sas.AssessementTemplateID=ab.ass_template_id
JOIN risk.mitigations m on m.uq_ass_schid=sa.uq_ass_schid
 
JOIN	
   suggestions_tbl stbl on stbl.mitigations_id=m.mitigations_id
   



   where stbl.TrackerID is not null and (stbl.status='Processing' or stbl.status='Assigned' or stbl.status='Completed')
   
GROUP BY stbl.TrackerID, m.mitigations_id ", con);


            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<ViewMitigationModelGet>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                   
                    pdata.Add(new ViewMitigationModelGet
                    {

                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        mitigations_id = Convert.ToInt32(dt.Rows[i]["mitigations_id"]),
                        TrackerID = dt.Rows[i]["TrackerID"].ToString(),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        Competency_id = Convert.ToInt32(dt.Rows[i]["Competency_id"]),
                       ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        verson_no = dt.Rows[i]["verson_no"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        sug_status = dt.Rows[i]["sug_status"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        
                    });
                }
            }

            return pdata;
        }









        [Route("api/ViewMitigationController/GetViewAssessmentActionList/{id}")]
        [HttpGet]
        public IEnumerable<ViewMitigationModel> GetAssessmentList(int id)
        {

            var pdata = new List<ViewMitigationModel>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand(@"
                     SELECT
    st.suggestions_id,
    st.mitigations_id,
    st.suggestions,
    st.status,
    st.created_date,
    st.suggested_by,
    st.remarks,
    st.acknowledge_by,
    st.action_required,
    st.notify_management,
    st.input_date,
    st.assign_responsibility,
    st.tentative_timeline,
    st.suggested_documents,
    st.action_priority,
    st.management_remarks,
    st.acknowledge,
st.completed_date,
    st.PO_remarks,
    st.file_path,
st.file_name,
  st.TrackerID,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name,
 MAX(assnmae.firstname) AS assnmae
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
LEFT JOIN
    risk.tbluser AS assnmae ON assnmae.usr_ID = st.assign_responsibility
WHERE
   st.TrackerID = " + id + " and (st.status='Processing' or st.status='Assigned' or st.status='Completed') GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions", con);

                cmd.CommandType = CommandType.Text;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new ViewMitigationModel
                    {
                        suggestions_id = dt.Rows[i]["suggestions_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["suggestions_id"]) : (int?)null,
                        mitigations_id = dt.Rows[i]["mitigations_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["mitigations_id"]) : (int?)null,
                        TrackerID = dt.Rows[i]["TrackerID"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        suggestions = dt.Rows[i]["suggestions"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        suggested_by = dt.Rows[i]["suggested_by"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["suggested_by"]) : (int?)null,
                        tentative_timeline = dt.Rows[i]["tentative_timeline"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["tentative_timeline"]).Date : (DateTime?)null,
                        input_date = dt.Rows[i]["input_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["input_date"]).Date : (DateTime?)null,
                        suggested_documents = dt.Rows[i]["suggested_documents"].ToString(),
                        remarks = dt.Rows[i]["remarks"].ToString(),
                        action_required = dt.Rows[i]["action_required"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["action_required"]) : (int?)null,
                        management_remarks = dt.Rows[i]["management_remarks"].ToString(),
                        action_priority = dt.Rows[i]["action_priority"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["action_priority"]) : (int?)null,
                        acknowledge = dt.Rows[i]["acknowledge"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["acknowledge"]) : (int?)null,
                        PO_remarks = dt.Rows[i]["PO_remarks"].ToString(),
                        file_path = dt.Rows[i]["file_path"].ToString(),
                        file_name = dt.Rows[i]["file_name"].ToString(),
                        Suggester_Name = dt.Rows[i]["Suggester_Name"].ToString(),
                        Acknowledger_Name = dt.Rows[i]["Acknowledger_Name"].ToString(),
                        completed_date = dt.Rows[i]["completed_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["completed_date"]).Date : (DateTime?)null,
                        assnmae        = $"{dt.Rows[i]["status"].ToString()}-{dt.Rows[i]["assnmae"].ToString()}"
                    });



                }





                return pdata;
            }


        }















        //get method for risk Assessor.  status is Assigned and based on role_id and trackid



        [Route("api/ViewMitigationController/GetViewMitigationAssigned/{Trackid}")]
        [HttpGet]
        public IEnumerable<ViewMitigationModel> GetAssessmentsListManagement(int Trackid)
        {

            List<ViewMitigationModel> suggestionsList = new List<ViewMitigationModel>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(@"
 SELECT " +
    "st.suggestions_id, " +
    "st.mitigations_id, " +
    "st.suggestions, " +
    "st.status, " +
    "st.created_date, " +
    "st.suggested_by, " +
    "st.remarks, " +
    "st.acknowledge_by, " +
    "st.action_required, " +
    "st.notify_management, " +
    "st.input_date, " +
    "st.assign_responsibility, " +
    "st.tentative_timeline, " +
    "st.suggested_documents, " +
    "st.action_priority, " +
    "st.comments, " +
    "st.acknowledge, " +
    "st.TrackerID, " +
    "tblrole.ROLE_ID, " +
    "MAX(CASE WHEN st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '7' AND st.status = 'Pending' THEN tblSuggester.firstname END) AS Suggester_Pending, " +
    "MAX(CASE WHEN st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '7' AND st.status = 'Pending' THEN tblAcknowledger.firstname END) AS Acknowledger_Pending, " +
    "MAX(CASE WHEN st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '5' AND st.status = 'Assigned' THEN tblSuggester.firstname END) AS Suggester_Assigned, " +
    "MAX(CASE WHEN st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '5' AND st.status = 'Assigned' THEN tblAcknowledger.firstname END) AS Acknowledger_Assigned, " +
    "MAX(CASE WHEN st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '8' AND st.status = 'completed' THEN tblSuggester.firstname END) AS Suggester_completed, " +
    "MAX(CASE WHEN st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '8' AND st.status = 'completed' THEN tblAcknowledger.firstname END) AS Acknowledger_completed " +
    "FROM " +
    "risk.suggestions_tbl AS st " +
    "LEFT JOIN " +
    "risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by " +
    "LEFT JOIN " +
    "risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by " +
    "LEFT JOIN " +
    "risk.tblrole AS tblrole ON tblrole.ROLE_ID = tblAcknowledger.ROLE_ID " +
    "WHERE " +
    "(st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '7' AND st.status = 'Pending') " +
    "OR (st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '5' AND st.status = 'Assigned') " +
    "OR (st.TrackerID = '" + Trackid + "' AND st.ROLE_ID = '8' AND st.status = 'completed') " +
    "GROUP BY " +
    "st.suggestions_id, " +
    "st.mitigations_id, " +
    "st.TrackerID", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new ViewMitigationModel
                            {
                                suggestions_id = row["suggestions_id"] != DBNull.Value ? Convert.ToInt32(row["suggestions_id"]) : (int?)null,
                                mitigations_id = row["mitigations_id"] != DBNull.Value ? Convert.ToInt32(row["mitigations_id"]) : (int?)null,
                                suggestions = row["suggestions"].ToString(),
                                TrackerID = row["TrackerID"].ToString(),
                                status = row["status"].ToString(),
                                created_date = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                suggested_by = row["suggested_by"] != DBNull.Value ? Convert.ToInt32(row["suggested_by"]) : (int?)null,
                                remarks = row["remarks"].ToString(),
                                acknowledge_by = row["acknowledge_by"] != DBNull.Value ? Convert.ToInt32(row["acknowledge_by"]) : (int?)null,
                                action_required = row["action_required"] != DBNull.Value ? Convert.ToInt32(row["action_required"]) : (int?)null,
                                notify_management = row["notify_management"] != DBNull.Value ? Convert.ToInt32(row["notify_management"]) : (int?)null,
                                input_date = row["input_date"] != DBNull.Value ? Convert.ToDateTime(row["input_date"]) : (DateTime?)null,
                                assign_responsibility = row["assign_responsibility"] != DBNull.Value ? Convert.ToInt32(row["assign_responsibility"]) : (int?)null,
                                tentative_timeline = row["tentative_timeline"] != DBNull.Value ? Convert.ToDateTime(row["tentative_timeline"]) : (DateTime?)null,
                                suggested_documents = row["suggested_documents"].ToString(),
                                action_priority = row["action_priority"] != DBNull.Value ? Convert.ToInt32(row["action_priority"]) : (int?)null,
                                comments = row["comments"].ToString(),
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                             
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }

            return suggestionsList;
        }



    }
}
