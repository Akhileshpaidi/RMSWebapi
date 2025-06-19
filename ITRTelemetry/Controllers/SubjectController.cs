using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class SubjectController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public IConfiguration Configuration { get; }

        public SubjectController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        //Getting Subject Details by SubjectID

        [Route("api/Subject/GetSubjectDetails")]
        [HttpGet]

        public IEnumerable<SubjectModel> GetSubjectDetails()
        {

            return this.mySqlDBContext.SubjectModels.Where(x => x.Subject_Status == "Active").ToList();
        }


        //Insert Subject  Details

        [Route("api/Subject/InsertSubjectDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SubjectModel SubjectModels)
        {
            try
            {
                SubjectModels.Subject_Name = SubjectModels.Subject_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.SubjectModels
                    .FirstOrDefault(d => d.Subject_Name == SubjectModels.Subject_Name && d.Subject_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Subject name with the same name already exists.");
                }
                // Proceed with the insertion
                var SubjectModel = this.mySqlDBContext.SubjectModels;
            SubjectModel.Add(SubjectModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            SubjectModels.Subject_CreatedDate = dt1;
            SubjectModels.Subject_Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Subject name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/Subject/UpdateSubjectDetails")]
        [HttpPut]
        public IActionResult UpdateSubject([FromBody] SubjectModel SubjectModels)
        {
            try
            {
                if (SubjectModels.Subject_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SubjectModels.Subject_Name = SubjectModels.Subject_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.SubjectModels
                  .FirstOrDefault(d => d.Subject_Name == SubjectModels.Subject_Name && d.Subject_id != SubjectModels.Subject_id && d.Subject_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Subject name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(SubjectModels);
                    this.mySqlDBContext.Entry(SubjectModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(SubjectModels);

                    Type type = typeof(SubjectModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SubjectModels, null) == null || property.GetValue(SubjectModels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Subject name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

    



        [Route("api/Subject/DeleteSubjectDetails")]
        [HttpDelete]
        public void DeleteSubject(int id)
        {
            var currentClass = new SubjectModel { Subject_id = id };
            currentClass.Subject_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Subject_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        [Route("api/Subject/GetSubjectTopic")]
        [HttpGet]
        public IEnumerable<GetSubjectsByTopics> GetSubjectTopic()
        {
            var pdata = new List<GetSubjectsByTopics>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(@"
         SELECT 
         Subject_id,
         Subject_Name
         FROM subject
  where Subject_Status='Active'", con);


                cmd.CommandType = CommandType.Text;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int subjectid = Convert.ToInt32(dt.Rows[i]["Subject_id"].ToString());

                        MySqlCommand cmd1 = new MySqlCommand("SELECT Subject_id, Topic_id,Topic_Name FROM topic where Subject_id='" + subjectid + "' and Topic_Status='Active'", con);

                        cmd1.CommandType = CommandType.Text;

                        MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                        DataTable dt1 = new DataTable();
                        da1.Fill(dt1);


                        List<Topics> topicslist = new List<Topics>();

                        if (dt1.Rows.Count > 0)
                        {

                            for (int j = 0; j < dt1.Rows.Count; j++)
                            {
                                Topics opt = new Topics();
                                opt.Subject_id = Convert.ToInt32(dt1.Rows[j]["Subject_id"].ToString());
                                opt.Topic_id = Convert.ToInt32(dt1.Rows[j]["Topic_id"].ToString());
                                opt.Topic_Name = dt1.Rows[j]["Topic_Name"].ToString();

                                topicslist.Add(opt);
                            }

                            pdata.Add(new GetSubjectsByTopics
                            {
                                Subject_id = Convert.ToInt32(dt.Rows[i]["Subject_id"].ToString()),
                                Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                                topics = topicslist
                            });
                        }


                    }
                }

            }

            catch (Exception ex)
            {
                con.Close();
            }

            finally
            {
                con.Close();
            }

            return pdata;
        }



    }


}

