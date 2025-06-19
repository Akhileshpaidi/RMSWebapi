using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using MySQLProvider;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class PeriodicityFactor : ControllerBase
    {
        private MySqlDBContext MySqlDBContext;
        public IConfiguration configuration { get; }

        public PeriodicityFactor(MySqlDBContext mysqldbcontext, IConfiguration Configuration)
        {
            this.MySqlDBContext = mysqldbcontext;
            this.configuration = Configuration;
        }


        [Route("api/PeriodicityFactor/GetPeriodicityFactorDetails")]
        [HttpGet]
        public IActionResult GetAllPeriodicityFactors()
        {
            try
            {
                var factors = this.MySqlDBContext.PeriodicityFactorModels.ToList();
                return Ok(factors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching periodicity factors.", error = ex.Message });
            }
        }

        [Route("api/PeriodicityFactor/GetMonthDetails")]
        [HttpGet]
        public IActionResult GetAllMonths()
        {
            try
            {
                var months = this.MySqlDBContext.MonthModels.ToList();
                return Ok(months);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching month details.", error = ex.Message });
            }
        }
    }
}
