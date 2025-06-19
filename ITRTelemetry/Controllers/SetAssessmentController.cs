using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using MySqlConnector;
using Org.BouncyCastle.Bcpg;
using Microsoft.Extensions.Configuration;
using System.Data;
using DocumentFormat.OpenXml.Bibliography;
using Ubiety.Dns.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using MySqlX.XDevAPI.Common;
using NuGet.Protocol.Core.Types;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using ITR_TelementaryAPI;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class SetAssessmentController : ControllerBase
    {
        private ClsEmail obj_Clsmail = new ClsEmail();
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        public SetAssessmentController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;


        }


        [Route("api/SetAssessment/GetAssessementTemplatelist")]
        [HttpGet]
        public IEnumerable<AssessmentPublisherModel> GetAssessementTemplatelist()
        {
            return this.mySqlDBContext.AssessmentPublisherModels.Where(x => x.status == "Active").ToList();
        }




        [Route("api/ScheduleUser/GetAssessementScheduleUser")]
        [HttpGet]
        public IEnumerable<tableuser> GetAssessementScheduleUser()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
 SELECT tbl.USR_ID,tbl.firstname,tbl.emailid FROM 
 risk.tbluser tbl", con);




            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<tableuser>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new tableuser
                    {


                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        emailid = dt.Rows[i]["emailid"].ToString(),





                    });
                }

            }
            return pdata;
        }









        [Route("api/SetAssessment/GetActiveSchedules")]
        [HttpGet]
        public IEnumerable<RepeatFrequencyModel> GetActiveSchedules()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
    SELECT 
        sa.*,
        us.firstname
    FROM 
        risk.schedule_assessment sa
    JOIN 
        risk.tbluser us ON us.USR_ID = sa.userid
    WHERE 
        sa.status = 'Active'
    ORDER BY 
        sa.created_date", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<RepeatFrequencyModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new RepeatFrequencyModel
                    {

                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"].ToString()),


                        value_Frequency = dt.Rows[i]["value_Frequency"].ToString(),

                        startDate = dt.Rows[i]["startDate"].ToString(),
                        endDate = dt.Rows[i]["endDate"].ToString(),
                        //firstname = dt.Rows[i]["firstname"].ToString(),
                        // ass_template_id = Convert.ToInt32(dt.Rows[i]["ass_template_id"].ToString()),
                        // pagetype = dt.Rows[i]["pagetype"].ToString(),


                        Assessment_start_Date = Convert.ToDateTime(dt.Rows[i]["Assessment_start_Date"].ToString()),
                        repeatEndDate = dt.Rows[i]["repeatEndDate"].ToString(),

                        frequency_period = dt.Rows[i]["frequency_period"].ToString(),
                        Duration_of_Assessment = dt.Rows[i]["Duration_of_Assessment"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"].ToString()),
                        objective = dt.Rows[i]["objective"].ToString()




                    });
                }
            }
            return pdata;
        }






        [Route("api/SetAssessment/GetActiveScheduleAssessmentsByUser/{userId}")]
        [HttpGet]
        public IEnumerable<RepeatFrequencyModel> GetActiveScheduleAssessments(int userId)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT sa.uq_ass_schid, 
       MAX(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
       MAX(sa.value_Frequency) AS value_Frequency,
       MAX(sa.startDate) AS startDate,
       MAX(sa.endDate) AS endDate,
       MAX(sa.objective) AS objective,
       MAX(sa.message) AS message,
       MAX(sa.Shuffle_Questions) AS Shuffle_Questions,
       MAX(sa.Shuffle_Answers) AS Shuffle_Answers,
       MAX(sa.ass_template_id) AS ass_template_id,
       MAX(sa.repeatEndDate) AS repeatEndDate,
       MAX(sa.frequency_period) AS frequency_period,
       MAX(sa.Duration_of_Assessment) AS Duration_of_Assessment,
       MAX(sa.created_date) AS created_date,
       MAX(sa.Date_Of_Request) AS Date_Of_Request,
       MAX(sa.objective) AS objective,
       MAX(sa.pagetype) AS pagetype,
       MAX(sa.userid) AS userid,
       MAX(us.firstname) AS firstname,
       MAX(ab.assessment_name) AS assessment_name,
       MAX(sa.AssessmentStatus) AS status
FROM risk.schedule_assessment sa
JOIN risk.tbluser us ON us.USR_ID = sa.login_userid
LEFT JOIN risk.assessment_builder ab ON ab.ass_template_id = sa.ass_template_id
where sa.login_userid='" + userId + "' and sa.AssessmentStatus ='Assessment Scheduled'  AND sa.status = 'Active' GROUP BY sa.uq_ass_schid;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<RepeatFrequencyModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new RepeatFrequencyModel
                    {

                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"].ToString()),


                        value_Frequency = dt.Rows[i]["value_Frequency"].ToString(),

                        startDate = dt.Rows[i]["startDate"].ToString(),
                        endDate = dt.Rows[i]["endDate"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),

                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),

                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),

                        repeatEndDate = dt.Rows[i]["repeatEndDate"].ToString(),

                        frequency_period = dt.Rows[i]["frequency_period"].ToString(),
                        Duration_of_Assessment = dt.Rows[i]["Duration_of_Assessment"].ToString(),
                        created_date = Convert.ToDateTime(dt.Rows[i]["created_date"].ToString()),
                        Date_Of_Request = dt.Rows[i]["date_Of_Request"].ToString(),
                        objective = dt.Rows[i]["objective"].ToString(),
                        message = dt.Rows[i]["message"].ToString(),
                        pagetype = dt.Rows[i]["pagetype"].ToString(),
                        Shuffle_Questions = Convert.ToInt32(dt.Rows[i]["Shuffle_Questions"].ToString()),
                        Shuffle_Answers = Convert.ToInt32(dt.Rows[i]["Shuffle_Answers"].ToString()),
                        userid = dt.Rows[i]["userid"].ToString(),
                        status = dt.Rows[i]["status"].ToString()



                    });
                }
            }
            return pdata;
        }



        [Route("api/SetAssessment/GetActiveScheduleAssessmentsById/{uq_ass_schid}")]
        [HttpGet]


        public IEnumerable<RepeatFrequencyModelNew> GetActiveScheduleAssessmentsById(int uq_ass_schid)
        {
            var pdata = new List<RepeatFrequencyModelNew>();
            List<int> docCategoryIDs = new List<int>();
            List<int> docSubCategoryIDs = new List<int>();
            List<int> Department_Master_ids = new List<int>();

            StringBuilder docCategoryNames = new StringBuilder();
   
            StringBuilder docSubCategoryNames = new StringBuilder();
            StringBuilder Department_Master_name = new StringBuilder();
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(@"
     SELECT 
    sa.*,
    tb_requester.firstname AS requester_name,
    dm.DocTypeName,
   
    en.Entity_Master_Name,
    ul.Unit_location_Master_name,
  
    ab.assessment_name
FROM risk.schedule_assessment sa
JOIN risk.tbluser tb_requester ON tb_requester.usr_ID = sa.userid
LEFT JOIN risk.doctype_master dm ON dm.DocTypeID = sa.DocTypeID

LEFT JOIN risk.entity_master en ON en.Entity_Master_id = sa.Entity_Master_id
LEFT JOIN risk.unit_location_master ul ON ul.Unit_location_Master_id = sa.unit_location_Master_id

LEFT JOIN risk.assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id
WHERE sa.uq_ass_schid = @uq_ass_schid AND sa.status = 'Active'", con))
                {
                    cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                       

                        if (dt.Rows.Count > 0)
                        {


                            for (var i = 0; i < dt.Rows.Count; i++)
                            {

                                string[] categoryIDs = dt.Rows[i]["Doc_CategoryID"].ToString().Split(',');
                                docCategoryIDs.AddRange(categoryIDs.Select(id => Convert.ToInt32(id.Trim())));

                                // Handle Doc_SubCategoryID: split by commas, convert to integers, and add to the list
                                string[] subCategoryIDs = dt.Rows[i]["Doc_SubCategoryID"].ToString().Split(',');
                                docSubCategoryIDs.AddRange(subCategoryIDs.Select(id => Convert.ToInt32(id.Trim())));

                                string[] DepartmentMaster_id = dt.Rows[i]["Department_Master_id"].ToString().Split(',');
                                Department_Master_ids.AddRange(DepartmentMaster_id.Select(id => Convert.ToInt32(id.Trim())));
                            }
                            docCategoryIDs = docCategoryIDs.Distinct().ToList();
                            docSubCategoryIDs = docSubCategoryIDs.Distinct().ToList();
                            Department_Master_ids = Department_Master_ids.Distinct().ToList();

                            foreach (var docCategoryID in docCategoryIDs)
                                {
                                    string seleQ = "SELECT Doc_CategoryName FROM doccategory_master WHERE Doc_CategoryID = @Doc_CategoryID";
                                    using (MySqlCommand cmd1 = new MySqlCommand(seleQ, con))
                                    {
                                        cmd1.Parameters.AddWithValue("@Doc_CategoryID", docCategoryID);

                                        using (MySqlDataReader reader = cmd1.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                        // Add the retrieved name to the list
                                        if (docCategoryNames.Length > 0)
                                        {
                                            docCategoryNames.Append(", ");
                                        }
                                        docCategoryNames.Append(reader["Doc_CategoryName"].ToString());
                                    }
                                        }
                                    }
                                }
                        foreach (var docSubCategoryID in docSubCategoryIDs)
                        {
                            string seleQuery = "select Doc_SubCategoryName From docsubcategory_master where Doc_SubCategoryID=@Doc_SubCategoryID";
                            using (MySqlCommand cmd2 = new MySqlCommand(seleQuery, con))
                            {
                                cmd2.Parameters.AddWithValue("@Doc_SubCategoryID", docSubCategoryID);
                                using (MySqlDataReader reader = cmd2.ExecuteReader()) {
                                    while (reader.Read()) {
                                        if (docSubCategoryNames.Length > 0)
                                        {
                                            docSubCategoryNames.Append(", ");
                                        }

                                            docSubCategoryNames.Append(reader["Doc_SubCategoryName"].ToString());
                                    }

                                }


                            }
                        }




                            foreach (var Department_Master_id in Department_Master_ids)
                            {
                                string seleQ = "SELECT Department_Master_name FROM department_master WHERE Department_Master_id = @Department_Master_id";
                                using (MySqlCommand cmd1 = new MySqlCommand(seleQ, con))
                                {
                                    cmd1.Parameters.AddWithValue("@Department_Master_id", Department_Master_id);

                                    using (MySqlDataReader reader = cmd1.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            // Add the retrieved name to the list
                                            if (Department_Master_name.Length > 0)
                                            {
                                                Department_Master_name.Append(", ");
                                            }
                                            Department_Master_name.Append(reader["Department_Master_name"].ToString());
                                        }
                                    }
                                }
                            }

                            for (var i = 0; i < dt.Rows.Count; i++)
                            {
                                pdata.Add(new RepeatFrequencyModelNew
                                {
                                    Schedule_Assessment_id = dt.Rows[i]["Schedule_Assessment_id"] != DBNull.Value
        ? Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"])
        : 0, // Default value for int

                                    DocTypeID = dt.Rows[i]["DocTypeID"] != DBNull.Value
        ? Convert.ToInt32(dt.Rows[i]["DocTypeID"])
        : 0,

                                    Entity_Master_id = dt.Rows[i]["Entity_Master_id"] != DBNull.Value
        ? Convert.ToInt32(dt.Rows[i]["Entity_Master_id"])
        : 0,

                                    Unit_location_Master_id = dt.Rows[i]["Unit_location_Master_id"] != DBNull.Value
        ? Convert.ToInt32(dt.Rows[i]["Unit_location_Master_id"])
        : 0,

                                    Department_Master_id = Department_Master_ids, // Assuming you handle null elsewhere

                                    ass_template_id = dt.Rows[i]["ass_template_id"] != DBNull.Value
        ? dt.Rows[i]["ass_template_id"].ToString()
        : string.Empty, // Default for strings

                                    value_Frequency = dt.Rows[i]["value_Frequency"] != DBNull.Value
        ? Convert.ToInt32(dt.Rows[i]["value_Frequency"]).ToString()
        : "0",

                                    repeatEndDate = dt.Rows[i]["repeatEndDate"] != DBNull.Value
        ? dt.Rows[i]["repeatEndDate"].ToString()
        : string.Empty,

                                    Date_Of_Request = dt.Rows[i]["Date_Of_Request"] != DBNull.Value
        ? dt.Rows[i]["Date_Of_Request"].ToString()
        : string.Empty,

                                    Duration_of_Assessment = dt.Rows[i]["Duration_of_Assessment"] != DBNull.Value
        ? dt.Rows[i]["Duration_of_Assessment"].ToString()
        : string.Empty,

                                    pagetype = dt.Rows[i]["pagetype"] != DBNull.Value
        ? dt.Rows[i]["pagetype"].ToString()
        : string.Empty,

                                    endDate = dt.Rows[i]["endDate"] != DBNull.Value
        ? dt.Rows[i]["endDate"].ToString()
        : string.Empty,

                                    frequency_period = dt.Rows[i]["frequency_period"] != DBNull.Value
        ? dt.Rows[i]["frequency_period"].ToString()
        : string.Empty,

                                    uq_ass_schid = dt.Rows[i]["uq_ass_schid"] != DBNull.Value
        ? dt.Rows[i]["uq_ass_schid"].ToString()
        : string.Empty,

                                    requester_name = dt.Rows[i]["requester_name"] != DBNull.Value
        ? dt.Rows[i]["requester_name"].ToString()
        : string.Empty,

                                    DocTypeName = dt.Rows[i]["DocTypeName"] != DBNull.Value
        ? dt.Rows[i]["DocTypeName"].ToString()
        : string.Empty,

                                    Doc_CategoryName = docCategoryNames != null
        ? Convert.ToString(docCategoryNames)
        : string.Empty,

                                    Doc_SubCategoryName = docSubCategoryNames != null
        ? Convert.ToString(docSubCategoryNames)
        : string.Empty,

                                    Entity_Master_Name = dt.Rows[i]["Entity_Master_Name"] != DBNull.Value
        ? dt.Rows[i]["Entity_Master_Name"].ToString()
        : string.Empty,

                                    Unit_location_Master_name = dt.Rows[i]["Unit_location_Master_name"] != DBNull.Value
        ? dt.Rows[i]["Unit_location_Master_name"].ToString()
        : string.Empty,

                                    Department_Master_name = Department_Master_name != null
        ? Convert.ToString(Department_Master_name)
        : string.Empty,

                                    assessment_name = dt.Rows[i]["assessment_name"] != DBNull.Value
        ? dt.Rows[i]["assessment_name"].ToString()
        : string.Empty,

                                    objective = dt.Rows[i]["objective"] != DBNull.Value
        ? dt.Rows[i]["objective"].ToString()
        : string.Empty,

                                    message = dt.Rows[i]["message"] != DBNull.Value
        ? dt.Rows[i]["message"].ToString()
        : string.Empty,

                                    Shuffle_Questions = dt.Rows[i]["Shuffle_Questions"] != DBNull.Value
        ? Convert.ToInt32(dt.Rows[i]["Shuffle_Questions"])
        : 0,

                                    Shuffle_Answers = dt.Rows[i]["Shuffle_Answers"] != DBNull.Value
        ? Convert.ToInt32(dt.Rows[i]["Shuffle_Answers"])
        : 0,

                                    startDate = dt.Rows[i]["startDate"] != DBNull.Value
        ? dt.Rows[i]["startDate"].ToString()
        : string.Empty,
                                });

                            }

                        }
                        
                    }
                }
            }

            return pdata;
        }






        [Route("api/SetAssessment/GetActiveSchedulesAssessment")]
        [HttpGet]
        public IEnumerable<scheduledMappedUserModel> GetActiveSchedulesAssessment()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT
    sa.defaultkey,
    MAX(sa.created_date) AS created_date,
    MAX(sa.riskAssesserid) AS riskAssesserid,
      MAX(sa.requstingperson) AS requstingpersonid,
    MAX(tb_requester.firstname) AS requester_name,
    MAX(tb_assesser.firstname) AS assessor_name
FROM
    risk.scheduled_mapped_user sa
JOIN
    risk.tbluser tb_requester ON tb_requester.usr_ID = sa.requstingperson
JOIN
    risk.tbluser tb_assesser ON tb_assesser.usr_ID = sa.riskAssesserid
WHERE
    sa.scheduledstatus = 'Active'
GROUP BY
    sa.defaultkey ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<scheduledMappedUserModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new scheduledMappedUserModel
                    {
                        // Scheduled_mapped_user_id = int.TryParse(dt.Rows[i]["Scheduled_mapped_user_id"].ToString(), out int scheduleId) ? scheduleId : 0,
                        defaultkey = dt.Rows[i]["defaultkey"].ToString(),
                        // requstingperson = dt.Rows[i]["requstingperson"].ToString(),

                        riskAssesserid = int.TryParse(dt.Rows[i]["riskAssesserid"].ToString(), out int riskAssesserid) ? riskAssesserid : 0,
                        // usr_ID = int.TryParse(dt.Rows[i]["usr_ID"].ToString(), out int userId) ? userId : 0,
                        requester_name = dt.Rows[i]["requester_name"].ToString(),
                        assessor_name = dt.Rows[i]["assessor_name"].ToString(),

                        created_date = DateTime.TryParse(dt.Rows[i]["created_date"].ToString(), out DateTime createdDate) ? createdDate : DateTime.MinValue,
                        //scheduledstatus = dt.Rows[i]["scheduledstatus"].ToString()
                    });

                }
            }
            return pdata;
        }


        [Route("api/SetAssessment/GetActiveSchedulesAssessmentByID/{defaultkey}")]
        [HttpGet]


        public IEnumerable<scheduledMappedUserModel> GetActiveSchedulesAssessmentByID(int defaultkey)
        {
            var pdata = new List<scheduledMappedUserModel>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(@"
SELECT 
    sa.*,
    tb_requester.firstname AS requester_name,
    dm.DocTypeName,
    dcm.Doc_CategoryName,
    dscm.Doc_SubCategoryName,
    tb_assessor.firstname AS assessor_name,
    en.Entity_Master_Name,
    ul.Unit_location_Master_name,
    dpm.Department_Master_name,
     sa.requstingperson
FROM risk.scheduled_mapped_user sa
JOIN risk.tbluser tb_requester ON tb_requester.usr_ID = sa.requstingperson
LEFT JOIN risk.doctype_master dm ON dm.DocTypeID = sa.tpauserid
JOIN risk.tbluser tb_assessor ON tb_assessor.usr_ID = sa.riskAssesserid
LEFT JOIN risk.doccategory_master dcm ON dcm.Doc_CategoryID = sa.tpauserid
LEFT JOIN risk.docsubcategory_master dscm ON dscm.Doc_SubCategoryID = sa.tpauserid
LEFT JOIN risk.entity_master en ON en.Entity_Master_id = sa.entity_Master_id
LEFT JOIN risk.unit_location_master ul ON ul.Unit_location_Master_id = sa.unit_location_Master_id
LEFT JOIN risk.department_master dpm ON dpm.Department_Master_id = sa.department_Master_id
WHERE sa.defaultkey = @defaultkey AND sa.scheduledstatus = 'Active'
", con))
                {
                    cmd.Parameters.AddWithValue("@defaultkey", defaultkey);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            scheduledMappedUserModel model = new scheduledMappedUserModel();

                            model.Scheduled_mapped_user_id = reader.IsDBNull(reader.GetOrdinal("Scheduled_mapped_user_id")) ? 0 : reader.GetInt32("Scheduled_mapped_user_id");
                            model.docTypeID = reader.IsDBNull(reader.GetOrdinal("docTypeID")) ? 0 : reader.GetInt32("docTypeID");
                            model.doc_CategoryID = reader.IsDBNull(reader.GetOrdinal("doc_CategoryID")) ? 0 : reader.GetInt32("doc_CategoryID");
                            model.doc_SubCategoryID = reader.IsDBNull(reader.GetOrdinal("doc_SubCategoryID")) ? 0 : reader.GetInt32("doc_SubCategoryID");
                            model.entity_Master_id = reader.IsDBNull(reader.GetOrdinal("entity_Master_id")) ? 0 : reader.GetInt32("entity_Master_id");
                            model.unit_location_Master_id = reader.IsDBNull(reader.GetOrdinal("unit_location_Master_id")) ? 0 : reader.GetInt32("unit_location_Master_id");
                            model.department_Master_id = reader.IsDBNull(reader.GetOrdinal("department_Master_id")) ? 0 : reader.GetInt32("department_Master_id");
                            model.requstingperson = reader.IsDBNull(reader.GetOrdinal("requstingperson")) ? 0 : reader.GetInt32("requstingperson");
                            model.defaultkey = reader.IsDBNull(reader.GetOrdinal("defaultkey")) ? string.Empty : reader.GetString("defaultkey");
                            model.tpaenitydescription = reader.IsDBNull(reader.GetOrdinal("tpaenitydescription")) ? string.Empty : reader.GetString("tpaenitydescription");
                            model.requester_name = reader.IsDBNull(reader.GetOrdinal("requester_name")) ? string.Empty : reader.GetString("requester_name");
                            model.DocTypeName = reader.IsDBNull(reader.GetOrdinal("DocTypeName")) ? string.Empty : reader.GetString("DocTypeName");
                            model.Doc_CategoryName = reader.IsDBNull(reader.GetOrdinal("Doc_CategoryName")) ? string.Empty : reader.GetString("Doc_CategoryName");
                            model.Doc_SubCategoryName = reader.IsDBNull(reader.GetOrdinal("Doc_SubCategoryName")) ? string.Empty : reader.GetString("Doc_SubCategoryName");
                            model.Entity_Master_Name = reader.IsDBNull(reader.GetOrdinal("Entity_Master_Name")) ? string.Empty : reader.GetString("Entity_Master_Name");
                            model.Unit_location_Master_name = reader.IsDBNull(reader.GetOrdinal("Unit_location_Master_name")) ? string.Empty : reader.GetString("Unit_location_Master_name");
                            model.Department_Master_name = reader.IsDBNull(reader.GetOrdinal("Department_Master_name")) ? string.Empty : reader.GetString("Department_Master_name");

                            model.created_date = reader.IsDBNull(reader.GetOrdinal("created_date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("created_date"));

                            pdata.Add(model);
                        }

                    }
                }
            }

            return pdata;
        }







        [Route("api/SetAssessment/insertOnetimeFrequency")]
        [HttpPost]

        public async Task<IActionResult> insertOnetimeFrequency([FromBody] RepeatFrequencyModelNew RepeatFrequencyModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            string versonNo = "0"; // Initialize with a default value
           // string selQuery = "SELECT verson_no FROM assessment_builder_versions WHERE ass_template_id=@ass_template_id";
            string selQuery = @"
    SELECT verson_no 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @ass_template_id 
    ORDER BY verson_no DESC 
    LIMIT 1";


            using (MySqlCommand cmd = new MySqlCommand(selQuery, con))
            {
                // Add parameter for the query
                cmd.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);

                // Ensure the connection is open
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                // Execute the query and retrieve the result
                object verson = cmd.ExecuteScalar();

                // Check if the result is not null
                if (verson != null)
                {
                    versonNo = verson.ToString(); // Assign the retrieved version number as a string
                }
                else
                {
                    Console.WriteLine("No version number found.");
                }
            }


            string insertQuery = "INSERT INTO schedule_assessment (Date_Of_Request,Duration_of_Assessment,userid, DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Entity_Master_id, Unit_location_Master_id,Department_Master_id, created_date, status, Shuffle_Questions, Shuffle_Answers, startDate, endDate, objective, message, ass_template_id, AssessmentStatus, mapped_user,pagetype,login_userid,uq_ass_schid,verson_no) VALUES (@Date_Of_Request,@Duration_of_Assessment, @userid, @DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID, @Entity_Master_id, @Unit_location_Master_id,@Department_Master_id, @created_date, @status, @Shuffle_Questions, @Shuffle_Answers, @startDate, @endDate, @objective, @message, @ass_template_id, @AssessmentStatus, @mapped_user,@pagetype,@login_userid,@uq_ass_schid,@verson_no)";

            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                string AssesmentScheID = GenerateUniqueAssSchId(con);
                string docCategoryString = RepeatFrequencyModels.Doc_CategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_CategoryID) : string.Empty;
                string docSubCategoryString = RepeatFrequencyModels.Doc_SubCategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_SubCategoryID) : string.Empty;
                string depId = RepeatFrequencyModels.Department_Master_id != null ? string.Join(",", RepeatFrequencyModels.Department_Master_id) : string.Empty;




                foreach (var mapuser in RepeatFrequencyModels.mapped_user)
                {

                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {
                        //if (DateTime.TryParse(RepeatFrequencyModels.Date_Of_Request, out DateTime parsedDate))
                        //{
                        //    myCommand1.Parameters.AddWithValue("@Date_Of_Request", parsedDate);
                        //}


                        myCommand1.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);
                        myCommand1.Parameters.AddWithValue("@repeatEndDate", RepeatFrequencyModels.repeatEndDate);
                        myCommand1.Parameters.AddWithValue("@userid", RepeatFrequencyModels.userid);
                        myCommand1.Parameters.AddWithValue("@DocTypeID", RepeatFrequencyModels.DocTypeID);
                        myCommand1.Parameters.AddWithValue("@Doc_CategoryID", docCategoryString);
                        myCommand1.Parameters.AddWithValue("@Doc_SubCategoryID", docSubCategoryString);
                        myCommand1.Parameters.AddWithValue("@Entity_Master_id", RepeatFrequencyModels.Entity_Master_id);
                        myCommand1.Parameters.AddWithValue("@Unit_location_Master_id", RepeatFrequencyModels.Unit_location_Master_id);
                        myCommand1.Parameters.AddWithValue("@Department_Master_id", depId);
                        myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@Date_Of_Request", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@status", "Active");
                        myCommand1.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                        myCommand1.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);

                        if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                        {
                            myCommand1.Parameters.AddWithValue("@startDate", stDate);
                        }

                        if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                        {
                            myCommand1.Parameters.AddWithValue("@endDate", edDate);
                        }

                        myCommand1.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                        myCommand1.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                        myCommand1.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);
                        myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Scheduled");


                        myCommand1.Parameters.AddWithValue("@mapped_user", mapuser);

                        //myCommand1.Parameters.AddWithValue("@Department_Master_id", RepeatFrequencyModels.Department_Master_id);
                        myCommand1.Parameters.AddWithValue("@pagetype", "internal One time");

                        myCommand1.Parameters.AddWithValue("@login_userid", RepeatFrequencyModels.login_userid);

                        myCommand1.Parameters.AddWithValue("@uq_ass_schid", AssesmentScheID);
                        myCommand1.Parameters.AddWithValue("@verson_no", versonNo);

                        myCommand1.ExecuteNonQuery();

                        int ScheduleAssessmentId = Convert.ToInt32(myCommand1.LastInsertedId.ToString());
                      


                        foreach (var exceptionUser in RepeatFrequencyModels.Exemption_user)
                        {

                            string insertQuery2 = "insert into scheduled_excluded_user(Exemption_user,Schedule_Assessment_id,created_date,Exemption_user_reason)values(@Exemption_user,@Schedule_Assessment_id,@created_date,@Exemption_user_reason)";


                            using (MySqlCommand myCommand3 = new MySqlCommand(insertQuery2, con))
                            {


                                myCommand3.Parameters.AddWithValue("@Schedule_Assessment_id", ScheduleAssessmentId);
                                myCommand3.Parameters.AddWithValue("Exemption_user", exceptionUser);
                                myCommand3.Parameters.AddWithValue("Exemption_user_reason", RepeatFrequencyModels.Exemption_user_reason);
                                myCommand3.Parameters.AddWithValue("@created_date", DateTime.Now);

                                myCommand3.ExecuteNonQuery();

                            }



                        }
                        // send notifictaion to Taskuser About the Scheduled Assessment

                        int userId = mapuser;
                        var userEmail = await mySqlDBContext.usermodels
                               .Where(x => x.USR_ID == mapuser)
                               .Select(x => x.emailid)
                               .FirstOrDefaultAsync();


                        string getTemplateNameQuery = @"
    SELECT assessment_name 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @AssessementTemplateID";

                        string templateName = string.Empty;

                        using (var cmd = new MySqlCommand(getTemplateNameQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@AssessementTemplateID", RepeatFrequencyModels.ass_template_id);
                            object result = await cmd.ExecuteScalarAsync();

                            if (result != null)
                            {
                                templateName = result.ToString();
                            }
                        }


                        int senderid = RepeatFrequencyModels.login_userid;
                        var request = HttpContext.Request;
                        string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);


                        obj_Clsmail.Assessmentschedule(userEmail, templateName, senderid, userId, baseUrl);




                    }
                }
                return Ok("one time frequency added successfully");
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



        [Route("api/SetAssessment/insertOnetimeFrequencyForExternal")]
        [HttpPost]

        public async Task<IActionResult> insertOnetimeFrequencyForExternal([FromBody] RepeatFrequencyModelNew RepeatFrequencyModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            string versonNo = "0"; // Initialize with a default value
                                   // string selQuery = "SELECT verson_no FROM assessment_builder WHERE ass_template_id=@ass_template_id";
            string selQuery = @"
    SELECT verson_no 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @ass_template_id 
    ORDER BY verson_no DESC 
    LIMIT 1";

            using (MySqlCommand cmd = new MySqlCommand(selQuery, con))
            {
                // Add parameter for the query
                cmd.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);

                // Ensure the connection is open
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                // Execute the query and retrieve the result
                object verson = cmd.ExecuteScalar();

                // Check if the result is not null
                if (verson != null)
                {
                    versonNo = verson.ToString(); // Assign the retrieved version number as a string
                }
                else
                {
                    Console.WriteLine("No version number found.");
                }
            }


            string insertQuery = "INSERT INTO schedule_assessment (Date_Of_Request,Duration_of_Assessment,userid, DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Entity_Master_id, Unit_location_Master_id,Department_Master_id, created_date, status, Shuffle_Questions, Shuffle_Answers, startDate, endDate, objective, message, ass_template_id, AssessmentStatus, mapped_user,pagetype,login_userid,uq_ass_schid,verson_no) VALUES (@Date_Of_Request,@Duration_of_Assessment, @userid, @DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID, @Entity_Master_id, @Unit_location_Master_id,@Department_Master_id, @created_date, @status, @Shuffle_Questions, @Shuffle_Answers, @startDate, @endDate, @objective, @message, @ass_template_id, @AssessmentStatus, @mapped_user,@pagetype,@login_userid,@uq_ass_schid,@verson_no)";

            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                string AssesmentScheID = GenerateUniqueAssSchId(con);
                string docCategoryString = RepeatFrequencyModels.Doc_CategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_CategoryID) : string.Empty;
                string docSubCategoryString = RepeatFrequencyModels.Doc_SubCategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_SubCategoryID) : string.Empty;
                string depId = RepeatFrequencyModels.Department_Master_id != null ? string.Join(",", RepeatFrequencyModels.Department_Master_id) : string.Empty;




                foreach (var mapuser in RepeatFrequencyModels.mapped_user)
                {

                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {
                        //if (DateTime.TryParse(RepeatFrequencyModels.Date_Of_Request, out DateTime parsedDate))
                        //{
                        //    myCommand1.Parameters.AddWithValue("@Date_Of_Request", parsedDate);
                        //}


                        myCommand1.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);
                        myCommand1.Parameters.AddWithValue("@repeatEndDate", RepeatFrequencyModels.repeatEndDate);
                        myCommand1.Parameters.AddWithValue("@userid", RepeatFrequencyModels.userid);
                        myCommand1.Parameters.AddWithValue("@DocTypeID", RepeatFrequencyModels.DocTypeID);
                        myCommand1.Parameters.AddWithValue("@Doc_CategoryID", docCategoryString);
                        myCommand1.Parameters.AddWithValue("@Doc_SubCategoryID", docSubCategoryString);
                        myCommand1.Parameters.AddWithValue("@Entity_Master_id", RepeatFrequencyModels.Entity_Master_id);
                        myCommand1.Parameters.AddWithValue("@Unit_location_Master_id", RepeatFrequencyModels.Unit_location_Master_id);
                        myCommand1.Parameters.AddWithValue("@Department_Master_id", depId);
                        myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@Date_Of_Request", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@status", "Active");
                        myCommand1.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                        myCommand1.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);

                        if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                        {
                            myCommand1.Parameters.AddWithValue("@startDate", stDate);
                        }

                        if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                        {
                            myCommand1.Parameters.AddWithValue("@endDate", edDate);
                        }

                        myCommand1.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                        myCommand1.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                        myCommand1.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);
                        myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Scheduled");


                        myCommand1.Parameters.AddWithValue("@mapped_user", mapuser);

                        //myCommand1.Parameters.AddWithValue("@Department_Master_id", RepeatFrequencyModels.Department_Master_id);
                        myCommand1.Parameters.AddWithValue("@pagetype", "external One time");

                        myCommand1.Parameters.AddWithValue("@login_userid", RepeatFrequencyModels.login_userid);

                        myCommand1.Parameters.AddWithValue("@uq_ass_schid", AssesmentScheID);
                        myCommand1.Parameters.AddWithValue("@verson_no", versonNo);

                        myCommand1.ExecuteNonQuery();

                        int ScheduleAssessmentId = Convert.ToInt32(myCommand1.LastInsertedId.ToString());

                        foreach (var exceptionUser in RepeatFrequencyModels.Exemption_user)
                        {

                            string insertQuery2 = "insert into scheduled_excluded_user(Exemption_user,Schedule_Assessment_id,created_date,Exemption_user_reason)values(@Exemption_user,@Schedule_Assessment_id,@created_date,@Exemption_user_reason)";


                            using (MySqlCommand myCommand3 = new MySqlCommand(insertQuery2, con))
                            {


                                myCommand3.Parameters.AddWithValue("@Schedule_Assessment_id", ScheduleAssessmentId);
                                myCommand3.Parameters.AddWithValue("Exemption_user", exceptionUser);
                                myCommand3.Parameters.AddWithValue("Exemption_user_reason", RepeatFrequencyModels.Exemption_user_reason);
                                myCommand3.Parameters.AddWithValue("@created_date", DateTime.Now);

                                myCommand3.ExecuteNonQuery();

                            }
                        }
                            // send notifictaion to Taskuser About the Scheduled Assessment

                            int userId = mapuser;
                            var userEmail = await mySqlDBContext.usermodels
                                   .Where(x => x.USR_ID == mapuser)
                                   .Select(x => x.emailid)
                                   .FirstOrDefaultAsync();


                            string getTemplateNameQuery = @"
    SELECT assessment_name 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @AssessementTemplateID";

                            string templateName = string.Empty;

                            using (var cmd = new MySqlCommand(getTemplateNameQuery, con))
                            {
                                cmd.Parameters.AddWithValue("@AssessementTemplateID", RepeatFrequencyModels.ass_template_id);
                                object result = await cmd.ExecuteScalarAsync();

                                if (result != null)
                                {
                                    templateName = result.ToString();
                                }
                            }


                            int senderid = RepeatFrequencyModels.login_userid;
                            var request = HttpContext.Request;
                            string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);


                            obj_Clsmail.Assessmentschedule(userEmail, templateName, senderid, userId, baseUrl);


                        

                    }
                }


                string updateQuery = "update scheduled_mapped_user set scheduledstatus=@scheduledstatus where defaultkey=@defaultkey";
                using (MySqlCommand command = new MySqlCommand(updateQuery, con))
                {
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    command.Parameters.AddWithValue("@scheduledstatus", "Completed");
                    command.Parameters.AddWithValue("@defaultkey", RepeatFrequencyModels.defaultkey);
                    command.ExecuteNonQuery();
                }

                    return Ok("one time frequency added successfully");
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













        [Route("api/SetAssessment/insertExternalOnetimeFrequency")]
        [HttpPost]

        public IActionResult insertInitiateExternalOnetimeFrequency([FromBody] RepeatFrequencyModel RepeatFrequencyModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "INSERT INTO schedule_assessment ( Duration_of_Assessment,userid, DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Entity_Master_id, Unit_location_Master_id, created_date, status, Shuffle_Questions, Shuffle_Answers, startDate, endDate, objective, message, ass_template_id, AssessmentStatus,repeatEndDate,pagetype) VALUES ( @Duration_of_Assessment, @userid, @DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID, @Entity_Master_id, @Unit_location_Master_id, @created_date, @status, @Shuffle_Questions, @Shuffle_Answers, @startDate, @endDate, @objective, @message, @ass_template_id, @AssessmentStatus,@repeatEndDate,@pagetype)";

            try
            {
                con.Open();
                string AssesmentScheID = GenerateUniqueAssSchId(con);
                //foreach (var mapuser in RepeatFrequencyModels.mapped_user)
                //{
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    //    if (DateTime.TryParse(RepeatFrequencyModels.Date_Of_Request, out DateTime parsedDate))
                    //    {
                    //        myCommand1.Parameters.AddWithValue("@Date_Of_Request", parsedDate);
                    //    }


                    myCommand1.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);
                    myCommand1.Parameters.AddWithValue("@repeatEndDate", RepeatFrequencyModels.repeatEndDate);
                    myCommand1.Parameters.AddWithValue("@userid", RepeatFrequencyModels.userid);
                    myCommand1.Parameters.AddWithValue("@DocTypeID", RepeatFrequencyModels.DocTypeID);
                    myCommand1.Parameters.AddWithValue("@Doc_CategoryID", RepeatFrequencyModels.Doc_CategoryID);
                    myCommand1.Parameters.AddWithValue("@Doc_SubCategoryID", RepeatFrequencyModels.Doc_SubCategoryID);
                    myCommand1.Parameters.AddWithValue("@Entity_Master_id", RepeatFrequencyModels.Entity_Master_id);
                    myCommand1.Parameters.AddWithValue("@Unit_location_Master_id", RepeatFrequencyModels.Unit_location_Master_id);
                    myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@status", "Active");
                    myCommand1.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                    myCommand1.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);

                    if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                    {
                        myCommand1.Parameters.AddWithValue("@startDate", stDate);
                    }

                    if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                    {
                        myCommand1.Parameters.AddWithValue("@endDate", edDate);
                    }

                    myCommand1.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                    myCommand1.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                    myCommand1.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);
                    myCommand1.Parameters.AddWithValue("@pagetype", "initiate external One time");
                    myCommand1.Parameters.AddWithValue("@uq_ass_schid", AssesmentScheID);
                    myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Scheduled");
                    // myCommand1.Parameters.AddWithValue("@mapped_user", mapuser);

                    myCommand1.ExecuteNonQuery();
                }
                //}

                return Ok("one time self frequency added successfully");
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











        [Route("api/SetAssessment/insertSelfOnetimeFrequency")]
        [HttpPost]

        public async Task<IActionResult> insertSelfOnetimeFrequency([FromBody] RepeatFrequencyModelNew RepeatFrequencyModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "INSERT INTO schedule_assessment (Date_Of_Request,Duration_of_Assessment,userid, DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Entity_Master_id, Unit_location_Master_id,Department_Master_id, created_date, status, Shuffle_Questions, Shuffle_Answers, startDate, endDate, objective, message, ass_template_id, AssessmentStatus, mapped_user,pagetype,login_userid,uq_ass_schid) VALUES (@Date_Of_Request,@Duration_of_Assessment, @userid, @DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID, @Entity_Master_id, @Unit_location_Master_id,@Department_Master_id, @created_date, @status, @Shuffle_Questions, @Shuffle_Answers, @startDate, @endDate, @objective, @message, @ass_template_id, @AssessmentStatus, @mapped_user,@pagetype,@login_userid,@uq_ass_schid)";

            try
            {
                con.Open();
                string AssesmentScheID = GenerateUniqueAssSchId(con);
                string docCategoryString = RepeatFrequencyModels.Doc_CategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_CategoryID) : string.Empty;
                string docSubCategoryString = RepeatFrequencyModels.Doc_SubCategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_SubCategoryID) : string.Empty;
                string depId = RepeatFrequencyModels.Department_Master_id != null ? string.Join(",", RepeatFrequencyModels.Department_Master_id) : string.Empty;




                foreach (var mapuser in RepeatFrequencyModels.mapped_user)
                {

                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {
                        //if (DateTime.TryParse(RepeatFrequencyModels.Date_Of_Request, out DateTime parsedDate))
                        //{
                        //    myCommand1.Parameters.AddWithValue("@Date_Of_Request", parsedDate);
                        //}


                        myCommand1.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);
                        myCommand1.Parameters.AddWithValue("@repeatEndDate", RepeatFrequencyModels.repeatEndDate);
                        myCommand1.Parameters.AddWithValue("@userid", RepeatFrequencyModels.userid);
                        myCommand1.Parameters.AddWithValue("@DocTypeID", RepeatFrequencyModels.DocTypeID);
                        myCommand1.Parameters.AddWithValue("@Doc_CategoryID", docCategoryString);
                        myCommand1.Parameters.AddWithValue("@Doc_SubCategoryID", docSubCategoryString);
                        myCommand1.Parameters.AddWithValue("@Entity_Master_id", RepeatFrequencyModels.Entity_Master_id);
                        myCommand1.Parameters.AddWithValue("@Unit_location_Master_id", RepeatFrequencyModels.Unit_location_Master_id);
                        myCommand1.Parameters.AddWithValue("@Department_Master_id", depId);
                        myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@Date_Of_Request", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@status", "Active");
                        myCommand1.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                        myCommand1.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);

                        if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                        {
                            myCommand1.Parameters.AddWithValue("@startDate", stDate);
                        }

                        if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                        {
                            myCommand1.Parameters.AddWithValue("@endDate", edDate);
                        }

                        myCommand1.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                        myCommand1.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                        myCommand1.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);
                        myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Scheduled");

                        myCommand1.Parameters.AddWithValue("@mapped_user", mapuser);

                        //myCommand1.Parameters.AddWithValue("@Department_Master_id", RepeatFrequencyModels.Department_Master_id);
                        myCommand1.Parameters.AddWithValue("@pagetype", "Self One Time");

                        myCommand1.Parameters.AddWithValue("@login_userid", RepeatFrequencyModels.login_userid);

                        myCommand1.Parameters.AddWithValue("@uq_ass_schid", AssesmentScheID);


                        myCommand1.ExecuteNonQuery();

                        int ScheduleAssessmentId = Convert.ToInt32(myCommand1.LastInsertedId.ToString());

                        foreach (var exceptionUser in RepeatFrequencyModels.Exemption_user)
                        {

                            string insertQuery2 = "insert into scheduled_excluded_user(Exemption_user,Schedule_Assessment_id,created_date,Exemption_user_reason)values(@Exemption_user,@Schedule_Assessment_id,@created_date,@Exemption_user_reason)";


                            using (MySqlCommand myCommand3 = new MySqlCommand(insertQuery2, con))
                            {


                                myCommand3.Parameters.AddWithValue("@Schedule_Assessment_id", ScheduleAssessmentId);
                                myCommand3.Parameters.AddWithValue("Exemption_user", exceptionUser);
                                myCommand3.Parameters.AddWithValue("Exemption_user_reason", RepeatFrequencyModels.Exemption_user_reason);
                                myCommand3.Parameters.AddWithValue("@created_date", DateTime.Now);

                                myCommand3.ExecuteNonQuery();

                            }



                        }


                        // send mail to Taskusers

                        int userId = int.Parse(RepeatFrequencyModels.userid);
                        var userEmail = await mySqlDBContext.usermodels
                               .Where(x => x.USR_ID == userId)
                               .Select(x => x.emailid)
                               .FirstOrDefaultAsync();


                        string getTemplateNameQuery = @"
    SELECT assessment_name 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @AssessementTemplateID";

                        string templateName = string.Empty;

                        using (var cmd = new MySqlCommand(getTemplateNameQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@AssessementTemplateID", RepeatFrequencyModels.ass_template_id);
                            object result = await cmd.ExecuteScalarAsync();

                            if (result != null)
                            {
                                templateName = result.ToString();
                            }
                        }


                        int senderid = RepeatFrequencyModels.login_userid;
                        var request = HttpContext.Request;
                        string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);


                        obj_Clsmail.Assessmentschedule(userEmail, templateName, senderid, userId, baseUrl);

                    }
                }
                return Ok("one time frequency added successfully");
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






        [Route("api/SetAssessment/insertRepeatFrequency")]
        [HttpPost]

        public async Task< IActionResult> insertRepeatFrequency([FromBody] RepeatFrequencyModelNew RepeatFrequencyModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var scheduledAssessments = new List<RepeatFrequencyModelNew>();

            string versonNo = "0"; // Initialize with a default value
                                   //  string selQuery = "SELECT verson_no FROM assessment_builder WHERE ass_template_id=@ass_template_id";

            string selQuery = @"
    SELECT verson_no 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @ass_template_id 
    ORDER BY verson_no DESC 
    LIMIT 1";

            using (MySqlCommand cmd = new MySqlCommand(selQuery, con))
            {
               
                cmd.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);

            
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                
                object verson = cmd.ExecuteScalar();

               
                if (verson != null)
                {
                    versonNo = verson.ToString(); // Assign the retrieved version number as a string
                }
                else
                {
                    Console.WriteLine("No version number found.");
                }
            }

            // Parse and validate start and end dates
            if (!DateTime.TryParse(RepeatFrequencyModels.startDate, out var startDate))
            {
                throw new ArgumentException("Invalid Start Date.");
            }

            if (!DateTime.TryParse(RepeatFrequencyModels.endDate, out var endDate))
            {
                throw new ArgumentException("Invalid End Date.");
            }

            if (!DateTime.TryParse(RepeatFrequencyModels.repeatEndDate, out var repeatEndDate))
            {
                throw new ArgumentException("Invalid Repeat End Date.");
            }

            var currentStartDate = startDate;
            var currentEndDate = endDate;
            var frequencyType = RepeatFrequencyModels.frequency_period?.ToLower();
            var frequencyValue = int.TryParse(RepeatFrequencyModels.value_Frequency, out var freqValue) ? freqValue : 1;
            

            while (currentStartDate <= repeatEndDate)
            {
                // Adjust the end date if it exceeds the repeat end date
                if (currentEndDate > repeatEndDate)
                {
                    currentEndDate = repeatEndDate;
                }

                // Create a new assessment for the current iteration
                scheduledAssessments.Add(new RepeatFrequencyModelNew
                {
                    Schedule_Assessment_id = RepeatFrequencyModels.Schedule_Assessment_id,
                    Date_Of_Request = RepeatFrequencyModels.Date_Of_Request,
                    value_Frequency = RepeatFrequencyModels.value_Frequency,
                    frequency_period = RepeatFrequencyModels.frequency_period,
                    Assessment_start_Date = currentStartDate,
                    Duration_of_Assessment = RepeatFrequencyModels.Duration_of_Assessment,
                    repeatEndDate = RepeatFrequencyModels.repeatEndDate,
                    userid = RepeatFrequencyModels.userid,
                    DocTypeID = RepeatFrequencyModels.DocTypeID,
                    Doc_CategoryID = RepeatFrequencyModels.Doc_CategoryID,
                    Doc_SubCategoryID = RepeatFrequencyModels.Doc_SubCategoryID,
                    Entity_Master_id = RepeatFrequencyModels.Entity_Master_id,
                    Unit_location_Master_id = RepeatFrequencyModels.Unit_location_Master_id,
                    created_date = DateTime.Now,
                    ass_template_id = RepeatFrequencyModels.ass_template_id,
                    Shuffle_Questions = RepeatFrequencyModels.Shuffle_Questions,
                    Shuffle_Answers = RepeatFrequencyModels.Shuffle_Answers,
                    startDate = currentStartDate.ToString("yyyy-MM-dd"),
                    endDate = currentEndDate.ToString("yyyy-MM-dd"),
                    objective = RepeatFrequencyModels.objective,
                    message = RepeatFrequencyModels.message,
                    status = RepeatFrequencyModels.status,
                    firstname = RepeatFrequencyModels.firstname,
                    AssessmentStatus = RepeatFrequencyModels.AssessmentStatus,
                    mapped_user = RepeatFrequencyModels.mapped_user,
                    tpauserid = RepeatFrequencyModels.tpauserid,
                    Exemption_user_reason = RepeatFrequencyModels.Exemption_user_reason,
                    Exemption_user = RepeatFrequencyModels.Exemption_user,
                    Department_Master_id = RepeatFrequencyModels.Department_Master_id,
                    pagetype = RepeatFrequencyModels.pagetype,
                    login_userid = RepeatFrequencyModels.login_userid,
                    uq_ass_schid = RepeatFrequencyModels.uq_ass_schid,
                    requester_name = RepeatFrequencyModels.requester_name,
                    DocTypeName = RepeatFrequencyModels.DocTypeName,
                    Doc_CategoryName = RepeatFrequencyModels.Doc_CategoryName,
                    Doc_SubCategoryName = RepeatFrequencyModels.Doc_SubCategoryName,
                    Entity_Master_Name = RepeatFrequencyModels.Entity_Master_Name,
                    Unit_location_Master_name = RepeatFrequencyModels.Unit_location_Master_name,
                    Department_Master_name = RepeatFrequencyModels.Department_Master_name,
                    assessment_name = RepeatFrequencyModels.assessment_name,
                    verson_no = versonNo,
                });

                // Update dates based on frequency
                switch (frequencyType)
                {
                    case "daily":
                        currentStartDate = currentStartDate.AddDays(frequencyValue);
                        currentEndDate = currentEndDate.AddDays(frequencyValue);
                        break;
                    case "weekly":
                        currentStartDate = currentStartDate.AddDays(frequencyValue * 7);
                        currentEndDate = currentEndDate.AddDays(frequencyValue * 7);
                        break;
                    case "monthly":
                        currentStartDate = currentStartDate.AddMonths(frequencyValue);
                        currentEndDate = currentEndDate.AddMonths(frequencyValue);
                        break;
                    case "yearly":
                        currentStartDate = currentStartDate.AddYears(frequencyValue);
                        currentEndDate = currentEndDate.AddYears(frequencyValue);
                        break;
                    default:
                        throw new Exception($"Unsupported frequency type: {frequencyType}");
                }
            }
            try
            {
                foreach (RepeatFrequencyModelNew RepeatFrequencyModel in scheduledAssessments)
                {
                    RepeatFrequencyInsertionCode(RepeatFrequencyModel);
                }
            }
            catch(Exception ex)
            {

            }
            return Ok("one time frequency added successfully");

         

        }







        //New Repeat Frequency code 

        public void RepeatFrequencyInsertionCode(RepeatFrequencyModelNew RepeatFrequencyModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "INSERT INTO schedule_assessment (Date_Of_Request,value_Frequency, frequency_period, Duration_of_Assessment, repeatEndDate, userid, DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Entity_Master_id, Unit_location_Master_id,Department_Master_id, created_date, status, Shuffle_Questions, Shuffle_Answers, startDate, endDate, objective, message, ass_template_id, AssessmentStatus, mapped_user,pagetype,login_userid,uq_ass_schid,verson_no) VALUES (@Date_Of_Request,@value_Frequency, @frequency_period, @Duration_of_Assessment, @repeatEndDate, @userid, @DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID, @Entity_Master_id, @Unit_location_Master_id,@Department_Master_id, @created_date, @status, @Shuffle_Questions, @Shuffle_Answers, @startDate, @endDate, @objective, @message, @ass_template_id, @AssessmentStatus, @mapped_user,@pagetype,@login_userid,@uq_ass_schid,@verson_no)";

           
                con.Open();
                string AssesmentScheID = GenerateUniqueAssSchId(con);
                string docCategoryString = RepeatFrequencyModels.Doc_CategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_CategoryID) : string.Empty;
                string docSubCategoryString = RepeatFrequencyModels.Doc_SubCategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_SubCategoryID) : string.Empty;
                string depId = RepeatFrequencyModels.Department_Master_id != null ? string.Join(",", RepeatFrequencyModels.Department_Master_id) : string.Empty;

                foreach (var mapuser in RepeatFrequencyModels.mapped_user)
            {
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    if (DateTime.TryParse(RepeatFrequencyModels.Date_Of_Request, out DateTime parsedDate))
                    {
                        myCommand1.Parameters.AddWithValue("@Date_Of_Request", parsedDate);
                    }


                    myCommand1.Parameters.AddWithValue("@value_Frequency", RepeatFrequencyModels.value_Frequency);
                    myCommand1.Parameters.AddWithValue("@frequency_period", RepeatFrequencyModels.frequency_period);

                    myCommand1.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);

                    myCommand1.Parameters.AddWithValue("@userid", RepeatFrequencyModels.userid);
                    myCommand1.Parameters.AddWithValue("@DocTypeID", RepeatFrequencyModels.DocTypeID);
                    myCommand1.Parameters.AddWithValue("@Doc_CategoryID", docCategoryString);
                    myCommand1.Parameters.AddWithValue("@Doc_SubCategoryID", docSubCategoryString);
                    myCommand1.Parameters.AddWithValue("@Entity_Master_id", RepeatFrequencyModels.Entity_Master_id);
                    myCommand1.Parameters.AddWithValue("@Unit_location_Master_id", RepeatFrequencyModels.Unit_location_Master_id);
                    myCommand1.Parameters.AddWithValue("@Date_Of_Request", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@Department_Master_id", depId);
                    myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@status", "Active");
                    myCommand1.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                    myCommand1.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);

                    if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                    {
                        myCommand1.Parameters.AddWithValue("@startDate", stDate);
                    }

                    if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                    {
                        myCommand1.Parameters.AddWithValue("@endDate", edDate);
                    }
                    if (DateTime.TryParse(RepeatFrequencyModels.repeatEndDate, out DateTime repEndDate))
                    {
                        myCommand1.Parameters.AddWithValue("@repeatEndDate", repEndDate);
                    }


                    myCommand1.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                    myCommand1.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                    myCommand1.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);

                    myCommand1.Parameters.AddWithValue("@mapped_user", mapuser);
                    myCommand1.Parameters.AddWithValue("@pagetype", "internal Repeat frequency");
                    myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Scheduled");
                    myCommand1.Parameters.AddWithValue("@login_userid", RepeatFrequencyModels.login_userid);

                    myCommand1.Parameters.AddWithValue("@uq_ass_schid", AssesmentScheID);
                    myCommand1.Parameters.AddWithValue("@verson_no", RepeatFrequencyModels.verson_no);
                    myCommand1.ExecuteNonQuery();
                    int ScheduleAssessmentId = Convert.ToInt32(myCommand1.LastInsertedId.ToString());

                    foreach (var exceptionUser in RepeatFrequencyModels.Exemption_user)
                    {

                        string insertQuery2 = "insert into scheduled_excluded_user(Exemption_user,Schedule_Assessment_id,created_date,Exemption_user_reason)values(@Exemption_user,@Schedule_Assessment_id,@created_date,@Exemption_user_reason)";


                        using (MySqlCommand myCommand3 = new MySqlCommand(insertQuery2, con))
                        {


                            myCommand3.Parameters.AddWithValue("@Schedule_Assessment_id", ScheduleAssessmentId);
                            myCommand3.Parameters.AddWithValue("Exemption_user", exceptionUser);
                            myCommand3.Parameters.AddWithValue("Exemption_user_reason", RepeatFrequencyModels.Exemption_user_reason);
                            myCommand3.Parameters.AddWithValue("@created_date", DateTime.Now);

                            myCommand3.ExecuteNonQuery();

                        }

                        con.Close();

                    }


                    // send notification to Taskuser About the Scheduled Assessment

                    int userId = int.Parse(RepeatFrequencyModels.userid);
                    var userEmail = mySqlDBContext.usermodels
                        .Where(x => x.USR_ID == userId)
                        .Select(x => x.emailid)
                        .FirstOrDefault();

                    string getTemplateNameQuery = @"
SELECT assessment_name 
FROM assessment_builder_versions 
WHERE ass_template_id = @AssessementTemplateID";

                    string templateName = string.Empty;

                    using (var cmd = new MySqlCommand(getTemplateNameQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@AssessementTemplateID", RepeatFrequencyModels.ass_template_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            templateName = result.ToString();
                        }
                    }

                    int senderid = RepeatFrequencyModels.login_userid;
                    var request = HttpContext.Request;
                    string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                    // Trigger email notification
                    obj_Clsmail.Assessmentschedule(userEmail, templateName, senderid, userId, baseUrl);



                }
            }



        }















        [Route("api/SetAssessment/updateMonitoredAssessment/{id}")]  
        [HttpPost]

        public IActionResult updateMonitoredAssessment([FromBody] RepeatFrequencyModelNew RepeatFrequencyModels, int id)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                // Step 1: Retrieve existing data for `uq_ass_schid`
                string selectQuery = "SELECT * FROM schedule_assessment WHERE uq_ass_schid = @uq_ass_schid LIMIT 1";
                Dictionary<string, object> existingData = new Dictionary<string, object>();

                using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, con))
                {
                    selectCommand.Parameters.AddWithValue("@uq_ass_schid", id);
                
                    using (MySqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for (int i = 1; i < reader.FieldCount; i++)
                            {
                                existingData[reader.GetName(i)] = reader.GetValue(i);
                            }
                        }
                    }
                }

                // Step 2: Update `Exemption_user` to inactive status if it's not null or empty
                if (RepeatFrequencyModels.Exemption_user != null && RepeatFrequencyModels.Exemption_user.Length > 0)
                {
                    string exemptionIds = string.Join(",", RepeatFrequencyModels.Exemption_user.Select((_, index) => $"@exemptUser{index}"));
                    string updateExemptQuery = $@"
            UPDATE schedule_assessment 
            SET status = 'inactive' 
            WHERE uq_ass_schid = @uq_ass_schid 
            AND mapped_user IN ({exemptionIds})";

                    using (MySqlCommand updateExemptCommand = new MySqlCommand(updateExemptQuery, con))
                    {
                        updateExemptCommand.Parameters.AddWithValue("@uq_ass_schid", id);
                        for (int i = 0; i < RepeatFrequencyModels.Exemption_user.Length; i++)
                        {
                            updateExemptCommand.Parameters.AddWithValue($"@exemptUser{i}", RepeatFrequencyModels.Exemption_user[i]);
                        }
                        updateExemptCommand.ExecuteNonQuery();
                    }
                }

                // Step 3: Update main data (excluding `Exemption_user`)
                string nonExemptIds = RepeatFrequencyModels.Exemption_user != null && RepeatFrequencyModels.Exemption_user.Length > 0
     ? string.Join(",", RepeatFrequencyModels.Exemption_user.Select(userId => $"'{userId}'"))
     : "''";

                string updateQuery = $@"
    UPDATE schedule_assessment 
    SET Duration_of_Assessment = @Duration_of_Assessment, 
        startDate = @startDate, 
        endDate = @endDate, 
        value_Frequency = @value_Frequency, 
        frequency_period = @frequency_period, 
        repeatEndDate = @repeatEndDate, 
        Shuffle_Questions = @Shuffle_Questions, 
        Shuffle_Answers = @Shuffle_Answers, 
        message = @message, 
        objective = @objective, 
        pagetype = @pagetype, 
        AssessmentStatus = @AssessmentStatus
    WHERE uq_ass_schid = @uq_ass_schid 
    AND mapped_user NOT IN ({nonExemptIds})";

                using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, con))
                {
                    updateCommand.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);
                    
                    updateCommand.Parameters.AddWithValue("@value_Frequency", Convert.ToInt32(RepeatFrequencyModels.value_Frequency));
                    updateCommand.Parameters.AddWithValue("@frequency_period", RepeatFrequencyModels.frequency_period);
                    updateCommand.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                    updateCommand.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);
                    updateCommand.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                    updateCommand.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                    updateCommand.Parameters.AddWithValue("@uq_ass_schid", id);
                    if (RepeatFrequencyModels.repeatEndDate != null)
                    {
                        if (DateTime.TryParse(RepeatFrequencyModels.repeatEndDate, out DateTime repEndDate))
                            updateCommand.Parameters.AddWithValue("@repeatEndDate", repEndDate);
                        updateCommand.Parameters.AddWithValue("@pagetype", "internal Repeat frequency");
                    }
                    else
                    {
                        updateCommand.Parameters.AddWithValue("@pagetype", "internal One time");
                        updateCommand.Parameters.AddWithValue("@repeatEndDate", RepeatFrequencyModels.repeatEndDate);
                    }
                   
                    updateCommand.Parameters.AddWithValue("@AssessmentStatus", "Assessment Rescheduled");
                   
                    if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                        updateCommand.Parameters.AddWithValue("@startDate", stDate);

                    if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                        updateCommand.Parameters.AddWithValue("@endDate", edDate);
                  

                    if (RepeatFrequencyModels.Exemption_user != null)
                    {
                        for (int i = 0; i < RepeatFrequencyModels.Exemption_user.Length; i++)
                        {
                            updateCommand.Parameters.AddWithValue($"@excludedUser{i}", RepeatFrequencyModels.Exemption_user[i]);
                        }
                    }

                    updateCommand.ExecuteNonQuery();
                }

                // Step 4: Insert new records for `mapped_user` if it's not null or empty
                if (RepeatFrequencyModels.mapped_user != null && RepeatFrequencyModels.mapped_user.Length > 0)
                {
                    string insertQuery = @"
            INSERT INTO schedule_assessment 
                (Duration_of_Assessment, startDate, endDate, value_Frequency, frequency_period, repeatEndDate, Shuffle_Questions, 
                Shuffle_Answers, message, objective, pagetype, AssessmentStatus, status, mapped_user, uq_ass_schid, Date_Of_Request, userid, DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Entity_Master_id, Unit_location_Master_id, Department_Master_id,ass_template_id,login_userid) 
            VALUES 
                (@Duration_of_Assessment, @startDate, @endDate, @value_Frequency, @frequency_period, @repeatEndDate, 
                @Shuffle_Questions, @Shuffle_Answers, @message, @objective, @pagetype, @AssessmentStatus, @status, @mapped_user, @uq_ass_schid, @Date_Of_Request, @userid, @DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID, @Entity_Master_id, @Unit_location_Master_id, @Department_Master_id,@ass_template_id,@login_userid)";

                    foreach (var userId in RepeatFrequencyModels.mapped_user.Except(RepeatFrequencyModels.Exemption_user ?? Array.Empty<int>()))
                    {
                        using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, con))
                        {
                            insertCommand.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);
                           
                            insertCommand.Parameters.AddWithValue("@value_Frequency", Convert.ToInt32(RepeatFrequencyModels.value_Frequency));
                            insertCommand.Parameters.AddWithValue("@frequency_period", RepeatFrequencyModels.frequency_period);
                            insertCommand.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                            insertCommand.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);
                            insertCommand.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                            insertCommand.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                            insertCommand.Parameters.AddWithValue("@uq_ass_schid", id);
                           
                            if (RepeatFrequencyModels.repeatEndDate != null)
                            {
                                if (DateTime.TryParse(RepeatFrequencyModels.repeatEndDate, out DateTime repEndDate))
                                    insertCommand.Parameters.AddWithValue("@repeatEndDate", repEndDate);
                                insertCommand.Parameters.AddWithValue("@pagetype", "internal Repeat frequency");
                            }
                            else
                            {
                                insertCommand.Parameters.AddWithValue("@pagetype", "internal One time");
                                insertCommand.Parameters.AddWithValue("@repeatEndDate", RepeatFrequencyModels.repeatEndDate);
                            }
                            insertCommand.Parameters.AddWithValue("@AssessmentStatus", "Assessment Rescheduled");
                            insertCommand.Parameters.AddWithValue("@status", "Active");
                            insertCommand.Parameters.AddWithValue("@mapped_user", userId);

                            if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                                insertCommand.Parameters.AddWithValue("@startDate", stDate);

                            if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                                insertCommand.Parameters.AddWithValue("@endDate", edDate);
                            

                            insertCommand.Parameters.AddWithValue("@Date_Of_Request", DateTime.Now);
                            insertCommand.Parameters.AddWithValue("@userid", existingData["userid"]);
                            insertCommand.Parameters.AddWithValue("@DocTypeID", existingData["DocTypeID"]);
                            insertCommand.Parameters.AddWithValue("@Doc_CategoryID", existingData["Doc_CategoryID"]);
                            insertCommand.Parameters.AddWithValue("@Doc_SubCategoryID", existingData["Doc_SubCategoryID"]);
                            insertCommand.Parameters.AddWithValue("@Entity_Master_id", existingData["Entity_Master_id"]);
                            insertCommand.Parameters.AddWithValue("@Unit_location_Master_id", existingData["Unit_location_Master_id"]);
                            insertCommand.Parameters.AddWithValue("@Department_Master_id", existingData["Department_Master_id"]);
                            insertCommand.Parameters.AddWithValue("@ass_template_id", existingData["ass_template_id"]);
                            insertCommand.Parameters.AddWithValue("@login_userid", existingData["login_userid"]);

                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }

                return Ok("Monitored assessment updated, new users added, and exemption users marked inactive successfully.");
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



        [Route("api/SetAssessment/insertSelfRepeatFrequency")]
        [HttpPost]

        public IActionResult insertSelfRepeatFrequency([FromBody] RepeatFrequencyModelNew RepeatFrequencyModels)
        {
            var scheduledAssessments = new List<RepeatFrequencyModelNew>();

            // Parse and validate start and end dates
            if (!DateTime.TryParse(RepeatFrequencyModels.startDate, out var startDate))
            {
                throw new ArgumentException("Invalid Start Date.");
            }

            if (!DateTime.TryParse(RepeatFrequencyModels.endDate, out var endDate))
            {
                throw new ArgumentException("Invalid End Date.");
            }

            if (!DateTime.TryParse(RepeatFrequencyModels.repeatEndDate, out var repeatEndDate))
            {
                throw new ArgumentException("Invalid Repeat End Date.");
            }

            var currentStartDate = startDate;
            var currentEndDate = endDate;
            var frequencyType = RepeatFrequencyModels.frequency_period?.ToLower();
            var frequencyValue = int.TryParse(RepeatFrequencyModels.value_Frequency, out var freqValue) ? freqValue : 1;

            while (currentStartDate <= repeatEndDate)
            {
                // Adjust the end date if it exceeds the repeat end date
                if (currentEndDate > repeatEndDate)
                {
                    currentEndDate = repeatEndDate;
                }

                // Create a new assessment for the current iteration
                scheduledAssessments.Add(new RepeatFrequencyModelNew
                {
                    Schedule_Assessment_id = RepeatFrequencyModels.Schedule_Assessment_id,
                    Date_Of_Request = RepeatFrequencyModels.Date_Of_Request,
                    value_Frequency = RepeatFrequencyModels.value_Frequency,
                    frequency_period = RepeatFrequencyModels.frequency_period,
                    Assessment_start_Date = currentStartDate,
                    Duration_of_Assessment = RepeatFrequencyModels.Duration_of_Assessment,
                    repeatEndDate = RepeatFrequencyModels.repeatEndDate,
                    userid = RepeatFrequencyModels.userid,
                    DocTypeID = RepeatFrequencyModels.DocTypeID,
                    Doc_CategoryID = RepeatFrequencyModels.Doc_CategoryID,
                    Doc_SubCategoryID = RepeatFrequencyModels.Doc_SubCategoryID,
                    Entity_Master_id = RepeatFrequencyModels.Entity_Master_id,
                    Unit_location_Master_id = RepeatFrequencyModels.Unit_location_Master_id,
                    created_date = DateTime.Now,
                    ass_template_id = RepeatFrequencyModels.ass_template_id,
                    Shuffle_Questions = RepeatFrequencyModels.Shuffle_Questions,
                    Shuffle_Answers = RepeatFrequencyModels.Shuffle_Answers,
                    startDate = currentStartDate.ToString("yyyy-MM-dd"),
                    endDate = currentEndDate.ToString("yyyy-MM-dd"),
                    objective = RepeatFrequencyModels.objective,
                    message = RepeatFrequencyModels.message,
                    status = RepeatFrequencyModels.status,
                    firstname = RepeatFrequencyModels.firstname,
                    AssessmentStatus = RepeatFrequencyModels.AssessmentStatus,
                    mapped_user = RepeatFrequencyModels.mapped_user,
                    tpauserid = RepeatFrequencyModels.tpauserid,
                    Exemption_user_reason = RepeatFrequencyModels.Exemption_user_reason,
                    Exemption_user = RepeatFrequencyModels.Exemption_user,
                    Department_Master_id = RepeatFrequencyModels.Department_Master_id,
                    pagetype = RepeatFrequencyModels.pagetype,
                    login_userid = RepeatFrequencyModels.login_userid,
                    uq_ass_schid = RepeatFrequencyModels.uq_ass_schid,
                    requester_name = RepeatFrequencyModels.requester_name,
                    DocTypeName = RepeatFrequencyModels.DocTypeName,
                    Doc_CategoryName = RepeatFrequencyModels.Doc_CategoryName,
                    Doc_SubCategoryName = RepeatFrequencyModels.Doc_SubCategoryName,
                    Entity_Master_Name = RepeatFrequencyModels.Entity_Master_Name,
                    Unit_location_Master_name = RepeatFrequencyModels.Unit_location_Master_name,
                    Department_Master_name = RepeatFrequencyModels.Department_Master_name,
                    assessment_name = RepeatFrequencyModels.assessment_name
                });

                // Update dates based on frequency
                switch (frequencyType)
                {
                    case "daily":
                        currentStartDate = currentStartDate.AddDays(frequencyValue);
                        currentEndDate = currentEndDate.AddDays(frequencyValue);
                        break;
                    case "weekly":
                        currentStartDate = currentStartDate.AddDays(frequencyValue * 7);
                        currentEndDate = currentEndDate.AddDays(frequencyValue * 7);
                        break;
                    case "monthly":
                        currentStartDate = currentStartDate.AddMonths(frequencyValue);
                        currentEndDate = currentEndDate.AddMonths(frequencyValue);
                        break;
                    case "yearly":
                        currentStartDate = currentStartDate.AddYears(frequencyValue);
                        currentEndDate = currentEndDate.AddYears(frequencyValue);
                        break;
                    default:
                        throw new Exception($"Unsupported frequency type: {frequencyType}");
                }
            }
            try
            {
                foreach (RepeatFrequencyModelNew RepeatFrequencyModel in scheduledAssessments)
                {
                    SelfRepeatFrequencyInsertionCode(RepeatFrequencyModel);
                }
            }
            catch (Exception ex)
            {

            }
            return Ok("one time frequency added successfully");



        }



        //Insert New Self Repeat Frequency

        public void SelfRepeatFrequencyInsertionCode(RepeatFrequencyModelNew RepeatFrequencyModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "INSERT INTO schedule_assessment (Date_Of_Request,value_Frequency, frequency_period, Duration_of_Assessment, repeatEndDate, userid, DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Entity_Master_id, Unit_location_Master_id,Department_Master_id, created_date, status, Shuffle_Questions, Shuffle_Answers, startDate, endDate, objective, message, ass_template_id, AssessmentStatus, mapped_user,pagetype,login_userid,uq_ass_schid) VALUES (@Date_Of_Request,@value_Frequency, @frequency_period, @Duration_of_Assessment, @repeatEndDate, @userid, @DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID, @Entity_Master_id, @Unit_location_Master_id,@Department_Master_id, @created_date, @status, @Shuffle_Questions, @Shuffle_Answers, @startDate, @endDate, @objective, @message, @ass_template_id, @AssessmentStatus, @mapped_user,@pagetype,@login_userid,@uq_ass_schid)";


            con.Open();
            string AssesmentScheID = GenerateUniqueAssSchId(con);
            string docCategoryString = RepeatFrequencyModels.Doc_CategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_CategoryID) : string.Empty;
            string docSubCategoryString = RepeatFrequencyModels.Doc_SubCategoryID != null ? string.Join(",", RepeatFrequencyModels.Doc_SubCategoryID) : string.Empty;
            string depId = RepeatFrequencyModels.Department_Master_id != null ? string.Join(",", RepeatFrequencyModels.Department_Master_id) : string.Empty;

            foreach (var mapuser in RepeatFrequencyModels.mapped_user)
            {
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    if (DateTime.TryParse(RepeatFrequencyModels.Date_Of_Request, out DateTime parsedDate))
                    {
                        myCommand1.Parameters.AddWithValue("@Date_Of_Request", parsedDate);
                    }


                    myCommand1.Parameters.AddWithValue("@value_Frequency", RepeatFrequencyModels.value_Frequency);
                    myCommand1.Parameters.AddWithValue("@frequency_period", RepeatFrequencyModels.frequency_period);

                    myCommand1.Parameters.AddWithValue("@Duration_of_Assessment", RepeatFrequencyModels.Duration_of_Assessment);

                    myCommand1.Parameters.AddWithValue("@userid", RepeatFrequencyModels.userid);
                    myCommand1.Parameters.AddWithValue("@DocTypeID", RepeatFrequencyModels.DocTypeID);
                    myCommand1.Parameters.AddWithValue("@Doc_CategoryID", docCategoryString);
                    myCommand1.Parameters.AddWithValue("@Doc_SubCategoryID", docSubCategoryString);
                    myCommand1.Parameters.AddWithValue("@Entity_Master_id", RepeatFrequencyModels.Entity_Master_id);
                    myCommand1.Parameters.AddWithValue("@Unit_location_Master_id", RepeatFrequencyModels.Unit_location_Master_id);
                    myCommand1.Parameters.AddWithValue("@Date_Of_Request", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@Department_Master_id", depId);
                    myCommand1.Parameters.AddWithValue("@created_date", DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@status", "Active");
                    myCommand1.Parameters.AddWithValue("@Shuffle_Questions", RepeatFrequencyModels.Shuffle_Questions);
                    myCommand1.Parameters.AddWithValue("@Shuffle_Answers", RepeatFrequencyModels.Shuffle_Answers);

                    if (DateTime.TryParse(RepeatFrequencyModels.startDate, out DateTime stDate))
                    {
                        myCommand1.Parameters.AddWithValue("@startDate", stDate);
                    }

                    if (DateTime.TryParse(RepeatFrequencyModels.endDate, out DateTime edDate))
                    {
                        myCommand1.Parameters.AddWithValue("@endDate", edDate);
                    }
                    if (DateTime.TryParse(RepeatFrequencyModels.repeatEndDate, out DateTime repEndDate))
                    {
                        myCommand1.Parameters.AddWithValue("@repeatEndDate", repEndDate);
                    }


                    myCommand1.Parameters.AddWithValue("@objective", RepeatFrequencyModels.objective);
                    myCommand1.Parameters.AddWithValue("@message", RepeatFrequencyModels.message);
                    myCommand1.Parameters.AddWithValue("@ass_template_id", RepeatFrequencyModels.ass_template_id);

                    myCommand1.Parameters.AddWithValue("@mapped_user", mapuser);
                    myCommand1.Parameters.AddWithValue("@pagetype", "Self Repeat frequency");
                    myCommand1.Parameters.AddWithValue("@AssessmentStatus", "Assessment Scheduled");
                    myCommand1.Parameters.AddWithValue("@login_userid", RepeatFrequencyModels.login_userid);

                    myCommand1.Parameters.AddWithValue("@uq_ass_schid", AssesmentScheID);

                    myCommand1.ExecuteNonQuery();
                    int ScheduleAssessmentId = Convert.ToInt32(myCommand1.LastInsertedId.ToString());

                    foreach (var exceptionUser in RepeatFrequencyModels.Exemption_user)
                    {

                        string insertQuery2 = "insert into scheduled_excluded_user(Exemption_user,Schedule_Assessment_id,created_date,Exemption_user_reason)values(@Exemption_user,@Schedule_Assessment_id,@created_date,@Exemption_user_reason)";


                        using (MySqlCommand myCommand3 = new MySqlCommand(insertQuery2, con))
                        {


                            myCommand3.Parameters.AddWithValue("@Schedule_Assessment_id", ScheduleAssessmentId);
                            myCommand3.Parameters.AddWithValue("Exemption_user", exceptionUser);
                            myCommand3.Parameters.AddWithValue("Exemption_user_reason", RepeatFrequencyModels.Exemption_user_reason);
                            myCommand3.Parameters.AddWithValue("@created_date", DateTime.Now);

                            myCommand3.ExecuteNonQuery();

                        }

                        con.Close();

                    }



                    // send notification to Taskuser About the Scheduled Assessment

                    int userId = int.Parse(RepeatFrequencyModels.userid);
                    var userEmail = mySqlDBContext.usermodels
                        .Where(x => x.USR_ID == userId)
                        .Select(x => x.emailid)
                        .FirstOrDefault();

                    string getTemplateNameQuery = @"
SELECT assessment_name 
FROM assessment_builder_versions 
WHERE ass_template_id = @AssessementTemplateID";

                    string templateName = string.Empty;

                    using (var cmd = new MySqlCommand(getTemplateNameQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@AssessementTemplateID", RepeatFrequencyModels.ass_template_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            templateName = result.ToString();
                        }
                    }

                    int senderid = RepeatFrequencyModels.login_userid;
                    var request = HttpContext.Request;
                    string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                
                    obj_Clsmail.Assessmentschedule(userEmail, templateName, senderid, userId, baseUrl);
                }
            }



        }



        private string GenerateUniqueAssSchId(MySqlConnection con)
        {
            string assSchId;



            do
            {
                // Use current date components for the ID
                string datePart = DateTime.Now.ToString("yyyy");

                // Ensure that datePart does not start with 0
                //if (datePart[0] == '0')
                //{
                //    datePart = datePart.Substring(1); // Remove the leading 0
                //}



                // Generate a random four-digit number
                string randomPart = new Random().Next(1000, 100000).ToString("D5");

                // Combine the random part
                assSchId = $"{datePart}{randomPart}";

                string checkDuplicateQuery = "SELECT COUNT(*) FROM schedule_assessment WHERE uq_ass_schid = @assSchId";
                using (MySqlCommand checkDuplicateCommand = new MySqlCommand(checkDuplicateQuery, con))
                {
                    checkDuplicateCommand.Parameters.AddWithValue("@assSchId", assSchId);
                    int count = Convert.ToInt32(checkDuplicateCommand.ExecuteScalar());
                    if (count == 0)
                    {
                        // Unique ID generated
                        break;
                    }
                }
            } while (true);

            return assSchId;
        }


        [Route("api/SetAssessment/GetOnetimeFrequency")]
        [HttpGet]
        public IEnumerable<ViewAssDetails> GetOnetimeFrequency(int userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT 
    sa.uq_ass_schid, 
    MAX(sa.Schedule_Assessment_id) AS Schedule_Assessment_id, 
    MAX(tu.USR_ID) AS login_userid, 
    MAX(tu.firstname) AS login_user_firstname, 
    MAX(em.Entity_Master_Name) AS Entity_Master_Name, 
    MAX(ul.Unit_location_Master_name) AS Unit_location_Master_name, 
    MAX(ab.ass_template_id) AS ass_template_id, 
    MAX(ab.assessment_name) AS assessment_name, 
    MAX(sa.Duration_of_Assessment) AS Duration_of_Assessment, 
    MAX(tp.Type_Name) AS Type_Name, 
    MAX(sp.SubType_Name) AS SubType_Name, 
    MAX(cs.Competency_Name) AS Competency_Name, 
    GROUP_CONCAT(DISTINCT umap.firstname ORDER BY umap.firstname ASC SEPARATOR ', ') AS mapped_users, 
    MAX(sa.startDate) AS startDate, 
    MAX(sa.endDate) AS endDate, 
    MAX(sa.AssessmentStatus) AS AssessmentStatus, 
    MAX(sa.value_Frequency) AS value_Frequency, 
    MAX(sa.frequency_period) AS frequency_period, 
    MAX(sa.Date_Of_Request) AS Date_Of_Request
FROM risk.schedule_assessment sa 
LEFT JOIN risk.assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id
LEFT JOIN risk.tbluser tu ON tu.USR_ID = sa.login_userid
LEFT JOIN risk.tbluser umap ON FIND_IN_SET(umap.USR_ID, sa.mapped_user) > 0
LEFT JOIN risk.entity_master em ON em.Entity_Master_id = sa.Entity_Master_id
LEFT JOIN risk.unit_location_master ul ON ul.Unit_location_Master_id = sa.Unit_location_Master_id
LEFT JOIN risk.type tp ON tp.Type_id = ab.Type_id
LEFT JOIN risk.sub_type sp ON sp.SubType_id = ab.SubType_id
LEFT JOIN risk.competency_skill cs ON cs.Competency_id = ab.Competency_id
WHERE sa.login_userid = @userid AND sa.AssessmentStatus <> @AssStatus And sa.status = 'Active'
GROUP BY sa.uq_ass_schid
", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.Parameters.AddWithValue("@AssStatus","Assessment Completed");

           
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<ViewAssDetails>();

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
      

                    pdata.Add(new ViewAssDetails
                    {
                     
                    
                        Schedule_Assessment_id = Convert.ToInt32(dt.Rows[i]["Schedule_Assessment_id"]),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        Duration_of_Assessment = dt.Rows[i]["Duration_of_Assessment"].ToString(),
                         startDate = Convert.ToDateTime(dt.Rows[i]["startDate"]).ToString("dd MMM yyyy"),
                        endDate = Convert.ToDateTime(dt.Rows[i]["endDate"]).ToString("dd MMM yyyy"),
                        firstname = dt.Rows[i]["login_user_firstname"].ToString(),
                        mapped_users = dt.Rows[i]["mapped_users"].ToString(),
                        Entity_Master_Name = dt.Rows[i]["Entity_Master_Name"].ToString(),
                        Unit_location_Master_name = dt.Rows[i]["Unit_location_Master_name"].ToString(),
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        AssessmentStatus = dt.Rows[i]["AssessmentStatus"].ToString(),
                        value_Frequency = dt.Rows[i]["value_Frequency"].ToString(),
                        frequency_period = dt.Rows[i]["frequency_period"].ToString()==null|| dt.Rows[i]["frequency_period"].ToString() ==""? "N/A": dt.Rows[i]["value_Frequency"].ToString()+" "+dt.Rows[i]["frequency_period"].ToString(),
                        Date_Of_Request = Convert.ToDateTime(dt.Rows[i]["Date_Of_Request"]).ToString("dd MMM yyyy"),
                   



                    });
                }
            }
            
            return pdata;
        }
        private string GenerateRandomColor()
        {
            Random random = new Random();
            return String.Format("#{0:X6}", random.Next(0x1000000));
        }



    }
}
