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
using Org.BouncyCastle.Asn1.Ocsp;
using DocumentFormat.OpenXml.Office2016.Excel;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DocumentController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public DocumentController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("api/SecondStepper/SecondStepperSaveDraftDetails")]
        [HttpPost]
        
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
                    con.Close();
                }
                //AddDocumentModels.addDoc_createdDate = dt1;

                AddDocumentModels.addDoc_Status = "Active";
                AddDocumentModels.Draft_Status = "Incomplete";
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


          
                    }
                    else
                    {
                        // Convert to PDF if it's not a PDF
                        pdfFilePath = Path.ChangeExtension(mainFilePath, "pdf");
                        ConvertDocxToPdf(mainFilePath, pdfFilePath);

                        int MainpageCount = GetPdfPageCount(pdfFilePath);
                        System.Diagnostics.Debug.Print("Page Count" + MainpageCount);

                        AddDocumentModels.MainpageCount = MainpageCount;

                    }

                  
                  
                    // Extract the file name from pdfFilePath
                    string fileName = Path.GetFileName(pdfFilePath);

                    // Use the relative path from the application's root to the file
                    string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), pdfFilePath).Replace("\\", "/");

                    // Combine them to form the URL
                    string fileUrl = $"{baseUrl}/{relativePath}";

                   
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
                return Ok();

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error:{ex}");
            }
        }


        [Route("api/ThirdStepper/ThirdStepperSaveDraftDetails")]
        [HttpPost]
        public async Task<IActionResult> updateParameter([FromForm] AddDocumentModel AddDocumentModels, [FromForm] IFormFile mainFile, [FromForm] List<IFormFile> supportFiles)
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
                    con.Close();
                }
                //AddDocumentModels.addDoc_createdDate = dt1;

                AddDocumentModels.addDoc_Status = "Active";
                AddDocumentModels.Draft_Status = "Incomplete";
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



                    }
                    else
                    {
                        // Convert to PDF if it's not a PDF
                        pdfFilePath = Path.ChangeExtension(mainFilePath, "pdf");
                        ConvertDocxToPdf(mainFilePath, pdfFilePath);

                        int MainpageCount = GetPdfPageCount(pdfFilePath);
                        System.Diagnostics.Debug.Print("Page Count" + MainpageCount);

                        AddDocumentModels.MainpageCount = MainpageCount;

                    }



                    // Extract the file name from pdfFilePath
                    string fileName = Path.GetFileName(pdfFilePath);

                    // Use the relative path from the application's root to the file
                    string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), pdfFilePath).Replace("\\", "/");

                    // Combine them to form the URL
                    string fileUrl = $"{baseUrl}/{relativePath}";


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
                return Ok();

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error:{ex}");
            }
        }
      

        [Route("api/SaveDraft/UpdateSaveDraftDetails")]
        [HttpPost, DisableRequestSizeLimit]

        public IActionResult UpdateSaveDraftDetails([FromBody] UpdateDoc AddDocumentModels)
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
                   

                        var documentidfolder = Path.Combine("Resources", documentid);
                        var VersionFolder = Path.Combine(documentidfolder, VersionControlNo);
                 
                    DirectoryInfo VersionFolderPath;
                    if (!Directory.Exists(VersionFolder))
                    {
                       
                        VersionFolderPath = Directory.CreateDirectory(VersionFolder);
                    }
                    else
                    {
                        
                        VersionFolderPath = new DirectoryInfo(VersionFolder);
                    }

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
                                        Linking_Doc_names=@Linking_Doc_names,ChangedBy=@ChangedBy,
                                         ChangedOn=@ChangedOn,
                                            Draft_Status=@Draft_Status
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
                            myCommand.Parameters.AddWithValue("@VersionControlNo", VersionControlNo);
                        myCommand.Parameters.AddWithValue("@ChangedBy", AddDocumentModels.ChangedBy);
                        myCommand.Parameters.AddWithValue("@ChangedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        myCommand.Parameters.AddWithValue("@Draft_Status", "Completed");

                        myCommand.Parameters.AddWithValue("@USR_ID", AddDocumentModels.USR_ID);
                            myCommand.ExecuteNonQuery();


                      


                            string coverpagepdf;
                            var reportPath = SavedDraftGenerateReport(AddDocumentModels, VersionFolderPath.FullName);
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
                                VersionControlNo = VersionControlNo
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
                                        VersionControlNo = VersionControlNo
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


                    //}
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


        //public IActionResult UpdateSaveDraftDetails([FromBody] UpdateDoc AddDocumentModels)
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

        //    string UpdateQuery = @"UPDATE add_doc SET publisher_comments = @publisher_comments,Doc_Linking_Status = @Doc_Linking_Status, 
        //                                Review_Frequency_Status = @Review_Frequency_Status, 

        //                                Title_Doc = @Title_Doc, 
        //                                Sub_title_doc = @Sub_title_doc, 
        //                                Obj_Doc = @Obj_Doc, 
        //                                DocTypeID = @DocTypeID, 
        //                                Doc_CategoryID = @Doc_CategoryID, 
        //                                Doc_SubCategoryID = @Doc_SubCategoryID, 
        //                                Eff_Date = @Eff_Date, 
        //                                Initial_creation_doc_date = @Initial_creation_doc_date, 
        //                                Doc_internal_num = @Doc_internal_num, 
        //                                Doc_Inter_ver_num = @Doc_Inter_ver_num, 
        //                                Doc_Phy_Valut_Loc = @Doc_Phy_Valut_Loc, 
        //                                Doc_process_Owner = @Doc_process_Owner, 
        //                                Doc_Approver = @Doc_Approver, 
        //                                Date_Doc_Approver = @Date_Doc_Approver, 
        //                                Date_Doc_Revision = @Date_Doc_Revision, 
        //                                AuthorityTypeID = @AuthorityTypeID, 
        //                                AuthoritynameID = @AuthoritynameID, 
        //                                Doc_Confidentiality = @Doc_Confidentiality, 
        //                                NatureOf_Doc_id = @NatureOf_Doc_id, 
        //                                indicative_reading_time = @indicative_reading_time, 
        //                                Keywords_tags = @Keywords_tags, 
        //                                freq_period_type = @freq_period_type, 
        //                                freq_period = @freq_period, 
        //                                review_start_Date = @review_start_Date, 
        //                                pub_doc = @pub_doc, 
        //                                USR_ID = @USR_ID, 
        //                                Time_period = @Time_period 
        //                            WHERE AddDoc_id = @AddDoc_id;";
        //    try
        //    {

        //        con.Open();
        //        using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
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
        //            myCommand.Parameters.AddWithValue("@Review_Frequency_Status", AddDocumentModels.Review_Frequency_Status);
        //            myCommand.Parameters.AddWithValue("@Doc_Linking_Status", AddDocumentModels.Doc_Linking_Status);
        //            myCommand.Parameters.AddWithValue("@USR_ID", AddDocumentModels.USR_ID);
        //                    myCommand.ExecuteNonQuery();

        //                }


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


        [Route("api/MainDocSaveDraft/MainDocSaveDraftDetails")]
        [HttpPost]
        public async Task<IActionResult> MainDocSaveDraftDetails([FromForm] AddDocumentModel AddDocumentModels, [FromForm] IFormFile mainFile, [FromForm] List<IFormFile> supportFiles)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {

                int supportFilesCount = supportFiles.Count;
                var httpContext = _httpContextAccessor.HttpContext;

                // Get the current HTTP request
                var request = HttpContext.Request;

                var documentidfolder = Path.Combine("Resources", AddDocumentModels.Document_Id);
                var VersionFolder = Path.Combine(documentidfolder, "1.0");
                DirectoryInfo VersionFolderPath = Directory.CreateDirectory(VersionFolder);
                // Build the base URL from the request
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);
                var mainFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Main"));
                var supportFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Support"));
                con.Open();


                {
                    string UpdateQuery = @"UPDATE add_doc SET                                          
                                       Title_Doc = @Title_Doc, 
                                       Sub_title_doc = @Sub_title_doc, 
                                      Obj_Doc = @Obj_Doc, 
                                       DocTypeID = @DocTypeID, 
                                       Doc_CategoryID = @Doc_CategoryID, 
                                       Doc_SubCategoryID = @Doc_SubCategoryID
                                  WHERE AddDoc_id = @AddDoc_id;";
            
                        using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                        {

                            myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                            myCommand.Parameters.AddWithValue("@Title_Doc", AddDocumentModels.Title_Doc);
                            myCommand.Parameters.AddWithValue("@Sub_title_doc", AddDocumentModels.Sub_title_doc);
                            myCommand.Parameters.AddWithValue("@Obj_Doc", AddDocumentModels.Obj_Doc);
                            myCommand.Parameters.AddWithValue("@DocTypeID", AddDocumentModels.DocTypeID);
                            myCommand.Parameters.AddWithValue("@Doc_CategoryID", AddDocumentModels.Doc_CategoryID);
                            myCommand.Parameters.AddWithValue("@Doc_SubCategoryID", AddDocumentModels.Doc_SubCategoryID);

                            myCommand.ExecuteNonQuery();

                        }
                       

                    if (mainFile.Length > 0)
                    {

                        string statuschange = "update documentrepository set Status='InActive' where Document_Id='" + AddDocumentModels.Document_Id + "' and Status='Active'  and FileCategory='Main' ";
                        using (MySqlCommand myCom = new MySqlCommand(statuschange, con))
                        {
                            myCom.ExecuteNonQuery();

                        }
                        // Save the file
                        var filePath = Path.Combine(supportFilesFolder.FullName, mainFile.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await mainFile.CopyToAsync(stream);
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
                            FileCategory = "Main",
                            Status = "Active",
                            Document_Id = AddDocumentModels.Document_Id,
                            VersionControlNo = "1.0",
                            documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            AddDoc_id = AddDocumentModels.AddDoc_id
                        };

                        // Add to the database context
                        mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                    }
                    //MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM risk.documentrepository
                    //where FileCategory=@FileCategory and AddDoc_id=@AddDoc_id and Document_Id=@Document_Id", con);

                    //cmd.CommandType = CommandType.Text;
                    //cmd.Parameters.AddWithValue("@FileCategory", "Main");
                    //cmd.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                    //cmd.Parameters.AddWithValue("@Document_Id", AddDocumentModels.Document_Id);

                    //MySqlDataAdapter da1 = new MySqlDataAdapter(cmd);

                    //DataTable dt1 = new DataTable();
                    //da1.Fill(dt1);
                    //// con.Close();

                    //if (dt1.Rows.Count > 0)
                    //{
                    //    int DocumentRepID = Convert.ToInt32(dt1.Rows[0]["DocumentRepID"].ToString());

                    //    string updatestatus = (@"update documentrepository set FileCategory=@FileCategory,FilePath=@FilePath,Status=@Status where DocumentRepID=@DocumentRepID ");
                    //    using (MySqlCommand myCommand12 = new MySqlCommand(updatestatus, con))
                    //    {
                    //        myCommand12.Parameters.AddWithValue("@FileCategory", "Main");
                    //        myCommand12.Parameters.AddWithValue("@Status", "Active");
                    //        myCommand12.Parameters.AddWithValue("@DocumentRepID", DocumentRepID);
                    //        myCommand12.Parameters.AddWithValue("@FilePath", AddDocumentModels.FilePath);
                    //        myCommand12.ExecuteNonQuery();

                    //    }

                    //}
                    foreach (var file in supportFiles)
                    {
                        if (file.Length > 0)
                        {

                            string statuschange = "update documentrepository set Status='InActive' where Document_Id='" + AddDocumentModels.Document_Id + "' and Status='Active'  and FileCategory='Support' ";
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
                                Document_Id = AddDocumentModels.Document_Id,
                                VersionControlNo = "1.0",
                                documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                AddDoc_id = AddDocumentModels.AddDoc_id
                            };

                            // Add to the database context
                            mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                        }
                    }
                    await mySqlDBContext.SaveChangesAsync();
                    

                    return Ok("successfully");

                }
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

        //public IActionResult MainDocSaveDraftDetails([FromBody] UpdateDoc AddDocumentModels) 
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

        //    string UpdateQuery = @"UPDATE add_doc SET                                          
        //                                Title_Doc = @Title_Doc, 
        //                                Sub_title_doc = @Sub_title_doc, 
        //                                Obj_Doc = @Obj_Doc, 
        //                                DocTypeID = @DocTypeID, 
        //                                Doc_CategoryID = @Doc_CategoryID, 
        //                                Doc_SubCategoryID = @Doc_SubCategoryID
        //                            WHERE AddDoc_id = @AddDoc_id;";
        //    try
        //    {

        //        con.Open();
        //        using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
        //        {

        //            myCommand.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
        //            myCommand.Parameters.AddWithValue("@Title_Doc", AddDocumentModels.Title_Doc);
        //            myCommand.Parameters.AddWithValue("@Sub_title_doc", AddDocumentModels.Sub_title_doc);
        //            myCommand.Parameters.AddWithValue("@Obj_Doc", AddDocumentModels.Obj_Doc);
        //            myCommand.Parameters.AddWithValue("@DocTypeID", AddDocumentModels.DocTypeID);
        //            myCommand.Parameters.AddWithValue("@Doc_CategoryID", AddDocumentModels.Doc_CategoryID);
        //            myCommand.Parameters.AddWithValue("@Doc_SubCategoryID", AddDocumentModels.Doc_SubCategoryID);

        //            myCommand.ExecuteNonQuery();

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



        [Route("api/AttributeSaveDraft/SaveDocument")]
        [HttpPost]
        public async Task<IActionResult> SaveDocument([FromForm] AddDocumentModel AddDocumentModels, [FromForm] IFormFile mainFile, [FromForm] List<IFormFile> supportFiles)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {

                int supportFilesCount = supportFiles.Count;
                var httpContext = _httpContextAccessor.HttpContext;

                // Get the current HTTP request
                var request = HttpContext.Request;

                var documentidfolder = Path.Combine("Resources", AddDocumentModels.Document_Id);
                var VersionFolder = Path.Combine(documentidfolder, "1.0");
                DirectoryInfo VersionFolderPath = Directory.CreateDirectory(VersionFolder);
                // Build the base URL from the request
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);
                var mainFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Main"));
                var supportFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Support"));
                con.Open();

                {
                   
                    if (mainFile.Length > 0)
                    {

                        string statuschange = "update documentrepository set Status='InActive' where Document_Id='" + AddDocumentModels.Document_Id + "' and Status='Active'  and FileCategory='Main' ";
                        using (MySqlCommand myCom = new MySqlCommand(statuschange, con))
                        {
                            myCom.ExecuteNonQuery();

                        }
                        // Save the file
                        var filePath = Path.Combine(supportFilesFolder.FullName, mainFile.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await mainFile.CopyToAsync(stream);
                        }
                         
                     
                        string fileName = Path.GetFileName(filePath);
                        string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath).Replace("\\", "/");
                        string fileUrl = $"{baseUrl}/{relativePath}";

                        // Create a file upload record with the generated URL

                        var fileUploadModel = new DocumentFilesuplodModel
                        {
                            Document_Name = fileName,
                            FilePath = fileUrl,
                            FileCategory = "Main",
                            Status = "Active",
                            Document_Id = AddDocumentModels.Document_Id,
                            VersionControlNo = "1.0",
                            documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            AddDoc_id = AddDocumentModels.AddDoc_id
                        };

                      
                        mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                    }
                   
                    foreach (var file in supportFiles)
                    {
                        if (file.Length > 0)
                        {

                            string statuschange = "update documentrepository set Status='InActive' where Document_Id='" + AddDocumentModels.Document_Id + "' and Status='Active'  and FileCategory='Support' ";
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

                          
                            string fileName = Path.GetFileName(filePath);
                            string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath).Replace("\\", "/");
                            string fileUrl = $"{baseUrl}/{relativePath}";

                          

                            var fileUploadModel = new DocumentFilesuplodModel
                            {
                                Document_Name = fileName,
                                FilePath = fileUrl,
                                FileCategory = "Support",
                                Status = "Active",
                                Document_Id = AddDocumentModels.Document_Id,
                                VersionControlNo = "1.0",
                                documentrepository_createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                AddDoc_id = AddDocumentModels.AddDoc_id
                            };

                           
                            mySqlDBContext.DocumentFilesuplodModels.Add(fileUploadModel);
                        }
                    }
                    await mySqlDBContext.SaveChangesAsync();


                    return Ok("successfully");

                }
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

        [Route("api/AttributeSaveDraft/AttributeSaveDraftDetails")]
        [HttpPost]
        public IActionResult AttributeSaveDraftDetails([FromBody] UpdateDoc AddDocumentModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string UpdateQuery = @"UPDATE add_doc SET 
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
                                     Keywords_tags = @Keywords_tags ,OtpMethod=@OtpMethod

                                    WHERE AddDoc_id = @AddDoc_id;";
            try
            {

                con.Open();
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {

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
                    myCommand.Parameters.AddWithValue("@OtpMethod", AddDocumentModels.OtpMethod);

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
        private bool DocumentIdExists(string docId)
        {
            return mySqlDBContext.AddDocumentModels.Any(doc => doc.Document_Id == docId);
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


        //public IActionResult SaveDraftDetails([FromBody] DocumentModel DocumentModels)
        // {

        //     MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
        //     string insertQuery = "insert into add_doc(Unit_location_Master_id,Entity_Master_id,DocTypeID,Doc_CategoryID,Doc_SubCategoryID,Title_Doc,Sub_title_doc,Obj_Doc)values(@Unit_location_Master_id,@Entity_Master_id,@DocTypeID,@Doc_CategoryID,@Doc_SubCategoryID,@Title_Doc,@Sub_title_doc,@Obj_Doc)";

        //     try
        //     {
        //         con.Open();
        //         using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
        //         {                  
        //             myCommand1.Parameters.AddWithValue("@Title_Doc", DocumentModels.Title_Doc);
        //             myCommand1.Parameters.AddWithValue("@Sub_title_doc", DocumentModels.Sub_title_doc);
        //             myCommand1.Parameters.AddWithValue("@Obj_Doc", DocumentModels.Obj_Doc);
        //             myCommand1.Parameters.AddWithValue("@DocTypeID", DocumentModels.DocTypeID);
        //             myCommand1.Parameters.AddWithValue("@Doc_CategoryID", DocumentModels.Doc_CategoryID);
        //             myCommand1.Parameters.AddWithValue("@Doc_SubCategoryID", DocumentModels.Doc_SubCategoryID);
        //             myCommand1.Parameters.AddWithValue("@Unit_location_Master_id", DocumentModels.Doc_SubCategoryID);
        //             myCommand1.Parameters.AddWithValue("@Entity_Master_id", DocumentModels.Doc_SubCategoryID);
        //             myCommand1.ExecuteNonQuery();
        //     }


        //         return Ok("added successfully");
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest($"Error: {ex.Message}");
        //     }
        //     finally
        //     {
        //         con.Close();
        //     }
        // }

        //public void UpdateSaveDraftDetails([FromForm] AddDocumentModel AddDocumentModels)
        //{

        //    if (AddDocumentModels.AddDoc_id == 0)
        //    {
        //    }
        //    else
        //    {

        //        AddDocumentModel updatedata = new AddDocumentModel();
        //        updatedata.AddDoc_id = AddDocumentModels.AddDoc_id;
        //        updatedata.Title_Doc = AddDocumentModels.Title_Doc;
        //        updatedata.Sub_title_doc = AddDocumentModels.Sub_title_doc;
        //        updatedata.Obj_Doc = AddDocumentModels.Obj_Doc;
        //        updatedata.DocTypeID = AddDocumentModels.DocTypeID;
        //        updatedata.Doc_CategoryID = AddDocumentModels.Doc_CategoryID;
        //        updatedata.Doc_SubCategoryID = AddDocumentModels.Doc_SubCategoryID;
        //        updatedata.Doc_Confidentiality = AddDocumentModels.Doc_Confidentiality;
        //        updatedata.OtpMethod = AddDocumentModels.OtpMethod;
        //        updatedata.Eff_Date = AddDocumentModels.Eff_Date;
        //        updatedata.Initial_creation_doc_date = AddDocumentModels.Initial_creation_doc_date;
        //        updatedata.Doc_internal_num = AddDocumentModels.Doc_internal_num;
        //        updatedata.Doc_Inter_ver_num = AddDocumentModels.Doc_Inter_ver_num;
        //        updatedata.Doc_Phy_Valut_Loc = AddDocumentModels.Doc_Phy_Valut_Loc;
        //        updatedata.Doc_process_Owner = AddDocumentModels.Doc_process_Owner;
        //        updatedata.Doc_Approver = AddDocumentModels.Doc_Approver;
        //        updatedata.Date_Doc_Approver = AddDocumentModels.Date_Doc_Approver;
        //        updatedata.Date_Doc_Revision = AddDocumentModels.Date_Doc_Revision;
        //        updatedata.AuthorityTypeID = AddDocumentModels.AuthorityTypeID;
        //        updatedata.AuthoritynameID = AddDocumentModels.AuthoritynameID;
        //        updatedata.NatureOf_Doc_id = AddDocumentModels.NatureOf_Doc_id;
        //        updatedata.Keywords_tags = AddDocumentModels.Keywords_tags;
        //        updatedata.freq_period = AddDocumentModels.freq_period;
        //        updatedata.freq_period_type = AddDocumentModels.freq_period_type;
        //        updatedata.review_start_Date = AddDocumentModels.review_start_Date;
        //        updatedata.publisher_comments = AddDocumentModels.publisher_comments;
        //        updatedata.indicative_reading_time = AddDocumentModels.indicative_reading_time;
        //        updatedata.Time_period = AddDocumentModels.Time_period;



        //        this.mySqlDBContext.Attach(updatedata);
        //        this.mySqlDBContext.Entry(updatedata).State = EntityState.Modified;

        //        var entry = this.mySqlDBContext.Entry(updatedata);

        //        Type type = typeof(AddDocumentModel);
        //        PropertyInfo[] properties = type.GetProperties();
        //        foreach (PropertyInfo property in properties)
        //        {
        //            if (property.GetValue(updatedata, null) == null || property.GetValue(updatedata, null).Equals(0))
        //            {
        //                entry.Property(property.Name).IsModified = false;
        //            }
        //        }

        //        this.mySqlDBContext.SaveChanges();
        //    }
        //}

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

        private string SavedDraftGenerateReport(UpdateDoc data, string outputBaseDirectory)
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

        [Route("api/DocumentDiscard/SaveDraftDiscard")]
        [HttpPost]
        public IActionResult SaveDraftDiscard([FromBody] AddDocumentModel AddDocumentModels)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                //MySqlCommand cmd = new MySqlCommand(@"Select * from add_doc  where AddDoc_id=@AddDoc_id and USR_ID=@USR_ID and add_doc.Draft_Status=@Draft_Status", con);

                //cmd.CommandType = CommandType.Text;
                //cmd.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                //cmd.Parameters.AddWithValue("@USR_ID", AddDocumentModels.USR_ID);
                //cmd.Parameters.AddWithValue("@Draft_Status", "Incomplete");

                //MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                //DataTable dt = new DataTable();
                //da.Fill(dt);
                ////con.Close();


                //if (dt.Rows.Count > 0)
                //{
                    //int AddDoc_id = Convert.ToInt32(dt.Rows[0]["AddDoc_id"].ToString());
                    string insertQuery = "update add_doc set Draft_Status =@Draft_Status,ChangedBy=@ChangedBy,ChangedOn=@ChangedOn where AddDoc_id=@AddDoc_id";

                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {
                        myCommand1.Parameters.AddWithValue("@Draft_Status", "Draft Discarded");
                        myCommand1.Parameters.AddWithValue("@AddDoc_id", AddDocumentModels.AddDoc_id);
                    // DiscardDateTime column as a Publisher_Date_Range 
                    // myCommand1.Parameters.AddWithValue("@Publisher_Date_Range", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    myCommand1.Parameters.AddWithValue("@ChangedBy", AddDocumentModels.ChangedBy);
                    myCommand1.Parameters.AddWithValue("@ChangedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    myCommand1.ExecuteNonQuery();
                    }

               // }

                return Ok("successfully");

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


    }
}
