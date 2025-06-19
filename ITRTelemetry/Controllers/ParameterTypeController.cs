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

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class ParameterTypeController
    {
        private readonly MySqlDBContext mySqlDBContext;

        public ParameterTypeController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/ParameterType/GetParamType")]
        [HttpGet]
        public IEnumerable<ParameterTypeModel> Get()
        {
            return this.mySqlDBContext.ParameterTypeModels.Where(x => x.Status == "Active").ToList();
        }

        [Route("api/ParameterType/InsertParameterType")]
        [HttpPost]
        public void InsertParameter([FromBody] ParameterTypeModel ParameterTypeModels)
        {
            
            var parameterTypeModels = this.mySqlDBContext.ParameterTypeModels;
            parameterTypeModels.Add(ParameterTypeModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            ParameterTypeModels.DateTime = dt1;
            ParameterTypeModels.Status = "Active";
            this.mySqlDBContext.SaveChanges();
           

        }


        [Route("api/ParameterType/UpdateParameter")]
        [HttpPut]
        public void UpdateParameter([FromBody] ParameterTypeModel ParameterTypeModels)
        {
            if (ParameterTypeModels.ParameterTypeID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(ParameterTypeModels);
                this.mySqlDBContext.Entry(ParameterTypeModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(ParameterTypeModels);

                Type type = typeof(ParameterTypeModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(ParameterTypeModels, null) == null || property.GetValue(ParameterTypeModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }

        }


        [Route("api/ParameterType/DeleteParameter")]
        [HttpDelete]
        public void DeleteParameter(int id)
        {
            var currentClass = new ParameterTypeModel { ParameterTypeID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
