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
using Newtonsoft.Json;

namespace ITR_TelementaryAPI.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class GetTypeofDataController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public GetTypeofDataController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/GetTypeofData/GetData")]
        [HttpGet]
        public IEnumerable<TypeofData> Get()
        {
            return this.mySqlDBContext.TypeofDatas.Where(x => x.Status == "Active").ToList();
        }

        [Route("api/GetTypeofData/InsertTypeData")]
        [HttpPost]
        public IActionResult InsertTypeofData([FromBody] TypeofData TypeofDatas)
        {
            var typeofData = this.mySqlDBContext.TypeofDatas;
            typeofData.Add(TypeofDatas);
            
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            TypeofDatas.DateTime = dt1;
            TypeofDatas.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }

        [Route("api/GetTypeofData/UpdateTypeData")]
        [HttpPut]
        public void UpdateTypeofData([FromBody] TypeofData TypeofDatas)
        {
            if (TypeofDatas.TIID == 0)
            {
                //this.mySqlDBContext.TypeofDatas.Add(TypeofDatas);
            }

            else
            {
                this.mySqlDBContext.Attach(TypeofDatas);
                this.mySqlDBContext.Entry(TypeofDatas).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(TypeofDatas);

                Type type = typeof(TypeofData);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(TypeofDatas, null) == null)
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();

            }


        }

        [Route("api/GetTypeofData/DeleteTypeData")]
        [HttpDelete]
        public void DeleteTypeofData(int id)
        {
            var currentClass = new TypeofData { TIID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
           this.mySqlDBContext.SaveChanges();

        }

    }
}
