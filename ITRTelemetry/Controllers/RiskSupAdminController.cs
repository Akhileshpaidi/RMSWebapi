using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySqlConnector;
using MySqlException = MySql.Data.MySqlClient.MySqlException;
using static DomainModel.control_activity_type;
using DocumentFormat.OpenXml.Office2019.Excel.RichData2;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RiskSupAdminController : ControllerBase
    {
        private readonly CommonDBContext mySqlDBContext;
        private object random;

        public IConfiguration Configuration { get; }
        public RiskSupAdminController(CommonDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;

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



      
    


















        //Risk Priority

        [Route("api/RiskSupAdminController/GetRiskPriority")]
        [HttpGet]
        public IEnumerable<RiskSupAdminModel> GetRiskPriority()
        {
            return this.mySqlDBContext.RiskSupAdminModels.Where(x => x.status == "Active")
                .OrderBy(r => r.rating_level_min)
                .ToList();
        }


        //Risk Priority Insert Method
        [Route("api/RiskSupAdminController/InsertRiskPriority")]
        [HttpPost]
        public async Task< IActionResult> InsertRiskPriority([FromBody] RiskSupAdminModel RiskSupAdminModels)
        {
            try
            {
                RiskSupAdminModels.risk_priority_name = RiskSupAdminModels.risk_priority_name?.Trim();

                var existingDepartment = this.mySqlDBContext.RiskSupAdminModels
                    .FirstOrDefault(d => d.risk_priority_name == RiskSupAdminModels.risk_priority_name && d.risk_priority_id != RiskSupAdminModels.risk_priority_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Priority with the same name already exists.");
                }
                if (RiskSupAdminModels.rating_level_min == null && RiskSupAdminModels.rating_level_max == null)
                {
                    return BadRequest("Error: Risk Rating Minimum value and  Maximum Values are null.");
                } 
                if (RiskSupAdminModels.rating_level_min > RiskSupAdminModels.rating_level_max)
                {
                    return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                }

                var existingColor = this.mySqlDBContext.RiskSupAdminModels
                    .FirstOrDefault(d => d.color_code == RiskSupAdminModels.color_code && d.risk_priority_id != RiskSupAdminModels.risk_priority_id && d.status == "Active");

                if (existingColor != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Priority with the same colour already exists.");
                }
                //if (!CheckValidation(RiskSupAdminModels.rating_level_min, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                //{
                //    return BadRequest("Error: Risk Priority Rating Range Not Valid.");
                //}
                var newMin = RiskSupAdminModels.rating_level_min ;
                var newMax = RiskSupAdminModels.rating_level_max;
                var existingRanges = await mySqlDBContext.RiskSupAdminModels
                    .Where(d => d.risk_priority_id != RiskSupAdminModels.risk_priority_id && d.status == "Active")
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
                    if (RiskSupAdminModels.rating_level_min != null &&
                        RiskSupAdminModels.rating_level_max != null)
                    {
               
                        if (RiskSupAdminModels.rating_level_min > RiskSupAdminModels.rating_level_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                        }
                    }

              

                var AuthorityNameModel = this.mySqlDBContext.RiskSupAdminModels;
                AuthorityNameModel.Add(RiskSupAdminModels);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskSupAdminModels.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskSupAdminModels.status = "Active";


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

        [Route("api/RiskSupAdminController/UpdateRiskPriority")]
        [HttpPut]
        public async Task<IActionResult> UpdateRiskPriority([FromBody] RiskSupAdminModel RiskSupAdminModels)
        {
            try
            {
                if (RiskSupAdminModels.risk_priority_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion Successful");
                }
                else
                {
                    RiskSupAdminModels.risk_priority_name = RiskSupAdminModels.risk_priority_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.RiskSupAdminModels
                     .FirstOrDefault(d => d.risk_priority_name == RiskSupAdminModels.risk_priority_name && d.risk_priority_id != RiskSupAdminModels.risk_priority_id && d.risk_priority_id != RiskSupAdminModels.risk_priority_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:  Risk Priority name already exists.");
                    }
                    var existingColor = this.mySqlDBContext.RiskSupAdminModels
                 .FirstOrDefault(d => d.color_code == RiskSupAdminModels.color_code && d.risk_priority_id != RiskSupAdminModels.risk_priority_id && d.status == "Active");

                    if (existingColor != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Priority with the same colour already exists.");
                    }


                    if (RiskSupAdminModels.rating_level_min > RiskSupAdminModels.rating_level_max)
                    {
                        return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                    }



                    var existingValues = await mySqlDBContext.RiskSupAdminModels
     .Where(x => x.risk_priority_id == RiskSupAdminModels.risk_priority_id)
     .Select(x => new
     {
         MinValue = x.rating_level_min,
         MaxValue = x.rating_level_max
     })
     .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = RiskSupAdminModels.rating_level_min ?? existingValues.MinValue;
                    var newMax = RiskSupAdminModels.rating_level_max ?? existingValues.MaxValue;

                    // Ensure min is not greater than max.
                   


                    var existingRanges = await mySqlDBContext.RiskSupAdminModels
                        .Where(d => d.risk_priority_id != RiskSupAdminModels.risk_priority_id && d.status=="Active")
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
                            if (RiskSupAdminModels.rating_level_min == null &&
                                RiskSupAdminModels.rating_level_max != null &&
                                existingValues.MinValue != 0)
                            {
                                //if (!CheckValidation(existingValues.MinValue, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                                //{
                                //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                                //}
                                if (existingValues.MinValue > RiskSupAdminModels.rating_level_max)
                                {
                                    return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                                }
                            }

                            if (RiskSupAdminModels.rating_level_min != null &&
                                RiskSupAdminModels.rating_level_max == null)
                            {


                                if (RiskSupAdminModels.rating_level_min > existingValues.MaxValue)
                                {
                                    return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                                }

                                //if (!CheckValidation(RiskSupAdminModels.rating_level_min, existingValues.MaxValue, RiskSupAdminModels.array))
                                //{
                                //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                                //}

                            }
                        }

                        //if (RiskSupAdminModels.rating_level_min != null && RiskSupAdminModels.rating_level_max != null)
                        //{
                        //    if (!CheckValidation(RiskSupAdminModels.rating_level_min, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                        //    {
                        //        return BadRequest("Error: Risk Priority Rating Range Not Valid.");
                        //    }
                        //}


                        // Existing department, update logic
                        this.mySqlDBContext.Attach(RiskSupAdminModels);
                        this.mySqlDBContext.Entry(RiskSupAdminModels).State = EntityState.Modified;

                        var entry = this.mySqlDBContext.Entry(RiskSupAdminModels);

                        Type type = typeof(RiskSupAdminModel);
                        PropertyInfo[] properties = type.GetProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetValue(RiskSupAdminModels, null) == null || property.GetValue(RiskSupAdminModels, null).Equals(0))
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

        [Route("api/RiskSupAdminController/DeleteRiskPriority")]
        [HttpDelete]
        public void DeleteRiskPriority(int id)
        {
            var currentClass = new RiskSupAdminModel { risk_priority_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }





        //Potentialbusinessimpact business impact



        [Route("api/RiskSupAdminController/GetPotentialbusinessimpact")]
        [HttpGet]
        public IEnumerable<potential_business_impact> GetPotentialbusinessimpact()
        {
            return this.mySqlDBContext.potential_business_impacts.Where(x => x.status == "Active")
                .OrderBy(r => r.potential_business_impact_id)
                .ToList();
        }


        //Risk Potentialbusinessimpact Insert Method
        [Route("api/RiskSupAdminController/InsertPotentialbusinessimpact")]
        [HttpPost]
        public IActionResult InsertPotentialbusinessimpact([FromBody] potential_business_impact potential_business_impacts)
        {
            try
            {
                potential_business_impacts.potential_business_impact_name = potential_business_impacts.potential_business_impact_name?.Trim();

                var existingDepartment = this.mySqlDBContext.potential_business_impacts
                    .FirstOrDefault(d => d.potential_business_impact_name == potential_business_impacts.potential_business_impact_name && d.potential_business_impact_id != potential_business_impacts.potential_business_impact_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Potential Business Impact with the same name already exists.");
                }
                // Proceed with the insertion
                var AuthorityNameModel = this.mySqlDBContext.potential_business_impacts;
                AuthorityNameModel.Add(potential_business_impacts);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                potential_business_impacts.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                potential_business_impacts.status = "Active";
                if (potential_business_impacts.potential_business_impact_show_des == null)
                {
                    potential_business_impacts.potential_business_impact_show_des = "No";
                }


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Potential Business Impact with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }



        //Update Potentialbusinessimpact method

        [Route("api/RiskSupAdminController/UpdatePotentialbusinessimpact")]
        [HttpPut]
        public IActionResult UpdatePotentialbusinessimpact([FromBody] potential_business_impact potential_business_impacts)
        {
            try
            {
                if (potential_business_impacts.potential_business_impact_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    potential_business_impacts.potential_business_impact_name = potential_business_impacts.potential_business_impact_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.potential_business_impacts
                     .FirstOrDefault(d => d.potential_business_impact_name == potential_business_impacts.potential_business_impact_name && d.potential_business_impact_id != potential_business_impacts.potential_business_impact_id && d.potential_business_impact_id != potential_business_impacts.potential_business_impact_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Potential Business Impact with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(potential_business_impacts);
                    this.mySqlDBContext.Entry(potential_business_impacts).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(potential_business_impacts);

                    Type type = typeof(potential_business_impact);
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
                    return BadRequest("Error: Potential Business Impact with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }



        //Delete Potentialbusinessimpact Method

        [Route("api/RiskSupAdminController/DeletePotentialbusinessimpact")]
        [HttpDelete]
        public void DeletePotentialbusinessimpact(int id)
        {
            var currentClass = new potential_business_impact { potential_business_impact_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //loss event threat category(l1)



        [Route("api/RiskSupAdminController/Getloss_event_threat_category")]
        [HttpGet]
        public IEnumerable<loss_event_threat_category> Getloss_event_threat_category()
        {
            return this.mySqlDBContext.loss_event_threat_categorys.Where(x => x.status == "Active")
                .OrderBy(r => r.Loss_Event_Threat_Category_id)
                .ToList();
        }


        //loss event threat category Insert Method
        [Route("api/RiskSupAdminController/Insertloss_event_threat_category")]
        [HttpPost]
        public IActionResult Insertloss_event_threat_category([FromBody] loss_event_threat_category loss_event_threat_categorys)
        {
            try
            {
                loss_event_threat_categorys.Loss_Event_Threat_Category_Name = loss_event_threat_categorys.Loss_Event_Threat_Category_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.loss_event_threat_categorys
                    .FirstOrDefault(d => d.Loss_Event_Threat_Category_Name == loss_event_threat_categorys.Loss_Event_Threat_Category_Name && d.Loss_Event_Threat_Category_id != loss_event_threat_categorys.Loss_Event_Threat_Category_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:Loss Event Threat Category L1 Name with the same name already exists.");
                }
                // Proceed with the insertion
                var loss_event_threat_category = this.mySqlDBContext.loss_event_threat_categorys;
                loss_event_threat_category.Add(loss_event_threat_categorys);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                loss_event_threat_categorys.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                loss_event_threat_categorys.status = "Active";

                if (loss_event_threat_categorys.Loss_Event_Threat_Category_show_desc == null)
                {
                    loss_event_threat_categorys.Loss_Event_Threat_Category_show_desc = "No";
                }

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

        [Route("api/RiskSupAdminController/Updateloss_event_threat_category")]
        [HttpPut]
        public IActionResult Updateloss_event_threat_category([FromBody] loss_event_threat_category loss_event_threat_categorys)
        {
            try
            {
                if (loss_event_threat_categorys.Loss_Event_Threat_Category_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    loss_event_threat_categorys.Loss_Event_Threat_Category_Name = loss_event_threat_categorys.Loss_Event_Threat_Category_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.loss_event_threat_categorys
                     .FirstOrDefault(d => d.Loss_Event_Threat_Category_Name == loss_event_threat_categorys.Loss_Event_Threat_Category_Name && d.Loss_Event_Threat_Category_id != loss_event_threat_categorys.Loss_Event_Threat_Category_id && d.Loss_Event_Threat_Category_id != loss_event_threat_categorys.Loss_Event_Threat_Category_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Loss Event Threat Category L1 Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(loss_event_threat_categorys);
                    this.mySqlDBContext.Entry(loss_event_threat_categorys).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(loss_event_threat_categorys);

                    Type type = typeof(loss_event_threat_category);
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

        [Route("api/RiskSupAdminController/Deleteloss_event_threat_category")]
        [HttpDelete]
        public void Deleteloss_event_threat_category(int id)
        {
            var currentClass = new loss_event_threat_category { Loss_Event_Threat_Category_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        // loss_event_threat_category_l2 



        [Route("api/RiskSupAdminController/Getloss_event_threat_category_l2")]
        [HttpGet]
        public IEnumerable<losseventthreacategory_l2> Getloss_event_threat_category_l2()
        {
            return this.mySqlDBContext.losseventthreacategory_l2s.Where(x => x.status == "Active")
                .OrderBy(r => r.lossEventThreaCategory_L2_id)
                .ToList();
        }


        //loss event threat category Insert Method
        [Route("api/RiskSupAdminController/Insertloss_event_threat_category_l2")]
        [HttpPost]
        public IActionResult Insertloss_event_threat_category_l2([FromBody] losseventthreacategory_l2 losseventthreacategory_l2s)
        {
            try
            {
                losseventthreacategory_l2s.lossEventThreaCategory_L2_Name = losseventthreacategory_l2s.lossEventThreaCategory_L2_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.losseventthreacategory_l2s
                    .FirstOrDefault(d => d.lossEventThreaCategory_L2_Name == losseventthreacategory_l2s.lossEventThreaCategory_L2_Name && d.lossEventThreaCategory_L2_id != losseventthreacategory_l2s.lossEventThreaCategory_L2_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:  Loss Event Threat Category L2 Name with the same name already exists.");
                }
                // Proceed with the insertion
                var losseventthreacategory_l2 = this.mySqlDBContext.losseventthreacategory_l2s;
                losseventthreacategory_l2.Add(losseventthreacategory_l2s);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l2s.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l2s.status = "Active";
                if (losseventthreacategory_l2s.lossEventThreaCategory_L2_show_des == null)
                {
                    losseventthreacategory_l2s.lossEventThreaCategory_L2_show_des = "No";
                }

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

        [Route("api/RiskSupAdminController/Updateloss_event_threat_category_l2")]
        [HttpPut]
        public IActionResult Updateloss_event_threat_category_l2([FromBody] losseventthreacategory_l2 losseventthreacategory_l2s)
        {
            try
            {
                if (losseventthreacategory_l2s.lossEventThreaCategory_L2_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    losseventthreacategory_l2s.lossEventThreaCategory_L2_Name = losseventthreacategory_l2s.lossEventThreaCategory_L2_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.losseventthreacategory_l2s
                     .FirstOrDefault(d => d.lossEventThreaCategory_L2_Name == losseventthreacategory_l2s.lossEventThreaCategory_L2_Name && d.lossEventThreaCategory_L2_id != losseventthreacategory_l2s.lossEventThreaCategory_L2_id && d.lossEventThreaCategory_L2_id != losseventthreacategory_l2s.lossEventThreaCategory_L2_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Loss Event Threat Category L2 Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(losseventthreacategory_l2s);
                    this.mySqlDBContext.Entry(losseventthreacategory_l2s).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(losseventthreacategory_l2s);

                    Type type = typeof(losseventthreacategory_l2);
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

        [Route("api/RiskSupAdminController/Deleteloss_event_threat_category_l2")]
        [HttpDelete]
        public void Deleteloss_event_threat_category_l2(int id)
        {
            var currentClass = new losseventthreacategory_l2 { lossEventThreaCategory_L2_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }




        // loss_event_threat_category_l3

        [Route("api/RiskSupAdminController/Getloss_event_threat_category_l3")]
        [HttpGet]
        public IEnumerable<losseventthreacategory_l3> Getloss_event_threat_category_l3()
        {
            return this.mySqlDBContext.losseventthreacategory_l3s.Where(x => x.status == "Active")
                .OrderBy(r => r.lossEventThreaCategory_L3_id)
                .ToList();
        }


        //loss event threat category l3 Insert Method
        [Route("api/RiskSupAdminController/Insertloss_event_threat_category_l3")]
        [HttpPost]
        public IActionResult Insertloss_event_threat_category_l3([FromBody] losseventthreacategory_l3 losseventthreacategory_l3s)
        {
            try
            {
                losseventthreacategory_l3s.lossEventThreaCategory_L3_Name = losseventthreacategory_l3s.lossEventThreaCategory_L3_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.losseventthreacategory_l3s
                    .FirstOrDefault(d => d.lossEventThreaCategory_L3_Name == losseventthreacategory_l3s.lossEventThreaCategory_L3_Name && d.lossEventThreaCategory_L3_id != losseventthreacategory_l3s.lossEventThreaCategory_L3_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:  Loss Event Threat Category L3 Name with the same name already exists.");
                }
                // Proceed with the insertion
                var losseventthreacategory_l3 = this.mySqlDBContext.losseventthreacategory_l3s;
                losseventthreacategory_l3.Add(losseventthreacategory_l3s);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l3s.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                losseventthreacategory_l3s.status = "Active";
                if (losseventthreacategory_l3s.lossEventThreaCategory_L3_show_des == null)
                {
                    losseventthreacategory_l3s.lossEventThreaCategory_L3_show_des = "No";
                }

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

        [Route("api/RiskSupAdminController/Updateloss_event_threat_category_l3")]
        [HttpPut]
        public IActionResult Updateloss_event_threat_category_l3([FromBody] losseventthreacategory_l3 losseventthreacategory_l3s)
        {
            try
            {
                if (losseventthreacategory_l3s.lossEventThreaCategory_L3_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    losseventthreacategory_l3s.lossEventThreaCategory_L3_Name = losseventthreacategory_l3s.lossEventThreaCategory_L3_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.losseventthreacategory_l3s
                     .FirstOrDefault(d => d.lossEventThreaCategory_L3_Name == losseventthreacategory_l3s.lossEventThreaCategory_L3_Name && d.lossEventThreaCategory_L3_id != losseventthreacategory_l3s.lossEventThreaCategory_L3_id && d.lossEventThreaCategory_L3_id != losseventthreacategory_l3s.lossEventThreaCategory_L3_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:  Loss Event Threat Category L3 Name with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(losseventthreacategory_l3s);
                    this.mySqlDBContext.Entry(losseventthreacategory_l3s).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(losseventthreacategory_l3s);

                    Type type = typeof(losseventthreacategory_l3);
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

        [Route("api/RiskSupAdminController/Deleteloss_event_threat_category_l3")]
        [HttpDelete]
        public void Deleteloss_event_threat_category_l3(int id)
        {
            var currentClass = new losseventthreacategory_l3 { lossEventThreaCategory_L3_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }






        //control_measure master page

        //get control_measure

        [Route("api/RiskSupAdminController/getcontrol_measureFUN")]
        [HttpGet]

        public IEnumerable<getcontrol_measure> getcontrol_measureFUN()
        {
            return this.mySqlDBContext.getcontrol_measures.Where(x => x.status == "active").OrderBy(x => x.control_measure_id).ToList();


        }

        // Insert control_measure

        [Route("api/RiskSupAdminController/insert_control_measure")]
        [HttpPost]

        public IActionResult insert_control_measure([FromBody] getcontrol_measure getcontrol_measures)
        {
            try
            {
                getcontrol_measures.control_measure_name = getcontrol_measures.control_measure_name?.Trim();

                var existing_name = this.mySqlDBContext.getcontrol_measures
                    .FirstOrDefault(x => x.control_measure_name == getcontrol_measures.control_measure_name && x.status == "Active");

                if (existing_name != null && existing_name.control_measure_id != getcontrol_measures.control_measure_id)
                {
                    return BadRequest("Error: Name Already Exists");
                }

                getcontrol_measures.created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                getcontrol_measures.status = "Active";

                // this.mySqlDBContext.getcontrol_measures.Add(getcontrol_measures);

                this.mySqlDBContext.SaveChanges();

                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error: Loss Event Threat Category L3 Name with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        // Update control_measure

        [Route("api/RiskSupAdminController/updatecontrol_measure")]
        [HttpPut]

        public IActionResult updatecontrol_measure([FromBody] getcontrol_measure getcontrol_measures)
        {
            try
            {

                if (getcontrol_measures.control_measure_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    getcontrol_measures.control_measure_name = getcontrol_measures.control_measure_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.getcontrol_measures
                     .FirstOrDefault(d => d.control_measure_name == getcontrol_measures.control_measure_name && d.control_measure_id != getcontrol_measures.control_measure_id && d.control_measure_id != getcontrol_measures.control_measure_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(getcontrol_measures);
                    this.mySqlDBContext.Entry(getcontrol_measures).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(getcontrol_measures);

                    Type type = typeof(getcontrol_measure);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(getcontrol_measures, null) == null || property.GetValue(getcontrol_measures, null).Equals(0))
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



        //Delete control_measure

        [Route("api/RiskSupAdminController/deletecontrol_measure")]
        [HttpDelete]
        public void deletecontrol_measure(int id)
        {
            var currentClass = new getcontrol_measure { control_measure_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //control_activity_type master page

        //get control_activity_type

        [Route("api/RiskSupAdminController/getcontrol_activity_type")]
        [HttpGet]

        public IEnumerable<control_activity_type> getcontrol_activity_type()
        {
            return this.mySqlDBContext.control_activity_types.Where(x => x.status == "Active").OrderBy(x => x.control_activity_type_id).ToList();


        }

        // Insert control_activity_type

        [Route("api/RiskSupAdminController/insertcontrol_activity_type")]
        [HttpPost]

        public IActionResult insertcontrol_activity_type([FromBody] control_activity_type control_activity_types)
        {
            try
            {
                control_activity_types.control_activity_type_name = control_activity_types.control_activity_type_name?.Trim();
                var existing_name = this.mySqlDBContext.control_activity_types.FirstOrDefault(x => x.control_activity_type_name == control_activity_types.control_activity_type_name && x.status == "Active" && x.control_activity_type_id != control_activity_types.control_activity_type_id);
                if (existing_name != null)
                {
                    return BadRequest("Error:Name Already Exist");

                }

                var data = this.mySqlDBContext.control_activity_types;
                data.Add(control_activity_types);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                control_activity_types.created_date = dt1;
                control_activity_types.status = "Active";
                //  this.mySqlDBContext.control_activity_types.Add(control_activity_types);
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


        // Update control_activity_type

        [Route("api/RiskSupAdminController/updatecontrol_activity_type")]
        [HttpPut]

        public IActionResult updatecontrol_activity_type([FromBody] control_activity_type control_activity_types)
        {
            try
            {

                if (control_activity_types.control_activity_type_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    control_activity_types.control_activity_type_name = control_activity_types.control_activity_type_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.control_activity_types
                     .FirstOrDefault(d => d.control_activity_type_name == control_activity_types.control_activity_type_name && d.control_activity_type_id != control_activity_types.control_activity_type_id && d.control_activity_type_id != control_activity_types.control_activity_type_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(control_activity_types);
                    this.mySqlDBContext.Entry(control_activity_types).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(control_activity_types);

                    Type type = typeof(control_activity_type);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(control_activity_types, null) == null || property.GetValue(control_activity_types, null).Equals(0))
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



        //Delete control_activity_type

        [Route("api/RiskSupAdminController/deletecontrol_activity_type")]
        [HttpDelete]
        public void deletecontrol_activity_type(int id)
        {
            var currentClass = new control_activity_type { control_activity_type_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //control_reference_type master page

        //get control_reference_type

        [Route("api/RiskSupAdminController/getcontrol_reference_type")]
        [HttpGet]

        public IEnumerable<control_reference_type> getcontrol_reference_type()
        {
            return this.mySqlDBContext.control_reference_types.Where(x => x.status == "Active").OrderBy(x => x.control_reference_type_id).ToList();


        }

        // Insert control_reference_type

        [Route("api/RiskSupAdminController/insertcontrol_reference_type")]
        [HttpPost]

        public IActionResult insertcontrol_reference_type([FromBody] control_reference_type control_reference_types)
        {
            try
            {
                control_reference_types.control_reference_type_name = control_reference_types.control_reference_type_name?.Trim();
                var existing_name = this.mySqlDBContext.control_reference_types.FirstOrDefault(x => x.control_reference_type_name == control_reference_types.control_reference_type_name && x.status == "Active" && x.control_reference_type_id != control_reference_types.control_reference_type_id);
                if (existing_name != null)
                {
                    return BadRequest("Error:Name Already Exist");

                }

                var data = this.mySqlDBContext.control_reference_types;
                data.Add(control_reference_types);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                control_reference_types.created_date = dt1;
                control_reference_types.status = "Active";

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


        // Update control_reference_type

        [Route("api/RiskSupAdminController/updatecontrol_reference_type")]
        [HttpPut]

        public IActionResult updatecontrol_reference_type([FromBody] control_reference_type control_reference_types)
        {
            try
            {

                if (control_reference_types.control_reference_type_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    control_reference_types.control_reference_type_name = control_reference_types.control_reference_type_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.control_reference_types
                     .FirstOrDefault(d => d.control_reference_type_name == control_reference_types.control_reference_type_name && d.control_reference_type_id != control_reference_types.control_reference_type_id && d.control_reference_type_id != control_reference_types.control_reference_type_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(control_reference_types);
                    this.mySqlDBContext.Entry(control_reference_types).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(control_reference_types);

                    Type type = typeof(control_reference_type);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(control_reference_types, null) == null || property.GetValue(control_reference_types, null).Equals(0))
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



        //Delete control_reference_type

        [Route("api/RiskSupAdminController/deletecontrol_reference_type")]
        [HttpDelete]
        public void deletecontrol_reference_type(int id)
        {
            var currentClass = new control_reference_type { control_reference_type_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }





        //Mitigation Control Default Settings


        //get Mitigation Action Required 

        [Route("api/RiskSupAdminController/getmitigation_action")]
        [HttpGet]

        public IEnumerable<mitigation_action> getmitigation_action()
        {
            return this.mySqlDBContext.mitigation_actions.Where(x => x.status == "Active").OrderBy(x => x.mitigation_action_id).ToList();


        }

        // Insert Mitigation Action Required 

        [Route("api/RiskSupAdminController/insertmitigation_action")]
        [HttpPost]

        public IActionResult insertmitigation_action([FromBody] mitigation_action mitigation_actions)
        {
            try
            {
                mitigation_actions.mitigation_action_name = mitigation_actions.mitigation_action_name?.Trim();
                var existing_name = this.mySqlDBContext.mitigation_actions.FirstOrDefault(x => x.mitigation_action_name == mitigation_actions.mitigation_action_name && x.status == "Active" && x.mitigation_action_id != mitigation_actions.mitigation_action_id);
                if (existing_name != null)
                {
                    return BadRequest("Error:Name Already Exist");

                }

                var data = this.mySqlDBContext.mitigation_actions;
                data.Add(mitigation_actions);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                mitigation_actions.created_date = dt1;
                mitigation_actions.status = "Active";

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

        [Route("api/RiskSupAdminController/updatemitigation_action")]
        [HttpPut]

        public IActionResult updatemitigation_action([FromBody] mitigation_action mitigation_actions)
        {
            try
            {

                if (mitigation_actions.mitigation_action_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    mitigation_actions.mitigation_action_name = mitigation_actions.mitigation_action_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.mitigation_actions
                     .FirstOrDefault(d => d.mitigation_action_name == mitigation_actions.mitigation_action_name && d.mitigation_action_id != mitigation_actions.mitigation_action_id && d.mitigation_action_id != mitigation_actions.mitigation_action_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(mitigation_actions);
                    this.mySqlDBContext.Entry(mitigation_actions).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(mitigation_actions);

                    Type type = typeof(mitigation_action);
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

        [Route("api/RiskSupAdminController/deletemitigation_action")]
        [HttpDelete]
        public void deletemitigation_action(int id)
        {
            var currentClass = new mitigation_action { mitigation_action_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }






        //get Action Priority List


        [Route("api/RiskSupAdminController/getaction_priority_list")]
        [HttpGet]

        public IEnumerable<action_priority_list> getaction_priority_list()
        {
            return this.mySqlDBContext.action_priority_lists.Where(x => x.status == "Active").OrderBy(x => x.action_priority_list_id).ToList();


        }

        // Insert Action Priority List


        [Route("api/RiskSupAdminController/insertaction_priority_list")]
        [HttpPost]

        public IActionResult insertmitigation_action([FromBody] action_priority_list action_priority_lists)
        {
            try
            {
                action_priority_lists.action_priority_list_name = action_priority_lists.action_priority_list_name?.Trim();
                var existing_name = this.mySqlDBContext.action_priority_lists.FirstOrDefault(x => x.action_priority_list_name == action_priority_lists.action_priority_list_name && x.status == "Active" && x.action_priority_list_id != action_priority_lists.action_priority_list_id);
                if (existing_name != null)
                {
                    return BadRequest("Error:Name Already Exist");

                }

                var data = this.mySqlDBContext.action_priority_lists;
                data.Add(action_priority_lists);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                action_priority_lists.created_date = dt1;
                action_priority_lists.status = "Active";

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


        [Route("api/RiskSupAdminController/updateaction_priority_list")]
        [HttpPut]

        public IActionResult updateaction_priority_list([FromBody] action_priority_list action_priority_lists)
        {
            try
            {

                if (action_priority_lists.action_priority_list_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    action_priority_lists.action_priority_list_name = action_priority_lists.action_priority_list_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.action_priority_lists
                     .FirstOrDefault(d => d.action_priority_list_name == action_priority_lists.action_priority_list_name && d.action_priority_list_id != action_priority_lists.action_priority_list_id && d.action_priority_list_id != action_priority_lists.action_priority_list_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(action_priority_lists);
                    this.mySqlDBContext.Entry(action_priority_lists).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(action_priority_lists);

                    Type type = typeof(action_priority_list);
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


        [Route("api/RiskSupAdminController/deleteaction_priority_lists")]
        [HttpDelete]
        public void deleteaction_priority_lists(int id)
        {
            var currentClass = new action_priority_list { action_priority_list_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //control risk of assessment

        //get control risk of assessment


        [Route("api/RiskSupAdminController/Get_control_risk_of_assessment")]
        [HttpGet]
        public IEnumerable<control_risk_of_assessment> Get_control_risk_of_assessment()
        {
            return this.mySqlDBContext.control_risk_of_assessments.Where(x => x.status == "Active")
                .OrderBy(r => r.control_risk_of_assessment_range_min)
                .ToList();
        }


        //control risk of assessment Insert Method
        [Route("api/RiskSupAdminController/Insert_control_risk_of_assessment")]
        [HttpPost]
        public async Task< IActionResult> Insert_control_risk_of_assessment([FromBody] control_risk_of_assessment control_risk_of_assessments)
        {
            try
            {
                control_risk_of_assessments.control_risk_of_assessment_name = control_risk_of_assessments.control_risk_of_assessment_name?.Trim();

                var existingDepartment = this.mySqlDBContext.control_risk_of_assessments
                    .FirstOrDefault(d => d.control_risk_of_assessment_name == control_risk_of_assessments.control_risk_of_assessment_name && d.control_risk_of_assessment_id != control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Risk of Assessment Name with the same name already exists.");
                }

                if (control_risk_of_assessments.control_risk_of_assessment_range_min > control_risk_of_assessments.control_risk_of_assessment_range_max)
                {
                    return BadRequest("Error: Risk Control Assessment Minimum value is greater that  Risk Control Assessment Maximum Value.");
                }
                //if (!CheckValidation(control_risk_of_assessments.control_risk_of_assessment_range_min, control_risk_of_assessments.control_risk_of_assessment_range_max, control_risk_of_assessments.array))
                //{
                //    return BadRequest("Error: Control Risk of Assessment Range Not Valid.");
                //}
                var existingColor = this.mySqlDBContext.control_risk_of_assessments
                   .FirstOrDefault(d => d.color_code == control_risk_of_assessments.color_code && d.control_risk_of_assessment_id != control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                if (existingColor != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Risk of Assessment colour with the same colour already exists.");
                }

                // Set min and max using either the provided values or the existing ones.
                var newMin = control_risk_of_assessments.control_risk_of_assessment_range_min;
                var newMax = control_risk_of_assessments.control_risk_of_assessment_range_max ;



                var existingRanges = await mySqlDBContext.control_risk_of_assessments
                    .Where(d => d.control_risk_of_assessment_id != control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active")
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
                    if (control_risk_of_assessments.control_risk_of_assessment_range_min != null &&
                        control_risk_of_assessments.control_risk_of_assessment_range_max != null )
                    {
                        //if (!CheckValidation(existingValues.MinValue, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                        //{
                        //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                        //}
                        if (control_risk_of_assessments.control_risk_of_assessment_range_min > control_risk_of_assessments.control_risk_of_assessment_range_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                        }
                    }

                    // Proceed with the insertion
                    var residual_risk_rating = this.mySqlDBContext.control_risk_of_assessments;
                residual_risk_rating.Add(control_risk_of_assessments);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                control_risk_of_assessments.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                control_risk_of_assessments.status = "Active";


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

        [Route("api/RiskSupAdminController/Update_control_risk_of_assessment")]
        [HttpPut]
        public async Task< IActionResult> Update_control_risk_of_assessment([FromBody] control_risk_of_assessment control_risk_of_assessments)

        {
            try
            {
                if (control_risk_of_assessments.control_risk_of_assessment_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    control_risk_of_assessments.control_risk_of_assessment_name = control_risk_of_assessments.control_risk_of_assessment_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.control_risk_of_assessments
                     .FirstOrDefault(d => d.control_risk_of_assessment_name == control_risk_of_assessments.control_risk_of_assessment_name && d.control_risk_of_assessment_id != control_risk_of_assessments.control_risk_of_assessment_id && d.control_risk_of_assessment_id != control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:  Control Risk of Assessment with the same name already exists.");
                    }
                    if (control_risk_of_assessments.control_risk_of_assessment_range_min > control_risk_of_assessments.control_risk_of_assessment_range_max)
                    {
                        return BadRequest("Error: Risk Control Assessment Minimum value is greater that  Risk Control Assessment Maximum Value.");
                    }

                    var existingValues = await mySqlDBContext.control_risk_of_assessments
     .Where(x => x.control_risk_of_assessment_id == control_risk_of_assessments.control_risk_of_assessment_id)
     .Select(x => new
     {
         MinValue = x.control_risk_of_assessment_range_min,
         MaxValue = x.control_risk_of_assessment_range_max
     })
     .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = control_risk_of_assessments.control_risk_of_assessment_range_min ?? existingValues.MinValue;
                    var newMax = control_risk_of_assessments.control_risk_of_assessment_range_max ?? existingValues.MaxValue;

                 

                    var existingRanges = await mySqlDBContext.control_risk_of_assessments
                        .Where(d => d.control_risk_of_assessment_id != control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active")
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
                        if (control_risk_of_assessments.control_risk_of_assessment_range_min == null &&
                            control_risk_of_assessments.control_risk_of_assessment_range_max != null &&
                            existingValues.MinValue != 0)
                        {
                            //if (!CheckValidation(existingValues.MinValue, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                            //{
                            //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                            //}
                            if (existingValues.MinValue > control_risk_of_assessments.control_risk_of_assessment_range_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }
                        }

                        if (control_risk_of_assessments.control_risk_of_assessment_range_min != null &&
                            control_risk_of_assessments.control_risk_of_assessment_range_max == null)
                        {


                            if (control_risk_of_assessments.control_risk_of_assessment_range_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }

                            //if (!CheckValidation(RiskSupAdminModels.rating_level_min, existingValues.MaxValue, RiskSupAdminModels.array))
                            //{
                            //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                            //}

                        }
                    }

                    var existingColor = this.mySqlDBContext.control_risk_of_assessments
                  .FirstOrDefault(d => d.color_code == control_risk_of_assessments.color_code && d.control_risk_of_assessment_id != control_risk_of_assessments.control_risk_of_assessment_id && d.status == "Active");

                    if (existingColor != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Control Risk of Assessment colour with the same colour already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(control_risk_of_assessments);
                    this.mySqlDBContext.Entry(control_risk_of_assessments).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(control_risk_of_assessments);

                    Type type = typeof(control_risk_of_assessment);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(control_risk_of_assessments, null) == null || property.GetValue(control_risk_of_assessments, null).Equals(0))
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

        [Route("api/RiskSupAdminController/Delete_control_risk_of_assessment")]
        [HttpDelete]
        public void Delete_control_risk_of_assessment(int id)
        {
            var currentClass = new control_risk_of_assessment { control_risk_of_assessment_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //Residual Risk Rating

        //get residual risk rating


        [Route("api/RiskSupAdminController/Getresidual_risk_rating")]
        [HttpGet]
        public IEnumerable<residual_risk_rating> Getresidual_risk_rating()
        {
            return this.mySqlDBContext.residual_risk_ratings.Where(x => x.status == "Active")
                .OrderBy(r => r.residual_risk_rating_min_rating)
                .ToList();
        }

        //residual_risk_rating Insert Method
        [Route("api/RiskSupAdminController/Insertresidual_risk_rating")]
        [HttpPost]
        public async Task< IActionResult> Insertresidual_risk_rating([FromBody] residual_risk_rating residual_risk_ratings)

        {
            try
            {
                residual_risk_ratings.residual_risk_rating_name = residual_risk_ratings.residual_risk_rating_name?.Trim();

                var existingDepartment = this.mySqlDBContext.residual_risk_ratings
                    .FirstOrDefault(d => d.residual_risk_rating_name == residual_risk_ratings.residual_risk_rating_name && d.residual_risk_rating_id != residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Residual Risk Rating with the same name already exists.");
                }
                if (residual_risk_ratings.residual_risk_rating_min_rating > residual_risk_ratings.residual_risk_rating_max_rating)
                {
                    return BadRequest("Error: Risk residual Minimum value is greater that  residual Maximum Value.");
                }
                //if (!CheckValidation(residual_risk_ratings.residual_risk_rating_min_rating, residual_risk_ratings.residual_risk_rating_max_rating, residual_risk_ratings.array))
                //{
                //    return BadRequest("Error: Residual Risk Rating Range Not Valid.");
                //}
                var EXISTINGCOLOR = this.mySqlDBContext.residual_risk_ratings
                 .FirstOrDefault(d => d.color_code == residual_risk_ratings.color_code && d.residual_risk_rating_id != residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                if (EXISTINGCOLOR != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Residual Risk colour with the same colour already exists.");
                }




                // Set min and max using either the provided values or the existing ones.
                var newMin = residual_risk_ratings.residual_risk_rating_min_rating ;
                var newMax = residual_risk_ratings.residual_risk_rating_max_rating;

                // Ensure min is not greater than max.



                var existingRanges = await mySqlDBContext.residual_risk_ratings
                    .Where(d => d.residual_risk_rating_id != residual_risk_ratings.residual_risk_rating_id && d.status == "Active")
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
                    if (residual_risk_ratings.residual_risk_rating_min_rating != null &&
                        residual_risk_ratings.residual_risk_rating_max_rating != null)
                    {
                        if (residual_risk_ratings.residual_risk_rating_min_rating > residual_risk_ratings.residual_risk_rating_max_rating)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                        }
                    }
                    // Proceed with the insertion
                    var residual_risk_rating = this.mySqlDBContext.residual_risk_ratings;
                residual_risk_rating.Add(residual_risk_ratings);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                residual_risk_ratings.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                residual_risk_ratings.status = "Active";


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

        [Route("api/RiskSupAdminController/Updateresidual_risk_ratings")]
        [HttpPut]
        public async Task< IActionResult> Updateresidual_risk_ratings([FromBody] residual_risk_rating residual_risk_ratings)
        {
            try
            {
                if (residual_risk_ratings.residual_risk_rating_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    residual_risk_ratings.residual_risk_rating_name = residual_risk_ratings.residual_risk_rating_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.residual_risk_ratings
                     .FirstOrDefault(d => d.residual_risk_rating_name == residual_risk_ratings.residual_risk_rating_name && d.residual_risk_rating_id != residual_risk_ratings.residual_risk_rating_id && d.residual_risk_rating_id != residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Residual Risk Rating with the same name already exists.");
                    }
                    if (residual_risk_ratings.residual_risk_rating_min_rating > residual_risk_ratings.residual_risk_rating_max_rating)
                    {
                        return BadRequest("Error: Risk residual Minimum value is greater that  residual Maximum Value.");
                    }

       
                    var existingValues = await mySqlDBContext.residual_risk_ratings
     .Where(x => x.residual_risk_rating_id == residual_risk_ratings.residual_risk_rating_id)
     .Select(x => new
     {
         MinValue = x.residual_risk_rating_min_rating,
         MaxValue = x.residual_risk_rating_max_rating
     })
     .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = residual_risk_ratings.residual_risk_rating_min_rating ?? existingValues.MinValue;
                    var newMax = residual_risk_ratings.residual_risk_rating_max_rating ?? existingValues.MaxValue;

                    // Ensure min is not greater than max.



                    var existingRanges = await mySqlDBContext.residual_risk_ratings
                        .Where(d => d.residual_risk_rating_id != residual_risk_ratings.residual_risk_rating_id && d.status == "Active")
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
                        if (residual_risk_ratings.residual_risk_rating_min_rating == null &&
                            residual_risk_ratings.residual_risk_rating_max_rating != null &&
                            existingValues.MinValue != 0)
                        {
                            //if (!CheckValidation(existingValues.MinValue, RiskSupAdminModels.rating_level_max, RiskSupAdminModels.array))
                            //{
                            //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                            //}
                            if (existingValues.MinValue > residual_risk_ratings.residual_risk_rating_max_rating)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }
                        }

                        if (residual_risk_ratings.residual_risk_rating_min_rating != null &&
                            residual_risk_ratings.residual_risk_rating_max_rating == null)
                        {


                            if (residual_risk_ratings.residual_risk_rating_min_rating > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }

                            //if (!CheckValidation(RiskSupAdminModels.rating_level_min, existingValues.MaxValue, RiskSupAdminModels.array))
                            //{
                            //    return BadRequest("Error:  Risk Priority Rating Range Not Valid.");
                            //}

                        }
                    }
                    var EXISTINGCOLOR = this.mySqlDBContext.residual_risk_ratings
               .FirstOrDefault(d => d.color_code == residual_risk_ratings.color_code && d.residual_risk_rating_id != residual_risk_ratings.residual_risk_rating_id && d.status == "Active");

                    if (EXISTINGCOLOR != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Residual Risk colour with the same colour already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(residual_risk_ratings);
                    this.mySqlDBContext.Entry(residual_risk_ratings).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(residual_risk_ratings);

                    Type type = typeof(residual_risk_rating);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(residual_risk_ratings, null) == null || property.GetValue(residual_risk_ratings, null).Equals(0))
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

        [Route("api/RiskSupAdminController/Deleteresidual_risk_ratings")]
        [HttpDelete]
        public void Deleteresidual_risk_ratings(int id)
        {
            var currentClass = new residual_risk_rating { residual_risk_rating_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }





        //Risk Control Effectiveness Rating

        //get Risk Control Effectiveness Rating


        [Route("api/RiskSupAdminController/Getriskcontroleffectivenessrating")]
        [HttpGet]
        public IEnumerable<riskcontroleffectivenessrating> Getriskcontroleffectivenessrating()
        {
            return this.mySqlDBContext.riskcontroleffectivenessratings.Where(x => x.risk_contr_eff_rating_status == "Active")
                .OrderBy(r => r.risk_contr_eff_rating_rating)
                .ToList();
        }

        //Risk Control Effectiveness Rating Insert Method
        [Route("api/RiskSupAdminController/Insertriskcontroleffectivenessrating")]
        [HttpPost]
        public IActionResult Insertresidual_Effectiveness_rating([FromBody] riskcontroleffectivenessrating riskcontroleffectivenessratings)

        {
            try
            {
                riskcontroleffectivenessratings.risk_contr_eff_rating_name = riskcontroleffectivenessratings.risk_contr_eff_rating_name?.Trim();

                var existingDepartment = this.mySqlDBContext.riskcontroleffectivenessratings
                    .FirstOrDefault(d => d.risk_contr_eff_rating_name == riskcontroleffectivenessratings.risk_contr_eff_rating_name && d.risk_contr_eff_rating_id != riskcontroleffectivenessratings.risk_contr_eff_rating_id && d.risk_contr_eff_rating_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Effectiveness Rating  with the same name already exists.");
                }

                var existingColor = this.mySqlDBContext.riskcontroleffectivenessratings
                   .FirstOrDefault(d => d.risk_contr_eff_rating_color == riskcontroleffectivenessratings.risk_contr_eff_rating_color && d.risk_contr_eff_rating_id != riskcontroleffectivenessratings.risk_contr_eff_rating_id && d.risk_contr_eff_rating_status == "Active");

                if (existingColor != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Effectiveness Rating with the same colour already exists.");
                }
                var existingRating = this.mySqlDBContext.riskcontroleffectivenessratings
                  .FirstOrDefault(d => d.risk_contr_eff_rating_rating == riskcontroleffectivenessratings.risk_contr_eff_rating_rating && d.risk_contr_eff_rating_id != riskcontroleffectivenessratings.risk_contr_eff_rating_id && d.risk_contr_eff_rating_status == "Active");

                if (existingRating != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Control Effectiveness Rating with the same rating already exists.");
                }



                // Proceed with the insertion
                var riskcontroleffectivenessrating = this.mySqlDBContext.riskcontroleffectivenessratings;
                riskcontroleffectivenessrating.Add(riskcontroleffectivenessratings);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskcontroleffectivenessratings.risk_contr_eff_rating_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskcontroleffectivenessratings.risk_contr_eff_rating_status = "Active";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Risk Control Effectiveness Rating with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update Risk Control Effectiveness Rating

        [Route("api/RiskSupAdminController/Updateriskcontroleffectivenessrating")]
        [HttpPut]
        public IActionResult Updateresidual_risk_ratings([FromBody] riskcontroleffectivenessrating riskcontroleffectivenessratings)
        {
            try
            {
                if (riskcontroleffectivenessratings.risk_contr_eff_rating_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    riskcontroleffectivenessratings.risk_contr_eff_rating_name = riskcontroleffectivenessratings.risk_contr_eff_rating_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.riskcontroleffectivenessratings
                     .FirstOrDefault(d => d.risk_contr_eff_rating_name == riskcontroleffectivenessratings.risk_contr_eff_rating_name && d.risk_contr_eff_rating_id != riskcontroleffectivenessratings.risk_contr_eff_rating_id && d.risk_contr_eff_rating_status == "Active");

                    if (existingDepartment != null)
                    {
                       //Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Control Effectiveness Rating with the same name already exists.");
                    }
                    var existingColor = this.mySqlDBContext.riskcontroleffectivenessratings
                  .FirstOrDefault(d => d.risk_contr_eff_rating_color == riskcontroleffectivenessratings.risk_contr_eff_rating_color && d.risk_contr_eff_rating_id != riskcontroleffectivenessratings.risk_contr_eff_rating_id && d.risk_contr_eff_rating_status == "Active");

                    if (existingColor != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Control Effectiveness Rating with the same colour already exists.");
                    }
                    var existingRating = this.mySqlDBContext.riskcontroleffectivenessratings
                      .FirstOrDefault(d => d.risk_contr_eff_rating_rating == riskcontroleffectivenessratings.risk_contr_eff_rating_rating && d.risk_contr_eff_rating_id != riskcontroleffectivenessratings.risk_contr_eff_rating_id && d.risk_contr_eff_rating_status == "Active");

                    if (existingRating != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Control Effectiveness Rating with the same rating already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(riskcontroleffectivenessratings);
                    this.mySqlDBContext.Entry(riskcontroleffectivenessratings).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(riskcontroleffectivenessratings);

                    Type type = typeof(riskcontroleffectivenessrating);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskcontroleffectivenessratings, null) == null || property.GetValue(riskcontroleffectivenessratings, null).Equals(0))
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
                    return BadRequest("Error: Risk Control Effectiveness Rating with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete Risk Control Effectiveness Rating

        [Route("api/RiskSupAdminController/Deleteriskcontroleffectivenessrating")]
        [HttpDelete]
        public void Deleteriskcontroleffectivenessrating(int id)
        {
            var currentClass = new riskcontroleffectivenessrating { risk_contr_eff_rating_id = id };
            currentClass.risk_contr_eff_rating_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("risk_contr_eff_rating_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }






        //BP Maturity Rating Scale Indicators

        //get BP Maturity Rating Scale Indicators


        [Route("api/RiskSupAdminController/Getbpmaturityratingscaleindicators")]
        [HttpGet]
        public IEnumerable<bpmaturityratingscaleindicators> Getbpmaturityratingscaleindicators()
        {
            return this.mySqlDBContext.bpmaturityratingscaleindicatorss.Where(x => x.status == "Active")
                .OrderBy(r => r.BPMaturityRatingScaleIndicators_rating_min)
                .ToList();
        }

        //BP Maturity Rating Scale Indicators Insert Method
        [Route("api/RiskSupAdminController/Insertbpmaturityratingscaleindicators")]
        [HttpPost]
        public async Task< IActionResult> Insertbpmaturityratingscaleindicators([FromBody] bpmaturityratingscaleindicators bpmaturityratingscaleindicatorss)

        {
            try
            {
                bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_name = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_name?.Trim();

                var existingDepartment = this.mySqlDBContext.bpmaturityratingscaleindicatorss
                    .FirstOrDefault(d => d.BPMaturityRatingScaleIndicators_name == bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_name && d.BPMaturityRatingScaleIndicators_id != bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: BP Maturity Rating Scale Indicators  with the same name already exists.");
                }
                if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min > bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max)
                {
                    return BadRequest("Error:BPMaturityRatingScaleIndicators Minimum value is greater that  BPMaturityRatingScaleIndicators Maximum Value.");
                }
                //if (!CheckValidation(bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min, bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max, bpmaturityratingscaleindicatorss.array))
                //{
                //    return BadRequest("Error: BP Maturity Rating Scale Indicators Range Not Valid.");
                //}

                // Set min and max using either the provided values or the existing ones.
                var newMin = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min ;
                var newMax = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max ;

                // Ensure min is not greater than max.



                var existingRanges = await mySqlDBContext.bpmaturityratingscaleindicatorss
                    .Where(d => d.BPMaturityRatingScaleIndicators_id != bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_id && d.status == "Active")
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
                            return BadRequest("Error: Risk BP Maturity Rating Rating Minimum value is greater that Maximum Valuue.");
                        }
                    }




                    // Proceed with the insertion
                    var bpmaturityratingscaleindicators = this.mySqlDBContext.bpmaturityratingscaleindicatorss;
                bpmaturityratingscaleindicators.Add(bpmaturityratingscaleindicatorss);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                bpmaturityratingscaleindicatorss.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                bpmaturityratingscaleindicatorss.status = "Active";


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

        [Route("api/RiskSupAdminController/Updatebpmaturityratingscaleindicators")]
        [HttpPut]
        public async Task< IActionResult> Updatebpmaturityratingscaleindicators([FromBody] bpmaturityratingscaleindicators bpmaturityratingscaleindicatorss)
        {
            try
            {
                if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_name = bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.bpmaturityratingscaleindicatorss
                     .FirstOrDefault(d => d.BPMaturityRatingScaleIndicators_name == bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_name && d.BPMaturityRatingScaleIndicators_id != bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_id && d.BPMaturityRatingScaleIndicators_id != bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: BP Maturity Rating Scale Indicators with the same name already exists.");
                    }
                    if (bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_min > bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_rating_max)
                    {
                        return BadRequest("Error:BPMaturityRatingScaleIndicators Minimum value is greater that  BPMaturityRatingScaleIndicators Maximum Value.");
                    }

            

                    var existingValues = await mySqlDBContext.bpmaturityratingscaleindicatorss
.Where(x => x.BPMaturityRatingScaleIndicators_id == bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_id)
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



                    var existingRanges = await mySqlDBContext.bpmaturityratingscaleindicatorss
                        .Where(d => d.BPMaturityRatingScaleIndicators_id != bpmaturityratingscaleindicatorss.BPMaturityRatingScaleIndicators_id && d.status == "Active")
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

                    Type type = typeof(bpmaturityratingscaleindicators);
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

        [Route("api/RiskSupAdminController/Deletebpmaturityratingscaleindicators")]
        [HttpDelete]
        public void Deletebpmaturityratingscaleindicators(int id)
        {
            var currentClass = new bpmaturityratingscaleindicators { BPMaturityRatingScaleIndicators_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Control Assessment Test Attributes

        //get Control Assessment Test Attributes


        [Route("api/RiskSupAdminController/Getcontrolassesstestattributes")]
        [HttpGet]
        public IEnumerable<controlassesstestattributes> Getcontrolassesstestattributes()
        {
            return this.mySqlDBContext.controlassesstestattributess.Where(x => x.status == "Active").ToList();
        }

        //Control Assessment Test Attributes Insert Method
        [Route("api/RiskSupAdminController/Insertcontrolassesstestattributes")]
        [HttpPost]
        public IActionResult Insertcontrolassesstestattributes([FromBody] controlassesstestattributes controlassesstestattributess)

        {
            try
            {
                controlassesstestattributess.ControlAssessTestAttributes_name = controlassesstestattributess.ControlAssessTestAttributes_name?.Trim();

                var existingDepartment = this.mySqlDBContext.controlassesstestattributess
                    .FirstOrDefault(d => d.ControlAssessTestAttributes_name == controlassesstestattributess.ControlAssessTestAttributes_name && d.ControlAssessTestAttributes_id != controlassesstestattributess.ControlAssessTestAttributes_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Assessment Test Attributes with the same name already exists.");
                }
                // Proceed with the insertion
                var controlassesstestattributes = this.mySqlDBContext.controlassesstestattributess;
                controlassesstestattributes.Add(controlassesstestattributess);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                controlassesstestattributess.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                controlassesstestattributess.status = "Active";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Control Assessment Test Attributes  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update Control Assessment Test Attributes

        [Route("api/RiskSupAdminController/Updatecontrolassesstestattributes")]
        [HttpPut]
        public IActionResult Updatecontrolassesstestattributes([FromBody] controlassesstestattributes controlassesstestattributess)
        {
            try
            {
                if (controlassesstestattributess.ControlAssessTestAttributes_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    controlassesstestattributess.ControlAssessTestAttributes_name = controlassesstestattributess.ControlAssessTestAttributes_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.controlassesstestattributess
                     .FirstOrDefault(d => d.ControlAssessTestAttributes_name == controlassesstestattributess.ControlAssessTestAttributes_name && d.ControlAssessTestAttributes_id != controlassesstestattributess.ControlAssessTestAttributes_id && d.ControlAssessTestAttributes_id != controlassesstestattributess.ControlAssessTestAttributes_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Control Assessment Test Attributes with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(controlassesstestattributess);
                    this.mySqlDBContext.Entry(controlassesstestattributess).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(controlassesstestattributess);

                    Type type = typeof(controlassesstestattributes);
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
                    return BadRequest("Error: Control Assessment Test Attributes with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete Control Assessment Test Attributes

        [Route("api/RiskSupAdminController/Deletecontrolassesstestattributes")]
        [HttpDelete]
        public void Deletecontrolassesstestattributes(int id)
        {
            var currentClass = new controlassesstestattributes { ControlAssessTestAttributes_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


        //Risk Treatment Decision List

        //get Risk Treatment Decision List


        [Route("api/RiskSupAdminController/GetRiskTreatmentDecisionLists")]
        [HttpGet]
        public IEnumerable<RiskTreatmentDecisionList> GetRiskTreatmentDecisionLists()
        {
            return this.mySqlDBContext.RiskTreatmentDecisionLists.Where(x => x.status == "Active").ToList();
        }

        // RiskTreatmentDecisionLists Insert Method
        [Route("api/RiskSupAdminController/InsertRiskTreatmentDecisionLists")]
        [HttpPost]
        public IActionResult InsertRiskTreatmentDecisionLists([FromBody] RiskTreatmentDecisionList RiskTreatmentDecisionLists)

        {
            try
            {
                RiskTreatmentDecisionLists.Risk_treatmentDecisionList_Name = RiskTreatmentDecisionLists.Risk_treatmentDecisionList_Name?.Trim();

                var existingDepartment = this.mySqlDBContext.RiskTreatmentDecisionLists
                    .FirstOrDefault(d => d.Risk_treatmentDecisionList_Name == RiskTreatmentDecisionLists.Risk_treatmentDecisionList_Name && d.Risk_treatmentDecisionList_id != RiskTreatmentDecisionLists.Risk_treatmentDecisionList_id && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:Risk Treatment Decision List with the same name already exists.");
                }
                // Proceed with the insertion
                var RiskTreatmentDecisionList = this.mySqlDBContext.RiskTreatmentDecisionLists;
                RiskTreatmentDecisionList.Add(RiskTreatmentDecisionLists);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskTreatmentDecisionLists.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskTreatmentDecisionLists.status = "Active";


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

        [Route("api/RiskSupAdminController/UpdateRiskTreatmentDecisionLists")]
        [HttpPut]
        public IActionResult UpdateRiskTreatmentDecisionLists([FromBody] RiskTreatmentDecisionList RiskTreatmentDecisionLists)
        {
            try
            {
                if (RiskTreatmentDecisionLists.Risk_treatmentDecisionList_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    RiskTreatmentDecisionLists.Risk_treatmentDecisionList_Name = RiskTreatmentDecisionLists.Risk_treatmentDecisionList_Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.RiskTreatmentDecisionLists
                     .FirstOrDefault(d => d.Risk_treatmentDecisionList_Name == RiskTreatmentDecisionLists.Risk_treatmentDecisionList_Name && d.Risk_treatmentDecisionList_id != RiskTreatmentDecisionLists.Risk_treatmentDecisionList_id && d.Risk_treatmentDecisionList_id != RiskTreatmentDecisionLists.Risk_treatmentDecisionList_id && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Treatment Decision List with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(RiskTreatmentDecisionLists);
                    this.mySqlDBContext.Entry(RiskTreatmentDecisionLists).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(RiskTreatmentDecisionLists);

                    Type type = typeof(RiskTreatmentDecisionList);
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

        [Route("api/RiskSupAdminController/DeleteRiskTreatmentDecisionLists")]
        [HttpDelete]
        public void DeleteRiskTreatmentDecisionLists(int id)
        {
            var currentClass = new RiskTreatmentDecisionList { Risk_treatmentDecisionList_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }






        //risk_treatmetdecisionmatrix

        //get risk_treatmetdecisionmatrix


        [Route("api/RiskSupAdminController/Getrisk_treatmetdecisionmatrix")]
        [HttpGet]
        public IEnumerable<risk_treatmetdecisionmatrix> Getrisk_treatmetdecisionmatrix()
        {
            return this.mySqlDBContext.risk_treatmetdecisionmatrixs.Where(x => x.status == "Active").ToList();
        }

        //risk_treatmetdecisionmatrixs Insert Method
        [Route("api/RiskSupAdminController/Insertrisk_treatmetdecisionmatrixs")]
        [HttpPost]
        public IActionResult Insertrisk_treatmetdecisionmatrixs([FromBody] risk_treatmetdecisionmatrix risk_treatmetdecisionmatrixs)

        {
            try
            {



                // Proceed with the insertion
                var risk_treatmetdecisionmatrix = this.mySqlDBContext.risk_treatmetdecisionmatrixs;
                risk_treatmetdecisionmatrix.Add(risk_treatmetdecisionmatrixs);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_treatmetdecisionmatrixs.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_treatmetdecisionmatrixs.status = "Active";


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

        [Route("api/RiskSupAdminController/Updaterisk_treatmetdecisionmatrixs")]
        [HttpPut]
        public IActionResult Updaterisk_treatmetdecisionmatrixs([FromBody] risk_treatmetdecisionmatrix risk_treatmetdecisionmatrixs)
        {
            try
            {
                if (risk_treatmetdecisionmatrixs.Risk_TreatmetDecisionMatrix_id == 0)
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

                    Type type = typeof(risk_treatmetdecisionmatrix);
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

        [Route("api/RiskSupAdminController/Deleterisk_treatmetdecisionmatrixs")]
        [HttpDelete]
        public void Deleterisk_treatmetdecisionmatrixs(int id)
        {
            var currentClass = new risk_treatmetdecisionmatrix { Risk_TreatmetDecisionMatrix_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }



        //risk_controltestdecisionlist

        //get risk_controltestdecisionlist


        [Route("api/RiskSupAdminController/Getrisk_controltestdecisionlist")]
        [HttpGet]
        public IEnumerable<risk_controltestdecisionlist> Getrisk_controltestdecisionlist()
        {
            return this.mySqlDBContext.risk_controltestdecisionlists.Where(x => x.status == "Active").ToList();
        }

        //risk_controltestdecisionlists Insert Method

        [Route("api/RiskSupAdminController/Insertrisk_controltestdecisionlists")]
        [HttpPost]
        public IActionResult Insertrisk_controltestdecisionlists([FromBody] risk_controltestdecisionlist risk_controltestdecisionlists)

        {
            try
            {
                risk_controltestdecisionlists.ControlTestDecisionName = risk_controltestdecisionlists.ControlTestDecisionName?.Trim();

                var existingDepartment = this.mySqlDBContext.risk_controltestdecisionlists
                   .FirstOrDefault(d => d.ControlTestDecisionName == risk_controltestdecisionlists.ControlTestDecisionName && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Test Decision  Name with the same name already exists.");
                }

                var existingDepartment1 = this.mySqlDBContext.risk_controltestdecisionlists
                 .FirstOrDefault(d => d.ControlTestDecisionRatingScore == risk_controltestdecisionlists.ControlTestDecisionRatingScore && d.status == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Test Decision rating with the same value already exists.");
                }

                var existingDepartment2 = this.mySqlDBContext.risk_controltestdecisionlists
               .FirstOrDefault(d => d.colorReference == risk_controltestdecisionlists.colorReference && d.status == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Control Test Decision color refrence with the same color already exists.");
                }



                // Proceed with the insertion
                var risk_controltestdecisionlist = this.mySqlDBContext.risk_controltestdecisionlists;
                risk_controltestdecisionlist.Add(risk_controltestdecisionlists);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_controltestdecisionlists.created_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_controltestdecisionlists.status = "Active";


                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {

                    return BadRequest("Error: Control Test Decision  with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        //Update risk_controltestdecisionlists

        [Route("api/RiskSupAdminController/Updaterisk_controltestdecisionlists")]
        [HttpPut]
        public IActionResult Updaterisk_controltestdecisionlists([FromBody] risk_controltestdecisionlist risk_controltestdecisionlists)
        {
            try
            {
                if (risk_controltestdecisionlists.Risk_controlTestDecisionList_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    risk_controltestdecisionlists.ControlTestDecisionName = risk_controltestdecisionlists.ControlTestDecisionName?.Trim();

                    var existingDepartment = this.mySqlDBContext.risk_controltestdecisionlists
                       .FirstOrDefault(d => d.ControlTestDecisionName == risk_controltestdecisionlists.ControlTestDecisionName && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Control Test Decision  Name with the same name already exists.");
                    }

                    var existingDepartment1 = this.mySqlDBContext.risk_controltestdecisionlists
                     .FirstOrDefault(d => d.ControlTestDecisionRatingScore == risk_controltestdecisionlists.ControlTestDecisionRatingScore && d.status == "Active");

                    if (existingDepartment1 != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Control Test Decision rating with the same value already exists.");
                    }

                    var existingDepartment2 = this.mySqlDBContext.risk_controltestdecisionlists
                   .FirstOrDefault(d => d.colorReference == risk_controltestdecisionlists.colorReference && d.status == "Active");

                    if (existingDepartment2 != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Control Test Decision color refrence with the same color already exists.");
                    }



                    // Existing department, update logic
                    this.mySqlDBContext.Attach(risk_controltestdecisionlists);
                    this.mySqlDBContext.Entry(risk_controltestdecisionlists).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(risk_controltestdecisionlists);

                    Type type = typeof(risk_controltestdecisionlist);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_controltestdecisionlists, null) == null || property.GetValue(risk_controltestdecisionlists, null).Equals(0))
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

                    return BadRequest("Error: Control Test Decision  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }



        }




        //Delete risk_controltestdecisionlists

        [Route("api/RiskSupAdminController/Deleterisk_controltestdecisionlists")]
        [HttpDelete]
        public void Deleterisk_controltestdecisionlists(int id)
        {
            var currentClass = new risk_controltestdecisionlist { Risk_controlTestDecisionList_id = id };
            currentClass.status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }






    }
}
