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
using DocumentFormat.OpenXml.Drawing.Charts;
using OpenXmlPowerTools;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class UpdateDocController:ControllerBase
    {


        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        public UpdateDocController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }




        [Route("api/SavedDraftDocument/GetPublishedData/{RoleId}/{userid}")]
        [HttpGet]
        public IEnumerable<ProvideAccessModel> GetPublishedData(int RoleId, int userid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();


           // MySqlCommand cmdnew = new MySqlCommand("SELECT Entity_Master_id, Unit_location_Master_id,USR_ID FROM tbluser where roles='" + RoleId + "' ", con);
            if (RoleId == 2)
            {

                MySqlCommand cmd = new MySqlCommand(@"SELECT 
                                e.AuthoritynameID,
                                tbl.firstname as publisher_name,
                                e.AuthorityTypeID,
                                e.NatureOf_Doc_id,
                                e.DocTypeID,
                                e.Doc_SubCategoryID,
                                e.Doc_CategoryID,
                                e.Title_Doc,
                                e.Doc_Confidentiality,
                                e.Eff_Date, e.Sub_title_doc,  e.Document_Id,  e.addDoc_createdDate,  e.VersionControlNo, e.freq_period_type,
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
                                e.Time_period,
                                e.Sub_title_doc,
                                e.AddDoc_id,
                                e.Obj_Doc,
                                e.Document_Id,
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
                          left outer join 
                                risk.tbluser tbl on tbl.USR_ID = e.USR_ID

                            WHERE 
                                e.addDoc_Status = 'Active' AND e.Draft_Status = 'Completed';
                            ", con);

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
                            //  document_name = dt.Rows[i]["document_name"].ToString(),
                            NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                            DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                            Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                            Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                            Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"].ToString(),
                            Eff_Date = dt.Rows[i]["Eff_Date"].ToString(),
                            addDoc_createdDate = dt.Rows[i]["addDoc_createdDate"].ToString(),
                            Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                            Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                            Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                            Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                            Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                            Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                            Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),


                            Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                            VersionControlNo = dt.Rows[i]["VersionControlNo"].ToString(),


                            Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                            freq_period = dt.Rows[i]["freq_period"].ToString() + " " + dt.Rows[i]["freq_period_type"].ToString(),
                            // freq_period_type = dt.Rows[i]["freq_period_type"].ToString(),
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


            else
            {

                MySqlCommand cmd = new MySqlCommand(@"SELECT 
                                e.AuthoritynameID,
                                tbl.firstname as publisher_name,
                                e.AuthorityTypeID,
                                e.NatureOf_Doc_id,
                                e.DocTypeID,
                                e.Doc_SubCategoryID,
                                e.Doc_CategoryID,
                                e.Title_Doc,
                                e.Doc_Confidentiality,
                                e.Eff_Date, e.Sub_title_doc,  e.Document_Id,  e.addDoc_createdDate,  e.VersionControlNo, e.freq_period_type,
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
                                e.Time_period,
                                e.Sub_title_doc,
                                e.AddDoc_id,
                                e.Obj_Doc,
                                e.Document_Id,
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
                          left outer join 
                                risk.tbluser tbl on tbl.USR_ID = e.USR_ID

                            WHERE 
                                e.addDoc_Status = 'Active' AND e.Draft_Status = 'Completed' AND e.USR_ID='"+ userid + "'", con);

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
                            //  document_name = dt.Rows[i]["document_name"].ToString(),
                            NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                            DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                            Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                            Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                            Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"].ToString(),
                            Eff_Date = dt.Rows[i]["Eff_Date"].ToString(),
                            addDoc_createdDate = dt.Rows[i]["addDoc_createdDate"].ToString(),
                            Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                            Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                            Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                            Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                            Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                            Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                            Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),


                            Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                            VersionControlNo = dt.Rows[i]["VersionControlNo"].ToString(),


                            Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                            freq_period = dt.Rows[i]["freq_period"].ToString() + " " + dt.Rows[i]["freq_period_type"].ToString(),
                            // freq_period_type = dt.Rows[i]["freq_period_type"].ToString(),
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


        }




        [Route("api/AddDocument/GetPubDocListReactivate")]
        [HttpGet]

        public IEnumerable<ProvideAccessModel> GetPubDocListReactivate()
        {

            

                MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
                con.Open();
                MySqlCommand cmd = new MySqlCommand(@"SELECT 
                                e.AuthoritynameID,
                                tbl.firstname as publisher_name,
                                e.AuthorityTypeID,
                                e.NatureOf_Doc_id,
                                e.DocTypeID,
                                e.Doc_SubCategoryID,
                                e.Doc_CategoryID,
                                e.Title_Doc,
                                e.Doc_Confidentiality,
                                e.Eff_Date, e.Sub_title_doc, e.Document_Id, e.addDoc_createdDate, e.VersionControlNo, e.freq_period_type,
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
                                e.Time_period,
                                e.Sub_title_doc,
                                e.AddDoc_id,
                                e.Obj_Doc,
                                e.Document_Id,
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
                          left outer join
                                risk.tbluser tbl on tbl.USR_ID = e.USR_ID where e.addDoc_Status='Disabled' &&  e.Draft_Status='Completed';", con);

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
                        //  document_name = dt.Rows[i]["document_name"].ToString(),
                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"].ToString(),
                        Eff_Date = dt.Rows[i]["Eff_Date"].ToString(),
                        addDoc_createdDate = dt.Rows[i]["addDoc_createdDate"].ToString(),
                        Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                        Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                        Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                        Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                        Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                        Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                        Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),


                        Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                        VersionControlNo = dt.Rows[i]["VersionControlNo"].ToString(),


                        Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                        freq_period = dt.Rows[i]["freq_period"].ToString() + " " + dt.Rows[i]["freq_period_type"].ToString(),
                        // freq_period_type = dt.Rows[i]["freq_period_type"].ToString(),
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






        [Route("api/AddDocument/DisablePublishedDoc")]
        [HttpPost]
        public IActionResult DisablePublishedDoc([FromBody] UpdateDoc AddDocumentModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string UpdateQuery = "UPDATE add_doc SET DisableReason=@DisableReason,addDoc_Status=@addDoc_Status,ChangedBy=@ChangedBy,ChangedOn=@ChangedOn WHERE AddDoc_id = @AddDoc_id";

            try
            {
                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                    myCommand.Parameters.AddWithValue("@DisableReason", AddDocumentModels.DisableReason);
                    myCommand.Parameters.AddWithValue("@addDoc_Status", "Disabled");
                    myCommand.Parameters.AddWithValue("@ChangedBy", AddDocumentModels.ChangedBy);
                    myCommand.Parameters.AddWithValue("@ChangedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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





        [Route("api/AddDocument/ReactivatePubDoc")]
        [HttpPost]
        public IActionResult ReactivatePublishedDoc([FromBody] UpdateDoc AddDocumentModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string UpdateQuery = "UPDATE add_doc SET "  + "addDoc_Status=@addDoc_Status " + "WHERE AddDoc_id = @AddDoc_id";

            try
            {
                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                  //  myCommand.Parameters.AddWithValue("@DisableReason", AddDocumentModels.DisableReason);
                    myCommand.Parameters.AddWithValue("@addDoc_Status", "InActive");

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












        [Route("api/UpdateDoc/UpdatePublishedDoc")]
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult UpdatePublishedDoc([FromBody] UpdateDoc AddDocumentModels )
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // Get the current HTTP request
                var request = HttpContext.Request;


                // Build the base URL from the request
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                con.Open();
                int AddDoc_Id = AddDocumentModels.AddDoc_id;
               
                //string insertquery = "INSERT INTO add_doc_old_data ( DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Title_Doc, Sub_title_doc, Eff_Date, Initial_creation_doc_date, Doc_internal_num, Doc_Inter_ver_num, Doc_Phy_Valut_Loc, Doc_process_Owner, Doc_Approver, Date_Doc_Revision, Date_Doc_Approver, AuthorityTypeID, AuthoritynameID, NatureOf_Doc_id, Doc_Confidentiality, indicative_reading_time, Keywords_tags, freq_period_type, freq_period, review_start_Date, pub_doc, Time_period, Obj_Doc, Doc_referenceNo, Revision_summary, addDoc_Status,VersionControlNo,AddDoc_id) VALUES (@DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID," +
                //          " @Title_Doc, @Sub_title_doc, @Eff_Date, @Initial_creation_doc_date, @Doc_internal_num, @Doc_Inter_ver_num, @Doc_Phy_Valut_Loc, @Doc_process_Owner, @Doc_Approver, @Date_Doc_Revision, @Date_Doc_Approver, @AuthorityTypeID, @AuthoritynameID, @NatureOf_Doc_id, @Doc_Confidentiality, @indicative_reading_time, @Keywords_tags, @freq_period_type, @freq_period, @review_start_Date, @pub_doc, @Time_period, @Obj_Doc, @Doc_referenceNo, @Revision_summary, @addDoc_Status,@VersionControlNo,@AddDoc_id)";
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

                                myCommand.Parameters.AddWithValue("@Linking_Doc_names", string.Join(",", titleDocs));
                               
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


                            string coverpagepdf;
                            var reportPath = GenerateReport(AddDocumentModels, VersionFolderPath.FullName);
                             coverpagepdf = Path.ChangeExtension(reportPath, "pdf");
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
                                AddDoc_id = AddDoc_Id,
                                Document_Id = AddDocumentModels.Document_Id,
                                VersionControlNo = newVersionControlNo
                            };
                            mySqlDBContext.DocumentFilesuplodModels.Add(coverPageUploadModel);

                        
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
                                    MergePdfFiles(coverpagepdf, existingMainFilePath, mergedPdfPath);

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




        private string GenerateReport(UpdateDoc data, string outputBaseDirectory)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            // Define the path to the output directory and ensure it exists
            var outputDirectory = Path.Combine(outputBaseDirectory, "CoverPages");
            Directory.CreateDirectory(outputDirectory);

            var outputPath = Path.Combine(outputDirectory, $"Report-{data.Document_Id}.docx");

            try
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(@"SELECT e.Title_Doc, e.Sub_title_doc, e.Eff_Date, e.Document_Id, e.VersionControlNo, e.Obj_Doc, t.DocTypeName, C.Doc_CategoryName, sc.Doc_SubCategoryName, a.AuthorityName, at.AuthorityTypeName, p.NatureOf_Doc_Name, e.Initial_creation_doc_date, e.Date_Doc_Revision, e.Doc_internal_num, e.Doc_Inter_ver_num, e.Doc_Phy_Valut_Loc, e.Doc_process_Owner, e.Doc_Approver, e.Date_Doc_Approver, e.AuthoritynameID, e.AuthorityTypeID, e.NatureOf_Doc_id, e.DocTypeID, e.Doc_SubCategoryID, e.Doc_CategoryID, e.Doc_Confidentiality, e.freq_period, e.freq_period_type, e.Keywords_tags, e.pub_doc, e.publisher_comments, e.indicative_reading_time, e.Time_period, e.review_start_Date, e.AddDoc_id, tblUser.firstname, e.addDoc_createdDate,e.MainpageCount,e.supportFilesCount FROM risk.add_doc e LEFT OUTER JOIN risk.tblUser ON tblUser.USR_ID = e.USR_ID LEFT OUTER JOIN risk.doctype_master t ON t.DocTypeID = e.DocTypeID LEFT OUTER JOIN risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID LEFT OUTER JOIN risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID LEFT OUTER JOIN risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID LEFT OUTER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID LEFT OUTER JOIN risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id WHERE e.AddDoc_id ='" + data.AddDoc_id + "'", con);

                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    // Path to the template document
                    var templatePath = "DocTemplate/PubDocTemplate1.dotx";
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
                                    DateTime PublishedDate;
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

                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["MainpageCount"].ToString();
                                }
                            }
                            else if (bookmarkStart.Name == "SupAttachments")
                            {
                                if (bookmarkText != null)
                                {

                                    bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["supportFilesCount"].ToString();
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



    }

}
