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
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class Defaultmodulerole : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Defaultmodulerole(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/defaulttaskuser/Getdefaulttaskuser")]
        [HttpGet]

        public IEnumerable<object> Getdefaulttaskuser()
        {

            var Details =( from defaultrolemaster in mySqlDBContext.defualtmodulerolemodels
                          join taskmaster in mySqlDBContext.TaskModels on defaultrolemaster.task_id equals taskmaster.task_id
                          join rolemaster in mySqlDBContext.RoleModels on defaultrolemaster.ROLE_ID equals rolemaster.ROLE_ID

                          select new
                          {
                              defaultrolemaster.idDefaultTaskuser,
                              taskmaster.task_id,
                              taskmaster.task_name,
                              rolemaster.ROLE_ID,
                              rolemaster.ROLE_NAME

                          })
                            .ToList();
            return Details;

        }
    }
}
