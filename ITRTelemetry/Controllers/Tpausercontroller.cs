using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DomainModel;
using ITR_TelementaryAPI;
using ITRTelemetry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]

    public class Tpausercontroller : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        ClsGlobal obj_ClsGlobal = new ClsGlobal();

        public Tpausercontroller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/createtpauserDetails/GetcreatetpauserDetails")]
        [HttpGet]

        public IEnumerable<object> GetcreatetpauserDetails()
        {
            //  return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active")).ToList();
            var userDetails = (from tpauser in mySqlDBContext.tpausermodels
                               join typeofusermaster in mySqlDBContext.typeofusermodels on tpauser.typeofuserid equals typeofusermaster.typeofuserid
                               join TPAEntitymaster in mySqlDBContext.TPAEntitymodels on tpauser.tpaenityid equals TPAEntitymaster.tpaenityid


                               where tpauser.status == "Inactive" || tpauser.status == "Active"
                               select new
                               {
                                   tpauser.tpauserid,
                                   tpauser.tpausername,
                                   tpauser.tpaemailid,
                                   TPAEntitymaster.tpaenityname,
                                   tpauser.tpamobilenumber,
                                   typeofusermaster.typeofusername,
                                   tpauser.designation,
                                   tpauser.status

                               })


            .ToList();

            return userDetails;



        }
        [Route("api/UserName/GettpaUserName")]
        [HttpGet]
        public int GetUserName(string userlogin)
        {
            bool tparowsExist = this.mySqlDBContext.tpausermodels.Any(x => x.USR_LOGIN == userlogin);
            bool userrowsExist = this.mySqlDBContext.usermodels.Any(x => x.USR_LOGIN == userlogin);

            //return rowsExist ? 1 : 0;
            if(tparowsExist || userrowsExist)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        [Route("api/UserName/GetIndividualtpaUserData")]
        [HttpGet]
        public IEnumerable<object> GetIndividualUserData(int userid)
        {
            return this.mySqlDBContext.tpausermodels.Where(x => (x.tpauserid == userid) && (x.status=="Active" || x.status == "Active")).ToList();
        }


        [Route("api/createtpauserDetails/InsertcreatetpauserDetails")]
        [HttpPost]

        public async Task<IActionResult> InsertcreatetpauserDetails([FromBody] tpamodel tpausermodels)
        {
            var tpamodel = this.mySqlDBContext.tpausermodels;
            tpamodel.Add(tpausermodels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            tpausermodels.createddate = dt1;
            tpausermodels.status = "Active";
            await mySqlDBContext.SaveChangesAsync();

            var usermodel = new usermodel
            {

                firstname = tpausermodels.tpausername,
                USR_LOGIN = tpausermodels.USR_LOGIN,
                emailid = tpausermodels.tpaemailid,
                mobilenumber = tpausermodels.tpamobilenumber,
                Designation = tpausermodels.tpadescription,
                tpaenityid = tpausermodels.tpaenityid,
                password = tpausermodels.password,
                USR_STATUS = tpausermodels.status,
                CREATED_DATE = tpausermodels.createddate,
                tpauserid = tpausermodels.tpauserid,
                defaultrole = tpausermodels.defaultrole,
                taskids = tpausermodels.taskids,
                typeofuser = tpausermodels.typeofuserid,
                isFirstLogin = 0

            };
            mySqlDBContext.usermodels.Add(usermodel);
            await mySqlDBContext.SaveChangesAsync();



            var userlocationmappingModel = new userlocationmappingModel
            {
                Entity_Master_id = usermodel.Entity_Master_id,
                Unit_location_Master_id = usermodel.Unit_location_Master_id,
                USR_ID = usermodel.USR_ID,
               taskID = int.Parse(usermodel.taskids),
                ROLE_ID = usermodel.defaultrole.ToString(), // Use the fetched Role_id here
                user_location_mapping_createddate = usermodel.CREATED_DATE,
                user_location_mapping_status = "Active"
            };
            mySqlDBContext.userlocationmappingModels.Add(userlocationmappingModel);
            await mySqlDBContext.SaveChangesAsync();

            var mapuserrolemodel = new mapuserrolemodel
            {
                user_location_mapping_id = userlocationmappingModel.user_location_mapping_id,
                ROLE_ID = usermodel.defaultrole, // Use the fetched Role_id here
                USR_ID = usermodel.USR_ID,
                taskID = int.Parse(usermodel.taskids),
                mapuserrolestatus = "Active"
            };
            mySqlDBContext.mapuserrolemodels.Add(mapuserrolemodel);
            await mySqlDBContext.SaveChangesAsync();

            return Ok();
        }

        [Route("api/createtauserDetails/UpdatetpacreateuserDetails")]
        [HttpPut]
        public void UpdatetpacreateuserDetails([FromBody] tpamodel tpausermodels)
        {
            if (tpausermodels.tpauserid == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(tpausermodels);
                this.mySqlDBContext.Entry(tpausermodels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(tpausermodels);

                Type type = typeof(tpamodel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(tpausermodels, null) == null || property.GetValue(tpausermodels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

            
                mySqlDBContext.SaveChanges();
                



                var tpauserid = tpausermodels.tpauserid;

                var usermodel = this.mySqlDBContext.usermodels.SingleOrDefault(u => u.tpauserid == tpauserid); 

                if (usermodel != null)
                {
                    usermodel.firstname = tpausermodels.tpausername;
                    usermodel.USR_LOGIN = tpausermodels.USR_LOGIN;
                    usermodel.emailid = tpausermodels.tpaemailid;

                    mySqlDBContext.Entry(usermodel).Property("firstname").IsModified = true;
                    mySqlDBContext.Entry(usermodel).Property("USR_LOGIN").IsModified = true;
                    mySqlDBContext.Entry(usermodel).Property("emailid").IsModified = true;
                    mySqlDBContext.SaveChanges();

                }
                else
                {
                    Console.WriteLine(" invalid tpauserid");
                }
            }

        }

        [Route("api/createtpauserDetails/DeletetpacreateDetails")]
        [HttpDelete]

        public void DeletetpacreateDetails(int userid)
        {
            var currentClass = new tpamodel { tpauserid = userid };
            currentClass.status = "Delete";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        [Route("api/createtpauserDetails/Getcreatetpauserbyid/{tpaenityid}")]
        [HttpGet]

        public IEnumerable<object> Getcreatetpauserbyid(int tpaenityid)
        {
            //  return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active")).ToList();
            var userDetails = (from tpauser in mySqlDBContext.tpausermodels
                               join typeofusermaster in mySqlDBContext.typeofusermodels on tpauser.typeofuserid equals typeofusermaster.typeofuserid
                               join TPAEntitymaster in mySqlDBContext.TPAEntitymodels on tpauser.tpaenityid equals TPAEntitymaster.tpaenityid


                               where  tpauser.status == "Active" && tpauser.tpaenityid == tpaenityid
                               select new
                               {
                                   tpauser.tpauserid,
                                   tpauser.tpausername,
                                   tpauser.tpaemailid,
                                   TPAEntitymaster.tpaenityname,
                                   tpauser.tpamobilenumber,
                                   typeofusermaster.typeofusername,

                               })


            .ToList();

            return userDetails;



        }
        [Route("api/createtpauserDetails/DeletecreatetpauserDetails")]
        [HttpDelete]

        public void DeletecreatetpauserDetails(int userid)
        {
            var user = this.mySqlDBContext.tpausermodels.FirstOrDefault(u => u.tpauserid == userid);

            if (user != null)
            {

                user.status = user.status == "Active" ? "Inactive" : "Active";


                this.mySqlDBContext.Entry(user).Property("status").IsModified = true;


                this.mySqlDBContext.SaveChanges();

            }
        }




        [Route("api/updatetpauserDetails/UpdatecreateusertpapasswordDetails")]
        [HttpPut]

        public IActionResult UpdatecreateusertpapasswordDetails([FromBody] tpamodel tpausermodels  )
        {


            if (tpausermodels.tpauserid == 0)
            {
                // Handle the case when USR_ID is 0 (e.g., insert logic)
                // You may want to return an appropriate response or redirect
                return BadRequest("USR_ID cannot be 0 for an update operation.");
            }
            else
            {
                // Attach the entity to the context
                this.mySqlDBContext.Attach(tpausermodels);

                // Update only the Password property
                this.mySqlDBContext.Entry(tpausermodels).Property("password").IsModified = true;
                mySqlDBContext.SaveChanges();
                var tpauserid = tpausermodels.tpauserid;

                var usermodel = this.mySqlDBContext.usermodels.SingleOrDefault(u => u.tpauserid == tpauserid);

                if (usermodel != null)
                {
                    // Update the password in usermodel
                    usermodel.password = tpausermodels.password;

                    // Mark the password property as modified
                    mySqlDBContext.Entry(usermodel).Property("password").IsModified = true;

                    // Save changes to the usermodel table
                    mySqlDBContext.SaveChanges();
                }
                else
                {
                    // Handle the case where the usermodel is not found
                    Console.WriteLine("UserModel not found for TpaUserId: " );
                }

                try
                {
                    // Save changes
                    this.mySqlDBContext.SaveChanges();
                    return Ok("Password updated successfully.");
                }
                catch (DbUpdateException ex)
                {
                    // Handle exceptions (e.g., unique constraint violation)
                    return BadRequest($"Error updating password: {ex.Message}");
                }
            }
        }
    }
}

