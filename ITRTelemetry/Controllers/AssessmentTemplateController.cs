using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;




namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AssessmentTemplateController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public AssessmentTemplateController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }



        [Route("api/Assessment/GetListOfQuestionsByID/{ass_template_id}")]
        [HttpGet]
        public IEnumerable<AssessmentTemplateModel> GetListOfQuestions(string ass_template_id)
        {
            var pdata = new List<AssessmentTemplateModel>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
 
      SELECT qd.*, sb.Subject_Name, tb.Topic_Name, 
             ab.ass_template_id, ab.assessment_name,ab.assessment_description, ag.Assessment_generationID ,cl.Skill_Level_Name FROM risk.questionbank qd  
               JOIN risk.assessment_generation_details agd ON agd.question_id = qd.question_id  
               JOIN risk.assessment_generation ag ON ag.Assessment_generationID = agd.Assessment_generationID  
               JOIN risk.subject sb ON sb.Subject_id = qd.subjectid  
               JOIN risk.topic tb ON tb.Topic_id = qd.topicid 
               JOIN risk.competency_check_level cl ON cl.check_level_id = qd.check_level
              JOIN risk.assessment_builder_versions ab ON ab.Assessment_generationID = ag.Assessment_generationID 
               WHERE agd.Status = 'Active' && ab.ass_template_id = @ass_template_id &&  ab.Status = 'Active'", con);

            cmd.Parameters.AddWithValue("@ass_template_id", ass_template_id);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
           

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {

                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssessmentTemplateModel
                    {
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        question = dt.Rows[i]["question"].ToString(),
                        response_type = Convert.ToInt32(dt.Rows[i]["response_type"].ToString()),
                        no_of_selectionchoices = Convert.ToInt32(dt.Rows[i]["no_of_selectionchoices"].ToString()),
                        correct_answer = Convert.ToInt32(dt.Rows[i]["correct_answer"].ToString()),
                        question_hint = dt.Rows[i]["question_hint"].ToString(),
                        questionmarked_favourite = dt.Rows[i]["questionmarked_favourite"].ToString(),
                        score_weightage = Convert.ToInt32(dt.Rows[i]["score_weightage"].ToString()),
                        check_level =dt.Rows[i]["Skill_Level_Name"].ToString(),
                        checklevel_weightage = Convert.ToSingle(dt.Rows[i]["checklevel_weightage"].ToString()),
                        estimated_time = Convert.ToInt32(dt.Rows[i]["estimated_time"].ToString()),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        assessor_randomselection = dt.Rows[i]["assessor_randomselection"].ToString(),
                        assessment_randomsetting = dt.Rows[i]["assessment_randomsetting"].ToString(),
                        subjectid = Convert.ToInt32(dt.Rows[i]["subjectid"].ToString()),
                        topicid = Convert.ToInt32(dt.Rows[i]["topicid"].ToString()),
                        ref_to_governance_control = dt.Rows[i]["ref_to_governance_control"].ToString(),
                        question_disabled = dt.Rows[i]["question_disabled"].ToString(),
                        objective = dt.Rows[i]["objective"].ToString(),
                        
                        //base64 = dt.Rows[i]["base64"].ToString(),
                        //Assessment_generationID = Convert.ToInt32(dt.Rows[i]["Assessment_generationID"].ToString()),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[0]["assessment_description"].ToString(),
                        userid = Convert.ToInt32(dt.Rows[i]["userid"].ToString()),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"].ToString()),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                    });

                }

            }
            return pdata;
        }



        //Method for add new questions in update template page 


        [Route("api/QuestionBank/UpdateNewQues/{ass_template_id}")]
        [HttpPost]
        public IActionResult UpdateNewQues([FromBody] QuestionBankModel QuestionBankModels,int ass_template_id)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "insert into questionbank(question,response_type,no_of_selectionchoices,correct_answer,question_hint,questionmarked_favourite,score_weightage,check_level,checklevel_weightage,estimated_time,keywords,assessor_randomselection,assessment_randomsetting,subjectid,topicid,ref_to_governance_control,question_disabled,objective,base64,userid,question_weightage,created_date,status)values(@question,@response_type,@no_of_selectionchoices,@correct_answer,@question_hint,@questionmarked_favourite,@score_weightage,@check_level,@checklevel_weightage,@estimated_time,@keywords,@assessor_randomselection,@assessment_randomsetting,@subjectid,@topicid,@ref_to_governance_control,@question_disabled,@objective,@base64,@userid,@question_weightage,@created_date,@status)";

            try
            {
                con.Open();
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    float question_weightage = QuestionBankModels.score_weightage * QuestionBankModels.checklevel_weightage;
                    myCommand1.Parameters.AddWithValue("@question", QuestionBankModels.question);
                    myCommand1.Parameters.AddWithValue("@response_type", QuestionBankModels.response_type);
                    myCommand1.Parameters.AddWithValue("@no_of_selectionchoices", QuestionBankModels.no_of_selectionchoices);
                    myCommand1.Parameters.AddWithValue("@correct_answer", QuestionBankModels.correct_answer);
                    myCommand1.Parameters.AddWithValue("@question_hint", QuestionBankModels.question_hint);
                    myCommand1.Parameters.AddWithValue("@questionmarked_favourite", QuestionBankModels.questionmarked_favourite);
                    myCommand1.Parameters.AddWithValue("@score_weightage", QuestionBankModels.score_weightage);
                    myCommand1.Parameters.AddWithValue("@check_level", QuestionBankModels.check_level);
                    myCommand1.Parameters.AddWithValue("@checklevel_weightage", QuestionBankModels.checklevel_weightage);
                    myCommand1.Parameters.AddWithValue("@estimated_time", QuestionBankModels.estimated_time);
                    myCommand1.Parameters.AddWithValue("@keywords", QuestionBankModels.keywords);
                    myCommand1.Parameters.AddWithValue("@assessor_randomselection", QuestionBankModels.assessor_randomselection);
                    myCommand1.Parameters.AddWithValue("@assessment_randomsetting", QuestionBankModels.assessment_randomsetting);
                    myCommand1.Parameters.AddWithValue("@subjectid", QuestionBankModels.subjectid);
                    myCommand1.Parameters.AddWithValue("@topicid", QuestionBankModels.topicid);
                    myCommand1.Parameters.AddWithValue("@ref_to_governance_control", QuestionBankModels.ref_to_governance_control);
                    myCommand1.Parameters.AddWithValue("@question_disabled", QuestionBankModels.question_disabled);
                    myCommand1.Parameters.AddWithValue("@objective", QuestionBankModels.objective);
                    myCommand1.Parameters.AddWithValue("@base64", QuestionBankModels.base64);
                    myCommand1.Parameters.AddWithValue("@question_weightage", question_weightage);
                    myCommand1.Parameters.AddWithValue("@userid", QuestionBankModels.userid);
                    myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@status", "Active");

                    myCommand1.ExecuteNonQuery();

                    // Get the last inserted primary key value
                    int questionID = Convert.ToInt32(myCommand1.LastInsertedId.ToString());



                    // Fetch generationid from generation based on userid
                    int userId = QuestionBankModels.userid;

                    string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_builder WHERE ass_template_id = @ass_template_id ";

                    using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                    {
                        myCommand4.Parameters.AddWithValue("@ass_template_id", ass_template_id);

                        // Execute the query and retrieve the generationid
                        object generationIdObject = myCommand4.ExecuteScalar();

                        if (generationIdObject != null)
                        {
                            int AssessmentgenerationID = Convert.ToInt32(generationIdObject);



                            //Query is added for insert values in assessment details

                          
                            string insertAssessGenDetailsQuery = "INSERT INTO assessment_generation_details (Assessment_generationID,question_id, CreatedDate, Status) VALUES (@Assessment_generationID,@question_id, @CreatedDate, @Status)";

                            using (MySqlCommand myCommand3 = new MySqlCommand(insertAssessGenDetailsQuery, con))
                            {
                                // Replace the parameters with the properties of AssessGenDetailsModel
                                myCommand3.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                                myCommand3.Parameters.AddWithValue("@question_id", questionID);
                                myCommand3.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                myCommand3.Parameters.AddWithValue("@Status","Active");

                                myCommand3.ExecuteNonQuery();
                            }
                        }

                        foreach (Options option in QuestionBankModels.options)
                        {
                            // Access and work with each option
                            Console.WriteLine($"Index: {option.index}, Value: {option.value}");
                            string insertQuery1 = "insert into questionbank_options(question_id,options,created_date,status,OptionId)values(@question_id,@options,@created_date,@status,@OptionId)";


                            using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery1, con))
                            {
                                myCommand2.Parameters.AddWithValue("@question_id", questionID);
                                myCommand2.Parameters.AddWithValue("@options", option.value);
                                myCommand2.Parameters.AddWithValue("@OptionId", option.index + 1);

                                myCommand2.Parameters.AddWithValue("@created_date", DateTime.Now);
                                myCommand2.Parameters.AddWithValue("@status", "Active");

                                myCommand2.ExecuteNonQuery();
                            }
                        }



                    }


                    return Ok("Question added successfully");
                }
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

        //provide access get
        [Route("api/Assessment/GetActiveAssesmentbyuser/{UserID}")]
        [HttpGet]
        public IEnumerable<AssessmentTemplateModel> GetActiveAssesmentbyuser(int UserID)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT ab.ass_template_id, ab.assessment_name, ab.assessment_description,ab.created_date, ab.status, ab.keywords, ab.assessment_builder_id,ab.user_id,tu.firstname,
       tn.Type_Name, sn.SubType_Name, cn.Competency_Name,ab.total_questions,ab.total_estimated_time,ab.verson_no
FROM risk.assessment_builder_versions ab
JOIN risk.tbluser tu ON tu.USR_ID  = ab.user_id
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
WHERE ab.Status = 'Active' and ab.user_id = @UserID ", con);
            cmd.Parameters.AddWithValue("@UserID", UserID);
            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssessmentTemplateModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssessmentTemplateModel
                    {
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"]),
                        total_estimated_time = Convert.ToInt32(dt.Rows[i]["total_estimated_time"]),
                        verson_no = Convert.ToInt32(dt.Rows[i]["verson_no"]),
                        firstname = dt.Rows[i]["firstname"].ToString()
                    });
                }
            }
            return pdata;
        }

        // for view assessment
        [Route("api/Assessment/GetActiveAssesmentforview/{UserID}")]
        [HttpGet]
        public IEnumerable<AssessmentTemplateModelNew> GetActiveAssesmentforview(int userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT  ab.ass_template_id, ab.assessment_name, ab.assessment_description,ab.created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name, sn.SubType_Name, cn.Competency_Name,um.firstname,ab.total_estimated_time,ab.total_questions,um.USR_ID
FROM risk.assessment_builder_versions ab
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.tbluser um on um.USR_ID = ab.user_id
JOIN risk.assement_provideacess aPa on aPa.AssessementTemplateID=ab.ass_template_id
WHERE ab.Status = 'Active' and aPa.UserID = @userid and( aPa.Access_Permissions='2' or aPa.Access_Permissions='1' )and aPa.Status='Active'", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("userid", userid);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssessmentTemplateModelNew>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new AssessmentTemplateModelNew
                    {
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"]),
                        total_estimated_time = Convert.ToInt32(dt.Rows[i]["total_estimated_time"]),
                    });
                }
            }
            return pdata;
        }






        [Route("api/Assessment/GetActiveAssesment/{UserID}")]
        [HttpGet]
        public IEnumerable<AssessmentTemplateModelNew> GetActiveAssesment(int userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT  ab.ass_template_id, ab.assessment_name, ab.assessment_description,ab.created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name, sn.SubType_Name, cn.Competency_Name,um.firstname,ab.total_estimated_time,ab.total_questions,um.USR_ID
FROM risk.assessment_builder_versions ab
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.tbluser um on um.USR_ID = ab.user_id
JOIN risk.assement_provideacess aPa on aPa.AssessementTemplateID=ab.ass_template_id
WHERE ab.Status = 'Active' and aPa.UserID = @userid and (aPa.Access_Permissions='2' or aPa.Access_Permissions='1')  and aPa.Status='Active'", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("userid", userid);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssessmentTemplateModelNew>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new AssessmentTemplateModelNew
                    {
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"]),
                        total_estimated_time = Convert.ToInt32(dt.Rows[i]["total_estimated_time"]),
                    });
                }
            }
            return pdata;
        }






        [Route("api/Assessment/GetActiveAssesmentbyid/{tempid}")]
        [HttpGet]
        public IEnumerable<AssessmentPublisherModel> GetActiveAssesmentbytempid(string tempid)
        {
           
            return this.mySqlDBContext.AssessmentPublisherModels.Where(x => x.ass_template_id == tempid).ToList();
        }




        [Route("api/Assessment/GetActiveAssesByID/{ass_template_id}")]
        [HttpGet]
        public IEnumerable<AssessmentTemplateModelNew> GetActiveAssessmentByID(string ass_template_id)
        {
            var pdata = new List<AssessmentTemplateModelNew>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT ab.ass_template_id, ab.assessment_name, ab.created_date, ab.updated_date, 
       us1.firstname AS user_firstname, us2.firstname AS updated_user_firstname,
       ab.assessment_description, ab.status, ab.show_hint, 
       ab.keywords, ab.assessment_builder_id, ab.show_explaination,
       tn.Type_Name, sn.SubType_Name, cn.Competency_Name,ab.total_questions
FROM risk.assessment_builder_versions ab
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
LEFT JOIN risk.tbluser us1 ON us1.USR_ID = ab.user_id
LEFT JOIN risk.tbluser us2 ON us2.USR_ID = ab.updated_user_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
WHERE ab.ass_template_id = @ass_template_id AND ab.status = 'Active'", con))
                {
                    cmd.Parameters.AddWithValue("@ass_template_id", ass_template_id);

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            for (var i = 0; i < dt.Rows.Count; i++)
                            {
                                pdata.Add(new AssessmentTemplateModelNew
                                {
                                    Type_Name = dt.Rows[i]["Type_Name"] != DBNull.Value ? dt.Rows[i]["Type_Name"].ToString() : null,

                                    assessment_builder_id = dt.Rows[i]["assessment_builder_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]) : (int?)null,
                                    ass_template_id = dt.Rows[i]["ass_template_id"] != DBNull.Value ? (dt.Rows[i]["ass_template_id"]).ToString() : null,
                                    assessment_name = dt.Rows[i]["assessment_name"] != DBNull.Value ? dt.Rows[i]["assessment_name"].ToString() : null,
                                    assessment_description = dt.Rows[i]["assessment_description"] != DBNull.Value ? dt.Rows[i]["assessment_description"].ToString() : null,
                                    SubType_Name = dt.Rows[i]["SubType_Name"] != DBNull.Value ? dt.Rows[i]["SubType_Name"].ToString() : null,
                                    Competency_Name = dt.Rows[i]["Competency_Name"] != DBNull.Value ? dt.Rows[i]["Competency_Name"].ToString() : null,
                                    created_date = dt.Rows[i]["created_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["created_date"]) : (DateTime?)null,
                                    updated_date = dt.Rows[i]["updated_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["updated_date"]) : (DateTime?)null,
                                    keywords = dt.Rows[i]["keywords"] != DBNull.Value ? dt.Rows[i]["keywords"].ToString() : null,
                                    status = dt.Rows[i]["status"] != DBNull.Value ? dt.Rows[i]["status"].ToString() : null,
                                    show_explaination = dt.Rows[i]["show_explaination"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["show_explaination"]) : (int?)null,
                                    show_hint = dt.Rows[i]["show_hint"] != DBNull.Value ? dt.Rows[i]["show_hint"].ToString() : null,
                                    firstname = dt.Rows[i]["user_firstname"] != DBNull.Value ? dt.Rows[i]["user_firstname"].ToString() : null,
                                    UpdateUsername = dt.Rows[i]["updated_user_firstname"] != DBNull.Value ? dt.Rows[i]["updated_user_firstname"].ToString() : null,
                                    total_questions = dt.Rows[i]["total_questions"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["total_questions"]) : null,


                                });
                            }
                        }
                    }
                }
            }

            return pdata;
        }


        //        [Route("api/Assessment/GetActiveAssesByID/{ass_template_id}")]
        //        [HttpGet]
        //        public IEnumerable<AssessmentTemplateModel> GetActiveAssessmentByID(string ass_template_id)
        //        {
        //            var pdata = new List<AssessmentTemplateModel>();

        //            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
        //            {
        //                con.Open();

        //                using (MySqlCommand cmd = new MySqlCommand(@"
        //                SELECT ab.ass_template_id, ab.assessment_name, ab.created_date, ab.updated_date, 
        //       us1.firstname AS user_firstname, us2.firstname AS updated_user_firstname,
        //       ab.assessment_description, ab.status, ab.show_hint, 
        //       ab.keywords, ab.assessment_builder_id, ab.show_explaination,
        //       tn.Type_Name, sn.SubType_Name, cn.Competency_Name
        //FROM risk.assessment_builder ab
        //JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
        //JOIN risk.type tn ON tn.Type_id = ab.Type_id
        //LEFT JOIN risk.tbluser us1 ON us1.USR_ID = ab.user_id
        //LEFT JOIN risk.tbluser us2 ON us2.USR_ID = ab.updated_user_id
        //JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
        //WHERE ab.ass_template_id = @ass_template_id AND ab.status = 'Active'", con)) 
        //                {
        //                    cmd.Parameters.AddWithValue("@ass_template_id", ass_template_id);

        //                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        //                    {
        //                        DataTable dt = new DataTable();
        //                        da.Fill(dt);

        //                        if (dt.Rows.Count > 0)
        //                        {
        //                            for (var i = 0; i < dt.Rows.Count; i++)
        //                            {
        //                                pdata.Add(new AssessmentTemplateModel
        //                                {
        //                                    Type_Name = dt.Rows[i]["Type_Name"] != DBNull.Value ? dt.Rows[i]["Type_Name"].ToString() : null,

        //                                    assessment_builder_id = dt.Rows[i]["assessment_builder_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]) : (int?)null,
        //                                    ass_template_id = dt.Rows[i]["ass_template_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["ass_template_id"]) : (int?)null,
        //                                    assessment_name = dt.Rows[i]["assessment_name"] != DBNull.Value ? dt.Rows[i]["assessment_name"].ToString() : null,
        //                                    assessment_description = dt.Rows[i]["assessment_description"] != DBNull.Value ? dt.Rows[i]["assessment_description"].ToString() : null,
        //                                    SubType_Name = dt.Rows[i]["SubType_Name"] != DBNull.Value ? dt.Rows[i]["SubType_Name"].ToString() : null,
        //                                    Competency_Name = dt.Rows[i]["Competency_Name"] != DBNull.Value ? dt.Rows[i]["Competency_Name"].ToString() : null,
        //                                    created_date = dt.Rows[i]["created_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["created_date"]) : (DateTime?)null,
        //                                    updated_date = dt.Rows[i]["updated_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["updated_date"]) : (DateTime?)null,
        //                                    keywords = dt.Rows[i]["keywords"] != DBNull.Value ? dt.Rows[i]["keywords"].ToString() : null,
        //                                    status = dt.Rows[i]["status"] != DBNull.Value ? dt.Rows[i]["status"].ToString() : null,
        //                                    show_explaination = dt.Rows[i]["show_explaination"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["show_explaination"]) : (int?)null,
        //                                    show_hint = dt.Rows[i]["show_hint"] != DBNull.Value ? dt.Rows[i]["show_hint"].ToString() : null,
        //                                    firstname = dt.Rows[i]["user_firstname"] != DBNull.Value ? dt.Rows[i]["user_firstname"].ToString() : null,
        //                                    UpdateUsername = dt.Rows[i]["updated_user_firstname"] != DBNull.Value ? dt.Rows[i]["updated_user_firstname"].ToString() : null,


        //                                }); 
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            return pdata;
        //        }


        [Route("api/Assessment/DisableAssessmentDetailsByID")]
        [HttpPut]
        public void DisableAssessmentDetails(string ass_template_id, string reason_for_disable,int disableby)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            // Step 1: Update only the latest version in assessment_builder_versions
            string updateLatestVersionQuery = @"
        UPDATE assessment_builder_versions abv
        JOIN (
            SELECT MAX(verson_no) AS max_version
            FROM assessment_builder_versions
            WHERE ass_template_id = @TemplateId
        ) AS latest
        ON abv.verson_no = latest.max_version AND abv.ass_template_id = @TemplateId
        SET abv.status = 'Delete',
            abv.reason_for_disable = @Reason,
            abv.disabled_date = @DisabledDate,
            abv.disableby = @disableby;";

            using (MySqlCommand command1 = new MySqlCommand(updateLatestVersionQuery, con))
            {
                command1.Parameters.AddWithValue("@TemplateId", ass_template_id);
                command1.Parameters.AddWithValue("@Reason", reason_for_disable);
                command1.Parameters.AddWithValue("@DisabledDate", DateTime.Now);
                command1.Parameters.AddWithValue("@disableby", disableby);
                command1.ExecuteNonQuery();
            }

            // Step 2: Update all records in assessment_builder
            string updateBuilderQuery = @"
        UPDATE assessment_builder 
        SET status = 'Delete', 
            reason_for_disable = @Reason, 
            disabled_date = @DisabledDate ,
            disableby = @disableby
        WHERE ass_template_id = @TemplateId;";

            using (MySqlCommand command2 = new MySqlCommand(updateBuilderQuery, con))
            {
                command2.Parameters.AddWithValue("@TemplateId", ass_template_id);
                command2.Parameters.AddWithValue("@Reason", reason_for_disable);
                command2.Parameters.AddWithValue("@DisabledDate", DateTime.Now);
                command2.Parameters.AddWithValue("@disableby", disableby);
                command2.ExecuteNonQuery();
            }

            con.Close();
        }



        [Route("api/Assessment/GetInActiveAssesmentByAll/{Useridvalue}")]
        [HttpGet]

        public IEnumerable<AssessmentTemplateModelNew> GetInActiveAssesmentByAll(int Useridvalue)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"

 SELECT  distinct(ab.ass_template_id), ab.assessment_name, ab.assessment_description,ab.created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name, sn.SubType_Name, cn.Competency_Name,um.firstname,ab.total_estimated_time,ab.total_questions,um.USR_ID
FROM risk.assessment_builder_versions ab
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.tbluser um on um.USR_ID = ab.user_id
JOIN risk.assement_provideacess aPa on aPa.AssessementTemplateID=ab.ass_template_id
WHERE ab.Status = 'Delete' and aPa.UserID=@userid and (aPa.Access_Permissions='2' or aPa.Access_Permissions='1') and aPa.Status='Active'", con);




            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@userid", Useridvalue);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AssessmentTemplateModelNew>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssessmentTemplateModelNew
                    {

                        //Assessment_generationID = Convert.ToInt32(dt.Rows[i]["Assessment_generationID"].ToString()),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[0]["assessment_description"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"].ToString()),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"].ToString()),
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),

                        status = dt.Rows[i]["status"].ToString(),




                    });
                }

            }
            return pdata;
        }


        [Route("api/Assessment/ReActivateAssessmentDetailsByID")]
        [HttpPut]
        public void ReActivateAssessmentDetailsByID(string ass_template_id)
        {
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                // Step 1: Reactivate assessment_builder if currently deleted
                string updateBuilderQuery = @"
            UPDATE assessment_builder 
            SET status = 'Active' 
            WHERE ass_template_id = @ass_template_id AND status = 'Delete';";

                using (MySqlCommand cmd1 = new MySqlCommand(updateBuilderQuery, con))
                {
                    cmd1.Parameters.AddWithValue("@ass_template_id", ass_template_id);
                    cmd1.ExecuteNonQuery();
                }

                // Step 2: Reactivate ONLY the latest version in assessment_builder_versions with status = 'Delete'
                string updateLatestVersionQuery = @"
            UPDATE assessment_builder_versions abv
            INNER JOIN (
                SELECT MAX(verson_no) AS latest_version
                FROM assessment_builder_versions
                WHERE ass_template_id = @ass_template_id AND status = 'Delete'
            ) AS latest
            ON abv.verson_no = latest.latest_version AND abv.ass_template_id = @ass_template_id
            SET abv.status = 'Active',
                abv.disabled_date = NULL,
                abv.reason_for_disable = NULL;";

                using (MySqlCommand cmd2 = new MySqlCommand(updateLatestVersionQuery, con))
                {
                    cmd2.Parameters.AddWithValue("@ass_template_id", ass_template_id);
                    cmd2.ExecuteNonQuery();
                }

                con.Close();
            }
        }





        [Route("api/Assessment/GetActiveAssesmentforUpdate/{Useridvalue}")]
        [HttpGet]
        public IEnumerable<AssessmentTemplateModelNew> GetActiveAssesmentforUpdate(int Useridvalue)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT  ab.ass_template_id, ab.assessment_name, ab.assessment_description,ab.created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name, sn.SubType_Name, cn.Competency_Name,um.firstname,ab.total_estimated_time,ab.total_questions,um.USR_ID
FROM risk.assessment_builder_versions ab
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.tbluser um on um.USR_ID = ab.user_id
JOIN risk.assement_provideacess aPa on aPa.AssessementTemplateID=ab.ass_template_id
WHERE ab.Status = 'Active' and aPa.UserID=@userid and aPa.Access_Permissions='2' and aPa.Status='Active'", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("userid", Useridvalue);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssessmentTemplateModelNew>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new AssessmentTemplateModelNew
                    {
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"]),
                        total_estimated_time = Convert.ToInt32(dt.Rows[i]["total_estimated_time"]),
                    });
                }
            }
            return pdata;
        }


        [Route("api/Assessment/GetActiveAssesmentfortpa/{UserID}")]
        [HttpGet]
        public IEnumerable<AssessmentTemplateModelNew> GetActiveAssesmentfortpa(int userid)
        {
            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();

            if (locations == null || locations.Count == 0)
                return new List<AssessmentTemplateModelNew>(); // no locations, return empty

            var pdata = new List<AssessmentTemplateModelNew>();
            using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                // Build IN clause dynamically
                var locationParams = string.Join(",", locations.Select((loc, i) => $"@loc{i}"));
                var query = $@"
SELECT  DISTINCT ab.ass_template_id, ab.assessment_name, ab.assessment_description, ab.created_date, ab.status, ab.keywords, ab.assessment_builder_id,
       tn.Type_Name, sn.SubType_Name, cn.Competency_Name, um.firstname, ab.total_estimated_time, ab.total_questions, um.USR_ID
FROM risk.assessment_builder_versions ab
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.tbluser um on um.USR_ID = ab.user_id
JOIN risk.assement_provideacess aPa on aPa.AssessementTemplateID = ab.ass_template_id
WHERE ab.Status = 'Active'
  AND aPa.UnitLocationMasterID IN ({locationParams})
  AND (aPa.Access_Permissions = '2' OR aPa.Access_Permissions = '1')
  AND aPa.Status = 'Active';";

                using (var cmd = new MySqlCommand(query, con))
                {
                    for (int i = 0; i < locations.Count; i++)
                        cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);

                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            pdata.Add(new AssessmentTemplateModelNew
                            {
                                Type_Name = row["Type_Name"].ToString(),
                                assessment_builder_id = Convert.ToInt32(row["assessment_builder_id"]),
                                ass_template_id = row["ass_template_id"].ToString(),
                                assessment_name = row["assessment_name"].ToString(),
                                assessment_description = row["assessment_description"].ToString(),
                                SubType_Name = row["SubType_Name"].ToString(),
                                Competency_Name = row["Competency_Name"].ToString(),
                                created_date = Convert.ToDateTime(row["created_date"]),
                                keywords = row["keywords"].ToString(),
                                status = row["status"].ToString(),
                                firstname = row["firstname"].ToString(),
                                total_questions = Convert.ToInt32(row["total_questions"]),
                                total_estimated_time = Convert.ToInt32(row["total_estimated_time"]),
                            });
                        }
                    }
                }
            }
            return pdata;
        }





    }
}
