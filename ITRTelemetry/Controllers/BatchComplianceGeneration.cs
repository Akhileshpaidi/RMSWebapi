using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using DomainModel;
using MySQLProvider;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.Tasks;
using MySqlConnector;
using ITR_TelementaryAPI.Models;
using System.Data;
using Microsoft.CodeAnalysis;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Ubiety.Dns.Core;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.ComponentModel;
using DocumentFormat.OpenXml.Drawing;
using static Peg.Base.PegBaseParser;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using iText.StyledXmlParser.Jsoup.Select;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class BatchComplianceGeneration : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BatchComplianceGeneration(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("api/BatchComplianceGeneration/CreateBatchCompanyCompliance")]
        [HttpGet]
        public IEnumerable<object> ProcessComplianceData()
        {
            var currentDate = DateTime.Now; // No parentheses needed

            var filteredData = mySqlDBContext.CurrentBatchComplianceModels
                .AsEnumerable()
                .Where(t => t.status == "Active" &&
                            DateTime.TryParse(t.startDate, out DateTime startDate) &&
                            startDate < currentDate) // Only past compliance
                .Select(t => new
                {
                    t.id,
                    t.locationDepartmentMappingId,
                    t.createCompanyComplianceId,
                    t.batchId,
                    t.endDate,
                    t.startDate,
                    t.taskId,
                    t.Apply_Scheduler_On,
                })
                .ToList();





            foreach (var data in filteredData)
            {
                var response = ProcessComplianceSchedulerData(data.createCompanyComplianceId, data.id, data.locationDepartmentMappingId, data.batchId, Convert.ToDateTime(data.startDate), Convert.ToDateTime(data.endDate), data.taskId, data.Apply_Scheduler_On);

                if (response is OkObjectResult okResponse)
                {
                    var result = okResponse.Value?.ToString();

                    if (result == "InActive")
                    {
                        var recordToUpdate = mySqlDBContext.CurrentBatchComplianceModels
                            .FirstOrDefault(t => t.id == data.id && t.createCompanyComplianceId == data.createCompanyComplianceId);

                        if (recordToUpdate != null)
                        {
                            recordToUpdate.status = result;

                            mySqlDBContext.SaveChanges();
                        }
                    }

                    else if (DateTime.TryParse(result, out var dueDate))
                    {
                        var recordToUpdate = mySqlDBContext.CurrentBatchComplianceModels
                            .FirstOrDefault(t => t.id == data.id && t.createCompanyComplianceId == data.createCompanyComplianceId);

                        if (recordToUpdate != null)
                        {
                            DateTime currentStartDate = DateTime.Parse(recordToUpdate.startDate);

                            recordToUpdate.startDate = new DateTime(dueDate.Year, currentStartDate.Month, currentStartDate.Day).ToString("yyyy-MM-dd");
                            recordToUpdate.batchId += 1;
                            mySqlDBContext.SaveChanges();
                        }
                    }

                    else
                    {
                        Console.WriteLine("No due date available.");
                    }
                }
                else
                {
                    Console.WriteLine("Unexpected response type.");
                }
            }
              return new List<object>
            {
                "Data processed"
            };
        }
        public IActionResult ProcessComplianceSchedulerData(int createCompanyComplianceId, int id, string locationDepartmentMappingId, int batchId, DateTime complianceStartDate, DateTime complianceEndDate, string taskId, string Apply_Scheduler_On)
        {
            var schedulerData = GetComplianceSchedulerData(createCompanyComplianceId, batchId);


            List<List<BatchCompliance>> allBatchCompliances = new List<List<BatchCompliance>>();

            List<BatchCompliance> allBatchComplianceForSaving = new List<BatchCompliance>();
            for (int i = 1; i <= schedulerData.Count; i++)
            {
                var record = schedulerData[i - 1];

                string currentCompliancePeriod = record.CurrentCompliancePeriod.ToString(); 
                string nextCompliancePeriod = record.NextCompliancePeriod.ToString();  
                int currentYearPeriodicityFactor = Convert.ToInt32(record.CurrentPeriodicityFactor);
                int dueDayPeriodicityFactor = Convert.ToInt32(record.DayMonthPeriodicityFactor);
                int nextYearPeriodicityFactor = Convert.ToInt32(record.NextPeriodicityFactor);
                int checkExtendStatus = Convert.ToInt32(record.ExtendStatus);
                DateTime schedulerStartDate = complianceStartDate;
                DateTime schedulerEndDate = complianceEndDate;

                DateTime? EffectiveFromDate = record.EffectiveFromDate;
                DateTime? EffectiveToDate = record.EffectiveToDate;

                int dueDay = Convert.ToInt32(record.DueDateDay);
                int month = Convert.ToInt32(record.DueDateMonth);
                int ExtendedDueDateDay = Convert.ToInt32(record.ExtendedDueDateDay);
                int ExtendedDueDateMonth = Convert.ToInt32(record.ExtendedDueDateMonth);

                string frequency = record.Frequency.ToString();

                var result = GenerateComplianceSchedule(currentCompliancePeriod, dueDay, month, ExtendedDueDateDay, ExtendedDueDateMonth, currentYearPeriodicityFactor, dueDayPeriodicityFactor, nextYearPeriodicityFactor, schedulerStartDate, schedulerEndDate, batchId, taskId, allBatchComplianceForSaving, frequency, EffectiveFromDate,EffectiveToDate, Apply_Scheduler_On, checkExtendStatus);

                if (result is List<BatchCompliance> batchComplianceList)
                {
                    foreach (var batchCompliance in batchComplianceList)
                    {
                        batchCompliance.create_company_compliance_id = createCompanyComplianceId;
                        batchCompliance.location_department_mapping_id = locationDepartmentMappingId;
                        batchCompliance.generate_current_batch_compliance_id = id;
                        batchCompliance.status = "Active";
                    }

                    allBatchComplianceForSaving.AddRange(batchComplianceList);
                    allBatchCompliances.Add(batchComplianceList);
                }
                else if (result is string && result.ToString() == "InActive")
                {
                    SaveComplianceData(allBatchComplianceForSaving);
                    return Ok("InActive");

                }

            }

            SaveComplianceData(allBatchComplianceForSaving);
            var lastDueDate = allBatchComplianceForSaving.OrderByDescending(x => x.compliance_due_date_created).FirstOrDefault()?.compliance_due_date_created;

            if (lastDueDate.HasValue)
            {
                return Ok(lastDueDate.Value.ToString("yyyy-MM-dd"));
            }
            else
            {
                return Ok("0");
            }
        }


        public object GenerateComplianceSchedule(string currentCompliancePeriod, int dueDay, int dueMonth, int ExtendedDueDateDay, int ExtendedDueDateMonth, int currentYearPeriodicityFactor, int dueDayPeriodicityFactor, int nextYearPeriodicityFactor, DateTime schedulerStartDate, DateTime schedulerEndDate, int batchId, string taskId, List<BatchCompliance> allBatchComplianceForSaving, string frequency, DateTime? EffectiveFromDate, DateTime? EffectiveToDate, string Apply_Scheduler_On, int checkExtendStatus)
        {
            DateTime complianceDueDateCreated = default(DateTime); 
            DateTime? ExtendedComplianceDueDateCreated = null;
            string compliancePeriod = "0";

            if (batchId == 1)
            {
                var firstInsertedCompliance = allBatchComplianceForSaving
                .OrderBy(b => b.compliance_id)
                .FirstOrDefault();

                            var lastInsertedCompliance = allBatchComplianceForSaving
                .OrderByDescending(b => b.compliance_id) // Assuming Id is an auto-increment field
                .FirstOrDefault();


                if (frequency == "Yearly")
                {
                    if(checkExtendStatus == 1)
                    {
                        ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                    }
                    else
                    {
                        complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        ExtendedComplianceDueDateCreated = null;

                    }
                    compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, complianceDueDateCreated, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                }
                // yearly completed


              else  if (frequency == "Half Year")
                {
                    if(firstInsertedCompliance != null)
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, lastInsertedCompliance.compliance_due_date_created, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                    }
                    else
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, complianceDueDateCreated, currentYearPeriodicityFactor, nextYearPeriodicityFactor);

                    }
                }
                // half yearly completed

               else if (frequency == "Quarterly")
                {
                    if (firstInsertedCompliance != null)
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, lastInsertedCompliance.compliance_due_date_created, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                    }
                    else
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, complianceDueDateCreated, currentYearPeriodicityFactor, nextYearPeriodicityFactor);

                    }
                }
                // Quarterly Completed

              else  if (frequency == "Monthly")
                {
                    if (firstInsertedCompliance != null)
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, lastInsertedCompliance.compliance_due_date_created, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                    }
                    else
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, complianceDueDateCreated, currentYearPeriodicityFactor, nextYearPeriodicityFactor);

                    }
                }
                // completed monthly
            }
            else
            {
                var firstInsertedCompliance = allBatchComplianceForSaving
                    .OrderBy(b => b.compliance_id)
                    .FirstOrDefault();

                        var lastInsertedCompliance = allBatchComplianceForSaving
                    .OrderByDescending(b => b.compliance_id) // Assuming Id is an auto-increment field
                    .FirstOrDefault();

                if (frequency == "Yearly")
                {
                    if (checkExtendStatus == 1)
                    {
                        ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                    }
                    else
                    {
                        complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        ExtendedComplianceDueDateCreated = null;
                    }
                    compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, schedulerStartDate, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                }
                // yearly completed


               else if (frequency == "Half Year")
                {
                    if (firstInsertedCompliance != null)
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, lastInsertedCompliance.compliance_due_date_created, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                    }
                    else
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;
                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, schedulerStartDate, currentYearPeriodicityFactor, nextYearPeriodicityFactor);

                    }
                }
                // completed half yearly


               else if (frequency == "Quarterly")
                {
                        if (firstInsertedCompliance != null)
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                            ExtendedComplianceDueDateCreated = null;

                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, lastInsertedCompliance.compliance_due_date_created, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                        }
                        else
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);

                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;

                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, schedulerStartDate, currentYearPeriodicityFactor, nextYearPeriodicityFactor);

                        }
                }
                // Quarterly Completed

               else if (frequency == "Monthly")
                {
                    if (firstInsertedCompliance != null)
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, firstInsertedCompliance.compliance_due_date_created);
                            ExtendedComplianceDueDateCreated = null;

                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, lastInsertedCompliance.compliance_due_date_created, currentYearPeriodicityFactor, nextYearPeriodicityFactor);
                    }
                    else
                    {
                        if (checkExtendStatus == 1)
                        {
                            ExtendedComplianceDueDateCreated = GetDueDate(ExtendedDueDateDay, ExtendedDueDateMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                        }
                        else
                        {
                            complianceDueDateCreated = GetDueDate(dueDay, dueMonth, dueDayPeriodicityFactor, schedulerStartDate);
                            ExtendedComplianceDueDateCreated = null;

                        }
                        compliancePeriod = AdjustYearsBasedOnFormat(currentCompliancePeriod, schedulerStartDate, currentYearPeriodicityFactor, nextYearPeriodicityFactor);

                    }
                }
                // completed monthly
            }

            if (schedulerEndDate != DateTime.MinValue && complianceDueDateCreated >= schedulerEndDate)
            {
                return "InActive";
            }

            else
            {
                string generatesComplianceID = GenerateComplianceID(taskId, allBatchComplianceForSaving);
                // Check if ExtendedComplianceDueDateCreated falls within the range
                DateTime finalComplianceDueDate;
                if (Apply_Scheduler_On == "ExtendedDueDate" && checkExtendStatus == 1)
                {
                    finalComplianceDueDate = (ExtendedComplianceDueDateCreated.HasValue && ExtendedComplianceDueDateCreated >= EffectiveFromDate &&   ExtendedComplianceDueDateCreated <= EffectiveToDate)  ? ExtendedComplianceDueDateCreated.Value  : complianceDueDateCreated;
                }
                else
                {
                    finalComplianceDueDate = complianceDueDateCreated;
                }

                List<BatchCompliance> batchComplianceList = new List<BatchCompliance>
                    {
                        new BatchCompliance
                        {
                            compliance_due_date_created = finalComplianceDueDate,
                            compliance_period = compliancePeriod,
                            compliance_id = generatesComplianceID,
                            //extended_compliance_due_date_created = ExtendedComplianceDueDateCreated
                        }
                    };

                return batchComplianceList;
            }

        }
        private static string AdjustYearsBasedOnFormat(string currentCompliancePeriod, DateTime schedulerStartDate, int currentYearPeriodicityFactor, int nextYearPeriodicityFactor)
        {
            int startYear = schedulerStartDate.Year;

            int currentYear = startYear + GetIncrementFactor(currentYearPeriodicityFactor);
            int nextYear = startYear + GetIncrementFactor(nextYearPeriodicityFactor);

            bool hasEndFormat = System.Text.RegularExpressions.Regex.IsMatch(
                currentCompliancePeriod,
                @"[A-Za-z]{3}(YYYY|YY)");

            MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(
                currentCompliancePeriod,
                @"([A-Za-z]{3})(YYYY|YY)");

            if (hasEndFormat && matches.Count >= 2)
            {
                string firstMonth = matches[0].Groups[1].Value;
                string firstYearFormat = matches[0].Groups[2].Value;

                string lastMonth = matches[matches.Count - 1].Groups[1].Value;
                string lastYearFormat = matches[matches.Count - 1].Groups[2].Value;

                string formattedOutput =
                    $"{firstMonth}{(firstYearFormat == "YYYY" ? currentYear.ToString() : (currentYear % 100).ToString("D2"))}-" +
                    $"{lastMonth}{(lastYearFormat == "YYYY" ? nextYear.ToString() : (nextYear % 100).ToString("D2"))}";

                return formattedOutput;
            }

            return currentCompliancePeriod
                .Replace("YYYY", currentYear.ToString())
                .Replace("YY", (currentYear % 100).ToString("D2"));
        }

        private static string AdjustYearsBasedOnFormat1(string currentCompliancePeriod, DateTime schedulerStartDate, int currentYearPeriodicityFactor, int nextYearPeriodicityFactor)
        {
            // Extract years in both full and short formats
            int startYear = schedulerStartDate.Year;
            string fullYear = startYear.ToString(); // Example: 2025
            string shortYear = (startYear % 100).ToString("D2"); // Example: 25

            // Detect if the format contains end placeholders (any month with YYYY or YY)
            bool hasEndFormat = System.Text.RegularExpressions.Regex.IsMatch(
                currentCompliancePeriod,
                @"[A-Za-z]{3}(YYYY|YY)");

            // Calculate the current year
            int currentYear = startYear + GetIncrementFactor(currentYearPeriodicityFactor);

            // Replace placeholders for the current year
            string formattedOutput = currentCompliancePeriod
                .Replace("YYYY", currentYear.ToString())
                .Replace("YY", (currentYear % 100).ToString("D2"));

            // If there's an end format, calculate the next year and replace placeholders dynamically
            if (hasEndFormat)
            {
                int nextYear = startYear + GetIncrementFactor(nextYearPeriodicityFactor);

                // Dynamically replace any end format with the next year
                formattedOutput = System.Text.RegularExpressions.Regex.Replace(
                    formattedOutput,
                    @"([A-Za-z]{3})(YYYY|YY)",
                    match =>
                    {
                        string month = match.Groups[1].Value; // Capture the month (e.g., "Mar")
                        string yearFormat = match.Groups[2].Value; // Capture the year format (e.g., "YYYY" or "YY")
                        return yearFormat == "YYYY"
                            ? $"{month}{nextYear}"
                            : $"{month}{(nextYear % 100):D2}";
                    });
            }

            return formattedOutput;
        }
        private static DateTime GetDueDate(int dueDay, int dueMonth, int periodicityFactor, DateTime schedulerStartDate)
        {
            int startYear = schedulerStartDate.Year;
            if (dueDay < 1 || dueDay > 31)
            {
                throw new ArgumentException("Invalid day. Day must be between 1 and 31.");
            }
            if (dueMonth < 1 || dueMonth > 12)
            {
                throw new ArgumentException("Invalid month. Month must be between 1 and 12.");
            }

            int yearAdjustment = GetIncrementFactor(periodicityFactor);

            int targetYear = startYear + yearAdjustment;

            DateTime dueDate;
            try
            {
                dueDate = new DateTime(targetYear, dueMonth, dueDay);
            }
            catch
            {
                int lastDayOfMonth = DateTime.DaysInMonth(targetYear, dueMonth);
                dueDate = new DateTime(targetYear, dueMonth, lastDayOfMonth);
            }

            return dueDate;
        }


        private static int GetIncrementFactor(int periodicityFactor)
        {
            return periodicityFactor switch
            {
                0 => 0,  // None
                1 => 0,  // Current Year
                2 => -1, // Preceding Year by 1
                3 => -2, // Preceding Year by 2
                4 => 1,  // Following Year
                5 => 1,  // Incremental by 1
                6 => 2,  // Incremental by 2
                7 => 3,  // Incremental by 3
                8 => 4,  // Incremental by 4
                9 => 5,  // Incremental by 5
                _ => throw new ArgumentException("Invalid Periodicity Factor"),
            };
        }

        public bool SaveComplianceData(List<BatchCompliance> currentBatchComplianceList)
        {
            try
            {
                if (currentBatchComplianceList != null && currentBatchComplianceList.Any())
                {
                    foreach (var currentCompliance in currentBatchComplianceList)
                    {

                        var batchCompliance = new BatchCompliance
                        {
                            compliance_id = currentCompliance.compliance_id,
                            compliance_due_date_created = currentCompliance.compliance_due_date_created,
                            compliance_period = currentCompliance.compliance_period,
                            create_company_compliance_id = currentCompliance.create_company_compliance_id,
                            location_department_mapping_id = currentCompliance.location_department_mapping_id,
                            generate_current_batch_compliance_id = currentCompliance.generate_current_batch_compliance_id,
                            status = "Active"
                    };

                        mySqlDBContext.BatchComplianceModels.Add(batchCompliance);
                    }

                    mySqlDBContext.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string GenerateComplianceID(string taskId, List<BatchCompliance> batchComplianceList)
        {
            string generatedBaseId = taskId;

            var existingIds = mySqlDBContext.BatchComplianceModels
                .Where(bcg => bcg.compliance_id.StartsWith(generatedBaseId))
                .Select(bcg => bcg.compliance_id)
                .ToList();

            var newBatchIds = batchComplianceList
                .Where(bc => bc.compliance_id.StartsWith(generatedBaseId))
                .Select(bc => bc.compliance_id)
                .ToList();

            var allIds = existingIds.Concat(newBatchIds).ToList();

            string newComplianceTaskId;

            if (allIds.Any())
            {
                int maxSuffix = allIds
                    .Select(id =>
                    {
                        string suffixPart = id.Substring(id.LastIndexOf('.') + 1);
                        return int.TryParse(suffixPart, out int parsedSuffix) ? parsedSuffix : 0;
                    })
                    .Max();

                newComplianceTaskId = $"{generatedBaseId}.{(maxSuffix + 1):D4}";
            }
            else
            {
                newComplianceTaskId = $"{generatedBaseId}.0001";
            }

            return newComplianceTaskId;
        }

        private string GenerateComplianceTaskID(int userId, int complianceId, string frequencyPrefix, List<BatchCompliance> batchComplianceList)
        {
            string generatedTaskId = $"{frequencyPrefix}-{userId}-{complianceId}-";

            var existingIds = mySqlDBContext.BatchComplianceModels
                .Where(bcg => bcg.task_id.StartsWith(generatedTaskId))
                .Select(bcg => bcg.task_id)
                .ToList();

            var newBatchIds = batchComplianceList
                .Where(bc => bc.task_id.StartsWith(generatedTaskId))
                .Select(bc => bc.task_id)
                .ToList();

            var allIds = existingIds.Concat(newBatchIds).ToList();

            string newComplianceId;

            if (allIds.Any())
            {
                int maxSuffix = allIds
                    .Select(id =>
                    {
                        string suffixPart = id.Substring(id.LastIndexOf('-') + 1);
                        return int.TryParse(suffixPart, out int parsedSuffix) ? parsedSuffix : 0;
                    })
                    .Max();

                newComplianceId = $"{generatedTaskId}{(maxSuffix + 1):D3}";
            }
            else
            {
                newComplianceId = $"{generatedTaskId}001";
            }

            return newComplianceId;
        }





        public List<dynamic> GetComplianceSchedulerData(int createCompanyComplianceId, int batchid)
        {
            string connectionString = Configuration.GetConnectionString("myDb1");

            string query = @"
  WITH OrderedRecords AS (
    SELECT
        scheduler.company_compliance_sheduler_id,
        scheduler.create_company_compliance_id,
        scheduler.currentcompliancePeriod,
        scheduler.nextCompliancePeriod,
        scheduler.dueDateDay,
        scheduler.dueDateMonth,
        scheduler.extendedDueDateDay,
        scheduler.extendedDueDateMonth,
        scheduler.extendstatus,
        scheduler.day_month_periodicity_factor,
        ccc.effective_from_date,
        ccc.effective_to_date,
        ccc.frequency,            
        
        -- Assign row numbers to each record to identify first, middle, and subsequent records
        ROW_NUMBER() OVER (ORDER BY scheduler.company_compliance_sheduler_id) AS RowNum,

        -- Conditionally fetch periodicity factors based on batch and row number
        CASE
            WHEN @Batch = 1 THEN scheduler.current_periodicity_factor
            ELSE CASE 
                WHEN ROW_NUMBER() OVER (ORDER BY scheduler.company_compliance_sheduler_id) = 1 THEN ccc.loop_current_periodicity_factor
                ELSE scheduler.current_periodicity_factor
            END
        END AS current_periodicity_factor,

        CASE
            WHEN @Batch = 1 THEN scheduler.next_periodicity_factor
            ELSE CASE 
                WHEN ROW_NUMBER() OVER (ORDER BY scheduler.company_compliance_sheduler_id) = 1 THEN ccc.loop_next_periodicity_factor
                ELSE scheduler.next_periodicity_factor
            END
        END AS next_periodicity_factor
    FROM
        company_compliance_scheduler_master scheduler 
    LEFT JOIN
        create_company_compliance ccc
        ON scheduler.create_company_compliance_id = ccc.create_company_compliance_id
    WHERE
        scheduler.create_company_compliance_id = @CreateCompanyComplianceId
)
SELECT *
FROM OrderedRecords
ORDER BY RowNum";


            //     --CASE
            //       --   WHEN @Batch = 1 THEN scheduler.day_month_periodicity_factor
            //        --  ELSE CASE
            //     --WHEN ROW_NUMBER() OVER(ORDER BY scheduler.company_compliance_sheduler_id) = 1 THEN ccc.loop_day_month_periodicity_factor
            //       --  ELSE scheduler.day_month_periodicity_factor
            //    -- END
            //-- END AS day_month_periodicity_factor

            List<dynamic> result = new List<dynamic>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CreateCompanyComplianceId", createCompanyComplianceId);
                        command.Parameters.AddWithValue("@Batch", batchid);

                        using (var reader = command.ExecuteReader())
                        {
                            bool isFirstRecord = true;

                            while (reader.Read())
                            {
                                var record = new
                                {
                                    CompanyComplianceSchedulerId = reader["company_compliance_sheduler_id"] != DBNull.Value ? reader["company_compliance_sheduler_id"] : null,
                                    CreateCompanyComplianceId = reader["create_company_compliance_id"] != DBNull.Value ? reader["create_company_compliance_id"] : null,
                                    CurrentCompliancePeriod = reader["currentcompliancePeriod"] != DBNull.Value ? reader["currentcompliancePeriod"] : null,
                                    NextCompliancePeriod = reader["nextCompliancePeriod"] != DBNull.Value ? reader["nextCompliancePeriod"] : null,
                                    DueDateDay = reader["dueDateDay"] != DBNull.Value ? reader["dueDateDay"] : null,
                                    DueDateMonth = reader["dueDateMonth"] != DBNull.Value ? reader["dueDateMonth"] : null,
                                    ExtendedDueDateDay = reader["extendedDueDateDay"] != DBNull.Value ? reader["extendedDueDateDay"] : null,
                                    ExtendedDueDateMonth = reader["extendedDueDateMonth"] != DBNull.Value ? reader["extendedDueDateMonth"] : null,
                                    EffectiveFromDate = reader["effective_from_date"] != DBNull.Value ? reader["effective_from_date"] : null,
                                    EffectiveToDate = reader["effective_to_date"] != DBNull.Value ? reader["effective_to_date"] : null,
                                    ExtendStatus = reader["extendstatus"] != DBNull.Value ? reader["extendstatus"] : null,
                                    Frequency = reader["frequency"] != DBNull.Value ? reader["frequency"] : null,
                                    CurrentPeriodicityFactor = reader["current_periodicity_factor"] != DBNull.Value ? Convert.ToDecimal(reader["current_periodicity_factor"]) : (decimal?)null,
                                    NextPeriodicityFactor = reader["next_periodicity_factor"] != DBNull.Value ? Convert.ToDecimal(reader["next_periodicity_factor"]) : (decimal?)null,
                                    DayMonthPeriodicityFactor = reader["day_month_periodicity_factor"] != DBNull.Value ? Convert.ToDecimal(reader["day_month_periodicity_factor"]) : (decimal?)null
                                };

                                result.Add(record);

                                if (isFirstRecord)
                                {
                                    isFirstRecord = false;
                                }
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return result;
        }


        public enum ComplianceFrequency
        {
            Monthly,
            Quarterly,
            HalfYearly,
            Yearly
        }

    }



}
