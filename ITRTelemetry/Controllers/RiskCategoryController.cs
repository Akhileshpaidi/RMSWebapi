using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class RiskCategoryController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public RiskCategoryController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/risk_category/GetriskcategoryDetails")]
        [HttpGet]

        public IEnumerable<RiskCategoryModel> GetNatureOfLawDetails()
        {
            return this.mySqlDBContext.RiskCategoryModels.Where(x => x.RiskCategoryStatus == "Active").ToList();
        }


        [Route("api/risk_category/InsertriskcategoryDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] RiskCategoryModel RiskCategoryModel)
        {
            var RiskCategoryModels = this.mySqlDBContext.RiskCategoryModels;
            RiskCategoryModels.Add(RiskCategoryModel);
            //DateTime dt = DateTime.Now;
            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //SectorModels.Department_Master_CreatedDate = dt1;
            RiskCategoryModel.RiskCategoryStatus = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/risk_category/UpdateriskcategoryDetails")]
        [HttpPut]

        public void UpdateriskcategoryDetails([FromBody] RiskCategoryModel RiskCategoryModels)
        {
            if (RiskCategoryModels.RiskCategoryMasterID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(RiskCategoryModels);
                this.mySqlDBContext.Entry(RiskCategoryModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(RiskCategoryModels);

                Type type = typeof(RiskCategoryModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(RiskCategoryModels, null) == null || property.GetValue(RiskCategoryModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }


        [Route("api/Nature_Of_Law/DeleteriskcategoryDetails")]
        [HttpDelete]

        public void DeleteriskcategoryDetails(int id)
        {
            var currentClass = new RiskCategoryModel { RiskCategoryMasterID = id };
            currentClass.RiskCategoryStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("RiskCategoryStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
