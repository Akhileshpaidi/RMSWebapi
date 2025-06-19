using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using DomainModel;
using System.Data;
using System.Text.RegularExpressions;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RiskDefaultNotifiersController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RiskDefaultNotifiersController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("api/RiskDefaultNotifiers/GetRiskDefaultNotifiers")]
        [HttpGet]

        public IEnumerable<object> GetRiskDefaultNotifiers()
        {
            var activeNotifiers = this.mySqlDBContext.RiskDefaultNotifierss
                                         .Where(x => x.Status == "Active")
                                         .ToList();

            var modifiedNotifiers = activeNotifiers.Select(x => new
            {
                x.RiskDefaultNotifiersID,
                x.Entity_Master_id,
                x.Unit_location_Master_id,
                x.Department_Master_id,
                emailid = x.emailid.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                additional_emailid_notifiers = (x.additional_emailid_notifiers != null) ?
                x.additional_emailid_notifiers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : new string[0],
                x.Status
            }).ToList();

            return modifiedNotifiers;

        }

        [Route("api/RiskDefaultNotifiers/InsertRiskDefaultNotifiers")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] RiskDefaultNotifiers RiskDefaultNotifiers)
        {
            var RiskDefaultNotifierss = this.mySqlDBContext.RiskDefaultNotifierss;
            RiskDefaultNotifierss.Add(RiskDefaultNotifiers);
            //DateTime dt = DateTime.Now;
            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //DefaultNotifiresModels.CreatedDate = dt1;
            RiskDefaultNotifiers.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/RiskDefaultNotifiers/UpdateRiskDefaultNotifiers")]
        [HttpPut]

        public void UpdateRiskDefaultNotifiers([FromBody] RiskDefaultNotifiers RiskDefaultNotifierss)
        {
            if (RiskDefaultNotifierss.RiskDefaultNotifiersID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(RiskDefaultNotifierss);
                this.mySqlDBContext.Entry(RiskDefaultNotifierss).State = EntityState.Modified;
                var entry = this.mySqlDBContext.Entry(RiskDefaultNotifierss);
                Type type = typeof(RiskDefaultNotifiers);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(RiskDefaultNotifierss, null) == null || property.GetValue(RiskDefaultNotifierss, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }

        [Route("api/RiskDefaultNotifiers/DeleteRiskDefaultNotifiers")]
        [HttpDelete]

        public void DeleteRiskDefaultNotifiers(int id)
        {
            var currentClass = new RiskDefaultNotifiers { RiskDefaultNotifiersID = id };
            currentClass.Status = "InActive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
