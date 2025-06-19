using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class Getuserbylocationcontroller : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Getuserbylocationcontroller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/userlocationmapping/GetuserlocationmappingDetails/{unit_location_mapping_id}")]
        [HttpGet]
        public IEnumerable<object> GetUnitLocationDetails(int unit_location_mapping_id)
        {

            //return this.mySqlDBContext.userlocationmappingModels.Where(x => x.user_location_mapping_status == "Active" && x.user_location_mapping_id == user_location_mapping_id).ToList();


            var deatils = (from tblusermaster in mySqlDBContext.usermodels
                            join usermappingtmaster in mySqlDBContext.userlocationmappingModels on tblusermaster.USR_ID equals usermappingtmaster.USR_ID
                           where  usermappingtmaster.user_location_mapping_status == "Active" && usermappingtmaster.Unit_location_Master_id == unit_location_mapping_id 
                           select  new { 
                                 tblusermaster.firstname,
                                 tblusermaster.lastname,
                                usermappingtmaster.USR_ID,
                            })
                            .Distinct();
            return deatils;
        }
    }
}
