
using DomainModel;
using Microsoft.AspNetCore.Mvc;
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
    public class risk_admin_controller : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;
        public risk_admin_controller(MySqlDBContext SqlDBContext)
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



        //loss event threat category(l1)



        [Route("api/RiskSupAdminController/Getrisk_admin_letc_l1")]
        [HttpGet]
        public IEnumerable<risk_admin_letc_l1> Getloss_event_threat_category()
        {
            return this.mySqlDBContext.risk_admin_letc_l1.Where(x => x.status == "Active")
                .OrderBy(r => r.risk_admin_LETC_L1_id)
                .ToList();
        }


        //loss event threat category Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_letc_l1")]
        [HttpPost]
        public IActionResult Insertloss_event_threat_category([FromBody] risk_admin_letc_l1 loss_event_threat_categorys)
        {
            try
            {
                loss_event_threat_categorys.risk_admin_LETC_L1_Name = loss_event_threat_categorys.risk_admin_LETC_L1_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_letc_l1
                    .FirstOrDefault(d => d.risk_admin_LETC_L1_Name == loss_event_threat_categorys.risk_admin_LETC_L1_Name && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:Loss Event Threat Category L1 Name with the same name already exists.");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_letc_l1
  .Where(d => d.isImported == "No")
 .Max(d => (int?)d.risk_admin_LETC_L1_id) ?? 0; // If no records are found, default to 0
                                                         // Increment the law_type_id by 1
                loss_event_threat_categorys.risk_admin_LETC_L1_id = maxLawTypeId + 1;
                // Proceed with the insertion
                var loss_event_threat_category = this.mySqlDBContext.risk_admin_letc_l1;
                loss_event_threat_category.Add(loss_event_threat_categorys);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                loss_event_threat_categorys.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                loss_event_threat_categorys.status = "Active";
                loss_event_threat_categorys.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:Loss Event Threat Category L1 Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Updateloss_event_threat_category

        [Route("api/RiskSupAdminController/Updaterisk_admin_letc_l1")]
        [HttpPut]
        public IActionResult Updateloss_event_threat_category([FromBody] risk_admin_letc_l1 loss_event_threat_categorys)
        {
            try
            {
                if (loss_event_threat_categorys.risk_admin_LETC_L1_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    loss_event_threat_categorys.risk_admin_LETC_L1_Name = loss_event_threat_categorys.risk_admin_LETC_L1_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_letc_l1
                     .FirstOrDefault(d => d.risk_admin_LETC_L1_Name == loss_event_threat_categorys.risk_admin_LETC_L1_Name && d.risk_admin_LETC_L1_id != loss_event_threat_categorys.risk_admin_LETC_L1_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Loss Event Threat Category L1 Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(loss_event_threat_categorys);
                    this.mySqlDBContext.Entry(loss_event_threat_categorys).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(loss_event_threat_categorys);

                    Type type = typeof(risk_admin_letc_l1);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(loss_event_threat_categorys, null) == null || property.GetValue(loss_event_threat_categorys, null).Equals(0))
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
                    return BadRequest("Error: Loss Event Threat Category L1 Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }



        //Delete loss_event_threat_category

        [Route("api/RiskSupAdminController/Deleterisk_admin_letc_l1")]
        [HttpDelete]
        public void Deleteloss_event_threat_category(int id)
        {
            var currentClass = new risk_admin_letc_l1 { risk_admin_LETC_L1_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        // loss_event_threat_category_l2 



        [Route("api/RiskSupAdminController/Getrisk_admin_letc_l2")]
        [HttpGet]
        public IEnumerable<risk_admin_letc_l2> Getloss_event_threat_category_l2()
        {
            return this.mySqlDBContext.risk_Admin_Letc_L2s.Where(x => x.status == "Active")
                .OrderBy(r => r.risk_admin_letc_l2_id)
                .ToList();
        }


        //loss event threat category Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_letc_l2")]
        [HttpPost]
        public IActionResult Insertloss_event_threat_category_l2([FromBody] risk_admin_letc_l2 losseventthreacategory_l2s)
        {
            try
            {
                losseventthreacategory_l2s.risk_admin_letc_l2_name = losseventthreacategory_l2s.risk_admin_letc_l2_name?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_Admin_Letc_L2s
                    .FirstOrDefault(d => d.risk_admin_letc_l2_name == losseventthreacategory_l2s.risk_admin_letc_l2_name && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:  Loss Event Threat Category L2 Name with the same name already exists.");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_Admin_Letc_L2s
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_letc_l2_id) ?? 0; // If no records are found, default to 0
                                               // Increment the law_type_id by 1
                losseventthreacategory_l2s.risk_admin_letc_l2_id = maxLawTypeId + 1;
                // Proceed with the insertion
                var losseventthreacategory_l2 = this.mySqlDBContext.risk_Admin_Letc_L2s;
                losseventthreacategory_l2.Add(losseventthreacategory_l2s);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l2s.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l2s.status = "Active";
                losseventthreacategory_l2s.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Loss Event Threat Category L2 Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Updateloss_event_threat_category

        [Route("api/RiskSupAdminController/Updaterisk_admin_letc_l2")]
        [HttpPut]
        public IActionResult Updateloss_event_threat_category_l2([FromBody] risk_admin_letc_l2 losseventthreacategory_l2s)
        {
            try
            {
                if (losseventthreacategory_l2s.risk_admin_letc_l2_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    losseventthreacategory_l2s.risk_admin_letc_l2_name = losseventthreacategory_l2s.risk_admin_letc_l2_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_Admin_Letc_L2s
                     .FirstOrDefault(d => d.risk_admin_letc_l2_name == losseventthreacategory_l2s.risk_admin_letc_l2_name && d.risk_admin_letc_l2_id != losseventthreacategory_l2s.risk_admin_letc_l2_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Loss Event Threat Category L2 Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(losseventthreacategory_l2s);
                    this.mySqlDBContext.Entry(losseventthreacategory_l2s).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(losseventthreacategory_l2s);

                    Type type = typeof(risk_admin_letc_l2);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(losseventthreacategory_l2s, null) == null || property.GetValue(losseventthreacategory_l2s, null).Equals(0))
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
                    return BadRequest("Error:  Loss Event Threat Category L2 Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }



        //Delete loss_event_threat_category

        [Route("api/RiskSupAdminController/Deleterisk_admin_letc_l2")]
        [HttpDelete]
        public void Deleteloss_event_threat_category_l2(int id)
        {
            var currentClass = new risk_admin_letc_l2 { risk_admin_letc_l2_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




        // loss_event_threat_category_l3

        [Route("api/RiskSupAdminController/Getrisk_admin_letc_l3")]
        [HttpGet]
        public IEnumerable<risk_admin_letc_l3> Getloss_event_threat_category_l3()
        {
            return this.mySqlDBContext.risk_admin_letc_l3.Where(x => x.status == "Active")
                .OrderBy(r => r.risk_admin_LETC_l3_id)
                .ToList();
        }


        //loss event threat category l3 Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_letc_l3")]
        [HttpPost]
        public IActionResult Insertloss_event_threat_category_l3([FromBody] risk_admin_letc_l3 losseventthreacategory_l3s)
        {
            try
            {
                losseventthreacategory_l3s.risk_admin_LETC_l3_name = losseventthreacategory_l3s.risk_admin_LETC_l3_name?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_letc_l3
                    .FirstOrDefault(d => d.risk_admin_LETC_l3_name == losseventthreacategory_l3s.risk_admin_LETC_l3_name && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:  Loss Event Threat Category L3 Name with the same name already exists.");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_letc_l3
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_LETC_l3_id) ?? 0; // If no records are found, default to 0
                                               // Increment the law_type_id by 1
                losseventthreacategory_l3s.risk_admin_LETC_l3_id = maxLawTypeId + 1;
                // Proceed with the insertion
                var losseventthreacategory_l3 = this.mySqlDBContext.risk_admin_letc_l3;
                losseventthreacategory_l3.Add(losseventthreacategory_l3s);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l3s.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l3s.status = "Active";
                losseventthreacategory_l3s.isImported = "No";

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



        //Updateloss_event_threat_category

        [Route("api/RiskSupAdminController/Updaterisk_admin_letc_l3")]
        [HttpPut]
        public IActionResult Updateloss_event_threat_category_l3([FromBody] risk_admin_letc_l3 losseventthreacategory_l3s)
        {
            try
            {
                if (losseventthreacategory_l3s.risk_admin_LETC_l3_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    losseventthreacategory_l3s.risk_admin_LETC_l3_name = losseventthreacategory_l3s.risk_admin_LETC_l3_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_letc_l3
                     .FirstOrDefault(d => d.risk_admin_LETC_l3_name == losseventthreacategory_l3s.risk_admin_LETC_l3_name && d.risk_admin_LETC_l3_id != losseventthreacategory_l3s.risk_admin_LETC_l3_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:  Loss Event Threat Category L3 Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(losseventthreacategory_l3s);
                    this.mySqlDBContext.Entry(losseventthreacategory_l3s).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(losseventthreacategory_l3s);

                    Type type = typeof(risk_admin_letc_l3);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(losseventthreacategory_l3s, null) == null || property.GetValue(losseventthreacategory_l3s, null).Equals(0))
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
                    return BadRequest("Error:  Loss Event Threat Category L3 Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }



        //Delete loss_event_threat_category

        [Route("api/RiskSupAdminController/Deleterisk_admin_letc_l3")]
        [HttpDelete]
        public void Deleteloss_event_threat_category_l3(int id)
        {
            var currentClass = new risk_admin_letc_l3 { risk_admin_LETC_l3_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Risk Control Effectiveness Rating
        //Get Risk Control Effectiveness Rating

        [Route("api/risk_admin_controller/getRiskContrEffeRating")]
        [HttpGet]

        public IEnumerable<riskConEffRatingModel> getRiskContrEffeRating()
        {
            return this.mySqlDBContext.riskConEffRatingModels.Where(d => d.status == "Active").OrderBy(d => d.risk_admin_RiskContrEffeRatingRating);
        }


        [Route("api/risk_admin_controller/InsertRiskContrEffeRating")]
        [HttpPost]
        public IActionResult InsertRiskContrEffeRating([FromBody] riskConEffRatingModel riskConEffRatingModels)
        {
            try
            {
                riskConEffRatingModels.risk_admin_RiskContrEffeRatingName = riskConEffRatingModels.risk_admin_RiskContrEffeRatingName?.Trim();

                var existedName = this.mySqlDBContext.riskConEffRatingModels.FirstOrDefault(d => d.risk_admin_RiskContrEffeRatingName == riskConEffRatingModels.risk_admin_RiskContrEffeRatingName && d.status == "Active");
                if (existedName != null)
                {
                    return BadRequest("Error:The Entered Risk Control Effectiveness Rating Name Was Already Exist");
                }
                var existingRatingVal = this.mySqlDBContext.riskConEffRatingModels.FirstOrDefault(d => d.risk_admin_RiskContrEffeRatingRating == riskConEffRatingModels.risk_admin_RiskContrEffeRatingRating && d.status == "Active");
                if (existingRatingVal != null)
                {
                    return BadRequest("Error:The selected rating value already exist");
                }

                var existingColor = this.mySqlDBContext.riskConEffRatingModels.FirstOrDefault(d => d.risk_admin_RiskContrEffeColor == riskConEffRatingModels.risk_admin_RiskContrEffeColor && d.status == "Active");
                if (existingColor != null)
                {
                    return BadRequest("Error:The Selected Color Already Exist");
                }

                var maxLawTypeId = this.mySqlDBContext.riskConEffRatingModels
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_RiskContrEffeRating_id) ?? 0; // If no records are found, default to 0
                                                 // Increment the law_type_id by 1
                riskConEffRatingModels.risk_admin_RiskContrEffeRating_id = maxLawTypeId + 1;

                riskConEffRatingModels.status = "Active";
                riskConEffRatingModels.isImported = "No";
                DateTime dt = DateTime.Now;
                string CurrentDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskConEffRatingModels.created_date = CurrentDate;

                var riskConEffRatingModel = this.mySqlDBContext.riskConEffRatingModels;
                riskConEffRatingModel.Add(riskConEffRatingModels);
                this.mySqlDBContext.SaveChanges();
                return Ok();

            }
            catch (Exception ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Control Effectiveness Rating Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }

            }




        }


        [Route("api/risk_admin_controller/UpdateRiskContrEffeRating")]
        [HttpPut]
        public IActionResult UpdateRiskContrEffeRating([FromBody] riskConEffRatingModel riskConEffRatingModels)
        {
            try
            {
                if (riskConEffRatingModels.risk_admin_RiskContrEffeRating_id == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {

                    riskConEffRatingModels.risk_admin_RiskContrEffeRatingName = riskConEffRatingModels.risk_admin_RiskContrEffeRatingName?.Trim();
                    var existedName = this.mySqlDBContext.riskConEffRatingModels.FirstOrDefault(d => d.risk_admin_RiskContrEffeRatingName == riskConEffRatingModels.risk_admin_RiskContrEffeRatingName && d.status == "Active");

                    if (existedName != null)
                    {
                        return BadRequest("Error:The Entered Risk Control Effectiveness Rating Name Was Already Exist ");
                    }
                    var existingRatingVal = this.mySqlDBContext.riskConEffRatingModels.FirstOrDefault(d => d.risk_admin_RiskContrEffeRatingRating == riskConEffRatingModels.risk_admin_RiskContrEffeRatingRating && d.status == "Active");
                    if (existingRatingVal != null)
                    {
                        return BadRequest("Error:The selected rating value already exist");
                    }

                    var existingColor = this.mySqlDBContext.riskConEffRatingModels.FirstOrDefault(d => d.risk_admin_RiskContrEffeColor == riskConEffRatingModels.risk_admin_RiskContrEffeColor && d.status == "Active");
                    if (existingColor != null)
                    {
                        return BadRequest("Error:The Selected Color Already Exist");
                    }

                    this.mySqlDBContext.Attach(riskConEffRatingModels);
                    this.mySqlDBContext.Entry(riskConEffRatingModels).State = EntityState.Modified;
                    var entry = this.mySqlDBContext.Entry(riskConEffRatingModels);
                    Type classData = typeof(riskConEffRatingModel);
                    PropertyInfo[] property = classData.GetProperties();
                    foreach (PropertyInfo prop in property)
                    {
                        if (prop.GetValue(riskConEffRatingModels, null) == null || prop.GetValue(riskConEffRatingModels, null).Equals(0))
                        {
                            entry.Property(prop.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok();

                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error:Risk Control Effectiveness Rating Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/risk_admin_controller/DeleteRiskConEffRating")]
        [HttpDelete]
        public void DeleteRiskConEffRating(int id)
        {
            var currentClass = new riskConEffRatingModel { risk_admin_RiskContrEffeRating_id = id };
            currentClass.status = "InActive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }



        //control risk of assessment

        //get control risk of assessment


        [Route("api/RiskSupAdminController/Getrisk_admin_control_risk_of_assessment")]
        [HttpGet]
        public IEnumerable<risk_admin_control_risk_of_assessment> Getrisk_admin_control_risk_of_assessment()
        {
            return this.mySqlDBContext.risk_admin_control_risk_of_assessments.Where(x => x.status == "Active")
                .OrderBy(r => r.control_risk_of_assessment_range_min)
                .ToList();
        }


        //control risk of assessment Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_control_risk_of_assessment")]
        [HttpPost]
        public async Task< IActionResult> Insertrisk_admin_control_risk_of_assessment([FromBody] risk_admin_control_risk_of_assessment risk_admin_control_risk_of_assessments)
        {
            try
            {
                risk_admin_control_risk_of_assessments.control_risk_of_assessment_name = risk_admin_control_risk_of_assessments.control_risk_of_assessment_name?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_control_risk_of_assessments
                    .FirstOrDefault(d => d.control_risk_of_assessment_name == risk_admin_control_risk_of_assessments.control_risk_of_assessment_name && d.control_risk_of_assessment_id != risk_admin_control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Risk of Assessment Name with the same name already exists.");
                }
                if (risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min > risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max)
                {
                    return BadRequest("Error: Risk control Assessment Minimum value is greater that  Risk control Assessment Maximum Value.");
                }
                //if (!CheckValidation(risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min, risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max, risk_admin_control_risk_of_assessments.array))
                //{
                //    return BadRequest("Error: Control Risk of Assessment Range Not Valid.");
                //}
                var existingColor = this.mySqlDBContext.risk_admin_control_risk_of_assessments
                   .FirstOrDefault(d => d.color_code == risk_admin_control_risk_of_assessments.color_code && d.control_risk_of_assessment_id != risk_admin_control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                if (existingColor != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Risk of Assessment colour with the same colour already exists.");
                }

                var newMin = risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min ;
                var newMax = risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max;



                var existingRanges = await mySqlDBContext.risk_admin_control_risk_of_assessments
                    .Where(d => d.control_risk_of_assessment_id != risk_admin_control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active")
                    .Select(d => new { MinValue = d.control_risk_of_assessment_range_min, MaxValue = d.control_risk_of_assessment_range_max })
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
                if (risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min != null &&
                      risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max != null)
                {

                    if (risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min > risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max)
                    {
                        return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                    }
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_control_risk_of_assessments
.Where(d => d.isImported == "No")
.Max(d => (int?)d.control_risk_of_assessment_id) ?? 0; // If no records are found, default to 0
                                                       // Increment the law_type_id by 1
                risk_admin_control_risk_of_assessments.control_risk_of_assessment_id = maxLawTypeId + 1;

                // Proceed with the insertion
                var residual_risk_rating = this.mySqlDBContext.risk_admin_control_risk_of_assessments;
                residual_risk_rating.Add(risk_admin_control_risk_of_assessments);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_control_risk_of_assessments.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_control_risk_of_assessments.status = "Active";
                risk_admin_control_risk_of_assessments.isImported = "No";


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


        //Update control risk of assessment

        [Route("api/RiskSupAdminController/Updaterisk_admin_control_risk_of_assessments")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_admin_control_risk_of_assessments([FromBody] risk_admin_control_risk_of_assessment risk_admin_control_risk_of_assessments)

        {
            try
            {
                if (risk_admin_control_risk_of_assessments.control_risk_of_assessment_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    risk_admin_control_risk_of_assessments.control_risk_of_assessment_name = risk_admin_control_risk_of_assessments.control_risk_of_assessment_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_control_risk_of_assessments
                     .FirstOrDefault(d => d.control_risk_of_assessment_name == risk_admin_control_risk_of_assessments.control_risk_of_assessment_name && d.control_risk_of_assessment_id != risk_admin_control_risk_of_assessments.control_risk_of_assessment_id && d.control_risk_of_assessment_id != risk_admin_control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:  Control Risk of Assessment with the same name already exists.");
                    }


                    var existingValues = await mySqlDBContext.risk_admin_control_risk_of_assessments
                  .Where(x => x.control_risk_of_assessment_id == risk_admin_control_risk_of_assessments.control_risk_of_assessment_id)
                  .Select(x => new
                  {
                      MinValue = x.control_risk_of_assessment_range_min,
                      MaxValue = x.control_risk_of_assessment_range_max
                  })
                  .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min ?? existingValues.MinValue;
                    var newMax = risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_control_risk_of_assessments
                        .Where(d => d.control_risk_of_assessment_id != risk_admin_control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active")
                        .Select(d => new { MinValue = d.control_risk_of_assessment_range_min, MaxValue = d.control_risk_of_assessment_range_max })
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
                        if (risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min == null &&
                              risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max != null &&

                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min != null &&
                            risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_max == null)
                        {


                            if (risk_admin_control_risk_of_assessments.control_risk_of_assessment_range_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }
                    var existingColor = this.mySqlDBContext.risk_admin_control_risk_of_assessments
                  .FirstOrDefault(d => d.color_code == risk_admin_control_risk_of_assessments.color_code && d.control_risk_of_assessment_id != risk_admin_control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                    if (existingColor != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Control Risk of Assessment colour with the same colour already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(risk_admin_control_risk_of_assessments);
                    this.mySqlDBContext.Entry(risk_admin_control_risk_of_assessments).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_control_risk_of_assessments);

                    Type type = typeof(risk_admin_control_risk_of_assessment);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_admin_control_risk_of_assessments, null) == null || property.GetValue(risk_admin_control_risk_of_assessments, null).Equals(0))
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
                    return BadRequest("Error:  Control Risk of Assessment with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }


        //Delete control risk of assessment

        [Route("api/RiskSupAdminController/Deleterisk_admin_control_risk_of_assessments")]
        [HttpDelete]
        public void Deleterisk_admin_control_risk_of_assessments(int id)
        {
            var currentClass = new risk_admin_control_risk_of_assessment { control_risk_of_assessment_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




        //Residual Risk Rating

        //get residual risk rating


        [Route("api/RiskSupAdminController/Getrisk_admin_residual_risk_ratings")]
        [HttpGet]
        public IEnumerable<risk_admin_residual_risk_rating> Getrisk_admin_residual_risk_ratings()
        {
            return this.mySqlDBContext.risk_admin_residual_risk_ratings.Where(x => x.status == "Active")
                .OrderBy(r => r.residual_risk_rating_min_rating)
                .ToList();
        }

        //residual_risk_rating Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_admin_residual_risk_ratings")]
        [HttpPost]
        public async Task< IActionResult> Insertrisk_admin_residual_risk_ratings([FromBody] risk_admin_residual_risk_rating risk_admin_residual_risk_ratings)

        {
            try
            {
                risk_admin_residual_risk_ratings.residual_risk_rating_name = risk_admin_residual_risk_ratings.residual_risk_rating_name?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_residual_risk_ratings
                    .FirstOrDefault(d => d.residual_risk_rating_name == risk_admin_residual_risk_ratings.residual_risk_rating_name && d.residual_risk_rating_id != risk_admin_residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Residual Risk Rating with the same name already exists.");
                }
                if (risk_admin_residual_risk_ratings.residual_risk_rating_min_rating > risk_admin_residual_risk_ratings.residual_risk_rating_max_rating)
                {
                    return BadRequest("Error: Risk Residual Minimum value is greater that  Risk Residual Maximum Value.");
                }

                //if (!CheckValidation(risk_admin_residual_risk_ratings.residual_risk_rating_min_rating, risk_admin_residual_risk_ratings.residual_risk_rating_max_rating, risk_admin_residual_risk_ratings.array))
                //{
                //    return BadRequest("Error: Residual Risk Rating Range Not Valid.");
                //}
                var EXISTINGCOLOR = this.mySqlDBContext.risk_admin_residual_risk_ratings
                 .FirstOrDefault(d => d.color_code == risk_admin_residual_risk_ratings.color_code && d.residual_risk_rating_id != risk_admin_residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                if (EXISTINGCOLOR != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Residual Risk colour with the same colour already exists.");
                }

                var newMin = risk_admin_residual_risk_ratings.residual_risk_rating_min_rating ;
                var newMax = risk_admin_residual_risk_ratings.residual_risk_rating_max_rating ;



                var existingRanges = await mySqlDBContext.risk_admin_residual_risk_ratings
                    .Where(d => d.residual_risk_rating_id != risk_admin_residual_risk_ratings.residual_risk_rating_id && d.status == "Active")
                    .Select(d => new { MinValue = d.residual_risk_rating_min_rating, MaxValue = d.residual_risk_rating_max_rating })
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
                if (risk_admin_residual_risk_ratings.residual_risk_rating_min_rating != null &&
                      risk_admin_residual_risk_ratings.residual_risk_rating_max_rating != null)
                {

                    if (risk_admin_residual_risk_ratings.residual_risk_rating_min_rating > risk_admin_residual_risk_ratings.residual_risk_rating_max_rating)
                    {
                        return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                    }
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_residual_risk_ratings
.Where(d => d.isImported == "No")
.Max(d => (int?)d.residual_risk_rating_id) ?? 0; // If no records are found, default to 0
                                                 // Increment the law_type_id by 1
                risk_admin_residual_risk_ratings.residual_risk_rating_id = maxLawTypeId + 1;
                // Proceed with the insertion
                var residual_risk_rating = this.mySqlDBContext.risk_admin_residual_risk_ratings;
                residual_risk_rating.Add(risk_admin_residual_risk_ratings);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_residual_risk_ratings.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_residual_risk_ratings.status = "Active";
                risk_admin_residual_risk_ratings.isImported = "No";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Residual Risk Rating with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update residual_risk_rating

        [Route("api/RiskSupAdminController/Updaterisk_admin_residual_risk_ratings")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_admin_residual_risk_ratings([FromBody] risk_admin_residual_risk_rating risk_admin_residual_risk_ratings)
        {
            try
            {
                if (risk_admin_residual_risk_ratings.residual_risk_rating_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    risk_admin_residual_risk_ratings.residual_risk_rating_name = risk_admin_residual_risk_ratings.residual_risk_rating_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_residual_risk_ratings
                     .FirstOrDefault(d => d.residual_risk_rating_name == risk_admin_residual_risk_ratings.residual_risk_rating_name && d.residual_risk_rating_id != risk_admin_residual_risk_ratings.residual_risk_rating_id && d.residual_risk_rating_id != risk_admin_residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Residual Risk Rating with the same name already exists.");
                    }
           
                    var EXISTINGCOLOR = this.mySqlDBContext.risk_admin_residual_risk_ratings
               .FirstOrDefault(d => d.color_code == risk_admin_residual_risk_ratings.color_code && d.residual_risk_rating_id != risk_admin_residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                    if (EXISTINGCOLOR != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Residual Risk colour with the same colour already exists.");
                    }





                    var existingValues = await mySqlDBContext.risk_admin_residual_risk_ratings
.Where(x => x.residual_risk_rating_id == risk_admin_residual_risk_ratings.residual_risk_rating_id)
.Select(x => new
{
    MinValue = x.residual_risk_rating_min_rating,
    MaxValue = x.residual_risk_rating_max_rating
})
.FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_admin_residual_risk_ratings.residual_risk_rating_min_rating ?? existingValues.MinValue;
                    var newMax = risk_admin_residual_risk_ratings.residual_risk_rating_max_rating ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_residual_risk_ratings
                        .Where(d => d.residual_risk_rating_id != risk_admin_residual_risk_ratings.residual_risk_rating_id && d.status == "Active")
                        .Select(d => new { MinValue = d.residual_risk_rating_min_rating, MaxValue = d.residual_risk_rating_max_rating })
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
                        if (risk_admin_residual_risk_ratings.residual_risk_rating_min_rating == null &&
                              risk_admin_residual_risk_ratings.residual_risk_rating_max_rating != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_admin_residual_risk_ratings.residual_risk_rating_max_rating)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_admin_residual_risk_ratings.residual_risk_rating_min_rating != null &&
                            risk_admin_residual_risk_ratings.residual_risk_rating_max_rating == null)
                        {


                            if (risk_admin_residual_risk_ratings.residual_risk_rating_min_rating > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }




                    // Existing department, update logic
                    this.mySqlDBContext.Attach(risk_admin_residual_risk_ratings);
                    this.mySqlDBContext.Entry(risk_admin_residual_risk_ratings).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_residual_risk_ratings);

                    Type type = typeof(risk_admin_residual_risk_rating);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_admin_residual_risk_ratings, null) == null || property.GetValue(risk_admin_residual_risk_ratings, null).Equals(0))
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
                    return BadRequest("Error: Residual Risk Rating with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete residual_risk_rating

        [Route("api/RiskSupAdminController/Deleterisk_admin_residual_risk_ratings")]
        [HttpDelete]
        public void Deleterisk_admin_residual_risk_ratings(int id)
        {
            var currentClass = new risk_admin_residual_risk_rating { residual_risk_rating_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




        //control_measure master page

        //get control_measure

        [Route("api/RiskSupAdminController/getrisk_admin_control_measures")]
        [HttpGet]

        public IEnumerable<risk_admin_control_measure> getrisk_admin_control_measures()
        {
            return this.mySqlDBContext.risk_admin_control_measures.Where(x => x.status == "active").OrderBy(x => x.control_measure_id).ToList();


        }

        // Insert control_measure

        [Route("api/RiskSupAdminController/insert_risk_admin_control_measures")]
        [HttpPost]

        public IActionResult insert_risk_admin_control_measures([FromBody] risk_admin_control_measure risk_admin_control_measures)
        {
            try
            {
                risk_admin_control_measures.control_measure_name = risk_admin_control_measures.control_measure_name?.Trim();

                var existing_name = this.mySqlDBContext.risk_admin_control_measures
                    .FirstOrDefault(x => x.control_measure_name == risk_admin_control_measures.control_measure_name && x.status == "Active");

                if (existing_name != null && existing_name.control_measure_id != risk_admin_control_measures.control_measure_id)
                {
                    return BadRequest("Error: control_measure Name Already Exists");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_control_measures
.Where(d => d.isImported == "No")
.Max(d => (int?)d.control_measure_id) ?? 0; // If no records are found, default to 0
                            // Increment the law_type_id by 1
                risk_admin_control_measures.control_measure_id = maxLawTypeId + 1;
                risk_admin_control_measures.created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_control_measures.status = "Active";
                risk_admin_control_measures.isImported = "No";

                this.mySqlDBContext.risk_admin_control_measures.Add(risk_admin_control_measures);

                this.mySqlDBContext.SaveChanges();

                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:control_measure Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        // Update control_measure

        [Route("api/RiskSupAdminController/updaterisk_admin_control_measures")]
        [HttpPut]

        public IActionResult updaterisk_admin_control_measures([FromBody] risk_admin_control_measure risk_admin_control_measures)
        {
            try
            {

                if (risk_admin_control_measures.control_measure_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    risk_admin_control_measures.control_measure_name = risk_admin_control_measures.control_measure_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_control_measures
                     .FirstOrDefault(d => d.control_measure_name == risk_admin_control_measures.control_measure_name && d.control_measure_id != risk_admin_control_measures.control_measure_id && d.control_measure_id != risk_admin_control_measures.control_measure_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:control_measure Name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(risk_admin_control_measures);
                    this.mySqlDBContext.Entry(risk_admin_control_measures).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_control_measures);

                    Type type = typeof(risk_admin_control_measure);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_admin_control_measures, null) == null || property.GetValue(risk_admin_control_measures, null).Equals(0))
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
                    return BadRequest("Error:control_measure name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete control_measure

        [Route("api/RiskSupAdminController/deleterisk_admin_control_measures")]
        [HttpDelete]
        public void deleterisk_admin_control_measures(int id)
        {
            var currentClass = new risk_admin_control_measure { control_measure_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //get Internal Control Component

        [Route("api/risk_admin_controller/get_internal_control_comp")]
        [HttpGet]

        public IEnumerable<risk_admin_inter_contr_comp> get_internal_control_comp()
        {
            return this.mySqlDBContext.risk_admin_inter_contr_comps.Where(d => d.status == "Active");
        }



        //get Internal Control Component by control_measure ID

        [Route("api/risk_admin_controller/get_internal_control_comp_by_id/{Id}")]
        [HttpGet]

        public IEnumerable<risk_admin_inter_contr_comp> get_internal_control_comp_by_id(int Id)
        {
            return this.mySqlDBContext.risk_admin_inter_contr_comps.Where(d => d.status == "Active" && d.risk_admin_inter_contr_comp_id==Id);
        }

        //insert Internal Control Component
        [Route("api/risk_admin_controller/insert_internal_control_comp")]
        [HttpPost]
        public IActionResult insert_internal_control_comp([FromBody] risk_admin_inter_contr_comp risk_admin_inter_contr_comps)
        {
            try {
                risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_name = risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_name?.Trim();

                var ExisteName = this.mySqlDBContext.risk_admin_inter_contr_comps.FirstOrDefault(d => d.risk_admin_inter_contr_comp_name == risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Internal Control Compoonent Name Already Exist");
                }

                var maxLawTypeId = this.mySqlDBContext.risk_admin_inter_contr_comps
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_inter_contr_comp_id) ?? 0; // If no records are found, default to 0
                            // Increment the law_type_id by 1
                risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_id = maxLawTypeId + 1;
                risk_admin_inter_contr_comps.status = "Active";
                risk_admin_inter_contr_comps.isImported = "No";
                DateTime d1 = DateTime.Now;
                string d2 = d1.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_inter_contr_comps.created_date = d2;
                this.mySqlDBContext.risk_admin_inter_contr_comps.Add(risk_admin_inter_contr_comps);
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:Internal Control Component Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        //Update Internal Control Component

        [Route("api/risk_admin_controller/Update_internal_con_comp")]
        [HttpPut]
        public IActionResult Update_internal_con_comp([FromBody] risk_admin_inter_contr_comp risk_admin_inter_contr_comps)
        {
            try { 
            if (risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_id == 0)
            {

                return Ok("Insertion successful");
            }
            else
            {

                risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_name = risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_name?.Trim();
                var ExisteName = this.mySqlDBContext.risk_admin_inter_contr_comps.FirstOrDefault(d => d.risk_admin_inter_contr_comp_name == risk_admin_inter_contr_comps.risk_admin_inter_contr_comp_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Internal Control Component Name Already Exist");
                }
                this.mySqlDBContext.Attach(risk_admin_inter_contr_comps);
                this.mySqlDBContext.Entry(risk_admin_inter_contr_comps).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(risk_admin_inter_contr_comps);
                Type Data = typeof(risk_admin_inter_contr_comp);
                PropertyInfo[] prop = Data.GetProperties();
                foreach (PropertyInfo p in prop)
                {
                    if (p.GetValue(risk_admin_inter_contr_comps, null) == null || p.GetValue(risk_admin_inter_contr_comps, null).Equals(0))
                    {
                        entry.Property(p.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
                return Ok("Insertion Successful");




            }
        }
             catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:control_measure name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete Internal Control Component
        [Route("api/risk_admin_controller/delete_internal_con_comp")]
        [HttpDelete]
        public void delete_internal_con_comp(int id)
        {
            var obj = new risk_admin_inter_contr_comp { risk_admin_inter_contr_comp_id = id };
            obj.status = "InActive";
            this.mySqlDBContext.Entry(obj).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }



        //Internal Control Principles


        //get Control Principles

        [Route("api/risk_admin_controller/getintercontrprinciplecontrolstatment")]
        [HttpGet]

        public IEnumerable<object> getintercontrprinciplecontrolstatment()
        {
            var Details = (from intercontrolprinciple in mySqlDBContext.risk_admin_inter_contr_principless
                           join intercontrolcomp in mySqlDBContext.risk_admin_inter_contr_comps on intercontrolprinciple.risk_admin_inter_contr_comp_id equals intercontrolcomp.risk_admin_inter_contr_comp_id
                           join controlmeasure in mySqlDBContext.risk_admin_control_measures on intercontrolprinciple.control_measure_id equals controlmeasure.control_measure_id
                           where intercontrolprinciple.status == "Active"
                           select new
                           {
                               intercontrolprinciple.risk_admin_inter_contr_Principles_id,
                               intercontrolprinciple.risk_admin_inter_contr_Principles_name,
                               intercontrolprinciple.control_measure_id,
                               controlmeasure.control_measure_name,
                               intercontrolprinciple.risk_admin_inter_contr_comp_id,
                               intercontrolcomp.risk_admin_inter_contr_comp_name,
                               combinename = controlmeasure.control_measure_name + "/" + intercontrolcomp.risk_admin_inter_contr_comp_name + "/" + intercontrolprinciple.risk_admin_inter_contr_Principles_name
                           })
                          .ToList();
            return Details;
        }



        [Route("api/risk_admin_controller/get_risk_admin_inter_contr_principless")]
        [HttpGet]

        public IEnumerable<risk_admin_inter_contr_principles> get_risk_admin_inter_contr_principless()
        {
            return this.mySqlDBContext.risk_admin_inter_contr_principless.Where(d => d.status == "Active");
        }

        //insert Internal Control Principles
        [Route("api/risk_admin_controller/insert_risk_admin_inter_contr_principless")]
        [HttpPost]
        public IActionResult insert_internal_control_comp([FromBody] risk_admin_inter_contr_principles risk_admin_inter_contr_principless)
        {
            try
            {
                risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_name = risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_name?.Trim();

                var ExisteName = this.mySqlDBContext.risk_admin_inter_contr_principless.FirstOrDefault(d => d.risk_admin_inter_contr_Principles_name == risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Internal Control principles Name Already Exist");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_inter_contr_principless
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_inter_contr_Principles_id) ?? 0;

                risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_id = maxLawTypeId + 1;
                risk_admin_inter_contr_principless.status = "Active";
                risk_admin_inter_contr_principless.isImported = "No";
                DateTime d1 = DateTime.Now;
                string d2 = d1.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_inter_contr_principless.created_date = d2;
                this.mySqlDBContext.risk_admin_inter_contr_principless.Add(risk_admin_inter_contr_principless);
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:Internal Control principles Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        //Update Internal Control Principles

        [Route("api/risk_admin_controller/Update_risk_admin_inter_contr_principless")]
        [HttpPut]
        public IActionResult Update_risk_admin_inter_contr_principless([FromBody] risk_admin_inter_contr_principles risk_admin_inter_contr_principless)
        {
            try
            {
                if (risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_id == 0)
                {

                    return Ok("Insertion successful");
                }
                else
                {

                    risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_name = risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_name?.Trim();
                    var ExisteName = this.mySqlDBContext.risk_admin_inter_contr_principless.FirstOrDefault(d => d.risk_admin_inter_contr_Principles_name == risk_admin_inter_contr_principless.risk_admin_inter_contr_Principles_name && d.status == "Active");
                    if (ExisteName != null)
                    {
                        return BadRequest("Error:Internal Control principles Name Already Exist");
                    }
                    this.mySqlDBContext.Attach(risk_admin_inter_contr_principless);
                    this.mySqlDBContext.Entry(risk_admin_inter_contr_principless).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_inter_contr_principless);
                    Type Data = typeof(risk_admin_inter_contr_principles);
                    PropertyInfo[] prop = Data.GetProperties();
                    foreach (PropertyInfo p in prop)
                    {
                        if (p.GetValue(risk_admin_inter_contr_principless, null) == null || p.GetValue(risk_admin_inter_contr_principless, null).Equals(0))
                        {
                            entry.Property(p.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Insertion Successful");
                }
            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    
                    return BadRequest("Error:principles name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete Internal Control Principles
        [Route("api/risk_admin_controller/delete_risk_admin_inter_contr_principless")]
        [HttpDelete]
        public void delete_risk_admin_inter_contr_principless(int id)
        {
            var obj = new risk_admin_inter_contr_principles { risk_admin_inter_contr_Principles_id = id };
            obj.status = "InActive";
            this.mySqlDBContext.Entry(obj).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }




        //get Control Activity Type

        [Route("api/risk_admin_controller/get_Control_Activity_Type")]
        [HttpGet]

        public IEnumerable<risk_admin_controlactivitytype> get_Control_Activity_Type()
        {
            return this.mySqlDBContext.risk_admin_controlactivitytypes.Where(d => d.status == "Active");
        }

        //insert risk_admin_controlactivitytype
        [Route("api/risk_admin_controller/insert_risk_admin_controlactivitytype")]
        [HttpPost]
        public IActionResult insert_risk_admin_controlactivitytype([FromBody] risk_admin_controlactivitytype risk_admin_controlactivitytypes)
        {
            try
            {
                risk_admin_controlactivitytypes.risk_admin_ControlActivityType_name = risk_admin_controlactivitytypes.risk_admin_ControlActivityType_name?.Trim();

                var ExisteName = this.mySqlDBContext.risk_admin_controlactivitytypes.FirstOrDefault(d => d.risk_admin_ControlActivityType_name == risk_admin_controlactivitytypes.risk_admin_ControlActivityType_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Control Activity Type Name Already Exist");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_controlactivitytypes
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_ControlActivityType_id) ?? 0;

                risk_admin_controlactivitytypes.risk_admin_ControlActivityType_id = maxLawTypeId + 1;
                risk_admin_controlactivitytypes.status = "Active";
                risk_admin_controlactivitytypes.isImported = "No";
                DateTime d1 = DateTime.Now;
                string d2 = d1.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_controlactivitytypes.created_date = d2;
                this.mySqlDBContext.risk_admin_controlactivitytypes.Add(risk_admin_controlactivitytypes);
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:Control Activity Type Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        //Update risk_admin_controlactivitytypes

        [Route("api/risk_admin_controller/Update_risk_admin_controlactivitytypes")]
        [HttpPut]
        public IActionResult Update_risk_admin_controlactivitytypes([FromBody] risk_admin_controlactivitytype risk_admin_controlactivitytypes)
        {
            try
            {
                if (risk_admin_controlactivitytypes.risk_admin_ControlActivityType_id == 0)
                {

                    return Ok("Insertion successful");
                }
                else
                {

                    risk_admin_controlactivitytypes.risk_admin_ControlActivityType_name = risk_admin_controlactivitytypes.risk_admin_ControlActivityType_name?.Trim();
                    var ExisteName = this.mySqlDBContext.risk_admin_controlactivitytypes.FirstOrDefault(d => d.risk_admin_ControlActivityType_name == risk_admin_controlactivitytypes.risk_admin_ControlActivityType_name && d.status == "Active");
                    if (ExisteName != null)
                    {
                        return BadRequest("Error:Control Activity Type Name Already Exist");
                    }
                    this.mySqlDBContext.Attach(risk_admin_controlactivitytypes);
                    this.mySqlDBContext.Entry(risk_admin_controlactivitytypes).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_controlactivitytypes);
                    Type Data = typeof(risk_admin_controlactivitytype);
                    PropertyInfo[] prop = Data.GetProperties();
                    foreach (PropertyInfo p in prop)
                    {
                        if (p.GetValue(risk_admin_controlactivitytypes, null) == null || p.GetValue(risk_admin_controlactivitytypes, null).Equals(0))
                        {
                            entry.Property(p.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Insertion Successful");
                }
            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error:Control Activity Type name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete risk_admin_controlactivitytype
        [Route("api/risk_admin_controller/delete_risk_admin_controlactivitytypes")]
        [HttpDelete]
        public void delete_risk_admin_controlactivitytypes(int id)
        {
            var obj = new risk_admin_controlactivitytype { risk_admin_ControlActivityType_id = id };
            obj.status = "InActive";
            this.mySqlDBContext.Entry(obj).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }



        //get Control Activity Nature

        [Route("api/risk_admin_controller/get_Control_Activity_Nature")]
        [HttpGet]

        public IEnumerable<risk_admin_control_activity_nature> get_Control_Activity_Nature()
        {
            return this.mySqlDBContext.risk_admin_control_activity_natures.Where(d => d.status == "Active");
        }

        //insert risk_admin_controlactivitynature
        [Route("api/risk_admin_controller/insert_risk_admin_controlactivity_nature")]
        [HttpPost]
        public IActionResult insert_risk_admin_controlactivity_nature([FromBody] risk_admin_control_activity_nature risk_admin_control_activity_natures)
        {
            try
            {
                risk_admin_control_activity_natures.risk_admin_control_activity_Nature_name = risk_admin_control_activity_natures.risk_admin_control_activity_Nature_name?.Trim();

                var ExisteName = this.mySqlDBContext.risk_admin_control_activity_natures.FirstOrDefault(d => d.risk_admin_control_activity_Nature_name == risk_admin_control_activity_natures.risk_admin_control_activity_Nature_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Control Activity Nature Name Already Exist");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_control_activity_natures
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_control_activity_Nature_id) ?? 0;

                risk_admin_control_activity_natures.risk_admin_control_activity_Nature_id = maxLawTypeId + 1;
                risk_admin_control_activity_natures.status = "Active";
                risk_admin_control_activity_natures.isImported = "No";
                DateTime d1 = DateTime.Now;
                string d2 = d1.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_control_activity_natures.created_date = d2;
                this.mySqlDBContext.risk_admin_control_activity_natures.Add(risk_admin_control_activity_natures);
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:Control Activity Nature Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        //Update risk_admin_controlactivity_nature

        [Route("api/risk_admin_controller/Update_risk_admin_controlactivity_nature")]
        [HttpPut]
        public IActionResult Update_risk_admin_controlactivity_nature([FromBody] risk_admin_control_activity_nature risk_admin_control_activity_natures)
        {
            try
            {
                if (risk_admin_control_activity_natures.risk_admin_control_activity_Nature_id == 0)
                {

                    return Ok("Insertion successful");
                }
                else
                {

                    risk_admin_control_activity_natures.risk_admin_control_activity_Nature_name = risk_admin_control_activity_natures.risk_admin_control_activity_Nature_name?.Trim();
                    var ExisteName = this.mySqlDBContext.risk_admin_control_activity_natures.FirstOrDefault(d => d.risk_admin_control_activity_Nature_name == risk_admin_control_activity_natures.risk_admin_control_activity_Nature_name && d.status == "Active");
                    if (ExisteName != null)
                    {
                        return BadRequest("Error:Control Activity Nature Name Already Exist");
                    }
                    this.mySqlDBContext.Attach(risk_admin_control_activity_natures);
                    this.mySqlDBContext.Entry(risk_admin_control_activity_natures).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_control_activity_natures);
                    Type Data = typeof(risk_admin_control_activity_nature);
                    PropertyInfo[] prop = Data.GetProperties();
                    foreach (PropertyInfo p in prop)
                    {
                        if (p.GetValue(risk_admin_control_activity_natures, null) == null || p.GetValue(risk_admin_control_activity_natures, null).Equals(0))
                        {
                            entry.Property(p.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Insertion Successful");
                }
            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error:Control Activity Nature name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete risk_admin_controlactivity_nature
        [Route("api/risk_admin_controller/delete_risk_admin_controlactivity_nature")]
        [HttpDelete]
        public void delete_risk_admin_controlactivity_nature(int id)
        {
            var obj = new risk_admin_control_activity_nature { risk_admin_control_activity_Nature_id = id };
            obj.status = "InActive";
            this.mySqlDBContext.Entry(obj).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }



        //get Control Activity sub Nature

        [Route("api/risk_admin_controller/get_Control_Activity_sub_Nature")]
        [HttpGet]

        public IEnumerable<risk_admin_control_activity_sub_nature> get_Control_Activity_sub_Nature()
        {
            return this.mySqlDBContext.risk_admin_control_activity_sub_natures.Where(d => d.status == "Active");
        }

        //insert risk_admin_controlactivity_sub_nature
        [Route("api/risk_admin_controller/insert_risk_admin_controlactivity_sub_nature")]
        [HttpPost]
        public IActionResult insert_risk_admin_controlactivity_sub_nature([FromBody] risk_admin_control_activity_sub_nature risk_admin_control_activity_sub_natures)
        {
            try
            {
                risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_name = risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_name?.Trim();

                var ExisteName = this.mySqlDBContext.risk_admin_control_activity_sub_natures.FirstOrDefault(d => d.risk_admin_Control_Activity_Sub_Nature_name == risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Control Activity sub Nature Name Already Exist");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_control_activity_sub_natures
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_Control_Activity_Sub_Nature_id) ?? 0;

                risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_id = maxLawTypeId + 1;
                risk_admin_control_activity_sub_natures.status = "Active";
                risk_admin_control_activity_sub_natures.isImported = "No";
                DateTime d1 = DateTime.Now;
                string d2 = d1.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_control_activity_sub_natures.created_date = d2;
                this.mySqlDBContext.risk_admin_control_activity_sub_natures.Add(risk_admin_control_activity_sub_natures);
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:Control Activity sub Nature Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        //Update risk_admin_controlactivity_sub_nature

        [Route("api/risk_admin_controller/Update_risk_admin_controlactivitysub_nature")]
        [HttpPut]
        public IActionResult Update_risk_admin_controlactivitysub_nature([FromBody] risk_admin_control_activity_sub_nature risk_admin_control_activity_sub_natures)
        {
            try
            {
                if (risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_id  == 0)
                {

                    return Ok("Insertion successful");
                }
                else
                {

                    risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_name = risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_name?.Trim();
                    var ExisteName = this.mySqlDBContext.risk_admin_control_activity_sub_natures.FirstOrDefault(d => d.risk_admin_Control_Activity_Sub_Nature_name == risk_admin_control_activity_sub_natures.risk_admin_Control_Activity_Sub_Nature_name && d.status == "Active");
                    if (ExisteName != null)
                    {
                        return BadRequest("Error:Control Activity sub Nature Name Already Exist");
                    }
                    this.mySqlDBContext.Attach(risk_admin_control_activity_sub_natures);
                    this.mySqlDBContext.Entry(risk_admin_control_activity_sub_natures).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_control_activity_sub_natures);
                    Type Data = typeof(risk_admin_control_activity_sub_nature);
                    PropertyInfo[] prop = Data.GetProperties();
                    foreach (PropertyInfo p in prop)
                    {
                        if (p.GetValue(risk_admin_control_activity_sub_natures, null) == null || p.GetValue(risk_admin_control_activity_sub_natures, null).Equals(0))
                        {
                            entry.Property(p.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Insertion Successful");
                }
            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error:Control Activity sub Nature name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete risk_admin_control_activity_sub_natures
        [Route("api/risk_admin_controller/delete_risk_admin_control_activity_sub_natures")]
        [HttpDelete]
        public void delete_risk_admin_control_activity_sub_natures(int id)
        {
            var obj = new risk_admin_control_activity_sub_nature { risk_admin_Control_Activity_Sub_Nature_id = id };
            obj.status = "InActive";
            this.mySqlDBContext.Entry(obj).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }


        //get Control Assertion Check 

        [Route("api/risk_admin_controller/get_Control_Assertion_Check")]
        [HttpGet]

        public IEnumerable<risk_db_control_assertion_check> get_Control_Assertion_Check()
        {
            return this.mySqlDBContext.risk_db_control_assertion_checks.Where(d => d.status == "Active");
        }

        //insert Control Assertion Check 
        [Route("api/risk_admin_controller/insert_risk_admin_Control_Assertion_Check")]
        [HttpPost]
        public IActionResult insert_risk_admin_Control_Assertion_Check([FromBody] risk_db_control_assertion_check risk_db_control_assertion_checks)
        {
            try
            {
                risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_name = risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_name?.Trim();

                var ExisteName = this.mySqlDBContext.risk_db_control_assertion_checks.FirstOrDefault(d => d.risk_db_Control_Assertion_Check_name == risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Control Assertion Check Name Already Exist");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_db_control_assertion_checks
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_db_Control_Assertion_Check_id) ?? 0;

                risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_id = maxLawTypeId + 1;
                risk_db_control_assertion_checks.status = "Active";
                risk_db_control_assertion_checks.isImported = "No";
                DateTime d1 = DateTime.Now;
                string d2 = d1.ToString("yyyy-MM-dd HH:mm:ss");
                risk_db_control_assertion_checks.created_date = d2;
                this.mySqlDBContext.risk_db_control_assertion_checks.Add(risk_db_control_assertion_checks);
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:Control Assertion Check Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        //Update Control Assertion Check 

        [Route("api/risk_admin_controller/Update_risk_admin_Control_Assertion_Check")]
        [HttpPut]
        public IActionResult Update_risk_admin_Control_Assertion_Check([FromBody] risk_db_control_assertion_check risk_db_control_assertion_checks)
        {
            try
            {
                if (risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_id == 0)
                {

                    return Ok("Insertion successful");
                }
                else
                {

                    risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_name = risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_name?.Trim();
                    var ExisteName = this.mySqlDBContext.risk_db_control_assertion_checks.FirstOrDefault(d => d.risk_db_Control_Assertion_Check_name == risk_db_control_assertion_checks.risk_db_Control_Assertion_Check_name && d.status == "Active");
                    if (ExisteName != null)
                    {
                        return BadRequest("Error:Control Assertion Check Name Already Exist");
                    }
                    this.mySqlDBContext.Attach(risk_db_control_assertion_checks);
                    this.mySqlDBContext.Entry(risk_db_control_assertion_checks).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_db_control_assertion_checks);
                    Type Data = typeof(risk_db_control_assertion_check);
                    PropertyInfo[] prop = Data.GetProperties();
                    foreach (PropertyInfo p in prop)
                    {
                        if (p.GetValue(risk_db_control_assertion_checks, null) == null || p.GetValue(risk_db_control_assertion_checks, null).Equals(0))
                        {
                            entry.Property(p.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Insertion Successful");
                }
            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error:Control Assertion Check name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete Control Assertion Check 
        [Route("api/risk_admin_controller/delete_risk_admin_Control_Assertion_Check")]
        [HttpDelete]
        public void delete_risk_admin_Control_Assertion_Check(int id)
        {
            var obj = new risk_db_control_assertion_check { risk_db_Control_Assertion_Check_id = id };
            obj.status = "InActive";
            this.mySqlDBContext.Entry(obj).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }



        //get risk_admin_control_reference_type

        [Route("api/risk_admin_controller/get_risk_admin_control_reference_type")]
        [HttpGet]

        public IEnumerable<risk_admin_control_reference_type> get_risk_admin_control_reference_type()
        {
            return this.mySqlDBContext.risk_admin_control_reference_types.Where(d => d.status == "Active");
        }

        //insert risk_admin_control_reference_type
        [Route("api/risk_admin_controller/insert_risk_admin_control_reference_type")]
        [HttpPost]
        public IActionResult insert_risk_admin_control_reference_type([FromBody] risk_admin_control_reference_type risk_admin_control_reference_types)
        {
            try
            {
                risk_admin_control_reference_types.risk_admin_Control_Reference_Type_name = risk_admin_control_reference_types.risk_admin_Control_Reference_Type_name?.Trim();

                var ExisteName = this.mySqlDBContext.risk_admin_control_reference_types.FirstOrDefault(d => d.risk_admin_Control_Reference_Type_name  == risk_admin_control_reference_types.risk_admin_Control_Reference_Type_name && d.status == "Active");
                if (ExisteName != null)
                {
                    return BadRequest("Error:Control Reference Type Name Already Exist");
                }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_control_reference_types
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_Control_Reference_Type_id) ?? 0;

                risk_admin_control_reference_types.risk_admin_Control_Reference_Type_id = maxLawTypeId + 1;
                risk_admin_control_reference_types.status = "Active";
                risk_admin_control_reference_types.isImported = "No";
                DateTime d1 = DateTime.Now;
                string d2 = d1.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_control_reference_types.created_date = d2;
                this.mySqlDBContext.risk_admin_control_reference_types.Add(risk_admin_control_reference_types);
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:Control Reference Type Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }


        //Update risk_admin_control_reference_types

        [Route("api/risk_admin_controller/Update_risk_admin_control_reference_types")]
        [HttpPut]
        public IActionResult Update_risk_admin_control_reference_types([FromBody] risk_admin_control_reference_type risk_admin_control_reference_types)
        {
            try
            {
                if (risk_admin_control_reference_types.risk_admin_Control_Reference_Type_id == 0)
                {

                    return Ok("Insertion successful");
                }
                else
                {

                    risk_admin_control_reference_types.risk_admin_Control_Reference_Type_name = risk_admin_control_reference_types.risk_admin_Control_Reference_Type_name?.Trim();
                    var ExisteName = this.mySqlDBContext.risk_admin_control_reference_types.FirstOrDefault(d => d.risk_admin_Control_Reference_Type_name == risk_admin_control_reference_types.risk_admin_Control_Reference_Type_name && d.status == "Active");
                    if (ExisteName != null)
                    {
                        return BadRequest("Error:Control Reference Type Name Already Exist");
                    }
                    this.mySqlDBContext.Attach(risk_admin_control_reference_types);
                    this.mySqlDBContext.Entry(risk_admin_control_reference_types).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_control_reference_types);
                    Type Data = typeof(risk_admin_control_reference_type);
                    PropertyInfo[] prop = Data.GetProperties();
                    foreach (PropertyInfo p in prop)
                    {
                        if (p.GetValue(risk_admin_control_reference_types, null) == null || p.GetValue(risk_admin_control_reference_types, null).Equals(0))
                        {
                            entry.Property(p.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    return Ok("Insertion Successful");
                }
            }
            catch (DbUpdateException ex)
            {

                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error:Control Reference Type name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Delete risk_admin_control_reference_types
        [Route("api/risk_admin_controller/delete_risk_admin_control_reference_types")]
        [HttpDelete]
        public void delete_risk_admin_control_reference_types(int id)
        {
            var obj = new risk_admin_control_reference_type { risk_admin_Control_Reference_Type_id = id };
            obj.status = "InActive";
            this.mySqlDBContext.Entry(obj).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();

        }


        //Control Acceptance Benchmark


        //get Control Acceptance Benchmark


        [Route("api/risk_admin_controller/Get_ControlAcceptanceBenchmark")]
        [HttpGet]
        public IEnumerable<risk_admin_con_accept_benchmark> Get_ControlAcceptanceBenchmark()
        {
            return this.mySqlDBContext.risk_admin_con_accept_benchmarks.Where(x => x.status == "Active")
                .OrderBy(r => r.risk_admin_con_accept_benchmark_min_level)
                .ToList();
        }


        //control Control Acceptance Benchmark
        [Route("api/risk_admin_controller/Insert_ControlAcceptanceBenchmark")]
        [HttpPost]
        public async Task< IActionResult> Insert_ControlAcceptanceBenchmark([FromBody] risk_admin_con_accept_benchmark risk_admin_con_accept_benchmarks)
        {
            try
            {
                risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_name = risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_name?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_admin_con_accept_benchmarks
                    .FirstOrDefault(d => d.risk_admin_con_accept_benchmark_name == risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_name && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Acceptance Benchmark Name with the same name already exists.");
                }
                if (risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level > risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level)
                {
                    return BadRequest("Error: Risk Benchmark Minimum value is greater that  Risk Benchmark Maximum Value.");
                }
                //if (!CheckValidation(risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level, risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level, risk_admin_con_accept_benchmarks.array))
                //{
                //    return BadRequest("Error: Control Acceptance Benchmark Range Not Valid.");
                //}
                var existingColor = this.mySqlDBContext.risk_admin_con_accept_benchmarks
                   .FirstOrDefault(d => d.risk_admin_con_accept_benchmark_color_code == risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_color_code && d.status == "Active");

                if (existingColor != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Acceptance Benchmark colour with the same colour already exists.");
                }
                // Set min and max using either the provided values or the existing ones.
                var newMin = risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level ;
                var newMax = risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level ;



                var existingRanges = await mySqlDBContext.risk_admin_con_accept_benchmarks
                    .Where(d => d.risk_admin_con_accept_benchmark_id != risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_id && d.status == "Active")
                    .Select(d => new { MinValue = d.risk_admin_con_accept_benchmark_min_level, MaxValue = d.risk_admin_con_accept_benchmark_max_level })
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
                    if (risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level != null &&
                          risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level != null )
                    {

                        if (risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level > risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                        }
                    }
                var maxLawTypeId = this.mySqlDBContext.risk_admin_con_accept_benchmarks
.Where(d => d.isImported == "No")
.Max(d => (int?)d.risk_admin_con_accept_benchmark_id) ?? 0;

                risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_id = maxLawTypeId + 1;
                // Proceed with the insertion
                var residual_risk_rating = this.mySqlDBContext.risk_admin_con_accept_benchmarks;
                residual_risk_rating.Add(risk_admin_con_accept_benchmarks);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_con_accept_benchmarks.created_date = dt1;
                risk_admin_con_accept_benchmarks.isImported = "No";
                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_admin_con_accept_benchmarks.status = "Active";
              


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Control Acceptance Benchmark with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update Control Acceptance Benchmark

        [Route("api/risk_admin_controller/Update_ControlAcceptanceBenchmark")]
        [HttpPut]
        public async Task< IActionResult> Update_ControlAcceptanceBenchmark([FromBody] risk_admin_con_accept_benchmark risk_admin_con_accept_benchmarks)

        {
            try
            {
                if (risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_name = risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_admin_con_accept_benchmarks
                     .FirstOrDefault(d => d.risk_admin_con_accept_benchmark_name == risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_name && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Control Acceptance Benchmark with the same name already exists.");
                    }
                    
                    var existingColor = this.mySqlDBContext.risk_admin_con_accept_benchmarks
                  .FirstOrDefault(d => d.risk_admin_con_accept_benchmark_color_code == risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_color_code && d.status == "Active");

                    if (existingColor != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Control Acceptance Benchmark colour with the same colour already exists.");
                    }





                    var existingValues = await mySqlDBContext.risk_admin_con_accept_benchmarks
.Where(x => x.risk_admin_con_accept_benchmark_id == risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_id)
.Select(x => new
{
    MinValue = x.risk_admin_con_accept_benchmark_min_level,
    MaxValue = x.risk_admin_con_accept_benchmark_max_level
})
.FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level ?? existingValues.MinValue;
                    var newMax = risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level ?? existingValues.MaxValue;



                    var existingRanges = await mySqlDBContext.risk_admin_con_accept_benchmarks
                        .Where(d => d.risk_admin_con_accept_benchmark_id != risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_id && d.status == "Active")
                        .Select(d => new { MinValue = d.risk_admin_con_accept_benchmark_min_level, MaxValue = d.risk_admin_con_accept_benchmark_max_level })
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
                        if (risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level == null &&
                              risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level != null &&
                            risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_max_level == null)
                        {


                            if (risk_admin_con_accept_benchmarks.risk_admin_con_accept_benchmark_min_level > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }



                    // Existing department, update logic
                    this.mySqlDBContext.Attach(risk_admin_con_accept_benchmarks);
                    this.mySqlDBContext.Entry(risk_admin_con_accept_benchmarks).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_admin_con_accept_benchmarks);

                    Type type = typeof(risk_admin_con_accept_benchmark);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_admin_con_accept_benchmarks, null) == null || property.GetValue(risk_admin_con_accept_benchmarks, null).Equals(0))
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
                    return BadRequest("Error:  Control Acceptance Benchmark with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }


        //Delete Control Acceptance Benchmark  

        [Route("api/risk_admin_controller/Delete_ControlAcceptanceBenchmark")]
        [HttpDelete]
        public void Delete_ControlAcceptanceBenchmark(int id)
        {
            var currentClass = new risk_admin_con_accept_benchmark { risk_admin_con_accept_benchmark_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
}
