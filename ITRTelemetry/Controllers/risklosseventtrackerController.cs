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
using DocumentFormat.OpenXml.Spreadsheet;
using static ITRTelemetry.Controllers.Componentcontroller;
using OpenXmlPowerTools;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class risklosseventtrackerController : Controller
    {
        private MySqlDBContext mySqlDBContext;
        ClsGlobal obj_ClsGlobal = new ClsGlobal();
           private ClsEmail obj_Clsmail = new ClsEmail();


        public risklosseventtrackerController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/losseventtracker/GetlosseventtrackerDetails")]
        [HttpGet]
        public IEnumerable<object> GetlosseventtrackerDetails()
        {
            // Fetch basic event details
            var losseventDetails = from lossevent in mySqlDBContext.risklosseventtrackermodels
                                   join unitlocation in mySqlDBContext.UnitLocationMasterModels
                                       on lossevent.unitlocationid equals unitlocation.Unit_location_Master_id
                                   join departmentmaster in mySqlDBContext.DepartmentModels
                                       on lossevent.departmentid equals departmentmaster.Department_Master_id
                                   join entitymaster in mySqlDBContext.UnitMasterModels
                                       on lossevent.entityid equals entitymaster.Entity_Master_id
                                   join businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                                       on lossevent.riskBusinessfunctionid equals businessfunction.riskBusinessfunctionid
                                   where lossevent.status == "Active"
                                   select new
                                   {
                                       lossevent.losseventtrackerid,
                                       lossevent.losseventname,
                                       lossevent.losseventdescription,
                                       lossevent.riskBusinessfunctionid,
                                       businessfunction.riskbusinessname,
                                       lossevent.entityid,
                                       entitymaster.Entity_Master_Name,
                                       lossevent.unitlocationid,
                                       unitlocation.Unit_location_Master_name,
                                       lossevent.departmentid,
                                       departmentmaster.Department_Master_name,
                                       lossevent.startValue,
                                       lossevent.endValues,
                                       reportingusers = lossevent.reportingusers,
                                       additionalusers = lossevent.additionalusers
                                   };

            // Fetch data and handle user information separately
            var detailsList = losseventDetails.ToList();

            var userIds = detailsList
         .SelectMany(d => d.reportingusers.Split(',').Concat(d.additionalusers.Split(',')))
         .Distinct()
         .ToList();

            // Ensure that `USR_ID` is properly converted to string if necessary
            var users = mySqlDBContext.usermodels
                .Where(u => userIds.Contains(u.USR_ID.ToString()))
                .ToList()
                .GroupBy(u => u.USR_ID.ToString())
                .ToDictionary(g => g.Key, g => g.First());

            // Final projection
            var result = detailsList.Select(d => new
            {
                d.losseventtrackerid,
                reportinguser = string.Join(",",
              d.reportingusers.Split(',').Select(id => users.ContainsKey(id) ? users[id].firstname : "Unknown")),
                additionaluser = string.Join(",",
              d.additionalusers.Split(',').Select(id => users.ContainsKey(id) ? users[id].firstname : "Unknown")),
                d.Entity_Master_Name,
                d.Unit_location_Master_name,
                d.losseventname,
                d.losseventdescription,
                d.endValues,
                d.startValue,
                d.riskbusinessname,
                d.Department_Master_name
            });

            return result.ToList();
        }

        [Route("api/losseventtracker/GetlosseventtrackerDetailsnyid/{losseventtrackerid}")]
        [HttpGet]
        public IEnumerable<object> GetlosseventtrackerDetailsnyid(int losseventtrackerid)
        {
            // Fetch basic event details
            var losseventDetails =( from lossevent in mySqlDBContext.risklosseventtrackermodels
                                   join unitlocation in mySqlDBContext.UnitLocationMasterModels
                                       on lossevent.unitlocationid equals unitlocation.Unit_location_Master_id
                                   join departmentmaster in mySqlDBContext.DepartmentModels
                                       on lossevent.departmentid equals departmentmaster.Department_Master_id
                                   join entitymaster in mySqlDBContext.UnitMasterModels
                                       on lossevent.entityid equals entitymaster.Entity_Master_id
                                   join businessfunction in mySqlDBContext.RiskBusinessFunctionModels
                                       on lossevent.riskBusinessfunctionid equals businessfunction.riskBusinessfunctionid
                                   where lossevent.status == "Active" && lossevent.losseventtrackerid == losseventtrackerid
                                   select new
                                   {
                                       lossevent.losseventtrackerid,
                                       lossevent.losseventname,
                                      desc= lossevent.losseventdescription,
                                       lossevent.riskBusinessfunctionid,
                                       businessfunction.riskbusinessname,
                                       lossevent.entityid,
                                       entitymaster.Entity_Master_Name,
                                       lossevent.unitlocationid,
                                       unitlocation.Unit_location_Master_name,
                                       lossevent.departmentid,
                                       departmentmaster.Department_Master_name,
                                       lossevent.startValue,
                                       lossevent.endValues,
                                       lossevent.reportingusers,
                                        lossevent.additionalusers,
                                     
                                  

                                 }).Distinct()
                               .ToList();

            return losseventDetails; 
        }

        [Route("api/losseventtracker/InsertlosseventtrackerDetails")]

        [HttpPost]
        public async Task<IActionResult> InsertParameter([FromBody] EncryptedRequestModel encryptedPayload)
        {

            try
            {
                var decryptedJson = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData);
                var risklosseventtrackermodels = JsonConvert.DeserializeObject<risklosseventtrackermodel>(decryptedJson);




                if(risklosseventtrackermodels.startValue < risklosseventtrackermodels.endValues)
                {
               

              
                var maxEndValue = this.mySqlDBContext.risklosseventtrackermodels
                    .Where(d => d.status == "Active")
                    .Max(d => d.endValues);

                if (risklosseventtrackermodels.startValue >  maxEndValue)
                {
                    



                var existingRecord = this.mySqlDBContext.risklosseventtrackermodels.FirstOrDefault(
                        d => d.losseventname == risklosseventtrackermodels.losseventname &&
                           d.losseventdescription == risklosseventtrackermodels.losseventdescription &&
                        d.entityid == risklosseventtrackermodels.entityid &&
                             d.unitlocationid == risklosseventtrackermodels.unitlocationid &&
                             d.departmentid == risklosseventtrackermodels.departmentid &&
                       d.riskBusinessfunctionid == risklosseventtrackermodels.riskBusinessfunctionid &&
                      
                        d.entityid == risklosseventtrackermodels.entityid &&
                       d.startValue == risklosseventtrackermodels.startValue &&
                        d.endValues == risklosseventtrackermodels.endValues &&
                             d.status == "Active");

                    if (existingRecord == null) // Check if record does not exist before adding
                    {
                        var risklosseventtrackermodel = new risklosseventtrackermodel
                        {
                            losseventname = risklosseventtrackermodels.losseventname,
                            losseventdescription = risklosseventtrackermodels.losseventdescription,
                            entityid = risklosseventtrackermodels.entityid,
                            departmentid = risklosseventtrackermodels.departmentid,
                            unitlocationid = risklosseventtrackermodels.unitlocationid,
                            riskBusinessfunctionid = risklosseventtrackermodels.riskBusinessfunctionid,
                            startValue = risklosseventtrackermodels.startValue,
                            endValues = risklosseventtrackermodels.endValues,
                            reportingusers = risklosseventtrackermodels.reportingusers,
                            additionalusers = risklosseventtrackermodels.additionalusers,
                            createdBy = risklosseventtrackermodels.createdBy,
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "Active"
                        };

                
                        this.mySqlDBContext.risklosseventtrackermodels.Add(risklosseventtrackermodel);
                    }
          
                this.mySqlDBContext.SaveChanges();


                        return Ok();
            }

            return BadRequest("Error: The start value should be greater than the maximum of all previous end values.");
          }
        return BadRequest("Error:End value should be greater than the  Start value.");
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


        [Route("api/losseventtracker/Deletelosseventtracker")]
        [HttpDelete]

        public void Deletelosseventtracker(int id)
        {
            try
            {
                var currentClass = new risklosseventtrackermodel { losseventtrackerid = id };
                currentClass.status = "Inactive";
                this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
                this.mySqlDBContext.SaveChanges();


            }
            catch
            {
                return;
            }


        }

        [Route("api/losseventtracker/UpdatelosseventtrackerDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] EncryptedRequestModel encryptedPayload)
        {
            try
            {

                string decryptedData = obj_ClsGlobal.DecryptAES(encryptedPayload.RequestData); // Implement the DecryptAES method
                Updatelossevent updateData = JsonConvert.DeserializeObject<Updatelossevent>(decryptedData);



                risklosseventtrackermodel risklosseventtrackermodels = updateData.risklosseventtrackermodels;

                bool combinationExists = this.mySqlDBContext.risklosseventtrackermodels
         .Any( d => d.losseventname == risklosseventtrackermodels.losseventname &&
                           d.losseventdescription == risklosseventtrackermodels.losseventdescription &&
                        d.entityid == risklosseventtrackermodels.entityid &&
                             d.unitlocationid == risklosseventtrackermodels.unitlocationid &&
                             d.departmentid == risklosseventtrackermodels.departmentid &&
                       d.riskBusinessfunctionid == risklosseventtrackermodels.riskBusinessfunctionid &&

                        d.entityid == risklosseventtrackermodels.entityid &&
                       d.startValue == risklosseventtrackermodels.startValue &&
                        d.endValues == risklosseventtrackermodels.endValues &&
                           d.status == "Active");

                if (combinationExists)
                {
                    return BadRequest("Error: Record already exists with the same combination of entity, unit location, department,business Process,RiskAppetite Name ,jective of Overall Risk Tolerance comparison .");
                }
                else
                {

                    if (risklosseventtrackermodels.losseventtrackerid == 0)
                    {
                        return Ok("Insertion Unsuccessful");
                    }
                    else
                    {
                        risklosseventtrackermodels.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        risklosseventtrackermodels.status = "Active";
                        // Existing department, update logic
                        this.mySqlDBContext.Attach(risklosseventtrackermodels);
                        this.mySqlDBContext.Entry(risklosseventtrackermodels).State = EntityState.Modified;

                        var entry = this.mySqlDBContext.Entry(risklosseventtrackermodels);

                        Type type = typeof(risklosseventtrackermodel);
                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetValue(risklosseventtrackermodels, null) == null || property.GetValue(risklosseventtrackermodels, null).Equals(0))
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
