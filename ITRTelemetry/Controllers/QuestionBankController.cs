using DomainModel;
using ExcelDataReader;
using ITR_TelementaryAPI.Models;
using ITRTelemetry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
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
    public class QuestionBankController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public QuestionBankController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }




        private readonly string excelFilePath = @"NewFolder\BulkUploadQuestions.xlsx";



        [Route("api/QuestionBank/AddQuestions")]
        [HttpPost]
        public IActionResult AddQuestion([FromBody] QuestionBankModel QuestionBankModels)
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
                    foreach (Options option in QuestionBankModels.options)
                    {
                        // Access and work with each option
                        Console.WriteLine($"Index: {option.index}, Value: {option.value}");
                        string insertQuery1 = "insert into questionbank_options(question_id,OptionId,options,created_date,status)values(@question_id,@OptionId,@options,@created_date,@status)";


                        using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery1, con))
                        {
                            myCommand2.Parameters.AddWithValue("@question_id", questionID);
                            myCommand2.Parameters.AddWithValue("@OptionId", option.index + 1);
                            myCommand2.Parameters.AddWithValue("@options", option.value);
                            myCommand2.Parameters.AddWithValue("@created_date", DateTime.Now);
                            myCommand2.Parameters.AddWithValue("@status", "Active");

                            myCommand2.ExecuteNonQuery();
                        }
                    }



                }


                return Ok("Question added successfully");
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





        [Route("api/QuestionBank/GetActiveQuestions")]
        [HttpGet]
        public IEnumerable<GetActiveQuestions> GetActiveQuestions()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
    SELECT 
        questionbank.question_id,
        questionbank.question,
        questionbank.response_type,
        questionbank.no_of_selectionchoices,
        questionbank.correct_answer,
        questionbank.question_hint,
        questionbank.questionmarked_favourite,
        questionbank.score_weightage,
        questionbank.check_level,
        questionbank.checklevel_weightage,
        questionbank.estimated_time,
        questionbank.keywords,
        questionbank.assessor_randomselection,
        questionbank.assessment_randomsetting,
        questionbank.subjectid,
        questionbank.topicid,
        questionbank.ref_to_governance_control,
        questionbank.question_disabled,
        questionbank.objective,
        questionbank.base64,
        questionbank.userid,
        questionbank.question_weightage,
        questionbank.created_date,
        questionbank.status,
        questionbank.reason_for_disable,
        Subject_Name,
        Topic_Name
    FROM
        questionbank
   JOIN
    topic ON topic.Topic_id = questionbank.topicid
JOIN
    subject ON subject.Subject_id = topic.Subject_id where questionbank.status='Active' ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetActiveQuestions>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetActiveQuestions
                    {
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        question = dt.Rows[i]["question"].ToString(),
                        response_type = Convert.ToInt32(dt.Rows[i]["response_type"].ToString()),
                        no_of_selectionchoices = Convert.ToInt32(dt.Rows[i]["no_of_selectionchoices"].ToString()),
                        correct_answer = Convert.ToInt32(dt.Rows[i]["correct_answer"].ToString()),
                        question_hint = dt.Rows[i]["question_hint"].ToString(),
                        questionmarked_favourite = dt.Rows[i]["questionmarked_favourite"].ToString(),
                        score_weightage = Convert.ToInt32(dt.Rows[i]["score_weightage"].ToString()),
                        check_level = Convert.ToInt32(dt.Rows[i]["check_level"].ToString()),
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
                        base64 = dt.Rows[i]["base64"].ToString(),
                        userid = Convert.ToInt32(dt.Rows[i]["userid"].ToString()),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"].ToString()),
                        status = dt.Rows[i]["question"].ToString(),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString()


                    });
                }
            }
            return pdata;
        }


        [Route("api/QuestionBank/GetActiveQuestionsByID")]
        [HttpGet]
        public IEnumerable<GetActiveQuestions> GetActiveQuestionsByID(int question_id)
        {
            var pdata = new List<GetActiveQuestions>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
    SELECT 
        questionbank.question_id,
        questionbank.question,
        questionbank.response_type,
        questionbank.no_of_selectionchoices,
        questionbank.correct_answer,
        questionbank.question_hint,
        questionbank.questionmarked_favourite,
        questionbank.score_weightage,
        questionbank.check_level,
        questionbank.checklevel_weightage,
        questionbank.estimated_time,
        questionbank.keywords,
        questionbank.assessor_randomselection,
        questionbank.assessment_randomsetting,
        questionbank.subjectid,
        questionbank.topicid,
        questionbank.ref_to_governance_control,
        questionbank.question_disabled,
        questionbank.objective,
        questionbank.base64,
        questionbank.userid,
        questionbank.question_weightage,
        questionbank.created_date,
        questionbank.status,
        questionbank.reason_for_disable,
        Subject_Name,
        Topic_Name
    FROM
        questionbank
   JOIN
    topic ON topic.Topic_id = questionbank.topicid
JOIN
    subject ON subject.Subject_id = topic.Subject_id where questionbank.question_id='" + question_id + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);



            MySqlCommand cmd1 = new MySqlCommand("SELECT questionbank_optionID, options FROM questionbank_options where question_id='" + question_id + "' and status='Active'", con);

            cmd1.CommandType = CommandType.Text;

            MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

            DataTable dt1 = new DataTable();
            da1.Fill(dt1);

            con.Close();

            List<Options> options = new List<Options>();

            if (dt1.Rows.Count > 0)
            {

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    Options opt = new Options();
                    opt.index = Convert.ToInt32(dt1.Rows[i]["questionbank_optionID"].ToString());
                    opt.value = dt1.Rows[i]["options"].ToString();

                    options.Add(opt);
                }
            }
            if (dt.Rows.Count > 0)
            {
                pdata.Add(new GetActiveQuestions
                {
                    question_id = Convert.ToInt32(dt.Rows[0]["question_id"].ToString()),
                    question = dt.Rows[0]["question"].ToString(),
                    response_type = Convert.ToInt32(dt.Rows[0]["response_type"].ToString()),
                    no_of_selectionchoices = Convert.ToInt32(dt.Rows[0]["no_of_selectionchoices"].ToString()),
                    correct_answer = Convert.ToInt32(dt.Rows[0]["correct_answer"].ToString()),
                    question_hint = dt.Rows[0]["question_hint"].ToString(),
                    questionmarked_favourite = dt.Rows[0]["questionmarked_favourite"].ToString(),
                    score_weightage = Convert.ToInt32(dt.Rows[0]["score_weightage"].ToString()),
                    check_level = Convert.ToInt32(dt.Rows[0]["check_level"].ToString()),
                    checklevel_weightage = Convert.ToSingle(dt.Rows[0]["checklevel_weightage"].ToString()),
                    estimated_time = Convert.ToInt32(dt.Rows[0]["estimated_time"].ToString()),
                    keywords = dt.Rows[0]["keywords"].ToString(),
                    assessor_randomselection = dt.Rows[0]["assessor_randomselection"].ToString(),
                    assessment_randomsetting = dt.Rows[0]["assessment_randomsetting"].ToString(),
                    subjectid = Convert.ToInt32(dt.Rows[0]["subjectid"].ToString()),
                    topicid = Convert.ToInt32(dt.Rows[0]["topicid"].ToString()),
                    ref_to_governance_control = dt.Rows[0]["ref_to_governance_control"].ToString(),
                    question_disabled = dt.Rows[0]["question_disabled"].ToString(),
                    objective = dt.Rows[0]["objective"].ToString(),
                    base64 = dt.Rows[0]["base64"].ToString(),
                    userid = Convert.ToInt32(dt.Rows[0]["userid"].ToString()),
                    created_date = Convert.ToDateTime(dt.Rows[0]["created_date"].ToString()),
                    status = dt.Rows[0]["question"].ToString(),
                    Subject_Name = dt.Rows[0]["Subject_Name"].ToString(),
                    Topic_Name = dt.Rows[0]["Topic_Name"].ToString(),
                    options = options
                });



            }
            return pdata;
        }

        [Route("api/QuestionBank/DisableQuestionByID")]
        [HttpPut]
        public void DisableQuestionByID(int question_id, string reason_for_disable)
        {


            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                string updateQuery = @"
            UPDATE questionbank 
            SET 
                status = 'InActive', 
                reason_for_disable = @reason_for_disable, 
                disable_date = @disable_date 
            WHERE 
                question_id = @question_id";

                using (MySqlCommand command = new MySqlCommand(updateQuery, con))
                {
                    command.Parameters.AddWithValue("@reason_for_disable", reason_for_disable);
                    command.Parameters.AddWithValue("@disable_date", DateTime.Now); // Current DateTime
                    command.Parameters.AddWithValue("@question_id", question_id);

                    command.ExecuteNonQuery();
                }
            }

        }
        [Route("api/QuestionBank/GetInActiveQuestionsAll/{userId}")]
        [HttpGet]
        public IEnumerable<GetActiveQuestions> GetInActiveQuestions(int userId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
    SELECT 
        questionbank.question_id,
        questionbank.question,
        questionbank.response_type,
        questionbank.no_of_selectionchoices,
        questionbank.correct_answer,
        questionbank.question_hint,
        questionbank.questionmarked_favourite,
        questionbank.score_weightage,
        questionbank.check_level,
        questionbank.checklevel_weightage,
        questionbank.estimated_time,
        questionbank.keywords,
        questionbank.assessor_randomselection,
        questionbank.assessment_randomsetting,
        questionbank.subjectid,
        questionbank.topicid,
        questionbank.ref_to_governance_control,
        questionbank.question_disabled,
        questionbank.objective,
        questionbank.base64,
        questionbank.userid,
        questionbank.question_weightage,
        questionbank.created_date,
        questionbank.status,
        questionbank.reason_for_disable,
        Subject_Name,
        Topic_Name
    FROM
        questionbank
   JOIN
    topic ON topic.Topic_id = questionbank.topicid
JOIN
    subject ON subject.Subject_id = topic.Subject_id where questionbank.status='InActive' AND questionbank.userid='"+ userId + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetActiveQuestions>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetActiveQuestions
                    {
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        question = dt.Rows[i]["question"].ToString(),
                        response_type = Convert.ToInt32(dt.Rows[i]["response_type"].ToString()),
                        no_of_selectionchoices = Convert.ToInt32(dt.Rows[i]["no_of_selectionchoices"].ToString()),
                        correct_answer = Convert.ToInt32(dt.Rows[i]["correct_answer"].ToString()),
                        question_hint = dt.Rows[i]["question_hint"].ToString(),
                        questionmarked_favourite = dt.Rows[i]["questionmarked_favourite"].ToString(),
                        score_weightage = Convert.ToInt32(dt.Rows[i]["score_weightage"].ToString()),
                        check_level = Convert.ToInt32(dt.Rows[i]["check_level"].ToString()),
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
                        base64 = dt.Rows[i]["base64"].ToString(),
                        userid = Convert.ToInt32(dt.Rows[i]["userid"].ToString()),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"].ToString()),
                        status = dt.Rows[i]["question"].ToString(),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString()


                    });
                }
            }
            return pdata;
        }
        [Route("api/QuestionBank/ReActivateQuestionByID")]
        [HttpPut]
        public void ReActivateQuestionByID(int question_id)
        {


            MySqlConnection con1 = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con1.Open();
            string updateQuery1 = "update questionbank set status='Active' where question_id='" + question_id + "'";
            MySqlCommand command1 = new MySqlCommand(updateQuery1, con1);
            command1.ExecuteNonQuery();
            con1.Close();


        }


        //Bulk Upload questions
        [Route("api/QuestionBank/UpdateBulkQuestions")]
        [HttpPost]
        public IActionResult InsertBulkQuestions([FromForm] BulkUploadQues BulkUploadQuess)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var dataSet = new DataSet();
            try
            {
                con.Open();
                if (BulkUploadQuess != null)
                {
                    // Access other form fields
                    var subjectId = BulkUploadQuess.subjectid;
                    var topicId = BulkUploadQuess.topicid;
                    var checkLevel = BulkUploadQuess.check_level;
                    var ChecklevelWeightage = BulkUploadQuess.checklevel_weightage;
                    var userid = BulkUploadQuess.userid;
                    // Access the file
                    IFormFile file = BulkUploadQuess.file;

                    // Process the file content as needed
                    // ...

                    using (var memoryStream = new MemoryStream())
                    {
                        file.CopyTo(memoryStream);
                        memoryStream.Position = 0; // Reset position to the beginning

                        // Use ExcelDataReader to read the Excel file content
                        IExcelDataReader excelReader;

                        if (Path.GetExtension(file.FileName).ToUpper() == ".XLS")
                        {
                            excelReader = ExcelReaderFactory.CreateBinaryReader(memoryStream);
                        }
                        else if (Path.GetExtension(file.FileName).ToUpper() == ".XLSX")
                        {
                            excelReader = ExcelReaderFactory.CreateOpenXmlReader(memoryStream);
                        }
                        else
                        {
                            // Handle unsupported file types
                            return BadRequest("Unsupported file format");
                        }

                        var conf = new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = _ => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                        };

                        dataSet = excelReader.AsDataSet(conf);

                        // validation for checking machine-generated report
                        if (dataSet.Tables.Count > 0)
                        {
                            DataTable dt_Excel = dataSet.Tables[0];
                            int column_count = dt_Excel.Columns.Count;
                            int row_count = dt_Excel.Rows.Count;

                            for (int i = 0; i < dt_Excel.Rows.Count; i++)
                            {

                                string insertQuery = "insert into questionbank(question,correct_answer,assessment_randomsetting,questionmarked_favourite,check_level,checklevel_weightage,estimated_time,subjectid,topicid,created_date,status,userid,assessor_randomselection,score_weightage)values(@question,@correct_answer,@assessment_randomsetting,@questionmarked_favourite,@check_level,@checklevel_weightage,@estimated_time,@subjectid,@topicid,@created_date,@status,@userid,@assessor_randomselection,@score_weightage)";



                                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                                {
                                    //float question_weightage = QuestionBankModels.score_weightage * QuestionBankModels.checklevel_weightage;
                                    string option1 = dt_Excel.Rows[i][1].ToString();
                                    string option2 = dt_Excel.Rows[i][2].ToString();
                                    string option3 = dt_Excel.Rows[i][3].ToString();
                                    string option4 = dt_Excel.Rows[i][4].ToString();
                                    string option5 = dt_Excel.Rows[i][5].ToString();
                                    string option6 = dt_Excel.Rows[i][6].ToString();
                                    string option7 = dt_Excel.Rows[i][7].ToString();
                                    string option8 = dt_Excel.Rows[i][8].ToString();
                                    string option9 = dt_Excel.Rows[i][9].ToString();
                                    string option10 = dt_Excel.Rows[i][10].ToString();

                                    myCommand1.Parameters.AddWithValue("@subjectid", subjectId);
                                    myCommand1.Parameters.AddWithValue("@topicid", topicId);
                                    myCommand1.Parameters.AddWithValue("@check_level", checkLevel);
                                    myCommand1.Parameters.AddWithValue("@checklevel_weightage", ChecklevelWeightage);
                                    myCommand1.Parameters.AddWithValue("@question", dt_Excel.Rows[i][0].ToString());
                                    myCommand1.Parameters.AddWithValue("@correct_answer", dt_Excel.Rows[i][11].ToString());
                                    myCommand1.Parameters.AddWithValue("@assessment_randomsetting", dt_Excel.Rows[i][16].ToString());
                                    myCommand1.Parameters.AddWithValue("@assessor_randomselection", dt_Excel.Rows[i][15].ToString());
                                    myCommand1.Parameters.AddWithValue("@estimated_time", dt_Excel.Rows[i][12].ToString());
                                    myCommand1.Parameters.AddWithValue("@questionmarked_favourite", dt_Excel.Rows[i][13].ToString());
                                    myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                                    myCommand1.Parameters.AddWithValue("@score_weightage", dt_Excel.Rows[i][14].ToString());
                                    myCommand1.Parameters.AddWithValue("@status", "Active");
                                    myCommand1.Parameters.AddWithValue("@userid", userid);

                                    myCommand1.ExecuteNonQuery();

                                    // Get the last inserted primary key value
                                    int questionID = Convert.ToInt32(myCommand1.LastInsertedId.ToString());
                                    for (int j = 0; j < 10; j++)
                                    {
                                        // Access and work with each option

                                        string insertQuery1 = "insert into questionbank_options(question_id,options,created_date,status)values(@question_id,@options,@created_date,@status)";


                                        using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery1, con))
                                        {
                                            myCommand2.Parameters.AddWithValue("@question_id", questionID);
                                            if (j == 0)
                                            {
                                                if (option1 != "") myCommand2.Parameters.AddWithValue("@options", option1);
                                                else break;
                                            }
                                            else if (j == 1)
                                            {
                                                if (option2 != "") myCommand2.Parameters.AddWithValue("@options", option2);
                                                else break;
                                            }
                                            else if (j == 2)
                                            {
                                                if (option3 != "") myCommand2.Parameters.AddWithValue("@options", option3);
                                                else break;
                                            }
                                            else if (j == 3)
                                            {
                                                if (option4 != "") myCommand2.Parameters.AddWithValue("@options", option4);
                                                else break;
                                            }
                                            else if (j == 4)
                                            {
                                                if (option5 != "") myCommand2.Parameters.AddWithValue("@options", option5);
                                                else break;
                                            }
                                            else if (j == 5)
                                            {
                                                if (option6 != "") myCommand2.Parameters.AddWithValue("@options", option6);
                                                else break;
                                            }
                                            else if (j == 6)
                                            {
                                                if (option7 != "") myCommand2.Parameters.AddWithValue("@options", option7);
                                                else break;
                                            }
                                            else if (j == 7)
                                            {
                                                if (option8 != "") myCommand2.Parameters.AddWithValue("@options", option8);
                                                else break;
                                            }
                                            else if (j == 8)
                                            {
                                                if (option9 != "") myCommand2.Parameters.AddWithValue("@options", option9);
                                                else break;
                                            }
                                            else
                                            {
                                                if (option10 != "") myCommand2.Parameters.AddWithValue("@options", option10);
                                                else break;
                                            }

                                            myCommand2.Parameters.AddWithValue("@created_date", DateTime.Now);
                                            myCommand2.Parameters.AddWithValue("@status", "Active");

                                            myCommand2.ExecuteNonQuery();
                                        }
                                    }


                                }

                            }

                            excelReader.Close();
                        }

                        // Rest of your code

                        return Ok("Data saved successfully");
                    }

                    // ...

                }
                else
                {
                    return BadRequest("Invalid data received");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving data");
            }
            finally
            {
                con.Close();
            }
            // Rest of your code
        }













        //Download Formate Method

        [Route("api/QuestionBank/DownloadExcelFile")]
        [HttpGet]

        public IActionResult DownloadExcelFile()
        {
            try
            {
                // Read the Excel file into a byte array
                byte[] fileBytes = System.IO.File.ReadAllBytes(excelFilePath);

                // Create a MemoryStream from the byte array
                MemoryStream memoryStream = new MemoryStream(fileBytes);

                // Return the file as a FileStreamResult
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "exceldownloadformat.xlsx");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log, return error response, etc.)
                return StatusCode(500, "Internal server error");
            }
        }











        [Route("api/QuestionBank/UpdateQuestions")]
        [HttpPost]
        public IActionResult UpdateQuestions([FromBody] UpdateQuestionBankModel QuestionBankModels)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string updateQuery = "UPDATE questionbank SET " +
            "question = @question, " +
            "response_type = @response_type, " +
            "no_of_selectionchoices = @no_of_selectionchoices, " +
            "correct_answer = @correct_answer, " +
            "question_hint = @question_hint, " +
            "questionmarked_favourite = @questionmarked_favourite, " +
            "score_weightage = @score_weightage, " +
            "check_level = @check_level, " +
            "checklevel_weightage = @checklevel_weightage, " +
            "estimated_time = @estimated_time, " +
            "keywords = @keywords, " +
            "assessor_randomselection = @assessor_randomselection, " +
            "assessment_randomsetting = @assessment_randomsetting, " +
            "subjectid = @subjectid, " +
            "topicid = @topicid, " +
            "ref_to_governance_control = @ref_to_governance_control, " +
            "question_disabled = @question_disabled, " +
            "objective = @objective, " +
            "base64 = @base64, " +
            "userid = @userid, " +
            "question_weightage = @question_weightage, " +
            "lastupdted_date = @lastupdted_date, " +
            "status = @status " +
            "where question_id=@question_id";
            try
            {
                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(updateQuery, con))
                {
                    int questionID = QuestionBankModels.question_id;

                    float question_weightage = QuestionBankModels.score_weightage * QuestionBankModels.checklevel_weightage;
                    myCommand.Parameters.AddWithValue("@question_id", questionID);
                    myCommand.Parameters.AddWithValue("@question", QuestionBankModels.question);
                    myCommand.Parameters.AddWithValue("@response_type", QuestionBankModels.response_type);
                    myCommand.Parameters.AddWithValue("@no_of_selectionchoices", QuestionBankModels.no_of_selectionchoices);
                    myCommand.Parameters.AddWithValue("@correct_answer", QuestionBankModels.correct_answer);
                    myCommand.Parameters.AddWithValue("@question_hint", QuestionBankModels.question_hint);
                    myCommand.Parameters.AddWithValue("@questionmarked_favourite", QuestionBankModels.questionmarked_favourite);
                    myCommand.Parameters.AddWithValue("@score_weightage", QuestionBankModels.score_weightage);
                    myCommand.Parameters.AddWithValue("@check_level", QuestionBankModels.check_level);
                    myCommand.Parameters.AddWithValue("@checklevel_weightage", QuestionBankModels.checklevel_weightage);
                    myCommand.Parameters.AddWithValue("@estimated_time", QuestionBankModels.estimated_time);
                    myCommand.Parameters.AddWithValue("@keywords", QuestionBankModels.keywords);
                    myCommand.Parameters.AddWithValue("@assessor_randomselection", QuestionBankModels.assessor_randomselection);
                    myCommand.Parameters.AddWithValue("@assessment_randomsetting", QuestionBankModels.assessment_randomsetting);
                    myCommand.Parameters.AddWithValue("@subjectid", QuestionBankModels.subjectid);
                    myCommand.Parameters.AddWithValue("@topicid", QuestionBankModels.topicid);
                    myCommand.Parameters.AddWithValue("@ref_to_governance_control", QuestionBankModels.ref_to_governance_control);
                    myCommand.Parameters.AddWithValue("@question_disabled", QuestionBankModels.question_disabled);
                    myCommand.Parameters.AddWithValue("@objective", QuestionBankModels.objective);
                    myCommand.Parameters.AddWithValue("@base64", QuestionBankModels.base64);
                    myCommand.Parameters.AddWithValue("@question_weightage", question_weightage);
                    myCommand.Parameters.AddWithValue("@userid", QuestionBankModels.userid);
                    myCommand.Parameters.AddWithValue("@lastupdted_date", DateTime.Now);
                    myCommand.Parameters.AddWithValue("@status", "Active");

                    myCommand.ExecuteNonQuery();


                    int response_type = QuestionBankModels.response_type;
                    string updateOptionsQuery = "update questionbank_options set status=@status where question_id=@question_id";

                    using (MySqlCommand myCmd = new MySqlCommand(updateOptionsQuery, con))
                    {
                        myCmd.Parameters.AddWithValue("@question_id", questionID);
                        myCmd.Parameters.AddWithValue("@status", "InActive");
                        myCmd.ExecuteNonQuery();
                    }


                    if (response_type == 1)
                    {
                        foreach (UpdateOptions option in QuestionBankModels.options)
                        {
                            Console.WriteLine($"Index: {option.index}, Value: {option.value}");
                            if (option.id == 0)
                            {
                                string updateQuery1 = "insert into questionbank_options(question_id,options,created_date,status,OptionId)values(@question_id,@options,@created_date,@status,@OptionId)";


                                using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery1, con))
                                {
                                    myCommand1.Parameters.AddWithValue("@question_id", questionID);
                                    myCommand1.Parameters.AddWithValue("@options", option.value);
                                    myCommand1.Parameters.AddWithValue("@OptionId", option.index + 1);
                                    myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                                    myCommand1.Parameters.AddWithValue("@status", "Active");

                                    myCommand1.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                string updateQuery2 = "update questionbank_options set options=@options,status=@status,OptionId=@OptionId where questionbank_optionID=@questionbank_optionID";


                                using (MySqlCommand myCommand2 = new MySqlCommand(updateQuery2, con))
                                {
                                    myCommand2.Parameters.AddWithValue("@questionbank_optionID", option.id);
                                    myCommand2.Parameters.AddWithValue("@options", option.value);
                                    myCommand2.Parameters.AddWithValue("@status", "Active");
                                    myCommand2.Parameters.AddWithValue("@OptionId", option.index + 1);


                                    myCommand2.ExecuteNonQuery();
                                }
                            }

                        }

                    }
                    else
                    {
                        string updateQuery3 = "update questionbank_options set options=@options,status=@status,OptionId=@OptionId where question_id=@question_id";

                        foreach (UpdateOptions option in QuestionBankModels.options)
                        {
                            using (MySqlCommand myCommand3 = new MySqlCommand(updateQuery3, con))
                            {
                                myCommand3.Parameters.AddWithValue("@question_id", questionID);
                                myCommand3.Parameters.AddWithValue("@options", option.value);
                                myCommand3.Parameters.AddWithValue("@status", "Active");
                                myCommand3.Parameters.AddWithValue("@OptionId", option.index + 1);

                                myCommand3.ExecuteNonQuery();
                            }
                        }
                    }



                }


                return Ok("Question updated successfully");
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

        [Route("api/QuestionBank/GetActiveQuestionsbyUserid/{userid}")]
        [HttpGet]
        public IEnumerable<GetActiveQuestions> GetActiveQuestionsbyUserid(int userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
    SELECT 
        questionbank.question_id,
        questionbank.question,
        questionbank.response_type,
        questionbank.no_of_selectionchoices,
        questionbank.correct_answer,
        questionbank.question_hint,
        questionbank.questionmarked_favourite,
        questionbank.score_weightage,
        questionbank.check_level,
        questionbank.checklevel_weightage,
        questionbank.estimated_time,
        questionbank.keywords,
        questionbank.assessor_randomselection,
        questionbank.assessment_randomsetting,
        questionbank.subjectid,
        questionbank.topicid,
        questionbank.ref_to_governance_control,
        questionbank.question_disabled,
        questionbank.objective,
        questionbank.base64,
        questionbank.userid,
        questionbank.question_weightage,
        questionbank.created_date,
        questionbank.status,
        questionbank.reason_for_disable,
        Subject_Name,
        Topic_Name
    FROM
        questionbank
   JOIN
    topic ON topic.Topic_id = questionbank.topicid
JOIN
    subject ON subject.Subject_id = topic.Subject_id where questionbank.status='Active' And questionbank.userid ='" + userid + "' ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetActiveQuestions>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetActiveQuestions
                    {
                        question_id = dt.Rows[i]["question_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["question_id"]) : 0,
                        question = dt.Rows[i]["question"] != DBNull.Value ? dt.Rows[i]["question"].ToString() : string.Empty,
                        response_type = dt.Rows[i]["response_type"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["response_type"]) : 0,
                        no_of_selectionchoices = dt.Rows[i]["no_of_selectionchoices"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["no_of_selectionchoices"]) : 0,
                        correct_answer = dt.Rows[i]["correct_answer"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["correct_answer"]) : 0,
                        question_hint = dt.Rows[i]["question_hint"] != DBNull.Value ? dt.Rows[i]["question_hint"].ToString() : string.Empty,
                        questionmarked_favourite = dt.Rows[i]["questionmarked_favourite"] != DBNull.Value ? dt.Rows[i]["questionmarked_favourite"].ToString() : string.Empty,
                        score_weightage = dt.Rows[i]["score_weightage"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["score_weightage"]) : 0,
                        check_level = dt.Rows[i]["check_level"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["check_level"]) : 0,
                        checklevel_weightage = dt.Rows[i]["checklevel_weightage"] != DBNull.Value ? Convert.ToSingle(dt.Rows[i]["checklevel_weightage"]) : 0.0f,
                        estimated_time = dt.Rows[i]["estimated_time"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["estimated_time"]) : 0,
                        keywords = dt.Rows[i]["keywords"] != DBNull.Value ? dt.Rows[i]["keywords"].ToString() : string.Empty,
                        assessor_randomselection = dt.Rows[i]["assessor_randomselection"] != DBNull.Value ? dt.Rows[i]["assessor_randomselection"].ToString() : string.Empty,
                        assessment_randomsetting = dt.Rows[i]["assessment_randomsetting"] != DBNull.Value ? dt.Rows[i]["assessment_randomsetting"].ToString() : string.Empty,
                        subjectid = dt.Rows[i]["subjectid"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["subjectid"]) : 0,
                        topicid = dt.Rows[i]["topicid"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["topicid"]) : 0,
                        ref_to_governance_control = dt.Rows[i]["ref_to_governance_control"] != DBNull.Value ? dt.Rows[i]["ref_to_governance_control"].ToString() : string.Empty,
                        question_disabled = dt.Rows[i]["question_disabled"] != DBNull.Value ? dt.Rows[i]["question_disabled"].ToString() : string.Empty,
                        objective = dt.Rows[i]["objective"] != DBNull.Value ? dt.Rows[i]["objective"].ToString() : string.Empty,
                        base64 = dt.Rows[i]["base64"] != DBNull.Value ? dt.Rows[i]["base64"].ToString() : string.Empty,
                        userid = dt.Rows[i]["userid"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["userid"]) : 0,
                        created_date = dt.Rows[i]["created_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["created_date"]) : DateTime.MinValue,
                        status = dt.Rows[i]["status"] != DBNull.Value ? dt.Rows[i]["status"].ToString() : string.Empty,
                        Subject_Name = dt.Rows[i]["Subject_Name"] != DBNull.Value ? dt.Rows[i]["Subject_Name"].ToString() : string.Empty,
                        Topic_Name = dt.Rows[i]["Topic_Name"] != DBNull.Value ? dt.Rows[i]["Topic_Name"].ToString() : string.Empty,



                    });
                }
            }
            return pdata;
        }



        [Route("api/QuestionBank/GetActiveQuestionsbyentityid/{userid}")]
        [HttpGet]
        public IEnumerable<object> GetActiveQuestionsbyentityid(int userid)

        {

            var userLocations = mySqlDBContext.userlocationmappingModels
                .Where(ad => ad.user_location_mapping_status == "Active" && ad.USR_ID == userid)
                .Select(ad => new { ad.Entity_Master_id, ad.Unit_location_Master_id })
                .Distinct()
                .ToList();

            var entityMasterIds = userLocations.Select(ul => ul.Entity_Master_id).ToList();
            var unitLocationMasterIds = userLocations.Select(ul => ul.Unit_location_Master_id).ToList();

   
            var users = mySqlDBContext.userlocationmappingModels
                .Where(ul => entityMasterIds.Contains(ul.Entity_Master_id) && unitLocationMasterIds.Contains(ul.Unit_location_Master_id))
                .Select(ul => ul.USR_ID)
                .Distinct()
                .ToList();

            var allQuestions = new List<GetActiveQuestions>();

            foreach (var userId in users)
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(@"
                SELECT 
                    questionbank.question_id,
                    questionbank.question,
                    questionbank.response_type,
                    questionbank.no_of_selectionchoices,
                    questionbank.correct_answer,
                    questionbank.question_hint,
                    questionbank.questionmarked_favourite,
                    questionbank.score_weightage,
                    questionbank.check_level,
                    questionbank.checklevel_weightage,
                    questionbank.estimated_time,
                    questionbank.keywords,
                    questionbank.assessor_randomselection,
                    questionbank.assessment_randomsetting,
                    questionbank.subjectid,
                    questionbank.topicid,
                    questionbank.ref_to_governance_control,
                    questionbank.question_disabled,
                    questionbank.objective,
                    questionbank.base64,
                    questionbank.userid,
                    questionbank.question_weightage,
                    questionbank.created_date,
                    questionbank.status,
                    questionbank.reason_for_disable,
                    Subject_Name,
                    Topic_Name,
                    ccl.Skill_Level_Name as checklevel_name,
                    tb.firstname as authorName,
                    dm.Department_Master_name as Department,
                    locMas.Unit_location_Master_name as Location,
                    em.Entity_Master_Name as Entity,tpaem.tpaenityname
                FROM
                    questionbank
                JOIN
                    topic ON topic.Topic_id = questionbank.topicid
                JOIN
                    subject ON subject.Subject_id = topic.Subject_id
                JOIN
                    tbluser as tb on tb.USR_ID=questionbank.userid
                
                LEFT JOIN 
                    department_master as dm on dm.Department_Master_id=tb.Department_Master_id
                LEFT JOIN
                    unit_location_master as locMas on locMas.Unit_location_Master_id=tb.Unit_location_Master_id
                LEFT JOIN 
                    entity_master as em on em.Entity_Master_id=tb.Entity_Master_id
                LEFT JOIN 
                    tpaenity as tpaem on tpaem.tpaenityid=tb.tpaenityid
                JOIN
                    competency_check_level as ccl on ccl.check_level_id=questionbank.check_level
                    
                WHERE
                    questionbank.status = 'Active' AND questionbank.userid = @userid", con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userId);
                        cmd.CommandType = CommandType.Text;

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            for (var i = 0; i < dt.Rows.Count; i++)
                            {
                                allQuestions.Add(new GetActiveQuestions
                                {
                                    question_id = dt.Rows[i]["question_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["question_id"]) : 0,
                                    question = dt.Rows[i]["question"] != DBNull.Value ? dt.Rows[i]["question"].ToString() : string.Empty,
                                    response_type = dt.Rows[i]["response_type"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["response_type"]) : 0,
                                    no_of_selectionchoices = dt.Rows[i]["no_of_selectionchoices"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["no_of_selectionchoices"]) : 0,
                                    correct_answer = dt.Rows[i]["correct_answer"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["correct_answer"]) : 0,
                                    question_hint = dt.Rows[i]["question_hint"] != DBNull.Value ? dt.Rows[i]["question_hint"].ToString() : string.Empty,
                                    questionmarked_favourite = dt.Rows[i]["questionmarked_favourite"] != DBNull.Value ? dt.Rows[i]["questionmarked_favourite"].ToString() : string.Empty,
                                    score_weightage = dt.Rows[i]["score_weightage"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["score_weightage"]) : 0,
                                    check_level = dt.Rows[i]["check_level"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["check_level"]) : 0,
                                    checklevel_weightage = dt.Rows[i]["checklevel_weightage"] != DBNull.Value ? Convert.ToSingle(dt.Rows[i]["checklevel_weightage"]) : 0.0f,
                                    estimated_time = dt.Rows[i]["estimated_time"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["estimated_time"]) : 0,
                                    keywords = dt.Rows[i]["keywords"] != DBNull.Value ? dt.Rows[i]["keywords"].ToString() : string.Empty,
                                    assessor_randomselection = dt.Rows[i]["assessor_randomselection"] != DBNull.Value ? dt.Rows[i]["assessor_randomselection"].ToString() : string.Empty,
                                    assessment_randomsetting = dt.Rows[i]["assessment_randomsetting"] != DBNull.Value ? dt.Rows[i]["assessment_randomsetting"].ToString() : string.Empty,
                                    subjectid = dt.Rows[i]["subjectid"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["subjectid"]) : 0,
                                    topicid = dt.Rows[i]["topicid"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["topicid"]) : 0,
                                    ref_to_governance_control = dt.Rows[i]["ref_to_governance_control"] != DBNull.Value ? dt.Rows[i]["ref_to_governance_control"].ToString() : string.Empty,
                                    question_disabled = dt.Rows[i]["question_disabled"] != DBNull.Value ? dt.Rows[i]["question_disabled"].ToString() : string.Empty,
                                    objective = dt.Rows[i]["objective"] != DBNull.Value ? dt.Rows[i]["objective"].ToString() : string.Empty,
                                    base64 = dt.Rows[i]["base64"] != DBNull.Value ? dt.Rows[i]["base64"].ToString() : string.Empty,
                                    userid = dt.Rows[i]["userid"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["userid"]) : 0,
                                    created_date = dt.Rows[i]["created_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["created_date"]) : DateTime.MinValue,
                                    status = dt.Rows[i]["status"] != DBNull.Value ? dt.Rows[i]["status"].ToString() : string.Empty,
                                    Subject_Name = dt.Rows[i]["Subject_Name"] != DBNull.Value ? dt.Rows[i]["Subject_Name"].ToString() : string.Empty,
                                    Topic_Name = dt.Rows[i]["Topic_Name"] != DBNull.Value ? dt.Rows[i]["Topic_Name"].ToString() : string.Empty,
                                    AuthorName = dt.Rows[i]["authorName"] != DBNull.Value ? dt.Rows[i]["authorName"].ToString() : string.Empty,
                                    departmentName = dt.Rows[i]["Department"] != DBNull.Value ? dt.Rows[i]["Department"].ToString() : "N/A",
                                    location = dt.Rows[i]["Location"] != DBNull.Value ? dt.Rows[i]["Location"].ToString() : "N/A",
                                    entity = dt.Rows[i]["Entity"] != DBNull.Value ? dt.Rows[i]["Entity"].ToString() : "N/A",
                                    tpaentity = dt.Rows[i]["tpaenityname"] != DBNull.Value ? dt.Rows[i]["tpaenityname"].ToString() : "N/A",
                                    checklevel_name = dt.Rows[i]["checklevel_name"] != DBNull.Value ? dt.Rows[i]["checklevel_name"].ToString() : string.Empty,
                                    
                                });
                            }
                        }
                    }
                }
            }

            return allQuestions;
        }
    

    }
}
