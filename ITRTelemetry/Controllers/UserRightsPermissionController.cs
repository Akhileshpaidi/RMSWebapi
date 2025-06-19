using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class UserRightsPermissionController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        public UserRightsPermissionController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }
        [Route("api/UserRightsPermission/GetUserRightsPermissionDetails")]
        [HttpGet]

        public IEnumerable<UserRightsPermissionModel> GetUserRightsPermissionDetails()
        {

            return this.mySqlDBContext.UserrightsModels.Where(x => x.Doc_perm_rights_status == "Active").ToList();
        }

        [Route("api/UserRightsPermission/GetUserRightsPermission/{user_location_mapping_id}/{doc_User_Access_mapping_id}")]
        [HttpGet]

        public IEnumerable<UserRightsResult> GetUserRightsPermission(int user_location_mapping_id, int doc_User_Access_mapping_id)
        {
            List<UserRightsResult> userRightsList = new List<UserRightsResult>();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    // First Query - Fetch Doc Permission Rights
                    string firstQuery = @"
                SELECT 
                    user_location_mapping_id, 
                    GROUP_CONCAT(DISTINCT Doc_perm_rights_id ORDER BY Doc_perm_rights_id ASC SEPARATOR ',') AS doc_perm_rights_id 
                FROM 
                    risk.doc_user_permission_mapping 
                WHERE 
                    user_location_mapping_id = @user_location_mapping_id 
                    AND Doc_User_Access_mapping_id = @doc_User_Access_mapping_id 
And  permissionstatus ='Active'
                GROUP BY 
                    user_location_mapping_id;";

                    using (MySqlCommand cmd = new MySqlCommand(firstQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@user_location_mapping_id", user_location_mapping_id);
                        cmd.Parameters.AddWithValue("@doc_User_Access_mapping_id", doc_User_Access_mapping_id);

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                // Map first query result
                                UserRightsResult userRightsResult = new UserRightsResult
                                {
                                    user_location_mapping_id = Convert.ToInt32(dt.Rows[0]["user_location_mapping_id"]),
                                    Doc_perm_rights_id = dt.Rows[0]["doc_perm_rights_id"].ToString()
                                };

                                // Second Query - Fetch Additional Information
                                string secondQuery = @"
                            SELECT 
                                ack_status, duedate, trakstatus, optionalreminder, noofdays, 
                                everyday, timeperiod, reqtimeperiod, validitydocument, startDate, endDate ,provideack_status
                            FROM 
                                risk.doc_taskuseracknowledment_status 
                            WHERE 
                                Doc_User_Access_mapping_id = @doc_User_Access_mapping_id
                                AND user_location_mapping_id = @user_location_mapping_id
                                AND status = 'Active';";

                                using (MySqlCommand secondCmd = new MySqlCommand(secondQuery, con))
                                {
                                    secondCmd.Parameters.AddWithValue("@doc_User_Access_mapping_id", doc_User_Access_mapping_id);
                                    secondCmd.Parameters.AddWithValue("@user_location_mapping_id", user_location_mapping_id);

                                    using (MySqlDataAdapter secondDa = new MySqlDataAdapter(secondCmd))
                                    {
                                        DataTable secondDt = new DataTable();
                                        secondDa.Fill(secondDt);

                                        if (secondDt.Rows.Count > 0)
                                        {
                                            DataRow secondRow = secondDt.Rows[0];

                                            // Fill the rest of the details
                                            userRightsResult.ack_status = secondRow["ack_status"]?.ToString();
                                            userRightsResult.duedate = secondRow["duedate"]?.ToString();
                                            userRightsResult.trakstatus = secondRow["trakstatus"]?.ToString();
                                            userRightsResult.optionalreminder = secondRow["optionalreminder"].ToString();
                                            userRightsResult.noofdays = Convert.ToInt32(secondRow["noofdays"]);
                                            userRightsResult.everyday = Convert.ToInt32(secondRow["everyday"]);
                                            userRightsResult.timeperiod = secondRow["timeperiod"]?.ToString();
                                            userRightsResult.reqtimeperiod = secondRow["reqtimeperiod"]?.ToString();
                                            userRightsResult.validitydocument = secondRow["validitydocument"]?.ToString();
                                            userRightsResult.startDate = secondRow["startDate"]?.ToString();
                                            userRightsResult.endDate = secondRow["endDate"]?.ToString();
                                            userRightsResult.provideack_status = secondRow["provideack_status"]?.ToString() ;
                                        }
                                    }
                                }

                                // Add the result to the list
                                userRightsList.Add(userRightsResult);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception here (use logging framework)
                Console.WriteLine("Error: " + ex.Message);
            }

            return userRightsList;
        } 
        [Route("api/UserRightsPermission/GetUserRightsPermissionDetails/{user_location_mapping_id}")]
        [HttpGet]
        public IEnumerable<object> GetUserRightsPermissionDetails(int user_location_mapping_id)

        {

            var details = (from userpermissionsmaster in mySqlDBContext.UserPermissionModels
                           join userrightspermissionmaster in mySqlDBContext.UserrightsModels on userpermissionsmaster.Doc_perm_rights_id equals userrightspermissionmaster.Doc_perm_rights_id
                           where userpermissionsmaster.user_location_mapping_id == user_location_mapping_id && userpermissionsmaster.permissionstatus =="Active"
                           select new
                           {
                               userpermissionsmaster.user_location_mapping_id,
                               userpermissionsmaster.USR_ID,
                               userrightspermissionmaster.Doc_perm_rights_id,
                               userrightspermissionmaster.publish_Name

                           })
                           .ToList();
            return details;


        }
    }

}
