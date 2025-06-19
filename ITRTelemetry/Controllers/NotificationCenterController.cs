using DocumentFormat.OpenXml.Wordprocessing;
using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Controllers
{
    public class NotificationCenterController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        private readonly MySqlDBContext mySqlDBContext;
        public NotificationCenterController(IConfiguration configuration, MySqlDBContext MySqlDBContext)
        {
            this.mySqlDBContext = MySqlDBContext;
            Configuration = configuration;
        }

        //  Getting inbox Mail Details
        [Route("api/NotificationCenterController/getInbox/{userId}")]
        [HttpGet]
        public IEnumerable<NotificationCenterModel> getInbox(int userId)
        {
            List<NotificationCenterModel> pdata = new List<NotificationCenterModel>();
            string getQuery = "select *,tu.firstname as senderName,tu.emailid as senderMail,tuser.firstname as receiverName,tuser.emailid as receiverMail from mailnotification mn left join tbluser tu on tu.USR_ID=mn.SenderID left join tbluser tuser on tuser.USR_ID=mn.ReceiverID where mn.ReceiverID=@userId ORDER BY mn.created_at DESC ";
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(getQuery, con))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    // cmd.Parameters.AddWithValue("RecevierStatus", "delivered");
                    try
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pdata.Add(new NotificationCenterModel
                                {
                                    MailNotificationID = reader["MailNotificationID"] != DBNull.Value ? Convert.ToInt32(reader["MailNotificationID"]) : 0,
                                    Subject = reader["Subject"] != DBNull.Value ? reader["Subject"].ToString() : "N/A",
                                    Body = reader["Body"] != DBNull.Value ? reader["Body"].ToString() : "N/A",
                                    SenderStatus = reader["SenderStatus"] != DBNull.Value ? reader["SenderStatus"].ToString() : "N/A",
                                    RecevierStatus = reader["RecevierStatus"] != DBNull.Value ? reader["RecevierStatus"].ToString() : "N/A",
                                    ThanksBody = reader["ThanksBody"] != DBNull.Value ? reader["ThanksBody"].ToString() : "N/A",
                                    created_at = reader.IsDBNull(reader.GetOrdinal("created_at")) ? "N/A" : Convert.ToDateTime(reader["created_at"]).ToString("dd MMMM yyyy hh:mm tt"),
                                    updated_at = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? "N/A" : Convert.ToDateTime(reader["updated_at"]).ToString("dd MMMM yyyy hh:mm tt"),
                                    senderName = reader["senderName"] != DBNull.Value ? reader["senderName"].ToString() : "N/A",
                                    receiverName = reader["receiverName"] != DBNull.Value ? reader["receiverName"].ToString() : "N/A",
                                    senderMail = reader["senderMail"] != DBNull.Value ? reader["senderMail"].ToString() : "N/A",
                                    receiverMail = reader["receiverMail"] != DBNull.Value ? reader["receiverMail"].ToString() : "N/A",
                                    favourite = reader["favourite"] != DBNull.Value ? Convert.ToInt32(reader["favourite"]) : 0,


                                });
                            }

                        }
                        return pdata.ToList();
                    }

                    catch (Exception ex)
                    {
                        return (IEnumerable<NotificationCenterModel>)BadRequest($"Error: {ex.Message}");
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }

        }

        //get outbox mail details
        [Route("api/NotificationCenterController/getOutbox/{userId}")]
        [HttpGet]
        public IEnumerable<NotificationCenterModel> getOutbox(int userId)
        {
            List<NotificationCenterModel> pdata = new List<NotificationCenterModel>();
            string getQuery = "select *,tu.firstname as senderName,tu.emailid as senderMail,tuser.firstname as receiverName,tuser.emailid as receiverMail from mailnotification mn left join tbluser tu on tu.USR_ID=mn.SenderID left join tbluser tuser on tuser.USR_ID=mn.ReceiverID where mn.SenderID=@userId and mn.SenderStatus=@SenderStatus ORDER BY mn.created_at DESC";
            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(getQuery, con))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    cmd.Parameters.AddWithValue("SenderStatus", "sent");
                    try
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pdata.Add(new NotificationCenterModel
                                {
                                    MailNotificationID = reader["MailNotificationID"] != DBNull.Value ? Convert.ToInt32(reader["MailNotificationID"]) : 0,
                                    Subject = reader["Subject"] != DBNull.Value ? reader["Subject"].ToString() : "N/A",
                                    Body = reader["Body"] != DBNull.Value ? reader["Body"].ToString() : "N/A",
                                    SenderStatus = reader["SenderStatus"] != DBNull.Value ? reader["SenderStatus"].ToString() : "N/A",
                                    RecevierStatus = reader["RecevierStatus"] != DBNull.Value ? reader["RecevierStatus"].ToString() : "N/A",
                                    ThanksBody = reader["ThanksBody"] != DBNull.Value ? reader["ThanksBody"].ToString() : "N/A",
                                    created_at = reader.IsDBNull(reader.GetOrdinal("created_at")) ? "N/A" : Convert.ToDateTime(reader["created_at"]).ToString("dd MMMM yyyy hh:mm tt"),
                                    updated_at = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? "N/A" : Convert.ToDateTime(reader["updated_at"]).ToString("dd MMMM yyyy hh:mm tt"),
                                    senderName = reader["senderName"] != DBNull.Value ? reader["senderName"].ToString() : "N/A",
                                    receiverName = reader["receiverName"] != DBNull.Value ? reader["receiverName"].ToString() : "N/A",
                                    senderMail = reader["senderMail"] != DBNull.Value ? reader["senderMail"].ToString() : "N/A",
                                    receiverMail = reader["receiverMail"] != DBNull.Value ? reader["receiverMail"].ToString() : "N/A",



                                });
                            }

                        }
                        return pdata;
                    }

                    catch (Exception ex)
                    {
                        return (IEnumerable<NotificationCenterModel>)BadRequest($"Error: {ex.Message}");
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }

        }



        [Route("api/NotificationCenterController/UpdateStatusAsRead")]
        [HttpPut]
        public IActionResult UpdateStatusAsRead([FromBody] NotificationCenterModel NotificationCenterModels)
        {
            string query = "Update mailnotification set RecevierStatus=@RecevierStatus where MailNotificationID=@MailNotificationID";

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                try
                {


                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@RecevierStatus", "read");
                        cmd.Parameters.AddWithValue("@MailNotificationID", NotificationCenterModels.MailNotificationID);
                        cmd.ExecuteNonQuery();
                    }
                    return Ok();

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


        //Add Favourite

        [Route("api/NotificationCenterController/AddFavourite")]
        [HttpPut]
        public IActionResult AddFavourite([FromBody] NotificationCenterModel NotificationCenterModels)
        {
            string query = "Update mailnotification set favourite=@favourite where MailNotificationID=@MailNotificationID";

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                try
                {


                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        if (NotificationCenterModels.favourite == 1) cmd.Parameters.AddWithValue("@favourite", 0);
                        else cmd.Parameters.AddWithValue("@favourite", 1);

                        cmd.Parameters.AddWithValue("@MailNotificationID", NotificationCenterModels.MailNotificationID);
                        cmd.ExecuteNonQuery();
                    }
                    return Ok();

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



        // for landing page 

        [Route("api/NotificationCenterController/getrecords/{userid}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationCenterModel>>> getrecords(int userid)
        {
            var top5Mails = await mySqlDBContext.NotificationCenterModels
          .AsNoTracking()
          .Where(m => m.SenderID == userid ||  (m.ReceiverID == userid))
          .OrderByDescending(m => m.created_at)
          .Select(m => new NotificationCenterModel
          {
              created_at = m.created_at,
              Subject = m.SenderID == userid ? "[Outbox] " + m.Subject :
                 m.ReceiverID == userid ? "[Inbox] " + m.Subject :
                 m.Subject
          })
          .Take(5)
          .ToListAsync();

            return Ok(top5Mails);
        }
    


    }
}
