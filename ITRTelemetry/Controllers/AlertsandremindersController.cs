using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace ITRTelemetry.Controllers
{

    [Produces("application/json")]
    public class AlertsandremindersController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public AlertsandremindersController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/AlertsAndreminder/GetAlertandreminder")]
        [HttpGet]

        public IEnumerable<object> GetAlertandreminder()
        {
            var alerts = (from alert in mySqlDBContext.Alertsandremindersmodels
                          join notification in mySqlDBContext.Notificationmailalerts on alert.notification equals notification.NotificationMailAlertid
                          select new
                          {
                              alerts_reminders_id = alert.alerts_reminders_id,
                              workactivity= alert.workactivity,
                              expectedDays=  alert.expectedDays,
                              expectedperiodicity= alert.expectedperiodicity,
                              expectedhoilday=  alert.expectedhoilday,
                              reminderalertdays  =  alert.reminderalertdays,
                              reminderalertperidicty = alert.reminderalertperidicty,
                              NotificationMailAlertid = notification.NotificationMailAlertid,
                              nameofAlert =  notification.NameofAlert,
                          })
                          .ToList();
            return alerts;
        }

        [Route("api/AlertsAndreminder/insertAlertandreminder")]
        [HttpPut]
        public IActionResult insertAlertandreminder([FromBody] List<Alertsandremindersmodel> pistdatalist)
        {
            try
            {
                foreach (var model in pistdatalist)
                {
                    var existingModel = mySqlDBContext.Alertsandremindersmodels
                        .FirstOrDefault(x => x.workactivity == model.workactivity);

                    if (existingModel != null)
                    {
                        // Update existing record
                        existingModel.expectedDays = model.expectedDays;
                        existingModel.expectedperiodicity = model.expectedperiodicity;
                        existingModel.expectedhoilday = model.expectedhoilday;
                        existingModel.reminderalertdays = model.reminderalertdays;
                        existingModel.reminderalertperidicty = model.reminderalertperidicty;
                        existingModel.notification = model.notification;
                        existingModel.createdby = model.createdby;
                        DateTime dt = DateTime.Now;
                        string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        existingModel.createddate = dt1;
                        existingModel.status = "Active";
                        mySqlDBContext.Alertsandremindersmodels.Update(existingModel);
                    }
                    else
                    {

                        var newModel = new Alertsandremindersmodel
                        {
                            workactivity = model.workactivity,
                            expectedDays = model.expectedDays,
                            expectedperiodicity = model.expectedperiodicity,
                            expectedhoilday = model.expectedhoilday,
                            reminderalertdays = model.reminderalertdays,
                            reminderalertperidicty = model.reminderalertperidicty,
                            notification = model.notification,
                            createdby = model.createdby,
                            status = "Active",
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    };

                        mySqlDBContext.Alertsandremindersmodels.Add(newModel);
                    }
                }

               
                mySqlDBContext.SaveChanges();

                return Ok("Data inserted or updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (logging code would go here)

                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [Route("api/AlertsAndreminder/GetApproveAlertReminders")]
        [HttpGet]
        public IActionResult GetApproveAlertReminders()
        {
            try
            {
                List<Alertsandremindersmodel> alertReminders = new List<Alertsandremindersmodel>();

                using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open(); 
                    string query = @"SELECT expectedDays, expectedhoilday 
                             FROM alerts_reminders 
                             WHERE workactivity = 'Review'"; 

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader()) 
                        {
                            while (reader.Read())
                            {
                                Alertsandremindersmodel reminder = new Alertsandremindersmodel
                                {
                                    expectedDays = reader.GetInt32("expectedDays"),
                                    expectedhoilday = reader.GetBoolean("expectedhoilday") 
                                };
                                alertReminders.Add(reminder);
                            }
                        }
                    }
                }

                return Ok(alertReminders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}

    

