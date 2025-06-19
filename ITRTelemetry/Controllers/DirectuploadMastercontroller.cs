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

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DirectuploadMastercontroller:ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public DirectuploadMastercontroller(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //[Route("api/DiectUploadsizemaster/InsertDiectUploadsizemasterDetails")]
        //[HttpPost]
        //public IActionResult InsertParameter([FromBody] DirectuploadModel DirectuploadModels)
        //{

        //    var DirectuploadModel = this.mySqlDBContext.DirectuploadModels;
        //    DirectuploadModel.Add(DirectuploadModels);
        //    DateTime dt = DateTime.Now;
        //    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //    DirectuploadModels.admin_config_createdDate = dt1;
        //    DirectuploadModels.admin_config_status = "Active";
        //    this.mySqlDBContext.SaveChanges();
        //    return Ok();
        //}


        [Route("api/DiectUploadsizemaster/GetMainDocuments")]
        [HttpGet]

        public IEnumerable<DirectuploadModel> GetMainDocuments()
        {
            return this.mySqlDBContext.DirectuploadModels.Where(x => x.admin_config_status == "Active" && x.FileCategory == "Mainfile").ToList();
        }


        [Route("api/DiectUploadsizemaster/GetSupportDocuments")]
        [HttpGet]

        public IEnumerable<DirectuploadModel> GetSupportDocuments()
        {
            return this.mySqlDBContext.DirectuploadModels.Where(x => x.admin_config_status == "Active" && x.FileCategory == "Supportedfiles").ToList();
        }

        //insert MainDocuments
        [Route("api/DiectUploadsizemaster/InsertMainDocuments")]
        [HttpPost]

        public IActionResult InsertMainDocuments([FromBody] DirectuploadModel DirectuploadModels)
        {
            try
            {
                var existingRecord = this.mySqlDBContext.DirectuploadModels.FirstOrDefault(x => x.FileCategory == "Mainfile");

                if (existingRecord == null)
                {
                    
                    // Set other properties for insertion
                    var DirectuploadModel = this.mySqlDBContext.DirectuploadModels;
                    DirectuploadModel.Add(DirectuploadModels);
                    DateTime dt = DateTime.Now;
                    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    DirectuploadModels.noOfDocuploaded = 1;
                    DirectuploadModels.sizelimit = DirectuploadModels.sizelimit;
                    DirectuploadModels.allowedFileTypes= DirectuploadModels.allowedFileTypes;
                    DirectuploadModels.admin_config_createdDate = dt1;
                    DirectuploadModels.FileCategory = "Mainfile";

                    DirectuploadModels.admin_config_status = "Active";
                }
                else
                {

                    existingRecord.sizelimit = DirectuploadModels.sizelimit;
                    existingRecord.allowedFileTypes = DirectuploadModels.allowedFileTypes;
                    DateTime dt = DateTime.Now;
                    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    existingRecord.admin_config_createdDate = dt1;
                    // Update other properties for the existing record
                    this.mySqlDBContext.DirectuploadModels.Update(existingRecord);
                }

                this.mySqlDBContext.SaveChanges();
                return Ok();

               
            }
            catch (Exception ex)
            {
                // Log the exception or print the details for debugging
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }

        }

        //insert SupportDocuments
        [Route("api/DiectUploadsizemaster/InsertSupportDocuments")]
        [HttpPost]

        public IActionResult InsertSupportDocuments([FromBody] DirectuploadModel DirectuploadModels)
        {
            try
            {
                var existingRecord = this.mySqlDBContext.DirectuploadModels.FirstOrDefault(x => x.FileCategory == "Supportedfiles");

                if (existingRecord == null)
                {

                    // Set other properties for insertion
                    var DirectuploadModel = this.mySqlDBContext.DirectuploadModels;
                    DirectuploadModel.Add(DirectuploadModels);
                    DateTime dt = DateTime.Now;
                    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    DirectuploadModels.noOfDocuploaded = DirectuploadModels.noOfDocuploaded;
                    DirectuploadModels.sizelimit = DirectuploadModels.sizelimit;
                    DirectuploadModels.allowedFileTypes = DirectuploadModels.allowedFileTypes;
                    DirectuploadModels.admin_config_createdDate = dt1;
                    DirectuploadModels.FileCategory = "Supportedfiles";

                    DirectuploadModels.admin_config_status = "Active";
                }
                else
                {

                    existingRecord.sizelimit = DirectuploadModels.sizelimit;
                    existingRecord.allowedFileTypes = DirectuploadModels.allowedFileTypes;
                    existingRecord.noOfDocuploaded = DirectuploadModels.noOfDocuploaded;
                    DateTime dt = DateTime.Now;
                    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    existingRecord.admin_config_createdDate = dt1;
                    // Update other properties for the existing record
                    this.mySqlDBContext.DirectuploadModels.Update(existingRecord);
                }

                this.mySqlDBContext.SaveChanges();
                return Ok();


            }
            catch (Exception ex)
            {
                // Log the exception or print the details for debugging
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }

           
        }


        [Route("api/DiectUploadsizemaster/UpdateMainDocuments")]
        [HttpPut]

        public void UpdateMainDocuments([FromBody] DirectuploadModel DirectuploadModels)
        {
            if (DirectuploadModels.admin_config_id == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(DirectuploadModels);
                this.mySqlDBContext.Entry(DirectuploadModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(DirectuploadModels);

                Type type = typeof(DirectuploadModel); 
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(DirectuploadModels, null) == null || property.GetValue(DirectuploadModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }
        [Route("api/DiectUploadsizemaster/DeleteMainDocuments")]
        [HttpDelete]

        public void DeleteMainDocuments(int id)
        {
            var currentClass = new DirectuploadModel { admin_config_id = id };
            currentClass.admin_config_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("admin_config_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        [Route("api/DiectUploadsizemaster/UpdateSupportDocuments")]
        [HttpPut]

        public void UpdateSupportDocuments([FromBody] DirectuploadModel DirectuploadModels)
        {
            if (DirectuploadModels.admin_config_id == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(DirectuploadModels);
                this.mySqlDBContext.Entry(DirectuploadModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(DirectuploadModels);

                Type type = typeof(DirectuploadModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(DirectuploadModels, null) == null || property.GetValue(DirectuploadModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }
        [Route("api/DiectUploadsizemaster/DeleteSupportDocuments")]
        [HttpDelete]

        public void DeleteSupportDocuments(int id)
        {
            var currentClass = new DirectuploadModel { admin_config_id = id };
            currentClass.admin_config_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("admin_config_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }


    }
    }
