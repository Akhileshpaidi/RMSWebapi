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
using System.Threading.Tasks;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RiskAdminController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public RiskAdminController(MySqlDBContext SqlDBContext)
        {
            this.mySqlDBContext = SqlDBContext;
        }



        public Boolean CheckValidation(int? start, int? end, List<int[]> array)
        {
            // Debugging alert equivalent
            //  Console.WriteLine($"{this.min} and {this.max} and {JsonConvert.SerializeObject(this.valArray)}");

            // Sort the array by the first element in each sub-array
            array.Sort((a, b) => a[0].CompareTo(b[0]));

            int n = array.Count;
            if (n > 0)
            {
                // Check if the range is completely outside the bounds of the sorted array
                if (end < array[0][0] || start > array[n - 1][1])
                    return true;

                for (int i = 0; i < n; i++)
                {
                    // Check if the start or end falls within any of the intervals
                    if ((start >= array[i][0] && start <= array[i][1]) || (end >= array[i][0] && end <= array[i][1]))
                        return false;

                    if (i > 0)
                    {
                        // Check if the range is between two intervals
                        if ((start < array[i][0] && start > array[i - 1][1]) && (end < array[i][0] && end > array[i - 1][1]))
                            return true;

                        // Check if the range partially overlaps with adjacent intervals
                        if ((start < array[i][0] && start > array[i - 1][1]) || (end < array[i][0] && end > array[i - 1][1]))
                            return false;
                    }
                }

                return false;
            }
            else
            {
                return true;
            }


        }




        // risk admin Type of risk 

        [Route("api/typeofRisk/GettypeofRisk")]
        [HttpGet]
        public IEnumerable<RiskAdminModel> GettypeofRisk()
        {
            return this.mySqlDBContext.riskAdminModels.Where(x => x.Risk_Admin_typeOfRisk_status == "Active").ToList();


        }

        [Route("api/typeofRisk/insertTypeofRisk")]
        [HttpPost]

        public IActionResult insertTypeofRisk([FromBody] RiskAdminModel riskAdminModel)
        {
            try
            {
                riskAdminModel.Risk_Admin_typeOfRisk_name = riskAdminModel.Risk_Admin_typeOfRisk_name.Trim();

                var existingComplianceGroup = this.mySqlDBContext.riskAdminModels
                    .FirstOrDefault(d => d.Risk_Admin_typeOfRisk_name == riskAdminModel.Risk_Admin_typeOfRisk_name && d.Risk_Admin_typeOfRisk_status == "Active");

                if (existingComplianceGroup != null)
                {
                    return BadRequest("Error: Type Risk with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.riskAdminModels
                  .Where(d => d.isImported == "No")
                 .Max(d => (int?)d.Risk_Admin_typeOfRisk_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                riskAdminModel.Risk_Admin_typeOfRisk_id = maxLawTypeId + 1;
                var typeRisk = this.mySqlDBContext.riskAdminModels;
                typeRisk.Add(riskAdminModel);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskAdminModel.Risk_Admin_typeOfRisk_date = dt1;
                riskAdminModel.Risk_Admin_typeOfRisk_status = "Active";
                riskAdminModel.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Type Risk with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/typeofRisk/updatetypeofRisk")]
        [HttpPut]

        public IActionResult updatetypeofRisk([FromBody] RiskAdminModel riskAdminModel)
        {

            try
            {
                if (riskAdminModel.Risk_Admin_typeOfRisk_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    riskAdminModel.Risk_Admin_typeOfRisk_name = riskAdminModel.Risk_Admin_typeOfRisk_name?.Trim();
                    var existingComplianceGroup = this.mySqlDBContext.riskAdminModels
                       .FirstOrDefault(d => d.Risk_Admin_typeOfRisk_name == riskAdminModel.Risk_Admin_typeOfRisk_name && d.Risk_Admin_typeOfRisk_id != riskAdminModel.Risk_Admin_typeOfRisk_id && d.Risk_Admin_typeOfRisk_status == "Active");

                    if (existingComplianceGroup != null)
                    {
                        return BadRequest("Error: Type Risk with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(riskAdminModel);
                    this.mySqlDBContext.Entry(riskAdminModel).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskAdminModel);

                    Type type = typeof(RiskAdminModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskAdminModel, null) == null || property.GetValue(riskAdminModel, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:Type Risk with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/typeofRisk/DeletetypeofRisk")]
        [HttpDelete]

        public void DeletetypeofRisk(int id)
        {
            var currentClass = new RiskAdminModel { Risk_Admin_typeOfRisk_id = id };
            currentClass.Risk_Admin_typeOfRisk_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Risk_Admin_typeOfRisk_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




        //risk Classification 

        [Route("api/riskClassification/GetriskClassification")]
        [HttpGet]

        public IEnumerable<risk_admin_classification> GetriskClassification()
        {
            return this.mySqlDBContext.riskAdminClassifications.Where(x => x.risk_admin_classification_status == "Active").ToList();
        }




        [Route("api/riskClassification/InsertriskClassification")]
        [HttpPost]
        public IActionResult InsertriskClassification([FromBody] risk_admin_classification risk_Admin_Classification)
        {
            try
            {
                risk_Admin_Classification.risk_admin_classification_name = risk_Admin_Classification.risk_admin_classification_name?.Trim();

                var existingDepartment = this.mySqlDBContext.riskAdminClassifications
                    .FirstOrDefault(d => d.risk_admin_classification_name == risk_Admin_Classification.risk_admin_classification_name && d.risk_admin_classification_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk classification Name with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.riskAdminClassifications
                  .Where(d => d.isImported == "No")
                 .Max(d => (int?)d.risk_admin_classification_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                risk_Admin_Classification.risk_admin_classification_id = maxLawTypeId + 1;
                // Proceed with the insertion
                var riskmatrixModel = this.mySqlDBContext.riskAdminClassifications;
                riskmatrixModel.Add(risk_Admin_Classification);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Classification.risk_admin_classification_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Classification.risk_admin_classification_status = "Active";
                risk_Admin_Classification.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk classification Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/riskClassification/UpdateriskClassification")]
        [HttpPut]
        public IActionResult UpdateriskClassification([FromBody] risk_admin_classification risk_Admin_Classification)
        {
            try
            {
                if (risk_Admin_Classification.risk_admin_classification_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Admin_Classification.risk_admin_classification_name = risk_Admin_Classification.risk_admin_classification_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.riskAdminClassifications
                      .FirstOrDefault(d => d.risk_admin_classification_name == risk_Admin_Classification.risk_admin_classification_name && d.risk_admin_classification_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Classification Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(risk_Admin_Classification);
                    this.mySqlDBContext.Entry(risk_Admin_Classification).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Classification);

                    Type type = typeof(risk_admin_classification);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Classification, null) == null || property.GetValue(risk_Admin_Classification, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk classification Name  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/riskClassification/DeleteriskClassification")]
        [HttpDelete]
        public void DeleteRiskCauselistModelDetails(int id)
        {
            var currentClass = new risk_admin_classification { risk_admin_classification_id = id };
            currentClass.risk_admin_classification_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_classification_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //risk  admin risk impact rating


        [Route("api/adminRiskImpact/GetadminRiskImpact")]
        [HttpGet]
        public IEnumerable<risk_admin_riskimpactrating> GetRiskImpact()
        {
            return this.mySqlDBContext.risk_admin_riskimpactrating.Where(x => x.risk_admin_riskImpactRating_status == "Active")
                .OrderBy(r => r.risk_admin_riskImpactRating_value)
                .ToList();
        }


        //[Route("api/adminRiskImpact/InsertadminRiskImpact")]
        //[HttpPost]
        //public IActionResult InsertadminRiskImpact([FromBody] risk_admin_riskimpactrating riskImpactRating)
        //{
        //    try
        //    {
        //        // Trim the name to avoid any issues with leading/trailing spaces
        //        riskImpactRating.risk_admin_riskImpactRating_name = riskImpactRating.risk_admin_riskImpactRating_name?.Trim();


        //        var existingDepartment = this.mySqlDBContext.risk_admin_riskimpactrating
        //          .FirstOrDefault(d => d.risk_admin_riskImpactRating_name == riskImpactRating.risk_admin_riskImpactRating_name && d.risk_admin_riskImpactRating_status == "Active");

        //        if (existingDepartment != null)
        //        {
        //            // Department with the same name already exists, return an error message
        //            return BadRequest("Error: Risk impact name with the same name already exists.");
        //        }

        //        var existingDepartment1 = this.mySqlDBContext.risk_admin_riskimpactrating
        //         .FirstOrDefault(d => d.risk_admin_riskImpactRating_value == riskImpactRating.risk_admin_riskImpactRating_value && d.risk_admin_riskImpactRating_status == "Active");

        //        if (existingDepartment1 != null)
        //        {
        //            // Department with the same name already exists, return an error message
        //            return BadRequest("Error: Risk factor value with the same value already exists.");
        //        }

        //        var existingDepartment2 = this.mySqlDBContext.risk_admin_riskimpactrating
        //       .FirstOrDefault(d => d.color_reference == riskImpactRating.color_reference && d.risk_admin_riskImpactRating_status == "Active");

        //        if (existingDepartment2 != null)
        //        {
        //            // Department with the same name already exists, return an error message
        //            return BadRequest("Error: Risk color refrence with the same color already exists.");
        //        }
        //        var maxLawTypeId = this.mySqlDBContext.risk_admin_riskimpactrating
        //     .Where(d => d.isImported == "No")
        //    .Max(d => (int?)d.risk_admin_riskImpactRating_id) ?? 0; // If no records are found, default to 0
        //                                                            // Increment the law_type_id by 1
        //        riskImpactRating.risk_admin_riskImpactRating_id = maxLawTypeId + 1;

        //        // Proceed with the insertion
        //        riskImpactRating.risk_admin_riskImpactRating_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //        riskImpactRating.risk_admin_riskImpactRating_status = "Active";
        //        riskImpactRating.isImported = "No";
        //        this.mySqlDBContext.risk_admin_riskimpactrating.Add(riskImpactRating);
        //        this.mySqlDBContext.SaveChanges();

        //        return Ok("Record inserted successfully.");
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
        //        {
        //            // MySQL error number 1062 corresponds to a duplicate entry violation
        //            return BadRequest("Error: A record with the same values already exists.");
        //        }
        //        else
        //        {
        //            // Handle other database update exceptions
        //            return BadRequest($"Error: {ex.Message}");
        //        }
        //    }
        //}

        [Route("api/adminRiskImpact/InsertadminRiskImpact")]
        [HttpPost]
        public IActionResult InsertadminRiskImpact([FromBody] risk_admin_riskimpactrating riskImpactRating)
        {
            try
            {
                riskImpactRating.risk_admin_riskImpactRating_name = riskImpactRating.risk_admin_riskImpactRating_name?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_riskimpactrating
                  .FirstOrDefault(d => d.risk_admin_riskImpactRating_name == riskImpactRating.risk_admin_riskImpactRating_name && d.risk_admin_riskImpactRating_status == "Active");

                if (existingDepartment != null)
                {
                    return BadRequest("Error: Risk impact name with the same name already exists.");
                }

                var existingDepartment1 = this.mySqlDBContext.risk_admin_riskimpactrating
                 .FirstOrDefault(d => d.risk_admin_riskImpactRating_value == riskImpactRating.risk_admin_riskImpactRating_value && d.risk_admin_riskImpactRating_status == "Active");

                if (existingDepartment1 != null)
                {
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }

                var existingDepartment2 = this.mySqlDBContext.risk_admin_riskimpactrating
               .FirstOrDefault(d => d.color_reference == riskImpactRating.color_reference && d.risk_admin_riskImpactRating_status == "Active");

                if (existingDepartment2 != null)
                {
                   return BadRequest("Error: Risk color refrence with the same color already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_riskimpactrating
              .Where(d => d.isImported == "No")
             .Max(d => (int?)d.risk_admin_riskImpactRating_id) ?? 0; 
                riskImpactRating.risk_admin_riskImpactRating_id = maxLawTypeId + 1;
                riskImpactRating.risk_admin_riskImpactRating_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                riskImpactRating.risk_admin_riskImpactRating_status = "Active";
                riskImpactRating.isImported = "No";
                this.mySqlDBContext.risk_admin_riskimpactrating.Add(riskImpactRating);
                this.mySqlDBContext.SaveChanges();

                return Ok("Record inserted successfully.");
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



        [Route("api/adminRiskImpact/UpdateadminRiskImpact")]
        [HttpPut]
        public IActionResult UpdateRiskImpact([FromBody] risk_admin_riskimpactrating riskImpactRating)
        {
            try
            {
                if (riskImpactRating.risk_admin_riskImpactRating_id == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    riskImpactRating.risk_admin_riskImpactRating_name = riskImpactRating.risk_admin_riskImpactRating_name?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_riskimpactrating
                        .FirstOrDefault(d => d.risk_admin_riskImpactRating_name == riskImpactRating.risk_admin_riskImpactRating_name
                                          && d.risk_admin_riskImpactRating_id != riskImpactRating.risk_admin_riskImpactRating_id
                                          && d.risk_admin_riskImpactRating_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk impact name with the same name already exists");
                    }

                    var existingValue = this.mySqlDBContext.risk_admin_riskimpactrating
                        .FirstOrDefault(d => d.risk_admin_riskImpactRating_value == riskImpactRating.risk_admin_riskImpactRating_value
                                          && d.risk_admin_riskImpactRating_id != riskImpactRating.risk_admin_riskImpactRating_id
                                          && d.risk_admin_riskImpactRating_status == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk factor value with the same value already exists.");
                    }

                    var existingColorReference = this.mySqlDBContext.risk_admin_riskimpactrating
                        .FirstOrDefault(d => d.color_reference == riskImpactRating.color_reference
                                          && d.risk_admin_riskImpactRating_id != riskImpactRating.risk_admin_riskImpactRating_id
                                          && d.risk_admin_riskImpactRating_status == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }

                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(riskImpactRating);
                    this.mySqlDBContext.Entry(riskImpactRating).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskImpactRating);
                    Type type = typeof(risk_admin_riskimpactrating);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskImpactRating, null) == null || property.GetValue(riskImpactRating, null).Equals(0))
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
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/adminRiskImpact/DeleteAdminRiskImpact")]
        [HttpDelete]
        public void DeleteRiskImpact(int id)
        {
            var currentClass = new risk_admin_riskimpactrating { risk_admin_riskImpactRating_id = id };
            currentClass.risk_admin_riskImpactRating_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_riskImpactRating_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //risk  admin risk likelihood factor occurence 


        [Route("api/risk_admin_likeoccfact/Getrisk_admin_likeoccfact")]
        [HttpGet]
        public IEnumerable<risk_admin_likeoccfact> Getrisk_admin_likeoccfact()
        {
            return this.mySqlDBContext.risk_admin_likeoccfact.Where(x => x.risk_admin_likeoccfact_status == "Active")
                .OrderBy(r => r.risk_admin_likeoccfact_value)
                .ToList();
        }


        [Route("api/risk_admin_likeoccfact/Insertrisk_admin_likeoccfact")]
        [HttpPost]
        public IActionResult Insertrisk_admin_likeoccfact([FromBody] risk_admin_likeoccfact risk_Admin_Likeoccfact)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Admin_Likeoccfact.risk_admin_likeoccfact_name = risk_Admin_Likeoccfact.risk_admin_likeoccfact_name?.Trim();


                var existingDepartment = this.mySqlDBContext.risk_admin_likeoccfact
                  .FirstOrDefault(d => d.risk_admin_likeoccfact_name == risk_Admin_Likeoccfact.risk_admin_likeoccfact_name && d.risk_admin_likeoccfact_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk likelihood name with the same name already exists.");
                }

                var existingDepartment1 = this.mySqlDBContext.risk_admin_likeoccfact
                 .FirstOrDefault(d => d.risk_admin_likeoccfact_value == risk_Admin_Likeoccfact.risk_admin_likeoccfact_value && d.risk_admin_likeoccfact_status == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }

                var existingDepartment2 = this.mySqlDBContext.risk_admin_likeoccfact
               .FirstOrDefault(d => d.color_reference == risk_Admin_Likeoccfact.color_reference && d.risk_admin_likeoccfact_status == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk color refrence with the same color already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_likeoccfact
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_likeoccfact_id) ?? 0; // If no records are found, default to 0
                                                        // Increment the law_type_id by 1
                risk_Admin_Likeoccfact.risk_admin_likeoccfact_id = maxLawTypeId + 1;


                // Proceed with the insertion
                risk_Admin_Likeoccfact.risk_admin_likeoccfact_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Likeoccfact.risk_admin_likeoccfact_status = "Active";
                risk_Admin_Likeoccfact.isImported = "No";
                this.mySqlDBContext.risk_admin_likeoccfact.Add(risk_Admin_Likeoccfact);
                this.mySqlDBContext.SaveChanges();

                return Ok("Record inserted successfully.");
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




        [Route("api/risk_admin_likeoccfact/Updaterisk_admin_likeoccfact")]
        [HttpPut]
        public IActionResult UpdateRiskImpact([FromBody] risk_admin_likeoccfact risk_Admin_Likeoccfact)
        {
            try
            {
                if (risk_Admin_Likeoccfact.risk_admin_likeoccfact_id == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Admin_Likeoccfact.risk_admin_likeoccfact_name = risk_Admin_Likeoccfact.risk_admin_likeoccfact_name?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_likeoccfact
                        .FirstOrDefault(d => d.risk_admin_likeoccfact_name == risk_Admin_Likeoccfact.risk_admin_likeoccfact_name
                                          && d.risk_admin_likeoccfact_id != risk_Admin_Likeoccfact.risk_admin_likeoccfact_id
                                          && d.risk_admin_likeoccfact_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk likelihood name with the same name already exists");
                    }

                    var existingValue = this.mySqlDBContext.risk_admin_likeoccfact
                        .FirstOrDefault(d => d.risk_admin_likeoccfact_value == risk_Admin_Likeoccfact.risk_admin_likeoccfact_value
                                          && d.risk_admin_likeoccfact_id != risk_Admin_Likeoccfact.risk_admin_likeoccfact_id
                                          && d.risk_admin_likeoccfact_status == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk factor value with the same value already exists.");
                    }

                    var existingColorReference = this.mySqlDBContext.risk_admin_likeoccfact
                        .FirstOrDefault(d => d.color_reference == risk_Admin_Likeoccfact.color_reference
                                          && d.risk_admin_likeoccfact_id != risk_Admin_Likeoccfact.risk_admin_likeoccfact_id
                                          && d.risk_admin_likeoccfact_status == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }

                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(risk_Admin_Likeoccfact);
                    this.mySqlDBContext.Entry(risk_Admin_Likeoccfact).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Likeoccfact);
                    Type type = typeof(risk_admin_likeoccfact);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Likeoccfact, null) == null || property.GetValue(risk_Admin_Likeoccfact, null).Equals(0))
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
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/risk_admin_likeoccfact/Deleterisk_admin_likeoccfact")]
        [HttpDelete]
        public void Deleterisk_admin_likeoccfact(int id)
        {
            var currentClass = new risk_admin_likeoccfact { risk_admin_likeoccfact_id = id };
            currentClass.risk_admin_likeoccfact_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_likeoccfact_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        // risk admin Risk Categorization

        [Route("api/risk_admin_risk_categorization/Getrisk_admin_risk_categorization")]
        [HttpGet]
        public IEnumerable<risk_admin_risk_categorization> Getrisk_admin_risk_categorization()
        {
            return this.mySqlDBContext.risk_admin_risk_categorization.Where(x => x.risk_admin_risk_categorizationStatus == "Active").ToList();


        }

        [Route("api/risk_admin_risk_categorization/insertrisk_admin_risk_categorization")]
        [HttpPost]

        public IActionResult insertrisk_admin_risk_categorization([FromBody] risk_admin_risk_categorization risk_Admin_Risk_Categorization)
        {
            try
            {
                risk_Admin_Risk_Categorization.risk_admin_risk_categorizationName = risk_Admin_Risk_Categorization.risk_admin_risk_categorizationName.Trim();

                var existingComplianceGroup = this.mySqlDBContext.risk_admin_risk_categorization
                    .FirstOrDefault(d => d.risk_admin_risk_categorizationName == risk_Admin_Risk_Categorization.risk_admin_risk_categorizationName && d.risk_admin_risk_categorizationStatus == "Active");

                if (existingComplianceGroup != null)
                {
                    return BadRequest("Error: Type Risk categorization with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_risk_categorization
                  .Where(d => d.isImported == "No")
                 .Max(d => (int?)d.risk_admin_risk_categorization_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                risk_Admin_Risk_Categorization.risk_admin_risk_categorization_id = maxLawTypeId + 1;
                var typeRisk = this.mySqlDBContext.risk_admin_risk_categorization;
                typeRisk.Add(risk_Admin_Risk_Categorization);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Risk_Categorization.risk_admin_risk_categorizationDate = dt1;
                risk_Admin_Risk_Categorization.risk_admin_risk_categorizationStatus = "Active";
                risk_Admin_Risk_Categorization.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Type Risk categorization with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/risk_admin_risk_categorization/updatetrisk_admin_risk_categorization")]
        [HttpPut]

        public IActionResult updatetrisk_admin_risk_categorization([FromBody] risk_admin_risk_categorization risk_Admin_Risk_Categorization)
        {

            try
            {
                if (risk_Admin_Risk_Categorization.risk_admin_risk_categorization_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Admin_Risk_Categorization.risk_admin_risk_categorizationName = risk_Admin_Risk_Categorization.risk_admin_risk_categorizationName?.Trim();
                    var existingComplianceGroup = this.mySqlDBContext.risk_admin_risk_categorization
                       .FirstOrDefault(d => d.risk_admin_risk_categorizationName == risk_Admin_Risk_Categorization.risk_admin_risk_categorizationName && d.risk_admin_risk_categorization_id != risk_Admin_Risk_Categorization.risk_admin_risk_categorization_id && d.risk_admin_risk_categorizationStatus == "Active");

                    if (existingComplianceGroup != null)
                    {
                        return BadRequest("Error: Type Risk categorization with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(risk_Admin_Risk_Categorization);
                    this.mySqlDBContext.Entry(risk_Admin_Risk_Categorization).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Risk_Categorization);

                    Type type = typeof(risk_admin_risk_categorization);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Risk_Categorization, null) == null || property.GetValue(risk_Admin_Risk_Categorization, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:Type Risk categorization with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/risk_admin_risk_categorization/Deleterisk_admin_risk_categorization")]
        [HttpDelete]

        public void Deleterisk_admin_risk_categorization(int id)
        {
            var currentClass = new risk_admin_risk_categorization { risk_admin_risk_categorization_id = id };
            currentClass.risk_admin_risk_categorizationStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_risk_categorizationStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        // risk admin Risk cause List

        [Route("api/risk_admin_causelist/Getrisk_admin_causelist")]
        [HttpGet]
        public IEnumerable<risk_admin_causelist> Getrisk_admin_causelist()
        {
            return this.mySqlDBContext.risk_admin_causelist.Where(x => x.risk_admin_causeListStatus == "Active").ToList();


        }

        [Route("api/risk_admin_causelist/insertrisk_admin_causelist")]
        [HttpPost]

        public IActionResult insertrisk_admin_causelist([FromBody] risk_admin_causelist risk_Admin_Causelist)
        {
            try
            {
                risk_Admin_Causelist.risk_admin_causeListName = risk_Admin_Causelist.risk_admin_causeListName.Trim();

                var existingComplianceGroup = this.mySqlDBContext.risk_admin_causelist
                    .FirstOrDefault(d => d.risk_admin_causeListName == risk_Admin_Causelist.risk_admin_causeListName && d.risk_admin_causeListStatus == "Active");

                if (existingComplianceGroup != null)
                {
                    return BadRequest("Error: Type Risk Cause List with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_causelist
                  .Where(d => d.isImported == "No")
                 .Max(d => (int?)d.risk_admin_causeList_id) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                risk_Admin_Causelist.risk_admin_causeList_id = maxLawTypeId + 1;
                var typeRisk = this.mySqlDBContext.risk_admin_causelist;
                typeRisk.Add(risk_Admin_Causelist);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Causelist.risk_admin_causeListdate = dt1;
                risk_Admin_Causelist.risk_admin_causeListStatus = "Active";
                risk_Admin_Causelist.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Type Risk Cause List with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/risk_admin_causelist/updaterisk_admin_causelist")]
        [HttpPut]

        public IActionResult updaterisk_admin_causelist([FromBody] risk_admin_causelist risk_Admin_Causelist)
        {

            try
            {
                if (risk_Admin_Causelist.risk_admin_causeList_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Admin_Causelist.risk_admin_causeListName = risk_Admin_Causelist.risk_admin_causeListName?.Trim();
                    var existingComplianceGroup = this.mySqlDBContext.risk_admin_causelist
                       .FirstOrDefault(d => d.risk_admin_causeListName == risk_Admin_Causelist.risk_admin_causeListName && d.risk_admin_causeList_id != risk_Admin_Causelist.risk_admin_causeList_id && d.risk_admin_causeListStatus == "Active");

                    if (existingComplianceGroup != null)
                    {
                        return BadRequest("Error: Type Risk Cause List with the same name already exists.");
                    }
                    this.mySqlDBContext.Attach(risk_Admin_Causelist);
                    this.mySqlDBContext.Entry(risk_Admin_Causelist).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Causelist);

                    Type type = typeof(risk_admin_causelist);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Causelist, null) == null || property.GetValue(risk_Admin_Causelist, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:Type Risk Cause List with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        [Route("api/risk_admin_causelist/Deleterisk_admin_causelist")]
        [HttpDelete]

        public void Deleterisk_admin_causelist(int id)
        {
            var currentClass = new risk_admin_causelist { risk_admin_causeList_id = id };
            currentClass.risk_admin_causeListStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_causeListStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
      
        }



        //Risk Admin Risk Priority

        [Route("api/risk_admin_riskpriority/Getrisk_admin_riskpriority")]
        [HttpGet]
        public IEnumerable<risk_admin_riskpriority> Getrisk_admin_riskpriority()
        {
            return this.mySqlDBContext.risk_admin_riskpriority.Where(x => x.risk_admin_riskPriorityStatus == "Active")
                .OrderBy(r => r.rating_level_min)
                .ToList();
        }


        //Risk Priority Insert Method
        [Route("api/risk_admin_riskpriority/Insertrisk_admin_riskpriority")]
        [HttpPost]
        public async Task< IActionResult> Insertrisk_admin_riskpriority([FromBody] risk_admin_riskpriority risk_Admin_Riskpriority)
        {
            try
            {
                risk_Admin_Riskpriority.risk_admin_riskPriorityName = risk_Admin_Riskpriority.risk_admin_riskPriorityName?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_riskpriority
                    .FirstOrDefault(d => d.risk_admin_riskPriorityName == risk_Admin_Riskpriority.risk_admin_riskPriorityName && d.risk_admin_riskPriorityId != risk_Admin_Riskpriority.risk_admin_riskPriorityId && d.risk_admin_riskPriorityStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Priority with the same name already exists.");
                }
                if (risk_Admin_Riskpriority.rating_level_min > risk_Admin_Riskpriority.rating_level_max)
                {
                    return BadRequest("Error: Risk priority Minimum value is greater that  Risk Priority Maximum Value.");
                }

              


                // Set min and max using either the provided values or the existing ones.
                var newMin = risk_Admin_Riskpriority.rating_level_min ;
                var newMax = risk_Admin_Riskpriority.rating_level_max ;



                var existingRanges = await mySqlDBContext.risk_admin_riskpriority
                    .Where(d => d.risk_admin_riskPriorityId != risk_Admin_Riskpriority.risk_admin_riskPriorityId && d.risk_admin_riskPriorityStatus == "Active")
                    .Select(d => new { MinValue = d.rating_level_min, MaxValue = d.rating_level_max })
                    .ToListAsync();

                // Check for overlapping ranges.
                foreach (var range in existingRanges)
                {
                    bool isOverlapping =
                        (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                        (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                        (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                    if (isOverlapping)
                    {
                        return BadRequest("Error: New rating values overlap with existing ranges.");
                    }
                }
              
                    // Check validation for min and max values
                    if (risk_Admin_Riskpriority.rating_level_min != null &&
                        risk_Admin_Riskpriority.rating_level_max != null 
                        )
                    {

                        if (risk_Admin_Riskpriority.rating_level_min > risk_Admin_Riskpriority.rating_level_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                    }
                }



                var maxLawTypeId = this.mySqlDBContext.risk_admin_riskpriority
                 .Where(d => d.isImported == "No")
                .Max(d => (int?)d.risk_admin_riskPriorityId) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                risk_Admin_Riskpriority.risk_admin_riskPriorityId = maxLawTypeId + 1;

                var AuthorityNameModel = this.mySqlDBContext.risk_admin_riskpriority;
                AuthorityNameModel.Add(risk_Admin_Riskpriority);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Riskpriority.risk_admin_riskPriorityDate = dt1;
                risk_Admin_Riskpriority.isImported = "No";

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Riskpriority.risk_admin_riskPriorityStatus = "Active";
               

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Priority with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Update priority method

        [Route("api/risk_admin_riskpriority/Updaterisk_admin_riskpriority")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_admin_riskpriority([FromBody] risk_admin_riskpriority risk_Admin_Riskpriority)
        {
            try
            {
                if (risk_Admin_Riskpriority.risk_admin_riskPriorityId == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Admin_Riskpriority.risk_admin_riskPriorityName = risk_Admin_Riskpriority.risk_admin_riskPriorityName?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_riskpriority
                        .FirstOrDefault(d => d.risk_admin_riskPriorityName == risk_Admin_Riskpriority.risk_admin_riskPriorityName
                                          && d.risk_admin_riskPriorityId != risk_Admin_Riskpriority.risk_admin_riskPriorityId
                                          && d.risk_admin_riskPriorityStatus == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk Priority name with the same name already exists");
                    }
                    if (risk_Admin_Riskpriority.rating_level_min > risk_Admin_Riskpriority.rating_level_max)
                    {
                        return BadRequest("Error: Risk priority Minimum value is greater that  Risk Priority Maximum Value.");
                    }
               

                    var existingColorReference = this.mySqlDBContext.risk_admin_riskpriority
                        .FirstOrDefault(d => d.color_code == risk_Admin_Riskpriority.color_code
                                          && d.risk_admin_riskPriorityId != risk_Admin_Riskpriority.risk_admin_riskPriorityId
                                          && d.risk_admin_riskPriorityStatus == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }






                    var existingValues = await mySqlDBContext.risk_admin_riskpriority
.Where(x => x.risk_admin_riskPriorityId == risk_Admin_Riskpriority.risk_admin_riskPriorityId)
.Select(x => new
{
 MinValue = x.rating_level_min,
 MaxValue = x.rating_level_max
})
.FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Admin_Riskpriority.rating_level_min ?? existingValues.MinValue;
                    var newMax = risk_Admin_Riskpriority.rating_level_max ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_riskpriority
                        .Where(d => d.risk_admin_riskPriorityId != risk_Admin_Riskpriority.risk_admin_riskPriorityId && d.risk_admin_riskPriorityStatus == "Active")
                        .Select(d => new { MinValue = d.rating_level_min, MaxValue = d.rating_level_max })
                        .ToListAsync();

                    // Check for overlapping ranges.
                    foreach (var range in existingRanges)
                    {
                        bool isOverlapping =
                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                        if (isOverlapping)
                        {
                            return BadRequest("Error: New rating values overlap with existing ranges.");
                        }
                    }
                    if (existingValues != null)
                    {
                        // Check validation for min and max values
                        if (risk_Admin_Riskpriority.rating_level_min == null &&
                            risk_Admin_Riskpriority.rating_level_max != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_Admin_Riskpriority.rating_level_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_Admin_Riskpriority.rating_level_min != null &&
                            risk_Admin_Riskpriority.rating_level_max == null)
                        {


                            if (risk_Admin_Riskpriority.rating_level_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }
                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(risk_Admin_Riskpriority);
                    this.mySqlDBContext.Entry(risk_Admin_Riskpriority).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Riskpriority);
                    Type type = typeof(risk_admin_riskpriority);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Riskpriority, null) == null || property.GetValue(risk_Admin_Riskpriority, null).Equals(0))
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
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }


        }



        //Delete Priority Method

        [Route("api/risk_admin_riskpriority/Deleterisk_admin_riskpriority")]
        [HttpDelete]
        public void Deleterisk_admin_riskpriority(int id)
        {
            var currentClass = new risk_admin_riskpriority { risk_admin_riskPriorityId = id };
            currentClass.risk_admin_riskPriorityStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_riskPriorityStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();

            //            var recordToDelete = this.mySqlDBContext.risk_admin_riskpriority
            //.FirstOrDefault(c => c.risk_admin_riskPriorityId == id);

            //            if (recordToDelete == null)
            //            {

            //                return NotFound("Error: Record not found.");
            //            }
            //            this.mySqlDBContext.risk_admin_riskpriority.Remove(recordToDelete);
            //            this.mySqlDBContext.SaveChanges();

            //            return Ok("Record deleted successfully.");
        }




        //Potential business impact



        [Route("api/risk_admin_potenbussimpact/Getrisk_admin_potenbussimpact")]
        [HttpGet]
        public IEnumerable<risk_admin_potenbussimpact> Getrisk_admin_potenbussimpact()
        {
            return this.mySqlDBContext.risk_admin_potenbussimpact.Where(x => x.risk_admin_potenBussImpactstatus == "Active")
                .OrderBy(r => r.risk_admin_potenBussImpactid)
                .ToList();
        }


        //Risk Priority Insert Method
        [Route("api/risk_admin_potenbussimpact/Insertrisk_admin_potenbussimpact")]
        [HttpPost]
        public IActionResult Insertrisk_admin_potenbussimpact([FromBody] risk_admin_potenbussimpact potential_business_impacts)
        {
            try
            {
                potential_business_impacts.risk_admin_potenBussImpactname = potential_business_impacts.risk_admin_potenBussImpactname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_potenbussimpact
                    .FirstOrDefault(d => d.risk_admin_potenBussImpactname == potential_business_impacts.risk_admin_potenBussImpactname && d.risk_admin_potenBussImpactstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Potential name with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_potenbussimpact
                 .Where(d => d.isImported == "No")
                .Max(d => (int?)d.risk_admin_potenBussImpactid) ?? 0; // If no records are found, default to 0

                // Increment the law_type_id by 1
                potential_business_impacts.risk_admin_potenBussImpactid = maxLawTypeId + 1;
                // Proceed with the insertion
                var AuthorityNameModel = this.mySqlDBContext.risk_admin_potenbussimpact;
                AuthorityNameModel.Add(potential_business_impacts);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                potential_business_impacts.risk_admin_potenBussImpactdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                potential_business_impacts.risk_admin_potenBussImpactstatus = "Active";
                potential_business_impacts.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: AuthorityName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Update priority method

        [Route("api/risk_admin_potenbussimpact/Updaterisk_admin_potenbussimpact")]
        [HttpPut]
        public IActionResult Updaterisk_admin_potenbussimpact([FromBody] risk_admin_potenbussimpact potential_business_impacts)
        {
            try
            {
                if (potential_business_impacts.risk_admin_potenBussImpactid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    potential_business_impacts.risk_admin_potenBussImpactname = potential_business_impacts.risk_admin_potenBussImpactname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_potenbussimpact
                     .FirstOrDefault(d => d.risk_admin_potenBussImpactname == potential_business_impacts.risk_admin_potenBussImpactname && d.risk_admin_potenBussImpactid != potential_business_impacts.risk_admin_potenBussImpactid && d.risk_admin_potenBussImpactstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: AuthorityName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(potential_business_impacts);
                    this.mySqlDBContext.Entry(potential_business_impacts).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(potential_business_impacts);

                    Type type = typeof(risk_admin_potenbussimpact);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(potential_business_impacts, null) == null || property.GetValue(potential_business_impacts, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: AuthorityName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }



        //Delete Priority Method

        [Route("api/risk_admin_potenbussimpact/Deleterisk_admin_potenbussimpact")]
        [HttpDelete]
        public void Deleterisk_admin_potenbussimpact(int id)
        {
            var currentClass = new risk_admin_potenbussimpact { risk_admin_potenBussImpactid = id };
            currentClass.risk_admin_potenBussImpactstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_potenBussImpactstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Risk risk risk Appetite

        [Route("api/risk_admin_riskappetite/Getrisk_admin_riskappetite")]
        [HttpGet]
        public IEnumerable<risk_admin_riskappetite> Getrisk_admin_riskappetite()
        {
            return this.mySqlDBContext.risk_admin_riskappetite.Where(x => x.risk_admin_RiskAppetiteStatus == "Active")
                .OrderBy(r => r.risk_level_range_min)
                .ToList();
        }



        [Route("api/risk_admin_riskappetite/Insertrisk_admin_riskappetite")]
        [HttpPost]
        public async Task< IActionResult> Insertrisk_admin_riskappetite([FromBody] risk_admin_riskappetite Insertrisk_admin_riskappetite)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                Insertrisk_admin_riskappetite.risk_admin_RiskAppetiteName = Insertrisk_admin_riskappetite.risk_admin_RiskAppetiteName?.Trim();


                var existingDepartment = this.mySqlDBContext.risk_admin_riskappetite
                  .FirstOrDefault(d => d.risk_admin_RiskAppetiteName == Insertrisk_admin_riskappetite.risk_admin_RiskAppetiteName && d.risk_admin_RiskAppetiteStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Appetite name with the same name already exists.");
                }


                //if (Insertrisk_admin_riskappetite.risk_level_range_min != null && Insertrisk_admin_riskappetite.risk_level_range_max != null)
                //{
                //    if (!CheckValidation(Insertrisk_admin_riskappetite.risk_level_range_min, Insertrisk_admin_riskappetite.risk_level_range_max, Insertrisk_admin_riskappetite.array))
                //    {
                //        return BadRequest("Error: Risk intensity rating Range Not Valid.");
                //    }
                //}


                if (Insertrisk_admin_riskappetite.risk_level_range_min > Insertrisk_admin_riskappetite.risk_level_range_max)
                {
                    return BadRequest("Error: Risk Appetite Minimum value is greater that  Risk Appetite Maximum Value.");
                }

                var existingDepartment2 = this.mySqlDBContext.risk_admin_riskappetite
               .FirstOrDefault(d => d.colour_reference == Insertrisk_admin_riskappetite.colour_reference && d.risk_admin_RiskAppetiteStatus == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk color refrence with the same color already exists.");
                }

                var newMin = Insertrisk_admin_riskappetite.risk_level_range_min;
                var newMax = Insertrisk_admin_riskappetite.risk_level_range_max ;



                var existingRanges = await mySqlDBContext.risk_admin_riskappetite
                    .Where(d => d.risk_admin_RiskAppetiteId != Insertrisk_admin_riskappetite.risk_admin_RiskAppetiteId && d.risk_admin_RiskAppetiteStatus == "Active")
                    .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                    .ToListAsync();

                // Check for overlapping ranges.
                foreach (var range in existingRanges)
                {
                    bool isOverlapping =
                        (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                        (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                        (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                    if (isOverlapping)
                    {
                        return BadRequest("Error: New rating values overlap with existing ranges.");
                    }
                }
          
                    if (Insertrisk_admin_riskappetite.risk_level_range_min != null &&
                        Insertrisk_admin_riskappetite.risk_level_range_max != null)
                    {

                        if (Insertrisk_admin_riskappetite.risk_level_range_min > Insertrisk_admin_riskappetite.risk_level_range_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                        }
                    }


                    // Proceed with the insertion
                    Insertrisk_admin_riskappetite.risk_admin_RiskAppetiteDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Insertrisk_admin_riskappetite.risk_admin_RiskAppetiteStatus = "Active";
                Insertrisk_admin_riskappetite.isImported = "No";
                this.mySqlDBContext.risk_admin_riskappetite.Add(Insertrisk_admin_riskappetite);
                this.mySqlDBContext.SaveChanges();

                return Ok("Record inserted successfully.");
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





        [Route("api/risk_admin_riskappetite/Updaterisk_admin_riskappetite")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_admin_riskappetite([FromBody] risk_admin_riskappetite risk_Admin_Riskappetite)
        {
            try
            {
                if (risk_Admin_Riskappetite.risk_admin_RiskAppetiteId == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Admin_Riskappetite.risk_admin_RiskAppetiteName = risk_Admin_Riskappetite.risk_admin_RiskAppetiteName?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_riskappetite
                        .FirstOrDefault(d => d.risk_admin_RiskAppetiteName == risk_Admin_Riskappetite.risk_admin_RiskAppetiteName
                                          && d.risk_admin_RiskAppetiteId != risk_Admin_Riskappetite.risk_admin_RiskAppetiteId
                                          && d.risk_admin_RiskAppetiteStatus == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk Appetite name with the same name already exists");
                    }

      

                    var existingColorReference = this.mySqlDBContext.risk_admin_riskappetite
                        .FirstOrDefault(d => d.colour_reference == risk_Admin_Riskappetite.colour_reference
                                          && d.risk_admin_RiskAppetiteId != risk_Admin_Riskappetite.risk_admin_RiskAppetiteId
                                          && d.risk_admin_RiskAppetiteStatus == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }

                    var existingValues = await mySqlDBContext.risk_admin_riskappetite
.Where(x => x.risk_admin_RiskAppetiteId == risk_Admin_Riskappetite.risk_admin_RiskAppetiteId)
.Select(x => new
{
    MinValue = x.risk_level_range_min,
    MaxValue = x.risk_level_range_max
})
.FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Admin_Riskappetite.risk_level_range_min ?? existingValues.MinValue;
                    var newMax = risk_Admin_Riskappetite.risk_level_range_max ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_riskappetite
                        .Where(d => d.risk_admin_RiskAppetiteId != risk_Admin_Riskappetite.risk_admin_RiskAppetiteId && d.risk_admin_RiskAppetiteStatus == "Active")
                        .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                        .ToListAsync();

                    // Check for overlapping ranges.
                    foreach (var range in existingRanges)
                    {
                        bool isOverlapping =
                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                        if (isOverlapping)
                        {
                            return BadRequest("Error: New rating values overlap with existing ranges.");
                        }
                    }
                    if (existingValues != null)
                    {
                        // Check validation for min and max values
                        if (risk_Admin_Riskappetite.risk_level_range_min == null &&
                            risk_Admin_Riskappetite.risk_level_range_max != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_Admin_Riskappetite.risk_level_range_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_Admin_Riskappetite.risk_level_range_min != null &&
                            risk_Admin_Riskappetite.risk_level_range_max == null)
                        {


                            if (risk_Admin_Riskappetite.risk_level_range_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }
                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(risk_Admin_Riskappetite);
                    this.mySqlDBContext.Entry(risk_Admin_Riskappetite).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Riskappetite);
                    Type type = typeof(risk_admin_riskappetite);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Riskappetite, null) == null || property.GetValue(risk_Admin_Riskappetite, null).Equals(0))
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
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/risk_admin_riskappetite/Deleterisk_admin_riskappetite")]
        [HttpDelete]
        public void Deleterisk_admin_riskappetite(int id)
        {
            var currentClass = new risk_admin_riskappetite { risk_admin_RiskAppetiteId = id };
            currentClass.risk_admin_RiskAppetiteStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_RiskAppetiteStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Risk risk risk Tolerance

        [Route("api/risk_admin_risktolerance/Getrisk_admin_risktolerance")]
        [HttpGet]
        public IEnumerable<risk_admin_risktolerance> Getrisk_admin_risktolerance()
        {
            return this.mySqlDBContext.risk_admin_risktolerance.Where(x => x.risk_admin_riskTolerancestatus == "Active")
                .OrderBy(r => r.risk_level_range_min)
                .ToList();
        }



        [Route("api/risk_admin_risktolerance/Insertrisk_admin_risktolerance")]
        [HttpPost]
        public async Task< IActionResult> Insertrisk_admin_risktolerance([FromBody] risk_admin_risktolerance risk_Admin_Risktolerance)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Admin_Risktolerance.risk_admin_riskToleranceName = risk_Admin_Risktolerance.risk_admin_riskToleranceName?.Trim();


                var existingDepartment = this.mySqlDBContext.risk_admin_risktolerance
                  .FirstOrDefault(d => d.risk_admin_riskToleranceName == risk_Admin_Risktolerance.risk_admin_riskToleranceName && d.risk_admin_riskTolerancestatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Tolerance name with the same name already exists.");
                }
                if (risk_Admin_Risktolerance.risk_level_range_min > risk_Admin_Risktolerance.risk_level_range_max)
                {
                    return BadRequest("Error: Risk Tolerance Minimum value is greater that  Risk Tolerance Maximum Value.");
                }
                

                //    if (risk_Admin_Risktolerance.risk_level_range_min != null && risk_Admin_Risktolerance.risk_level_range_max != null)
                //{
                //    if (!CheckValidation(risk_Admin_Risktolerance.risk_level_range_min, risk_Admin_Risktolerance.risk_level_range_max, risk_Admin_Risktolerance.array))
                //    {
                //        return BadRequest("Error: Risk Tolerance rating Range Not Valid.");
                //    }
                //}



                var existingDepartment2 = this.mySqlDBContext.risk_admin_risktolerance
               .FirstOrDefault(d => d.colour_reference == risk_Admin_Risktolerance.colour_reference && d.risk_admin_riskTolerancestatus == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk color refrence with the same color already exists.");
                }

                var newMin = risk_Admin_Risktolerance.risk_level_range_min ;
                var newMax = risk_Admin_Risktolerance.risk_level_range_max ;



                var existingRanges = await mySqlDBContext.risk_admin_risktolerance
                    .Where(d => d.risk_admin_riskToleranceid != risk_Admin_Risktolerance.risk_admin_riskToleranceid && d.risk_admin_riskTolerancestatus == "Active")
                    .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                    .ToListAsync();

                // Check for overlapping ranges.
                foreach (var range in existingRanges)
                {
                    bool isOverlapping =
                        (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                        (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                        (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                    if (isOverlapping)
                    {
                        return BadRequest("Error: New rating values overlap with existing ranges.");
                    }
                }
       
                    // Check validation for min and max values
                    if (risk_Admin_Risktolerance.risk_level_range_min != null &&
                          risk_Admin_Risktolerance.risk_level_range_max != null)
                    {

                        if (risk_Admin_Risktolerance.risk_level_range_min > risk_Admin_Risktolerance.risk_level_range_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                        }
                    }

                    // Proceed with the insertion
                    risk_Admin_Risktolerance.risk_admin_riskToleranceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Risktolerance.risk_admin_riskTolerancestatus = "Active";

                risk_Admin_Risktolerance.isImported = "No";
                this.mySqlDBContext.risk_admin_risktolerance.Add(risk_Admin_Risktolerance);
                this.mySqlDBContext.SaveChanges();

                return Ok("Record inserted successfully.");
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





        [Route("api/risk_admin_risktolerance/Updaterisk_admin_risktolerance")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_admin_risktolerance([FromBody] risk_admin_risktolerance risk_Admin_Risktolerance)
        {
            try
            {
                if (risk_Admin_Risktolerance.risk_admin_riskToleranceid == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Admin_Risktolerance.risk_admin_riskToleranceName = risk_Admin_Risktolerance.risk_admin_riskToleranceName?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_risktolerance
                        .FirstOrDefault(d => d.risk_admin_riskToleranceName == risk_Admin_Risktolerance.risk_admin_riskToleranceName
                                          && d.risk_admin_riskToleranceid != risk_Admin_Risktolerance.risk_admin_riskToleranceid
                                          && d.risk_admin_riskTolerancestatus == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk Tolerance name with the same name already exists");
                    }

                    var existingColorReference = this.mySqlDBContext.risk_admin_risktolerance
                        .FirstOrDefault(d => d.colour_reference == risk_Admin_Risktolerance.colour_reference
                                          && d.risk_admin_riskToleranceid != risk_Admin_Risktolerance.risk_admin_riskToleranceid
                                          && d.risk_admin_riskTolerancestatus == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }



                    var existingValues = await mySqlDBContext.risk_admin_risktolerance
.Where(x => x.risk_admin_riskToleranceid == risk_Admin_Risktolerance.risk_admin_riskToleranceid)
.Select(x => new
{
    MinValue = x.risk_level_range_min,
    MaxValue = x.risk_level_range_max
})
.FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Admin_Risktolerance.risk_level_range_min ?? existingValues.MinValue;
                    var newMax = risk_Admin_Risktolerance.risk_level_range_max ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_risktolerance
                        .Where(d => d.risk_admin_riskToleranceid != risk_Admin_Risktolerance.risk_admin_riskToleranceid && d.risk_admin_riskTolerancestatus == "Active")
                        .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                        .ToListAsync();

                    // Check for overlapping ranges.
                    foreach (var range in existingRanges)
                    {
                        bool isOverlapping =
                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                        if (isOverlapping)
                        {
                            return BadRequest("Error: New rating values overlap with existing ranges.");
                        }
                    }
                    if (existingValues != null)
                    {
                        // Check validation for min and max values
                        if (risk_Admin_Risktolerance.risk_level_range_min == null &&
                              risk_Admin_Risktolerance.risk_level_range_max != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_Admin_Risktolerance.risk_level_range_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_Admin_Risktolerance.risk_level_range_min != null &&
                            risk_Admin_Risktolerance.risk_level_range_max == null)
                        {


                            if (risk_Admin_Risktolerance.risk_level_range_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }

                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(risk_Admin_Risktolerance);
                    this.mySqlDBContext.Entry(risk_Admin_Risktolerance).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Risktolerance);
                    Type type = typeof(risk_admin_risktolerance);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Risktolerance, null) == null || property.GetValue(risk_Admin_Risktolerance, null).Equals(0))
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
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/risk_admin_risktolerance/Deleterisk_admin_risktolerance")]
        [HttpDelete]
        public void Deleterisk_admin_risktolerance(int id)
        {
            var currentClass = new risk_admin_risktolerance { risk_admin_riskToleranceid = id };
            currentClass.risk_admin_riskTolerancestatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_riskTolerancestatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Risk risk risk Inherent Risk Rating Level

        [Route("api/risk_admin_inherriskratinglevl/Getrisk_admin_inherriskratinglevl")]
        [HttpGet]
        public IEnumerable<risk_admin_inherriskratinglevl> Getrisk_admin_inherriskratinglevl()
        {
            return this.mySqlDBContext.risk_admin_inherriskratinglevl.Where(x => x.risk_admin_inherRiskRatingstatus == "Active")
                .OrderBy(r => r.risk_level_range_min)
                .ToList();
        }



        [Route("api/risk_admin_inherriskratinglevl/Insertrisk_admin_inherriskratinglevl")]
        [HttpPost]
        public async Task< IActionResult> Insertrisk_admin_inherriskratinglevl([FromBody] risk_admin_inherriskratinglevl risk_Admin_Inherriskratinglevl)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlname = risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlname?.Trim();


                var existingDepartment = this.mySqlDBContext.risk_admin_inherriskratinglevl
                  .FirstOrDefault(d => d.risk_admin_inherRiskRatingLevlname == risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlname && d.risk_admin_inherRiskRatingstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Inherent name with the same name already exists.");
                }
                if (risk_Admin_Inherriskratinglevl.risk_level_range_min > risk_Admin_Inherriskratinglevl.risk_level_range_max )
                {
                    return BadRequest("Error: Risk Inherret Minimum value is greater that  Risk Inherret Maximum Value.");
                }


                //    if (risk_Admin_Inherriskratinglevl.risk_level_range_min != null && risk_Admin_Inherriskratinglevl.risk_level_range_max != null)
                //{
                //    if (!CheckValidation(risk_Admin_Inherriskratinglevl.risk_level_range_min, risk_Admin_Inherriskratinglevl.risk_level_range_max, risk_Admin_Inherriskratinglevl.array))
                //    {
                //        return BadRequest("Error: Risk Inherent rating Range Not Valid.");
                //    }
                //}
                // Set min and max using either the provided values or the existing ones.
                var newMin = risk_Admin_Inherriskratinglevl.risk_level_range_min ;
                var newMax = risk_Admin_Inherriskratinglevl.risk_level_range_max ;



                var existingRanges = await mySqlDBContext.risk_admin_inherriskratinglevl
                    .Where(d => d.risk_admin_inherRiskRatingLevlid != risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlid && d.risk_admin_inherRiskRatingstatus == "Active")
                    .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                    .ToListAsync();

                // Check for overlapping ranges.
                foreach (var range in existingRanges)
                {
                    bool isOverlapping =
                        (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                        (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                        (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                    if (isOverlapping)
                    {
                        return BadRequest("Error: New rating values overlap with existing ranges.");
                    }
                }
     

                    if (risk_Admin_Inherriskratinglevl.risk_level_range_min != null &&
                          risk_Admin_Inherriskratinglevl.risk_level_range_max != null)
                    {

                        if (risk_Admin_Inherriskratinglevl.risk_level_range_min > risk_Admin_Inherriskratinglevl.risk_level_range_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                        }
                    }


                    var existingDepartment2 = this.mySqlDBContext.risk_admin_inherriskratinglevl
               .FirstOrDefault(d => d.colour_reference == risk_Admin_Inherriskratinglevl.colour_reference && d.risk_admin_inherRiskRatingstatus == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk color refrence with the same color already exists.");
                }



                // Proceed with the insertion
                risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingstatus = "Active";
                risk_Admin_Inherriskratinglevl.isImported = "No";

                this.mySqlDBContext.risk_admin_inherriskratinglevl.Add(risk_Admin_Inherriskratinglevl);
                this.mySqlDBContext.SaveChanges();

                return Ok("Record inserted successfully.");
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





        [Route("api/risk_admin_inherriskratinglevl/Updaterisk_admin_inherriskratinglevl")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_admin_inherriskratinglevl([FromBody] risk_admin_inherriskratinglevl risk_Admin_Inherriskratinglevl)
        {
            try
            {
                if (risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlid == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlname = risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlname?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_inherriskratinglevl
                        .FirstOrDefault(d => d.risk_admin_inherRiskRatingLevlname == risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlname
                                          && d.risk_admin_inherRiskRatingLevlid != risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlid
                                          && d.risk_admin_inherRiskRatingstatus == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk Inherent name with the same name already exists");
                    }


                    var existingColorReference = this.mySqlDBContext.risk_admin_inherriskratinglevl
                        .FirstOrDefault(d => d.colour_reference == risk_Admin_Inherriskratinglevl.colour_reference
                                          && d.risk_admin_inherRiskRatingLevlid != risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlid
                                          && d.risk_admin_inherRiskRatingstatus == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }




                    var existingValues = await mySqlDBContext.risk_admin_inherriskratinglevl
.Where(x => x.risk_admin_inherRiskRatingLevlid == risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlid)
.Select(x => new
{
    MinValue = x.risk_level_range_min,
    MaxValue = x.risk_level_range_max
})
.FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Admin_Inherriskratinglevl.risk_level_range_min ?? existingValues.MinValue;
                    var newMax = risk_Admin_Inherriskratinglevl.risk_level_range_max ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_inherriskratinglevl
                        .Where(d => d.risk_admin_inherRiskRatingLevlid != risk_Admin_Inherriskratinglevl.risk_admin_inherRiskRatingLevlid && d.risk_admin_inherRiskRatingstatus == "Active")
                        .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                        .ToListAsync();

                    // Check for overlapping ranges.
                    foreach (var range in existingRanges)
                    {
                        bool isOverlapping =
                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                        if (isOverlapping)
                        {
                            return BadRequest("Error: New rating values overlap with existing ranges.");
                        }
                    }
                    if (existingValues != null)
                    {
                        // Check validation for min and max values
                        if (risk_Admin_Inherriskratinglevl.risk_level_range_min == null &&
                              risk_Admin_Inherriskratinglevl.risk_level_range_max != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_Admin_Inherriskratinglevl.risk_level_range_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_Admin_Inherriskratinglevl.risk_level_range_min != null &&
                            risk_Admin_Inherriskratinglevl.risk_level_range_max == null)
                        {


                            if (risk_Admin_Inherriskratinglevl.risk_level_range_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }






                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(risk_Admin_Inherriskratinglevl);
                    this.mySqlDBContext.Entry(risk_Admin_Inherriskratinglevl).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Inherriskratinglevl);
                    Type type = typeof(risk_admin_inherriskratinglevl);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Inherriskratinglevl, null) == null || property.GetValue(risk_Admin_Inherriskratinglevl, null).Equals(0))
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
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }




        [Route("api/risk_admin_inherriskratinglevl/Deleterisk_admin_inherriskratinglevl")]
        [HttpDelete]
        public void Deleterisk_admin_inherriskratinglevl(int id)
        {
            var currentClass = new risk_admin_inherriskratinglevl { risk_admin_inherRiskRatingLevlid = id };
            currentClass.risk_admin_inherRiskRatingstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_inherRiskRatingstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        //Risk risk risk Intensity 

        [Route("api/risk_admin_riskintensity/Getrisk_admin_riskintensity")]
        [HttpGet]
        public IEnumerable<risk_admin_riskintensity> Getrisk_admin_riskintensity()
        {
            return this.mySqlDBContext.risk_admin_riskintensity.Where(x => x.risk_admin_riskIntensityStatus == "Active")
                .OrderBy(r => r.risk_level_range_min)
                .ToList();
        }



        [Route("api/risk_admin_riskintensity/Insertrisk_admin_riskintensity")]
        [HttpPost]
        //public async Task< IActionResult> Insertrisk_admin_riskintensity([FromBody] risk_admin_riskintensity risk_Admin_Riskintensity)
        //{
        //    try
        //    {
        //        // Trim the name to avoid any issues with leading/trailing spaces
        //        risk_Admin_Riskintensity.risk_admin_riskIntensityname = risk_Admin_Riskintensity.risk_admin_riskIntensityname?.Trim();


        //        var existingDepartment = this.mySqlDBContext.risk_admin_riskintensity
        //          .FirstOrDefault(d => d.risk_admin_riskIntensityname == risk_Admin_Riskintensity.risk_admin_riskIntensityname && d.risk_admin_riskIntensityStatus == "Active");

        //        //var existingDepartment = this.mySqlDBContext.risk_admin_riskintensity
        //        //  .FirstOrDefault(d => d.risk_admin_riskIntensityname == risk_Admin_Riskintensity.risk_admin_riskIntensityname
        //        //   && d.risk_admin_riskIntensityid != risk_Admin_Riskintensity.risk_admin_riskIntensityid && d.risk_admin_riskIntensityStatus == "Active");

        //        if (existingDepartment != null)
        //        {
        //            // Department with the same name already exists, return an error message
        //            return BadRequest("Error: Risk Intensity name with the same name already exists.");
        //        }
               
        //        if (risk_Admin_Riskintensity.risk_level_range_min > risk_Admin_Riskintensity.risk_level_range_max)
        //        {
        //            return BadRequest("Error: Risk Inherret Minimum value is greater that  Risk Inherret Maximum Value.");
        //        }
                

        //        //    if (risk_Admin_Riskintensity.risk_level_range_min != null && risk_Admin_Riskintensity.risk_level_range_max != null)
        //        //{
        //        //    if (!CheckValidation(risk_Admin_Riskintensity.risk_level_range_min, risk_Admin_Riskintensity.risk_level_range_max, risk_Admin_Riskintensity.array))
        //        //    {
        //        //        return BadRequest("Error: Risk Intensity rating Range Not Valid.");
        //        //    }
        //        //}



        //        var existingDepartment2 = this.mySqlDBContext.risk_admin_riskintensity
        //       .FirstOrDefault(d => d.colour_reference == risk_Admin_Riskintensity.colour_reference && d.risk_admin_riskIntensityStatus == "Active");

        //        if (existingDepartment2 != null)
        //        {
        //            // Department with the same name already exists, return an error message
        //            return BadRequest("Error: Risk color refrence with the same color already exists.");
        //        }
        //        var newMin = risk_Admin_Riskintensity.risk_level_range_min ;
        //        var newMax = risk_Admin_Riskintensity.risk_level_range_max ;



        //        var existingRanges = await mySqlDBContext.risk_admin_inherriskratinglevl
        //            .Where(d => d.risk_admin_inherRiskRatingLevlid != risk_Admin_Riskintensity.risk_admin_riskIntensityid && d.risk_admin_inherRiskRatingstatus == "Active")
        //            .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
        //            .ToListAsync();

        //        // Check for overlapping ranges.
        //        foreach (var range in existingRanges)
        //        {
        //            bool isOverlapping =
        //                (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
        //                (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
        //                (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

        //            if (isOverlapping)
        //            {
        //                return BadRequest("Error: New rating values overlap with existing ranges.");
        //            }
        //        }

        //            // Check validation for min and max values
        //            if (risk_Admin_Riskintensity.risk_level_range_min != null &&
        //                  risk_Admin_Riskintensity.risk_level_range_max != null )
        //            {

        //                if (risk_Admin_Riskintensity.risk_level_range_min > risk_Admin_Riskintensity.risk_level_range_max)
        //                {
        //                    return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
        //                }
        //            }


        //            // Proceed with the insertion
        //            risk_Admin_Riskintensity.risk_admin_riskIntensitydate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //        risk_Admin_Riskintensity.risk_admin_riskIntensityStatus = "Active";
        //        risk_Admin_Riskintensity.isImported = "No";

        //        this.mySqlDBContext.risk_admin_riskintensity.Add(risk_Admin_Riskintensity);
        //        this.mySqlDBContext.SaveChanges();

        //        return Ok("Record inserted successfully.");
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
        //        {
        //            // MySQL error number 1062 corresponds to a duplicate entry violation
        //            return BadRequest("Error: A record with the same values already exists.");
        //        }
        //        else
        //        {
        //            // Handle other database update exceptions
        //            return BadRequest($"Error: {ex.Message}");
        //        }
        //    }
        //}

        public async Task<IActionResult> Insertrisk_admin_riskintensity([FromBody] risk_admin_riskintensity risk_Admin_Riskintensity)
        {
            try
            {
                  risk_Admin_Riskintensity.risk_admin_riskIntensityname = risk_Admin_Riskintensity.risk_admin_riskIntensityname?.Trim();


                var existingDepartment = this.mySqlDBContext.risk_admin_riskintensity
                  .FirstOrDefault(d => d.risk_admin_riskIntensityname == risk_Admin_Riskintensity.risk_admin_riskIntensityname
                   && d.risk_admin_riskIntensityid != risk_Admin_Riskintensity.risk_admin_riskIntensityid && d.risk_admin_riskIntensityStatus == "Active");

                if (existingDepartment != null)
                {
                    return BadRequest("Error: Risk inherent rating name with the same name already exists.");
                }

                if (risk_Admin_Riskintensity.risk_level_range_min > risk_Admin_Riskintensity.risk_level_range_max)
                {
                    return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                }


                //if (!CheckValidation(riskInherentRatingLevel.Risk_inherent_rating_level_min, riskInherentRatingLevel.Risk_inherent_rating_level_max, riskInherentRatingLevel.array))
                //{
                //    return BadRequest("Error: Risk inherent rating Range Not Valid.");
                //}

                var colorRepeat = this.mySqlDBContext.risk_admin_riskintensity
                .FirstOrDefault(d => d.colour_reference == risk_Admin_Riskintensity.colour_reference
                 && d.risk_admin_riskIntensityid != risk_Admin_Riskintensity.risk_admin_riskIntensityid && d.risk_admin_riskIntensityStatus == "Active");

                if (colorRepeat != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk inherent rating colour reference with the same colour already exists.");
                }




                // Set min and max using either the provided values or the existing ones.
                var newMin = risk_Admin_Riskintensity.risk_level_range_min;
                var newMax = risk_Admin_Riskintensity.risk_level_range_max;



                var existingRanges = await mySqlDBContext.risk_admin_riskintensity
                    .Where(d => d.risk_admin_riskIntensityid != risk_Admin_Riskintensity.risk_admin_riskIntensityid && d.risk_admin_riskIntensityStatus == "Active")
                    .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                    .ToListAsync();

                // Check for overlapping ranges.
                foreach (var range in existingRanges)
                {
                    bool isOverlapping =
                        (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                        (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                        (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                    if (isOverlapping)
                    {
                        return BadRequest("Error: New rating values overlap with existing ranges.");
                    }
                }

                // Check validation for min and max values
                if (risk_Admin_Riskintensity.risk_level_range_min != null &&
                    risk_Admin_Riskintensity.risk_level_range_max != null)
                {

                    if (risk_Admin_Riskintensity.risk_level_range_min > risk_Admin_Riskintensity.risk_level_range_max)
                    {
                        return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                    }
                }

                risk_Admin_Riskintensity.risk_admin_riskIntensitydate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Admin_Riskintensity.risk_admin_riskIntensityStatus = "Active";

                this.mySqlDBContext.risk_admin_riskintensity.Add(risk_Admin_Riskintensity);
                this.mySqlDBContext.SaveChanges();

                return Ok("Record inserted successfully.");
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


        [Route("api/risk_admin_riskintensity/Updaterisk_admin_riskintensity")]
        [HttpPut]
        //        public async Task< IActionResult> Updaterisk_admin_riskintensity([FromBody] risk_admin_riskintensity risk_Admin_Inherriskratinglevl)
        //        {
        //            try
        //            {
        //                if (risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid == 0)
        //                {
        //                    // Logic for handling new risk likelihood (insertion) goes here
        //                    return Ok("Insertion successful");
        //                }
        //                else
        //                {
        //                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
        //                    risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityname = risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityname?.Trim();

        //                    // Check for duplicates in each field while excluding the current record
        //                    var existingName = this.mySqlDBContext.risk_admin_riskintensity
        //                        .FirstOrDefault(d => d.risk_admin_riskIntensityname == risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityname
        //                                          && d.risk_admin_riskIntensityid != risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid
        //                                          && d.risk_admin_riskIntensityStatus == "Active");

        //                    if (existingName != null)
        //                    {
        //                        return BadRequest("Error: Risk Intensity name with the same name already exists");
        //                    }


        //                    var existingColorReference = this.mySqlDBContext.risk_admin_riskintensity
        //                        .FirstOrDefault(d => d.colour_reference == risk_Admin_Inherriskratinglevl.colour_reference
        //                                          && d.risk_admin_riskIntensityid != risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid
        //                                          && d.risk_admin_riskIntensityStatus == "Active");

        //                    if (existingColorReference != null)
        //                    {
        //                        return BadRequest("Error: Risk color reference with the same color already exists.");
        //                    }



        //                    var existingValues = await mySqlDBContext.risk_admin_riskintensity
        //.Where(x => x.risk_admin_riskIntensityid == risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid)
        //.Select(x => new
        //{
        //MinValue = x.risk_level_range_min,
        //MaxValue = x.risk_level_range_max
        //})
        //.FirstOrDefaultAsync();


        //                    // Set min and max using either the provided values or the existing ones.
        //                    var newMin = risk_Admin_Inherriskratinglevl.risk_level_range_min ?? existingValues.MinValue;
        //                    var newMax = risk_Admin_Inherriskratinglevl.risk_level_range_max ?? existingValues.MaxValue;



        //                    var existingRanges = await mySqlDBContext.risk_admin_inherriskratinglevl
        //                        .Where(d => d.risk_admin_inherRiskRatingLevlid != risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid && d.risk_admin_inherRiskRatingstatus == "Active")
        //                        .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
        //                        .ToListAsync();

        //                    // Check for overlapping ranges.
        //                    foreach (var range in existingRanges)
        //                    {
        //                        bool isOverlapping =
        //                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
        //                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
        //                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

        //                        if (isOverlapping)
        //                        {
        //                            return BadRequest("Error: New rating values overlap with existing ranges.");
        //                        }
        //                    }
        //                    if (existingValues != null)
        //                    {
        //                        // Check validation for min and max values
        //                        if (risk_Admin_Inherriskratinglevl.risk_level_range_min == null &&
        //                              risk_Admin_Inherriskratinglevl.risk_level_range_max != null &&
        //                            existingValues.MinValue != 0)
        //                        {

        //                            if (existingValues.MinValue > risk_Admin_Inherriskratinglevl.risk_level_range_max)
        //                            {
        //                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
        //                            }
        //                        }

        //                        if (risk_Admin_Inherriskratinglevl.risk_level_range_min != null &&
        //                            risk_Admin_Inherriskratinglevl.risk_level_range_max == null)
        //                        {


        //                            if (risk_Admin_Inherriskratinglevl.risk_level_range_min > existingValues.MaxValue)
        //                            {
        //                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
        //                            }



        //                        }
        //                    }





        //                    // If no duplicates are found, proceed with the update
        //                    this.mySqlDBContext.Attach(risk_Admin_Inherriskratinglevl);
        //                    this.mySqlDBContext.Entry(risk_Admin_Inherriskratinglevl).State = EntityState.Modified;

        //                    var entry = this.mySqlDBContext.Entry(risk_Admin_Inherriskratinglevl);
        //                    Type type = typeof(risk_admin_riskintensity);
        //                    PropertyInfo[] properties = type.GetProperties();

        //                    foreach (PropertyInfo property in properties)
        //                    {
        //                        if (property.GetValue(risk_Admin_Inherriskratinglevl, null) == null || property.GetValue(risk_Admin_Inherriskratinglevl, null).Equals(0))
        //                        {
        //                            entry.Property(property.Name).IsModified = false;
        //                        }
        //                    }

        //                    this.mySqlDBContext.SaveChanges();
        //                    return Ok("Update successful");
        //                }
        //            }
        //            catch (DbUpdateException ex)
        //            {
        //                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
        //                {
        //                    return BadRequest("Error: A record with the same values already exists.");
        //                }
        //                else
        //                {
        //                    return BadRequest($"Error: {ex.Message}");
        //                }
        //            }
        //        }

        public async Task<IActionResult> Updaterisk_admin_riskintensity([FromBody] risk_admin_riskintensity risk_Admin_Inherriskratinglevl)
        {
            try
            {
                if (risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid == 0)
                {
                     return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityname = risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityname?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_riskintensity
                        .FirstOrDefault(d => d.risk_admin_riskIntensityname == risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityname
                                          && d.risk_admin_riskIntensityid != risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid
                                          && d.risk_admin_riskIntensityStatus == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk inherent name with the same name already exists");
                    }
                    if (risk_Admin_Inherriskratinglevl.risk_level_range_min > risk_Admin_Inherriskratinglevl.risk_level_range_max)
                    {
                        return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                    }



                    var existingColorReference = this.mySqlDBContext.risk_admin_riskintensity
                        .FirstOrDefault(d => d.colour_reference == risk_Admin_Inherriskratinglevl.colour_reference
                                          && d.risk_admin_riskIntensityid != risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid
                                          && d.risk_admin_riskIntensityStatus == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }

                    var existingValues = await mySqlDBContext.risk_admin_riskintensity
     .Where(x => x.risk_admin_riskIntensityid == risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid)
     .Select(x => new
     {
         MinValue = x.risk_level_range_min,
         MaxValue = x.risk_level_range_max
     })
     .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Admin_Inherriskratinglevl.risk_level_range_min ?? existingValues.MinValue;
                    var newMax = risk_Admin_Inherriskratinglevl.risk_level_range_max ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_riskintensity
                        .Where(d => d.risk_admin_riskIntensityid != risk_Admin_Inherriskratinglevl.risk_admin_riskIntensityid && d.risk_admin_riskIntensityStatus == "Active")
                        .Select(d => new { MinValue = d.risk_level_range_min, MaxValue = d.risk_level_range_max })
                        .ToListAsync();

                    // Check for overlapping ranges.
                    foreach (var range in existingRanges)
                    {
                        bool isOverlapping =
                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                        if (isOverlapping)
                        {
                            return BadRequest("Error: New rating values overlap with existing ranges.");
                        }
                    }
                    if (existingValues != null)
                    {
                        // Check validation for min and max values
                        if (risk_Admin_Inherriskratinglevl.risk_level_range_min == null &&
                            risk_Admin_Inherriskratinglevl.risk_level_range_max != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_Admin_Inherriskratinglevl.risk_level_range_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_Admin_Inherriskratinglevl.risk_level_range_min != null &&
                            risk_Admin_Inherriskratinglevl.risk_level_range_max == null)
                        {


                            if (risk_Admin_Inherriskratinglevl.risk_level_range_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }

                    this.mySqlDBContext.Attach(risk_Admin_Inherriskratinglevl);
                    this.mySqlDBContext.Entry(risk_Admin_Inherriskratinglevl).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Admin_Inherriskratinglevl);
                    Type type = typeof(risk_admin_riskintensity);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Admin_Inherriskratinglevl, null) == null || property.GetValue(risk_Admin_Inherriskratinglevl, null).Equals(0))
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
                    return BadRequest("Error: A record with the same values already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        [Route("api/risk_admin_riskintensity/Deleterisk_admin_riskintensity")]
        [HttpDelete]
        public void Deleterisk_admin_riskintensity(int id)
        {
            var currentClass = new risk_admin_riskintensity { risk_admin_riskIntensityid = id };
            currentClass.risk_admin_riskIntensityStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_riskIntensityStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Getting Nature of Control Performance Details

        [Route("api/NatureOfControlPerformance/Getrisk_admin_naturecontrperf")]
        [HttpGet]

        public IEnumerable<risk_admin_naturecontrperf> GetNatureOfControlPerformanceModelDetails()
        {
            return this.mySqlDBContext.risk_admin_naturecontrperf.Where(x => x.risk_admin_NatureContrPerfStatus == "Active").ToList();
        }


        [Route("api/NatureOfControlPerformance/Insertrisk_admin_naturecontrperf")]
        [HttpPost]
        public IActionResult InsertNatureOfControlPerformanceModelDetails([FromBody] risk_admin_naturecontrperf riskControlMatrixAttModels)
        {
            try
            {
                riskControlMatrixAttModels.risk_admin_NatureContrPerfname = riskControlMatrixAttModels.risk_admin_NatureContrPerfname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_naturecontrperf
                    .FirstOrDefault(d => d.risk_admin_NatureContrPerfname == riskControlMatrixAttModels.risk_admin_NatureContrPerfname && d.risk_admin_NatureContrPerfStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Nature Control Name with the same name already exists.");
                }

                var existingDepartment1 = this.mySqlDBContext.risk_admin_naturecontrperf
                .FirstOrDefault(d => d.risk_natureOf_control_perf_rating == riskControlMatrixAttModels.risk_natureOf_control_perf_rating && d.risk_admin_NatureContrPerfStatus == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }


                var maxLawTypeId = this.mySqlDBContext.risk_admin_naturecontrperf
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_NatureContrPerfid) ?? 0; // If no records are found, default to 0
                                          // Increment the law_type_id by 1
                riskControlMatrixAttModels.risk_admin_NatureContrPerfid = maxLawTypeId + 1;

                // Proceed with the insertion
                var riskmatrixModel = this.mySqlDBContext.risk_admin_naturecontrperf;
                riskmatrixModel.Add(riskControlMatrixAttModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlMatrixAttModels.risk_admin_NatureContrPerfdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlMatrixAttModels.risk_admin_NatureContrPerfStatus = "Active";
                riskControlMatrixAttModels.isImported = "No";

                this.mySqlDBContext.SaveChanges();
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


        [Route("api/NatureOfControlPerformance/Updaterisk_admin_naturecontrperf")]
        [HttpPut]
        public IActionResult UpdateNatureOfControlPerformanceModelDetails([FromBody] risk_admin_naturecontrperf riskControlMatrixAttModel)
        {
            try
            {
                if (riskControlMatrixAttModel.risk_admin_NatureContrPerfid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskControlMatrixAttModel.risk_admin_NatureContrPerfname = riskControlMatrixAttModel.risk_admin_NatureContrPerfname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_naturecontrperf
                      .FirstOrDefault(d => d.risk_admin_NatureContrPerfname == riskControlMatrixAttModel.risk_admin_NatureContrPerfname && d.risk_admin_NatureContrPerfid != riskControlMatrixAttModel.risk_admin_NatureContrPerfid && d.risk_admin_NatureContrPerfStatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Nature Control Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(riskControlMatrixAttModel);
                    this.mySqlDBContext.Entry(riskControlMatrixAttModel).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskControlMatrixAttModel);

                    Type type = typeof(risk_admin_naturecontrperf);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskControlMatrixAttModel, null) == null || property.GetValue(riskControlMatrixAttModel, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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


        [Route("api/NatureOfControlPerformance/Deleterisk_admin_naturecontrperf")]
        [HttpDelete]
        public void DeleteNatureOfControlPerformanceModelDetails(int id)
        {
            var currentClass = new risk_admin_naturecontrperf { risk_admin_NatureContrPerfid = id };
            currentClass.risk_admin_NatureContrPerfStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_NatureContrPerfStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Getting Control level Details

        [Route("api/ControlLevel/Getrisk_admin_contrlevel")]
        [HttpGet]

        public IEnumerable<risk_admin_contrlevel> GetControlLevelModelDetails()
        {
            return this.mySqlDBContext.risk_admin_contrlevel.Where(x => x.risk_admin_contrLevelstatus == "Active").ToList();
        }




        [Route("api/ControlLevel/Insertrisk_admin_contrlevel")]
        [HttpPost]
        public IActionResult InsertControlLevelModelDetails([FromBody] risk_admin_contrlevel riskControlLevel)
        {
            try
            {
                riskControlLevel.risk_admin_contrLevelName = riskControlLevel.risk_admin_contrLevelName?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_contrlevel
                    .FirstOrDefault(d => d.risk_admin_contrLevelName == riskControlLevel.risk_admin_contrLevelName && d.risk_admin_contrLevelstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Level Name with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_contrlevel
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_contrLevelid) ?? 0; // If no records are found, default to 0
                                                 // Increment the law_type_id by 1
                riskControlLevel.risk_admin_contrLevelid = maxLawTypeId + 1;

                // Proceed with the insertion
                var riskmatrixModel = this.mySqlDBContext.risk_admin_contrlevel;
                riskmatrixModel.Add(riskControlLevel);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlLevel.risk_admin_contrLeveldate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControlLevel.risk_admin_contrLevelstatus = "Active";
                riskControlLevel.isImported = "No";

                this.mySqlDBContext.SaveChanges();
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


        [Route("api/ControlLevel/Updaterisk_admin_contrlevel")]
        [HttpPut]
        public IActionResult UpdateControlLevelModelDetails([FromBody] risk_admin_contrlevel riskControlLevel)
        {
            try
            {
                if (riskControlLevel.risk_admin_contrLevelid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskControlLevel.risk_admin_contrLevelName = riskControlLevel.risk_admin_contrLevelName?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_contrlevel
                      .FirstOrDefault(d => d.risk_admin_contrLevelName == riskControlLevel.risk_admin_contrLevelName && d.risk_admin_contrLevelstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Control Level Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(riskControlLevel);
                    this.mySqlDBContext.Entry(riskControlLevel).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskControlLevel);

                    Type type = typeof(risk_admin_contrlevel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskControlLevel, null) == null || property.GetValue(riskControlLevel, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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



        [Route("api/ControlLevel/Deleterisk_admin_contrlevel")]
        [HttpDelete]
        public void DeleteControlLevelModelDetails(int id)
        {
            var currentClass = new risk_admin_contrlevel { risk_admin_contrLevelid = id };
            currentClass.risk_admin_contrLevelstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_contrLevelstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Getting Control Dependencies Details

        [Route("api/Controldependency/Getrisk_admin_contrdepen")]
        [HttpGet]

        public IEnumerable<risk_admin_contrdepen> GetControldependencyModelDetails()
        {
            return this.mySqlDBContext.risk_admin_contrdepen.Where(x => x.risk_admin_contrDepenstatus == "Active").ToList();
        }




        [Route("api/Controldependency/Insertrisk_admin_contrdepen")]
        [HttpPost]
        public IActionResult InsertControldependencyModelDetails([FromBody] risk_admin_contrdepen riskControldependency)
        {
            try
            {
                riskControldependency.risk_admin_contrDepenname = riskControldependency.risk_admin_contrDepenname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_contrdepen
                    .FirstOrDefault(d => d.risk_admin_contrDepenname == riskControldependency.risk_admin_contrDepenname && d.risk_admin_contrDepenstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Dependency Name with the same name already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_contrdepen
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_contrDepenid) ?? 0; // If no records are found, default to 0
                                                 // Increment the law_type_id by 1
                riskControldependency.risk_admin_contrDepenid = maxLawTypeId + 1;


                // Proceed with the insertion
                var riskmatrixModel = this.mySqlDBContext.risk_admin_contrdepen;
                riskmatrixModel.Add(riskControldependency);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControldependency.risk_admin_contrDependate = dt1;
                riskControldependency.isImported = "No";
                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskControldependency.risk_admin_contrDepenstatus = "Active";

                this.mySqlDBContext.SaveChanges();
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


        [Route("api/Controldependency/Updaterisk_admin_contrdepen")]
        [HttpPut]
        public IActionResult UpdateControldependencyModelDetails([FromBody] risk_admin_contrdepen riskControldependency)
        {
            try
            {
                if (riskControldependency.risk_admin_contrDepenid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskControldependency.risk_admin_contrDepenname = riskControldependency.risk_admin_contrDepenname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_contrdepen
                      .FirstOrDefault(d => d.risk_admin_contrDepenname == riskControldependency.risk_admin_contrDepenname && d.risk_admin_contrDepenstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Control Dependency Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(riskControldependency);
                    this.mySqlDBContext.Entry(riskControldependency).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskControldependency);

                    Type type = typeof(risk_admin_contrdepen);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskControldependency, null) == null || property.GetValue(riskControldependency, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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



        [Route("api/Controldependency/Deleterisk_admin_contrdepen")]
        [HttpDelete]
        public void DeleteControldependencyModelDetails(int id)
        {
            var currentClass = new risk_admin_contrdepen { risk_admin_contrDepenid = id };
            currentClass.risk_admin_contrDepenstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_contrDepenstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Risk  Nature of control Occurrence 

        [Route("api/NatureOfControl/Getrisk_admin_natucontroccur")]
        [HttpGet]
        public IEnumerable<risk_admin_natucontroccur> GetNatureOfControl()
        {
            return this.mySqlDBContext.risk_admin_natucontroccur.Where(x => x.risk_admin_natucontroccurstatus == "Active")
                .OrderBy(r => r.risk_natureof_cont_occu_rating)
                .ToList();
        }



        [Route("api/NatureOfControl/Insertrisk_admin_natucontroccur")]
        [HttpPost]
        public IActionResult InsertNatureOfControl([FromBody] risk_admin_natucontroccur risk_Natureof_Cont_Occu)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Natureof_Cont_Occu.risk_admin_natucontroccurName = risk_Natureof_Cont_Occu.risk_admin_natucontroccurName?.Trim();


                var existingDepartment = this.mySqlDBContext.risk_admin_natucontroccur
                  .FirstOrDefault(d => d.risk_admin_natucontroccurName == risk_Natureof_Cont_Occu.risk_admin_natucontroccurName && d.risk_admin_natucontroccurstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk intensity name with the same name already exists.");
                }

                var existingDepartment1 = this.mySqlDBContext.risk_admin_natucontroccur
                 .FirstOrDefault(d => d.risk_natureof_cont_occu_rating == risk_Natureof_Cont_Occu.risk_natureof_cont_occu_rating && d.risk_admin_natucontroccurstatus == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_natucontroccur
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_natucontroccurid) ?? 0; // If no records are found, default to 0
                                                      // Increment the law_type_id by 1
                risk_Natureof_Cont_Occu.risk_admin_natucontroccurid = maxLawTypeId + 1;

                // Proceed with the insertion
                risk_Natureof_Cont_Occu.risk_admin_natucontroccurdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Natureof_Cont_Occu.risk_admin_natucontroccurstatus = "Active";
                risk_Natureof_Cont_Occu.isImported = "No";

                this.mySqlDBContext.risk_admin_natucontroccur.Add(risk_Natureof_Cont_Occu);
                this.mySqlDBContext.SaveChanges();

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





        [Route("api/NatureOfControl/Updaterisk_admin_natucontroccur")]
        [HttpPut]
        public IActionResult UpdateNatureOfControl([FromBody] risk_admin_natucontroccur risk_Natureof_Cont_Occu)
        {
            try
            {
                if (risk_Natureof_Cont_Occu.risk_admin_natucontroccurid == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Natureof_Cont_Occu.risk_admin_natucontroccurName = risk_Natureof_Cont_Occu.risk_admin_natucontroccurName?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_admin_natucontroccur
                        .FirstOrDefault(d => d.risk_admin_natucontroccurName == risk_Natureof_Cont_Occu.risk_admin_natucontroccurName
                                          && d.risk_admin_natucontroccurid != risk_Natureof_Cont_Occu.risk_admin_natucontroccurid
                                          && d.risk_admin_natucontroccurstatus == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk intensity name with the same name already exists");
                    }

                    var existingValue = this.mySqlDBContext.risk_admin_natucontroccur
                        .FirstOrDefault(d => d.risk_natureof_cont_occu_rating == risk_Natureof_Cont_Occu.risk_natureof_cont_occu_rating
                                          && d.risk_admin_natucontroccurid != risk_Natureof_Cont_Occu.risk_admin_natucontroccurid
                                          && d.risk_admin_natucontroccurstatus == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk factor value with the same value already exists.");
                    }



                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(risk_Natureof_Cont_Occu);
                    this.mySqlDBContext.Entry(risk_Natureof_Cont_Occu).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Natureof_Cont_Occu);
                    Type type = typeof(risk_admin_natucontroccur);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Natureof_Cont_Occu, null) == null || property.GetValue(risk_Natureof_Cont_Occu, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();

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




        [Route("api/NatureOfControl/Deleterisk_admin_natucontroccur")]
        [HttpDelete]
        public void DeleteNatureOfControl(int id)
        {
            var currentClass = new risk_admin_natucontroccur { risk_admin_natucontroccurid = id };
            currentClass.risk_admin_natucontroccurstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_natucontroccurstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Risk  Frequency of Control Applied

        [Route("api/FrequencyOfControlApplied/Getrisk_admin_frqcontrapplid")]
        [HttpGet]
        public IEnumerable<risk_admin_frqcontrapplid> GetFrequencyOfControlApplied()
        {
            return this.mySqlDBContext.risk_Admin_Frqcontrapplids.Where(x => x.risk_admin_frqcontrapplidStatus == "Active")
                .OrderBy(r => r.risk_frqof_contr_appl_rating)
                .ToList();
        }



        [Route("api/FrequencyOfControlApplied/Insertrisk_admin_frqcontrapplid")]
        [HttpPost]
        public IActionResult InsertFrequencyOfControlApplied([FromBody] risk_admin_frqcontrapplid risk_Frqof_Contr_Appl)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidname = risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidname?.Trim();


                var existingDepartment = this.mySqlDBContext.risk_Admin_Frqcontrapplids
                  .FirstOrDefault(d => d.risk_admin_frqcontrapplidname == risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidname && d.risk_admin_frqcontrapplidStatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Frequency name with the same name already exists.");
                }

                var existingDepartment1 = this.mySqlDBContext.risk_Admin_Frqcontrapplids
                 .FirstOrDefault(d => d.risk_frqof_contr_appl_rating == risk_Frqof_Contr_Appl.risk_frqof_contr_appl_rating && d.risk_admin_frqcontrapplidStatus == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk factor value with the same value already exists.");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_Admin_Frqcontrapplids
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_frqcontrapplidid) ?? 0; // If no records are found, default to 0
                                                     // Increment the law_type_id by 1
                risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidid = maxLawTypeId + 1;


                // Proceed with the insertion
                risk_Frqof_Contr_Appl.risk_admin_frqcontrappliddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidStatus = "Active";
                risk_Frqof_Contr_Appl.isImported = "No";

                this.mySqlDBContext.risk_Admin_Frqcontrapplids.Add(risk_Frqof_Contr_Appl);
                this.mySqlDBContext.SaveChanges();

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





        [Route("api/FrequencyOfControlApplied/Updaterisk_admin_frqcontrapplid")]
        [HttpPut]
        public IActionResult UpdateFrequencyOfControlApplied([FromBody] risk_admin_frqcontrapplid risk_Frqof_Contr_Appl)
        {
            try
            {
                if (risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidid == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidname = risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidname?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.mySqlDBContext.risk_Admin_Frqcontrapplids
                        .FirstOrDefault(d => d.risk_admin_frqcontrapplidname == risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidname
                                          && d.risk_admin_frqcontrapplidid != risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidid
                                          && d.risk_admin_frqcontrapplidStatus == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk frquency name with the same name already exists");
                    }

                    var existingValue = this.mySqlDBContext.risk_Admin_Frqcontrapplids
                        .FirstOrDefault(d => d.risk_frqof_contr_appl_rating == risk_Frqof_Contr_Appl.risk_frqof_contr_appl_rating
                                          && d.risk_admin_frqcontrapplidid != risk_Frqof_Contr_Appl.risk_admin_frqcontrapplidid
                                          && d.risk_admin_frqcontrapplidStatus == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk factor value with the same value already exists.");
                    }



                    // If no duplicates are found, proceed with the update
                    this.mySqlDBContext.Attach(risk_Frqof_Contr_Appl);
                    this.mySqlDBContext.Entry(risk_Frqof_Contr_Appl).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Frqof_Contr_Appl);
                    Type type = typeof(risk_admin_natucontroccur);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Frqof_Contr_Appl, null) == null || property.GetValue(risk_Frqof_Contr_Appl, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();

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




        [Route("api/FrequencyOfControlApplied/Deleterisk_admin_frqcontrapplid")]
        [HttpDelete]
        public void DeleteFrequencyOfControlApplied(int id)
        {
            var currentClass = new risk_admin_frqcontrapplid { risk_admin_frqcontrapplidid = id };
            currentClass.risk_admin_frqcontrapplidStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_frqcontrapplidStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //BP Maturity Rating Scale Indicators

        [Route("api/RiskSupAdminController/Getrisk_admin_bpmatratscaleindicator")]
        [HttpGet]
        public IEnumerable<risk_admin_bpmatratscaleindicator> Getbpmaturityratingscaleindicators()
        {
            return this.mySqlDBContext.risk_admin_bpmatratscaleindicator.Where(x => x.risk_admin_bpmatratscaleindicatorstatus == "Active")
                .OrderBy(r => r.BPMaturityRatingScaleIndicators_rating_min)
                .ToList();
        }


        [Route("api/RiskSupAdminController/Insertrisk_admin_bpmatratscaleindicator")]
        [HttpPost]
        public async Task< IActionResult> Insertbpmaturityratingscaleindicators([FromBody] risk_admin_bpmatratscaleindicator bpmaturityratingscaleindicatorss)

        {
            try
            {
                bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorname = bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_bpmatratscaleindicator
                    .FirstOrDefault(d => d.risk_admin_bpmatratscaleindicatorname == bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorname && d.risk_admin_bpmatratscaleindicatorstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: BP Maturity Rating Scale Indicators  with the same name already exists.");
                }


                if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min > bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max)
                {
                    return BadRequest("Error: Risk Bp Maturity Minimum value is greater that  Risk  Maximum Value.");
                }
                //if (!CheckValidation(bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min, bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max, bpmaturityratingscaleindicatorss.array))
                //{
                //    return BadRequest("Error: BP Maturity Rating Scale Indicators Range Not Valid.");
                //}
                var newMin = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min ;
                var newMax = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max ;

                // Ensure min is not greater than max.



                var existingRanges = await mySqlDBContext.risk_admin_bpmatratscaleindicator
                    .Where(d => d.risk_admin_bpmatratscaleindicatorid != bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorid && d.risk_admin_bpmatratscaleindicatorstatus == "Active")
                    .Select(d => new { MinValue = d.BPMaturityRatingScaleIndicators_rating_min, MaxValue = d.BPMaturityRatingScaleIndicators_rating_max })
                    .ToListAsync();

                // Check for overlapping ranges.
                foreach (var range in existingRanges)
                {
                    bool isOverlapping =
                        (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                        (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                        (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                    if (isOverlapping)
                    {
                        return BadRequest("Error: New rating values overlap with existing ranges.");
                    }
                }
           
                    // Check validation for min and max values
                    if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min != null &&
                        bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max != null)
                    {
                        if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min > bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                        }
                    }




                    var maxLawTypeId = this.mySqlDBContext.risk_admin_bpmatratscaleindicator
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_bpmatratscaleindicatorid) ?? 0; // If no records are found, default to 0
                                                     // Increment the law_type_id by 1
                bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorid = maxLawTypeId + 1;
                // Proceed with the insertion
                var bpmaturityratingscaleindicators = this.mySqlDBContext.risk_admin_bpmatratscaleindicator;
                bpmaturityratingscaleindicators.Add(bpmaturityratingscaleindicatorss);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatordate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorstatus = "Active";
                bpmaturityratingscaleindicatorss.isImported = "No";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: BP Maturity Rating Scale Indicators  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update BP Maturity Rating Scale Indicators

        [Route("api/RiskSupAdminController/Updaterisk_admin_bpmatratscaleindicator")]
        [HttpPut]
        public async Task<IActionResult> Updatebpmaturityratingscaleindicators([FromBody] risk_admin_bpmatratscaleindicator bpmaturityratingscaleindicatorss)
        {
            try
            {
                if (bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorname = bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_bpmatratscaleindicator
                     .FirstOrDefault(d => d.risk_admin_bpmatratscaleindicatorname == bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorname && d.risk_admin_bpmatratscaleindicatorid != bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorid && d.risk_admin_bpmatratscaleindicatorstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: BP Maturity Rating Scale Indicators with the same name already exists.");
                    }




                    var existingValues = await mySqlDBContext.risk_admin_bpmatratscaleindicator
           .Where(x => x.risk_admin_bpmatratscaleindicatorid == bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorid)
           .Select(x => new
           {
               MinValue = x.BPMaturityRatingScaleIndicators_rating_min,
               MaxValue = x.BPMaturityRatingScaleIndicators_rating_max
           })
           .FirstOrDefaultAsync();
             

                    // Set min and max using either the provided values or the existing ones.
                    var newMin = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min ?? existingValues.MinValue;
                    var newMax = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max ?? existingValues.MaxValue;

                    // Ensure min is not greater than max.



                    var existingRanges = await mySqlDBContext.risk_admin_bpmatratscaleindicator
                        .Where(d => d.risk_admin_bpmatratscaleindicatorid != bpmaturityratingscaleindicatorss.risk_admin_bpmatratscaleindicatorid && d.risk_admin_bpmatratscaleindicatorstatus == "Active")
                        .Select(d => new { MinValue = d.BPMaturityRatingScaleIndicators_rating_min, MaxValue = d.BPMaturityRatingScaleIndicators_rating_max })
                        .ToListAsync();

                    // Check for overlapping ranges.
                    foreach (var range in existingRanges)
                    {
                        bool isOverlapping =
                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                        if (isOverlapping)
                        {
                            return BadRequest("Error: New rating values overlap with existing ranges.");
                        }
                    }
                    if (existingValues != null)
                    {
                        // Check validation for min and max values
                        if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min == null &&
                            bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max != null &&
                            existingValues.MinValue != 0)
                        {
                            //if (!CheckValidation(existingValues.MinValue, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                            //{
                            //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                            //}
                            if (existingValues.MinValue > bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }
                        }

                        if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min != null &&
                            bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max == null)
                        {


                            if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }

                            //if (!CheckValidation(RiskSupAdminModels.rating_level_min, existingValues.MaxValue, RiskSupAdminModels.array))
                            //{
                            //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                            //}

                        }
                    }
                    // Existing department, update logic
                    this.mySqlDBContext.Attach(bpmaturityratingscaleindicatorss);
                    this.mySqlDBContext.Entry(bpmaturityratingscaleindicatorss).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(bpmaturityratingscaleindicatorss);

                    Type type = typeof(risk_admin_bpmatratscaleindicator);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(bpmaturityratingscaleindicatorss, null) == null || property.GetValue(bpmaturityratingscaleindicatorss, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: BP Maturity Rating Scale Indicators with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete BP Maturity Rating Scale Indicators

        [Route("api/RiskSupAdminController/Deleterisk_admin_bpmatratscaleindicator")]
        [HttpDelete]
        public void Deletebpmaturityratingscaleindicators(int id)
        {
            var currentClass = new risk_admin_bpmatratscaleindicator { risk_admin_bpmatratscaleindicatorid = id };
            currentClass.risk_admin_bpmatratscaleindicatorstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_bpmatratscaleindicatorstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Control Assessment Test Attributes

        [Route("api/RiskSupAdminController/Getrisk_admin_contrasstestatt")]
        [HttpGet]
        public IEnumerable<risk_admin_contrasstestatt> Getcontrolassesstestattributes()
        {
            return this.mySqlDBContext.risk_admin_contrasstestatt.Where(x => x.risk_admin_contrAssTestAttstatus == "Active").ToList();
        }

        //Control Assessment Test Attributes Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_contrasstestatt")]
        [HttpPost]
        public IActionResult Insertcontrolassesstestattributes([FromBody] risk_admin_contrasstestatt controlassesstestattributess)

        {
            try
            {
                controlassesstestattributess.risk_admin_contrAssTestAttname = controlassesstestattributess.risk_admin_contrAssTestAttname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_contrasstestatt
                    .FirstOrDefault(d => d.risk_admin_contrAssTestAttname == controlassesstestattributess.risk_admin_contrAssTestAttname && d.risk_admin_contrAssTestAttstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Effectiveness Rating  with the same name already exists.");
                }


                var maxLawTypeId = this.mySqlDBContext.risk_admin_contrasstestatt
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_contrAssTestAttid) ?? 0; // If no records are found, default to 0
                                                             // Increment the law_type_id by 1
                controlassesstestattributess.risk_admin_contrAssTestAttid = maxLawTypeId + 1;
                // Proceed with the insertion
                var controlassesstestattributes = this.mySqlDBContext.risk_admin_contrasstestatt;
                controlassesstestattributes.Add(controlassesstestattributess);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                controlassesstestattributess.risk_admin_contrAssTestAttdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                controlassesstestattributess.risk_admin_contrAssTestAttstatus = "Active";
                controlassesstestattributess.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Control Effectiveness Rating  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update Control Assessment Test Attributes

        [Route("api/RiskSupAdminController/Updaterisk_admin_contrasstestatt")]
        [HttpPut]
        public IActionResult Updatecontrolassesstestattributes([FromBody] risk_admin_contrasstestatt controlassesstestattributess)
        {
            try
            {
                if (controlassesstestattributess.risk_admin_contrAssTestAttid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    controlassesstestattributess.risk_admin_contrAssTestAttname = controlassesstestattributess.risk_admin_contrAssTestAttname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_contrasstestatt
                     .FirstOrDefault(d => d.risk_admin_contrAssTestAttname == controlassesstestattributess.risk_admin_contrAssTestAttname && d.risk_admin_contrAssTestAttid != controlassesstestattributess.risk_admin_contrAssTestAttid && d.risk_admin_contrAssTestAttstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: AuthorityName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(controlassesstestattributess);
                    this.mySqlDBContext.Entry(controlassesstestattributess).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(controlassesstestattributess);

                    Type type = typeof(risk_admin_contrasstestatt);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(controlassesstestattributess, null) == null || property.GetValue(controlassesstestattributess, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: AuthorityName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete Control Assessment Test Attributes

        [Route("api/RiskSupAdminController/Deleterisk_admin_contrasstestatt")]
        [HttpDelete]
        public void Deletecontrolassesstestattributes(int id)
        {
            var currentClass = new risk_admin_contrasstestatt { risk_admin_contrAssTestAttid = id };
            currentClass.risk_admin_contrAssTestAttstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_contrAssTestAttstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Initial Assessment Impact Factor 

        [Route("api/initialAssessmentImpact/Getrisk_admin_iniassimpfact")]
        [HttpGet]

        public IEnumerable<risk_admin_iniassimpfact> GetinitialAssessmentImpactModelDetails()
        {
            return this.mySqlDBContext.risk_admin_iniassimpfact.Where(x => x.risk_admin_Iniassimpfactstatus == "Active").ToList();
        }




        [Route("api/initialAssessmentImpact/Insertrisk_admin_iniassimpfact")]
        [HttpPost]
        public IActionResult InsertinitialAssessmentImpactModelDetails([FromBody] risk_admin_iniassimpfact risk_Initial_Assessment_Impact_Factor)
        {
            try
            {
                risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactname = risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_iniassimpfact
                    .FirstOrDefault(d => d.risk_admin_Iniassimpfactname == risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactname && d.risk_admin_Iniassimpfactstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk initial assessment impact Name with the same name already exists.");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_iniassimpfact
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_Iniassimpfactid) ?? 0; // If no records are found, default to 0
                                                      // Increment the law_type_id by 1
                risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactid = maxLawTypeId + 1;
                // Proceed with the insertion
                var riskmatrixModel = this.mySqlDBContext.risk_admin_iniassimpfact;
                riskmatrixModel.Add(risk_Initial_Assessment_Impact_Factor);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactstatus = "Active";
                risk_Initial_Assessment_Impact_Factor.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk initial assessment impact Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/initialAssessmentImpact/Updaterisk_admin_iniassimpfact")]
        [HttpPut]
        public IActionResult UpdateinitialAssessmentImpactModelDetails([FromBody] risk_admin_iniassimpfact risk_Initial_Assessment_Impact_Factor)
        {
            try
            {
                if (risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactname = risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_iniassimpfact
                      .FirstOrDefault(d => d.risk_admin_Iniassimpfactname == risk_Initial_Assessment_Impact_Factor.risk_admin_Iniassimpfactname && d.risk_admin_Iniassimpfactstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk initial assessment impact Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(risk_Initial_Assessment_Impact_Factor);
                    this.mySqlDBContext.Entry(risk_Initial_Assessment_Impact_Factor).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Initial_Assessment_Impact_Factor);

                    Type type = typeof(risk_admin_iniassimpfact);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Initial_Assessment_Impact_Factor, null) == null || property.GetValue(risk_Initial_Assessment_Impact_Factor, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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
                    return BadRequest("Error: Risk initial assessment impact Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/initialAssessmentImpact/Deleterisk_admin_iniassimpfact")]
        [HttpDelete]
        public void DeleteinitialAssessmentImpactModelDetails(int id)
        {
            var currentClass = new risk_admin_iniassimpfact { risk_admin_Iniassimpfactid = id };
            currentClass.risk_admin_Iniassimpfactstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_Iniassimpfactstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        //Risk Mitigation Decision List

        [Route("api/risk_mitigation_decision/Getrisk_admin_mitdecilist")]
        [HttpGet]

        public IEnumerable<risk_admin_mitdecilist> Getrisk_mitigation_decisionModelDetails()
        {
            return this.mySqlDBContext.risk_admin_mitdecilist.Where(x => x.risk_admin_MitdeciListstatus == "Active").ToList();
        }




        [Route("api/risk_mitigation_decision/Insertrisk_admin_mitdecilist")]
        [HttpPost]
        public IActionResult Insertrisk_mitigation_decisionModelDetails([FromBody] risk_admin_mitdecilist risk_Mitigation_Decision)
        {
            try
            {
                risk_Mitigation_Decision.risk_admin_MitdeciListname = risk_Mitigation_Decision.risk_admin_MitdeciListname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_mitdecilist
                    .FirstOrDefault(d => d.risk_admin_MitdeciListname == risk_Mitigation_Decision.risk_admin_MitdeciListname && d.risk_admin_MitdeciListstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk mitigation decision Name with the same name already exists.");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_mitdecilist
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_MitdeciListid) ?? 0; // If no records are found, default to 0
                                                    // Increment the law_type_id by 1
                risk_Mitigation_Decision.risk_admin_MitdeciListid = maxLawTypeId + 1;
                // Proceed with the insertion
                var riskmatrixModel = this.mySqlDBContext.risk_admin_mitdecilist;
                riskmatrixModel.Add(risk_Mitigation_Decision);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Mitigation_Decision.risk_admin_MitdeciListdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Mitigation_Decision.risk_admin_MitdeciListstatus = "Active";
                risk_Mitigation_Decision.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk mitigation decision Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/risk_mitigation_decision/Updaterisk_admin_mitdecilist")]
        [HttpPut]
        public IActionResult Updaterisk_mitigation_decisionModelDetails([FromBody] risk_admin_mitdecilist risk_Mitigation_Decision)
        {
            try
            {
                if (risk_Mitigation_Decision.risk_admin_MitdeciListid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Mitigation_Decision.risk_admin_MitdeciListname = risk_Mitigation_Decision.risk_admin_MitdeciListname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_mitdecilist
                      .FirstOrDefault(d => d.risk_admin_MitdeciListname == risk_Mitigation_Decision.risk_admin_MitdeciListname && d.risk_admin_MitdeciListstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Mitigation decision Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(risk_Mitigation_Decision);
                    this.mySqlDBContext.Entry(risk_Mitigation_Decision).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Mitigation_Decision);

                    Type type = typeof(risk_admin_mitdecilist);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Mitigation_Decision, null) == null || property.GetValue(risk_Mitigation_Decision, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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
                    return BadRequest("Error: Risk mitigation decision Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/risk_mitigation_decision/Deleterisk_admin_mitdecilist")]
        [HttpDelete]
        public void Deleterisk_mitigation_decisionModelDetails(int id)
        {
            var currentClass = new risk_admin_mitdecilist { risk_admin_MitdeciListid = id };
            currentClass.risk_admin_MitdeciListstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_MitdeciListstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Assessment Control Acceptance Criteria

        [Route("api/risk_asses_contr_accep_crit/Getrisk_admin_asscontracptcrit")]
        [HttpGet]

        public IEnumerable<risk_admin_asscontracptcrit> Getrisk_asses_contr_accep_critModelDetails()
        {
            return this.mySqlDBContext.risk_admin_asscontracptcrit.Where(x => x.risk_admin_asscontracptCritstatus == "Active").ToList();
        }




        [Route("api/risk_asses_contr_accep_crit/Insertrisk_admin_asscontracptcrit")]
        [HttpPost]
        public async Task<IActionResult> Insertrisk_asses_contr_accep_critModelDetails([FromBody] risk_admin_asscontracptcrit risk_Asses_Contr_Accep_Crit)
        {
            try
            {
                risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritname = risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_asscontracptcrit
                    .FirstOrDefault(d => d.risk_admin_asscontracptCritname == risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritname && d.risk_admin_asscontracptCritstatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:Assessment Control Acceptance Criteria Name with the same name already exists.");
                }
                //if (!CheckValidation(risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range, risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range, risk_Asses_Contr_Accep_Crit.array))
                //{
                //    return BadRequest("Error: Assessment Control Acceptance Range Not Valid.");
                //}
                var newMin = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range ;
                var newMax = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range ;

                // Ensure min is not greater than max.



                var existingRanges = await mySqlDBContext.risk_admin_asscontracptcrit
                    .Where(d => d.risk_admin_asscontracptCritid != risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritid && d.risk_admin_asscontracptCritstatus == "Active")
                    .Select(d => new { MinValue = d.risk_Asses_contr_accep_crit_min_range, MaxValue = d.risk_Asses_contr_accep_crit_max_range })
                    .ToListAsync();

                // Check for overlapping ranges.
                foreach (var range in existingRanges)
                {
                    bool isOverlapping =
                        (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                        (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                        (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                    if (isOverlapping)
                    {
                        return BadRequest("Error: New rating values overlap with existing ranges.");
                    }
                }
              
                    if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range != null &&
                        risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range != null )
                    {
                        
                        if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range > risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                        }
                    }

                    var maxLawTypeId = this.mySqlDBContext.risk_admin_asscontracptcrit
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_asscontracptCritid) ?? 0; // If no records are found, default to 0
                                                  // Increment the law_type_id by 1
                risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritid = maxLawTypeId + 1;
                // Proceed with the insertion
                var riskmatrixModel = this.mySqlDBContext.risk_admin_asscontracptcrit;
                riskmatrixModel.Add(risk_Asses_Contr_Accep_Crit);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritstatus = "Active";
                risk_Asses_Contr_Accep_Crit.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:Assessment Control Acceptance Criteria Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/risk_asses_contr_accep_crit/Updaterisk_admin_asscontracptcrit")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_asses_contr_accep_critModelDetails([FromBody] risk_admin_asscontracptcrit risk_Asses_Contr_Accep_Crit)
        {
            try
            {
                if (risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritname = risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_asscontracptcrit
                      .FirstOrDefault(d => d.risk_admin_asscontracptCritname == risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritname && d.risk_admin_asscontracptCritstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Assessment Control Acceptance Criteria Name with the same name already exists.");
                    }




                    var existingValues = await mySqlDBContext.risk_admin_asscontracptcrit
.Where(x => x.risk_admin_asscontracptCritid == risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritid)
.Select(x => new
{
    MinValue = x.risk_Asses_contr_accep_crit_min_range,
    MaxValue = x.risk_Asses_contr_accep_crit_max_range
})
.FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range ?? existingValues.MinValue;
                    var newMax = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range ?? existingValues.MaxValue;

                    // Ensure min is not greater than max.



                    var existingRanges = await mySqlDBContext.risk_admin_asscontracptcrit
                        .Where(d => d.risk_admin_asscontracptCritid != risk_Asses_Contr_Accep_Crit.risk_admin_asscontracptCritid && d.risk_admin_asscontracptCritstatus == "Active")
                        .Select(d => new { MinValue = d.risk_Asses_contr_accep_crit_min_range, MaxValue = d.risk_Asses_contr_accep_crit_max_range })
                        .ToListAsync();

                    // Check for overlapping ranges.
                    foreach (var range in existingRanges)
                    {
                        bool isOverlapping =
                            (newMin >= range.MinValue && newMin <= range.MaxValue) ||  // New min inside an existing range
                            (newMax >= range.MinValue && newMax <= range.MaxValue) ||  // New max inside an existing range
                            (newMin <= range.MinValue && newMax >= range.MaxValue);    // New range fully encloses an existing range

                        if (isOverlapping)
                        {
                            return BadRequest("Error: New rating values overlap with existing ranges.");
                        }
                    }
                    if (existingValues != null)
                    {
                        // Check validation for min and max values
                        if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range == null &&
                            risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range != null &&
                            existingValues.MinValue != 0)
                        {
                            //if (!CheckValidation(existingValues.MinValue, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                            //{
                            //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                            //}
                            if (existingValues.MinValue > risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }
                        }

                        if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range != null &&
                            risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range == null)
                        {


                            if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }
                        }
                    }
                    //        if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range != null && risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range != null)
                    //{
                    //    if (!CheckValidation(risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range, risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range, risk_Asses_Contr_Accep_Crit.array))
                    //    {
                    //        return BadRequest("Error: Assessment Control Acceptance Criteria Range Not Valid.");
                    //    }
                    //}


                    // Existing department, update logic

                    this.mySqlDBContext.Attach(risk_Asses_Contr_Accep_Crit);
                    this.mySqlDBContext.Entry(risk_Asses_Contr_Accep_Crit).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_Asses_Contr_Accep_Crit);

                    Type type = typeof(risk_admin_asscontracptcrit);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Asses_Contr_Accep_Crit, null) == null || property.GetValue(risk_Asses_Contr_Accep_Crit, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
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
                    return BadRequest("Error:Assessment Control Acceptance Criteria Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/risk_asses_contr_accep_crit/Deleterisk_admin_asscontracptcrit")]
        [HttpDelete]
        public void Deleterisk_asses_contr_accep_critModelDetails(int id)
        {
            var currentClass = new risk_admin_asscontracptcrit { risk_admin_asscontracptCritid = id };
            currentClass.risk_admin_asscontracptCritstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_asscontracptCritstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Risk Treatment Decision List


        [Route("api/RiskSupAdminController/Getrisk_admin_risktredecilist")]
        [HttpGet]
        public IEnumerable<risk_admin_risktredecilist> GetRiskTreatmentDecisionLists()
        {
            return this.mySqlDBContext.risk_admin_risktredecilist.Where(x => x.risk_admin_risktredeciliststatus == "Active").ToList();
        }

        // RiskTreatmentDecisionLists Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_risktredecilist")]
        [HttpPost]
        public IActionResult InsertRiskTreatmentDecisionLists([FromBody] risk_admin_risktredecilist RiskTreatmentDecisionLists)

        {
            try
            {
                RiskTreatmentDecisionLists.risk_admin_risktredecilistname = RiskTreatmentDecisionLists.risk_admin_risktredecilistname?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_risktredecilist
                    .FirstOrDefault(d => d.risk_admin_risktredecilistname == RiskTreatmentDecisionLists.risk_admin_risktredecilistname && d.risk_admin_risktredeciliststatus == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:Risk Treatment Decision List with the same name already exists.");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_risktredecilist
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_risktredecilistid) ?? 0; // If no records are found, default to 0
                                                       // Increment the law_type_id by 1
                RiskTreatmentDecisionLists.risk_admin_risktredecilistid = maxLawTypeId + 1;
                // Proceed with the insertion
                var RiskTreatmentDecisionList = this.mySqlDBContext.risk_admin_risktredecilist;
                RiskTreatmentDecisionList.Add(RiskTreatmentDecisionLists);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskTreatmentDecisionLists.risk_admin_risktredecilistdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskTreatmentDecisionLists.risk_admin_risktredeciliststatus = "Active";
                RiskTreatmentDecisionLists.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Treatment Decision List  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update RiskTreatmentDecisionLists

        [Route("api/RiskSupAdminController/Updaterisk_admin_risktredecilist")]
        [HttpPut]
        public IActionResult UpdateRiskTreatmentDecisionLists([FromBody] risk_admin_risktredecilist RiskTreatmentDecisionLists)
        {
            try
            {
                if (RiskTreatmentDecisionLists.risk_admin_risktredecilistid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    RiskTreatmentDecisionLists.risk_admin_risktredecilistname = RiskTreatmentDecisionLists.risk_admin_risktredecilistname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_risktredecilist
                     .FirstOrDefault(d => d.risk_admin_risktredecilistname == RiskTreatmentDecisionLists.risk_admin_risktredecilistname && d.risk_admin_risktredecilistid != RiskTreatmentDecisionLists.risk_admin_risktredecilistid && d.risk_admin_risktredeciliststatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Treatment Decision List with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(RiskTreatmentDecisionLists);
                    this.mySqlDBContext.Entry(RiskTreatmentDecisionLists).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(RiskTreatmentDecisionLists);

                    Type type = typeof(risk_admin_risktredecilist);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(RiskTreatmentDecisionLists, null) == null || property.GetValue(RiskTreatmentDecisionLists, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Treatment Decision List with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete RiskTreatmentDecisionLists

        [Route("api/RiskSupAdminController/Deleterisk_admin_risktredecilist")]
        [HttpDelete]
        public void DeleteRiskTreatmentDecisionLists(int id)
        {
            var currentClass = new risk_admin_risktredecilist { risk_admin_risktredecilistid = id };
            currentClass.risk_admin_risktredeciliststatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_risktredeciliststatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }






        //risk_treatmetdecisionmatrix


        [Route("api/RiskSupAdminController/Getrisk_admin_risktrdecimatrix")]
        [HttpGet]
        public IEnumerable<risk_admin_risktrdecimatrix> Getrisk_treatmetdecisionmatrix()
        {
            return this.mySqlDBContext.risk_admin_risktrdecimatrix.Where(x => x.risk_admin_risktrdecimatrixstatus == "Active").ToList();
        }

        //risk_treatmetdecisionmatrixs Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_risktrdecimatrix")]
        [HttpPost]
        public IActionResult Insertrisk_treatmetdecisionmatrixs([FromBody] risk_admin_risktrdecimatrix risk_treatmetdecisionmatrixs)

        {
            try
            {


                var maxLawTypeId = this.mySqlDBContext.risk_admin_risktrdecimatrix
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_risktrdecimatrixid) ?? 0; 
                risk_treatmetdecisionmatrixs.risk_admin_risktrdecimatrixid = maxLawTypeId + 1;

                // Proceed with the insertion
                var risk_treatmetdecisionmatrix = this.mySqlDBContext.risk_admin_risktrdecimatrix;
                risk_treatmetdecisionmatrix.Add(risk_treatmetdecisionmatrixs);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_treatmetdecisionmatrixs.risk_admin_risktrdecimatrixdate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_treatmetdecisionmatrixs.risk_admin_risktrdecimatrixstatus = "Active";
                risk_treatmetdecisionmatrixs.isImported = "No";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error: Risk Treatment Decision Matrix  with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update risk_treatmetdecisionmatrixs

        [Route("api/RiskSupAdminController/Updaterisk_admin_risktrdecimatrix")]
        [HttpPut]
        public IActionResult Updaterisk_treatmetdecisionmatrixs([FromBody] risk_admin_risktrdecimatrix risk_treatmetdecisionmatrixs)
        {
            try
            {
                if (risk_treatmetdecisionmatrixs.risk_admin_risktrdecimatrixid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {


                    // Existing department, update logic
                    this.mySqlDBContext.Attach(risk_treatmetdecisionmatrixs);
                    this.mySqlDBContext.Entry(risk_treatmetdecisionmatrixs).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_treatmetdecisionmatrixs);

                    Type type = typeof(risk_admin_risktrdecimatrix);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_treatmetdecisionmatrixs, null) == null || property.GetValue(risk_treatmetdecisionmatrixs, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error: Risk Treatment Decision Matrix with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete risk_treatmetdecisionmatrixs

        [Route("api/RiskSupAdminController/Deleterisk_admin_risktrdecimatrix")]
        [HttpDelete]
        public void Deleterisk_treatmetdecisionmatrixs(int id)
        {
            var currentClass = new risk_admin_risktrdecimatrix { risk_admin_risktrdecimatrixid = id };
            currentClass.risk_admin_risktrdecimatrixstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_risktrdecimatrixstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        //Mitigation Control Default Settings


        [Route("api/RiskSupAdminController/getrisk_admin_mitactireq")]
        [HttpGet]

        public IEnumerable<risk_admin_mitactireq> getmitigation_action()
        {
            return this.mySqlDBContext.risk_admin_mitactireq.Where(x => x.risk_admin_MitActiReqstatus == "Active").OrderBy(x => x.risk_admin_MitActiReqid).ToList();


        }

        // Insert Mitigation Action Required 

        [Route("api/RiskSupAdminController/insertrisk_admin_mitactireq")]
        [HttpPost]

        public IActionResult insertmitigation_action([FromBody] risk_admin_mitactireq mitigation_actions)
        {
            try
            {
                mitigation_actions.risk_admin_MitActiReqname = mitigation_actions.risk_admin_MitActiReqname?.Trim();
                var existing_name = this.mySqlDBContext.risk_admin_mitactireq.FirstOrDefault(x => x.risk_admin_MitActiReqname == mitigation_actions.risk_admin_MitActiReqname && x.risk_admin_MitActiReqstatus == "Active");
                if (existing_name != null)
                {
                    return BadRequest("Error:Name Already Exist");

                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_mitactireq
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_MitActiReqid) ?? 0;
                mitigation_actions.risk_admin_MitActiReqid = maxLawTypeId + 1;
                var data = this.mySqlDBContext.risk_admin_mitactireq;
                data.Add(mitigation_actions);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                mitigation_actions.risk_admin_MitActiReqdate = dt1;
                mitigation_actions.risk_admin_MitActiReqstatus = "Active";
                mitigation_actions.isImported = "No";
                this.mySqlDBContext.SaveChanges();
                return Ok();

            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Loss Event Threat Category L3 Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }

            }

        }


        // Update Mitigation Action Required 

        [Route("api/RiskSupAdminController/updaterisk_admin_mitactireq")]
        [HttpPut]

        public IActionResult updatemitigation_action([FromBody] risk_admin_mitactireq mitigation_actions)
        {
            try
            {

                if (mitigation_actions.risk_admin_MitActiReqid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    mitigation_actions.risk_admin_MitActiReqname = mitigation_actions.risk_admin_MitActiReqname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_mitactireq
                     .FirstOrDefault(d => d.risk_admin_MitActiReqname == mitigation_actions.risk_admin_MitActiReqname && d.risk_admin_MitActiReqid != mitigation_actions.risk_admin_MitActiReqid && d.risk_admin_MitActiReqstatus == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Name already exists.");
                    }
  
                    // Existing department, update logic
                    this.mySqlDBContext.Attach(mitigation_actions);
                    this.mySqlDBContext.Entry(mitigation_actions).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(mitigation_actions);

                    Type type = typeof(risk_admin_mitactireq);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(mitigation_actions, null) == null || property.GetValue(mitigation_actions, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }

            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete Mitigation Action Required 

        [Route("api/RiskSupAdminController/deleterisk_admin_mitactireq")]
        [HttpDelete]
        public void deletemitigation_action(int id)
        {
            var currentClass = new risk_admin_mitactireq { risk_admin_MitActiReqid = id };
            currentClass.risk_admin_MitActiReqstatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_MitActiReqstatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }






        //get Action Priority List


        [Route("api/RiskSupAdminController/getrisk_admin_actiprilist")]
        [HttpGet]

        public IEnumerable<risk_admin_actiprilist> getaction_priority_list()
        {
            return this.mySqlDBContext.risk_admin_actiprilist.Where(x => x.risk_admin_actiPriListstats == "Active").OrderBy(x => x.risk_admin_actiPriListid).ToList();


        }

        // Insert Action Priority List


        [Route("api/RiskSupAdminController/insertrisk_admin_actiprilist")]
        [HttpPost]

        public IActionResult insertmitigation_action([FromBody] risk_admin_actiprilist action_priority_lists)
        {
            try
            {
                action_priority_lists.risk_admin_actiPriListname = action_priority_lists.risk_admin_actiPriListname?.Trim();
                var existing_name = this.mySqlDBContext.risk_admin_actiprilist
                    .FirstOrDefault(x => x.risk_admin_actiPriListname == action_priority_lists.risk_admin_actiPriListname && x.risk_admin_actiPriListstats == "Active");
                if (existing_name != null)
                {
                    return BadRequest("Error:Name Already Exist");

                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_actiprilist
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_actiPriListid) ?? 0;
                action_priority_lists.risk_admin_actiPriListid = maxLawTypeId + 1;

                var data = this.mySqlDBContext.risk_admin_actiprilist;
                data.Add(action_priority_lists);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                action_priority_lists.risk_admin_actiPriListdate = dt1;
                action_priority_lists.risk_admin_actiPriListstats = "Active";
                action_priority_lists.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();

            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Loss Event Threat Category L3 Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }

            }

        }


        // Update Action Priority List


        [Route("api/RiskSupAdminController/updaterisk_admin_actiprilist")]
        [HttpPut]

        public IActionResult updateaction_priority_list([FromBody] risk_admin_actiprilist action_priority_lists)
        {
            try
            {

                if (action_priority_lists.risk_admin_actiPriListid == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    action_priority_lists.risk_admin_actiPriListname = action_priority_lists.risk_admin_actiPriListname?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_actiprilist
                     .FirstOrDefault(d => d.risk_admin_actiPriListname == action_priority_lists.risk_admin_actiPriListname && d.risk_admin_actiPriListid != action_priority_lists.risk_admin_actiPriListid && d.risk_admin_actiPriListstats == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(action_priority_lists);
                    this.mySqlDBContext.Entry(action_priority_lists).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(action_priority_lists);

                    Type type = typeof(risk_admin_actiprilist);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(action_priority_lists, null) == null || property.GetValue(action_priority_lists, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }

            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete Action Priority List


        [Route("api/RiskSupAdminController/deleterisk_admin_actiprilist")]
        [HttpDelete]
        public void deleteaction_priority_lists(int id)
        {
            var currentClass = new risk_admin_actiprilist { risk_admin_actiPriListid = id };
            currentClass.risk_admin_actiPriListstats = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_admin_actiPriListstats").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }

}


