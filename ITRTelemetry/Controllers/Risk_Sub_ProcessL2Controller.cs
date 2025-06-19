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
namespace ITRTelemetry.Controllers

{
    [Produces("application/json")]
    public class Risk_Sub_ProcessL2Controller : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Risk_Sub_ProcessL2Controller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/BusinessSubProcessL2/GetBusinessSubProcesssL2")]
        [HttpGet]

        public IEnumerable<Risk_Sub_ProcessL2> GetBusinessSubProcesssL2()
        {
            return this.mySqlDBContext.Risk_Sub_ProcessL2s.Where(x => x.ProcessL2Status == "Active").ToList();
        }
        //public IEnumerable<object> GetBusinessSubProcesssL2()
        //{
        //    var details = (from SubProcess2 in this.mySqlDBContext.Risk_Sub_ProcessL2s
        //                   join businessProcessL1 in this.mySqlDBContext.Risk_Sub_ProcessL1s
        //                   on SubProcess2.BusinessProcessL1ID equals businessProcessL1.BusinessProcessL1ID
        //                   join Process in this.mySqlDBContext.Risk_BusinessProcesss on SubProcess2.businessprocessID equals Process.businessprocessID
        //                   where SubProcess2.ProcessL2Status == "Active"
        //                   select new
        //                   {

        //                       SubProcess2.BusinessProcessL2ID,
        //                       SubProcess2.entityid,
        //                     SubProcess2.unitlocationid,
        //                       SubProcess2.departmentid,
        //                       SubProcess2.riskBusinessfunctionid,
        //                   SubProcess2.BusinessSubProcessL2Name,
        //                      SubProcess2.BusinessSubProcessL2Description,
        //                       SubProcess2.SubProcessObjestive,
        //                       Process.BusinessProcessName,
        //                       Process.businessprocessID,
        //                   })
        //                     .Distinct()
        //         .ToList();

        //    return details;
        //}



        [Route("api/BusinessSubProcessL2/InsertBusinessSubProcessL2")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] Risk_Sub_ProcessL2 Risk_Sub_ProcessL2s)
        {
            try
            {
                Risk_Sub_ProcessL2s.BusinessSubProcessL2Name = Risk_Sub_ProcessL2s.BusinessSubProcessL2Name?.Trim();

                var existingDepartment = this.mySqlDBContext.Risk_Sub_ProcessL2s
                    .FirstOrDefault(d => d.BusinessSubProcessL2Name == Risk_Sub_ProcessL2s.BusinessSubProcessL2Name && d.BusinessProcessL2ID == Risk_Sub_ProcessL2s.BusinessProcessL2ID && d.ProcessL2Status == "Active");


                if (existingDepartment != null)
                {
                    return BadRequest("Error:  Business Sub-ProcessL2 with the same name already exists.");
                }

                var Risk_Sub_ProcessL2 = this.mySqlDBContext.Risk_Sub_ProcessL2s;
                Risk_Sub_ProcessL2.Add(Risk_Sub_ProcessL2s);
                //DateTime dt = DateTime.Now;
                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                //AuthorityNameModels.Authority_CreatedDate = dt1;

                //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                Risk_Sub_ProcessL2s.ProcessL2Status = "Active";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    return BadRequest("Error:  Business Sub-ProcessL2 with the same name already exists.");
                }
                else
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
        [Route("api/BusinessSubProcessL2/UpdateBusinessSubProcessL2")]
        [HttpPut]
        public IActionResult UpdateBusiness([FromBody] Risk_Sub_ProcessL2 Risk_Sub_ProcessL2s)
        {
            try
            {
                if (Risk_Sub_ProcessL2s.BusinessProcessL2ID == 0)
                {
                    return Ok("Insertion successful");
                }
                else
                {
                    Risk_Sub_ProcessL2s.BusinessSubProcessL2Name = Risk_Sub_ProcessL2s.BusinessSubProcessL2Name?.Trim();

                    var existingDepartment = this.mySqlDBContext.Risk_Sub_ProcessL2s
                     .FirstOrDefault(d => d.BusinessSubProcessL2Name == Risk_Sub_ProcessL2s.BusinessSubProcessL2Name && d.BusinessProcessL2ID != Risk_Sub_ProcessL2s.BusinessProcessL2ID && d.ProcessL2Status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Business Sub-ProcessL2 with the same name already exists.");
                    }

                    // Existing department, update logic
                    this.mySqlDBContext.Attach(Risk_Sub_ProcessL2s);
                    this.mySqlDBContext.Entry(Risk_Sub_ProcessL2s).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(Risk_Sub_ProcessL2s);

                    Type type = typeof(Risk_Sub_ProcessL2);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(Risk_Sub_ProcessL2s, null) == null || property.GetValue(Risk_Sub_ProcessL2s, null).Equals(0))
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
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error:  Business Sub-ProcessL2 with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }

        }

        [Route("api/BusinessSubProcessL2/DeleteBusinessSubProcessL2")]
        [HttpDelete]
        public void DeleteBusinessSubProcessL2(int id)
        {
            var currentClass = new Risk_Sub_ProcessL2 { BusinessProcessL2ID = id };
            currentClass.ProcessL2Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("ProcessL2Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
