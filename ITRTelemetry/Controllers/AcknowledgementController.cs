using DomainModel;
using MySQLProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System;
using ITR_TelementaryAPI;
using ITRTelemetry.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using OpenXmlPowerTools;
using System.Configuration;
using System.Reflection;
using System.Linq;
using System.Data;
using System.ComponentModel.DataAnnotations;
using ITR_TelementaryAPI.Models;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using System.Diagnostics;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AcknowledgementController : Controller
    {

        private ClsEmail obj_Clsmail= new ClsEmail();
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration configuration { get; }
       public AcknowledgementController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            //configuration = configuration;
        }

        [Route("api/Acknowledgement/GetAcknowlwdgedata")]
        [HttpGet]
        public IEnumerable<object> GetAcknowlwdgedata(int USR_ID)

        {
            var currentDate = DateTime.Today;

            var details = (from doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels
                           join addDocumentMaster in mySqlDBContext.AddDocumentModels on doc_task.AddDoc_id equals addDocumentMaster.AddDoc_id
                           join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
                           join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
                           join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
                           join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
                           join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
                           join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
                           where doc_task.status == "Active"
                                 && doc_task.ack_status == "true"
                                 && doc_task.USR_ID == USR_ID
                                 && addDocumentMaster.addDoc_Status == "Active" 
                           select new
                           {
                               doc_task.AddDoc_id,
                               addDocumentMaster.Title_Doc,
                               natureMaster.NatureOf_Doc_Name,
                               authorityTypeMaster.AuthorityTypeName,
                               authorityMasters.AuthorityName,
                               addDocumentMaster.Keywords_tags,
                               addDocumentMaster.Doc_Confidentiality,
                               docTypeMaster.docTypeName,
                               docSubcatMaster.Doc_SubCategoryName,
                               docCatMaster.Doc_CategoryName,
                               doc_task.startDate,
                               doc_task.endDate
                           })
                          .ToList() // Fetch data into memory
                          .Where(d => {
                              DateTime startDate;
                              DateTime? endDate = null;
                              bool startDateParsed = DateTime.TryParse(d.startDate, out startDate);
                              bool endDateParsed = string.IsNullOrEmpty(d.endDate) || DateTime.TryParse(d.endDate, out DateTime parsedEndDate) && (endDate = parsedEndDate) != null;

                              return startDateParsed
                                     && endDateParsed
                                     && currentDate >= startDate
                                     && (endDate == null || currentDate <= endDate);
                          })
                          .Distinct();

            return details;





            //var currentDate = DateTime.Now;
            //var details = (from doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels
            //               join addDocumentMaster in mySqlDBContext.AddDocumentModels on doc_task.AddDoc_id equals addDocumentMaster.AddDoc_id
            //               join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
            //               join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
            //               join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
            //               join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
            //               join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
            //               join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
            //               where (doc_task.status == "Active") && (doc_task.ack_status == "true") && (doc_task.USR_ID == USR_ID) && (addDocumentMaster.addDoc_Status == "Active") && (currentDate >= doc_task.startDate && currentDate <= doc_task.endDate)

            //               select new
            //               {
            //                   doc_task.AddDoc_id,
            //                   addDocumentMaster.Title_Doc,
            //                   natureMaster.NatureOf_Doc_Name,
            //                   authorityTypeMaster.AuthorityTypeName,
            //                   authorityMasters.AuthorityName,
            //                   addDocumentMaster.Keywords_tags,
            //                   addDocumentMaster.Doc_Confidentiality,
            //                   docTypeMaster.docTypeName,
            //                   docSubcatMaster.Doc_SubCategoryName,
            //                   docCatMaster.Doc_CategoryName
            //               })
            //              .Distinct();

            //return details;


        }
        [Route("api/Acknowledgement/GetAckUserAccessability")]
        [HttpGet]
        public IEnumerable<object> GetUserAccessability(int USR_ID)

        {
            var currentDate = DateTime.Today;
            var details =
                           (from doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels
                            join addDocumentMaster in mySqlDBContext.AddDocumentModels on doc_task.AddDoc_id equals addDocumentMaster.AddDoc_id
                            join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
                            join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
                            join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
                         join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
                            join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
                           join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
                            where (doc_task.status == "Active") &&
                  (doc_task.ack_status == "false" || doc_task.ack_status == "" || doc_task.ack_status == "Acknowledged" || doc_task.ack_status == "Read Later") &&
                  (doc_task.USR_ID == USR_ID) && 
                  (addDocumentMaster.addDoc_Status == "Active") 
                            select new
                            {
                                doc_task.AddDoc_id,
                                addDocumentMaster.Title_Doc,
                                natureMaster.NatureOf_Doc_Name,
                                authorityTypeMaster.AuthorityTypeName,
                                authorityMasters.AuthorityName,
                                addDocumentMaster.Keywords_tags,
                                addDocumentMaster.Doc_Confidentiality,
                                docTypeMaster.docTypeName,
                                docSubcatMaster.Doc_SubCategoryName,
                                docCatMaster.Doc_CategoryName,
                                doc_task.startDate,
                                doc_task.endDate
                            })
                          .ToList() // Fetch data into memory
                          .Where(d => {
                              DateTime startDate;
                              DateTime? endDate = null;
                              bool startDateParsed = DateTime.TryParse(d.startDate, out startDate);
                              bool endDateParsed = string.IsNullOrEmpty(d.endDate) || DateTime.TryParse(d.endDate, out DateTime parsedEndDate) && (endDate = parsedEndDate) != null;

                              return startDateParsed
                                     && endDateParsed
                                     && currentDate >= startDate
                                     && (endDate == null || currentDate <= endDate);
                          })
                          .Distinct();
            return details;

        }
        [Route("api/Acknowledgement/GetAckReadCompletedUserAccessability")]
        [HttpGet]
        public IEnumerable<object> GetReadCompletedUserAccessability(int USR_ID)

        {
            var currentDate = DateTime.Today;
            var details = (from doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels
                           join addDocumentMaster in mySqlDBContext.AddDocumentModels on doc_task.AddDoc_id equals addDocumentMaster.AddDoc_id
                           join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
                           join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
                           join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
                           join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
                           join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
                           join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
                           where (doc_task.status == "Active") && (doc_task.ack_status == "Reading Completed") && (doc_task.USR_ID == USR_ID) && (addDocumentMaster.addDoc_Status == "Active") 
                           select new
                           {
                               doc_task.AddDoc_id,
                               addDocumentMaster.Title_Doc,
                               natureMaster.NatureOf_Doc_Name,
                               authorityTypeMaster.AuthorityTypeName,
                               authorityMasters.AuthorityName,
                               addDocumentMaster.Keywords_tags,
                               addDocumentMaster.Doc_Confidentiality,
                               docTypeMaster.docTypeName,
                               docSubcatMaster.Doc_SubCategoryName,
                               docCatMaster.Doc_CategoryName,
                               doc_task.startDate,
                               doc_task.endDate

                           })
                            .ToList() // Fetch data into memory
                             .Where(d => {
                                 DateTime startDate;
                                 DateTime? endDate = null;
                                 bool startDateParsed = DateTime.TryParse(d.startDate, out startDate);
                                 bool endDateParsed = string.IsNullOrEmpty(d.endDate) || DateTime.TryParse(d.endDate, out DateTime parsedEndDate) && (endDate = parsedEndDate) != null;

                                 return startDateParsed
                                        && endDateParsed
                                        && currentDate >= startDate
                                        && (endDate == null || currentDate <= endDate);
                             })
                          .Distinct();
            return details;

        }










        [Route("api/Acknowledgement/InsertReadCompleteAcknowledgeDetails")]
        [HttpPost]
        public async Task<IActionResult> InsertAcknowledgeDetails([FromBody] doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels)
        {
                var existingRecord = await this.mySqlDBContext.doctaskuseracknowledmentstatusmodels.FirstOrDefaultAsync(x =>
                x.AddDoc_id == doctaskuseracknowledmentstatusmodels.AddDoc_id &&
                x.USR_ID == doctaskuseracknowledmentstatusmodels.USR_ID &&
                x.Document_Id == doctaskuseracknowledmentstatusmodels.Document_Id && x.status == "Active");

            if (existingRecord == null)
            {
                // Record doesn't exist, return a message
                return BadRequest("not exist");
            }
            else
            {
                //if (existingRecord.ack_status == "Reading Completed")
                //{
                //    // Document already marked as Read Complete, return a message
                //    existingRecord.ack_status = "Reading Completed";
                //    existingRecord.Favorite = doctaskuseracknowledmentstatusmodels.Favorite;
                //    await mySqlDBContext.SaveChangesAsync();
                //    return Ok();
                //}

                // Update ack_status to Read Complete
                existingRecord.ack_status = "Reading Completed";
                existingRecord.readComplete_date = System.DateTime.Now;
                existingRecord.Favorite = doctaskuseracknowledmentstatusmodels.Favorite;
                await mySqlDBContext.SaveChangesAsync();
                return Ok();
            }

        }

            //var acknowledgmentmodel = this.mySqlDBContext.AcknowledgementModels;
            //acknowledgmentmodel.Add(AcknowledgementModels);
            //DateTime dt = DateTime.Now;
            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //AcknowledgementModels.Created_Date = dt1;
            //AcknowledgementModels.Status = "Reading Completed";
            //await mySqlDBContext.SaveChangesAsync();
           // return Ok();
            

        [Route("api/Acknowledgement/InsertReadLaterAcknowledgeDetails")]
        [HttpPost]
        public async Task<IActionResult> InsertReadLaterAcknowledgeDetails([FromBody] doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels)
        {
            var existingRecord = await this.mySqlDBContext.doctaskuseracknowledmentstatusmodels.FirstOrDefaultAsync(x =>
            x.AddDoc_id == doctaskuseracknowledmentstatusmodels.AddDoc_id &&
            x.USR_ID == doctaskuseracknowledmentstatusmodels.USR_ID &&
            x.Document_Id == doctaskuseracknowledmentstatusmodels.Document_Id   && x.status =="Active");

            if (existingRecord == null)
            {
                // Record doesn't exist, return a message
                return BadRequest("not exist");
            }
            else
            {
                //if (existingRecord.ack_status == "Read Later")
                //{
                //    // Document already marked as Read Complete, return a message
                //    existingRecord.ack_status = "Read Later";
                //    existingRecord.Favorite = doctaskuseracknowledmentstatusmodels.Favorite;
                //    await mySqlDBContext.SaveChangesAsync();
                //    return Ok();
                //}

                // Update ack_status to Read Complete
                existingRecord.ack_status = "Read Later";
                existingRecord.readLater_date = System.DateTime.Now;
                existingRecord.Favorite = doctaskuseracknowledmentstatusmodels.Favorite;
                await mySqlDBContext.SaveChangesAsync();
                return Ok();
            }
        }

        //var acknowledgmentmodel = this.mySqlDBContext.AcknowledgementModels;
        //acknowledgmentmodel.Add(AcknowledgementModels);
        //DateTime dt = DateTime.Now;
        //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //AcknowledgementModels.Created_Date = dt1;
        //AcknowledgementModels.Status = "Read Later";
        //await mySqlDBContext.SaveChangesAsync();
        //return Ok();

        [Route("api/Acknowledgement/UpdateAcknowledgeDetails")]
        [HttpPost]
        public async Task<IActionResult> InsertReadUpdateAcknowledgeDetails([FromBody] doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels)
        {
            var existingRecord = await this.mySqlDBContext.doctaskuseracknowledmentstatusmodels.FirstOrDefaultAsync(x =>
            x.AddDoc_id == doctaskuseracknowledmentstatusmodels.AddDoc_id &&
            x.USR_ID == doctaskuseracknowledmentstatusmodels.USR_ID &&
            x.Document_Id == doctaskuseracknowledmentstatusmodels.Document_Id && x.status =="Active");

            if (existingRecord == null)
            {
                // Record doesn't exist, return a message
                return BadRequest("not exist");
            }
            else
            {
                existingRecord.Favorite = doctaskuseracknowledmentstatusmodels.Favorite;

                await mySqlDBContext.SaveChangesAsync();
                return Ok();
            }
        }
        [Route("api/Acknowledgement/UpdateDocStatus")]
        [HttpPut]
        public async Task<IActionResult> UpdateDocStatus([FromBody] doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels)
        {
            try
            {
                var existingRecord = await this.mySqlDBContext.doctaskuseracknowledmentstatusmodels.FirstOrDefaultAsync(x =>
                        x.AddDoc_id == doctaskuseracknowledmentstatusmodels.AddDoc_id &&
                        x.USR_ID == doctaskuseracknowledmentstatusmodels.USR_ID && x.status == "Active");
           
                if (existingRecord !=null)
                existingRecord.ack_status = "Acknowledged";
                existingRecord.acknowledged_date = System.DateTime.Now;
                await mySqlDBContext.SaveChangesAsync();
                return Ok();

            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(409, "Concurrency conflict");
            }
            catch (Exception ex) 
            {
                return Ok(ex);
            }
        }

        [Route("api/Acknowledgement/Sendmail")]
        [HttpPost]
        public void Sendmail([FromBody] MailRequestModel mailRequest)
        {
            obj_Clsmail.SendEmail(mailRequest.emailToAddress, mailRequest.subject, mailRequest.body);
        }

        [Route("api/Acknowledgement/UserPermissionWiseDownLoadFiles")]
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            try
            {

                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Invalid file name provided.");
                    return BadRequest("Invalid file name provided.");
                }

                // Debugging: Print file path
                Console.WriteLine($"File Path: {filePath}");

                Uri uri = new Uri(filePath);
                string relativePath = uri.LocalPath.TrimStart('/');

                // Assuming filePath is the local file path on the server
                // string localFilePath = Path.Combine("YourLocalFolderPath", relativePath);

                if (System.IO.File.Exists(relativePath))
                {
                    // Read the file content
                    byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(relativePath);

                    // Determine the file name from the local file path
                    string extractedFileName = Path.GetFileName(relativePath);

                    // Determine the file type based on the file extension or fileType parameter
                    string contentType = GetContentType(extractedFileName);

                    // Return the file content as a FileResult
                    return File(fileBytes, contentType, extractedFileName);
                }
                else
                {
                    Console.WriteLine("File does not exist at the specified path.");
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Internal Server Error: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        private string GetContentType(string fileName)
        {
            // Implement logic to determine the content type based on the file extension or fileType parameter
            // For simplicity, assume the content type based on the file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                case ".docx":
                    return "application/msword";
                case ".xls":
                case ".xlsx":
                    return "application/vnd.ms-excel";
                default:
                    return "application/octet-stream"; // Default content type for binary data
            }
        }

    }
}
