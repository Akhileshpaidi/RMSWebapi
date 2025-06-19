using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using MySqlConnector;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using DomainModel;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Bibliography;
using System.Reflection;
using Google.Protobuf.Collections;
using Microsoft.CodeAnalysis;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DepartmentlocationmappingController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public DepartmentlocationmappingController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/departmentlocationmapping/GetdepartmentlocationmappingDetails")]
        [HttpGet]

        public IEnumerable<object> GetdepartmentlocationmappingDetails()
        {
            return this.mySqlDBContext.Departmentlocationmappingmodels.Where(x => x.status == "Active").ToList();

        }


        [Route("api/locationdepartmentmapping/GetlocationdepartmentmappingDetails")]
        [HttpGet]
        public IEnumerable<object> GetlocationdepartmentmappingDetails()
        
        
        {
            var details = (from departmentlocation in mySqlDBContext.Departmentlocationmappingmodels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           where departmentlocation.status == "Active"
                           select new
                           {
                               departmentlocation.locationdepartmentmappingid,
                               departmentlocation.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentlocation.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentlocation.departmentid,
                               departmentmaster.Department_Master_name,
                               departmentname = $"{departmentmaster.Department_Master_name}<{unitlocation.Unit_location_Master_name}><{entitymaster.Entity_Master_Name}>"
                           })
                          .ToList();

            return details;
        }




        [Route("api/locationdepartmentmapping/GetlocationdepartmentmappingDetailsbyid/{userid}")]
        [HttpGet]
        public IEnumerable<object> GetlocationdepartmentmappingDetailsbyid(int userid)


        {
            var details = (from departmentlocation in mySqlDBContext.Departmentlocationmappingmodels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           where departmentlocation.status == "Active" && departmentlocation.createdby == userid
                           select new
                           {
                               departmentlocation.locationdepartmentmappingid,
                               departmentlocation.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentlocation.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentlocation.departmentid,
                               departmentmaster.Department_Master_name,
                               departmentname = $"{departmentmaster.Department_Master_name}<{unitlocation.Unit_location_Master_name}><{entitymaster.Entity_Master_Name}>"
                           })
                          .ToList();

            return details;
        }

        //[Route("api/locationdepartmentmapping/GetDepartments")]
        //[HttpGet]
        //public IEnumerable<object> GetUnitLocation(int userid, [FromQuery] List<int> locitionids)

        //{
        //    //var entity = (from department in mySqlDBContext.DepartmentModels
        //    //              join departmentMapping in mySqlDBContext.Departmentlocationmappingmodels on department.Department_Master_id equals departmentMapping.departmentid
        //    //              where departmentMapping.USR_ID == userid
        //    //              && locitionids.Contains(departmentMapping.unitlocationid)
        //    //              select new
        //    //              {
        //    //                  departmentId = departmentMapping.Department_Master_id,
        //    //                  departmentName = department.Unit_location_Master_name
        //    //              })
        //    //       .Distinct()
        //    //       .ToList();
        //    //return entity;
        //    var entity = (from department in mySqlDBContext.DepartmentModels
        //                  join departmentMapping in mySqlDBContext.Departmentlocationmappingmodels
        //                    on (int?)department.Department_Master_id equals Convert.ToInt32(departmentMapping.departmentid)
        //                  where departmentMapping.createdby == userid
        //                  let unitLocationIdInt = int.TryParse(departmentMapping.unitlocationid, out int parsedId) ? parsedId : (int?)null
        //                  where unitLocationIdInt.HasValue && locitionids.Contains(unitLocationIdInt.Value)
        //                  select new
        //                  {
        //                      departmentId = department.Department_Master_id,
        //                      departmentName = department.Department_Master_name
        //                  })
        //                   .Distinct()
        //                   .ToList();

        //    return entity;


        //}
        [Route("api/locationdepartmentmapping/GetDepartments")]
        [HttpGet]
        public IEnumerable<object> GetUnitLocation(int userid, [FromQuery] List<int> locitionids)
        {
            // Step 1: Retrieve all relevant records from the database
            var entity = (from department in mySqlDBContext.DepartmentModels
                          join departmentMapping in mySqlDBContext.Departmentlocationmappingmodels
                          on department.Department_Master_id.ToString() equals departmentMapping.departmentid
                          select new
                          {
                              departmentId = department.Department_Master_id,
                              departmentName = department.Department_Master_name,
                              unitLocationId = departmentMapping.unitlocationid // Keep as string
                          })
                          .Distinct()
                          .ToList(); // Executes the query and brings the data into memory

            // Step 2: Filter in-memory based on the provided location IDs
            var filteredEntity = entity
                .Where(e => int.TryParse(e.unitLocationId, out int parsedId) && locitionids.Contains(parsedId))
                .Select(e => new
                {
                    e.departmentId,
                    // Uncomment the next line if you want to include departmentName in the result
                    e.departmentName 
                })
                .ToList();

            return filteredEntity;
        }





        [Route("api/locationdepartmentmapping/GetlocationdepartDetails/{userid}")]
        [HttpGet]
        public IEnumerable<object> GetlocationdepartDetails(int userid)
        {
            var details = (from departmentmapping in mySqlDBContext.Departmentlocationmappingmodels
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentmapping.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentmapping.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentmapping.entityid equals entitymaster.Entity_Master_id
                           where departmentmapping.status == "Active" && departmentmapping.createdby == userid
                           select new
                           {
                               departmentmapping.locationdepartmentmappingid,
                               departmentmapping.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               entitymaster.Entity_Master_Name,
                               departmentmapping.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                           })
                           .ToList();
            return details;
        
        }



        [Route("api/locationdepartmentmapping/GetdepartmentmappingDetailsbyid/{department_Master_id}")]
        [HttpGet]

        public IEnumerable<object> GetuserActivityDetailsbyid(int department_Master_id)
        {
            var details = (from departmentmapping in mySqlDBContext.Departmentlocationmappingmodels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentmapping.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on departmentmapping.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels on departmentmapping.entityid equals entitymaster.Entity_Master_id
                           where departmentmapping.status == "Active" && departmentmapping.departmentid == department_Master_id.ToString()
                           select new
                           {
                               departmentmapping.locationdepartmentmappingid,
                               departmentmapping.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentmapping.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentmapping.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name
                           })
                            .Distinct()
                .ToList();

            return details;
        }


        [Route("api/departmentlocationmapping/GetdepartmentmappingDetails/{userid}")]
        [HttpGet]

        public IEnumerable<object> GetdepartmentmappingDetails(int userid)
        {
       
            var details = (from departmentlocation in mySqlDBContext.Departmentlocationmappingmodels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels
                           on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels
                           on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels
                           on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           where departmentlocation.status == "Active" && departmentlocation.createdby == userid
                           select new
                           {
                               departmentlocation.locationdepartmentmappingid,
                               departmentlocation.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentlocation.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentlocation.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               departmentname =$"{departmentmaster.Department_Master_name}<{unitlocation.Unit_location_Master_name}><{entitymaster.Entity_Master_Name}>"
                           }).ToList();

            return details.Cast<object>();
        }
        [Route("api/departmentlocationmapping/GetdepartmentmappingDetails")]
        [HttpGet]

        public IEnumerable<object> GetdepartmentmappingDetails()
        {

            var details = (from departmentlocation in mySqlDBContext.Departmentlocationmappingmodels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels
                           on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels
                           on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                           join entitymaster in mySqlDBContext.UnitMasterModels
                           on departmentlocation.entityid equals entitymaster.Entity_Master_id
                           where departmentlocation.status == "Active"
                           select new
                           {
                               departmentlocation.locationdepartmentmappingid,
                               departmentlocation.entityid,
                               entitymaster.Entity_Master_Name,
                               departmentlocation.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentlocation.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               departmentname = $"{departmentmaster.Department_Master_name}<{unitlocation.Unit_location_Master_name}><{entitymaster.Entity_Master_Name}>"
                           }).ToList();

            return details.Cast<object>();
        }


        [Route("api/departmentlocationmapping/InsertdepartmentlocationmappingDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Departmentlocationmappingmodel Departmentlocationmappingmodels)
        {
            try
            {

         
                var unitLocationIds = Departmentlocationmappingmodels.unitlocationid.ToString();


                var locationIdsArray = unitLocationIds.Split(',');
                foreach (var unitLocationId in locationIdsArray)
                {

                    var existingRecord = this.mySqlDBContext.Departmentlocationmappingmodels.FirstOrDefault(
                        d => d.entityid == Departmentlocationmappingmodels.entityid &&
                             d.unitlocationid == unitLocationId &&
                             d.departmentid == Departmentlocationmappingmodels.departmentid);

                    if (existingRecord == null) // Check if record does not exist before adding
                    {
                        var departmentLocationMappingModel = new Departmentlocationmappingmodel
                        {
                            unitlocationid = unitLocationId,
                            entityid = Departmentlocationmappingmodels.entityid,
                            departmentid = Departmentlocationmappingmodels.departmentid,
                            createdby = Departmentlocationmappingmodels.createdby,
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "Active"
                        };

                        // Add the DepartmentLocationMappingModel object to the context
                        this.mySqlDBContext.Departmentlocationmappingmodels.Add(departmentLocationMappingModel);
                    }
                }
                // Save changes to the database
                this.mySqlDBContext.SaveChanges();

                return Ok();
            }

            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Activity with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/locationdepartmentmapping/InsertlocationdepartmentmappingDetails")]
        [HttpPost]
        public IActionResult InsertlocationdepartmentmappingDetails([FromBody] Departmentlocationmappingmodel Departmentlocationmappingmodels)
        {
            try
            {
                var departmentIdString = Departmentlocationmappingmodels.departmentid.ToString();
                var departmentIds = departmentIdString.Split(',');

                foreach (var departmentId in departmentIds)
                {
                    var existingRecord = this.mySqlDBContext.Departmentlocationmappingmodels.FirstOrDefault(
                        d => d.entityid == Departmentlocationmappingmodels.entityid &&
                             d.unitlocationid == Departmentlocationmappingmodels.unitlocationid &&
                             d.departmentid == departmentId);

                    if (existingRecord == null) // Check if record does not exist before adding
                    {
                        var departmentLocationMappingModel = new Departmentlocationmappingmodel
                        {
                            departmentid = departmentId,
                            unitlocationid = Departmentlocationmappingmodels.unitlocationid,
                            entityid = Departmentlocationmappingmodels.entityid,
                            createdby = Departmentlocationmappingmodels.createdby,
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "Active"
                        };

                        // Add the DepartmentLocationMappingModel object to the context
                        this.mySqlDBContext.Departmentlocationmappingmodels.Add(departmentLocationMappingModel);
                    }
                }

                // Save changes to the database (moved outside the loop to save all changes at once)
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





        [Route("api/locationdepartmentmapping/UpdatelocationdepartmentmappingDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] UpdateData2 updateData)
         {
            try
            {

                Departmentlocationmappingmodel Departmentlocationmappingmodels = updateData.Departmentlocationmappingmodels;

                bool combinationExists = this.mySqlDBContext.Departmentlocationmappingmodels
         .Any(d => d.departmentid == Departmentlocationmappingmodels.departmentid &&
                   d.unitlocationid == Departmentlocationmappingmodels.unitlocationid &&
                   d.entityid == Departmentlocationmappingmodels.entityid);

                if (combinationExists)
                {
                    return BadRequest("Error: Record already exists with the same combination of entity, unit location, and department.");
                }
                else
                {

                    if (Departmentlocationmappingmodels.locationdepartmentmappingid == 0)
                    {
                        return Ok("Insertion unsuccessful");
                    }
                    else
                    {
                        Departmentlocationmappingmodels.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        // Existing department, update logic
                        this.mySqlDBContext.Attach(Departmentlocationmappingmodels);
                        this.mySqlDBContext.Entry(Departmentlocationmappingmodels).State = EntityState.Modified;

                        var entry = this.mySqlDBContext.Entry(Departmentlocationmappingmodels);

                        Type type = typeof(Departmentlocationmappingmodel);
                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetValue(Departmentlocationmappingmodels, null) == null || property.GetValue(Departmentlocationmappingmodels, null).Equals(0))
                            {
                                entry.Property(property.Name).IsModified = false;
                            }
                        }

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
                    return BadRequest("Error: Combination already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/departmentlocation/getentityid/{userid}")]
        [HttpGet]

        public IEnumerable<object> getentityid(int userid)
        {
            var entity = (from usermapping in mySqlDBContext.userlocationmappingModels
                          join entitymaster in mySqlDBContext.UnitMasterModels on usermapping.Entity_Master_id equals entitymaster.Entity_Master_id
                          where usermapping.user_location_mapping_status == "Active" && entitymaster.Entity_Master_Status == "Active" && usermapping.USR_ID == userid
                          select new
                          {
                              Entity_Master_id = usermapping.Entity_Master_id,
                              USR_ID = usermapping.USR_ID,
                              Entity_Master_Name = entitymaster.Entity_Master_Name
                          })
                         .Distinct()
                         .ToList();
            return entity;

        }

        [Route("api/departmentlocation/getunitlocationid")]
        [HttpGet]
        public IEnumerable<object> getunitlocationid(int userid, int entityids)

        {
            var entity = (from usermapping in mySqlDBContext.userlocationmappingModels

                          join unitlocation in mySqlDBContext.UnitLocationMasterModels on usermapping.Unit_location_Master_id equals unitlocation.Unit_location_Master_id

                          where usermapping.user_location_mapping_status == "Active" && unitlocation.Unit_location_Master_Status == "Active"
                          && usermapping.USR_ID == userid &&  usermapping.Entity_Master_id == entityids
                         
                          select new
                          {
                              Entity_Master_id = usermapping.Entity_Master_id,
                              USR_ID = usermapping.USR_ID,
                              Unit_location_Master_id = unitlocation.Unit_location_Master_id,
                              Unit_location_Master_name = unitlocation.Unit_location_Master_name
                          })
                   .Distinct()
                   .ToList();
            return entity;
        }


    }
}
