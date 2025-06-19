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
    public class UnitController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public UnitController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/UnitMaster/GetUnitMasterDetails")]
        [HttpGet]

        public IEnumerable<UnitModel> GetUnitMasterDetails()
        {
            return this.mySqlDBContext.UnitModels.Where(x => x.UnitStatus == "Active").ToList();
        }


        [Route("api/UnitMaster/InsertUnitMasterDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] UnitModel UnitModel)
        {
            var UnitModels = this.mySqlDBContext.UnitModels;
            UnitModels.Add(UnitModel);
            //DateTime dt = DateTime.Now;
            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //SectorModels.Department_Master_CreatedDate = dt1;
            UnitModel.UnitStatus = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/UnitMaster/UpdateUnitMasterDetails")]
        [HttpPut]

        public void UpdateUnitMasterDetails([FromBody] UnitModel UnitModels)
        {
            if (UnitModels.UnitMasterID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(UnitModels);
                this.mySqlDBContext.Entry(UnitModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(UnitModels);

                Type type = typeof(UnitModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(UnitModels, null) == null || property.GetValue(UnitModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }


        [Route("api/UnitMaster/DeleteUnitMasterDetails")]
        [HttpDelete]

        public void DeleteUnitMasterDetails(int id)
        {
            var currentClass = new UnitModel { UnitMasterID = id };
            currentClass.UnitStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("UnitStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
