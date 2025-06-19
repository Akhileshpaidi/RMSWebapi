using DocumentFormat.OpenXml.ExtendedProperties;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using System.Reflection;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]

    public class RisksamplingstandardsController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public RisksamplingstandardsController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        [Route("api/risksamplingstandards/Getrisksamplingstandards")]
        [HttpGet]
        public IEnumerable<object> Getrisksamplingstandards()
        {
            var details = (from sample in mySqlDBContext.risksamplingstandardsmodel
                           join frequencyControl in mySqlDBContext.risk_Admin_Frqcontrapplids
                               on sample.frequencyControl equals frequencyControl.risk_admin_frqcontrapplidid into frequencyControlJoin
                           from frequencyControl in frequencyControlJoin.DefaultIfEmpty()
                           join keycontrols in mySqlDBContext.keyControls
                               on sample.risksamplingstandardsId equals keycontrols.risksamplingstandardsId into keycontrolsJoin
                           from keycontrols in keycontrolsJoin.DefaultIfEmpty()
                           join nonkeycontrols in mySqlDBContext.nonKeyControls
                               on sample.risksamplingstandardsId equals nonkeycontrols.risksamplingstandardsId into nonkeycontrolsJoin
                           from nonkeycontrols in nonkeycontrolsJoin.DefaultIfEmpty()
                           join Generalkeycontrols in mySqlDBContext.generalControls
                               on sample.risksamplingstandardsId equals Generalkeycontrols.risksamplingstandardsId into GeneralkeycontrolsJoin
                           from Generalkeycontrols in GeneralkeycontrolsJoin.DefaultIfEmpty()
                           where sample.status == "Active"
                           select new
                           {
                               sample.risksamplingstandardsId,
                               sample.samplingAuthority,
                               sample.frequencyControl,
                               frequencyControl.risk_admin_frqcontrapplidid,
                               frequencyControl.risk_admin_frqcontrapplidname,
                               keycontrols.controltype,
                               keycontrols.keyControlsrangeStartValue,
                               keycontrols.keyControlsendValue,
                               keycontrols.keyControlsstartValue,
                               keycontrols.keyControlstransactionDescription,
                               keycontrols.keyControlstextDescription,
                               keycontrols.keyControlspercentageDescription,
                               nonkeycontrols.noncontroltype,
                               nonkeycontrols.nonKeyControlsrangeStartValue,
                               nonkeycontrols.nonKeyControlsendValue,
                               nonkeycontrols.nonKeyControlstextDescription,
                               nonkeycontrols.nonKeyControlspercentageDescription,
                               nonkeycontrols.nonKeyControlsstartValue,
                               nonkeycontrols.nonKeyControlstransactionDescription,
                               Generalkeycontrols.gencontroltype,
                               Generalkeycontrols.generalControlsstartValue,
                               Generalkeycontrols.generalControlsendValue,
                               Generalkeycontrols.generalControlsrangeStartValue,
                               Generalkeycontrols.generalControlspercentageDescription,
                               Generalkeycontrols.generalControlstextDescription,
                               Generalkeycontrols.generalControlstransactionDescription,



                           }).ToList();
            return details;


        }





        [Route("api/risksamplingstandards/Getrisksamplingstandardsbyid/{risksamplingstandardsId}")]
        [HttpGet]
        public IEnumerable<object> Getrisksamplingstandards(int risksamplingstandardsId)
        {
            var details = (from sample in mySqlDBContext.risksamplingstandardsmodel
                           join frequencyControl in mySqlDBContext.risk_Admin_Frqcontrapplids
                               on sample.frequencyControl equals frequencyControl.risk_admin_frqcontrapplidid into frequencyControlJoin
                           from frequencyControl in frequencyControlJoin.DefaultIfEmpty()
                           join keycontrols in mySqlDBContext.keyControls
                               on sample.risksamplingstandardsId equals keycontrols.risksamplingstandardsId into keycontrolsJoin
                           from keycontrols in keycontrolsJoin.DefaultIfEmpty()
                           join nonkeycontrols in mySqlDBContext.nonKeyControls
                               on sample.risksamplingstandardsId equals nonkeycontrols.risksamplingstandardsId into nonkeycontrolsJoin
                           from nonkeycontrols in nonkeycontrolsJoin.DefaultIfEmpty()
                           join Generalkeycontrols in mySqlDBContext.generalControls
                               on sample.risksamplingstandardsId equals Generalkeycontrols.risksamplingstandardsId into GeneralkeycontrolsJoin
                           from Generalkeycontrols in GeneralkeycontrolsJoin.DefaultIfEmpty()
                           where sample.status == "Active" && sample.risksamplingstandardsId == risksamplingstandardsId
                           select new
                           {
                               sample.risksamplingstandardsId,
                               sample.samplingAuthority,
                               sample.frequencyControl,
                               frequencyControl.risk_admin_frqcontrapplidid,
                               frequencyControl.risk_admin_frqcontrapplidname,
                               keycontrols.controltype,
                               keycontrols.keyControlsrangeStartValue,
                               keycontrols.keyControlsendValue,
                               keycontrols.keyControlsstartValue,
                               keycontrols.keyControlstransactionDescription,
                               keycontrols.keyControlstextDescription,
                               keycontrols.keyControlspercentageDescription,
                               keycontrols.keyControlscheckedstatus,
                               nonkeycontrols.noncontroltype,
                               nonkeycontrols.nonKeyControlsrangeStartValue,
                               nonkeycontrols.nonKeyControlsendValue,
                               nonkeycontrols.nonKeyControlstextDescription,
                               nonkeycontrols.nonKeyControlspercentageDescription,
                               nonkeycontrols.nonKeyControlsstartValue,
                               nonkeycontrols.nonKeyControlstransactionDescription,
                               nonkeycontrols.nonKeyControlscheckedstatus,
                               Generalkeycontrols.gencontroltype,
                               Generalkeycontrols.generalControlsstartValue,
                               Generalkeycontrols.generalControlsendValue,
                               Generalkeycontrols.generalControlsrangeStartValue,
                               Generalkeycontrols.generalControlspercentageDescription,
                               Generalkeycontrols.generalControlstextDescription,
                               Generalkeycontrols.generalControlstransactionDescription,
                               Generalkeycontrols.generalControlscheckedstatus 


                           }).ToList();
            return details;


        }






        [Route("api/risksamplingstandards/insertrisksamplingstandards")]
        [HttpPut]


        public IActionResult insertrisksamplingstandards([FromBody] risksamplingstandards dataToSend)
        {
            try
            {

                var existingrecord = this.mySqlDBContext.risksamplingstandardsmodel.FirstOrDefault(d => d.samplingAuthority == dataToSend.samplingAuthority && d.frequencyControl == dataToSend.frequencyControl && d.status == "Active");
                if (existingrecord != null)
                {
                    return BadRequest("Record Already Exisiting  With this Combinations");

                }

                var risksamplingstandards = new risksamplingstandards
                {
                    samplingAuthority = dataToSend.samplingAuthority,
                    frequencyControl = dataToSend.frequencyControl,
                    createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    status = "Active",
                };
                mySqlDBContext.risksamplingstandardsmodel.Add(risksamplingstandards);
                mySqlDBContext.SaveChanges();
                if (dataToSend.KeyControls != null)
                {

                    var keyControl = new keyControls
                    {
                        controltype = dataToSend.KeyControls.controltype,
                        keyControlscheckedstatus  = dataToSend.KeyControls.keyControlscheckedstatus,
                        keyControlsstartValue = dataToSend.KeyControls.keyControlsstartValue,
                        keyControlsendValue = dataToSend.KeyControls.keyControlsendValue,
                        keyControlstransactionDescription = dataToSend.KeyControls.keyControlstransactionDescription,
                        keyControlsrangeStartValue = dataToSend.KeyControls.keyControlsrangeStartValue,
                        keyControlspercentageDescription = dataToSend.KeyControls.keyControlspercentageDescription,
                        keyControlstextDescription = dataToSend.KeyControls.keyControlstextDescription,
                        risksamplingstandardsId = risksamplingstandards.risksamplingstandardsId // Link to the main record
                    };
                    mySqlDBContext.keyControls.Add(keyControl);
                }
                if (dataToSend.nonKeyControls != null)
                {
                    var nonKeyControl = new nonKeyControls
                    {
                        noncontroltype = dataToSend.nonKeyControls.noncontroltype,
                       nonKeyControlscheckedstatus = dataToSend.nonKeyControls.nonKeyControlscheckedstatus,
                        nonKeyControlsstartValue = dataToSend.nonKeyControls.nonKeyControlsstartValue,
                        nonKeyControlsendValue = dataToSend.nonKeyControls.nonKeyControlsendValue,
                        nonKeyControlstransactionDescription = dataToSend.nonKeyControls.nonKeyControlstransactionDescription,
                        nonKeyControlsrangeStartValue = dataToSend.nonKeyControls.nonKeyControlsrangeStartValue,
                        nonKeyControlspercentageDescription = dataToSend.nonKeyControls.nonKeyControlspercentageDescription,
                        nonKeyControlstextDescription = dataToSend.nonKeyControls.nonKeyControlstextDescription,
                        risksamplingstandardsId = risksamplingstandards.risksamplingstandardsId // Link to the main record
                    };
                    mySqlDBContext.nonKeyControls.Add(nonKeyControl);
                }
                if (dataToSend.generalControls != null)
                {
                    var generalControl = new generalControls
                    {
                        gencontroltype = dataToSend.generalControls.gencontroltype,
                        generalControlscheckedstatus = dataToSend.generalControls.generalControlscheckedstatus,
                        generalControlsstartValue = dataToSend.generalControls.generalControlsstartValue,
                        generalControlsendValue = dataToSend.generalControls.generalControlsendValue,
                        generalControlstransactionDescription = dataToSend.generalControls.generalControlstransactionDescription,
                        generalControlsrangeStartValue = dataToSend.generalControls.generalControlsrangeStartValue,
                        generalControlspercentageDescription = dataToSend.generalControls.generalControlspercentageDescription,
                        generalControlstextDescription = dataToSend.generalControls.generalControlstextDescription,
                        risksamplingstandardsId = risksamplingstandards.risksamplingstandardsId // Link to the main record
                    };
                    mySqlDBContext.generalControls.Add(generalControl);
                }






                mySqlDBContext.SaveChanges();

                return Ok("Data saved successfully.");
            }
            catch (Exception ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Record Already Exisiting  With this Combinations");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }








        [Route("api/risksamplingstandards/Deleterisksamplingstandards")]
        [HttpDelete]
        public void Deleterisksamplingstandards(int id)
        {
            var currentClass = new risksamplingstandards { risksamplingstandardsId = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }





        [Route("api/risksamplingstandards/Updaterisksamplingstandards")]
        [HttpPost]


        public IActionResult Updaterisksamplingstandards([FromBody] risksamplingstandards dataToSend)
        {
       
            if (dataToSend.risksamplingstandardsId == 0)
            {
                return Ok("Insertion Unsuccessful");
            }
            else
            {

                // Retrieve the existing risksamplingstandards record
                var existingRiskSamplingStandards = mySqlDBContext.risksamplingstandardsmodel
                    .FirstOrDefault(x => x.risksamplingstandardsId == dataToSend.risksamplingstandardsId);

                if (existingRiskSamplingStandards == null)
                {
                    return Ok("Update Unsuccessful: Record not found.");
                }

                // Update risksamplingstandards fields
                existingRiskSamplingStandards.samplingAuthority = dataToSend.samplingAuthority;
                existingRiskSamplingStandards.frequencyControl = dataToSend.frequencyControl;
             

      
                if (dataToSend.KeyControls != null)
                {
                    var existingKeyControl = mySqlDBContext.keyControls
                        .FirstOrDefault(k => k.risksamplingstandardsId == dataToSend.risksamplingstandardsId);

                    if (existingKeyControl != null)
                    {
                        existingKeyControl.controltype = dataToSend.KeyControls.controltype;
                        existingKeyControl.keyControlscheckedstatus = dataToSend.KeyControls.keyControlscheckedstatus;
                        existingKeyControl.keyControlsstartValue = dataToSend.KeyControls.keyControlsstartValue;
                        existingKeyControl.keyControlsendValue = dataToSend.KeyControls.keyControlsendValue;
                        existingKeyControl.keyControlstransactionDescription = dataToSend.KeyControls.keyControlstransactionDescription;
                        existingKeyControl.keyControlsrangeStartValue = dataToSend.KeyControls.keyControlsrangeStartValue;
                        existingKeyControl.keyControlspercentageDescription = dataToSend.KeyControls.keyControlspercentageDescription;
                        existingKeyControl.keyControlstextDescription = dataToSend.KeyControls.keyControlstextDescription;
                    }
                    else
                    {
                        var newKeyControl = new keyControls
                        {
                            controltype = dataToSend.KeyControls.controltype,
                           keyControlscheckedstatus = dataToSend.KeyControls.keyControlscheckedstatus,
                            keyControlsstartValue = dataToSend.KeyControls.keyControlsstartValue,
                            keyControlsendValue = dataToSend.KeyControls.keyControlsendValue,
                            keyControlstransactionDescription = dataToSend.KeyControls.keyControlstransactionDescription,
                            keyControlsrangeStartValue = dataToSend.KeyControls.keyControlsrangeStartValue,
                            keyControlspercentageDescription = dataToSend.KeyControls.keyControlspercentageDescription,
                            keyControlstextDescription = dataToSend.KeyControls.keyControlstextDescription,
                            risksamplingstandardsId = dataToSend.risksamplingstandardsId
                        };
                        mySqlDBContext.keyControls.Add(newKeyControl);
                    }
                }

                if (dataToSend.nonKeyControls != null)
                {
                    var existingNonKeyControl = mySqlDBContext.nonKeyControls
                        .FirstOrDefault(nk => nk.risksamplingstandardsId == dataToSend.risksamplingstandardsId);

                    if (existingNonKeyControl != null)
                    {
                        existingNonKeyControl.noncontroltype = dataToSend.nonKeyControls.noncontroltype;
                        existingNonKeyControl.nonKeyControlscheckedstatus = dataToSend.nonKeyControls.nonKeyControlscheckedstatus;
                        existingNonKeyControl.nonKeyControlsstartValue = dataToSend.nonKeyControls.nonKeyControlsstartValue;
                        existingNonKeyControl.nonKeyControlsendValue = dataToSend.nonKeyControls.nonKeyControlsendValue;
                        existingNonKeyControl.nonKeyControlstransactionDescription = dataToSend.nonKeyControls.nonKeyControlstransactionDescription;
                        existingNonKeyControl.nonKeyControlsrangeStartValue = dataToSend.nonKeyControls.nonKeyControlsrangeStartValue;
                        existingNonKeyControl.nonKeyControlspercentageDescription = dataToSend.nonKeyControls.nonKeyControlspercentageDescription;
                        existingNonKeyControl.nonKeyControlstextDescription = dataToSend.nonKeyControls.nonKeyControlstextDescription;
                    }
                    else
                    {
                        var newNonKeyControl = new nonKeyControls
                        {
                            noncontroltype = dataToSend.nonKeyControls.noncontroltype,
                            nonKeyControlscheckedstatus = dataToSend.nonKeyControls.nonKeyControlscheckedstatus,
                            nonKeyControlsstartValue = dataToSend.nonKeyControls.nonKeyControlsstartValue,
                            nonKeyControlsendValue = dataToSend.nonKeyControls.nonKeyControlsendValue,
                            nonKeyControlstransactionDescription = dataToSend.nonKeyControls.nonKeyControlstransactionDescription,
                            nonKeyControlsrangeStartValue = dataToSend.nonKeyControls.nonKeyControlsrangeStartValue,
                            nonKeyControlspercentageDescription = dataToSend.nonKeyControls.nonKeyControlspercentageDescription,
                            nonKeyControlstextDescription = dataToSend.nonKeyControls.nonKeyControlstextDescription,
                            risksamplingstandardsId = dataToSend.risksamplingstandardsId
                        };
                        mySqlDBContext.nonKeyControls.Add(newNonKeyControl);
                    }
                }

                if (dataToSend.generalControls != null)
                {
                    var existingGeneralControl = mySqlDBContext.generalControls
                        .FirstOrDefault(gc => gc.risksamplingstandardsId == dataToSend.risksamplingstandardsId);

                    if (existingGeneralControl != null)
                    {
                        existingGeneralControl.gencontroltype = dataToSend.generalControls.gencontroltype;
                        existingGeneralControl.generalControlscheckedstatus = dataToSend.generalControls.generalControlscheckedstatus;
                        existingGeneralControl.generalControlsstartValue = dataToSend.generalControls.generalControlsstartValue;
                        existingGeneralControl.generalControlsendValue = dataToSend.generalControls.generalControlsendValue;
                        existingGeneralControl.generalControlstransactionDescription = dataToSend.generalControls.generalControlstransactionDescription;
                        existingGeneralControl.generalControlsrangeStartValue = dataToSend.generalControls.generalControlsrangeStartValue;
                        existingGeneralControl.generalControlspercentageDescription = dataToSend.generalControls.generalControlspercentageDescription;
                        existingGeneralControl.generalControlstextDescription = dataToSend.generalControls.generalControlstextDescription;
                    }
                    else
                    {
                        var newGeneralControl = new generalControls
                        {
                            gencontroltype = dataToSend.generalControls.gencontroltype,
                            generalControlscheckedstatus = dataToSend.generalControls.generalControlscheckedstatus,
                            generalControlsstartValue = dataToSend.generalControls.generalControlsstartValue,
                            generalControlsendValue = dataToSend.generalControls.generalControlsendValue,
                            generalControlstransactionDescription = dataToSend.generalControls.generalControlstransactionDescription,
                            generalControlsrangeStartValue = dataToSend.generalControls.generalControlsrangeStartValue,
                            generalControlspercentageDescription = dataToSend.generalControls.generalControlspercentageDescription,
                            generalControlstextDescription = dataToSend.generalControls.generalControlstextDescription,
                            risksamplingstandardsId = dataToSend.risksamplingstandardsId
                        };
                        mySqlDBContext.generalControls.Add(newGeneralControl);
                    }
                }

                mySqlDBContext.SaveChanges();

                return Ok("Data Updated Successfully.");
            

        }
        }
    }
}

