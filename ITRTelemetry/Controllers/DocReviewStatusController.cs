using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System;
using MySQLProvider;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Charts;
using OpenXmlPowerTools;
using DocumentFormat.OpenXml.Drawing;
using DataTable = System.Data.DataTable;
using System.Threading.Tasks;
using System.IO;
using Path = System.IO.Path;

namespace ITRTelemetry.Controllers
{
    //  [Route("api/[controller]")]
    // [ApiController]
    [Produces("application/json")]
    public class DocReviewStatusController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;




        public DocReviewStatusController(MySqlDBContext mySqlDBContext, IConfiguration configuration , IHttpContextAccessor httpContextAccessor)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

        }

        //[Route("api/DocReviewStatus/GetDocReviewStatusbyid/{AddDoc_id}")]
        //[HttpGet]


        //public IEnumerable<dynamic> GetPublishedDraftDatabyid(int AddDoc_id)
        //{
        //    var connectionString = Configuration["ConnectionStrings:myDb1"];
        //    var query = "SELECT review_start_Date FROM risk.add_doc WHERE AddDoc_id = @AddDocId";

        //    using (var con = new MySqlConnection(connectionString))
        //    {
        //        con.Open();
        //        using (var cmd = new MySqlCommand(query, con))
        //        {
        //            cmd.Parameters.AddWithValue("@AddDocId", AddDoc_id);
        //           
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                var resultList = new List<dynamic>();
        //                while (reader.Read())
        //                {
        //                    var todayDate = DateTime.Today.ToString("yyyy-MM-dd");
        //                    var reviewStartDate = reader.GetDateTime(reader.GetOrdinal("review_start_Date")).ToString("yyyy-MM-dd");

        //                    resultList.Add(new
        //                    {
        //                        today_Date = todayDate,
        //                        review_start_Date = reviewStartDate
        //                    });
        //                }
        //                return resultList;
        //            }
        //        }
        //    }
        //}

        [Route("api/DocReviewStatus/GetDocReviewStatusbyid/{AddDoc_id}")]
        [HttpGet]


        public IEnumerable<ProvideAccessModel> GetDocReviewStatusbyid(int AddDoc_id)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            //MySqlCommand cmd = new MySqlCommand("SELECT e.USR_ID,a.AuthorityName,e.NatureOf_Doc_id,a.AuthoritynameID,e.Title_Doc,e.Sub_title_doc,e.Obj_Doc,e.Doc_Approver,e.freq_period_type,e.freq_period,e.Date_Doc_Revision,e.AddDoc_id,t.DocTypeName,e.Doc_SubCategoryID,e.Doc_CategoryID,e.DocTypeID,C.Doc_CategoryName,sc.Doc_SubCategoryName,e.review_start_Date,e.Doc_internal_num,p.NatureOf_Doc_Name,e.Doc_Inter_ver_num,e.Doc_process_Owner,e.Initial_creation_doc_date FROM risk.add_doc e  Left Outer JOIN  risk.doctype_master t ON t.DocTypeID = e.DocTypeID Left Outer JOIN risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID Left Outer JOIN risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID Left Outer JOIN risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID left Outer JOIN risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id left outer join risk.tbluser tbl on tbl.USR_ID = e.USR_ID  where e.AddDoc_id='" + AddDoc_id + "' && e.addDoc_Status = 'Active' && e.Draft_Status = 'Completed'; ", con);
            MySqlCommand cmd = new MySqlCommand("SELECT " + "e.USR_ID," + "e.AuthoritynameID," + "e.AuthorityTypeID," + "e.NatureOf_Doc_id," + "e.DocTypeID," + "e.Doc_SubCategoryID," + "e.Doc_CategoryID," + "e.Title_Doc," + "e.Doc_Confidentiality," + "e.Eff_Date," + "e.Initial_creation_doc_date," + "e.Doc_internal_num," + "e.Doc_Inter_ver_num," + "e.Doc_Phy_Valut_Loc," + "e.Doc_process_Owner," + "e.Doc_Approver," + "e.Date_Doc_Revision," +
              "e.Date_Doc_Approver," + "e.freq_period," + "e.Keywords_tags," + "e.pub_doc," + "e.publisher_comments," + "e.indicative_reading_time," + "e.Time_period," + "e.review_start_Date," + "e.freq_period_type," +
    "e.Sub_title_doc," + "e.AddDoc_id," + "e.OtpMethod," +
    "e.Obj_Doc," +
     "e.Review_Frequency_Status," +
      "e.Doc_Linking_Status," +
   "e.Document_Id," + "e.VersionControlNo," + "e.Linking_Doc_names," +
   "b.firstname," +
   "e.addDoc_createdDate," +
   "t.DocTypeName," +
   "C.Doc_CategoryName," +
   "sc.Doc_SubCategoryName," +
   "a.AuthorityName," +
  "at.AuthorityTypeName," +
   "p.NatureOf_Doc_Name," + "e.Entity_Master_id," + "e.Unit_location_Master_id" +
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
" Left Outer JOIN " +
   "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id where e.AddDoc_id ='" + AddDoc_id + "'  ;", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            System.Data.DataTable dt = new System.Data.DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ProvideAccessModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string review_start_DateString =(dt.Rows[i]["review_start_Date"].ToString()!="")? Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("dd-MM-yyyy"):"";
                    DateTime startdate = DateTime.ParseExact(review_start_DateString, "dd-MM-yyyy", null);
                    string todaysdate = Convert.ToDateTime(System.DateTime.Now).ToString("dd-MM-yyyy");
                    DateTime enddate = DateTime.ParseExact(todaysdate, "dd-MM-yyyy", null);
                    TimeSpan span = startdate - enddate;
                    int NoofDays = span.Days;
                    //string validation = "";
                    string validation = GetValidationMessage(NoofDays);

                    //if (NoofDays >= 0 && NoofDays <= 30)
                    //{
                    //    validation = "Take Immediate Action";
                        
                    //}
                    //else if (NoofDays < 0)
                    //{
                    //    validation = "Expired";
                
                    //}
                    //else if (NoofDays >= 31 && NoofDays <= 60)
                    //{

                    //    validation = "Expiring Soon";

                    //}
                    //else if (NoofDays >= 60)
                    //{

                    //    validation = "Not Due";
                    //}
                    pdata.Add(new ProvideAccessModel
                    {

                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        //Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                        NatureOf_Doc_id = Convert.ToInt32(dt.Rows[i]["NatureOf_Doc_id"].ToString()),
                        AuthoritynameID = Convert.ToInt32(dt.Rows[i]["AuthoritynameID"].ToString()),

                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        
                        review_start_Date = Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("yyyy-MM-dd"),
                        NoofDays = span.Days,
                        validations = validation,

                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),

                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                        Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                        Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        Date_Doc_Revision = Convert.ToDateTime(dt.Rows[i]["Date_Doc_Revision"]).ToString("yyyy-MM-dd"),
                        freq_period = dt.Rows[i]["freq_period"].ToString(),
                        freq_period_type = dt.Rows[i]["freq_period_type"].ToString(),
                        Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                       
                        Initial_creation_doc_date = Convert.ToDateTime(dt.Rows[i]["Initial_creation_doc_date"]).ToString("yyyy-MM-dd"),
                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        Sub_title_doc = dt.Rows[i]["Sub_title_doc"].ToString(),
                        Obj_Doc = dt.Rows[i]["Obj_Doc"].ToString(),



                    }); ; ; ;
                }
            }
            return pdata;

        }

        [Route("api/SavedDraftDocuments/GetPublishedData2/{RoleId}/{userid}")]
        [HttpGet]
        public IEnumerable<ProvideAccessModel> GetPublishedData(int RoleId, int userid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            string roleprivileges = (RoleId == 2) ? "" : "&& e.USR_ID=" + userid ;
            MySqlCommand cmd = new MySqlCommand("SELECT e.USR_ID,b.firstname,e.VersionControlNo,e.AuthoritynameID,e.Document_Id,e.review_start_Date,e.AuthorityTypeID,e.NatureOf_Doc_id,e.DocTypeID,e.Doc_SubCategoryID,e.Doc_CategoryID,e.Title_Doc,e.Doc_Confidentiality,e.Eff_Date,e.Initial_creation_doc_date,e.Doc_internal_num,e.Doc_Inter_ver_num,e.Doc_Phy_Valut_Loc,e.Doc_process_Owner,e.Doc_Approver,e.Date_Doc_Revision,e.Date_Doc_Approver,e.freq_period,e.Keywords_tags,e.pub_doc,e.publisher_comments,e.indicative_reading_time,e.Time_period,e.Sub_title_doc,e.AddDoc_id,e.Obj_Doc,e.Document_Id,t.DocTypeName,C.Doc_CategoryName,sc.Doc_SubCategoryName,a.AuthorityName,at.AuthorityTypeName,p.NatureOf_Doc_Name FROM risk.add_doc e Left Outer JOIN  " +
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
"risk.tblUser b ON b.USR_ID = e.USR_ID" +
" Left Outer JOIN " +
    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id" +

    " where e.addDoc_Status='Active' &&  e.Draft_Status='Completed' && e.review_start_Date is not null  "+ roleprivileges , con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            System.Data.DataTable dt = new System.Data.DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ProvideAccessModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    string review_start_DateString = Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("dd-MM-yyyy");
                    DateTime startdate = DateTime.ParseExact(review_start_DateString, "dd-MM-yyyy", null);
                    string todaysdate = Convert.ToDateTime(System.DateTime.Now).ToString("dd-MM-yyyy");
                    DateTime enddate = DateTime.ParseExact(todaysdate, "dd-MM-yyyy", null);
                    TimeSpan span = startdate - enddate;
                    int NoofDays = span.Days;
                    // string validation = "";
                    string validation = GetValidationMessage(NoofDays);

                    //if (NoofDays >= 0 && NoofDays <= 30)
                    //{
                    //    validation = "Take Immediate Action";
                    //}
                    //else if (NoofDays < 0)
                    //{

                    //    //this.daysDifference=0;
                    //    validation = "Expired";
                    //}
                    //else if (NoofDays >= 31 && NoofDays <= 60)
                    //{

                    //    validation = "Expiring Soon";

                    //}
                    //else if (NoofDays >= 60)
                    //{

                    //    validation = "Not Due";
                    //}
                    pdata.Add(new ProvideAccessModel
                    {
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                        Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        Publisher_name = dt.Rows[i]["firstname"].ToString(),
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
                        //Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                        Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                        Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                        Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                        Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                        Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                        Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),
                        VersionControlNo = dt.Rows[i]["VersionControlNo"].ToString(),
                      
                        review_start_Date = Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("dd-MM-yyyy"),
                        NoofDays = span.Days,
                        validations = validation,
                        USR_ID = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),
                        freq_period = dt.Rows[i]["freq_period"].ToString(),
                        Keywords_tags = dt.Rows[i]["Keywords_tags"].ToString(),
                        pub_doc = dt.Rows[i]["pub_doc"].ToString(),
                        publisher_comments = dt.Rows[i]["publisher_comments"].ToString(),
                        indicative_reading_time = dt.Rows[i]["indicative_reading_time"].ToString(),
                        Time_period = dt.Rows[i]["Time_period"].ToString(),
                        Initial_creation_doc_date = Convert.ToDateTime(dt.Rows[i]["Initial_creation_doc_date"]).ToString("yyyy-MM-dd"),


                    }); ; ; ;
                }
            }
            return pdata;

        }
        [Route("api/ReviewStatus/UpdatePublishedDocument")]
        [HttpPost]
        public IActionResult UpdatePublishedDoc([FromBody] UpdateDoc AddDocumentModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                con.Open();
                int AddDoc_Id = AddDocumentModels.AddDoc_id;

                string insertQuery = "INSERT INTO risk.add_doc_old_data (DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Title_Doc, Sub_title_doc, Eff_Date, Initial_creation_doc_date, Doc_internal_num, Doc_Inter_ver_num, Doc_Phy_Valut_Loc, Doc_process_Owner, Doc_Approver, Date_Doc_Revision, Date_Doc_Approver, AuthorityTypeID, AuthoritynameID, NatureOf_Doc_id, Doc_Confidentiality, indicative_reading_time, Keywords_tags, freq_period_type, freq_period, review_start_Date, pub_doc, Time_period, Obj_Doc, Doc_referenceNo, Revision_summary, AddDoc_id, addDoc_Status, VersionControlNo) " +
    "SELECT DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Title_Doc, Sub_title_doc, Eff_Date, Initial_creation_doc_date, Doc_internal_num, Doc_Inter_ver_num, Doc_Phy_Valut_Loc, Doc_process_Owner, Doc_Approver, Date_Doc_Revision, Date_Doc_Approver, AuthorityTypeID, AuthoritynameID, NatureOf_Doc_id, Doc_Confidentiality, indicative_reading_time, Keywords_tags, freq_period_type, freq_period, review_start_Date, pub_doc, Time_period, Obj_Doc, Doc_referenceNo, Revision_summary, AddDoc_id, 'InActive' AS addDoc_Status, VersionControlNo FROM risk.add_doc WHERE AddDoc_id = @AddDoc_id";

                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@AddDoc_id", AddDoc_Id);
                    myCommand1.ExecuteNonQuery();
                }


                MySqlCommand cmd = new MySqlCommand("SELECT * from add_doc where AddDoc_id='" + AddDoc_Id + "'", con);

                cmd.CommandType = CommandType.Text;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                System.Data.DataTable dt = new System.Data.DataTable();
                da.Fill(dt);

                string newVersionControlNo;
                if (dt.Rows.Count > 0)
                {
                    string VersionControlNo = dt.Rows[0]["VersionControlNo"].ToString();
                    string documentid = dt.Rows[0]["Document_Id"].ToString();
                    AddDocumentModels.Document_Id = documentid;
                    if (decimal.TryParse(VersionControlNo, out decimal versionDecimal))
                    {
                        decimal newVersionControlNoDecimal = versionDecimal + 0.1M;

                        // Use Math.Round to round the result to one decimal place
                        newVersionControlNoDecimal = Math.Round(newVersionControlNoDecimal, 1);

                        newVersionControlNo = newVersionControlNoDecimal.ToString();

                        var documentidfolder = Path.Combine("Resources", documentid);
                        var VersionFolder = Path.Combine(documentidfolder, newVersionControlNo);
                        DirectoryInfo VersionFolderPath = Directory.CreateDirectory(VersionFolder);//version generation in document id folder 


                        string UpdateQuery = @"
                                    UPDATE add_doc 
                                    SET 
                                        publisher_comments = @publisher_comments, 
                                        Doc_Linking_Status = @Doc_Linking_Status, 
                                        Review_Frequency_Status = @Review_Frequency_Status, 
                                        VersionControlNo = @VersionControlNo, 
                                        Title_Doc = @Title_Doc, 
                                        Sub_title_doc = @Sub_title_doc, 
                                        Obj_Doc = @Obj_Doc, 
                                        DocTypeID = @DocTypeID, 
                                        Doc_CategoryID = @Doc_CategoryID, 
                                        Doc_SubCategoryID = @Doc_SubCategoryID, 
                                        Eff_Date = @Eff_Date, 
                                        Initial_creation_doc_date = @Initial_creation_doc_date, 
                                        Doc_internal_num = @Doc_internal_num, 
                                        Doc_Inter_ver_num = @Doc_Inter_ver_num, 
                                        Doc_Phy_Valut_Loc = @Doc_Phy_Valut_Loc, 
                                        Doc_process_Owner = @Doc_process_Owner, 
                                        Doc_Approver = @Doc_Approver, 
                                        Date_Doc_Approver = @Date_Doc_Approver, 
                                        Date_Doc_Revision = @Date_Doc_Revision, 
                                        AuthorityTypeID = @AuthorityTypeID, 
                                        AuthoritynameID = @AuthoritynameID, 
                                        Doc_Confidentiality = @Doc_Confidentiality, 
                                        NatureOf_Doc_id = @NatureOf_Doc_id, 
                                        indicative_reading_time = @indicative_reading_time, 
                                        Keywords_tags = @Keywords_tags, 
                                        freq_period_type = @freq_period_type, 
                                        freq_period = @freq_period, 
                                        review_start_Date = @review_start_Date, 
                                        pub_doc = @pub_doc, 
                                        USR_ID = @USR_ID, 
                                        Time_period = @Time_period, 
                                        Linking_Doc_names=@Linking_Doc_names,
                                           ChangedBy=@ChangedBy,
                                         ChangedOn=@ChangedOn
                                    WHERE AddDoc_id = @AddDoc_id";


                        using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                        {
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

                                myCommand.Parameters.AddWithValue("@Linking_Doc_names", titleDocs);

                            }
                            else
                            {
                                myCommand.Parameters.AddWithValue("@Linking_Doc_names", DBNull.Value);
                            }
                            myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                            myCommand.Parameters.AddWithValue("@Title_Doc", AddDocumentModels.Title_Doc);
                            myCommand.Parameters.AddWithValue("@Sub_title_doc", AddDocumentModels.Sub_title_doc);
                            myCommand.Parameters.AddWithValue("@Obj_Doc", AddDocumentModels.Obj_Doc);
                            myCommand.Parameters.AddWithValue("@DocTypeID", AddDocumentModels.DocTypeID);
                            myCommand.Parameters.AddWithValue("@Doc_CategoryID", AddDocumentModels.Doc_CategoryID);
                            myCommand.Parameters.AddWithValue("@Doc_SubCategoryID", AddDocumentModels.Doc_SubCategoryID);

                            myCommand.Parameters.AddWithValue("@Doc_Confidentiality", AddDocumentModels.Doc_Confidentiality);
                            myCommand.Parameters.AddWithValue("@Eff_Date", AddDocumentModels.Eff_Date);
                            myCommand.Parameters.AddWithValue("@Initial_creation_doc_date", AddDocumentModels.Initial_creation_doc_date);
                            myCommand.Parameters.AddWithValue("@Doc_internal_num", AddDocumentModels.Doc_internal_num);
                            myCommand.Parameters.AddWithValue("@Doc_Inter_ver_num", AddDocumentModels.Doc_Inter_ver_num);
                            myCommand.Parameters.AddWithValue("@Doc_Phy_Valut_Loc", AddDocumentModels.Doc_Phy_Valut_Loc);
                            myCommand.Parameters.AddWithValue("@Doc_process_Owner", AddDocumentModels.Doc_process_Owner);
                            myCommand.Parameters.AddWithValue("@Doc_Approver", AddDocumentModels.Doc_Approver);
                            myCommand.Parameters.AddWithValue("@Date_Doc_Approver", AddDocumentModels.Date_Doc_Approver);
                            myCommand.Parameters.AddWithValue("@Date_Doc_Revision", AddDocumentModels.Date_Doc_Revision);
                            myCommand.Parameters.AddWithValue("@AuthorityTypeID", AddDocumentModels.AuthorityTypeID);
                            myCommand.Parameters.AddWithValue("@AuthoritynameID", AddDocumentModels.AuthoritynameID);
                            myCommand.Parameters.AddWithValue("@NatureOf_Doc_id", AddDocumentModels.NatureOf_Doc_id);
                            myCommand.Parameters.AddWithValue("@Keywords_tags", AddDocumentModels.Keywords_tags);

                            myCommand.Parameters.AddWithValue("@freq_period", AddDocumentModels.freq_period);
                            myCommand.Parameters.AddWithValue("@freq_period_type", AddDocumentModels.freq_period_type);
                            myCommand.Parameters.AddWithValue("@review_start_Date", AddDocumentModels.review_start_Date);
                            myCommand.Parameters.AddWithValue("@pub_doc", AddDocumentModels.pub_doc);
                            myCommand.Parameters.AddWithValue("@publisher_comments", AddDocumentModels.publisher_comments);
                            myCommand.Parameters.AddWithValue("@indicative_reading_time", AddDocumentModels.indicative_reading_time);
                            myCommand.Parameters.AddWithValue("@Time_period", AddDocumentModels.Time_period);
                            myCommand.Parameters.AddWithValue("@Review_Frequency_Status", AddDocumentModels.Review_Frequency_Status);
                            myCommand.Parameters.AddWithValue("@Doc_Linking_Status", AddDocumentModels.Doc_Linking_Status);
                            myCommand.Parameters.AddWithValue("@VersionControlNo", newVersionControlNo);
                            myCommand.Parameters.AddWithValue("@ChangedBy", AddDocumentModels.ChangedBy);
                            myCommand.Parameters.AddWithValue("@ChangedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            myCommand.Parameters.AddWithValue("@USR_ID", AddDocumentModels.USR_ID);
                            myCommand.ExecuteNonQuery();


                            string updateStatusQuery = "UPDATE documentrepository SET Status = 'InActive' WHERE AddDoc_id = @AddDoc_id AND FileCategory IN ('Cover Page', 'Published')";
                            using (MySqlCommand myCmd = new MySqlCommand(updateStatusQuery, con))
                            {
                                myCmd.Parameters.AddWithValue("@AddDoc_id", AddDoc_Id);
                                myCmd.ExecuteNonQuery();
                            }


                            //string coverpagepdf;
                            //var reportPath = GenerateReport(AddDocumentModels, VersionFolderPath.FullName);
                            //coverpagepdf = Path.ChangeExtension(reportPath, "pdf");
                            //ConvertDocxToPdf(reportPath, coverpagepdf);

                            //string coverPageFileName = Path.GetFileName(coverpagepdf);
                            //string coverPageRelativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), coverpagepdf).Replace("\\", "/");
                            //string coverPageUrl = $"{baseUrl}/{coverPageRelativePath}";
                            //// Add cover page to database
                            //var coverPageUploadModel = new DocumentFilesuplodModel
                            //{
                            //    Document_Name = coverPageFileName,
                            //    FilePath = coverPageUrl,
                            //    FileCategory = "Cover Page",
                            //    Status = "Active",
                            //    documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            //    AddDoc_id = AddDoc_Id,
                            //    Document_Id = AddDocumentModels.Document_Id,
                            //    VersionControlNo = newVersionControlNo
                            //};
                            //mySqlDBContext.DocumentFilesuplodModels.Add(coverPageUploadModel);


                            string existingMainFilePath = "";

                            string selectMainFileQuery = "SELECT FilePath FROM documentrepository WHERE AddDoc_id = @AddDoc_id AND FileCategory = 'Main' AND Status = 'Active'";
                            string mainFilePath = string.Empty;

                            using (MySqlCommand selectMainFileCmd = new MySqlCommand(selectMainFileQuery, con))
                            {
                                selectMainFileCmd.Parameters.AddWithValue("@AddDoc_id", AddDoc_Id);
                                using (var reader = selectMainFileCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        string fileUrl = reader["FilePath"].ToString();
                                        existingMainFilePath = ConvertUrlToLocalPath(fileUrl, request);
                                    }
                                }
                            }

                            // Check if the main file path has been retrieved
                            if (!string.IsNullOrWhiteSpace(existingMainFilePath))
                            {

                                if (System.IO.File.Exists(existingMainFilePath))
                                {
                                    string mergedPdfFileName = "Published.pdf";
                                    string mergedPdfPath = Path.Combine(VersionFolderPath.FullName, mergedPdfFileName); // Path for the merged PDF file

                                    // Use the existing main file to merge with the cover page


                                   // MergePdfFiles(coverpagepdf, existingMainFilePath, mergedPdfPath);

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
                                        AddDoc_id = AddDoc_Id,
                                        Document_Id = AddDocumentModels.Document_Id,
                                        VersionControlNo = newVersionControlNo
                                    };
                                    mySqlDBContext.DocumentFilesuplodModels.Add(mergedPdfUploadModel);
                                    mySqlDBContext.SaveChanges();
                                }
                                else
                                {
                                    return BadRequest("The main file does not exist on the server.");
                                }
                            }
                            else
                            {
                                return BadRequest("Main file path could not be retrieved from the database.");
                            }




                            mySqlDBContext.SaveChanges();
                        }


                    }
                }

                return Ok("Updated Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            finally
            {
                con.Close();
            }

        }
        private string ConvertUrlToLocalPath(string fileUrl, HttpRequest request)
        {
            // Get the base URL from the request
            string baseUrl = $"{request.Scheme}://{request.Host}";

            // Replace the base URL with the server's local path to the Resources directory
            string resourcesPath = Path.Combine(Directory.GetCurrentDirectory());
            string localPath = fileUrl.Replace(baseUrl, resourcesPath);

            // Convert virtual path to absolute path
            localPath = localPath.Replace("/", Path.DirectorySeparatorChar.ToString());

            return localPath;
        }
        //**Update**//



        [Route("api/ReviewStatus/UpdateversionchangeDocument")]
        [HttpPost]

        public async Task<IActionResult> UpdateversionchangeDocument([FromForm] ProvideAccessModel ProvideAccessModels, [FromForm] IFormFile mainFile, [FromForm] List<IFormFile> supportFiles)
        {

            var httpContext = _httpContextAccessor.HttpContext;

            // Get the current HTTP request
            var request = HttpContext.Request;

            // Build the base URL from the request
            string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);


            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            if (ProvideAccessModels != null)
            {

                try
                {

                    con.Open();
                    int AddDoc_Id = ProvideAccessModels.AddDoc_id;
                    MySqlCommand cmd = new MySqlCommand("SELECT * from add_doc where AddDoc_id='" + AddDoc_Id + "'", con);

                    cmd.CommandType = CommandType.Text;

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    string newVersionControlNo;
                    if (dt.Rows.Count > 0)
                    {
                        string VersionControlNo = dt.Rows[0]["VersionControlNo"].ToString();
                        string documentid = dt.Rows[0]["Document_Id"].ToString();
                        ProvideAccessModels.Document_Id = documentid;



                        if (decimal.TryParse(VersionControlNo, out decimal versionDecimal))
                        {
                            string currentVersion = VersionControlNo; // Replace this with your actual version string

                            // Split the version string into major and minor parts
                            string[] parts = currentVersion.Split('.');
                            int majorVersion = int.Parse(parts[0]);

                            // Increment the major version and set the minor version to 0
                            majorVersion++;
                            int minorVersion = 0;

                            // Concatenate the parts to form the new version string
                            string newVersion = $"{majorVersion}.{minorVersion}";
                            //newVersionControlNo = VersionControlNo + 1;

                            string documentuniqueid = dt.Rows[0]["Document_Id"].ToString();

                            var documentidfolder = System.IO.Path.Combine("Resources", documentid);
                            var VersionFolder = System.IO.Path.Combine(documentidfolder, newVersion);
                            DirectoryInfo VersionFolderPath = Directory.CreateDirectory(VersionFolder);



                            // update to inactive

                            string UpdateQuery = "update add_doc set addDoc_Status='InActive' where AddDoc_id='" + AddDoc_Id + "'";
                            using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                            {
                                myCommand.ExecuteNonQuery();

                            }

                            // insert new record with updated version

                            string insertquery = "INSERT INTO add_doc ( DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Title_Doc, Sub_title_doc, Eff_Date, Initial_creation_doc_date, Doc_internal_num, Doc_Inter_ver_num, Doc_Phy_Valut_Loc, Doc_process_Owner, Doc_Approver, Date_Doc_Revision, Date_Doc_Approver, AuthorityTypeID, AuthoritynameID, NatureOf_Doc_id, Doc_Confidentiality, indicative_reading_time, Keywords_tags, freq_period_type, freq_period, review_start_Date, pub_doc, Time_period, Obj_Doc, addDoc_Status,VersionControlNo,Document_Id,Draft_Status,publisher_comments) VALUES (@DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID," +
                                " @Title_Doc, @Sub_title_doc, @Eff_Date, @Initial_creation_doc_date, @Doc_internal_num, @Doc_Inter_ver_num, @Doc_Phy_Valut_Loc, @Doc_process_Owner, @Doc_Approver, @Date_Doc_Revision, @Date_Doc_Approver, @AuthorityTypeID, @AuthoritynameID, @NatureOf_Doc_id, @Doc_Confidentiality, @indicative_reading_time, @Keywords_tags, @freq_period_type, @freq_period, @review_start_Date, @pub_doc, @Time_period, @Obj_Doc, @addDoc_Status,@VersionControlNo,@Document_Id,@Draft_Status,@publisher_comments,@addDoc_createdDate)";
                            using (MySqlCommand myCommand = new MySqlCommand(insertquery, con))
                            {


                                myCommand.Parameters.AddWithValue("@DocTypeID", ProvideAccessModels.DocTypeID);
                                myCommand.Parameters.AddWithValue("@Doc_CategoryID", ProvideAccessModels.Doc_CategoryID);
                                myCommand.Parameters.AddWithValue("@Doc_SubCategoryID", ProvideAccessModels.Doc_SubCategoryID);
                                myCommand.Parameters.AddWithValue("@Title_Doc", ProvideAccessModels.Title_Doc);
                                myCommand.Parameters.AddWithValue("@Sub_title_doc", ProvideAccessModels.Sub_title_doc);
                                myCommand.Parameters.AddWithValue("@Obj_Doc", ProvideAccessModels.Obj_Doc);
                                myCommand.Parameters.AddWithValue("@publisher_comments", ProvideAccessModels.publisher_comments);


                                myCommand.Parameters.AddWithValue("@Eff_Date", ProvideAccessModels.Eff_Date);
                                myCommand.Parameters.AddWithValue("@Initial_creation_doc_date", ProvideAccessModels.Initial_creation_doc_date);
                                myCommand.Parameters.AddWithValue("@Doc_internal_num", ProvideAccessModels.Doc_internal_num);
                                myCommand.Parameters.AddWithValue("@Doc_Inter_ver_num", ProvideAccessModels.Doc_Inter_ver_num);
                                myCommand.Parameters.AddWithValue("@Doc_Phy_Valut_Loc", ProvideAccessModels.Doc_Phy_Valut_Loc);
                                myCommand.Parameters.AddWithValue("@Doc_process_Owner", ProvideAccessModels.Doc_process_Owner);
                                myCommand.Parameters.AddWithValue("@Doc_Approver", ProvideAccessModels.Doc_Approver);
                                myCommand.Parameters.AddWithValue("@AuthorityTypeID", ProvideAccessModels.AuthorityTypeID);
                                myCommand.Parameters.AddWithValue("@AuthoritynameID", ProvideAccessModels.AuthoritynameID);
                                myCommand.Parameters.AddWithValue("@NatureOf_Doc_id", ProvideAccessModels.NatureOf_Doc_id);
                                myCommand.Parameters.AddWithValue("@Doc_Confidentiality", ProvideAccessModels.Doc_Confidentiality);
                                myCommand.Parameters.AddWithValue("@indicative_reading_time", ProvideAccessModels.indicative_reading_time);
                                myCommand.Parameters.AddWithValue("@Keywords_tags", ProvideAccessModels.Keywords_tags);
                                myCommand.Parameters.AddWithValue("@freq_period_type", ProvideAccessModels.freq_period_type);
                                myCommand.Parameters.AddWithValue("@pub_doc", ProvideAccessModels.pub_doc);
                                myCommand.Parameters.AddWithValue("@Time_period", ProvideAccessModels.Time_period);
                                myCommand.Parameters.AddWithValue("@review_start_Date", ProvideAccessModels.review_start_Date);
                                myCommand.Parameters.AddWithValue("@Date_Doc_Revision", ProvideAccessModels.Date_Doc_Revision);
                                myCommand.Parameters.AddWithValue("@Date_Doc_Approver", ProvideAccessModels.Date_Doc_Approver);
                                myCommand.Parameters.AddWithValue("@freq_period", ProvideAccessModels.freq_period);
                                myCommand.Parameters.AddWithValue("@VersionControlNo", newVersion);
                                myCommand.Parameters.AddWithValue("@Document_Id", documentuniqueid);
                                myCommand.Parameters.AddWithValue("@Draft_Status", "Completed");
                                myCommand.Parameters.AddWithValue("@addDoc_Status", "Active");

                                myCommand.Parameters.AddWithValue("@addDoc_createdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                myCommand.ExecuteNonQuery();

                                int AddDocid = Convert.ToInt32(myCommand.LastInsertedId.ToString());

                           

                            string insertQuery2 = "insert into document_revision_mapping(Doc_referenceNo, Revision_summary, AddDoc_id, VersionControlNo, Document_Id) values (@Doc_referenceNo, @Revision_summary, @AddDoc_id, @VersionControlNo, @Document_Id)";

                            for (int i = 0; i < ProvideAccessModels.Doc_referenceNo.Count; i++)
                            {
                                UpdateDoc_referenceNo Doc_referenceNo = ProvideAccessModels.Doc_referenceNo[i];
                                UpdateRevision_summary Revision_summary = ProvideAccessModels.Revision_summary[i];

                                string dt1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                if (Doc_referenceNo.id == 0)
                                {
                                    using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery2, con))
                                    {
                                        myCommand2.Parameters.AddWithValue("@Doc_referenceNo", Doc_referenceNo.value);
                                        myCommand2.Parameters.AddWithValue("@Revision_summary", Revision_summary.value);
                                        myCommand2.Parameters.AddWithValue("@AddDoc_id", ProvideAccessModels.AddDoc_id);
                                        myCommand2.Parameters.AddWithValue("@Document_Id", documentuniqueid);
                                        myCommand2.Parameters.AddWithValue("@VersionControlNo", newVersion);

                                        myCommand2.ExecuteNonQuery();
                                    }
                                }
                            }

                            var mainFilesFolder = Directory.CreateDirectory(System.IO.Path.Combine(VersionFolder, "Main"));
                            var supportFilesFolder = Directory.CreateDirectory(System.IO.Path.Combine(VersionFolder, "Support"));
                            string coverpagepdf;
                            var reportPath = GenerateReport(ProvideAccessModels, VersionFolderPath.FullName);
                            coverpagepdf = System.IO.Path.ChangeExtension(reportPath.ToString(), "pdf");
                            ConvertDocxToPdf(reportPath, coverpagepdf);

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
                                AddDoc_id = AddDocid
                            };
                            mySqlDBContext.DocumentFilesuplodModels.Add(coverPageUploadModel);
                            if (string.IsNullOrEmpty(reportPath.ToString()))
                                {
                                    return BadRequest("Report generation failed.");
                                }


                            if (mainFile != null && mainFile.Length > 0)
                            {
                                var mainFilePath = System.IO.Path.Combine(mainFilesFolder.FullName, mainFile.FileName);

                                using (var stream = new FileStream(mainFilePath, FileMode.Create))
                                {
                                    await mainFile.CopyToAsync(stream);
                                }
                                string pdfFilePath;
                                string mergedPdfFileName = "Published.pdf";
                                string mergedPdfPath = System.IO.Path.Combine(VersionFolderPath.FullName, mergedPdfFileName); // Path for the merged PDF file
                                                                                                                    // Check if the file is already a PDF
                                if (System.IO.Path.GetExtension(mainFile.FileName).ToLower() == ".pdf")
                                {
                                    pdfFilePath = mainFilePath; // Use the uploaded file directly if it's a PDF
                                    MergePdfFiles(coverpagepdf, pdfFilePath, mergedPdfPath);
                                }
                                else
                                {
                                    // Convert to PDF if it's not a PDF
                                    pdfFilePath = System.IO.Path.ChangeExtension(mainFilePath, "pdf");
                                    ConvertDocxToPdf(mainFilePath, pdfFilePath);
                                    MergePdfFiles(coverpagepdf, pdfFilePath, mergedPdfPath);
                                }
                                // Generate the URL for the merged PDF
                                string mergedPdfFile = System.IO.Path.GetFileName(mergedPdfPath);
                                string mergedPdfRelativePath = System.IO.Path.GetRelativePath(Directory.GetCurrentDirectory(), mergedPdfPath).Replace("\\", "/");
                                string mergedPdfUrl = $"{baseUrl}/{mergedPdfRelativePath}";

                                // Add merged PDF to database
                                var mergedPdfUploadModel = new DocumentFilesuplodModel
                                {
                                    Document_Name = mergedPdfFile,
                                    FilePath = mergedPdfUrl,
                                    FileCategory = "Published",
                                    Status = "Active",
                                    documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    AddDoc_id = ProvideAccessModels.AddDoc_id
                                };
                                mySqlDBContext.DocumentFilesuplodModels.Add(mergedPdfUploadModel);


                                // Extract the file name from pdfFilePath
                                string fileName = System.IO.Path.GetFileName(pdfFilePath);


                                // Use the relative path from the application's root to the file
                                string relativePath = System.IO.Path.GetRelativePath(Directory.GetCurrentDirectory(), pdfFilePath).Replace("\\", "/");

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
                                    AddDoc_id = ProvideAccessModels.AddDoc_id
                                };

                                // Add to the database context
                                mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                            }
                            else
                            {
                                return BadRequest("Main file is required.");
                            }


                            foreach (var file in supportFiles)
                            {
                                if (file.Length > 0)
                                {
                                    // Save the file
                                    var filePath = System.IO.Path.Combine(supportFilesFolder.FullName, file.FileName);

                                    using (var stream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await file.CopyToAsync(stream);
                                    }

                                    // Generate URL for the file
                                    string fileName = System.IO.Path.GetFileName(filePath);
                                    string relativePath = System.IO.Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath).Replace("\\", "/");
                                    string fileUrl = $"{baseUrl}/{relativePath}";

                                    // Create a file upload record with the generated URL

                                    var fileUploadModel = new DocumentFilesuplodModel
                                    {
                                        Document_Name = fileName,
                                        FilePath = fileUrl,
                                        FileCategory = "Support",
                                        Status = "Active",
                                        documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        AddDoc_id = AddDocid
                                    };

                                    // Add to the database context
                                    mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                                }
                            }
                                await mySqlDBContext.SaveChangesAsync();
                            } 
                        }
                    }

                    return Ok("Updated Successfully");
                }


                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
                finally
                {
                    con.Close();
                }

            }
            else
            {
                return BadRequest("Updation Failed");
            }

        }

        private void MergePdfFiles(string coverpagepdf, string pdfFilePath, string mergedPdfPath)
        {
            throw new NotImplementedException();
        }

        private void ConvertDocxToPdf(object reportPath, string coverpagepdf)
        {
            throw new NotImplementedException();
        }

        private object GenerateReport(ProvideAccessModel provideAccessModels, string fullName)
        {
            throw new NotImplementedException();
        }

        //*Version*//

        [Route("api/ReviewStartDate/DisableGetPublishedDatabyid/{AddDoc_id}")]
        [HttpGet]


        public IEnumerable<ProvideAccessModel> DisableGetPublishedDatabyid(int AddDoc_id)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT " + "e.AuthoritynameID," + "e.AuthorityTypeID," + "e.NatureOf_Doc_id," + "e.DocTypeID," + "e.Doc_SubCategoryID," + "e.Doc_CategoryID," + "e.Title_Doc," + "e.Doc_Confidentiality," + "e.Eff_Date," + "e.Initial_creation_doc_date," + "e.Doc_internal_num," + "e.Doc_Inter_ver_num," + "e.Doc_Phy_Valut_Loc," + "e.Doc_process_Owner," + "e.Doc_Approver," + "e.Date_Doc_Revision," +
               "e.Date_Doc_Approver," + "e.freq_period," + "e.Keywords_tags," + "e.pub_doc," + "e.publisher_comments," + "e.indicative_reading_time," + "e.Time_period," + "e.review_start_Date," + "e.freq_period_type," +
     "e.Sub_title_doc," + "e.AddDoc_id," +
     "e.Obj_Doc," +
      "e.Review_Frequency_Status," +
       "e.Doc_Linking_Status," +
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
    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id where e.AddDoc_id ='" + AddDoc_id + "';", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            System.Data.DataTable dt = new System.Data.DataTable();
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
                        //Document_Id = Convert.ToInt32(dt.Rows[i]["Document_Id"].ToString()),
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
                        review_start_Date = dt.Rows[i]["review_start_Date"].ToString(),
                        freq_period_type = dt.Rows[i]["freq_period_type"].ToString(),
                        freq_period = dt.Rows[i]["freq_period"].ToString(),
                        Keywords_tags = dt.Rows[i]["Keywords_tags"].ToString(),
                        pub_doc = dt.Rows[i]["pub_doc"].ToString(),
                        publisher_comments = dt.Rows[i]["publisher_comments"].ToString(),
                        indicative_reading_time = dt.Rows[i]["indicative_reading_time"].ToString(),
                        Time_period = dt.Rows[i]["Time_period"].ToString(),
                        Review_Frequency_Status = Review_Frequency_Status,
                        Doc_Linking_Status = Doc_Linking_Status


                    });
                }
            }
            return pdata;

        }

        [Route("api/ReviewStartDate/UpdateDisablePublishedDoc")]
        [HttpPost]
        public IActionResult UpdateDisablePublishedDoc([FromBody] UpdateDoc AddDocumentModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string UpdateQuery = "UPDATE add_doc SET " + "DisableReason=@DisableReason, " + "addDoc_Status=@addDoc_Status " + "WHERE AddDoc_id = @AddDoc_id";

            try
            {
                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                    myCommand.Parameters.AddWithValue("@DisableReason", AddDocumentModels.DisableReason);
                    myCommand.Parameters.AddWithValue("@addDoc_Status", "Disabled");

                    myCommand.ExecuteNonQuery();
                }

                return Ok("Updated Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            finally
            {
                con.Close();
            }
        }
        //**Disable**//

        public string GetValidationMessage(int NoofDays)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string validation = "";
         
            MySqlDataAdapter da = new MySqlDataAdapter($"SELECT * FROM reviewstatussettings WHERE {NoofDays} >=MinimumDays and {NoofDays}<=MaximumDays and (ReviewStatusName='Expiring Soon' or ReviewStatusName='Take Immediate Action') ", con);
            DataTable dt=new DataTable();
            da.Fill(dt);
            
            if(dt.Rows.Count > 0)
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
    }

}
