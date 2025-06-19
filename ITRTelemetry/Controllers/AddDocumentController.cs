using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using ITR_TelementaryAPI.Models;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
//using Aspose.Words;
//using Document = Aspose.Words.Document;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Diagnostics;
using iText.Kernel.Pdf;

using iText.Kernel.Utils;
using Ubiety.Dns.Core;
using Google.Protobuf.WellKnownTypes;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AddDocumentController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;



        public AddDocumentController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        //Getting AddDocument Details by AddDocumentID  

        [Route("api/AddDocument/GetAddDocumentDetails")]
        [HttpGet]

        public IEnumerable<AddDocumentModel> GetAddDocumentDetails()
        {

            return this.mySqlDBContext.AddDocumentModels.Where(x => x.addDoc_Status == "Active").ToList();

        }

        [Route("api/AddDocument/GetPublishedDocumentList")]
        [HttpGet]

        public IEnumerable<AddDocumentModel> GetPublishedDocumentList()
        {

            return this.mySqlDBContext.AddDocumentModels.Where(x => x.addDoc_Status == "Active").ToList();

        }



        [Route("api/SavedDraftDocument/GetSavedDraftDetails")]
        [HttpGet]

        public IEnumerable<AddDocumentModel> GetSavedDraftDetails()
        {

            return this.mySqlDBContext.AddDocumentModels.Where(x => x.addDoc_Status == "Active" && x.Draft_Status == "Pending").ToList();

        }


        [Route("api/PublishedDocDocument/GetPublishedDocDetails")]
        [HttpGet]

        public IEnumerable<AddDocumentModel> GetPublishedDocDetails()
        {

            //return this.mySqlDBContext.AddDocumentModels.Where(x => x.addDoc_Status == "Active" && x.Draft_Status == "Completed").ToList();
            return this.mySqlDBContext.AddDocumentModels.Where(x => x.addDoc_Status == "Active" &&  x.Draft_Status == "Completed");

        }


        [Route("api/SavedDraftDocument/GetDraftData")]
        [HttpGet]
        public IEnumerable<ProvideAccessModel> GetDraftData()

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT " + "e.AuthoritynameID," + "e.Publisher_name," + "e.AuthorityTypeID," + "e.NatureOf_Doc_id," + "e.DocTypeID," + "e.Doc_SubCategoryID," + "e.Doc_CategoryID," + "e.Title_Doc," + "e.Doc_Confidentiality," + "e.Eff_Date," + "e.Initial_creation_doc_date," + "e.Doc_internal_num," + "e.Doc_Inter_ver_num," + "e.Doc_Phy_Valut_Loc," + "e.Doc_process_Owner," + "e.Doc_Approver," + "e.Date_Doc_Revision," +
               "e.Date_Doc_Approver," + "e.freq_period," + "e.Keywords_tags," + "e.pub_doc," + "e.publisher_comments," + "e.indicative_reading_time," + "e.Time_period," +
     "e.Sub_title_doc," + "e.AddDoc_id," +
     "e.Obj_Doc," +
    "e.Document_Id," +
   
    "t.DocTypeName," +
    "C.Doc_CategoryName," +
    "sc.Doc_SubCategoryName," +
    "a.AuthorityName," +
   "at.AuthorityTypeName," +
    "p.NatureOf_Doc_Name " +
   "FROM " +
    "risk.add_doc e" +

" Left Outer JOIN " +
    "risk.doctype_master t ON t.DocTypeID = e.DocTypeID" +
" Left Outer JOIN " +
    "risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID" +
" Left Outer JOIN " +
    "risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID" +
" Left Outer JOIN " +
    "risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID" +
" Left Outer JOIN " +
    "risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID" +
" Left Outer JOIN " +
    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id where e.addDoc_Status='Active' &&  e.Draft_Status='Incomplete';", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ProvideAccessModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new ProvideAccessModel
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        Publisher_name = dt.Rows[i]["Publisher_name"].ToString(),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        AuthoritynameID = Convert.ToInt32(dt.Rows[i]["AuthoritynameID"].ToString()),
                        AuthorityTypeID = Convert.ToInt32(dt.Rows[i]["AuthorityTypeID"].ToString()),
                        NatureOf_Doc_id = Convert.ToInt32(dt.Rows[i]["NatureOf_Doc_id"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        Sub_title_doc = dt.Rows[i]["Sub_title_doc"].ToString(),
                        Obj_Doc = dt.Rows[i]["Obj_Doc"].ToString(),
                       // document_name = dt.Rows[i]["document_name"].ToString(),
                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"].ToString(),
                        Eff_Date = dt.Rows[i]["Eff_Date"].ToString(),
                        Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                        Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                        Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                        Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                        Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                        Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                        Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),

                        Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                        freq_period = dt.Rows[i]["freq_period"].ToString(),
                        Keywords_tags = dt.Rows[i]["Keywords_tags"].ToString(),
                        pub_doc = dt.Rows[i]["pub_doc"].ToString(),
                        publisher_comments = dt.Rows[i]["publisher_comments"].ToString(),
                        indicative_reading_time = dt.Rows[i]["indicative_reading_time"].ToString(),
                        Time_period = dt.Rows[i]["Time_period"].ToString(),

                    });
                }
            }
            return pdata;

        }




        [Route("api/SavedDraftDocument/GetDraftsbyid/{AddDoc_id}")]
        [HttpGet]


        public IEnumerable<ProvideAccessModel> GetSavedDraftDetailsbyid(int AddDoc_id)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT " +
              "e.Title_Doc," +
     "e.Sub_title_doc," +
     "e.Obj_Doc," +
    "e.Document_Id," +
   
    "t.DocTypeName," +
    "C.Doc_CategoryName," +
    "sc.Doc_SubCategoryName," +
    "a.AuthorityName," +
   "at.AuthorityTypeName," +
    "p.NatureOf_Doc_Name " +
   "FROM " +
    "risk.add_doc e" +

" Left Outer JOIN " +
    "risk.doctype_master t ON t.DocTypeID = e.DocTypeID" +
" Left Outer JOIN " +
    "risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID" +
" Left Outer JOIN " +
    "risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID" +
" Left Outer JOIN " +
    "risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID" +
" Left Outer JOIN " +
    "risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID" +
" Left Outer JOIN " +
    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id  where e.AddDoc_id ='" + AddDoc_id + "';", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ProvideAccessModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new ProvideAccessModel
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["Document_Id"].ToString()),
                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                       // Document_Id = Convert.ToInt32(dt.Rows[i]["Document_Id"].ToString()),
                        //DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        //AuthoritynameID = Convert.ToInt32(dt.Rows[i]["AuthoritynameID"].ToString()),
                        //AuthorityTypeID = Convert.ToInt32(dt.Rows[i]["AuthorityTypeID"].ToString()),
                        //NatureOf_Doc_id = Convert.ToInt32(dt.Rows[i]["NatureOf_Doc_id"].ToString()),
                        //Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        Sub_title_doc = dt.Rows[i]["Sub_title_doc"].ToString(),
                        Obj_Doc = dt.Rows[i]["Obj_Doc"].ToString(),
                        //document_name = dt.Rows[i]["document_name"].ToString(),
                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),

                    });
                }
            }
            return pdata;

        }




        [Route("api/SavedDraftDocument/GetPublishedDatabyid/{AddDoc_id}")]
        [HttpGet]


        public IEnumerable<ProvideAccessModel> GetPublishedDatabyid(int AddDoc_id)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT " + "e.USR_ID," + "e.AuthoritynameID," + "e.AuthorityTypeID," + "e.NatureOf_Doc_id," + "e.DocTypeID," + "e.Doc_SubCategoryID," + "e.Doc_CategoryID," + "e.Title_Doc," + "e.Doc_Confidentiality," + "e.Eff_Date," + "e.Initial_creation_doc_date," + "e.Doc_internal_num," + "e.Doc_Inter_ver_num," + "e.Doc_Phy_Valut_Loc," + "e.Doc_process_Owner," + "e.Doc_Approver," + "e.Date_Doc_Revision," +
               "e.Date_Doc_Approver," + "e.freq_period," + "e.Keywords_tags," + "e.pub_doc," + "e.publisher_comments," + "e.indicative_reading_time," + "e.Time_period," + "e.review_start_Date," + "e.freq_period_type," +
     "e.Sub_title_doc," + "e.AddDoc_id," + "e.OtpMethod," +
     "e.Obj_Doc," +
      "e.Review_Frequency_Status," +
       "e.Doc_Linking_Status," +
    "e.Document_Id," + "e.Linking_Doc_names," +
    "b.firstname," +
    "e.addDoc_createdDate," +
    "t.DocTypeName," +
    "C.Doc_CategoryName," +
    "sc.Doc_SubCategoryName," +
    "a.AuthorityName," +
   "at.AuthorityTypeName," +
    "p.NatureOf_Doc_Name," + "e.Entity_Master_id," + "e.Unit_location_Master_id," +
      "ack.startDate,"+
     "ack.endDate"+
   " FROM " +
    "risk.add_doc e" +

" Left Outer JOIN " +
    "risk.doctype_master t ON t.DocTypeID = e.DocTypeID" +

    " Left Outer JOIN " +
    "risk.entity_master m ON m.Entity_Master_id = e.Entity_Master_id" +
     " Left Outer JOIN " +
    "risk.unit_location_master n ON n.Unit_location_Master_id = e.Unit_location_Master_id" +

" Left Outer JOIN " +
    "risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID" +
" Left Outer JOIN " +
    "risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID" +
" Left Outer JOIN " +
    "risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID" +
" Left Outer JOIN " +
    "risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID" +
     " Left Outer JOIN " +
 "risk.tblUser b ON b.USR_ID = e.USR_ID" +
   " Left Outer JOIN" +
   " risk.doc_taskuseracknowledment_status ack ON ack.Document_Id = e.Document_Id"+
" Left Outer JOIN " +
    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id where e.AddDoc_id ='" + AddDoc_id + "'  ;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt); 
            con.Close();
            var pdata = new List<ProvideAccessModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    int Review_Frequency_Status;
                    int Doc_Linking_Status;
                    if (dt.Rows[i]["Review_Frequency_Status"] != null && !string.IsNullOrEmpty(dt.Rows[i]["Review_Frequency_Status"].ToString()))
                    {
                        int.TryParse(dt.Rows[i]["Review_Frequency_Status"].ToString(), out Review_Frequency_Status);
                    }
                    else
                    {
                        Review_Frequency_Status = 0;
                    }

                    // Check and convert Doc_Linking_Status
                    if (dt.Rows[i]["Doc_Linking_Status"] != null && !string.IsNullOrEmpty(dt.Rows[i]["Doc_Linking_Status"].ToString()))
                    {
                        int.TryParse(dt.Rows[i]["Doc_Linking_Status"].ToString(), out Doc_Linking_Status);
                    }
                    else
                    {
                        Doc_Linking_Status = 0;
                    }
                    pdata.Add(new ProvideAccessModel
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                        //VersionControlNo = dt.Rows[i]["VersionControlNo"].ToString(),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        AuthoritynameID = Convert.ToInt32(dt.Rows[i]["AuthoritynameID"].ToString()),
                        AuthorityTypeID = Convert.ToInt32(dt.Rows[i]["AuthorityTypeID"].ToString()),
                        NatureOf_Doc_id = Convert.ToInt32(dt.Rows[i]["NatureOf_Doc_id"].ToString()),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Entity_Master_id = dt.Rows[i]["Entity_Master_id"].ToString(),

                        Unit_location_Master_id =dt.Rows[i]["Unit_location_Master_id"].ToString(),
                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        Sub_title_doc = dt.Rows[i]["Sub_title_doc"].ToString(),
                        Obj_Doc = dt.Rows[i]["Obj_Doc"].ToString(),
                       // document_name = dt.Rows[i]["document_name"].ToString(),
                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"].ToString(),
                        OtpMethod = dt.Rows[i]["OtpMethod"].ToString(),
                        Linking_Doc_names = dt.Rows[i]["Linking_Doc_names"].ToString(),
                       
                        Eff_Date = dt.Rows[i]["Eff_Date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Eff_Date"]).ToString("yyyy-MM-dd") : "",
                          

                    Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Initial_creation_doc_date"]).ToString("yyyy-MM-dd") : "",
                        Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Date_Doc_Revision"]).ToString("yyyy-MM-dd") : "",
                        Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Date_Doc_Approver"]).ToString("yyyy-MM-dd") : "",

                        //Initial_creation_doc_date = Convert.ToDateTime(dt.Rows[i]["Initial_creation_doc_date"]).ToString("yyyy-MM-dd"),
                        //Date_Doc_Revision = Convert.ToDateTime(dt.Rows[i]["Date_Doc_Revision"]).ToString("yyyy-MM-dd"),
                        //Date_Doc_Approver = Convert.ToDateTime(dt.Rows[i]["Date_Doc_Approver"]).ToString("yyyy-MM-dd"),
                        //review_start_Date = Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("yyyy-MM-dd"),
                        review_start_Date = dt.Rows[i]["review_start_Date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("yyyy-MM-dd") : "",

                        // Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                        Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                        Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                        Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                        Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),

                        Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                       // Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),

                      //  Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                       // review_start_Date = dt.Rows[i]["review_start_Date"].ToString(),
                        freq_period_type = dt.Rows[i]["freq_period_type"].ToString(),
                        freq_period = dt.Rows[i]["freq_period"].ToString(),
                        Keywords_tags = dt.Rows[i]["Keywords_tags"].ToString(),
                        //pub_doc = dt.Rows[i]["pub_doc"].ToString(),
                        pub_doc = string.IsNullOrEmpty(dt.Rows[i]["pub_doc"].ToString()) ? null : dt.Rows[i]["pub_doc"].ToString(),

                        firstname = dt.Rows[i]["firstname"].ToString(),
                        //CREATED_DATE = dt.Rows[i]["addDoc_createdDate"].ToString(),
                        CREATED_DATE = Convert.ToDateTime(dt.Rows[i]["addDoc_createdDate"]).ToString("yyyy-MM-dd"),
                        publisher_comments = dt.Rows[i]["publisher_comments"].ToString(),
                        indicative_reading_time = dt.Rows[i]["indicative_reading_time"].ToString(),
                        Time_period = dt.Rows[i]["Time_period"].ToString(),
                        Review_Frequency_Status = Review_Frequency_Status,
                        Doc_Linking_Status = Doc_Linking_Status,
                        //startdate = dt.Rows[i]["startDate"].ToString(),
                        //enddate = dt.Rows[i]["endDate"].ToString(),


                    });
                }
            }
            return pdata;

        }




        [Route("api/AcknowlwdgementData/GetAcknowlwdgementData")]
        [HttpGet]


        public IEnumerable<AckReqModel> GetAcknoweledgementData()

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT " +
              "e.Title_Doc," + "e.Doc_Confidentiality," + "e.Eff_Date," + "e.Initial_creation_doc_date," + "e.Doc_internal_num," + "e.Doc_Inter_ver_num," + "e.Doc_Phy_Valut_Loc," + "e.Doc_process_Owner," + "e.Doc_Approver," + "e.Date_Doc_Revision," +
               "e.Date_Doc_Approver," + "e.freq_period," + "e.Keywords_tags," + "e.pub_doc," + "e.publisher_comments," + "e.indicative_reading_time," + "e.Time_period," +
     "e.Sub_title_doc," + "e.AddDoc_id," +
     "e.Obj_Doc," + "e.Publisher_Date_Range , " +
    "e.Document_Id," +
   
    "t.DocTypeName," +
    "C.Doc_CategoryName," +
    "sc.Doc_SubCategoryName," +
    "a.AuthorityName," +
   "at.AuthorityTypeName," +
    "p.NatureOf_Doc_Name " +
   "FROM " +
    "risk.add_doc e" +

" Left Outer JOIN " +
    "risk.doctype_master t ON t.DocTypeID = e.DocTypeID" +
" Left Outer JOIN " +
    "risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID" +
" Left Outer JOIN " +
    "risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID" +
" Left Outer JOIN " +
    "risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID" +
" Left Outer JOIN " +
    "risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID" +
" Left Outer JOIN " +
    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AckReqModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AckReqModel
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        //Document_Id = Convert.ToInt32(dt.Rows[i]["Document_Id"].ToString()),
                        //DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        //AuthoritynameID = Convert.ToInt32(dt.Rows[i]["AuthoritynameID"].ToString()),
                        //AuthorityTypeID = Convert.ToInt32(dt.Rows[i]["AuthorityTypeID"].ToString()),
                        //NatureOf_Doc_id = Convert.ToInt32(dt.Rows[i]["NatureOf_Doc_id"].ToString()),
                        //Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        Sub_title_doc = dt.Rows[i]["Sub_title_doc"].ToString(),
                        Obj_Doc = dt.Rows[i]["Obj_Doc"].ToString(),
                       // document_name = dt.Rows[i]["document_name"].ToString(),
                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"].ToString(),
                        Eff_Date = dt.Rows[i]["Eff_Date"].ToString(),
                        Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                        Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                        Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                        Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                        Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                        Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                        Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),

                        Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                        freq_period = dt.Rows[i]["freq_period"].ToString(),
                        Keywords_tags = dt.Rows[i]["Keywords_tags"].ToString(),
                        pub_doc = dt.Rows[i]["pub_doc"].ToString(),
                        publisher_comments = dt.Rows[i]["publisher_comments"].ToString(),
                        indicative_reading_time = dt.Rows[i]["indicative_reading_time"].ToString(),
                        Time_period = dt.Rows[i]["Time_period"].ToString(),
                        Publisher_Date_Range = dt.Rows[i]["Publisher_Date_Range"].ToString(),

                    });
                }
            }
            return pdata;

        }





        [Route("api/ProvideAccess/GetProvideAccessDetails")]
        [HttpGet]

        public IEnumerable<ProvideAccessModel> GetProvideAccessDetails(string provideAccess)

        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT " +
              "e.Select_Opt, " +
  "e.AddDoc_id," +
  "e.Title_Doc," +
  "t.DocTypeName," +
  "C.Doc_CategoryName," +
  "sc.Doc_SubCategoryName," +
  "a.AuthorityName," +
 "at.AuthorityTypeName," +
  "p.NatureOf_Doc_Name " +
 "FROM " +
  "risk.add_doc e" +
" JOIN " +
  "risk.doctype_master t ON t.DocTypeID = e.DocTypeID" +
" JOIN " +
  "risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID" +
" JOIN " +
  "risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID" +
" JOIN " +
  "risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID" +
" JOIN " +
  "risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID" +
" JOIN " +
"risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ProvideAccessModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new ProvideAccessModel
                    {

                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),

                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        Select_Opt = dt.Rows[i]["Select_Opt"].ToString(),
                    });
                }
            }
            return pdata;

        }




        //[Route("api/AddDocument/UpdatePublishedDocData")]
        //[HttpPut]
        //public void UpdateAuthorityType([FromBody] ProvideAccessModel ProvideAccessModels)
        //{
        //    if (ProvideAccessModels.AddDoc_id == 0)
        //    {
        //    }
        //    else
        //    {
        //        this.mySqlDBContext.Attach(ProvideAccessModels);
        //        this.mySqlDBContext.Entry(ProvideAccessModels).State = EntityState.Modified;

        //        var entry = this.mySqlDBContext.Entry(ProvideAccessModels);

        //        Type type = typeof(ProvideAccessModel);
        //        PropertyInfo[] properties = type.GetProperties();
        //        foreach (PropertyInfo property in properties)
        //        {
        //            if (property.GetValue(ProvideAccessModels, null) == null || property.GetValue(ProvideAccessModels, null).Equals(0))
        //            {
        //                entry.Property(property.Name).IsModified = false;
        //            }
        //        }

        //        this.mySqlDBContext.SaveChanges();
        //    }

        //}


        //[Route("api/AddDocument/UpdatePublishedDoc")]
        //[HttpPost]
        //public IActionResult UpdatePublishedDoc([FromBody] UpdateDoc AddDocumentModels)
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

        //    try
        //    {

        //        con.Open();
        //        int AddDoc_Id = AddDocumentModels.AddDoc_id;
        //    MySqlCommand cmd = new MySqlCommand("SELECT * from add_doc where AddDoc_id='" + AddDoc_Id + "'", con);

        //    cmd.CommandType = CommandType.Text;

        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

        //    DataTable dt = new DataTable();
        //    da.Fill(dt);

        //    string newVersionControlNo;
        //        if (dt.Rows.Count > 0)
        //        {
        //            string VersionControlNo = dt.Rows[0]["VersionControlNo"].ToString();

        //            if (decimal.TryParse(VersionControlNo, out decimal versionDecimal))
        //            {
        //                decimal newVersionControlNoDecimal = versionDecimal + 0.1M;

        //                // Use Math.Round to round the result to one decimal place
        //                newVersionControlNoDecimal = Math.Round(newVersionControlNoDecimal, 1);

        //                newVersionControlNo = newVersionControlNoDecimal.ToString();



        //                string UpdateQuery = "UPDATE add_doc SET " + "publisher_comments=@publisher_comments, "
        //                + "Doc_Linking_Status=@Doc_Linking_Status, "
        //                + "Review_Frequency_Status=@Review_Frequency_Status, " + "VersionControlNo=@VersionControlNo, " +
        // "Title_Doc = @Title_Doc, " +
        // "Sub_title_doc = @Sub_title_doc, " +
        // "Obj_Doc = @Obj_Doc, " +
        // "DocTypeID = @DocTypeID, " +
        // "Doc_CategoryID = @Doc_CategoryID, " +
        //"Doc_SubCategoryID = @Doc_SubCategoryID, " +
        //"Eff_Date = @Eff_Date, " +
        //"Initial_creation_doc_date = @Initial_creation_doc_date, " +
        //"Doc_internal_num = @Doc_internal_num, " +
        //"Doc_Inter_ver_num = @Doc_Inter_ver_num, " +
        //"Doc_Phy_Valut_Loc = @Doc_Phy_Valut_Loc, " +
        //"Doc_process_Owner = @Doc_process_Owner, " +
        //"Doc_Approver = @Doc_Approver, " +
        //"Date_Doc_Approver = @Date_Doc_Approver, " +
        //"Date_Doc_Revision = @Date_Doc_Revision, " +
        //"AuthorityTypeID = @AuthorityTypeID, " +
        //"AuthoritynameID = @AuthoritynameID, " + "Doc_Confidentiality = @Doc_Confidentiality, " +
        //"NatureOf_Doc_id = @NatureOf_Doc_id, " + "indicative_reading_time = @indicative_reading_time, " +
        //"Keywords_tags = @Keywords_tags, " + "freq_period_type = @freq_period_type, " + "freq_period = @freq_period, " + "review_start_Date = @review_start_Date, " + "pub_doc = @pub_doc, " + "Time_period = @Time_period " +
        //"where AddDoc_id = @AddDoc_id";

        //                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
        //                {

        //                    myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
        //                    myCommand.Parameters.AddWithValue("@Title_Doc", AddDocumentModels.Title_Doc);
        //                    myCommand.Parameters.AddWithValue("@Sub_title_doc", AddDocumentModels.Sub_title_doc);
        //                    myCommand.Parameters.AddWithValue("@Obj_Doc", AddDocumentModels.Obj_Doc);
        //                    myCommand.Parameters.AddWithValue("@DocTypeID", AddDocumentModels.DocTypeID);
        //                    myCommand.Parameters.AddWithValue("@Doc_CategoryID", AddDocumentModels.Doc_CategoryID);
        //                    myCommand.Parameters.AddWithValue("@Doc_SubCategoryID", AddDocumentModels.Doc_SubCategoryID);

        //                    myCommand.Parameters.AddWithValue("@Doc_Confidentiality", AddDocumentModels.Doc_Confidentiality);
        //                    myCommand.Parameters.AddWithValue("@Eff_Date", AddDocumentModels.Eff_Date);
        //                    myCommand.Parameters.AddWithValue("@Initial_creation_doc_date", AddDocumentModels.Initial_creation_doc_date);
        //                    myCommand.Parameters.AddWithValue("@Doc_internal_num", AddDocumentModels.Doc_internal_num);
        //                    myCommand.Parameters.AddWithValue("@Doc_Inter_ver_num", AddDocumentModels.Doc_Inter_ver_num);
        //                    myCommand.Parameters.AddWithValue("@Doc_Phy_Valut_Loc", AddDocumentModels.Doc_Phy_Valut_Loc);
        //                    myCommand.Parameters.AddWithValue("@Doc_process_Owner", AddDocumentModels.Doc_process_Owner);
        //                    myCommand.Parameters.AddWithValue("@Doc_Approver", AddDocumentModels.Doc_Approver);
        //                    myCommand.Parameters.AddWithValue("@Date_Doc_Approver", AddDocumentModels.Date_Doc_Approver);
        //                    myCommand.Parameters.AddWithValue("@Date_Doc_Revision", AddDocumentModels.Date_Doc_Revision);
        //                    myCommand.Parameters.AddWithValue("@AuthorityTypeID", AddDocumentModels.AuthorityTypeID);
        //                    myCommand.Parameters.AddWithValue("@AuthoritynameID", AddDocumentModels.AuthoritynameID);
        //                    myCommand.Parameters.AddWithValue("@NatureOf_Doc_id", AddDocumentModels.NatureOf_Doc_id);
        //                    myCommand.Parameters.AddWithValue("@Keywords_tags", AddDocumentModels.Keywords_tags);

        //                    myCommand.Parameters.AddWithValue("@freq_period", AddDocumentModels.freq_period);
        //                    myCommand.Parameters.AddWithValue("@freq_period_type", AddDocumentModels.freq_period_type);
        //                    myCommand.Parameters.AddWithValue("@review_start_Date", AddDocumentModels.review_start_Date);
        //                    myCommand.Parameters.AddWithValue("@pub_doc", AddDocumentModels.pub_doc);
        //                    myCommand.Parameters.AddWithValue("@publisher_comments", AddDocumentModels.publisher_comments);
        //                    myCommand.Parameters.AddWithValue("@indicative_reading_time", AddDocumentModels.indicative_reading_time);
        //                    myCommand.Parameters.AddWithValue("@Time_period", AddDocumentModels.Time_period);
        //                    myCommand.Parameters.AddWithValue("@Review_Frequency_Status", 1);
        //                    myCommand.Parameters.AddWithValue("@Doc_Linking_Status", 1);
        //                    myCommand.Parameters.AddWithValue("@VersionControlNo", newVersionControlNo);

        //                    myCommand.ExecuteNonQuery();


        //                }
        //            }
        //        }

        //        return Ok("Updated Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //    finally
        //    {
        //        con.Close();
        //    }

        //}



        [Route("api/AddDocument/DisablePublishedDocDetails")]
        [HttpPut]
        public IActionResult DisablePublishedDoc([FromBody] AddDocumentModel AddDocumentModels)
        {
            var AddDoc_id = AddDocumentModels.AddDoc_id;
            //var DisableReason = AddDocumentModels.DisableReason;
            //var AddDocumentModel = this.mySqlDBContext.AddDocumentModels;
            //AddDocumentModel.Add(AddDocumentModels);
            //AddDocumentModels.addDoc_Status = "Disabled";
            // return this.mySqlDBContext.AddDocumentModels.Where(x => x.Draft_Status == "Completed").ToList();

            var document = this.mySqlDBContext.AddDocumentModels.FirstOrDefault(x => x.AddDoc_id == AddDoc_id);

            if (document != null)
            {
                //document.DisableReason = DisableReason;
                document.addDoc_Status = "Disabled";

                // Save changes to the database
                this.mySqlDBContext.SaveChanges();

                return Ok("Record updated successfully");
            }
            else
            {
                return NotFound("Record not found");
            }
        }



        [Route("api/AddDocument/ReactivatePublishedDocDetails")]
        [HttpPut]
        public IActionResult ReactivatePublishedDoc([FromBody] AddDocumentModel AddDocumentModels)
        {
            var AddDoc_id = AddDocumentModels.AddDoc_id;
            //var DisableReason = AddDocumentModels.DisableReason;
            //var AddDocumentModel = this.mySqlDBContext.AddDocumentModels;
            //AddDocumentModel.Add(AddDocumentModels);
            //AddDocumentModels.addDoc_Status = "Disabled";
            // return this.mySqlDBContext.AddDocumentModels.Where(x => x.Draft_Status == "Completed").ToList();

            var document = this.mySqlDBContext.AddDocumentModels.FirstOrDefault(x => x.AddDoc_id == AddDoc_id);

            if (document != null)
            {
                //document.DisableReason = DisableReason;
                document.addDoc_Status = "Active";

                // Save changes to the database
                this.mySqlDBContext.SaveChanges();

                return Ok("Record updated successfully");
            }
            else
            {
                return NotFound("Record not found");
            }
        }









        [Route("api/AddDocument/InsertAddDocumentDetails")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> InsertParameter([FromForm] AddDocumentModel AddDocumentModels, [FromForm] IFormFile mainFile, [FromForm] List<IFormFile> supportFiles)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            try
            {
                int supportFilesCount = supportFiles.Count;
                // string supportFilesCountAsString = supportFilesCount.ToString();

                var httpContext = _httpContextAccessor.HttpContext;

                // Get the current HTTP request
                var request = HttpContext.Request;


                // Build the base URL from the request
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                string documentid = "";

                AddDocumentModels.Document_Id = GenerateDocumentId();


                documentid = AddDocumentModels.Document_Id;
                var AddDocumentModel = this.mySqlDBContext.AddDocumentModels;
                AddDocumentModel.Add(AddDocumentModels);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

                AddDocumentModels.addDoc_createdDate = dt1;



                if (!string.IsNullOrEmpty(AddDocumentModels.pub_doc))
                {
                    string pubDocValue = AddDocumentModels.pub_doc;
                    string[] pubDocIds = pubDocValue.Split(',');
                    List<string> titleDocs = new List<string>();
                    string query = "SELECT Title_Doc FROM add_doc WHERE AddDoc_id = @pub_doc";

                    foreach (string pubDocId in pubDocIds)
                    {
                        using (MySqlCommand command = new MySqlCommand(query, con))
                        {
                            command.Parameters.AddWithValue("@pub_doc", pubDocId);

                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string titleDoc = reader["Title_Doc"].ToString();
                                    titleDocs.Add(titleDoc);
                                }
                            }
                        }
                    }
                    AddDocumentModels.Linking_Doc_names = string.Join(",", titleDocs);
                
                }
                //AddDocumentModels.addDoc_createdDate = dt1;

                AddDocumentModels.addDoc_Status = "Active";
                AddDocumentModels.Draft_Status = "Completed";
                AddDocumentModels.VersionControlNo = "1.0";

                AddDocumentModels.supportFilesCount = supportFilesCount;
               
               

                //AddDocumentModels.USR_ID = 1;
                await mySqlDBContext.SaveChangesAsync();

                var newFolderName = Path.Combine("Resources", documentid);
                DirectoryInfo docidFolderName = Directory.CreateDirectory(newFolderName);//document id Path generation

                var VersionFolder = Path.Combine(newFolderName, "1.0");
                DirectoryInfo VersionFolderPath = Directory.CreateDirectory(VersionFolder);//version generation in document id folder 


                var mainFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Main"));
                var supportFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Support"));
                string coverpagepdf;
               // var reportPath;
                //var reportPath = GenerateReport(AddDocumentModels, VersionFolderPath.FullName);
                //coverpagepdf = Path.ChangeExtension(reportPath, "pdf");

                //ConvertDocxToPdf(reportPath, coverpagepdf);

                //int pageCount = GetPdfPageCount(coverpagepdf);
                //System.Diagnostics.Debug.Print("Page Count" + pageCount);


                // Do something with the generated report, like returning a path or the file itself
                //return Ok(new { ReportPath = reportPath });
                // Check if the report was generated successfully
                // Generate the URL for the cover page


                if (mainFile != null && mainFile.Length > 0)
                {
                    var mainFilePath = Path.Combine(mainFilesFolder.FullName, mainFile.FileName);

                    using (var stream = new FileStream(mainFilePath, FileMode.Create))
                    {
                        await mainFile.CopyToAsync(stream);
                    }
                    string pdfFilePath;
                    string mergedPdfFileName = "Published.pdf";
                    string mergedPdfPath = Path.Combine(VersionFolderPath.FullName, mergedPdfFileName); // Path for the merged PDF file
                                                                                                        // Check if the file is already a PDF
                    if (Path.GetExtension(mainFile.FileName).ToLower() == ".pdf")
                    {
                        pdfFilePath = mainFilePath; // Use the uploaded file directly if it's a PDF
                        int MainpageCount = GetPdfPageCount(pdfFilePath);
                        System.Diagnostics.Debug.Print("Page Count" + MainpageCount);

                        AddDocumentModels.MainpageCount = MainpageCount;

                       var  reportPath = GenerateReport(AddDocumentModels, VersionFolderPath.FullName);
                        coverpagepdf = Path.ChangeExtension(reportPath, "pdf");

                        ConvertDocxToPdf(reportPath, coverpagepdf);

                        MergePdfFiles(coverpagepdf, pdfFilePath, mergedPdfPath);
                    }
                    else
                    {
                        // Convert to PDF if it's not a PDF
                        pdfFilePath = Path.ChangeExtension(mainFilePath, "pdf");
                        ConvertDocxToPdf(mainFilePath, pdfFilePath);

                        int MainpageCount = GetPdfPageCount(pdfFilePath);
                        System.Diagnostics.Debug.Print("Page Count" + MainpageCount);

                        AddDocumentModels.MainpageCount = MainpageCount;

                       var  reportPath = GenerateReport(AddDocumentModels, VersionFolderPath.FullName);
                        coverpagepdf = Path.ChangeExtension(reportPath, "pdf");

                        ConvertDocxToPdf(reportPath, coverpagepdf);

                        MergePdfFiles(coverpagepdf, pdfFilePath, mergedPdfPath);
                    }

                    string coverPageFileName = Path.GetFileName(coverpagepdf);
                    string coverPageRelativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), coverpagepdf).Replace("\\", "/");
                    string coverPageUrl = $"{baseUrl}/{coverPageRelativePath}";
                    // Add cover page to database
                    var coverPageUploadModel = new DocumentFilesuplodModel
                    {
                        Document_Name = coverPageFileName,
                        FilePath = coverPageUrl,
                        FileCategory = "Cover Page",
                        Status = "Active",
                        documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        AddDoc_id = AddDocumentModels.AddDoc_id,
                        Document_Id = AddDocumentModels.Document_Id,
                        VersionControlNo = AddDocumentModels.VersionControlNo
                    };
                    mySqlDBContext.DocumentFilesuplodModels.Add(coverPageUploadModel);
                    if (string.IsNullOrEmpty(coverpagepdf))
                    {
                        return BadRequest("Report generation failed.");
                    }
                    // Generate the URL for the merged PDF
                    string mergedPdfFile = Path.GetFileName(mergedPdfPath);
                    string mergedPdfRelativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), mergedPdfPath).Replace("\\", "/");
                    string mergedPdfUrl = $"{baseUrl}/{mergedPdfRelativePath}";

                    // Add merged PDF to database
                    var mergedPdfUploadModel = new DocumentFilesuplodModel
                    {
                        Document_Name = mergedPdfFile,
                        FilePath = mergedPdfUrl,
                        FileCategory = "Published",
                        Status = "Active",
                        documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        AddDoc_id = AddDocumentModels.AddDoc_id,
                        Document_Id = AddDocumentModels.Document_Id,
                        VersionControlNo = AddDocumentModels.VersionControlNo
                    };
                    mySqlDBContext.DocumentFilesuplodModels.Add(mergedPdfUploadModel);


                    // Extract the file name from pdfFilePath
                    string fileName = Path.GetFileName(pdfFilePath);

                    // Use the relative path from the application's root to the file
                    string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), pdfFilePath).Replace("\\", "/");

                    // Combine them to form the URL
                    string fileUrl = $"{baseUrl}/{relativePath}";

                    // Convert to PDF
                    // var pdfFilePath = Path.ChangeExtension(mainFilePath, "pdf");
                    //  ConvertToPdf(mainFilePath, pdfFilePath);
                    //ConvertDocxToPdf(mainFilePath, pdfFilePath);
                    // Create a file upload record
                    var fileUploadModel = new DocumentFilesuplodModel
                    {
                        Document_Name = fileName,
                        FilePath = fileUrl,
                        FileCategory = "Main",
                        Status = "Active",
                        documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        AddDoc_id = AddDocumentModels.AddDoc_id,
                        Document_Id = AddDocumentModels.Document_Id,
                        VersionControlNo = AddDocumentModels.VersionControlNo
                    };

                    // Add to the database context
                    mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                }
                else
                {
                    return BadRequest("Main file is required.");
                }
                // Handle supportFiles uploads
                foreach (var file in supportFiles)
                {
                    if (file.Length > 0)
                    {
                        // Save the file
                        var filePath = Path.Combine(supportFilesFolder.FullName, file.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Generate URL for the file
                        string fileName = Path.GetFileName(filePath);
                        string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath).Replace("\\", "/");
                        string fileUrl = $"{baseUrl}/{relativePath}";

                        // Create a file upload record with the generated URL

                        var fileUploadModel = new DocumentFilesuplodModel
                        {
                            Document_Name = fileName,
                            FilePath = fileUrl,
                            FileCategory = "Support",
                            Status = "Active",
                            documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            AddDoc_id = AddDocumentModels.AddDoc_id,
                            Document_Id = AddDocumentModels.Document_Id,
                            VersionControlNo = AddDocumentModels.VersionControlNo
                        };

                        // Add to the database context
                        mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                    }
                }
                await mySqlDBContext.SaveChangesAsync();

                string insertQuery2 = "insert into document_revision_mapping(Doc_referenceNo, Revision_summary, AddDoc_id, VersionControlNo, Document_Id,Doc_rev_map_createdDate,Doc_rev_map_status) values (@Doc_referenceNo, @Revision_summary, @AddDoc_id, @VersionControlNo, @Document_Id,@Doc_rev_map_createdDate,@Doc_rev_map_status)";

                string dt2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (MySqlCommand myCommand = new MySqlCommand(insertQuery2, con))
                {

                    myCommand.Parameters.AddWithValue("@Doc_referenceNo", AddDocumentModels.Doc_referenceNo);
                    myCommand.Parameters.AddWithValue("@Revision_summary", AddDocumentModels.Revision_summary);
                    myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                    myCommand.Parameters.AddWithValue("@Document_Id", AddDocumentModels.Document_Id);
                    myCommand.Parameters.AddWithValue("@VersionControlNo", AddDocumentModels.VersionControlNo);
                    myCommand.Parameters.AddWithValue("@Doc_rev_map_createdDate", dt2);
                    myCommand.Parameters.AddWithValue("@Doc_rev_map_status", "Active");
                   
                     myCommand.ExecuteNonQuery();
                  
                    
                }

                return Ok(new { message = "Document Published Successfully", Document_Id = AddDocumentModels.Document_Id , VersionControlNo =AddDocumentModels.VersionControlNo , Title_Doc =AddDocumentModels.Title_Doc , addDoc_createdDate =AddDocumentModels.addDoc_createdDate, Eff_Date = Convert.ToDateTime(AddDocumentModels.Eff_Date).ToString("yyyy-MM-dd"), AddDoc_id = AddDocumentModels.AddDoc_id });

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error:{ex}");
            }
          
            finally
            {
                con.Close();
            }
        }

        //private void ConvertToPdf(string inputFilePath, string outputFilePath)
        //{
        //    // Load the document
        //    var document = new Document(inputFilePath);

        //    // Save as PDF
        //    document.Save(outputFilePath, SaveFormat.Pdf);
        //}


        private static Random rng = new Random();

        private string GenerateDocumentId()
        {
            string entitySerialStr = GetNextEntitySerialNumber();

            string yymmdd = DateTime.Now.ToString("yyMMdd");
            string randomNumber = rng.Next(0, 100000).ToString("D5");

            string docId = $"{entitySerialStr}-{yymmdd}-{randomNumber}";

            while (DocumentIdExists(docId)) // Keep generating until we get a unique ID
            {
                randomNumber = rng.Next(0, 100000).ToString("D5");
                docId = $"{entitySerialStr}-{yymmdd}-{randomNumber}";
            }

            return docId;
        }

        private string GetNextEntitySerialNumber()
        {
            // First, just retrieve the latest Document_Id from the database.
            var latestDocumentId = mySqlDBContext.AddDocumentModels
                                                 .Where(doc => !string.IsNullOrEmpty(doc.Document_Id))
                                                 .OrderByDescending(doc => doc.Document_Id)
                                                 .Select(doc => doc.Document_Id)
                                                 .FirstOrDefault();

            int maxSerialNo = 0;

            if (!string.IsNullOrEmpty(latestDocumentId))
            {
                // Now, do the splitting and parsing here.
                var maxSerialStr = latestDocumentId.Split('-')[0];

                if (int.TryParse(maxSerialStr, out var serial))
                {
                    maxSerialNo = serial;
                }
            }

            int nextSerial = maxSerialNo + 1;

            return nextSerial.ToString("D3"); // Convert it back to string with leading zeroes.
        }

        private bool DocumentIdExists(string docId)
        {
            return mySqlDBContext.AddDocumentModels.Any(doc => doc.Document_Id == docId);
        }

        private string GenerateReport(AddDocumentModel data, string outputBaseDirectory)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            // Define the path to the output directory and ensure it exists
            var outputDirectory = Path.Combine(outputBaseDirectory, "CoverPages");
            Directory.CreateDirectory(outputDirectory);

            var outputPath = Path.Combine(outputDirectory, $"Report-{data.Document_Id}.docx");

            try
            {
                DataTable dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(@"SELECT e.Title_Doc, e.Sub_title_doc, e.Eff_Date, e.Document_Id, e.VersionControlNo, e.Obj_Doc, t.DocTypeName, C.Doc_CategoryName, sc.Doc_SubCategoryName, a.AuthorityName, at.AuthorityTypeName, p.NatureOf_Doc_Name, e.Initial_creation_doc_date, e.Date_Doc_Revision, e.Doc_internal_num, e.Doc_Inter_ver_num, e.Doc_Phy_Valut_Loc, e.Doc_process_Owner, e.Doc_Approver, e.Date_Doc_Approver, e.AuthoritynameID, e.AuthorityTypeID, e.NatureOf_Doc_id, e.DocTypeID, e.Doc_SubCategoryID, e.Doc_CategoryID, e.Doc_Confidentiality, e.freq_period, e.freq_period_type, e.Keywords_tags, e.pub_doc, e.publisher_comments, e.indicative_reading_time, e.Time_period, e.review_start_Date, e.AddDoc_id, tblUser.firstname, e.addDoc_createdDate FROM risk.add_doc e LEFT OUTER JOIN risk.tblUser ON tblUser.USR_ID = e.USR_ID LEFT OUTER JOIN risk.doctype_master t ON t.DocTypeID = e.DocTypeID LEFT OUTER JOIN risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID LEFT OUTER JOIN risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID LEFT OUTER JOIN risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID LEFT OUTER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID LEFT OUTER JOIN risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id WHERE e.AddDoc_id ='" + data.AddDoc_id + "'", con);

                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    DateTime PublishedDate = new DateTime();
                    // Path to the template document
                    //var templatePath = "DocTemplate/PubDocTemplate1.dotx";
                    var templatePath = "DocTemplate/PubDocTemplate2.dotx";
                    
                    System.IO.File.Copy(templatePath, outputPath);
                    // Open the template document for reading
                    using (WordprocessingDocument doc = WordprocessingDocument.Open(outputPath, true))
                    {
                        doc.ChangeDocumentType(WordprocessingDocumentType.Document);

                        // Assign a reference to the existing document body.
                        Body body = doc.MainDocumentPart.Document.Body;


                        IDictionary<String, BookmarkStart> bookmarkMap = new Dictionary<String, BookmarkStart>();
                        foreach (BookmarkStart bookmarkStart in doc.MainDocumentPart.RootElement.Descendants<BookmarkStart>())
                        {
                            Run bookmarkText = bookmarkStart.NextSibling<Run>();
                            bookmarkMap[bookmarkStart.Name] = bookmarkStart;

                            if (bookmarkStart.Name == "title1")
                            {
                                if (bookmarkText != null)
                                {
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Title_Doc"].ToString();
                                }
                            }
                            else if (bookmarkStart.Name == "title2")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Title_Doc"].ToString();
                            }
                            else if (bookmarkStart.Name == "Subtitle1")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Sub_title_doc"].ToString();
                            }

                            else if (bookmarkStart.Name == "Subtitle2")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Sub_title_doc"].ToString();
                            }
                            else if (bookmarkStart.Name == "Objective")
                            {
                                if (bookmarkText != null)
                                {
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Obj_Doc"].ToString();
                                }
                            }
                            else if (bookmarkStart.Name == "DocumentID")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Document_Id"].ToString();
                            }
                            else if (bookmarkStart.Name == "DocumentID2")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Document_Id"].ToString();
                            }
                            else if (bookmarkStart.Name == "DocEffectiveDate")
                            {
                                if (bookmarkText != null)
                                {
                                    DateTime DocEffectiveDate;
                                    if (DateTime.TryParse(dt.Rows[0]["Eff_Date"].ToString(), out DocEffectiveDate))
                                    {
                                        // Format the date to a string without the time component
                                        bookmarkText.GetFirstChild<Text>().Text = DocEffectiveDate.ToString("dd-MM-yyyy");
                                    }
                                }
                            }
                            else if (bookmarkStart.Name == "DocTypeName")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["DocTypeName"].ToString();
                            }
                            else if (bookmarkStart.Name == "Doc_CategoryName")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Doc_CategoryName"].ToString();
                            }
                            else if (bookmarkStart.Name == "Doc_SubCategoryName")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Doc_SubCategoryName"].ToString();
                            }
                            else if (bookmarkStart.Name == "Initial_creation_doc_date")
                            {
                                if (bookmarkText != null)
                                {
                                    DateTime initialCreationDate;
                                    if (DateTime.TryParse(dt.Rows[0]["Initial_creation_doc_date"].ToString(), out initialCreationDate))
                                    {
                                        // Format the date to a string without the time component
                                        bookmarkText.GetFirstChild<Text>().Text = initialCreationDate.ToString("dd-MM-yyyy");
                                    }
                                }
                            }
                            else if (bookmarkStart.Name == "Date_Doc_Revision")
                            {
                                if (bookmarkText != null)
                                {
                                    DateTime docRevisionDate;
                                    if (DateTime.TryParse(dt.Rows[0]["Date_Doc_Revision"].ToString(), out docRevisionDate))
                                    {
                                        // Format the date to a string without the time component
                                        bookmarkText.GetFirstChild<Text>().Text = docRevisionDate.ToString("dd-MM-yyyy");
                                    }
                                }
                            }

                            else if (bookmarkStart.Name == "Doc_internal_num")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Doc_internal_num"].ToString();
                            }
                            else if (bookmarkStart.Name == "Doc_Inter_ver_num")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Doc_Inter_ver_num"].ToString();
                            }
                            else if (bookmarkStart.Name == "Doc_process_Owner")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Doc_process_Owner"].ToString();
                            }
                            else if (bookmarkStart.Name == "Doc_Approver")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Doc_Approver"].ToString();
                            }
                            else if (bookmarkStart.Name == "NatureOf_Doc_Name2")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["NatureOf_Doc_Name"].ToString();
                            }
                            else if (bookmarkStart.Name == "NatureOf_Doc_Name1")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["NatureOf_Doc_Name"].ToString();
                            }
                            else if (bookmarkStart.Name == "Page2NatureOf_Doc_Name")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["NatureOf_Doc_Name"].ToString();
                            }
                            else if (bookmarkStart.Name == "Publishedby")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["firstname"].ToString();
                            }
                            else if (bookmarkStart.Name == "PublishedDate")
                            {
                                if (bookmarkText != null)
                                {

                                    if (DateTime.TryParse(dt.Rows[0]["addDoc_createdDate"].ToString(), out PublishedDate))
                                    {
                                        // Format the date to a string without the time component
                                        bookmarkText.GetFirstChild<Text>().Text = PublishedDate.ToString("dd-MM-yyyy");
                                    }
                                }
                            }
                            else if (bookmarkStart.Name == "addDoc_createdDate")
                            {
                                if (bookmarkText != null)
                                {
                                    DateTime addDoc_createdDate;
                                    if (DateTime.TryParse(dt.Rows[0]["addDoc_createdDate"].ToString(), out addDoc_createdDate))
                                    {
                                        // Format the date to a string without the time component
                                        bookmarkText.GetFirstChild<Text>().Text = addDoc_createdDate.ToString("dd-MM-yyyy");
                                    }
                                }
                            }
                            else if (bookmarkStart.Name == "SideVersionReporDate")
                            {
                                if (bookmarkText != null)
                                {
                                    DateTime VersionReporDate;
                                    if (DateTime.TryParse(dt.Rows[0]["addDoc_createdDate"].ToString(), out VersionReporDate))
                                    {
                                        // Format the date to a string without the time component
                                        bookmarkText.GetFirstChild<Text>().Text = VersionReporDate.ToString("dd-MM-yyyy");
                                    }
                                }
                            }
                            else if (bookmarkStart.Name == "VersionDate")
                            {
                                if (bookmarkText != null)
                                {
                                    DateTime VersionDate;
                                    if (DateTime.TryParse(dt.Rows[0]["addDoc_createdDate"].ToString(), out VersionDate))
                                    {
                                        // Format the date to a string without the time component
                                        bookmarkText.GetFirstChild<Text>().Text = VersionDate.ToString("dd-MM-yyyy");
                                    }
                                }
                            }
                            else if (bookmarkStart.Name == "VersionControlNo")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["VersionControlNo"].ToString();
                            }
                            else if (bookmarkStart.Name == "VersionControlNo2")
                            {
                                if (bookmarkText != null)
                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["VersionControlNo"].ToString();
                            }
                            else if (bookmarkStart.Name == "freqPeriod")
                            {
                                if (bookmarkText != null)
                                {
                                    string freqPeriod = dt.Rows[0]["freq_period"].ToString();
                                    string freqPeriodType = dt.Rows[0]["freq_period_type"].ToString();
                                    bookmarkText.GetFirstChild<Text>().Text = freqPeriod + " " + freqPeriodType;
                                }
                            }
                            else if (bookmarkStart.Name == "NameOfAuthority")
                            {
                                if (bookmarkText != null)
                                {
                                    string authorityName = dt.Rows[0]["AuthorityName"].ToString();
                                    string authorityTypeName = dt.Rows[0]["AuthorityTypeName"].ToString();
                                    bookmarkText.GetFirstChild<Text>().Text = authorityTypeName + "-" + authorityName;
                                }
                            }

                            else if (bookmarkStart.Name == "MaindocPages")
                            {
                                if (bookmarkText != null)
                                {

                                    bookmarkText.GetFirstChild<Text>().Text = data.MainpageCount.ToString();
                                }
                            }
                            else if (bookmarkStart.Name == "SupAttachments")
                            {
                                if (bookmarkText != null)
                                {

                                    bookmarkText.GetFirstChild<Text>().Text = data.supportFilesCount.ToString();
                                }
                            }
                        }
                        //string f1 = dt.Rows[0]["NCR"].ToString() + ".docx";
                        // ... (the rest of your code where you open the document)

                        // After updating the bookmarks in the main body, access the footers
                        foreach (FooterPart footerPart in doc.MainDocumentPart.FooterParts)
                        {
                            // Find all bookmarks in the current footer part
                            foreach (BookmarkStart bookmarkStart in footerPart.RootElement.Descendants<BookmarkStart>())
                            {
                                // Find the run that follows the bookmark start
                                Run bookmarkText = bookmarkStart.NextSibling<Run>();

                                // Check if this is the bookmark you want to update
                                if (bookmarkStart.Name == "NatureOf_Doc_Name1")
                                {
                                    if (bookmarkText != null)
                                    {
                                        // Update the text of the first Text element within the run
                                        bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["NatureOf_Doc_Name"].ToString();
                                    }
                                }
                            }
                        }

                        // ... (the rest of your code where you save and close the document)


                        doc.Close();

                    }
                    //string watermarkText = dt.Rows[0]["Document_Id"].ToString()+"_"+ dt.Rows[0]["VersionControlNo"].ToString()+"_"+PublishedDate.ToString();
                    //AddWatermarkToBody(outputPath, watermarkText);
                    //AddWatermark(outputPath,  "DO NOT COPY");




                }



                return outputPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw; // Re-throw the exception to be caught by the calling method
            }
        }






        private void ConvertDocxToPdf(string docxFilePath, string pdfFilePath)
        {
            var libreOfficePath = @"C:\Program Files\LibreOffice\program\soffice.exe"; // Update this path
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = libreOfficePath,
                    Arguments = $"--headless --convert-to pdf --outdir \"{Path.GetDirectoryName(pdfFilePath)}\" \"{docxFilePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }

        [Route("api/Adddocument/getentity/{userid}")]
        [HttpGet]

        public IEnumerable<object> getentity(int userid)
        {
            var entity = (from usermapping in mySqlDBContext.userlocationmappingModels
                          join entitymaster in mySqlDBContext.UnitMasterModels on usermapping.Entity_Master_id equals entitymaster.Entity_Master_id
                          where usermapping.user_location_mapping_status == "Active" && entitymaster.Entity_Master_Status == "Active" && usermapping.USR_ID == userid
                          select new
                          {
                              Entity_Master_id = usermapping.Entity_Master_id,
                              USR_ID = usermapping.USR_ID,
                              Entity_Master_Name = entitymaster.Entity_Master_Name
                          })
                         .Distinct()
                         .ToList();
            return entity;

        }

        [Route("api/getregisterunitlocation/getregisterunitlocationdetails")]
        [HttpGet]
        public IEnumerable<object> getregisterunitlocationdetails([FromQuery] int EntityID, [FromQuery] int userid)

        {
            var entity = (from usermapping in mySqlDBContext.userlocationmappingModels

                          join unitlocation in mySqlDBContext.UnitLocationMasterModels on usermapping.Unit_location_Master_id equals unitlocation.Unit_location_Master_id

                          where usermapping.user_location_mapping_status == "Active" && usermapping.Entity_Master_id == EntityID
                          && usermapping.USR_ID == userid

                          select new
                          {
                              Entity_Master_id = usermapping.Entity_Master_id,
                              USR_ID = usermapping.USR_ID,
                              Unit_location_Master_id = unitlocation.Unit_location_Master_id,
                              Unit_location_Master_name = unitlocation.Unit_location_Master_name
                          })
                   .Distinct()
                   .ToList();
            return entity;
        }


        [Route("api/Adddocument/getunitlocation")]
      [HttpGet]
        public IEnumerable<object> GetUnitLocation(int userid, [FromQuery] List<int> entityids)

        {
                   var entity = (from usermapping in mySqlDBContext.userlocationmappingModels

                          join unitlocation in mySqlDBContext.UnitLocationMasterModels on usermapping.Unit_location_Master_id equals unitlocation.Unit_location_Master_id

                          where usermapping.user_location_mapping_status == "Active"
                          && unitlocation.Unit_location_Master_Status == "Active"
                          && usermapping.USR_ID == userid
                          && entityids.Contains(usermapping.Entity_Master_id)
                           select new
                           {
                              Entity_Master_id = usermapping.Entity_Master_id,
                              USR_ID = usermapping.USR_ID,
                              Unit_location_Master_id = unitlocation.Unit_location_Master_id,
                              Unit_location_Master_name = unitlocation.Unit_location_Master_name
                           })
                          .Distinct()
                          .ToList();
                           return entity;
        }

        [Route("api/getAdddocument/getunitlocation")]
        [HttpGet]
        public IEnumerable<object> GetUnitLocation()
        {
            var entity = (from usermapping in mySqlDBContext.userlocationmappingModels

                          join unitlocation in mySqlDBContext.UnitLocationMasterModels on usermapping.Unit_location_Master_id equals unitlocation.Unit_location_Master_id

                          where usermapping.user_location_mapping_status == "Active"
                          && unitlocation.Unit_location_Master_Status == "Active"

                          select new
                          {
                              Entity_Master_id = usermapping.Entity_Master_id,
                              USR_ID = usermapping.USR_ID,
                              Unit_location_Master_id = unitlocation.Unit_location_Master_id,
                              Unit_location_Master_name = unitlocation.Unit_location_Master_name
                          })
                   .Distinct()
                   .ToList();
            return entity;
        }


        private void MergePdfFiles(string coverPagePdfPath, string mainPdfPath, string outputPdfPath)
        {
            using (PdfDocument pdf = new PdfDocument(new PdfWriter(outputPdfPath)))
            {
                PdfMerger merger = new PdfMerger(pdf);

                // Merge cover page PDF
                using (PdfDocument coverPage = new PdfDocument(new PdfReader(coverPagePdfPath)))
                {
                    merger.Merge(coverPage, 1, coverPage.GetNumberOfPages());
                }

                // Merge main document PDF
                using (PdfDocument mainPdf = new PdfDocument(new PdfReader(mainPdfPath)))
                {
                    merger.Merge(mainPdf, 1, mainPdf.GetNumberOfPages());
                }
            }
        }

        static int GetPdfPageCount(string filePath)
        {
            try
            {
                //using (PdfiumViewer.PdfDocument pdfDocument = PdfiumViewer.PdfDocument.Load(filePath))
                //{
                //    return pdfDocument.PageCount;
                //}
                using (PdfDocument mainPdf = new PdfDocument(new PdfReader(filePath)))
                {

                    return mainPdf.GetNumberOfPages();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return -1; // Return -1 if there's an error
            }
        }
        [Route("api/EntityMaster/GetEntityMaster")]
        [HttpGet]

        public IEnumerable<UnitMasterModel> GetEntityMaster()
        {
            return this.mySqlDBContext.UnitMasterModels.Where(x => x.Entity_Master_Status == "Active").ToList();
        }

        [Route("api/UnitLocation/GetUnitLocationMaster")]
        [HttpGet]

        public IEnumerable<UnitLocationMasterModel> GetUnitLocationMaster()
        {
            return this.mySqlDBContext.UnitLocationMasterModels.Where(x => x.Unit_location_Master_Status == "Active").ToList();
        }




    }



}
