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
    public class AppspecifyconfigurationController : Controller
    {
        private MySqlDBContext mySqlDBContext;

        public AppspecifyconfigurationController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/Appspecifyconfiguration/getAppspecifyconfiguration")]
        [HttpGet]
        public IActionResult GetAppspecifyConfiguration()
        {
            try
            {
                var configuration = mySqlDBContext.Appspecifyconfigurtions
                    .Where(x => x.status == "Active");

                if (configuration == null)
                {
                    return NotFound("Configuration not found.");
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [Route("api/Appspecifyconfiguration/insertAppspecifyconfiguration")]
        [HttpPut]
        public IActionResult insertAppspecifyconfiguration([FromBody] List<Appspecifyconfigurtion> pistdatalist)
        {
            try
            {
                foreach (var model in pistdatalist)
                {
                    var existingModel = mySqlDBContext.Appspecifyconfigurtions
                        .FirstOrDefault(x => x.configuration == model.configuration);

                    if (existingModel != null)
                    {
                        // Update existing record
                        existingModel.grantpermission = model.grantpermission;
                        existingModel.parameters = model.parameters;
                        existingModel.timeperiod = model.timeperiod;

                        existingModel.createdby = model.createdby;
                        DateTime dt = DateTime.Now;
                        string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        existingModel.createddate = dt1;
                        existingModel.status = "Active";
                        mySqlDBContext.Appspecifyconfigurtions.Update(existingModel);
                    }
                    else
                    {

                        var newModel = new Appspecifyconfigurtion
                        {
                            configuration = model.configuration,
                            grantpermission = model.grantpermission,
                            parameters = model.parameters,
                            timeperiod = model.timeperiod,

                            createdby = model.createdby,
                            status = "Active",
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        };

                        mySqlDBContext.Appspecifyconfigurtions.Add(newModel);
                    }
                }


                mySqlDBContext.SaveChanges();

                return Ok("Data inserted or updated successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (logging code would go here)

                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }


        [Route("api/Appspecifyconfiguration/insertAppspecifycommonconfiguration")]
        [HttpPut]
        public IActionResult insertAppspecifycommonconfiguration([FromBody] commonsettingpermission payload)

        {
            try
            {
              
                var existingModel = mySqlDBContext.commonsettingpermissions
                    .FirstOrDefault(x => x.configuration == payload.configuration);

                if (existingModel != null)
                {
                    // Update existing record
                    existingModel.permission = payload.permission;
                    existingModel.filesize = payload.filesize;

                    existingModel.createdby = payload.createdby;
                    DateTime dt = DateTime.Now;
                    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    existingModel.createddate = dt1;
                    existingModel.status = "Active";
                    mySqlDBContext.commonsettingpermissions.Update(existingModel);
                }
                else
                {

                        var newmodel = new commonsettingpermission
                        {
                            configuration = payload.configuration,
                            permission = payload.permission,
                            filesize = payload.filesize,
                            createdby = payload.createdby,
                            status = "Active",
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        };

                        mySqlDBContext.commonsettingpermissions.Add(newmodel);
                }
            


            mySqlDBContext.SaveChanges();

            return Ok("Data inserted or updated successfully.");
        }
            catch (Exception ex)
            {
                // Log the exception (logging code would go here)

                return StatusCode(500, "Internal server error: " + ex.Message);
             }

        }




        [Route("api/Appspecifyconfiguration/getAppspecifycommonconfiguration")]
      [HttpGet]
        public IActionResult GetAppspecifyCommonConfiguration()
        {
            try
            {
                var configuration = mySqlDBContext.commonsettingpermissions
                    .FirstOrDefault( x=> x.status == "Active");

                if (configuration == null)
                {
                    return NotFound("Configuration not found.");
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

    }

}
