using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DomainModel;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using DomainModel;
using System;
using System.Collections.Generic;
using System.Data;

using System.Globalization;
using iText.StyledXmlParser.Jsoup.Parser;
using DocumentFormat.OpenXml.Spreadsheet;
using iText.StyledXmlParser.Jsoup.Select;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2013.Excel;


//using DocumentFormat.OpenXml.Drawing.Charts;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class GovControlReportsController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public GovControlReportsController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }


        //GCT REPORTS

        //Getting Published Document Data

        [Route("api/GovControlReportsController/GetPubDocList")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> GetPubDocListReactivate([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == DateModels.userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GetPubDocListModel>();



            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string basequery = @"
        SELECT 
            e.AuthoritynameID,
            tbl.firstname as publisher_name,
            e.AuthorityTypeID,
            e.NatureOf_Doc_id,
            e.DocTypeID,
            e.Doc_SubCategoryID,
            e.Doc_CategoryID,
            e.Title_Doc,
            e.Doc_Confidentiality,
            e.Eff_Date, 
            e.Sub_title_doc, 
             e.review_start_Date,
            e.Document_Id, 
            e.addDoc_createdDate, 
            e.VersionControlNo, 
            e.freq_period_type,
            e.Initial_creation_doc_date,
            e.Doc_internal_num,
            e.Doc_Inter_ver_num,
            e.Doc_Phy_Valut_Loc,
            e.Doc_process_Owner,
            e.Doc_Approver,
            e.Date_Doc_Revision,
            e.Date_Doc_Approver,
            e.freq_period,
            e.Keywords_tags,
            e.pub_doc,
            e.publisher_comments,
            e.indicative_reading_time,
            e.Review_Frequency_Status,
            e.Time_period,
            e.AddDoc_id,
            e.Obj_Doc,
            e.addDoc_Status,
e.supportFilesCount,e.ChangedOn,e.ChangedBy,
            t.DocTypeName,
            C.Doc_CategoryName,
            sc.Doc_SubCategoryName,
            a.AuthorityName,
            at.AuthorityTypeName,
            p.NatureOf_Doc_Name
        FROM
            risk.add_doc e
        LEFT OUTER JOIN
            risk.doctype_master t ON t.DocTypeID = e.DocTypeID
        LEFT OUTER JOIN
            risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
        LEFT OUTER JOIN
            risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
        LEFT OUTER JOIN
            risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
        LEFT OUTER JOIN
            risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
        LEFT OUTER JOIN
            risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
        LEFT OUTER JOIN
            risk.tbluser tbl ON tbl.USR_ID = e.USR_ID
        WHERE
             e.addDoc_Status = @addDoc_Status
            AND e.Draft_Status = @Draft_Status AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Publishing" => "DATE(e.addDoc_createdDate) <= @Today AND DATE(e.addDoc_createdDate) >= @MonthAgo",
                "Effective" => "DATE(e.Eff_Date) <= @Today AND DATE(e.Eff_Date) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = basequery + dateCondition;

            if (locations.Any())
            {
                var locationConditions = string.Join(",", locations.Select((loc, index) => $"@loc{index}"));
                finalQuery += " AND (" + string.Join(" OR ", locations.Select((loc, index) => $"FIND_IN_SET(@loc{index}, e.Unit_location_Master_id) > 0")) + ")";
            }

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@addDoc_Status", "Active");
                        cmd.Parameters.AddWithValue("@Draft_Status", "Completed");

                        if (locations.Any())
                        {
                            for (int i = 0; i < locations.Count; i++)
                            {
                                cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);
                            }
                        }

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            foreach (DataRow row in dt.Rows)
                            {
                                string originalPubDoc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty;
                                string resolvedPubDoc = "";

                                if (!string.IsNullOrWhiteSpace(originalPubDoc))
                                {
                                    var idList = originalPubDoc.Split(',')
                                                               .Select(id => id.Trim())
                                                               .Where(id => int.TryParse(id, out _))
                                                               .ToList();

                                    if (idList.Any())
                                    {
                                        var idParams = string.Join(",", idList.Select((id, index) => $"@id{index}"));
                                        var docIdQuery = $"SELECT AddDoc_id, Document_Id FROM risk.add_doc WHERE AddDoc_id IN ({idParams})";

                                        using (var cmd2 = new MySqlCommand(docIdQuery, con))
                                        {
                                            for (int i = 0; i < idList.Count; i++)
                                            {
                                                cmd2.Parameters.AddWithValue($"@id{i}", idList[i]);
                                            }

                                            using (var rdr = cmd2.ExecuteReader())
                                            {
                                                var resolved = new List<string>();
                                                while (rdr.Read())
                                                {
                                                    resolved.Add(rdr["Document_Id"]?.ToString());
                                                }
                                                resolvedPubDoc = string.Join(",", resolved);
                                            }
                                        }
                                    }
                                }


                                string reviewStartDateString = row["review_start_Date"] != DBNull.Value ? row["review_start_Date"].ToString() : string.Empty;
                                DateTime startDate;
                                int noOfDays = 0;
                                string validation = string.Empty;

                                if (!string.IsNullOrEmpty(reviewStartDateString))
                                {
                                    try
                                    {
                                        startDate = DateTime.ParseExact(reviewStartDateString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                                        string todaysDate = DateTime.Now.ToString("dd-MM-yyyy");
                                        DateTime endDate = DateTime.ParseExact(todaysDate, "dd-MM-yyyy", null);
                                        TimeSpan span = startDate - endDate;
                                        noOfDays = span.Days;
                                        validation = GetValidationMessage(noOfDays);
                                    }
                                    catch (FormatException)
                                    {
                                        // Handle parsing error if the date format is incorrect
                                        validation = "Invalid review start date format";
                                    }
                                }

                                pdata.Add(new GetPubDocListModel
                                {
                                    AddDoc_id = row["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(row["AddDoc_id"]) : 0,
                                    AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                    Publisher_name = row["publisher_name"] != DBNull.Value ? row["publisher_name"].ToString() : string.Empty,
                                    DocTypeID = row["DocTypeID"] != DBNull.Value ? Convert.ToInt32(row["DocTypeID"]) : 0,
                                    AuthoritynameID = row["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(row["AuthoritynameID"]) : 0,
                                    AuthorityTypeID = row["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(row["AuthorityTypeID"]) : 0,
                                    NatureOf_Doc_id = row["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(row["NatureOf_Doc_id"]) : 0,
                                    Doc_CategoryID = row["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_CategoryID"]) : 0,
                                    Doc_SubCategoryID = row["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_SubCategoryID"]) : 0,
                                    AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                    Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                    Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                    Obj_Doc = row["Obj_Doc"] != DBNull.Value ? row["Obj_Doc"].ToString() : string.Empty,
                                    addDoc_Status = row["addDoc_Status"] != DBNull.Value ? row["addDoc_Status"].ToString() : string.Empty,
                                    NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                    DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                    Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                    Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,
                                    Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                    Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                    addDoc_createdDate = row["addDoc_createdDate"] != DBNull.Value ? row["addDoc_createdDate"].ToString() : string.Empty,
                                    doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                    Initial_creation_doc_date = row["Initial_creation_doc_date"] != DBNull.Value ? row["Initial_creation_doc_date"].ToString() : string.Empty,
                                    Doc_internal_num = row["Doc_internal_num"] != DBNull.Value ? row["Doc_internal_num"].ToString() : string.Empty,
                                    Doc_Inter_ver_num = row["Doc_Inter_ver_num"] != DBNull.Value ? row["Doc_Inter_ver_num"].ToString() : string.Empty,
                                    Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"] != DBNull.Value ? row["Doc_Phy_Valut_Loc"].ToString() : string.Empty,
                                    Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                    Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,
                                    Date_Doc_Revision = row["Date_Doc_Revision"] != DBNull.Value ? row["Date_Doc_Revision"].ToString() : string.Empty,
                                    Review_Frequency_Status = row["Review_Frequency_Status"] != DBNull.Value ? Convert.ToInt32(row["Review_Frequency_Status"]) : 0,
                                    Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                    VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                    Date_Doc_Approver = row["Date_Doc_Approver"] != DBNull.Value ? row["Date_Doc_Approver"].ToString() : string.Empty,
                                    freq_period = (row["freq_period"] != DBNull.Value ? row["freq_period"].ToString() : string.Empty) + " " + (row["freq_period_type"] != DBNull.Value ? row["freq_period_type"].ToString() : string.Empty),
                                    Keywords_tags = row["Keywords_tags"] != DBNull.Value ? row["Keywords_tags"].ToString() : string.Empty,
                                    //  pub_doc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty,
                                    pub_doc = resolvedPubDoc,
                                    publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                    supportFilesCount = row["supportFilesCount"] != DBNull.Value ? Convert.ToInt32(row["supportFilesCount"]) : 0,
                                    indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                    NoofDays = noOfDays,
                                    validations = validation,
                                });
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }


        //Getting discarded Drafts

        [Route("api/GovControlReportsController/GetDraftDiscardedList")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> GetDraftDiscardedList([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GetPubDocListModel>();
            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == DateModels.userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string basequery = @"
        SELECT 
            e.AuthoritynameID,
            tbl.firstname as publisher_name,
            e.AuthorityTypeID,
            e.NatureOf_Doc_id,
            e.DocTypeID,
            e.Doc_SubCategoryID,
            e.Doc_CategoryID,
            e.Title_Doc,
            e.Doc_Confidentiality,
            e.Eff_Date, 
            e.Sub_title_doc, 
            e.Document_Id, 
            e.addDoc_createdDate, 
            e.VersionControlNo, 
            e.freq_period_type,
            e.Initial_creation_doc_date,
            e.Doc_internal_num,
            e.Doc_Inter_ver_num,
            e.Doc_Phy_Valut_Loc,
            e.Doc_process_Owner,
            e.Doc_Approver,
            e.Date_Doc_Revision,
            e.Date_Doc_Approver,
            e.freq_period,
            e.Keywords_tags,
            e.pub_doc,
            e.publisher_comments,
            e.indicative_reading_time,
            e.Review_Frequency_Status,
            e.Time_period,
            e.AddDoc_id,
            e.Obj_Doc,e.supportFilesCount,
            e.addDoc_Status,
            t.DocTypeName,
            C.Doc_CategoryName,
            sc.Doc_SubCategoryName,
            a.AuthorityName,
            at.AuthorityTypeName,
            p.NatureOf_Doc_Name,e.ChangedOn,e.ChangedBy,tblu.firstname as discardedby
        FROM
            risk.add_doc e
        LEFT OUTER JOIN
            risk.doctype_master t ON t.DocTypeID = e.DocTypeID
        LEFT OUTER JOIN
            risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
        LEFT OUTER JOIN
            risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
        LEFT OUTER JOIN
            risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
        LEFT OUTER JOIN
            risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
        LEFT OUTER JOIN
            risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
        LEFT OUTER JOIN
            risk.tbluser tbl ON tbl.USR_ID = e.USR_ID
 LEFT OUTER JOIN
            risk.tbluser tblu ON tblu.USR_ID = e.ChangedBy
        WHERE
            e.Draft_Status = @Draft_Status AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Discard" => "DATE(e.ChangedOn) <= @Today AND DATE(e.ChangedOn) >= @MonthAgo",

                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = basequery + dateCondition;
            if (locations.Any())
            {
                var locationConditions = string.Join(",", locations.Select((loc, index) => $"@loc{index}"));
                finalQuery += " AND (" + string.Join(" OR ", locations.Select((loc, index) => $"FIND_IN_SET(@loc{index}, e.Unit_location_Master_id) > 0")) + ")";
            }

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@Draft_Status", "Draft Discarded");

                        if (locations.Any())
                        {
                            for (int i = 0; i < locations.Count; i++)
                            {
                                cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);
                            }
                        }
                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GetPubDocListModel
                                    {
                                        AddDoc_id = row["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(row["AddDoc_id"]) : 0,
                                        AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                        Publisher_name = row["publisher_name"] != DBNull.Value ? row["publisher_name"].ToString() : string.Empty,
                                        DocTypeID = row["DocTypeID"] != DBNull.Value ? Convert.ToInt32(row["DocTypeID"]) : 0,
                                        AuthoritynameID = row["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(row["AuthoritynameID"]) : 0,
                                        AuthorityTypeID = row["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(row["AuthorityTypeID"]) : 0,
                                        NatureOf_Doc_id = row["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(row["NatureOf_Doc_id"]) : 0,
                                        Doc_CategoryID = row["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_CategoryID"]) : 0,
                                        Doc_SubCategoryID = row["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_SubCategoryID"]) : 0,
                                        AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                        Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                        Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                        Obj_Doc = row["Obj_Doc"] != DBNull.Value ? row["Obj_Doc"].ToString() : string.Empty,
                                        addDoc_Status = row["addDoc_Status"] != DBNull.Value ? row["addDoc_Status"].ToString() : string.Empty,
                                        NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,
                                        Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                        Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                        addDoc_createdDate = row["addDoc_createdDate"] != DBNull.Value ? row["addDoc_createdDate"].ToString() : string.Empty,
                                        doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                        lastEditedby = row["discardedby"] != DBNull.Value ? row["discardedby"].ToString() : string.Empty,
                                        Initial_creation_doc_date = row["Initial_creation_doc_date"] != DBNull.Value ? row["Initial_creation_doc_date"].ToString() : string.Empty,
                                        Doc_internal_num = row["Doc_internal_num"] != DBNull.Value ? row["Doc_internal_num"].ToString() : string.Empty,
                                        Doc_Inter_ver_num = row["Doc_Inter_ver_num"] != DBNull.Value ? row["Doc_Inter_ver_num"].ToString() : string.Empty,
                                        Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"] != DBNull.Value ? row["Doc_Phy_Valut_Loc"].ToString() : string.Empty,
                                        Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                        Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,
                                        Date_Doc_Revision = row["Date_Doc_Revision"] != DBNull.Value ? row["Date_Doc_Revision"].ToString() : string.Empty,
                                        Review_Frequency_Status = row["Review_Frequency_Status"] != DBNull.Value ? Convert.ToInt32(row["Review_Frequency_Status"]) : 0,
                                        Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                        VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                        Date_Doc_Approver = row["Date_Doc_Approver"] != DBNull.Value ? row["Date_Doc_Approver"].ToString() : string.Empty,
                                        freq_period = (row["freq_period"] != DBNull.Value ? row["freq_period"].ToString() : string.Empty) + " " + (row["freq_period_type"] != DBNull.Value ? row["freq_period_type"].ToString() : string.Empty),
                                        Keywords_tags = row["Keywords_tags"] != DBNull.Value ? row["Keywords_tags"].ToString() : string.Empty,
                                        pub_doc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty,
                                        supportFilesCount = row["supportFilesCount"] != DBNull.Value ? Convert.ToInt32(row["supportFilesCount"]) : 0,
                                        publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                        indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }




        //Getting drafts data

        [Route("api/GovControlReportsController/GetDrftsSaved/{userid}")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> GetDrftsSaved(int userid)

        {

            var pdata = new List<GetPubDocListModel>();


            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();

            string basequery = @"
        SELECT 
            e.AuthoritynameID,
            tbl.firstname as publisher_name,
            e.AuthorityTypeID,
            e.NatureOf_Doc_id,
            e.DocTypeID,
            e.Doc_SubCategoryID,
            e.Doc_CategoryID,
            e.Title_Doc,
            e.Doc_Confidentiality,
            e.Eff_Date, 
            e.Sub_title_doc, 
            e.Document_Id, 
            e.addDoc_createdDate, 
            e.VersionControlNo, 
            e.freq_period_type,
            e.Initial_creation_doc_date,
            e.Doc_internal_num,
            e.Doc_Inter_ver_num,
            e.Doc_Phy_Valut_Loc,
            e.Doc_process_Owner,
            e.Doc_Approver,
            e.Date_Doc_Revision,
            e.Date_Doc_Approver,
            e.freq_period,
            e.Keywords_tags,
            e.pub_doc,e.supportFilesCount,
            e.publisher_comments,
            e.indicative_reading_time,
            e.Review_Frequency_Status,
            e.Time_period,
            e.AddDoc_id,
            e.Obj_Doc,
            e.addDoc_Status,
            t.DocTypeName,
            C.Doc_CategoryName,
            sc.Doc_SubCategoryName,
            a.AuthorityName,e.ChangedOn,e.ChangedBy,tblu.firstname as lastEditedBy,
            at.AuthorityTypeName,
            p.NatureOf_Doc_Name
        FROM
            risk.add_doc e
        LEFT OUTER JOIN
            risk.doctype_master t ON t.DocTypeID = e.DocTypeID
        LEFT OUTER JOIN
            risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
        LEFT OUTER JOIN
            risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
        LEFT OUTER JOIN
            risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
        LEFT OUTER JOIN
            risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
        LEFT OUTER JOIN
            risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
        LEFT OUTER JOIN
            risk.tbluser tbl ON tbl.USR_ID = e.USR_ID
LEFT OUTER JOIN
            risk.tbluser tblu ON tblu.USR_ID = e.ChangedBy
        WHERE
             e.addDoc_Status = @addDoc_Status
            AND e.Draft_Status = @Draft_Status";



            if (locations.Any())
            {
                var locationConditions = string.Join(",", locations.Select((loc, index) => $"@loc{index}"));
                basequery += " AND (" + string.Join(" OR ", locations.Select((loc, index) => $"FIND_IN_SET(@loc{index}, e.Unit_location_Master_id) > 0")) + ")";
            }

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(basequery, con))
                    {
                        // Add parameters to the command

                        cmd.Parameters.AddWithValue("@addDoc_Status", "Active");
                        cmd.Parameters.AddWithValue("@Draft_Status", "Incomplete");


                        if (locations.Any())
                        {
                            for (int i = 0; i < locations.Count; i++)
                            {
                                cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);
                            }
                        }

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GetPubDocListModel
                                    {
                                        doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                        lastEditedby = row["lastEditedBy"] != DBNull.Value ? row["lastEditedBy"].ToString() : string.Empty,
                                        AddDoc_id = row["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(row["AddDoc_id"]) : 0,
                                        AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                        Publisher_name = row["publisher_name"] != DBNull.Value ? row["publisher_name"].ToString() : string.Empty,
                                        DocTypeID = row["DocTypeID"] != DBNull.Value ? Convert.ToInt32(row["DocTypeID"]) : 0,
                                        AuthoritynameID = row["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(row["AuthoritynameID"]) : 0,
                                        AuthorityTypeID = row["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(row["AuthorityTypeID"]) : 0,
                                        NatureOf_Doc_id = row["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(row["NatureOf_Doc_id"]) : 0,
                                        Doc_CategoryID = row["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_CategoryID"]) : 0,
                                        Doc_SubCategoryID = row["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_SubCategoryID"]) : 0,
                                        AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                        Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                        Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                        Obj_Doc = row["Obj_Doc"] != DBNull.Value ? row["Obj_Doc"].ToString() : string.Empty,
                                        addDoc_Status = row["addDoc_Status"] != DBNull.Value ? row["addDoc_Status"].ToString() : string.Empty,
                                        NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,
                                        Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                        Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                        addDoc_createdDate = row["addDoc_createdDate"] != DBNull.Value ? row["addDoc_createdDate"].ToString() : string.Empty,
                                        Initial_creation_doc_date = row["Initial_creation_doc_date"] != DBNull.Value ? row["Initial_creation_doc_date"].ToString() : string.Empty,
                                        Doc_internal_num = row["Doc_internal_num"] != DBNull.Value ? row["Doc_internal_num"].ToString() : string.Empty,
                                        Doc_Inter_ver_num = row["Doc_Inter_ver_num"] != DBNull.Value ? row["Doc_Inter_ver_num"].ToString() : string.Empty,
                                        Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"] != DBNull.Value ? row["Doc_Phy_Valut_Loc"].ToString() : string.Empty,
                                        Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                        Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,
                                        Date_Doc_Revision = row["Date_Doc_Revision"] != DBNull.Value ? row["Date_Doc_Revision"].ToString() : string.Empty,
                                        Review_Frequency_Status = row["Review_Frequency_Status"] != DBNull.Value ? Convert.ToInt32(row["Review_Frequency_Status"]) : 0,
                                        Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                        VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                        Date_Doc_Approver = row["Date_Doc_Approver"] != DBNull.Value ? row["Date_Doc_Approver"].ToString() : string.Empty,
                                        freq_period = (row["freq_period"] != DBNull.Value ? row["freq_period"].ToString() : string.Empty) + " " + (row["freq_period_type"] != DBNull.Value ? row["freq_period_type"].ToString() : string.Empty),
                                        Keywords_tags = row["Keywords_tags"] != DBNull.Value ? row["Keywords_tags"].ToString() : string.Empty,
                                        pub_doc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty,
                                        supportFilesCount = row["supportFilesCount"] != DBNull.Value ? Convert.ToInt32(row["supportFilesCount"]) : 0,
                                        publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                        indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }


        //List of Document Versioning


        [Route("api/GovControlReportsController/GetDocumentVersioning")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> GetDocumentVersioning([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GetPubDocListModel>();
            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == DateModels.userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string basequery = @"
        SELECT 
            e.AuthoritynameID,
            tbl.firstname as publisher_name,
            e.AuthorityTypeID,
            e.NatureOf_Doc_id,
            e.DocTypeID,
            e.Doc_SubCategoryID,
            e.Doc_CategoryID,
            e.Title_Doc,
            e.Doc_Confidentiality,
            e.Eff_Date, 
            e.Sub_title_doc, 
            e.Document_Id, 
            e.addDoc_createdDate, 
            e.VersionControlNo, 
            e.freq_period_type,
            e.Initial_creation_doc_date,
            e.Doc_internal_num,
            e.Doc_Inter_ver_num,
            e.Doc_Phy_Valut_Loc,
            e.Doc_process_Owner,
            e.Doc_Approver,
            e.Date_Doc_Revision,
            e.Date_Doc_Approver,
            e.freq_period,
            e.Keywords_tags,
            e.pub_doc,
 e.review_start_Date,
            e.publisher_comments,
            e.indicative_reading_time,
            e.Review_Frequency_Status,
            e.Time_period,
            e.AddDoc_id,
            e.Obj_Doc,
            e.addDoc_Status,
            t.DocTypeName,e.supportFilesCount,
            C.Doc_CategoryName,
            sc.Doc_SubCategoryName,
            a.AuthorityName,
            at.AuthorityTypeName,
            p.NatureOf_Doc_Name,e.ChangedBy,tblu.firstname as lastEditedBy,e.ChangedOn
        FROM
            risk.add_doc e
        LEFT OUTER JOIN
            risk.doctype_master t ON t.DocTypeID = e.DocTypeID
        LEFT OUTER JOIN
            risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
        LEFT OUTER JOIN
            risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
        LEFT OUTER JOIN
            risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
        LEFT OUTER JOIN
            risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
        LEFT OUTER JOIN
            risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
        LEFT OUTER JOIN
            risk.tbluser tbl ON tbl.USR_ID = e.USR_ID
LEFT OUTER JOIN
            risk.tbluser tblu ON tblu.USR_ID = e.ChangedBy
        WHERE
             e.addDoc_Status = @addDoc_Status
            AND e.Draft_Status = @Draft_Status AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Publishing" => "DATE(e.addDoc_createdDate) <= @Today AND DATE(e.addDoc_createdDate) >= @MonthAgo",
                "Effective" => "DATE(e.Eff_Date <= @Today) AND DATE(e.Eff_Date) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = basequery + dateCondition;
            if (locations.Any())
            {
                var locationConditions = string.Join(",", locations.Select((loc, index) => $"@loc{index}"));
                finalQuery += " AND (" + string.Join(" OR ", locations.Select((loc, index) => $"FIND_IN_SET(@loc{index}, e.Unit_location_Master_id) > 0")) + ")";
            }

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@addDoc_Status", "Active");
                        cmd.Parameters.AddWithValue("@Draft_Status", "Completed");

                        if (locations.Any())
                        {
                            for (int i = 0; i < locations.Count; i++)
                            {
                                cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);
                            }
                        }
                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    string originalPubDoc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty;
                                    string resolvedPubDoc = "";

                                    if (!string.IsNullOrWhiteSpace(originalPubDoc))
                                    {
                                        var idList = originalPubDoc.Split(',')
                                                                   .Select(id => id.Trim())
                                                                   .Where(id => int.TryParse(id, out _))
                                                                   .ToList();

                                        if (idList.Any())
                                        {
                                            var idParams = string.Join(",", idList.Select((id, index) => $"@id{index}"));
                                            var docIdQuery = $"SELECT AddDoc_id, Document_Id FROM risk.add_doc WHERE AddDoc_id IN ({idParams})";

                                            using (var cmd2 = new MySqlCommand(docIdQuery, con))
                                            {
                                                for (int i = 0; i < idList.Count; i++)
                                                {
                                                    cmd2.Parameters.AddWithValue($"@id{i}", idList[i]);
                                                }

                                                using (var rdr = cmd2.ExecuteReader())
                                                {
                                                    var resolved = new List<string>();
                                                    while (rdr.Read())
                                                    {
                                                        resolved.Add(rdr["Document_Id"]?.ToString());
                                                    }
                                                    resolvedPubDoc = string.Join(",", resolved);
                                                }
                                            }
                                        }
                                    }
                                    string reviewStartDateString = row["review_start_Date"] != DBNull.Value ? row["review_start_Date"].ToString() : string.Empty;
                                    DateTime startDate;
                                    int noOfDays = 0;
                                    string validation = string.Empty;

                                    if (!string.IsNullOrEmpty(reviewStartDateString))
                                    {
                                        try
                                        {
                                            startDate = DateTime.ParseExact(reviewStartDateString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                                            string todaysDate = DateTime.Now.ToString("dd-MM-yyyy");
                                            DateTime endDate = DateTime.ParseExact(todaysDate, "dd-MM-yyyy", null);
                                            TimeSpan span = startDate - endDate;
                                            noOfDays = span.Days;
                                            validation = GetValidationMessage(noOfDays);
                                        }
                                        catch (FormatException)
                                        {
                                            // Handle parsing error if the date format is incorrect
                                            validation = "Invalid review start date format";
                                        }
                                    }
                                    pdata.Add(new GetPubDocListModel
                                    {
                                        doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                        lastEditedby = row["lastEditedBy"] != DBNull.Value ? row["lastEditedBy"].ToString() : string.Empty,
                                        AddDoc_id = row["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(row["AddDoc_id"]) : 0,
                                        AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                        Publisher_name = row["publisher_name"] != DBNull.Value ? row["publisher_name"].ToString() : string.Empty,
                                        DocTypeID = row["DocTypeID"] != DBNull.Value ? Convert.ToInt32(row["DocTypeID"]) : 0,
                                        AuthoritynameID = row["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(row["AuthoritynameID"]) : 0,

                                        AuthorityTypeID = row["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(row["AuthorityTypeID"]) : 0,
                                        NatureOf_Doc_id = row["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(row["NatureOf_Doc_id"]) : 0,
                                        Doc_CategoryID = row["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_CategoryID"]) : 0,
                                        Doc_SubCategoryID = row["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_SubCategoryID"]) : 0,
                                        AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                        Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                        Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                        Obj_Doc = row["Obj_Doc"] != DBNull.Value ? row["Obj_Doc"].ToString() : string.Empty,
                                        addDoc_Status = row["addDoc_Status"] != DBNull.Value ? row["addDoc_Status"].ToString() : string.Empty,
                                        NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,


                                        Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                        Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                        addDoc_createdDate = row["addDoc_createdDate"] != DBNull.Value ? row["addDoc_createdDate"].ToString() : string.Empty,
                                        Initial_creation_doc_date = row["Initial_creation_doc_date"] != DBNull.Value ? row["Initial_creation_doc_date"].ToString() : string.Empty,
                                        Doc_internal_num = row["Doc_internal_num"] != DBNull.Value ? row["Doc_internal_num"].ToString() : string.Empty,
                                        Doc_Inter_ver_num = row["Doc_Inter_ver_num"] != DBNull.Value ? row["Doc_Inter_ver_num"].ToString() : string.Empty,
                                        Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"] != DBNull.Value ? row["Doc_Phy_Valut_Loc"].ToString() : string.Empty,
                                        Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                        Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,

                                        Date_Doc_Revision = row["Date_Doc_Revision"] != DBNull.Value ? row["Date_Doc_Revision"].ToString() : string.Empty,
                                        Review_Frequency_Status = row["Review_Frequency_Status"] != DBNull.Value ? Convert.ToInt32(row["Review_Frequency_Status"]) : 0,
                                        Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                        VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                        latestVersion = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                        Date_Doc_Approver = row["Date_Doc_Approver"] != DBNull.Value ? row["Date_Doc_Approver"].ToString() : string.Empty,

                                        freq_period = (row["freq_period"] != DBNull.Value ? row["freq_period"].ToString() : string.Empty) + " " + (row["freq_period_type"] != DBNull.Value ? row["freq_period_type"].ToString() : string.Empty),
                                        Keywords_tags = row["Keywords_tags"] != DBNull.Value ? row["Keywords_tags"].ToString() : string.Empty,
                                        //pub_doc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty,
                                        pub_doc = resolvedPubDoc,
                                        supportFilesCount = row["supportFilesCount"] != DBNull.Value ? Convert.ToInt32(row["supportFilesCount"]) : 0,
                                        publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                        indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                        NoofDays = noOfDays,
                                        validations = validation,
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }




        //list of Disabled Published Document


        [Route("api/GovControlReportsController/GetDocumentDisabled")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> GetDocumentDisabled([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GetPubDocListModel>();
            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == DateModels.userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string basequery = @"
        SELECT 
            e.AuthoritynameID,
            tbl.firstname as publisher_name,
            e.AuthorityTypeID,
            e.NatureOf_Doc_id,
            e.DocTypeID,
            e.Doc_SubCategoryID,
            e.Doc_CategoryID,
            e.Title_Doc,
            e.Doc_Confidentiality,
            e.Eff_Date, 
            e.Sub_title_doc, 
            e.Document_Id, 
            e.addDoc_createdDate, 
            e.VersionControlNo, 
            e.freq_period_type,
            e.Initial_creation_doc_date,
            e.Doc_internal_num,
            e.Doc_Inter_ver_num,
            e.Doc_Phy_Valut_Loc,
            e.Doc_process_Owner,
            e.Doc_Approver,
            e.Date_Doc_Revision,
            e.Date_Doc_Approver,
            e.freq_period,
            e.Keywords_tags,
            e.pub_doc,e.supportFilesCount,
            e.publisher_comments,
            e.indicative_reading_time,
            e.Review_Frequency_Status,
            e.Time_period,
            e.AddDoc_id,
            e.Obj_Doc,
            e.addDoc_Status,
e.DisableReason,e.ChangedBy,tblu.firstname as lastEditedBy,e.ChangedOn,
            t.DocTypeName,
            C.Doc_CategoryName,
            sc.Doc_SubCategoryName,
            a.AuthorityName,
            at.AuthorityTypeName,
            p.NatureOf_Doc_Name
        FROM
            risk.add_doc e
        LEFT OUTER JOIN
            risk.doctype_master t ON t.DocTypeID = e.DocTypeID
        LEFT OUTER JOIN
            risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
        LEFT OUTER JOIN
            risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
        LEFT OUTER JOIN
            risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
        LEFT OUTER JOIN
            risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
        LEFT OUTER JOIN
            risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
        LEFT OUTER JOIN
            risk.tbluser tbl ON tbl.USR_ID = e.USR_ID
LEFT OUTER JOIN
            risk.tbluser tblu ON tblu.USR_ID = e.ChangedBy
        WHERE
             e.addDoc_Status = @addDoc_Status AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Publishing" => "DATE(e.addDoc_createdDate) <= @Today AND DATE(e.addDoc_createdDate) >= @MonthAgo",
                "Effective" => "DATE(e.Eff_Date) <= @Today AND DATE(e.Eff_Date) >= @MonthAgo",
                "Disable" => "DATE(e.ChangedOn) <= @Today AND DATE(e.ChangedOn) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = basequery + dateCondition;
            if (locations.Any())
            {
                var locationConditions = string.Join(",", locations.Select((loc, index) => $"@loc{index}"));
                finalQuery += " AND (" + string.Join(" OR ", locations.Select((loc, index) => $"FIND_IN_SET(@loc{index}, e.Unit_location_Master_id) > 0")) + ")";
            }

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@addDoc_Status", "Disabled");
                        if (locations.Any())
                        {
                            for (int i = 0; i < locations.Count; i++)
                            {
                                cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);
                            }
                        }

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GetPubDocListModel
                                    {
                                        doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                        lastEditedby = row["lastEditedBy"] != DBNull.Value ? row["lastEditedBy"].ToString() : string.Empty,
                                        AddDoc_id = row["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(row["AddDoc_id"]) : 0,
                                        AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                        Publisher_name = row["publisher_name"] != DBNull.Value ? row["publisher_name"].ToString() : string.Empty,
                                        DocTypeID = row["DocTypeID"] != DBNull.Value ? Convert.ToInt32(row["DocTypeID"]) : 0,
                                        AuthoritynameID = row["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(row["AuthoritynameID"]) : 0,
                                        AuthorityTypeID = row["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(row["AuthorityTypeID"]) : 0,
                                        NatureOf_Doc_id = row["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(row["NatureOf_Doc_id"]) : 0,
                                        Doc_CategoryID = row["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_CategoryID"]) : 0,
                                        Doc_SubCategoryID = row["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_SubCategoryID"]) : 0,
                                        AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                        Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                        Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                        Obj_Doc = row["Obj_Doc"] != DBNull.Value ? row["Obj_Doc"].ToString() : string.Empty,
                                        addDoc_Status = row["addDoc_Status"] != DBNull.Value ? row["addDoc_Status"].ToString() : string.Empty,
                                        NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,
                                        Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                        Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                        addDoc_createdDate = row["addDoc_createdDate"] != DBNull.Value ? row["addDoc_createdDate"].ToString() : string.Empty,
                                        Initial_creation_doc_date = row["Initial_creation_doc_date"] != DBNull.Value ? row["Initial_creation_doc_date"].ToString() : string.Empty,
                                        Doc_internal_num = row["Doc_internal_num"] != DBNull.Value ? row["Doc_internal_num"].ToString() : string.Empty,
                                        Doc_Inter_ver_num = row["Doc_Inter_ver_num"] != DBNull.Value ? row["Doc_Inter_ver_num"].ToString() : string.Empty,
                                        Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"] != DBNull.Value ? row["Doc_Phy_Valut_Loc"].ToString() : string.Empty,
                                        Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                        Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,
                                        Date_Doc_Revision = row["Date_Doc_Revision"] != DBNull.Value ? row["Date_Doc_Revision"].ToString() : string.Empty,
                                        Review_Frequency_Status = row["Review_Frequency_Status"] != DBNull.Value ? Convert.ToInt32(row["Review_Frequency_Status"]) : 0,
                                        Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                        VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                        Date_Doc_Approver = row["Date_Doc_Approver"] != DBNull.Value ? row["Date_Doc_Approver"].ToString() : string.Empty,
                                        freq_period = (row["freq_period"] != DBNull.Value ? row["freq_period"].ToString() : string.Empty) + " " + (row["freq_period_type"] != DBNull.Value ? row["freq_period_type"].ToString() : string.Empty),
                                        Keywords_tags = row["Keywords_tags"] != DBNull.Value ? row["Keywords_tags"].ToString() : string.Empty,
                                        pub_doc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty,
                                        supportFilesCount = row["supportFilesCount"] != DBNull.Value ? Convert.ToInt32(row["supportFilesCount"]) : 0,
                                        publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                        DisableReason = row["DisableReason"] != DBNull.Value ? row["DisableReason"].ToString() : string.Empty,
                                        indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }






        //list of Published Document Review Status


        [Route("api/GovControlReportsController/GetDocumentReviewStatus")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> GetDocumentReviewStatus([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GetPubDocListModel>();

            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == DateModels.userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();

            try
            {
                // Validate and parse the dates
                if (string.IsNullOrEmpty(DateModels.today) || string.IsNullOrEmpty(DateModels.oneMonthAgo))
                {
                    throw new ArgumentException("Date values cannot be null or empty");
                }

                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string basequery = @"
    SELECT 
        e.AuthoritynameID,
        tbl.firstname as publisher_name,
        e.AuthorityTypeID,
        e.NatureOf_Doc_id,
        e.DocTypeID,
        e.Doc_SubCategoryID,
        e.Doc_CategoryID,
        e.Title_Doc,
        e.Doc_Confidentiality,
        e.Eff_Date, 
        e.Sub_title_doc, 
        e.Document_Id, 
        e.addDoc_createdDate, 
        e.VersionControlNo, 
        e.freq_period_type,
        e.Initial_creation_doc_date,
        e.Doc_internal_num,
        e.Doc_Inter_ver_num,
        e.Doc_Phy_Valut_Loc,
        e.Doc_process_Owner,
        e.Doc_Approver,
        e.Date_Doc_Revision,
        e.Date_Doc_Approver,
        e.freq_period,
        e.Keywords_tags,
        e.pub_doc,e.supportFilesCount,
        e.publisher_comments,
        e.indicative_reading_time,
        e.Review_Frequency_Status,
        e.Time_period,
        e.AddDoc_id,
        e.Obj_Doc,
        e.addDoc_Status,
        e.review_start_Date,
        e.DisableReason,
        t.DocTypeName,
        C.Doc_CategoryName,
        sc.Doc_SubCategoryName,
        a.AuthorityName,
        at.AuthorityTypeName,
        p.NatureOf_Doc_Name,e.ChangedOn
    FROM
        risk.add_doc e
    LEFT OUTER JOIN
        risk.doctype_master t ON t.DocTypeID = e.DocTypeID
    LEFT OUTER JOIN
        risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
    LEFT OUTER JOIN
        risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
    LEFT OUTER JOIN
        risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
    LEFT OUTER JOIN
        risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
    LEFT OUTER JOIN
        risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
    LEFT OUTER JOIN
        risk.tbluser tbl ON tbl.USR_ID = e.USR_ID
    WHERE
        e.addDoc_Status = @addDoc_Status AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Publishing" => "DATE(e.addDoc_createdDate) <= @Today AND DATE(e.addDoc_createdDate) >= @MonthAgo",
                "Effective" => "DATE(e.Eff_Date) <= @Today AND DATE(e.Eff_Date) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = basequery + dateCondition;
            if (locations.Any())
            {
                var locationConditions = string.Join(",", locations.Select((loc, index) => $"@loc{index}"));
                finalQuery += " AND (" + string.Join(" OR ", locations.Select((loc, index) => $"FIND_IN_SET(@loc{index}, e.Unit_location_Master_id) > 0")) + ")";
            }

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@addDoc_Status", "Active");

                        if (locations.Any())
                        {
                            for (int i = 0; i < locations.Count; i++)
                            {
                                cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);
                            }
                        }


                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    string reviewStartDateString = row["review_start_Date"] != DBNull.Value ? row["review_start_Date"].ToString() : string.Empty;
                                    DateTime startDate;
                                    int noOfDays = 0;
                                    string validation = string.Empty;

                                    if (!string.IsNullOrEmpty(reviewStartDateString))
                                    {
                                        try
                                        {
                                            startDate = DateTime.ParseExact(reviewStartDateString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                                            string todaysDate = DateTime.Now.ToString("dd-MM-yyyy");
                                            DateTime endDate = DateTime.ParseExact(todaysDate, "dd-MM-yyyy", null);
                                            TimeSpan span = startDate - endDate;
                                            noOfDays = span.Days;
                                            validation = GetValidationMessage(noOfDays);
                                        }
                                        catch (FormatException)
                                        {
                                            // Handle parsing error if the date format is incorrect
                                            validation = "Invalid review start date format";
                                        }
                                    }


                                    string originalPubDoc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty;
                                    string resolvedPubDoc = "";

                                    if (!string.IsNullOrWhiteSpace(originalPubDoc))
                                    {
                                        var idList = originalPubDoc.Split(',')
                                                                   .Select(id => id.Trim())
                                                                   .Where(id => int.TryParse(id, out _))
                                                                   .ToList();

                                        if (idList.Any())
                                        {
                                            var idParams = string.Join(",", idList.Select((id, index) => $"@id{index}"));
                                            var docIdQuery = $"SELECT AddDoc_id, Document_Id FROM risk.add_doc WHERE AddDoc_id IN ({idParams})";

                                            using (var cmd2 = new MySqlCommand(docIdQuery, con))
                                            {
                                                for (int i = 0; i < idList.Count; i++)
                                                {
                                                    cmd2.Parameters.AddWithValue($"@id{i}", idList[i]);
                                                }

                                                using (var rdr = cmd2.ExecuteReader())
                                                {
                                                    var resolved = new List<string>();
                                                    while (rdr.Read())
                                                    {
                                                        resolved.Add(rdr["Document_Id"]?.ToString());
                                                    }
                                                    resolvedPubDoc = string.Join(",", resolved);
                                                }
                                            }
                                        }
                                    }

                                    pdata.Add(new GetPubDocListModel
                                    {
                                        doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                        AddDoc_id = row["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(row["AddDoc_id"]) : 0,
                                        AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                        Publisher_name = row["publisher_name"] != DBNull.Value ? row["publisher_name"].ToString() : string.Empty,
                                        DocTypeID = row["DocTypeID"] != DBNull.Value ? Convert.ToInt32(row["DocTypeID"]) : 0,
                                        AuthoritynameID = row["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(row["AuthoritynameID"]) : 0,
                                        AuthorityTypeID = row["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(row["AuthorityTypeID"]) : 0,
                                        NatureOf_Doc_id = row["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(row["NatureOf_Doc_id"]) : 0,
                                        Doc_CategoryID = row["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_CategoryID"]) : 0,
                                        Doc_SubCategoryID = row["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_SubCategoryID"]) : 0,
                                        AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                        Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                        Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                        Obj_Doc = row["Obj_Doc"] != DBNull.Value ? row["Obj_Doc"].ToString() : string.Empty,

                                        addDoc_Status = row["addDoc_Status"] != DBNull.Value ? row["addDoc_Status"].ToString() : string.Empty,
                                        NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,
                                        Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                        Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                        addDoc_createdDate = row["addDoc_createdDate"] != DBNull.Value ? row["addDoc_createdDate"].ToString() : string.Empty,
                                        Initial_creation_doc_date = row["Initial_creation_doc_date"] != DBNull.Value ? row["Initial_creation_doc_date"].ToString() : string.Empty,
                                        Doc_internal_num = row["Doc_internal_num"] != DBNull.Value ? row["Doc_internal_num"].ToString() : string.Empty,
                                        Doc_Inter_ver_num = row["Doc_Inter_ver_num"] != DBNull.Value ? row["Doc_Inter_ver_num"].ToString() : string.Empty,
                                        Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"] != DBNull.Value ? row["Doc_Phy_Valut_Loc"].ToString() : string.Empty,
                                        Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                        Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,
                                        Date_Doc_Revision = row["Date_Doc_Revision"] != DBNull.Value ? row["Date_Doc_Revision"].ToString() : string.Empty,
                                        Review_Frequency_Status = row["Review_Frequency_Status"] != DBNull.Value ? Convert.ToInt32(row["Review_Frequency_Status"]) : 0,
                                        Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                        VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                        Date_Doc_Approver = row["Date_Doc_Approver"] != DBNull.Value ? row["Date_Doc_Approver"].ToString() : string.Empty,
                                        freq_period = (row["freq_period"] != DBNull.Value ? row["freq_period"].ToString() : string.Empty) + " " + (row["freq_period_type"] != DBNull.Value ? row["freq_period_type"].ToString() : string.Empty),
                                        Keywords_tags = row["Keywords_tags"] != DBNull.Value ? row["Keywords_tags"].ToString() : string.Empty,
                                        //  pub_doc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty,
                                        pub_doc = resolvedPubDoc,
                                        supportFilesCount = row["supportFilesCount"] != DBNull.Value ? Convert.ToInt32(row["supportFilesCount"]) : 0,
                                        publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                        DisableReason = row["DisableReason"] != DBNull.Value ? row["DisableReason"].ToString() : string.Empty,
                                        indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                        NoofDays = noOfDays,
                                        validations = validation,
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }




        public string GetValidationMessage(int NoofDays)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string validation = "";

            MySqlDataAdapter da = new MySqlDataAdapter($"SELECT * FROM reviewstatussettings WHERE {NoofDays} >=MinimumDays and {NoofDays}<=MaximumDays and (ReviewStatusName='Expiring Soon' or ReviewStatusName='Take Immediate Action') ", con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                validation = dt.Rows[0]["ReviewStatusName"].ToString();
            }
            else
            {
                MySqlDataAdapter da1 = new MySqlDataAdapter($"SELECT * FROM reviewstatussettings WHERE {NoofDays} >MinimumDays  and ReviewStatusName='Not Due'", con);
                DataTable dt1 = new DataTable();
                da1.Fill(dt1);
                if (dt1.Rows.Count > 0)
                {
                    validation = dt1.Rows[0]["ReviewStatusName"].ToString();
                }
                else
                {
                    MySqlDataAdapter da2 = new MySqlDataAdapter($"SELECT * FROM reviewstatussettings WHERE {NoofDays} <MinimumDays and ReviewStatusName='Expired'", con);
                    DataTable dt2 = new DataTable();
                    da2.Fill(dt2);
                    if (dt2.Rows.Count > 0)
                    {
                        validation = dt2.Rows[0]["ReviewStatusName"].ToString();
                    }

                }
            }
            return validation;
        }




        //List of Acknowledgement Requested Document


        [Route("api/GovControlReportsController/GetAcknowledgementRequested")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> GetAcknowledgementRequested([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GetPubDocListModel>();
            var locations = (from usermapping in mySqlDBContext.userlocationmappingModels
                             where usermapping.user_location_mapping_status == "Active"
                                   && usermapping.USR_ID == DateModels.userid
                             select usermapping.Unit_location_Master_id).Distinct().ToList();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string basequery = @"
SELECT 
    ad.AuthoritynameID,
    tbl.firstname as publisher_name,
    ad.AuthorityTypeID,
    ad.NatureOf_Doc_id,
    ad.DocTypeID,
    ad.Doc_SubCategoryID,
    ad.Doc_CategoryID,
    ad.Title_Doc,
    ad.Doc_Confidentiality,
    ad.Eff_Date, 
    ad.Sub_title_doc, 
ad.review_start_Date,
    ad.Document_Id, 
    ad.addDoc_createdDate, 
    ad.VersionControlNo, 
    ad.freq_period_type,
    ad.Initial_creation_doc_date,
    ad.Doc_internal_num,
    ad.Doc_Inter_ver_num,
    ad.Doc_process_Owner,
    ad.Doc_Phy_Valut_Loc,
    ad.Doc_process_Owner,
    ad.Doc_process_Owner,
    ad.Doc_Approver,
    ad.Date_Doc_Revision,
    ad.Date_Doc_Approver,
    ad.freq_period,
    ad.Keywords_tags,
    ad.pub_doc,ad.ChangedOn,
    ad.pub_doc,ad.supportFilesCount,
            ad.publisher_comments,
            ad.indicative_reading_time,
    ad.Review_Frequency_Status,
    ad.Time_period,
    ad.AddDoc_id,
    ad.Obj_Doc,
    ad.addDoc_Status,
    ad.DisableReason,
    ack_counts.duedate,ack_counts.ack_status,ack_counts.Doc_User_Access_mapping_id,ack_counts.provideack_status,
    t.DocTypeName,
    C.Doc_CategoryName,
    sc.Doc_SubCategoryName,
    a.AuthorityName,
    at.AuthorityTypeName,
    p.NatureOf_Doc_Name,IFNULL(ack_counts.ack_count, 0) AS ack_count,
 ulm.Unit_location_Master_name AS locationname,
  em.Entity_Master_Name AS companyname
FROM
    risk.doc_user_access_mapping e  
LEFT OUTER JOIN
    risk.add_doc ad ON ad.AddDoc_id = e.AddDoc_id
LEFT OUTER JOIN
    risk.doctype_master t ON t.DocTypeID = ad.DocTypeID
LEFT OUTER JOIN
    risk.doccategory_master C ON C.Doc_CategoryID = ad.Doc_CategoryID
LEFT OUTER JOIN
    risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = ad.Doc_SubCategoryID
LEFT OUTER JOIN
    risk.authorityname_master a ON a.AuthoritynameID = ad.AuthoritynameID
LEFT OUTER JOIN
    risk.authoritytype_master at ON at.AuthorityTypeID = ad.AuthorityTypeID
LEFT OUTER JOIN
    risk.natureof_doc p ON p.NatureOf_Doc_id = ad.NatureOf_Doc_id
LEFT OUTER JOIN
    risk.tbluser tbl ON tbl.USR_ID = ad.USR_ID

 LEFT OUTER JOIN 
            risk.entity_master em ON em.Entity_Master_id = e.Entity_Master_id
        LEFT OUTER JOIN 
            risk.unit_location_master ulm ON ulm.Unit_location_Master_id = e.Unit_location_Master_id
LEFT OUTER JOIN
(SELECT 
     Doc_User_Access_mapping_id, 
     COUNT(*) AS ack_count,max(duedate) as duedate,max(ack_status)as ack_status,MAX(provideack_status) AS provideack_status
 FROM 
     risk.doc_taskuseracknowledment_status
 GROUP BY 
     Doc_User_Access_mapping_id) AS ack_counts
ON 
    ack_counts.Doc_User_Access_mapping_id = e.Doc_User_Access_mapping_id
WHERE
    ad.addDoc_Status = @addDoc_Status AND  ";
            //LEFT OUTER JOIN
            // risk.doc_user_access_mapping uam ON uam.AddDoc_id = e.AddDoc_id

            string dateCondition = DateModels.datetype switch
            {
                "Publishing" => "DATE(ad.addDoc_createdDate) <= @Today AND DATE(ad.addDoc_createdDate) >= @MonthAgo",
                "Effective" => "DATE(ad.Eff_Date) <= @Today AND DATE(ad.Eff_Date) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = basequery + dateCondition;
            if (locations.Any())
            {
                var locationConditions = string.Join(",", locations.Select((loc, index) => $"@loc{index}"));
                finalQuery += " AND (" + string.Join(" OR ", locations.Select((loc, index) => $"FIND_IN_SET(@loc{index}, e.Unit_location_Master_id) > 0")) + ")";
            }

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@addDoc_Status", "Active");

                        if (locations.Any())
                        {
                            for (int i = 0; i < locations.Count; i++)
                            {
                                cmd.Parameters.AddWithValue($"@loc{i}", locations[i]);
                            }
                        }

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    DateTime? duedate = null;
                                    int? daysLeft = null;

                                    if (row["duedate"] != DBNull.Value)
                                    {
                                        if (DateTime.TryParse(row["duedate"].ToString(), out DateTime parsedDate))
                                        {
                                            duedate = parsedDate;
                                            daysLeft = (duedate - DateTime.Today).Value.Days;
                                        }
                                    }
                                    string originalPubDoc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty;
                                    string resolvedPubDoc = "";

                                    if (!string.IsNullOrWhiteSpace(originalPubDoc))
                                    {
                                        var idList = originalPubDoc.Split(',')
                                                                   .Select(id => id.Trim())
                                                                   .Where(id => int.TryParse(id, out _))
                                                                   .ToList();

                                        if (idList.Any())
                                        {
                                            var idParams = string.Join(",", idList.Select((id, index) => $"@id{index}"));
                                            var docIdQuery = $"SELECT AddDoc_id, Document_Id FROM risk.add_doc WHERE AddDoc_id IN ({idParams})";

                                            using (var cmd2 = new MySqlCommand(docIdQuery, con))
                                            {
                                                for (int i = 0; i < idList.Count; i++)
                                                {
                                                    cmd2.Parameters.AddWithValue($"@id{i}", idList[i]);
                                                }

                                                using (var rdr = cmd2.ExecuteReader())
                                                {
                                                    var resolved = new List<string>();
                                                    while (rdr.Read())
                                                    {
                                                        resolved.Add(rdr["Document_Id"]?.ToString());
                                                    }
                                                    resolvedPubDoc = string.Join(",", resolved);
                                                }
                                            }
                                        }
                                    }


                                    string reviewStartDateString = row["review_start_Date"] != DBNull.Value ? row["review_start_Date"].ToString() : string.Empty;
                                    DateTime startDate;
                                    int noOfDays = 0;
                                    string validation = string.Empty;

                                    if (!string.IsNullOrEmpty(reviewStartDateString))
                                    {
                                        try
                                        {
                                            startDate = DateTime.ParseExact(reviewStartDateString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                                            string todaysDate = DateTime.Now.ToString("dd-MM-yyyy");
                                            DateTime endDate = DateTime.ParseExact(todaysDate, "dd-MM-yyyy", null);
                                            TimeSpan span = startDate - endDate;
                                            noOfDays = span.Days;
                                            validation = GetValidationMessage(noOfDays);
                                        }
                                        catch (FormatException)
                                        {
                                            // Handle parsing error if the date format is incorrect
                                            validation = "Invalid review start date format";
                                        }
                                    }

                                    pdata.Add(new GetPubDocListModel
                                    {
                                        doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                        AddDoc_id = row["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(row["AddDoc_id"]) : 0,
                                        AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                        Publisher_name = row["publisher_name"] != DBNull.Value ? row["publisher_name"].ToString() : string.Empty,
                                        DocTypeID = row["DocTypeID"] != DBNull.Value ? Convert.ToInt32(row["DocTypeID"]) : 0,
                                        AuthoritynameID = row["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(row["AuthoritynameID"]) : 0,
                                        AuthorityTypeID = row["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(row["AuthorityTypeID"]) : 0,
                                        NatureOf_Doc_id = row["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(row["NatureOf_Doc_id"]) : 0,
                                        Doc_CategoryID = row["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_CategoryID"]) : 0,
                                        Doc_SubCategoryID = row["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(row["Doc_SubCategoryID"]) : 0,
                                        AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                        Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                        Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                        Obj_Doc = row["Obj_Doc"] != DBNull.Value ? row["Obj_Doc"].ToString() : string.Empty,
                                        addDoc_Status = row["addDoc_Status"] != DBNull.Value ? row["addDoc_Status"].ToString() : string.Empty,
                                        NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,
                                        Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                        Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                        addDoc_createdDate = row["addDoc_createdDate"] != DBNull.Value ? row["addDoc_createdDate"].ToString() : string.Empty,
                                        Initial_creation_doc_date = row["Initial_creation_doc_date"] != DBNull.Value ? row["Initial_creation_doc_date"].ToString() : string.Empty,
                                        Doc_internal_num = row["Doc_internal_num"] != DBNull.Value ? row["Doc_internal_num"].ToString() : string.Empty,
                                        Doc_Inter_ver_num = row["Doc_Inter_ver_num"] != DBNull.Value ? row["Doc_Inter_ver_num"].ToString() : string.Empty,
                                        Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"] != DBNull.Value ? row["Doc_Phy_Valut_Loc"].ToString() : string.Empty,
                                        Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                        Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,
                                        Date_Doc_Revision = row["Date_Doc_Revision"] != DBNull.Value ? row["Date_Doc_Revision"].ToString() : string.Empty,
                                        Review_Frequency_Status = row["Review_Frequency_Status"] != DBNull.Value ? Convert.ToInt32(row["Review_Frequency_Status"]) : 0,
                                        Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                        VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                        Date_Doc_Approver = row["Date_Doc_Approver"] != DBNull.Value ? row["Date_Doc_Approver"].ToString() : string.Empty,
                                        duedate = duedate,
                                        // ack_status = (row["ack_status"] != DBNull.Value && !string.IsNullOrEmpty(row["ack_status"].ToString())) ? "Yes" : "No",
                                        ack_status = row["ack_status"] != DBNull.Value
    ? (string.Equals(row["ack_status"].ToString(), "true", StringComparison.OrdinalIgnoreCase)
        ? "Acknowledgement Pending"
        : string.Equals(row["ack_status"].ToString(), "false", StringComparison.OrdinalIgnoreCase)
            ? "Acknowledgement Not Required"
            : row["ack_status"].ToString())
    : "Reading in Progress",
                                        provideack_status = (row["provideack_status"] != DBNull.Value && Convert.ToBoolean(row["provideack_status"])) ? "Yes" : "No",
                                        DaysLeft = (daysLeft > 0) ? daysLeft : 0,
                                        Doc_User_Access_mapping_id = row["Doc_User_Access_mapping_id"] != DBNull.Value ? Convert.ToInt32(row["Doc_User_Access_mapping_id"]) : 0,
                                        noOfMappedUsers = Convert.ToInt32(row["ack_count"]),

                                        companyname = row["companyname"] != DBNull.Value ? row["companyname"].ToString() : string.Empty,
                                        locationname = row["locationname"] != DBNull.Value ? row["locationname"].ToString() : string.Empty,
                                        // pub_doc = row["pub_doc"] != DBNull.Value ? row["pub_doc"].ToString() : string.Empty,
                                        pub_doc = resolvedPubDoc,
                                        supportFilesCount = row["supportFilesCount"] != DBNull.Value ? Convert.ToInt32(row["supportFilesCount"]) : 0,
                                        publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                        indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),


                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new InvalidOperationException("Data retrieval failed", ex);
            }

            return pdata;
        }




        [Route("api/GovControlReportsController/Acknowledgementdocumentsusers/{docid}")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> Acknowledgementdocumentsusers(int docid)
        {
            var pdata = new List<GetPubDocListModel>();
            string basequery = @"
        SELECT 
            e.AuthoritynameID,
            tbl.firstname as publisher_name,
            e.AuthorityTypeID,
            e.NatureOf_Doc_id,
            e.DocTypeID,
            e.Doc_SubCategoryID,
            e.Doc_CategoryID,
            e.Title_Doc,
            e.Doc_Confidentiality,
            e.Eff_Date, 
            e.Sub_title_doc, 
            e.Document_Id, 
            e.addDoc_createdDate, 
            e.VersionControlNo, 
            e.freq_period_type,
            e.Initial_creation_doc_date,
            e.Doc_internal_num,
            e.Doc_Inter_ver_num,
            e.Doc_Phy_Valut_Loc,
            e.Doc_process_Owner,
            e.Doc_Approver,
            e.Date_Doc_Revision,
            e.Date_Doc_Approver,
            e.freq_period,
            e.Keywords_tags,
            e.pub_doc,
            e.supportFilesCount,
            e.publisher_comments,
            e.indicative_reading_time,
            e.Review_Frequency_Status,
            e.Time_period,
            e.AddDoc_id,
            e.Obj_Doc,
            e.addDoc_Status,
            e.DisableReason,
            userack.duedate,
            t.DocTypeName,
            C.Doc_CategoryName,
            sc.Doc_SubCategoryName,
            a.AuthorityName,
            at.AuthorityTypeName,
            p.NatureOf_Doc_Name,
            tu.firstname as nameofuser,
            tu.emailid,
            userack.USR_ID,
            userack.ack_status,
            userack.createddate as ackusercreateddate,
            userack.acknowledged_date,
            userack.readComplete_date,
            ulm.Unit_location_Master_name AS locationname,
            em.Entity_Master_Name AS companyname,
 (SELECT GROUP_CONCAT(dpr.publish_Name SEPARATOR ', ')
     FROM risk.doc_perm_rights dpr
     JOIN risk.doc_user_permission_mapping dupm 
       ON dpr.Doc_perm_rights_id = dupm.Doc_perm_rights_id
     WHERE dupm.Doc_User_Access_mapping_id = uam.Doc_User_Access_mapping_id
       AND dupm.USR_ID = userack.USR_ID) AS permissions_names
        FROM
            risk.add_doc e
        LEFT OUTER JOIN
            risk.doctype_master t ON t.DocTypeID = e.DocTypeID
        LEFT OUTER JOIN
            risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
        LEFT OUTER JOIN
            risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
        LEFT OUTER JOIN
            risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
        LEFT OUTER JOIN
            risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
        LEFT OUTER JOIN
            risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
        LEFT OUTER JOIN
            risk.tbluser tbl ON tbl.USR_ID = e.USR_ID
        LEFT OUTER JOIN
            risk.doc_taskuseracknowledment_status userack ON userack.AddDoc_id = e.AddDoc_id
        LEFT OUTER JOIN
            risk.doc_user_access_mapping uam ON uam.doc_User_Access_mapping_id = userack.doc_User_Access_mapping_id                            
        LEFT OUTER JOIN
            risk.tbluser tu ON tu.USR_ID = userack.USR_ID
        LEFT OUTER JOIN 
            entity_master em ON em.Entity_Master_id = uam.Entity_Master_id
        LEFT OUTER JOIN 
            unit_location_master ulm ON ulm.Unit_location_Master_id = uam.Unit_location_Master_id
         WHERE
            e.addDoc_Status = 'Active' AND userack.doc_User_Access_mapping_id = @doc_User_Access_mapping_id";

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(basequery, con))
                    {
                        cmd.Parameters.AddWithValue("@addDoc_Status", "Active");
                        cmd.Parameters.AddWithValue("@doc_User_Access_mapping_id", docid);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            foreach (DataRow row in dt.Rows)
                            {
                                DateTime? duedate = null;
                                int? daysLeft = null;

                                if (row["duedate"] != DBNull.Value && DateTime.TryParse(row["duedate"].ToString(), out DateTime parsedDate))
                                {
                                    duedate = parsedDate;
                                    daysLeft = (duedate.Value - DateTime.Today).Days;
                                }

                                DateTime? ackusercreateddate = row["ackusercreateddate"] as DateTime?;
                                DateTime? acknowledgementdate = row["acknowledged_date"] as DateTime?;
                                DateTime? readcompleted = row["readComplete_date"] as DateTime?;

                                TimeSpan? timeTakenToComplete = null;
                                if (acknowledgementdate.HasValue && readcompleted.HasValue)
                                {
                                    timeTakenToComplete = readcompleted.Value - acknowledgementdate.Value;
                                }

                                TimeSpan? timeToAcknowledge = null;
                                if (ackusercreateddate.HasValue && acknowledgementdate.HasValue)
                                {
                                    timeToAcknowledge = acknowledgementdate.Value - ackusercreateddate.Value;
                                }

                                pdata.Add(new GetPubDocListModel
                                {
                                    AddDoc_id = Convert.ToInt32(row["AddDoc_id"]),
                                    AuthorityName = row["AuthorityName"].ToString(),
                                    Publisher_name = row["publisher_name"].ToString(),
                                    DocTypeID = Convert.ToInt32(row["DocTypeID"]),
                                    AuthoritynameID = Convert.ToInt32(row["AuthoritynameID"]),
                                    AuthorityTypeID = Convert.ToInt32(row["AuthorityTypeID"]),
                                    NatureOf_Doc_id = Convert.ToInt32(row["NatureOf_Doc_id"]),
                                    Doc_CategoryID = Convert.ToInt32(row["Doc_CategoryID"]),
                                    Doc_SubCategoryID = Convert.ToInt32(row["Doc_SubCategoryID"]),
                                    AuthorityTypeName = row["AuthorityTypeName"].ToString(),
                                    Title_Doc = row["Title_Doc"].ToString(),
                                    Sub_title_doc = row["Sub_title_doc"].ToString(),
                                    Obj_Doc = row["Obj_Doc"].ToString(),
                                    addDoc_Status = row["addDoc_Status"].ToString(),
                                    NatureOf_Doc_Name = row["NatureOf_Doc_Name"].ToString(),
                                    DocTypeName = row["DocTypeName"].ToString(),
                                    Doc_CategoryName = row["Doc_CategoryName"].ToString(),
                                    Doc_SubCategoryName = row["Doc_SubCategoryName"].ToString(),
                                    Doc_Confidentiality = row["Doc_Confidentiality"].ToString(),
                                    Eff_Date = row["Eff_Date"].ToString(),
                                    addDoc_createdDate = row["addDoc_createdDate"].ToString(),
                                    Initial_creation_doc_date = row["Initial_creation_doc_date"].ToString(),
                                    Doc_internal_num = row["Doc_internal_num"].ToString(),
                                    Doc_Inter_ver_num = row["Doc_Inter_ver_num"].ToString(),
                                    Doc_Phy_Valut_Loc = row["Doc_Phy_Valut_Loc"].ToString(),
                                    Doc_process_Owner = row["Doc_process_Owner"].ToString(),
                                    Doc_Approver = row["Doc_Approver"].ToString(),
                                    Date_Doc_Revision = row["Date_Doc_Revision"].ToString(),
                                    Review_Frequency_Status = Convert.ToInt32(row["Review_Frequency_Status"]),
                                    Document_Id = row["Document_Id"].ToString(),
                                    VersionControlNo = row["VersionControlNo"].ToString(),
                                    Date_Doc_Approver = row["Date_Doc_Approver"].ToString(),
                                    companyname = row["companyname"].ToString(),
                                    locationname = row["locationname"].ToString(),
                                    ack_status = row["ack_status"] != DBNull.Value
    ? (string.Equals(row["ack_status"].ToString(), "true", StringComparison.OrdinalIgnoreCase)
        ? "Acknowledgement Pending"
        : string.Equals(row["ack_status"].ToString(), "false", StringComparison.OrdinalIgnoreCase)
            ? "Acknowledgement Not Required"
            : row["ack_status"].ToString())
    : "Reading in Progress",
                                    emailid = row["emailid"].ToString(),
                                    indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                    nameofuser = row["nameofuser"].ToString(),
                                    permissions_names = row["permissions_names"] != DBNull.Value ? row["permissions_names"].ToString() : string.Empty,
                                    mappingDate = ackusercreateddate,
                                    acknowledgementdate = acknowledgementdate,
                                    readcompleted = readcompleted,
                                    duedate = duedate,
                                    DaysLeft = daysLeft,
                                    TimeTakenToComplete = timeTakenToComplete,
                                    TimeToAcknowledge = timeToAcknowledge
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new InvalidOperationException("Data retrieval failed", ex);
            }

            return pdata;
        }


        // List of Published document Repository

        [Route("api/GovControlReportsController/PublishedDocRepository/{userId}")]
        [HttpGet]
        public IEnumerable<GetPubDocListModel> PublishedDocRepository(DateModel Datemodels, int userId)
        {
            var pdata = new List<GetPubDocListModel>();
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;


            try
            {
                // Validate and parse the dates
                if (string.IsNullOrEmpty(Datemodels.today) || string.IsNullOrEmpty(Datemodels.oneMonthAgo))
                {
                    throw new ArgumentException("Date values cannot be null or empty");
                }

                todayDate = DateTime.ParseExact(Datemodels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(Datemodels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");

            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }




            string baseQuery = @"select ad.Title_Doc,ad.ChangedOn,ad.Sub_title_doc,ad.Document_Id,ad.VersionControlNo,ad.Eff_Date,dt.DocTypeName,dc.Doc_CategoryName,dsc.Doc_SubCategoryName,ackdoc.acknowledged_date,ackdoc.ack_status,ackdoc.provideack_status,
dm.Department_Master_name,em.Entity_Master_Name, ulm.Unit_location_Master_name,tpub.firstname as publisherName,ad.addDoc_createdDate as publishingdate,ad.Doc_internal_num,ad.Time_period,ad.Doc_Confidentiality,nd.NatureOf_Doc_Name,au.AuthorityTypeName,
an.AuthorityName,ad.Doc_process_Owner,ad.Doc_Approver,ad.publisher_comments,ad.supportFilesCount,ad.indicative_reading_time,ackdoc.readComplete_date,(SELECT GROUP_CONCAT(dpr.publish_Name SEPARATOR ', ')
     FROM risk.doc_perm_rights dpr
     JOIN risk.doc_user_permission_mapping dupm 
       ON dpr.Doc_perm_rights_id = dupm.Doc_perm_rights_id
     WHERE dupm.Doc_User_Access_mapping_id = ackdoc.Doc_User_Access_mapping_id
       AND dupm.USR_ID = ackdoc.USR_ID) AS permissions_names from add_doc as ad
join doc_taskuseracknowledment_status as ackdoc on ackdoc.AddDoc_id = ad.AddDoc_id
join tbluser as tu on tu.USR_ID=ackdoc.USR_ID
join doctype_master as dt on dt.DocTypeID=ad.DocTypeID
join doccategory_master as dc on dc.Doc_CategoryID=ad.Doc_CategoryID
join docsubcategory_master as dsc on dsc.Doc_SubCategoryID=ad.Doc_SubCategoryID
join entity_master as em on em.Entity_Master_id=tu.Entity_Master_id
join unit_location_master as ulm on ulm.Unit_location_Master_id=tu.Unit_location_Master_id
join department_master as dm on dm.Department_Master_id=tu.Department_Master_id
join tbluser as tpub on tpub.USR_ID=ad.USR_ID 
join natureof_doc as nd on nd.NatureOf_Doc_id=ad.NatureOf_Doc_id
join authoritytype_master as au on au.AuthorityTypeID=ad.AuthorityTypeID
join authorityname_master as an on an.AuthoritynameID=ad.AuthoritynameID where ackdoc.USR_ID=@userId AND ";



            string DateQuery = Datemodels.datetype switch
            {
                "Publishing" => "DATE(ad.addDoc_createdDate)>=@startdate AND DATE(ad.addDoc_createdDate)<=@enddate",
                "Effective" => "DATE(ad.Eff_Date)>=@startdate AND DATE(ad.Eff_Date)<=@enddate",
                _ => throw new ArgumentException("Invalid Date Type")

            };

            string finalQuery = baseQuery + DateQuery;
            try
            {


                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@startdate", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@enddate", formattedToday);


                        using (var ad = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            ad.Fill(dt);
                            if (dt.Rows.Count > 0)
                            {

                                foreach (DataRow row in dt.Rows)
                                {
                                    try
                                    {
                                        // Safe conversion methods
                                        bool isParsed;
                                        int addDocId = 0;
                                        int supportFilesCount = 0;
                                        DateTime? acknowledgementdate = null;
                                        DateTime? readcompleted = null;

                                        isParsed = int.TryParse(row["Document_Id"].ToString(), out addDocId);
                                        if (!isParsed)
                                        {
                                            Console.WriteLine($"Error parsing Document_Id: {row["Document_Id"]}");
                                        }

                                        isParsed = int.TryParse(row["supportFilesCount"].ToString(), out supportFilesCount);
                                        if (!isParsed)
                                        {
                                            Console.WriteLine($"Error parsing supportFilesCount: {row["supportFilesCount"]}");
                                        }

                                        isParsed = DateTime.TryParse(row["acknowledged_date"].ToString(), out DateTime parsedAcknowledgementDate);
                                        if (isParsed)
                                        {
                                            acknowledgementdate = parsedAcknowledgementDate;
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Error parsing acknowledged_date: {row["acknowledged_date"]}");
                                        }

                                        isParsed = DateTime.TryParse(row["readComplete_date"].ToString(), out DateTime parsedReadCompletedDate);
                                        if (isParsed)
                                        {
                                            readcompleted = parsedReadCompletedDate;
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Error parsing readComplete_date: {row["readComplete_date"]}");
                                        }

                                        TimeSpan? timeTakenToComplete = null;
                                        if (acknowledgementdate.HasValue && readcompleted.HasValue)
                                        {
                                            timeTakenToComplete = readcompleted.Value - acknowledgementdate.Value;
                                        }
                                        string indicativeReadingTime = row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty;
                                        string timePeriod = row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty;

                                        string combinedTime = !string.IsNullOrEmpty(indicativeReadingTime) && !string.IsNullOrEmpty(timePeriod)
                                            ? $"{indicativeReadingTime} {timePeriod}"
                                            : (!string.IsNullOrEmpty(indicativeReadingTime) ? indicativeReadingTime : (!string.IsNullOrEmpty(timePeriod) ? timePeriod : "N/A"));

                                        pdata.Add(new GetPubDocListModel
                                        {
                                            AddDoc_id = addDocId,
                                            Document_Id = row["Document_Id"] != DBNull.Value ? row["Document_Id"].ToString() : string.Empty,
                                            doc_last_Edited_On = row["ChangedOn"] != DBNull.Value ? row["ChangedOn"].ToString() : string.Empty,
                                            VersionControlNo = row["VersionControlNo"] != DBNull.Value ? row["VersionControlNo"].ToString() : string.Empty,
                                            Title_Doc = row["Title_Doc"] != DBNull.Value ? row["Title_Doc"].ToString() : string.Empty,
                                            Sub_title_doc = row["Sub_title_doc"] != DBNull.Value ? row["Sub_title_doc"].ToString() : string.Empty,
                                            DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                            Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : string.Empty,
                                            Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : string.Empty,
                                            ack_status = row["ack_status"] != DBNull.Value
    ? (row["ack_status"].ToString().ToLower() == "true"
        ? "Acknowledgement is Pending"
        : row["ack_status"].ToString().ToLower() == "false"
            ? "Reading in Progress"
            : row["ack_status"].ToString())
    : "No",

                                            provideack_status = (row["provideack_status"] != DBNull.Value && Convert.ToBoolean(row["provideack_status"])) ? "Yes" : "No",
                                            acknowledgementdate = acknowledgementdate,
                                            // companyname = row["Department_Master_name"] != DBNull.Value ? row["Department_Master_name"].ToString() : string.Empty,
                                            companyname = row["Entity_Master_Name"] != DBNull.Value ? row["Entity_Master_Name"].ToString() : string.Empty,
                                            locationname = row["Unit_location_Master_name"] != DBNull.Value ? row["Unit_location_Master_name"].ToString() : string.Empty,//
                                            Eff_Date = row["Eff_Date"] != DBNull.Value ? row["Eff_Date"].ToString() : string.Empty,
                                            Publisher_name = row["publisherName"] != DBNull.Value ? row["publisherName"].ToString() : string.Empty,
                                            addDoc_createdDate = row["publishingdate"] != DBNull.Value ? row["publishingdate"].ToString() : string.Empty,
                                            Doc_internal_num = row["doc_internal_num"] != DBNull.Value ? row["doc_internal_num"].ToString() : string.Empty,
                                            Doc_Confidentiality = row["Doc_Confidentiality"] != DBNull.Value ? row["Doc_Confidentiality"].ToString() : string.Empty,
                                            NatureOf_Doc_Name = row["NatureOf_Doc_Name"] != DBNull.Value ? row["NatureOf_Doc_Name"].ToString() : string.Empty,
                                            AuthorityName = row["AuthorityName"] != DBNull.Value ? row["AuthorityName"].ToString() : string.Empty,
                                            AuthorityTypeName = row["AuthorityTypeName"] != DBNull.Value ? row["AuthorityTypeName"].ToString() : string.Empty,
                                            Doc_process_Owner = row["Doc_process_Owner"] != DBNull.Value ? row["Doc_process_Owner"].ToString() : string.Empty,
                                            Doc_Approver = row["Doc_Approver"] != DBNull.Value ? row["Doc_Approver"].ToString() : string.Empty,
                                            supportFilesCount = supportFilesCount,
                                            publisher_comments = row["publisher_comments"] != DBNull.Value ? row["publisher_comments"].ToString() : string.Empty,
                                            //  indicative_reading_time = row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty,
                                            //Time_period  = row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty,
                                            //indicative_reading_time = indicativeReadingTime,
                                            indicative_reading_time = (row["indicative_reading_time"] != DBNull.Value ? row["indicative_reading_time"].ToString() : string.Empty) + (row["Time_period"] != DBNull.Value ? row["Time_period"].ToString() : string.Empty),
                                            Time_period = timePeriod,
                                            combinedReadingTime = combinedTime,
                                            TimeTakenToComplete = timeTakenToComplete,
                                            permissions_names = row["permissions_names"] != DBNull.Value ? row["permissions_names"].ToString() : string.Empty,
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error processing row: {ex.Message}");
                                        // Optionally log or rethrow the exception
                                    }
                                }



                            }

                        }


                    }



                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }






            return pdata;


        }



        //GCA REPORTS
        //Getting Assessment Template List

        [Route("api/GovControlReportsController/GetAllAssesTemplates")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> GetAllAssesTemplates([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GettingAssessmentTemplateModel>();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            // Define the base query
            string baseQuery = @"
        SELECT ab.ass_template_id, ab.total_questions, ab.total_estimated_time, ab.assessment_name, ab.verson_no,ab.updated_user_id,
               ab.created_date, ab.updated_date, us1.firstname AS user_firstname, 
               us2.firstname AS updated_user_firstname, ab.assessment_description, 
               ab.status, ab.show_hint, ab.keywords, ab.assessment_builder_id, 
               ab.show_explaination, tn.Type_Name, sn.SubType_Name, cn.Competency_Name 
        FROM risk.assessment_builder_versions ab 
        LEFT JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id 
        LEFT JOIN risk.type tn ON tn.Type_id = ab.Type_id 
        LEFT JOIN risk.tbluser us1 ON us1.USR_ID = ab.user_id 
        LEFT JOIN risk.tbluser us2 ON us2.USR_ID = ab.updated_user_id 
        LEFT JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id 
        WHERE ab.status = 'Active' AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Creation" => "CAST(ab.created_date AS DATE) <= @Today AND CAST(ab.created_date AS DATE) >= @MonthAgo",
                "Edited" => "CAST(ab.updated_date AS DATE) <= @Today AND CAST(ab.updated_date AS DATE) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = baseQuery + dateCondition;

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {

                                    pdata.Add(new GettingAssessmentTemplateModel
                                    {
                                        Type_Name = row["Type_Name"] != DBNull.Value ? row["Type_Name"].ToString() : null,
                                        assessment_builder_id = row["assessment_builder_id"] != DBNull.Value ? Convert.ToInt32(row["assessment_builder_id"]) : (int?)null,
                                        ass_template_id = row["ass_template_id"] != DBNull.Value ? row["ass_template_id"].ToString() : null,
                                        assessment_name = row["assessment_name"] != DBNull.Value ? row["assessment_name"].ToString() : null,
                                        assessment_description = row["assessment_description"] != DBNull.Value ? row["assessment_description"].ToString() : null,
                                        SubType_Name = row["SubType_Name"] != DBNull.Value ? row["SubType_Name"].ToString() : null,
                                        Competency_Name = row["Competency_Name"] != DBNull.Value ? row["Competency_Name"].ToString() : null,
                                        created_date = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                        updated_date = row["updated_date"] != DBNull.Value ? Convert.ToDateTime(row["updated_date"]) : (DateTime?)null,
                                        keywords = row["keywords"] != DBNull.Value ? row["keywords"].ToString() : null,
                                        status = row["status"] != DBNull.Value ? row["status"].ToString() : null,
                                        show_explaination = row["show_explaination"] != DBNull.Value && row["show_explaination"].ToString() == "1" ? "Yes" : "No",
                                        show_hint = row["show_hint"] != DBNull.Value && row["show_hint"].ToString() == "1" ? "Yes" : "No",
                                        firstname = row["user_firstname"] != DBNull.Value ? row["user_firstname"].ToString() : null,
                                        UpdateUsername = row["updated_user_firstname"] != DBNull.Value ? row["updated_user_firstname"].ToString() : null,
                                        total_questions = row["total_questions"] != DBNull.Value ? row["total_questions"].ToString() : null,
                                        total_estimated_time = row["total_estimated_time"] != DBNull.Value ? row["total_estimated_time"].ToString() : null,
                                        verson_no = row["verson_no"] != DBNull.Value ? Convert.ToInt32(row["verson_no"]) : (int?)null,
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }


        //Get Disabled Assessment Templates


        [Route("api/GovControlReportsController/GetAllDisabledAssesTemplates")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> GetAllDisabledAssesTemplates([FromQuery] DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GettingAssessmentTemplateModel>();

            try
            {
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string baseQuery = @"
        SELECT ab.ass_template_id, ab.total_questions, ab.total_estimated_time, ab.reason_for_disable,ab.verson_no,
               ab.assessment_name, ab.created_date, ab.updated_date,ab.disabled_date, 
               us1.firstname AS user_firstname, us2.firstname AS updated_user_firstname,us3.firstname AS disableby_user_firstname,
               ab.assessment_description, ab.status, ab.show_hint, 
               ab.keywords, ab.assessment_builder_id, ab.show_explaination,
               tn.Type_Name, sn.SubType_Name, cn.Competency_Name
        FROM risk.assessment_builder_versions ab
        LEFT JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
        LEFT JOIN risk.type tn ON tn.Type_id = ab.Type_id
        LEFT JOIN risk.tbluser us1 ON us1.USR_ID = ab.user_id
        LEFT JOIN risk.tbluser us2 ON us2.USR_ID = ab.updated_user_id
LEFT JOIN risk.tbluser us3 ON us3.USR_ID = ab.disableby
        LEFT JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
        WHERE ab.status = 'InActive' AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Creation" => "CAST(ab.created_date AS DATE) <= @Today AND CAST(ab.created_date AS DATE) >= @MonthAgo",
                "Edited" => "CAST(ab.updated_date AS DATE) <= @Today AND CAST(ab.updated_date AS DATE) >= @MonthAgo",
                "Disabled" => "CAST(ab.disabled_date AS DATE) <= @Today AND CAST(ab.disabled_date AS DATE) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = baseQuery + dateCondition;

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GettingAssessmentTemplateModel
                                    {
                                        Type_Name = row["Type_Name"] != DBNull.Value ? row["Type_Name"].ToString() : null,
                                        assessment_builder_id = row["assessment_builder_id"] != DBNull.Value ? Convert.ToInt32(row["assessment_builder_id"]) : (int?)null,
                                        ass_template_id = row["ass_template_id"] != DBNull.Value ? row["ass_template_id"].ToString() : null,
                                        assessment_name = row["assessment_name"] != DBNull.Value ? row["assessment_name"].ToString() : null,
                                        assessment_description = row["assessment_description"] != DBNull.Value ? row["assessment_description"].ToString() : null,
                                        SubType_Name = row["SubType_Name"] != DBNull.Value ? row["SubType_Name"].ToString() : null,
                                        Competency_Name = row["Competency_Name"] != DBNull.Value ? row["Competency_Name"].ToString() : null,
                                        created_date = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                        updated_date = row["updated_date"] != DBNull.Value ? Convert.ToDateTime(row["updated_date"]) : (DateTime?)null,
                                        disabled_date = row["disabled_date"] != DBNull.Value ? Convert.ToDateTime(row["disabled_date"]) : (DateTime?)null,
                                        keywords = row["keywords"] != DBNull.Value ? row["keywords"].ToString() : null,
                                        status = row["status"] != DBNull.Value ? row["status"].ToString() : null,//disableby_user_firstname
                                        disableby = row["disableby_user_firstname"] != DBNull.Value ? row["disableby_user_firstname"].ToString() : null,
                                        show_explaination = row["show_explaination"] != DBNull.Value && row["show_explaination"].ToString() == "1" ? "Yes" : "No",
                                        show_hint = row["show_hint"] != DBNull.Value && row["show_hint"].ToString() == "1" ? "Yes" : "No",
                                        firstname = row["user_firstname"] != DBNull.Value ? row["user_firstname"].ToString() : null,
                                        UpdateUsername = row["updated_user_firstname"] != DBNull.Value ? row["updated_user_firstname"].ToString() : null,
                                        total_questions = row["total_questions"] != DBNull.Value ? row["total_questions"].ToString() : null,
                                        total_estimated_time = row["total_estimated_time"] != DBNull.Value ? row["total_estimated_time"].ToString() : null,
                                        reason_for_disable = row["reason_for_disable"] != DBNull.Value ? row["reason_for_disable"].ToString() : null,
                                        verson_no = row["verson_no"] != DBNull.Value ? Convert.ToInt32(row["verson_no"]) : (int?)null,

                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching disabled assessment templates", ex);
            }

            return pdata;
        }


        //Get Provide Access Template

        [Route("api/GovControlReportsController/GetAllAssesTemplatesProvideAccess/{today}/{monthago}/{userid}")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> GetAllAssesTemplatesProvideAccess(string today, string monthago, int userid)

        {

            DateTime todayDate = DateTime.ParseExact(today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime monthAgoDate = DateTime.ParseExact(monthago, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string formattedToday = todayDate.ToString("yyyy-MM-dd");
            string formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");


            var pdata = new List<GettingAssessmentTemplateModel>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT 
    ab.ass_template_id,
    pa.verson_no,
    pa.UnitLocationMasterID,
    pa.EntityMasterID,
ul.Unit_location_Master_name,
em.Entity_Master_Name,
 GROUP_CONCAT(DISTINCT CONCAT(us3.firstname) SEPARATOR ', ') AS user_names,
    COUNT(DISTINCT pa.UserID) AS user_count,
    MAX(ab.total_questions) AS total_questions,
    MAX(ab.total_estimated_time) AS total_estimated_time,
    MAX(ab.reason_for_disable) AS reason_for_disable,
    MAX(ab.assessment_name) AS assessment_name,
    MAX(ab.created_date) AS created_date,
    MAX(ab.updated_date) AS updated_date,
    MAX(us1.firstname) AS user_firstname,
    MAX(us2.firstname) AS updated_user_firstname,
    MAX(ab.assessment_description) AS assessment_description,
    MAX(ab.status) AS status,
    MAX(ab.show_hint) AS show_hint,
    MAX(ab.keywords) AS keywords,
    MAX(ab.assessment_builder_id) AS assessment_builder_id,
    MAX(ab.show_explaination) AS show_explaination,
    MAX(tn.Type_Name) AS Type_Name,
    MAX(sn.SubType_Name) AS SubType_Name,
    MAX(cn.Competency_Name) AS Competency_Name

FROM 
    risk.assessment_builder_versions ab
JOIN 
    risk.assement_provideacess pa ON pa.AssessementTemplateID = ab.ass_template_id
        AND pa.verson_no = ab.verson_no 
Left JOIN risk.entity_master em ON em.Entity_Master_id=pa.EntityMasterID
Left Join risk.unit_location_master ul ON ul.Unit_location_Master_id=pa.UnitLocationMasterID
LEFT JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
LEFT JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
LEFT JOIN 
    risk.tbluser us1 ON us1.USR_ID = ab.user_id 
LEFT JOIN risk.tbluser us3 ON us3.USR_ID = pa.UserID
LEFT JOIN 
    risk.tbluser us2 ON us2.USR_ID = ab.updated_user_id
LEFT JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id 
  WHERE 
             pa.Status = 'Active'
            AND ab.user_id = @userid
            AND DATE(ab.created_date) <= @formattedToday
            AND DATE(ab.created_date) >= @formattedMonthAgo
        GROUP BY 
            ab.ass_template_id, pa.verson_no, pa.UnitLocationMasterID, pa.EntityMasterID;", con))
                {
                    // Add parameters to the command
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@formattedToday", formattedToday);
                    cmd.Parameters.AddWithValue("@formattedMonthAgo", formattedMonthAgo);
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            for (var i = 0; i < dt.Rows.Count; i++)
                            {
                                pdata.Add(new GettingAssessmentTemplateModel
                                {
                                    Type_Name = dt.Rows[i]["Type_Name"] != DBNull.Value ? dt.Rows[i]["Type_Name"].ToString() : null,

                                    assessment_builder_id = dt.Rows[i]["assessment_builder_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]) : (int?)null,
                                    ass_template_id = dt.Rows[i]["ass_template_id"] != DBNull.Value ? dt.Rows[i]["ass_template_id"].ToString() : null,
                                    assessment_name = dt.Rows[i]["assessment_name"] != DBNull.Value ? dt.Rows[i]["assessment_name"].ToString() : null,
                                    assessment_description = dt.Rows[i]["assessment_description"] != DBNull.Value ? dt.Rows[i]["assessment_description"].ToString() : null,
                                    SubType_Name = dt.Rows[i]["SubType_Name"] != DBNull.Value ? dt.Rows[i]["SubType_Name"].ToString() : null,
                                    Competency_Name = dt.Rows[i]["Competency_Name"] != DBNull.Value ? dt.Rows[i]["Competency_Name"].ToString() : null,
                                    created_date = dt.Rows[i]["created_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["created_date"]) : (DateTime?)null,
                                    updated_date = dt.Rows[i]["updated_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["updated_date"]) : (DateTime?)null,
                                    keywords = dt.Rows[i]["keywords"] != DBNull.Value ? dt.Rows[i]["keywords"].ToString() : null,
                                    status = dt.Rows[i]["status"] != DBNull.Value ? dt.Rows[i]["status"].ToString() : null,
                                    show_explaination = dt.Rows[i]["show_explaination"] != DBNull.Value && dt.Rows[i]["show_explaination"].ToString() == "1" ? "Yes" : "No",
                                    show_hint = dt.Rows[i]["show_hint"] != DBNull.Value && dt.Rows[i]["show_hint"].ToString() == "1" ? "Yes" : "No",

                                    firstname = dt.Rows[i]["user_firstname"] != DBNull.Value ? dt.Rows[i]["user_firstname"].ToString() : null,
                                    UpdateUsername = dt.Rows[i]["updated_user_firstname"] != DBNull.Value ? dt.Rows[i]["updated_user_firstname"].ToString() : null,

                                    total_questions = dt.Rows[i]["total_questions"] != DBNull.Value ? dt.Rows[i]["total_questions"].ToString() + " Questions" : null,
                                    total_estimated_time = dt.Rows[i]["total_estimated_time"] != DBNull.Value ? dt.Rows[i]["total_estimated_time"].ToString() + " Minutes" : null,
                                    reason_for_disable = dt.Rows[i]["reason_for_disable"] != DBNull.Value ? dt.Rows[i]["reason_for_disable"].ToString() : null,

                                    CompanyLocation = dt.Rows[i]["Unit_location_Master_name"] != DBNull.Value ? dt.Rows[i]["Unit_location_Master_name"].ToString() : null,
                                    CompanyName = dt.Rows[i]["Entity_Master_Name"] != DBNull.Value ? dt.Rows[i]["Entity_Master_Name"].ToString() : null,
                                    usersCount = dt.Rows[i]["user_count"] != DBNull.Value ? dt.Rows[i]["user_count"].ToString() : null,
                                    verson_no = dt.Rows[i]["verson_no"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["verson_no"]) : (int?)null,
                                    user_names = dt.Rows[i]["user_names"] != DBNull.Value ? dt.Rows[i]["user_names"].ToString() : null,


                                });
                            }
                        }
                    }
                }
            }

            return pdata;
        }

        //provide accesss second gris

        [Route("api/GovControlReportsController/GetProvideAccessAssessorsList/{today}/{monthago}/{tempid}/{verson_no}/{userid}")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> GetProvideAccessAssessorsList(string today, string monthago, string tempid, string verson_no, int userid)

        {

            DateTime todayDate = DateTime.ParseExact(today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime monthAgoDate = DateTime.ParseExact(monthago, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string formattedToday = todayDate.ToString("yyyy-MM-dd");
            string formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");

            var pdata = new List<GettingAssessmentTemplateModel>();

            using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(@"
              SELECT 
    ab.ass_template_id,
    pa.verson_no,
    pa.UnitLocationMasterID,
    pa.EntityMasterID,
ul.Unit_location_Master_name,
em.Entity_Master_Name,
    MAX(pa.UserID) AS Users,
 MAX(us3.firstname) AS AssessorName,
 MAX(us3.emailid) AS Email,
    MAX(ab.total_questions) AS total_questions,
    MAX(ab.total_estimated_time) AS total_estimated_time,
    MAX(ab.reason_for_disable) AS reason_for_disable,
    MAX(ab.assessment_name) AS assessment_name,
    MAX(ab.created_date) AS created_date,
    MAX(ab.updated_date) AS updated_date,
    MAX(us1.firstname) AS user_firstname,
    MAX(us2.firstname) AS updated_user_firstname,
    MAX(ab.assessment_description) AS assessment_description,
    MAX(ab.status) AS status,
    MAX(ab.show_hint) AS show_hint,
    MAX(ab.keywords) AS keywords,
    MAX(ab.assessment_builder_id) AS assessment_builder_id,
    MAX(ab.show_explaination) AS show_explaination,
    MAX(tn.Type_Name) AS Type_Name,
    MAX(sn.SubType_Name) AS SubType_Name,
 MAX(pa.CreatedDate) AS CreatedDate,  
MAX(permission.AssUserPermissionName) As AssUserPermissionName,
    MAX(cn.Competency_Name) AS Competency_Name
FROM 
    risk.assessment_builder_versions ab
JOIN 
    risk.assement_provideacess pa ON pa.AssessementTemplateID = ab.ass_template_id
        AND pa.verson_no = ab.verson_no 
Left JOIN risk.entity_master em ON em.Entity_Master_id=pa.EntityMasterID
Left Join risk.unit_location_master ul ON ul.Unit_location_Master_id=pa.UnitLocationMasterID
LEFT JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
LEFT JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
LEFT JOIN 
    risk.tbluser us1 ON us1.USR_ID = ab.user_id
LEFT JOIN 
    risk.tbluser us2 ON us2.USR_ID = ab.updated_user_id
LEFT JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id 
    left join risk.tbluser us3 ON us3.USR_ID=pa.UserID
LEFT JOIN risk.ass_user_permissionlist permission ON permission.Ass_User_PermissionListid = pa.Access_Permissions

WHERE 
    pa.Status = 'Active'
And DATE(ab.created_date) <= '" + formattedToday + "' && DATE(ab.created_date) >= '" + formattedMonthAgo + "' and ab.ass_template_id='" + tempid + "' and pa.verson_no='" + verson_no + "' GROUP BY ab.ass_template_id,pa.verson_no, pa.UnitLocationMasterID, pa.EntityMasterID, pa.UserID;", con)
                {

                })


                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        for (var i = 0; i < dt.Rows.Count; i++)
                        {
                            pdata.Add(new GettingAssessmentTemplateModel
                            {
                                Type_Name = dt.Rows[i]["Type_Name"] != DBNull.Value ? dt.Rows[i]["Type_Name"].ToString() : null,
                                //assessment_builder_id = dt.Rows[i]["assessment_builder_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["assessment_builder_id"]) : (int?)null,
                                ass_template_id = dt.Rows[i]["ass_template_id"] != DBNull.Value ? dt.Rows[i]["ass_template_id"].ToString() : null,
                                assessment_name = dt.Rows[i]["assessment_name"] != DBNull.Value ? dt.Rows[i]["assessment_name"].ToString() : null,
                                assessment_description = dt.Rows[i]["assessment_description"] != DBNull.Value ? dt.Rows[i]["assessment_description"].ToString() : null,
                                SubType_Name = dt.Rows[i]["SubType_Name"] != DBNull.Value ? dt.Rows[i]["SubType_Name"].ToString() : null,
                                Competency_Name = dt.Rows[i]["Competency_Name"] != DBNull.Value ? dt.Rows[i]["Competency_Name"].ToString() : null,
                                created_date = dt.Rows[i]["created_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["created_date"]) : (DateTime?)null,
                                updated_date = dt.Rows[i]["updated_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["updated_date"]) : (DateTime?)null,
                                keywords = dt.Rows[i]["keywords"] != DBNull.Value ? dt.Rows[i]["keywords"].ToString() : null,
                                status = dt.Rows[i]["status"] != DBNull.Value ? dt.Rows[i]["status"].ToString() : null,
                                show_explaination = dt.Rows[i]["show_explaination"] != DBNull.Value && dt.Rows[i]["show_explaination"].ToString() == "1" ? "Yes" : "No",
                                show_hint = dt.Rows[i]["show_hint"] != DBNull.Value && dt.Rows[i]["show_hint"].ToString() == "1" ? "Yes" : "No",

                                firstname = dt.Rows[i]["user_firstname"] != DBNull.Value ? dt.Rows[i]["user_firstname"].ToString() : null,
                                UpdateUsername = dt.Rows[i]["updated_user_firstname"] != DBNull.Value ? dt.Rows[i]["updated_user_firstname"].ToString() : null,
                                total_questions = dt.Rows[i]["total_questions"] != DBNull.Value ? dt.Rows[i]["total_questions"].ToString() + " Questions" : null,
                                total_estimated_time = dt.Rows[i]["total_estimated_time"] != DBNull.Value ? dt.Rows[i]["total_estimated_time"].ToString() + " Minutes" : null,
                                reason_for_disable = dt.Rows[i]["reason_for_disable"] != DBNull.Value ? dt.Rows[i]["reason_for_disable"].ToString() : null,

                                CompanyLocation = dt.Rows[i]["Unit_location_Master_name"] != DBNull.Value ? dt.Rows[i]["Unit_location_Master_name"].ToString() : null,
                                CompanyName = dt.Rows[i]["Entity_Master_Name"] != DBNull.Value ? dt.Rows[i]["Entity_Master_Name"].ToString() : null,
                                Users = dt.Rows[i]["Users"] != DBNull.Value ? dt.Rows[i]["Users"].ToString() : null,
                                userName = dt.Rows[i]["AssessorName"] != DBNull.Value ? dt.Rows[i]["AssessorName"].ToString() : null,
                                email = dt.Rows[i]["Email"] != DBNull.Value ? dt.Rows[i]["Email"].ToString() : null,
                                verson_no = dt.Rows[i]["verson_no"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["verson_no"]) : (int?)null,//
                                CreatedDate = dt.Rows[i]["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["CreatedDate"]) : (DateTime?)null, // mapped date
                                AssUserPermissionName = dt.Rows[i]["AssUserPermissionName"] != DBNull.Value ? dt.Rows[i]["AssUserPermissionName"].ToString() : null,


                            });
                        }
                    }
                }

            }

            return pdata;
        }





        //Getting Assessment Scheduled List and Acknowledment requested

        [Route("api/GovControlReportsController/getAssScheduledList")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> getAssScheduledList([FromQuery] DateModel DateModels)
        {

            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GettingAssessmentTemplateModel>();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Format dates including milliseconds
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            // Define the base query with a placeholder for the date condition
            string baseQuery = @"
WITH AssessmentCount AS (
    SELECT 
        sa.uq_ass_schid,
        COUNT(sa.uq_ass_schid) AS total_mapped_users,
        MAX(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
        MAX(sa.Date_Of_Request) AS Date_Of_Request,
        MAX(sa.frequency_period) AS frequency_period,
        MAX(sa.Duration_of_Assessment) AS Duration_of_Assessment,
        MAX(sa.repeatEndDate) AS repeatEndDate,
        MAX(sa.startDate) AS startDate,
        MAX(sa.endDate) AS endDate,
        MAX(sa.ass_template_id) AS ass_template_id,
        MAX(sa.AssessmentStatus) AS AssessmentStatus,
        MAX(sa.pagetype) AS pagetype,
        MAX(sa.created_date) AS scheduled_date,
        MAX(sa.status) AS scheduled_status,
        MAX(ab.Competency_id) AS Competency_id,
        MAX(ab.Type_id) AS Type_id,
        MAX(ab.assessment_name) AS assessment_name,
        MAX(ab.total_questions) AS total_questions,
        MAX(ab.total_estimated_time) AS total_estimated_time,
        MAX(tu.firstname) AS assessor_name,
 MAX(tu1.firstname) AS createdname,
        MAX(tn.Type_Name) AS Type_Name,
        MAX(sn.SubType_Name) AS SubType_Name,
        MAX(cn.Competency_Name) AS Competency_Name,
        MAX(dt.DocTypeName) AS DocTypeName,
        MAX(dc.Doc_CategoryName) AS Doc_CategoryName,
        MAX(dsc.Doc_SubCategoryName) AS Doc_SubCategoryName,
       MAx(sa.verson_no)  As  verson_no
    FROM 
        schedule_assessment sa
    JOIN 
        assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id
 AND sa.verson_no = ab.verson_no 
    JOIN 
        tbluser tu ON tu.USR_ID = sa.userid
 JOIN 
        tbluser tu1 ON tu1.USR_ID = sa.login_userid
    JOIN 
        risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
    JOIN 
        risk.sub_type sn ON sn.SubType_id = ab.SubType_id
    JOIN 
        risk.type tn ON tn.Type_id = ab.Type_id
    JOIN 
        risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
    JOIN 
        risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
    JOIN 
        risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
    WHERE 
        sa.status = 'Active' AND sa.AssessmentStatus != 'Assessment Completed' AND {0}
    GROUP BY 
        sa.uq_ass_schid 
)
SELECT 
    ua.Schedule_Assessment_id,
    ua.Date_Of_Request,
    ua.frequency_period,
    ua.Duration_of_Assessment,
    ua.repeatEndDate,
    ua.startDate,
    ua.endDate,
    ua.ass_template_id AS templateid,
    ua.AssessmentStatus,
    ua.uq_ass_schid,
    ua.total_mapped_users,
    ua.pagetype,
    ua.createdname,
    ua.scheduled_date AS scheduled_created_date,
    ua.scheduled_status,
    ua.Competency_id,
    ua.Type_id,
    ua.assessment_name,
    ua.total_questions,
    ua.total_estimated_time,
    ua.assessor_name,
    ua.Type_Name,
    ua.SubType_Name,
    ua.Competency_Name,
    ua.DocTypeName,
    ua.Doc_CategoryName,
    ua.Doc_SubCategoryName,
    ua.verson_no
FROM 
    AssessmentCount ua;";

            // Determine the date condition based on the datetype
            string dateCondition = DateModels.datetype switch
            {
                "Due" => " DATE(sa.endDate) <= @Today AND DATE(sa.endDate) >= @MonthAgo",
                "Acknowledgemet" => " DATE(sa.created_date) <= @Today AND DATE(sa.created_date) >= @MonthAgo",
                "Request" => " DATE(sa.Date_Of_Request) <= @Today AND DATE(sa.Date_Of_Request) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            // Combine the base query with the date condition
            string finalQuery = string.Format(baseQuery, dateCondition);


            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GettingAssessmentTemplateModel
                                    {
                                        //Scheduled Assessmen Fields
                                        Schedule_Assessment_id = (int)(row["Schedule_Assessment_id"] != DBNull.Value ? Convert.ToInt32(row["Schedule_Assessment_id"]) : (int?)null),
                                        Date_Of_Request = (DateTime)(row["Date_Of_Request"] != DBNull.Value ? Convert.ToDateTime(row["Date_Of_Request"]) : (DateTime?)null),
                                        frequency_period = row["frequency_period"] != DBNull.Value ? row["frequency_period"].ToString() : null,
                                        Duration_of_Assessment = row["Duration_of_Assessment"] != DBNull.Value ? row["Duration_of_Assessment"].ToString() + " Seconds" : null,
                                        //repeatEndDate = (DateTime)(row["repeatEndDate"] != DBNull.Value ? Convert.ToDateTime(row["repeatEndDate"]) : (DateTime?)null),
                                        startDate = (DateTime)(row["startDate"] != DBNull.Value ? Convert.ToDateTime(row["startDate"]) : (DateTime?)null),
                                        endDate = (DateTime)(row["endDate"] != DBNull.Value ? Convert.ToDateTime(row["endDate"]) : (DateTime?)null),
                                        ass_template_id = row["templateid"] != DBNull.Value ? row["templateid"].ToString() : null,
                                        AssessmentStatus = row["AssessmentStatus"] != DBNull.Value ? row["AssessmentStatus"].ToString() : null,
                                        uq_ass_schid = row["uq_ass_schid"] != DBNull.Value ? row["uq_ass_schid"].ToString() : null,
                                        pagetype = row["pagetype"] != DBNull.Value ? row["pagetype"].ToString() : null,
                                        scheduled_created_date = (DateTime)(row["scheduled_created_date"] != DBNull.Value ? Convert.ToDateTime(row["scheduled_created_date"]) : (DateTime?)null),
                                        scheduled_status = row["scheduled_status"] != DBNull.Value ? row["scheduled_status"].ToString() : null,
                                        //AssessorName = row["assessor_name"] != DBNull.Value ? row["assessor_name"].ToString() : null,
                                        AssessorName = row["createdname"] != DBNull.Value ? row["createdname"].ToString() : null,
                                        total_mapped_users = (int)(row["total_mapped_users"] != DBNull.Value ? Convert.ToInt32(row["total_mapped_users"]) : (int?)null),

                                        //assessment builder
                                        assessment_name = row["assessment_name"] != DBNull.Value ? row["assessment_name"].ToString() : null,
                                        total_questions = row["total_questions"] != DBNull.Value ? row["total_questions"].ToString() + " Questions" : null,
                                        total_estimated_time = row["total_estimated_time"] != DBNull.Value ? row["total_estimated_time"].ToString() + " Minutes" : null,
                                        Type_Name = row["Type_Name"] != DBNull.Value ? row["Type_Name"].ToString() : null,
                                        SubType_Name = row["SubType_Name"] != DBNull.Value ? row["SubType_Name"].ToString() : null,
                                        Competency_Name = row["Competency_Name"] != DBNull.Value ? row["Competency_Name"].ToString() : null,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : null,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : null,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : null,
                                        verson_no = row["verson_no"] != DBNull.Value ? Convert.ToInt32(row["verson_no"]) : (int?)null,





                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }


        //Getting Assessment Scheduled List of mapped users


        public List<int> GetAssessmentIds(string id)
        {
            var assessmentids = new List<int>();
            string assessquery = "SELECT Schedule_Assessment_id FROM schedule_assessment WHERE uq_ass_schid = @UqAssSchid";

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(assessquery, con))
                    {
                        // Add parameter to the command to prevent SQL injection
                        cmd.Parameters.AddWithValue("@UqAssSchid", id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Assuming Schedule_Assessment_id is an int
                                int scheduleAssessmentId = reader.GetInt32(0);
                                assessmentids.Add(scheduleAssessmentId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log it and/or rethrow it)
                throw new ApplicationException("An error occurred while fetching assessment IDs", ex);
            }

            return assessmentids;
        }

        //Mapped user list in scheduled assessment

        [Route("api/GovControlReportsController/getAssScheduledListmappedusers/{id}/{verson_no}")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> GetScheduledAssessments(string id, string verson_no, DateModel DateModels)
        {
            var pdata = new List<GettingAssessmentTemplateModel>();
            var assessmentids = GetAssessmentIds(id); // Assuming GetAssessmentIds method exists

            if (assessmentids.Count == 0)
            {
                return pdata; // No assessments to query
            }

            // Convert the list of IDs to a comma-separated string
            string ids = string.Join(",", assessmentids);

            string baseQuery = @"
SELECT sa.Schedule_Assessment_id,sa.acknowledgemet_date,sa.verson_no, sa.Date_Of_Request,ulm.Unit_location_Master_name as locationname,tm.Department_Master_id,dm.Department_Master_name as departmentname,sa.frequency_period,tm.emailid as emailid,tm.Entity_Master_id,tm.Unit_location_Master_id,em.Entity_Master_Name as companyname,
       sa.Duration_of_Assessment, sa.repeatEndDate, sa.startDate, sa.endDate, sa.ass_template_id as templateid, sa.AssessmentStatus, sa.uq_ass_schid, sa.pagetype, sa.created_date as scheduled_date, sa.status as scheduled_status,
       ab.Competency_id, ab.verson_no , ab.Type_id, ab.assessment_name, ab.total_questions, ab.total_estimated_time, tu.firstname as assessor_name,  tm1.firstname as riskassessor, tn.Type_Name, sn.SubType_Name,
       cn.Competency_Name, dt.DocTypeName, dc.Doc_CategoryName, dsc.Doc_SubCategoryName,tm.firstname as mappedusername
FROM schedule_assessment sa
JOIN assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id  AND sa.verson_no = ab.verson_no 
JOIN tbluser tu ON tu.USR_ID = sa.userid
JOIN tbluser tm ON tm.USR_ID = sa.mapped_user
JOIN tbluser tm1 ON tm1.USR_ID = sa.login_userid
join department_master dm on dm.Department_Master_id=tm.Department_Master_id
JOIN entity_master em ON em.Entity_Master_id = tm.Entity_Master_id
join unit_location_master ulm on ulm.Unit_location_Master_id=tm.Unit_location_Master_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
JOIN risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
JOIN risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
WHERE sa.status = 'Active' AND sa.Schedule_Assessment_id IN (" + ids + ")AND sa.verson_no = '" + verson_no + @"'";

            try//
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(baseQuery, con))
                    {
                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GettingAssessmentTemplateModel
                                    {
                                        Schedule_Assessment_id = (int)(row["Schedule_Assessment_id"] != DBNull.Value ? Convert.ToInt32(row["Schedule_Assessment_id"]) : (int?)null),
                                        Date_Of_Request = (DateTime)(row["Date_Of_Request"] != DBNull.Value ? Convert.ToDateTime(row["Date_Of_Request"]) : (DateTime?)null),
                                        frequency_period = row["frequency_period"] != DBNull.Value ? row["frequency_period"].ToString() : null,
                                        Duration_of_Assessment = row["Duration_of_Assessment"] != DBNull.Value ? row["Duration_of_Assessment"].ToString() : null,
                                        //repeatEndDate = (DateTime)(row["repeatEndDate"] != DBNull.Value ? Convert.ToDateTime(row["repeatEndDate"]) : (DateTime?)null),
                                        startDate = (DateTime)(row["startDate"] != DBNull.Value ? Convert.ToDateTime(row["startDate"]) : (DateTime?)null),
                                        endDate = (DateTime)(row["endDate"] != DBNull.Value ? Convert.ToDateTime(row["endDate"]) : (DateTime?)null),
                                        ass_template_id = row["templateid"] != DBNull.Value ? row["templateid"].ToString() : null,
                                        AssessmentStatus = row["AssessmentStatus"] != DBNull.Value ? row["AssessmentStatus"].ToString() : null,
                                        uq_ass_schid = row["uq_ass_schid"] != DBNull.Value ? row["uq_ass_schid"].ToString() : null,
                                        pagetype = row["pagetype"] != DBNull.Value ? row["pagetype"].ToString() : null,
                                        scheduled_created_date = (DateTime)(row["scheduled_date"] != DBNull.Value ? Convert.ToDateTime(row["scheduled_date"]) : (DateTime?)null),
                                        scheduled_status = row["scheduled_status"] != DBNull.Value ? row["scheduled_status"].ToString() : null,
                                        AssessorName = row["riskassessor"] != DBNull.Value ? row["riskassessor"].ToString() : null,
                                        mapped_user_name = row["mappedusername"] != DBNull.Value ? row["mappedusername"].ToString() : null,
                                        //total_mapped_users = (int)(row["total_mapped_users"] != DBNull.Value ? Convert.ToInt32(row["total_mapped_users"]) : (int?)null),
                                        emailid = row["emailid"] != DBNull.Value ? row["emailid"].ToString() : null,
                                        entityname = row["companyname"] != DBNull.Value ? row["companyname"].ToString() : null,
                                        unitlocationname = row["locationname"] != DBNull.Value ? row["locationname"].ToString() : null,
                                        AcknowledgementStatus = row["acknowledgemet_date"] != DBNull.Value ? "Acknowledged" : "Not Acknowledged",
                                        acknowledgemet_date = row["acknowledgemet_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["acknowledgemet_date"]) : null,
                                        depatment = row["departmentname"] != DBNull.Value ? row["departmentname"].ToString() : null,
                                        //assessment builder
                                        assessment_name = row["assessment_name"] != DBNull.Value ? row["assessment_name"].ToString() : null,
                                        total_questions = row["total_questions"] != DBNull.Value ? row["total_questions"].ToString() + " Questions" : null,
                                        total_estimated_time = row["total_estimated_time"] != DBNull.Value ? row["total_estimated_time"].ToString() + " Minutes" : null,
                                        Type_Name = row["Type_Name"] != DBNull.Value ? row["Type_Name"].ToString() : null,
                                        SubType_Name = row["SubType_Name"] != DBNull.Value ? row["SubType_Name"].ToString() : null,
                                        Competency_Name = row["Competency_Name"] != DBNull.Value ? row["Competency_Name"].ToString() : null,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : null,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : null,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : null,
                                        verson_no = row["verson_no"] != DBNull.Value ? Convert.ToInt32(row["verson_no"]) : (int?)null,

                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log it, rethrow it, or other appropriate action)
                throw new ApplicationException("An error occurred while fetching scheduled assessments", ex);
            }

            return pdata;
        }



        //Exempted user list in scheduled assessment

        [Route("api/GovControlReportsController/getexempteduserlist/{id}")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> getexempteduserlist(string id, DateModel DateModels)
        {
            var pdata = new List<GettingAssessmentTemplateModel>();
            var assessmentids = GetAssessmentIds(id); // Assuming GetAssessmentIds method exists

            if (assessmentids.Count == 0)
            {
                return pdata; // No assessments to query
            }

            // Convert the list of IDs to a comma-separated string
            string ids = string.Join(",", assessmentids);

            string baseQuery = @"
SELECT sa.Schedule_Assessment_id, sa.Date_Of_Request,ulm.Unit_location_Master_name as locationname,tm.Department_Master_id,dm.Department_Master_name as departmentname,sa.frequency_period,tm.emailid as emailid,tm.Entity_Master_id,tm.Unit_location_Master_id,em.Entity_Master_Name as companyname,
       sa.Duration_of_Assessment, sa.repeatEndDate, sa.startDate, sa.endDate, sa.ass_template_id as templateid, sa.AssessmentStatus, sa.uq_ass_schid, sa.pagetype, sa.created_date as scheduled_date, sa.status as scheduled_status,
       ab.Competency_id,ab.verson_no, ab.Type_id, ab.assessment_name, ab.total_questions, ab.total_estimated_time, tu.firstname as assessor_name, tn.Type_Name, sn.SubType_Name,seu.Exemption_user_reason as exemptionreason,
       cn.Competency_Name, dt.DocTypeName, dc.Doc_CategoryName, dsc.Doc_SubCategoryName,tm.firstname as mappedusername
FROM schedule_assessment sa
JOIN assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id
JOIN tbluser tu ON tu.USR_ID = sa.userid
JOIN scheduled_excluded_user seu ON seu.Schedule_Assessment_id = sa.Schedule_Assessment_id
JOIN tbluser tm ON tm.USR_ID = seu.Exemption_user
join department_master dm on dm.Department_Master_id=tm.Department_Master_id
JOIN entity_master em ON em.Entity_Master_id = tm.Entity_Master_id
join unit_location_master ulm on ulm.Unit_location_Master_id=tm.Unit_location_Master_id
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
JOIN risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
JOIN risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
JOIN risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
WHERE sa.status = 'Active' AND sa.Schedule_Assessment_id IN (" + ids + ")";

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(baseQuery, con))
                    {
                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GettingAssessmentTemplateModel
                                    {
                                        Schedule_Assessment_id = (int)(row["Schedule_Assessment_id"] != DBNull.Value ? Convert.ToInt32(row["Schedule_Assessment_id"]) : (int?)null),
                                        Date_Of_Request = (DateTime)(row["Date_Of_Request"] != DBNull.Value ? Convert.ToDateTime(row["Date_Of_Request"]) : (DateTime?)null),
                                        frequency_period = row["frequency_period"] != DBNull.Value ? row["frequency_period"].ToString() : null,
                                        Duration_of_Assessment = row["Duration_of_Assessment"] != DBNull.Value ? row["Duration_of_Assessment"].ToString() : null,
                                        //repeatEndDate = (DateTime)(row["repeatEndDate"] != DBNull.Value ? Convert.ToDateTime(row["repeatEndDate"]) : (DateTime?)null),
                                        startDate = (DateTime)(row["startDate"] != DBNull.Value ? Convert.ToDateTime(row["startDate"]) : (DateTime?)null),
                                        endDate = (DateTime)(row["endDate"] != DBNull.Value ? Convert.ToDateTime(row["endDate"]) : (DateTime?)null),
                                        ass_template_id = row["templateid"] != DBNull.Value ? row["templateid"].ToString() : null,
                                        AssessmentStatus = row["AssessmentStatus"] != DBNull.Value ? row["AssessmentStatus"].ToString() : null,
                                        uq_ass_schid = row["uq_ass_schid"] != DBNull.Value ? row["uq_ass_schid"].ToString() : null,
                                        pagetype = row["pagetype"] != DBNull.Value ? row["pagetype"].ToString() : null,
                                        scheduled_created_date = (DateTime)(row["scheduled_date"] != DBNull.Value ? Convert.ToDateTime(row["scheduled_date"]) : (DateTime?)null),
                                        scheduled_status = row["scheduled_status"] != DBNull.Value ? row["scheduled_status"].ToString() : null,
                                        AssessorName = row["assessor_name"] != DBNull.Value ? row["assessor_name"].ToString() : null,
                                        mapped_user_name = row["mappedusername"] != DBNull.Value ? row["mappedusername"].ToString() : null,
                                        //total_mapped_users = (int)(row["total_mapped_users"] != DBNull.Value ? Convert.ToInt32(row["total_mapped_users"]) : (int?)null),
                                        emailid = row["emailid"] != DBNull.Value ? row["emailid"].ToString() : null,
                                        entityname = row["companyname"] != DBNull.Value ? row["companyname"].ToString() : null,
                                        unitlocationname = row["locationname"] != DBNull.Value ? row["locationname"].ToString() : null,
                                        depatment = row["departmentname"] != DBNull.Value ? row["departmentname"].ToString() : null,
                                        exemptionreason = row["exemptionreason"] != DBNull.Value ? row["exemptionreason"].ToString() : null,



                                        //assessment builder
                                        assessment_name = row["assessment_name"] != DBNull.Value ? row["assessment_name"].ToString() : null,
                                        total_questions = row["total_questions"] != DBNull.Value ? row["total_questions"].ToString() + " Questions" : null,
                                        total_estimated_time = row["total_estimated_time"] != DBNull.Value ? row["total_estimated_time"].ToString() + " Seconds" : null,
                                        Type_Name = row["Type_Name"] != DBNull.Value ? row["Type_Name"].ToString() : null,
                                        SubType_Name = row["SubType_Name"] != DBNull.Value ? row["SubType_Name"].ToString() : null,
                                        Competency_Name = row["Competency_Name"] != DBNull.Value ? row["Competency_Name"].ToString() : null,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : null,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : null,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : null,
                                        verson_no = row["verson_no"] != DBNull.Value ? Convert.ToInt32(row["verson_no"]) : (int?)null,

                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log it, rethrow it, or other appropriate action)
                throw new ApplicationException("An error occurred while fetching scheduled assessments", ex);
            }

            return pdata;
        }



        //List of Assessments Scheduled With Status(Task Owner)



        [Route("api/GovControlReportsController/GetScheduledAssessmentsTaskOwners/{id}")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> GetScheduledAssessmentsTaskOwners(int id, DateModel DateModels)
        {
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GettingAssessmentTemplateModel>();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                formattedToday = DateModels.datetype == "Acknowledgement"
                    ? todayDate.ToString("yyyy-MM-dd")
                    : todayDate.ToString("yyyy-MM-dd");

                formattedMonthAgo = DateModels.datetype == "Acknowledgement"
                    ? monthAgoDate.ToString("yyyy-MM-dd")
                    : monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string baseQuery = @"
SELECT 
    sa.Schedule_Assessment_id,
    sa.acknowledgemet_date, 
    sa.Date_Of_Request,
    ulm.Unit_location_Master_name AS locationname,
    sa.frequency_period,
    tm.emailid AS emailid,
    tm.Entity_Master_id,
    tm.Unit_location_Master_id,
    em.Entity_Master_Name AS companyname,
    sa.Duration_of_Assessment,
    sa.repeatEndDate,
    sa.startDate,
    sa.endDate,
    sa.ass_template_id AS templateid,
    sa.AssessmentStatus,sa.message,
    sa.uq_ass_schid,
    sa.pagetype,
    sa.created_date AS scheduled_date,
    sa.status AS scheduled_status,
    ab.Competency_id,
    ab.Type_id,
    ab.assessment_name,
ab.verson_no,
ab.status,
    ab.total_questions,
    ab.total_estimated_time,
    tu.firstname AS assessor_name,
 rq.firstname AS requestingpersonname,
    tn.Type_Name,
    sn.SubType_Name,
    cn.Competency_Name,
    dt.DocTypeName,
    dc.Doc_CategoryName,
    dsc.Doc_SubCategoryName,
    tm.firstname AS mappedusername,
      CASE 
        WHEN DATE(sa.endDate) = CURRENT_DATE THEN 1
        ELSE TIMESTAMPDIFF(DAY, CURRENT_DATE, sa.endDate)
    END AS timeleft_seconds
FROM 
    schedule_assessment sa
JOIN 
    assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id
JOIN 
    tbluser rq ON rq.USR_ID = sa.userid
JOIN 
    tbluser tu ON tu.USR_ID = sa.login_userid
JOIN 
    tbluser tm ON tm.USR_ID = sa.mapped_user
JOIN 
    entity_master em ON em.Entity_Master_id = tm.Entity_Master_id
JOIN 
    unit_location_master ulm ON ulm.Unit_location_Master_id = tm.Unit_location_Master_id
JOIN 
    risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN 
    risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN 
    risk.type tn ON tn.Type_id = ab.Type_id
JOIN 
    risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
JOIN 
    risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
JOIN 
    risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
WHERE sa.status = 'Active' AND sa.mapped_user = @MappedUser AND  (
    ab.status = 'Active' 
    OR ab.status = 'InActive'
  ) AND sa.verson_no = ab.verson_no AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Due" => "DATE(sa.endDate) <= @Today AND DATE(sa.endDate) >= @MonthAgo",
                "Acknowledgement" => "DATE(sa.acknowledgemet_date) <= @Today AND DATE(sa.acknowledgemet_date) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            // Combine the base query with the date condition
            string finalQuery = baseQuery + dateCondition;

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters
                        cmd.Parameters.AddWithValue("@MappedUser", id);
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GettingAssessmentTemplateModel
                                    {
                                        Schedule_Assessment_id = row["Schedule_Assessment_id"] != DBNull.Value ? (int?)Convert.ToInt32(row["Schedule_Assessment_id"]) : null,
                                        Date_Of_Request = row["Date_Of_Request"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["Date_Of_Request"]) : null,
                                        frequency_period = row["frequency_period"] != DBNull.Value ? row["frequency_period"].ToString() : null,
                                        Duration_of_Assessment = row["Duration_of_Assessment"] != DBNull.Value ? row["Duration_of_Assessment"].ToString() : null,
                                        acknowledgemet_date = row["acknowledgemet_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["acknowledgemet_date"]) : null,
                                        startDate = row["startDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["startDate"]) : null,
                                        endDate = row["endDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["endDate"]) : null,
                                        ass_template_id = row["templateid"] != DBNull.Value ? row["templateid"].ToString() : null,

                                        AssessmentStatus = row["AssessmentStatus"] != DBNull.Value ? row["AssessmentStatus"].ToString() : null,
                                        uq_ass_schid = row["uq_ass_schid"] != DBNull.Value ? row["uq_ass_schid"].ToString() : null,
                                        pagetype = row["pagetype"] != DBNull.Value ? row["pagetype"].ToString() : null,
                                        scheduled_created_date = row["scheduled_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["scheduled_date"]) : null,
                                        scheduled_status = row["scheduled_status"] != DBNull.Value ? row["scheduled_status"].ToString() : null,
                                        AssessorName = row["assessor_name"] != DBNull.Value ? row["assessor_name"].ToString() : null,//
                                        requestingpersonname = row["requestingpersonname"] != DBNull.Value ? row["requestingpersonname"].ToString() : null,
                                        mapped_user_name = row["mappedusername"] != DBNull.Value ? row["mappedusername"].ToString() : null,
                                        emailid = row["emailid"] != DBNull.Value ? row["emailid"].ToString() : null,
                                        entityname = row["companyname"] != DBNull.Value ? row["companyname"].ToString() : null,
                                        unitlocationname = row["locationname"] != DBNull.Value ? row["locationname"].ToString() : null,
                                        remaining_time_left = row["timeleft_seconds"] != DBNull.Value ? row["timeleft_seconds"].ToString() + " days" : null,

                                        assessment_name = row["assessment_name"] != DBNull.Value ? row["assessment_name"].ToString() : null,
                                        total_questions = row["total_questions"] != DBNull.Value ? row["total_questions"].ToString() + " Questions" : null,
                                        total_estimated_time = row["total_estimated_time"] != DBNull.Value ? row["total_estimated_time"].ToString() + "Minutes" : null,
                                        Type_Name = row["Type_Name"] != DBNull.Value ? row["Type_Name"].ToString() : null,
                                        SubType_Name = row["SubType_Name"] != DBNull.Value ? row["SubType_Name"].ToString() : null,
                                        Competency_Name = row["Competency_Name"] != DBNull.Value ? row["Competency_Name"].ToString() : null,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : null,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : null,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : null,
                                        message = row["message"] != DBNull.Value ? row["message"].ToString() : null,
                                        verson_no = row["verson_no"] != DBNull.Value ? Convert.ToInt32(row["verson_no"]) : (int?)null,
                                        AcknowledgementStatus = row["acknowledgemet_date"] != DBNull.Value ? "Acknowledged" : "Not Acknowledged"
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (log it, rethrow it, or other appropriate action)
                throw new ApplicationException("An error occurred while fetching scheduled assessments", ex);
            }

            return pdata;
        }



        //Getting Assessment Scheduled With Status 


        [Route("api/GovControlReportsController/getAssScheduledListwithStatus")]
        [HttpGet]
        public IEnumerable<GettingAssessmentTemplateModel> getAssScheduledListwithStatus([FromQuery] DateModel DateModels)
        {

            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;
            var pdata = new List<GettingAssessmentTemplateModel>();

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Format dates including milliseconds
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            // Define the base query with a placeholder for the date condition
            string baseQuery = @"
WITH AssessmentCount AS (
    SELECT 
        sa.uq_ass_schid,
        sa.ass_template_id,
        sa.verson_no,
        COUNT(sa.uq_ass_schid) AS total_mapped_users,
        MAX(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
        MAX(sa.Date_Of_Request) AS Date_Of_Request,
        MAX(sa.frequency_period) AS frequency_period,
        MAX(sa.Duration_of_Assessment) AS Duration_of_Assessment,
        MAX(sa.repeatEndDate) AS repeatEndDate,
        MAX(sa.startDate) AS startDate,
        MAX(sa.endDate) AS endDate,
        MAX(sa.AssessmentStatus) AS AssessmentStatus,
        MAX(sa.pagetype) AS pagetype,
        MAX(sa.created_date) AS scheduled_date,
        MAX(sa.status) AS scheduled_status,
        MAX(tu.firstname) AS assessor_name,
        MAX(dt.DocTypeName) AS DocTypeName,
        MAX(dc.Doc_CategoryName) AS Doc_CategoryName,
        MAX(dsc.Doc_SubCategoryName) AS Doc_SubCategoryName
    FROM 
        schedule_assessment sa
    JOIN tbluser tu ON tu.USR_ID = sa.userid
  
    JOIN risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
    JOIN risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
    JOIN risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
    WHERE 
        sa.status = 'Active' AND {0}
    GROUP BY 
        sa.uq_ass_schid,
        sa.ass_template_id,
        sa.verson_no
)
SELECT 
    ua.Schedule_Assessment_id,
    ua.Date_Of_Request,
    ua.frequency_period,
    ua.Duration_of_Assessment,
    ua.repeatEndDate,
    ua.startDate,
    ua.endDate,
    ua.ass_template_id AS templateid,
    ua.AssessmentStatus,
    ua.uq_ass_schid,
    ua.total_mapped_users,
    ua.pagetype,
    ua.scheduled_date AS scheduled_created_date,
    ua.scheduled_status,
    ua.assessor_name,
    cn.Competency_Name,
    tn.Type_Name,
    sn.SubType_Name,
    ua.DocTypeName,
    ua.Doc_CategoryName,
    ua.Doc_SubCategoryName,
    ua.verson_no,
    ab.assessment_name,
    ab.total_questions,
    ab.total_estimated_time,
    ab.Competency_id,
    ab.Type_id
FROM 
    AssessmentCount ua
JOIN assessment_builder_versions ab 
    ON ab.ass_template_id = ua.ass_template_id AND ab.verson_no = ua.verson_no
JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
JOIN risk.type tn ON tn.Type_id = ab.Type_id
ORDER BY
    ua.ass_template_id,
    ua.verson_no;
";


            // Determine the date condition based on the datetype
            string dateCondition = DateModels.datetype switch
            {
                "Due" => "DATE(sa.endDate) <= @Today AND DATE(sa.endDate) >= @MonthAgo",
                "Acknowledgemet" => "DATE(sa.created_date)<= @Today AND DATE(sa.created_date) >= @MonthAgo",
                "Request" => "DATE(sa.Date_Of_Request) <= @Today AND DATE(sa.Date_Of_Request) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            // Combine the base query with the date condition
            string finalQuery = string.Format(baseQuery, dateCondition);


            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new GettingAssessmentTemplateModel
                                    {
                                        //Scheduled Assessmen Fields
                                        Schedule_Assessment_id = (int)(row["Schedule_Assessment_id"] != DBNull.Value ? Convert.ToInt32(row["Schedule_Assessment_id"]) : (int?)null),
                                        Date_Of_Request = (DateTime)(row["Date_Of_Request"] != DBNull.Value ? Convert.ToDateTime(row["Date_Of_Request"]) : (DateTime?)null),
                                        frequency_period = row["frequency_period"] != DBNull.Value ? row["frequency_period"].ToString() : null,
                                        Duration_of_Assessment = row["Duration_of_Assessment"] != DBNull.Value ? row["Duration_of_Assessment"].ToString() : null,
                                        //repeatEndDate = (DateTime)(row["repeatEndDate"] != DBNull.Value ? Convert.ToDateTime(row["repeatEndDate"]) : (DateTime?)null),
                                        startDate = (DateTime)(row["startDate"] != DBNull.Value ? Convert.ToDateTime(row["startDate"]) : (DateTime?)null),
                                        endDate = (DateTime)(row["endDate"] != DBNull.Value ? Convert.ToDateTime(row["endDate"]) : (DateTime?)null),
                                        ass_template_id = row["templateid"] != DBNull.Value ? row["templateid"].ToString() : null,
                                        AssessmentStatus = row["AssessmentStatus"] != DBNull.Value ? row["AssessmentStatus"].ToString() : null,
                                        uq_ass_schid = row["uq_ass_schid"] != DBNull.Value ? row["uq_ass_schid"].ToString() : null,
                                        pagetype = row["pagetype"] != DBNull.Value ? row["pagetype"].ToString() : null,
                                        scheduled_created_date = (DateTime)(row["scheduled_created_date"] != DBNull.Value ? Convert.ToDateTime(row["scheduled_created_date"]) : (DateTime?)null),
                                        scheduled_status = row["scheduled_status"] != DBNull.Value ? row["scheduled_status"].ToString() : null,
                                        AssessorName = row["assessor_name"] != DBNull.Value ? row["assessor_name"].ToString() : null,
                                        total_mapped_users = (int)(row["total_mapped_users"] != DBNull.Value ? Convert.ToInt32(row["total_mapped_users"]) : (int?)null),

                                        //assessment builder
                                        assessment_name = row["assessment_name"] != DBNull.Value ? row["assessment_name"].ToString() : null,
                                        total_questions = row["total_questions"] != DBNull.Value ? row["total_questions"].ToString() + " Questions" : null,
                                        total_estimated_time = row["total_estimated_time"] != DBNull.Value ? row["total_estimated_time"].ToString() + "Minutes" : null,
                                        Type_Name = row["Type_Name"] != DBNull.Value ? row["Type_Name"].ToString() : null,
                                        SubType_Name = row["SubType_Name"] != DBNull.Value ? row["SubType_Name"].ToString() : null,
                                        Competency_Name = row["Competency_Name"] != DBNull.Value ? row["Competency_Name"].ToString() : null,
                                        DocTypeName = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : null,
                                        Doc_CategoryName = row["Doc_CategoryName"] != DBNull.Value ? row["Doc_CategoryName"].ToString() : null,
                                        Doc_SubCategoryName = row["Doc_SubCategoryName"] != DBNull.Value ? row["Doc_SubCategoryName"].ToString() : null,
                                        verson_no = row["verson_no"] != DBNull.Value ? Convert.ToInt32(row["verson_no"]) : (int?)null,






                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                throw new ApplicationException("An error occurred while fetching assessment templates", ex);
            }

            return pdata;
        }

        //Score IndicatorOverall
        [Route("api/GovControlReportsController/GetCompletedAssessWithResultBySI")]
        [HttpGet]

        public IEnumerable<CompletedAssScheduleModel> GetCompletedAssessWithResultBySI([FromQuery] DateModel DateModels)
        {
            var pdata = new List<CompletedAssScheduleModel>();
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Format the dates based on the date type
                if (DateModels.datetype == "Acknowledgemet" || DateModels.datetype == "Completion")
                {
                    formattedToday = todayDate.ToString("yyyy-MM-dd");
                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
                }

                else
                {
                    formattedToday = todayDate.ToString("yyyy-MM-dd");
                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid date format", ex);
            }

            string baseQuery = @"
        SELECT 
            ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
            sa.ass_template_id,
            ANY_VALUE(ab.assessment_name) AS assessment_name,
     ANY_VALUE(sa.verson_no) AS verson_no,
            ANY_VALUE(ab.assessment_description) AS assessment_description,
            DATE(ANY_VALUE(sa.created_date)) AS created_date,
            ANY_VALUE(ab.status) AS status,
            ANY_VALUE(ab.total_questions) AS no_of_questions,
            ANY_VALUE(ab.keywords) AS keywords,
            ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
            ANY_VALUE(tn.Type_Name) AS Type_Name,
            ANY_VALUE(sn.SubType_Name) AS SubType_Name,
            ANY_VALUE(dt.DocTypeName) AS document_type,
            ANY_VALUE(dc.Doc_CategoryName) AS document_category,
            ANY_VALUE(dsc.Doc_SubCategoryName) AS Sub_category,
            ANY_VALUE(cn.Competency_Name) AS Competency_Name,
            DATE(ANY_VALUE(startDate)) AS startDate,
            DATE(ANY_VALUE(endDate)) AS endDate,    
            ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
            ANY_VALUE(sa.pagetype) AS AssessmentType,
            ANY_VALUE(tu.firstname) AS assessor_name,
            sa.uq_ass_schid,
          ANY_VALUE(sas.EndDateTime)As EndDateTime,
            ANY_VALUE(mapped_user) AS mapped_user,
            ANY_VALUE(ab.Competency_id) AS Competency_id,
            DATE(ANY_VALUE(sas.StartDateTime)) AS StartDateTime,
            ANY_VALUE(sa.value_Frequency) AS frequencyvalue,
            ANY_VALUE(sa.frequency_period) AS frequency_period,
           (
        SELECT COUNT(*) 
        FROM schedule_assessment sasa
        WHERE sasa.uq_ass_schid = sa.uq_ass_schid
    ) AS total_scheduled_users,

 
    (
        SELECT COUNT(*) 
        FROM scheduled_ass_status sub_sas
        WHERE sub_sas.uq_ass_schid = sa.uq_ass_schid 
          AND sub_sas.Status = 'Result Published'
         
    ) AS total_completed_users,
            ANY_VALUE(sas.EndDateTime) AS EndDateTime,
            ANY_VALUE(sa.acknowledgemet_date) AS acknowledgemet_date
        FROM 
            assessment_builder_versions ab
            LEFT JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
            LEFT JOIN risk.type tn ON tn.Type_id = ab.Type_id
            LEFT JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
            LEFT JOIN risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id  and sa.verson_no = ab.verson_no
            LEFT JOIN risk.tbluser tu ON tu.USR_ID = sa.login_userid
           LEFT  JOIN risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
            LEFT JOIN risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
           LEFT  JOIN risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
            LEFT JOIN scheduled_ass_status sas ON sas.AssessementTemplateID = ab.ass_template_id   
            LEFT JOIN mitigations mt ON sas.AssessementTemplateID = mt.ass_template_id
             
        WHERE 
            sa.uq_ass_schid IS NOT NULL 
            AND sas.Status = 'Result Published' 
            AND sa.AssessmentStatus = 'Assessment Completed' 
            AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Due" => "DATE(endDate) <= @Today AND DATE(endDate) >= @MonthAgo",
                "Acknowledgemet" => "DATE(acknowledgemet_date) <= @Today AND DATE(acknowledgemet_date) >= @MonthAgo",
                "Completion" => "DATE(sas.EndDateTime) <= @Today AND DATE(sas.EndDateTime) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = baseQuery + dateCondition + " GROUP BY sa.ass_template_id, sa.uq_ass_schid";

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                string assessmentCompletedDate = row["StartDateTime"] != DBNull.Value && row["EndDateTime"] != DBNull.Value
                                    ? ((DateTime)row["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["EndDateTime"]).ToString("dd-MM-yyyy")
                                    : string.Empty;



                                pdata.Add(new CompletedAssScheduleModel
                                {
                                    Type_Name = row["Type_Name"].ToString(),
                                    assessment_builder_id = Convert.ToInt32(row["assessment_builder_id"]),
                                    Schedule_Assessment_id = Convert.ToInt32(row["Schedule_Assessment_id"]),
                                    Competency_id = Convert.ToInt32(row["Competency_id"]),
                                    ass_template_id = row["ass_template_id"].ToString(),
                                    verson_no = Convert.ToInt32(row["verson_no"]),
                                    assessment_name = row["assessment_name"].ToString(),
                                    assessment_description = row["assessment_description"].ToString(),
                                    SubType_Name = row["SubType_Name"].ToString(),
                                    Competency_Name = row["Competency_Name"].ToString(),
                                    created_date = Convert.ToDateTime(row["created_date"]),
                                    startDate = Convert.ToDateTime(row["startDate"]),
                                    endDate = Convert.ToDateTime(row["endDate"]),
                                    keywords = row["keywords"].ToString(),
                                    status = row["status"].ToString(),
                                    AssessmentStatus = row["AssessmentStatus"].ToString(),
                                    uq_ass_schid = row["uq_ass_schid"].ToString(),
                                    AssessementDueDate = ((DateTime)row["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["endDate"]).ToString("dd-MM-yyyy"),
                                    AssessementcompletedDate = assessmentCompletedDate,
                                    document_type = row["document_type"].ToString(),
                                    document_category = row["document_category"].ToString(),
                                    document_subCategory = row["Sub_category"].ToString(),
                                    EndDateTime = Convert.ToDateTime(row["EndDateTime"]),
                                    number_of_ques = Convert.ToInt32(row["no_of_questions"]),
                                    assessment_type = row["AssessmentType"].ToString(),
                                    assessor_name = row["assessor_name"].ToString(),
                                    frequencyperiod = row["frequencyvalue"].ToString() + row["frequency_period"].ToString(),
                                    total_scheduled_users = Convert.ToInt32(row["total_scheduled_users"]),
                                    total_completed_users = Convert.ToInt32(row["total_completed_users"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching scheduled assessments", ex);
            }

            return pdata;
            //            var pdata = new List<CompletedAssScheduleModel>();
            //            DateTime todayDate, monthAgoDate;
            //            string formattedToday, formattedMonthAgo;

            //            try
            //            {
            //                // Validate and parse the dates
            //                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            //                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            //                // Format the dates based on the date type
            //                if (DateModels.datetype == "Acknowledgemet" || DateModels.datetype == "Completion")
            //                {
            //                    formattedToday = todayDate.ToString("yyyy-MM-dd");
            //                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            //                }

            //                else
            //                {
            //                    formattedToday = todayDate.ToString("yyyy-MM-dd");
            //                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            //                }
            //            }
            //            catch (FormatException ex)
            //            {
            //                throw new ArgumentException("Invalid date format", ex);
            //            }

            //            string baseQuery = @"
            //        SELECT 
            //            ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
            //            sa.ass_template_id,
            //            ANY_VALUE(ab.assessment_name) AS assessment_name,
            //ANY_VALUE(ab.verson_no) AS verson_no,
            //            ANY_VALUE(ab.assessment_description) AS assessment_description,
            //            DATE(ANY_VALUE(sa.created_date)) AS created_date,
            //            ANY_VALUE(ab.status) AS status,
            //            ANY_VALUE(ab.total_questions) AS no_of_questions,
            //            ANY_VALUE(ab.keywords) AS keywords,
            //            ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
            //            ANY_VALUE(tn.Type_Name) AS Type_Name,
            //            ANY_VALUE(sn.SubType_Name) AS SubType_Name,
            //            ANY_VALUE(dt.DocTypeName) AS document_type,
            //            ANY_VALUE(dc.Doc_CategoryName) AS document_category,
            //            ANY_VALUE(dsc.Doc_SubCategoryName) AS Sub_category,
            //            ANY_VALUE(cn.Competency_Name) AS Competency_Name,
            //            DATE(ANY_VALUE(startDate)) AS startDate,
            //            DATE(ANY_VALUE(endDate)) AS endDate,    
            //            ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
            //            ANY_VALUE(sa.pagetype) AS AssessmentType,
            //            ANY_VALUE(tu.firstname) AS assessor_name,
            //            sa.uq_ass_schid,
            //            ANY_VALUE(mapped_user) AS mapped_user,
            //            ANY_VALUE(ab.Competency_id) AS Competency_id,
            //            DATE(ANY_VALUE(sas.StartDateTime)) AS StartDateTime,
            //            ANY_VALUE(sa.value_Frequency) AS frequencyvalue,
            //            ANY_VALUE(sa.frequency_period) AS frequency_period,
            //            COUNT(sas.uq_ass_schid = sa.uq_ass_schid) AS Countofusers,
            //            ANY_VALUE(sas.EndDateTime) AS EndDateTime,
            //            ANY_VALUE(sa.acknowledgemet_date) AS acknowledgemet_date
            //        FROM 
            //            assessment_builder_versions ab
            //            JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
            //            JOIN risk.type tn ON tn.Type_id = ab.Type_id
            //            JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
            //            JOIN risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id
            //            JOIN risk.tbluser tu ON tu.USR_ID = sa.userid
            //            JOIN risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
            //            JOIN risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
            //            JOIN risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
            //            JOIN scheduled_ass_status sas ON sas.AssessementTemplateID = ab.ass_template_id
            //        WHERE 
            //            sa.uq_ass_schid IS NOT NULL 
            //            AND sas.Status = 'Result Published' 
            //            AND sa.AssessmentStatus = 'Assessment Completed' 
            //            AND ";

            //            string dateCondition = DateModels.datetype switch
            //            {
            //                "Due" => "DATE(endDate) <= @Today AND DATE(endDate) >= @MonthAgo",
            //                "Acknowledgemet" => "DATE(acknowledgemet_date) <= @Today AND DATE(acknowledgemet_date) >= @MonthAgo",
            //                "Completion" => "DATE(sas.EndDateTime) <= @Today AND DATE(sas.EndDateTime) >= @MonthAgo",
            //                _ => throw new ArgumentException("Invalid date type")
            //            };

            //            string finalQuery = baseQuery + dateCondition + " GROUP BY sa.ass_template_id, sa.uq_ass_schid";

            //            try
            //            {
            //                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            //                {
            //                    con.Open();
            //                    using (var cmd = new MySqlCommand(finalQuery, con))
            //                    {
            //                        cmd.Parameters.AddWithValue("@Today", formattedToday);
            //                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

            //                        using (var da = new MySqlDataAdapter(cmd))
            //                        {
            //                            var dt = new DataTable();
            //                            da.Fill(dt);
            //                            foreach (DataRow row in dt.Rows)
            //                            {
            //                                string assessmentCompletedDate = row["StartDateTime"] != DBNull.Value && row["EndDateTime"] != DBNull.Value
            //                                    ? ((DateTime)row["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["EndDateTime"]).ToString("dd-MM-yyyy")
            //                                    : string.Empty;

            //                                pdata.Add(new CompletedAssScheduleModel
            //                                {
            //                                    Type_Name = row["Type_Name"].ToString(),
            //                                    assessment_builder_id = Convert.ToInt32(row["assessment_builder_id"]),
            //                                    Schedule_Assessment_id = Convert.ToInt32(row["Schedule_Assessment_id"]),
            //                                    Competency_id = Convert.ToInt32(row["Competency_id"]),
            //                                    ass_template_id = row["ass_template_id"].ToString(),
            //                                    assessment_name = row["assessment_name"].ToString(),
            //                                    assessment_description = row["assessment_description"].ToString(),
            //                                    SubType_Name = row["SubType_Name"].ToString(),
            //                                    Competency_Name = row["Competency_Name"].ToString(),
            //                                    created_date = Convert.ToDateTime(row["created_date"]),
            //                                    startDate = Convert.ToDateTime(row["startDate"]),
            //                                    endDate = Convert.ToDateTime(row["endDate"]),
            //                                    keywords = row["keywords"].ToString(),
            //                                    status = row["status"].ToString(),
            //                                    uq_ass_schid = row["uq_ass_schid"].ToString(),
            //                                    AssessementDueDate = ((DateTime)row["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["endDate"]).ToString("dd-MM-yyyy"),
            //                                    AssessementcompletedDate = assessmentCompletedDate,
            //                                    document_type = row["document_type"].ToString(),
            //                                    document_category = row["document_category"].ToString(),
            //                                    document_subCategory = row["Sub_category"].ToString(),
            //                                    total_mapped_users = Convert.ToInt32(row["Countofusers"]),
            //                                    number_of_ques = Convert.ToInt32(row["no_of_questions"]),
            //                                    assessment_type = row["AssessmentType"].ToString(),
            //                                    assessor_name = row["assessor_name"].ToString(),
            //                                    frequencyperiod = row["frequencyvalue"].ToString() + row["frequency_period"].ToString(),
            //                                    verson_no = Convert.ToInt32(row["verson_no"])
            //                                });
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                throw new ApplicationException("An error occurred while fetching scheduled assessments", ex);
            //            }

            //            return pdata;
        }

        //List of Assessments Completed With ScoreIndicator (sub , topic)



        [Route("api/GovControlReportsController/GetSubjectTopicResultBySI/{AssessementTemplateID}/{uq_ass_schid}/{verson_no}")]
        [HttpGet]



        public IEnumerable<Getcountsubjecttopic> GetSubjectTopicResultBySI(string AssessementTemplateID, string uq_ass_schid, string verson_no)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@" SELECT
                  tbluser.USR_ID, Any_Value(firstname) as firstname,scoreIndicator as scoreIndicators, Subject_Name, Topic_Name, topicid as topic_id,
                    (select count(*) from questionbank inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
                    inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID
where topicid= topic_id and ass_template_id=@AssessementTemplateID And assessment_builder_versions.verson_no= @verson_no)AS No_of_Questions, 
                     (select count(user_ass_ans_details.question_id)
                     from user_ass_ans_details 
                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id
                   
where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
          ((select count(user_ass_ans_details.question_id)
                     from user_ass_ans_details 
                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id
                    
where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where topicid= topic_id))*100 as ScoreIndicator,
(SELECT Score_Name 
  FROM score_indicator 
  WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
  LIMIT 1) AS ScoreIndicatorName
                FROM
              assessment_builder_versions AS a
          INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
          INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
        inner join  subject on subject.Subject_id= qb.subjectid
        inner join topic on topic.Topic_id= qb.topicid
        inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
inner join scheduled_ass_status on scheduled_ass_status.AssessementTemplateID=a.ass_template_id and scheduled_ass_status.uq_ass_schid=schedule_assessment.uq_ass_schid
        inner join score_indicator on score_indicator.Score_Name=scheduled_ass_status.scoreIndicator
       
        left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
        left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
          WHERE
              a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid And a.verson_no= @verson_no
          GROUP BY
              tbluser.USR_ID,a.user_id,scoreIndicator, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@verson_no", verson_no);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Getcountsubjecttopic>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {



                    pdata.Add(new Getcountsubjecttopic
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        no_of_answered_qstns = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        ScoreIndicator = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreIndicatorName = dt.Rows[i]["scoreIndicators"].ToString(),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),


                    });




                }
            }
            return pdata;
        }

        //List of Assessments Completed With KeyImprovements (sub , topic)
        [Route("api/GovControlReportsController/GetSubjectTopicResultByKI/{AssessementTemplateID}/{uq_ass_schid}/{verson_no}")]
        [HttpGet]

        public IEnumerable<Getcountsubjecttopic> GetSubjectTopicResultByKI(string AssessementTemplateID, string uq_ass_schid, string verson_no)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@" SELECT
                  tbluser.USR_ID, Any_Value(firstname) as firstname,scoreIndicator as scoreIndicators,scheduled_ass_status.key_Impr_Indicator_id,Key_Impr_Indicator_Name, Subject_Name, Topic_Name, topicid as topic_id,
                    (select count(*) from questionbank inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
                    inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID
where topicid= topic_id and ass_template_id=@AssessementTemplateID  And assessment_builder_versions.verson_no= @verson_no )AS No_of_Questions, 
                     (select count(user_ass_ans_details.question_id)
                     from user_ass_ans_details 
                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id
                   
where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
          ((select count(user_ass_ans_details.question_id)
                     from user_ass_ans_details 
                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id
                    
where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where topicid= topic_id))*100 as ScoreIndicator,
(SELECT Score_Name 
  FROM score_indicator 
  WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
  LIMIT 1) AS ScoreIndicatorName
                FROM
              assessment_builder_versions AS a
          INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
          INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
        inner join  subject on subject.Subject_id= qb.subjectid
        inner join topic on topic.Topic_id= qb.topicid
        inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
inner join scheduled_ass_status on scheduled_ass_status.AssessementTemplateID=a.ass_template_id and scheduled_ass_status.uq_ass_schid=schedule_assessment.uq_ass_schid
        inner join score_indicator on score_indicator.Score_Name=scheduled_ass_status.scoreIndicator
        inner join key_impr_indicator on key_impr_indicator.Key_Impr_Indicator_id=scheduled_ass_status.key_Impr_Indicator_id
        left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
        left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
          WHERE
              a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid  And a.verson_no= @verson_no
          GROUP BY
              tbluser.USR_ID,a.user_id,scoreIndicator,scheduled_ass_status.key_Impr_Indicator_id,Key_Impr_Indicator_Name, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@verson_no", verson_no);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Getcountsubjecttopic>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {



                    pdata.Add(new Getcountsubjecttopic
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        no_of_answered_qstns = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        ScoreIndicator = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreIndicatorName = dt.Rows[i]["scoreIndicators"].ToString(),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),
                        Key_Impr_Indicator_Name = dt.Rows[i]["Key_Impr_Indicator_Name"].ToString(),


                    });




                }
            }
            return pdata;
        }

        //List of Assessments Completed With Questions (sub , topic)
        [Route("api/GovControlReportsController/GetQuestionsBySubjects/{AssessementTemplateID}/{uq_ass_schid}")]
        [HttpGet]

        public IEnumerable<Getcountsubjecttopic> GetQuestionsBySubjects(string AssessementTemplateID, string uq_ass_schid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@" SELECT
             qb.question_id,     question,Subject_Name, Topic_Name, topicid as topic_id,
                    (select count(*) from questionbank where topicid= topic_id)AS No_of_Questions, 
                     (select count(user_ass_ans_details.question_id)
                     from user_ass_ans_details 
                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id
                   
where  AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
       Skill_Level_Name
                FROM
              assessment_builder_versions AS a
          INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
          INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
        inner join  subject on subject.Subject_id= qb.subjectid
        inner join topic on topic.Topic_id= qb.topicid
        inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
       inner join competency_check_level on competency_check_level.check_level_id=qb.check_level
        left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
        left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
          WHERE
              a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid
          GROUP BY
             qb.question_id,question,Topic_Name, Subject_Name , topicid order by qb.question_id; ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Getcountsubjecttopic>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {



                    pdata.Add(new Getcountsubjecttopic
                    {
                        firstname = dt.Rows[i]["question"].ToString(),


                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        no_of_answered_qstns = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),

                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),
                        Skill_Level_Name = dt.Rows[i]["Skill_Level_Name"].ToString(),


                    });




                }
            }
            return pdata;
        }





        //List of Assessments Completed With Result 

        [Route("api/GovControlReportsController/GetCompletedAssessWithResult")]
        [HttpGet]

        public IEnumerable<CompletedAssScheduleModel> GetCompletedAssessWithResult([FromQuery] DateModel DateModels)
        {
            var pdata = new List<CompletedAssScheduleModel>();
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Format the dates based on the date type
                if (DateModels.datetype == "Acknowledgemet" || DateModels.datetype == "Completion")
                {
                    formattedToday = todayDate.ToString("yyyy-MM-dd");
                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
                }

                else
                {
                    formattedToday = todayDate.ToString("yyyy-MM-dd");
                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid date format", ex);
            }

            string baseQuery = @"
        SELECT 
            ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
            sa.ass_template_id,
            ANY_VALUE(ab.assessment_name) AS assessment_name,
     ANY_VALUE(sa.verson_no) AS verson_no,
            ANY_VALUE(ab.assessment_description) AS assessment_description,
            DATE(ANY_VALUE(sa.created_date)) AS created_date,
            ANY_VALUE(ab.status) AS status,
            ANY_VALUE(ab.total_questions) AS no_of_questions,
            ANY_VALUE(ab.keywords) AS keywords,
            ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
            ANY_VALUE(tn.Type_Name) AS Type_Name,
            ANY_VALUE(sn.SubType_Name) AS SubType_Name,
            ANY_VALUE(dt.DocTypeName) AS document_type,
            ANY_VALUE(dc.Doc_CategoryName) AS document_category,
            ANY_VALUE(dsc.Doc_SubCategoryName) AS Sub_category,
            ANY_VALUE(cn.Competency_Name) AS Competency_Name,
            DATE(ANY_VALUE(startDate)) AS startDate,
            DATE(ANY_VALUE(endDate)) AS endDate,    
            ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
            ANY_VALUE(sa.pagetype) AS AssessmentType,
            ANY_VALUE(tu.firstname) AS assessor_name,
            sa.uq_ass_schid,
            ANY_VALUE(mapped_user) AS mapped_user,
            ANY_VALUE(ab.Competency_id) AS Competency_id,
            DATE(ANY_VALUE(sas.StartDateTime)) AS StartDateTime,
            ANY_VALUE(sa.value_Frequency) AS frequencyvalue,
            ANY_VALUE(sa.frequency_period) AS frequency_period,
           (
        SELECT COUNT(*) 
        FROM schedule_assessment sasa
        WHERE sasa.uq_ass_schid = sa.uq_ass_schid
    ) AS total_scheduled_users,

 
    (
        SELECT COUNT(*) 
        FROM scheduled_ass_status sub_sas
        WHERE sub_sas.uq_ass_schid = sa.uq_ass_schid 
          AND sub_sas.Status = 'Result Published'
         
    ) AS total_completed_users,
            ANY_VALUE(sas.EndDateTime) AS EndDateTime,
            ANY_VALUE(sa.acknowledgemet_date) AS acknowledgemet_date
        FROM 
            assessment_builder_versions ab
            LEFT JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
            LEFT JOIN risk.type tn ON tn.Type_id = ab.Type_id
            LEFT JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
            LEFT JOIN risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id  and sa.verson_no = ab.verson_no
            LEFT JOIN risk.tbluser tu ON tu.USR_ID = sa.login_userid
           LEFT  JOIN risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
            LEFT JOIN risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
           LEFT  JOIN risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
            LEFT JOIN scheduled_ass_status sas ON sas.AssessementTemplateID = ab.ass_template_id   
            LEFT JOIN mitigations mt ON sas.AssessementTemplateID = mt.ass_template_id
             
        WHERE 
            sa.uq_ass_schid IS NOT NULL 
            AND sas.Status = 'Result Published' 
            AND sa.AssessmentStatus = 'Assessment Completed' 
            AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Due" => "DATE(endDate) <= @Today AND DATE(endDate) >= @MonthAgo",
                "Acknowledgemet" => "DATE(acknowledgemet_date) <= @Today AND DATE(acknowledgemet_date) >= @MonthAgo",
                "Completion" => "DATE(sas.EndDateTime) <= @Today AND DATE(sas.EndDateTime) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = baseQuery + dateCondition + " GROUP BY sa.ass_template_id, sa.uq_ass_schid";

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                string assessmentCompletedDate = row["StartDateTime"] != DBNull.Value && row["EndDateTime"] != DBNull.Value
                                    ? ((DateTime)row["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["EndDateTime"]).ToString("dd-MM-yyyy")
                                    : string.Empty;



                                pdata.Add(new CompletedAssScheduleModel
                                {
                                    Type_Name = row["Type_Name"].ToString(),
                                    assessment_builder_id = Convert.ToInt32(row["assessment_builder_id"]),
                                    Schedule_Assessment_id = Convert.ToInt32(row["Schedule_Assessment_id"]),
                                    Competency_id = Convert.ToInt32(row["Competency_id"]),
                                    ass_template_id = row["ass_template_id"].ToString(),
                                    verson_no = Convert.ToInt32(row["verson_no"]),
                                    assessment_name = row["assessment_name"].ToString(),
                                    assessment_description = row["assessment_description"].ToString(),
                                    SubType_Name = row["SubType_Name"].ToString(),
                                    Competency_Name = row["Competency_Name"].ToString(),
                                    created_date = Convert.ToDateTime(row["created_date"]),
                                    startDate = Convert.ToDateTime(row["startDate"]),
                                    endDate = Convert.ToDateTime(row["endDate"]),
                                    keywords = row["keywords"].ToString(),
                                    status = row["status"].ToString(),
                                    AssessmentStatus = row["AssessmentStatus"].ToString(),
                                    uq_ass_schid = row["uq_ass_schid"].ToString(),
                                    AssessementDueDate = ((DateTime)row["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["endDate"]).ToString("dd-MM-yyyy"),
                                    AssessementcompletedDate = assessmentCompletedDate,
                                    document_type = row["document_type"].ToString(),
                                    document_category = row["document_category"].ToString(),
                                    document_subCategory = row["Sub_category"].ToString(),
                                    EndDateTime = Convert.ToDateTime(row["EndDateTime"]),
                                    number_of_ques = Convert.ToInt32(row["no_of_questions"]),
                                    assessment_type = row["AssessmentType"].ToString(),
                                    assessor_name = row["assessor_name"].ToString(),
                                    frequencyperiod = row["frequencyvalue"].ToString() + row["frequency_period"].ToString(),
                                    total_scheduled_users = Convert.ToInt32(row["total_scheduled_users"]),
                                    total_completed_users = Convert.ToInt32(row["total_completed_users"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching scheduled assessments", ex);
            }

            return pdata;
        }




        //List of Assessments Completed With Status (GetComptencySkill)


        [Route("api/GovControlReportsController/GetComptencySkillResult/{AssessementTemplateID}/{uq_ass_schid}")]
        [HttpGet]

        public IEnumerable<GetComptencyskill> GetComptencySkill(string AssessementTemplateID, string uq_ass_schid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version
            MySqlCommand cmd = new MySqlCommand(@"SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
                               (select distinct verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid) as version,
                                 (select count(*) from questionbank 
inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
where check_level= check_level_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID and assessment_generation_details.Status='Active')AS No_of_Questions, 
                                 (select count(user_ass_ans_details.question_id)
                                 from user_ass_ans_details 
                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
 (
                SELECT COUNT(*)
                FROM user_ass_ans_details uad2
                INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.check_level = check_levelid
                WHERE uad2.userid = tbluser.USR_ID
                  AND uad2.AssessementTemplateID = @AssessementTemplateID
                  AND uad2.uq_ass_schid = @uq_ass_schid
                  AND qb2.correct_answer = uad2.user_Selected_Ans
            ) AS Correct_Answers,
                     (SELECT  IFNULL(round(
    (SUM(qb.score_weightage * qb.checklevel_weightage) / 
        (SELECT SUM(qb2.score_weightage * qb2.checklevel_weightage) 
         FROM questionbank qb2
         INNER JOIN assessment_generation_details agd 
             ON agd.question_id = qb2.question_id
         INNER JOIN assessment_builder_versions abv 
             ON abv.Assessment_generationID = agd.Assessment_generationID
         WHERE abv.ass_template_id = @AssessementTemplateID 
           AND abv.verson_no = version 
           AND qb2.check_level = check_levelid)
    ) * 100,2),0) AS ScoreIndicator
FROM user_ass_ans_details uad
INNER JOIN questionbank qb 
    ON qb.question_id = uad.question_id 
    AND qb.check_level = check_levelid
WHERE uad.Status = 'Active'
    AND uad.TypeofQuestion = 'Multiple'
    AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    AND uad.userid= tbluser.USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans)as ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName,ROUND(
        (
            (
                SELECT COUNT(*)
                FROM user_ass_ans_details uad2
                INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.check_level = check_levelid
                WHERE uad2.userid = tbluser.USR_ID
                    AND uad2.AssessementTemplateID = @AssessementTemplateID
                    AND uad2.uq_ass_schid = @uq_ass_schid
                    AND qb2.correct_answer = uad2.user_Selected_Ans
            ) * 100.0
        ) /
        (
            SELECT COUNT(*)
            FROM questionbank 
            INNER JOIN assessment_generation_details agd ON agd.question_id = questionbank.question_id
            INNER JOIN assessment_builder_versions abv ON abv.Assessment_generationID = agd.Assessment_generationID AND abv.verson_no = version
            WHERE abv.ass_template_id = @AssessementTemplateID
                AND questionbank.check_level = check_levelid
                AND agd.Status = 'Active'
        ), 2
    ) AS AccuracyPercentage
                            FROM
                          assessment_builder_versions AS a
                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
                      INNER JOIN risk.competency_check_level AS ccl ON ccl.check_level_id = qb.check_level
                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
                    inner join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id and user_ass_ans_details.uq_ass_schid= schedule_assessment.uq_ass_schid
                    inner join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user

                      WHERE
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID
                      ;
            ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetComptencyskill>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {




                    pdata.Add(new GetComptencyskill
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        userid = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        No_of_answered_Questions = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        scoreindictor = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
                        Skill_Level_Name = dt.Rows[i]["Skill_Level_Name"].ToString(),
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["Correct_Answers"]),
                        AccuracyPercentage = Convert.ToDouble(dt.Rows[i]["AccuracyPercentage"]),

                    });




                }
            }
            return pdata;

        }
        //            var pdata = new List<GetComptencyskill>();
        //            string connectionString = Configuration["ConnectionStrings:myDb1"];
        //            string query = @"

        //     SELECT
        //                  tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
        //                    (select count(*) from questionbank 
        //inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
        //                    inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID
        //                    where check_level= check_levelid and ass_template_id=@AssessementTemplateID AND assessment_builder_versions.verson_no = @verson_no)AS No_of_Questions, 
        //                     (select count(user_ass_ans_details.question_id)
        //                     from user_ass_ans_details 
        //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

        //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
        //          ((select count(user_ass_ans_details.question_id)
        //                     from user_ass_ans_details 
        //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

        //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where check_level= check_levelid))*100 as ScoreIndicator,
        //(SELECT Score_Name 
        //  FROM score_indicator 
        //  WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
        //  LIMIT 1) AS ScoreIndicatorName
        //                FROM
        //              assessment_builder_versions AS a
        //          INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
        //          INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
        //          INNER JOIN risk.competency_check_level AS ccl ON ccl.check_level_id = qb.check_level
        //        inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
        //        left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
        //        left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
        //          WHERE
        //              a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid  AND a.verson_no = @verson_no
        //          GROUP BY
        //              tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID
        //          ;
        //    ";

        //            try
        //            {
        //                using (MySqlConnection con = new MySqlConnection(connectionString))
        //                {
        //                    using (MySqlCommand cmd = new MySqlCommand(query, con))
        //                    {
        //                        cmd.CommandType = CommandType.Text;
        //                        cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
        //                        cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
        //                        cmd.Parameters.AddWithValue("@verson_no", verson_no);

        //                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
        //                        DataTable dt = new DataTable();

        //                        da.Fill(dt);

        //                        if (dt.Rows.Count > 0)
        //                        {
        //                            for (var i = 0; i < dt.Rows.Count; i++)
        //                            {



        //                                pdata.Add(new GetComptencyskill
        //                                {
        //                                    firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
        //                                    userid = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

        //                                    No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
        //                                    No_of_answered_Questions = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
        //                                    scoreindictor = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
        //                                    ScoreName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
        //                                    Skill_Level_Name = dt.Rows[i]["Skill_Level_Name"].ToString(),


        //                                });




        //                            }
        //                        }

        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Handle exception
        //                // You can log the error message or throw a new exception
        //                throw new Exception("Error while fetching data: " + ex.Message, ex);
        //            }

        //            return pdata;





        //List of Assessments Completed With Status (sub , topic)



        [Route("api/GovControlReportsController/GetSubjectTopicResult/{AssessementTemplateID}/{uq_ass_schid}")]
        [HttpGet]

        public IEnumerable<Getcountsubjecttopic> GetSubjectTopicResult(string AssessementTemplateID, string uq_ass_schid)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //==Version 


            MySqlCommand cmd = new MySqlCommand(@" SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
                                (select distinct verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid ) as version,
                                (select count(*) from questionbank 
                                inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
where topicid= topic_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID  and assessment_generation_details.Status='Active'
                          )AS No_of_Questions, 
                                 (select count(user_ass_ans_details.question_id)
                                 from user_ass_ans_details 
                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,

    (SELECT COUNT(*)
     FROM user_ass_ans_details uad2
     INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.topicid = topic_id
     WHERE uad2.userid = tbluser.USR_ID 
       AND uad2.AssessementTemplateID = @AssessementTemplateID 
       AND uad2.uq_ass_schid = @uq_ass_schid
       AND qb2.correct_answer = uad2.user_Selected_Ans
    ) AS CorrectAnswers,
                     (SELECT  IFNULL(round(
    (SUM(qb.score_weightage * qb.checklevel_weightage) / 
        (SELECT SUM(qb2.score_weightage * qb2.checklevel_weightage) 
         FROM questionbank qb2
         INNER JOIN assessment_generation_details agd 
             ON agd.question_id = qb2.question_id
         INNER JOIN assessment_builder_versions abv 
             ON abv.Assessment_generationID = agd.Assessment_generationID
         WHERE abv.ass_template_id = @AssessementTemplateID 
           AND abv.verson_no = version 
           AND qb2.topicid= topic_id)
    ) * 100,2),0) AS ScoreIndicator
FROM user_ass_ans_details uad
INNER JOIN questionbank qb 
    ON qb.question_id = uad.question_id 
    AND qb.topicid= topic_id
WHERE uad.Status = 'Active'
    AND uad.TypeofQuestion = 'Multiple'
    AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    AND uad.UserID = tbluser.USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans)  as ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName,
ROUND(
    (
        SELECT COUNT(*) 
        FROM user_ass_ans_details uad
        INNER JOIN questionbank qb ON qb.question_id = uad.question_id 
        WHERE uad.AssessementTemplateID = @AssessementTemplateID
          AND uad.uq_ass_schid = @uq_ass_schid
          AND uad.UserID = tbluser.USR_ID
          AND qb.topicid = topic_id      
          AND qb.correct_answer = uad.user_Selected_Ans
    ) * 100.0 / 
    NULLIF(
        (
            SELECT COUNT(*) 
            FROM user_ass_ans_details uad
            INNER JOIN questionbank qb ON qb.question_id = uad.question_id
            WHERE uad.AssessementTemplateID = @AssessementTemplateID
              AND uad.uq_ass_schid = @uq_ass_schid
              AND uad.UserID = tbluser.USR_ID
              AND qb.topicid = topic_id         
        ), 0
    )
, 2) AS Accuracy


                            FROM
                          assessment_builder_versions AS a
                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
                    inner join  subject on subject.Subject_id= qb.subjectid
                    inner join topic on topic.Topic_id= qb.topicid
                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
                    left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
                    left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
                      WHERE
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //cmd.Parameters.AddWithValue("@verson_no", verson_no);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Getcountsubjecttopic>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {



                    pdata.Add(new Getcountsubjecttopic
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        no_of_answered_qstns = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        ScoreIndicator = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreIndicatorName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"]),
                        AccuracyPercentage = Convert.ToDouble(dt.Rows[i]["Accuracy"]),
                    });




                }
            }
            return pdata;
            //            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            //            con.Open();
            //            MySqlCommand cmd = new MySqlCommand(@" SELECT
            //                  tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
            //                    (select count(*) from questionbank inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
            //                    inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID
            //where topicid= topic_id and ass_template_id=@AssessementTemplateID   AND assessment_builder_versions.verson_no = @verson_no)AS No_of_Questions, 
            //                     (select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
            //          ((select count(user_ass_ans_details.question_id)
            //                     from user_ass_ans_details 
            //                               INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            //where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )/ (select count(*) from questionbank where topicid= topic_id))*100 as ScoreIndicator,
            //(SELECT Score_Name 
            //  FROM score_indicator 
            //  WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
            //  LIMIT 1) AS ScoreIndicatorName
            //                FROM
            //              assessment_builder_versions AS a
            //          INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
            //          INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
            //        inner join  subject on subject.Subject_id= qb.subjectid
            //        inner join topic on topic.Topic_id= qb.topicid
            //        inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
            //        left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
            //        left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
            //          WHERE
            //              a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid and a.verson_no = @verson_no
            //          GROUP BY
            //              tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

            //            cmd.CommandType = CommandType.Text;

            //            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            //            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            //            cmd.Parameters.AddWithValue("@verson_no", verson_no);
            //            DataTable dt = new DataTable();
            //            da.Fill(dt);
            //            con.Close();
            //            var pdata = new List<Getcountsubjecttopic>();
            //            if (dt.Rows.Count > 0)
            //            {
            //                for (var i = 0; i < dt.Rows.Count; i++)
            //                {



            //                    pdata.Add(new Getcountsubjecttopic
            //                    {
            //                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
            //                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

            //                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
            //                        no_of_answered_qstns = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
            //                        ScoreIndicator = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
            //                        ScoreIndicatorName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
            //                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
            //                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),


            //                    });




            //                }
            //            }
            //            return pdata;
        }


        //List of Assessments Completed With ScoreIndicator (sub , topic)//(Over All)

        [Route("api/GovControlReportsController/GetUsersScoreCounts/{AssessementTemplateID}/{uq_ass_schid}")]
        [HttpGet]

        public IEnumerable<GetcountsofUserScore> GetUsersScoreCounts(string AssessementTemplateID, string uq_ass_schid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            MySqlCommand cmd = new MySqlCommand(@"
        SELECT
           assessment_builder_versions.ass_template_id,
            user_ass_ans_details.UserID,
            tbluser.firstname,
            COALESCE(kii.Key_Impr_Indicator_Name, '') AS Key_Impr_Indicator_Name,
            scheduled_ass_status.Remarks,
            scheduled_ass_status.uq_ass_schid, tn.Type_Name,sn.SubType_Name, cn.Competency_Name,
            COUNT(DISTINCT questionbank.question_id) AS total_questions,

            (
                SELECT COUNT(question_id)
                FROM risk.user_ass_ans_details AS uad
                WHERE uad.AssessementTemplateID = @AssessementTemplateID
                AND uad.uq_ass_schid = @uq_ass_schid
                AND uad.UserID = user_ass_ans_details.UserID
            ) AS TotalQuestionsAnswered,

            (SELECT Percentage FROM scheduled_ass_status 
             WHERE AssessementTemplateID=@AssessementTemplateID 
             AND uq_ass_schid = @uq_ass_schid 
             AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS Percentages,

            (SELECT TaskOwnerScore FROM scheduled_ass_status 
             WHERE AssessementTemplateID=@AssessementTemplateID 
             AND uq_ass_schid = @uq_ass_schid 
             AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS OverallScore,

            (SELECT TotalScore FROM scheduled_ass_status 
             WHERE AssessementTemplateID=@AssessementTemplateID 
             AND uq_ass_schid = @uq_ass_schid 
             AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS TotalScoreofAssessement,

            (SELECT Score_Name 
             FROM score_indicator 
             WHERE Percentages BETWEEN scoreminrange AND scoremaxrange
             LIMIT 1) AS ScoreName,

           (
    SELECT CASE 
        WHEN DATEDIFF(sas.EndDateTime, sa.acknowledgemet_date) = 0 THEN 1
        ELSE DATEDIFF(sas.EndDateTime, sa.acknowledgemet_date)
    END  
    FROM scheduled_ass_status sas
    INNER JOIN schedule_assessment sa ON sas.uq_ass_schid = sa.uq_ass_schid
    WHERE sas.Status = 'Assessment Completed'
      AND sas.uq_ass_schid = @uq_ass_schid
      AND sas.AssessementTemplateID = @AssessementTemplateID
      AND sas.UserID = user_ass_ans_details.UserID
    LIMIT 1
) AS Days,

            (
                SELECT COUNT(*)
                FROM user_ass_ans_details uad2
                INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
                WHERE uad2.AssessementTemplateID = @AssessementTemplateID
                AND uad2.uq_ass_schid = @uq_ass_schid
                AND uad2.UserID = user_ass_ans_details.UserID
                AND uad2.user_Selected_Ans = qb2.correct_answer
            ) AS CorrectAnswers,

            (
                SELECT ROUND(
                    (
                        SELECT COUNT(*)
                        FROM user_ass_ans_details uad2
                        INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
                        WHERE uad2.AssessementTemplateID = @AssessementTemplateID
                        AND uad2.uq_ass_schid = @uq_ass_schid
                        AND uad2.UserID = user_ass_ans_details.UserID
                        AND uad2.user_Selected_Ans = qb2.correct_answer
                    ) * 100.0 / 
                    (
                        SELECT COUNT(*)
                        FROM user_ass_ans_details uad3
                        WHERE uad3.AssessementTemplateID = @AssessementTemplateID
                        AND uad3.uq_ass_schid = @uq_ass_schid
                        AND uad3.UserID = user_ass_ans_details.UserID
                    ), 2
                )
            ) AS AccuracyPercentage

        FROM
            assessment_builder_versions
       INNER JOIN assessment_generation_details ON assessment_generation_details.Assessment_generationID = assessment_builder_versions.Assessment_generationID
        INNER JOIN questionbank ON questionbank.question_id = assessment_generation_details.question_id
        INNER JOIN questionbank_options ON questionbank_options.question_id = questionbank.question_id
        INNER JOIN user_ass_ans_details ON user_ass_ans_details.question_id = questionbank.question_id AND user_ass_ans_details.uq_ass_schid = @uq_ass_schid
        INNER JOIN tbluser ON tbluser.USR_ID = user_ass_ans_details.UserID
        INNER JOIN scheduled_ass_status ON scheduled_ass_status.AssessementTemplateID = user_ass_ans_details.AssessementTemplateID 
            AND scheduled_ass_status.uq_ass_schid = @uq_ass_schid 
            AND scheduled_ass_status.UserID = user_ass_ans_details.UserID
        INNER JOIN key_impr_indicator kii ON scheduled_ass_status.key_Impr_Indicator_id = kii.key_Impr_Indicator_id
        INNER JOIN risk.sub_type sn ON sn.SubType_id = assessment_builder_versions.SubType_id
INNER JOIN risk.type tn ON tn.Type_id = assessment_builder_versions.Type_id
INNER JOIN risk.competency_skill cn ON cn.Competency_id = assessment_builder_versions.Competency_id
        WHERE
            assessment_builder_versions.ass_template_id = @AssessementTemplateID

        GROUP BY
            user_ass_ans_details.UserID, tbluser.firstname, scheduled_ass_status.Remarks, kii.Key_Impr_Indicator_Name ,tn.Type_Name,sn.SubType_Name, cn.Competency_Name;
    ", con);

            cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();

            var pdata = new List<GetcountsofUserScore>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string daysvalue = "";
                    int day11 = 0;

                    // Try parsing safely
                    if (int.TryParse(dt.Rows[i]["Days"].ToString(), out day11))
                    {
                        // if day11 is 0 or 1, treat it as "1 Day"
                        if (day11 <= 1)
                        {
                            day11 = 1;  // enforce minimum 1 day in display
                            daysvalue = " Day";
                        }
                        else
                        {
                            daysvalue = " Days";
                        }
                    }
                    else
                    {
                        // fallback if parsing fails
                        day11 = 1;
                        daysvalue = " Day";
                    }


                    pdata.Add(new GetcountsofUserScore
                    {
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        UserID = Convert.ToInt32(dt.Rows[i]["UserID"].ToString()),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        ScoreName = dt.Rows[i]["ScoreName"].ToString(),
                        Key_Impr_Indicator_Name = dt.Rows[i]["Key_Impr_Indicator_Name"].ToString(),
                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"].ToString()),
                        TotalQuestionsAnswered = Convert.ToInt32(dt.Rows[i]["TotalQuestionsAnswered"].ToString()),
                        OverallScore = dt.Rows[i]["OverallScore"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["OverallScore"].ToString()) : 0,
                        TotalScoreofAssessement = dt.Rows[i]["TotalScoreofAssessement"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["TotalScoreofAssessement"].ToString()) : 0,
                        Percentage = dt.Rows[i]["Percentages"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["Percentages"].ToString()) : 0,
                        Remarks = dt.Rows[i]["Remarks"].ToString(),
                        Days = day11 + daysvalue,
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"].ToString()),
                        AccuracyPercentage = dt.Rows[i]["AccuracyPercentage"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["AccuracyPercentage"].ToString()) : 0
                    });
                }
            }

            // To remove duplicate UserID records
            pdata = pdata.GroupBy(x => x.UserID).Select(g => g.First()).ToList();

            return pdata;
            //            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            //            con.Open();
            //            MySqlCommand cmd = new MySqlCommand(@"
            //SELECT
            //    user_ass_ans_details.UserID,
            //    tbluser.firstname,
            //COUNT(DISTINCT questionbank.question_id) AS total_questions,

            //    (
            //        SELECT COUNT(question_id)
            //        FROM risk.user_ass_ans_details AS uad
            //        WHERE uad.AssessementTemplateID = @AssessementTemplateID
            //        AND uad.uq_ass_schid = @uq_ass_schid
            //        AND uad.UserID = user_ass_ans_details.UserID
            //    ) AS TotalQuestionsAnswered,
            //    (SELECT Percentage FROM scheduled_ass_status 
            //    WHERE AssessementTemplateID=@AssessementTemplateID 
            //    AND uq_ass_schid = @uq_ass_schid 
            //    AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS Percentages,
            //    (SELECT TaskOwnerScore FROM scheduled_ass_status 
            //    WHERE AssessementTemplateID=@AssessementTemplateID 
            //    AND uq_ass_schid = @uq_ass_schid 
            //    AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS OverallScore,
            //    (SELECT TotalScore FROM scheduled_ass_status 
            //    WHERE AssessementTemplateID=@AssessementTemplateID 
            //    AND uq_ass_schid = @uq_ass_schid 
            //    AND scheduled_ass_status.UserID=user_ass_ans_details.UserID) AS TotalScoreofAssessement,
            //    (SELECT Score_Name 
            //     FROM score_indicator 
            //     WHERE Percentages BETWEEN scoreminrange AND scoremaxrange
            //     LIMIT 1) AS ScoreName,
            // ( select CASE 
            //                        WHEN DATEDIFF(EndDateTime, StartDateTime) = 0 THEN 1 
            //                        ELSE DATEDIFF(EndDateTime, StartDateTime) 
            //                    END  AS Days
            //             from  scheduled_ass_status where Status='Assessment Completed'  and uq_ass_schid=@uq_ass_schid and AssessementTemplateID=@AssessementTemplateID
            //             AND UserID=user_ass_ans_details.UserID  LIMIT 1)  AS Days 
            //FROM
            //    assessment_builder_versions
            //INNER JOIN assessment_generation_details ON assessment_generation_details.Assessment_generationID = assessment_builder_versions.Assessment_generationID 
            //INNER JOIN questionbank ON questionbank.question_id = assessment_generation_details.question_id
            //INNER JOIN questionbank_options ON questionbank_options.question_id = questionbank.question_id
            //INNER JOIN risk.user_ass_ans_details ON risk.user_ass_ans_details.AssessementTemplateID = assessment_builder_versions.ass_template_id
            //INNER JOIN tbluser ON tbluser.USR_ID = user_ass_ans_details.UserID
            //INNER JOIN scheduled_ass_status ON scheduled_ass_status.AssessementTemplateID = user_ass_ans_details.AssessementTemplateID
            //WHERE
            //    assessment_builder_versions.ass_template_id = @AssessementTemplateID and assessment_builder_versions.verson_no = @verson_no
            //GROUP BY
            //    user_ass_ans_details.UserID, tbluser.firstname;





            //", con); cmd.CommandType = CommandType.Text;

            //            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            //            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            //            cmd.Parameters.AddWithValue("@verson_no", verson_no);
            //            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            //            DataTable dt = new DataTable();
            //            da.Fill(dt);
            //            con.Close();
            //            var pdata = new List<GetcountsofUserScore>();
            //            if (dt.Rows.Count > 0)
            //            {
            //                for (var i = 0; i < dt.Rows.Count; i++)
            //                {
            //                    string daysvalue = "";
            //                    int day11 = dt.Rows[i]["Days"].ToString() != "" ? Convert.ToInt32(dt.Rows[i]["Days"].ToString()) : 0;
            //                    if (day11 == 1 || day11 == 0)
            //                        daysvalue = " Day";
            //                    else
            //                        daysvalue = " Days";
            //                    pdata.Add(new GetcountsofUserScore
            //                    {

            //                        UserID = Convert.ToInt32(dt.Rows[i]["UserID"].ToString()),
            //                        firstname = dt.Rows[i]["firstname"].ToString(),
            //                        ScoreName = dt.Rows[i]["ScoreName"].ToString(),
            //                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"].ToString()),
            //                        TotalQuestionsAnswered = Convert.ToInt32(dt.Rows[i]["TotalQuestionsAnswered"].ToString()),
            //                        OverallScore = dt.Rows[i]["OverallScore"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["OverallScore"].ToString()) : 0,
            //                        TotalScoreofAssessement = dt.Rows[i]["TotalScoreofAssessement"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["TotalScoreofAssessement"].ToString()) : 0,
            //                        Percentage = dt.Rows[i]["Percentages"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["Percentages"].ToString()) : 0,


            //                        Days = day11 + daysvalue,

            //                    });

            //                }

            //            }
            //            return pdata;

        }

        //List of Assessments Completed By UserId(TaskOwner)

        [Route("api/GovControlReportsController/GetCompletedAssessWithResultByUserId")]
        [HttpGet]

        public IEnumerable<CompletedAssScheduleModel> GetCompletedAssessWithResultByUserId([FromQuery] DateModel DateModels, int mapped_user)
        {
            var pdata = new List<CompletedAssScheduleModel>();
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(DateModels.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(DateModels.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);



                // Format the dates based on the date type
                if (DateModels.datetype == "Acknowledgemet" || DateModels.datetype == "Completion")
                {
                    formattedToday = todayDate.ToString("yyyy-MM-dd");
                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
                }

                else
                {
                    formattedToday = todayDate.ToString("yyyy-MM-dd");
                    formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
                }
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid date format", ex);
            }
            //
            string baseQuery = @"
        SELECT 
            ANY_VALUE(sa.Schedule_Assessment_id) AS Schedule_Assessment_id,
            sa.ass_template_id,

    (
        SELECT COUNT(*)
        FROM user_ass_ans_details uad2
        INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
        WHERE uad2.UserID = sa.mapped_user
          AND uad2.AssessementTemplateID = sa.ass_template_id
          AND uad2.uq_ass_schid = sa.uq_ass_schid
          AND qb2.correct_answer = uad2.user_Selected_Ans
    ) AS CorrectAnswers,
   ROUND(
        (
            SELECT COUNT(*)
            FROM user_ass_ans_details uad2
            INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
            WHERE uad2.UserID = sa.mapped_user
              AND uad2.AssessementTemplateID = sa.ass_template_id
              AND uad2.uq_ass_schid = sa.uq_ass_schid
              AND qb2.correct_answer = uad2.user_Selected_Ans
        ) * 100.0 / NULLIF(ANY_VALUE(ab.total_questions), 0),
    2) AS Accuracy,
ANY_VALUE(ab.total_estimated_time) AS total_estimated_time,
            ANY_VALUE(ab.assessment_name) AS assessment_name,
            ANY_VALUE(sa.verson_no) AS verson_no,
            ANY_VALUE(ab.assessment_description) AS assessment_description,
            DATE(ANY_VALUE(sa.created_date)) AS created_date,
            ANY_VALUE(ab.status) AS status,
            ANY_VALUE(ab.total_questions) AS no_of_questions,
            ANY_VALUE(ab.keywords) AS keywords,
            ANY_VALUE(ab.assessment_builder_id) AS assessment_builder_id,
            ANY_VALUE(tn.Type_Name) AS Type_Name,
            ANY_VALUE(sn.SubType_Name) AS SubType_Name,
            ANY_VALUE(dt.DocTypeName) AS document_type,
            ANY_VALUE(dc.Doc_CategoryName) AS document_category,
            ANY_VALUE(dsc.Doc_SubCategoryName) AS Sub_category,
            ANY_VALUE(cn.Competency_Name) AS Competency_Name,
            DATE(ANY_VALUE(startDate)) AS startDate,
            DATE(ANY_VALUE(endDate)) AS endDate,    
            ANY_VALUE(sa.AssessmentStatus) AS AssessmentStatus,
            ANY_VALUE(sa.pagetype) AS AssessmentType,
            ANY_VALUE(tu.firstname) AS assessor_name,
            ANY_VALUE(rq.firstname) AS requestingname,
           sa.uq_ass_schid,
            ANY_VALUE(mapped_user) AS mapped_user,
            ANY_VALUE(ab.Competency_id) AS Competency_id,
            DATE(ANY_VALUE(sas.StartDateTime)) AS StartDateTime,
            ANY_VALUE(sa.value_Frequency) AS frequencyvalue,
            ANY_VALUE(sa.frequency_period) AS frequency_period,
            COUNT(sas.uq_ass_schid = sa.uq_ass_schid) AS Countofusers,
            ANY_VALUE(sas.EndDateTime) AS EndDateTime,
            ANY_VALUE(sa.acknowledgemet_date) AS acknowledgemet_date,
            ANY_VALUE(sas.Remarks) AS Remarks,

        (
          SELECT sas2.scoreIndicator
          FROM scheduled_ass_status sas2
          WHERE sas2.uq_ass_schid = sa.uq_ass_schid
            AND sas2.UserID = sa.mapped_user
          LIMIT 1
        ) AS scoreIndicator,

        (
          SELECT kii.Key_Impr_Indicator_Name
          FROM scheduled_ass_status sas2
          JOIN key_impr_indicator kii ON kii.key_Impr_Indicator_id = sas2.Key_Impr_Indicator_id
          WHERE sas2.uq_ass_schid = sa.uq_ass_schid
            AND sas2.UserID = @mapped_user
          LIMIT 1
        ) AS Key_Impr_Indicator_Name
        FROM 
            assessment_builder_versions ab
            JOIN risk.sub_type sn ON sn.SubType_id = ab.SubType_id
            JOIN risk.type tn ON tn.Type_id = ab.Type_id
            JOIN risk.competency_skill cn ON cn.Competency_id = ab.Competency_id
            JOIN risk.schedule_assessment sa ON sa.ass_template_id = ab.ass_template_id and sa.verson_no = ab.verson_no
            JOIN risk.tbluser tu ON tu.USR_ID = sa.login_userid
            JOIN risk.tbluser rq ON rq.USR_ID = sa.userid
            JOIN risk.doctype_master dt ON dt.DocTypeID = sa.DocTypeID
            JOIN risk.doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
            JOIN risk.docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
            JOIN scheduled_ass_status sas ON sas.uq_ass_schid = sa.uq_ass_schid
            JOIN key_impr_indicator  ON key_impr_indicator.key_Impr_Indicator_id = sas.Key_Impr_Indicator_id
        WHERE 
            sa.uq_ass_schid IS NOT NULL and
            sas.Status='Result Published' and sa.mapped_user=@mapped_user 
            AND sa.AssessmentStatus = 'Assessment Completed' 
            AND ";

            string dateCondition = DateModels.datetype switch
            {
                "Due" => "DATE(sa.endDate) <= @Today AND DATE(sa.endDate) >= @MonthAgo",
                "Acknowledgemet" => "DATE(sa.acknowledgemet_date) <= @Today AND DATE(sa.acknowledgemet_date) >= @MonthAgo",
                "Completion" => "DATE(sas.EndDateTime) <= @Today AND DATE(sas.EndDateTime) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = baseQuery + dateCondition + " GROUP BY sa.ass_template_id, sa.uq_ass_schid,sa.verson_no";

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(finalQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Today", formattedToday);
                        cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                        cmd.Parameters.AddWithValue("@mapped_user", mapped_user);


                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            foreach (DataRow row in dt.Rows)
                            {
                                string assessmentCompletedDate = row["StartDateTime"] != DBNull.Value && row["EndDateTime"] != DBNull.Value
                                    ? ((DateTime)row["StartDateTime"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["EndDateTime"]).ToString("dd-MM-yyyy")
                                    : string.Empty;

                                pdata.Add(new CompletedAssScheduleModel
                                {
                                    Type_Name = row["Type_Name"].ToString(),
                                    assessment_builder_id = Convert.ToInt32(row["assessment_builder_id"]),
                                    Schedule_Assessment_id = Convert.ToInt32(row["Schedule_Assessment_id"]),
                                    Competency_id = Convert.ToInt32(row["Competency_id"]),
                                    ass_template_id = row["ass_template_id"].ToString(),
                                    verson_no = Convert.ToInt32(row["verson_no"]),
                                    assessment_name = row["assessment_name"].ToString(),
                                    assessment_description = row["assessment_description"].ToString(),
                                    SubType_Name = row["SubType_Name"].ToString(),
                                    Competency_Name = row["Competency_Name"].ToString(),
                                    created_date = Convert.ToDateTime(row["created_date"]),
                                    startDate = Convert.ToDateTime(row["startDate"]),
                                    endDate = Convert.ToDateTime(row["endDate"]),
                                    AssessmentStatus = row["AssessmentStatus"].ToString(),
                                    keywords = row["keywords"].ToString(),
                                    status = row["status"].ToString(),
                                    uq_ass_schid = row["uq_ass_schid"].ToString(),
                                    AssessementDueDate = ((DateTime)row["startDate"]).ToString("dd-MM-yyyy") + " to " + ((DateTime)row["endDate"]).ToString("dd-MM-yyyy"),
                                    AssessementcompletedDate = Convert.ToDateTime(row["EndDateTime"]).ToString(),
                                    document_type = row["document_type"].ToString(),
                                    document_category = row["document_category"].ToString(),
                                    document_subCategory = row["Sub_category"].ToString(),
                                    total_mapped_users = Convert.ToInt32(row["Countofusers"]),
                                    total_estimated_time = row["total_estimated_time"].ToString() + " Minutes",
                                    number_of_ques = Convert.ToInt32(row["no_of_questions"]),
                                    assessment_type = row["AssessmentType"].ToString(),
                                    assessor_name = row["assessor_name"].ToString(),
                                    frequencyperiod = row["frequencyvalue"].ToString() + row["frequency_period"].ToString(),
                                    requestingname = row["requestingname"] != DBNull.Value ? row["requestingname"].ToString() : "",
                                    scoreIndicator = row["scoreIndicator"].ToString(),
                                    Key_Impr_Indicator_Name = row["Key_Impr_Indicator_Name"].ToString(),
                                    Remarks = row["Remarks"].ToString(),
                                    CorrectAnswers = row["CorrectAnswers"] != DBNull.Value ? Convert.ToInt32(row["CorrectAnswers"]) : 0,
                                    AccuracyPercentage = Convert.ToDouble(row["Accuracy"]),
                                 

                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching scheduled assessments", ex);
            }

            return pdata;
        }


        //List of Assessments Completed By UserID/(TaskOwner) (GetComptencySkill)


        [Route("api/GovControlReportsController/GetComptencySkillResultByUserId/{AssessementTemplateID}/{uq_ass_schid}/{mapped_user}")]
        [HttpGet]

        public IEnumerable<GetComptencyskill> GetComptencySkillResultByUserId(string AssessementTemplateID, string uq_ass_schid, int mapped_user)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            MySqlCommand cmd = new MySqlCommand(@" SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Skill_Level_Name,check_level_id as check_levelid,
                               (select verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid and mapped_user=@USR_ID) as version,
                                 (select count(*) from questionbank 
inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
where check_level= check_level_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID and assessment_generation_details.Status='Active')AS No_of_Questions, 
                                 (select count(user_ass_ans_details.question_id)
                                 from user_ass_ans_details 
                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and check_level= check_levelid

            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,
(
    SELECT COUNT(*)
    FROM user_ass_ans_details uad2
    INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
    WHERE qb2.check_level = check_level_id
      AND uad2.UserID = tbluser.USR_ID
      AND uad2.AssessementTemplateID = @AssessementTemplateID
      AND uad2.uq_ass_schid = @uq_ass_schid
      AND qb2.correct_answer = uad2.user_Selected_Ans
) AS CorrectAnswers,

(
    SELECT ROUND(
        (
            SELECT COUNT(*)
            FROM user_ass_ans_details uad2
            INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
            WHERE qb2.check_level = check_level_id
              AND uad2.UserID = tbluser.USR_ID
              AND uad2.AssessementTemplateID = @AssessementTemplateID
              AND uad2.uq_ass_schid = @uq_ass_schid
              AND qb2.correct_answer = uad2.user_Selected_Ans
        ) / NULLIF(
            (
                SELECT COUNT(*)
                FROM questionbank qb3
                INNER JOIN assessment_generation_details agd2 ON agd2.question_id = qb3.question_id
                INNER JOIN assessment_builder_versions abv2 ON abv2.Assessment_generationID = agd2.Assessment_generationID
                WHERE qb3.check_level = check_level_id
                  AND abv2.ass_template_id = @AssessementTemplateID
                  AND abv2.verson_no = version
                  AND agd2.Status = 'Active'
            ), 0) * 100, 2
    )
) AS Accuracy,

                   (SELECT  IFNULL(round(
    (SUM(qb.score_weightage * qb.checklevel_weightage) / 
        (SELECT SUM(qb2.score_weightage * qb2.checklevel_weightage) 
         FROM questionbank qb2
         INNER JOIN assessment_generation_details agd 
             ON agd.question_id = qb2.question_id
         INNER JOIN assessment_builder_versions abv 
             ON abv.Assessment_generationID = agd.Assessment_generationID
         WHERE abv.ass_template_id = @AssessementTemplateID 
           AND abv.verson_no = version 
           AND qb2.check_level = check_levelid)
    ) * 100,2),0) AS ScoreIndicator
FROM user_ass_ans_details uad
INNER JOIN questionbank qb 
    ON qb.question_id = uad.question_id 
    AND qb.check_level = check_levelid
WHERE uad.Status = 'Active'
    AND uad.TypeofQuestion = 'Multiple'
    AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    AND uad.UserID = @USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans) AS ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName
                            FROM
                          assessment_builder_versions AS a
                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
                      INNER JOIN risk.competency_check_level AS ccl ON ccl.check_level_id = qb.check_level
                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
                    left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
                    left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
                      WHERE
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid  and USR_ID=@USR_ID
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Skill_Level_Name,check_level_id order by tbluser.USR_ID;
", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetComptencyskill>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetComptencyskill
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        userid = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        No_of_answered_Questions = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        scoreindictor = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
                        Skill_Level_Name = dt.Rows[i]["Skill_Level_Name"].ToString(),
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"]),
                        AccuracyPercentage = Convert.ToDouble(dt.Rows[i]["Accuracy"]),


                    });



                }
            }
            return pdata;
        }


        //List of Assessments Completed By UserId/(TaskOwner) (sub , topic)



        [Route("api/GovControlReportsController/GetSubjectTopicResultByUserId/{AssessementTemplateID}/{uq_ass_schid}/{mapped_user}")]
        [HttpGet]

        public IEnumerable<Getcountsubjecttopic> GetSubjectTopicResultByUserId(string AssessementTemplateID, string uq_ass_schid, int mapped_user)
        {


//            ROUND(
//    (
//        SELECT COUNT(*)
//        FROM user_ass_ans_details uad
//        INNER JOIN questionbank qb ON qb.question_id = uad.question_id
//        WHERE uad.AssessementTemplateID = @AssessementTemplateID
//          AND uad.uq_ass_schid = @uq_ass_schid
//          AND uad.UserID = tbluser.USR_ID
//          AND qb.correct_answer = uad.user_Selected_Ans
//    ) * 100.0 /
//    NULLIF(
//        (
//            SELECT COUNT(*)
//            FROM user_ass_ans_details uad
//            WHERE uad.AssessementTemplateID = @AssessementTemplateID
//              AND uad.uq_ass_schid = @uq_ass_schid
//              AND uad.UserID = tbluser.USR_ID
//        ), 0
//    )
//, 2) AS Accuracy
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            MySqlCommand cmd = new MySqlCommand(@"  SELECT
                              tbluser.USR_ID, Any_Value(firstname) as firstname, Subject_Name, Topic_Name, topicid as topic_id,
                            (select verson_no from  schedule_assessment where ass_template_id=@AssessementTemplateID   AND uq_ass_schid = @uq_ass_schid and mapped_user=@USR_ID) as version,
                                (select count(*) from questionbank 
                                inner join assessment_generation_details on assessment_generation_details.question_id=questionbank.question_id
inner join assessment_builder_versions on assessment_builder_versions.Assessment_generationID=assessment_generation_details.Assessment_generationID and verson_no=version
where topicid= topic_id and assessment_builder_versions.ass_template_id=@AssessementTemplateID and assessment_generation_details.Status='Active'
                          )AS No_of_Questions,  
                                 (select count(user_ass_ans_details.question_id)
                                 from user_ass_ans_details 
                                           INNER JOIN questionbank AS qb ON qb.question_id = user_ass_ans_details.question_id and topicid= topic_id

            where user_ass_ans_details.userid= tbluser.USR_ID and AssessementTemplateID=@AssessementTemplateID and uq_ass_schid=@uq_ass_schid )  as no_of_answered_qstns,


                     
 (SELECT  IFNULL(round(
    (SUM(qb.score_weightage * qb.checklevel_weightage) / 
        (SELECT SUM(qb2.score_weightage * qb2.checklevel_weightage) 
         FROM questionbank qb2
         INNER JOIN assessment_generation_details agd 
             ON agd.question_id = qb2.question_id
         INNER JOIN assessment_builder_versions abv 
             ON abv.Assessment_generationID = agd.Assessment_generationID
         WHERE abv.ass_template_id = @AssessementTemplateID 
           AND abv.verson_no = version 
           AND qb2.topicid= topic_id)
    ) * 100,2),0) AS ScoreIndicator
FROM user_ass_ans_details uad
INNER JOIN questionbank qb 
    ON qb.question_id = uad.question_id 
    AND qb.topicid= topic_id
WHERE uad.Status = 'Active'
    AND uad.TypeofQuestion = 'Multiple'
    AND uad.AssessementTemplateID = @AssessementTemplateID
    AND uad.uq_ass_schid = @uq_ass_schid
    AND uad.UserID = @USR_ID
    AND qb.correct_answer = uad.user_Selected_Ans) as ScoreIndicator,
            (SELECT Score_Name 
              FROM score_indicator 
              WHERE ScoreIndicator BETWEEN scoreminrange AND scoremaxrange
              LIMIT 1) AS ScoreIndicatorName,
  ( SELECT COUNT(*)
  FROM user_ass_ans_details uad2
  INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
  WHERE qb2.topicid = topic_id
    AND uad2.userid = @USR_ID 
    AND uad2.AssessementTemplateID = @AssessementTemplateID 
    AND uad2.uq_ass_schid = @uq_ass_schid
    AND qb2.correct_answer = uad2.user_Selected_Ans
            ) AS CorrectAnswers,
ROUND(
    IFNULL(
        (
            SELECT COUNT(*)
            FROM user_ass_ans_details uad2
            INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id AND qb2.topicid = topic_id
            WHERE uad2.userid = tbluser.USR_ID 
              AND uad2.AssessementTemplateID = @AssessementTemplateID 
              AND uad2.uq_ass_schid = @uq_ass_schid
              AND qb2.correct_answer = uad2.user_Selected_Ans
        ) 
        / 
        NULLIF(
            (
                SELECT COUNT(*) 
                FROM questionbank 
                INNER JOIN assessment_generation_details agd 
                    ON agd.question_id = questionbank.question_id
                INNER JOIN assessment_builder_versions abv 
                    ON abv.Assessment_generationID = agd.Assessment_generationID 
                    AND abv.verson_no = version
                WHERE questionbank.topicid = topic_id 
                  AND abv.ass_template_id = @AssessementTemplateID 
                  AND agd.Status = 'Active'
            ), 0
        )
    , 0) * 100
, 2) AS Accuracy
                            FROM
                          assessment_builder_versions AS a
                      INNER JOIN assessment_generation_details AS agd ON agd.Assessment_generationID = a.Assessment_generationID
                      INNER JOIN questionbank AS qb ON qb.question_id = agd.question_id
                    inner join  subject on subject.Subject_id= qb.subjectid
                    inner join topic on topic.Topic_id= qb.topicid
                    inner join schedule_assessment on schedule_assessment.ass_template_id=a.ass_template_id
                    left join user_ass_ans_details on user_ass_ans_details.question_id= qb.question_id
                    left join tbluser on tbluser.USR_ID= schedule_assessment.mapped_user
                      WHERE
                          a.ass_template_id = @AssessementTemplateID and schedule_assessment.uq_ass_schid=@uq_ass_schid and USR_ID=@USR_ID
                      GROUP BY
                          tbluser.USR_ID,a.user_id, Topic_Name, Subject_Name , topicid order by tbluser.USR_ID ", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Getcountsubjecttopic>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {



                    pdata.Add(new Getcountsubjecttopic
                    {
                        firstname = dt.Rows[i]["firstname"].ToString() + " (" + Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()) + ")",
                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        No_of_Questions = Convert.ToInt32(dt.Rows[i]["No_of_Questions"]),
                        no_of_answered_qstns = Convert.ToInt32(dt.Rows[i]["no_of_answered_qstns"]),
                        ScoreIndicator = Convert.ToDouble(dt.Rows[i]["ScoreIndicator"]),
                        ScoreIndicatorName = dt.Rows[i]["ScoreIndicatorName"].ToString(),
                        Subject_Name = dt.Rows[i]["Subject_Name"].ToString(),
                        Topic_Name = dt.Rows[i]["Topic_Name"].ToString(),
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"]),
                        AccuracyPercentage = Convert.ToDouble(dt.Rows[i]["Accuracy"]),

                    });




                }
            }
            return pdata;

        }


        //List of Assessments Completed With ScoreIndicator (sub , topic)//(Over All)

        [Route("api/GovControlReportsController/GetUsersScoreCountsByUserId/{AssessementTemplateID}/{uq_ass_schid}/{mapped_user}")]
        [HttpGet]

        public IEnumerable<GetcountsofUserScore> GetUsersScoreCountsByUserId(string AssessementTemplateID, string uq_ass_schid, int mapped_user)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
SELECT
    uad.UserID,
    tu.firstname,
sas.Remarks,
abv.ass_template_id,
schedule_assessment.uq_ass_schid, tn.Type_Name,sn.SubType_Name, cn.Competency_Name,
    COALESCE(kii.Key_Impr_Indicator_Name, '') AS Key_Impr_Indicator_Name,
    COUNT(DISTINCT qb.question_id) AS total_questions,
    (
        SELECT COUNT(question_id)
        FROM risk.user_ass_ans_details AS uad_sub
        WHERE uad_sub.AssessementTemplateID = @AssessementTemplateID
          AND uad_sub.uq_ass_schid = @uq_ass_schid
          AND uad_sub.UserID = uad.UserID
    ) AS TotalQuestionsAnswered,
    sas.Percentage AS Percentages,
    sas.TaskOwnerScore AS OverallScore,
    sas.TotalScore AS TotalScoreofAssessement,
(
    SELECT COUNT(*)
    FROM user_ass_ans_details uad2
    INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
    WHERE uad2.UserID = uad.UserID
      AND uad2.AssessementTemplateID = @AssessementTemplateID
      AND uad2.uq_ass_schid = @uq_ass_schid
      AND qb2.correct_answer = uad2.user_Selected_Ans
) AS CorrectAnswers,

ROUND(
    IFNULL(
        (
            SELECT COUNT(*)
            FROM user_ass_ans_details uad2
            INNER JOIN questionbank qb2 ON qb2.question_id = uad2.question_id
            WHERE uad2.UserID = uad.UserID
              AND uad2.AssessementTemplateID = @AssessementTemplateID
              AND uad2.uq_ass_schid = @uq_ass_schid
              AND qb2.correct_answer = uad2.user_Selected_Ans
        ) / NULLIF(COUNT(DISTINCT qb.question_id), 0)
    , 0) * 100, 2
) AS Accuracy,

    (
        SELECT si.Score_Name
        FROM score_indicator si
        WHERE sas.Percentage BETWEEN si.scoreminrange AND si.scoremaxrange
        LIMIT 1
    ) AS ScoreName,

    CASE
        WHEN DATEDIFF(sas.EndDateTime, sas.StartDateTime) = 0 THEN 1
        ELSE DATEDIFF(sas.EndDateTime, sas.StartDateTime)
    END AS Days
FROM assessment_builder_versions abv
INNER JOIN assessment_generation_details agd ON agd.Assessment_generationID = abv.Assessment_generationID
INNER JOIN questionbank qb ON qb.question_id = agd.question_id
INNER JOIN questionbank_options qbo ON qbo.question_id = qb.question_id
left join schedule_assessment on schedule_assessment.ass_template_id = abv.ass_template_id and schedule_assessment.verson_no = abv.verson_no
INNER JOIN user_ass_ans_details uad ON uad.question_id = qb.question_id AND uad.uq_ass_schid = @uq_ass_schid
        INNER JOIN risk.sub_type sn ON sn.SubType_id = abv.SubType_id
INNER JOIN risk.type tn ON tn.Type_id = abv.Type_id
INNER JOIN risk.competency_skill cn ON cn.Competency_id = abv.Competency_id
INNER JOIN tbluser tu ON tu.USR_ID = uad.UserID
INNER JOIN (
    -- Get one scheduled_ass_status row per user (latest EndDateTime)
    SELECT s1.*
    FROM scheduled_ass_status s1
    INNER JOIN (
        SELECT UserID, MAX(EndDateTime) AS MaxEndDateTime
        FROM scheduled_ass_status
        WHERE Status = 'Result Published'
          AND uq_ass_schid = @uq_ass_schid
          AND AssessementTemplateID = @AssessementTemplateID
        GROUP BY UserID
    ) s2 ON s1.UserID = s2.UserID AND s1.EndDateTime = s2.MaxEndDateTime
    WHERE s1.Status = 'Result Published'
      AND s1.uq_ass_schid = @uq_ass_schid
      AND s1.AssessementTemplateID = @AssessementTemplateID
) sas ON sas.UserID = uad.UserID
LEFT JOIN key_impr_indicator kii ON kii.key_Impr_Indicator_id = sas.key_Impr_Indicator_id
WHERE abv.ass_template_id = @AssessementTemplateID
  AND tu.USR_ID = @USR_ID
GROUP BY uad.UserID, tu.firstname, kii.Key_Impr_Indicator_Name, sas.Percentage, sas.TaskOwnerScore, sas.TotalScore, sas.EndDateTime, sas.StartDateTime,sas.Remarks,schedule_assessment.uq_ass_schid,abv.ass_template_id,tn.Type_Name,sn.SubType_Name, cn.Competency_Name
; ", con); cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@uq_ass_schid", uq_ass_schid);
            cmd.Parameters.AddWithValue("@USR_ID", mapped_user);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetcountsofUserScore>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string daysvalue = "";
                    int day11 = dt.Rows[i]["Days"].ToString() != "" ? Convert.ToInt32(dt.Rows[i]["Days"].ToString()) : 0;
                    if (day11 == 1 || day11 == 0)
                        daysvalue = " Day";
                    else
                        daysvalue = " Days";
                    pdata.Add(new GetcountsofUserScore
                    {
                        ass_template_id = dt.Rows[i]["ass_template_id"].ToString(),
                        uq_ass_schid = dt.Rows[i]["uq_ass_schid"].ToString(),
                        SubType_Name = dt.Rows[i]["SubType_Name"].ToString(),
                        Competency_Name = dt.Rows[i]["Competency_Name"].ToString(),
                        Type_Name = dt.Rows[i]["Type_Name"].ToString(),
                        UserID = Convert.ToInt32(dt.Rows[i]["UserID"].ToString()),
                        firstname = dt.Rows[i]["firstname"].ToString(),
                        ScoreName = dt.Rows[i]["ScoreName"].ToString(),
                        Key_Impr_Indicator_Name = dt.Rows[i]["Key_Impr_Indicator_Name"].ToString(),
                        total_questions = Convert.ToInt32(dt.Rows[i]["total_questions"].ToString()),
                        TotalQuestionsAnswered = Convert.ToInt32(dt.Rows[i]["TotalQuestionsAnswered"].ToString()),
                        OverallScore = dt.Rows[i]["OverallScore"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["OverallScore"].ToString()) : 0,
                        TotalScoreofAssessement = dt.Rows[i]["TotalScoreofAssessement"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["TotalScoreofAssessement"].ToString()) : 0,
                        Percentage = dt.Rows[i]["Percentages"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["Percentages"].ToString()) : 0,
                        CorrectAnswers = Convert.ToInt32(dt.Rows[i]["CorrectAnswers"].ToString()),
                        AccuracyPercentage = dt.Rows[i]["Accuracy"].ToString() != "" ? Convert.ToDouble(dt.Rows[i]["Accuracy"].ToString()) : 0,
                        Remarks = dt.Rows[i]["Remarks"].ToString(),
                        Days = dt.Rows[i]["Days"].ToString() + daysvalue,

                    });

                }

            }
            pdata = pdata.GroupBy(x => x.UserID).Select(g => g.First()).ToList();
            return pdata;

        }

        //Mitigation Action Plan

        [Route("api/GovControlReportsContoller/MitigationActionPlan")]
        [HttpGet]

        public IEnumerable<MitigationDataModel> MitigationActionPlan(DateModel dateModel)
        {
            var pdata = new List<MitigationDataModel>();
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(dateModel.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(dateModel.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string query = @"WITH ranked_suggestions AS (
    SELECT 
        st.suggestions_id,
        st.mitigations_id,
         sa.uq_ass_schid,
        st.suggestions,
        st.status,
        st.created_date,
        st.suggested_by,
        st.acknowledge_by,
        st.action_required,
        st.notify_management,
        st.assign_responsibility,
        st.tentative_timeline,
        st.suggested_documents,
        st.action_priority,
        st.remarks,
        st.TrackerID,
        st.PO_remarks,
        st.management_remarks,
        st.completed_date,
        sa.startDate,
        sa.endDate,
        sa.userid,
        sa.DocTypeID,
        sa.Doc_CategoryID,
        sa.Doc_SubCategoryID,
        ab.Type_id,
        ab.SubType_id,
ab.ass_template_id,
        ab.ass_template_id AS AssessmentId,
        ab.assessment_name AS assessmentName,
        em.Entity_Master_Name AS CompanyName,
        ULM.Unit_location_Master_name AS locationName,
        tuass.firstname AS AssessorName,
        tu.firstname AS MitigationReqBy,
        dt.DocTypeName AS DocTypeName,
        dc.Doc_CategoryName AS DocCatName,
        dsc.Doc_SubCategoryName AS DocSubCatName,
        tp.Type_Name AS AssessType,
        subtp.SubType_Name AS Subtypename,
       cs.CompletedStatusCount,
          tc.TotalTrackerCount AS TrackerIDCount,
        ROW_NUMBER() OVER (PARTITION BY st.TrackerID ORDER BY st.created_date DESC) AS rn
    FROM 
        suggestions_tbl st
   inner JOIN mitigations m ON m.mitigations_id = st.mitigations_id
    inner JOIN schedule_assessment sa ON sa.uq_ass_schid = m.uq_ass_schid
    LEFT JOIN assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id and ab.verson_no = sa.verson_no
    LEFT OUTER JOIN 
        entity_master em ON em.Entity_Master_id = sa.Entity_Master_id
    LEFT OUTER JOIN 
        unit_location_master ULM ON ULM.Unit_location_Master_id = sa.Unit_location_Master_id
    LEFT OUTER JOIN 
        tbluser tu ON tu.USR_ID = st.acknowledge_by
    LEFT OUTER JOIN 
        doctype_master dt ON dt.DocTypeID = sa.DocTypeID
    LEFT OUTER JOIN 
        doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
    LEFT OUTER JOIN 
        docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
    LEFT OUTER JOIN 
        type tp ON tp.Type_id = ab.Type_id
    LEFT OUTER JOIN 
        sub_type subtp ON subtp.SubType_id = ab.SubType_id
    LEFT OUTER JOIN 
        tbluser tuass ON tuass.USR_ID = sa.login_userid
 LEFT JOIN (
            SELECT TrackerID, COUNT(*) AS CompletedStatusCount
            FROM suggestions_tbl
            WHERE status = 'completed'   and suggestions_tbl.assign_responsibility = @userid
            GROUP BY TrackerID
        ) cs ON cs.TrackerID = st.TrackerID
       
        LEFT JOIN (
            SELECT TrackerID, COUNT(*) AS TotalTrackerCount
            FROM suggestions_tbl  WHERE  suggestions_tbl.assign_responsibility = @userid
            GROUP BY TrackerID
        ) tc ON tc.TrackerID = st.TrackerID 
    WHERE 
        st.TrackerID IS NOT NULL AND (st.status  ='Commented' or st. status = 'completed' OR st. status = 'Processing' OR  st.status = 'Assigned' ) and st.assign_responsibility = @userid
)
SELECT 
    suggestions_id,
    mitigations_id,
    suggestions,
uq_ass_schid,
    status,
    created_date,
    suggested_by,
    acknowledge_by,
    action_required,
    notify_management,
    assign_responsibility,
    tentative_timeline,
    suggested_documents,
    action_priority,
    remarks,
    TrackerID,
    PO_remarks,
    management_remarks,
    completed_date,
    startDate,
    endDate,
    userid,
    DocTypeID,
    Doc_CategoryID,
    Doc_SubCategoryID,
    Type_id,
    SubType_id,
    CompanyName,
    locationName,
    AssessorName,
    MitigationReqBy,
    DocTypeName,
    DocCatName,
    DocSubCatName,
    AssessType,
    Subtypename,
    TrackerIDCount,
    CompletedStatusCount,
    AssessmentId,
ass_template_id,
    assessmentName
FROM 
    ranked_suggestions
WHERE 
rn = 1 and 
  (status  ='Commented' OR status = 'completed' OR status = 'Processing' OR status = 'Assigned')";

            string DateQuery = dateModel.datetype switch
            {
                "CompletionDate" => "DATE(endDate) <= @Today AND DATE(endDate) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid Date Type")
            };

            string finalQuery = string.Format(query, DateQuery);

            using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (var cmd = new MySqlCommand(finalQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Today", formattedToday);
                    cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                    cmd.Parameters.AddWithValue("@userid", dateModel.userid);

                    using (var ad = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        ad.Fill(dt);
                        try
                        {
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new MitigationDataModel
                                    {
                                        assessment_id = row["ass_template_id"] != DBNull.Value ? row["ass_template_id"].ToString() : string.Empty,
                                        assessment_name = row["assessmentName"] != DBNull.Value ? row["assessmentName"].ToString() : string.Empty,
                                        TrackerID = row["TrackerID"] != DBNull.Value ? row["TrackerID"].ToString() : string.Empty,
                                        mitigationcreateddate = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                        mitigation_requestedby = row["MitigationReqBy"] != DBNull.Value ? row["MitigationReqBy"].ToString() : string.Empty,
                                        num_of_Tasks = row["TrackerIDCount"] != DBNull.Value ? row["TrackerIDCount"].ToString() : "0",
                                        num_of_tasks_completed = row["CompletedStatusCount"] != DBNull.Value ? row["CompletedStatusCount"].ToString() : "0",
                                        status = row["status"] != DBNull.Value ? row["status"].ToString() : string.Empty,
                                        uq_ass_schid = row["uq_ass_schid"] != DBNull.Value ? row["uq_ass_schid"].ToString() : string.Empty,
                                        assessment_startDate = row["startDate"] != DBNull.Value ? Convert.ToDateTime(row["startDate"]) : DateTime.MinValue,
                                        assesssment_endDate = row["endDate"] != DBNull.Value ? Convert.ToDateTime(row["endDate"]) : DateTime.MinValue,
                                        mitigations_id = Convert.ToInt32(row["mitigations_id"]),
                                        person_requesting_assess = row["MitigationReqBy"] != DBNull.Value ? row["MitigationReqBy"].ToString() : string.Empty,
                                        doctype = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        docCategory = row["DocCatName"] != DBNull.Value ? row["DocCatName"].ToString() : string.Empty,
                                        document_subCategor = row["DocSubCatName"] != DBNull.Value ? row["DocSubCatName"].ToString() : string.Empty,
                                        assess_type = row["AssessType"] != DBNull.Value ? row["AssessType"].ToString() : string.Empty,
                                        assess_subType = row["Subtypename"] != DBNull.Value ? row["Subtypename"].ToString() : string.Empty,
                                        name_of_assessor = row["AssessorName"] != DBNull.Value ? row["AssessorName"].ToString() : string.Empty,

                                        //overal_score_award = row["name_of_assessor"].ToString(),
                                        //key_result = row["key_result"].ToString(),

                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle data processing error
                            throw new Exception("Error processing data from the database", ex);
                        }
                    }
                }
            }
            return pdata;
        }





        //Mitigatio Task list 


        [Route("api/GovControlReportsContoller/MitigationTaskList/{id}")]
        [HttpGet]

        public IEnumerable<MitigationDataModel> MitigationTaskList(DateModel dateModel, int id)
        {
            var pdata = new List<MitigationDataModel>();
            DateTime todayDate, monthAgoDate;

            try
            {
                // Parse dates safely
                todayDate = DateTime.ParseExact(dateModel.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(dateModel.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid date format", ex);
            }

            string query = @"
WITH ranked_suggestions AS (
    SELECT 
        st.suggestions_id,
        st.mitigations_id,
        st.suggestions,
        st.status,
        st.created_date,
        st.suggested_by,
        st.acknowledge_by,
        st.action_required,
        st.notify_management,
        st.assign_responsibility,
        st.tentative_timeline,
        st.suggested_documents,
        st.action_priority,
        st.remarks,
        st.TrackerID,
        st.PO_remarks,
        st.management_remarks,
        st.completed_date,
        sa.startDate,
        sa.endDate,
        sa.userid,
        sa.DocTypeID,
        sa.Doc_CategoryID,
        sa.Doc_SubCategoryID,
        sa.uq_ass_schid,
        ab.Type_id,
        ab.SubType_id,
        asscomp.EndDateTime as AssCompletionDate,
        ab.ass_template_id AS AssessmentId,
        ab.assessment_name AS assessmentName,
        em.Entity_Master_Name AS CompanyName,
        ULM.Unit_location_Master_name AS locationName,
        tuass.firstname AS AssessorName,
            tu21.emailid AS EmailId,
        tu.firstname AS MitigationReqBy,
 tu21.firstname AS assign_responsibility1,
        dt.DocTypeName AS DocTypeName,
        dc.Doc_CategoryName AS DocCatName,
        dsc.Doc_SubCategoryName AS DocSubCatName,
        tp.Type_Name AS AssessType,
        subtp.SubType_Name AS Subtypename,
 ROW_NUMBER() OVER (PARTITION BY st.suggestions_id ORDER BY st.created_date DESC) AS rn
    FROM 
        suggestions_tbl st
  inner JOIN mitigations m ON m.mitigations_id = st.mitigations_id
    inner JOIN schedule_assessment sa ON sa.uq_ass_schid = m.uq_ass_schid
    LEFT JOIN assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id and ab.verson_no = sa.verson_no
    LEFT JOIN entity_master em ON em.Entity_Master_id = sa.Entity_Master_id
    LEFT JOIN unit_location_master ULM ON ULM.Unit_location_Master_id = sa.Unit_location_Master_id
    LEFT JOIN tbluser tu ON tu.USR_ID = st.acknowledge_by
    LEFT JOIN tbluser tu21 ON tu21.USR_ID = st.assign_responsibility
    LEFT JOIN doctype_master dt ON dt.DocTypeID = sa.DocTypeID
    LEFT JOIN doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
    LEFT JOIN docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
    LEFT JOIN type tp ON tp.Type_id = ab.Type_id
    LEFT JOIN sub_type subtp ON subtp.SubType_id = ab.SubType_id
    LEFT JOIN tbluser tuass ON tuass.USR_ID = sa.userid
    LEFT JOIN scheduled_ass_status asscomp ON asscomp.uq_ass_schid = sa.uq_ass_schid
    WHERE st.TrackerID = @id  and st.assign_responsibility = @userid

)
SELECT 
    * 
FROM ranked_suggestions
WHERE  rn = 1  and  (status = 'completed' OR status = 'Processing' OR status = 'Assigned');";

            string dateCondition = dateModel.datetype switch
            {
                "CompletionDate" => "DATE(endDate) <= @Today AND DATE(endDate) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid Date Type")
            };

            string finalQuery = string.Format(query, dateCondition);

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                using (var cmd = new MySqlCommand(finalQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Today", todayDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@MonthAgo", monthAgoDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@userid", dateModel.userid);
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            var model = new MitigationDataModel
                            {
                                uq_ass_schid = row["uq_ass_schid"].ToString(),
                                assessment_id = row["AssessmentId"]?.ToString(),
                                assessment_name = row["assessmentName"]?.ToString(),
                                TrackerID = row["TrackerID"]?.ToString(),
                                mitigationcreateddate = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                company_name = row["CompanyName"]?.ToString(),
                                locationName = row["locationName"]?.ToString(),
                                person_requesting_assess = row["assign_responsibility1"]?.ToString(),
                                asigneeEmailId = row["EmailId"]?.ToString(),
                                suggestions_id = Convert.ToInt32(row["suggestions_id"]),
                                suggestions = row["suggestions"]?.ToString(),
                                mitigation_requestedby = row["MitigationReqBy"]?.ToString(),//
                                notify_management = Convert.ToInt32(row["notify_management"]) == 1 ? "Yes" : "No",
                                AssCompletionDate = row["AssCompletionDate"] != DBNull.Value ? Convert.ToDateTime(row["AssCompletionDate"]) : DateTime.MinValue,
                                assessment_startDate = row["tentative_timeline"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["tentative_timeline"]) : null,
                                assesssment_endDate = row["completed_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["completed_date"]) : null,
                                status = row["status"]?.ToString()
                            };

                            pdata.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and/or rethrow as needed
                throw new Exception("Error while retrieving mitigation task data", ex);
            }

            return pdata;
        }



        //Getting Unacknowledged List 

        [Route("api/GovControlReportsContoller/GetunAcknowledgedList/{id}")]
        [HttpGet]
        public IEnumerable<MitigationDataModel> GetunAcknowledgedList(int id, DateModel dateModle)
        {
            var pdata = new List<MitigationDataModel>();
            string query = @"SELECT mt.uq_ass_schid, abv.assessment_name, tu.firstname, mt.mitigations_id, st.suggestions, st.status, st.created_date, st.suggested_by
FROM suggestions_tbl AS st
JOIN mitigations AS mt ON mt.mitigations_id = st.mitigations_id
JOIN assessment_builder_versions AS abv ON abv.ass_template_id = mt.ass_template_id
JOIN tbluser AS tu ON tu.USR_ID = st.suggested_by
WHERE st.mitigations_id = @id AND st.status = 'Commented' and st.TrackerID = '0'  and( st.acknowledge is NULL or st.acknowledge is Not NULL)";


            using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var ad = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        ad.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            pdata.Add(new MitigationDataModel
                            {
                                assessment_id = row["uq_ass_schid"].ToString(),
                                assessment_name = row["assessment_name"].ToString(),
                                suggestions = row["suggestions"].ToString(),
                                name_of_assessor = row["firstname"].ToString(),
                                mitigationcreateddate = Convert.ToDateTime(row["created_date"].ToString()),
                                status = row["status"].ToString(),


                            });
                        }

                    }
                }
            }

            return pdata;
        }


        //Question Bank Reserve Listing

        [Route("api/GovControlReportsContoller/GetQuestionBank")]
        [HttpGet]

        public IEnumerable<QuestionModel_for_Report> GetQuestionBank(DateModel date_model)
        {
            var pdata = new List<QuestionModel_for_Report>();
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(date_model.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(date_model.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }



            string query = @"
SELECT 
    COUNT(qb.question_id) AS question_count,
    qb.userid,
 
    COUNT(CASE WHEN qb.no_of_times_used IS NOT NULL THEN 1 END) AS used_count,
    COUNT(CASE WHEN qb.status = 'Active' THEN 1 END) AS active_count,
    MAX(qb.created_date) AS recent_question_added_date,
    tu.firstname AS created_by,
    dm.Department_Master_name,
    em.Entity_Master_Name AS company_name,
    ulm.Unit_location_Master_name AS location_name
FROM 
    questionbank AS qb
    JOIN tbluser tu ON tu.USR_ID = qb.userid
    LEFT JOIN department_master dm ON dm.Department_Master_id = tu.Department_Master_id
     LEFT JOIN unit_location_master ulm ON ulm.Unit_location_Master_id = tu.Unit_location_Master_id
     LEFT JOIN entity_master em ON em.Entity_Master_id = tu.Entity_Master_id
WHERE {0}
GROUP BY 
    qb.userid, tu.firstname, dm.Department_Master_name, em.Entity_Master_Name, ulm.Unit_location_Master_name";

            string subquery = date_model.datetype switch
            {
                "CreatedDate" => "DATE(qb.created_date) <= @today AND DATE(qb.created_date) >= @monthAgo",
                "LastEditedDate" => "DATE(qb.last_edited_date) <= @today AND DATE(qb.last_edited_date) >= @monthAgo",
                "DisableDate" => "DATE(qb.disable_date) <= @today AND DATE(qb.disable_date) >= @monthAgo",
                _ => throw new ArgumentException("Invalid date type")
            };

            string finalQuery = string.Format(query, subquery);




            using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                using (var cmd = new MySqlCommand(finalQuery, con))
                {
                    using (var ad = new MySqlDataAdapter(cmd))
                    {
                        cmd.Parameters.AddWithValue("@today", formattedToday);
                        cmd.Parameters.AddWithValue("@monthAgo", formattedMonthAgo);
                        var dt = new DataTable();
                        ad.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            pdata.Add(new QuestionModel_for_Report
                            {
                                user = Convert.ToInt32(row["userid"].ToString()),
                                created_by = row["created_by"].ToString(),
                                company_name = row["company_name"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["company_name"].ToString())
    ? row["company_name"].ToString()
    : "N/A",

                                location_name = row["location_name"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["location_name"].ToString())
    ? row["location_name"].ToString()
    : "N/A",

                                department = row["Department_Master_name"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["Department_Master_name"].ToString())
    ? row["Department_Master_name"].ToString()
    : "N/A",

                                number_of_questions_created = Convert.ToInt32(row["question_count"].ToString()),
                                no_of_questions_used = Convert.ToInt32(row["used_count"].ToString()),
                                no_of_active_questions = Convert.ToInt32(row["active_count"].ToString()),
                                que_last_created_on = Convert.ToDateTime(row["recent_question_added_date"].ToString()),


                            });


                        }
                    }
                }
            }


            return pdata;
        }


        //Question Bank Reserve Listing Active Questions And My Question bank

        [Route("api/GovControlReportsContoller/userquestionsList/{user_id}")]
        [HttpGet]
        public IEnumerable<QuestionModel_for_Report> userquestionsList(int user_id, DateModel dateModel)
        {
            var pdata = new List<QuestionModel_for_Report>();

            string query = @"SELECT 
  qb.question_id,qb.question,qb.no_of_times_used,qb.created_date,qb.status,qb.estimated_time,qb.keywords,qb.objective,tu.firstname as created_by,  qb.disable_date,qb.lastupdted_date,
  ccl.Skill_Level_Name as com_check_level,qb.checklevel_weightage,sub.Subject_Name as subjectname, top.Topic_Name
FROM 
    questionbank AS qb
    join tbluser tu on tu.USR_ID=qb.userid
    join competency_check_level ccl on ccl.check_level_id=qb.check_level
    join subject sub on sub.Subject_id =qb.subjectid
    join topic top on top.Topic_id=qb.topicid
    where qb.userid=@userid and qb.status=@status
 ";
            using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@userid", user_id);
                    cmd.Parameters.AddWithValue("@status", "Active");
                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {

                            pdata.Add(new QuestionModel_for_Report
                            {
                                question_id = int.TryParse(row["question_id"].ToString(), out int questionId) ? questionId : 0,
                                question = row["question"].ToString(),
                                created_by = row["created_by"].ToString(),
                                created_date = DateTime.TryParse(row["created_date"].ToString(), out DateTime createdDate) ? createdDate : DateTime.MinValue,
                                competency_check_level = row["com_check_level"].ToString(),
                                weightage = float.TryParse(row["checklevel_weightage"].ToString(), out float weightage) ? (float?)weightage : null,
                                number_of_times_use = int.TryParse(row["no_of_times_used"].ToString(), out int numberOfTimesUse) ? numberOfTimesUse : 0,
                                estimated_time = int.TryParse(row["estimated_time"].ToString(), out int estimatedTime) ? estimatedTime : 0,
                                subject = row["subjectname"].ToString(),
                                topic = row["Topic_Name"].ToString(),
                                status = row["status"].ToString(),
                                keywords = row["keywords"].ToString(),
                                objective_in_Assessment = row["objective"].ToString(),
                                disable_date = row["disable_date"].ToString(),
                                lastupdted_date = row["lastupdted_date"].ToString(),
                            });



                        }
                    }
                }
            }


            return pdata;
        }



        //Question Bank Reserve Listing InActive Questions

        [Route("api/GovControlReportsContoller/userquestionsListforInactive/{user_id}")]
        [HttpGet]
        public IEnumerable<QuestionModel_for_Report> userquestionsListforInactive(int user_id, DateModel datemodel)
        {
            var pdata = new List<QuestionModel_for_Report>();

            string query = @"SELECT 
  qb.question_id,qb.question,qb.no_of_times_used,qb.created_date,qb.status,qb.estimated_time,tu.firstname as created_by,qb.disable_date,
  ccl.Skill_Level_Name as com_check_level,qb.checklevel_weightage,sub.Subject_Name as subjectname, top.Topic_Name
FROM 
    questionbank AS qb
    join tbluser tu on tu.USR_ID=qb.userid
    join competency_check_level ccl on ccl.check_level_id=qb.check_level
    join subject sub on sub.Subject_id =qb.subjectid
    join topic top on top.Topic_id=qb.topicid
    where qb.userid=@userid and qb.status=@status
 ";
            using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@userid", user_id);
                    cmd.Parameters.AddWithValue("@status", "InActive");
                    using (var da = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            pdata.Add(new QuestionModel_for_Report
                            {
                                question_id = int.TryParse(row["question_id"].ToString(), out int questionId) ? questionId : 0,
                                question = row["question"].ToString(),
                                created_by = row["created_by"].ToString(),
                                created_date = DateTime.TryParse(row["created_date"].ToString(), out DateTime createdDate) ? createdDate : DateTime.MinValue,
                                competency_check_level = row["com_check_level"].ToString(),
                                weightage = float.TryParse(row["checklevel_weightage"].ToString(), out float weightage) ? (float?)weightage : null,
                                number_of_times_use = int.TryParse(row["no_of_times_used"].ToString(), out int numberOfTimesUse) ? numberOfTimesUse : 0,
                                status = row["status"].ToString(),
                                disable_date = row["disable_date"].ToString(),

                            });


                        }
                    }
                }
            }


            return pdata;
        }



        //for count for expired Assesment
        [Route("api/GovControlReportsContoller/getexpiredAssessmentcount/{user_id}")]
        [HttpGet]
        public IActionResult GetExpiredAssessmentCount(int user_id)
        {
            int expiredCount = 0;

            string query = @"
        SELECT COUNT(*) 
        FROM risk.schedule_assessment 
        WHERE AssessmentStatus = 'Assessment Expired' 
          AND mapped_user = @UserId";

            try
            {

                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", user_id);

                        con.Open(); // Open the connection you created here

                        expiredCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                return Ok(new { ExpiredCount = expiredCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }





        //for count for Assessment Scheduled
        [Route("api/GovControlReportsContoller/getScheduledAssessmentcount/{user_id}")]
        [HttpGet]
        public IActionResult getScheduledAssessmentcount(int user_id)
        {
            int scheduledCount = 0;

            string query = @"
        SELECT COUNT(*) 
        FROM risk.schedule_assessment 
        WHERE (AssessmentStatus = 'Assessment Scheduled'  or schedule_assessment.AssessmentStatus='Assessment Rescheduled') 
          AND mapped_user = @UserId";

            try
            {

                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", user_id);

                        con.Open(); // Open the connection you created here

                        scheduledCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                return Ok(new { scheduledCount = scheduledCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }



        //Mitigation Action Plan

        [Route("api/GovControlReportsContoller/MitigationActionPlancrc")]
        [HttpGet]

        public IEnumerable<MitigationDataModel> MitigationActionPlancrc(DateModel dateModel)
        {
            var pdata = new List<MitigationDataModel>();
            DateTime todayDate, monthAgoDate;
            string formattedToday, formattedMonthAgo;

            try
            {
                // Validate and parse the dates
                todayDate = DateTime.ParseExact(dateModel.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(dateModel.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                formattedToday = todayDate.ToString("yyyy-MM-dd");
                formattedMonthAgo = monthAgoDate.ToString("yyyy-MM-dd");
            }
            catch (FormatException ex)
            {
                // Handle invalid date format
                throw new ArgumentException("Invalid date format", ex);
            }

            string query = @"WITH ranked_suggestions AS (
    SELECT 
        st.suggestions_id,
        st.mitigations_id,
         sa.uq_ass_schid,
        st.suggestions,
        st.status,
        st.created_date,
        st.suggested_by,
        st.acknowledge_by,
        st.action_required,
        st.notify_management,
        st.assign_responsibility,
        st.tentative_timeline,
        st.suggested_documents,
        st.action_priority,
        st.remarks,
        st.TrackerID,
        st.PO_remarks,
        st.management_remarks,
        st.completed_date,
        sa.startDate,
        sa.endDate,
        sa.userid,
        sa.DocTypeID,
        sa.Doc_CategoryID,
        sa.Doc_SubCategoryID,
        ab.Type_id,
        ab.SubType_id,
ab.ass_template_id,
        ab.ass_template_id AS AssessmentId,
        ab.assessment_name AS assessmentName,
        em.Entity_Master_Name AS CompanyName,
        ULM.Unit_location_Master_name AS locationName,
        tuass.firstname AS AssessorName,
        tu.firstname AS MitigationReqBy,
        dt.DocTypeName AS DocTypeName,
        dc.Doc_CategoryName AS DocCatName,
        dsc.Doc_SubCategoryName AS DocSubCatName,
        tp.Type_Name AS AssessType,
        subtp.SubType_Name AS Subtypename,
       cs.CompletedStatusCount,
          tc.TotalTrackerCount AS TrackerIDCount,
        ROW_NUMBER() OVER (PARTITION BY st.TrackerID ORDER BY st.created_date DESC) AS rn
    FROM 
        suggestions_tbl st
 inner JOIN mitigations m ON m.mitigations_id = st.mitigations_id
    inner JOIN schedule_assessment sa ON sa.uq_ass_schid = m.uq_ass_schid
    LEFT JOIN assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id and ab.verson_no = sa.verson_no
    LEFT OUTER JOIN 
        entity_master em ON em.Entity_Master_id = sa.Entity_Master_id
    LEFT OUTER JOIN 
        unit_location_master ULM ON ULM.Unit_location_Master_id = sa.Unit_location_Master_id
    LEFT OUTER JOIN 
        tbluser tu ON tu.USR_ID = st.acknowledge_by
    LEFT OUTER JOIN 
        doctype_master dt ON dt.DocTypeID = sa.DocTypeID
    LEFT OUTER JOIN 
        doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
    LEFT OUTER JOIN 
        docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
    LEFT OUTER JOIN 
        type tp ON tp.Type_id = ab.Type_id
    LEFT OUTER JOIN 
        sub_type subtp ON subtp.SubType_id = ab.SubType_id
    LEFT OUTER JOIN 
        tbluser tuass ON tuass.USR_ID = sa.login_userid
 LEFT JOIN (
            SELECT TrackerID, COUNT(*) AS CompletedStatusCount
            FROM suggestions_tbl
            WHERE status = 'completed'   
            GROUP BY TrackerID
        ) cs ON cs.TrackerID = st.TrackerID
       
        LEFT JOIN (
            SELECT TrackerID, COUNT(*) AS TotalTrackerCount
            FROM suggestions_tbl where  suggestions_tbl.action_required !='2'  
            GROUP BY TrackerID
        ) tc ON tc.TrackerID = st.TrackerID 
    WHERE 
        (st.TrackerID IS NOT NULL or st.TrackerID IS  NULL)  AND (st.status  ='Commented' or st. status = 'completed' OR st. status = 'Processing' OR  st.status = 'Assigned' ) 
)
SELECT 
    suggestions_id,
    mitigations_id,
    suggestions,
uq_ass_schid,
    status,
    created_date,
    suggested_by,
    acknowledge_by,
    action_required,
    notify_management,
    assign_responsibility,
    tentative_timeline,
    suggested_documents,
    action_priority,
    remarks,
    TrackerID,
    PO_remarks,
    management_remarks,
    completed_date,
    startDate,
    endDate,
    userid,
    DocTypeID,
    Doc_CategoryID,
    Doc_SubCategoryID,
    Type_id,
    SubType_id,
    CompanyName,
    locationName,
    AssessorName,
    MitigationReqBy,
    DocTypeName,
    DocCatName,
    DocSubCatName,
    AssessType,
    Subtypename,
    TrackerIDCount,
    CompletedStatusCount,
    AssessmentId,
ass_template_id,
    assessmentName
FROM 
    ranked_suggestions
WHERE 
rn = 1 and 
  (status  ='Commented' OR status = 'completed' OR status = 'Processing' OR status = 'Assigned')";

            string DateQuery = dateModel.datetype switch
            {
                "CompletionDate" => "DATE(endDate) <= @Today AND DATE(endDate) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid Date Type")
            };

            string finalQuery = string.Format(query, DateQuery);

            using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
            {
                con.Open();
                using (var cmd = new MySqlCommand(finalQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Today", formattedToday);
                    cmd.Parameters.AddWithValue("@MonthAgo", formattedMonthAgo);
                    //cmd.Parameters.AddWithValue("@userid", dateModel.userid);

                    using (var ad = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        ad.Fill(dt);
                        try
                        {
                            if (dt.Rows.Count > 0)
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    pdata.Add(new MitigationDataModel
                                    {
                                        assessment_id = row["ass_template_id"] != DBNull.Value ? row["ass_template_id"].ToString() : string.Empty,
                                        assessment_name = row["assessmentName"] != DBNull.Value ? row["assessmentName"].ToString() : string.Empty,
                                        TrackerID = row["TrackerID"] != DBNull.Value ? row["TrackerID"].ToString() : string.Empty,
                                        mitigationcreateddate = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                        mitigation_requestedby = row["MitigationReqBy"] != DBNull.Value ? row["MitigationReqBy"].ToString() : string.Empty,
                                        num_of_Tasks = row["TrackerIDCount"] != DBNull.Value ? row["TrackerIDCount"].ToString() : "0",
                                        num_of_tasks_completed = row["CompletedStatusCount"] != DBNull.Value ? row["CompletedStatusCount"].ToString() : "0",
                                        status = row["status"] != DBNull.Value ? row["status"].ToString() : string.Empty,
                                        uq_ass_schid = row["uq_ass_schid"] != DBNull.Value ? row["uq_ass_schid"].ToString() : string.Empty,
                                        assessment_startDate = row["startDate"] != DBNull.Value ? Convert.ToDateTime(row["startDate"]) : DateTime.MinValue,
                                        assesssment_endDate = row["endDate"] != DBNull.Value ? Convert.ToDateTime(row["endDate"]) : DateTime.MinValue,
                                        mitigations_id = Convert.ToInt32(row["mitigations_id"]),
                                        person_requesting_assess = row["MitigationReqBy"] != DBNull.Value ? row["MitigationReqBy"].ToString() : string.Empty,
                                        doctype = row["DocTypeName"] != DBNull.Value ? row["DocTypeName"].ToString() : string.Empty,
                                        docCategory = row["DocCatName"] != DBNull.Value ? row["DocCatName"].ToString() : string.Empty,
                                        document_subCategor = row["DocSubCatName"] != DBNull.Value ? row["DocSubCatName"].ToString() : string.Empty,
                                        assess_type = row["AssessType"] != DBNull.Value ? row["AssessType"].ToString() : string.Empty,
                                        assess_subType = row["Subtypename"] != DBNull.Value ? row["Subtypename"].ToString() : string.Empty,
                                        name_of_assessor = row["AssessorName"] != DBNull.Value ? row["AssessorName"].ToString() : string.Empty,

                                        //overal_score_award = row["name_of_assessor"].ToString(),
                                        //key_result = row["key_result"].ToString(),

                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle data processing error
                            throw new Exception("Error processing data from the database", ex);
                        }
                    }
                }
            }
            return pdata;
        }



        //Mitigatio Task list crc


        [Route("api/GovControlReportsContoller/MitigationTaskListcrc/{mitId}")]
        [HttpGet]

        public IEnumerable<MitigationDataModel> MitigationTaskListcrc(DateModel dateModel, int mitId)
        {
            var pdata = new List<MitigationDataModel>();
            DateTime todayDate, monthAgoDate;

            try
            {
                // Parse dates safely
                todayDate = DateTime.ParseExact(dateModel.today, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                monthAgoDate = DateTime.ParseExact(dateModel.oneMonthAgo, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid date format", ex);
            }

            string query = @"
WITH ranked_suggestions AS (
    SELECT 
        st.suggestions_id,
        st.mitigations_id,
        st.suggestions,
        st.status,
        st.created_date,
        st.suggested_by,
        st.acknowledge_by,
        st.action_required,
        st.notify_management,
        st.assign_responsibility,
        st.tentative_timeline,
        st.suggested_documents,
        st.action_priority,
        st.remarks,
        st.TrackerID,
        st.PO_remarks,
        st.management_remarks,
        st.completed_date,
        sa.startDate,
        sa.endDate,
        sa.userid,
        sa.DocTypeID,
        sa.Doc_CategoryID,
        sa.Doc_SubCategoryID,
        sa.uq_ass_schid,
        ab.Type_id,
        ab.SubType_id,
        asscomp.EndDateTime as AssCompletionDate,
        ab.ass_template_id AS AssessmentId,
        ab.assessment_name AS assessmentName,
        em.Entity_Master_Name AS CompanyName,
        ULM.Unit_location_Master_name AS locationName,
        tuass.firstname AS AssessorName,
        tu21.emailid AS EmailId,
        tu.firstname AS MitigationReqBy,
 tu21.firstname AS assign_responsibility1,
        dt.DocTypeName AS DocTypeName,
        dc.Doc_CategoryName AS DocCatName,
        dsc.Doc_SubCategoryName AS DocSubCatName,
        tp.Type_Name AS AssessType,
        subtp.SubType_Name AS Subtypename,
 ROW_NUMBER() OVER (PARTITION BY st.suggestions_id ORDER BY st.created_date DESC) AS rn
    FROM 
        suggestions_tbl st
 inner JOIN mitigations m ON m.mitigations_id = st.mitigations_id
    inner JOIN schedule_assessment sa ON sa.uq_ass_schid = m.uq_ass_schid
    LEFT JOIN assessment_builder_versions ab ON ab.ass_template_id = sa.ass_template_id and ab.verson_no = sa.verson_no
    LEFT JOIN entity_master em ON em.Entity_Master_id = sa.Entity_Master_id
    LEFT JOIN unit_location_master ULM ON ULM.Unit_location_Master_id = sa.Unit_location_Master_id
    LEFT JOIN tbluser tu ON tu.USR_ID = st.acknowledge_by
    LEFT JOIN tbluser tu21 ON tu21.USR_ID = st.assign_responsibility
    LEFT JOIN doctype_master dt ON dt.DocTypeID = sa.DocTypeID
    LEFT JOIN doccategory_master dc ON dc.Doc_CategoryID = sa.Doc_CategoryID
    LEFT JOIN docsubcategory_master dsc ON dsc.Doc_SubCategoryID = sa.Doc_SubCategoryID
    LEFT JOIN type tp ON tp.Type_id = ab.Type_id
    LEFT JOIN sub_type subtp ON subtp.SubType_id = ab.SubType_id
    LEFT JOIN tbluser tuass ON tuass.USR_ID = sa.userid
    LEFT JOIN scheduled_ass_status asscomp ON asscomp.uq_ass_schid = sa.uq_ass_schid
    WHERE st.mitigations_id = @id  

)
SELECT 
    * 
FROM ranked_suggestions
WHERE  rn = 1  and  (status = 'completed' OR status = 'Processing' OR status = 'Assigned');";

            string dateCondition = dateModel.datetype switch
            {
                "CompletionDate" => "DATE(endDate) <= @Today AND DATE(endDate) >= @MonthAgo",
                _ => throw new ArgumentException("Invalid Date Type")
            };

            string finalQuery = string.Format(query, dateCondition);

            try
            {
                using (var con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                using (var cmd = new MySqlCommand(finalQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Today", todayDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@MonthAgo", monthAgoDate.ToString("yyyy-MM-dd"));
                   // cmd.Parameters.AddWithValue("@userid", dateModel.userid);
                    cmd.Parameters.AddWithValue("@id", mitId);

                    using (var adapter = new MySqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            var model = new MitigationDataModel
                            {
                                uq_ass_schid = row["uq_ass_schid"].ToString(),
                                assessment_id = row["AssessmentId"]?.ToString(),
                                assessment_name = row["assessmentName"]?.ToString(),
                                TrackerID = row["TrackerID"]?.ToString(),
                                mitigationcreateddate = row["created_date"] != DBNull.Value ? Convert.ToDateTime(row["created_date"]) : (DateTime?)null,
                                company_name = row["CompanyName"]?.ToString(),
                                locationName = row["locationName"]?.ToString(),
                                person_requesting_assess = row["assign_responsibility1"]?.ToString(),
                                asigneeEmailId = row["EmailId"]?.ToString(),
                                suggestions_id = Convert.ToInt32(row["suggestions_id"]),
                                suggestions = row["suggestions"]?.ToString(),
                                mitigation_requestedby = row["MitigationReqBy"]?.ToString(),//
                                notify_management = Convert.ToInt32(row["notify_management"]) == 1 ? "Yes" : "No",
                                AssCompletionDate = row["AssCompletionDate"] != DBNull.Value ? Convert.ToDateTime(row["AssCompletionDate"]) : DateTime.MinValue,
                                assessment_startDate = row["tentative_timeline"] != DBNull.Value? (DateTime?)Convert.ToDateTime(row["tentative_timeline"]): null,
                                assesssment_endDate = row["completed_date"] != DBNull.Value? (DateTime?)Convert.ToDateTime(row["completed_date"]): null,
                                status = row["status"]?.ToString()
                            };

                            pdata.Add(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and/or rethrow as needed
                throw new Exception("Error while retrieving mitigation task data", ex);
            }

            return pdata;
        }






    }
}
