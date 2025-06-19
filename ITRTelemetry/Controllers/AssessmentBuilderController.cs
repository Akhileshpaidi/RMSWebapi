using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DomainModel;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Data;
using Ubiety.Dns.Core;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using iText.Commons.Utils;
using DocumentFormat.OpenXml.Bibliography;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AssessmentBuilderController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public AssessmentBuilderController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/AssessmentBuilder/PutQuestionsUsed")]
        [HttpPut]
        public IActionResult PutQuestionsUsed([FromBody] questionBankUpdate questionBankUpdates)
        {
            if (questionBankUpdates?.no_of_times_used == null || !questionBankUpdates.no_of_times_used.Any())
            {
                return BadRequest("No questions provided");
            }

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open(); // Make sure to open the connection

                foreach (var questionId in questionBankUpdates.no_of_times_used)
                {
                    string putQuery = "UPDATE questionbank SET no_of_times_used = CASE WHEN no_of_times_used IS NULL THEN 1 ELSE no_of_times_used + 1 END WHERE question_id = @Qid";

                    using (MySqlCommand cmd = new MySqlCommand(putQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Qid", questionId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return Ok("Updated Successfully");
        }



        [Route("api/AssessmentBuilder/GetCheckLevels")]
        [HttpGet]
        public IEnumerable<CheckLevelModel> GetCheckLevels()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var pdata = new List<CheckLevelModel>();
            try
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM competency_check_level where Check_Level_Status='Active' ORDER BY check_level_id ASC LIMIT 5", con);

                cmd.CommandType = CommandType.Text;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                con.Close();

                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        pdata.Add(new CheckLevelModel
                        {
                            check_level_id = Convert.ToInt32(dt.Rows[i]["check_level_id"].ToString()),
                            Position = dt.Rows[i]["Position"].ToString(),
                            Skill_Level_Name = dt.Rows[i]["Skill_Level_Name"].ToString(),
                            Check_Level_Weightage = dt.Rows[i]["Check_Level_Weightage"].ToString()


                        });
                    }
                }

            }
            catch (Exception ex)
            { }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }
            return pdata;

        }
        [Route("api/AssessmentBuilder/getCountValues/{userid}")]
        [HttpPost]

        public IActionResult GetCountValues([FromBody] List<int> topicIds, int userid)
        {
            try
            {
                // Assume you have a method to retrieve count values from the database based on topicIds
                var countValues = GetCountValuesFromDatabase(topicIds, userid);

                return Ok(countValues);
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                return StatusCode(500, "Internal Server Error");
            }
        }

        private Dictionary<string, int> GetCountValuesFromDatabase(List<int> topicIds, int userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var countValues = new Dictionary<string, int>();
            try
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM competency_check_level where Check_Level_Status='Active' ORDER BY check_level_id ASC LIMIT 5", con);

                cmd.CommandType = CommandType.Text;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);


                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        int check_level_id = Convert.ToInt32(dt.Rows[i]["check_level_id"].ToString());
                        int questionscount = 0;

                        foreach (int topicId in topicIds)
                        {


                            MySqlCommand cmd1 = new MySqlCommand("SELECT * FROM questionbank where status='Active' and check_level='" + check_level_id + "' and userid='" + userid + "' and topicid='" + topicId + "'", con);

                            cmd1.CommandType = CommandType.Text;

                            MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                            DataTable dt1 = new DataTable();
                            da1.Fill(dt1);
                            questionscount = questionscount + dt1.Rows.Count;

                        }
                        countValues[i.ToString()] = questionscount;
                    }
                }

            }
            catch (Exception ex)
            { }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }



            return countValues;
        }


        [Route("api/AssessmentBuilder/getTotalCountValues")]
        [HttpPost]

        public IActionResult getTotalCountValues([FromBody] FormRequest form)
        {
            if (form == null || form.SelectedTopics == null || form.UsersList == null)
            {
                return BadRequest("Invalid data received.");
            }
            else
            {
                var topicIds = form.SelectedTopics;
                var userIds = form.UsersList;
                try
                {
                    // Assume you have a method to retrieve count values from the database based on topicIds
                    var countValues = GetCountValuesforTotalDB(topicIds, userIds);

                    return Ok(countValues);
                }
                catch (Exception ex)
                {
                    // Log the error or handle it appropriately
                    return StatusCode(500, "Internal Server Error");
                }

            }






        }

        private Dictionary<string, int> GetCountValuesforTotalDB(List<int> topicIds, List<int> userIds)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var countValues = new Dictionary<string, int>();
            try
            {
                con.Open();

                // Retrieve the competency levels
                string competencyQuery = "SELECT * FROM competency_check_level WHERE Check_Level_Status='Active' ORDER BY check_level_id ASC LIMIT 5";
                MySqlCommand cmd = new MySqlCommand(competencyQuery, con);

                DataTable dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        int check_level_id = Convert.ToInt32(dt.Rows[i]["check_level_id"]);
                        int questionscount = 0;

                        foreach (int topicId in topicIds)
                        {
                            // Build the `IN` clause for user IDs dynamically
                            string userIdList = string.Join(",", userIds);

                            string questionQuery = $@"
                        SELECT COUNT(*) AS QuestionCount 
                        FROM questionbank 
                        WHERE status='Active' 
                          AND check_level={check_level_id} 
                          AND topicid={topicId} 
                          AND userid IN ({userIdList})";

                            MySqlCommand cmd1 = new MySqlCommand(questionQuery, con);

                            // Execute the query and fetch the question count
                            object result = cmd1.ExecuteScalar();
                            questionscount += result != null ? Convert.ToInt32(result) : 0;
                        }

                        countValues[i.ToString()] = questionscount;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (e.g., log the error)
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }

            return countValues;
        }







        [Route("api/AssessmentBuilder/AssessmentGeneration")]
        [HttpPost]
        public IActionResult AssessmentGeneration([FromBody] AssessmentGenerationNew AssessmentGenerationModels)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "insert into assessment_generation(QuestionMixtype,No_of_Questions,MostUsedQuestions,FavouritesDefaults,RecentlyAdded,TimeEstimate,TimeEstimateInputMin,TimeEstimateInputMax,UserID,CreatedDate,Status)values(@QuestionMixtype,@No_of_Questions,@MostUsedQuestions,@FavouritesDefaults,@RecentlyAdded,@TimeEstimate,@TimeEstimateInputMin,@TimeEstimateInputMax,@UserID,@CreatedDate,@Status)";
            string questionsFrom = AssessmentGenerationModels.QuestionsListFrom;
            try
            {
                con.Open();
                string UpdateQuery = "update assessment_generation set Status=@Status where UserID=@UserID ";

                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@Status", "Inactive");
                    myCommand.Parameters.AddWithValue("@UserID", AssessmentGenerationModels.UserID);

                    myCommand.ExecuteNonQuery();

                }
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {

                    myCommand1.Parameters.AddWithValue("@QuestionMixtype", AssessmentGenerationModels.QuestionMixtype);
                    myCommand1.Parameters.AddWithValue("@No_of_Questions", AssessmentGenerationModels.No_of_Questions);
                    myCommand1.Parameters.AddWithValue("@MostUsedQuestions", AssessmentGenerationModels.MostUsedQuestions);
                    myCommand1.Parameters.AddWithValue("@FavouritesDefaults", AssessmentGenerationModels.FavouritesDefaults);
                    myCommand1.Parameters.AddWithValue("@RecentlyAdded", AssessmentGenerationModels.RecentlyAdded);
                    myCommand1.Parameters.AddWithValue("@TimeEstimate", AssessmentGenerationModels.TimeEstimate);
                    myCommand1.Parameters.AddWithValue("@TimeEstimateInputMin", AssessmentGenerationModels.TimeEstimateInputMin);
                    myCommand1.Parameters.AddWithValue("@TimeEstimateInputMax", AssessmentGenerationModels.TimeEstimateInputMax);
                    myCommand1.Parameters.AddWithValue("@UserID", AssessmentGenerationModels.UserID);
                    myCommand1.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@Status", "Active");


                    myCommand1.ExecuteNonQuery();

                    // Get the last inserted primary key value
                    int Assessment_generationID = Convert.ToInt32(myCommand1.LastInsertedId.ToString());
                    string[] topics = AssessmentGenerationModels.Topics;
                    string allTopics = string.Join(",", topics);
                    foreach (string topicid in AssessmentGenerationModels.Topics)
                    {

                        string insertQuery2 = "insert into assessment_topics(Assessment_generationID,Topic_id)values(@Assessment_generationID,@Topic_id)";


                        using (MySqlCommand myCommand3 = new MySqlCommand(insertQuery2, con))
                        {
                            myCommand3.Parameters.AddWithValue("@Assessment_generationID", Assessment_generationID);
                            myCommand3.Parameters.AddWithValue("@Topic_id", topicid);
                            myCommand3.ExecuteNonQuery();
                        }
                    }


                    foreach (Competancychecks option in AssessmentGenerationModels.Competancychecks)
                    {
                        if (option.value != "" && option.value != "0")
                        {
                            string insertQuery1 = "insert into assessment_competancychecks(Assessment_generationID,check_level_id,check_level_percentage)values(@Assessment_generationID,@check_level_id,@check_level_percentage)";


                            using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery1, con))
                            {
                                myCommand2.Parameters.AddWithValue("@Assessment_generationID", Assessment_generationID);
                                myCommand2.Parameters.AddWithValue("@check_level_id", option.id);
                                myCommand2.Parameters.AddWithValue("@check_level_percentage", option.value);
                                myCommand2.ExecuteNonQuery();
                            }


                            // Generate Question Paper
                            if (questionsFrom == "My Questions")
                            {

                                using (MySqlCommand cmd1 = new MySqlCommand("SELECT question_id FROM questionbank WHERE status='Active' AND topicid IN (" + allTopics + ") AND userid=@userid AND check_level=@OptionId ORDER BY created_date DESC, RAND()  LIMIT @OptionValue", con))
                                {
                                    int competencyquestions = Convert.ToInt32(option.value);
                                    cmd1.CommandType = CommandType.Text;
                                    cmd1.Parameters.AddWithValue("@OptionId", option.id);
                                    cmd1.Parameters.AddWithValue("@OptionValue", competencyquestions);
                                    cmd1.Parameters.AddWithValue("@userid", AssessmentGenerationModels.UserID);
                                    using (MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1))
                                    {
                                        DataTable dt1 = new DataTable();
                                        da1.Fill(dt1);

                                        // Insert into Assessmentgeneration Details table
                                        for (int questions = 0; questions < dt1.Rows.Count; questions++)
                                        {
                                            int questionid = Convert.ToInt32(dt1.Rows[questions]["question_id"]);






                                            string insertquestions = "INSERT INTO assessment_generation_details(Assessment_generationID, question_id, Status, CreatedDate) VALUES (@Assessment_generationID, @question_id, @Status, @CreatedDate)";

                                            using (MySqlCommand myCommand2 = new MySqlCommand(insertquestions, con))
                                            {
                                                myCommand2.Parameters.AddWithValue("@Assessment_generationID", Assessment_generationID);
                                                myCommand2.Parameters.AddWithValue("@question_id", questionid);
                                                myCommand2.Parameters.AddWithValue("@Status", "Active");
                                                myCommand2.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                                myCommand2.ExecuteNonQuery();
                                            }





                                        }
                                    }
                                }






                            }

                            else
                            {
                                var userLocations = mySqlDBContext.userlocationmappingModels
                                    .Where(ad => ad.user_location_mapping_status == "Active" && ad.USR_ID == AssessmentGenerationModels.UserID)
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

                                if (users.Any())
                                {
                                    // Convert user IDs to a comma-separated string for the IN clause
                                    string userIdsString = string.Join(",", users);

                                    using (MySqlCommand cmd1 = new MySqlCommand(
                                        "SELECT question_id FROM questionbank WHERE status='Active' AND userid IN (" + userIdsString + ") AND topicid IN (" + allTopics + ") AND check_level=@OptionId ORDER BY created_date DESC, RAND() LIMIT @OptionValue", con))
                                    {
                                        int competencyquestions = Convert.ToInt32(option.value);
                                        cmd1.CommandType = CommandType.Text;
                                        cmd1.Parameters.AddWithValue("@OptionId", option.id);
                                        cmd1.Parameters.AddWithValue("@OptionValue", competencyquestions);

                                        using (MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1))
                                        {
                                            DataTable dt1 = new DataTable();
                                            da1.Fill(dt1);

                                            // Insert into Assessmentgeneration Details table
                                            for (int questions = 0; questions < dt1.Rows.Count; questions++)
                                            {
                                                int questionid = Convert.ToInt32(dt1.Rows[questions]["question_id"]);

                                                string insertquestions = "INSERT INTO assessment_generation_details(Assessment_generationID, question_id, Status, CreatedDate) VALUES (@Assessment_generationID, @question_id, @Status, @CreatedDate)";

                                                using (MySqlCommand myCommand2 = new MySqlCommand(insertquestions, con))
                                                {
                                                    myCommand2.Parameters.AddWithValue("@Assessment_generationID", Assessment_generationID);
                                                    myCommand2.Parameters.AddWithValue("@question_id", questionid);
                                                    myCommand2.Parameters.AddWithValue("@Status", "Active");
                                                    myCommand2.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                                    myCommand2.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }




                            }

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



        [Route("api/AssessmentBuilder/AssessmentGenValidationCheck")]
        [HttpPost]
        public IActionResult AssessmentGenValidationCheck([FromBody] AssessmentGeneration AssessmentGenerationModels)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                con.Open();
                string QuestionMixtype = AssessmentGenerationModels.QuestionMixtype;
                int No_of_Questions = AssessmentGenerationModels.No_of_Questions;
                string MostUsedQuestions = AssessmentGenerationModels.MostUsedQuestions;
                string FavouritesDefaults = AssessmentGenerationModels.FavouritesDefaults;
                string RecentlyAdded = AssessmentGenerationModels.RecentlyAdded;
                string TimeEstimate = AssessmentGenerationModels.TimeEstimate;
                int TimeEstimateInputMin = AssessmentGenerationModels.TimeEstimateInputMin;
                int TimeEstimateInputMax = AssessmentGenerationModels.TimeEstimateInputMax;

                // first validation - checking total question are there in question bank or not w.r.t selected topics
                int questionscount = 0;
                foreach (string topicid in AssessmentGenerationModels.Topics)
                {
                    int Topic_id = Convert.ToInt32(topicid);

                    MySqlCommand cmd = new MySqlCommand("select count(question_id) from questionbank where topicid='" + Topic_id + "' and  status='Active'", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        questionscount = questionscount + Convert.ToInt32(dt.Rows[0][0].ToString());
                    }

                }

                if (QuestionMixtype == "Competency Check Level")
                {

                    if (questionscount >= No_of_Questions)
                    {
                        // validation at competancy check level 
                        int sumofcompetancyquestions = 0;
                        int mixcatcount = 0;
                        foreach (Competancychecks option in AssessmentGenerationModels.Competancychecks)
                        {
                            int check_level_id = option.id;
                            string check_level_percentage = option.value;
                            int competancycheckquestions = 0;
                            mixcatcount++;
                            if (option.value != "" && option.value != "0")
                            {
                                int percentagequestions = Convert.ToInt32(option.value);
                                sumofcompetancyquestions = sumofcompetancyquestions + percentagequestions;
                                foreach (string topicid in AssessmentGenerationModels.Topics)
                                {
                                    int Topic_id = Convert.ToInt32(topicid);

                                    MySqlCommand cmd = new MySqlCommand("select count(question_id) from questionbank where topicid='" + Topic_id + "' and check_level='" + check_level_id + "' and  status='Active'", con);

                                    cmd.CommandType = CommandType.Text;

                                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                                    DataTable dt = new DataTable();
                                    da.Fill(dt);
                                    if (dt.Rows.Count > 0)
                                    {
                                        competancycheckquestions = competancycheckquestions + Convert.ToInt32(dt.Rows[0][0].ToString());
                                    }


                                }

                                if (competancycheckquestions < percentagequestions)
                                {
                                    // insufficient questions

                                    errorMessages.Append("Insufficient questions for Mix Cat " + mixcatcount).AppendLine();

                                }
                            }

                        }

                        if (No_of_Questions != sumofcompetancyquestions)
                        {
                            errorMessages.Append("Sum of Mix category questions should be equal to Total questions").AppendLine();
                        }
                    }

                    else
                    {
                        // insufficienct questions 
                        errorMessages.Append("Insufficient questions in the question bank for the selected topics.").AppendLine();


                    }


                }
                else
                {
                    // first validation - checking total question are there in question bank or not w.r.t selected topics
                    if (questionscount >= No_of_Questions)
                    {

                    }

                    else
                    {
                        // insufficienct questions 
                        errorMessages.Append("Insufficient questions in the question bank for the selected topics.").AppendLine();

                    }
                }


                if (errorMessages.Length > 0)
                {
                    return BadRequest("Validation errors:\n" + errorMessages.ToString());
                }

                return Ok("No Validation errors");
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



        //Method for Assessment temp Generation Random Selection







        [Route("api/AssessmentBuilder/RandomAssessmentGeneration")]
        [HttpPost]
        public IActionResult RandomAssessmentGeneration([FromBody] AssessmentGenerationRand AssessmentGenerationModels)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "insert into assessment_generation(QuestionMixtype,No_of_Questions,TimeEstimateInputMin,TimeEstimateInputMax,UserID,CreatedDate,Status)values(@QuestionMixtype,@No_of_Questions,@TimeEstimateInputMin,@TimeEstimateInputMax,@UserID,@CreatedDate,@Status)";

            try
            {
                con.Open();

                string UpdateQuery = "update assessment_generation set Status=@Status where UserID=@UserID ";

                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@Status", "Inactive");
                    myCommand.Parameters.AddWithValue("@UserID", AssessmentGenerationModels.UserID);

                    myCommand.ExecuteNonQuery();

                }
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {

                    myCommand1.Parameters.AddWithValue("@QuestionMixtype", AssessmentGenerationModels.QuestionMixtype);
                    myCommand1.Parameters.AddWithValue("@No_of_Questions", AssessmentGenerationModels.No_of_Questions);

                    myCommand1.Parameters.AddWithValue("@TimeEstimateInputMin", AssessmentGenerationModels.TimeEstimateInputMin);
                    myCommand1.Parameters.AddWithValue("@TimeEstimateInputMax", AssessmentGenerationModels.TimeEstimateInputMax);
                    myCommand1.Parameters.AddWithValue("@UserID", AssessmentGenerationModels.UserID);
                    myCommand1.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@Status", "Active");


                    myCommand1.ExecuteNonQuery();

                    // Get the last inserted primary key value
                    int Assessment_generationID = Convert.ToInt32(myCommand1.LastInsertedId.ToString());
                    string[] topics = AssessmentGenerationModels.Topics;
                    string alltopics = String.Join(",", topics.ToArray());


                    foreach (string topicid in AssessmentGenerationModels.Topics)
                    {

                        string insertQuery2 = "insert into assessment_topics(Assessment_generationID,Topic_id)values(@Assessment_generationID,@Topic_id)";


                        using (MySqlCommand myCommand3 = new MySqlCommand(insertQuery2, con))
                        {
                            myCommand3.Parameters.AddWithValue("@Assessment_generationID", Assessment_generationID);
                            myCommand3.Parameters.AddWithValue("@Topic_id", topicid);
                            myCommand3.ExecuteNonQuery();
                        }
                    }



                    string unionQuery = BuildUnionQuery(AssessmentGenerationModels.RandomserialNumbers, AssessmentGenerationModels.Topics, AssessmentGenerationModels.TimeEstimateInputMin, AssessmentGenerationModels.TimeEstimateInputMax);
                    int[] resultIds = ExecuteUnionQuery(con, unionQuery);
                    int aquestionslength = AssessmentGenerationModels.No_of_Questions;
                    for (int Questionindex = 0; Questionindex < aquestionslength; Questionindex++)
                    {
                        int QuestionId = resultIds[Questionindex];
                        string insertquestions = "INSERT INTO assessment_generation_details(Assessment_generationID, question_id, Status, CreatedDate) VALUES (@Assessment_generationID, @question_id, @Status, @CreatedDate)";

                        using (MySqlCommand myCommand6 = new MySqlCommand(insertquestions, con))
                        {
                            myCommand6.Parameters.AddWithValue("@Assessment_generationID", Assessment_generationID);
                            myCommand6.Parameters.AddWithValue("@question_id", QuestionId);
                            myCommand6.Parameters.AddWithValue("@Status", "Active");
                            myCommand6.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                            myCommand6.ExecuteNonQuery();
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






        private string BuildUnionQuery(int[] randomNumbers, string[] topics, int min_value, int max_value)
        {
            string alltopics = string.Join(",", topics);

            StringBuilder unionQuery = new StringBuilder();

            for (int i = 0; i < randomNumbers.Length; i++)
            {
                int randomNum = randomNumbers[i];

                switch (randomNum)
                {
                    case 1:
                        unionQuery.Append($"(SELECT question_id FROM questionbank WHERE status = 'Active' AND topicid IN ({alltopics}) ORDER BY no_of_times_used DESC) ");
                        break;
                    case 2:
                        unionQuery.Append($"(SELECT question_id FROM questionbank WHERE status = 'Active' AND topicid IN ({alltopics}) AND questionmarked_favourite = 'Yes') ");
                        break;
                    case 3:
                        unionQuery.Append($"(SELECT question_id FROM questionbank WHERE status = 'Active' AND topicid IN ({alltopics}) ORDER BY created_date DESC) ");
                        break;
                    case 4:
                        unionQuery.Append($"(SELECT question_id FROM questionbank WHERE status = 'Active' AND topicid IN ({alltopics}) AND estimated_time BETWEEN {min_value} AND {max_value}) ");
                        break;
                    default:
                        break;
                }

                if (i < randomNumbers.Length - 1)
                {
                    unionQuery.Append("UNION ");
                }
            }

            return unionQuery.ToString();
        }


        private int[] ExecuteUnionQuery(MySqlConnection connection, string unionQuery)
        {
            List<int> resultIds = new List<int>();

            try
            {

                using (MySqlCommand command = new MySqlCommand(unionQuery, connection))
                {




                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int questionId = reader.GetInt32(0);
                            resultIds.Add(questionId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or print the exception details for debugging
                Console.WriteLine($"Error in ExecuteUnionQuery: {ex.Message}");
                // Rethrow the exception to indicate the error
                throw;
            }

            return resultIds.ToArray();
        }














        //Method for Customized Assessment



        [Route("api/QuestionBank/CustomizedAssessment")]
        [HttpPost]
        public IActionResult CustomizedAssessment([FromBody] QuestionBankModel QuestionBankModels)
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

                    string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_generation WHERE UserID = @UserID AND Status = 'Active'";

                    using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                    {
                        myCommand4.Parameters.AddWithValue("@UserID", userId);

                        // Execute the query and retrieve the generationid
                        object generationIdObject = myCommand4.ExecuteScalar();

                        if (generationIdObject != null)
                        {
                            int AssessmentgenerationID = Convert.ToInt32(generationIdObject);



                            //Query is added for insert values in assessment details

                            AssessGenDetailsModel assessGenDetailsModel = new AssessGenDetailsModel
                            {
                                Assessment_generationID = AssessmentgenerationID,
                                question_id = questionID,
                                CreatedDate = DateTime.Now,
                                Status = "Active"
                            };

                            string insertAssessGenDetailsQuery = "INSERT INTO assessment_generation_details (Assessment_generationID,question_id, CreatedDate, Status) VALUES (@Assessment_generationID,@question_id, @CreatedDate, @Status)";

                            using (MySqlCommand myCommand3 = new MySqlCommand(insertAssessGenDetailsQuery, con))
                            {
                                // Replace the parameters with the properties of AssessGenDetailsModel
                                myCommand3.Parameters.AddWithValue("@Assessment_generationID", assessGenDetailsModel.Assessment_generationID);
                                myCommand3.Parameters.AddWithValue("@question_id", assessGenDetailsModel.question_id);
                                myCommand3.Parameters.AddWithValue("@CreatedDate", assessGenDetailsModel.CreatedDate);
                                myCommand3.Parameters.AddWithValue("@Status", assessGenDetailsModel.Status);

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


        //Get Method for search criteria in Customized Assessment



        [Route("api/AssessmentBuilder/GetQuestions/{UserId}")]
        [HttpGet]
        public IEnumerable<QuestionModel> GetQuestions(int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {

                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_generation WHERE UserID = @UserID AND Status = 'Active'";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@UserID", UserId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);

                        // Query to retrieve questions based on Assessment_generationID
                        string query = "SELECT qd.*, sb.Subject_Name, tb.Topic_Name, cl.Skill_Level_Name FROM risk.questionbank qd " +
 "JOIN risk.assessment_generation_details agd ON agd.question_id = qd.question_id " +
 "JOIN risk.subject sb ON sb.Subject_id = qd.subjectid " +
 "JOIN risk.topic tb ON tb.Topic_id = qd.topicid " +
 "JOIN risk.competency_check_level cl ON cl.check_level_id = qd.check_level " +
 "WHERE agd.Status = 'Active' AND agd.Assessment_generationID = @AssessmentgenerationID " +
 "ORDER BY cl.Skill_Level_Name";

                        using (MySqlCommand command = new MySqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@AssessmentgenerationID", AssessmentgenerationID);

                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);

                                // Convert DataTable to a list of QuestionBankModel
                                List<QuestionModel> questionList = new List<QuestionModel>();
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    // Assuming you have a method to convert a DataRow to QuestionBankModel
                                    QuestionModel question = ConvertDataRowToQuestionBankModel(row);
                                    questionList.Add(question);
                                }

                                return questionList;
                            }
                        }
                    }
                    else
                    {
                        // Handle the case when Assessment_generationID is not found
                        return new List<QuestionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModel>();
            }
            finally
            {
                con.Close();
            }
        }

        // Helper method to convert DataRow to QuestionBankModel
        private QuestionModel ConvertDataRowToQuestionBankModel(DataRow row)
        {
            QuestionModel questionModel = new QuestionModel();

            questionModel.question_id = row["question_id"] != DBNull.Value ? Convert.ToInt32(row["question_id"]) : (int?)null;
            questionModel.question = row["question"] != DBNull.Value ? Convert.ToString(row["question"]) : null;
            questionModel.response_type = row["response_type"] != DBNull.Value ? Convert.ToInt32(row["response_type"]) : (int?)null;
            questionModel.no_of_selectionchoices = row["no_of_selectionchoices"] != DBNull.Value ? Convert.ToInt32(row["no_of_selectionchoices"]) : (int?)null;
            questionModel.correct_answer = row["correct_answer"] != DBNull.Value ? Convert.ToInt32(row["correct_answer"]) : (int?)null;
            questionModel.question_hint = row["question_hint"] != DBNull.Value ? Convert.ToString(row["question_hint"]) : null;
            questionModel.questionmarked_favourite = row["questionmarked_favourite"] != DBNull.Value ? Convert.ToString(row["questionmarked_favourite"]) : null;
            questionModel.score_weightage = row["score_weightage"] != DBNull.Value ? Convert.ToInt32(row["score_weightage"]) : (int?)null;
            questionModel.check_level = row["check_level"] != DBNull.Value ? Convert.ToInt32(row["check_level"]) : (int?)null;
            questionModel.checklevel_weightage = row["checklevel_weightage"] != DBNull.Value ? Convert.ToSingle(row["checklevel_weightage"]) : (float?)null;
            questionModel.estimated_time = row["estimated_time"] != DBNull.Value ? Convert.ToInt32(row["estimated_time"]) : (int?)null;
            questionModel.keywords = row["keywords"] != DBNull.Value ? Convert.ToString(row["keywords"]) : null;
            questionModel.assessor_randomselection = row["assessor_randomselection"] != DBNull.Value ? Convert.ToString(row["assessor_randomselection"]) : null;
            questionModel.assessment_randomsetting = row["assessment_randomsetting"] != DBNull.Value ? Convert.ToString(row["assessment_randomsetting"]) : null;
            questionModel.subjectid = row["subjectid"] != DBNull.Value ? Convert.ToInt32(row["subjectid"]) : (int?)null;
            questionModel.topicid = row["topicid"] != DBNull.Value ? Convert.ToInt32(row["topicid"]) : (int?)null;
            questionModel.ref_to_governance_control = row["ref_to_governance_control"] != DBNull.Value ? Convert.ToString(row["ref_to_governance_control"]) : null;
            questionModel.question_disabled = row["question_disabled"] != DBNull.Value ? Convert.ToString(row["question_disabled"]) : null;
            questionModel.objective = row["objective"] != DBNull.Value ? Convert.ToString(row["objective"]) : null;
            questionModel.Topic_Name = row["Topic_Name"] != DBNull.Value ? Convert.ToString(row["Topic_Name"]) : null;
            questionModel.Subject_Name = row["Subject_Name"] != DBNull.Value ? Convert.ToString(row["Subject_Name"]) : null;
            questionModel.Skill_Level_Name = row["Skill_Level_Name"] != DBNull.Value ? Convert.ToString(row["Skill_Level_Name"]) : null;

            questionModel.created_date = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]).Date : (DateTime?)null;

            return questionModel;
        }



        //New Helper
        private QuestionModelNew ConvertDataRowToQuestionBankModelNew(DataRow row)
        {
            QuestionModelNew questionModel = new QuestionModelNew();   

            questionModel.question_id = row["question_id"] != DBNull.Value ? Convert.ToInt32(row["question_id"]) : (int?)null;
            questionModel.question = row["question"] != DBNull.Value ? Convert.ToString(row["question"]) : null;
            questionModel.response_type = row["response_type"] != DBNull.Value ? Convert.ToInt32(row["response_type"]) : (int?)null;
            questionModel.no_of_selectionchoices = row["no_of_selectionchoices"] != DBNull.Value ? Convert.ToInt32(row["no_of_selectionchoices"]) : (int?)null;
            questionModel.correct_answer = row["correct_answer"] != DBNull.Value ? Convert.ToInt32(row["correct_answer"]) : (int?)null;
            questionModel.question_hint = row["question_hint"] != DBNull.Value ? Convert.ToString(row["question_hint"]) : null;
            questionModel.questionmarked_favourite = row["questionmarked_favourite"] != DBNull.Value ? Convert.ToString(row["questionmarked_favourite"]) : null;
            questionModel.score_weightage = row["score_weightage"] != DBNull.Value ? Convert.ToInt32(row["score_weightage"]) : (int?)null;
            questionModel.check_level = row["check_level"] != DBNull.Value ? Convert.ToInt32(row["check_level"]) : (int?)null;
            questionModel.checklevel_weightage = row["checklevel_weightage"] != DBNull.Value ? Convert.ToSingle(row["checklevel_weightage"]) : (float?)null;
            questionModel.estimated_time = row["estimated_time"] != DBNull.Value ? Convert.ToInt32(row["estimated_time"]) : (int?)null;
            questionModel.keywords = row["keywords"] != DBNull.Value ? Convert.ToString(row["keywords"]) : null;
            questionModel.assessor_randomselection = row["assessor_randomselection"] != DBNull.Value ? Convert.ToString(row["assessor_randomselection"]) : null;
            questionModel.assessment_randomsetting = row["assessment_randomsetting"] != DBNull.Value ? Convert.ToString(row["assessment_randomsetting"]) : null;
            questionModel.subjectid = row["subjectid"] != DBNull.Value ? Convert.ToInt32(row["subjectid"]) : (int?)null;
            questionModel.topicid = row["topicid"] != DBNull.Value ? Convert.ToInt32(row["topicid"]) : (int?)null;
            questionModel.ref_to_governance_control = row["ref_to_governance_control"] != DBNull.Value ? Convert.ToString(row["ref_to_governance_control"]) : null;
            questionModel.question_disabled = row["question_disabled"] != DBNull.Value ? Convert.ToString(row["question_disabled"]) : null;
            questionModel.objective = row["objective"] != DBNull.Value ? Convert.ToString(row["objective"]) : null;
            questionModel.Topic_Name = row["Topic_Name"] != DBNull.Value ? Convert.ToString(row["Topic_Name"]) : null;
            questionModel.Subject_Name = row["Subject_Name"] != DBNull.Value ? Convert.ToString(row["Subject_Name"]) : null;
            questionModel.checklevel_name = row["checklevel_name"] != DBNull.Value ? Convert.ToString(row["checklevel_name"]) : null;

            // Author Name (First name)
            // Author Name (First name)
            questionModel.authorName = row["authorName"] != DBNull.Value ? Convert.ToString(row["authorName"]) : null;

            // Location, Department, and Entity
            questionModel.location = row["Location"] != DBNull.Value ? Convert.ToString(row["Location"]) : "N/A";
            questionModel.departmentName = row["Department"] != DBNull.Value ? Convert.ToString(row["Department"]) : "N/A";
            questionModel.entity = row["Entity"] != DBNull.Value ? Convert.ToString(row["Entity"]) :"N/A";
            questionModel.tpaentity = row["tpaenityname"] != DBNull.Value ? Convert.ToString(row["tpaenityname"]) : "N/A";
            // Created Date (converted to DateTime if available)
            questionModel.created_date = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]).Date : (DateTime?)null;

            return questionModel;
        }






        [Route("api/AssessmentBuilder/GetAllQuestions/{UserId}")]
        [HttpGet]
        public IEnumerable<QuestionModel> GetAllQuestions(int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {

                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_generation WHERE UserID = @UserID AND Status = 'Active'";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@UserID", UserId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);

                        // Query to retrieve questions based on Assessment_generationID
                        string query = "SELECT qd.*, sb.Subject_Name, tb.Topic_Name, cl.Skill_Level_Name " +
  "FROM risk.questionbank qd " +
  "JOIN risk.subject sb ON sb.Subject_id = qd.subjectid " +
  "JOIN risk.topic tb ON tb.Topic_id = qd.topicid " +
  "JOIN risk.competency_check_level cl ON cl.check_level_id = qd.check_level " +
  "WHERE qd.Status = 'Active' AND qd.UserID = @UserID " +
  "AND qd.question_id NOT IN (SELECT question_id FROM risk.assessment_generation_details WHERE Assessment_generationID = @Assessment_generationID AND Status = 'Active')";

                        using (MySqlCommand command = new MySqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                            command.Parameters.AddWithValue("@UserID", UserId);

                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);

                                // Convert DataTable to a list of QuestionBankModel
                                List<QuestionModel> questionList = new List<QuestionModel>();
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    // Assuming you have a method to convert a DataRow to QuestionBankModel
                                    QuestionModel question = ConvertDataRowToQuestionBankModel(row);
                                    questionList.Add(question);
                                }

                                return questionList;
                            }
                        }
                    }
                    else
                    {
                        // Handle the case when Assessment_generationID is not found
                        return new List<QuestionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModel>();
            }
            finally
            {
                con.Close();
            }
        }



        //Getting all questions for ADD 

        [Route("api/AssessmentBuilder/GetAllQuestionsForAdd/{UserId}")]
        [HttpGet]
        public IEnumerable<QuestionModel> GetAllQuestionsForAdd(int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var userLocations = mySqlDBContext.userlocationmappingModels
              .Where(ad => ad.user_location_mapping_status == "Active" && ad.USR_ID == UserId)
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

            try
            {

                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_generation WHERE UserID = @UserID AND Status = 'Active'";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@UserID", UserId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);
                        List<QuestionModel> questionList = new List<QuestionModel>();
                        foreach (var userId in users)
                        {

                            // Query to retrieve questions based on Assessment_generationID
                            string query = "SELECT qd.*, sb.Subject_Name, tb.Topic_Name, cl.Skill_Level_Name " +
                                            "FROM risk.questionbank qd " +
                                            "JOIN risk.subject sb ON sb.Subject_id = qd.subjectid " +
                                            "JOIN risk.topic tb ON tb.Topic_id = qd.topicid " +
                                            "JOIN risk.competency_check_level cl ON cl.check_level_id = qd.check_level " +
                                            "WHERE qd.Status = 'Active' AND qd.UserID = @UserID " +
                                            "AND qd.question_id NOT IN (SELECT question_id FROM risk.assessment_generation_details WHERE Assessment_generationID = @Assessment_generationID AND Status = 'Active')";

                            using (MySqlCommand command = new MySqlCommand(query, con))
                            {
                                command.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                                command.Parameters.AddWithValue("@UserID", userId);

                                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                                {
                                    DataTable dataTable = new DataTable();
                                    adapter.Fill(dataTable);

                                    // Convert DataTable to a list of QuestionBankModel
                                  
                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        // Assuming you have a method to convert a DataRow to QuestionBankModel
                                        QuestionModel question = ConvertDataRowToQuestionBankModel(row);
                                        questionList.Add(question);
                                    }

                                   
                                }
                            }

                        }
                        return questionList;
                    }
                    else
                    {
                        // Handle the case when Assessment_generationID is not found
                        return new List<QuestionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModel>();
            }
            finally
            {
                con.Close();
            }
        }




        [Route("api/AssessmentBuilder/GetAllQuestionsforupdate/{TempId}/{UserId}")]
        [HttpGet]
        public IEnumerable<QuestionModel> GetAllQuestionsforupdate(string TempId,int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {

                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_builder WHERE ass_template_id = @ass_template_id ";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@ass_template_id", TempId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);

                        // Query to retrieve questions based on Assessment_generationID
                        string query = "SELECT qd.*, sb.Subject_Name, tb.Topic_Name, cl.Skill_Level_Name " +
  "FROM risk.questionbank qd " +
  "JOIN risk.subject sb ON sb.Subject_id = qd.subjectid " +
  "JOIN risk.topic tb ON tb.Topic_id = qd.topicid " +
  "JOIN risk.competency_check_level cl ON cl.check_level_id = qd.check_level " +
  "WHERE qd.Status = 'Active' AND qd.UserID = @UserID " +
  "AND qd.question_id NOT IN (SELECT question_id FROM risk.assessment_generation_details WHERE Assessment_generationID = @Assessment_generationID AND Status = 'Active')";

                        using (MySqlCommand command = new MySqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                            command.Parameters.AddWithValue("@UserID", UserId);

                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);

                                // Convert DataTable to a list of QuestionBankModel
                                List<QuestionModel> questionList = new List<QuestionModel>();
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    // Assuming you have a method to convert a DataRow to QuestionBankModel
                                    QuestionModel question = ConvertDataRowToQuestionBankModel(row);
                                    questionList.Add(question);
                                }

                                return questionList;
                            }
                        }
                    }
                    else
                    {
                        // Handle the case when Assessment_generationID is not found
                        return new List<QuestionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModel>();
            }
            finally
            {
                con.Close();
            }
        }

        // Getting all questions for update 


        [Route("api/AssessmentBuilder/GetAllQuestionsForAdd/{TempId}/{UserId}")]
        [HttpGet]
        public IEnumerable<QuestionModel> GetAllQuestionsForUpdate(string TempId, int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var userLocations = mySqlDBContext.userlocationmappingModels
              .Where(ad => ad.user_location_mapping_status == "Active" && ad.USR_ID == UserId)
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

            try
            {

                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_builder WHERE ass_template_id = @ass_template_id ";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@ass_template_id", TempId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);
                        List<QuestionModel> questionList = new List<QuestionModel>();
                        foreach (var userId in users)
                        {

                            // Query to retrieve questions based on Assessment_generationID
                            string query = "SELECT qd.*, sb.Subject_Name, tb.Topic_Name, cl.Skill_Level_Name " +
                                            "FROM risk.questionbank qd " +
                                            "JOIN risk.subject sb ON sb.Subject_id = qd.subjectid " +
                                            "JOIN risk.topic tb ON tb.Topic_id = qd.topicid " +
                                            "JOIN risk.competency_check_level cl ON cl.check_level_id = qd.check_level " +
                                            "WHERE qd.Status = 'Active' AND qd.UserID = @UserID " +
                                            "AND qd.question_id NOT IN (SELECT question_id FROM risk.assessment_generation_details WHERE Assessment_generationID = @Assessment_generationID AND Status = 'Active')";

                            using (MySqlCommand command = new MySqlCommand(query, con))
                            {
                                command.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                                command.Parameters.AddWithValue("@UserID", userId);

                                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                                {
                                    DataTable dataTable = new DataTable();
                                    adapter.Fill(dataTable);

                                    // Convert DataTable to a list of QuestionBankModel

                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        // Assuming you have a method to convert a DataRow to QuestionBankModel
                                        QuestionModel question = ConvertDataRowToQuestionBankModel(row);
                                        questionList.Add(question);
                                    }


                                }
                            }

                        }
                        return questionList;
                    }
                    else
                    {
                        // Handle the case when Assessment_generationID is not found
                        return new List<QuestionModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModel>();
            }
            finally
            {
                con.Close();
            }
        }






        [Route("api/AssessmentBuilder/GetAllQuestionsforCustomization/{UserId}")]
        [HttpGet]
        public IEnumerable<QuestionModelNew> GetAllQuestionsforCustomization(int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {

                con.Open();

                // Query to retrieve questions based on Assessment_generationID
                string query = @"
            SELECT 
                qb.question_id,
                qb.question,
                qb.response_type,
                qb.no_of_selectionchoices,
                qb.correct_answer,
                qb.question_hint,
                qb.questionmarked_favourite,
                qb.score_weightage,
                qb.check_level,
                qb.checklevel_weightage,
                qb.estimated_time,
                qb.keywords,
                qb.assessor_randomselection,
                qb.assessment_randomsetting,
                qb.subjectid,
                qb.topicid,
                qb.ref_to_governance_control,
                qb.question_disabled,
                qb.objective,
                qb.base64,
                qb.userid,
                qb.question_weightage,
                qb.created_date,
                qb.status,
                qb.reason_for_disable,
                sb.Subject_Name,
                tp.Topic_Name,
                ccl.Skill_Level_Name AS checklevel_name,
                tu.firstname AS authorName,
                dm.Department_Master_name AS Department,
                ulm.Unit_location_Master_name AS Location,
                em.Entity_Master_Name AS Entity,tpaem.tpaenityname
            FROM 
                risk.questionbank qb
             JOIN 
                risk.topic tp ON tp.Topic_id = qb.topicid
            JOIN 
                risk.subject sb ON sb.Subject_id = tp.Subject_id
            JOIN 
                risk.tbluser tu ON tu.USR_ID = qb.userid
            Left JOIN  
                risk.department_master dm ON dm.Department_Master_id = tu.Department_Master_id
            Left JOIN  
                risk.unit_location_master ulm ON ulm.Unit_location_Master_id = tu.Unit_location_Master_id
            Left JOIN  
                risk.entity_master em ON em.Entity_Master_id = tu.Entity_Master_id
            LEFT JOIN 
                    tpaenity as tpaem on tpaem.tpaenityid=tu.tpaenityid
            JOIN 
                risk.competency_check_level ccl ON ccl.check_level_id = qb.check_level
            WHERE 
                qb.status = 'Active'
                AND qb.userid = @UserID";


                using (MySqlCommand command = new MySqlCommand(query, con))
                {

                    command.Parameters.AddWithValue("@UserID", UserId);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Convert DataTable to a list of QuestionBankModel
                        List<QuestionModelNew> questionList = new List<QuestionModelNew>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            // Assuming you have a method to convert a DataRow to QuestionBankModel
                            QuestionModelNew question = ConvertDataRowToQuestionBankModelNew(row);

                            questionList.Add(question);
                        }

                        return questionList;
                    }
                }






            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModelNew>();
            }
            finally
            {
                con.Close();
            }
        }



        [Route("api/AssessmentBuilder/GetAllQuestionsfromQuestionBankReserve")]
        [HttpGet]
        public IEnumerable<QuestionModel> GetAllQuestionsfromQuestionBankReserve()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {

                con.Open();

                // Query to retrieve questions based on Assessment_generationID
                string query = "SELECT qd.*, sb.Subject_Name, tb.Topic_Name, cl.Skill_Level_Name " +
"FROM risk.questionbank qd " +
"JOIN risk.subject sb ON sb.Subject_id = qd.subjectid " +
"JOIN risk.topic tb ON tb.Topic_id = qd.topicid " +
"JOIN risk.competency_check_level cl ON cl.check_level_id = qd.check_level " +
"WHERE qd.Status = 'Active'";


                using (MySqlCommand command = new MySqlCommand(query, con))
                {



                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Convert DataTable to a list of QuestionBankModel
                        List<QuestionModel> questionList = new List<QuestionModel>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            // Assuming you have a method to convert a DataRow to QuestionBankModel
                            QuestionModel question = ConvertDataRowToQuestionBankModel(row);

                            questionList.Add(question);
                        }

                        return questionList;
                    }
                }



            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModel>();
            }
            finally
            {
                con.Close();
            }
        }









        //Status update for selected questions 


        [Route("api/AssessmentBuilder/GenQuestStaUpdated/{UserId}")]
        [HttpPost]

        public IActionResult GenQuestStaUpdated([FromBody] List<int> selectedQuestionIds, int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {
                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_generation WHERE UserID = @UserID AND Status = 'Active'";
                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@UserID", UserId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);




                        var detailsToUpdate = this.mySqlDBContext.AssessGenDetailsModels
  .Where(detail => selectedQuestionIds.Contains(detail.question_id) && detail.Assessment_generationID == AssessmentgenerationID)
  .ToList();
                        // Update the status of each Assessment Generation Detail
                        foreach (var detail in detailsToUpdate)
                        {
                            detail.Status = "inactive"; // Set the new status value
                        }

                        // Save changes to the database
                        this.mySqlDBContext.SaveChanges();

                        return Ok(new { Message = "Status updated successfully." });
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return BadRequest(new { Message = "Error updating status.", Error = ex.Message });

            }
            finally
            {
                con.Close();

            }
        }









        [Route("api/AssessmentBuilder/GenQuestStaUpdatedforTempId/{tempid}")]
        [HttpPost]

        public IActionResult GenQuestStaUpdatedfortempid([FromBody] List<int> selectedQuestionIds, string tempid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {
                if (selectedQuestionIds != null)
                {
                    con.Open();

                    // Retrieve Assessment_generationID based on UserID and Status
                    string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_builder WHERE ass_template_id = @ass_template_id AND Status = 'Active'";
                    using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                    {
                        myCommand4.Parameters.AddWithValue("@ass_template_id", tempid);

                        // Execute the query and retrieve the Assessment_generationID
                        object generationIdObject = myCommand4.ExecuteScalar();

                        if (generationIdObject != null)
                        {
                            int AssessmentgenerationID = Convert.ToInt32(generationIdObject);




                            var detailsToUpdate = this.mySqlDBContext.AssessGenDetailsModels
      .Where(detail => selectedQuestionIds.Contains(detail.question_id) && detail.Assessment_generationID == AssessmentgenerationID)
      .ToList();
                            // Update the status of each Assessment Generation Detail
                            foreach (var detail in detailsToUpdate)
                            {
                                detail.Status = "inactive"; // Set the new status value
                            }

                            // Save changes to the database
                            this.mySqlDBContext.SaveChanges();

                            return Ok(new { Message = "Status updated successfully." });
                        }
                    }
                    return Ok();
                }
                else
                {
                    return BadRequest(new { Message = "Please select atleast one question." });
                }
                
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return BadRequest(new { Message = "Error updating status.", Error = ex.Message });

            }
            finally
            {
                con.Close();

            }
        }







        [Route("api/AssessmentBuilder/UpdateGenDetails/{tempid}")]
        [HttpPost]
        public IActionResult UpdateGenDetails([FromBody] List<int> selectedQuestionIds, string tempid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {
                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_builder WHERE ass_template_id = @ass_template_id ";
                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@ass_template_id", tempid);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);

                        // Insert each selected question into Assessment_generation_details table
                        foreach (int questionId in selectedQuestionIds)
                        {
                            AssessGenDetailsModel newDetail = new AssessGenDetailsModel
                            {
                                Assessment_generationID = AssessmentgenerationID,
                                question_id = questionId,
                                Status = "Active",
                                // Other properties if needed
                            };

                            this.mySqlDBContext.AssessGenDetailsModels.Add(newDetail);
                        }

                        // Save changes to the database
                        this.mySqlDBContext.SaveChanges();

                        return Ok(new { Message = "Questions inserted successfully." });
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return BadRequest(new { Message = "Error inserting questions.", Error = ex.Message });
            }
            finally
            {
                con.Close();
            }
        }


        //Insert questions in Generation table

        [Route("api/AssessmentBuilder/insertGenDetails/{UserId}")]

        [HttpPost]
        public IActionResult insertGenDetails([FromBody] List<int> selectedQuestionIds, int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {
                con.Open();

                // Retrieve Assessment_generationID based on UserID and Status
                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_generation WHERE UserID = @UserID AND Status = 'Active'";
                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@UserID", UserId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);

                        // Insert each selected question into Assessment_generation_details table
                        foreach (int questionId in selectedQuestionIds)
                        {
                            AssessGenDetailsModel newDetail = new AssessGenDetailsModel
                            {
                                Assessment_generationID = AssessmentgenerationID,
                                question_id = questionId,
                                Status = "Active",
                                // Other properties if needed
                            };

                            this.mySqlDBContext.AssessGenDetailsModels.Add(newDetail);
                        }

                        // Save changes to the database
                        this.mySqlDBContext.SaveChanges();

                        return Ok(new { Message = "Questions inserted successfully." });
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return BadRequest(new { Message = "Error inserting questions.", Error = ex.Message });
            }
            finally
            {
                con.Close();
            }
        }




        //iunsert Customization Questions


        [Route("api/AssessmentBuilder/insertGenQuestions/{UserId}")]
        [HttpPost]
        public IActionResult InsertCustomizationQuestions([FromBody] List<int> selectedQuestionIds, int UserId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                // First, find and update the status of the last assessment generation
                string updateStatusQuery = "UPDATE assessment_generation SET Status = 'Inactive' WHERE UserID = @UserID AND Status = 'Active'";

                using (MySqlCommand updateStatusCommand = new MySqlCommand(updateStatusQuery, con))
                {
                    updateStatusCommand.Parameters.AddWithValue("@UserID", UserId);

                    updateStatusCommand.ExecuteNonQuery();
                }

                // Now, insert a new assessment generation
                string insertQuery = "INSERT INTO assessment_generation(UserID, CreatedDate, Status) VALUES (@UserID, @CreatedDate, @Status)";

                using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, con))
                {
                    insertCommand.Parameters.AddWithValue("@UserID", UserId);
                    insertCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    insertCommand.Parameters.AddWithValue("@Status", "Active");

                    insertCommand.ExecuteNonQuery();

                    int assessmentGenerationId = Convert.ToInt32(insertCommand.LastInsertedId.ToString());

                    foreach (int questionId in selectedQuestionIds)
                    {









                        AssessGenDetailsModel newDetail = new AssessGenDetailsModel
                        {
                            Assessment_generationID = assessmentGenerationId,
                            question_id = questionId,
                            Status = "Active",
                            // Other properties if needed
                        };

                        this.mySqlDBContext.AssessGenDetailsModels.Add(newDetail);
                    }

                    // Save changes to the database
                    this.mySqlDBContext.SaveChanges();

                    return Ok(new { Message = "Questions inserted successfully." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error inserting questions.", Error = ex.Message });
            }
            finally
            {
                con.Close();
            }
        }







        [Route("api/AssessmentBuilder/GetAllAssessmentTemplates")]
        [HttpGet]
        public IEnumerable<QuestionModel> GetAllAssessmentTemplates()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {
                con.Open();

                // Query to retrieve all rows from risk.assessment_builder
                string query = "SELECT * FROM risk.assessment_builder";

                using (MySqlCommand command = new MySqlCommand(query, con))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Convert DataTable to a list of QuestionBankModel
                        List<QuestionModel> questionList = new List<QuestionModel>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            // Assuming you have a method to convert a DataRow to QuestionBankModel
                            QuestionModel question = ConvertDataRowToQuestionBankModel(row);
                            questionList.Add(question);
                        }

                        return questionList;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                return new List<QuestionModel>();
            }
            finally
            {
                con.Close();
            }
        }















        //insert Assessment builder values



        [Route("api/AssessmentBuilder/UpdateAssBuilder/{tempid}/{userid}")]
        [HttpPost]
        public IActionResult UpdateAssBuilder([FromBody] AssessmentPublisherModel AssessmentPublisherModels, int tempid, int userid)
        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_builder WHERE ass_template_id = @ass_template_id ";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@ass_template_id", tempid);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);



                        string updateQuery = "UPDATE assessment_builder SET Competency_id=@Competency_id, show_explaination=@show_explanation, show_hint=@show_hint, Type_id=@Type_id, SubType_id=@SubType_id, assessment_name=@assessment_name, assessment_description=@assessment_description, keywords=@keywords,updated_date=@updated_date,updated_user_id=@updated_user_id,status=@status WHERE Assessment_generationID=@Assessment_generationID and  ass_template_id=@ass_template_id";

                        using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                            myCommand1.Parameters.AddWithValue("@ass_template_id", tempid);
                            myCommand1.Parameters.AddWithValue("@Competency_id", AssessmentPublisherModels.Competency_id);
                            myCommand1.Parameters.AddWithValue("@show_explanation", AssessmentPublisherModels.show_explaination);
                            myCommand1.Parameters.AddWithValue("@show_hint", AssessmentPublisherModels.show_hint);
                            myCommand1.Parameters.AddWithValue("@Type_id", AssessmentPublisherModels.Type_id);
                            myCommand1.Parameters.AddWithValue("@SubType_id", AssessmentPublisherModels.SubType_id);
                            myCommand1.Parameters.AddWithValue("@assessment_name", AssessmentPublisherModels.assessment_name);
                            myCommand1.Parameters.AddWithValue("@assessment_description", AssessmentPublisherModels.assessment_description);
                            myCommand1.Parameters.AddWithValue("@keywords", AssessmentPublisherModels.keywords);
                            myCommand1.Parameters.AddWithValue("@updated_date", DateTime.Now);
                            myCommand1.Parameters.AddWithValue("@status", "Active");
                            myCommand1.Parameters.AddWithValue("@updated_user_id", userid);


                            myCommand1.ExecuteNonQuery();
                        }






                    }
                }


                return Ok("Assessment Builder values inserted successfully");
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




        //New Update method for assessment builder 


        [Route("api/AssessmentBuilder/UpdateAssBuilderNew")]
        [HttpPost]
        public IActionResult UpdateAssBuilderNew([FromBody] AssessmentPublisherModelNew AssessmentPublisherModels)
        {
             



            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_builder WHERE ass_template_id = @ass_template_id ";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@ass_template_id", AssessmentPublisherModels.ass_template_id);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);
                        int totalFields = 0;

                        var totalQuesIDs = this.mySqlDBContext.AssessGenDetailsModels.Where(x => x.Assessment_generationID == AssessmentgenerationID && x.Status == "Active").Select(x=>x.question_id).ToList();
                        var totalNumberOfQues = this.mySqlDBContext.AssessGenDetailsModels.Count(x => x.Assessment_generationID == AssessmentgenerationID && x.Status == "Active");
                        var estimatedTime = this.mySqlDBContext.QuestionBankModels.Where(item => totalQuesIDs.Contains(item.question_id) && item.status == "Active").Sum(item => item.estimated_time);


                        string updateQuery = "UPDATE assessment_builder SET Competency_id=@Competency_id, show_explaination=@show_explanation, show_hint=@show_hint, Type_id=@Type_id, SubType_id=@SubType_id, assessment_name=@assessment_name, assessment_description=@assessment_description, keywords=@keywords,updated_date=@updated_date,updated_user_id=@updated_user_id,status=@status,verson_no = verson_no + 1,total_estimated_time=@total_estimated_time,total_questions=@total_questions WHERE Assessment_generationID=@Assessment_generationID and  ass_template_id=@ass_template_id";

                        using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                            myCommand1.Parameters.AddWithValue("@ass_template_id", AssessmentPublisherModels.ass_template_id);
                            myCommand1.Parameters.AddWithValue("@Competency_id", AssessmentPublisherModels.Competency_id);
                            myCommand1.Parameters.AddWithValue("@show_explanation", AssessmentPublisherModels.show_explaination);
                            myCommand1.Parameters.AddWithValue("@show_hint", AssessmentPublisherModels.show_hint);
                            myCommand1.Parameters.AddWithValue("@Type_id", AssessmentPublisherModels.Type_id);
                            myCommand1.Parameters.AddWithValue("@SubType_id", AssessmentPublisherModels.SubType_id);
                            myCommand1.Parameters.AddWithValue("@assessment_name", AssessmentPublisherModels.assessment_name);
                            myCommand1.Parameters.AddWithValue("@assessment_description", AssessmentPublisherModels.assessment_description);
                            myCommand1.Parameters.AddWithValue("@keywords", AssessmentPublisherModels.keywords);
                            myCommand1.Parameters.AddWithValue("@updated_date", DateTime.Now);
                            myCommand1.Parameters.AddWithValue("@status", "Active");
                            myCommand1.Parameters.AddWithValue("@updated_user_id", AssessmentPublisherModels.user_id);
                            myCommand1.Parameters.AddWithValue("@total_estimated_time", estimatedTime);
                            myCommand1.Parameters.AddWithValue("@total_questions", totalNumberOfQues);



                            myCommand1.ExecuteNonQuery();
                        }


                        //Update Version Table 
                        int newPrimaryKey = 0;
                        string insertBackupQuery = @"INSERT INTO assessment_generation 
                                (QuestionMixtype, No_of_Questions, MostUsedQuestions, FavouritesDefaults, RecentlyAdded, TimeEstimate, TimeEstimateInputMin, TimeEstimateInputMax, UserID, CreatedDate, Status) 
                                SELECT QuestionMixtype, No_of_Questions, MostUsedQuestions, FavouritesDefaults, RecentlyAdded, TimeEstimate, TimeEstimateInputMin, TimeEstimateInputMax, UserID, @CreatedDate, @Status 
                                FROM assessment_generation WHERE Assessment_generationID = @Assessment_generationID;

                                SELECT LAST_INSERT_ID();";

                        using (MySqlCommand myCommand2 = new MySqlCommand(insertBackupQuery, con))
                        {
                            myCommand2.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                            myCommand2.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                            myCommand2.Parameters.AddWithValue("@Status", "Active");
                       

                            object result = myCommand2.ExecuteScalar();


                            newPrimaryKey = Convert.ToInt32(result);


                        }

                        foreach (int questionId in AssessmentPublisherModels.OldQuestionsIds)
                        {
                            string insertQ = @"
                                    INSERT INTO assessment_generation_details 
                                    (Assessment_generationID, question_id, Status, CreatedDate) 
                                    VALUES 
                                    (@Assessment_generationID, @question_id, @Status, @CreatedDate);";

                            using (MySqlCommand cmd = new MySqlCommand(insertQ, con))
                            {
                                cmd.Parameters.AddWithValue("@Assessment_generationID", newPrimaryKey);
                                cmd.Parameters.AddWithValue("@question_id", questionId);
                                cmd.Parameters.AddWithValue("@Status", "Active");
                                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                                cmd.ExecuteNonQuery();
                            }






                        }


                        string updateBuilderVersion = @"
                                        UPDATE assessment_builder_versions
                                        SET Assessment_generationID = @Assessment_generationID
                                        WHERE ass_template_id = @ass_template_id
                                        ORDER BY verson_no DESC
                                        LIMIT 1;";

                        using (MySqlCommand myCommand = new MySqlCommand(updateBuilderVersion, con))
                        {
                            myCommand.Parameters.AddWithValue("@Assessment_generationID", newPrimaryKey);
                            myCommand.Parameters.AddWithValue("@ass_template_id", AssessmentPublisherModels.ass_template_id);

                            int rowsAffected = myCommand.ExecuteNonQuery();

                        }
                        // InActiving the Older version
                        string deactivateOldVersion = @"
    UPDATE assessment_builder_versions
    SET status = 'InActive'
    WHERE ass_template_id = @ass_template_id
    ORDER BY verson_no DESC
    LIMIT 1;";

                        using (MySqlCommand cmd = new MySqlCommand(deactivateOldVersion, con))
                        {
                            cmd.Parameters.AddWithValue("@ass_template_id", AssessmentPublisherModels.ass_template_id);
                            cmd.ExecuteNonQuery();
                        }
                        string InsertQ = "INSERT INTO assessment_builder_versions (assessment_builder_id,Assessment_generationID, Competency_id, show_explaination, show_hint, Type_id, SubType_id, assessment_name, assessment_description, keywords, created_date, status, ass_template_id, user_id, total_questions, total_estimated_time, verson_no, backup_date,updated_date,updated_user_id) " +
                                         "SELECT assessment_builder_id,Assessment_generationID, Competency_id, show_explaination, show_hint, Type_id, SubType_id, assessment_name, assessment_description, keywords, created_date, status, ass_template_id, user_id, total_questions, total_estimated_time, verson_no, @backup_date,@updated_date,@updated_user_id " +
                                         "FROM assessment_builder WHERE ass_template_id=@ass_template_id";

                        using (MySqlCommand myCommand2 = new MySqlCommand(InsertQ, con))
                        {
                            myCommand2.Parameters.AddWithValue("@ass_template_id", AssessmentPublisherModels.ass_template_id);
                            myCommand2.Parameters.AddWithValue("@backup_date", DateTime.Now);
                            myCommand2.Parameters.AddWithValue("@updated_date", DateTime.Now);
                            myCommand2.Parameters.AddWithValue("@updated_user_id", AssessmentPublisherModels.user_id);

                            myCommand2.ExecuteNonQuery();
                        }







                    }
                }


                return Ok("Assessment Builder values inserted successfully");
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






        [Route("api/AssessmentBuilder/InsertAssBuilder/{UserId}")]
        [HttpPost]
        public IActionResult InsertAssBuilder([FromBody] AssessmentPublisherModel AssessmentPublisherModels, int UserId)
        {



            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                string selectGenerationIdQuery = "SELECT Assessment_generationID FROM assessment_generation WHERE UserID = @UserID AND Status = 'Active'";

                using (MySqlCommand myCommand4 = new MySqlCommand(selectGenerationIdQuery, con))
                {
                    myCommand4.Parameters.AddWithValue("@UserID", UserId);

                    // Execute the query and retrieve the Assessment_generationID
                    object generationIdObject = myCommand4.ExecuteScalar();

                    if (generationIdObject != null)
                    {
                        int AssessmentgenerationID = Convert.ToInt32(generationIdObject);

                        string assTemplateId = GenerateUniqueAssTemplateId(con);

                        string insertQuery = "insert into assessment_builder(Assessment_generationID,Competency_id,show_explaination,show_hint,Type_id,SubType_id,assessment_name,assessment_description,keywords,created_date,status,ass_template_id,user_id,total_questions,total_estimated_time,verson_no)values(@Assessment_generationID,@Competency_id,@show_explaination,@show_hint,@Type_id,@SubType_id,@assessment_name,@assessment_description,@keywords,@created_date,@status,@ass_template_id,@user_id,@total_questions,@total_estimated_time,@verson_no)";


                        using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))

                        {
                            myCommand1.Parameters.AddWithValue("@Assessment_generationID", AssessmentgenerationID);
                            myCommand1.Parameters.AddWithValue("@Competency_id", AssessmentPublisherModels.Competency_id);
                            myCommand1.Parameters.AddWithValue("@show_explaination", AssessmentPublisherModels.show_explaination);
                            myCommand1.Parameters.AddWithValue("@show_hint", AssessmentPublisherModels.show_hint);
                            myCommand1.Parameters.AddWithValue("@Type_id", AssessmentPublisherModels.Type_id);
                            myCommand1.Parameters.AddWithValue("@SubType_id", AssessmentPublisherModels.SubType_id);
                            myCommand1.Parameters.AddWithValue("@assessment_name", AssessmentPublisherModels.assessment_name);
                            myCommand1.Parameters.AddWithValue("@assessment_description", AssessmentPublisherModels.assessment_description);
                            myCommand1.Parameters.AddWithValue("@keywords", AssessmentPublisherModels.keywords);
                            myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                            myCommand1.Parameters.AddWithValue("@status", "Active");
                            myCommand1.Parameters.AddWithValue("@ass_template_id", assTemplateId);
                            myCommand1.Parameters.AddWithValue("@user_id", UserId);
                            myCommand1.Parameters.AddWithValue("@total_questions", AssessmentPublisherModels.total_questions);
                            myCommand1.Parameters.AddWithValue("@total_estimated_time", AssessmentPublisherModels.total_estimated_time);
                            myCommand1.Parameters.AddWithValue("@verson_no", 1);

                            myCommand1.ExecuteNonQuery();




                        }

                        string insertBackupQuery = "INSERT INTO assessment_builder_versions (assessment_builder_id,Assessment_generationID, Competency_id, show_explaination, show_hint, Type_id, SubType_id, assessment_name, assessment_description, keywords, created_date, status, ass_template_id, user_id, total_questions, total_estimated_time, verson_no, backup_date) " +
                                         "SELECT assessment_builder_id,Assessment_generationID, Competency_id, show_explaination, show_hint, Type_id, SubType_id, assessment_name, assessment_description, keywords, created_date, status, ass_template_id, user_id, total_questions, total_estimated_time, verson_no, @backup_date " +
                                         "FROM assessment_builder WHERE ass_template_id=@ass_template_id";

                        using (MySqlCommand myCommand2 = new MySqlCommand(insertBackupQuery, con))
                        {
                            myCommand2.Parameters.AddWithValue("@ass_template_id", assTemplateId);
                            myCommand2.Parameters.AddWithValue("@backup_date", DateTime.Now);

                            myCommand2.ExecuteNonQuery();
                        }


                        //assessment Provide Access
                        // Retrieve UserLocationMappingID
                        string getUserLocationMappingIdQuery = @"SELECT user_location_mapping_id, Entity_Master_id, Unit_location_Master_id 
                                         FROM user_location_mapping 
                                         WHERE USR_ID = @UserId";

                        int? user_location_mapping_id = null;
                        int? Unit_location_Master_id = null;
                        int? Entity_Master_id = null;

                        using (MySqlCommand myCommand3 = new MySqlCommand(getUserLocationMappingIdQuery, con))
                        {
                            myCommand3.Parameters.AddWithValue("@UserId", UserId);

                            using (MySqlDataReader reader = myCommand3.ExecuteReader())
                            {
                                if (reader.Read()) // Check if a row is returned
                                {
                                    user_location_mapping_id = reader["user_location_mapping_id"] as int?;
                                    Entity_Master_id = reader["Entity_Master_id"] as int?;
                                    Unit_location_Master_id = reader["Unit_location_Master_id"] as int?;
                                }
                            }
                        }

                        // assessment builder id
                         string builderid = @"SELECT assessment_builder_id
                                         FROM assessment_builder 
                                         WHERE Assessment_generationID = @AssessmentgenerationID";


                        int? assessment_builder_id = null;

                        using (MySqlCommand myCommand3 = new MySqlCommand(builderid, con))
                        {
                            myCommand3.Parameters.AddWithValue("@AssessmentgenerationID", AssessmentgenerationID);

                            using (MySqlDataReader reader = myCommand3.ExecuteReader())
                            {
                                if (reader.Read()) // Check if a row is returned
                                {
                                    assessment_builder_id = reader["assessment_builder_id"] as int?;
                                }
                            }
                        }
                        string insertAccessQuery = (@"insert into assement_provideacess(AssessementTemplateID,Access_Permissions,Status,CreatedDate,UserID,createdby,UserloactionmappingID,EntityMasterID,UnitLocationMasterID,assessment_builder_id)values
                    (@AssessementTemplateID,@Access_Permissions,@Status,@CreatedDate,@UserID,@createdby,@UserloactionmappingID,@EntityMasterID,@UnitLocationMasterID,@assessment_builder_id)");


                        using (MySqlCommand myCommand = new MySqlCommand(insertAccessQuery, con))
                        {

                            myCommand.Parameters.AddWithValue("@AssessementTemplateID", assTemplateId);

                            myCommand.Parameters.AddWithValue("@Access_Permissions", 2);

                            myCommand.Parameters.AddWithValue("@Status", "Active");
                            myCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                            myCommand.Parameters.AddWithValue("@UserID", UserId);
                            myCommand.Parameters.AddWithValue("@createdby", UserId);
                            myCommand.Parameters.AddWithValue("@UserloactionmappingID", user_location_mapping_id);
                            myCommand.Parameters.AddWithValue("@EntityMasterID", Entity_Master_id);
                            myCommand.Parameters.AddWithValue("@UnitLocationMasterID", Unit_location_Master_id);
                            myCommand.Parameters.AddWithValue("@assessment_builder_id", assessment_builder_id);


                            myCommand.ExecuteNonQuery();




                        }


                    }


                    return Ok("Assessment Builder values inserted successfully");
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
        private string GenerateUniqueAssTemplateId(MySqlConnection con)
        {
            string assTemplateId;

            do
            {
                // Use current date components for the ID
                string datePart = DateTime.Now.ToString("yyyyMMdd");


                // Ensure that datePart does not start with 0
                //if (datePart[0] == '0')
                //{
                //    datePart = datePart.Substring(1); // Remove the leading 0
                //}

                // Generate a random three-digit number
                string randomPart = new Random().Next(100, 1000).ToString("D3");

                // Combine date and random parts
                assTemplateId = $"{datePart}{randomPart}";

                string checkDuplicateQuery = "SELECT COUNT(*) FROM assessment_builder WHERE ass_template_id = @assTemplateId";
                using (MySqlCommand checkDuplicateCommand = new MySqlCommand(checkDuplicateQuery, con))
                {
                    checkDuplicateCommand.Parameters.AddWithValue("@assTemplateId", assTemplateId);
                    int count = Convert.ToInt32(checkDuplicateCommand.ExecuteScalar());
                    if (count == 0)
                    {
                        // Unique ID generated
                        break;
                    }
                }
            } while (true);

            return assTemplateId;
        }




    }

}
