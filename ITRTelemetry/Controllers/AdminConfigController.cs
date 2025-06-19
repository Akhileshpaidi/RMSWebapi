using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.CodeAnalysis;
using DomainModel;

namespace ITRTelemetry.Controllers
{

    [Produces("application/json")]
    public class AdminConfigController: ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public AdminConfigController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/AdminConfig/GetAdminConfigDetails")]
        [HttpGet]

        public IEnumerable<DirectuploadModel> GetAdminConfigDetails()
        {

            return this.mySqlDBContext.DirectuploadModels.Where(x => x.admin_config_status == "Active").ToList();

        }

     

    }
}
