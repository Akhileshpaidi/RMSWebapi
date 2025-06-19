using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;
using DomainModel;
using MySQLProvider;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ITR_TelementaryAPI.Controllers
{

    public class UploadController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public UploadController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/Upload")]
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                var files = Request.Form.Files[0];
                var path = Path.GetFullPath("Resources");
                string folderPath = "";
                string folderNewName = "";
                var finalPath = "";
                if (Request.Headers["uploadType"] == "Folder")
                {
                    folderPath = files.Name.Substring(0, files.Name.IndexOf("/"));
                    folderNewName = folderPath + Request.Headers["DateTime"];
                    finalPath = Path.Combine("Resources", "Folders", folderNewName);
                    DirectoryInfo newFolderName = Directory.CreateDirectory(finalPath);
                }
                else
                {
                    finalPath = Path.Combine("Resources", "Folders");


                }
                //var folderName = Path.Combine("Resources", "Folders", folderPath);
                var flightID = Request.Headers["flightID"];
                var tsid = Request.Headers["tsid"];
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), finalPath);
                if (files.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(files.ContentDisposition).FileName.ToString();
                    var fullPath = Path.Combine(pathToSave, Request.Headers["DateTime"] + fileName);
                    var extention = Path.GetExtension(fullPath);
                    var dbPath = Path.Combine(finalPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        files.CopyTo(stream);
                    }

                    string currentFile = Request.Headers["CurrentFile"];
                    string totalLength = Request.Headers["TotalLength"];
                    if (int.Parse(currentFile) == int.Parse(totalLength) - 1)
                    {
                        FolderUpload folderUpload = new FolderUpload();
                        folderUpload.folderName = folderPath;
                        folderUpload.folderNameUnique = folderNewName;
                        folderUpload.tsID = int.Parse(tsid);
                        folderUpload.flightID = int.Parse(flightID);
                        var loginID = Request.Headers["loginID"];//vishnu
                        folderUpload.dateTime = DateTime.Now.ToString();
                        folderUpload.year = Request.Headers["year"];
                        folderUpload.missionId = int.Parse(Request.Headers["missionId"]);
                        folderUpload.loginID = int.Parse(loginID);
                        if (Request.Headers["uploadType"] == "File")
                        {
                            folderUpload.folderNameUnique = Request.Headers["DateTime"] + fileName;
                            folderUpload.fileName = fileName;
                            folderUpload.fileExtension = extention;
                        }


                        var uploadModel = this.mySqlDBContext.FolderUpload;
                        uploadModel.Add(folderUpload);
                        this.mySqlDBContext.SaveChanges();
                        if (Request.Headers["uploadType"] == "Folder")
                        {
                            CreateZip(finalPath);
                            Directory.Delete(finalPath, true);
                        }


                    }

                    return Ok(new { dbPath, finalPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error:{ex}");
            }
        }
        [Route("api/Upload/Delete")]
        [HttpGet]
        public IActionResult Delete(string fileName , string orgFileName)
        {
            try
            {

                var path = Path.GetFullPath("Resources");
                var finalPath = Path.Combine("Resources", "Folders");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), finalPath);
                var fullPath = Path.Combine(pathToSave, fileName);
                var dbPath = Path.Combine(finalPath, fileName);
                System.IO.File.Delete(dbPath);
                this.mySqlDBContext.Remove(this.mySqlDBContext.FolderUpload.Single(a => a.folderNameUnique == orgFileName));
                this.mySqlDBContext.SaveChanges();

                return Ok(new { dbPath, finalPath });

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error:{ex}");
            }
        }

        void CreateZip(string source)
        {
            ZipFile.CreateFromDirectory(source, source + ".zip",
            CompressionLevel.Optimal, false);


        }
        [Route("api/Upload/DownLoadFiles")]
        [HttpGet]
        public async Task<FileStream> DownloadFile(string fileName)
        {
            //var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var finalPath = Path.Combine("Resources", "Folders", fileName);
            // currentDirectory = finalPath + "\\src\\assets";
            //var file = Path.Combine(Path.Combine(finalPath, "attachments"), fileName);
            return new FileStream(finalPath, FileMode.Open, FileAccess.Read);
        }

        [Route("api/Upload/GetAllFolders")]
        [HttpGet]
        public IEnumerable<ViewFolderUpload> Get()
        {


            return (from folderUploaded in this.mySqlDBContext.FolderUpload
                    join mission in this.mySqlDBContext.MissionModels on folderUploaded.missionId equals mission.MissionID
                    join station in this.mySqlDBContext.TelemetryStationModels on folderUploaded.tsID equals station.TSID
                    join flights in this.mySqlDBContext.FlightModels on folderUploaded.flightID equals flights.FlightID
                    select new ViewFolderUpload()
                    {
                        missionId = mission.MissionID,
                        missionName = mission.MissionName,
                        folderName = folderUploaded.folderName,
                        folderNameUnique = folderUploaded.folderNameUnique,
                        year = folderUploaded.year,
                        dateTime = folderUploaded.dateTime,
                        folderID = folderUploaded.folderID,
                        flightID = flights.FlightID,
                        flightName = flights.FlightNumber,
                        tsID = station.TSID,
                        stationName = station.Name,
                        fileName = folderUploaded.fileName,
                        fileExtension = folderUploaded.fileExtension

                    }).ToList();

        }
    }
}