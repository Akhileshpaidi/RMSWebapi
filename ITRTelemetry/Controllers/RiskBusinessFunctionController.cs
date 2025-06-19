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
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using ITR_TelementaryAPI;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;



namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RiskBusinessFunctionController : Controller
    {
        private MySqlDBContext mySqlDBContext;
        ClsGlobal obj_ClsGlobal = new ClsGlobal();

        public RiskBusinessFunctionController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        [Route("api/RiskBusinessFunction/GetRiskBusinessFunction")]
        [HttpGet]
        public IEnumerable<object> GetRiskBusinessFunction()
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessfunction.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessfunction.entityid equals entitymaster.Entity_Master_id

                           where businessfunction.status == "Active" 
                           select new
                           {
                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               businessfunction.riskbusinessdescription,
                               businessfunction.entityid,
                               entitymaster.Entity_Master_Name,
                               businessfunction.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               businessfunction.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               riskname =$"{businessfunction.riskbusinessname}<{departmentmaster.Department_Master_name}><{entitymaster.Entity_Master_Name}><{unitlocation.Unit_location_Master_name}>"
                           })
                             .Distinct()
                 .ToList();

            return details;
        }


        [Route("api/RiskBusinessFunction/GetRiskBusinessFunctionDetails")]
        [HttpGet]

        public IEnumerable<object> GetRiskBusinessFunctionDetails()
        {

            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessfunction.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessfunction.entityid equals entitymaster.Entity_Master_id

                           where businessfunction.status == "Active"
                           select new
                           {
                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               businessfunction.riskbusinessdescription,
                               businessfunction.entityid,
                               entitymaster.Entity_Master_Name,
                               businessfunction.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               businessfunction.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               departmentname = $"{departmentmaster.Department_Master_name}<{entitymaster.Entity_Master_Name}><{unitlocation.Unit_location_Master_name}>"
                           }).ToList();

            return details.Cast<object>();
        }

        [Route("api/Department/GetDepartment")]
        [HttpGet]

        public IEnumerable<object> GetDepartment(int EntityMasterid, int Unitid)
        {

            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessfunction.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessfunction.entityid equals entitymaster.Entity_Master_id

                           where businessfunction.status == "Active" && entitymaster.Entity_Master_id == EntityMasterid
                           && unitlocation.Unit_location_Master_id == Unitid
                           select new
                           {
                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               businessfunction.riskbusinessdescription,
                               businessfunction.entityid,
                               entitymaster.Entity_Master_Name,
                               businessfunction.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,

                           })
                           .Distinct()
                           .ToList();

            var distinctDepartmentDetails = details
            .GroupBy(d => d.Department_Master_id)
            .Select(g => g.First()) // Keep only the first occurrence per departmentid
            .ToList();

            return distinctDepartmentDetails.Cast<object>();
        }

        [Route("api/Department/GetAllDepartments")]
        [HttpGet]

        public IEnumerable<object> GetAllDepartments()
        {

            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessfunction.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessfunction.entityid equals entitymaster.Entity_Master_id

                           where businessfunction.status == "Active"
                           select new
                           {
                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               businessfunction.riskbusinessdescription,
                               businessfunction.entityid,
                               entitymaster.Entity_Master_Name,
                               businessfunction.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,

                           })
                           .Distinct()
                           .ToList();

            var distinctDepartmentDetails = details
            .GroupBy(d => d.Department_Master_id)
            .Select(g => g.First()) // Keep only the first occurrence per departmentid
            .ToList();

            return distinctDepartmentDetails.Cast<object>();
        }

        [Route("api/Function/GetFunctionDetails")]
        [HttpGet]

        public IEnumerable<object> GetFunctionDetails(int EntityMasterid, int Unitid ,int Departmentid)
        {

            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessfunction.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessfunction.entityid equals entitymaster.Entity_Master_id

                           where businessfunction.status == "Active" && entitymaster.Entity_Master_id == EntityMasterid
                           && unitlocation.Unit_location_Master_id == Unitid && departmentmaster.Department_Master_id == Departmentid
                           select new
                           {
                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               businessfunction.riskbusinessdescription,
                               businessfunction.entityid,
                               entitymaster.Entity_Master_Name,
                               businessfunction.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               businessfunction.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                               deptname = $"{departmentmaster.Department_Master_name}<{unitlocation.Unit_location_Master_name}>",
                               departmentname = $"{departmentmaster.Department_Master_name}<{entitymaster.Entity_Master_Name}><{unitlocation.Unit_location_Master_name}>"
                           }).ToList();

            return details.Cast<object>();
        }


        [Route("api/RiskBusinessFunction/GetRiskBusinessFunctionid/{riskBusinessfunctionid}")]
        [HttpGet]

        public IEnumerable<object> GetRiskBusinessFunctionid(int riskBusinessfunctionid)
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessfunction.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessfunction.entityid equals entitymaster.Entity_Master_id

                           where businessfunction.status == "Active" && businessfunction.riskBusinessfunctionid == riskBusinessfunctionid
                           select new
                           {
                               businessfunction.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               businessfunction.riskbusinessdescription,
                               businessfunction.entityid,
                               entitymaster.Entity_Master_Name,
                               businessfunction.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               businessfunction.departmentid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name
                           })
                            .Distinct()
                .ToList();

            return details;
        }


        [Route("api/RiskBusinessFunction/InsertRiskBusinessFunctionDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] EncryptedRequestModel encryptedPayload )
        {
            try
            {

                var decryptedJson = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData);
                var RiskBusinessFunctionModels = JsonConvert.DeserializeObject<RiskBusinessFunctionModel>(decryptedJson);


                
                var unitLocationIds = RiskBusinessFunctionModels.unitlocationid.ToString();


                var locationIdsstringArray = unitLocationIds.Split(',');



                foreach (var unitLocationId in locationIdsstringArray)
                {

                    var existingRecord = this.mySqlDBContext.RiskBusinessFunctionModels.FirstOrDefault(
                        d => d.riskbusinessname == RiskBusinessFunctionModels.riskbusinessname &&
                        d.entityid == RiskBusinessFunctionModels.entityid &&
                             d.unitlocationid == RiskBusinessFunctionModels.unitlocationid &&
                             d.departmentid == RiskBusinessFunctionModels.departmentid &&
                             d.status == "Active");

                    if (existingRecord == null) // Check if record does not exist before adding
                    {
                        var RiskBusinessFunctionModel = new RiskBusinessFunctionModel
                        {
                            riskbusinessname = RiskBusinessFunctionModels.riskbusinessname,
                            riskbusinessdescription = RiskBusinessFunctionModels.riskbusinessdescription,
                            unitlocationid = unitLocationId,
                            entityid = RiskBusinessFunctionModels.entityid,
                            departmentid = RiskBusinessFunctionModels.departmentid,
                            createdby = RiskBusinessFunctionModels.createdby,
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "Active"
                        };

                        // Add the DepartmentLocationMappingModel object to the context
                        this.mySqlDBContext.RiskBusinessFunctionModels.Add(RiskBusinessFunctionModel);
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

        [Route("api/RiskBusinessFunction/DeleteRiskBusinessFunction")]
        [HttpDelete]

        public void DeleteRiskBusinessFunction(int id)
        {
            var currectclass = new RiskBusinessFunctionModel { riskBusinessfunctionid = id };
            currectclass.status = "Inactive";
            this.mySqlDBContext.Entry(currectclass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();


        }



        [Route("api/RiskBusinessFunction/UpdateRiskBusinessFunctionDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] EncryptedRequestModel encryptedPayload)
        {
            try
            {
                string decryptedData = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData); // Implement the DecryptAES method
                UpdateDatabusiness updateData = JsonConvert.DeserializeObject<UpdateDatabusiness>(decryptedData);


                RiskBusinessFunctionModel RiskBusinessFunctionModels = updateData.RiskBusinessFunctionModels;

                bool combinationExists = this.mySqlDBContext.RiskBusinessFunctionModels
         .Any(d => d.riskbusinessname == RiskBusinessFunctionModels.riskbusinessname &&
                        d.entityid == RiskBusinessFunctionModels.entityid &&
                             d.unitlocationid == RiskBusinessFunctionModels.unitlocationid &&
                             d.departmentid == RiskBusinessFunctionModels.departmentid &&
                             d.status == "Active");

                if (combinationExists)
                {
                    return BadRequest("Error: Record already exists with the same combination of entity, unit location, and department.");
                }
                else
                {

                    if (RiskBusinessFunctionModels.riskBusinessfunctionid == 0)
                    {
                        return Ok("Insertion Unsuccessful");
                    }
                    else
                    {
                        RiskBusinessFunctionModels.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        RiskBusinessFunctionModels.status = "Active";
                        // Existing department, update logic
                        this.mySqlDBContext.Attach(RiskBusinessFunctionModels);
                        this.mySqlDBContext.Entry(RiskBusinessFunctionModels).State = EntityState.Modified;

                        var entry = this.mySqlDBContext.Entry(RiskBusinessFunctionModels);

                        Type type = typeof(RiskBusinessFunctionModel);
                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetValue(RiskBusinessFunctionModels, null) == null || property.GetValue(RiskBusinessFunctionModels, null).Equals(0))
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

    }
}