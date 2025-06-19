using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySQLProvider;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data;
using System;
using Ubiety.Dns.Core;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.Threading.Tasks;
using System.Linq;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DocRevMapController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;


        public DocRevMapController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }


        [Route("api/SavedDraftDocument/GetPublishedData")]
        [HttpGet]
        public IEnumerable<ProvideAccessModel> GetPublishedData()

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





        [Route("api/DocVerAddDocument/UpdateversionchangeDoc")]
        [HttpPost]

        public async Task<IActionResult> UpdateVersionChange([FromForm] ProvideAccessModel ProvideAccessModels, [FromForm] IFormFile mainFile, [FromForm] List<IFormFile> supportFiles)
        {

            int supportFilesCount = supportFiles.Count;
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
                        ProvideAccessModels.supportFilesCount = supportFilesCount;


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

                            var documentidfolder = Path.Combine("Resources", documentid);
                            var VersionFolder = Path.Combine(documentidfolder, newVersion);
                            DirectoryInfo VersionFolderPath = Directory.CreateDirectory(VersionFolder);



                            // update to inactive

                            string UpdateQuery = "update add_doc set addDoc_Status='InActive' where Document_Id='" + documentuniqueid + "'";
                            using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                            {
                                myCommand.ExecuteNonQuery();

                            }

                            // insert new record with updated version

                            string insertquery = "INSERT INTO add_doc ( DocTypeID, Doc_CategoryID, Doc_SubCategoryID, Title_Doc, Sub_title_doc, Eff_Date, Initial_creation_doc_date, Doc_internal_num, Doc_Inter_ver_num, Doc_Phy_Valut_Loc, Doc_process_Owner, Doc_Approver, Date_Doc_Revision, Date_Doc_Approver, AuthorityTypeID, AuthoritynameID, NatureOf_Doc_id, Doc_Confidentiality, indicative_reading_time, Keywords_tags, freq_period_type, freq_period, review_start_Date, pub_doc, Time_period, Obj_Doc, addDoc_Status,VersionControlNo,Document_Id,Draft_Status,publisher_comments,addDoc_createdDate,USR_ID,status_permission,OtpMethod,Linking_Doc_names,Entity_Master_id,Unit_location_Master_id,Review_Frequency_Status,Doc_Linking_Status,ChangedBy,ChangedOn) VALUES (@DocTypeID, @Doc_CategoryID, @Doc_SubCategoryID," +
                                " @Title_Doc, @Sub_title_doc, @Eff_Date, @Initial_creation_doc_date, @Doc_internal_num, @Doc_Inter_ver_num, @Doc_Phy_Valut_Loc, @Doc_process_Owner, @Doc_Approver, @Date_Doc_Revision, @Date_Doc_Approver, @AuthorityTypeID, @AuthoritynameID, @NatureOf_Doc_id, @Doc_Confidentiality, @indicative_reading_time, @Keywords_tags, @freq_period_type, @freq_period, @review_start_Date, @pub_doc, @Time_period, @Obj_Doc, @addDoc_Status,@VersionControlNo,@Document_Id,@Draft_Status,@publisher_comments,@addDoc_createdDate,@USR_ID, @status_permission,@OtpMethod,@Linking_Doc_names,@Entity_Master_id,@Unit_location_Master_id,@Review_Frequency_Status,@Doc_Linking_Status,@ChangedBy,@ChangedOn)";
                            using (MySqlCommand myCommand = new MySqlCommand(insertquery, con))
                            {

                                if (!string.IsNullOrEmpty(ProvideAccessModels.pub_doc))
                                {
                                    string pubDocValue = ProvideAccessModels.pub_doc;
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
                                   // string.Join(",", titleDocs);
                                    myCommand.Parameters.AddWithValue("@Linking_Doc_names", string.Join(",", titleDocs));

                                }
                                else
                                {
                                    myCommand.Parameters.AddWithValue("@Linking_Doc_names", DBNull.Value);
                                }

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
                                myCommand.Parameters.AddWithValue("@USR_ID", ProvideAccessModels.USR_ID);
                                myCommand.Parameters.AddWithValue("@VersionControlNo", newVersion);
                                myCommand.Parameters.AddWithValue("@Document_Id", documentuniqueid);
                                myCommand.Parameters.AddWithValue("@OtpMethod", ProvideAccessModels.OtpMethod);
                                myCommand.Parameters.AddWithValue("@status_permission", "Permission");

                                myCommand.Parameters.AddWithValue("@Entity_Master_id" , ProvideAccessModels.Entity_Master_id);
                                myCommand.Parameters.AddWithValue("@Unit_location_Master_id", ProvideAccessModels.Unit_location_Master_id);
                                myCommand.Parameters.AddWithValue("@Review_Frequency_Status", ProvideAccessModels.Review_Frequency_Status);
                                myCommand.Parameters.AddWithValue("@Doc_Linking_Status", ProvideAccessModels.Doc_Linking_Status);

                                myCommand.Parameters.AddWithValue("@Draft_Status", "Completed");
                                myCommand.Parameters.AddWithValue("@addDoc_Status", "Active");

                                myCommand.Parameters.AddWithValue("@addDoc_createdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                myCommand.Parameters.AddWithValue("@ChangedBy", ProvideAccessModels.ChangedBy);
                                myCommand.Parameters.AddWithValue("@ChangedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                myCommand.ExecuteNonQuery();

                                string UpdateQuery1 = "update document_revision_mapping set Doc_rev_map_status='InActive' where Document_Id='" + documentuniqueid + "'";
                                using (MySqlCommand myCommand1 = new MySqlCommand(UpdateQuery1, con))
                                {
                                    myCommand1.ExecuteNonQuery();

                                }

                                int AddDocid = Convert.ToInt32(myCommand.LastInsertedId.ToString());

                        

                                string insertQuery2 = "insert into document_revision_mapping(Doc_referenceNo, Revision_summary, AddDoc_id, VersionControlNo, Document_Id,Doc_rev_map_createdDate,Doc_rev_map_status) values (@Doc_referenceNo, @Revision_summary, @AddDoc_id, @VersionControlNo, @Document_Id,@Doc_rev_map_createdDate,@Doc_rev_map_status)";

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
                                            myCommand2.Parameters.AddWithValue("@AddDoc_id", AddDocid);
                                            myCommand2.Parameters.AddWithValue("@Document_Id", documentuniqueid);
                                            myCommand2.Parameters.AddWithValue("@VersionControlNo", newVersion);
                                            myCommand2.Parameters.AddWithValue("@Doc_rev_map_createdDate", dt1);
                                            myCommand2.Parameters.AddWithValue("@Doc_rev_map_status", "Active");

                                            myCommand2.ExecuteNonQuery();
                                        }
                                    }
                                }

                                var mainFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Main"));
                                var supportFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Support"));
                                string coverpagepdf;
                               // var reportPath = GenerateReport(ProvideAccessModels, VersionFolderPath.FullName);
                             //   coverpagepdf = Path.ChangeExtension(reportPath, "pdf");
                             //   ConvertDocxToPdf(reportPath, coverpagepdf);
                              //  string coverPageFileName = Path.GetFileName(coverpagepdf);
                              //  string coverPageRelativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), coverpagepdf).Replace("\\", "/");
                             //   string coverPageUrl = $"{baseUrl}/{coverPageRelativePath}";

                                string UpdateQuery2 = "update documentrepository set Status='InActive' where Document_Id='"+ documentuniqueid + "' and Status='Active'  and FileCategory  in ('Cover Page','Published','Main')";
                                using (MySqlCommand myCommand1 = new MySqlCommand(UpdateQuery2, con))
                                {
                                    myCommand1.ExecuteNonQuery();

                                }

                                //// Add cover page to database
                                //var coverPageUploadModel = new DocumentFilesuplodModel
                                //{
                                //    Document_Name = coverPageFileName,
                                //    FilePath = coverPageUrl,
                                //    FileCategory = "Cover Page",
                                //    Status = "Active",
                                //    Document_Id= documentuniqueid,
                                //    VersionControlNo= newVersion,
                                //    documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                //    AddDoc_id = AddDocid
                                //};
                                //mySqlDBContext.DocumentFilesuplodModels.Add(coverPageUploadModel);
                                //if (string.IsNullOrEmpty(reportPath))
                                //{
                                //    return BadRequest("Report generation failed.");
                                //}


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

                                        ProvideAccessModels.MainpageCount = MainpageCount;

                                        var reportPath = GenerateReport(ProvideAccessModels, VersionFolderPath.FullName);
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

                                        ProvideAccessModels.MainpageCount = MainpageCount;

                                        var reportPath = GenerateReport(ProvideAccessModels, VersionFolderPath.FullName);
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

                                        Document_Id = documentuniqueid,
                                            VersionControlNo= newVersion,
                                         
                                            AddDoc_id = AddDocid
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
                                        Document_Id = documentuniqueid,
                                        VersionControlNo = newVersion,

                                        AddDoc_id = AddDocid
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
                                        Document_Id = documentuniqueid,
                                        VersionControlNo = newVersion,

                                        AddDoc_id = AddDocid
                                    };

                                    // Add to the database context
                                    mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                                }
                                else
                                {
                                    return BadRequest("Main file is required.");
                                }


                                string pagecount = "update add_doc set MainpageCount=@MainpageCount , supportFilesCount=@supportFilesCount where AddDoc_id='" + AddDocid + "' ";
                                using (MySqlCommand countcmd = new MySqlCommand(pagecount, con))
                                {
                                    countcmd.Parameters.AddWithValue("@MainpageCount", ProvideAccessModels.MainpageCount);
                                    countcmd.Parameters.AddWithValue("@supportFilesCount", ProvideAccessModels.supportFilesCount);

                                    countcmd.ExecuteNonQuery();

                                }

                                foreach (var file in supportFiles)
                                {
                                    if (file.Length > 0)
                                    {

                                        string statuschange = "update documentrepository set Status='InActive' where Document_Id='" + documentuniqueid + "' and Status='Active'  and FileCategory='Support' ";
                                        using (MySqlCommand myCom = new MySqlCommand(statuschange, con))
                                        {
                                            myCom.ExecuteNonQuery();

                                        }
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
                                            Document_Id = documentuniqueid,
                                            VersionControlNo = newVersion,
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

        private string GenerateReport(ProvideAccessModel data, string outputBaseDirectory)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            // Define the path to the output directory and ensure it exists
            var outputDirectory = Path.Combine(outputBaseDirectory, "CoverPages");
            Directory.CreateDirectory(outputDirectory);

            var outputPath = Path.Combine(outputDirectory, $"Report-{data.Document_Id}.docx");

            try
            {
                DataTable dt = new DataTable();
                // MySqlDataAdapter da = new MySqlDataAdapter(@"SELECT e.Title_Doc, e.Sub_title_doc, e.Eff_Date, e.Document_Id, e.VersionControlNo, e.Obj_Doc, t.DocTypeName, C.Doc_CategoryName, sc.Doc_SubCategoryName, a.AuthorityName, at.AuthorityTypeName, p.NatureOf_Doc_Name, e.Initial_creation_doc_date, e.Date_Doc_Revision, e.Doc_internal_num, e.Doc_Inter_ver_num, e.Doc_Phy_Valut_Loc, e.Doc_process_Owner, e.Doc_Approver, e.Date_Doc_Approver, e.AuthoritynameID, e.AuthorityTypeID, e.NatureOf_Doc_id, e.DocTypeID, e.Doc_SubCategoryID, e.Doc_CategoryID, e.Doc_Confidentiality, e.freq_period, e.freq_period_type, e.Keywords_tags, e.pub_doc, e.publisher_comments, e.indicative_reading_time, e.Time_period, e.review_start_Date, e.AddDoc_id, tblUser.firstname, e.addDoc_createdDate,(SELECT MIN(ad.addDoc_createdDate) FROM risk.add_doc ad WHERE ad.Document_Id = e.Document_Id AND ad.VersionControlNo = '1.0') AS FirstVersionCreatedDate  FROM risk.add_doc e LEFT OUTER JOIN risk.tblUser ON tblUser.USR_ID = e.USR_ID LEFT OUTER JOIN risk.doctype_master t ON t.DocTypeID = e.DocTypeID LEFT OUTER JOIN risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID LEFT OUTER JOIN risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID LEFT OUTER JOIN risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID LEFT OUTER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID LEFT OUTER JOIN risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id WHERE  e.addDoc_Status = 'Active' and   e.Document_Id ='" + data.Document_Id + "'", con);
                MySqlDataAdapter da = new MySqlDataAdapter(@"SELECT e.Title_Doc, e.Sub_title_doc, e.Eff_Date, e.Document_Id, e.VersionControlNo,
 e.Obj_Doc, t.DocTypeName, C.Doc_CategoryName, sc.Doc_SubCategoryName, a.AuthorityName,
 at.AuthorityTypeName, p.NatureOf_Doc_Name, e.Initial_creation_doc_date, e.Date_Doc_Revision, 
 e.Doc_internal_num, e.Doc_Inter_ver_num, e.Doc_Phy_Valut_Loc, e.Doc_process_Owner, e.Doc_Approver, 
 e.Date_Doc_Approver, e.AuthoritynameID, e.AuthorityTypeID, e.NatureOf_Doc_id, e.DocTypeID, 
 e.Doc_SubCategoryID, e.Doc_CategoryID, e.Doc_Confidentiality, e.freq_period, e.freq_period_type, 
 e.Keywords_tags, e.pub_doc, e.publisher_comments, e.indicative_reading_time, e.Time_period, 
 e.review_start_Date, e.AddDoc_id, tblUser.firstname, e.addDoc_createdDate,
 (SELECT MIN(ad.addDoc_createdDate) FROM risk.add_doc ad
 WHERE ad.Document_Id = e.Document_Id AND cast(ad.VersionControlNo as char) like '1.%')
 AS FirstVersionCreatedDate  FROM risk.add_doc e
 LEFT OUTER JOIN risk.tblUser ON tblUser.USR_ID = e.USR_ID 
 LEFT OUTER JOIN risk.doctype_master t ON t.DocTypeID = e.DocTypeID
 LEFT OUTER JOIN risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID 
 LEFT OUTER JOIN risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID 
 LEFT OUTER JOIN risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID 
 LEFT OUTER JOIN risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
 LEFT OUTER JOIN risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id 
 WHERE  e.addDoc_Status = 'Active' and   e.Document_Id ='" + data.Document_Id + "' ", con);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    // Path to the template document
                    var templatePath = "DocTemplate/PubVersionChangeTemplate2.dotx";
                   
                    System.IO.File.Copy(templatePath, outputPath, overwrite: true);

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
                            
                            else if (bookmarkStart.Name == "VersionDate")
                            {
                                if (bookmarkText != null)
                                {
                                    DateTime VersionDate;
                                    if (DateTime.TryParse(dt.Rows[0]["FirstVersionCreatedDate"].ToString(), out VersionDate))
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

                        foreach (FooterPart footerPart in doc.MainDocumentPart.FooterParts)
                        {
                            // Find all bookmarks in the current footer part
                            foreach (BookmarkStart bookmarkStart in footerPart.RootElement.Descendants<BookmarkStart>())
                            {
                                // Find the run that follows the bookmark start
                                Run bookmarkText = bookmarkStart.NextSibling<Run>();

                                // Check if this is the bookmark you want to update
                                
                                if (bookmarkStart.Name == "DocumentID2")
                                {
                                    if (bookmarkText != null)
                                        bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["Document_Id"].ToString();
                                }
                                else if (bookmarkStart.Name == "VersionControlNo2")
                                {
                                    if (bookmarkText != null)
                                        bookmarkText.GetFirstChild<Text>().Text = dt.Rows[0]["VersionControlNo"].ToString();
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

                            }
                        }


                        string versionHistoryQuery = "SELECT VersionControlNo, Doc_rev_map_createdDate, Doc_referenceNo, Revision_summary FROM risk.document_revision_mapping WHERE Document_Id='" + data.Document_Id + "'";

                        DataTable dtVersionHistory = new DataTable();
                        MySqlDataAdapter daVersionHistory = new MySqlDataAdapter(versionHistoryQuery, con);
                        daVersionHistory.Fill(dtVersionHistory);

                        Table versionHistoryTable = doc.MainDocumentPart.Document.Body.Elements<Table>().Last();

                        TableRow staticRow = versionHistoryTable.Elements<TableRow>().Last();

                    

                        // Keep track of the last version number and date to identify duplicates
                        string lastVersion = null;
                        string lastDate = null;
                        TableRow lastRow = null;

                        foreach (DataRow historyRow in dtVersionHistory.Rows.Cast<DataRow>().Reverse())
                        {
                            string currentVersion = historyRow["VersionControlNo"].ToString();
                            string currentDate = Convert.ToDateTime(historyRow["Doc_rev_map_createdDate"]).ToString("dd-MM-yyyy");
                            string Doc_referenceNo = historyRow["Doc_referenceNo"].ToString();
                            string Revision_summary = historyRow["Revision_summary"].ToString();

                            TableRow newRow = (TableRow)staticRow.CloneNode(true);
                            TableCell versionCell = newRow.Elements<TableCell>().ElementAt(0);
                            TableCell dateCell = newRow.Elements<TableCell>().ElementAt(1);
                            TableCell referenceCell = newRow.Elements<TableCell>().ElementAt(2);
                            TableCell summaryCell = newRow.Elements<TableCell>().ElementAt(3);

                            if (currentVersion == lastVersion && currentDate == lastDate)
                            {
                                // Hide the version and date cells
                                HideCellContent(versionCell);
                                HideCellContent(dateCell);
                            }
                            else
                            {
                                // Insert the data into the new cells
                                versionCell.Elements<Paragraph>().First().Elements<Run>().First().Elements<Text>().First().Text = currentVersion;
                                dateCell.Elements<Paragraph>().First().Elements<Run>().First().Elements<Text>().First().Text = currentDate;
                            }

                            referenceCell.Elements<Paragraph>().First().Elements<Run>().First().Elements<Text>().First().Text = Doc_referenceNo;
                            summaryCell.Elements<Paragraph>().First().Elements<Run>().First().Elements<Text>().First().Text = Revision_summary;
                            versionHistoryTable.InsertBefore(newRow, staticRow);

                            // Update last version and date
                            lastVersion = currentVersion;
                            lastDate = currentDate;
                            lastRow = newRow;
                        }

                        // Helper method to hide the content of a cell
                        void HideCellContent(TableCell cell)
                        {
                            // Set the text of the cell to empty
                            Paragraph p = cell.Elements<Paragraph>().First();
                            Run r = p.Elements<Run>().First();
                            Text t = r.Elements<Text>().First();
                            t.Text = "";

                            // Create or get the TableCellProperties
                            TableCellProperties tcp = cell.Elements<TableCellProperties>().FirstOrDefault();
                            if (tcp == null)
                            {
                                tcp = new TableCellProperties();
                                cell.Append(tcp);
                            }

                            // Create new borders and set them to nil (no border)
                            TableCellBorders borders = new TableCellBorders(
                                new TopBorder() { Val = BorderValues.Nil },
                               /// new BottomBorder() { Val = BorderValues.Nil },
                                //new LeftBorder() { Val = BorderValues.Nil },
                                //new RightBorder() { Val = BorderValues.Nil },
                                new InsideHorizontalBorder() { Val = BorderValues.Nil },
                                new InsideVerticalBorder() { Val = BorderValues.Nil }
                            );
                            // Set the TableCellProperties to use the new borders
                            tcp.Append(borders);
                      
                        }


                        // Helper method to merge the given cell with the target cell and remove borders



                        doc.MainDocumentPart.Document.Save();
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
    }
}
