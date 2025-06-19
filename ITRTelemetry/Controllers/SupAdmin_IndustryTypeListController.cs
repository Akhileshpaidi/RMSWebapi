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
using Microsoft.Extensions.Configuration;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class SupAdmin_IndustryTypeListController : Controller
    {
        private CommonDBContext commonDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_IndustryTypeListController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
        }
        [Route("api/SuperAdminIndustryTypeList/GetIndustryTypeListDetails")]
        [HttpGet]

        public IEnumerable<SupAdmin_IndustryTypeListModel> GetIndustryTypeListDetails()
        {
            return this.commonDBContext.SupAdmin_IndustryTypeListModels.Where(x => x.status == "Active").ToList();
        }

        [Route("api/SuperAdminIndustryTypeList/GetindustryByBusinessID/{businessIDs}")]
        [HttpGet]
        
        public IActionResult GetIndustryByBusinessIDs(string businessIDs)
        {
            // Split the input string into a list of integers
            var businessIDsList = businessIDs.Split(',').Select(int.Parse).ToList();

            // Perform the query
            var result = (from business in commonDBContext.SupAdmin_BusinessSectorListModels
                          join industry in commonDBContext.SupAdmin_IndustryTypeListModels
                          on business.businesssectorid equals industry.businesssectorid
                          into industriesGroup
                          from industry in industriesGroup.DefaultIfEmpty()
                          where businessIDsList.Contains(business.businesssectorid) && industry.status == "Active"
                          select new
                          {
                              BusinessName = business.businesssectorname,
                              IndustryName = industry.industrytypename,
                              IndustryTypeID = industry.industrytypeid,
                          })
                         .GroupBy(x => x.BusinessName)
                         .Select(g => new
                         {
                             BusinessName = g.Key,
                             industrytypename = g.Select(x => x.IndustryName).ToList(),
                             industrytypeid = g.Select(x => x.IndustryTypeID).ToList()
                         })
                         .ToList();

            // Return the result
            return Ok(result);
        }


        [Route("api/SuperAdminIndustryTypeList/InsertAdminIndustryTypeListDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] SupAdmin_IndustryTypeListModel SupAdmin_IndustryTypeListModels)
        {
            try
            {
                SupAdmin_IndustryTypeListModels.industrytypename = SupAdmin_IndustryTypeListModels.industrytypename?.Trim();
                var existingDepartment = this.commonDBContext.SupAdmin_IndustryTypeListModels
                    .FirstOrDefault(d => d.industrytypename == SupAdmin_IndustryTypeListModels.industrytypename && d.status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                // Proceed with the insertion


                var Maxindid = this.commonDBContext.SupAdmin_IndustryTypeListModels.Max(d => (int?)d.industrytypeid) ?? 5000;

                SupAdmin_IndustryTypeListModels.industrytypeid = Maxindid + 1;

                var SupAdmin_IndustryTypeListModel = this.commonDBContext.SupAdmin_IndustryTypeListModels;
                SupAdmin_IndustryTypeListModel.Add(SupAdmin_IndustryTypeListModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SupAdmin_IndustryTypeListModels.createddate = dt1;
                SupAdmin_IndustryTypeListModels.status = "Active";
                SupAdmin_IndustryTypeListModels.industrytyptable = "industrytype";
                this.commonDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [Route("api/SuperAdminIndustryTypeList/UpdateIndustryTypeListDetails")]
        [HttpPut]
        public IActionResult UpdateType([FromBody] SupAdmin_IndustryTypeListModel SupAdmin_IndustryTypeListModels)
        {
            try
            {
                if (SupAdmin_IndustryTypeListModels.industrytypeid == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    SupAdmin_IndustryTypeListModels.industrytypename = SupAdmin_IndustryTypeListModels.industrytypename?.Trim();
                    var existingDepartment = this.commonDBContext.SupAdmin_IndustryTypeListModels
                  .FirstOrDefault(d => d.industrytypename == SupAdmin_IndustryTypeListModels.industrytypename && d.industrytypeid != SupAdmin_IndustryTypeListModels.industrytypeid && d.status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: TypeName with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.commonDBContext.Attach(SupAdmin_IndustryTypeListModels);
                    this.commonDBContext.Entry(SupAdmin_IndustryTypeListModels).State = EntityState.Modified;

                    var entry = this.commonDBContext.Entry(SupAdmin_IndustryTypeListModels);

                    Type type = typeof(SupAdmin_IndustryTypeListModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(SupAdmin_IndustryTypeListModels, null) == null || property.GetValue(SupAdmin_IndustryTypeListModels, null).Equals(0))
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
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: TypeName with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }



        [Route("api/SuperAdminIndustryTypeList/DeleteIndustryTypeListDetails")]
        [HttpDelete]
        public void DeleteentityType(int id)
        {
            var currentClass = new SupAdmin_IndustryTypeListModel { industrytypeid = id };
            currentClass.status = "Inactive";
            this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
            this.commonDBContext.SaveChanges();
        }

    }
}
