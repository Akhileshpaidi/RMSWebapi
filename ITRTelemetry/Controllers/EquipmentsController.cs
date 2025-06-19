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

    [ApiController]
    public class EquipmentsController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public EquipmentsController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/Equipments/GeEquipmentDetails")]
        [HttpGet]

        public IEnumerable<EquipmentsModel> Get()
        {
            return this.mySqlDBContext.EquipmentsModels.Where(x => x.Status == "Active").ToList();
        }

        [Route("api/Equipments/InsertEquipment")]
        [HttpPost]
        public IActionResult InsertEquipment([FromBody] EquipmentsModel EquipmentModels)
        {
            var equipmentModels = this.mySqlDBContext.EquipmentsModels;
            equipmentModels.Add(EquipmentModels);
           
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            EquipmentModels.DateTime = dt1;
            EquipmentModels.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/Equipments/UpdateEquipment")]
        [HttpPut]
        public void UpdateEquipment([FromBody] EquipmentsModel EquipmentModels)
        {
            if (EquipmentModels.EquipmentID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(EquipmentModels);
                this.mySqlDBContext.Entry(EquipmentModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(EquipmentModels);

                Type type = typeof(EquipmentsModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(EquipmentModels, null) == null)
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
           
        }

        [Route("api/Equipments/DeleteEquipment")]
        [HttpDelete]
        public void DeleteEquipment(int id)
        {
            var currentClass = new EquipmentsModel { EquipmentID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        }
}
