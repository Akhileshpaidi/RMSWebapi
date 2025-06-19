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
    public class riskoverallappititeandtrolleranceController : Controller
    {
        private MySqlDBContext mySqlDBContext;
        ClsGlobal obj_ClsGlobal = new ClsGlobal();

        public riskoverallappititeandtrolleranceController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }



        [Route("api/Riskoverallappitite/GetRiskoverallappititeDetails")]
        [HttpGet]

        public IEnumerable<object> GetRiskoverallappititeDetails()
        {

            var details = (from Riskoverallappitite in mySqlDBContext.riskoverallappititeModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on Riskoverallappitite.unitlocationid equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on Riskoverallappitite.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on Riskoverallappitite.entityid equals entitymaster.Entity_Master_id
                           join businessprocess in mySqlDBContext.Risk_BusinessProcesss on Riskoverallappitite.businessprocessID equals businessprocess.businessprocessID.ToString()
                           join businessfunction in mySqlDBContext.RiskBusinessFunctionModels on Riskoverallappitite.riskBusinessfunctionid equals businessfunction.riskBusinessfunctionid
                           join acceptancde in mySqlDBContext.risk_admin_asscontracptcrit on Riskoverallappitite.acceptanceid equals acceptancde.risk_admin_asscontracptCritid
                           where Riskoverallappitite.status == "Active"
                           select new
                           {
                               Riskoverallappitite.overallriskappititeid,
                               Riskoverallappitite.RiskAppetitename,
                               Riskoverallappitite.trolerancecoparison,
                               Riskoverallappitite.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               Riskoverallappitite.entityid,
                               entitymaster.Entity_Master_Name,
                               Riskoverallappitite.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               Riskoverallappitite.departmentid,
                               departmentmaster.Department_Master_name,
                               Riskoverallappitite.businessprocessID,
                               businessprocess.BusinessProcessName,
                               Riskoverallappitite.trigervalue,
                               Riskoverallappitite.acceptanceid,
                               acceptancde.risk_admin_asscontracptCritname,
                               acceptancde.risk_admin_asscontracptCritid



                           }).ToList();

            return details;
        }

        [Route("api/Riskoverallappitite/GetRiskoverallappititeDetailsid/{overallriskappititeid}")]
        [HttpGet]

        public IEnumerable<object> GetRiskoverallappititeDetailsid(int overallriskappititeid)
        {

            var details = (from Riskoverallappitite in mySqlDBContext.riskoverallappititeModels
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on Riskoverallappitite.unitlocationid equals unitlocation.Unit_location_Master_id
                           join departmentmaster in mySqlDBContext.DepartmentModels on Riskoverallappitite.departmentid equals departmentmaster.Department_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on Riskoverallappitite.entityid equals entitymaster.Entity_Master_id
                           join businessprocess in mySqlDBContext.Risk_BusinessProcesss on Riskoverallappitite.businessprocessID equals businessprocess.businessprocessID.ToString()
                           join businessfunction in mySqlDBContext.RiskBusinessFunctionModels on Riskoverallappitite.riskBusinessfunctionid equals businessfunction.riskBusinessfunctionid
                          join acceptancde in mySqlDBContext .risk_admin_asscontracptcrit on Riskoverallappitite.acceptanceid equals acceptancde.risk_admin_asscontracptCritid
                           where Riskoverallappitite.status == "Active" && Riskoverallappitite.overallriskappititeid == overallriskappititeid
                           select new
                           {
                               Riskoverallappitite.overallriskappititeid,
                               Riskoverallappitite.RiskAppetitename,
                               Riskoverallappitite.trolerancecoparison,
                               Riskoverallappitite.riskBusinessfunctionid,
                               businessfunction.riskbusinessname,
                               Riskoverallappitite.entityid,
                               entitymaster.Entity_Master_Name,
                               Riskoverallappitite.unitlocationid,
                               unitlocation.Unit_location_Master_name,
                               Riskoverallappitite.departmentid,
                               departmentmaster.Department_Master_name,
                               Riskoverallappitite.businessprocessID,
                               businessprocess.BusinessProcessName,
                               Riskoverallappitite.trigervalue,
                               Riskoverallappitite.acceptanceid,
                               acceptancde.risk_admin_asscontracptCritname,
                               acceptancde.risk_admin_asscontracptCritid




                           }).Distinct()
                .ToList();

            return details; 
        }

        [Route("api/Riskoverallappitite/InsertRiskoverallappititeDetails")]
 
        [HttpPost]
        public IActionResult InsertParameter([FromBody] EncryptedRequestModel encryptedPayload)
        {
      
            try
            {
                var decryptedJson = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData);
                var riskoverallappititeModels = JsonConvert.DeserializeObject<riskoverallappititeModel>(decryptedJson);

                var businessprocessIDs = riskoverallappititeModels.businessprocessID.ToString();


                var businessprocessIdsstringArray = businessprocessIDs.Split(',');



                foreach (var processesid in businessprocessIdsstringArray)
                {

                    var existingRecord = this.mySqlDBContext.riskoverallappititeModels.FirstOrDefault(
                        d => d.RiskAppetitename == riskoverallappititeModels.RiskAppetitename &&
                           d.trolerancecoparison == riskoverallappititeModels.trolerancecoparison &&
                        d.entityid == riskoverallappititeModels.entityid &&
                             d.unitlocationid == riskoverallappititeModels.unitlocationid &&
                             d.departmentid == riskoverallappititeModels.departmentid &&
                       d.riskBusinessfunctionid == riskoverallappititeModels.riskBusinessfunctionid &&
                       d.businessprocessID == riskoverallappititeModels.businessprocessID &&
                       d.trigervalue == riskoverallappititeModels.trigervalue &&
                       d.acceptanceid == riskoverallappititeModels.acceptanceid &&
                             d.status == "Active");

                    if (existingRecord == null) // Check if record does not exist before adding
                    {
                        var riskoverallappititeModel = new riskoverallappititeModel
                        {
                            RiskAppetitename = riskoverallappititeModels.RiskAppetitename,
                            trolerancecoparison = riskoverallappititeModels.trolerancecoparison,
                            businessprocessID = processesid,
                            entityid = riskoverallappititeModels.entityid,
                            departmentid = riskoverallappititeModels.departmentid,
                            unitlocationid = riskoverallappititeModels.unitlocationid,
                            riskBusinessfunctionid = riskoverallappititeModels.riskBusinessfunctionid,
                            trigervalue = riskoverallappititeModels.trigervalue,
                            acceptanceid = riskoverallappititeModels.acceptanceid,
                            createdBy = riskoverallappititeModels.createdBy,
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "Active"
                        };

                        // Add the DepartmentLocationMappingModel object to the context
                        this.mySqlDBContext.riskoverallappititeModels.Add(riskoverallappititeModel);
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
              
        
        [Route("api/Riskoverallappitite/DeleteriskRiskoverallappititeetails")]
                [HttpDelete]

                public void DeleteriskRiskoverallappititeetails(int id)
                {
                    try
                    {
                        var currentClass = new riskoverallappititeModel { overallriskappititeid = id };
                        currentClass.status = "Inactive";
                        this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
                        this.mySqlDBContext.SaveChanges();


                    }
                    catch
                    {
                        return;
                    }


                }


        [Route("api/Riskoverallappitite/UpdateRiskoverallappititeDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] EncryptedRequestModel encryptedPayload)
        {
            try
            {
                string decryptedData = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData); // Implement the DecryptAES method
                UpdateoverallriskAppetite updateData = JsonConvert.DeserializeObject<UpdateoverallriskAppetite>(decryptedData);


                riskoverallappititeModel riskoverallappititeModels = updateData.riskoverallappititeModels;

                bool combinationExists = this.mySqlDBContext.riskoverallappititeModels
         .Any(d => d.RiskAppetitename == riskoverallappititeModels.RiskAppetitename &&
         d.trolerancecoparison == riskoverallappititeModels.trolerancecoparison &&
                        d.entityid == riskoverallappititeModels.entityid &&
                             d.unitlocationid == riskoverallappititeModels.unitlocationid &&
                             d.departmentid == riskoverallappititeModels.departmentid &&
                       d.riskBusinessfunctionid == riskoverallappititeModels.riskBusinessfunctionid &&
                       d.businessprocessID == riskoverallappititeModels.businessprocessID &&
                       d.trigervalue == riskoverallappititeModels.trigervalue &&
                       d.acceptanceid == riskoverallappititeModels.acceptanceid &&
                             d.status == "Active");

                if (combinationExists)
                {
                    return BadRequest("Error: Record already exists with the same combination of entity, unit location, department,business Process,RiskAppetite Name ,jective of Overall Risk Tolerance comparison .");
                }
                else
                {

                    if (riskoverallappititeModels.overallriskappititeid == 0)
                    {
                        return Ok("Insertion Unsuccessful");
                    }
                    else
                    {
                        riskoverallappititeModels.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        riskoverallappititeModels.status = "Active";
                        // Existing department, update logic
                        this.mySqlDBContext.Attach(riskoverallappititeModels);
                        this.mySqlDBContext.Entry(riskoverallappititeModels).State = EntityState.Modified;

                        var entry = this.mySqlDBContext.Entry(riskoverallappititeModels);

                        Type type = typeof(riskoverallappititeModel);
                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetValue(riskoverallappititeModels, null) == null || property.GetValue(riskoverallappititeModels, null).Equals(0))
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

