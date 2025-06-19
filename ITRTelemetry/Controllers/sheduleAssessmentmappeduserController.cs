using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Permissions;
using System.Net.Mail;
using System.Net;
using ITR_TelementaryAPI;
using Microsoft.Extensions.Logging;
using Google.Protobuf.WellKnownTypes;

namespace ITRTelemetry.Controllers

{
    public class sheduleAssessmentmappeduserController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        private ClsEmail obj_Clsmail = new ClsEmail();

        public sheduleAssessmentmappeduserController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/sheduledAssessmentmappeduser/InsertsheduledAssessmentmappeduser")]
        [HttpPost]

        public async Task<IActionResult> InsertsheduledAssessmentmappeduser([FromBody] sheduledAssessmentmappeduser sheduledAssessmentmappedusermodels)
        {
            try
            {
                var uniqueDefaultKey = GenerateDefaultKey();
                //var includes = sheduledAssessmentmappedusermodels.usr_ID.Split(',');
                //foreach (var usrIdinc in includes)
                {
                   // sheduledAssessmentmappedusermodels.usr_ID = usrIdinc;
                    var sheduledAssessmentmappeduser = this.mySqlDBContext.sheduledAssessmentmappedusermodels;

                    sheduledAssessmentmappeduser.Add(sheduledAssessmentmappedusermodels);



                    DateTime dt = DateTime.Now;
                    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    sheduledAssessmentmappedusermodels.created_date = dt1;
                    sheduledAssessmentmappedusermodels.status = "scopelocked";
                    sheduledAssessmentmappedusermodels.scheduledstatus = "Active";
                    sheduledAssessmentmappedusermodels.Scheduled_mapped_user_id = 0;
                    sheduledAssessmentmappedusermodels.defaultkey = uniqueDefaultKey.ToString();
                    mySqlDBContext.sheduledAssessmentmappedusermodels.Add(sheduledAssessmentmappedusermodels);
                    mySqlDBContext.SaveChangesAsync();



                }
                // TPA User
                string userEmail = "";
                string userName = "";
                int tpaUserId = sheduledAssessmentmappedusermodels.tpauserid;

                var user = await mySqlDBContext.tpausermodels
                    .Where(x => x.tpauserid == tpaUserId)
                    .Select(x => new tpamodel
                    {
                        tpaemailid = x.tpaemailid,
                        tpausername = x.tpausername
                    })
                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    userEmail = user.tpaemailid;
                    userName = user.tpausername;

                }



                // users
                //string users = sheduledAssessmentmappedusermodels.usr_ID;
                //string[] userIds = users.Split(',');
                //var userIds = includes;
                //List<string> firstNamesList = new List<string>();

                //foreach (var userId in userIds)
                //{
                //    int userIdInt = int.Parse(userId);
                //    var firstName = await mySqlDBContext.usermodels
                //        .Where(x => x.USR_ID == userIdInt)
                //        .Select(x => x.firstname)
                //        .FirstOrDefaultAsync();

                //    if (firstName != null)
                //    {
                //        firstNamesList.Add(firstName);
                //    }
                //}

                //// Entity Name
                //int entityMasterid = sheduledAssessmentmappedusermodels.entity_Master_id;

                //var entity = await mySqlDBContext.UnitMasterModels
                //    .Where(x => x.Entity_Master_id == entityMasterid)
                //    .Select(x => new UnitMasterModel
                //    {
                //        Entity_Master_Name = x.Entity_Master_Name
                //    })
                //    .FirstOrDefaultAsync();
                //string entityname = entity?.Entity_Master_Name;

                //// Unit Name
                //int UnitLocitionMasterid = sheduledAssessmentmappedusermodels.unit_location_Master_id;

                //var unit = await mySqlDBContext.UnitLocationMasterModels
                //    .Where(x => x.Unit_location_Master_id == UnitLocitionMasterid)
                //    .Select(x => new UnitLocationMasterModel
                //    {
                //        Unit_location_Master_name = x.Unit_location_Master_name
                //    })
                //    .FirstOrDefaultAsync();
                //string unitname = unit?.Unit_location_Master_name;

                obj_Clsmail.TpaMapedUsersLocked(userEmail, userName );


                //if (sheduledAssessmentmappedusermodels.excludedusrid != null)
                //{

                //    var excludes = sheduledAssessmentmappedusermodels.excludedusrid.Split(',');
                //    foreach (var usrId in excludes)
                //    {
                //        var Sheduledexcluededusersmodels = new Sheduledexcluededusersmodel
                //        {
                //            Exemption_user = int.Parse(usrId),  // Assuming USR_ID is present in existingEntity
                //            defaultkey = sheduledAssessmentmappedusermodels.defaultkey,
                //            excludeddescription = sheduledAssessmentmappedusermodels.exculededdescription,
                //            created_date = sheduledAssessmentmappedusermodels.created_date,
                //        };
                //        mySqlDBContext.Sheduledexcluededusersmodels.Add(Sheduledexcluededusersmodels);
                //        mySqlDBContext.SaveChangesAsync();
                //    }
                //        return Ok(new { Message = "Data inserted successfully" });
                //    }
                //else
                //    {
                        return Ok(new { Message = "Data inserted successfully" });
                    

                } 
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Error: {ex.Message}");

                // Return an error response
                return StatusCode(500, new { Error = "Internal Server Error" });
            }

            int GenerateDefaultKey()
            {
                // Retrieve the current count of records
                var currentCount = mySqlDBContext.sheduledAssessmentmappedusermodels.Count();

                // Increment the count for the next record
                return 10000 + currentCount;
            }
        }



        [Route("api/tpaexternalmappedunlockedscope/Insertstpaexternalmappedunlockedscope")]
        [HttpPost]

        public async Task<IActionResult> Insertstpaexternalmappedunlockedscope([FromBody] sheduledAssessmentmappeduser sheduledAssessmentmappedusermodels)
        {
            try
            {
                var uniqueDefaultKey = GenerateDefaultKey();
                //var includes = sheduledAssessmentmappedusermodels.usr_ID.Split(',');
                //foreach (var usrIdinc in includes)
                {
                   // sheduledAssessmentmappedusermodels.usr_ID = usrIdinc;
                    var sheduledAssessmentmappeduser = this.mySqlDBContext.sheduledAssessmentmappedusermodels;

                    sheduledAssessmentmappeduser.Add(sheduledAssessmentmappedusermodels);



                    DateTime dt = DateTime.Now;
                    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    sheduledAssessmentmappedusermodels.created_date = dt1;
                    sheduledAssessmentmappedusermodels.status = "scopeunlocked";
                    sheduledAssessmentmappedusermodels.scheduledstatus = "Active";
                    sheduledAssessmentmappedusermodels.Scheduled_mapped_user_id = 0;
                    sheduledAssessmentmappedusermodels.defaultkey = uniqueDefaultKey.ToString();
                    mySqlDBContext.sheduledAssessmentmappedusermodels.Add(sheduledAssessmentmappedusermodels);
                    mySqlDBContext.SaveChangesAsync();
                }
                // TPA User
                string userEmail = "";
                string userName = "";
                int tpaUserId = sheduledAssessmentmappedusermodels.tpauserid;

                var user = await mySqlDBContext.tpausermodels
                    .Where(x => x.tpauserid == tpaUserId)
                    .Select(x => new tpamodel
                    {
                        tpaemailid = x.tpaemailid,
                        tpausername = x.tpausername
                    })
                    .FirstOrDefaultAsync();
                if (user != null)
                {
                    userEmail = user.tpaemailid;
                    userName = user.tpausername;

                }
                // requesting person
                int rqstperson = int .Parse(sheduledAssessmentmappedusermodels.requstingperson);

                var rqstperson1 = await mySqlDBContext.usermodels
                    .Where(x => x.USR_ID == rqstperson)
                    .Select(x => new usermodel
                    {
                        firstname = x.firstname
                    })
                    .FirstOrDefaultAsync();
                string rqstname = rqstperson1?.firstname;

                var usertable = await mySqlDBContext.usermodels
                  .Where(x => x.tpauserid == tpaUserId)
                  .Select(x => new usermodel
                  {
                      USR_ID = x.USR_ID,
                     
                  })
                  .FirstOrDefaultAsync();

                int senderid = sheduledAssessmentmappedusermodels.riskAssesserid;
                int userid = usertable?.USR_ID ?? 0;
                string description = sheduledAssessmentmappedusermodels.tpaenitydescription;

                obj_Clsmail.TpaMapedUsersUnLocked( userEmail, description, rqstname, senderid, userid);

                //(string emailToAddress, string templateName, int senderid, int userId, string baseUrl)

                // users
                //string users = sheduledAssessmentmappedusermodels.usr_ID;
                //string[] userIds = users.Split(',');

                //int entityMasterid = sheduledAssessmentmappedusermodels.entity_Master_id;

                //var entity = await mySqlDBContext.UnitMasterModels
                //    .Where(x => x.Entity_Master_id == entityMasterid)
                //    .Select(x => new UnitMasterModel
                //    {
                //        Entity_Master_Name = x.Entity_Master_Name
                //    })
                //    .FirstOrDefaultAsync();
                //string entityname = entity?.Entity_Master_Name;

                //// Unit Name
                //int UnitLocitionMasterid = sheduledAssessmentmappedusermodels.unit_location_Master_id;

                //var unit = await mySqlDBContext.UnitLocationMasterModels
                //    .Where(x => x.Unit_location_Master_id == UnitLocitionMasterid)
                //    .Select(x => new UnitLocationMasterModel
                //    {
                //        Unit_location_Master_name = x.Unit_location_Master_name
                //    })
                //    .FirstOrDefaultAsync();
                //string unitname = unit?.Unit_location_Master_name;



                //if (sheduledAssessmentmappedusermodels.excludedusrid != null)
                //{
                //    var excludes = sheduledAssessmentmappedusermodels.excludedusrid.Split(',');
                //    foreach (var usrId in excludes)
                //    {
                //        var Sheduledexcluededusersmodels = new Sheduledexcluededusersmodel
                //        {
                //            Exemption_user = int.Parse(usrId),
                //            defaultkey = sheduledAssessmentmappedusermodels.defaultkey,
                //            excludeddescription = sheduledAssessmentmappedusermodels.exculededdescription,
                //            created_date = sheduledAssessmentmappedusermodels.created_date,
                //        };
                //        mySqlDBContext.Sheduledexcluededusersmodels.Add(Sheduledexcluededusersmodels);
                //        mySqlDBContext.SaveChangesAsync();

                //    }
                //    return Ok(new { Message = "Data inserted successfully" });
                //}
                //else
                //{
                return Ok(new { Message = "Data inserted successfully" });
                

            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Error: {ex.Message}");

                // Return an error response
                return StatusCode(500, new { Error = "Internal Server Error" });
            }
            int GenerateDefaultKey()
            {
                // Retrieve the current count of records
                var currentCount = mySqlDBContext.sheduledAssessmentmappedusermodels.Count();

                // Increment the count for the next record
                return 10000 + currentCount;
            }
        }
    }
}
