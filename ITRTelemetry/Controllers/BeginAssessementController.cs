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
        public IEnumerable<AssScheduleModelNew> GetActiveAssesment(int userid)
        {
            string mappeduser = userid.ToString();
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
 SELECT distinct schedule_assessment.Schedule_Assessment_id,schedule_assessment.verson_no,ab.assessment_builder_versionsID,schedule_assessment.AssessmentStatus,ab.ass_template_id,tbluser.firstname, ab.assessment_name, ab.assessment_description,DATE(schedule_assessment.Date_Of_Request)as  created_date, ab.status, ab.keywords,
       tn.Type_Name,sn.SubType_Name, cn.Competency_Name,DATE(startDate) as startDate,DATE(endDate) as endDate,schedule_assessment.objective,schedule_assessment.message,uq_ass_schid,tbluser2.firstname AS builder_version_firstname
FROM risk.assessment_builder_versions ab
inner JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
inner JOIN risk.type tn ON tn.Type_id = ab.Type_id
inner JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
inner Join risk.schedule_assessment on risk.schedule_assessment.ass_template_id=ab.ass_template_id   
inner join tbluser on tbluser.USR_ID=schedule_assessment.mapped_user
inner join tbluser AS tbluser2 ON tbluser2.USR_ID = ab.user_id
WHERE ab.Status = 'Active' 
 And mapped_user=@mapped_user and( schedule_assessment.AssessmentStatus='Assessment Scheduled' or schedule_assessment.AssessmentStatus='Assessment Rescheduled') and  schedule_assessment.status = 'Active'
 and (CURRENT_DATE>=endDate or startDate=CURRENT_DATE) and schedule_assessment.verson_no=ab.verson_no ", con);

            // and schedule_assessment.AssessmentStatus<>'Assessment Completed'
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@mapped_user", userid);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssScheduleModelNew>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssScheduleModelNew
                    {
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        //assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_builder_versionsID = Convert.ToInt32(dt.Rows[i]["assessment_builder_versionsID"]),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["AssessmentStatus"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        verson_no = dt.Rows[i]["verson_no"].ToString(),
                        objective = dt.Rows[i]["objective"].ToString(),
                        message = dt.Rows[i]["message"].ToString(),
                        builder_version_firstname = dt.Rows[i]["builder_version_firstname"].ToString()
                    });
                }
            }
            return pdata;
        }



        [Route("api/BeginAssessementController/GetQuestions")]
        [HttpGet]

        public List<BeginAssModel> GetQuestions(string AssessementTemplateID, string versonNo)
        {

            var pdata = new List<BeginAssModel>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select distinct questionbank.question_id,question,base64,response_type,question_hint,show_hint from assessment_builder_versions
inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder_versions.Assessment_generationID
inner join questionbank on questionbank.question_id=assessment_generation_details.question_id
inner join questionbank_options on questionbank_options.question_id=questionbank.question_id
where ass_template_id=@AssessementTemplateID AND verson_no=@verson_no AND assessment_generation_details.Status=@Status order by questionbank.question_id", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@verson_no", versonNo);
            cmd.Parameters.AddWithValue("@Status", "Active");

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {


                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    int question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString());
                    MySqlCommand cmd1 = new MySqlCommand(@"select distinct options,questionbank_optionID,questionbank.question_id,OptionId from assessment_builder_versions
inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder_versions.Assessment_generationID
inner join questionbank on questionbank.question_id=assessment_generation_details.question_id
inner join questionbank_options on questionbank_options.question_id=questionbank.question_id
where ass_template_id=@AssessementTemplateID AND verson_no=@verson_no and questionbank.question_id='" + question_id + "' order by questionbank.question_id ", con);

                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
                    cmd1.Parameters.AddWithValue("@verson_no", versonNo);

                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    List<BeginAssQstnsoptions> options = new List<BeginAssQstnsoptions>();
                    for (var j = 0; j < dt1.Rows.Count; j++)
                    {
                        BeginAssQstnsoptions options1 = new BeginAssQstnsoptions();
                        options1.questionbank_optionID = Convert.ToInt32(dt1.Rows[j]["questionbank_optionID"].ToString());
                        options1.OptionId = Convert.ToInt32(dt1.Rows[j]["OptionId"].ToString());
                        options1.options = dt1.Rows[j]["options"].ToString();
                        options.Add(options1);
                    }
                    pdata.Add(new BeginAssModel
                    {

                        question = dt.Rows[i]["question"].ToString(),
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        base64 = dt.Rows[i]["base64"].ToString(),
                        show_hint = dt.Rows[i]["show_hint"].ToString(),
                        question_hint = dt.Rows[i]["question_hint"].ToString(),
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
                if (UserAnswers.Count>0)
                {
                    if (UserAnswers[0].finalsubmit == 0)
                    {
                        string insertQuerystatus = (@"insert into scheduled_ass_status(AssessementTemplateID,StartDateTime,EndDateTime,UserID,Status,CreatedDate,uq_ass_schid)values
                    (@AssessementTemplateID,@StartDateTime,@EndDateTime,@UserID,@Status,@CreatedDate,@uq_ass_schid)");



                        using (MySqlCommand myCommand11 = new MySqlCommand(insertQuerystatus, con))
                        {

                            myCommand11.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[0].AssessementTemplateID);
                            // myCommand11.Parameters.AddWithValue("@EndDateTime", DateTime.Now);
                            myCommand11.Parameters.AddWithValue("@UserID", UserAnswers[0].UserID);
                            myCommand11.Parameters.AddWithValue("@uq_ass_schid", UserAnswers[0].uq_ass_schid);
                            myCommand11.Parameters.AddWithValue("@Status", "Active");
                            myCommand11.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                            myCommand11.Parameters.AddWithValue("@StartDateTime", DateTime.Now);
                            myCommand11.Parameters.AddWithValue("@EndDateTime", DateTime.Now);


                            myCommand11.ExecuteNonQuery();

                        }
                    }
                    else if (UserAnswers[0].finalsubmit == 1)
                    {
                        MySqlCommand cmd = new MySqlCommand(@"
SELECT * FROM risk.scheduled_ass_status where AssessementTemplateID=@AssessementTemplateID and UserID=@UserID and uq_ass_schid=@uq_ass_schid", con);

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[0].AssessementTemplateID);
                        cmd.Parameters.AddWithValue("@UserID", UserAnswers[0].UserID);
                        cmd.Parameters.AddWithValue("@uq_ass_schid", UserAnswers[0].uq_ass_schid);

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

                            string insertQuerystatus = (@"insert into scheduled_ass_status(AssessementTemplateID,StartDateTime,EndDateTime,UserID,Status,CreatedDate,uq_ass_schid)values
                    (@AssessementTemplateID,@StartDateTime,@EndDateTime,@UserID,@Status,@CreatedDate,@uq_ass_schid)");



                            using (MySqlCommand myCommand11 = new MySqlCommand(insertQuerystatus, con))
                            {

                                myCommand11.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[0].AssessementTemplateID);
                                // myCommand11.Parameters.AddWithValue("@EndDateTime", DateTime.Now);
                                myCommand11.Parameters.AddWithValue("@UserID", UserAnswers[0].UserID);
                                myCommand11.Parameters.AddWithValue("@uq_ass_schid", UserAnswers[0].uq_ass_schid);
                                myCommand11.Parameters.AddWithValue("@Status", "Active");
                                myCommand11.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                myCommand11.Parameters.AddWithValue("@StartDateTime", DateTime.Now);
                                myCommand11.Parameters.AddWithValue("@EndDateTime", DateTime.Now);


                                myCommand11.ExecuteNonQuery();

                            }
                        }
                    }

                    for (int i = 0; i < UserAnswers.Count; i++)
                    {

                        MySqlCommand cmd = new MySqlCommand(@"
SELECT * FROM risk.user_ass_ans_details where AssessementTemplateID=@AssessementTemplateID and UserID=@UserID and question_id=@question_id and uq_ass_schid=@uq_ass_schid", con);

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@AssessementTemplateID", UserAnswers[i].AssessementTemplateID);
                        cmd.Parameters.AddWithValue("@UserID", UserAnswers[i].UserID);
                        cmd.Parameters.AddWithValue("@question_id", UserAnswers[i].question_id);
                        cmd.Parameters.AddWithValue("@uq_ass_schid", UserAnswers[i].uq_ass_schid);

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

                            insertQuery = (@"insert into user_ass_ans_details(AssessementTemplateID,question_id,user_Selected_Ans,UserID,TypeofQuestion,TextFieldAnswer,Status,CreatedDate,uq_ass_schid)values
                    (@AssessementTemplateID,@question_id,@user_Selected_Ans,@UserID,@TypeofQuestion,@TextFieldAnswer,@Status,@CreatedDate,@uq_ass_schid)");


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
                                myCommand1.Parameters.AddWithValue("@uq_ass_schid", UserAnswers[i].uq_ass_schid);
                                myCommand1.Parameters.AddWithValue("@Status", "Active");
                                myCommand1.Parameters.AddWithValue("@CreatedDate", DateTime.Now);


                                myCommand1.ExecuteNonQuery();

                            }
                        }
                    }

                }
                else
                {

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

        public IEnumerable<object> GetUserAnswersList(string AssessementTemplateID, int UserID, string uq_ass_schid)
        {

            var pdata = new List<attemptedqstns>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT * FROM risk.user_ass_ans_details where AssessementTemplateID=@AssessementTemplateID and UserID=@UserID and uq_ass_schid=@uq_ass_schid", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);

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

                        user_Selected_Ans = Convert.ToInt32(dt.Rows[i]["user_Selected_Ans"].ToString()),
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

SELECT distinct schedule_assessment.Schedule_Assessment_id,ab.ass_template_id,tbluser.firstname, ab.assessment_name, ab.assessment_description,schedule_assessment.verson_no,DATE(schedule_assessment.created_date)as  created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name,sn.SubType_Name, cn.Competency_Name,DATE(startDate) as startDate,DATE(endDate) as endDate,schedule_assessment.objective,schedule_assessment.message,uq_ass_schid,tbluser2.firstname AS builder_version_firstname
FROM risk.assessment_builder_versions ab

JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
Join risk.schedule_assessment on risk.schedule_assessment.ass_template_id=ab.ass_template_id
join tbluser on tbluser.USR_ID=schedule_assessment.mapped_user
inner join tbluser AS tbluser2 ON tbluser2.USR_ID = ab.user_id
WHERE ab.Status = 'Active' and (schedule_assessment.AssessmentStatus='Assessement Initiated' or schedule_assessment.AssessmentStatus='Assessement Acknowledged' or schedule_assessment.AssessmentStatus='Assessement Attempt Later' )
And mapped_user=@mapped_user
 and (CURRENT_DATE>=endDate or startDate=CURRENT_DATE) ", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@mapped_user", userid);


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
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        verson_no = dt.Rows[i]["verson_no"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        created_dates = Convert.ToDateTime(dt.Rows[i]["created_date"]).ToString("dd-MM-yyyy"),
                        startDates = Convert.ToDateTime(dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy"),
                        endDates = Convert.ToDateTime(dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        objective = dt.Rows[i]["objective"].ToString(),
                        message = dt.Rows[i]["message"].ToString(),
                        builder_version_firstname = dt.Rows[i]["builder_version_firstname"].ToString()

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
                if (RepeatFrequencyModel.AssessmentStatus == "Acknowledged")
                    cmd = new MySqlCommand(@"SELECT * FROM risk.schedule_assessment where mapped_user = @mapped_user and ass_template_id=@ass_template_id and AssessmentStatus='Assessment Scheduled' and uq_ass_schid=@uq_ass_schid", con);
                else if (RepeatFrequencyModel.AssessmentStatus == "Initiated" || RepeatFrequencyModel.AssessmentStatus == "Attempt Later" || RepeatFrequencyModel.AssessmentStatus == "Assessment Rescheduled")
                    cmd = new MySqlCommand(@"SELECT * FROM risk.schedule_assessment where mapped_user = @mapped_user and ass_template_id=@ass_template_id and AssessmentStatus='Assessement Acknowledged'and uq_ass_schid=@uq_ass_schid  ", con);
                else
                    cmd = new MySqlCommand(@"SELECT * FROM risk.schedule_assessment where mapped_user = @mapped_user and ass_template_id=@ass_template_id and (AssessmentStatus='Assessement Initiated' or AssessmentStatus='Assessement Acknowledged' or AssessmentStatus='Assessment Rescheduled') and uq_ass_schid=@uq_ass_schid ", con);


                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModel.ass_template_id);
                cmd.Parameters.AddWithValue("@mapped_user", RepeatFrequencyModel.userid);
                cmd.Parameters.AddWithValue("@uq_ass_schid", RepeatFrequencyModel.uq_ass_schid);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                //con.Close();


                if (dt.Rows.Count > 0)
                {
                    int Schedule_Assessment_id = Convert.ToInt32(dt.Rows[0]["Schedule_Assessment_id"].ToString());

                    string updateQuery = "update schedule_assessment set AssessmentStatus =@AssessmentStatus where Schedule_Assessment_id=@Schedule_Assessment_id";
                    string updateDateQuery = "update schedule_assessment set acknowledgemet_date =@acknowledgemet_date where Schedule_Assessment_id=@Schedule_Assessment_id";

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

                    //Update Acknowledgement Date Column

                    if (RepeatFrequencyModel.AssessmentStatus == "Acknowledged")
                    {
                        using (MySqlCommand myCommand2 = new MySqlCommand(updateDateQuery, con))
                        {
                            myCommand2.Parameters.AddWithValue("@Schedule_Assessment_id", Schedule_Assessment_id);
                            myCommand2.Parameters.AddWithValue("@acknowledgemet_date", DateTime.Now);
                            myCommand2.ExecuteNonQuery();
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
where mapped_user = @mapped_user and uq_ass_schid=@uq_ass_schid and  CURRENT_DATE > endDate ", con);


                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@mapped_user", RepeatFrequencyModel.userid);
                cmd.Parameters.AddWithValue("@uq_ass_schid", RepeatFrequencyModel.uq_ass_schid);

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

                cmd = new MySqlCommand(@"
select  sum(score_weightage * checklevel_weightage) AS TotalScore,(SELECT SUM(weightage) AS total_weightage
FROM (
    SELECT DISTINCT
        uad.question_id,
        uad.user_Selected_Ans,
        qb.correct_answer,
        (SELECT (score_weightage * checklevel_weightage) 
         FROM questionbank 
         WHERE correct_answer = uad.user_Selected_Ans 
         AND question_id = uad.question_id) AS weightage
    FROM
        user_ass_ans_details uad
    INNER JOIN questionbank qb ON qb.question_id = uad.question_id
    WHERE
        uad.Status = 'Active'
        AND uad.TypeofQuestion = 'Multiple'
        AND uad.AssessementTemplateID = @AssessementTemplateID
        AND uad.uq_ass_schid = @uq_ass_schid
        AND uad.UserID = @userid
) AS subquery) as Taskownerscore from assessment_builder_versions
inner join assessment_generation_details on assessment_generation_details.Assessment_generationID=assessment_builder_versions.Assessment_generationID
inner join questionbank qb on qb.question_id=assessment_generation_details.question_id
    where ass_template_id = @AssessementTemplateID
     ", con);





                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@AssessementTemplateID", assessementscore.AssessementTemplateID);
                cmd.Parameters.AddWithValue("@userid", assessementscore.UserID);
                cmd.Parameters.AddWithValue("@uq_ass_schid", assessementscore.uq_ass_schid);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    string taskownscore;
                    if (dt.Rows[0]["TaskOwnerScore"].ToString() == "" || dt.Rows[0]["TaskOwnerScore"].ToString() == null)
                        taskownscore = "0";
                    else
                        taskownscore = dt.Rows[0]["TaskOwnerScore"].ToString();


                    decimal TotalScore = Convert.ToDecimal(dt.Rows[0]["TotalScore"].ToString());
                    decimal TaskOwnerScore = Convert.ToDecimal(taskownscore);
                    decimal percentagee = TaskOwnerScore / TotalScore;
                    decimal percentage1 = (percentagee * 100);
                    string roundedPercentage = percentage1.ToString("0.000");
                    decimal percentage = Convert.ToDecimal(roundedPercentage.ToString());
                    MySqlCommand cmd1 = new MySqlCommand(@"
SELECT * FROM risk.scheduled_ass_status where AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid  and UserID=@UserID", con);

                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.AddWithValue("@AssessementTemplateID", assessementscore.AssessementTemplateID);
                    cmd1.Parameters.AddWithValue("@UserID", assessementscore.UserID);
                    cmd1.Parameters.AddWithValue("@uq_ass_schid", assessementscore.uq_ass_schid);
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




        // for update of Auto udate of status

        [Route("api/BeginAssessementController/autoUpdateExpiredStatus")]
        [HttpPost]

        public IActionResult autoUpdateExpiredStatus()

        {

            { 
                try
                {
                    using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                    {
                        con.Open();

                        // Step 1: Auto-update expired assessments
                        string updateQuery = @"
                UPDATE schedule_assessment 
                SET AssessmentStatus = @AssessmentStatus 
                WHERE CURRENT_DATE > endDate 
                AND AssessmentStatus != 'Assessment Completed'";

                        using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, con))
                        {
                            updateCmd.Parameters.AddWithValue("@AssessmentStatus", "Assessment Expired");
                            updateCmd.ExecuteNonQuery();
                        }

                        // Step 2: Fetch all assessments (no userid condition)
                        string selectQuery = @"SELECT * FROM schedule_assessment";

                        using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, con))
                        {
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(selectCmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                var result = dt.AsEnumerable().Select(row => new
                                {
                                    schedule_Assessment_id = row["Schedule_Assessment_id"],
                                    endDate = Convert.ToDateTime(row["endDate"]),
                                    status = row["AssessmentStatus"],
                                    // Add other fields as needed
                                });

                                return Ok(result);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }




    }
}
