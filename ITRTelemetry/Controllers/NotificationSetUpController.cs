using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class NotificationSetUpController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationSetUpController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        //public IEnumerable<object> ViewNotificationDetail(int riskMasterID)
        //{
        //    var details = (from notification in mySqlDBContext.Risk_NotificationSetUps
        //                   where notification.Status == "Active" && notification.RiskRegisterMasterID == riskMasterID
        //                   select new
        //                   {
        //                       notification.RisknotificationsetupID,
        //                       notification.EscalationStatus,
        //                       notification.RiskRegisterMasterID,
        //                       notification.EnterDays,
        //                       notification.DefaultNotifiers,
        //                       notification.AdditionalNotifiers,
        //                       notification.EnterComb,

        //                   })
        //                     .Distinct()
        //         .ToList();

        //    return details;
        //}
        [Route("api/ViewNotification/ViewNotificationDetail/{riskMasterID}")]
        [HttpGet]
        
        public object ViewNotificationDetail(int riskMasterID)
        {
            var notifications = (from notification in mySqlDBContext.Risk_NotificationSetUps
                                 where notification.Status == "Active" && notification.RiskRegisterMasterID == riskMasterID
                                 select new
                                 {
                                     notification.RisknotificationsetupID,
                                     notification.EscalationStatus,
                                     notification.RiskRegisterMasterID,
                                     notification.EnterDays,
                                     notification.DefaultNotifiers,
                                     notification.AdditionalNotifiers,
                                     notification.EnterComb
                                 }).ToList();

            // Map notifications to a flat object with properties suffixed by EscalationStatus
            var result = new
            {
                RisknotificationsetupID1 = notifications.FirstOrDefault(n => n.EscalationStatus == 1)?.RisknotificationsetupID,
                EscalationStatus1 = notifications.FirstOrDefault(n => n.EscalationStatus == 1)?.EscalationStatus,
                RiskRegisterMasterID1 = notifications.FirstOrDefault(n => n.EscalationStatus == 1)?.RiskRegisterMasterID,
                EnterDays1 = notifications.FirstOrDefault(n => n.EscalationStatus == 1)?.EnterDays,
                DefaultNotifiers1 = notifications.FirstOrDefault(n => n.EscalationStatus == 1)?.DefaultNotifiers,
                AdditionalNotifiers1 = notifications.FirstOrDefault(n => n.EscalationStatus == 1)?.AdditionalNotifiers,
                EnterComb1 = notifications.FirstOrDefault(n => n.EscalationStatus == 1)?.EnterComb,

                RisknotificationsetupID2 = notifications.FirstOrDefault(n => n.EscalationStatus == 2)?.RisknotificationsetupID,
                EscalationStatus2 = notifications.FirstOrDefault(n => n.EscalationStatus == 2)?.EscalationStatus,
                RiskRegisterMasterID2 = notifications.FirstOrDefault(n => n.EscalationStatus == 2)?.RiskRegisterMasterID,
                EnterDays2 = notifications.FirstOrDefault(n => n.EscalationStatus == 2)?.EnterDays,
                DefaultNotifiers2 = notifications.FirstOrDefault(n => n.EscalationStatus == 2)?.DefaultNotifiers,
                AdditionalNotifiers2 = notifications.FirstOrDefault(n => n.EscalationStatus == 2)?.AdditionalNotifiers,
                EnterComb2 = notifications.FirstOrDefault(n => n.EscalationStatus == 2)?.EnterComb,

                RisknotificationsetupID3 = notifications.FirstOrDefault(n => n.EscalationStatus == 3)?.RisknotificationsetupID,
                EscalationStatus3 = notifications.FirstOrDefault(n => n.EscalationStatus == 3)?.EscalationStatus,
                RiskRegisterMasterID3 = notifications.FirstOrDefault(n => n.EscalationStatus == 3)?.RiskRegisterMasterID,
                EnterDays3 = notifications.FirstOrDefault(n => n.EscalationStatus == 3)?.EnterDays,
                DefaultNotifiers3 = notifications.FirstOrDefault(n => n.EscalationStatus == 3)?.DefaultNotifiers,
                AdditionalNotifiers3 = notifications.FirstOrDefault(n => n.EscalationStatus == 3)?.AdditionalNotifiers,
                EnterComb3 = notifications.FirstOrDefault(n => n.EscalationStatus == 3)?.EnterComb,

                RisknotificationsetupID4 = notifications.FirstOrDefault(n => n.EscalationStatus == 4)?.RisknotificationsetupID,
                EscalationStatus4 = notifications.FirstOrDefault(n => n.EscalationStatus == 4)?.EscalationStatus,
                RiskRegisterMasterID4 = notifications.FirstOrDefault(n => n.EscalationStatus == 4)?.RiskRegisterMasterID,
                EnterDays4 = notifications.FirstOrDefault(n => n.EscalationStatus == 4)?.EnterDays,
                DefaultNotifiers4 = notifications.FirstOrDefault(n => n.EscalationStatus == 4)?.DefaultNotifiers,
                AdditionalNotifiers4 = notifications.FirstOrDefault(n => n.EscalationStatus == 4)?.AdditionalNotifiers,
                EnterComb4 = notifications.FirstOrDefault(n => n.EscalationStatus == 4)?.EnterComb
            };

            return result;
        }


        [Route("api/Notification/SaveNotificationDetails")]
        [HttpPost]
        public IActionResult SaveNotificationDetails([FromBody] List<NotificationSetUpModel> notificationSetUpModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "insert into notificationsetup(AddDoc_id,Document_Id,USR_ID,review_start_Date,EscalationStatus,EnterDays,DefaultNotifiers,AdditionalNotifiers,Status,CreatedDate,EnterComb)values(@AddDoc_id,@Document_Id,@USR_ID,@review_start_Date,@EscalationStatus,@EnterDays,@DefaultNotifiers,@AdditionalNotifiers,@Status,@CreatedDate,@EnterComb)";

            try
            {
                con.Open();

                foreach (var model in notificationSetUpModels)
                {
                    using (MySqlCommand myCommand = new MySqlCommand(insertQuery, con))
                    {
                        myCommand.Parameters.AddWithValue("@EscalationStatus", model.EscalationStatus);
                        myCommand.Parameters.AddWithValue("@EnterDays", model.EnterDays);
                        myCommand.Parameters.AddWithValue("@DefaultNotifiers", model.DefaultNotifiers);
                        myCommand.Parameters.AddWithValue("@AdditionalNotifiers", model.AdditionalNotifiers);
                        myCommand.Parameters.AddWithValue("@EnterComb", model.EnterComb);
                       myCommand.Parameters.AddWithValue("@AddDoc_id", model.AddDoc_id);
                        myCommand.Parameters.AddWithValue("@Document_Id", model.Document_Id);
                        myCommand.Parameters.AddWithValue("@USR_ID", model.USR_ID);
                       myCommand.Parameters.AddWithValue("@review_start_Date", model.review_start_Date);
                        myCommand.Parameters.AddWithValue("@Status", "Active");
                        myCommand.Parameters.AddWithValue("@CreatedDate", System.DateTime.Now);
                        myCommand.ExecuteNonQuery();
                    }
                }

                return Ok("added successfully");
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

        [Route("api/RiskNotification/AddRiskNotificationDetails")]
        [HttpPost]
        public IActionResult AddRiskNotificationDetails([FromBody] List<Risk_NotificationSetUp> Risk_NotificationSetUps)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "insert into risk_notificationsetup(RiskRegisterMasterID,USR_ID,review_start_Date,EscalationStatus,EnterDays,DefaultNotifiers,AdditionalNotifiers,Status,CreatedDate,EnterComb)values(@RiskRegisterMasterID,@USR_ID,@review_start_Date,@EscalationStatus,@EnterDays,@DefaultNotifiers,@AdditionalNotifiers,@Status,@CreatedDate,@EnterComb)";

            try
            {
                con.Open();

                foreach (var model in Risk_NotificationSetUps)
                {
                    using (MySqlCommand myCommand = new MySqlCommand(insertQuery, con))
                    {
                        myCommand.Parameters.AddWithValue("@EscalationStatus", model.EscalationStatus);
                        myCommand.Parameters.AddWithValue("@EnterDays", model.EnterDays);
                        myCommand.Parameters.AddWithValue("@DefaultNotifiers", model.DefaultNotifiers);
                        myCommand.Parameters.AddWithValue("@AdditionalNotifiers", model.AdditionalNotifiers);
                        myCommand.Parameters.AddWithValue("@EnterComb", model.EnterComb);
                        myCommand.Parameters.AddWithValue("@RiskRegisterMasterID", model.RiskRegisterMasterID);
                        myCommand.Parameters.AddWithValue("@USR_ID", model.USR_ID);
                        myCommand.Parameters.AddWithValue("@review_start_Date", model.review_start_Date);
                        myCommand.Parameters.AddWithValue("@Status", "Active");
                        myCommand.Parameters.AddWithValue("@CreatedDate", System.DateTime.Now);
                        myCommand.ExecuteNonQuery();
                    }
                }

                return Ok("added successfully");
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


    }
}
