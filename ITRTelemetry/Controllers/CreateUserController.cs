using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

    //[ApiController]
    public class CreateUserController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        ClsGlobal obj_ClsGlobal = new ClsGlobal();
        public CreateUserController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting Role Details

        [Route("api/createuser/getuserdetails")]
        [HttpGet]
        public IEnumerable<object> getuserdetails()
        {
            // return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active" && x.taskids =="1")).ToList();

            var username = (from user in mySqlDBContext.usermodels
                            where user.USR_STATUS == "Active" || user.USR_STATUS == "Inactive" && user.typeofuser == '1'
                            select new
                            {
                                user.USR_ID,
                                user.firstname
                            })
                           .ToList();
            return username;
        }
        [Route("api/createuser/getuserdetailsbyunit")]
        [HttpGet]
        public IEnumerable<object> getuserdetailsbyunit()
        {
            // return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active" && x.taskids =="1")).ToList();

            var username = (from user in mySqlDBContext.usermodels
                            join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                            join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                            where user.USR_STATUS == "Active" || user.USR_STATUS == "Inactive" && user.typeofuser == '1'
                            select new
                            {
                                user.USR_ID,
                                user.firstname,
                                entitymaster.Entity_Master_Name,
                                unitmaster.Unit_location_Master_name,
                                name = $" {user.firstname} / {entitymaster.Entity_Master_Name} / { unitmaster.Unit_location_Master_name}"
                            })
                           .ToList();
            return username;
        }

        [Route("api/createuserDetails/GetcreateuserDetails")]
        [HttpGet]

        public IEnumerable<object> GetcreateuserDetails()
        {
            //  return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active")).ToList();
            var userDetails = from user in mySqlDBContext.usermodels
                              join mapUserRole in mySqlDBContext.mapuserrolemodels on user.USR_ID equals mapUserRole.USR_ID
                              join role in mySqlDBContext.RoleModels on mapUserRole.ROLE_ID equals role.ROLE_ID
                              join Department in mySqlDBContext.DepartmentModels on user.Department_Master_id equals Department.Department_Master_id
                              join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                              join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                              join typeofusermaster in mySqlDBContext.typeofusermodels on user.typeofuser equals typeofusermaster.typeofuserid
                              where user.USR_STATUS == "Inactive" || user.USR_STATUS == "Active" && typeofusermaster.typeofusername == "internal"
                              group role by new
                              {
                                  user.USR_ID,
                                  user.firstname,
                                  user.emailid,
                                  user.mobilenumber,
                                  user.roles,
                                  user.Designation,
                                  unitmaster.Unit_location_Master_name,
                                  typeofusermaster.typeofusername,
                                  user.USR_STATUS,
                                  entitymaster.Entity_Master_Name,
                                  Department.Department_Master_name
                              } into groupedRoles
                              orderby groupedRoles.Key.USR_ID // Add this line to order by user ID
                              select new
                              {
                                  usR_ID = groupedRoles.Key.USR_ID,
                                  Name = groupedRoles.Key.firstname,
                                  Emailid = groupedRoles.Key.emailid,
                                  Mobilenumber = groupedRoles.Key.mobilenumber,
                                  Roles = groupedRoles.Key.roles,
                                  Designation = groupedRoles.Key.Designation,
                                  typeofuser = groupedRoles.Key.typeofusername,
                                  entityname =groupedRoles.Key.Entity_Master_Name,
                                  Unit_location_Master_name =groupedRoles.Key.Unit_location_Master_name,
                                  Department_Master_name = groupedRoles.Key.Department_Master_name,
                                   RoleNames = string.Join(",", groupedRoles.Select(RoleModel => RoleModel.ROLE_NAME)),
                                  usR_STATUS= groupedRoles.Key.USR_STATUS
                              };

            var result = userDetails.ToList();

            return result;



        }

        [Route("api/createuserDetails/GetIndividualUserData")]
        [HttpGet]
        public IEnumerable<usermodel> GetIndividualUserData(int userid)
        {
            return this.mySqlDBContext.usermodels.Where(x => (x.USR_ID == userid) && (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active")).ToList();
        }

        [Route("api/createuserDetails/GetUserRoles")]
        [HttpGet]
        public IEnumerable<mapuserrolemodel> GetUserRoles(int userid)
        {
            return this.mySqlDBContext.mapuserrolemodels.Where(x => (x.USR_ID == userid)).ToList();

        }
        [Route("api/UserName/GetUserName")]
        [HttpGet]
        public int GetUserName(string userlogin)
        {
            bool rowsExist = this.mySqlDBContext.usermodels.Any(x => x.USR_LOGIN == userlogin);

            return rowsExist ? 1 : 0;
        }



        [Route("api/createuserDetails/InsertcreateuserDetails")]
        [HttpPost]

        public async Task<IActionResult> InsertcreateuserDetails([FromBody] usermodel usermodels)
        {
            var usermodel = this.mySqlDBContext.usermodels;
            usermodel.Add(usermodels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            usermodels.CREATED_DATE = dt1;
            usermodels.USR_STATUS = "Active";
            await mySqlDBContext.SaveChangesAsync();

            var tasks = usermodels.taskids.Split(','); // Assuming idDefaultTaskuser is a comma-separated string.

            foreach (var taskId in tasks)
            {
                int taskIdInt;
                if (!int.TryParse(taskId, out taskIdInt))
                {
                  continue; 
                }

                var roleId = await mySqlDBContext.defualtmodulerolemodels
                .Where(x => x.task_id == taskIdInt)
               .Select(x => x.ROLE_ID)  
                  .FirstOrDefaultAsync();

                var userlocationmappingModel = new userlocationmappingModel
                {
                    Entity_Master_id = usermodels.Entity_Master_id,
                    Unit_location_Master_id = usermodels.Unit_location_Master_id,
                    USR_ID = usermodels.USR_ID,
                    taskID=taskIdInt,
                    ROLE_ID = roleId.ToString(), // Use the fetched Role_id here
                    user_location_mapping_createddate = usermodels.CREATED_DATE,
                    user_location_mapping_status = "Active"
                };
                mySqlDBContext.userlocationmappingModels.Add(userlocationmappingModel);
                await mySqlDBContext.SaveChangesAsync();

                var mapuserrolemodel = new mapuserrolemodel
                {
                    user_location_mapping_id = userlocationmappingModel.user_location_mapping_id,
                    ROLE_ID = roleId, // Use the fetched Role_id here
                    USR_ID = usermodels.USR_ID,
                    taskID = taskIdInt,
                    mapuserrolestatus = "Active"
                };
                mySqlDBContext.mapuserrolemodels.Add(mapuserrolemodel);
                await mySqlDBContext.SaveChangesAsync();
            }

            return Ok();
        }


            [Route("api/createuserDetails/DeletecreateuserDetails")]
        [HttpDelete]

        public void DeletecreateuserDetails(int userid)
        {
            var user = this.mySqlDBContext.usermodels.FirstOrDefault(u => u.USR_ID == userid);

            if (user != null)
            {

                user.USR_STATUS = user.USR_STATUS == "Active" ? "Inactive" : "Active";


                this.mySqlDBContext.Entry(user).Property("USR_STATUS").IsModified = true;


                this.mySqlDBContext.SaveChanges();

            }
        }


        [Route("api/createuserDetails/DeletecreateDetails")]
        [HttpDelete]

        public void DeletecreateDetails(int userid)
        {
            var currentClass = new usermodel { USR_ID = userid };
            currentClass.USR_STATUS = "Delete";
            this.mySqlDBContext.Entry(currentClass).Property("USR_STATUS").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        [Route("api/createuserDetails/UpdatecreateuserDetails")]
        [HttpPost]
        public void Updateusermodel([FromBody] usermodel usermodels)
        {
            if (usermodels.USR_ID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(usermodels);
                this.mySqlDBContext.Entry(usermodels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(usermodels);

                Type type = typeof(usermodel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(usermodels, null) == null || property.GetValue(usermodels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                // this.mySqlDBContext.SaveChanges();
                //var roles = usermodels.roles.Split(",");


                //var existingRoleEntries = mySqlDBContext.mapuserrolemodels.Where(x => x.USR_ID == usermodels.USR_ID);
                //mySqlDBContext.mapuserrolemodels.RemoveRange(existingRoleEntries);

                //mySqlDBContext.SaveChanges();

                //foreach (var roleId in roles)
                //{
                //    var mapuserrolemodel = new mapuserrolemodel
                //    {
                //        //USR_ID = usermodels.USR_ID,
                //        ROLE_ID = Convert.ToInt32(roleId),
                //        USR_ID = usermodels.USR_ID
                //    };
                //    mySqlDBContext.mapuserrolemodels.Add(mapuserrolemodel);
                //}

                mySqlDBContext.SaveChanges();
                // return Ok();
            }

        }





        [Route("api/updateuserDetails/UpdatecreateuserpasswordDetails")]
        [HttpPut]

        public IActionResult Updateuserpassword([FromBody] usermodel usermodels)
        {


            if (usermodels.USR_ID == 0)
            {
                // Handle the case when USR_ID is 0 (e.g., insert logic)
                // You may want to return an appropriate response or redirect
                return BadRequest("USR_ID cannot be 0 for an update operation.");
            }
            else
            {
                // Attach the entity to the context
              this.  mySqlDBContext.Attach(usermodels);

                // Update only the Password property
                this.mySqlDBContext.Entry(usermodels).Property("password").IsModified = true;

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
        [Route("api/updateuserDetails/UpdatecurrentuserpasswordDetails")]
        [HttpPost]

        public IActionResult UpdatecurrentuserpasswordDetails([FromBody] EncryptedRequestModel encryptedRequest)
        {
            try
            {
                // Decrypt the payload
                string decryptedPayload = obj_ClsGlobal.DecryptAES(encryptedRequest.RequestData);
                var payload = JsonConvert.DeserializeObject<ChangePasswordModel>(decryptedPayload);


                int userId = payload.userid;
              
                string oldencryptpass = obj_ClsGlobal.EncryptAES(payload.OldPassword);
                string newencryptpass = obj_ClsGlobal.EncryptAES(payload.NewPassword);
                string currentPassword = "";
                var tpauserid = payload.tpauserid;
                // Get session details
                //mySqlDBContext.ValidateAuthId(authId: payload.AuthId, userId: out userId);

                // Validate session

                if (tpauserid == null)
                {
                    if (userId == 0)
                    {
                        return Ok(new Response { ResponseCode = "7", ResponseDesc = "Invalid Session!!!", ResponseData = "" });
                    }

                    // Get current password
                    var userEntity = mySqlDBContext.usermodels.FirstOrDefault(u => u.USR_ID == userId);
                    if (userEntity != null)
                    {
                        currentPassword = userEntity.password;
                    }

                    // Check if the old password matches
                    if (currentPassword != oldencryptpass)
                    {
                        return Ok(new Response { ResponseCode = "1", ResponseDesc = "Incorrect old password!!!", ResponseData = "" });
                    }

                    // Check if the old and new passwords are the same
                    if (newencryptpass == oldencryptpass)
                    {
                        return Ok(new Response { ResponseCode = "6", ResponseDesc = "New and old password cannot be the same!!!", ResponseData = "" });
                    }

                    // Update the password to the new password
                    userEntity.password = newencryptpass;

                    // Save changes
                    mySqlDBContext.SaveChanges();

                    return Ok(new Response { ResponseCode = "0", ResponseDesc = "Your Password has been changed successfully.", ResponseData = "" });

                }
                else
                {

                    if (userId == 0)
                    {
                        return Ok(new Response { ResponseCode = "7", ResponseDesc = "Invalid Session!!!", ResponseData = "" });
                    }

                    // Get current password
                    var userEntity = mySqlDBContext.usermodels.FirstOrDefault(u => u.USR_ID == userId);
                    if (userEntity != null)
                    {
                        currentPassword = userEntity.password;
                    }

                    // Check if the old password matches
                    if (currentPassword != oldencryptpass)
                    {
                        return Ok(new Response { ResponseCode = "1", ResponseDesc = "Incorrect old password!!!", ResponseData = "" });
                    }

                    // Check if the old and new passwords are the same
                    if (newencryptpass == oldencryptpass)
                    {
                        return Ok(new Response { ResponseCode = "6", ResponseDesc = "New and old password cannot be the same!!!", ResponseData = "" });
                    }

                    // Update the password to the new password
                    userEntity.password = newencryptpass;

                    // Save changes
                    mySqlDBContext.SaveChanges();

              
                    var tpamodel = this.mySqlDBContext.tpausermodels.SingleOrDefault(u => u.tpauserid == tpauserid);

                    if (tpamodel != null)
                    {
                        tpamodel.password = userEntity.password;

                        // Mark the password property as modified
                        mySqlDBContext.Entry(tpamodel).Property("password").IsModified = true;

                        // Save changes to the usermodel table
                        mySqlDBContext.SaveChanges();
                    }



                    return Ok(new Response { ResponseCode = "0", ResponseDesc = "Your Password has been changed successfully.", ResponseData = "" });
                }

            }
            catch (Exception ex)
            {
                return Ok(new Response { ResponseCode = "2", ResponseDesc = "System Error", ResponseData = ex.Message });
            }
        }
        public class Response
        {
            public string ResponseCode { get; set; }
            public string ResponseDesc { get; set; }
            public string ResponseData { get; set; }
        }


    
    }
}
