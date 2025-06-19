using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using MySqlConnector;
using DocumentFormat.OpenXml.Bibliography;
using iText.StyledXmlParser.Jsoup.Select;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DocumentmasterController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public DocumentmasterController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        //Getting Log Details

        [Route("api/Documentmaster/GetDocumentmaster")]
        [HttpGet]

        public IEnumerable<DocumentmasterModel> GetDocumentmaster()
        {
            return this.mySqlDBContext.DocumentmasterModels.ToList();

        }

        [Route("api/Documentmaster/GetDepositoryCount")]
        [HttpGet]
        public IEnumerable<DepositorycountModel> GetDepositoryCount([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select count(AddDoc_id) AS SummaryCount from risk.add_doc where addDoc_Status='Active'", con);
            //MySqlCommand cmd = new MySqlCommand(@"select count(AddDoc_id) AS SummaryCount from risk.add_doc 
            //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
            //AND Initial_creation_doc_date <= @ToDate)", con);

            //cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            //cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));

            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DepositorycountModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DepositorycountModel
                    {
                        SummaryCount = Convert.ToInt32(dt.Rows[i]["SummaryCount"].ToString())
                    });
                }
            }
            return pdata;
        }


        //[Route("api/Documentmaster/GetDepositoryCount")]
        //[HttpGet]
        //public IEnumerable<DepositorycountModel> GetDepositoryCount()
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(@"
        //    select count(AddDoc_id) AS SummaryCount from risk.add_doc WHERE Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY", con);

        //    cmd.CommandType = CommandType.Text;
        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
        //    con.Close();
        //    var pdata = new List<DepositorycountModel>();
        //    if (dt.Rows.Count > 0)
        //    {
        //        for (var i = 0; i < dt.Rows.Count; i++)
        //        {
        //            pdata.Add(new DepositorycountModel
        //            {
        //                SummaryCount = Convert.ToInt32(dt.Rows[i]["SummaryCount"].ToString())
        //            });
        //        }

        //    }
        //    return pdata;
        //}
        [Route("api/Documentmaster/GetDraftStatus")]
        [HttpGet]
        public IEnumerable<DraftStatuscountModel> GetDraftStatus([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT Draft_Status,COUNT(*) AS Status_Count FROM risk.add_doc 
            WHERE addDoc_Status='Active' GROUP BY Draft_Status", con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT Draft_Status,COUNT(*) AS Status_Count FROM risk.add_doc 
            //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
            //AND Initial_creation_doc_date <= @ToDate) GROUP BY Draft_Status", con);
            //cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            //cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));

            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DraftStatuscountModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DraftStatuscountModel
                    {
                        Status_Count = Convert.ToInt32(dt.Rows[i]["Status_Count"].ToString()),
                        Draft_Status = (dt.Rows[i]["Draft_Status"].ToString())
                    });
                }
            }
            return pdata;
        }


        //[Route("api/Documentmaster/GetDraftStatus")]
        //[HttpGet]
        //public IEnumerable<DraftStatuscountModel> GetDraftStatus()
        
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(@"
        //    SELECT Draft_Status,COUNT(*) AS Status_Count FROM risk.add_doc WHERE Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY GROUP BY Draft_Status", con);

        //    cmd.CommandType = CommandType.Text;
        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
        //    con.Close();
        //    var pdata = new List<DraftStatuscountModel>();
        //    if (dt.Rows.Count > 0)
        //    {
        //        for (var i = 0; i < dt.Rows.Count; i++)
        //        {
        //            pdata.Add(new DraftStatuscountModel
        //            {
        //                Status_Count = Convert.ToInt32(dt.Rows[i]["Status_Count"].ToString()),
        //                Draft_Status = (dt.Rows[i]["Draft_Status"].ToString())
        //            });
        //        }

        //    }
        //    return pdata;
        //}

        [Route("api/Documentmaster/GetNewlyAddedDoc")]
        [HttpGet]
        public IEnumerable<DepositorycountModel> GetNewlyAddedDoc([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        // public IActionResult GetNewlyAddedDoc([FromBody] DepositorycountModel DocumentmasterModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            DateTime date = DateTime.Now;
            string enddate = date.ToString("yyyy-MM-dd");
            //"2024-08-08";
            DateTime pastDate = date.AddDays(-10);
            string startdate = pastDate.ToString("yyyy-MM-dd");
            //"2024-04-01";
            MySqlCommand cmd = new MySqlCommand(@"SELECT COUNT(AddDoc_id) AS SummaryCount FROM risk.add_doc WHERE addDoc_Status='Active' AND (DATE(DATE(addDoc_createdDate)) >= @FromDate AND DATE(DATE(addDoc_createdDate)) <= @ToDate)", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DepositorycountModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DepositorycountModel
                    {
                        SummaryCount = Convert.ToInt32(dt.Rows[i]["SummaryCount"].ToString())
                        //Draft_Status = (dt.Rows[i]["Draft_Status"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDisaleddoc")]
        [HttpGet]
        public IEnumerable<DisaledcountModel> GetDisaleddoc([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select count(AddDoc_id) AS SummaryCount from risk.add_doc 
            where (addDoc_Status='Disabled') AND (DATE(DATE(addDoc_createdDate)) >= @FromDate AND DATE(DATE(addDoc_createdDate)) <= @ToDate) GROUP BY Draft_Status", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DisaledcountModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DisaledcountModel
                    {
                        SummaryCount = Convert.ToInt32(dt.Rows[i]["SummaryCount"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetAllPublishedDoc")]
        [HttpGet]
        public IEnumerable<AllPublishedDoc> GetAllPublishedDoc([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)

        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            select AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='Active' AND  (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)";
                //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate  (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
                //AND Initial_creation_doc_date <= @ToDate)";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='Active' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='Active' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)";
            }


            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select AddDoc_id,Title_Doc,Doc_Confidentiality,Eff_Date,Initial_creation_doc_date,Doc_process_Owner,Doc_Approver FROM risk.add_doc 
            //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
            //AND Initial_creation_doc_date <= @ToDate)", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AllPublishedDoc>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AllPublishedDoc
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        Title_Doc = (dt.Rows[i]["Title_Doc"].ToString()),
                        Doc_Confidentiality = (dt.Rows[i]["Doc_Confidentiality"].ToString()),
                        Eff_Date = (dt.Rows[i]["Eff_Date"].ToString()),
                        Initial_creation_doc_date = (dt.Rows[i]["Initial_creation_doc_date"].ToString()),
                        Doc_process_Owner = (dt.Rows[i]["Doc_process_Owner"].ToString()),
                        Doc_Approver = (dt.Rows[i]["Doc_Approver"].ToString()),
                        DocTypeName = (dt.Rows[i]["DocTypeName"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString()),
                        Document_Version = (dt.Rows[i]["Document_Version"].ToString()),
                        addDoc_createdDate= (dt.Rows[i]["addDoc_createdDate"].ToString()),
                        doc_Classification = (dt.Rows[i]["doc_Classification"].ToString()),
                        name_of_Authority = (dt.Rows[i]["name_of_Authority"].ToString()),
                        publish_Authority = (dt.Rows[i]["publish_Authority"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDocumentsSummary")]
        [HttpGet]
        public IEnumerable<DocumentsSummaryModel> GetDocumentsSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
                        SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (
    SELECT 
        r.AddDoc_id,
        r.Initial_creation_doc_date,
        r.addDoc_createdDate,
        r.addDoc_Status,
        CASE 
            WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
            WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
            WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
            ELSE 'No Status'
        END AS ReviewStatusName,
        ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
            CASE 
                WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
                WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
                WHEN rs.minimumdays = 60 THEN 3
                ELSE 4
            END
        ) AS row_num
    FROM risk.add_doc r
    LEFT JOIN risk.reviewstatussettings rs 
        ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
        OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
        OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
WHERE (row_num = 1) AND addDoc_Status='Active'
AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
GROUP BY ReviewStatusName";
                //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
                //AND Initial_creation_doc_date <= @ToDate)";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
                        SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (
    SELECT 
        r.AddDoc_id,
        r.Initial_creation_doc_date,
        r.Eff_Date,
        r.addDoc_Status,
        CASE 
            WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
            WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
            WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
            ELSE 'No Status'
        END AS ReviewStatusName,
        ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
            CASE 
                WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
                WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
                WHEN rs.minimumdays = 60 THEN 3
                ELSE 4
            END
        ) AS row_num
    FROM risk.add_doc r
    LEFT JOIN risk.reviewstatussettings rs 
        ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
        OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
        OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
WHERE (row_num = 1) AND addDoc_Status='Active' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
GROUP BY ReviewStatusName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
                        SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (
    SELECT 
        r.AddDoc_id,
        r.Initial_creation_doc_date,
        r.Draft_Status,
        r.ChangedOn,
        r.addDoc_Status,
        CASE 
            WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
            WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
            WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
            ELSE 'No Status'
        END AS ReviewStatusName,
        ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
            CASE 
                WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
                WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
                WHEN rs.minimumdays = 60 THEN 3
                ELSE 4
            END
        ) AS row_num
    FROM risk.add_doc r
    LEFT JOIN risk.reviewstatussettings rs 
        ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
        OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
        OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
            WHERE (row_num = 1) AND addDoc_Status='Active' AND (Draft_Status='Draft Discarded') AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY ReviewStatusName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (
    SELECT 
        r.AddDoc_id,
        r.Initial_creation_doc_date,
        r.addDoc_Status,
        r.ChangedOn,
        CASE 
            WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
            WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
            WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
            ELSE 'No Status'
        END AS ReviewStatusName,
        ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
            CASE 
                WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
                WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
                WHEN rs.minimumdays = 60 THEN 3
                ELSE 4
            END
        ) AS row_num
    FROM risk.add_doc r
    LEFT JOIN risk.reviewstatussettings rs 
        ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
        OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
        OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
            WHERE (row_num = 1) AND addDoc_Status='Active' AND (addDoc_Status='InActive') AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)
             GROUP BY ReviewStatusName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
                        SELECT ReviewStatusName,COUNT(*) AS status_count
FROM (
    SELECT 
        r.AddDoc_id,
        r.Initial_creation_doc_date,
        tas.createddate,
        r.addDoc_Status,
        CASE 
            WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
            WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
            WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
            ELSE 'No Status'
        END AS ReviewStatusName,
        ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
            CASE 
                WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
                WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
                WHEN rs.minimumdays = 60 THEN 3
                ELSE 4
            END
        ) AS row_num
    FROM 
        risk.add_doc r 
        LEFT JOIN risk.reviewstatussettings rs 
            ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
            OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
            OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
        INNER JOIN risk.doc_taskuseracknowledment_status tas 
            ON r.AddDoc_id = tas.AddDoc_id
) AS subquery
WHERE row_num = 1 AND addDoc_Status='Active' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
GROUP BY ReviewStatusName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //            SELECT ReviewStatusName, COUNT(*) AS status_count
            //FROM (
            //    SELECT 
            //        r.AddDoc_id,
            //        r.Initial_creation_doc_date,
            //        CASE 
            //            WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
            //            WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
            //            WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
            //            ELSE 'No Status'
            //        END AS ReviewStatusName,
            //        ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
            //            CASE 
            //                WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
            //                WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
            //                WHEN rs.minimumdays = 60 THEN 3
            //                ELSE 4
            //            END
            //        ) AS row_num
            //    FROM add_doc r
            //    LEFT JOIN reviewstatussettings rs 
            //        ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
            //        OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
            //        OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
            //) AS subquery
            //WHERE (row_num = 1)
            //AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL 30 DAY)
            //OR (Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
            //GROUP BY ReviewStatusName", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocumentsSummaryModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocumentsSummaryModel
                    {
                        Status_Count = Convert.ToInt32(dt.Rows[i]["Status_Count"].ToString()),
                        ReviewStatusName = (dt.Rows[i]["ReviewStatusName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDocTypeData")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetDocTypeData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
             select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
             inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
             Where addDoc_Status='Active' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate) group by add_doc.DocTypeID";
                //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
                //AND Initial_creation_doc_date <= @ToDate)";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            WHERE addDoc_Status='Active' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) group by add_doc.DocTypeID";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) group by add_doc.DocTypeID";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            WHERE addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) group by add_doc.DocTypeID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
             select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
             inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
             inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
             Where addDoc_Status='Active' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
             group by add_doc.DocTypeID";
            }


            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            //inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            //Where (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate)
            //group by add_doc.DocTypeID", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDocCategoryData")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetDocCategoryData([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate,[FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            where addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_CategoryID";
                //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
                //AND Initial_creation_doc_date <= @ToDate)";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND (Draft_Status='Draft Discarded') AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE (doccategory_master.DocTypeID = @DocTypeID) AND (addDoc_Status='InActive') AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            group by add_doc.Doc_CategoryID";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            //inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            //where(doccategory_master.DocTypeID = @DocTypeID) AND((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY)
            //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
            //group by add_doc.Doc_CategoryID", con);
            
            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDocSubCategoryData")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetDocSubCategoryData([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (Draft_Status='Draft Discarded') AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (addDoc_Status='InActive') AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            group by add_doc.Doc_SubCategoryID";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
           // MySqlCommand cmd = new MySqlCommand(@"
           //select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           //inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           //Where (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) 
           //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
           //group by add_doc.Doc_SubCategoryID", con);

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDocConfidentiality")]
        [HttpGet]
        public IEnumerable<DocConfidentialityModel> GetDocConfidentiality([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            SELECT COALESCE(Doc_Confidentiality, 'NULL') AS Doc_Confidentiality,COUNT(*) AS count FROM risk.add_doc
            WHERE addDoc_Status='Active' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate) 
            GROUP BY COALESCE(Doc_Confidentiality, 'NULL')";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select COALESCE(Doc_Confidentiality, 'NULL') AS Doc_Confidentiality,COUNT(*) AS count from risk.add_doc
            WHERE addDoc_Status='Active' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            GROUP BY COALESCE(Doc_Confidentiality, 'NULL')";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select COALESCE(Doc_Confidentiality, 'NULL') AS Doc_Confidentiality,COUNT(*) AS count from risk.add_doc
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY COALESCE(Doc_Confidentiality, 'NULL')";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select COALESCE(Doc_Confidentiality, 'NULL') AS Doc_Confidentiality,COUNT(*) AS count from risk.add_doc
            WHERE addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY COALESCE(Doc_Confidentiality, 'NULL')";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select COALESCE(Doc_Confidentiality, 'NULL') AS Doc_Confidentiality,COUNT(*) AS count from risk.add_doc
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            GROUP BY COALESCE(Doc_Confidentiality, 'NULL')";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            //AND Initial_creation_doc_date <= @ToDate) group by Doc_Confidentiality", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocConfidentialityModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocConfidentialityModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Doc_Confidentiality = (dt.Rows[i]["Doc_Confidentiality"].ToString())
                    });
                }

            }
            return pdata;
        }
       
        [Route("api/Documentmaster/GetDocAuthoritydata")]
        [HttpGet]
        public IEnumerable<DocAuthorityDataModel> GetDocAuthoritydata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             WHERE addDoc_Status='Active' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
             GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            WHERE addDoc_Status='Active' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            WHERE addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
            // INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            // WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate)
            // GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName;", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocAuthorityDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocAuthorityDataModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        AuthorityTypeName = (dt.Rows[i]["AuthorityTypeName"].ToString()),
                        AuthorityName = (dt.Rows[i]["AuthorityName"].ToString())
                    });
                }

            }
            return pdata;
        }
        
        [Route("api/Documentmaster/GetRecentDocdata")]
        [HttpGet]
        public IEnumerable<RecentDocDataModel> GetRecentDocdata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();

            MySqlCommand cmd = new MySqlCommand(@"SELECT AddDoc_id, Title_Doc,DATE_FORMAT(Initial_creation_doc_date, '%Y-%m-%d') AS Initial_creation_doc_date, addDoc_createdDate FROM risk.add_doc
            WHERE addDoc_Status='Active' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            ORDER BY addDoc_createdDate DESC LIMIT 4", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<RecentDocDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new RecentDocDataModel
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        Title_Doc = (dt.Rows[i]["Title_Doc"].ToString()),
                        Initial_creation_doc_date = (dt.Rows[i]["Initial_creation_doc_date"].ToString()),
                        addDoc_createdDate = (dt.Rows[i]["addDoc_createdDate"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetNatureofDocdata")]
        [HttpGet]
        public IEnumerable<NatureOfDocModel> GetNatureofDocdata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
             SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             WHERE addDoc_Status='Active' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
             group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='Active' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
            // inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            // WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate)
            // group by natureof_doc.NatureOf_Doc_id", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<NatureOfDocModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new NatureOfDocModel
                    {
                        Count = Convert.ToInt32(dt.Rows[i]["Count"].ToString()),
                        NatureOf_Doc_Name = (dt.Rows[i]["NatureOf_Doc_Name"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetReadingTimedata")]
        [HttpGet]
        public IEnumerable<DocReadingTimeModel> GetReadingTimedata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
             SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
             WHERE addDoc_Status='Active' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
             GROUP BY Time_period,indicative_reading_time";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            WHERE addDoc_Status='Active' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            GROUP BY Time_period,indicative_reading_time";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY Time_period,indicative_reading_time";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            WHERE addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY Time_period,indicative_reading_time";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            GROUP BY Time_period,indicative_reading_time";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            // WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate)
            // GROUP BY Time_period,indicative_reading_time", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocReadingTimeModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocReadingTimeModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Time_period = (dt.Rows[i]["Time_period"].ToString()),
                        indicative_reading_time =(dt.Rows[i]["indicative_reading_time"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetSavedDraftDocTypeData")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetSavedDraftDocTypeData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Draft_Status FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            Where addDoc_Status='Active' AND Draft_Status='Incomplete' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.DocTypeID,Draft_Status";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Draft_Status FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE addDoc_Status='Active' AND Draft_Status='Incomplete' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by add_doc.DocTypeID,Draft_Status";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Draft_Status FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE addDoc_Status='Active' AND Draft_Status='Incomplete' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.DocTypeID,Draft_Status";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Draft_Status FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE Draft_Status='Incomplete' AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.DocTypeID,Draft_Status";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Draft_Status FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Incomplete' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by add_doc.DocTypeID,Draft_Status";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Draft_Status FROM risk.add_doc
            //inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            //Where Draft_Status='Completed' AND (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate)
            //group by add_doc.DocTypeID,Draft_Status", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString()),
                        Draft_Status= (dt.Rows[i]["Draft_Status"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetSavedDraftDocCategoryData")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetSavedDraftDocCategoryData([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName,Draft_Status from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            where addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND Draft_Status='Incomplete' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_CategoryID,Draft_Status";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName,Draft_Status from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND Draft_Status='Incomplete' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by add_doc.Doc_CategoryID,Draft_Status";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName,Draft_Status from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND Draft_Status='Incomplete' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_CategoryID,Draft_Status";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName,Draft_Status from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE (doccategory_master.DocTypeID = @DocTypeID) AND Draft_Status='Incomplete' AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_CategoryID,Draft_Status";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName,Draft_Status from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND Draft_Status='Incomplete' AND (DATE(doc_taskuseracknowledment_status).createddate >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by add_doc.Doc_CategoryID,Draft_Status";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName,Draft_Status from risk.add_doc
            //inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            //where (doccategory_master.DocTypeID = @DocTypeID) AND Draft_Status='Completed' AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY)
            //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
            //group by add_doc.Doc_CategoryID,Draft_Status", con);

            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Draft_Status = (dt.Rows[i]["Draft_Status"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetSavedDraftDocSubCategoryData")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetSavedDraftDocSubCategoryData([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName,Draft_Status from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where addDoc_Status='Active' AND Draft_Status='Incomplete' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_SubCategoryID,Draft_Status";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName,Draft_Status from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE addDoc_Status='Active' AND Draft_Status='Incomplete' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID,Draft_Status";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName,Draft_Status from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE addDoc_Status='Active' AND Draft_Status='Incomplete' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID,Draft_Status";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName,Draft_Status from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE Draft_Status='Incomplete' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID,Draft_Status";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName,Draft_Status from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Incomplete' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID,Draft_Status";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
           // MySqlCommand cmd = new MySqlCommand(@"
           //select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName,Draft_Status from risk.add_doc
           //inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           //Where Draft_Status='Completed' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) 
           //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
           //group by add_doc.Doc_SubCategoryID,Draft_Status", con);

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Draft_Status = (dt.Rows[i]["Draft_Status"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }
            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDiscardedDraftDocTypeData")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetDiscardedDraftDocTypeData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Doc_CategoryName,
            count(doccategory_master.Doc_CategoryID) as Doccategory_Count,Doc_SubCategoryName,count(docsubcategory_master.Doc_SubCategoryID) as Docsubcategory_Count FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.DocTypeID,Doc_CategoryName,Doc_SubCategoryName";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Doc_CategoryName,
            count(doccategory_master.Doc_CategoryID) as Doccategory_Count,Doc_SubCategoryName,count(docsubcategory_master.Doc_SubCategoryID) as Docsubcategory_Count FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by add_doc.DocTypeID,Doc_CategoryName,Doc_SubCategoryName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Doc_CategoryName,
            count(doccategory_master.Doc_CategoryID) as Doccategory_Count,Doc_SubCategoryName,count(docsubcategory_master.Doc_SubCategoryID) as Docsubcategory_Count FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID 
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.DocTypeID,Doc_CategoryName,Doc_SubCategoryName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Doc_CategoryName,
            count(doccategory_master.Doc_CategoryID) as Doccategory_Count,Doc_SubCategoryName,count(docsubcategory_master.Doc_SubCategoryID) as Docsubcategory_Count FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE Draft_Status='Draft Discarded' AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.DocTypeID,Doc_CategoryName,Doc_SubCategoryName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Doc_CategoryName,
            count(doccategory_master.Doc_CategoryID) as Doccategory_Count,Doc_SubCategoryName,count(docsubcategory_master.Doc_SubCategoryID) as Docsubcategory_Count FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by add_doc.DocTypeID,Doc_CategoryName,Doc_SubCategoryName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
//            MySqlCommand cmd = new MySqlCommand(@"
//            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName,Doc_CategoryName,
//count(doccategory_master.Doc_CategoryID) as Doccategory_Count,Doc_SubCategoryName,count(docsubcategory_master.Doc_SubCategoryID) as Docsubcategory_Count FROM risk.add_doc
//inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
//inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
//inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
//            Where Draft_Status!='Completed' AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
//            group by add_doc.DocTypeID,Doc_CategoryName,Doc_SubCategoryName", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString()),
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        //[Route("api/Documentmaster/GetDiscardedDraftDocCategoryData")]
        //[HttpGet]
        //public IEnumerable<DocCategoryDataModel> GetDiscardedDraftDocCategoryData([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(@"
        //    select doctype_master.DocTypeName, add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName,Draft_Status from risk.add_doc
        //    inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
        //    inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
        //    where Draft_Status!='Completed' AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY)
        //    OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
        //    group by add_doc.Doc_CategoryID,Draft_Status,doctype_master.DocTypeName", con);

        //    cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
        //    cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
        //    cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
        //    cmd.CommandType = CommandType.Text;
        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
        //    con.Close();
        //    var pdata = new List<DocCategoryDataModel>();
        //    if (dt.Rows.Count > 0)
        //    {
        //        for (var i = 0; i < dt.Rows.Count; i++)
        //        {
        //            pdata.Add(new DocCategoryDataModel
        //            {
        //                Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
        //                DoctypeName = (dt.Rows[i]["DoctypeName"].ToString()),
        //                Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
        //                Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
        //                Draft_Status = (dt.Rows[i]["Draft_Status"].ToString()),
        //                Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
        //            });
        //        }

        //    }
        //    return pdata;
        //}

        //[Route("api/Documentmaster/GetDiscardedDraftDocSubCategoryData")]
        //[HttpGet]
        //public IEnumerable<DocSubcategoryDataModel> GetDiscardedDraftDocSubCategoryData([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(@"
        //   select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName,Draft_Status from risk.add_doc
        //   inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
        //   Where Draft_Status!='Completed' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) 
        //   OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
        //   group by add_doc.Doc_SubCategoryID,Draft_Status", con);

        //    cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
        //    cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
        //    cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
        //    cmd.CommandType = CommandType.Text;
        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
        //    con.Close();
        //    var pdata = new List<DocSubcategoryDataModel>();
        //    if (dt.Rows.Count > 0)
        //    {
        //        for (var i = 0; i < dt.Rows.Count; i++)
        //        {
        //            pdata.Add(new DocSubcategoryDataModel
        //            {
        //                Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
        //                Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
        //                Draft_Status = (dt.Rows[i]["Draft_Status"].ToString()),
        //                Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
        //            });
        //        }
        //    }
        //    return pdata;
        //}

         [Route("api/Documentmaster/GetDisabledDocTypeData")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetDisabledDocTypeData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            Where (addDoc_Status='InActive') AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID  
            WHERE (addDoc_Status='InActive') AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE (addDoc_Status='InActive') AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE (addDoc_Status='InActive') AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE (addDoc_Status='InActive') AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by add_doc.DocTypeID";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            //inner join doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            //Where (addDoc_Status='InActive') AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
            //group by add_doc.DocTypeID", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDisabledDocCategoryData")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetDisabledDocCategoryData([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            where (addDoc_Status='InActive') AND (doccategory_master.DocTypeID = @DocTypeID) AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID 
            WHERE (addDoc_Status='InActive') AND (doccategory_master.DocTypeID = @DocTypeID) AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE (addDoc_Status='InActive') AND (doccategory_master.DocTypeID = @DocTypeID) AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID 
            WHERE (addDoc_Status='InActive') AND (doccategory_master.DocTypeID = @DocTypeID) AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE (addDoc_Status='InActive') AND (doccategory_master.DocTypeID = @DocTypeID) AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by add_doc.Doc_CategoryID";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            //inner join doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            //where (addDoc_Status='InActive') AND (doccategory_master.DocTypeID = @DocTypeID) AND((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY)
            //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
            //group by add_doc.Doc_CategoryID", con);
            
            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDisabledDocSubCategoryData")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetDisabledDocSubCategoryData([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where (addDoc_Status='InActive') AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE (addDoc_Status='InActive') AND (doccategory_master.Doc_CategoryID = @Doc_CategoryID) AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE (addDoc_Status='InActive') AND (doccategory_master.Doc_CategoryID = @Doc_CategoryID) AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID 
            WHERE (addDoc_Status='InActive') AND (doccategory_master.Doc_CategoryID = @Doc_CategoryID) AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE (addDoc_Status='InActive') AND (doccategory_master.Doc_CategoryID = @Doc_CategoryID) AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by add_doc.Doc_SubCategoryID";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
           // MySqlCommand cmd = new MySqlCommand(@"
           //select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           //inner join docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           //Where (addDoc_Status='InActive') AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) 
           //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
           //group by add_doc.Doc_SubCategoryID", con);

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetDisabledDocConfidentiality")]
        [HttpGet]
        public IEnumerable<DocConfidentialityModel> GetDisabledDocConfidentiality([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            WHERE (addDoc_Status='InActive') AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate) 
            group by Doc_Confidentiality";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc  
            WHERE (addDoc_Status='InActive') AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by Doc_Confidentiality";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc
            WHERE (addDoc_Status='InActive') AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by Doc_Confidentiality";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            WHERE (addDoc_Status='InActive') AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by Doc_Confidentiality";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE (addDoc_Status='InActive') AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by Doc_Confidentiality";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            //WHERE (addDoc_Status='InActive') AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            //AND Initial_creation_doc_date <= @ToDate)) group by Doc_Confidentiality", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocConfidentialityModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocConfidentialityModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Doc_Confidentiality = (dt.Rows[i]["Doc_Confidentiality"].ToString())
                    });
                }

            }
            return pdata;
        }
        [Route("api/Documentmaster/GetDisabledDocAuthoritydata")]
        [HttpGet]
        public IEnumerable<DocAuthorityDataModel> GetDisabledDocAuthoritydata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             WHERE (addDoc_Status='InActive') AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
             GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID 
            WHERE (addDoc_Status='InActive') AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            WHERE (addDoc_Status='InActive') AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID 
            WHERE (addDoc_Status='InActive') AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
             INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE (addDoc_Status='InActive') AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM add_doc 
            // INNER JOIN authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            // WHERE (addDoc_Status='InActive') AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate))
            // GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName;", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocAuthorityDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocAuthorityDataModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        AuthorityTypeName = (dt.Rows[i]["AuthorityTypeName"].ToString()),
                        AuthorityName = (dt.Rows[i]["AuthorityName"].ToString())
                    });
                }

            }
            return pdata;
        }
        
        [Route("api/Documentmaster/GetDisabledNatureofDocdata")]
        [HttpGet]
        public IEnumerable<NatureOfDocModel> GetDisabledNatureofDocdata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             WHERE (addDoc_Status='InActive') AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
             group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE (addDoc_Status='InActive') AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE (addDoc_Status='InActive') AND Draft_Status='Draft Discarded' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            WHERE (addDoc_Status='InActive') AND addDoc_Status='InActive' AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE (addDoc_Status='InActive') AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate) 
            group by natureof_doc.NatureOf_Doc_id";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
            // inner join natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            // WHERE (addDoc_Status='InActive') AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate))
            // group by natureof_doc.NatureOf_Doc_id", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<NatureOfDocModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new NatureOfDocModel
                    {
                        Count = Convert.ToInt32(dt.Rows[i]["Count"].ToString()),
                        NatureOf_Doc_Name = (dt.Rows[i]["NatureOf_Doc_Name"].ToString())
                    });
                }

            }
            return pdata;
        }

        //TaskOwner Api
        [Route("api/Documentmaster/GetTaskownerDepositoryCount")]
        [HttpGet]
        public IEnumerable<DepositorycountModel> GetTaskownerDepositoryCount([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select count(doc_taskuseracknowledment_status.AddDoc_id) AS SummaryCount from risk.doc_taskuseracknowledment_status 
            inner join risk.add_doc on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID=@userid ", con);
            //AND ( DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());

            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DepositorycountModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DepositorycountModel
                    {
                        SummaryCount = Convert.ToInt32(dt.Rows[i]["SummaryCount"].ToString())
                    });
                }
            }
            return pdata;
        }
      
        [Route("api/Documentmaster/GetTONewlyAddedDoc")]
        [HttpGet]
        public IEnumerable<DepositorycountModel> GetTONewlyAddedDoc([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid)
        // public IActionResult GetNewlyAddedDoc([FromBody] DepositorycountModel DocumentmasterModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            DateTime date = DateTime.Now;
            string enddate = date.ToString("yyyy-MM-dd");
            //"2024-08-08";
            DateTime pastDate = date.AddDays(-10);
            string startdate = pastDate.ToString("yyyy-MM-dd");
            //"2024-04-01";
            MySqlCommand cmd = new MySqlCommand(@"SELECT COUNT(add_doc.AddDoc_id) AS SummaryCount FROM risk.add_doc 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DepositorycountModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DepositorycountModel
                    {
                        SummaryCount = Convert.ToInt32(dt.Rows[i]["SummaryCount"].ToString())
                        //Draft_Status = (dt.Rows[i]["Draft_Status"].ToString())
                    });
                }

            }
            return pdata;
        }


        [Route("api/Documentmaster/GetTaskownerAllPublishedDoc")]
        [HttpGet]
        public IEnumerable<AllPublishedDoc> GetTaskownerAllPublishedDoc([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            select add_doc.AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(add_doc.Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID=@userid and (( DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))";
                //WHERE (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
                //AND Initial_creation_doc_date <= @ToDate)";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(add_doc.Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID=@userid and (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            select add_doc.AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(add_doc.Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND ack_status='Reading Completed'
            AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.AddDoc_id,DocTypeName,Doc_CategoryName,Doc_SubCategoryName,Title_Doc,CONCAT(add_doc.Document_Id, ' : ', VersionControlNo) AS Document_Version,
            Initial_creation_doc_date,Doc_Confidentiality,Eff_Date,Doc_process_Owner,Doc_Approver,addDoc_createdDate,NatureOf_Doc_Name as doc_Classification,
            authorityname_master.AuthorityName as name_of_Authority,authoritytype_master.AuthorityTypeName As publish_Authority FROM risk.add_doc 
            left join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID
            left join risk.doccategory_master on doccategory_master.Doc_CategoryID=add_doc.Doc_CategoryID
            left join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            left join risk.authorityname_master on authorityname_master.AuthoritynameID=add_doc.AuthoritynameID
            left join risk.authoritytype_master on authoritytype_master.AuthorityTypeID=add_doc.AuthorityTypeID
            left join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID=@userid and (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.AddDoc_id,Title_Doc,Doc_Confidentiality,Eff_Date,Initial_creation_doc_date,Doc_process_Owner,Doc_Approver FROM risk.add_doc 
            //inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            //WHERE doc_taskuseracknowledment_status.USR_ID=@userid and ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR ( Initial_creation_doc_date >= @FromDate 
            //AND Initial_creation_doc_date <= @ToDate))", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AllPublishedDoc>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AllPublishedDoc
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        Title_Doc = (dt.Rows[i]["Title_Doc"].ToString()),
                        Doc_Confidentiality = (dt.Rows[i]["Doc_Confidentiality"].ToString()),
                        Eff_Date = (dt.Rows[i]["Eff_Date"].ToString()),
                        Initial_creation_doc_date = (dt.Rows[i]["Initial_creation_doc_date"].ToString()),
                        Doc_process_Owner = (dt.Rows[i]["Doc_process_Owner"].ToString()),
                        Doc_Approver = (dt.Rows[i]["Doc_Approver"].ToString()),
                        DocTypeName = (dt.Rows[i]["DocTypeName"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString()),
                        Document_Version = (dt.Rows[i]["Document_Version"].ToString()),
                        addDoc_createdDate = (dt.Rows[i]["addDoc_createdDate"].ToString()),
                        doc_Classification = (dt.Rows[i]["doc_Classification"].ToString()),
                        name_of_Authority = (dt.Rows[i]["name_of_Authority"].ToString()),
                        publish_Authority = (dt.Rows[i]["publish_Authority"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocumentsSummary")]
        [HttpGet]
        public IEnumerable<DocumentsSummaryModel> GetTaskownerDocumentsSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
                        SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (SELECT r.AddDoc_id,r.Initial_creation_doc_date,tas.USR_ID,addDoc_createdDate,addDoc_Status,
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
	  WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
	  ELSE 'No Status'
	  END AS ReviewStatusName,
	  ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
	  WHEN rs.minimumdays = 60 THEN 3
	  ELSE 4
	  END) AS row_num
    FROM risk.add_doc r inner JOIN risk.doc_taskuseracknowledment_status tas ON r.AddDoc_id = tas.AddDoc_id 
    LEFT JOIN risk.reviewstatussettings rs ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
	OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
	OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
WHERE (row_num = 1) AND addDoc_Status='Active' AND USR_ID = @userid AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
GROUP BY ReviewStatusName";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
                        SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (SELECT r.AddDoc_id,r.Initial_creation_doc_date,tas.USR_ID,addDoc_createdDate,addDoc_Status,
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
	  WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
	  ELSE 'No Status'
	  END AS ReviewStatusName,
	  ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
	  WHEN rs.minimumdays = 60 THEN 3
	  ELSE 4
	  END) AS row_num
    FROM risk.add_doc r inner JOIN risk.doc_taskuseracknowledment_status tas ON r.AddDoc_id = tas.AddDoc_id 
    LEFT JOIN risk.reviewstatussettings rs ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
	OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
	OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
WHERE (row_num = 1) AND addDoc_Status='Active' AND USR_ID = @userid AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
GROUP BY ReviewStatusName";
            }
            else if (datetype == "Reading Date")
            {
                query = @"SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (SELECT r.AddDoc_id,r.Initial_creation_doc_date,tas.USR_ID,addDoc_createdDate,addDoc_Status,
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
	  WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
	  ELSE 'No Status'
	  END AS ReviewStatusName,
	  ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
	  WHEN rs.minimumdays = 60 THEN 3
	  ELSE 4
	  END) AS row_num
    FROM risk.add_doc r inner JOIN risk.doc_taskuseracknowledment_status tas ON r.AddDoc_id = tas.AddDoc_id 
    LEFT JOIN risk.reviewstatussettings rs ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
	OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
	OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
            WHERE (row_num = 1) AND addDoc_Status='Active' AND (USR_ID = @userid) AND (ack_status='Reading Completed') AND (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate) 
            GROUP BY ReviewStatusName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"SELECT ReviewStatusName, COUNT(*) AS status_count
FROM (SELECT r.AddDoc_id,r.Initial_creation_doc_date,tas.USR_ID,addDoc_createdDate,addDoc_Status,
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
	  WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
	  ELSE 'No Status'
	  END AS ReviewStatusName,
	  ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
	  CASE 
	  WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
	  WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
	  WHEN rs.minimumdays = 60 THEN 3
	  ELSE 4
	  END) AS row_num
    FROM risk.add_doc r inner JOIN risk.doc_taskuseracknowledment_status tas ON r.AddDoc_id = tas.AddDoc_id 
    LEFT JOIN risk.reviewstatussettings rs ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
	OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
	OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
) AS subquery
WHERE (row_num = 1) AND addDoc_Status='Active' AND (USR_ID = @userid) AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
GROUP BY ReviewStatusName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
//            MySqlCommand cmd = new MySqlCommand(@"
//            SELECT ReviewStatusName, COUNT(*) AS status_count
//FROM (
//    SELECT 
//        r.AddDoc_id,
//        r.Initial_creation_doc_date,
//        tas.USR_ID, -- Include tas.USR_ID in the subquery SELECT list
//        CASE 
//            WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0 THEN rs.ReviewStatusName
//            WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN rs.ReviewStatusName
//            WHEN rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0 THEN rs.ReviewStatusName
//            ELSE 'No Status'
//        END AS ReviewStatusName,
//        ROW_NUMBER() OVER (PARTITION BY r.AddDoc_id ORDER BY 
//            CASE 
//                WHEN rs.minimumdays = 0 AND rs.maximumdays = 0 THEN 1
//                WHEN DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays THEN 2
//                WHEN rs.minimumdays = 60 THEN 3
//                ELSE 4
//            END
//        ) AS row_num
//    FROM risk.add_doc r
//    inner JOIN risk.doc_taskuseracknowledment_status tas 
//        ON r.AddDoc_id = tas.AddDoc_id 
//    LEFT JOIN risk.reviewstatussettings rs 
//        ON (rs.minimumdays = 0 AND rs.maximumdays = 0 AND DATEDIFF(CURDATE(), r.review_start_date) > 0)
//        OR (DATEDIFF(CURDATE(), r.review_start_date) BETWEEN rs.minimumdays AND rs.maximumdays)
//        OR (rs.minimumdays = 60 AND DATEDIFF(r.review_start_date, CURDATE()) > 0)
//) AS subquery
//WHERE (row_num = 1) 
//  AND USR_ID = @userid
//  AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL 30 DAY)
//  OR (Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
//GROUP BY ReviewStatusName;
//", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocumentsSummaryModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocumentsSummaryModel
                    {
                        Status_Count = Convert.ToInt32(dt.Rows[i]["Status_Count"].ToString()),
                        ReviewStatusName = (dt.Rows[i]["ReviewStatusName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocTypeData")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetTaskownerDocTypeData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            SELECT add_doc.DocTypeID, 
                   COUNT(add_doc.DocTypeID) AS Doctype_Count,
                   doctype_master.DocTypeName 
            FROM risk.add_doc
            INNER JOIN risk.doctype_master 
                ON doctype_master.DocTypeID = add_doc.DocTypeID
            LEFT JOIN risk.doc_taskuseracknowledment_status 
                ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid 
              AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            GROUP BY add_doc.DocTypeID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT add_doc.DocTypeID, 
                   COUNT(add_doc.DocTypeID) AS Doctype_Count,
                   doctype_master.DocTypeName 
            FROM risk.add_doc
            INNER JOIN risk.doctype_master 
                ON doctype_master.DocTypeID = add_doc.DocTypeID
            LEFT JOIN risk.doc_taskuseracknowledment_status 
                ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid 
              AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            GROUP BY add_doc.DocTypeID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT add_doc.DocTypeID, 
                   COUNT(add_doc.DocTypeID) AS Doctype_Count,
                   doctype_master.DocTypeName 
            FROM risk.add_doc
            INNER JOIN risk.doctype_master 
                ON doctype_master.DocTypeID = add_doc.DocTypeID
            LEFT JOIN risk.doc_taskuseracknowledment_status 
                ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid 
              AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            GROUP BY add_doc.DocTypeID";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            SELECT add_doc.DocTypeID, 
                   COUNT(add_doc.DocTypeID) AS Doctype_Count,
                   doctype_master.DocTypeName 
            FROM risk.add_doc
            INNER JOIN risk.doctype_master 
                ON doctype_master.DocTypeID = add_doc.DocTypeID
            LEFT JOIN risk.doc_taskuseracknowledment_status 
                ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND ack_status='Reading Completed'
              AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)
            GROUP BY add_doc.DocTypeID";
            }

            MySqlCommand cmd = new MySqlCommand(query, con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocCategoryData")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetTaskownerDocCategoryData([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (doccategory_master.DocTypeID = @DocTypeID) AND(DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid and (doccategory_master.DocTypeID = @DocTypeID)
            AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            GROUP BY add_doc.Doc_CategoryID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid and (doccategory_master.DocTypeID = @DocTypeID)
            AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            GROUP BY add_doc.Doc_CategoryID";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid and (doccategory_master.DocTypeID = @DocTypeID) AND (ack_status='Reading Completed')
            AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)
            GROUP BY add_doc.Doc_CategoryID";
            }

            MySqlCommand cmd = new MySqlCommand(query, con);
//            MySqlCommand cmd = new MySqlCommand(@"
//            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
//inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
//left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
//where (doc_taskuseracknowledment_status.USR_ID=@userid) and (doccategory_master.DocTypeID = @DocTypeID) AND((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY)
//OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
//group by add_doc.Doc_CategoryID", con);

            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocSubCategoryData")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetTaskownerDocSubCategoryData([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           Where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND(DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           Where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID)
            AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            GROUP BY add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           Where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID)
            AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            GROUP BY add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           Where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND (ack_status='Reading Completed')
            AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)
            GROUP BY add_doc.Doc_SubCategoryID";
            }

            MySqlCommand cmd = new MySqlCommand(query, con);
           // MySqlCommand cmd = new MySqlCommand(@"
           //select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           //inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           //left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           //Where (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) 
           //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
           //group by add_doc.Doc_SubCategoryID", con);

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTODocConfidentiality")]
        [HttpGet]
        public IEnumerable<DocConfidentialityModel> GetTODocConfidentiality([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            group by Doc_Confidentiality";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            GROUP BY Doc_Confidentiality";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid 
            AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            GROUP BY Doc_Confidentiality";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND ack_status='Reading Completed'
            AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)
            GROUP BY Doc_Confidentiality";
            }

            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            //left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            //WHERE (doc_taskuseracknowledment_status.USR_ID=@userid) and ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            //AND Initial_creation_doc_date <= @ToDate)) group by Doc_Confidentiality", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocConfidentialityModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocConfidentialityModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Doc_Confidentiality = (dt.Rows[i]["Doc_Confidentiality"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTODocAuthoritydata")]
        [HttpGet]
        public IEnumerable<DocAuthorityDataModel> GetTODocAuthoritydata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
             INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
             WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) AND (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
             GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
             INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
             INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid 
            AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
             INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND ack_status='Reading Completed'
            AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)
            GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            }

            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
            // INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            // left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            // WHERE (doc_taskuseracknowledment_status.USR_ID=@userid) AND ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate))
            // GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocAuthorityDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocAuthorityDataModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        AuthorityTypeName = (dt.Rows[i]["AuthorityTypeName"].ToString()),
                        AuthorityName = (dt.Rows[i]["AuthorityName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTONatureofDocdata")]
        [HttpGet]
        public IEnumerable<NatureOfDocModel> GetTONatureofDocdata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
             WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (addDoc_createdDate >= @FromDate
             AND addDoc_createdDate <= @ToDate) group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid 
            AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            group by natureof_doc.NatureOf_Doc_id";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND ack_status='Reading Completed'
            AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)
            group by natureof_doc.NatureOf_Doc_id";
            }

            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
            // inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            // left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            // WHERE (doc_taskuseracknowledment_status.USR_ID=@userid) and ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate)) group by natureof_doc.NatureOf_Doc_id", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<NatureOfDocModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new NatureOfDocModel
                    {
                        Count = Convert.ToInt32(dt.Rows[i]["Count"].ToString()),
                        NatureOf_Doc_Name = (dt.Rows[i]["NatureOf_Doc_Name"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTOReadingTimedata")]
        [HttpGet]
        public IEnumerable<DocReadingTimeModel> GetTOReadingTimedata([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
             WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate) 
             GROUP BY Time_period,indicative_reading_time";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate)
            GROUP BY Time_period,indicative_reading_time";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid 
            AND (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            GROUP BY Time_period,indicative_reading_time";
            }
            else if (datetype == "Reading Date")
            {
                query = @"
            SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID = @userid AND ack_status='Reading Completed'
            AND (DATE(doc_taskuseracknowledment_status.readComplete_date) >= @FromDate AND DATE(doc_taskuseracknowledment_status.readComplete_date) <= @ToDate)
            GROUP BY Time_period,indicative_reading_time";
            }

            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            // left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            // WHERE (doc_taskuseracknowledment_status.USR_ID=@userid) and ((Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate)) GROUP BY Time_period,indicative_reading_time", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocReadingTimeModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocReadingTimeModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Time_period = (dt.Rows[i]["Time_period"].ToString()),
                        indicative_reading_time = (dt.Rows[i]["indicative_reading_time"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTOAckReqSummary")]
        [HttpGet]
        public IEnumerable<AckReqSummaryModel> GetTOAckReqSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT 'Total Requested' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) AND (DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY)
            UNION ALL
            SELECT 'Total Acknowledged' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND ack_status = 'true' AND (doc_taskuseracknowledment_status.USR_ID=@userid) AND (DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY)", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AckReqSummaryModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AckReqSummaryModel
                    {
                        Acknowledment = (dt.Rows[i]["Acknowledgment"].ToString()),
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString())
                        //total_acknowledged = (dt.Rows[i]["total_acknowledged"].ToString()),
                        //total_requested = (dt.Rows[i]["total_requested"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocTypeDataAckreq")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetTaskownerDocTypeDataAckreq([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            Where addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID=@userid 
            group by add_doc.DocTypeID", con);
            //and (addDoc_createdDate >= CURRENT_DATE - INTERVAL '30' DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocCategoryDataAckreq")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetTaskownerDocCategoryDataAckreq([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (doccategory_master.DocTypeID = @DocTypeID)
group by add_doc.Doc_CategoryID", con);
            // AND (addDoc_createdDate >= @FromDate AND addDoc_createdDate <= @ToDate)

            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocSubCategoryDataAckreq")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetTaskownerDocSubCategoryDataAckreq([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
           select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           Where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID)
           group by add_doc.Doc_SubCategoryID", con);
            // AND (addDoc_createdDate >= @FromDate AND addDoc_createdDate <= @ToDate)

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }
        //Newly Added Documents for ContentController
        [Route("api/Documentmaster/GetContentManagerDocTypeDataNew")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetContentManagerDocTypeDataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            Where addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.DocTypeID";
            // OR (addDoc_createdDate >= @FromDate AND addDoc_createdDate <= @ToDate)
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.DocTypeID";
                // OR (Eff_Date >= @FromDate AND Eff_Date <= @ToDate)
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID  
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.DocTypeID";
                // OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE addDoc_Status='InActive' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.DocTypeID";
                // OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate)
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.DocTypeID";
                // OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate)
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            //inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            //Where ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY))
            //group by add_doc.DocTypeID", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@days", days);
            //cmd.Parameters.AddWithValue("@datetype", datetype);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString())
                        //Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString()),
                        //Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        //Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        //Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }
        [Route("api/Documentmaster/GetContentManagerDocCategoryDataNew")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetContentManagerDocCategoryDataNew([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            Where addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            WHERE addDoc_Status='InActive' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            //inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            //left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            //where (doc_taskuseracknowledment_status.USR_ID=@userid) and (doccategory_master.DocTypeID = @DocTypeID) AND((Initial_creation_doc_date >= CURDATE() - INTERVAL @days DAY) OR (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY)
            //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
            //group by add_doc.Doc_CategoryID", con);

            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            //cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetContentManagerDocSubCategoryDataNew")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetContentManagerDocSubCategoryDataNew([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string datetype, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            Where addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            WHERE addDoc_Status='InActive' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
           // MySqlCommand cmd = new MySqlCommand(@"
           //select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           //inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           //left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           //Where (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((Initial_creation_doc_date >= CURDATE() - INTERVAL @days DAY) OR (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) 
           //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
           //group by add_doc.Doc_SubCategoryID", con);

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            //cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetCMDocConfidentialityNew")]
        [HttpGet]
        public IEnumerable<DocConfidentialityModel> GetCMDocConfidentialityNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //string query = "";
            //if (datetype == "Publishing Date")
            //{
            //    query = @"select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            //WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (addDoc_createdDate >= @FromDate AND addDoc_createdDate <= @ToDate)) 
            //group by Doc_Confidentiality";
            //}
            //else if (datetype == "Effective Date")
            //{
            //    query = @"
            //select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            //WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            //group by Doc_Confidentiality";
            //}
            //else if (datetype == "Discard Date")
            //{
            //    query = @"
            //select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            //WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //group by Doc_Confidentiality";
            //}
            //else if (datetype == "Disable Date")
            //{
            //    query = @"
            //select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc _doc
            //WHERE addDoc_Status='InActive' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //group by Doc_Confidentiality";
            //}
            //else if (datetype == "Acknowledgement Date")
            //{
            //    query = @"
            //select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            //inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            //WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            //group by Doc_Confidentiality";
            //}
            //MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlCommand cmd = new MySqlCommand(@"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY)) 
            group by Doc_Confidentiality", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocConfidentialityModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocConfidentialityModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Doc_Confidentiality = (dt.Rows[i]["Doc_Confidentiality"].ToString())
                    });
                }

            }
            return pdata;
        }
        
        [Route("api/Documentmaster/GetCMDocAuthoritydataNew")]
        [HttpGet]
        public IEnumerable<DocAuthorityDataModel> GetCMDocAuthoritydataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //string query = "";
            //if (datetype == "Publishing Date")
            //{
            //    query = @"SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
            // INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            // WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (addDoc_createdDate >= @FromDate AND addDoc_createdDate <= @ToDate))
            // GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            //}
            //else if (datetype == "Effective Date")
            //{
            //    query = @"
            //SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
            // INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            //WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            //group by at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            //}
            //else if (datetype == "Discard Date")
            //{
            //    query = @"
            //SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
            // INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            //WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //group by at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            //}
            //else if (datetype == "Disable Date")
            //{
            //    query = @"
            //SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
            // INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            //WHERE addDoc_Status='InActive' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //group by at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            //}
            //else if (datetype == "Acknowledgement Date")
            //{
            //    query = @"
            //SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
            // INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
            // INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
            //inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            //WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            //group by at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName";
            //}
            //MySqlCommand cmd = new MySqlCommand(query, con);
            
            MySqlCommand cmd = new MySqlCommand(@"SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc
             INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             WHERE addDoc_Status = 'Active' AND((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY))
             GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocAuthorityDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocAuthorityDataModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        AuthorityTypeName = (dt.Rows[i]["AuthorityTypeName"].ToString()),
                        AuthorityName = (dt.Rows[i]["AuthorityName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetCMNatureofDocdataNew")]
        [HttpGet]
        public IEnumerable<NatureOfDocModel> GetCMNatureofDocdataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)) 
             group by natureof_doc.NatureOf_Doc_id";
            }
            //else if (datetype == "Effective Date")
            //{
            //    query = @"
            //SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
            // inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            //WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            //group by natureof_doc.NatureOf_Doc_id";
            //}
            //else if (datetype == "Discard Date")
            //{
            //    query = @"
            //SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
            // inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id 
            //WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //group by natureof_doc.NatureOf_Doc_id";
            //}
            //else if (datetype == "Disable Date")
            //{
            //    query = @"
            //SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
            // inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
            //WHERE addDoc_Status='InActive' AND (((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //group by natureof_doc.NatureOf_Doc_id";
            //}
            //else if (datetype == "Acknowledgement Date")
            //{
            //    query = @"
            //SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
            // inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id 
            //inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            //WHERE addDoc_Status='Active' AND (((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            //group by natureof_doc.NatureOf_Doc_id";
            //}
            //MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlCommand cmd = new MySqlCommand(@"SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY)) 
             group by natureof_doc.NatureOf_Doc_id", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<NatureOfDocModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new NatureOfDocModel
                    {
                        Count = Convert.ToInt32(dt.Rows[i]["Count"].ToString()),
                        NatureOf_Doc_Name = (dt.Rows[i]["NatureOf_Doc_Name"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetCMReadingTimedataNew")]
        [HttpGet]
        public IEnumerable<DocReadingTimeModel> GetCMReadingTimedataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //string query = "";
            //if (datetype == "Publishing Date")
            //{
            //    query = @"SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            // WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)) 
            // GROUP BY Time_period,indicative_reading_time";
            //}
            //else if (datetype == "Effective Date")
            //{
            //    query = @"
            //SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc 
            //WHERE addDoc_Status='Active' AND ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            //GROUP BY Time_period,indicative_reading_time";
            //}
            //else if (datetype == "Discard Date")
            //{
            //    query = @"
            //SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            //WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //GROUP BY Time_period,indicative_reading_time";
            //}
            //else if (datetype == "Disable Date")
            //{
            //    query = @"
            //SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            //WHERE addDoc_Status='InActive' AND (((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            //GROUP BY Time_period,indicative_reading_time";
            //}
            //else if (datetype == "Acknowledgement Date")
            //{
            //    query = @"
            //SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
            //inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            //WHERE addDoc_Status='Active' AND (((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            //GROUP BY Time_period,indicative_reading_time";
            //}
            //MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlCommand cmd = new MySqlCommand(@"SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
             WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY)) 
             GROUP BY Time_period,indicative_reading_time", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocReadingTimeModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocReadingTimeModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Time_period = (dt.Rows[i]["Time_period"].ToString()),
                        indicative_reading_time = (dt.Rows[i]["indicative_reading_time"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetCMAckReqSummary")]
        [HttpGet]
        public IEnumerable<AckReqSummaryModel> GetCMAckReqSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"SELECT 'Total Requested' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate)
            UNION ALL
            SELECT 'Total Acknowledged' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND ack_status = 'true' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            SELECT 'Total Requested' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            UNION ALL
            SELECT 'Total Acknowledged' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND ack_status = 'true' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            SELECT 'Total Requested' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            UNION ALL
            SELECT 'Total Acknowledged' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND ack_status= 'true' AND Draft_Status='Draft Discarded' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            SELECT 'Total Requested' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='InActive' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            UNION ALL
            SELECT 'Total Acknowledged' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='InActive' AND ack_status = 'true' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            SELECT 'Total Requested' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            UNION ALL
            SELECT 'Total Acknowledged' AS Acknowledgment, COUNT(*) AS count
            FROM risk.add_doc INNER JOIN risk.doc_taskuseracknowledment_status ON add_doc.AddDoc_id = doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND ack_status= 'true' AND (((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
            //MySqlCommand cmd = new MySqlCommand(@"SELECT COUNT(*) AS total_requested, SUM(CASE WHEN ack_status = 'true' THEN 1 ELSE 0 END) AS total_acknowledged
            // FROM risk.doc_taskuseracknowledment_status inner join risk.add_doc on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            // WHERE (doc_taskuseracknowledment_status.USR_ID=@userid) and ((Initial_creation_doc_date >= CURDATE() - INTERVAL @days DAY) OR (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate
            // AND Initial_creation_doc_date <= @ToDate))", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AckReqSummaryModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AckReqSummaryModel
                    {
                        Acknowledment = (dt.Rows[i]["Acknowledgment"].ToString()),
                        count =  Convert.ToInt32(dt.Rows[i]["count"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetCMDocTypeDataAckreq")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetCMDocTypeDataAckreq([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            Where addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id  
            WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id  
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='InActive' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            group by add_doc.DocTypeID";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            group by add_doc.DocTypeID";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);

            //MySqlCommand cmd = new MySqlCommand(@"
            //select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            //inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            //left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            //Where doc_taskuseracknowledment_status.USR_ID=@userid and ((Initial_creation_doc_date >= CURDATE() - INTERVAL @days DAY) OR (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) OR (Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
            //group by add_doc.DocTypeID", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetCMDocCategoryDataAckreq")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetCMDocCategoryDataAckreq([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            where addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))
            group by add_doc.Doc_CategoryID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='InActive' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
            inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (doccategory_master.DocTypeID = @DocTypeID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            group by add_doc.Doc_CategoryID,Doc_CategoryName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
//            MySqlCommand cmd = new MySqlCommand(@"
//            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
//inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
//left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
//where (doc_taskuseracknowledment_status.USR_ID=@userid) and (doccategory_master.DocTypeID = @DocTypeID) AND((Initial_creation_doc_date >= CURDATE() - INTERVAL @days DAY) OR (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY)
//OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
//group by add_doc.Doc_CategoryID", con);

            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetCMDocSubCategoryDataAckreq")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetCMDocSubCategoryDataAckreq([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days, [FromQuery] string datetype)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string query = "";
            if (datetype == "Publishing Date")
            {
                query = @"select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            Where addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))
            group by add_doc.Doc_SubCategoryID";
            }
            else if (datetype == "Effective Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(Eff_Date) >= @FromDate AND DATE(Eff_Date) <= @ToDate))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            else if (datetype == "Discard Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND Draft_Status='Draft Discarded' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            else if (datetype == "Disable Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='InActive' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(ChangedOn) >= @FromDate AND DATE(ChangedOn) <= @ToDate))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            else if (datetype == "Acknowledgement Date")
            {
                query = @"
            select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
            inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
            inner join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id
            WHERE addDoc_Status='Active' AND (docsubcategory_master.Doc_CategoryID = @Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(doc_taskuseracknowledment_status.createddate) >= @FromDate AND DATE(doc_taskuseracknowledment_status.createddate) <= @ToDate))
            group by add_doc.Doc_SubCategoryID,Doc_SubCategoryName";
            }
            MySqlCommand cmd = new MySqlCommand(query, con);
           // MySqlCommand cmd = new MySqlCommand(@"
           //select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           //inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           //left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           //Where (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((Initial_creation_doc_date >= CURDATE() - INTERVAL @days DAY) OR (Initial_creation_doc_date >= CURRENT_DATE - INTERVAL '30' DAY) 
           //OR(Initial_creation_doc_date >= @FromDate AND Initial_creation_doc_date <= @ToDate))
           //group by add_doc.Doc_SubCategoryID", con);

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        //Newly Added Documents
        [Route("api/Documentmaster/GetTaskownerDocTypeDataNew")]
        [HttpGet]
        public IEnumerable<DocTypeDataModel> GetTaskownerDocTypeDataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
            select add_doc.DocTypeID, count(add_doc.DocTypeID) as Doctype_Count,doctype_master.DocTypeName FROM risk.add_doc
            inner join risk.doctype_master on doctype_master.DocTypeID=add_doc.DocTypeID 
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            Where addDoc_Status='Active' AND doc_taskuseracknowledment_status.USR_ID=@userid and ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))
            group by add_doc.DocTypeID", con);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocTypeDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocTypeDataModel
                    {
                        Doctype_Count = Convert.ToInt32(dt.Rows[i]["Doctype_Count"].ToString()),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        DoctypeName = (dt.Rows[i]["DoctypeName"].ToString())
                    });
                }

            }
            return pdata;
        }
        [Route("api/Documentmaster/GetTaskownerDocCategoryDataNew")]
        [HttpGet]
        public IEnumerable<DocCategoryDataModel> GetTaskownerDocCategoryDataNew([FromQuery] int DocTypeID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
            select add_doc.Doc_CategoryID,count(add_doc.Doc_CategoryID) as Doccategory_Count,doccategory_master.Doc_CategoryName from risk.add_doc
inner join risk.doccategory_master on doccategory_master.Doc_CategoryID = add_doc.Doc_CategoryID
left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (doccategory_master.DocTypeID = @DocTypeID) AND((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) 
OR(DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))
group by add_doc.Doc_CategoryID", con);

            cmd.Parameters.AddWithValue("@DocTypeID", DocTypeID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocCategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocCategoryDataModel
                    {
                        Doccategory_Count = Convert.ToInt32(dt.Rows[i]["Doccategory_Count"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_CategoryName = (dt.Rows[i]["Doc_CategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTaskownerDocSubCategoryDataNew")]
        [HttpGet]
        public IEnumerable<DocSubcategoryDataModel> GetTaskownerDocSubCategoryDataNew([FromQuery] int DocCategoryID, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
           select add_doc.Doc_SubCategoryID,count(add_doc.Doc_SubCategoryID) as Docsubcategory_Count,docsubcategory_master.Doc_SubCategoryName from risk.add_doc
           inner join risk.docsubcategory_master on docsubcategory_master.Doc_SubCategoryID=add_doc.Doc_SubCategoryID
           left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
           Where addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and (docsubcategory_master.Doc_CategoryID=@Doc_CategoryID) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) 
           OR(DATE(addDoc_createdDate) >= @FromDate AND DATE(addDoc_createdDate) <= @ToDate))
           group by add_doc.Doc_SubCategoryID", con);

            cmd.Parameters.AddWithValue("@Doc_CategoryID", DocCategoryID);
            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocSubcategoryDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocSubcategoryDataModel
                    {
                        Docsubcategory_Count = Convert.ToInt32(dt.Rows[i]["Docsubcategory_Count"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        Doc_SubCategoryName = (dt.Rows[i]["Doc_SubCategoryName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTODocConfidentialityNew")]
        [HttpGet]
        public IEnumerable<DocConfidentialityModel> GetTODocConfidentialityNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
            select count(Doc_Confidentiality) as count,Doc_Confidentiality from risk.add_doc 
            left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
            WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and ((addDoc_createdDate >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate
            AND DATE(addDoc_createdDate) <= @ToDate)) group by Doc_Confidentiality", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocConfidentialityModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocConfidentialityModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Doc_Confidentiality = (dt.Rows[i]["Doc_Confidentiality"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTODocAuthoritydataNew")]
        [HttpGet]
        public IEnumerable<DocAuthorityDataModel> GetTODocAuthoritydataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT at.AuthorityTypeName,an.AuthorityName,COUNT(add_doc.AddDoc_id) AS count FROM risk.add_doc 
             INNER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = add_doc.AuthorityTypeID
             INNER JOIN risk.authorityname_master an ON an.AuthoritynameID = add_doc.AuthoritynameID
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
             WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) AND ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate
             AND DATE(addDoc_createdDate) <= @ToDate))
             GROUP BY at.AuthorityTypeName,an.AuthorityName ORDER BY at.AuthorityTypeName,an.AuthorityName", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocAuthorityDataModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocAuthorityDataModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        AuthorityTypeName = (dt.Rows[i]["AuthorityTypeName"].ToString()),
                        AuthorityName = (dt.Rows[i]["AuthorityName"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTONatureofDocdataNew")]
        [HttpGet]
        public IEnumerable<NatureOfDocModel> GetTONatureofDocdataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT count(natureof_doc.NatureOf_Doc_id) as Count,NatureOf_Doc_Name  FROM risk.add_doc 
             inner join risk.natureof_doc on natureof_doc.NatureOf_Doc_id=add_doc.NatureOf_Doc_id
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
             WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and ((DATE(addDoc_createdDate) >= CURDATE() - INTERVAL @days DAY) OR (DATE(addDoc_createdDate) >= @FromDate
             AND DATE(addDoc_createdDate) <= @ToDate)) group by natureof_doc.NatureOf_Doc_id", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<NatureOfDocModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new NatureOfDocModel
                    {
                        Count = Convert.ToInt32(dt.Rows[i]["Count"].ToString()),
                        NatureOf_Doc_Name = (dt.Rows[i]["NatureOf_Doc_Name"].ToString())
                    });
                }

            }
            return pdata;
        }

        [Route("api/Documentmaster/GetTOReadingTimedataNew")]
        [HttpGet]
        public IEnumerable<DocReadingTimeModel> GetTOReadingTimedataNew([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string userid, [FromQuery] int days)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT Time_period,indicative_reading_time,COUNT(*) AS count FROM risk.add_doc
             left join risk.doc_taskuseracknowledment_status on add_doc.AddDoc_id=doc_taskuseracknowledment_status.AddDoc_id 
             WHERE addDoc_Status='Active' AND (doc_taskuseracknowledment_status.USR_ID=@userid) and ((DATE(DATE(addDoc_createdDate)) >= CURDATE() - INTERVAL @days DAY) OR (DATE(DATE(addDoc_createdDate)) >= @FromDate
             AND DATE(DATE(addDoc_createdDate)) <= @ToDate)) GROUP BY Time_period,indicative_reading_time", con);

            cmd.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@userid", userid.ToString());
            cmd.Parameters.AddWithValue("@days", days);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DocReadingTimeModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DocReadingTimeModel
                    {
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString()),
                        Time_period = (dt.Rows[i]["Time_period"].ToString()),
                        indicative_reading_time = (dt.Rows[i]["indicative_reading_time"].ToString())
                    });
                }

            }
            return pdata;
        }

        //Insert Documentmaster Details
        //[Route("api/Documentmaster/InsertDocumentmaster")]
        //[HttpPost]
        //public IActionResult InsertParameter([FromBody] DocumentmasterModel DocumentmasterModels)
        //{
        //    var DocumentmasterModel = this.mySqlDBContext.DocumentmasterModels;
        //    DocumentmasterModel.Add(DocumentmasterModels);
        //    DateTime dt = DateTime.Now;
        //    //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //    DocumentmasterModels.created_date = dt;
        //    DocumentmasterModels.CreatedBy = "KrishnaLekkala";
        //    DocumentmasterModels.document_description = "For Testing Purpose";
        //    //this.mySqlDBContext.SaveChanges();
        //    return Ok();
        //}

    }
}
