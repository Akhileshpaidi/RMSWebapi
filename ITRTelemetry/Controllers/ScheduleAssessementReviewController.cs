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
    public class ScheduleAssessementReviewController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public ScheduleAssessementReviewController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }


        [Route("api/ScheduleAssessementReviewController/GetCompletedScheduleAssesment")]
        [HttpGet]
        public IEnumerable<AssScheduleModel> GetCompletedAssesment()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //            MySqlCommand cmd = new MySqlCommand(@"SELECT  
            //    ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
            //    sa.ass_template_id,
            //    ANY_VALUE(ab.assessment_name) AS assessment_name,
            //    ANY_VALUE(ab.assessment_description) AS assessment_description,
            //    DATE(ANY_VALUE(ab.created_date)) AS created_date,
            //    ANY_VALUE(ab.status) AS status,
            //    ANY_VALUE(ab.keywords) AS keywords,
            //    ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
            //    ANY_VALUE(tn.Type_Name) AS Type_Name,
            //    ANY_VALUE(sn.SubType_Name) AS SubType_Name,
            //    ANY_VALUE(cn.Competency_Name) AS Competency_Name,
            //    DATE(ANY_VALUE(sa.startDate)) AS startDate,
            //    DATE(ANY_VALUE(sa.endDate)) AS endDate,
            //    ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
            //ANY_VALUE( sa.pagetype) AS pagetype,
            //    sa.uq_ass_schid,
            //    ANY_VALUE(sa.mapped_user) AS mapped_user,
            //    ANY_VALUE(ab.Competency_id) AS Competency_id,
            //    ANY_VALUE(sas.StartDateTime) AS StartDateTime,
            //    ANY_VALUE(sas.EndDateTime) AS EndDateTime
            //FROM 
            //    risk.assessment_builder ab
            //JOIN 
            //    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
            //JOIN 
            //    risk.type tn ON tn.Type_id = ab.Type_id
            //JOIN 
            //    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
            //JOIN 
            //    risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id
            //JOIN 
            //    risk.scheduled_ass_status sas ON sas.AssessementTemplateID = ab.ass_template_id
            //WHERE 
            //    sa.uq_ass_schid IS NOT NULL 
            //    AND sas.Status = 'Assessment Completed' 
            //    AND sa.uq_ass_schid IN (
            //        SELECT uq_ass_schid
            //        FROM risk.schedule_assessment
            //        WHERE status = 'Active'
            //        GROUP BY uq_ass_schid
            //        HAVING COUNT(CASE WHEN AssessmentStatus != 'Assessment Completed' THEN 1 END) = 0
            //    ) OR sa.endDate <= CURDATE()
            //GROUP BY 
            //    sa.ass_template_id, sa.uq_ass_schid;
            // ", con);

            MySqlCommand cmd = new MySqlCommand(@"  SELECT  

    ab.ass_template_id,

    ANY_VALUE(ab.assessment_name) AS assessment_name,
    ANY_VALUE(ab.assessment_description) AS assessment_description,
    DATE(ANY_VALUE(sa.created_date)) AS created_date,
    ANY_VALUE(ab.status) AS status,
    ANY_VALUE(ab.keywords) AS keywords,
    ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
    ANY_VALUE(tn.Type_Name) AS Type_Name,
    ANY_VALUE(sn.SubType_Name) AS SubType_Name,
    ANY_VALUE(cn.Competency_Name) AS Competency_Name,
    DATE(ANY_VALUE(sa.startDate)) AS startDate,
    DATE(ANY_VALUE(sa.endDate)) AS endDate,
    ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
ANY_VALUE( sa.pagetype) AS pagetype,
    sa.uq_ass_schid,
    ANY_VALUE(sa.mapped_user) AS mapped_user,
    ANY_VALUE(ab.Competency_id) AS Competency_id,
    ANY_VALUE(sas.StartDateTime) AS StartDateTime,
    ANY_VALUE(sas.EndDateTime) AS EndDateTime,
      (select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no,
(SELECT COUNT(DISTINCT mapped_user)
     FROM risk.schedule_assessment
     WHERE uq_ass_schid = sa.uq_ass_schid) AS total_assigned_users,
 (SELECT COUNT(DISTINCT mapped_user)
     FROM risk.schedule_assessment
     WHERE uq_ass_schid = sa.uq_ass_schid
       AND AssessmentStatus = 'Assessment Completed') AS completed_users,
  CASE 
        WHEN (
            SELECT COUNT(*) 
            FROM risk.schedule_assessment sa2 
            WHERE sa2.uq_ass_schid = sa.uq_ass_schid
        ) = (
            SELECT COUNT(*) 
            FROM risk.schedule_assessment sa3 
            WHERE sa3.uq_ass_schid = sa.uq_ass_schid AND sa3.AssessmentStatus = 'Assessment Completed'
        )
        THEN 'Assessment Completed'
        ELSE 'Assessment in Progress'
    END AS OverallAssessmentStatus
FROM 
    risk.assessment_builder_versions ab
JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN 
    risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id and sa.verson_no = ab.verson_no
JOIN 
    risk.scheduled_ass_status sas ON sas.uq_ass_schid = sa.uq_ass_schid
WHERE 
    sa.uq_ass_schid IS NOT NULL 
    AND (sas.Status = 'Assessment Completed' ) and sa.verson_no=ab.verson_no and (sa.AssessmentStatus='Assessment Completed' or sa.AssessmentStatus='Assessment Rescheduled')
  
GROUP BY 
    ab.ass_template_id, sa.uq_ass_schid; ", con);
            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssScheduleModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssScheduleModel
                    {
                        pagetype = dt.Rows[i]["pagetype"].ToString(),
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        // Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        Competency_id = Convert.ToInt32(dt.Rows[i]["Competency_id"]),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["AssessmentStatus"].ToString(),
                        verson_no = dt.Rows[i]["verson_no"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        //firstname = dt.Rows[i]["Username"].ToString(),
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                        total_assigned_users = Convert.ToInt32(dt.Rows[i]["total_assigned_users"]),
                        completed_users = Convert.ToInt32(dt.Rows[i]["completed_users"]),
                        OverallAssessmentStatus = dt.Rows[i]["OverallAssessmentStatus"].ToString(),
                    });
                }
            }
            return pdata;
        }

        [Route("api/ScheduleAssessementReviewController/GetCounts")]
        [HttpGet]

        public IEnumerable<Getcounts0fAssessment> GetCounts(string AssessementTemplateID, string uq_ass_schid)
        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //=== For Version
            MySqlCommand cmd = new MySqlCommand(@"


                            select(select count(distinct mapped_user) from schedule_assessment where ass_template_id = @AssessementTemplateID and uq_ass_schid=@uq_ass_schid) as No_of_Users_Assigned,(select count(ass_template_id) from schedule_assessment where ass_template_id = @AssessementTemplateID and AssessmentStatus = 'Assessment Completed' and uq_ass_schid=@uq_ass_schid) as No_of_Users_completed,
            (select count(*) from schedule_assessment where ass_template_id = @AssessementTemplateID and AssessmentStatus = 'Assessement Initiated'and uq_ass_schid=@uq_ass_schid ) as No_of_Users_incomplete,
            (select count(*) from schedule_assessment where ass_template_id = @AssessementTemplateID and AssessmentStatus = 'Assessement Expired'and uq_ass_schid=@uq_ass_schid ) as No_of_Users_ass_Expired,Doc_CategoryName,DocTypeName,Doc_SubCategoryName,schedule_assessment.uq_ass_schid,
            (select assessment_name  from assessment_builder_versions where ass_template_id=@AssessementTemplateID) as assessment_name,schedule_assessment.endDate
             from schedule_assessment 
            join risk.doccategory_master on doccategory_master.Doc_CategoryID=schedule_assessment.Doc_CategoryID
            join risk.doctype_master on doctype_master.DocTypeID=schedule_assessment.DocTypeID
            join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=schedule_assessment.Doc_SubCategoryID
            where ass_template_id = @AssessementTemplateID and uq_ass_schid=@uq_ass_schid  group by Doc_CategoryName,DocTypeName,Doc_SubCategoryName,schedule_assessment.endDate ", con);
            //            MySqlCommand cmd = new MySqlCommand(@"
            //                   select(select count(distinct mapped_user) from schedule_assessment 
            //where ass_template_id = @AssessementTemplateID and verson_no=@verson_no) as No_of_Users_Assigned,
            //(select count(ass_template_id) from schedule_assessment where ass_template_id = @AssessementTemplateID 
            //and AssessmentStatus = 'Assessment Completed' and verson_no=@verson_no) as No_of_Users_completed,
            //(select count(*) from schedule_assessment where ass_template_id = @AssessementTemplateID 
            //and AssessmentStatus in( 'Assessement Initiated','Assessment Scheduled')and verson_no=@verson_no ) as No_of_Users_incomplete,
            //(select count(*) from schedule_assessment where ass_template_id = @AssessementTemplateID 
            //and AssessmentStatus = 'Assessement Expired'and verson_no=@verson_no ) as No_of_Users_ass_Expired,
            //Doc_CategoryName,DocTypeName,Doc_SubCategoryName,
            //(select assessment_name  from assessment_builder where ass_template_id=@AssessementTemplateID) as assessment_name
            // from schedule_assessment 
            //join risk.doccategory_master on doccategory_master.Doc_CategoryID=schedule_assessment.Doc_CategoryID
            //join risk.doctype_master on doctype_master.DocTypeID=schedule_assessment.DocTypeID
            //join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=schedule_assessment.Doc_SubCategoryID
            //where ass_template_id = @AssessementTemplateID and verson_no=@verson_no 
            // group by Doc_CategoryName,DocTypeName,Doc_SubCategoryName ", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Getcounts0fAssessment>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new Getcounts0fAssessment
                    {
                        
                        No_of_Users_Assigned = Convert.ToInt32(dt.Rows[i]["No_of_Users_Assigned"].ToString()),
                        No_of_Users_completed = Convert.ToInt32(dt.Rows[i]["No_of_Users_completed"].ToString()),
                        No_of_Users_incomplete = Convert.ToInt32(dt.Rows[i]["No_of_Users_incomplete"].ToString()),
                        No_of_Users_ass_Expired = Convert.ToInt32(dt.Rows[i]["No_of_Users_ass_Expired"].ToString()),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                    });

                }

            }
            return pdata;

        }


        //New My Mitigation Tas


        [Route("api/ScheduleAssessementReviewController/GetUsersScoreCounts")]
        [HttpGet]

        public IEnumerable<GetcountsofUserScore> GetUsersScoreCounts(string AssessementTemplateID, string uq_ass_schid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            MySqlCommand cmd = new MySqlCommand(@"
        SELECT
            user_ass_ans_details.UserID,
            tbluser.firstname,scheduled_ass_status.uq_ass_schid,
            COALESCE(kii.Key_Impr_Indicator_Name, '') AS Key_Impr_Indicator_Name,
            scheduled_ass_status.Remarks,tn.Type_Name,sn.SubType_Name, cn.Competency_Name,
            COUNT(DISTINCT questionbank.question_id) AS total_questions,

            (
                SELECT COUNT(question_id)
                FROM risk.user_ass_ans_details AS uad
                WHERE uad.AssessementTemplateID = @AssessementTemplateID
                AND uad.uq_ass_schid = @uq_ass_schid
                AND uad.UserID = user_ass_ans_details.UserID
            ) AS TotalQuestionsAnswered,

            (SELECT Percentage FROM scheduled_ass_status 
             WHERE AssessementTemplateID=@AssessementTemplateID 
             AND uq_ass_schid = @uq_ass_schid 
             AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS Percentages,

            (SELECT TaskOwnerScore FROM scheduled_ass_status 
             WHERE AssessementTemplateID=@AssessementTemplateID 
             AND uq_ass_schid = @uq_ass_schid 
             AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS OverallScore,

            (SELECT TotalScore FROM scheduled_ass_status 
             WHERE AssessementTemplateID=@AssessementTemplateID 
             AND uq_ass_schid = @uq_ass_schid 
             AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS TotalScoreofAssessement,

            (SELECT Score_Name 
             FROM score_indicator 
             WHERE Percentages BETWEEN scoreminrange AND scoremaxrange
             LIMIT 1) AS ScoreName,

           (
    SELECT CASE 
        WHEN DATEDIFF(sas.EndDateTime, sa.acknowledgemet_date) = 0 THEN 1
        ELSE DATEDIFF(sas.EndDateTime, sa.acknowledgemet_date)
    END  
    FROM scheduled_ass_status sas
    INNER JOIN schedule_assessment sa ON sas.uq_ass_schid = sa.uq_ass_schid
    WHERE sas.Status = 'Assessment Completed'
      AND sas.uq_ass_schid = @uq_ass_schid
      AND sas.AssessementTemplateID = @AssessementTemplateID
      AND sas.UserID = user_ass_ans_details.UserID
    LIMIT 1
) AS Days,

            (
                SELECT COUNT(*)
                FROM user_ass_ans_details uad2
                INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
                WHERE uad2.AssessementTemplateID = @AssessementTemplateID
                AND uad2.uq_ass_schid = @uq_ass_schid
                AND uad2.UserID = user_ass_ans_details.UserID
                AND uad2.user_Selected_Ans = qb2.correct_answer
            ) AS CorrectAnswers,

            (
                SELECT ROUND(
                    (
                        SELECT COUNT(*)
                        FROM user_ass_ans_details uad2
                        INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
                        WHERE uad2.AssessementTemplateID = @AssessementTemplateID
                        AND uad2.uq_ass_schid = @uq_ass_schid
                        AND uad2.UserID = user_ass_ans_details.UserID
                        AND uad2.user_Selected_Ans = qb2.correct_answer
                    ) * 100.0 / 
                    (
                        SELECT COUNT(*)
                        FROM user_ass_ans_details uad3
                        WHERE uad3.AssessementTemplateID = @AssessementTemplateID
                        AND uad3.uq_ass_schid = @uq_ass_schid
                        AND uad3.UserID = user_ass_ans_details.UserID
                    ), 2
                )
            ) AS AccuracyPercentage

        FROM
            assessment_builder_versions
        INNER JOIN assessment_generation_details ON assessment_generation_details.Assessment_generationID = assessment_builder_versions.Assessment_generationID
        INNER JOIN questionbank ON questionbank.question_id = assessment_generation_details.question_id
        INNER JOIN questionbank_options ON questionbank_options.question_id = questionbank.question_id
        INNER JOIN user_ass_ans_details ON user_ass_ans_details.question_id = questionbank.question_id AND user_ass_ans_details.uq_ass_schid = @uq_ass_schid
        INNER JOIN tbluser ON tbluser.USR_ID = user_ass_ans_details.UserID
        INNER JOIN scheduled_ass_status ON scheduled_ass_status.AssessementTemplateID = user_ass_ans_details.AssessementTemplateID 
            AND scheduled_ass_status.uq_ass_schid = @uq_ass_schid 
            AND scheduled_ass_status.UserID = user_ass_ans_details.UserID
        LEFT JOIN key_impr_indicator kii ON scheduled_ass_status.key_Impr_Indicator_id = kii.key_Impr_Indicator_id
        INNER JOIN risk.sub_type sn ON sn.SubType_id = assessment_builder_versions.SubType_id
INNER JOIN risk.type tn ON tn.Type_id = assessment_builder_versions.Type_id
INNER JOIN risk.competency_skill cn ON cn.Competency_id = assessment_builder_versions.Competency_id
        WHERE
            assessment_builder_versions.ass_template_id = @AssessementTemplateID

        GROUP BY
            user_ass_ans_details.UserID, tbluser.firstname, scheduled_ass_status.Remarks, kii.Key_Impr_Indicator_Name,tn.Type_Name,sn.SubType_Name, cn.Competency_Name;
    ", con);

            cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<GetcountsofUserScore>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string daysvalue = "";
                    int day11 = 0;

                    // Try parsing safely
                    if (int.TryParse(dt.Rows[i]["Days"].ToString(), out day11))
                    {
                        // if day11 is 0 or 1, treat it as "1 Day"
                        if (day11 <= 1)
                        {
                            day11 = 1;  // enforce minimum 1 day in display
                            daysvalue = " Day";
                        }
                        else
                        {
                            daysvalue = " Days";
                        }
                    }
                    else
                    {
                        // fallback if parsing fails
                        day11 = 1;
                        daysvalue = " Day";
                    }


                    pdata.Add(new GetcountsofUserScore
                    {
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        UserID = Convert.ToInt32(dt.Rows[i]["UserID"].ToString()),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        ScoreName = dt.Rows[i]["ScoreName"].ToString(),
                        Key_Impr_Indicator_Name = dt.Rows[i]["Key_Impr_Indicator_Name"].ToString(),
                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"].ToString()),
                        TotalQuestionsAnswered = Convert.ToInt32(dt.Rows[i]["TotalQuestionsAnswered"].ToString()),
                        OverallScore = dt.Rows[i]["OverallScore"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["OverallScore"].ToString()) : 0,
                        TotalScoreofAssessement = dt.Rows[i]["TotalScoreofAssessement"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["TotalScoreofAssessement"].ToString()) : 0,
                        Percentage = dt.Rows[i]["Percentages"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["Percentages"].ToString()) : 0,
                        Remarks = dt.Rows[i]["Remarks"].ToString(),
                        Days = day11 + daysvalue,
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"].ToString()),
                        AccuracyPercentage = dt.Rows[i]["AccuracyPercentage"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["AccuracyPercentage"].ToString()) : 0
                    });
                }
            }

            // To remove duplicate UserID records
            pdata = pdata.GroupBy(x => x.UserID).Select(g => g.First()).ToList();

            return pdata;
        }



        //---------------------kavya Query back up result analysis 
        //public IEnumerable<GetcountsofUserScore> GetUsersScoreCounts(string AssessementTemplateID, string uq_ass_schid)
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
        //    con.Open();
        //    //==Version
        //    MySqlCommand cmd = new MySqlCommand(@"
        //    SELECT
        //        user_ass_ans_details.UserID,
        //        tbluser.firstname,COALESCE(kii.Key_Impr_Indicator_Name, '') AS Key_Impr_Indicator_Name,scheduled_ass_status.Remarks,
        //    COUNT(DISTINCT questionbank.question_id) AS total_questions,

        //        (
        //            SELECT COUNT(question_id)
        //            FROM risk.user_ass_ans_details AS uad
        //            WHERE uad.AssessementTemplateID = @AssessementTemplateID
        //            AND uad.uq_ass_schid = @uq_ass_schid
        //            AND uad.UserID = user_ass_ans_details.UserID
        //        ) AS TotalQuestionsAnswered,
        //        (SELECT Percentage FROM scheduled_ass_status 
        //        WHERE AssessementTemplateID=@AssessementTemplateID 
        //        AND uq_ass_schid = @uq_ass_schid 
        //        AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS Percentages,
        //        (SELECT TaskOwnerScore FROM scheduled_ass_status 
        //        WHERE AssessementTemplateID=@AssessementTemplateID 
        //        AND uq_ass_schid = @uq_ass_schid 
        //        AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS OverallScore,
        //        (SELECT TotalScore FROM scheduled_ass_status 
        //        WHERE AssessementTemplateID=@AssessementTemplateID 
        //        AND uq_ass_schid = @uq_ass_schid 
        //        AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS TotalScoreofAssessement,
        //        (SELECT Score_Name 
        //         FROM score_indicator 
        //         WHERE Percentages BETWEEN scoreminrange AND scoremaxrange
        //         LIMIT 1) AS ScoreName,
        //     ( select CASE 
        //                            WHEN DATEDIFF(EndDateTime, StartDateTime) = 0 THEN 1 
        //                            ELSE DATEDIFF(EndDateTime, StartDateTime) 
        //                        END  AS Days


        //                 from  scheduled_ass_status where Status='Assessment Completed'  and uq_ass_schid=@uq_ass_schid and AssessementTemplateID=@AssessementTemplateID
        //                 AND UserID=user_ass_ans_details.UserID  LIMIT 1)  AS Days 
        //    FROM
        //        assessment_builder_versions
        //    INNER JOIN assessment_generation_details ON assessment_generation_details.Assessment_generationID = assessment_builder_versions.Assessment_generationID
        //    INNER JOIN questionbank ON questionbank.question_id = assessment_generation_details.question_id
        //    INNER JOIN questionbank_options ON questionbank_options.question_id = questionbank.question_id
        //   inner join user_ass_ans_details on user_ass_ans_details.question_id= questionbank.question_id and user_ass_ans_details.uq_ass_schid=@uq_ass_schid
        //    INNER JOIN tbluser ON tbluser.USR_ID = user_ass_ans_details.UserID

        //    INNER JOIN scheduled_ass_status ON scheduled_ass_status.AssessementTemplateID = user_ass_ans_details.AssessementTemplateID and scheduled_ass_status.uq_ass_schid=@uq_ass_schid AND scheduled_ass_status.UserID = user_ass_ans_details.UserID
        //     left join key_impr_indicator kii on scheduled_ass_status.key_Impr_Indicator_id = kii.key_Impr_Indicator_id
        //    WHERE
        //        assessment_builder_versions.ass_template_id = @AssessementTemplateID
        //    GROUP BY
        //        user_ass_ans_details.UserID, tbluser.firstname,scheduled_ass_status.Remarks,kii.Key_Impr_Indicator_Name;





        //    ", con);
        //    cmd.CommandType = CommandType.Text;
        //    //            MySqlCommand cmd = new MySqlCommand(@"
        //    //SELECT
        //    //    user_ass_ans_details.UserID,
        //    //    tbluser.firstname,
        //    //    COUNT(DISTINCT questionbank.question_id) AS total_questions,

        //    //    -- Total Questions Answered
        //    //    (
        //    //        SELECT COUNT(question_id)
        //    //        FROM risk.user_ass_ans_details AS uad
        //    //        JOIN schedule_assessment ON schedule_assessment.uq_ass_schid = uad.uq_ass_schid
        //    //        WHERE uad.AssessementTemplateID = @AssessementTemplateID
        //    //        AND schedule_assessment.verson_no = @verson_no
        //    //        AND uad.UserID = user_ass_ans_details.UserID
        //    //    ) AS TotalQuestionsAnswered,

        //    //    -- Percentages Calculation
        //    //    (SELECT Percentage 
        //    //     FROM scheduled_ass_status 
        //    //     JOIN schedule_assessment ON schedule_assessment.uq_ass_schid = scheduled_ass_status.uq_ass_schid
        //    //     WHERE scheduled_ass_status.AssessementTemplateID = @AssessementTemplateID 
        //    //     AND schedule_assessment.verson_no = @verson_no
        //    //     AND scheduled_ass_status.UserID = user_ass_ans_details.UserID
        //    //     LIMIT 1) AS Percentages,

        //    //    -- Overall Score
        //    //    (SELECT TaskOwnerScore 
        //    //     FROM scheduled_ass_status 
        //    //     JOIN schedule_assessment ON schedule_assessment.uq_ass_schid = scheduled_ass_status.uq_ass_schid
        //    //     WHERE scheduled_ass_status.AssessementTemplateID = @AssessementTemplateID 
        //    //     AND schedule_assessment.verson_no = @verson_no
        //    //     AND scheduled_ass_status.UserID = user_ass_ans_details.UserID
        //    //     LIMIT 1) AS OverallScore,

        //    //    -- Total Score of Assessment
        //    //    (SELECT TotalScore 
        //    //     FROM scheduled_ass_status 
        //    //     JOIN schedule_assessment ON schedule_assessment.uq_ass_schid = scheduled_ass_status.uq_ass_schid
        //    //     WHERE scheduled_ass_status.AssessementTemplateID = @AssessementTemplateID 
        //    //     AND schedule_assessment.verson_no = @verson_no
        //    //     AND scheduled_ass_status.UserID = user_ass_ans_details.UserID
        //    //     LIMIT 1) AS TotalScoreofAssessement,

        //    //    -- Score Name Calculation (Fixed)
        //    //    (SELECT Score_Name 
        //    //     FROM score_indicator 
        //    //     WHERE (SELECT Percentage 
        //    //            FROM scheduled_ass_status 
        //    //            WHERE scheduled_ass_status.AssessementTemplateID = @AssessementTemplateID 
        //    //            AND scheduled_ass_status.UserID = user_ass_ans_details.UserID
        //    //            LIMIT 1)
        //    //           BETWEEN scoreminrange AND scoremaxrange
        //    //     LIMIT 1) AS ScoreName,

        //    //    -- Days Calculation
        //    //    (SELECT 
        //    //         CASE 
        //    //             WHEN DATEDIFF(EndDateTime, StartDateTime) = 0 THEN 1 
        //    //             ELSE DATEDIFF(EndDateTime, StartDateTime) 
        //    //         END  
        //    //     FROM scheduled_ass_status 
        //    //     JOIN schedule_assessment ON schedule_assessment.uq_ass_schid = scheduled_ass_status.uq_ass_schid
        //    //     WHERE scheduled_ass_status.Status = 'Assessment Completed'  
        //    //     AND schedule_assessment.verson_no = @verson_no 
        //    //     AND scheduled_ass_status.AssessementTemplateID = @AssessementTemplateID
        //    //     AND scheduled_ass_status.UserID = user_ass_ans_details.UserID  
        //    //     LIMIT 1) AS Days 

        //    //FROM assessment_builder
        //    //INNER JOIN assessment_generation_details 
        //    //    ON assessment_generation_details.Assessment_generationID = assessment_builder.Assessment_generationID
        //    //INNER JOIN questionbank 
        //    //    ON questionbank.question_id = assessment_generation_details.question_id
        //    //INNER JOIN questionbank_options 
        //    //    ON questionbank_options.question_id = questionbank.question_id
        //    //INNER JOIN risk.user_ass_ans_details 
        //    //    ON risk.user_ass_ans_details.AssessementTemplateID = assessment_builder.ass_template_id
        //    //INNER JOIN tbluser 
        //    //    ON tbluser.USR_ID = user_ass_ans_details.UserID
        //    //INNER JOIN scheduled_ass_status 
        //    //    ON scheduled_ass_status.AssessementTemplateID = user_ass_ans_details.AssessementTemplateID

        //    //WHERE assessment_builder.ass_template_id = @AssessementTemplateID
        //    //GROUP BY user_ass_ans_details.UserID, tbluser.firstname;






        //    //", con); cmd.CommandType = CommandType.Text;

        //    cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
        //    //cmd.Parameters.AddWithValue("@verson_no", verson_no);
        //    cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);

        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
        //    con.Close();
        //    var pdata = new List<GetcountsofUserScore>();
        //    if (dt.Rows.Count > 0)
        //    {
        //        for (var i = 0; i < dt.Rows.Count; i++)
        //        {
        //            string daysvalue = "";
        //            int day11 = dt.Rows[i]["Days"].ToString() != "" ? Convert.ToInt32(dt.Rows[i]["Days"].ToString()) : 0;
        //            if (day11 == 1 || day11 == 0)
        //                daysvalue = " Day";
        //            else
        //                daysvalue = " Days";
        //            pdata.Add(new GetcountsofUserScore
        //            {

        //                UserID = Convert.ToInt32(dt.Rows[i]["UserID"].ToString()),
        //                firstname = dt.Rows[i]["firstname"].ToString(),
        //                ScoreName = dt.Rows[i]["ScoreName"].ToString(),
        //                Key_Impr_Indicator_Name = dt.Rows[i]["Key_Impr_Indicator_Name"].ToString(),
        //                total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"].ToString()),
        //                TotalQuestionsAnswered = Convert.ToInt32(dt.Rows[i]["TotalQuestionsAnswered"].ToString()),
        //                OverallScore = dt.Rows[i]["OverallScore"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["OverallScore"].ToString()) : 0,
        //                TotalScoreofAssessement = dt.Rows[i]["TotalScoreofAssessement"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["TotalScoreofAssessement"].ToString()) : 0,
        //                Percentage = dt.Rows[i]["Percentages"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["Percentages"].ToString()) : 0,
        //                Remarks = dt.Rows[i]["Remarks"].ToString(),

        //                Days = day11 + daysvalue,

        //            });

        //        }

        //    }
        //    // to remove Duplicate for Userid Duplicate Records
        //    pdata = pdata.GroupBy(x => x.UserID).Select(g => g.First()).ToList();
        //    return pdata;

        //}


        [Route("api/ScheduleAssessementReviewController/GetAssessmentResultsDetails")]
        [HttpGet]

        public IEnumerable<AssessmentResultsModel> GetAssessmentResultsDetails(string AssessementTemplateID, string uq_ass_schid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT DISTINCT uad.UserAss_Ans_DetailsID,firstname,qb.ref_to_governance_control,ccl.Skill_Level_Name AS CheckLevelName,
tm.topic_name AS TopicName,
sm.subject_name AS SubjectName, qb.question_id,qb.question,uad.user_Selected_Ans AS user_Selected_An, 
 qb.correct_answer as correct_answer_id,
    (select options from questionbank_options where question_id=qb.question_id and OptionId=qb.correct_answer) AS correct_answer,
    (select options from questionbank_options where question_id=qb.question_id and OptionId=user_Selected_An )AS user_Selected_Ans
from assessment_builder_versions
inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder_versions.Assessment_generationID
inner join questionbank qb on qb.question_id=assessment_generation_details.question_id
LEFT JOIN competency_check_level ccl ON qb.check_level = ccl.check_level_id
LEFT JOIN topic tm ON qb.topicid = tm.Topic_id
LEFT JOIN subject sm ON qb.subjectid = sm.Subject_id
LEFT JOIN 
    user_ass_ans_details uad ON qb.question_id = uad.question_id
    AND uad.Status = 'Active'
    AND uad.TypeofQuestion = 'Multiple'
    AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid =@uq_ass_schid
inner join tbluser on tbluser.USR_ID=uad.UserID
WHERE uad.Status = 'Active' and uad.TypeofQuestion='Multiple' And AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AssessmentResultsModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string UserAnswer = dt.Rows[i]["user_Selected_Ans"].ToString();
                    string CorrectAnswer = dt.Rows[i]["correct_answer"].ToString();
                    string isCorrect = (UserAnswer == CorrectAnswer) ? "Correct" : "Wrong";
                    //string imageUrl = (isCorrect == "Yes") ? "url_to_correct_image" : "url_to_wrong_image";

                    pdata.Add(new AssessmentResultsModel
                    {
                        ref_to_governance_control = dt.Rows[i]["ref_to_governance_control"].ToString(),
                        //AssessementTemplateID = Convert.ToInt32(dt.Rows[i]["AssessementTemplateID"].ToString()),
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"]),
                        question = dt.Rows[i]["question"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        user_Selected_Ans = UserAnswer,
                        UserAss_Ans_DetailsID = Convert.ToInt32(dt.Rows[i]["UserAss_Ans_DetailsID"]),
                        correct_answer = CorrectAnswer,
                        IsAnswerCorrect = isCorrect,
                        CheckLevelName = dt.Rows[i]["CheckLevelName"].ToString(),
                        TopicName = dt.Rows[i]["TopicName"].ToString(),
                        SubjectName = dt.Rows[i]["SubjectName"].ToString(),
                    });
                }
            }
            return pdata;

        }



        [Route("api/ScheduleAssessementReviewController/GetComptencySkill")]
        [HttpGet]

        public IEnumerable<GetComptencyskill> GetComptencySkill(string AssessementTemplateID, string uq_ass_schid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version
            MySqlCommand cmd = new MySqlCommand(@"SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
                               (select distinct verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid) as version,
                                 (select count(*) from questionbank 
inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
where check_level= check_level_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID and assessment_generation_details.Status='Active')AS No_of_Questions, 
                                 (select count(user_ass_ans_details.question_id)
                                 from user_ass_ans_details 
                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
 (
                SELECT COUNT(*)
                FROM user_ass_ans_details uad2
                INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.check_level = check_levelid
                WHERE uad2.userid = tbluser.USR_ID
                  AND uad2.AssessementTemplateID = @AssessementTemplateID
                  AND uad2.uq_ass_schid = @uq_ass_schid
                  AND qb2.correct_answer = uad2.user_Selected_Ans
            ) AS Correct_Answers,
                     (SELECT  IFNULL(round(
    (SUM(qb.score_weightage * qb.checklevel_weightage) / 
        (SELECT SUM(qb2.score_weightage * qb2.checklevel_weightage) 
         FROM questionbank qb2
         INNER JOIN assessment_generation_details agd 
             ON agd.question_id = qb2.question_id
         INNER JOIN assessment_builder_versions abv 
             ON abv.Assessment_generationID = agd.Assessment_generationID
         WHERE abv.ass_template_id = @AssessementTemplateID 
           AND abv.verson_no = version 
           AND qb2.check_level = check_levelid)
    ) * 100,2),0) AS ScoreIndicator
FROM user_ass_ans_details uad
INNER JOIN questionbank qb 
    ON qb.question_id = uad.question_id 
    AND qb.check_level = check_levelid
WHERE uad.Status = 'Active'
    AND uad.TypeofQuestion = 'Multiple'
    AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    AND uad.userid= tbluser.USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans)as ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName,ROUND(
        (
            (
                SELECT COUNT(*)
                FROM user_ass_ans_details uad2
                INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.check_level = check_levelid
                WHERE uad2.userid = tbluser.USR_ID
                    AND uad2.AssessementTemplateID = @AssessementTemplateID
                    AND uad2.uq_ass_schid = @uq_ass_schid
                    AND qb2.correct_answer = uad2.user_Selected_Ans
            ) * 100.0
        ) /
        (
            SELECT COUNT(*)
            FROM questionbank 
            INNER JOIN assessment_generation_details agd ON agd.question_id = questionbank.question_id
            INNER JOIN assessment_builder_versions abv ON abv.Assessment_generationID = agd.Assessment_generationID AND abv.verson_no = version
            WHERE abv.ass_template_id = @AssessementTemplateID
                AND questionbank.check_level = check_levelid
                AND agd.Status = 'Active'
        ), 2
    ) AS AccuracyPercentage
                            FROM
                          assessment_builder_versions AS a
                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
                      INNER JOIN risk.competency_check_level AS ccl ON ccl.check_level_id = qb.check_level
                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
                    inner join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id and user_ass_ans_details.uq_ass_schid= schedule_assessment.uq_ass_schid
                    inner join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user

                      WHERE
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID
                      ;
            ", con);


            //            MySqlCommand cmd = new MySqlCommand(@"SELECT
            //                              tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
            //                               (select distinct verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid) as version,
            //                                 (select count(*) from questionbank 
            //inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
            //inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
            //where check_level= check_level_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID)AS No_of_Questions, 
            //                                 (select count(user_ass_ans_details.question_id)
            //                                 from user_ass_ans_details 
            //                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

            //            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
            //                      ((select count(user_ass_ans_details.question_id)
            //                                 from user_ass_ans_details 
            //                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

            //            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where check_level= check_levelid))*100 as ScoreIndicator,
            //            (SELECT Score_Name 
            //              FROM score_indicator 
            //              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
            //              LIMIT 1) AS ScoreIndicatorName
            //                            FROM
            //                          assessment_builder_versions AS a
            //                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
            //                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
            //                      INNER JOIN risk.competency_check_level AS ccl ON ccl.check_level_id = qb.check_level
            //                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
            //                    inner join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id and user_ass_ans_details.uq_ass_schid= schedule_assessment.uq_ass_schid
            //                    inner join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user

            //                      WHERE
            //                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid
            //                      GROUP BY
            //                          tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID
            //                      ;
            //            ", con);

            //            MySqlCommand cmd = new MySqlCommand(@"SELECT
            //                  tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
            //(select count(*) from questionbank inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
            //                    inner join assessment_builder on assessment_builder.Assessment_generationID=assessment_generation_details.Assessment_generationID
            //                    where check_level= check_levelid and ass_template_id=@AssessementTemplateID)AS No_of_Questions, 
            //                     (select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid
            //                    join schedule_assessment on schedule_assessment.uq_ass_schid=user_ass_ans_details.uq_ass_schid
            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and verson_no=@verson_no )  as no_of_answered_qstns,
            //          ((select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid
            //                    join schedule_assessment on schedule_assessment.uq_ass_schid=user_ass_ans_details.uq_ass_schid
            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and verson_no=@verson_no )/ (select count(*) from questionbank where check_level= check_levelid))*100 as ScoreIndicator,
            //(SELECT Score_Name 
            //  FROM score_indicator 
            //  WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
            //  LIMIT 1) AS ScoreIndicatorName
            //                FROM
            //              assessment_builder AS a
            //          INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
            //          INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
            //          INNER JOIN risk.competency_check_level AS ccl ON ccl.check_level_id = qb.check_level
            //        inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
            //        left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
            //        left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
            //          WHERE
            //              a.ass_template_id = @AssessementTemplateID and schedule_assessment.verson_no=@verson_no
            //          GROUP BY
            //              tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID
            //          ;
            //", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetComptencyskill>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    //pdata.Add(new GetComptencyskillmodel
                    //{
                    //    firstname = dt.Rows[i]["firstname"].ToString(),
                    //    userid = Convert.ToInt32(dt.Rows[i]["userid"].ToString()),

                    //    No_of_Easy_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Easy_Questions"]),
                    //    No_of_Easy_answered_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Easy_answered_Questions"]),
                    //    Easyscoreindictor = Convert.ToDouble(dt.Rows[i]["Easyscoreindictor"]),
                    //    No_of_Medium_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Medium_Questions"]),
                    //    No_of_medium_answered_Questions = Convert.ToInt32(dt.Rows[i]["No_of_medium_answered_Questions"]),
                    //    Mediumscoreindictor = Convert.ToDouble(dt.Rows[i]["Mediumscoreindictor"]),
                    //    No_of_Hard_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Hard_Questions"]),
                    //    No_of_hard_answered_Questions = Convert.ToInt32(dt.Rows[i]["No_of_hard_answered_Questions"]),
                    //    Hardscoreindictor = Convert.ToDouble(dt.Rows[i]["Hardscoreindictor"]),


                    //    HardScoreName = dt.Rows[i]["HardScoreName"].ToString(),
                    //    EasyScoreName = dt.Rows[i]["EasyScoreName"].ToString(),
                    //    MediumScoreName = dt.Rows[i]["MediumScoreName"].ToString(),
                    //    No_of_Qstns = Convert.ToInt32(dt.Rows[i]["No_of_Easy_Questions"]),

                    //});



                    pdata.Add(new GetComptencyskill
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        userid = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        No_of_answered_Questions = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        scoreindictor = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
                        Skill_Level_Name = dt.Rows[i]["Skill_Level_Name"].ToString(),
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["Correct_Answers"]),
                        AccuracyPercentage = Convert.ToDouble(dt.Rows[i]["AccuracyPercentage"]),

                    });




                }
            }
            return pdata;

        }







        [Route("api/ScheduleAssessementReviewController/GetSubjectTopic")]
        [HttpGet]

        public IEnumerable<Getcountsubjecttopic> GetSubjectTopic(string AssessementTemplateID, string uq_ass_schid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version 


            MySqlCommand cmd = new MySqlCommand(@" SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
                                (select distinct verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid ) as version,
                                (select count(*) from questionbank 
                                inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
where topicid= topic_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID  and assessment_generation_details.Status='Active'
                          )AS No_of_Questions, 
                                 (select count(user_ass_ans_details.question_id)
                                 from user_ass_ans_details 
                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,

    (SELECT COUNT(*)
     FROM user_ass_ans_details uad2
     INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.topicid = topic_id
     WHERE uad2.userid = tbluser.USR_ID 
       AND uad2.AssessementTemplateID = @AssessementTemplateID 
       AND uad2.uq_ass_schid = @uq_ass_schid
       AND qb2.correct_answer = uad2.user_Selected_Ans
    ) AS CorrectAnswers,
                     (SELECT  IFNULL(round(
    (SUM(qb.score_weightage * qb.checklevel_weightage) / 
        (SELECT SUM(qb2.score_weightage * qb2.checklevel_weightage) 
         FROM questionbank qb2
         INNER JOIN assessment_generation_details agd 
             ON agd.question_id = qb2.question_id
         INNER JOIN assessment_builder_versions abv 
             ON abv.Assessment_generationID = agd.Assessment_generationID
         WHERE abv.ass_template_id = @AssessementTemplateID 
           AND abv.verson_no = version 
           AND qb2.topicid= topic_id)
    ) * 100,2),0) AS ScoreIndicator
FROM user_ass_ans_details uad
INNER JOIN questionbank qb 
    ON qb.question_id = uad.question_id 
    AND qb.topicid= topic_id
WHERE uad.Status = 'Active'
    AND uad.TypeofQuestion = 'Multiple'
    AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    AND uad.UserID = tbluser.USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans)  as ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName,ROUND(
    (
        SELECT COUNT(*) 
        FROM user_ass_ans_details uad
        INNER JOIN questionbank qb ON qb.question_id = uad.question_id 
        WHERE uad.AssessementTemplateID = @AssessementTemplateID
          AND uad.uq_ass_schid = @uq_ass_schid
          AND uad.UserID = tbluser.USR_ID
          AND qb.topicid = topic_id      
          AND qb.correct_answer = uad.user_Selected_Ans
    ) * 100.0 / 
    NULLIF(
        (
            SELECT COUNT(*) 
            FROM user_ass_ans_details uad
            INNER JOIN questionbank qb ON qb.question_id = uad.question_id
            WHERE uad.AssessementTemplateID = @AssessementTemplateID
              AND uad.uq_ass_schid = @uq_ass_schid
              AND uad.UserID = tbluser.USR_ID
              AND qb.topicid = topic_id         
        ), 0
    )
, 2) AS Accuracy


                            FROM
                          assessment_builder_versions AS a
                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
                    inner join  subject on subject.Subject_id= qb.subjectid
                    inner join topic on topic.Topic_id= qb.topicid
                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
                    left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
                    left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
                      WHERE
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);
            //            MySqlCommand cmd = new MySqlCommand(@" SELECT
            //                              tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
            //                                (select distinct verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid ) as version,
            //                                (select count(*) from questionbank 
            //                                inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
            //inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
            //where topicid= topic_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID
            //                          )AS No_of_Questions, 
            //                                 (select count(user_ass_ans_details.question_id)
            //                                 from user_ass_ans_details 
            //                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            //            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
            //                      ((select count(user_ass_ans_details.question_id)
            //                                 from user_ass_ans_details 
            //                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            //            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where topicid= topic_id))*100 as ScoreIndicator,
            //            (SELECT Score_Name 
            //              FROM score_indicator 
            //              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
            //              LIMIT 1) AS ScoreIndicatorName
            //                            FROM
            //                          assessment_builder_versions AS a
            //                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
            //                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
            //                    inner join  subject on subject.Subject_id= qb.subjectid
            //                    inner join topic on topic.Topic_id= qb.topicid
            //                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
            //                    left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
            //                    left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
            //                      WHERE
            //                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid
            //                      GROUP BY
            //                          tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

            //            MySqlCommand cmd = new MySqlCommand(@"SELECT
            //                  tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
            //                    (select count(*) from questionbank where topicid= topic_id)AS No_of_Questions, 
            //                     (select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id
            //                    join schedule_assessment on schedule_assessment.uq_ass_schid=user_ass_ans_details.uq_ass_schid
            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and verson_no=@verson_no )  as no_of_answered_qstns,
            //          ((select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id
            //                    join schedule_assessment on schedule_assessment.uq_ass_schid=user_ass_ans_details.uq_ass_schid
            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and verson_no=@verson_no )/ (select count(*) from questionbank where topicid= topic_id))*100 as ScoreIndicator,
            //(SELECT Score_Name 
            //  FROM score_indicator 
            //  WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
            //  LIMIT 1) AS ScoreIndicatorName
            //                FROM
            //              assessment_builder AS a
            //          INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
            //          INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
            //        inner join  subject on subject.Subject_id= qb.subjectid
            //        inner join topic on topic.Topic_id= qb.topicid
            //        inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
            //        left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
            //        left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
            //          WHERE
            //              a.ass_template_id = @AssessementTemplateID and schedule_assessment.verson_no=@verson_no
            //          GROUP BY
            //              tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Getcountsubjecttopic>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {



                    pdata.Add(new Getcountsubjecttopic
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        no_of_answered_qstns = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        ScoreIndicator = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreIndicatorName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"]),
                        AccuracyPercentage = Convert.ToDouble(dt.Rows[i]["Accuracy"]),
                    });




                }
            }
            return pdata;

        }


        [Route("api/ScheduleAssessementReviewController/GetTimeBarchatDetails")]
        [HttpGet]

        public IEnumerable<GetTimeTakenDetails> GetTimeBarchatDetails(string AssessementTemplateID, string uq_ass_schid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version 
            MySqlCommand cmd = new MySqlCommand(@" select days,count(days) as Users from (select distinct mapped_user,     (select firstname from tbluser where USR_ID=mapped_user) as firstname,
                        ( select CASE 
                                    WHEN DATEDIFF(EndDateTime, StartDateTime) = 0 THEN 1 
                                    ELSE DATEDIFF(EndDateTime, StartDateTime) 
                                END  AS Days
                         from  scheduled_ass_status where Status='Assessment Completed'  and uq_ass_schid=@uq_ass_schid and AssessementTemplateID=@AssessementTemplateID AND UserID=mapped_user)  AS Days 
                         from schedule_assessment 

            where schedule_assessment.uq_ass_schid=@uq_ass_schid  and ass_template_id=@AssessementTemplateID ) as tbl
            group by days order by days  ", con);
            //            MySqlCommand cmd = new MySqlCommand(@"  select days,count(days) as Users from (select distinct mapped_user,  
            // (select firstname from tbluser where USR_ID=mapped_user) as firstname,
            //            ( select CASE 
            //                        WHEN DATEDIFF(EndDateTime, StartDateTime) = 0 THEN @verson_no 
            //                        ELSE DATEDIFF(EndDateTime, StartDateTime) 
            //                    END  AS Days
            //             from  scheduled_ass_status
            //                         join schedule_assessment on schedule_assessment.uq_ass_schid=scheduled_ass_status.uq_ass_schid
            //             where scheduled_ass_status.Status='Assessment Completed'  
            // 		and verson_no=@verson_no and AssessementTemplateID=@AssessementTemplateID AND scheduled_ass_status.UserID=mapped_user)  AS Days 
            //             from schedule_assessment 

            //where schedule_assessment.verson_no=@verson_no  and ass_template_id=@AssessementTemplateID ) as tbl
            //group by days order by days   ", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            // cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetTimeTakenDetails>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new GetTimeTakenDetails
                    {
                        //firstname = dt.Rows[i]["firstname"].ToString(),
                        USR_ID = Convert.ToInt32(dt.Rows[i]["Users"].ToString()),

                        Days = dt.Rows[i]["Days"].ToString() != "" ? Convert.ToInt32(dt.Rows[i]["Days"]) : 0,



                    });




                }
            }
            return pdata;

        }



        [Route("api/ScheduleAssessementReviewController/GetScoreIndicators")]
        [HttpGet]

        public IEnumerable<GetScoreIndicatorModel> GetScoreIndicators(string AssessementTemplateID, string uq_ass_schid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            // == Version
            MySqlCommand cmd = new MySqlCommand(@"select Distinct Percentage ,(SELECT Score_Name 
            FROM score_indicator 
            WHERE Percentage BETWEEN scoreminrange AND scoremaxrange
            LIMIT 1) as ScoreIndicator,(SELECT Score_id 
            FROM score_indicator 
            WHERE Percentage BETWEEN scoreminrange AND scoremaxrange
            LIMIT 1) as Score_id ,count(Percentage) as no_of_users from scheduled_ass_status  where AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid group by Percentage ", con);
            //            MySqlCommand cmd = new MySqlCommand(@"SELECT DISTINCT
            //    si.Score_id,
            //    si.Score_Name AS ScoreIndicator
            //FROM scheduled_ass_status sas
            //JOIN schedule_assessment sa 
            //    ON sa.uq_ass_schid = sas.uq_ass_schid
            //JOIN score_indicator si  
            //    ON sas.Percentage BETWEEN si.scoreminrange AND si.scoremaxrange
            //WHERE sa.ass_template_id = @AssessementTemplateID 
            //AND sa.verson_no = @verson_no;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetScoreIndicatorModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new GetScoreIndicatorModel
                    {

                        ScoreIndicator = dt.Rows[i]["ScoreIndicator"].ToString(),
                        Score_id = dt.Rows[i]["Score_id"].ToString() != "" ? Convert.ToInt32(dt.Rows[i]["Score_id"].ToString()) : 0,
                        no_of_users = dt.Rows[i]["no_of_users"].ToString() != "" ? Convert.ToInt32(dt.Rows[i]["no_of_users"].ToString()) : 0



                    });




                }
            }
            return pdata;

        }


        [Route("api/ScheduleAssessementReviewController/GetScoreKeyImprovements")]
        [HttpGet]

        public IEnumerable<GetScoreIndicatorModel> GetScoreKeyImprovements(int score_id, string Competency_id)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select Key_Impr_Indicator_Name,Key_Impr_Indicator_id from key_impr_indicator where  Competency_id=@Competency_id and Score_id=@Score_id ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@Score_id", score_id);
            cmd.Parameters.AddWithValue("@Competency_id", Competency_id);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetScoreIndicatorModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new GetScoreIndicatorModel
                    {

                        Key_Impr_Indicator_Name = dt.Rows[i]["Key_Impr_Indicator_Name"].ToString(),
                        Key_Impr_Indicator_id = Convert.ToInt32(dt.Rows[i]["Key_Impr_Indicator_id"].ToString())



                    });




                }
            }
            return pdata;

        }




        [Route("api/ScheduleAssessementReviewController/GetQuestionsUsersList")]
        [HttpGet]

        public IEnumerable<GetQuestionsUserModel> GetQuestionsUsersList(string AssessementTemplateID, string uq_ass_schid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version
            MySqlCommand cmd = new MySqlCommand(@"SELECT 
                qb.question_id,
                qb.question,
                 qb.correct_answer as answer,ccl.Skill_Level_Name AS CheckLevelName,
tm.topic_name AS TopicName,
sm.subject_name AS SubjectName,
                (select options from questionbank_options where question_id=qb.question_id and OptionId=answer )AS correct_answer,
             	(SELECT COUNT(DISTINCT UserID) FROM user_ass_ans_details WHERE question_id = qb.question_id and user_Selected_Ans = qb.correct_answer and  ass_template_id = @AssessementTemplateID
                AND uq_ass_schid =@uq_ass_schid ) AS correct_answered_users,
                  (SELECT COUNT(DISTINCT UserID) FROM user_ass_ans_details WHERE question_id = qb.question_id and user_Selected_Ans != qb.correct_answer and  ass_template_id = @AssessementTemplateID
                AND uq_ass_schid =@uq_ass_schid ) AS Not_correct_answered_users,
             (SELECT COUNT(DISTINCT UserID) FROM user_ass_ans_details WHERE question_id = qb.question_id and  ass_template_id = @AssessementTemplateID
                AND uq_ass_schid =@uq_ass_schid  ) AS total_users_attempted,
                (SELECT COUNT(DISTINCT mapped_user) FROM schedule_assessment where ass_template_id = @AssessementTemplateID
                AND uq_ass_schid =@uq_ass_schid ) - (SELECT COUNT(DISTINCT UserID) FROM user_ass_ans_details WHERE question_id = qb.question_id and AssessementTemplateID = @AssessementTemplateID
                AND uq_ass_schid =@uq_ass_schid ) AS not_attempted_users
            from assessment_builder_versions
            inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder_versions.Assessment_generationID
            inner join questionbank qb on qb.question_id=assessment_generation_details.question_id
            LEFT JOIN 
                user_ass_ans_details uad ON qb.question_id = uad.question_id
                AND uad.Status = 'Active'
                AND uad.TypeofQuestion = 'Multiple'
                AND uad.AssessementTemplateID = @AssessementTemplateID
                AND uad.uq_ass_schid =@uq_ass_schid 
            LEFT JOIN 
                questionbank_options qbo ON qbo.question_id = qb.question_id AND qbo.questionbank_optionID = qb.correct_answer
LEFT JOIN competency_check_level ccl ON qb.check_level = ccl.check_level_id
LEFT JOIN topic tm ON qb.topicid = tm.Topic_id
LEFT JOIN subject sm ON qb.subjectid = sm.Subject_id
            LEFT JOIN 
                tbluser ON tbluser.USR_ID = uad.UserID
            where ass_template_id=@AssessementTemplateID and uq_ass_schid =@uq_ass_schid 
            GROUP BY 
                qb.question_id, 
                qb.question, 
                 qb.correct_answer
                order by qb.question_id;  ", con);

            //            MySqlCommand cmd = new MySqlCommand(@"SELECT 
            //    qb.question_id,
            //    qb.question,
            //     qb.correct_answer as answer,
            //    (select options from questionbank_options where question_id=qb.question_id and OptionId=answer )AS correct_answer,
            //    COUNT(CASE WHEN uad.user_Selected_Ans = qb.correct_answer THEN uad.UserID END) AS correct_answered_users,
            //    COUNT(CASE WHEN uad.user_Selected_Ans != qb.correct_answer THEN uad.UserID END) AS Not_correct_answered_users,
            // (SELECT COUNT(DISTINCT UserID) FROM user_ass_ans_details WHERE question_id = qb.question_id) AS total_users_attempted,
            //    (SELECT COUNT(DISTINCT mapped_user) FROM schedule_assessment where schedule_assessment.ass_template_id = @AssessementTemplateID
            //    AND verson_no = 1) - (SELECT COUNT(DISTINCT user_ass_ans_details.UserID) FROM user_ass_ans_details
            //     join schedule_assessment on schedule_assessment.uq_ass_schid=user_ass_ans_details.uq_ass_schid
            //    WHERE question_id = qb.question_id and uad.AssessementTemplateID = @AssessementTemplateID
            //    AND verson_no = 1) AS not_attempted_users
            //from assessment_builder
            //inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder.Assessment_generationID
            //inner join questionbank qb on qb.question_id=assessment_generation_details.question_id
            //LEFT JOIN 
            //    user_ass_ans_details uad ON qb.question_id = uad.question_id
            //    AND uad.Status = 'Active'
            //    AND uad.TypeofQuestion = 'Multiple'
            //    AND uad.AssessementTemplateID = @AssessementTemplateID
            //  join schedule_assessment on schedule_assessment.uq_ass_schid=uad.uq_ass_schid
            //LEFT JOIN 
            //    questionbank_options qbo ON qbo.question_id = qb.question_id AND qbo.questionbank_optionID = qb.correct_answer
            //LEFT JOIN 
            //    tbluser ON tbluser.USR_ID = uad.UserID
            //where schedule_assessment.ass_template_id=@AssessementTemplateID and schedule_assessment.verson_no=@verson_no
            //GROUP BY 
            //    qb.question_id, 
            //    qb.question, 
            //     qb.correct_answer
            //    order by qb.question_id;
            //", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetQuestionsUserModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new GetQuestionsUserModel
                    {

                        question = dt.Rows[i]["question"].ToString(),
                        correct_answer = dt.Rows[i]["correct_answer"].ToString(),
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        No_of_Users_Answered_Attempted = Convert.ToInt32(dt.Rows[i]["total_users_attempted"].ToString()),
                        No_of_Users_Answered_Correct = Convert.ToInt32(dt.Rows[i]["correct_answered_users"].ToString()),
                        No_of_Users_Answered_InCorrect = Convert.ToInt32(dt.Rows[i]["Not_correct_answered_users"].ToString()),
                        No_of_Users_Answered_Not_Attempted = Convert.ToInt32(dt.Rows[i]["not_attempted_users"].ToString()),
                        CheckLevelName = dt.Rows[i]["CheckLevelName"].ToString(),
                        TopicName = dt.Rows[i]["TopicName"].ToString(),
                        SubjectName = dt.Rows[i]["SubjectName"].ToString(),


                    });




                }
            }
            return pdata;
        }



        [Route("api/ScheduleAssessementReviewController/GetListofUsers")]
        [HttpGet]

        public IEnumerable<GetQuestionsUserModel> GetListofUsers(string AssessementTemplateID, string uq_ass_schid, int question_id, int Users)

        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd;

            //----- This if conditions is for Users selection Based on the grid --------------
            if (Users == 1)//------this is for correct_answered_users -----------
            {
                cmd = new MySqlCommand(@" SELECT firstname,question,correct_answer
      
FROM (
    SELECT firstname,question,  qb.correct_answer as answer,
    (select options from questionbank_options where question_id=qb.question_id and OptionId=answer )AS correct_answer,
               COUNT(CASE WHEN uad.user_Selected_Ans = qb.correct_answer THEN uad.UserID END) AS correct_answered_users
    FROM questionbank AS qb
    JOIN user_ass_ans_details uad ON uad.question_id = qb.question_id
    LEFT JOIN questionbank_options qbo ON qbo.question_id = qb.question_id AND qbo.questionbank_optionID = qb.correct_answer
    LEFT JOIN tbluser ON tbluser.USR_ID = uad.UserID
    WHERE qb.question_id = @question_id  AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    GROUP BY firstname,question, qbo.options
) AS subquery
WHERE correct_answered_users > 0;
", con);
            }
            else if (Users == 2) //------this is for Not_correct_answered_users -----------
            {
                cmd = new MySqlCommand(@" SELECT firstname,question,correct_answer
      
FROM (
    SELECT firstname,question,  qb.correct_answer as answer,
    (select options from questionbank_options where question_id=qb.question_id and OptionId=answer )AS correct_answer,
           COUNT(CASE WHEN uad.user_Selected_Ans != qb.correct_answer THEN uad.UserID END) AS Not_correct_answered_users
    FROM questionbank AS qb
    JOIN user_ass_ans_details uad ON uad.question_id = qb.question_id
    LEFT JOIN questionbank_options qbo ON qbo.question_id = qb.question_id AND qbo.questionbank_optionID = qb.correct_answer
    LEFT JOIN tbluser ON tbluser.USR_ID = uad.UserID
    WHERE qb.question_id = @question_id  AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    GROUP BY firstname,question, qbo.options
) AS subquery
WHERE Not_correct_answered_users > 0;
", con);
            }
            else //------this is for not_attempted_users -----------
            {
                cmd = new MySqlCommand(@" select distinct USR_ID,firstname,
    (SELECT qb.question FROM questionbank AS qb WHERE qb.question_id = @question_id) AS question,
    (SELECT qbo.options FROM questionbank_options AS qbo WHERE qbo.question_id = @question_id AND qbo.OptionId = (SELECT qb.correct_answer FROM questionbank AS qb WHERE qb.question_id = @question_id)) AS correct_answer
FROM tbluser

    join schedule_assessment on schedule_assessment.mapped_user=tbluser.USR_ID
     where USR_ID not in(
    select userid from user_ass_ans_details as uad where question_id = @question_id and uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid =@uq_ass_schid)
    and ass_template_id = @AssessementTemplateID
    AND schedule_assessment.uq_ass_schid = @uq_ass_schid
    
", con);
            }
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@question_id", question_id);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetQuestionsUserModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new GetQuestionsUserModel
                    {

                        question = dt.Rows[i]["question"].ToString(),
                        correct_answer = dt.Rows[i]["correct_answer"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),


                    });




                }
            }
            return pdata;
        }





    }








}
