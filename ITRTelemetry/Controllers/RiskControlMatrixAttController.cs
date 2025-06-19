using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System;
using MySQLProvider;
using System.Linq;
using MySql.Data.MySqlClient;
namespace ITRTelemetry.Controllers
{
    public class RiskControlMatrixAttController : ControllerBase
    {

        private readonly CommonDBContext commonDBContext;

        public RiskControlMatrixAttController(CommonDBContext commonDBContext)
        {

            this.commonDBContext = commonDBContext;
        }


        //Getting Nature of Control Performance Details

        [Route("api/NatureOfControlPerformance/GetNatureOfControlPerformanceModelDetails")]
        [HttpGet]

        public IEnumerable<RiskControlMatrixAttModel> GetNatureOfControlPerformanceModelDetails()
        {
            return this.commonDBContext.riskControlMatrixAttModels.Where(x => x.Risk_natureOf_control_perf_status == "Active").ToList();
        }




        [Route("api/NatureOfControlPerformance/InsertNatureOfControlPerformanceModelDetails")]
        [HttpPost]
        public IActionResult InsertNatureOfControlPerformanceModelDetails([FromBody] RiskControlMatrixAttModel riskControlMatrixAttModels)
        {
            try
            {
                riskControlMatrixAttModels.Risk_natureOf_control_perf_name = riskControlMatrixAttModels.Risk_natureOf_control_perf_name?.Trim();
               
                var existingDepartment = this.commonDBContext.riskControlMatrixAttModels
                    .FirstOrDefault(d => d.Risk_natureOf_control_perf_name == riskControlMatrixAttModels.Risk_natureOf_control_perf_name  && d.Risk_natureOf_control_perf_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Nature Control Name with the same name already exists.");
                }

                var existingDepartment1 = this.commonDBContext.riskControlMatrixAttModels
                .FirstOrDefault(d => d.risk_natureOf_control_perf_rating == riskControlMatrixAttModels.risk_natureOf_control_perf_rating && d.Risk_natureOf_control_perf_status == "Active");
               
                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }



                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.riskControlMatrixAttModels;
                riskmatrixModel.Add(riskControlMatrixAttModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlMatrixAttModels.Risk_natureOf_control_perf_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlMatrixAttModels.Risk_natureOf_control_perf_status = "Active";

                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk Nature Control Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/NatureOfControlPerformance/UpdateNatureOfControlPerformanceModelDetails")]
        [HttpPut]
        public IActionResult UpdateNatureOfControlPerformanceModelDetails([FromBody] RiskControlMatrixAttModel riskControlMatrixAttModel)
        {
            try
            {
                if (riskControlMatrixAttModel.natureOf_control_perf_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskControlMatrixAttModel.Risk_natureOf_control_perf_name = riskControlMatrixAttModel.Risk_natureOf_control_perf_name?.Trim();

                    var existingDepartment = this.commonDBContext.riskControlMatrixAttModels
                      .FirstOrDefault(d => d.Risk_natureOf_control_perf_name == riskControlMatrixAttModel.Risk_natureOf_control_perf_name && d.natureOf_control_perf_id != riskControlMatrixAttModel.natureOf_control_perf_id && d.Risk_natureOf_control_perf_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Nature Control Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(riskControlMatrixAttModel);
                    this.commonDBContext.Entry(riskControlMatrixAttModel).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskControlMatrixAttModel);

                    Type type = typeof(RiskControlMatrixAttModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskControlMatrixAttModel, null) == null || property.GetValue(riskControlMatrixAttModel, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok(new { message = "Update successful" });
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Nature Control Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/NatureOfControlPerformance/DeleteNatureOfControlPerformanceModelDetails")]
        [HttpDelete]
        public void DeleteNatureOfControlPerformanceModelDetails(int id)
        {
            var currentClass = new RiskControlMatrixAttModel { natureOf_control_perf_id = id };
            currentClass.Risk_natureOf_control_perf_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("Risk_natureOf_control_perf_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }





        //Getting Control level Details

        [Route("api/ControlLevel/GetControlLevelModelDetails")]
        [HttpGet]

        public IEnumerable<RiskControlLevel> GetControlLevelModelDetails()
        {
            return this.commonDBContext.riskControlLevels.Where(x => x.Risk_Control_level_status == "Active").ToList();
        }




        [Route("api/ControlLevel/InsertControlLevelModelDetails")]
        [HttpPost]
        public IActionResult InsertControlLevelModelDetails([FromBody] RiskControlLevel riskControlLevel)
        {
            try
            {
                riskControlLevel.Risk_Control_level_name = riskControlLevel.Risk_Control_level_name?.Trim();

                var existingDepartment = this.commonDBContext.riskControlLevels
                    .FirstOrDefault(d => d.Risk_Control_level_name == riskControlLevel.Risk_Control_level_name && d.Risk_Control_level_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Level Name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.riskControlLevels;
                riskmatrixModel.Add(riskControlLevel);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlLevel.Risk_Control_level_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlLevel.Risk_Control_level_status = "Active";

                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk Control Level Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/ControlLevel/UpdateControlLevelModelDetails")]
        [HttpPut]
        public IActionResult UpdateControlLevelModelDetails([FromBody] RiskControlLevel riskControlLevel)
        {
            try
            {
                if (riskControlLevel.Risk_Control_level_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskControlLevel.Risk_Control_level_name = riskControlLevel.Risk_Control_level_name?.Trim();

                    var existingDepartment = this.commonDBContext.riskControlLevels
                      .FirstOrDefault(d => d.Risk_Control_level_name == riskControlLevel.Risk_Control_level_name && d.Risk_Control_level_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Control Level Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(riskControlLevel);
                    this.commonDBContext.Entry(riskControlLevel).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskControlLevel);

                    Type type = typeof(RiskControlLevel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskControlLevel, null) == null || property.GetValue(riskControlLevel, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok(new { message = "Update successful" });
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Control Level Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/ControlLevel/DeleteControlLevelModelDetails")]
        [HttpDelete]
        public void DeleteControlLevelModelDetails(int id)
        {
            var currentClass = new RiskControlLevel { Risk_Control_level_id = id };
            currentClass.Risk_Control_level_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("Risk_Control_level_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }



        //Getting Control Dependencies Details

        [Route("api/Controldependency/GetControldependencyModelDetails")]
        [HttpGet]

        public IEnumerable<RiskControldependency> GetControldependencyModelDetails()
        {
            return this.commonDBContext.riskControldependencies.Where(x => x.risk_control_dependencies_status == "Active").ToList();
        }




        [Route("api/Controldependency/InsertControldependencyModelDetails")]
        [HttpPost]
        public IActionResult InsertControldependencyModelDetails([FromBody] RiskControldependency riskControldependency)
        {
            try
            {
                riskControldependency.risk_control_dependencies_name = riskControldependency.risk_control_dependencies_name?.Trim();

                var existingDepartment = this.commonDBContext.riskControldependencies
                    .FirstOrDefault(d => d.risk_control_dependencies_name == riskControldependency.risk_control_dependencies_name && d.risk_control_dependencies_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Dependency Name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.riskControldependencies;
                riskmatrixModel.Add(riskControldependency);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControldependency.risk_control_dependencies_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControldependency.risk_control_dependencies_status = "Active";

                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk Control Dependency Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/Controldependency/UpdateControldependencyModelDetails")]
        [HttpPut]
        public IActionResult UpdateControldependencyModelDetails([FromBody] RiskControldependency riskControldependency)
        {
            try
            {
                if (riskControldependency.risk_control_dependencies_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskControldependency.risk_control_dependencies_name = riskControldependency.risk_control_dependencies_name?.Trim();

                    var existingDepartment = this.commonDBContext.riskControldependencies
                      .FirstOrDefault(d => d.risk_control_dependencies_name == riskControldependency.risk_control_dependencies_name && d.risk_control_dependencies_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Control Dependency Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(riskControldependency);
                    this.commonDBContext.Entry(riskControldependency).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskControldependency);

                    Type type = typeof(RiskControldependency);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskControldependency, null) == null || property.GetValue(riskControldependency, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok(new { message = "Update successful" });
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Control Dependency Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/Controldependency/DeleteControldependencyModelDetails")]
        [HttpDelete]
        public void DeleteControldependencyModelDetails(int id)
        {
            var currentClass = new RiskControldependency { risk_control_dependencies_id = id };
            currentClass.risk_control_dependencies_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_control_dependencies_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }



        //Risk  Nature of control Occurrence 

        [Route("api/NatureOfControl/GetNatureOfControl")]
        [HttpGet]
        public IEnumerable<risk_natureof_cont_occu> GetNatureOfControl()
        {
            return this.commonDBContext.risk_Natureof_Cont_Occus.Where(x => x.risk_natureof_cont_occu_status == "Active")
                .OrderBy(r => r.risk_natureof_cont_occu_rating)
                .ToList();
        }



        [Route("api/NatureOfControl/InsertNatureOfControl")]
        [HttpPost]
        public IActionResult InsertNatureOfControl([FromBody] risk_natureof_cont_occu risk_Natureof_Cont_Occu)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Natureof_Cont_Occu.risk_natureof_cont_occu_name = risk_Natureof_Cont_Occu.risk_natureof_cont_occu_name?.Trim();


                var existingDepartment = this.commonDBContext.risk_Natureof_Cont_Occus
                  .FirstOrDefault(d => d.risk_natureof_cont_occu_name == risk_Natureof_Cont_Occu.risk_natureof_cont_occu_name && d.risk_natureof_cont_occu_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk intensity name with the same name already exists.");
                }

                var existingDepartment1 = this.commonDBContext.risk_Natureof_Cont_Occus
                 .FirstOrDefault(d => d.risk_natureof_cont_occu_rating == risk_Natureof_Cont_Occu.risk_natureof_cont_occu_rating && d.risk_natureof_cont_occu_status == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }



                // Proceed with the insertion
                risk_Natureof_Cont_Occu.risk_natureof_cont_occu_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Natureof_Cont_Occu.risk_natureof_cont_occu_status = "Active";

                this.commonDBContext.risk_Natureof_Cont_Occus.Add(risk_Natureof_Cont_Occu);
                this.commonDBContext.SaveChanges();

                return Ok(new { message = "Record inserted successfully." });

            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }





        [Route("api/NatureOfControl/UpdateNatureOfControl")]
        [HttpPut]
        public IActionResult UpdateNatureOfControl([FromBody] risk_natureof_cont_occu risk_Natureof_Cont_Occu)
        {
            try
            {
                if (risk_Natureof_Cont_Occu.risk_natureof_cont_occu_id == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Natureof_Cont_Occu.risk_natureof_cont_occu_name = risk_Natureof_Cont_Occu.risk_natureof_cont_occu_name?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.commonDBContext.risk_Natureof_Cont_Occus
                        .FirstOrDefault(d => d.risk_natureof_cont_occu_name == risk_Natureof_Cont_Occu.risk_natureof_cont_occu_name
                                          && d.risk_natureof_cont_occu_id != risk_Natureof_Cont_Occu.risk_natureof_cont_occu_id
                                          && d.risk_natureof_cont_occu_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk intensity name with the same name already exists");
                    }

                    var existingValue = this.commonDBContext.risk_Natureof_Cont_Occus
                        .FirstOrDefault(d => d.risk_natureof_cont_occu_rating == risk_Natureof_Cont_Occu.risk_natureof_cont_occu_rating
                                          && d.risk_natureof_cont_occu_id != risk_Natureof_Cont_Occu.risk_natureof_cont_occu_id
                                          && d.risk_natureof_cont_occu_status == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk factor value with the same value already exists.");
                    }



                    // If no duplicates are found, proceed with the update
                    this.commonDBContext.Attach(risk_Natureof_Cont_Occu);
                    this.commonDBContext.Entry(risk_Natureof_Cont_Occu).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risk_Natureof_Cont_Occu);
                    Type type = typeof(risk_natureof_cont_occu);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Natureof_Cont_Occu, null) == null || property.GetValue(risk_Natureof_Cont_Occu, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();

                    return Ok(new { message = "Update successful." });

                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/NatureOfControl/DeleteNatureOfControl")]
        [HttpDelete]
        public void DeleteNatureOfControl(int id)
        {
            var currentClass = new risk_natureof_cont_occu { risk_natureof_cont_occu_id = id };
            currentClass.risk_natureof_cont_occu_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_natureof_cont_occu_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


        //Risk  Frequency of Control Applied

        [Route("api/FrequencyOfControlApplied/GetFrequencyOfControlApplied")]
        [HttpGet]
        public IEnumerable<risk_frqof_contr_appl> GetFrequencyOfControlApplied()
        {
            return this.commonDBContext.risk_frqof_contr_appl.Where(x => x.risk_frqof_contr_appl_status == "Active")
                .OrderBy(r => r.risk_frqof_contr_appl_rating)
                .ToList();
        }



        [Route("api/FrequencyOfControlApplied/InsertFrequencyOfControlApplied")]
        [HttpPost]
        public IActionResult InsertFrequencyOfControlApplied([FromBody] risk_frqof_contr_appl risk_Frqof_Contr_Appl)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Frqof_Contr_Appl.risk_frqof_contr_appl_name = risk_Frqof_Contr_Appl.risk_frqof_contr_appl_name?.Trim();


                var existingDepartment = this.commonDBContext.risk_frqof_contr_appl
                  .FirstOrDefault(d => d.risk_frqof_contr_appl_name == risk_Frqof_Contr_Appl.risk_frqof_contr_appl_name && d.risk_frqof_contr_appl_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Frequency name with the same name already exists.");
                }

                var existingDepartment1 = this.commonDBContext.risk_frqof_contr_appl
                 .FirstOrDefault(d => d.risk_frqof_contr_appl_rating == risk_Frqof_Contr_Appl.risk_frqof_contr_appl_rating && d.risk_frqof_contr_appl_status == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }



                // Proceed with the insertion
                risk_Frqof_Contr_Appl.risk_frqof_contr_appl_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Frqof_Contr_Appl.risk_frqof_contr_appl_status = "Active";

                this.commonDBContext.risk_frqof_contr_appl.Add(risk_Frqof_Contr_Appl);
                this.commonDBContext.SaveChanges();

                return Ok(new { message = "Record inserted successfully." });

            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }





        [Route("api/FrequencyOfControlApplied/UpdateFrequencyOfControlApplied")]
        [HttpPut]
        public IActionResult UpdateFrequencyOfControlApplied([FromBody] risk_frqof_contr_appl risk_Frqof_Contr_Appl)
        {
            try
            {
                if (risk_Frqof_Contr_Appl.risk_frqof_contr_appl_id == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Frqof_Contr_Appl.risk_frqof_contr_appl_name = risk_Frqof_Contr_Appl.risk_frqof_contr_appl_name?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.commonDBContext.risk_frqof_contr_appl
                        .FirstOrDefault(d => d.risk_frqof_contr_appl_name == risk_Frqof_Contr_Appl.risk_frqof_contr_appl_name
                                          && d.risk_frqof_contr_appl_id != risk_Frqof_Contr_Appl.risk_frqof_contr_appl_id
                                          && d.risk_frqof_contr_appl_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk frquency name with the same name already exists");
                    }

                    var existingValue = this.commonDBContext.risk_frqof_contr_appl
                        .FirstOrDefault(d => d.risk_frqof_contr_appl_rating == risk_Frqof_Contr_Appl.risk_frqof_contr_appl_rating
                                          && d.risk_frqof_contr_appl_id != risk_Frqof_Contr_Appl.risk_frqof_contr_appl_id
                                          && d.risk_frqof_contr_appl_status == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk factor value with the same value already exists.");
                    }



                    // If no duplicates are found, proceed with the update
                    this.commonDBContext.Attach(risk_Frqof_Contr_Appl);
                    this.commonDBContext.Entry(risk_Frqof_Contr_Appl).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risk_Frqof_Contr_Appl);
                    Type type = typeof(risk_frqof_contr_appl);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Frqof_Contr_Appl, null) == null || property.GetValue(risk_Frqof_Contr_Appl, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();

                    return Ok(new { message = "Update successful." });

                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/FrequencyOfControlApplied/DeleteFrequencyOfControlApplied")]
        [HttpDelete]
        public void DeleteFrequencyOfControlApplied(int id)
        {
            var currentClass = new risk_frqof_contr_appl { risk_frqof_contr_appl_id = id };
            currentClass.risk_frqof_contr_appl_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_frqof_contr_appl_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


       


    }
}
