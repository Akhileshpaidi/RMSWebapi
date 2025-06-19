using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel;
using iText.StyledXmlParser.Jsoup.Select;
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
    public class MyAssessementsReviewController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public MyAssessementsReviewController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/MyAssessementsReviewController/GetCompletedScheduleAssesment")]
        [HttpGet]
        public IEnumerable<AssScheduleModel> GetCompletedAssesment(int mapped_user)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version
            MySqlCommand cmd = new MySqlCommand(@" SELECT  sa.Schedule_Assessment_id, ab.ass_template_id,ANY_VALUE(ab.assessment_name) AS assessment_name,
    ANY_VALUE(ab.assessment_description) AS assessment_description,
    DATE(ANY_VALUE(ab.created_date)) AS created_date,
    ANY_VALUE(ab.status) AS status,
    ANY_VALUE(ab.keywords) AS keywords,
    ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
    ANY_VALUE(tn.Type_Name) AS Type_Name,
    ANY_VALUE(sn.SubType_Name) AS SubType_Name,
    ANY_VALUE(cn.Competency_Name) AS Competency_Name,
    DATE(ANY_VALUE(sa.startDate)) AS startDate,
    DATE(ANY_VALUE(sa.endDate)) AS endDate,
    ANY_VALUE(sas.status) AS AssessmentStatus,
ANY_VALUE( sa.pagetype) AS pagetype,
    sa.uq_ass_schid,
    ANY_VALUE(sa.mapped_user) AS mapped_user,
    ANY_VALUE(ab.Competency_id) AS Competency_id,
    ANY_VALUE(sas.StartDateTime) AS StartDateTime,
    ANY_VALUE(sas.EndDateTime) AS EndDateTime,
      (select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no,
   (select distinct objective from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as objective,
   (select distinct message from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as message
FROM 
    risk.assessment_builder_versions ab
JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
 JOIN 
    risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id and sa.mapped_user=@mapped_user and sa.verson_no=ab.verson_no
 JOIN 
    risk.scheduled_ass_status sas ON sas.uq_ass_schid = sa.uq_ass_schid and sas.UserID=@mapped_user 
WHERE 
    sa.uq_ass_schid IS NOT NULL  and sas.Status='Result Published'

GROUP BY 
    ab.ass_template_id, sa.uq_ass_schid,sa.Schedule_Assessment_id;
                                 ", con);
//            MySqlCommand cmd = new MySqlCommand(@"


//      select  ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
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
//    DATE(ANY_VALUE(startDate)) AS startDate,
//    DATE(ANY_VALUE(endDate)) AS endDate,
//    ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
//    sa.uq_ass_schid,
//    ANY_VALUE(mapped_user) AS mapped_user,
//ANY_VALUE(ab.Competency_id) as Competency_id,
//ANY_VALUE(sa.Schedule_Assessment_id) as Schedule_Assessment_id ,
//        ANY_VALUE(sas.StartDateTime) as StartDateTime,
//       ANY_VALUE(sas.EndDateTime) as EndDateTime ,
//       sa.verson_no
// from assessment_builder ab
//   JOIN 
//    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
//JOIN 
//    risk.type tn ON tn.Type_id = ab.Type_id
//JOIN 
//    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
//JOIN 
//    risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id and mapped_user=@mapped_user
//    join  scheduled_ass_status sas on sas.AssessementTemplateID=ab.ass_template_id
//WHERE 
//    sa.uq_ass_schid IS NOT NULL and sas.Status='Result Published' and sa.AssessmentStatus='Assessment Completed' and sas.UserID=@mapped_user

//GROUP BY 
//    sa.ass_template_id, sa.uq_ass_schid,sa.verson_no


// ", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@mapped_user", mapped_user);

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
                       
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
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
                        status = dt.Rows[i]["status"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        verson_no = dt.Rows[i]["verson_no"].ToString(),
                        objective = dt.Rows[i]["objective"].ToString(),
                        message = dt.Rows[i]["message"].ToString(),
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }


        [Route("api/MyAssessementsReviewController/GetCompletedScheduleAssesmentDetails")]
        [HttpGet]
        public IEnumerable<AssScheduleModel> GetCompletedScheduleAssesmentDetails(string AssessementTemplateID, string uq_ass_schid, int mapped_user)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"


   select  ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
    sa.ass_template_id,
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
       ANY_VALUE(sas.EndDateTime) as EndDateTime ,
  ANY_VALUE(firstname) as firstname,
(select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no,
    (select Remarks  from  scheduled_ass_status where AssessementTemplateID=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid and UserID=@USR_ID) as Remarks

 from assessment_builder_versions ab
   JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN 
    risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id
    join  scheduled_ass_status sas on sas.AssessementTemplateID=ab.ass_template_id
left join tbluser on tbluser.USR_ID= sa.mapped_user
WHERE 
    sa.uq_ass_schid IS NOT NULL and sas.Status='Result Published' and ab.ass_template_id=@AssessementTemplateID and sa.uq_ass_schid=@uq_ass_schid

GROUP BY 
    sa.ass_template_id, sa.uq_ass_schid


 ", con);


            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);
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

                    string startDates = Convert.ToDateTime(dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy");
                    DateTime startdatee = DateTime.ParseExact(startDates, "dd-MM-yyyy", null);
                    pdata.Add(new AssScheduleModel
                    {

                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        Competency_id = Convert.ToInt32(dt.Rows[i]["Competency_id"]),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        startDates = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy"),

                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        endDates = ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        verson_no = dt.Rows[i]["verson_no"].ToString(),
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        Remarks = dt.Rows[i]["Remarks"].ToString(),
                    });
                }
            }
            return pdata;
        }



        [Route("api/MyAssessementsReviewController/GetSubjectTopic")]
        [HttpGet]

        public IEnumerable<Getcountsubjecttopic> GetSubjectTopic(string AssessementTemplateID, string uq_ass_schid, int mapped_user)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version
            //            MySqlCommand cmd = new MySqlCommand(@" SELECT
            //                  tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
            //                    (select count(*) from questionbank where topicid= topic_id)AS No_of_Questions, 
            //                     (select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
            //          ((select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where topicid= topic_id))*100 as ScoreIndicator,
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
            //              a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid and USR_ID=@USR_ID
            //          GROUP BY
            //              tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

           // -- working

    //        ROUND(
    //IFNULL(
    //    (SELECT COUNT(*)
    //     FROM user_ass_ans_details uad2
    //     INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.topicid = topic_id
    //     WHERE uad2.userid = @USR_ID
    //       AND uad2.AssessementTemplateID = @AssessementTemplateID
    //       AND uad2.uq_ass_schid = @uq_ass_schid
    //       AND qb2.correct_answer = uad2.user_Selected_Ans
    //    )
    //    /
    //    NULLIF((
    //        SELECT COUNT(*) FROM questionbank
    //        INNER JOIN assessment_generation_details agd ON agd.question_id = questionbank.question_id
    //        INNER JOIN assessment_builder_versions abv ON abv.Assessment_generationID = agd.Assessment_generationID AND abv.verson_no = version
    //        WHERE questionbank.topicid = topic_id AND abv.ass_template_id = @AssessementTemplateID AND agd.Status = 'Active'
    //    ), 0)
    //, 0) *100, 2) AS Accuracy
            MySqlCommand cmd = new MySqlCommand(@"  SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
                            (select verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid and mapped_user=@USR_ID) as version,
                                (select count(*) from questionbank 
                                inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
where topicid= topic_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID and assessment_generation_details.Status='Active'
                          )AS No_of_Questions,  
                                 (select count(user_ass_ans_details.question_id)
                                 from user_ass_ans_details 
                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,


                     
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
    AND uad.UserID = @USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans) as ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName,
  ( SELECT COUNT(*)
  FROM user_ass_ans_details uad2
  INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
  WHERE qb2.topicid = topic_id
    AND uad2.userid = @USR_ID 
    AND uad2.AssessementTemplateID = @AssessementTemplateID 
    AND uad2.uq_ass_schid = @uq_ass_schid
    AND qb2.correct_answer = uad2.user_Selected_Ans
            ) AS CorrectAnswers,
ROUND(
    IFNULL(
        (
            SELECT COUNT(*)
            FROM user_ass_ans_details uad2
            INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.topicid = topic_id
            WHERE uad2.userid = tbluser.USR_ID 
              AND uad2.AssessementTemplateID = @AssessementTemplateID 
              AND uad2.uq_ass_schid = @uq_ass_schid
              AND qb2.correct_answer = uad2.user_Selected_Ans
        ) 
        / 
        NULLIF(
            (
                SELECT COUNT(*) 
                FROM questionbank 
                INNER JOIN assessment_generation_details agd 
                    ON agd.question_id = questionbank.question_id
                INNER JOIN assessment_builder_versions abv 
                    ON abv.Assessment_generationID = agd.Assessment_generationID 
                    AND abv.verson_no = version
                WHERE questionbank.topicid = topic_id 
                  AND abv.ass_template_id = @AssessementTemplateID 
                  AND agd.Status = 'Active'
            ), 0
        )
    , 0) * 100
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
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid and USR_ID=@USR_ID
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);
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




        [Route("api/MyAssessementsReviewController/GetUsersScoreCounts")]
        [HttpGet]

        public IEnumerable<GetcountsofUserScore> GetUsersScoreCounts(string AssessementTemplateID, string uq_ass_schid, int mapped_user)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT
    uad.UserID,
    tu.firstname,sas.uq_ass_schid,
    COALESCE(kii.Key_Impr_Indicator_Name, '') AS Key_Impr_Indicator_Name,tn.Type_Name,sn.SubType_Name, cn.Competency_Name,
    COUNT(DISTINCT qb.question_id) AS total_questions,
    (
        SELECT COUNT(question_id)
        FROM risk.user_ass_ans_details AS uad_sub
        WHERE uad_sub.AssessementTemplateID = @AssessementTemplateID
          AND uad_sub.uq_ass_schid = @uq_ass_schid
          AND uad_sub.UserID = uad.UserID
    ) AS TotalQuestionsAnswered,
    sas.Percentage AS Percentages,
    sas.TaskOwnerScore AS OverallScore,
    sas.TotalScore AS TotalScoreofAssessement,
(
    SELECT COUNT(*)
    FROM user_ass_ans_details uad2
    INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
    WHERE uad2.UserID = uad.UserID
      AND uad2.AssessementTemplateID = @AssessementTemplateID
      AND uad2.uq_ass_schid = @uq_ass_schid
      AND qb2.correct_answer = uad2.user_Selected_Ans
) AS CorrectAnswers,

ROUND(
    IFNULL(
        (
            SELECT COUNT(*)
            FROM user_ass_ans_details uad2
            INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
            WHERE uad2.UserID = uad.UserID
              AND uad2.AssessementTemplateID = @AssessementTemplateID
              AND uad2.uq_ass_schid = @uq_ass_schid
              AND qb2.correct_answer = uad2.user_Selected_Ans
        ) / NULLIF(COUNT(DISTINCT qb.question_id), 0)
    , 0) * 100, 2
) AS Accuracy,

    (
        SELECT si.Score_Name
        FROM score_indicator si
        WHERE sas.Percentage BETWEEN si.scoreminrange AND si.scoremaxrange
        LIMIT 1
    ) AS ScoreName,

    CASE
        WHEN DATEDIFF(sas.EndDateTime, sas.StartDateTime) = 0 THEN 1
        ELSE DATEDIFF(sas.EndDateTime, sas.StartDateTime)
    END AS Days
FROM assessment_builder_versions abv
INNER JOIN assessment_generation_details agd ON agd.Assessment_generationID = abv.Assessment_generationID
INNER JOIN questionbank qb ON qb.question_id = agd.question_id
INNER JOIN questionbank_options qbo ON qbo.question_id = qb.question_id
INNER JOIN user_ass_ans_details uad ON uad.question_id = qb.question_id AND uad.uq_ass_schid = @uq_ass_schid
INNER JOIN tbluser tu ON tu.USR_ID = uad.UserID
INNER JOIN (
    -- Get one scheduled_ass_status row per user (latest EndDateTime)
    SELECT s1.*
    FROM scheduled_ass_status s1
    INNER JOIN (
        SELECT UserID, MAX(EndDateTime) AS MaxEndDateTime
        FROM scheduled_ass_status
        WHERE Status = 'Result Published'
          AND uq_ass_schid = @uq_ass_schid
          AND AssessementTemplateID = @AssessementTemplateID
        GROUP BY UserID
    ) s2 ON s1.UserID = s2.UserID AND s1.EndDateTime = s2.MaxEndDateTime
    WHERE s1.Status = 'Result Published'
      AND s1.uq_ass_schid = @uq_ass_schid
      AND s1.AssessementTemplateID = @AssessementTemplateID
) sas ON sas.UserID = uad.UserID
LEFT JOIN key_impr_indicator kii ON kii.key_Impr_Indicator_id = sas.key_Impr_Indicator_id
        INNER JOIN risk.sub_type sn ON sn.SubType_id = abv.SubType_id
INNER JOIN risk.type tn ON tn.Type_id = abv.Type_id
INNER JOIN risk.competency_skill cn ON cn.Competency_id = abv.Competency_id
WHERE abv.ass_template_id = @AssessementTemplateID
  AND tu.USR_ID = @USR_ID
GROUP BY uad.UserID, tu.firstname, kii.Key_Impr_Indicator_Name, sas.Percentage, sas.TaskOwnerScore, sas.TotalScore, sas.EndDateTime, sas.StartDateTime,tn.Type_Name,sn.SubType_Name, cn.Competency_Name,sas.uq_ass_schid
; ", con); cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);

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
                   int day11 = dt.Rows[i]["Days"].ToString() != "" ? Convert.ToInt32(dt.Rows[i]["Days"].ToString()) : 0;
                    if (day11 == 1 ||day11==0)
                        daysvalue = " Day";
                    else
                        daysvalue = " Days";
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
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"].ToString()),
                        AccuracyPercentage = dt.Rows[i]["Accuracy"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["Accuracy"].ToString()) : 0,

                        Days = dt.Rows[i]["Days"].ToString() + daysvalue,

                    });

                }

            }
            return pdata;

        }


        [Route("api/MyAssessementsReviewController/GetComptencySkill")]
        [HttpGet]

        public IEnumerable<GetComptencyskill> GetComptencySkill(string AssessementTemplateID, string uq_ass_schid , int mapped_user)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==version
            //            MySqlCommand cmd = new MySqlCommand(@" SELECT
            //                  tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
            //                    (select count(*) from questionbank inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
            //                    inner join assessment_builder on assessment_builder.Assessment_generationID=assessment_generation_details.Assessment_generationID
            //                    where check_level= check_levelid and ass_template_id=@AssessementTemplateID)AS No_of_Questions, 
            //                     (select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
            //          ((select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where check_level= check_levelid))*100 as ScoreIndicator,
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
            //              a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid  and tbluser.USR_ID=@USR_ID
            //          GROUP BY
            //              tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID;
            //", con);
            //            MySqlCommand cmd = new MySqlCommand(@" SELECT
            //                              tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
            //                               (select verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid and mapped_user=@USR_ID) as version,
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
            //                    left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
            //                    left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
            //                      WHERE
            //                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid  and USR_ID=@USR_ID
            //                      GROUP BY
            //                          tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID
            //", con);


            MySqlCommand cmd = new MySqlCommand(@" SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
                               (select verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid and mapped_user=@USR_ID) as version,
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
    INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
    WHERE qb2.check_level = check_level_id
      AND uad2.UserID = tbluser.USR_ID
      AND uad2.AssessementTemplateID = @AssessementTemplateID
      AND uad2.uq_ass_schid = @uq_ass_schid
      AND qb2.correct_answer = uad2.user_Selected_Ans
) AS CorrectAnswers,

(
    SELECT ROUND(
        (
            SELECT COUNT(*)
            FROM user_ass_ans_details uad2
            INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
            WHERE qb2.check_level = check_level_id
              AND uad2.UserID = tbluser.USR_ID
              AND uad2.AssessementTemplateID = @AssessementTemplateID
              AND uad2.uq_ass_schid = @uq_ass_schid
              AND qb2.correct_answer = uad2.user_Selected_Ans
        ) / NULLIF(
            (
                SELECT COUNT(*)
                FROM questionbank qb3
                INNER JOIN assessment_generation_details agd2 ON agd2.question_id = qb3.question_id
                INNER JOIN assessment_builder_versions abv2 ON abv2.Assessment_generationID = agd2.Assessment_generationID
                WHERE qb3.check_level = check_level_id
                  AND abv2.ass_template_id = @AssessementTemplateID
                  AND abv2.verson_no = version
                  AND agd2.Status = 'Active'
            ), 0) * 100, 2
    )
) AS Accuracy,

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
    AND uad.UserID = @USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans) AS ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName
                            FROM
                          assessment_builder_versions AS a
                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
                      INNER JOIN risk.competency_check_level AS ccl ON ccl.check_level_id = qb.check_level
                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
                    left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
                    left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
                      WHERE
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid  and USR_ID=@USR_ID
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID;
", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetComptencyskill>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetComptencyskill
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        userid = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        No_of_answered_Questions = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        scoreindictor = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
                        Skill_Level_Name = dt.Rows[i]["Skill_Level_Name"].ToString(),
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"]),
                        AccuracyPercentage = Convert.ToDouble(dt.Rows[i]["Accuracy"]),


                    });



                }
            }
            return pdata;

        }


        [Route("api/MyAssessementsReviewController/GetAssessmentResultsDetails")]
        [HttpGet]

        public IEnumerable<AssessmentResultsModel> GetAssessmentResultsDetails(string AssessementTemplateID, string uq_ass_schid, int mapped_user)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //            MySqlCommand cmd = new MySqlCommand(@"SELECT DISTINCT uad.UserAss_Ans_DetailsID,firstname,qb.ref_to_governance_control, uad.question_id,qb.question,uao.options AS user_Selected_Ans,qbo.options AS correct_answer 
            //FROM user_ass_ans_details uad
            //INNER JOIN questionbank qb ON qb.question_id = uad.question_id 
            //left JOIN questionbank_options uao ON uao.question_id = qb.question_id AND uao.questionbank_optionID = uad.user_Selected_Ans
            //left JOIN questionbank_options qbo ON qbo.question_id = qb.question_id AND qbo.questionbank_optionID = qb.correct_answer
            //inner join tbluser on tbluser.USR_ID=uad.UserID
            //WHERE uad.Status = 'Active' and uad.TypeofQuestion='Multiple' And AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid and tbluser.USR_ID=@USR_ID", con);
            MySqlCommand cmd = new MySqlCommand(@"   select  DISTINCT uad.UserAss_Ans_DetailsID,firstname,ccl.Skill_Level_Name AS CheckLevelName,
tm.topic_name AS TopicName,
sm.subject_name AS SubjectName,qb.ref_to_governance_control, qb.question_id,qb.question,uad.user_Selected_Ans AS user_Selected_An, 
 qb.correct_answer as answer,
    (select options from questionbank_options where question_id=qb.question_id and OptionId=answer )AS correct_answer,
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
LEFT JOIN 
    questionbank_options qbo ON qbo.question_id = qb.question_id AND qbo.questionbank_optionID = qb.correct_answer
LEFT JOIN 
    tbluser ON tbluser.USR_ID = uad.UserID and USR_ID=@USR_ID
where ass_template_id=@AssessementTemplateID  AND uad.UserID = @USR_ID   and verson_no in(  select distinct verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid and mapped_user=@USR_ID)
order by qb.question_id
", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);
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
                        UserAss_Ans_DetailsID = dt.Rows[i]["UserAss_Ans_DetailsID"].ToString()!=""?Convert.ToInt32(dt.Rows[i]["UserAss_Ans_DetailsID"]):0,
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



    }

}
