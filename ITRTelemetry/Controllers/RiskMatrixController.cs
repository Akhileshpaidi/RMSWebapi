using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using MySqlConnector;
using MySQLProvider;
using System.Threading.Tasks;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RiskMatrixController : ControllerBase
    {


        private readonly CommonDBContext commonDBContext;

        public RiskMatrixController(CommonDBContext commonDBContext)
        {

            this.commonDBContext = commonDBContext;
        }



        public Boolean CheckValidation(int? start, int? end, List<int[]> array)
        {
            // Debugging alert equivalent
            //  Console.WriteLine($"{this.min} and {this.max} and {JsonConvert.SerializeObject(this.valArray)}");

            // Sort the array by the first element in each sub-array
            if (array.Count() > 0) {
                array.Sort((a, b) => a[0].CompareTo(b[0]));

                int n = array.Count;

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







        //Getting categorization Details

        [Route("api/Riskcategorization/GetRiskcategorizationModelDetails")]
        [HttpGet]

        public IEnumerable<RiskMatrixModel> GetRiskcategorizationModelDetails()
        {
            return this.commonDBContext.RiskMatrixModels.Where(x => x.Risk_categorization_status == "Active").ToList();
        }




        [Route("api/Riskcategorization/InsertRiskcategorizationModelDetails")]
        [HttpPost]
        public IActionResult InsertRiskcategorization([FromBody] RiskMatrixModel RiskMatrixModels)
        {
            try
            {
                RiskMatrixModels.Risk_categorization_name = RiskMatrixModels.Risk_categorization_name?.Trim();

                var existingDepartment = this.commonDBContext.RiskMatrixModels
                    .FirstOrDefault(d => d.Risk_categorization_name == RiskMatrixModels.Risk_categorization_name && d.Risk_categorization_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Categorization Name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.RiskMatrixModels;
                riskmatrixModel.Add(RiskMatrixModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskMatrixModels.Risk_categorization_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                RiskMatrixModels.Risk_categorization_status = "Active";

                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk Categorization Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/Riskcategorization/UpdateRiskcategorizationModelDetails")]
        [HttpPut]
        public IActionResult UpdateRiskcategorization([FromBody] RiskMatrixModel riskMatrixModel)
        {
            try
            {
                if (riskMatrixModel.Risk_categorization_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskMatrixModel.Risk_categorization_name = riskMatrixModel.Risk_categorization_name?.Trim();

                    var existingDepartment = this.commonDBContext.RiskMatrixModels
                      .FirstOrDefault(d => d.Risk_categorization_name == riskMatrixModel.Risk_categorization_name && d.Risk_categorization_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Categorization Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(riskMatrixModel);
                    this.commonDBContext.Entry(riskMatrixModel).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskMatrixModel);

                    Type type = typeof(RiskMatrixModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskMatrixModel, null) == null || property.GetValue(riskMatrixModel, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
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
                    return BadRequest("Error: Risk Categorization Name  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/Riskcategorization/DeleteRiskcategorizationModelDetails")]
        [HttpDelete]
        public void DeleteRiskcategorization(int id)
        {
            var currentClass = new RiskMatrixModel { Risk_categorization_id = id };
            currentClass.Risk_categorization_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("Risk_categorization_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }



        //Getting Cause list Details

        [Route("api/RiskCauselist/GetRiskRiskCauselistModelDetails")]
        [HttpGet]

        public IEnumerable<RiskMatrixcauseList> GetRiskRiskCauselistModelDetails()
        {
            return this.commonDBContext.riskMatrixcauseLists.Where(x => x.Risk_cause_list_status == "Active").ToList();
        }




        [Route("api/RiskCauselist/InsertRiskCauselistModelDetails")]
        [HttpPost]
        public IActionResult InsertRiskCauselistModelDetails([FromBody] RiskMatrixcauseList riskMatrixcauseList)
        {
            try
            {
                riskMatrixcauseList.Risk_cause_list_name = riskMatrixcauseList.Risk_cause_list_name?.Trim();

                var existingDepartment = this.commonDBContext.riskMatrixcauseLists
                    .FirstOrDefault(d => d.Risk_cause_list_name == riskMatrixcauseList.Risk_cause_list_name && d.Risk_cause_list_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Cause List Name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.riskMatrixcauseLists;
                riskmatrixModel.Add(riskMatrixcauseList);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskMatrixcauseList.Risk_cause_list_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                riskMatrixcauseList.Risk_cause_list_status = "Active";

                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Risk Cause List Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/RiskCauselist/UpdateRiskCauselistModelDetails")]
        [HttpPut]
        public IActionResult UpdateRiskCauselistModelDetails([FromBody] RiskMatrixcauseList riskMatrixcauseList)
        {
            try
            {
                if (riskMatrixcauseList.Risk_cause_list_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    riskMatrixcauseList.Risk_cause_list_name = riskMatrixcauseList.Risk_cause_list_name?.Trim();

                    var existingDepartment = this.commonDBContext.riskMatrixcauseLists
                      .FirstOrDefault(d => d.Risk_cause_list_name == riskMatrixcauseList.Risk_cause_list_name && d.Risk_cause_list_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Cause List Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(riskMatrixcauseList);
                    this.commonDBContext.Entry(riskMatrixcauseList).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskMatrixcauseList);

                    Type type = typeof(RiskMatrixcauseList);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskMatrixcauseList, null) == null || property.GetValue(riskMatrixcauseList, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
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
                    return BadRequest("Error: Risk Cause List Name  with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/RiskCauselist/DeleteRiskCauselistModelDetails")]
        [HttpDelete]
        public void DeleteRiskCauselistModelDetails(int id)
        {
            var currentClass = new RiskMatrixcauseList { Risk_cause_list_id = id };
            currentClass.Risk_cause_list_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("Risk_cause_list_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }








        //Risk Likelihood of occurence factor

        [Route("api/RiskLilklihoodOccurence/GetRiskLilklihoodOccurence")]
        [HttpGet]
        public IEnumerable<RiskLikelihood> GetRiskLilklihoodOccurence()
        {
            return this.commonDBContext.riskLikelihoods.Where(x => x.risk_likelihood_occ_factor_status == "Active")
                .OrderBy(r => r.risk_likelihood_occ_factor_value)
                .ToList();
        }



        [Route("api/RiskLilklihoodOccurence/InsertRiskLilklihoodOccurence")]
        [HttpPost]
        public IActionResult InsertRiskLilklihoodOccurence([FromBody] RiskLikelihood riskLikelihood)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                riskLikelihood.risk_likelihood_occ_factor_name = riskLikelihood.risk_likelihood_occ_factor_name?.Trim();

                var existingDepartment = this.commonDBContext.riskLikelihoods
                   .FirstOrDefault(d => d.risk_likelihood_occ_factor_name == riskLikelihood.risk_likelihood_occ_factor_name && d.risk_likelihood_occ_factor_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Likelihood of Occurrence Factor Name with the same name already exists.");
                }

                var existingDepartment1 = this.commonDBContext.riskLikelihoods
                 .FirstOrDefault(d => d.risk_likelihood_occ_factor_value == riskLikelihood.risk_likelihood_occ_factor_value && d.risk_likelihood_occ_factor_status == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Likelihood of Occurrence Factor value with the same value already exists.");
                }

                var existingDepartment2 = this.commonDBContext.riskLikelihoods
               .FirstOrDefault(d => d.colour_reference == riskLikelihood.colour_reference && d.risk_likelihood_occ_factor_status == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Likelihood of Occurrence Factor color refrence with the same color already exists.");
                }

  
                // Proceed with the insertion
                riskLikelihood.risk_likelihood_occ_factor_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                riskLikelihood.risk_likelihood_occ_factor_status = "Active";

                this.commonDBContext.riskLikelihoods.Add(riskLikelihood);
                this.commonDBContext.SaveChanges();

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





        [Route("api/RiskLilklihoodOccurence/UpdateRiskLilklihoodOccurence")]
        [HttpPut]
        public IActionResult UpdateRiskLilklihoodOccurence([FromBody] RiskLikelihood riskLikelihood)
        {
            try
            {
                if (riskLikelihood.risk_likelihood_occ_factor_id == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    riskLikelihood.risk_likelihood_occ_factor_name = riskLikelihood.risk_likelihood_occ_factor_name?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.commonDBContext.riskLikelihoods
                        .FirstOrDefault(d => d.risk_likelihood_occ_factor_name == riskLikelihood.risk_likelihood_occ_factor_name
                                          && d.risk_likelihood_occ_factor_id != riskLikelihood.risk_likelihood_occ_factor_id
                                          && d.risk_likelihood_occ_factor_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk Likelihood of Occurrence Factor name with the same name already exists.");
                    }

                    var existingValue = this.commonDBContext.riskLikelihoods
                        .FirstOrDefault(d => d.risk_likelihood_occ_factor_value == riskLikelihood.risk_likelihood_occ_factor_value
                                          && d.risk_likelihood_occ_factor_id != riskLikelihood.risk_likelihood_occ_factor_id
                                          && d.risk_likelihood_occ_factor_status == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk Likelihood of Occurrence Factor value with the same value already exists.");
                    }

                    var existingColorReference = this.commonDBContext.riskLikelihoods
                        .FirstOrDefault(d => d.colour_reference == riskLikelihood.colour_reference
                                          && d.risk_likelihood_occ_factor_id != riskLikelihood.risk_likelihood_occ_factor_id
                                          && d.risk_likelihood_occ_factor_status == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk Likelihood of Occurrence Factor color reference with the same color already exists.");
                    }

                    // If no duplicates are found, proceed with the update
                    this.commonDBContext.Attach(riskLikelihood);
                    this.commonDBContext.Entry(riskLikelihood).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskLikelihood);
                    Type type = typeof(RiskLikelihood);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskLikelihood, null) == null || property.GetValue(riskLikelihood, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
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



        [Route("api/RiskLilklihoodOccurence/DeleteRiskLilklihoodOccurence")]
        [HttpDelete]
        public void DeleteRiskLilklihoodOccurence(int id)
        {
            var currentClass = new RiskLikelihood { risk_likelihood_occ_factor_id = id };
            currentClass.risk_likelihood_occ_factor_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_likelihood_occ_factor_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


        //Risk Risk impact of occurence factor

        [Route("api/RiskImpact/GetRiskImpact")]
        [HttpGet]
        public IEnumerable<RiskImpactRating> GetRiskImpact()
        {
            return this.commonDBContext.riskImpactRatings.Where(x => x.risk_impactrating_status == "Active")
                .OrderBy(r => r.RiskImpactRatingScale)
                .ToList();
        }


        [Route("api/RiskImpact/InsertRiskImpact")]
        [HttpPost]
        public IActionResult InsertRiskImpact([FromBody] RiskImpactRating riskImpactRating)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                riskImpactRating.RiskImpactRatingName = riskImpactRating.RiskImpactRatingName?.Trim();


                var existingDepartment = this.commonDBContext.riskImpactRatings
                  .FirstOrDefault(d => d.RiskImpactRatingName == riskImpactRating.RiskImpactRatingName && d.risk_impactrating_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk impact name with the same name already exists.");
                }

                var existingDepartment1 = this.commonDBContext.riskImpactRatings
                 .FirstOrDefault(d => d.RiskImpactRatingScale == riskImpactRating.RiskImpactRatingScale && d.risk_impactrating_status == "Active");

                if (existingDepartment1 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk Impact value with the same value already exists.");
                }

                var existingDepartment2 = this.commonDBContext.riskImpactRatings
               .FirstOrDefault(d => d.colour_reference == riskImpactRating.colour_reference && d.risk_impactrating_status == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk color refrence with the same color already exists.");
                }



                // Proceed with the insertion
                riskImpactRating.risk_impactrating_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                riskImpactRating.risk_impactrating_status = "Active";

                this.commonDBContext.riskImpactRatings.Add(riskImpactRating);
                this.commonDBContext.SaveChanges();

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




        [Route("api/RiskImpact/UpdateRiskImpact")]
        [HttpPut]
        public IActionResult UpdateRiskImpact([FromBody] RiskImpactRating riskImpactRating)
        {
            try
            {
                if (riskImpactRating.ImpactRatingID == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    riskImpactRating.RiskImpactRatingName = riskImpactRating.RiskImpactRatingName?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.commonDBContext.riskImpactRatings
                        .FirstOrDefault(d => d.RiskImpactRatingName == riskImpactRating.RiskImpactRatingName
                                          && d.ImpactRatingID != riskImpactRating.ImpactRatingID
                                          && d.risk_impactrating_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk impact name with the same name already exists");
                    }

                    var existingValue = this.commonDBContext.riskImpactRatings
                        .FirstOrDefault(d => d.RiskImpactRatingScale == riskImpactRating.RiskImpactRatingScale
                                          && d.ImpactRatingID != riskImpactRating.ImpactRatingID
                                          && d.risk_impactrating_status == "Active");

                    if (existingValue != null)
                    {
                        return BadRequest("Error: Risk factor value with the same value already exists.");
                    }

                    var existingColorReference = this.commonDBContext.riskImpactRatings
                        .FirstOrDefault(d => d.colour_reference == riskImpactRating.colour_reference
                                          && d.ImpactRatingID != riskImpactRating.ImpactRatingID
                                          && d.risk_impactrating_status == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }

                    // If no duplicates are found, proceed with the update
                    this.commonDBContext.Attach(riskImpactRating);
                    this.commonDBContext.Entry(riskImpactRating).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskImpactRating);
                    Type type = typeof(RiskImpactRating);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskImpactRating, null) == null || property.GetValue(riskImpactRating, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
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




        [Route("api/RiskImpact/DeleteRiskImpact")]
        [HttpDelete]
        public void DeleteRiskImpact(int id)
        {
            var currentClass = new RiskImpactRating { ImpactRatingID = id };
            currentClass.risk_impactrating_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_impactrating_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }




        //Risk inherent risk rating level 

        [Route("api/RiskInherentRatingLevel/GetRiskInherentRatingLevel")]
        [HttpGet]
        public IEnumerable<RiskInherentRatingLevel> GetRiskInherentRatingLevel()
        {
            return this.commonDBContext.riskInherentRatingLevels.Where(x => x.Risk_inherent_rating_level_status == "Active")
                .OrderBy(r => r.Risk_inherent_rating_level_min)
                .ToList();
        }



        [Route("api/RiskInherentRatingLevel/InsertRiskInherentRatingLevel")]
        [HttpPost]
        public async Task< IActionResult> InsertRiskInherentRatingLevel([FromBody] RiskInherentRatingLevel riskInherentRatingLevel)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                riskInherentRatingLevel.Risk_inherent_rating_level_name = riskInherentRatingLevel.Risk_inherent_rating_level_name?.Trim();


                var existingDepartment = this.commonDBContext.riskInherentRatingLevels
                  .FirstOrDefault(d => d.Risk_inherent_rating_level_name == riskInherentRatingLevel.Risk_inherent_rating_level_name
                   && d.Risk_inherent_rating_level_id != riskInherentRatingLevel.Risk_inherent_rating_level_id && d.Risk_inherent_rating_level_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk inherent rating name with the same name already exists.");
                }

                if (riskInherentRatingLevel.Risk_inherent_rating_level_min > riskInherentRatingLevel.Risk_inherent_rating_level_max)
                {
                    return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                }


                //if (!CheckValidation(riskInherentRatingLevel.Risk_inherent_rating_level_min, riskInherentRatingLevel.Risk_inherent_rating_level_max, riskInherentRatingLevel.array))
                //{
                //    return BadRequest("Error: Risk inherent rating Range Not Valid.");
                //}

                var colorRepeat = this.commonDBContext.riskInherentRatingLevels
                .FirstOrDefault(d => d.colour_reference == riskInherentRatingLevel.colour_reference
                 && d.Risk_inherent_rating_level_id != riskInherentRatingLevel.Risk_inherent_rating_level_id && d.Risk_inherent_rating_level_status == "Active");

                if (colorRepeat != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk inherent rating colour reference with the same colour already exists.");
                }




                // Set min and max using either the provided values or the existing ones.
                var newMin = riskInherentRatingLevel.Risk_inherent_rating_level_min ;
                var newMax = riskInherentRatingLevel.Risk_inherent_rating_level_max;



                var existingRanges = await commonDBContext.riskInherentRatingLevels
                    .Where(d => d.Risk_inherent_rating_level_id != riskInherentRatingLevel.Risk_inherent_rating_level_id && d.Risk_inherent_rating_level_status == "Active")
                    .Select(d => new { MinValue = d.Risk_inherent_rating_level_min, MaxValue = d.Risk_inherent_rating_level_max })
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
                    if (riskInherentRatingLevel.Risk_inherent_rating_level_min != null &&
                        riskInherentRatingLevel.Risk_inherent_rating_level_max != null)
                    {

                        if (riskInherentRatingLevel.Risk_inherent_rating_level_min > riskInherentRatingLevel.Risk_inherent_rating_level_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                        }
                    }

                    




                // Proceed with the insertion
                riskInherentRatingLevel.Risk_inherent_rating_level_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                riskInherentRatingLevel.Risk_inherent_rating_level_status = "Active";

                this.commonDBContext.riskInherentRatingLevels.Add(riskInherentRatingLevel);
                this.commonDBContext.SaveChanges();

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





        [Route("api/RiskInherentRatingLevel/UpdateRiskInherentRatingLevel")]
        [HttpPut]
        public async Task<IActionResult> UpdateRiskInherentRatingLevel([FromBody] RiskInherentRatingLevel riskInherentRatingLevel)
        {
            try
            {
                if (riskInherentRatingLevel.Risk_inherent_rating_level_id == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    riskInherentRatingLevel.Risk_inherent_rating_level_name = riskInherentRatingLevel.Risk_inherent_rating_level_name?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.commonDBContext.riskInherentRatingLevels
                        .FirstOrDefault(d => d.Risk_inherent_rating_level_name == riskInherentRatingLevel.Risk_inherent_rating_level_name
                                          && d.Risk_inherent_rating_level_id != riskInherentRatingLevel.Risk_inherent_rating_level_id
                                          && d.Risk_inherent_rating_level_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk inherent name with the same name already exists");
                    }
                    if (riskInherentRatingLevel.Risk_inherent_rating_level_min > riskInherentRatingLevel.Risk_inherent_rating_level_max)
                    {
                        return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                    }



                    var existingColorReference = this.commonDBContext.riskInherentRatingLevels
                        .FirstOrDefault(d => d.colour_reference == riskInherentRatingLevel.colour_reference
                                          && d.Risk_inherent_rating_level_id != riskInherentRatingLevel.Risk_inherent_rating_level_id
                                          && d.Risk_inherent_rating_level_status == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }

                    var existingValues = await commonDBContext.riskInherentRatingLevels
     .Where(x => x.Risk_inherent_rating_level_id == riskInherentRatingLevel.Risk_inherent_rating_level_id)
     .Select(x => new
     {
         MinValue = x.Risk_inherent_rating_level_min,
         MaxValue = x.Risk_inherent_rating_level_min
     })
     .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = riskInherentRatingLevel.Risk_inherent_rating_level_min ?? existingValues.MinValue;
                    var newMax = riskInherentRatingLevel.Risk_inherent_rating_level_max ?? existingValues.MaxValue;

                 

                    var existingRanges = await commonDBContext.riskInherentRatingLevels
                        .Where(d => d.Risk_inherent_rating_level_id != riskInherentRatingLevel.Risk_inherent_rating_level_id && d.Risk_inherent_rating_level_status == "Active")
                        .Select(d => new { MinValue = d.Risk_inherent_rating_level_min, MaxValue = d.Risk_inherent_rating_level_max })
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
                        if (riskInherentRatingLevel.Risk_inherent_rating_level_min == null &&
                            riskInherentRatingLevel.Risk_inherent_rating_level_max != null &&
                            existingValues.MinValue != 0)
                        {
                           
                            if (existingValues.MinValue > riskInherentRatingLevel.Risk_inherent_rating_level_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (riskInherentRatingLevel.Risk_inherent_rating_level_min != null &&
                            riskInherentRatingLevel.Risk_inherent_rating_level_max == null)
                        {


                            if (riskInherentRatingLevel.Risk_inherent_rating_level_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }

                        

                        }
                    }

                    this.commonDBContext.Attach(riskInherentRatingLevel);
                    this.commonDBContext.Entry(riskInherentRatingLevel).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(riskInherentRatingLevel);
                    Type type = typeof(RiskInherentRatingLevel);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(riskInherentRatingLevel, null) == null || property.GetValue(riskInherentRatingLevel, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
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




        [Route("api/RiskInherentRatingLevel/DeleteRiskInherentRatingLevel")]
        [HttpDelete]
        public void DeleteRiskInherentRatingLevel(int id)
        {
            var currentClass = new RiskInherentRatingLevel { Risk_inherent_rating_level_id = id };
            currentClass.Risk_inherent_rating_level_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("Risk_inherent_rating_level_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }


        //Risk  risk intensity rating

        [Route("api/RiskIntensity/GetRiskRiskIntensity")]
        [HttpGet]
        public IEnumerable<risk_intensity> GetRiskRiskIntensity()
        {
            return this.commonDBContext.risk_Intensities.Where(x => x.risk_intensity_status == "Active")
                .OrderBy(r => r.risk_intensity_level_range_min)
                .ToList();
        }



        [Route("api/RiskIntensity/InsertRiskIntensity")]
        [HttpPost]
        public async Task< IActionResult> InsertRiskIntensity([FromBody] risk_intensity risk_Intensity)
        {
            try
            {
                // Trim the name to avoid any issues with leading/trailing spaces
                risk_Intensity.risk_intensity_name = risk_Intensity.risk_intensity_name?.Trim();


                var existingDepartment = this.commonDBContext.risk_Intensities
                  .FirstOrDefault(d => d.risk_intensity_name == risk_Intensity.risk_intensity_name &&d.risk_intensity_id!= risk_Intensity.risk_intensity_id && d.risk_intensity_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk intensity name with the same name already exists.");
                }
                if (risk_Intensity.risk_intensity_level_range_min > risk_Intensity.risk_intensity_level_range_max)
                {
                    return BadRequest("Error: Risk intensity Minimum value is greater that  intensity Maximum Value.");
                }

                //if (risk_Intensity.risk_intensity_level_range_min != null && risk_Intensity.risk_intensity_level_range_max != null)
                //{
                //    if (!CheckValidation(risk_Intensity.risk_intensity_level_range_min, risk_Intensity.risk_intensity_level_range_max, risk_Intensity.array))
                //    {
                //        return BadRequest("Error: Risk intensity rating Range Not Valid.");
                //    }
                //}



                // Set min and max using either the provided values or the existing ones.
                var newMin = risk_Intensity.risk_intensity_level_range_min ;
                var newMax = risk_Intensity.risk_intensity_level_range_max ;



                var existingRanges = await commonDBContext.risk_Intensities
                    .Where(d => d.risk_intensity_id != risk_Intensity.risk_intensity_id && d.risk_intensity_status == "Active")
                    .Select(d => new { MinValue = d.risk_intensity_level_range_min, MaxValue = d.risk_intensity_level_range_max })
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
                    if (risk_Intensity.risk_intensity_level_range_min != null &&
                        risk_Intensity.risk_intensity_level_range_max != null)
                    {

                        if (risk_Intensity.risk_intensity_level_range_min > risk_Intensity.risk_intensity_level_range_max)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                        }
                    }



                var existingDepartment2 = this.commonDBContext.risk_Intensities
               .FirstOrDefault(d => d.colour_reference == risk_Intensity.colour_reference && d.risk_intensity_id != risk_Intensity.risk_intensity_id&& d.risk_intensity_status == "Active");

                if (existingDepartment2 != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk color refrence with the same color already exists.");
                }



                // Proceed with the insertion
                risk_Intensity.risk_intensity_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Intensity.risk_intensity_status = "Active";

                this.commonDBContext.risk_Intensities.Add(risk_Intensity);
                this.commonDBContext.SaveChanges();

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





        [Route("api/RiskIntensity/UpdateRiskIntensity")]
        [HttpPut]
        public async Task< IActionResult> UpdateRiskIntensity([FromBody] risk_intensity risk_Intensity)
        {
            try
            {
                if (risk_Intensity.risk_intensity_id == 0)
                {
                    // Logic for handling new risk likelihood (insertion) goes here
                    return Ok("Insertion successful");
                }
                else
                {
                    // Trim and normalize the strings to avoid issues with leading/trailing spaces and case sensitivity
                    risk_Intensity.risk_intensity_name = risk_Intensity.risk_intensity_name?.Trim();

                    // Check for duplicates in each field while excluding the current record
                    var existingName = this.commonDBContext.risk_Intensities
                        .FirstOrDefault(d => d.risk_intensity_name == risk_Intensity.risk_intensity_name
                                          && d.risk_intensity_id != risk_Intensity.risk_intensity_id
                                          && d.risk_intensity_status == "Active");

                    if (existingName != null)
                    {
                        return BadRequest("Error: Risk intensity name with the same name already exists");
                    }
                    if (risk_Intensity.risk_intensity_level_range_min > risk_Intensity.risk_intensity_level_range_max)
                    {
                        return BadRequest("Error: Risk intensity Minimum value is greater that  intensity Maximum Value.");
                    }

                    var existingValues = await commonDBContext.risk_Intensities
           .Where(x => x.risk_intensity_id == risk_Intensity.risk_intensity_id)
           .Select(x => new
           {
               MinValue = x.risk_intensity_level_range_min,
               MaxValue = x.risk_intensity_level_range_max
           })
           .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Intensity.risk_intensity_level_range_min ?? existingValues.MinValue;
                    var newMax = risk_Intensity.risk_intensity_level_range_max ?? existingValues.MaxValue;



                    var existingRanges = await commonDBContext.risk_Intensities
                        .Where(d => d.risk_intensity_id != risk_Intensity.risk_intensity_id && d.risk_intensity_status == "Active")
                        .Select(d => new { MinValue = d.risk_intensity_level_range_min, MaxValue = d.risk_intensity_level_range_max })
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
                        if (risk_Intensity.risk_intensity_level_range_min == null &&
                            risk_Intensity.risk_intensity_level_range_max != null &&
                            existingValues.MinValue != 0)
                        {

                            if (existingValues.MinValue > risk_Intensity.risk_intensity_level_range_max)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                            }
                        }

                        if (risk_Intensity.risk_intensity_level_range_min != null &&
                            risk_Intensity.risk_intensity_level_range_max == null)
                        {


                            if (risk_Intensity.risk_intensity_level_range_min > existingValues.MaxValue)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Valuue.");
                            }



                        }
                    }
                    var existingColorReference = this.commonDBContext.risk_Intensities
                        .FirstOrDefault(d => d.colour_reference == risk_Intensity.colour_reference
                                          && d.risk_intensity_id != risk_Intensity.risk_intensity_id
                                          && d.risk_intensity_status == "Active");

                    if (existingColorReference != null)
                    {
                        return BadRequest("Error: Risk color reference with the same color already exists.");
                    }

                    // If no duplicates are found, proceed with the update
                    this.commonDBContext.Attach(risk_Intensity);
                    this.commonDBContext.Entry(risk_Intensity).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risk_Intensity);
                    Type type = typeof(risk_intensity);
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Intensity, null) == null || property.GetValue(risk_Intensity, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.commonDBContext.SaveChanges();
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




        [Route("api/RiskIntensity/DeleteRiskIntensity")]
        [HttpDelete]
        public void DeleteRiskIntensity(int id)
        {
            var currentClass = new risk_intensity { risk_intensity_id = id };
            currentClass.risk_intensity_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_intensity_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }






        //Initial Assessment Impact Factor 

        [Route("api/initialAssessmentImpact/GetinitialAssessmentImpactModelDetails")]
        [HttpGet]

        public IEnumerable<risk_initial_assessment_impact_factor> GetinitialAssessmentImpactModelDetails()
        {
            return this.commonDBContext.risk_Initial_Assessment_Impact_Factors.Where(x => x.risk_ini_ass_imp_status == "Active").ToList();
        }




        [Route("api/initialAssessmentImpact/InsertinitialAssessmentImpactModelDetails")]
        [HttpPost]
        public IActionResult InsertinitialAssessmentImpactModelDetails([FromBody] risk_initial_assessment_impact_factor risk_Initial_Assessment_Impact_Factor)
        {
            try
            {
                risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_name = risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_name?.Trim();

                var existingDepartment = this.commonDBContext.risk_Initial_Assessment_Impact_Factors
                    .FirstOrDefault(d => d.risk_ini_ass_imp_name == risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_name && d.risk_ini_ass_imp_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk initial assessment impact Name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.risk_Initial_Assessment_Impact_Factors;
                riskmatrixModel.Add(risk_Initial_Assessment_Impact_Factor);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_status = "Active";

                this.commonDBContext.SaveChanges();
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


        [Route("api/initialAssessmentImpact/UpdateinitialAssessmentImpactModelDetails")]
        [HttpPut]
        public IActionResult UpdateinitialAssessmentImpactModelDetails([FromBody] risk_initial_assessment_impact_factor risk_Initial_Assessment_Impact_Factor)
        {
            try
            {
                if (risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_name = risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_name?.Trim();

                    var existingDepartment = this.commonDBContext.risk_Initial_Assessment_Impact_Factors
                      .FirstOrDefault(d => d.risk_ini_ass_imp_name == risk_Initial_Assessment_Impact_Factor.risk_ini_ass_imp_name && d.risk_ini_ass_imp_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk initial assessment impact Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(risk_Initial_Assessment_Impact_Factor);
                    this.commonDBContext.Entry(risk_Initial_Assessment_Impact_Factor).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risk_Initial_Assessment_Impact_Factor);

                    Type type = typeof(risk_initial_assessment_impact_factor);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Initial_Assessment_Impact_Factor, null) == null || property.GetValue(risk_Initial_Assessment_Impact_Factor, null).Equals(0))
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
                    return BadRequest("Error: Risk initial assessment impact Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/initialAssessmentImpact/DeleteinitialAssessmentImpactModelDetails")]
        [HttpDelete]
        public void DeleteinitialAssessmentImpactModelDetails(int id)
        {
            var currentClass = new risk_initial_assessment_impact_factor { risk_ini_ass_imp_id = id };
            currentClass.risk_ini_ass_imp_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_ini_ass_imp_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }



        //Risk Mitigation Decision List

        [Route("api/risk_mitigation_decision/Getrisk_mitigation_decisionModelDetails")]
        [HttpGet]

        public IEnumerable<risk_mitigation_decision> Getrisk_mitigation_decisionModelDetails()
        {
            return this.commonDBContext.risk_Mitigation_Decisions.Where(x => x.Risk_mitigation_decision_status == "Active").ToList();
        }




        [Route("api/risk_mitigation_decision/Insertrisk_mitigation_decisionModelDetails")]
        [HttpPost]
        public IActionResult Insertrisk_mitigation_decisionModelDetails([FromBody] risk_mitigation_decision risk_Mitigation_Decision)
        {
            try
            {
                risk_Mitigation_Decision.Risk_mitigation_decision_name = risk_Mitigation_Decision.Risk_mitigation_decision_name?.Trim();

                var existingDepartment = this.commonDBContext.risk_Mitigation_Decisions
                    .FirstOrDefault(d => d.Risk_mitigation_decision_name == risk_Mitigation_Decision.Risk_mitigation_decision_name && d.Risk_mitigation_decision_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Risk mitigation decision Name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.risk_Mitigation_Decisions;
                riskmatrixModel.Add(risk_Mitigation_Decision);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Mitigation_Decision.Risk_mitigation_decision_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Mitigation_Decision.Risk_mitigation_decision_status = "Active";

                this.commonDBContext.SaveChanges();
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


        [Route("api/risk_mitigation_decision/Updaterisk_mitigation_decisionModelDetails")]
        [HttpPut]
        public IActionResult Updaterisk_mitigation_decisionModelDetails([FromBody] risk_mitigation_decision risk_Mitigation_Decision)
        {
            try
            {
                if (risk_Mitigation_Decision.Risk_mitigation_decision_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Mitigation_Decision.Risk_mitigation_decision_name = risk_Mitigation_Decision.Risk_mitigation_decision_name?.Trim();

                    var existingDepartment = this.commonDBContext.risk_Mitigation_Decisions
                      .FirstOrDefault(d => d.Risk_mitigation_decision_name == risk_Mitigation_Decision.Risk_mitigation_decision_name && d.Risk_mitigation_decision_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Risk Mitigation decision Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(risk_Mitigation_Decision);
                    this.commonDBContext.Entry(risk_Mitigation_Decision).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risk_Mitigation_Decision);

                    Type type = typeof(risk_mitigation_decision);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Mitigation_Decision, null) == null || property.GetValue(risk_Mitigation_Decision, null).Equals(0))
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
                    return BadRequest("Error: Risk mitigation decision Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/risk_mitigation_decision/Deleterisk_mitigation_decisionModelDetails")]
        [HttpDelete]
        public void Deleterisk_mitigation_decisionModelDetails(int id)
        {
            var currentClass = new risk_mitigation_decision { Risk_mitigation_decision_id = id };
            currentClass.Risk_mitigation_decision_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("Risk_mitigation_decision_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }




        //Assessment Control Acceptance Criteria

        [Route("api/risk_asses_contr_accep_crit/Getrisk_asses_contr_accep_critModelDetails")]
        [HttpGet]

        public IEnumerable<risk_asses_contr_accep_crit> Getrisk_asses_contr_accep_critModelDetails()
        {
            return this.commonDBContext.risk_Asses_Contr_Accep_Crits.Where(x => x.risk_Asses_contr_accep_crit_status == "Active").ToList();
        }




        [Route("api/risk_asses_contr_accep_crit/Insertrisk_asses_contr_accep_critModelDetails")]
        [HttpPost]
        public async Task< IActionResult> Insertrisk_asses_contr_accep_critModelDetails([FromBody] risk_asses_contr_accep_crit risk_Asses_Contr_Accep_Crit)
        {
            try
            {
                risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_name = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_name?.Trim();

                var existingDepartment = this.commonDBContext.risk_Asses_Contr_Accep_Crits
                    .FirstOrDefault(d => d.risk_Asses_contr_accep_crit_name == risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_name && d.risk_Asses_contr_accep_crit_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:Assessment Control Acceptance Criteria Name with the same name already exists.");
                }

                if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range > risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range)
                {
                    return BadRequest("Error:Assessment Control Acceptance CriteriaMinimum value is greater thatAssessment Control Acceptance CriteriaMaximum Value.");
                }

                //if (!CheckValidation(risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range, risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range, risk_Asses_Contr_Accep_Crit.array))
                //{
                //    return BadRequest("Error: Assessment Control Acceptance Range Not Valid.");
                //}



                var newMin = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range;
                var newMax = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range ;



                var existingRanges = await commonDBContext.risk_Asses_Contr_Accep_Crits
                    .Where(d => d.risk_Asses_contr_accep_crit_id != risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_id && d.risk_Asses_contr_accep_crit_status == "Active")
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
     
                    // Check validation for min and max values
                    if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range != null &&
                        risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range != null)
                    {

                        if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range > risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range)
                        {
                            return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
                        }
                    }
                    // Proceed with the insertion
                    var riskmatrixModel = this.commonDBContext.risk_Asses_Contr_Accep_Crits;
                riskmatrixModel.Add(risk_Asses_Contr_Accep_Crit);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_status = "Active";

                this.commonDBContext.SaveChanges();
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


        [Route("api/risk_asses_contr_accep_crit/Updaterisk_asses_contr_accep_critModelDetails")]
        [HttpPut]
        public async Task< IActionResult> Updaterisk_asses_contr_accep_critModelDetails([FromBody] risk_asses_contr_accep_crit risk_Asses_Contr_Accep_Crit)
        {
            try
            {
                if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_name = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_name?.Trim();

                    var existingDepartment = this.commonDBContext.risk_Asses_Contr_Accep_Crits
                      .FirstOrDefault(d => d.risk_Asses_contr_accep_crit_name == risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_name &&  d.risk_Asses_contr_accep_crit_id != risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_id && d.risk_Asses_contr_accep_crit_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Assessment Control Acceptance Criteria Name with the same name already exists.");
                    }

                    if (risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range > risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range)
                    {
                        return BadRequest("Error:Assessment Control Acceptance CriteriaMinimum value is greater thatAssessment Control Acceptance CriteriaMaximum Value.");
                    }

                    var existingValues = await commonDBContext.risk_Asses_Contr_Accep_Crits
   .Where(x => x.risk_Asses_contr_accep_crit_id == risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_id)
   .Select(x => new
   {
       MinValue = x.risk_Asses_contr_accep_crit_min_range,
       MaxValue = x.risk_Asses_contr_accep_crit_max_range
   })
   .FirstOrDefaultAsync();


                    // Set min and max using either the provided values or the existing ones.
                    var newMin = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_min_range ?? existingValues.MinValue;
                    var newMax = risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range ?? existingValues.MaxValue;



                    var existingRanges = await commonDBContext.risk_Asses_Contr_Accep_Crits
                        .Where(d => d.risk_Asses_contr_accep_crit_id != risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_id && d.risk_Asses_contr_accep_crit_status == "Active")
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

                            if (existingValues.MinValue > risk_Asses_Contr_Accep_Crit.risk_Asses_contr_accep_crit_max_range)
                            {
                                return BadRequest("Error: Risk Rating Minimum value is greater that Maximum Value.");
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


                    // Existing department, update logic

                    this.commonDBContext.Attach(risk_Asses_Contr_Accep_Crit);
                    this.commonDBContext.Entry(risk_Asses_Contr_Accep_Crit).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(risk_Asses_Contr_Accep_Crit);

                    Type type = typeof(risk_asses_contr_accep_crit);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(risk_Asses_Contr_Accep_Crit, null) == null || property.GetValue(risk_Asses_Contr_Accep_Crit, null).Equals(0))
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
                    return BadRequest("Error:Assessment Control Acceptance Criteria Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/risk_asses_contr_accep_crit/Deleterisk_asses_contr_accep_critModelDetails")]
        [HttpDelete]
        public void Deleterisk_asses_contr_accep_critModelDetails(int id)
        {
            var currentClass = new risk_asses_contr_accep_crit { risk_Asses_contr_accep_crit_id = id };
            currentClass.risk_Asses_contr_accep_crit_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("risk_Asses_contr_accep_crit_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }



        //Control Testing PARAMETERS/ Control RELEVANCE Category

         [Route("api/cont_test_cont_relevance/Getcont_test_cont_relevanceModelDetails")]
        [HttpGet]

        public IEnumerable<cont_test_cont_relevance> Getcont_test_cont_relevanceModelDetails()
        {
            return this.commonDBContext.cont_Test_Cont_Relevances.Where(x => x.cont_test_cont_relevance_status == "Active").ToList();
        }




        [Route("api/cont_test_cont_relevance/Insertcont_test_cont_relevanceModelDetails")]
        [HttpPost]
        public IActionResult Insertcont_test_cont_relevanceModelDetails([FromBody] cont_test_cont_relevance cont_Test_Cont_Relevance)
        {
            try
            {
                cont_Test_Cont_Relevance.cont_test_cont_relevance_name = cont_Test_Cont_Relevance.cont_test_cont_relevance_name?.Trim();

                var existingDepartment = this.commonDBContext.cont_Test_Cont_Relevances
                    .FirstOrDefault(d => d.cont_test_cont_relevance_name == cont_Test_Cont_Relevance.cont_test_cont_relevance_name && d.cont_test_cont_relevance_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error:Control Testing Parameters Name with the same name already exists.");
                }
                // Proceed with the insertion
                var riskmatrixModel = this.commonDBContext.cont_Test_Cont_Relevances;
                riskmatrixModel.Add(cont_Test_Cont_Relevance);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                cont_Test_Cont_Relevance.cont_test_cont_relevance_date = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                cont_Test_Cont_Relevance.cont_test_cont_relevance_status = "Active";

                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:Control Testing Parameters Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


        [Route("api/cont_test_cont_relevance/Updatecont_test_cont_relevanceModelDetails")]
        [HttpPut]
        public IActionResult Updatecont_test_cont_relevanceModelDetails([FromBody] cont_test_cont_relevance cont_Relevance)
        {
            try
            {
                if (cont_Relevance.cont_test_cont_relevance_id == 0)
                {
                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response

                    return Ok("Insertion successful");
                }
                else
                {
                    cont_Relevance.cont_test_cont_relevance_name = cont_Relevance.cont_test_cont_relevance_name?.Trim();

                    var existingDepartment = this.commonDBContext.cont_Test_Cont_Relevances
                      .FirstOrDefault(d => d.cont_test_cont_relevance_name == cont_Relevance.cont_test_cont_relevance_name && d.cont_test_cont_relevance_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error:Control Testing Parameters Name with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.commonDBContext.Attach(cont_Relevance);
                    this.commonDBContext.Entry(cont_Relevance).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(cont_Relevance);

                    Type type = typeof(cont_test_cont_relevance);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(cont_Relevance, null) == null || property.GetValue(cont_Relevance, null).Equals(0))
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
                    return BadRequest("Error:Control Testing Parameters Name with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/cont_test_cont_relevance/Deletecont_test_cont_relevanceModelDetails")]
        [HttpDelete]
        public void Deletecont_test_cont_relevanceModelDetails(int id)
        {
            var currentClass = new cont_test_cont_relevance { cont_test_cont_relevance_id = id };
            currentClass.cont_test_cont_relevance_status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("cont_test_cont_relevance_status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }



    }
}
