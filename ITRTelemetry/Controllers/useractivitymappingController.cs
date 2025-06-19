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
using DocumentFormat.OpenXml.Spreadsheet;
using static ITRTelemetry.Controllers.Componentcontroller;
using System.Diagnostics;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class useractivitymappingController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public useractivitymappingController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/useractivitymapping/GetuseractivitymappingDetails")]
        [HttpGet]

        public IEnumerable<useractivitymapping> GetuseractivitymappingDetails()
        {
            return this.mySqlDBContext.useractivitymappingmodels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/useractivitymapping/GetuserActivityDetails")]
        [HttpGet]

        public IEnumerable<object> GetuserActivityDetails()
        {
            var details = (from useractivity in mySqlDBContext.useractivitymappingmodels
                           join Activity in mySqlDBContext.ActivityWorkgroupModels on useractivity.activityworkgroup_id equals Activity.activity_Workgroup_id
                           join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on Activity.locationdepartmentmappingid equals departmentlocation.locationdepartmentmappingid.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           join role in mySqlDBContext.RoleModels on Activity.roles equals role.ROLE_ID
                           join tbuser in mySqlDBContext.usermodels on useractivity.userid equals tbuser.USR_ID.ToString()
                           where useractivity.status == "Active"
                           select new
                           {
                               useractivity.user_workgroup_mapping_id,
                               useractivity.user_workgroup_mapping_name,
                               useractivity.user_workgroup_mapping_desc,
                               useractivity.useractivitymappingunigueid,
                               Activity.activity_Workgroup_id,
                               Activity.name_ActivityWorkgroup,
                               Activity.unigueActivityid,
                               role.ROLE_ID,
                               useractivity.userid,
                               role.ROLE_NAME,
                               tbuser.firstname,
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

        [Route("api/useractivitymapping/GetuserDetails")]
        [HttpGet]
        public IEnumerable<object> GetuserDetails()
        {
            var details = (from useractivity in mySqlDBContext.useractivitymappingmodels
                           join Activity in mySqlDBContext.ActivityWorkgroupModels on useractivity.activityworkgroup_id equals Activity.activity_Workgroup_id
                           join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on Activity.locationdepartmentmappingid equals departmentlocation.locationdepartmentmappingid.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           join role in mySqlDBContext.RoleModels on Activity.roles equals role.ROLE_ID

                           join tbuser in mySqlDBContext.usermodels on useractivity.userid equals tbuser.USR_ID.ToString()
                           where useractivity.status == "Active"
                           select new
                           {
                               tbuser.USR_ID,
                               useractivity.userid,
                               tbuser.firstname,
                               useractivity.user_workgroup_mapping_id,
                               useractivity.useractivitymappingunigueid,
                               useractivity.user_workgroup_mapping_name,
                               useractivity.user_workgroup_mapping_desc,
                               Activity.activity_Workgroup_id,
                               Activity.name_ActivityWorkgroup,
                               role.ROLE_NAME,
                               Activity.unigueActivityid,
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

            [Route("api/useractivitymapping/GetuserActivityDetailsbyid/{usR_ID}")]
        [HttpGet]

        public IEnumerable<object> GetuserActivityDetailsbyid(string usR_ID)
        {
            var details = (from useractivity in mySqlDBContext.useractivitymappingmodels
                           join tbuser in mySqlDBContext.usermodels on useractivity.userid equals tbuser.USR_ID.ToString()
                           join Activity in mySqlDBContext.ActivityWorkgroupModels on useractivity.activityworkgroup_id equals Activity.activity_Workgroup_id
                           join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on Activity.locationdepartmentmappingid equals departmentlocation.locationdepartmentmappingid.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           where useractivity.status == "Active" && useractivity.userid == usR_ID
                           select new
                           {
                               useractivity.user_workgroup_mapping_id,
                               useractivity.user_workgroup_mapping_name,
                               useractivity.user_workgroup_mapping_desc,
                               useractivity.useractivitymappingunigueid,
                               useractivity.activityworkgroup_id,
                               Activity.activity_Workgroup_id,
                               Activity.name_ActivityWorkgroup

                           } )
                            .Distinct()
                .ToList();

            return details;
        }



        [Route("api/ActivityWorkgroup/GetActivityDetailsbyid/{activityid}")]
        [HttpGet]

        public IEnumerable<object> GetActivityDetailsbyid(int activityid)
        {
            var details = (from Activity in mySqlDBContext.ActivityWorkgroupModels
                           join useractivity in mySqlDBContext.useractivitymappingmodels on Activity.activity_Workgroup_id equals useractivity.activityworkgroup_id
                           join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on Activity.locationdepartmentmappingid equals departmentlocation.locationdepartmentmappingid.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           join role in mySqlDBContext.RoleModels on Activity.roles equals role.ROLE_ID
                           where Activity.status == "Active" && Activity.activity_Workgroup_id == activityid
                           select new
                           {
                               useractivity.user_workgroup_mapping_id,
                               useractivity.activityworkgroup_id,
                               Activity.activity_Workgroup_id,
                               Activity.name_ActivityWorkgroup,
                               Activity.unigueActivityid,
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

        [Route("api/useractivitymapping/InsertuseractivitymappingDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] useractivitymapping useractivitymappingmodels)
        {
            try
            {

              
                    useractivitymappingmodels.user_workgroup_mapping_name = useractivitymappingmodels.user_workgroup_mapping_name?.Trim();
                    //var existinguseractivity = this.mySqlDBContext.useractivitymappingmodels
                    //    .FirstOrDefault(d => d.user_workgroup_mapping_name == useractivitymappingmodels.user_workgroup_mapping_name && d.status == "Active");

                    //if (existinguseractivity != null)
                    //{
                    //    // Department with the same name already exists, return an error message
                    //    return BadRequest("Error: user Activity mapping with the same name already exists.");
                    //}
                    // Proceed with the insertion

                    var users = useractivitymappingmodels.userid.ToString();
                    var usersArray = users.Split(',');
                    var totalCount = this.mySqlDBContext.useractivitymappingmodels.Count();

                    foreach (var user in usersArray)
                    {
                    bool combinationExists = mySqlDBContext.useractivitymappingmodels
                 .Any(d => d.user_workgroup_mapping_name == useractivitymappingmodels.user_workgroup_mapping_name &&
                           d.activityworkgroup_id == useractivitymappingmodels.activityworkgroup_id
                           && d.userid == user);

                    if (combinationExists)
                    {
                        //return BadRequest("Error: Record already exists with the same combination of nameActivityWorkgroup, department and role.");
                        continue;
                    }
                    else
                    {
                        totalCount++;

                        var uniqueDefaultKey = GenerateDefaultKey(totalCount);

                        var newuseractivitymapping = new useractivitymapping
                        {
                            userid = user,
                            user_workgroup_mapping_name = useractivitymappingmodels.user_workgroup_mapping_name,
                            user_workgroup_mapping_desc = useractivitymappingmodels.user_workgroup_mapping_desc,
                            createdby = useractivitymappingmodels.createdby,
                            activityworkgroup_id = useractivitymappingmodels.activityworkgroup_id,
                            status = "Active",
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            useractivitymappingunigueid = uniqueDefaultKey

                        };
                        this.mySqlDBContext.useractivitymappingmodels.Add(newuseractivitymapping);
                    }
                    }
                    this.mySqlDBContext.SaveChanges();
                    return Ok();
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
                return $"UW-{mappingId}";
            }


        [Route("api/useractivitymapping/UpdateuseractivitymappingDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] UpdateData1 updateData)
        {
            try
            {
               
                useractivitymapping useractivitymappingmodels = updateData.useractivitymappingmodels;

                if (useractivitymappingmodels.user_workgroup_mapping_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    useractivitymappingmodels.user_workgroup_mapping_name = useractivitymappingmodels.user_workgroup_mapping_name?.Trim();
                    var existinguseractivity = this.mySqlDBContext.useractivitymappingmodels
                      .FirstOrDefault(d => d.user_workgroup_mapping_name == useractivitymappingmodels.user_workgroup_mapping_name && d.user_workgroup_mapping_id != useractivitymappingmodels.user_workgroup_mapping_id && d.status == "Active");

                    if (existinguseractivity != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: UserActivity name  Name with the same name already exists.");
                    }
                    useractivitymappingmodels.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    // Existing department, update logic
                    this.mySqlDBContext.Attach(useractivitymappingmodels);
                    this.mySqlDBContext.Entry(useractivitymappingmodels).State = EntityState.Modified;
            
                    var entry = this.mySqlDBContext.Entry(useractivitymappingmodels);

                    Type type = typeof(useractivitymapping);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(useractivitymappingmodels, null) == null || property.GetValue(useractivitymappingmodels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Update successful");
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
