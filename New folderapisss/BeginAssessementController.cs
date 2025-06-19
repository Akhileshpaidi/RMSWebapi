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
    public class BeginAssessementController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public BeginAssessementController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }


        [Route("api/BeginAssessementController/GetActiveScheduleAssesment")]
        [HttpGet]
        public IEnumerable<AssScheduleModel> GetActiveAssesment(int userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"

SELECT schedule_assessment.Schedule_Assessment_id,ab.ass_template_id, ab.assessment_name, ab.assessment_description,DATE(ab.created_date)as  created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name,sn.SubType_Name, cn.Competency_Name,DATE(startDate) as startDate,DATE(endDate) as endDate
FROM risk.assessment_builder ab

JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
Join risk.schedule_assessment on risk.schedule_assessment.ass_template_id=ab.ass_template_id
WHERE ab.Status = 'Active' and schedule_assessment.AssessmentStatus='Assessment Scheduled' And userid=@userid
 and endDate >= CURRENT_DATE ", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@userid", userid);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssScheduleModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssScheduleModel
                    {
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        ass_template_id = Convert.ToInt32(dt.Rows[i]["ass_template_id"]),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString()
                    });
                }
            }
            return pdata;
        }



        [Route("api/BeginAssessementController/GetQuestions")]
        [HttpGet]

        public List<BeginAssModel> GetQuestions(int AssessementTemplateID)
        {

            var pdata = new List<BeginAssModel>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select distinct questionbank.question_id,question,base64,response_type from assessment_builder
inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder.Assessment_generationID
inner join questionbank on questionbank.question_id=assessment_generation_details.question_id
inner join questionbank_options on questionbank_options.question_id=questionbank.question_id
where ass_template_id=@AssessementTemplateID order by questionbank.question_id", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@Status", "Active");

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {


                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    int question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString());
                    MySqlCommand cmd1 = new MySqlCommand(@"select distinct options,questionbank_optionID,questionbank.question_id from assessment_builder
inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder.Assessment_generationID
inner join questionbank on questionbank.question_id=assessment_generation_details.question_id
inner join questionbank_options on questionbank_options.question_id=questionbank.question_id
where ass_template_id=@AssessementTemplateID and questionbank.question_id='" + question_id + "' order by questionbank.question_id ", con);

                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
                    //cmd.Parameters.AddWithValue("@Status", "Active");

                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    List<BeginAssQstnsoptions> options = new List<BeginAssQstnsoptions>();
                    for (var j = 0; j < dt1.Rows.Count; j++)
                    {
                        BeginAssQstnsoptions options1 = new BeginAssQstnsoptions();
                        options1.questionbank_optionID = Convert.ToInt32(dt1.Rows[j]["questionbank_optionID"].ToString());
                        options1.options = dt1.Rows[j]["options"].ToString();
                        options.Add(options1);
                    }
                    pdata.Add(new BeginAssModel
                    {

                        question = dt.Rows[i]["question"].ToString(),
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        base64 = dt.Rows[i]["base64"].ToString(),
                        response_type = Convert.ToInt32(dt.Rows[i]["response_type"].ToString()),

                        BeginAssQstnsoptions = options,


                    });

                }

            }
            return pdata;

        }


        [Route("api/BeginAssessementController/GetCountQstns")]
        [HttpGet]

        public IEnumerable<object> GetCountQstns(int AssessementTemplateID)
        {

            var pdata = new List<TotalQuestions>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
 
   
SELECT 
    COUNT(DISTINCT questionbank.question_id) AS total,
    GROUP_CONCAT(DISTINCT questionbank.question_id ORDER BY questionbank.question_id) AS question_ids
FROM 
    assessment_builder
INNER JOIN 
    assessment_generation_details ON assessment_generation_details.Assessment_generationID = assessment_builder.Assessment_generationID
INNER JOIN 
    questionbank ON questionbank.question_id = assessment_generation_details.question_id
INNER JOIN 
    questionbank_options ON questionbank_options.question_id = questionbank.question_id
WHERE 
    ass_template_id = @AssessementTemplateID
GROUP BY 
    questionbank.question_id;", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new TotalQuestions
                    {

                        total = Convert.ToInt32(dt.Rows[i]["total"].ToString()),
                        question_id = Convert.ToInt32(dt.Rows[i]["question_ids"].ToString()),

                    });

                }

            }
            return pdata;

        }



        [Route("api/BeginAssessementController/UserAnswers")]
        [HttpPost]
        public IActionResult UserAnswer([FromBody] List<user_ass_ans_details> UserAnswers)
        
        
        
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                if (UserAnswers[0].finalsubmit == 0)
                {
                    string insertQuerystatus = (@"insert into scheduled_ass_status(AssessementTemplateID,StartDateTime,UserID,Status,CreatedDate)values
                    (@AssessementTemplateID,@StartDateTime,@UserID,@Status,@CreatedDate)");



                    using (MySqlCommand myCommand11 = new MySqlCommand(insertQuerystatus, con))
                    {

                        myCommand11.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[0].AssessementTemplateID);
                        // myCommand11.Parameters.AddWithValue("@EndDateTime", DateTime.Now);
                        myCommand11.Parameters.AddWithValue("@UserID", UserAnswers[0].UserID);
                        myCommand11.Parameters.AddWithValue("@Status", "Active");
                        myCommand11.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        myCommand11.Parameters.AddWithValue("@StartDateTime", DateTime.Now);


                        myCommand11.ExecuteNonQuery();

                    }
                }
                else if (UserAnswers[0].finalsubmit == 1)
                {
                    MySqlCommand cmd = new MySqlCommand(@"
SELECT * FROM risk.scheduled_ass_status where AssessementTemplateID=@AssessementTemplateID and UserID=@UserID", con);

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[0].AssessementTemplateID);
                    cmd.Parameters.AddWithValue("@UserID", UserAnswers[0].UserID);

                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd);

                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                   // con.Close();

                    if (dt1.Rows.Count > 0)
                    {
                        int Scheduled_Ass_StatusID = Convert.ToInt32(dt1.Rows[0]["Scheduled_Ass_StatusID"].ToString());
                        string updatestatus = (@"update scheduled_ass_status set EndDateTime=@EndDateTime,Status=@Status where Scheduled_Ass_StatusID=@Scheduled_Ass_StatusID ");
                        using (MySqlCommand myCommand12 = new MySqlCommand(updatestatus, con))
                        {
                            myCommand12.Parameters.AddWithValue("@EndDateTime", DateTime.Now);
                            myCommand12.Parameters.AddWithValue("@Status", "Assessment Completed");
                            myCommand12.Parameters.AddWithValue("@Scheduled_Ass_StatusID", Scheduled_Ass_StatusID);
                            myCommand12.ExecuteNonQuery();

                        }

                    }
                    else
                    {

                        string insertQuerystatus = (@"insert into scheduled_ass_status(AssessementTemplateID,StartDateTime,UserID,Status,CreatedDate)values
                    (@AssessementTemplateID,@StartDateTime,@UserID,@Status,@CreatedDate)");



                        using (MySqlCommand myCommand11 = new MySqlCommand(insertQuerystatus, con))
                        {

                            myCommand11.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[0].AssessementTemplateID);
                            // myCommand11.Parameters.AddWithValue("@EndDateTime", DateTime.Now);
                            myCommand11.Parameters.AddWithValue("@UserID", UserAnswers[0].UserID);
                            myCommand11.Parameters.AddWithValue("@Status", "Active");
                            myCommand11.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                            myCommand11.Parameters.AddWithValue("@StartDateTime", DateTime.Now);


                            myCommand11.ExecuteNonQuery();

                        }
                    }
                }

                for (int i = 0; i < UserAnswers.Count; i++)
                {

                    MySqlCommand cmd = new MySqlCommand(@"
SELECT * FROM risk.user_ass_ans_details where AssessementTemplateID=@AssessementTemplateID and UserID=@UserID and question_id=@question_id", con);

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[i].AssessementTemplateID);
                    cmd.Parameters.AddWithValue("@UserID", UserAnswers[i].UserID);
                    cmd.Parameters.AddWithValue("@question_id", UserAnswers[i].question_id);

                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd);

                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    //con.Close();

                    if (dt1.Rows.Count > 0)
                    {
                        int UserAss_Ans_DetailsID = Convert.ToInt32(dt1.Rows[0]["UserAss_Ans_DetailsID"].ToString());
                        string UpdateQuery;

                        UpdateQuery = (@"Update  user_ass_ans_details set user_Selected_Ans=@user_Selected_Ans,TextFieldAnswer=@TextFieldAnswer where UserAss_Ans_DetailsID=@UserAss_Ans_DetailsID");


                        using (MySqlCommand myCommand1 = new MySqlCommand(UpdateQuery, con))
                        {

                            myCommand1.Parameters.AddWithValue("@UserAss_Ans_DetailsID", UserAss_Ans_DetailsID);
                         myCommand1.Parameters.AddWithValue("@user_Selected_Ans", UserAnswers[i].user_Selected_Ans);
                            myCommand1.Parameters.AddWithValue("@TextFieldAnswer", UserAnswers[i].TextFieldAnswer);


                            myCommand1.ExecuteNonQuery();

                        }

                    }
                    else
                    {
                        string insertQuery;

                        insertQuery = (@"insert into user_ass_ans_details(AssessementTemplateID,question_id,user_Selected_Ans,UserID,TypeofQuestion,TextFieldAnswer,Status,CreatedDate)values
                    (@AssessementTemplateID,@question_id,@user_Selected_Ans,@UserID,@TypeofQuestion,@TextFieldAnswer,@Status,@CreatedDate)");


                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                        {

                            myCommand1.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[i].AssessementTemplateID);
                            myCommand1.Parameters.AddWithValue("@question_id", UserAnswers[i].question_id);
                            myCommand1.Parameters.AddWithValue("@UserID", UserAnswers[i].UserID);

                            if (UserAnswers[i].TextFieldAnswer == "")
                            {
                                myCommand1.Parameters.AddWithValue("@TypeofQuestion", "Multiple");
                                myCommand1.Parameters.AddWithValue("@user_Selected_Ans", UserAnswers[i].user_Selected_Ans);

                            }
                            else
                            {
                                myCommand1.Parameters.AddWithValue("@TypeofQuestion", "Text");
                                myCommand1.Parameters.AddWithValue("@user_Selected_Ans", 0);

                            }
                            myCommand1.Parameters.AddWithValue("@TextFieldAnswer", UserAnswers[i].TextFieldAnswer);
                             myCommand1.Parameters.AddWithValue("@Status", "Active");
                            myCommand1.Parameters.AddWithValue("@CreatedDate", DateTime.Now);


                            myCommand1.ExecuteNonQuery();

                        }
                    }
                }


                // }


                return Ok("successfully");

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

        [Route("api/BeginAssessementController/GetUserAnswersList")]
        [HttpGet]

        public IEnumerable<object> GetUserAnswersList(int AssessementTemplateID, int UserID)
        {

            var pdata = new List<attemptedqstns>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT * FROM risk.user_ass_ans_details where AssessementTemplateID=@AssessementTemplateID and UserID=@UserID", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@UserID", UserID);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            if (dt.Rows.Count > 0)
            {


                for (var i = 0; i < dt.Rows.Count; i++)
                {
                   
                 
                     
                     
                    pdata.Add(new attemptedqstns
                    {

                        
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        
                        user_Selected_Ans= Convert.ToInt32(dt.Rows[i]["user_Selected_Ans"].ToString()),
                        TextFieldAnswer = dt.Rows[i]["TextFieldAnswer"].ToString(),

                    });

                }

            }
           
            return pdata;

        }


        [Route("api/BeginAssessementController/GetAssesmentsInitiated")]
        [HttpGet]
        public IEnumerable<AssScheduleModel> GetAssesmentsInitiated(int userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"

SELECT schedule_assessment.Schedule_Assessment_id,ab.ass_template_id, ab.assessment_name, ab.assessment_description,DATE(ab.created_date)as  created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name,sn.SubType_Name, cn.Competency_Name,DATE(startDate) as startDate,DATE(endDate) as endDate
FROM risk.assessment_builder ab

JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
Join risk.schedule_assessment on risk.schedule_assessment.ass_template_id=ab.ass_template_id
WHERE ab.Status = 'Active' and schedule_assessment.AssessmentStatus='Assessement Initiated' And userid=@UserID
 and endDate >= CURRENT_DATE ", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@UserID", userid);


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssScheduleModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssScheduleModel
                    {
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        ass_template_id = Convert.ToInt32(dt.Rows[i]["ass_template_id"]),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString()
                    });
                }
            }
            return pdata;
        }





        [Route("api/BeginAssessementController/UpdateStatus")]
        [HttpPost]
        
        public IActionResult UpdateStatus([FromBody] RepeatFrequencyModel RepeatFrequencyModel)
        
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            try
            {
                con.Open();
                MySqlCommand cmd;
                if(RepeatFrequencyModel.AssessmentStatus == "Acknowledged" )
                 cmd = new MySqlCommand(@"SELECT * FROM risk.schedule_assessment where userid = @userid and ass_template_id=@ass_template_id and AssessmentStatus='Assessment Scheduled'", con);
                else if(RepeatFrequencyModel.AssessmentStatus == "Initiated" || RepeatFrequencyModel.AssessmentStatus == "Attempt Later")
                    cmd = new MySqlCommand(@"SELECT * FROM risk.schedule_assessment where userid = @userid and ass_template_id=@ass_template_id and AssessmentStatus='Assessement Acknowledged' ", con);
                else
                 cmd = new MySqlCommand(@"SELECT * FROM risk.schedule_assessment where userid = @userid and ass_template_id=@ass_template_id and AssessmentStatus='Assessement Initiated' ", con);

                
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModel.ass_template_id);
                cmd.Parameters.AddWithValue("@UserID", RepeatFrequencyModel.userid);
               
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                //con.Close();


                if (dt.Rows.Count > 0)
                {
                    int Schedule_Assessment_id = Convert.ToInt32(dt.Rows[0]["Schedule_Assessment_id"].ToString());

                    string updateQuery = "update schedule_assessment set AssessmentStatus =@AssessmentStatus where Schedule_Assessment_id=@Schedule_Assessment_id";

                    using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery, con))
                    {
                         myCommand1.Parameters.AddWithValue("@Schedule_Assessment_id", Schedule_Assessment_id);
                        if (RepeatFrequencyModel.AssessmentStatus == "Acknowledged")
                        {
                            myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessement Acknowledged");
                        }
                        else if (RepeatFrequencyModel.AssessmentStatus == "Initiated")
                        {
                            myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessement Initiated");
                        }
                        else if (RepeatFrequencyModel.AssessmentStatus == "Attempt Later")
                        {
                            myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessement Attempt Later");
                        }
                        else
                        {
                            myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Completed");

                        }
                        myCommand1.ExecuteNonQuery();





                    }


                }
                else
                {
                    return Ok();
                }

                return Ok("successfully");

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


        [Route("api/BeginAssessementController/UpdateExpiredStatus")]
        [HttpPost]

        public IActionResult UpdateExpiredStatus([FromBody] RepeatFrequencyModel RepeatFrequencyModel)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            try
            {
                con.Open();
                MySqlCommand cmd;
             
                    cmd = new MySqlCommand(@"SELECT * FROM risk.schedule_assessment 
where userid = @userid and AssessmentStatus='Assessment Scheduled'and endDate < CURRENT_DATE", con);
               

                cmd.CommandType = CommandType.Text;
              
                cmd.Parameters.AddWithValue("@UserID", RepeatFrequencyModel.userid);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                //con.Close();


                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"].ToString());

                        string updateQuery = "update schedule_assessment set AssessmentStatus =@AssessmentStatus where Schedule_Assessment_id=@Schedule_Assessment_id";

                        using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@Schedule_Assessment_id", Schedule_Assessment_id);
                                myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessement Expired");

                            
                            myCommand1.ExecuteNonQuery();





                        }

                    }
                }
                else
                {
                    return Ok();
                }

                return Ok("successfully");

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




        [Route("api/BeginAssessementController/AssessmentScoreDetails")]
        [HttpPost]

        public IActionResult AssessmentScore([FromBody] user_ass_ans_details assessementscore)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            try
            {
                con.Open();
                MySqlCommand cmd;

                cmd = new MySqlCommand(@"SELECT
(SELECT 
    SUM(correct_answers_total_weightage) AS total_correct_answers_weightage
FROM (
    SELECT 
        SUM(score_weightage * checklevel_weightage) AS correct_answers_total_weightage
    FROM 
        risk.user_ass_ans_details 
    INNER JOIN 
        questionbank ON questionbank.question_id = user_ass_ans_details.question_id
    WHERE 
        AssessementTemplateID = @AssessementTemplateID AND 
        user_ass_ans_details.userid = @userid AND 
        user_Selected_Ans = questionbank.correct_answer
    GROUP BY 
        user_ass_ans_details.question_id
) AS correct_answers_subquery)
as Taskownerscore
,(SELECT 
    SUM(total) AS total_sum
FROM (
    SELECT 
        score_weightage * checklevel_weightage AS total
    FROM 
        assessment_builder
    INNER JOIN 
        assessment_generation_details ON assessment_generation_details.Assessment_generationID = assessment_builder.Assessment_generationID
    INNER JOIN 
        questionbank ON questionbank.question_id = assessment_generation_details.question_id
    INNER JOIN 
        questionbank_options ON questionbank_options.question_id = questionbank.question_id
    INNER JOIN 
        user_ass_ans_details ON user_ass_ans_details.AssessementTemplateID = assessment_builder.ass_template_id
    WHERE 
        ass_template_id = @AssessementTemplateID
    GROUP BY 
        questionbank.question_id, user_Selected_Ans, questionbank.correct_answer
) AS total_query) as TotalScore


 from risk.user_ass_ans_details 
inner join questionbank on questionbank.question_id=user_ass_ans_details.question_id
where AssessementTemplateID=@AssessementTemplateID and user_ass_ans_details.userid=@userid; ", con);


                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@AssessementTemplateID", assessementscore.AssessementTemplateID);
                cmd.Parameters.AddWithValue("@userid", assessementscore.UserID);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {

                    decimal TotalScore = Convert.ToDecimal(dt.Rows[0]["TotalScore"].ToString());
                    decimal TaskOwnerScore = Convert.ToDecimal(dt.Rows[0]["TaskOwnerScore"].ToString());
                    decimal percentagee = TaskOwnerScore / TotalScore;
                    decimal percentage1 = (percentagee * 100);
                    string roundedPercentage = percentage1.ToString("0.000");
                    decimal percentage = Convert.ToDecimal(roundedPercentage.ToString());
                    MySqlCommand cmd1 = new MySqlCommand(@"
SELECT * FROM risk.scheduled_ass_status where AssessementTemplateID=@AssessementTemplateID and UserID=@UserID", con);

                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.AddWithValue("@AssessementTemplateID", assessementscore.AssessementTemplateID);
                    cmd1.Parameters.AddWithValue("@UserID", assessementscore.UserID);

                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    // con.Close();

                    if (dt1.Rows.Count > 0)
                    {
                        int Scheduled_Ass_StatusID = Convert.ToInt32(dt1.Rows[0]["Scheduled_Ass_StatusID"].ToString());
                        string updatestatus = (@"update scheduled_ass_status set Status=@Status,TaskOwnerScore=@TaskOwnerScore,TotalScore=@TotalScore,percentage=@percentage where Scheduled_Ass_StatusID=@Scheduled_Ass_StatusID ");
                        using (MySqlCommand myCommand12 = new MySqlCommand(updatestatus, con))
                        {
                            myCommand12.Parameters.AddWithValue("@TaskOwnerScore", TaskOwnerScore);
                            myCommand12.Parameters.AddWithValue("@TotalScore", TotalScore);
                            myCommand12.Parameters.AddWithValue("@percentage", percentage);
                            myCommand12.Parameters.AddWithValue("@Status", "Assessment Completed");
                            myCommand12.Parameters.AddWithValue("@Scheduled_Ass_StatusID", Scheduled_Ass_StatusID);
                            myCommand12.ExecuteNonQuery();

                        }




                    }

                    else
                    {
                        return Ok();
                    }

                   
                }
                return Ok("successfully");
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





    }
}
