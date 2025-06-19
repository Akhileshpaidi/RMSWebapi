using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using Org.BouncyCastle.Bcpg;
using DocumentFormat.OpenXml.Bibliography;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]

    public class UserLocationMappingController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public UserLocationMappingController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        //getting DepartmentMaster Detail by id
        [Route("api/userlocationmapping/GetuserlocationmappingDetails")]
        [HttpGet]
        public IEnumerable<userlocationmappingModel> GetuserlocationmappingDetails()
        {
            return this.mySqlDBContext.userlocationmappingModels.Where(x => x.user_location_mapping_status == "Active").ToList();
        }

        [Route("api/userlocationmapping/GetuserDetails")]
        [HttpGet]
        public IEnumerable<object> GetuserDetails()
        {
            var deatils = (from  userpermission in mySqlDBContext.UserPermissionModels
                           join userlocation in mySqlDBContext.userlocationmappingModels on  userpermission.user_location_mapping_id  equals userlocation.user_location_mapping_id
                           join tbluser in mySqlDBContext.usermodels on userlocation.USR_ID equals tbluser.USR_ID
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on userlocation.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                            join entitymaster in mySqlDBContext.UnitMasterModels on userlocation.Entity_Master_id equals entitymaster.Entity_Master_id
                           where userpermission.permissionstatus == "Active"              
                           select new
                           {
                               userpermission.doc_user_permission_mapping_pkid,
                               userlocation.Entity_Master_id,
                               entitymaster.Entity_Master_Name,
                               userlocation.Unit_location_Master_id,
                               unitlocation.Unit_location_Master_name,
                               userlocation.USR_ID,
                               tbluser.firstname,
                               userpermission.user_location_mapping_id
                           }
                )
                .GroupBy(x => x.user_location_mapping_id)
                .Select(g => g.First())
                .ToList();
            return deatils;
        }



        [Route("api/userlocationmapping/GetuserDetailsbyid/{unit_location_Master_id}")]
        [HttpGet]
        public IEnumerable<object> GetuserDetailsbyid(int unit_location_Master_id)
        {
            var deatils = (from userpermission in mySqlDBContext.UserPermissionModels
                           join userlocation in mySqlDBContext.userlocationmappingModels on userpermission.user_location_mapping_id equals userlocation.user_location_mapping_id
                           join tbluser in mySqlDBContext.usermodels on userlocation.USR_ID equals tbluser.USR_ID
                           join department in mySqlDBContext.DepartmentModels on tbluser.Department_Master_id equals department.Department_Master_id
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on userlocation.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on userlocation.Entity_Master_id equals entitymaster.Entity_Master_id
                           where userpermission.permissionstatus == "Active" && userlocation.Unit_location_Master_id == unit_location_Master_id
                           select new
                           {
                               userpermission.doc_user_permission_mapping_pkid,
                               userlocation.Entity_Master_id,
                               entitymaster.Entity_Master_Name,
                               userlocation.Unit_location_Master_id,
                               unitlocation.Unit_location_Master_name,
                               department.Department_Master_name,
                               userlocation.USR_ID,
                               tbluser.firstname,
                               userpermission.user_location_mapping_id
                           }
                )
                .GroupBy(x => x.user_location_mapping_id)
                .Select(g => g.First())
                .ToList();
            return deatils;
        }





        [Route("api/userlocationmapping/InsertuserlocationmappingDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] userlocationmappingModel userlocationmappingModels)
        {
            var userlocationmappingModel = this.mySqlDBContext.userlocationmappingModels;
            userlocationmappingModel.Add(userlocationmappingModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            userlocationmappingModels.user_location_mapping_createddate = dt1;
            userlocationmappingModels.user_location_mapping_status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }



        [Route("api/userlocationmapping/UpdateuserlocationmappingDetails")]
        [HttpPut]

        public void UpdateuserlocationmappingModel([FromBody] userlocationmappingModel userlocationmappingModels)
        {
            if (userlocationmappingModels.user_location_mapping_id == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(userlocationmappingModels);
                this.mySqlDBContext.Entry(userlocationmappingModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(userlocationmappingModels);

                Type type = typeof(userlocationmappingModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(userlocationmappingModels, null) == null || property.GetValue(userlocationmappingModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }
        [Route("api/userlocationmapping/userlocationmappingDetails")]
        [HttpDelete]

        public void Deleteuserlocationmapping(int id)
        {
            var currentClass = new userlocationmappingModel { user_location_mapping_id = id };
            currentClass.user_location_mapping_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("user_location_mapping_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

            var currentclass2 = new mapuserrolemodel { user_location_mapping_id = id };
            currentclass2.mapuserrolestatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("mapuserrolestatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        [Route("api/UserLocationMapping/GetuserDetailsbyUserID")]
        [HttpGet]

        public IEnumerable<object> GetuserDetailsbyUserID(int userid)
        {
            //  return this.mySqlDBContext.usermodels.Where(x => (x.USR_STATUS == "Inactive" || x.USR_STATUS == "Active")).ToList();
            var userDetails = from user in mySqlDBContext.usermodels
                              join mapUserRole in mySqlDBContext.mapuserrolemodels on user.USR_ID equals mapUserRole.USR_ID
                              join role in mySqlDBContext.RoleModels on mapUserRole.ROLE_ID equals role.ROLE_ID
                              join Department in mySqlDBContext.DepartmentModels on user.Department_Master_id equals Department.Department_Master_id
                              join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                              join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id

                              where user.USR_STATUS == "Inactive" || user.USR_STATUS == "Active" && user.USR_ID == userid
                              group role by new
                              {
                                  user.USR_ID,
                                  user.firstname,
                                  user.emailid,
                                  user.mobilenumber,
                                  user.roles,
                                  user.Designation,
                                  unitmaster.Unit_location_Master_name,
                                  user.USR_STATUS,
                                  entitymaster.Entity_Master_Name,
                                  Department.Department_Master_name
                              } into groupedRoles
                              orderby groupedRoles.Key.USR_ID // Add this line to order by user ID
                              select new
                              {
                                  usR_ID = groupedRoles.Key.USR_ID,
                                  firstname = groupedRoles.Key.firstname,
                                  Emailid = groupedRoles.Key.emailid,
                                  Mobilenumber = groupedRoles.Key.mobilenumber,
                                  Roles = groupedRoles.Key.roles,
                                  Designation = groupedRoles.Key.Designation,
                                  entityname = groupedRoles.Key.Entity_Master_Name,
                                  Unit_location_Master_name = groupedRoles.Key.Unit_location_Master_name,
                                  Department_Master_name = groupedRoles.Key.Department_Master_name,
                                  RoleNames = string.Join(",", groupedRoles.Select(RoleModel => RoleModel.ROLE_NAME)),
                                  usR_STATUS = groupedRoles.Key.USR_STATUS
                              };

            var result = userDetails.ToList();

            return result;



        }


        [Route("api/UserLocationMapping/GetuserDetailsbyUser/{unit_location_Master_id}")]
        [HttpGet]

        public IEnumerable<object> GetuserDetailsbyUser(int unit_location_Master_id)
        {

            var userDetails = from user in mySqlDBContext.userlocationmappingModels
                              join tbuser in mySqlDBContext.usermodels on user.USR_ID equals tbuser.USR_ID
                              join task in mySqlDBContext.TaskModels on user.taskID equals task.task_id
                              join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                              join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                              join Department in mySqlDBContext.DepartmentModels on tbuser.Department_Master_id equals Department.Department_Master_id
                              where user.user_location_mapping_status == "Active" && user.Unit_location_Master_id == unit_location_Master_id
                              select new
                              {
                                  tbuser.USR_ID,
                                  user.user_location_mapping_id,
                                  tbuser.firstname,
                                  user.user_location_mapping_End_Date,
                                  user.user_location_mapping_start_Date,
                                  unitmaster.Unit_location_Master_name,
                                  user.taskID,
                                  task.task_name,
                                  ROLE_ID = user.ROLE_ID,
                                  entitymaster.Entity_Master_Name,
                                  Department.Department_Master_name,
                                  user.user_location_mapping_status
                              };

            var detailsList = userDetails.ToList();
            var filteredDetailsList = detailsList
                                      .Where(detail => detail.ROLE_ID.Split(',').Select(role => role.Trim()).Contains("3"))
                                      .ToList();


            var roleIds = filteredDetailsList
        .SelectMany(d => d.ROLE_ID.Split(','))
        .Distinct()
        .ToList();
            var roles = mySqlDBContext.RoleModels
              .Where(u => roleIds.Contains(u.ROLE_ID.ToString()))
              .ToList()
              .GroupBy(u => u.ROLE_ID.ToString())
              .ToDictionary(g => g.Key, g => g.First());

            var result = filteredDetailsList.Select(d => new
            {
                d.user_location_mapping_id,
                roleNames = string.Join(",",
              d.ROLE_ID.Split(',').Select(id => roles.ContainsKey(id) ? roles[id].ROLE_NAME : "Unknown")),
                d.firstname,
                d.user_location_mapping_End_Date,
                d.user_location_mapping_start_Date,
                d.Unit_location_Master_name,
                d.taskID,
                d.task_name,
                d.ROLE_ID,
                d.Entity_Master_Name,
                d.Department_Master_name,
                d.user_location_mapping_status,
                d.USR_ID

            });

            return result.ToList();


        }



        [Route("api/AssessmentUserLocationMapping/AssessmentGetuserDetailsbyUser/{unit_location_Master_id}")]
        [HttpGet]

        public IEnumerable<object> AssessmentGetuserDetailsbyUser(int unit_location_Master_id)
        {
            var userDetails = from user in mySqlDBContext.userlocationmappingModels
                              join tbuser in mySqlDBContext.usermodels on user.USR_ID equals tbuser.USR_ID
                              join task in mySqlDBContext.TaskModels on user.taskID equals task.task_id
                              join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                              join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                              join Department in mySqlDBContext.DepartmentModels on tbuser.Department_Master_id equals Department.Department_Master_id
                              where user.user_location_mapping_status == "Active"
                                    && user.Unit_location_Master_id == unit_location_Master_id
                              select new
                              {
                                  user.user_location_mapping_id,
                                  tbuser.firstname,
                                  tbuser.USR_ID,
                                  user.user_location_mapping_End_Date,
                                  user.user_location_mapping_start_Date,
                                  unitmaster.Unit_location_Master_name,
                                  user.taskID,
                                  task.task_name,
                                  ROLE_ID = user.ROLE_ID,
                                  entitymaster.Entity_Master_Name,
                                  Department.Department_Master_name,
                                  user.user_location_mapping_status
                              };

            var detailsList = userDetails.ToList();

            // Filter to match ROLE_IDs 5, 7, or 8
            var filteredDetailsList = detailsList
                .Where(detail => detail.ROLE_ID.Split(',')
                .Select(role => role.Trim())
                .Any(role => new[] { "5", "7", "8" }.Contains(role)))
                .ToList();

            var roleIds = filteredDetailsList
                .SelectMany(d => d.ROLE_ID.Split(','))
                .Distinct()
                .ToList();

            var roles = mySqlDBContext.RoleModels
                .Where(u => roleIds.Contains(u.ROLE_ID.ToString()))
                .ToList()
                .GroupBy(u => u.ROLE_ID.ToString())
                .ToDictionary(g => g.Key, g => g.First());

            var result = filteredDetailsList.Select(d => new
            {
                d.user_location_mapping_id,
                roleNames = string.Join(",",
                    d.ROLE_ID.Split(',').Select(id => roles.ContainsKey(id) ? roles[id].ROLE_NAME : "Unknown")),
                    d.USR_ID,
                d.firstname,
                d.user_location_mapping_End_Date,
                d.user_location_mapping_start_Date,
                d.Unit_location_Master_name,
                d.taskID,
                d.task_name,
                d.ROLE_ID,
                d.Entity_Master_Name,
                d.Department_Master_name,
                d.user_location_mapping_status
            });

            return result.ToList();
        }


        // fetch locations based on userid 

        [Route("api/UserLocationMapping/GetuserlocationbyUser/{userid}")]
        [HttpGet]
        public IEnumerable<object> GetuserlocationbyUser(int userid)
        {
            // Step 1: Get all Unit_location_Master_id for the given user with Active mapping
            var userUnitLocationIds = mySqlDBContext.userlocationmappingModels
                .Where(u => u.USR_ID == userid && u.user_location_mapping_status == "Active")
                .Select(u => u.Unit_location_Master_id)
                .Distinct()
                .ToList();

            // Step 2: Use those IDs to filter in the main query
            var userDetails = from user in mySqlDBContext.userlocationmappingModels
                              join tbuser in mySqlDBContext.usermodels on user.USR_ID equals tbuser.USR_ID
                              join task in mySqlDBContext.TaskModels on user.taskID equals task.task_id
                              join entitymaster in mySqlDBContext.UnitMasterModels on user.Entity_Master_id equals entitymaster.Entity_Master_id
                              join unitmaster in mySqlDBContext.UnitLocationMasterModels on user.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                              join Department in mySqlDBContext.DepartmentModels on tbuser.Department_Master_id equals Department.Department_Master_id
                              where user.user_location_mapping_status == "Active"
                                    
                              select new
                              {
                                  tbuser.USR_ID,
                                  user.user_location_mapping_id,
                                  tbuser.firstname,
                                  user.user_location_mapping_End_Date,
                                  user.user_location_mapping_start_Date,
                                  unitmaster.Unit_location_Master_name,
                                  user.taskID,
                                  task.task_name,
                                  ROLE_ID = user.ROLE_ID,
                                  entitymaster.Entity_Master_Name,
                                  Department.Department_Master_name,
                                  user.user_location_mapping_status
                              };

            // Step 3: Convert to list and filter by role ID
            var detailsList = userDetails.ToList();
            var filteredDetailsList = detailsList
                .Where(detail => detail.ROLE_ID.Split(',').Select(role => role.Trim()).Contains("3"))
                .ToList();

            // Step 4: Extract all unique role IDs
            var roleIds = filteredDetailsList
                .SelectMany(d => d.ROLE_ID.Split(','))
                .Select(id => id.Trim())
                .Distinct()
                .ToList();

            // Step 5: Get corresponding role names
            var roles = mySqlDBContext.RoleModels
                .Where(r => roleIds.Contains(r.ROLE_ID.ToString()))
                .ToList()
                .GroupBy(r => r.ROLE_ID.ToString())
                .ToDictionary(g => g.Key, g => g.First());

            // Step 6: Final projection
            var result = filteredDetailsList.Select(d => new
            {
                d.user_location_mapping_id,
                roleNames = string.Join(",",
                    d.ROLE_ID.Split(',').Select(id => roles.ContainsKey(id.Trim()) ? roles[id.Trim()].ROLE_NAME : "Unknown")),
                d.firstname,
                d.user_location_mapping_End_Date,
                d.user_location_mapping_start_Date,
                d.Unit_location_Master_name,
                d.taskID,
                d.task_name,
                d.ROLE_ID,
                d.Entity_Master_Name,
                d.Department_Master_name,
                d.user_location_mapping_status,
                d.USR_ID
            });

            return result.ToList();
        }




    }
}
