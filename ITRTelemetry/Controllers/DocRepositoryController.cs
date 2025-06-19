
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
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

using System.Configuration;

using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Office2010.Word;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using DocumentFormat.OpenXml.Spreadsheet;


namespace ITRTelemetry.Controllers

{
    [Produces("application/json")]
    public class DocRepositoryController : ControllerBase

    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public DocRepositoryController(MySqlDBContext mySqlDBContext, IConfiguration configuration)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/DocRepository/GetDocRepositoryDetails/{AddDoc_id}")]
        [HttpGet]

        public IEnumerable<DocumentFilesuplodModel> GetDocRepositoryDetails(int AddDoc_id)
        {
            return this.mySqlDBContext.DocumentFilesuplodModels.Where(x => x.Status == "Active" && x.AddDoc_id == AddDoc_id).ToList();
        }


        [Route("api/DocRepository/GetDocRepositoryDetails")]
        [HttpGet]
        //public IEnumerable<LinkedDocInfo> GetDocRepositoryDetails1([FromQuery] int AddDoc_id, int user_id, string application, string name, string department, string doc_classification)

        //{
        //    // Fetch all active documents for the specified AddDoc_id
        //    var documents = this.mySqlDBContext.DocumentFilesuplodModels
        //                                       .Where(x => x.Status == "Active" && x.AddDoc_id == AddDoc_id)
        //                                       .ToList();

        //    List<LinkedDocInfo> linkedDocInfos = new List<LinkedDocInfo>();

        //    // Get the current date and time as watermark text
        //    string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    string watermarkText = application +" | "+ name + " | " + department + " | " + doc_classification + " | " + currentDateTime + " | ";
        //    string folder_name = name +"_"+ user_id;
        //    foreach (var document in documents)
        //    {
        //        if (!string.IsNullOrEmpty(document.FilePath))
        //        {
        //            string filePathToReturn = document.FilePath;

        //            if (document.FileCategory == "Published")
        //            {
        //                // Create a new watermarked PDF
        //                filePathToReturn = AddWatermark(document.FilePath, folder_name, watermarkText);

        //                // Optionally update the database with the new file path
        //                document.FilePath = filePathToReturn;
        //                //this.mySqlDBContext.SaveChanges();
        //            }
        //                linkedDocInfos.Add(new LinkedDocInfo
        //            {
        //                DocumentName = document.Document_Name,
        //                FilePath = filePathToReturn
        //                });
        //        }

        //    }

        //    return linkedDocInfos;
        //}
        public async Task<IEnumerable<object>> GetDocRepositoryDetails([FromQuery] int AddDoc_id, int user_id, string application, string name, string department, string doc_classification)
        {
            // Fetch all active documents for the specified AddDoc_id
            var documents = await this.mySqlDBContext.DocumentFilesuplodModels
                                               .Where(x => x.Status == "Active" && x.AddDoc_id == AddDoc_id)
                                               .ToListAsync();

            List<object> result = new List<object>();

            // Get the current date and time as watermark text
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string watermarkText = $"{application} | {name} | {department} | {doc_classification} | {currentDateTime} | ";
            string folder_name = $"{name}_{user_id}";

            foreach (var document in documents)
            {
                string watermarkedFilePath = null;

                if (document.FileCategory == "Published" && !string.IsNullOrEmpty(document.FilePath))
                {
                    watermarkedFilePath = await Task.Run(() => AddWatermark(document.FilePath, folder_name, watermarkText));
                }

                result.Add(new
                {
                    document.DocumentRepID,
                    document.AddDoc_id,
                    document.Document_Id,
                    document.VersionControlNo,
                    document.FileCategory,
                    document.Document_Name,
                    document.FilePath,
                    WatermarkedFilePath = watermarkedFilePath,
                    document.Status,
                    document.documentrepository_createdDate
                });
            }

            return result;
        }

        // watermark behind text
        private string AddWatermarknotinuse(string inputPdfPath, string folder_name, string watermarkText)
        {
            // Construct the base URL using Request properties
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fileUrl = new Uri(new Uri(baseUrl), inputPdfPath);

            // Remove "Published.pdf" from the input path
            var deleteFileUrl = inputPdfPath.Replace("Published.pdf", string.Empty);

            // Get full file path
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileUrl.LocalPath.TrimStart('/').Replace('/', '\\'));
            string userFolderPath = Path.Combine(Path.GetDirectoryName(filePath), folder_name);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string relativeOutputPath = Path.Combine(folder_name, fileName + "_watermarked.pdf").Replace('\\', '/');
            string fullUrl = $"{deleteFileUrl}/{relativeOutputPath}";

            // Create directory if not exists
            Directory.CreateDirectory(userFolderPath);
            string outputPdfPath = Path.Combine(userFolderPath, fileName + "_watermarked.pdf");

            using (var pdfReader = new PdfReader(filePath))
            using (var pdfWriter = new PdfWriter(outputPdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                int numberOfPages = pdfDocument.GetNumberOfPages();

                for (int pageIndex = 1; pageIndex <= numberOfPages; pageIndex++)
                {
                    PdfPage page = pdfDocument.GetPage(pageIndex);

                    // Use NewContentStreamBefore to ensure watermark is behind the content
                    var pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDocument);

                    float pageWidth = page.GetPageSize().GetWidth();
                    float pageHeight = page.GetPageSize().GetHeight();

                    // Handle page rotation
                    int rotation = page.GetRotation();
                    switch (rotation)
                    {
                        case 90:
                            pdfCanvas.ConcatMatrix(0, 1, -1, 0, pageHeight, 0);
                            break;
                        case 180:
                            pdfCanvas.ConcatMatrix(-1, 0, 0, -1, pageWidth, pageHeight);
                            break;
                        case 270:
                            pdfCanvas.ConcatMatrix(0, -1, 1, 0, 0, pageWidth);
                            break;
                    }

                    // Begin text for watermark
                    pdfCanvas.BeginText()
                        .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 10)
                        .SetColor(new DeviceGray(0.85f), true);

                    float spacing = Math.Min(pageWidth, pageHeight) * 0.05f; // Adjust spacing dynamically
                    float fontSize = Math.Min(pageWidth, pageHeight) * 0.02f; // Adjust font size dynamically
                    PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    // Calculate text dimensions
                    float textWidth = font.GetWidth(watermarkText) * fontSize / 1000;
                    float textHeight = fontSize;

                    // Rotation factor for 45 degrees
                    float rotationFactor = (float)Math.Cos(Math.PI / 4);
                    float rotatedWidth = rotationFactor * textWidth + rotationFactor * textHeight;
                    float rotatedHeight = rotationFactor * textWidth + rotationFactor * textHeight;

                    // Loop through coordinates to place watermarks
                    for (float x = -rotatedWidth; x < pageWidth + rotatedWidth; x += rotatedWidth + spacing)
                    {
                        for (float y = -rotatedHeight; y < pageHeight + rotatedHeight; y += rotatedHeight + spacing)
                        {
                            pdfCanvas.SetTextMatrix(rotationFactor, rotationFactor, -rotationFactor, rotationFactor, x, y);
                            pdfCanvas.ShowText(watermarkText);
                        }
                    }

                    pdfCanvas.EndText();
                    pdfCanvas.Release();
                }
            }

            return fullUrl;
        }


        // watermark above text
        private string AddWatermarkcurrent(string inputPdfPath, string folder_name, string watermarkText)
        {
            // Construct the base URL using Request properties
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fileUrl = new Uri(new Uri(baseUrl), inputPdfPath);

            // Remove "Published.pdf" from the input path
            var deleteFileUrl = inputPdfPath.Replace("Published.pdf", string.Empty);

            // Get full file path
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileUrl.LocalPath.TrimStart('/').Replace('/', '\\'));
            string userFolderPath = Path.Combine(Path.GetDirectoryName(filePath), folder_name);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string relativeOutputPath = Path.Combine(folder_name, fileName + "_watermarked.pdf").Replace('\\', '/');
            string fullUrl = $"{deleteFileUrl}/{relativeOutputPath}";

            // Create directory if not exists
            Directory.CreateDirectory(userFolderPath);
            string outputPdfPath = Path.Combine(userFolderPath, fileName + "_watermarked.pdf");

            using (var pdfReader = new PdfReader(filePath))
            using (var pdfWriter = new PdfWriter(outputPdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                int numberOfPages = pdfDocument.GetNumberOfPages();

                for (int pageIndex = 1; pageIndex <= numberOfPages; pageIndex++)
                {
                    PdfPage page = pdfDocument.GetPage(pageIndex);

                    // Use NewContentStreamAfter to ensure watermark is above the content
                    var pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDocument);

                    float pageWidth = page.GetPageSize().GetWidth();
                    float pageHeight = page.GetPageSize().GetHeight();

                    // Handle page rotation
                    int rotation = page.GetRotation();
                    switch (rotation)
                    {
                        case 90:
                            pdfCanvas.ConcatMatrix(0, 1, -1, 0, pageHeight, 0);
                            break;
                        case 180:
                            pdfCanvas.ConcatMatrix(-1, 0, 0, -1, pageWidth, pageHeight);
                            break;
                        case 270:
                            pdfCanvas.ConcatMatrix(0, -1, 1, 0, 0, pageWidth);
                            break;
                    }

                    // Begin text for watermark
                    pdfCanvas.BeginText()
                        .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 10)
                        .SetColor(new DeviceGray(0.85f), true);

                    float spacing = Math.Min(pageWidth, pageHeight) * 0.05f; // Adjust spacing dynamically
                    float fontSize = Math.Min(pageWidth, pageHeight) * 0.02f; // Adjust font size dynamically
                    PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    // Calculate text dimensions
                    float textWidth = font.GetWidth(watermarkText) * fontSize / 1000;
                    float textHeight = fontSize;

                    // Rotation factor for 45 degrees
                    float rotationFactor = (float)Math.Cos(Math.PI / 4);
                    float rotatedWidth = rotationFactor * textWidth + rotationFactor * textHeight;
                    float rotatedHeight = rotationFactor * textWidth + rotationFactor * textHeight;

                    // Loop through coordinates to place watermarks
                    for (float x = -rotatedWidth; x < pageWidth + rotatedWidth; x += rotatedWidth + spacing)
                    {
                        for (float y = -rotatedHeight; y < pageHeight + rotatedHeight; y += rotatedHeight + spacing)
                        {
                            pdfCanvas.SetTextMatrix(rotationFactor, rotationFactor, -rotationFactor, rotationFactor, x, y);
                            pdfCanvas.ShowText(watermarkText);
                        }
                    }

                    pdfCanvas.EndText();
                    pdfCanvas.Release();
                }
            }

            return fullUrl;
        }
        //important
        private string AddWatermark2(string inputPdfPath, string folder_name, string watermarkText)
        {
            // Construct the base URL using Request properties
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var fileUrl = new Uri(new Uri(baseUrl), inputPdfPath);

            // Remove "Published.pdf" from the input path
            var deleteFileUrl = inputPdfPath.Replace("Published.pdf", string.Empty);

            // Get full file path
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileUrl.LocalPath.TrimStart('/').Replace('/', '\\'));
            string userFolderPath = Path.Combine(Path.GetDirectoryName(filePath), folder_name);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string relativeOutputPath = Path.Combine(folder_name, fileName + "_watermarked.pdf").Replace('\\', '/');
            string fullUrl = $"{deleteFileUrl}/{relativeOutputPath}";

            // Create directory if it doesn't exist
            Directory.CreateDirectory(userFolderPath);
            string outputPdfPath = Path.Combine(userFolderPath, fileName + "_watermarked.pdf");

            int retryCount = 5; // Number of retries
            int retryDelay = 1000; // Delay between retries in milliseconds

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    // Open the input file with FileShare.ReadWrite to allow access by other processes
                    using (var pdfReader = new PdfReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    using (var pdfWriter = new PdfWriter(outputPdfPath))
                    using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
                    {
                        int numberOfPages = pdfDocument.GetNumberOfPages();

                        for (int pageIndex = 1; pageIndex <= numberOfPages; pageIndex++)
                        {
                            PdfPage page = pdfDocument.GetPage(pageIndex);

                            // Add watermark above existing content
                            var pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDocument);

                            float pageWidth = page.GetPageSize().GetWidth();
                            float pageHeight = page.GetPageSize().GetHeight();

                            // Handle page rotation
                            int rotation = page.GetRotation();
                            switch (rotation)
                            {
                                case 90:
                                    pdfCanvas.ConcatMatrix(0, 1, -1, 0, pageHeight, 0);
                                    break;
                                case 180:
                                    pdfCanvas.ConcatMatrix(-1, 0, 0, -1, pageWidth, pageHeight);
                                    break;
                                case 270:
                                    pdfCanvas.ConcatMatrix(0, -1, 1, 0, 0, pageWidth);
                                    break;
                            }

                            // Begin text for watermark
                            pdfCanvas.BeginText()
                                .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 10)
                                .SetColor(new DeviceGray(0.85f), true);

                            float spacing = Math.Min(pageWidth, pageHeight) * 0.05f; // Adjust spacing dynamically
                            float fontSize = Math.Min(pageWidth, pageHeight) * 0.02f; // Adjust font size dynamically
                            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                            // Calculate text dimensions
                            float textWidth = font.GetWidth(watermarkText) * fontSize / 1000;
                            float textHeight = fontSize;

                            // Rotation factor for 45 degrees
                            float rotationFactor = (float)Math.Cos(Math.PI / 4);
                            float rotatedWidth = rotationFactor * textWidth + rotationFactor * textHeight;
                            float rotatedHeight = rotationFactor * textWidth + rotationFactor * textHeight;

                            // Loop through coordinates to place watermarks
                            for (float x = -rotatedWidth; x < pageWidth + rotatedWidth; x += rotatedWidth + spacing)
                            {
                                for (float y = -rotatedHeight; y < pageHeight + rotatedHeight; y += rotatedHeight + spacing)
                                {
                                    pdfCanvas.SetTextMatrix(rotationFactor, rotationFactor, -rotationFactor, rotationFactor, x, y);
                                    pdfCanvas.ShowText(watermarkText);
                                }
                            }

                            pdfCanvas.EndText();
                            pdfCanvas.Release();
                        }
                    }

                    // Break out of the retry loop if successful
                    break;
                }
                catch (IOException ex)
                {
                    if (attempt == retryCount)
                    {
                        throw; // Rethrow the exception if max retries are reached
                    }

                    // Wait before retrying
                    System.Threading.Thread.Sleep(retryDelay);
                }
            }

            return fullUrl;
        }

        private string AddWatermark(string inputPdfPath, string folder_name, string watermarkText)
        {
            // Construct the base URL using Request properties
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            // Combine the base URL with the input file path to form the full URL
            var fileUrl = new Uri(new Uri(baseUrl), inputPdfPath);
            // Remove "Published.pdf" from the input path




            var DeletefileUrl = inputPdfPath.Replace("Published.pdf", string.Empty);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileUrl.LocalPath.TrimStart('/').Replace('/', '\\'));
            string userFolderPath = Path.Combine(Path.GetDirectoryName(filePath), folder_name.ToString());
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string relativeOutputPath = Path.Combine(folder_name, fileName + "_watermarked.pdf").Replace('\\', '/');
            string fullUrl = $"{DeletefileUrl}/{relativeOutputPath}";
            Directory.CreateDirectory(userFolderPath);
            string outputPdfPath = Path.Combine(
                userFolderPath,
                Path.GetFileNameWithoutExtension(filePath) + "_watermarked.pdf");
            using (var pdfReader = new PdfReader(filePath))
            using (var pdfWriter = new PdfWriter(outputPdfPath))
            using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                int numberOfPages = pdfDocument.GetNumberOfPages();

                for (int pageIndex = 3; pageIndex <= numberOfPages; pageIndex++)
                {
                    PdfPage page = pdfDocument.GetPage(pageIndex);
                    var pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDocument);

                    float pageWidth = page.GetPageSize().GetWidth();
                    float pageHeight = page.GetPageSize().GetHeight();

                    pdfCanvas.BeginText()
                        .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 10)
                        .SetColor(new DeviceGray(0.90f), true);
                    float spacing = 0.02f;

                    // Calculate the width and height of the watermark text when rotated
                    float textHeight = 10;  // Font size is 20, so the height is 20
                    PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    float textWidth = font.GetWidth(watermarkText) * 10 / 1000; // Get width of the watermark text

                    // Calculate the distance between watermarks based on rotated text size
                    // float rotatedWidth = Math.Abs(0.707f) * textWidth + Math.Abs(0.707f) * textHeight; // Adjust for rotation
                    //float rotatedHeight = Math.Abs(0.707f) * textWidth + Math.Abs(0.707f) * textHeight; // Adjust for rotation
                    float rotationFactor = 0.707f; // 45-degree rotation factor
                    float rotatedWidth = rotationFactor * textWidth + rotationFactor * textHeight; // Adjust for rotation
                    float rotatedHeight = rotationFactor * textWidth + rotationFactor * textHeight; // Adjust for rotation

                    // Loop through X and Y coordinates to fill the page with watermarks
                    //for (float x = 0; x < pageWidth; x += rotatedWidth + spacing)
                    //{
                    //    for (float y = 0; y < pageHeight; y += rotatedHeight + spacing)
                    //    {
                    //        // Set the watermark position and rotation
                    //        pdfCanvas.SetTextMatrix(0.707f, 0.707f, -0.707f, 0.707f, x, y);
                    //        pdfCanvas.ShowText(watermarkText);
                    //    }
                    //}
                    for (float x = 0; x < pageWidth; x += rotatedWidth + spacing)  // Reduced spacing here
                    {
                        for (float y = 0; y < pageHeight; y += rotatedHeight + spacing)  // Reduced spacing here
                        {
                            // Set the watermark position and rotation
                            pdfCanvas.SetTextMatrix(rotationFactor, rotationFactor, -rotationFactor, rotationFactor, x, y);
                            pdfCanvas.ShowText(watermarkText);
                        }
                    }

                    pdfCanvas.EndText();
                    pdfCanvas.Release();
                }
            }

            return fullUrl;
        }

        [Route("api/DocRepository/GetLinkedDocuments")]
        [HttpGet]
        public IEnumerable<LinkedDocInfo> GetLinkedDocFilePaths([FromQuery] int AddDocId, string document_Id)
        {
            // Fetch all active documents for the specified AddDoc_id and document_Id
            var documents = this.mySqlDBContext.AddDocumentModels
                                                .Where(x => x.Document_Id == document_Id && x.AddDoc_id == AddDocId && x.addDoc_Status == "Active")
                                                .ToList();

            List<LinkedDocInfo> linkedDocInfos = new List<LinkedDocInfo>();

            // Iterate through each document and check for pub_doc
            foreach (var doc in documents)
            {
                if (!string.IsNullOrEmpty(doc.pub_doc))
                {
                    var linkedDocIds = doc.pub_doc.Split(',').Select(int.Parse).ToList();

                    // Find matching documents based on Document_Name (Title_Doc)
                    var matchingDocs = this.mySqlDBContext.AddDocumentModels
                                                          .Where(x => linkedDocIds.Contains(x.AddDoc_id))
                                                          .Select(x => new { x.Document_Id, x.Title_Doc })
                                                          .ToList();

                    // Iterate through the matching documents
                    foreach (var matchingDoc in matchingDocs)
                    {
                        // Find file paths where FileCategory is "Published"
                        var filePaths = this.mySqlDBContext.DocumentFilesuplodModels
                                                           .Where(x => x.Document_Id == matchingDoc.Document_Id && x.FileCategory == "Published" && x.Status == "Active")
                                                           .Select(x => x.FilePath)
                                                           .ToList();

                        // Add each file path along with the document name to the result list
                        foreach (var filePath in filePaths)
                        {
                            linkedDocInfos.Add(new LinkedDocInfo
                            {
                                DocumentName = matchingDoc.Title_Doc,
                                FilePath = filePath
                            });
                        }
                    }
                }
            }

            return linkedDocInfos;
        }








    }
}
