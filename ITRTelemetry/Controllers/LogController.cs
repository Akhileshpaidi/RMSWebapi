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
    public class LogController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public LogController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting Log Details

        [Route("api/log/GetlogDetails")]
        [HttpGet]

        public IEnumerable<LogModel> GetlogDetails()
        {
            return this.mySqlDBContext.LogModels.ToList();
        }

        ////Insert Role Details

        //[Route("api/log/InsertLogDetails")]
        //[HttpPost]
        //public IActionResult InsertParameter([FromBody] LogModel LogModels)
        //{
        //    var LogModel = this.mySqlDBContext.LogModels;
        //    LogModel.Add(LogModels);
        //    DateTime dt = DateTime.Now;
        //    //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //    LogModels.UPDATED_TIME = dt;
        //    LogModels.LOG_INFO = "Krishna";
        //    this.mySqlDBContext.SaveChanges();
        //    return Ok();
        //}

    }
}
