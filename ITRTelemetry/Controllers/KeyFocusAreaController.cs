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
using Newtonsoft.Json;
using ITR_TelementaryAPI;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class KeyFocusAreaController : Controller
    {
        private MySqlDBContext mySqlDBContext;
        ClsGlobal obj_ClsGlobal = new ClsGlobal();

        public KeyFocusAreaController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/KeyFocusArea/GetentityKeyFocusArea")]
        [HttpGet]
        public IEnumerable<object> GetentityKeyFocusArea()
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join entitymaster in mySqlDBContext.UnitMasterModels on businessfunction.entityid equals entitymaster.Entity_Master_id
                           where businessfunction.status == "Active"
                           select new
                           {
                               businessfunction.entityid,
                               entitymaster.Entity_Master_id,
                               entitymaster.Entity_Master_Name,
                           })
                             .Distinct()
                 .ToList();

            return details;
        }

        [Route("api/KeyFocusArea/GetunitKeyFocusArea/{entityid}")]
        [HttpGet]
        public IEnumerable<object> GetunitKeyFocusArea(int entityid)
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on businessfunction.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                           where businessfunction.status == "Active" && businessfunction.entityid == entityid
                           select new
                           {
                               businessfunction.entityid,
                               unitlocation.Unit_location_Master_id,
                               unitlocation.Unit_location_Master_name,
                           })
                             .Distinct()
                 .ToList();

            return details;
        }


        [Route("api/KeyFocusArea/GetdeptKeyFocusArea/{unitid}")]
        [HttpGet]
        public IEnumerable<object> GetdeptKeyFocusArea(int unitid)
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                           join departmentmaster in mySqlDBContext.DepartmentModels on businessfunction.departmentid equals departmentmaster.Department_Master_id
                           where businessfunction.status == "Active" && businessfunction.unitlocationid == unitid.ToString()
                           select new
                           {
                               businessfunction.entityid,
                               departmentmaster.Department_Master_id,
                               departmentmaster.Department_Master_name,
                           })
                             .Distinct()
                 .ToList();

            return details;
        }



        [Route("api/KeyFocusArea/GetbusiKeyFocusArea/{depid}")]
        [HttpGet]
        public IEnumerable<object> GetbusiKeyFocusArea(int depid)
        {
            var details = (from businessfunction in mySqlDBContext.RiskBusinessFunctionModels 
                           where businessfunction.status == "Active" && businessfunction.departmentid == depid
                           select new
                           {
                              
                               businessfunction.riskBusinessfunctionid,
                                businessfunction.riskbusinessname,

                           })
                             .Distinct()
                 .ToList();

            return details;
        }


        [Route("api/KeyFocusArea/GetbusprocesseiKeyFocusArea/{businessdunctionid}")]
        [HttpGet]
        public IEnumerable<object> GetbusprocesseiKeyFocusArea(int businessdunctionid)
        {
            var details = (from businessprocess in mySqlDBContext.Risk_BusinessProcesss
                           where businessprocess.BuinessProcessStatus == "Active" && businessprocess.departmentid == businessdunctionid
                           select new
                           {

                               businessprocess.businessprocessID,
                                businessprocess.BusinessProcessName,

                           })
                             .Distinct()
                 .ToList();

            return details;
        }

        [Route("api/KeyFocusArea/InsertKeyFocusAreaDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] EncryptedRequestModel encryptedPayload)
        {
            try
            {

                var decryptedJson = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData);
                var RIskKeyfocusAreaModels = JsonConvert.DeserializeObject<RIskKeyfocusAreaModel>(decryptedJson);



                var businessprocessessids = RIskKeyfocusAreaModels.businessprocessID.ToString();


                var businessprocessidArray = businessprocessessids.Split(',');



                foreach (var businessprocessessid in businessprocessidArray)
                {

                    var existingRecord = this.mySqlDBContext.RIskKeyfocusAreaModels.FirstOrDefault(
                        d => d.keyfousname == RIskKeyfocusAreaModels.keyfousname &&
                        d.entityid == RIskKeyfocusAreaModels.entityid &&
                             d.unitlocationid == RIskKeyfocusAreaModels.unitlocationid &&
                             d.departmentid == RIskKeyfocusAreaModels.departmentid &&
                             d.businessprocessID == RIskKeyfocusAreaModels.businessprocessID &&
                             d.bpmaturity == RIskKeyfocusAreaModels.bpmaturity &&
                             d.status == "Active");

                    if (existingRecord == null) // Check if record does not exist before adding
                    {
                        var RIskKeyfocusAreaModel = new RIskKeyfocusAreaModel
                        {
                            keyfousname = RIskKeyfocusAreaModels.keyfousname,
                            keyfousdescription = RIskKeyfocusAreaModels.keyfousdescription,
                            unitlocationid = RIskKeyfocusAreaModels.unitlocationid,
                            businessprocessID = businessprocessessid,
                            riskBusinessfunctionid = RIskKeyfocusAreaModels.riskBusinessfunctionid,
                            entityid = RIskKeyfocusAreaModels.entityid,
                            departmentid = RIskKeyfocusAreaModels.departmentid,
                            bpmaturity = RIskKeyfocusAreaModels.bpmaturity,
                            createdby = RIskKeyfocusAreaModels.createdby,
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "Active"
                        };

                        // Add the DepartmentLocationMappingModel object to the context
                        this.mySqlDBContext.RIskKeyfocusAreaModels.Add(RIskKeyfocusAreaModel);
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



        [Route("api/KeyFocusArea/GetKeyFocusAreaDetails")]
        [HttpGet]

        public IEnumerable<object> GetKeyFocusAreaDetails()
        {

            var details = (from keyfocus in mySqlDBContext.RIskKeyfocusAreaModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on keyfocus.unitlocationid equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on keyfocus.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on keyfocus.entityid equals entitymaster.Entity_Master_id
                           join businessprocess in mySqlDBContext.Risk_BusinessProcesss on keyfocus.businessprocessID equals businessprocess.businessprocessID.ToString()
                           join businessfunction in mySqlDBContext.RiskBusinessFunctionModels on keyfocus.riskBusinessfunctionid equals businessfunction.riskBusinessfunctionid
                            join bpmaturity in mySqlDBContext.risk_admin_iniassimpfact on keyfocus.bpmaturity equals bpmaturity.risk_admin_Iniassimpfactid
                           where keyfocus.status == "Active"
                           select new
                           {
                               keyfocus.keyfocusareaid,
                               keyfocus.keyfousname,
                               keyfocus.keyfousdescription,
                               keyfocus.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               keyfocus.entityid,
                               entitymaster.Entity_Master_Name,
                               keyfocus.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               keyfocus.departmentid,
                               departmentmaster.Department_Master_name,
                               keyfocus.businessprocessID,
                               businessprocess.BusinessProcessName,
                               keyfocus.bpmaturity,
                               bpmaturity.risk_admin_Iniassimpfactname
                      
                             

                           }).ToList();

            return details.Cast<object>();
        }




        [Route("api/KeyFocusArea/GetKeyFocusAreaDetailsbyid/{keyfocusareaid}")]
        [HttpGet]

        public IEnumerable<object> GetKeyFocusAreaDetailsbyid(int keyfocusareaid)
        {

            var details = (from keyfocus in mySqlDBContext.RIskKeyfocusAreaModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on keyfocus.unitlocationid equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on keyfocus.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on keyfocus.entityid equals entitymaster.Entity_Master_id
                           join businessprocess in mySqlDBContext.Risk_BusinessProcesss on keyfocus.businessprocessID equals businessprocess.businessprocessID.ToString()
                           join businessfunction in mySqlDBContext.RiskBusinessFunctionModels on keyfocus.riskBusinessfunctionid equals businessfunction.riskBusinessfunctionid
                           join bpmaturity in mySqlDBContext.risk_admin_iniassimpfact on keyfocus.bpmaturity equals bpmaturity.risk_admin_Iniassimpfactid
                           where keyfocus.status == "Active" && keyfocus.keyfocusareaid == keyfocusareaid
                           select new
                           {
                               keyfocus.keyfocusareaid,
                               keyfocus.keyfousname,
                               keyfocus.keyfousdescription,
                               keyfocus.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               keyfocus.entityid,
                               entitymaster.Entity_Master_Name,
                               keyfocus.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               keyfocus.departmentid,
                               departmentmaster.Department_Master_name,
                               keyfocus.businessprocessID,
                               businessprocess.BusinessProcessName,
                               keyfocus.bpmaturity,
                               bpmaturity.risk_admin_Iniassimpfactname



                           }).Distinct()
                .ToList();

            return details; ;
        }

        [Route("api/KeyFocusArea/DeleteKeyFocusArea")]
        [HttpDelete]

        public void DeleteKeyFocusArea(int id)
        {
            var currectclass = new RIskKeyfocusAreaModel { keyfocusareaid = id };
            currectclass.status = "Inactive";
            this.mySqlDBContext.Entry(currectclass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();


        }


        [Route("api/KeyFocusArea/UpdateKeyFocusAreaDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] EncryptedRequestModel encryptedPayload)
        {
            try
            {
                string decryptedData = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData); // Implement the DecryptAES method
                Updatekeyfocusarea updateData = JsonConvert.DeserializeObject<Updatekeyfocusarea>(decryptedData);


                RIskKeyfocusAreaModel RIskKeyfocusAreaModels = updateData.RIskKeyfocusAreaModels;

                bool combinationExists = this.mySqlDBContext.RIskKeyfocusAreaModels
         .Any(d => d.keyfousname == RIskKeyfocusAreaModels.keyfousname &&
                        d.entityid == RIskKeyfocusAreaModels.entityid &&
                             d.unitlocationid == RIskKeyfocusAreaModels.unitlocationid &&
                             d.departmentid == RIskKeyfocusAreaModels.departmentid &&
                             d.businessprocessID == RIskKeyfocusAreaModels.businessprocessID &&
                             d.riskBusinessfunctionid == RIskKeyfocusAreaModels.riskBusinessfunctionid &&
                             d.bpmaturity == RIskKeyfocusAreaModels.bpmaturity &&
                             d.status == "Active");

                if (combinationExists)
                {
                    return BadRequest("Error: Record already exists with the same combination of entity, unit location, and department.");
                }
                else
                {

                    if (RIskKeyfocusAreaModels.keyfocusareaid == 0)
                    {
                        return Ok("Insertion Unsuccessful");
                    }
                    else
                    {
                        RIskKeyfocusAreaModels.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        RIskKeyfocusAreaModels.status = "Active";
                        // Existing department, update logic
                        this.mySqlDBContext.Attach(RIskKeyfocusAreaModels);
                        this.mySqlDBContext.Entry(RIskKeyfocusAreaModels).State = EntityState.Modified;

                        var entry = this.mySqlDBContext.Entry(RIskKeyfocusAreaModels);

                        Type type = typeof(RIskKeyfocusAreaModel);
                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetValue(RIskKeyfocusAreaModels, null) == null || property.GetValue(RIskKeyfocusAreaModels, null).Equals(0))
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
