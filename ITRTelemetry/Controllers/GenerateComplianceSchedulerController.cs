using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using DomainModel;
using System.Reflection;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class GenerateComplianceSchedulerController : ControllerBase
    {
        private MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
       // private readonly BatchComplianceGeneration _batchComplianceGeneration;
        public GenerateComplianceSchedulerController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
          //  _batchComplianceGeneration = batchComplianceGeneration;
        }

        [Route("api/GenerateComplianceScheduler/CreateBatchCompanyCompliance")]
        [HttpGet]
       // public void ProcessComplianceData()
        public IEnumerable<object> ProcessComplianceData()
        {
            // Get today's date and calculate 90 days ago
            // Get today's date and calculate 90 days ago
            DateTime today = DateTime.Now;
            DateTime ninetyDaysAgo = today.AddDays(-90);

            // Convert 90-days-ago date to a string in "yyyy-MM-dd" format
            string ninetyDaysAgoString = ninetyDaysAgo.ToString("yyyy-MM-dd");

            var filteredData = mySqlDBContext.CurrentBatchComplianceModels
                .Where(t => string.Compare(t.startDate, ninetyDaysAgoString) <= 0) // Correct comparison
                .Select(t => new
                {
                  //  t.userId,
                    t.createCompanyComplianceId,
                    t.batchId,
                    t.startDate,
                    t.frequency,
                })
                .ToList();



            // Step 2: Process each filtered record
            foreach (var data in filteredData)
            {

                //List<List<BatchCompliance>> schedulerData = _batchComplianceGeneration.ProcessComplianceSchedulerData(data.createCompanyComplianceId, data.userId, data.batchId);
            }

            // Save changes to the database
            mySqlDBContext.SaveChanges();
            return null;
        }
    }
}



