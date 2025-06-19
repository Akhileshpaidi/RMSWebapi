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
using Newtonsoft.Json;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class ActivityWorkgroupcontroller : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public ActivityWorkgroupcontroller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/ActivityWorkgroup/GetActivityWorkgroupDetails")]
        [HttpGet]

        public IEnumerable<ActivityWorkgroupModel> GetActivityWorkgroupDetails()
        {
            return this.mySqlDBContext.ActivityWorkgroupModels.Where(x => x.status == "Active").ToList();
        }



        [Route("api/ActivityWorkgroup/GetActivityDetails")]
        [HttpGet]

        public IEnumerable<object> GetActivityDetails()
        {

            var details = ( from Activity in mySqlDBContext.ActivityWorkgroupModels

                           join  departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on Activity.locationdepartmentmappingid equals departmentlocation.locationdepartmentmappingid.ToString()
                            join unitlocation in mySqlDBContext.UnitLocationMasterModels  on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                            join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                            join role in mySqlDBContext.RoleModels on Activity.roles equals role.ROLE_ID
                            where Activity.status == "Active"
                           select new
                           {
                               Activity.activity_Workgroup_id,
                               Activity.name_ActivityWorkgroup,
                               Activity.unigueActivityid,
                               Activity.roles,
                               role.ROLE_NAME,
                               departmentlocation.locationdepartmentmappingid,
                               departmentlocation.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentlocation.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentlocation.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name
                           }).ToList();

            return details.Cast<object>();
        }


        [Route("api/ActivityWorkgroup/GetActivitybyid/{userid}")]
        [HttpGet]

        public IEnumerable<object> GetActivitybyid(int userid)
        {

            var details = (from Activity in mySqlDBContext.ActivityWorkgroupModels

                           join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on Activity.locationdepartmentmappingid equals departmentlocation.locationdepartmentmappingid.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           join role in mySqlDBContext.RoleModels on Activity.roles equals role.ROLE_ID
                           where Activity.status == "Active" && Activity.createdby == userid
                           select new
                           {
                               Activity.activity_Workgroup_id,
                               Activity.name_ActivityWorkgroup,
                               Activity.unigueActivityid,
                               Activity.roles,
                               role.ROLE_NAME,
                               departmentlocation.locationdepartmentmappingid,
                               departmentlocation.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentlocation.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentlocation.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name
                           }).ToList();

            return details.Cast<object>();
        }


        [Route("api/ActivityWorkgroup/GetActivityWorkgroupbyid/{activity_Workgroup_id}")]
        [HttpGet]

        public IEnumerable<object> GetuserActivityDetailsbyid(int activity_Workgroup_id)
        {
            var details = (from Activity in mySqlDBContext.ActivityWorkgroupModels

                           join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on Activity.locationdepartmentmappingid equals departmentlocation.locationdepartmentmappingid.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           join role in mySqlDBContext.RoleModels on Activity.roles equals role.ROLE_ID 
                           where Activity.status == "Active"  && Activity.activity_Workgroup_id == activity_Workgroup_id
                           select new
                           {
                               Activity.activity_Workgroup_id,
                               Activity.name_ActivityWorkgroup,
                               Activity.desc_ActivityWorkgroup,
                               Activity.unigueActivityid,
                               Activity.roles,
                               role.ROLE_NAME,
                               departmentlocation.locationdepartmentmappingid,
                               departmentlocation.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentlocation.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentlocation.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name
                           })
                          
                            .Distinct()
                .ToList();

            return details;
        }



        [Route("api/ActivityWorkgroup/InsertActivityWorkgroupDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] ActivityWorkgroupModel activityWorkgroupModels)
        {
            try
            {
                var departmentIds = activityWorkgroupModels.locationdepartmentmappingid.ToString();
                var departmentIdsArray = departmentIds.Split(',');

                var totalCount = this.mySqlDBContext.ActivityWorkgroupModels.Count();
                bool combinationExists = false; // Initialize combinationExists variable outside the loop

                foreach (var departmentId in departmentIdsArray)
                {
                    combinationExists = mySqlDBContext.ActivityWorkgroupModels
                        .Any(d => d.name_ActivityWorkgroup == activityWorkgroupModels.name_ActivityWorkgroup &&
                                  d.locationdepartmentmappingid == departmentId &&
                                  d.roles == activityWorkgroupModels.roles);

                    if (combinationExists)
                    {
                        // If combination exists, set combinationExists to true but continue processing other combinations
                        continue;
                    }
                    else
                    {
                        totalCount++;
                        var uniqueDefaultKey = GenerateDefaultKey(totalCount);

                        var newActivityWorkgroupModel = new ActivityWorkgroupModel
                        {
                            locationdepartmentmappingid = departmentId,
                            name_ActivityWorkgroup = activityWorkgroupModels.name_ActivityWorkgroup,
                            desc_ActivityWorkgroup = activityWorkgroupModels.desc_ActivityWorkgroup,
                            roles = activityWorkgroupModels.roles,
                            createdby = activityWorkgroupModels.createdby,
                            status = "Active",
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            unigueActivityid = uniqueDefaultKey
                        };

                        this.mySqlDBContext.ActivityWorkgroupModels.Add(newActivityWorkgroupModel);
                    }
                }

                // Save changes only if combination does not exist
                if (!combinationExists)
                {
                    this.mySqlDBContext.SaveChanges();
                    return Ok();
                }
                else
                {
                    return BadRequest("Error: Record already exists with the same combination of nameActivityWorkgroup, department, and role.");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        private string GenerateDefaultKey(int currentCount)
        {
           // var currentCount = mySqlDBContext.ActivityWorkgroupModels.Count() + 1;

            var mappingId = currentCount.ToString("000"); 
            return $"AW-{mappingId}";
        }






        [Route("api/ActivityWorkgroup/UpdateActivityWorkgroupDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] ActivityWorkgroupModel activityWorkgroupModels)
        {
            try
            {

                //ActivityWorkgroupModel activityWorkgroupModels = updateData.activityWorkgroupModels;

                bool combinationExists = mySqlDBContext.ActivityWorkgroupModels
                      .Any(d => d.name_ActivityWorkgroup == activityWorkgroupModels.name_ActivityWorkgroup &&
                                d.locationdepartmentmappingid == activityWorkgroupModels.locationdepartmentmappingid
                                && d.roles ==activityWorkgroupModels.roles);

                if (combinationExists)
                {
                    return BadRequest("Error: Record already exists with the same combination of nameActivityWorkgroup, department and role.");
                }
                else
                {

                    if (activityWorkgroupModels.activity_Workgroup_id == 0)
                    {
                        return Ok("Insertion Failed");
                    }
                    else
                    {
                        activityWorkgroupModels.name_ActivityWorkgroup = activityWorkgroupModels.name_ActivityWorkgroup?.Trim();
                        var existingactivityworkgroup = this.mySqlDBContext.ActivityWorkgroupModels
                          .FirstOrDefault(d => d.name_ActivityWorkgroup == activityWorkgroupModels.name_ActivityWorkgroup && d.activity_Workgroup_id != activityWorkgroupModels.activity_Workgroup_id && d.status == "Active");

                        if (existingactivityworkgroup != null)
                        {
                            // Department with the same name already exists, return an error message
                            return BadRequest("Error: UserActivity name  Name with the same name already exists.");
                        }
                        activityWorkgroupModels.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        activityWorkgroupModels.status = "Active";
                        // Existing department, update logic
                        this.mySqlDBContext.Attach(activityWorkgroupModels);
                        mySqlDBContext.Entry(activityWorkgroupModels).State = EntityState.Modified;
                        this.mySqlDBContext.SaveChanges();
                        return Ok("Update successful");
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

    }
}
