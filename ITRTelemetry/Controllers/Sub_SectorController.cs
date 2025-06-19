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
    public class Sub_SectorController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;

        public Sub_SectorController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/SubSectorMaster/GetSubSectorMasterDetails")]
        [HttpGet]

        public IEnumerable<Sub_SectorModel> GetSubSectorMasterDetails()
        {
            return this.mySqlDBContext.Sub_SectorModels.Where(x => x.Sub_SectorStatus == "Active").ToList();
        }
       

        [Route("api/SubSectorMaster/InsertSubSectorMasterDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] Sub_SectorModel Sub_SectorModel)
        {
            var Sub_SectorModels = this.mySqlDBContext.Sub_SectorModels;
            Sub_SectorModels.Add(Sub_SectorModel);
            //DateTime dt = DateTime.Now;
            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //SectorModels.Department_Master_CreatedDate = dt1;
            Sub_SectorModel.Sub_SectorStatus = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/SubSectorMaster/UpdateSubSectorMasterDetails")]
        [HttpPut]

        public void UpdateSubSectorMasterDetails([FromBody] Sub_SectorModel Sub_SectorModels)
        {
            if (Sub_SectorModels.SubSectorMasterID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(Sub_SectorModels);
                this.mySqlDBContext.Entry(Sub_SectorModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(Sub_SectorModels);

                Type type = typeof(Sub_SectorModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(Sub_SectorModels, null) == null || property.GetValue(Sub_SectorModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }


        [Route("api/SubSectorMaster/DeleteSubSectorMasterDetails")]
        [HttpDelete]

        public void DeleteSubSectorMasterDetails(int id)
        {
            var currentClass = new Sub_SectorModel { SubSectorMasterID = id };
            currentClass.Sub_SectorStatus = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Sub_SectorStatus").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }
    }
}
