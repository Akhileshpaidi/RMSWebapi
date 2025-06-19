using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AssessmentResultsController : ControllerBase

    {
        public string IsAnswerCorrectImageUrl { get; set; }
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public AssessmentResultsController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        private readonly IHttpContextAccessor _httpContextAccessor;

        [Route("api/AssessmentResults/GetAssessmentResultsDetails")]
        [HttpGet]

        public IEnumerable<AssessmentResultsModel> GetAssessmentResultsDetails(int AssessementTemplateID)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT DISTINCT uad.UserAss_Ans_DetailsID,qb.ref_to_governance_control, uad.question_id,qb.question,uao.options AS user_Selected_Ans,qbo.options AS correct_answer FROM user_ass_ans_details uad
INNER JOIN questionbank qb ON qb.question_id = uad.question_id 
left JOIN questionbank_options uao ON uao.question_id = qb.question_id AND uao.questionbank_optionID = uad.user_Selected_Ans
left JOIN questionbank_options qbo ON qbo.question_id = qb.question_id AND qbo.questionbank_optionID = qb.correct_answer
WHERE uad.Status = 'Active' AND uad.UserID = 23 And AssessementTemplateID=566", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

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
                        user_Selected_Ans = UserAnswer,
                        UserAss_Ans_DetailsID = Convert.ToInt32(dt.Rows[i]["UserAss_Ans_DetailsID"]),
                        correct_answer = CorrectAnswer,
                        IsAnswerCorrect = isCorrect
                    });
                }
            }
            return pdata;

        }

        [Route("api/AssessmentQuestions/GetQuestionMaster")]
        [HttpGet]

        public IEnumerable<AssessmentResults> GetQuestionMaster()

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT question_id,question from questionbank where Status='Active' ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AssessmentResults>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssessmentResults
                    {
                        question_id = Convert.ToInt32(dt.Rows[i]["question_id"].ToString()),
                        question = dt.Rows[i]["question"].ToString(),

                       

                    });
                }
            }
            return pdata;

        }
    }
}
