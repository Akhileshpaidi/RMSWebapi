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
    public class RiskAssessmementTemplateController : Controller
    {

        private MySqlDBContext mySqlDBContext;

        public RiskAssessmementTemplateController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/RiskAssessmentTemplate/GetRiskAssessmentTemplate")]
        [HttpGet]

        public IEnumerable<object> GetRiskAssessmentTemplate()
        {
            var details = (from Assessment in mySqlDBContext.RiskAssessmentTemplateModels
                           select new
                           {
                               Assessment.assessmenttemplateid,
                               Assessment.configuration,

                               // Set remarks to null if the value is "0"
                               remarks = Assessment.remarks == "0" ? null : Assessment.remarks,

                               // Directly use the boolean value of togglecontrols
                               togglecontrols = Assessment.togglecontrols,

                               // Set controls to null if the value is "0"
                               controls = Assessment.controls == "0" ? null : Assessment.controls
                           }).ToList();
            return details;
        }



        [Route("api/RiskAssessmementTemplate/insertRiskAssessmementTemplate")]
        [HttpPut]
        public IActionResult insertRiskAssessmementTemplate([FromBody] List<RiskAssessmentTemplateModel> pistdatalist)
        {
            try
            {
                foreach (var model in pistdatalist)
                {
                    var existingModel = mySqlDBContext.RiskAssessmentTemplateModels
                        .FirstOrDefault(x => x.configuration == model.configuration);

                    if (existingModel != null)
                    {
                        // Update existing record
                        existingModel.controls = model.controls;
                        existingModel.remarks = model.remarks;
                       existingModel.togglecontrols = model.togglecontrols;
                        existingModel.createdby = model.createdby;
                        DateTime dt = DateTime.Now;
                        string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        existingModel.createddate = dt1;
                        existingModel.status = "Active";
                        mySqlDBContext.RiskAssessmentTemplateModels.Update(existingModel);
                    }
                    else
                    {

                        var newModel = new RiskAssessmentTemplateModel
                        {
                            configuration = model.configuration,
                            controls = model.controls,
                            togglecontrols = model.togglecontrols,
                            remarks = model.remarks,

                            createdby = model.createdby,
                            status = "Active",
                            createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        };

                        mySqlDBContext.RiskAssessmentTemplateModels.Add(newModel);
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

    }
}
