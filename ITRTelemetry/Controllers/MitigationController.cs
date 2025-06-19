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
using Org.BouncyCastle.Ocsp;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using ITR_TelementaryAPI;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class MitigationController : ControllerBase
    {
        private ClsEmail obj_Clsmail = new ClsEmail();
        private readonly MySqlDBContext mySqlDBContext;
        private object random;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "MitigationFolder");

        public IConfiguration Configuration { get; }


        public MitigationController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor _httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            httpContextAccessor = _httpContextAccessor;
        }





        [Route("api/MitigationController/GetSuggestionsListManagement/{id}")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> GetSuggestionsListManagement(int id)
        {

            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
    st.acknowledge,
mitigations.uq_ass_schid,


    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
LEFT JOIN
    risk.mitigations AS mitigations ON mitigations.mitigations_id = st.mitigations_id

 
WHERE
    st.status = 'Active' AND st.mitigations_id = " + id + " GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge", con);


                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
                            {
                                uq_ass_schid = row["uq_ass_schid"].ToString(),
                                suggestions_id = row["suggestions_id"] != DBNull.Value ? Convert.ToInt32(row["suggestions_id"]) : (int?)null,
                                mitigations_id = row["mitigations_id"] != DBNull.Value ? Convert.ToInt32(row["mitigations_id"]) : (int?)null,
                                suggestions = row["suggestions"].ToString(),
                                status = row["status"].ToString(),
                              
                                //assessment_name = row["assessmentName"]?.ToString(),
                          
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
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString()
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


        [Route("api/MitigationController/GetActionsList/{id}")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> GetActionsList(int id)
        {
            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            string connectionString = Configuration["ConnectionStrings:myDb1"];
            string query = @"
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
            st.acknowledge,
            MAX(tblSuggester.firstname) AS Suggester_Name,
            MAX(tblAcknowledger.firstname) AS Acknowledger_Name
        FROM
            risk.suggestions_tbl st
        LEFT JOIN
            risk.tbluser tblSuggester ON tblSuggester.usr_ID = st.suggested_by
        LEFT JOIN
            risk.tbluser tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
        WHERE
            st.status = 'Active' AND st.assign_responsibility = @Id
        GROUP BY
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
            st.acknowledge";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suggestionsList.Add(new suggestionsModelforView
                            {
                                suggestions_id = reader.IsDBNull(reader.GetOrdinal("suggestions_id")) ? (int?)null : reader.GetInt32("suggestions_id"),
                                mitigations_id = reader.IsDBNull(reader.GetOrdinal("mitigations_id")) ? (int?)null : reader.GetInt32("mitigations_id"),
                                suggestions = reader.IsDBNull(reader.GetOrdinal("suggestions")) ? null : reader.GetString("suggestions"),
                                status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString("status"),
                                created_date = reader.IsDBNull(reader.GetOrdinal("created_date")) ? (DateTime?)null : reader.GetDateTime("created_date"),
                                suggested_by = reader.IsDBNull(reader.GetOrdinal("suggested_by")) ? (int?)null : reader.GetInt32("suggested_by"),
                                remarks = reader.IsDBNull(reader.GetOrdinal("remarks")) ? null : reader.GetString("remarks"),
                                acknowledge_by = reader.IsDBNull(reader.GetOrdinal("acknowledge_by")) ? (int?)null : reader.GetInt32("acknowledge_by"),
                                action_required = reader.IsDBNull(reader.GetOrdinal("action_required")) ? (int?)null : reader.GetInt32("action_required"),
                                notify_management = reader.IsDBNull(reader.GetOrdinal("notify_management")) ? (int?)null : reader.GetInt32("notify_management"),
                                input_date = reader.IsDBNull(reader.GetOrdinal("input_date")) ? (DateTime?)null : reader.GetDateTime("input_date"),
                                assign_responsibility = reader.IsDBNull(reader.GetOrdinal("assign_responsibility")) ? (int?)null : reader.GetInt32("assign_responsibility"),
                                tentative_timeline = reader.IsDBNull(reader.GetOrdinal("tentative_timeline")) ? (DateTime?)null : reader.GetDateTime("tentative_timeline"),
                                suggested_documents = reader.IsDBNull(reader.GetOrdinal("suggested_documents")) ? null : reader.GetString("suggested_documents"),
                                action_priority = reader.IsDBNull(reader.GetOrdinal("action_priority")) ? (int?)null : reader.GetInt32("action_priority"),
                                acknowledge = reader.IsDBNull(reader.GetOrdinal("acknowledge")) ? (int?)null : reader.GetInt32("acknowledge"),
                                Suggester_Name = reader.IsDBNull(reader.GetOrdinal("Suggester_Name")) ? null : reader.GetString("Suggester_Name"),
                                Acknowledger_Name = reader.IsDBNull(reader.GetOrdinal("Acknowledger_Name")) ? null : reader.GetString("Acknowledger_Name")
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







        //Getting Updated Acknowledgement suggestions
        [Route("api/MitigationController/GetAcknowledgedSuggestionsList")]
        [HttpGet]
        public IEnumerable<AcknowledgeSuggestionssModel> GetAcknowledgedSuggestionsList()
        {
            try
            {
                var suggestionsList = this.mySqlDBContext.AcknowledgeSuggestionssModels.ToList();

                // Handle null values explicitly for all columns
                foreach (var suggestion in suggestionsList)
                {
                    // Example: Handle null value for each property
                    var acknowledge_suggestions_id = suggestion.acknowledge_suggestions_id;
                    var suggestions_id = suggestion.suggestions_id;
                    var suggestions = suggestion.suggestions ?? "N/A";
                    var comment = suggestion.comment ?? "N/A";
                    var status = suggestion.status ?? "N/A";
                    var created_date = suggestion.created_date != DateTime.MinValue ? suggestion.created_date : DateTime.MinValue;
                    var acknowledge_by = suggestion.acknowledge_by;

                    // Perform any other necessary handling for nullable properties
                }

                return suggestionsList;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Exception: {ex.Message}");
                throw; // Rethrow the exception to propagate it further if needed
            }
        }


        //Getting mitigation data 


        //[Route("api/AddDocument/GetMitigationDetails")]
        //[HttpGet]
        //public IEnumerable<MitigationModel?> GetMitigationDetails()
        //{
        //    try
        //    {
        //        var mitigationList = this.mySqlDBContext.MitigationModels.ToList();

        //        // Handle null values explicitly for DateTime properties
        //        foreach (var mitigation in mitigationList)
        //        {
        //            // Example: Handle null value for each DateTime property
        //            mitigation.created_date = mitigation.created_date != null ? mitigation.created_date : DateTime.MinValue;
        //            mitigation.input_date = mitigation.input_date != null ? mitigation.input_date : DateTime.MinValue;
        //            mitigation.tentative_timeline = mitigation.tentative_timeline != null ? mitigation.tentative_timeline : DateTime.MinValue;

        //            // Example: Handle null value for string property
        //            mitigation.status = mitigation.status ?? "N/A";
        //            mitigation.suggested_documents = mitigation.suggested_documents ?? "N/A";

        //            // Perform any other necessary handling for nullable properties
        //        }



        //        return mitigationList;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log or handle the exception as needed
        //        Console.WriteLine($"Exception in GetMitigationDetails: {ex.Message}");
        //        throw; // Rethrow the exception to propagate it further if needed
        //    }
        //}

        // Get Mitigation Details by Id

        //[Route("api/AddDocument/GetMitigationDetailsbyId/{Mitigationid}")]
        //[HttpGet]
        //public IEnumerable<MitigationModel?> GetMitigationDetails(int Mitigationid)
        //{
        //    try
        //    {
        //        var mitigationList = this.mySqlDBContext.MitigationModels.Where(x => x.mitigations_id == Mitigationid && x.status == "Active").ToList();

        //        // Handle null values explicitly for DateTime properties
        //        foreach (var mitigation in mitigationList)
        //        {
        //            // Example: Handle null value for each DateTime property
        //            mitigation.created_date = mitigation.created_date != null ? mitigation.created_date : DateTime.MinValue;
        //            mitigation.input_date = mitigation.input_date != null ? mitigation.input_date : DateTime.MinValue;
        //            mitigation.tentative_timeline = mitigation.tentative_timeline != null ? mitigation.tentative_timeline : DateTime.MinValue;

        //            // Example: Handle null value for string property
        //            mitigation.status = mitigation.status ?? "N/A";
        //            mitigation.suggested_documents = mitigation.suggested_documents ?? "N/A";
        //            mitigation.comments = mitigation.comments ?? "N/A";
        //            mitigation.remarks = mitigation.remarks ?? "N/A";

        //            // Perform any other necessary handling for nullable properties
        //        }



        //        return mitigationList;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log or handle the exception as needed
        //        Console.WriteLine($"Exception in GetMitigationDetails: {ex.Message}");
        //        throw; // Rethrow the exception to propagate it further if needed
        //    }
        //}



        [Route("api/MitigationController/GetResponsersList")]
        [HttpGet]
        public IEnumerable<usermodel> GetResponsersList()
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var pdata = new List<usermodel>();
            try
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT mpu.USR_ID, ust.firstname FROM map_user_role AS mpu JOIN tbluser AS ust ON ust.USR_ID = mpu.USR_ID WHERE mpu.ROLE_ID = (SELECT ROLE_ID FROM tblrole WHERE ROLE_NAME = 'Process  Owner') AND mpu.mapuserrolestatus = 'Active' ", con);

                cmd.CommandType = CommandType.Text;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                con.Close();

                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        pdata.Add(new usermodel
                        {
                            USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),
                            firstname = dt.Rows[i]["firstname"].ToString(),
                           // roles = dt.Rows[i]["roles"].ToString(),
                            //  Check_Level_Weightage = dt.Rows[i]["Check_Level_Weightage"].ToString()


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



        //Update Mitigation Action Fields



        //[Route("api/MitigationController/SaveMitigationAction/{ActionId}")]
        //[HttpPost]
        //public IActionResult SaveMitigationAction([FromBody] MitigationModel MitigationModels, int ActionId) 
        //{




        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

        //    try
        //    {
        //        con.Open();







        //                string updateQuery = "UPDATE mitigations SET action_required=@action_required, notify_management=@notify_management, input_date=@input_date, assign_responsibility=@assign_responsibility, tentative_timeline=@tentative_timeline, suggested_documents=@suggested_documents, action_priority=@action_priority,comments=@comments WHERE mitigations_id=@mitigations_id ";

        //                using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery, con))
        //                {
        //                    myCommand1.Parameters.AddWithValue("@mitigations_id", ActionId);
        //                    //myCommand1.Parameters.AddWithValue("@ass_template_id", tempid);
        //                    myCommand1.Parameters.AddWithValue("@action_required", MitigationModels.action_required);
        //                    myCommand1.Parameters.AddWithValue("@notify_management", MitigationModels.notify_management);
        //                    myCommand1.Parameters.AddWithValue("@input_date", MitigationModels.input_date);
        //                    myCommand1.Parameters.AddWithValue("@assign_responsibility", MitigationModels.assign_responsibility);
        //                    myCommand1.Parameters.AddWithValue("@tentative_timeline", MitigationModels.tentative_timeline);
        //                    myCommand1.Parameters.AddWithValue("@suggested_documents", MitigationModels.suggested_documents);
        //                    myCommand1.Parameters.AddWithValue("@action_priority", MitigationModels.action_priority);
        //            myCommand1.Parameters.AddWithValue("@comments", MitigationModels.comments);



        //                    myCommand1.ExecuteNonQuery();
        //                }








        //        return Ok("Mitigation Action Updated successfully");
        //    }



        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //    finally
        //    {
        //        con.Close();
        //    }
        //}



        //Update Mitigation Action Status Completed

        [Route("api/MitigationController/SubmitMitigationStatus/{mitigationId}")]
        [HttpPost]
        public IActionResult SubmitMitigationStatus([FromBody] MitigationModel MitigationModels, int mitigationId)
        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();







                string updateQuery = "UPDATE mitigations SET remarks=@remarks, status=@status WHERE mitigations_id=@mitigations_id";

                using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@mitigations_id", mitigationId);

                    //myCommand1.Parameters.AddWithValue("@remarks", MitigationModels.remarks);
                    myCommand1.Parameters.AddWithValue("@status", "Completed");

                    // myCommand1.Parameters.AddWithValue("@updated_date", DateTime.Now);
                    // myCommand1.Parameters.AddWithValue("@status", "Active");
                    /// myCommand1.Parameters.AddWithValue("@updated_user_id", userid);


                    myCommand1.ExecuteNonQuery();
                }








                return Ok("Mitigation Action  Status Updated successfully");
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




        //Insert Suggestions in Acknowledge Table

        [Route("api/MitigationController/AcknowledgedSuggestions/{userid}")]
        [HttpPost]
        public IActionResult AcknowledgedSuggestions([FromBody] List<int> SelectedSuugestionIds, int userid)
        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();




                foreach (int ActionId in SelectedSuugestionIds)
                {
                    string suggestionActionQuery = "SELECT suggestions FROM suggestions_tbl WHERE suggestions_id = @suggestions_id ";
                    using (MySqlCommand myCommand2 = new MySqlCommand(suggestionActionQuery, con))
                    {
                        myCommand2.Parameters.AddWithValue("@suggestions_id", ActionId);
                        string suggestionAction = myCommand2.ExecuteScalar() as string;





                        string InsertQuery = "insert into acknowledge_suggestions(suggestions_id,suggestions,status,created_date,acknowledge_by)values(@suggestions_id,@suggestions,@status,@created_date,@acknowledge_by)";
                        using (MySqlCommand myCommand1 = new MySqlCommand(InsertQuery, con))
                        {
                            myCommand1.Parameters.AddWithValue("@status", "Active");
                            myCommand1.Parameters.AddWithValue("@suggestions", suggestionAction);
                            myCommand1.Parameters.AddWithValue("@suggestions_id", ActionId);
                            myCommand1.Parameters.AddWithValue("@acknowledge_by", userid);

                            myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);


                            myCommand1.ExecuteNonQuery();
                        }
                    }
                }












                return Ok("Mitigation Action Updated successfully");
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



        //Insert New Suggestions in Acknowledge Table by CRC

        [Route("api/MitigationController/AddedSuggestions/{userid}/{AssessId}")]
        [HttpPost]
        public IActionResult AddedSuggestions([FromBody] List<string> suggestions, int userid, int AssessId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                int lastInsertedMitigationid;
                string InsertNewMitigation = "INSERT INTO mitigations(assessment_id, status, created_date) VALUES (@assessment_id, @status, @created_date); SELECT LAST_INSERT_ID();";
                using (MySqlCommand myCommand2 = new MySqlCommand(InsertNewMitigation, con))
                {
                    myCommand2.Parameters.AddWithValue("@status", "Active");
                    myCommand2.Parameters.AddWithValue("@assessment_id", AssessId);
                    myCommand2.Parameters.AddWithValue("@created_date", DateTime.Now);

                    lastInsertedMitigationid = Convert.ToInt32(myCommand2.ExecuteScalar());
                }



                foreach (string newsuggestions in suggestions)
                {
                    string InsertQuery = "INSERT INTO acknowledge_suggestions(mitigations_id, suggestions_id, suggestions, status, created_date, acknowledge_by) VALUES (@mitigations_id, @suggestions_id, @suggestions, @status, @created_date, @acknowledge_by)";
                    using (MySqlCommand myCommand1 = new MySqlCommand(InsertQuery, con))
                    {
                        myCommand1.Parameters.AddWithValue("@status", "Active");
                        myCommand1.Parameters.AddWithValue("@suggestions", newsuggestions);
                        myCommand1.Parameters.AddWithValue("@suggestions_id", 0);  // Assuming suggestions_id and mitigations_id are the same
                        myCommand1.Parameters.AddWithValue("@acknowledge_by", userid);
                        myCommand1.Parameters.AddWithValue("@mitigations_id", lastInsertedMitigationid);
                        myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);

                        myCommand1.ExecuteNonQuery();
                    }
                }

                return Ok("Mitigation Action Updated successfully");
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












        // Methods For Mitigation Controls 

        //Assessment Result

        //Scheduled Assessment

        //Insert mitigation data and insert Suggestions if mitigation plan needed (Assessor)



        [Route("api/MitigationController/InsertSuggestionsAndMitigationId")]
        [HttpPost]
        public IActionResult InsertSuggestionsAndMitigationId([FromBody] MitigationCreatingModel MitigationCreatingModels)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "INSERT INTO mitigations (status, created_date, action_required, assessment_id,ass_template_id,uq_ass_schid,overallremarks,AssessmentStatus,proposed_by) VALUES (@status, @created_date, @action_required, @assessment_id,@ass_template_id,@uq_ass_schid,@overallremarks,@AssessmentStatus,@proposed_by)";


            try
            {
                con.Open();
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@status", "Pending");
                    myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@action_required", MitigationCreatingModels.action_required);
                    myCommand1.Parameters.AddWithValue("@assessment_id", MitigationCreatingModels.assessment_id);
                    //myCommand1.Parameters.AddWithValue("@ass_template_id", MitigationCreatingModels.ass_template_id);              
                    myCommand1.Parameters.AddWithValue("@uq_ass_schid", MitigationCreatingModels.uq_ass_schid);
                    myCommand1.Parameters.AddWithValue("@ass_template_id", MitigationCreatingModels.ass_template_id);
                    myCommand1.Parameters.AddWithValue("@overallremarks", MitigationCreatingModels.overallremarks);
                    myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Commented");
                    myCommand1.Parameters.AddWithValue("@proposed_by", MitigationCreatingModels.proposed_by);
                    //    myCommand1.Parameters.AddWithValue("@action_status", "sendRC");




                    myCommand1.ExecuteNonQuery();

                    // Get the last inserted primary key value
                    if (MitigationCreatingModels.suggestions != null && MitigationCreatingModels.suggestions.Length > 0)

                    {


                        int mitigationId = Convert.ToInt32(myCommand1.LastInsertedId.ToString());




                        foreach (string suggestion in MitigationCreatingModels.suggestions)
                        {
                            if (suggestion != null)
                            {

                                string insertQuery1 = "insert into suggestions_tbl(mitigations_id,suggestions,suggested_by,created_date,status,acknowledge)values(@mitigations_id,@suggestions,@suggested_by,@created_date,@status,@acknowledge)";


                                using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery1, con))
                                {
                                    myCommand2.Parameters.AddWithValue("@mitigations_id", mitigationId);
                                    myCommand2.Parameters.AddWithValue("@suggestions", suggestion);
                                    myCommand2.Parameters.AddWithValue("@suggested_by", MitigationCreatingModels.suggested_by);
                                    myCommand2.Parameters.AddWithValue("@created_date", DateTime.Now);
                                    myCommand2.Parameters.AddWithValue("@status", "Commented");
                                    myCommand2.Parameters.AddWithValue("@acknowledge", null);
                                    myCommand2.ExecuteNonQuery();
                                }
                            }
                        }


                    }

                    foreach (object indicators in MitigationCreatingModels.scoreindicators)
                    {
                        if (indicators != null)
                        {
                            var indicator = JsonConvert.DeserializeObject<dynamic>(indicators.ToString());

                            MySqlDataAdapter cd = new MySqlDataAdapter("SELECT * FROM score_indicator WHERE Score_Name='" + indicator.scoreIndicator.ToString() + "'", con);
                            DataTable dtt = new DataTable();
                            cd.Fill(dtt);
                            if (dtt.Rows.Count > 0)
                            {

                                MySqlCommand cmd = new MySqlCommand(@"select Distinct Percentage,Scheduled_Ass_StatusID ,(SELECT Score_Name 
FROM score_indicator 
WHERE    Score_Name=@Score_Name and Percentage BETWEEN scoreminrange AND scoremaxrange 
LIMIT 1 ) as ScoreIndicator,(SELECT Score_id 
FROM score_indicator 
WHERE Percentage BETWEEN scoreminrange AND scoremaxrange
LIMIT 1) as Score_id  from scheduled_ass_status  where  (Percentage BETWEEN @scoreminrange AND @scoremaxrange) AND AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid", con);

                                cmd.CommandType = CommandType.Text;

                                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                                cmd.Parameters.AddWithValue("@AssessementTemplateID", MitigationCreatingModels.ass_template_id);
                                cmd.Parameters.AddWithValue("@uq_ass_schid", MitigationCreatingModels.uq_ass_schid);
                                cmd.Parameters.AddWithValue("@Score_Name", indicator.scoreIndicator.ToString());
                                cmd.Parameters.AddWithValue("@scoreminrange", Convert.ToInt32(dtt.Rows[0]["scoreminrange"]));
                                cmd.Parameters.AddWithValue("@scoremaxrange", Convert.ToInt32(dtt.Rows[0]["scoremaxrange"]));
                                DataTable dt = new DataTable();
                                da.Fill(dt);
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    int Scheduled_Ass_StatusID = Convert.ToInt32(dt.Rows[i]["Scheduled_Ass_StatusID"].ToString());


                                    string update = (@"update scheduled_ass_status set key_Impr_Indicator_id=@key_Impr_Indicator_id,scoreIndicator=@scoreIndicator,Remarks=@Remarks,Status=@Status where Scheduled_Ass_StatusID=@Scheduled_Ass_StatusID ");
                                    using (MySqlCommand myCommand12 = new MySqlCommand(update, con))
                                    {
                                        int keyImprIndicatorId;
                                        myCommand12.Parameters.AddWithValue("@key_Impr_Indicator_id",
                                            int.TryParse(indicator.key_Impr_Indicator_Name?.ToString(), out keyImprIndicatorId) ? (object)keyImprIndicatorId : DBNull.Value);
                                        myCommand12.Parameters.AddWithValue("@scoreIndicator",
                                            indicator.scoreIndicator == null ? (object)DBNull.Value : indicator.scoreIndicator.ToString());
                                        myCommand12.Parameters.AddWithValue("@Remarks",
                                            indicator.Remarks == null ? (object)DBNull.Value : indicator.Remarks.ToString());

                                        myCommand12.Parameters.AddWithValue("@Scheduled_Ass_StatusID", Scheduled_Ass_StatusID);
                                        myCommand12.Parameters.AddWithValue("@Status", "Result Published");
                                        myCommand12.ExecuteNonQuery();


                                    }

                                    // Get UserID
                                    int userId = 0;
                                    string getUserQuery = "SELECT UserID FROM scheduled_ass_status WHERE Scheduled_Ass_StatusID = @Scheduled_Ass_StatusID";
                                    using (MySqlCommand getUserCmd = new MySqlCommand(getUserQuery, con))
                                    {
                                        getUserCmd.Parameters.AddWithValue("@Scheduled_Ass_StatusID", Scheduled_Ass_StatusID);
                                        var userResult = getUserCmd.ExecuteScalar();
                                        if (userResult != null)
                                        {
                                            userId = Convert.ToInt32(userResult);
                                        }
                                    }

                                    // Get User Email
                                    string userEmail = string.Empty;
                                    string getEmailQuery = "SELECT emailid FROM tbluser WHERE USR_ID = @UserID";
                                    using (MySqlCommand getEmailCmd = new MySqlCommand(getEmailQuery, con))
                                    {
                                        getEmailCmd.Parameters.AddWithValue("@UserID", userId);
                                        var emailResult = getEmailCmd.ExecuteScalar();
                                        if (emailResult != null)
                                        {
                                            userEmail = emailResult.ToString();
                                        }
                                    }

                                    // Get Assessment Template Name
                                    string templateName = string.Empty;
                                    string getTemplateQuery = @"
    SELECT abv.assessment_name 
    FROM scheduled_ass_status sas
    INNER JOIN assessment_builder_versions abv ON abv.ass_template_id = sas.AssessementTemplateID
    WHERE sas.Scheduled_Ass_StatusID = @Scheduled_Ass_StatusID";
                                    using (MySqlCommand getTemplateCmd = new MySqlCommand(getTemplateQuery, con))
                                    {
                                        getTemplateCmd.Parameters.AddWithValue("@Scheduled_Ass_StatusID", Scheduled_Ass_StatusID);
                                        var tempResult = getTemplateCmd.ExecuteScalar();
                                        if (tempResult != null)
                                        {
                                            templateName = tempResult.ToString();
                                        }
                                    }
                                    int senderid = MitigationCreatingModels.suggested_by;
                                    var request = HttpContext.Request;
                                    string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                                    obj_Clsmail.Assessmentresultpublish(userEmail, templateName, senderid, userId, baseUrl);

                                }
                            }
                        }
                    }



                }


                return Ok("Suggestions added successfully");
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






        //Management Monitored Assessment Grid

        //Get Assessments Data for selection grid (Management)


        [Route("api/MitigationController/GetAssessmentdata")]
        [HttpGet]
        public IEnumerable<AssSchedulemitigationModel> GetAssessmentdata()
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select distinct m.mitigations_id,m.ass_template_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
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
        stbl.TrackerID,
        (select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid ) as verson_no
      
 from risk.assessment_builder_versions ab
 
   left JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
left JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
left JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
left JOIN 
    risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id
left JOIN	
   risk.scheduled_ass_status sas on sas.AssessementTemplateID=ab.ass_template_id
inner JOIN risk.mitigations m on m.uq_ass_schid=sa.uq_ass_schid 
inner JOIN	
   risk.suggestions_tbl stbl on stbl.mitigations_id=m.mitigations_id
   
   where stbl.TrackerID is null and stbl.status='Commented' AND sa.verson_no = ab.verson_no  and m.status='pending' 
   
GROUP BY stbl.TrackerID, m.mitigations_id,m.ass_template_id,m.uq_ass_schid", con);
            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssSchedulemitigationModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssSchedulemitigationModel
                    {

                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        mitigations_id = Convert.ToInt32(dt.Rows[i]["mitigations_id"]),
                        TrackerID = dt.Rows[i]["TrackerID"].ToString(),
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
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }




        [Route("api/MitigationController/GetTimeBarchatDetails")]
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
                         from  scheduled_ass_status where  uq_ass_schid=@uq_ass_schid and AssessementTemplateID=@AssessementTemplateID AND UserID=mapped_user)  AS Days 
                         from schedule_assessment 

            where schedule_assessment.uq_ass_schid=@uq_ass_schid  and ass_template_id=@AssessementTemplateID ) as tbl
            group by days order by days  ", con);
            
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





        //Get Assessments List by mitigation id 


        [Route("api/MitigationController/GetSuggestionListById/{id}")]
        [HttpGet]

        public IEnumerable<suggestionsModelforView> GetSuggestionListById(int id)
        {


            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
    st.acknowledge,
  st.TrackerID,
m.status,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
LEFT JOIN
    risk.mitigations AS m ON m.mitigations_id = st.mitigations_id
WHERE
   st.mitigations_id = " + id + " and st.TrackerID is null and m.status='pending' GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
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
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString()
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




        //Insert New Suggestions by Management

        [Route("api/MitigationController/insertManagementSuggestionsList")]
        [HttpPost]
        public IActionResult insertManagementSuggestionsList([FromBody] suggestionsModel suggestionsModels)
        {
            try
            {
                   
                // Proceed with the insertion
                var suggestionsModel = this.mySqlDBContext.suggestionsModels;
                suggestionsModel.Add(suggestionsModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                suggestionsModels.created_date = dt;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                suggestionsModels.status = "Commented";



                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {

                // Handle other database update exceptions
                return BadRequest($"Error: {ex.Message}");

            }
        }


        //Update existed suggestions (management)


        [Route("api/MitigationController/UpdateSuggestionManagement")]
        [HttpPut]
        public IActionResult UpdateSuggestionManagement([FromBody] suggestionsModel suggestionsModels)
        {
            try
            {


                var existingEntity = this.mySqlDBContext.suggestionsModels.Find(suggestionsModels.suggestions_id);
                if (existingEntity == null)
                {
                    return NotFound("Entity not found");
                }


                // Update only non-null and non-zero properties
                var entry = this.mySqlDBContext.Entry(existingEntity);
                Type type = typeof(suggestionsModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    var newValue = property.GetValue(suggestionsModels);
                    var existingValue = property.GetValue(existingEntity);

                    // Check if the new value is not null and not equal to the existing value
                    if (newValue != null && !newValue.Equals(existingValue))
                    {
                        entry.Property(property.Name).CurrentValue = newValue;
                    }
                }

                this.mySqlDBContext.SaveChanges();
                return Ok("Update successful");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


        //Submit Button to update status of the mitigation status


        [Route("api/MitigationController/UpdateMitigationStatus/{mitigations_id}")]
        [HttpPut]
        public IActionResult UpdateMitStatus(int mitigations_id)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {
                con.Open();







                string updateQuery = "UPDATE mitigations SET status=@status WHERE mitigations_id=@mitigations_id";

                using (MySqlCommand myCommand1 = new MySqlCommand(updateQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@mitigations_id", mitigations_id);


                    myCommand1.Parameters.AddWithValue("@status", "Active");


                    myCommand1.ExecuteNonQuery();
                }








                return Ok("Mitigation Action  Status Updated successfully");
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






        //MONITORED ASSESSMENT





        //Get Assessment data for CRC  (Dropdown Grid) selection.




        [Route("api/MitigationController/GetMitigatedScheduleAssesment")]
        [HttpGet]
        public IEnumerable<AssSchedulemitigationModel> GetCompletedAssesment()
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select distinct m.mitigations_id ,m.ass_template_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
    ANY_VALUE(ab.assessment_name) AS assessment_name,
    ANY_VALUE(ab.assessment_description) AS assessment_description,
    DATE(ANY_VALUE(sa.created_date)) AS created_date,
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
        stbl.TrackerID,  (select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no
      
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
   
   where  (stbl.TrackerID IS NULL and m.status = 'active') and sa.verson_no=ab.verson_no 
    AND (stbl.status = 'Commented' OR TIMESTAMPDIFF(HOUR, m.created_date, NOW()) > 24)
   
GROUP BY stbl.TrackerID, m.mitigations_id,m.ass_template_id", con);


            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssSchedulemitigationModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssSchedulemitigationModel
                    {

                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        mitigations_id = Convert.ToInt32(dt.Rows[i]["mitigations_id"]),
                        TrackerID = dt.Rows[i]["TrackerID"].ToString(),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        Competency_id = Convert.ToInt32(dt.Rows[i]["Competency_id"]),
                         ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        verson_no =  dt.Rows[i]["verson_no"].ToString(),
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
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }




        //Get Template Id by mitigation id 



        [Route("api/MitigationController/GetTemplateIdmitigationid/{id}")]
        [HttpGet]
        public IEnumerable<AssSchedulemitigationModel> GetTemplateIdmitigationid(int id)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"


   select distinct m.mitigations_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
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
       ANY_VALUE(sas.EndDateTime) as EndDateTime 
 from assessment_builder ab
 
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
   
JOIN risk.mitigations m on m.assessment_id=sa.Schedule_Assessment_id WHERE m.mitigations_id='" + id + "' GROUP BY m.mitigations_id;", con);


            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssSchedulemitigationModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssSchedulemitigationModel
                    {

                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        mitigations_id = Convert.ToInt32(dt.Rows[i]["mitigations_id"]),
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
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }





        //Get All Suggestions data for CRC (Grid)





        [Route("api/MitigationController/GetSuggestionsList/{id}")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> GetSuggestionsList(int id)
        {


            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
    st.acknowledge,
  st.TrackerID,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
LEFT JOIN
    risk.mitigations AS m ON m.mitigations_id = st.mitigations_id

WHERE
   st.mitigations_id = " + id + " and st.TrackerID is null and st.status!='inactive' and (m.status='active' OR TIMESTAMPDIFF(HOUR, m.created_date, NOW()) > 24)  GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
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
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString()
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




        //Insert new suggestion for CRC (Grid)




        [Route("api/MitigationController/insertSuggestionsList/{id}")]
        [HttpPost]
        public IActionResult insertSuggestionsList([FromBody] suggestionsModel suggestionsModels, int id)
        {
            try
            {

                // Proceed with the insertion
                var suggestionsModel = this.mySqlDBContext.suggestionsModels;
                suggestionsModel.Add(suggestionsModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                suggestionsModels.created_date = dt;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                suggestionsModels.status = "Commented";
                // suggestionsModels.acknowledge = 1;
                suggestionsModels.suggested_by = id;

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {

                // Handle other database update exceptions
                return BadRequest($"Error: {ex.Message}");

            }
        }



        //Update existed suggestion for CRC (Grid)




        [Route("api/MitigationController/UpdateSuggestionsList")]
        [HttpPut]
        public IActionResult UpdateSuggestionsList([FromBody] suggestionsModel suggestionsModels)
        {
            try
            {


                var existingEntity = this.mySqlDBContext.suggestionsModels.Find(suggestionsModels.suggestions_id);
                //if (existingEntity == null)
                //{
                //    return NotFound("Entity not found");
                //}
                if (suggestionsModels.notify_management == 1 && suggestionsModels.action_required == 1 && suggestionsModels.acknowledge == 1)
                {
                    suggestionsModels.status = "Processing";
                }
                else if (suggestionsModels.assign_responsibility != null && suggestionsModels.action_required == 1 && suggestionsModels.acknowledge == 1)
                {
                    suggestionsModels.status = "Assigned";
                }
                else
                {
                    suggestionsModels.status = "Commented";
                }

                // Update only non-null and non-zero properties
                var entry = this.mySqlDBContext.Entry(existingEntity);
                Type type = typeof(suggestionsModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    var newValue = property.GetValue(suggestionsModels);
                    var existingValue = property.GetValue(existingEntity);

                    // Check if the new value is not null and not equal to the existing value
                    if (newValue != null && !newValue.Equals(existingValue))
                    {
                        entry.Property(property.Name).CurrentValue = newValue;
                    }
                }

                this.mySqlDBContext.SaveChanges();
                if (suggestionsModels.status == "Assigned" || suggestionsModels.status == "Processing")
                {
                    int userId = suggestionsModels.assign_responsibility ?? 0;
                    int mitigationId = suggestionsModels.mitigations_id ?? 0;
                    string toEmail = string.Empty;
                    string assessmentName = string.Empty;
                    int senderId = 0;

                    using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                    {
                        con.Open();

                        string query = @"
            SELECT u.emailid, ab.assessment_name, s.suggested_by
            FROM suggestions_tbl s
            INNER JOIN mitigations m ON s.mitigations_id = m.mitigations_id
            INNER JOIN assessment_builder_versions ab ON m.ass_template_id = ab.ass_template_id
            INNER JOIN tbluser u ON u.USR_ID = @userId
            WHERE s.suggestions_id = @suggestionsId
        ";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@suggestionsId", suggestionsModels.suggestions_id);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    toEmail = reader["emailid"].ToString();
                                    assessmentName = reader["assessment_name"].ToString();
                                    senderId = Convert.ToInt32(reader["suggested_by"]);
                                }
                            }
                        }

                        // Only proceed if all values were fetched correctly
                        if (!string.IsNullOrEmpty(toEmail) && !string.IsNullOrEmpty(assessmentName) && senderId > 0)
                        {
                            var request = HttpContext.Request;
                            string baseUrl = $"{request.Scheme}://{request.Host}";

                            // Call your mail function with the relevant values
                            obj_Clsmail.suggestiontoprocessowner(toEmail, assessmentName, senderId, userId, baseUrl);
                        }
                    }
                }

                return Ok("Update successful");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


        //Delete existed suggestion for CRC (Grid)


        [Route("api/MitigationController/deleteSuggestionsList")]
        [HttpDelete]
        public void deleteSuggestionsList(int id)
        {
            var currentClass = new suggestionsModel { suggestions_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Get Unacknowledge suggestions data for new mitigation action CRC (Grid) for more mitigation action required




        [Route("api/MitigationController/UnAckGetSuggestionsList/{id}")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> UnAckGetSuggestionsList(int id)
        {

            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
  
    st.acknowledge, st.TrackerID,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
LEFT JOIN
    risk.mitigations AS m ON m.mitigations_id = st.mitigations_id
WHERE
   st.mitigations_id = " + id + " and st.TrackerID is null and st.status='commented' and (m.status='active' OR TIMESTAMPDIFF(HOUR, m.created_date, NOW()) > 24) GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
                            {
                                suggestions_id = row["suggestions_id"] != DBNull.Value ? Convert.ToInt32(row["suggestions_id"]) : (int?)null,
                                mitigations_id = row["mitigations_id"] != DBNull.Value ? Convert.ToInt32(row["mitigations_id"]) : (int?)null,
                                suggestions = row["suggestions"].ToString(),
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
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString()
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


        //Update trackerids as 0 if select no more mitigation action required CRC (Grid)


        [Route("api/MitigationController/UpdateTrackerID/{mitigationId}")]
        [HttpPost]
        public IActionResult UpdateTrackerID([FromBody] MitigationModel MitigationModels, int mitigationId)
        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();



                var suggestionsid = new List<int>();



                MySqlCommand cmd = new MySqlCommand("SELECT suggestions_id FROM suggestions_tbl WHERE mitigations_id=@mitigationId and TrackerID is null", con);
                cmd.Parameters.AddWithValue("@mitigationId", mitigationId); // Replace yourMitigationIdVariable with the actual value

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suggestionsid.Add(reader.GetInt32(0));
                    }
                }


                foreach (var suggestion in suggestionsid)
                {


                    string updateTemplateId = "UPDATE suggestions_tbl SET TrackerID=@TrackerID WHERE suggestions_id=@suggestions_id";



                    using (MySqlCommand myCommand1 = new MySqlCommand(updateTemplateId, con))
                    {
                        myCommand1.Parameters.AddWithValue("@suggestions_id", suggestion);

                        myCommand1.Parameters.AddWithValue("@TrackerID", '0');



                        myCommand1.ExecuteNonQuery();
                    }


                }

                return Ok("Tracker id Updated successfully");
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




        //Generate tracker id for acknowledged tasks for submit button crc(grid)

        [Route("api/MitigationController/UpdateActionStatus/{mitigationId}")]
        [HttpPost]
        public IActionResult UpdateActionStatus([FromBody] MitigationModel MitigationModels, int mitigationId)
        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();



                var suggestionsid = new List<int>();

                Random random = new Random();

                MySqlCommand cmd = new MySqlCommand("SELECT suggestions_id FROM suggestions_tbl WHERE mitigations_id=@mitigationId AND acknowledge='1' and TrackerID is null", con);
                cmd.Parameters.AddWithValue("@mitigationId", mitigationId); // Replace yourMitigationIdVariable with the actual value

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suggestionsid.Add(reader.GetInt32(0));
                    }
                }





                int GenerateUniqueRandomNumber()
                {
                    int randomNumber;
                    do
                    {
                        randomNumber = random.Next(100, 1000); // Generate a random 3-digit number
                        MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(*) FROM suggestions_tbl WHERE TrackerID = @TrackerID", con);
                        checkCmd.Parameters.AddWithValue("@TrackerID", randomNumber);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count == 0)
                        {
                            break;
                        }
                    } while (true);

                    return randomNumber;
                }

                int trackerID = GenerateUniqueRandomNumber();

                foreach (var suggestion in suggestionsid)
                {


                    string updateTemplateId = "UPDATE suggestions_tbl SET TrackerID=@TrackerID WHERE suggestions_id=@suggestions_id";



                    using (MySqlCommand myCommand1 = new MySqlCommand(updateTemplateId, con))
                    {
                        myCommand1.Parameters.AddWithValue("@suggestions_id", suggestion);

                        myCommand1.Parameters.AddWithValue("@TrackerID", trackerID);



                        myCommand1.ExecuteNonQuery();
                    }


                }

                string resetTrackerIdQuery = "UPDATE suggestions_tbl SET TrackerID = NULL WHERE mitigations_id = @mitigationId AND acknowledge IS NULL";

                using (MySqlCommand resetCmd = new MySqlCommand(resetTrackerIdQuery, con))
                {
                    resetCmd.Parameters.AddWithValue("@mitigationId", mitigationId);
                    resetCmd.ExecuteNonQuery();
                }


                return Ok("Mitigation Action  Status Updated successfully");
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




        //Management Inputs 


        //Get Assessment data for Management  (Dropdown Grid) selection.



        [Route("api/MitigationController/GetMitigationListManagement")]
        [HttpGet]
        public IEnumerable<AssSchedulemitigationModel> GetMitigationListManagement()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select distinct m.mitigations_id,m.ass_template_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
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
        stbl.TrackerID, (select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no
      
 from assessment_builder ab
 
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
   
   where stbl.TrackerID is not null and stbl.status='Processing' and sa.verson_no=ab.verson_no 
   
GROUP BY stbl.TrackerID, m.mitigations_id", con);


            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssSchedulemitigationModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssSchedulemitigationModel
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
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }




        //Get Mitigation ID AND Template ID by tracker id 

        [Route("api/MitigationController/GetTemplateId/{id}")]
        [HttpGet]
        public IEnumerable<AssSchedulemitigationModel> GetTemplateId(string id)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            int mitigationId = -1; // Initialize with a default value

            // Create MySqlConnection and MySqlCommand


            // SQL query to select mitigations_id from suggestions_tbl where TrackerID matches the parameter
            string sql = "SELECT mitigations_id FROM suggestions_tbl WHERE TrackerID=@TrackerID";

            // Create MySqlCommand object with the SQL query and connection
            using (MySqlCommand cmd1 = new MySqlCommand(sql, con))
            {
                // Add parameter to the query and set its value
                cmd1.Parameters.AddWithValue("@TrackerID", id);

                // Execute the query
                object result = cmd1.ExecuteScalar();

                // Check if the result is not null and convert it to an integer
                if (result != null && result != DBNull.Value)
                {
                    mitigationId = Convert.ToInt32(result);
                }
            }





            MySqlCommand cmd = new MySqlCommand(@"


   select distinct m.mitigations_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
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
       ANY_VALUE(sas.EndDateTime) as EndDateTime 
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
   
JOIN risk.mitigations m on m.assessment_id=sa.Schedule_Assessment_id WHERE m.mitigations_id='" + mitigationId + "' GROUP BY m.mitigations_id;", con);


            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssSchedulemitigationModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssSchedulemitigationModel
                    {

                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        mitigations_id = Convert.ToInt32(dt.Rows[i]["mitigations_id"]),
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
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }









        //Get All Tasks data where status is pending  management (Grid)



        [Route("api/MitigationController/GetAssessmentsListManagement/{id}")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> GetAssessmentsListManagement(string id)
        {

            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
    st.acknowledge,
  st.TrackerID,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
WHERE
   st.TrackerID = " + id + " and st.status='Processing' GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
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

                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString()
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


        //Insert tasks by management (Grid)



        [Route("api/MitigationController/insertTasksManagement/{id}/{trackerid}")]
        [HttpPost]
        public IActionResult insertTasksManagement([FromBody] suggestionsModel suggestionsModels, int id, string trackerid)
        {
            try
            {

                // Proceed with the insertion
                var suggestionsModel = this.mySqlDBContext.suggestionsModels;
                suggestionsModel.Add(suggestionsModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                suggestionsModels.created_date = dt;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                suggestionsModels.status = "Processing";
                suggestionsModels.acknowledge = 1;
                suggestionsModels.notify_management = 1;
                suggestionsModels.suggested_by = id;
                suggestionsModels.TrackerID = trackerid;
                suggestionsModels.action_required = 1;
                suggestionsModels.input_date = dt;


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {

                // Handle other database update exceptions
                return BadRequest($"Error: {ex.Message}");

            }
        }







        //Update tasks change status is assigned for management (Grid)




        [Route("api/MitigationController/UpdateTaskListManagement")]
        [HttpPut]
        public IActionResult UpdateTaskListManagement([FromBody] suggestionsModel suggestionsModels)
        {
            try
            {


                var existingEntity = this.mySqlDBContext.suggestionsModels.Find(suggestionsModels.suggestions_id);
                if (existingEntity == null)
                {
                    return NotFound("Entity not found");
                }

                else if (suggestionsModels.assign_responsibility != null)
                {
                    suggestionsModels.status = "Processing";
                }

                // Update only non-null and non-zero properties
                var entry = this.mySqlDBContext.Entry(existingEntity);
                Type type = typeof(suggestionsModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    var newValue = property.GetValue(suggestionsModels);
                    var existingValue = property.GetValue(existingEntity);

                    // Check if the new value is not null and not equal to the existing value
                    if (newValue != null && !newValue.Equals(existingValue))
                    {
                        entry.Property(property.Name).CurrentValue = newValue;
                    }
                }

                this.mySqlDBContext.SaveChanges();
                return Ok("Update successful");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }







        // Submit button for management 





        [Route("api/MitigationController/UpdateTasksStatus")]
        [HttpPost]
        public IActionResult UpdateTasksStatus([FromBody] suggestionsModel suggestionsModels)
        {




            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);



            try
            {


                var existingEntity = this.mySqlDBContext.suggestionsModels.Find(suggestionsModels.suggestions_id);
                //if (existingEntity == null)
                //{
                //    return NotFound("Entity not found");
                //}


                suggestionsModels.status = "Assigned";


                // Update only non-null and non-zero properties
                var entry = this.mySqlDBContext.Entry(existingEntity);
                Type type = typeof(suggestionsModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    var newValue = property.GetValue(suggestionsModels);
                    var existingValue = property.GetValue(existingEntity);

                    // Check if the new value is not null and not equal to the existing value
                    if (newValue != null && !newValue.Equals(existingValue))
                    {
                        entry.Property(property.Name).CurrentValue = newValue;
                    }
                }

                this.mySqlDBContext.SaveChanges();
                return Ok("Update successful");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }










            //try
            //{
            //    con.Open();



            //    var suggestionsid = new List<int>();



            //    MySqlCommand cmd = new MySqlCommand("SELECT suggestions_id FROM suggestions_tbl WHERE TrackerID=@TrackerID and status='processing'", con);
            //    cmd.Parameters.AddWithValue("@TrackerID", id); // Replace yourMitigationIdVariable with the actual value

            //    using (MySqlDataReader reader = cmd.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            suggestionsid.Add(reader.GetInt32(0));
            //        }
            //    }


            //    foreach (var suggestion in suggestionsid)
            //    {


            //        string updateTrackerId = "UPDATE suggestions_tbl SET status=@status WHERE suggestions_id=@suggestions_id";



            //        using (MySqlCommand myCommand1 = new MySqlCommand(updateTrackerId, con))
            //        {
            //            myCommand1.Parameters.AddWithValue("@suggestions_id", suggestion);

            //            myCommand1.Parameters.AddWithValue("@status", "Assigned");



            //            myCommand1.ExecuteNonQuery();
            //        }


            //    }

            //    return Ok("Status Updated successfully");
            //}



            //catch (Exception ex)
            //{
            //    return BadRequest($"Error: {ex.Message}");
            //}
            //finally
            //{
            //    con.Close();
            //}


        }






        //My Mitigation Tasks 


        // Get Assessment data for PO  (Dropdown Grid) selection.


        [Route("api/MitigationController/GetAssessmentList/{userId}")]
        [HttpGet]
        public IEnumerable<AssSchedulemitigationModel> GetAssessmentList(int userId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select distinct m.mitigations_id,m.ass_template_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
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
        stbl.TrackerID, (select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no     
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
   
   where stbl.TrackerID is not null and stbl.status='Assigned' and stbl.assign_responsibility=@assign_responsibility and sa.verson_no=ab.verson_no 
   
GROUP BY stbl.TrackerID, m.mitigations_id,stbl.assign_responsibility", con);

            cmd.Parameters.AddWithValue("assign_responsibility", userId);

            cmd.CommandType = CommandType.Text;

           
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssSchedulemitigationModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssSchedulemitigationModel
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
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }

        //Get Tasks data for PO (Grid)

        [Route("api/MitigationController/GetAllTaskData/{id}")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> GetAllTaskData(string id)
        {

            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
    st.acknowledge,
  st.TrackerID,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
WHERE
   st.TrackerID = " + id + " and st.status='Assigned' GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
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
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString()
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

        //Get Mitigation TasksNew
        [Route("api/MitigationController/GetAllTaskDatabyUser")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> GetAllTaskDatabyUser(string id,int UserId)
        {

            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
    st.acknowledge,
   
st.management_remarks,
  st.TrackerID,
mitigations.uq_ass_schid,tn.Type_Name,sn.SubType_Name, cn.Competency_Name,assessment_builder_versions.assessment_name,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
LEFT JOIN
    risk.mitigations AS mitigations ON mitigations.mitigations_id = st.mitigations_id
LEFT JOIN schedule_assessment AS schedule_assessment on mitigations.uq_ass_schid = schedule_assessment.uq_ass_schid 
LEFT JOIN assessment_builder_versions AS assessment_builder_versions on schedule_assessment.ass_template_id  = assessment_builder_versions.ass_template_id
  LEFT JOIN risk.sub_type sn ON sn.SubType_id = assessment_builder_versions.SubType_id
LEFT JOIN risk.type tn ON tn.Type_id = assessment_builder_versions.Type_id
LEFT JOIN risk.competency_skill cn ON cn.Competency_id = assessment_builder_versions.Competency_id

WHERE
   st.TrackerID = " + id + " and st.assign_responsibility=" + UserId + " and st.status='Assigned' GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge,tn.Type_Name,sn.SubType_Name, cn.Competency_Name,assessment_builder_versions.assessment_name", con)
                    {

                    };
                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
                            {
                                uq_ass_schid = row["uq_ass_schid"].ToString(),
                                SubType_Name = row["SubType_Name"].ToString(),
                                Competency_Name = row["Competency_Name"].ToString(),
                                Type_Name = row["Type_Name"].ToString(),
                                aassessmentname = row["assessment_name"].ToString(),
                                suggestions_id = row["suggestions_id"] != DBNull.Value ? Convert.ToInt32(row["suggestions_id"]) : (int?)null,
                                mitigations_id = row["mitigations_id"] != DBNull.Value ? Convert.ToInt32(row["mitigations_id"]) : (int?)null,
                                suggestions = row["suggestions"].ToString(),
                                TrackerID = row["TrackerID"].ToString(),
                                status = row["status"].ToString(),
                               // assessment_name = row["assessment_name"].ToString(),
                                created_date = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                suggested_by = row["suggested_by"] != DBNull.Value ? Convert.ToInt32(row["suggested_by"]) : (int?)null,
                                remarks = row["remarks"].ToString(),
                                management_remarks = row["management_remarks"]!= DBNull.Value ? row["management_remarks"].ToString() : "N/A",
                                acknowledge_by = row["acknowledge_by"] != DBNull.Value ? Convert.ToInt32(row["acknowledge_by"]) : (int?)null,
                                action_required = row["action_required"] != DBNull.Value ? Convert.ToInt32(row["action_required"]) : (int?)null,
                                notify_management = row["notify_management"] != DBNull.Value ? Convert.ToInt32(row["notify_management"]) : (int?)null,
                                input_date = row["input_date"] != DBNull.Value ? Convert.ToDateTime(row["input_date"]) : (DateTime?)null,
                                assign_responsibility = row["assign_responsibility"] != DBNull.Value ? Convert.ToInt32(row["assign_responsibility"]) : (int?)null,
                                tentative_timeline = row["tentative_timeline"] != DBNull.Value ? Convert.ToDateTime(row["tentative_timeline"]) : (DateTime?)null,
                                suggested_documents = row["suggested_documents"].ToString(),
                                action_priority = row["action_priority"] != DBNull.Value ? Convert.ToInt32(row["action_priority"]) : (int?)null,
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString()
                            }); ; ;
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





        //Update Remarks and suggestion files for selected task (PO)

        [Route("api/MitigationController/updateTaskSuggestions")]
        [HttpPost]

        public async Task<IActionResult> UpdatefiletaskStatus([FromForm] suggestionmodel suggestionsModels)
        {



            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);



            try
            {
                con.Open();



                if (!Directory.Exists(_uploadPath))
                {
                    Directory.CreateDirectory(_uploadPath);
                }

               
                if(suggestionsModels.file==null || suggestionsModels.file.Length==0)
                {
                    return BadRequest("File is not uploaded");
                }
                var httpContext= httpContextAccessor.HttpContext;
                var request=httpContext.Request;
                string baseUrl = $"{request.Scheme}://{request.Host}";

                string file_name = $"{suggestionsModels.file.FileName}";
                string file_path=Path.Combine(_uploadPath, file_name);

                using( var stream=new FileStream(file_path, FileMode.Create))  
                {
                    suggestionsModels.file.CopyToAsync(stream);
                }
                string fileUrl = $"{baseUrl}/Resources/MitigationFolder/{file_name}";







                string updateTrackerId = "UPDATE suggestions_tbl SET status = @status, PO_remarks = @PO_remarks, file_path = @file_path,file_name=@file_name ,completed_date=@completed_date WHERE suggestions_id = @suggestions_id";



                using (MySqlCommand myCommand1 = new MySqlCommand(updateTrackerId, con))
                {
                    myCommand1.Parameters.AddWithValue("@suggestions_id", suggestionsModels.suggestions_id);
                    myCommand1.Parameters.AddWithValue("@file_path", fileUrl);
                    myCommand1.Parameters.AddWithValue("@PO_remarks", suggestionsModels.PO_remarks);
                    myCommand1.Parameters.AddWithValue("@file_name", file_name);
                    myCommand1.Parameters.AddWithValue("@status", "completed");
                    myCommand1.Parameters.AddWithValue("@completed_date", DateTime.Now);



                    myCommand1.ExecuteNonQuery();
                }




                return Ok("Status Updated successfully");
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



        //Update Mitigation Action

        //Dropdown selection for CRC for Update assignees,timeline,suggestive files



        [Route("api/MitigationController/GetAssessmentsUpdate")]
        [HttpGet]
        public IEnumerable<AssSchedulemitigationModel> GetAssessmentsUpdate()
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select distinct m.mitigations_id,m.ass_template_id,
   ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
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
        stbl.TrackerID,
       (select distinct verson_no from schedule_assessment where uq_ass_schid= sa.uq_ass_schid) as verson_no
      
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
   where stbl.TrackerID is not null and (stbl.status='Processing' or stbl.status='Assigned' or stbl.status='Completed') and sa.verson_no=ab.verson_no 
   
GROUP BY stbl.TrackerID, m.mitigations_id", con);


            cmd.CommandType = CommandType.Text;


            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<AssSchedulemitigationModel>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string AssessementcompletedDate1 = "";
                    if (dt.Rows[i]["StartDateTime"] != DBNull.Value && dt.Rows[i]["EndDateTime"] != DBNull.Value)

                        AssessementcompletedDate1 = ((DateTime)dt.Rows[i]["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["EndDateTime"]).ToString("dd-MM-yyyy");
                    else
                        AssessementcompletedDate1 = "";
                    pdata.Add(new AssSchedulemitigationModel
                    {

                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]),
                        mitigations_id = Convert.ToInt32(dt.Rows[i]["mitigations_id"]),
                        TrackerID = dt.Rows[i]["TrackerID"].ToString(),
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        Competency_id = Convert.ToInt32(dt.Rows[i]["Competency_id"]),
                       ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        verson_no = dt.Rows[i]["verson_no"].ToString(),
                        assessment_description = dt.Rows[i]["assessment_description"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"]),
                        startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]),
                        keywords = dt.Rows[i]["keywords"].ToString(),
                        status = dt.Rows[i]["status"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        AssessementDueDate = ((DateTime)dt.Rows[i]["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)dt.Rows[i]["endDate"]).ToString("dd-MM-yyyy"),
                        AssessementcompletedDate = AssessementcompletedDate1.ToString(),
                    });
                }
            }
            return pdata;
        }


        //Updation Grid for Crc 


        [Route("api/MitigationController/UpdateBasedOnStatus/{id}")]
        [HttpGet]
        public IEnumerable<suggestionsModelforView> UpdateBasedOnStatus(string id)
        {

            List<suggestionsModelforView> suggestionsList = new List<suggestionsModelforView>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                try
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
    st.acknowledge,
  st.completed_date,
  st.management_remarks,
st.PO_remarks,
  st.TrackerID,
    MAX(tblSuggester.firstname) AS Suggester_Name,
    MAX(tblAcknowledger.firstname) AS Acknowledger_Name,
MAX(tblAssigner.firstname) AS AssignerName
FROM
    risk.suggestions_tbl AS st
LEFT JOIN
    risk.tbluser AS tblSuggester ON tblSuggester.usr_ID = st.suggested_by
LEFT JOIN
    risk.tbluser AS tblAcknowledger ON tblAcknowledger.usr_ID = st.acknowledge_by
LEFT JOIN
    risk.tbluser AS tblAssigner ON tblAssigner.usr_ID = st.assign_responsibility
WHERE
   st.TrackerID = " + id + " and (st.status='Processing' or st.status='Completed' or st.status='Assigned') GROUP BY st.suggestions_id,st.mitigations_id,st.suggestions,st.status,st.created_date,st.suggested_by,st.remarks,st.acknowledge_by,st.action_required,st.notify_management,st.input_date,st.assign_responsibility,st.tentative_timeline,st.suggested_documents,st.action_priority,st.acknowledge", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            suggestionsList.Add(new suggestionsModelforView
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
                                acknowledge = row["acknowledge"] != DBNull.Value ? Convert.ToInt32(row["acknowledge"]) : (int?)null,
                                Suggester_Name = row["Suggester_Name"].ToString(),
                                Acknowledger_Name = row["Acknowledger_Name"].ToString(),
                                AssignerName = row["AssignerName"].ToString(),
                                management_remarks = row["management_remarks"].ToString(),
                                PO_remarks = row["PO_remarks"].ToString(),
                                completed_date = row["completed_date"] != DBNull.Value ? Convert.ToDateTime(row["completed_date"]) : (DateTime?)null,

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



        //Update Selected Assignee and Tentative date 

        [Route("api/MitigationController/UpdateSuggestion")]
        [HttpPut]
        public IActionResult UpdateSuggestion([FromBody] suggestionsModel suggestionsModels)
        {
            try
            {


                var existingEntity = this.mySqlDBContext.suggestionsModels.Find(suggestionsModels.suggestions_id);





                // Update only non-null and non-zero properties
                var entry = this.mySqlDBContext.Entry(existingEntity);
                Type type = typeof(suggestionsModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    var newValue = property.GetValue(suggestionsModels);
                    var existingValue = property.GetValue(existingEntity);

                    // Check if the new value is not null and not equal to the existing value
                    if (newValue != null && !newValue.Equals(existingValue))
                    {
                        entry.Property(property.Name).CurrentValue = newValue;
                    }
                }

                this.mySqlDBContext.SaveChanges();
                return Ok("Update successful");
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


    }
}
