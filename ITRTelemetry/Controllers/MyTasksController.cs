using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Permissions;
using Microsoft.Extensions.Configuration;
using System.Configuration;



namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class MyTasksController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        private IEnumerable<object> doc_User_Access_mapping_id;
        public IConfiguration Configuration { get; }

        public MyTasksController(MySqlDBContext mySqlDBContext , IConfiguration configuration)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }
     
        [Route("api/MyTasks/GetMyTasksDetails")]
        [HttpGet]
        public IEnumerable<object> GetMyTasksDetailDashb()
        {

            var excludedDocumentIds = mySqlDBContext.AddDocumentModels
       .GroupBy(ad => ad.Document_Id)
       .Where(group => group.Count() > 1)
       .Select(group => group.Key)
       .ToList();

            var currentDate = DateTime.Now;
            string validationStatus;
            var details = (from documentmaster in mySqlDBContext.AddDocumentModels
                           join doctypemaster in mySqlDBContext.DocTypeMasterModels on documentmaster.DocTypeID equals doctypemaster.docTypeID
                           join doccategorymaster in mySqlDBContext.DocCategoryMasterModels on documentmaster.Doc_CategoryID equals doccategorymaster.Doc_CategoryID
                           join docsubcategorymaster in mySqlDBContext.DocSubCategoryModels on documentmaster.Doc_SubCategoryID equals docsubcategorymaster.Doc_SubCategoryID

                           join authoritypemaster in mySqlDBContext.AuthorityTypeMasters on documentmaster.AuthorityTypeID equals authoritypemaster.AuthorityTypeID

                           join authoritynamemaster in mySqlDBContext.AuthorityNameModels on documentmaster.AuthoritynameID equals authoritynamemaster.AuthoritynameID

            // join entitymaster in mySqlDBContext.UnitMasterModels on documentmaster.Entity_Master_id equals entitymaster.Entity_Master_id

            //  join unitmaster in mySqlDBContext.UnitLocationMasterModels on documentmaster.Unit_location_Master_id equals unitmaster.Unit_location_Master_id

                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on documentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where documentmaster.status_permission == null &&
                                 documentmaster.addDoc_Status == "Active" &&
                                 documentmaster.Draft_Status == "Completed" &&
                                 !excludedDocumentIds.Contains(documentmaster.Document_Id)
                           select new
                           {
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                               doctypemaster.docTypeName,
                               doccategorymaster.Doc_CategoryName,
                               docsubcategorymaster.Doc_SubCategoryName,
                               authoritynamemaster.AuthorityName,
                               authoritypemaster.AuthorityTypeName,
                               naturemaster.NatureOf_Doc_Name,
                               documentmaster.Doc_Confidentiality,
                               
                               // EntityName = entitymaster.Entity_Master_Name, // Replace with the actual property name
                               // UnitLocationName = unitmaster.Unit_location_Master_name, // Replace with the actual property name
                               ReviewStartDate = documentmaster.review_start_Date, // Include the review start date in the result

                           })
                           .Distinct()
                           .ToList()
            .Select(detail => new
            {
                detail.Title_Doc,
                detail.AddDoc_id,
                detail.Document_Id,
                detail.docTypeName,
                detail.Doc_CategoryName,
                detail.Doc_SubCategoryName,
                detail.AuthorityName,
                detail.AuthorityTypeName,
                detail.NatureOf_Doc_Name,
                detail.Doc_Confidentiality,
                //  detail.EntityName,
                // detail.UnitLocationName,
                detail.ReviewStartDate,
                Validation = CalculateValidationStatus(detail.ReviewStartDate, currentDate),
                NoofDays = CalculateNoofDays(detail.ReviewStartDate, currentDate)
            })
               .ToList();



            //Function to calculate validation status
            string CalculateValidationStatus(DateTime? reviewStartDate, DateTime currentDate)
            {
                if (reviewStartDate.HasValue)
                {
                    TimeSpan span = reviewStartDate.Value - currentDate;
                    int NoofDays = span.Days;

                    if (NoofDays >= 0 && NoofDays <= 30)
                    {
                        return "Take Immediate Action";
                    }
                    else if (NoofDays < 0)
                    {
                        return "Expired";
                    }
                    else if (NoofDays >= 31 && NoofDays <= 60)
                    {
                        return "Expiring Soon";
                    }
                    else if (NoofDays >= 60)
                    {
                        return "Not Due";
                    }
                }

                return "No Date"; // Handle other cases as needed
            }
            int CalculateNoofDays(DateTime? reviewStartDate, DateTime currentDate)
            {
                if (reviewStartDate.HasValue)
                {
                    TimeSpan span = reviewStartDate.Value - currentDate;
                    return span.Days;
                }

                return 0; // Handle other cases as needed
            }

            return details;


        }



        [Route("api/MyTasks/GetPendingCount")]
        [HttpGet]
        public int  GetPendingCount()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select count(*) as PendingCount  from risk.add_doc where addDoc_Status='Active' and Draft_Status='pending';", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pendingcount=0;
            if (dt.Rows.Count > 0)
            {
                pendingcount = Convert.ToInt32(dt.Rows[0]["PendingCount"].ToString());
              
                
            }
            return pendingcount;
        }

        //[Route("api/MyTasks/GetDocumentCount")]
        //[HttpGet]
        //public int GetDocumentCount()
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb2"]);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(@"select count(*) as DocumentCount from risk.add_doc where addDoc_Status='Active';", con);

        //    cmd.CommandType = CommandType.Text;

        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
        //    con.Close();

        //    var DocumentCount = 0;
        //    if (dt.Rows.Count > 0)
        //    {
        //        DocumentCount = Convert.ToInt32(dt.Rows[0]["DocumentCount"].ToString());


        //    }
        //    return DocumentCount;
        //}


        [Route("api/MyTasks/GetDocumentCount")]
        [HttpGet]
        public int GetDocumentCount()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            // Calculate the date 30 days ago from the current date
            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

            // Use parameterized query to avoid SQL injection
            MySqlCommand cmd = new MySqlCommand(
                @"SELECT COUNT(*) AS DocumentCount 
          FROM risk.add_doc 
          WHERE addDoc_Status='Active' 
          AND addDoc_createdDate >= @StartDate;", con);

            cmd.Parameters.AddWithValue("@StartDate", thirtyDaysAgo);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var DocumentCount = 0;
            if (dt.Rows.Count > 0)
            {
                DocumentCount = Convert.ToInt32(dt.Rows[0]["DocumentCount"].ToString());
            }

            return DocumentCount;
        }


        //    [Route("api/MyTasks/GetMyTasksDetailssss")]
        //    [HttpGet]
        //    public object GetMyTasksDetailDash()
        //    {
        //        var currentDate = DateTime.Now;

        //        var details = (from documentmaster in mySqlDBContext.AddDocumentModels
        //                           // ... (your existing query)
        //                       select new
        //                       {
        //                           documentmaster.Title_Doc,
        //                           documentmaster.AddDoc_id,
        //                           documentmaster.Document_Id,
        //                           // ... other properties
        //                           ReviewStartDate = documentmaster.review_start_Date,
        //                       })
        //                       .Distinct()
        //                       .ToList();

        //        var validationCounts = new Dictionary<string, int>
        //{
        //    {"Take Immediate Action", 0},
        //    {"Expired", 0},
        //    {"Expiring Soon", 0},
        //    {"Not Due", 0}
        //};

        //        foreach (var detail in details)
        //        {
        //            var validationStatus = CalculateValidationStatus(detail.ReviewStartDate, currentDate);

        //            // Increment the count for the validation status
        //            validationCounts[validationStatus]++;

        //            // Additional processing for each detail, if needed
        //        }

        //        // Include validation counts in the result
        //        var result = new
        //        {
        //            Details = details,
        //            ValidationCounts = validationCounts
        //        };

        //        return result;
        //    }

        //    string CalculateValidationStatus(DateTime? reviewStartDate, DateTime currentDate)
        //    {
        //        if (reviewStartDate.HasValue)
        //        {
        //            TimeSpan span = reviewStartDate.Value - currentDate;
        //            int noOfDays = span.Days;

        //            if (noOfDays >= 0 && noOfDays <= 30)
        //            {
        //                return "Take Immediate Action";
        //            }
        //            else if (noOfDays < 0)
        //            {
        //                return "Expired";
        //            }
        //            else if (noOfDays >= 31 && noOfDays <= 60)
        //            {
        //                return "Expiring Soon";
        //            }
        //            else if (noOfDays >= 60)
        //            {
        //                return "Not Due";
        //            }
        //        }

        //        return "No Date";
        //    }

        [Route("api/MyTasks/GetimmediateActionCount")]
        [HttpGet]
        public object GetMyTasksDetailDashbbbb()
        {
            var currentDate = DateTime.Now;

            var details = (from documentmaster in mySqlDBContext.AddDocumentModels
                               // ... (your existing query)
                           select new
                           {
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                              ReviewStartDate = documentmaster.review_start_Date,
                           })
                           .Distinct()
                           .ToList();

            int immediateActionCount = 0;
            //int expiredCount = 0;
            //int expiringSoonCount = 0;
            //int notDueCount = 0;

            foreach (var detail in details)
            {
                var validationStatus = CalculateValidationStatus(detail.ReviewStartDate, currentDate);

                // Increment the count for the validation status
                switch (validationStatus)
                {
                    case "Take Immediate Action":
                        immediateActionCount++;
                        break;
                    //case "Expired":
                    //    expiredCount++;
                    //    break;
                    //case "Expiring Soon":
                    //    expiringSoonCount++;
                    //    break;
                    //case "Not Due":
                    //    notDueCount++;
                    //    break;
                }

                // Additional processing for each detail, if needed
            }

            // Include validation counts in the result
            var result = new
            {
                Details = details,
                ImmediateActionCount = immediateActionCount
                //ExpiredCount = expiredCount,
                //ExpiringSoonCount = expiringSoonCount,
                //NotDueCount = notDueCount
            };

            return result;
        }

        string CalculateValidationStatus(DateTime? reviewStartDate, DateTime currentDate)
        {
            if (reviewStartDate.HasValue)
            {
                TimeSpan span = reviewStartDate.Value - currentDate;
                int noOfDays = span.Days;

                if (noOfDays >= 0 && noOfDays <= 30)
                {
                    return "Take Immediate Action";
                }
                else if (noOfDays < 0)
                {
                    return "Expired";
                }
                else if (noOfDays >= 31 && noOfDays <= 60)
                {
                    return "Expiring Soon";
                }
                else if (noOfDays >= 60)
                {
                    return "Not Due";
                }
            }

            return "No Date";
        }

        [Route("api/MyTasks/GetexpiredCount")]
        [HttpGet]
        public object GetMyTasksDetailExp()
        {
            var currentDate = DateTime.Now;

            var details = (from documentmaster in mySqlDBContext.AddDocumentModels
                          select new
                           {
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                               ReviewStartDate = documentmaster.review_start_Date,
                           })
                           .Distinct()
                           .ToList();
             int expiredCount = 0;
          foreach (var detail in details)
            {
                var validationStatus = CalculateValidationStatus1(detail.ReviewStartDate, currentDate);

                // Increment the count for the validation status
                switch (validationStatus)
                {     
                    case "Expired":
                            expiredCount++;
                            break;
                }
             }

            var result = new
            {
                Details = details,
                 ExpiredCount = expiredCount,
             };

            return result;
        }

        string CalculateValidationStatus1(DateTime? reviewStartDate, DateTime currentDate)
        {
            if (reviewStartDate.HasValue)
            {
                TimeSpan span = reviewStartDate.Value - currentDate;
                int noOfDays = span.Days;

                if (noOfDays >= 0 && noOfDays <= 30)
                {
                    return "Take Immediate Action";
                }
                else if (noOfDays < 0)
                {
                    return "Expired";
                }
                else if (noOfDays >= 31 && noOfDays <= 60)
                {
                    return "Expiring Soon";
                }
                else if (noOfDays >= 60)
                {
                    return "Not Due";
                }
            }

            return "No Date";
        }

        [Route("api/MyTasks/GetExpiringCount")]
        [HttpGet]
        public object GetMyTasksDetailExpiring()
        {
            var currentDate = DateTime.Now;

            var details = (from documentmaster in mySqlDBContext.AddDocumentModels
                           select new
                           {
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                               ReviewStartDate = documentmaster.review_start_Date,
                           })
                           .Distinct()
                           .ToList();
            int expiringSoonCount = 0;
            foreach (var detail in details)
            {
                var validationStatus = CalculateValidationStatus2(detail.ReviewStartDate, currentDate);

               switch (validationStatus)
                {
                    case "Expiring":
                        expiringSoonCount++;
                        break;
                }
            }

            var result = new
            {
                Details = details,
                expiringSoonCount = expiringSoonCount,
            };

            return result;
        }

        string CalculateValidationStatus2(DateTime? reviewStartDate, DateTime currentDate)
        {
            if (reviewStartDate.HasValue)
            {
                TimeSpan span = reviewStartDate.Value - currentDate;
                int noOfDays = span.Days;

                if (noOfDays >= 0 && noOfDays <= 30)
                {
                    return "Take Immediate Action";
                }
                else if (noOfDays < 0)
                {
                    return "Expired";
                }
                else if (noOfDays >= 31 && noOfDays <= 60)
                {
                    return "Expiring Soon";
                }
                else if (noOfDays >= 60)
                {
                    return "Not Due";
                }
            }

            return "No Date";
        }

        [Route("api/MyTasks/GetNotDueCount")]
        [HttpGet]
        public object GetMyTasksDetailNot()
        {
            var currentDate = DateTime.Now;

            var details = (from documentmaster in mySqlDBContext.AddDocumentModels
                           select new
                           {
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                               ReviewStartDate = documentmaster.review_start_Date,
                           })
                           .Distinct()
                           .ToList();
            int NotDueCount = 0;
            foreach (var detail in details)
            {
                var validationStatus = CalculateValidationStatus3(detail.ReviewStartDate, currentDate);

                switch (validationStatus)
                {
                    case "Not Due":
                        NotDueCount++;
                        break;
                }
            }

            var result = new
            {
                Details = details,
                NotDueCount = NotDueCount,
            };

            return result;
        }

        string CalculateValidationStatus3(DateTime? reviewStartDate, DateTime currentDate)
        {
            if (reviewStartDate.HasValue)
            {
                TimeSpan span = reviewStartDate.Value - currentDate;
                int noOfDays = span.Days;

                if (noOfDays >= 0 && noOfDays <= 30)
                {
                    return "Take Immediate Action";
                }
                else if (noOfDays < 0)
                {
                    return "Expired";
                }
                else if (noOfDays >= 31 && noOfDays <= 60)
                {
                    return "Expiring Soon";
                }
                else if (noOfDays >= 60)
                {
                    return "Not Due";
                }
            }

            return "No Date";
        }
    }
}
