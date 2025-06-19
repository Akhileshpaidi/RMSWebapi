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
    public class SectorController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public SectorController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/SectorMaster/GetSectorMasterDetails")]
        [HttpGet]

        public IEnumerable<SectorModel> GetDepartmentMasterDetails()
        {
            return this.mySqlDBContext.SectorModels.Where(x => x.Status == "Active").ToList();
        }
        [Route("api/SectorMaster/InsertSectorMasterDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] SectorModel SectorModel)
        {
            var SectorModels = this.mySqlDBContext.SectorModels;
            SectorModels.Add(SectorModel);
            //DateTime dt = DateTime.Now;
            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //SectorModels.Department_Master_CreatedDate = dt1;
            SectorModel.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/SectorMaster/UpdateSectorMasterDetails")]
        [HttpPut]

        public void UpdateSectorMasterDetails([FromBody] SectorModel SectorModels)
        {
            if (SectorModels.SectorID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(SectorModels);
                this.mySqlDBContext.Entry(SectorModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(SectorModels);

                Type type = typeof(SectorModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(SectorModels, null) == null || property.GetValue(SectorModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }


        [Route("api/SectorMaster/DeleteSectorMasterDetails")]
        [HttpDelete]

        public void DeleteSectorMasterDetails(int id)
        {
            var currentClass = new SectorModel { SectorID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
   

}
