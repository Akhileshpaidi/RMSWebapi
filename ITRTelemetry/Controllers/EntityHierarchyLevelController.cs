using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]

    public class EntityHierarchyLevelController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        private IConfiguration constring;
        public EntityHierarchyLevelController(MySqlDBContext mySqlDBContext,IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            constring = configuration;
        }

        [Route("api/EntityHierarchyLevel/GetEntityHierarchyLevels")]
        [HttpGet] 
        public ActionResult GetEntityHierarchyLevels()
        {
            var result=this.mySqlDBContext.EntityHeirarchyLevelModels.ToList();
            return Ok(result);
        }
        [Route("api/EntityHierarchyLevel/UpdateEntityHierarchyLevels")]
        [HttpPost]
        public ActionResult UpdateEntityHierarchyLevels([FromBody] List<EntityHeirarchyLevelModel> EntityHeirarchyLevelModels)
        {
            foreach (var model in EntityHeirarchyLevelModels)
            {
                model.ModifiedDate = DateTime.Now;
                var existingEntity = this.mySqlDBContext.EntityHeirarchyLevelModels.Find(model.EntityHeirarchyLevelID);
                if (existingEntity != null)
                {
                    this.mySqlDBContext.Entry(existingEntity).CurrentValues.SetValues(model);
                }
            }
            this.mySqlDBContext.SaveChanges();
            return Ok("Data Updated Successfully");
        }
    }
}
