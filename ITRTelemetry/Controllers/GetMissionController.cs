using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;

namespace ITR_TelementaryAPI.Controllers
{
    [Produces("application/json")]
    public class GetMissionController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public GetMissionController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;

        }

        // GET: api/<GetMissionController>
        // GET: api/GetMission
        [Route("api/GetMission/GetMissionDetails")]
        [HttpGet]
        public IEnumerable<Object> Get()
        {
            return this.mySqlDBContext.MissionModels.Where(x => x.Status == "Active").ToList();
        }

        [Route("api/GetMission/InsertMission")]
        [HttpPost]
        public IActionResult InsertMission([FromBody] MissionModel MissionModels)
        {
            var missionModels = this.mySqlDBContext.MissionModels;
            missionModels.Add(MissionModels);
            
            DateTime dt = DateTime.Now;
            string date = dt.ToString("yyyy-MM-dd");
            string time = dt.ToString("HH:mm:ss");
            MissionModels.Date = date;
            MissionModels.Time = time;
            MissionModels.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();

        }

        [Route("api/GetMission/GetMissionByYear")]
        [HttpGet]
        public IEnumerable<MissionModel> GetByYear(string ID)
        {
            return this.mySqlDBContext.MissionModels.Where(x => x.Year == ID).ToList();
        }

        [Route("api/GetMission/UpdateMission")]
        [HttpPut]
        public void UpdateMission([FromBody] MissionModel MissionModels)
        {
            if (MissionModels.MissionID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(MissionModels);
                this.mySqlDBContext.Entry(MissionModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(MissionModels);

                Type type = typeof(MissionModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(MissionModels, null) == null || property.GetValue(MissionModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
           

        }

        [Route("api/GetMission/DeleteMission")]
        [HttpDelete]
        public void DeleteMission(int id)
        {
            var currentClass = new MissionModel { MissionID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        [Route("api/GetMission/getEquipmentsByMission")]
        [HttpGet]
        public IEnumerable<object> getEquipmentsByMission(int id)
        {

            var ParamData = (from missiontable in mySqlDBContext.MissionModels
                             join equipmentmaster in mySqlDBContext.EquipmentsModels on missiontable.EquipmentID equals equipmentmaster.EquipmentID
                             select new
                             {
                                 missiontable.MissionID,
                                 equipmentmaster.EquipmentID,
                                 equipmentmaster.EquipmentName

                             }).Where(x => x.MissionID == id).ToList();


            return ParamData;
        }

        [Route("api/GetMission/GetMissionBySystem")]
        [HttpGet]
        public IEnumerable<MissionModel> GetBySystem(int SystemID,string Year)
        {
            return this.mySqlDBContext.MissionModels.Where(x => x.EquipmentID == SystemID && x.Year == Year).ToList();
        }

        [Route("api/GetMission/GetMissions")]
        [HttpGet]
        public IEnumerable<MissionModel> GetMissions(int SystemID)
        {
            return this.mySqlDBContext.MissionModels.Where(x => x.EquipmentID == SystemID ).ToList();
        }

    }
}
