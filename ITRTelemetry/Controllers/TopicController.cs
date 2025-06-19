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
    public class TopicController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public TopicController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }



        [Route("api/topic/GettopicDetailsById/{Subject_id}")]
        [HttpGet]

        public IEnumerable<TopicModel> GetTopicModelDetailsbyId(int Subject_id)
        {

            return this.mySqlDBContext.TopicModels.Where(x => x.Topic_Status == "Active" && x.Subject_id == Subject_id).ToList();
        }

        //Getting topic Details by topicID

        [Route("api/topic/GettopicDetails")]
        [HttpGet]

        public IEnumerable<TopicModel> GettopicDetails()
        {


            return this.mySqlDBContext.TopicModels.Where(x => x.Topic_Status == "Active").ToList();
        }


        

        //Insert Subject  Details

        [Route("api/topic/InserttopicDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] TopicModel TopicModels)
        {

            try
            {

                TopicModels.Topic_Name = TopicModels.Topic_Name?.Trim();
                var existingDepartment = this.mySqlDBContext.TopicModels
                    .FirstOrDefault(d => d.Topic_Name == TopicModels.Topic_Name && d.Subject_id == TopicModels.Subject_id && d.Topic_Status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Topic name with the same name already exists.");
                }
                var TopicModel = this.mySqlDBContext.TopicModels;
            TopicModel.Add(TopicModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            TopicModels.Topic_createddate = dt1;
            TopicModels.Topic_Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
 }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Topic name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }


        }

        [Route("api/topic/UpdatetopicDetails")]
        [HttpPut]
        public IActionResult Updatetopic([FromBody] TopicModel TopicModels)
        {
            try
            {
                if (TopicModels.Topic_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    TopicModels.Topic_Name = TopicModels.Topic_Name?.Trim();
                    var existingDepartment = this.mySqlDBContext.TopicModels
                  .FirstOrDefault(d => d.Topic_Name == TopicModels.Topic_Name && d.Subject_id != TopicModels.Subject_id && d.Topic_Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Topic with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(TopicModels);
                    this.mySqlDBContext.Entry(TopicModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(TopicModels);

                    Type type = typeof(TopicModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(TopicModels, null) == null || property.GetValue(TopicModels, null).Equals(0))
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
                    return BadRequest("Error: Topic with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

    



        [Route("api/topic/DeletetopicDetails")]
        [HttpDelete]
        public void Deletetopic(int id)
        {
            var currentClass = new TopicModel { Topic_id = id };
            currentClass.Topic_Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Topic_Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        [Route("api/topic/GettopicDetailsByMultipleIds")]
        [HttpGet]
        public IActionResult GettopicDetailsByMultipleIds([FromQuery] int[] selectedSubjectIDs)
        {
            List<TopicModel> topicDetails = new List<TopicModel>();
           MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            try{
                con.Open();
                foreach (int subjectId in selectedSubjectIDs)
                {

                    MySqlCommand cmd = new MySqlCommand("SELECT Topic_id,Topic_Name, topic.Subject_id,Subject_Name FROM topic inner join subject on subject.Subject_id=topic.Subject_id WHERE topic.Subject_id = @Subject_id order by subject.Subject_id asc", con);
                    cmd.Parameters.AddWithValue("@Subject_id", subjectId);
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                   {
                      DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                           foreach (DataRow row in dt.Rows)
                            {
                             topicDetails.Add(new TopicModel
                             {
                               Topic_id = Convert.ToInt32(row["Topic_id"]),
                               Topic_Name = row["Topic_Name"].ToString()+" - "+ row["Subject_Name"].ToString(),
                               Subject_id = Convert.ToInt32(row["Subject_id"])

                              });

                            }
                        }
                    }


                }

            }
          catch (Exception ex)
          {
           return StatusCode(500, "Internal server error");
          }
          finally
          {
             con.Close();

          }
           return Ok(topicDetails);

        }
    }
}
