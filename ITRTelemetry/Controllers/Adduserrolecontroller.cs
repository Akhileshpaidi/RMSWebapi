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
using System.Data;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Spreadsheet;
using static ITRTelemetry.Controllers.Componentcontroller;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]

    //[ApiController]
    public class Adduserrolecontroller : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Adduserrolecontroller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;

        }


        //[Route("api/Adduserrole/InsertAdduserroleDetails")]
        //[HttpPost]

        //public IActionResult InsertAdduserroleDetails([FromBody] userlocationmappingModel payload)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var entity = new userlocationmappingModel
        //            {

        //                user_location_mapping_status = "Active",

        //                user_location_mapping_createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //                //  ROLE_ID = string.Join(",", payload.ROLE_ID),
        //                ROLE_ID = payload.ROLE_ID,
        //                USR_ID = payload.USR_ID,
        //                Unit_location_Master_id = payload.Unit_location_Master_id,
        //                Entity_Master_id = payload.Entity_Master_id,
        //                user_location_mapping_start_Date = payload.user_location_mapping_start_Date,
        //                user_location_mapping_End_Date = payload.user_location_mapping_End_Date
        //            };

        //            mySqlDBContext.userlocationmappingModels.Add(entity);
        //            mySqlDBContext.SaveChanges();

        //            var roleIds = payload.ROLE_ID.Split(',');

        //            foreach (var roleId in roleIds)
        //            {
        //                var mapuserrolemodel = new mapuserrolemodel
        //                {
        //                    USR_ID = entity.USR_ID,
        //                    ROLE_ID = int.Parse(roleId)
        //                };

        //                mySqlDBContext.mapuserrolemodels.Add(mapuserrolemodel);
        //            }

        //            mySqlDBContext.SaveChanges();

        //            return Ok(new { Message = "Data saved successfully" });
        //        }
        //        else
        //        {
        //            return BadRequest(new { Message = "Invalid model state" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception or handle it as needed
        //        return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
        //    }
        //}


        [Route("api/Adduserrole/UpdateAdduserroleDetails")]
        [HttpPut]
       
        public IActionResult UpdateAdduserroleDetails([FromBody] userlocationmappingModel payload)
        {

            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {

                if (ModelState.IsValid)
                {
                    var existingEntity = mySqlDBContext.userlocationmappingModels
     .FirstOrDefault(e => e.Unit_location_Master_id == payload.Unit_location_Master_id
                         && e.Entity_Master_id == payload.Entity_Master_id
                         && e.USR_ID == payload.USR_ID && e.taskID == payload.taskID);

                    if (existingEntity == null)
                    {
                        // If the entity does not exist, create a new one
                        var newEntity = new userlocationmappingModel
                        {
                            // Assign properties from the payload
                            user_location_mapping_id = payload.user_location_mapping_id,
                            ROLE_ID = payload.ROLE_ID,
                            USR_ID = payload.USR_ID,
                            Unit_location_Master_id = payload.Unit_location_Master_id,
                            Entity_Master_id = payload.Entity_Master_id,
                            taskID = payload.taskID,

                            user_location_mapping_createddate = dt1,
                            user_location_mapping_start_Date = payload.user_location_mapping_start_Date,
                            user_location_mapping_End_Date = payload.user_location_mapping_End_Date,
                            user_location_mapping_status = "Active"
                        };

                        mySqlDBContext.userlocationmappingModels.Add(newEntity);

                        mySqlDBContext.SaveChanges();


                        // Create mapuserrolemodel for the new entity
                        if (!string.IsNullOrEmpty(payload.ROLE_ID))
                        {
                            var roleIds = payload.ROLE_ID.Split(',');
                            foreach (var roleId in roleIds)
                            {
                                var mapuserrolemodel = new mapuserrolemodel
                                {
                                    USR_ID = newEntity.USR_ID,  // Assuming USR_ID is present in existingEntity
                                    ROLE_ID = int.Parse(roleId),
                                    taskID = newEntity.taskID,
                                    user_location_mapping_id = newEntity.user_location_mapping_id,
                                    mapuserrolestatus = "Active"
                                };
                                mySqlDBContext.mapuserrolemodels.Add(mapuserrolemodel);
                            }
                        }
                    }
                    else
                    {
                        if (existingEntity.ROLE_ID != payload.ROLE_ID)
                        {
                            // Get the existing role IDs
                            var existingRoleIds = mySqlDBContext.mapuserrolemodels
                                .Where(m => m.user_location_mapping_id == existingEntity.user_location_mapping_id)
                                .Select(m => m.ROLE_ID)
                                .ToList();

                            // Parse the new role IDs from the payload
                            var newRoleIds = !string.IsNullOrEmpty(payload.ROLE_ID) ?
                                payload.ROLE_ID.Split(',').Select(int.Parse).ToList() :
                                new List<int>();

                            // Identify roles to remove (roles present in existing but not in new)
                            var rolesToRemove = existingRoleIds.Except(newRoleIds).ToList();

                            // Remove roles that are not present in the new roles
                            var roleMappingsToRemove = mySqlDBContext.mapuserrolemodels
                                .Where(m => m.user_location_mapping_id == existingEntity.user_location_mapping_id &&
                                            rolesToRemove.Contains(m.ROLE_ID))
                                .ToList();

                            mySqlDBContext.mapuserrolemodels.RemoveRange(roleMappingsToRemove);

                            // Identify new roles to add (roles present in new but not in existing)
                            var rolesToAdd = newRoleIds.Except(existingRoleIds).ToList();

                            foreach (var roleId in rolesToAdd)
                            {
                                var mapuserrolemodel = new mapuserrolemodel
                                {
                                    USR_ID = existingEntity.USR_ID,  // Assuming USR_ID is present in existingEntity
                                    ROLE_ID = roleId,
                                    taskID = existingEntity.taskID,
                                    user_location_mapping_id = existingEntity.user_location_mapping_id,
                                    mapuserrolestatus = "Active"
                                };
                                mySqlDBContext.mapuserrolemodels.Add(mapuserrolemodel);
                            }
                        
                        existingEntity.ROLE_ID = payload.ROLE_ID;
                    }

                    existingEntity.Unit_location_Master_id = payload.Unit_location_Master_id;
                    existingEntity.Entity_Master_id = payload.Entity_Master_id;
                    existingEntity.taskID = payload.taskID;

                    existingEntity.user_location_mapping_start_Date = payload.user_location_mapping_start_Date;
                    existingEntity.user_location_mapping_End_Date = payload.user_location_mapping_End_Date;
                }

                mySqlDBContext.SaveChanges();

                return Ok(new { Message = "Data updated successfully" });
            }
                else
                {
                    return BadRequest(new { Message = "Invalid model state" });
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [Route("api/Adduserrole/GetAdduserroleDetails")]
        [HttpGet]

        public IEnumerable<object> GetAdduserroleDetails()
        
        {
            //  return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active")).ToList();
            var userDetails = from user in mySqlDBContext.userlocationmappingModels
                              join mapUserRole in mySqlDBContext.mapuserrolemodels on user.user_location_mapping_id equals mapUserRole.user_location_mapping_id
                              join role in mySqlDBContext.RoleModels on mapUserRole.ROLE_ID equals role.ROLE_ID
                              join tbuser in mySqlDBContext.usermodels on user.USR_ID equals tbuser.USR_ID
                              join task in mySqlDBContext.TaskModels on user.taskID equals task.task_id
                              join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                              join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id

                              where  user.user_location_mapping_status == "Active"
                              group role by new
                              {
                                 user.user_location_mapping_id,
                               
                                 tbuser.firstname,
                                 user.user_location_mapping_End_Date,
                                 user.user_location_mapping_start_Date,
                                  unitmaster.Unit_location_Master_name,
                                  user.taskID,
                                  task.task_name,
                                  entitymaster.Entity_Master_Name,

                                  user.user_location_mapping_status  // Include user location mapping status in the grouping key
                              } into groupedRoles
                              orderby groupedRoles.Key.user_location_mapping_id
                              select new
                              {

                                  user_location_mapping_id = groupedRoles.Key.user_location_mapping_id,
                                  Name = groupedRoles.Key.firstname,
                                  Roles = groupedRoles.Select(r => r.ROLE_ID),
                                  entityname = groupedRoles.Key.Entity_Master_Name,
                                  taskname = groupedRoles.Key.task_name,
                                  user_location_mapping_start_Date = groupedRoles.Key.user_location_mapping_start_Date,
                                  user_location_mapping_End_Date = groupedRoles.Key.user_location_mapping_End_Date,
                                  Unit_location_Master_name = groupedRoles.Key.Unit_location_Master_name,
                                  RoleNames = string.Join(",", groupedRoles.Select(r => r.ROLE_NAME)),
                                  user_location_mapping_status = groupedRoles.Select(u => u.ROLE_STATUS).FirstOrDefault()
                              };

            var result = userDetails.ToList();

            return result;



        }


        [Route("api/Adduserrole/deleteAdduserroleDetails")]
        [HttpDelete]

        public void deleteAdduserroleDetails(int id)
        {
            var currentClass = new userlocationmappingModel { user_location_mapping_id = id };
            currentClass.user_location_mapping_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("user_location_mapping_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        [Route("api/Adduserrole/GetAdduserroleDetailsforselected")]
        [HttpGet]

        public async Task<ActionResult<IEnumerable<RoleModel>>> GetAdduserroleDetailsforselected(int userId, int entityId, int unitLocationId,int taskid)
        {
            try
            {
                var userLocationMapping = await mySqlDBContext.userlocationmappingModels
                    .Where(ulm => ulm.USR_ID == userId && ulm.Entity_Master_id == entityId && ulm.Unit_location_Master_id == unitLocationId && ulm.taskID == taskid)
                    .FirstOrDefaultAsync();

                if (userLocationMapping == null)
                {
                    // Handle the case where user location mapping is not found
                    return NotFound("User location mapping not found");
                }

                // Assuming that roles are stored as a comma-separated string in ROLE_ID
                var roleIds = userLocationMapping.ROLE_ID?.Split(',').Select(int.Parse).ToList() ?? new List<int>();

                //var roles = await mySqlDBContext.mapuserrolemodels
                //    .Where(ur => roleIds.Contains(ur.ROLE_ID))
                //    .Select(ur => new RoleModel
                //    {
                //        ROLE_ID = ur.ROLE_ID,
                        
                //        // Map other properties as needed
                //    })
                //    .ToListAsync();

                return Ok(roleIds);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Route("api/Adduserrole/GetAdduserroleDetailsbyid/{user23}")]
        [HttpGet]

        public IEnumerable<object> GetAdduserroleDetailsbyid(int user23)

        {
            //  return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active")).ToList();
            var userDetails = from user in mySqlDBContext.userlocationmappingModels
                              join mapUserRole in mySqlDBContext.mapuserrolemodels on user.user_location_mapping_id equals mapUserRole.user_location_mapping_id
                              join role in mySqlDBContext.RoleModels on mapUserRole.ROLE_ID equals role.ROLE_ID
                              join tbuser in mySqlDBContext.usermodels on user.USR_ID equals tbuser.USR_ID
                              join task in mySqlDBContext.TaskModels on user.taskID equals task.task_id
                              join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                              join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id

                              where user.user_location_mapping_status == "Active" && user.USR_ID == user23
                              group role by new
                              {
                                  user.user_location_mapping_id,

                                  tbuser.firstname,
                                  user.user_location_mapping_End_Date,
                                  user.user_location_mapping_start_Date,
                                  unitmaster.Unit_location_Master_name,
                                  user.taskID,
                                  task.task_name,
                                  entitymaster.Entity_Master_Name,

                                  user.user_location_mapping_status  // Include user location mapping status in the grouping key
                              } into groupedRoles
                              orderby groupedRoles.Key.user_location_mapping_id
                              select new
                              {

                                  user_location_mapping_id = groupedRoles.Key.user_location_mapping_id,
                                  Name = groupedRoles.Key.firstname,
                                  Roles = groupedRoles.Select(r => r.ROLE_ID),
                                  entityname = groupedRoles.Key.Entity_Master_Name,
                                  taskname = groupedRoles.Key.task_name,
                                  user_location_mapping_start_Date = groupedRoles.Key.user_location_mapping_start_Date,
                                  user_location_mapping_End_Date = groupedRoles.Key.user_location_mapping_End_Date,
                                  Unit_location_Master_name = groupedRoles.Key.Unit_location_Master_name,
                                  RoleNames = string.Join(",", groupedRoles.Select(r => r.ROLE_NAME)),
                                  user_location_mapping_status = groupedRoles.Select(u => u.ROLE_STATUS).FirstOrDefault()
                              };

            var result = userDetails.ToList();

            return result;



        }


    }
}
