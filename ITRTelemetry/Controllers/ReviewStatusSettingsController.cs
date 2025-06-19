using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System;
using MySQLProvider;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Charts;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Drawing;
using System.Threading.Tasks;
using System.Linq;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class ReviewStatusSettingsController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ReviewStatusSettingsController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

        }
        [Route("api/ReviewStatusSettings/GetReviewStatusData")]
        [HttpGet]
        public ActionResult GetReviewStatusData()
        {
            var result = this.mySqlDBContext.ReviewStatusSettingsModels.ToList();
            return Ok(result);
        }


        [Route("api/ReviewStatusSettings/UpdateReviewStatus")]
        [HttpPost]
        public ActionResult UpdateReviewStatus([FromBody] List<ReviewStatusSettingsModel> ReviewStatusSettingsModels)

        {
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                string insertquery = "Update reviewstatussettings set ReviewStatusName=@ReviewStatusName,MinimumDays=@MinimumDays,MaximumDays=@MaximumDays,createdate=@createdate,Status=@Status where  ReviewStatusID = @ReviewStatusID";

                using (MySqlCommand myCommand = new MySqlCommand(insertquery, con))
                {
                    foreach (var model in ReviewStatusSettingsModels)
                    {
                        myCommand.Parameters.Clear();
                        myCommand.Parameters.AddWithValue("@ReviewStatusID", model.ReviewStatusID);

                        myCommand.Parameters.AddWithValue("@ReviewStatusName", model.ReviewStatusName);

                        myCommand.Parameters.AddWithValue("@MinimumDays", model.MinimumDays);
                        myCommand.Parameters.AddWithValue("@MaximumDays", model.MaximumDays);
                        myCommand.Parameters.AddWithValue("@createdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        myCommand.Parameters.AddWithValue("@Status", "Active");

                        myCommand.ExecuteNonQuery();
                    }
                }

                con.Close();
            }
            return Ok("Saved Successfully");


        }
    }
}