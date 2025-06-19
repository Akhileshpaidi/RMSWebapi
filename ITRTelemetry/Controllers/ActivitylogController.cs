using DomainModel;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]

    public class ActivitylogController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public ActivitylogController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        //Getting Activitylog Details

        [Route("api/Activitylog/GetActivitylog")]
        [HttpGet]

        public IEnumerable<ActivitylogModel> GetActivitylog()
        {
            return this.mySqlDBContext.ActivitylogModels.ToList();
        }

        //Insert Activitylog Details

        //[Route("api/Activitylog/InsertActivitylog")]
        //[HttpPost]
        //public IActionResult InsertParameter([FromBody] ActivitylogModel ActivitylogModels)
        //{
        //    var ActivitylogModel = this.mySqlDBContext.ActivitylogModels;
        //    ActivitylogModel.Add(ActivitylogModels);
        //    DateTime dt = DateTime.Now;
        //    //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //    ActivitylogModels.CreatedDate = dt;
        //    ActivitylogModels.DocumentFileName = "Krishna";
        //    this.mySqlDBContext.SaveChanges();
        //    return Ok();
        //}






    }
}
