using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Net.Http.Headers;
using System.IO.Compression;
using DomainModel;
using MySQLProvider;
using System.Text;
using MySql.Data.MySqlClient;
using ITR_TelementaryAPI.Models;
using System.Data;

namespace ITRTelemetry.Controllers
{
    public class ParseDataUploadController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public ParseDataUploadController(MySqlDBContext mySqlDBContext)
        {

            this.mySqlDBContext = mySqlDBContext;
        }

        public IEnumerable<EquipmentParameterModel> Get(int EquipmentID)
        {
            return this.mySqlDBContext.EquipmentParameterModels.Where(x => x.EquipmentID == EquipmentID).ToList();
        }

        [Route("api/ParseDataUpload")]
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult ParseDataUpload()
        {
            try
            {
                var files = Request.Form.Files[0];
                // var path = Path.GetFullPath("ParseDataFiles");
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

                var systemID = Request.Headers["systemID"];
                var tsid = Request.Headers["tsid"];
                var flightID = Request.Headers["flightID"];
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
                       // folderUpload.folderNameUnique = folderNewName;
                        folderUpload.tsID = int.Parse(tsid);
                        folderUpload.flightID = int.Parse(flightID);
                        folderUpload.dateTime = DateTime.Now.ToString();
                        folderUpload.year = Request.Headers["year"];
                        folderUpload.missionId = int.Parse(Request.Headers["missionId"]);
                        //if (Request.Headers["uploadType"] == "File")
                        //{
                            folderUpload.folderNameUnique = Request.Headers["DateTime"] + fileName;
                            folderUpload.fileName = fileName;
                            folderUpload.fileExtension = extention;
                       // }


                        var uploadModel = this.mySqlDBContext.FolderUpload;
                        uploadModel.Add(folderUpload);
                        this.mySqlDBContext.SaveChanges();
                        //if (Request.Headers["uploadType"] == "Folder")
                        //{
                        //    CreateZip(finalPath);
                        //    Directory.Delete(finalPath, true);
                        //}


                    }
                    

                    int EquipmentID = Convert.ToInt32(systemID);
                    int Fid = Convert.ToInt32(flightID);

                    if (EquipmentID != 0)
                    {
                        try
                        {
                            byte[] arr1 = ReadFile(fullPath);
                            bool packetvalidity = false;
                            packetvalidity = DataParse(arr1, EquipmentID);
                           
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, $"Internal Server Error:{ex}");
                        }

                    }
                    return Ok(new { EquipmentID = "Enter Equipment ID" });

                }
                else
                {
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500,$"Internal Server Error:{ex}");
            }

           // return View();
        }
        void CreateZip(string source)
        {
            ZipFile.CreateFromDirectory(source, source + ".zip",
            CompressionLevel.Optimal, false);


        }

        public bool DataParse(byte[] arr1, int Equip)
        {
            var packetid = 0;
            int EquipmentID = Equip;
            PacketModel packet = new PacketModel();
            packet.EquipmentID = EquipmentID;
            EquipmentParameterDetailModel det = new EquipmentParameterDetailModel();
            var temp = Get(EquipmentID).ToArray();
            var packettemp = GetPacketDetails(EquipmentID).ToArray();

            var flightID = Request.Headers["flightID"];
            if (packettemp.Length > 0)
            {
                for (int k = 0; k <= packettemp.GetUpperBound(0); k++)
                {
                    string s = packettemp[k].ParameterName;
                    string s1 = packettemp[k].StartByte.ToString();
                    string s2 = packettemp[k].EndByte.ToString();
                    string s3 = packettemp[k].DataType;

                    if (s2 == "")
                    {
                        s2 = s1;
                    }

                    int startbyte = Convert.ToInt32(s1);
                    int endbyte = Convert.ToInt32(s2);
                    int j = 0;
                    int size = endbyte - (startbyte - 1);
                    byte[] paramByteArray = new byte[size];
                    for (int i = startbyte; i <= endbyte; i++)
                    {
                        paramByteArray[j] = arr1[i];
                        j++;
                    }
                    //if (BitConverter.IsLittleEndian)
                    //    Array.Reverse(paramByteArray);                             
                    string paramlength = paramByteArray.Length.ToString();
                    string DataType = s3;
                    var holdvalue = "";
                    switch (DataType)
                    {
                        case "Unsigned char":
                            holdvalue = paramByteArray[0].ToString();
                            System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);
                            break;

                        case "Float":
                            float valueBack = BitConverter.ToSingle(paramByteArray, 0);
                            holdvalue = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);
                            break;

                        case "Long":
                            long longValueBack = BitConverter.ToInt64(paramByteArray, 0);
                            holdvalue = longValueBack.ToString();
                            break;

                        case "Short":
                            short shortValueBack = BitConverter.ToInt16(paramByteArray, 0);
                            holdvalue = shortValueBack.ToString();
                            break;

                        case "Bool":
                            bool boolValueBack = BitConverter.ToBoolean(paramByteArray, 0);
                            holdvalue = boolValueBack.ToString();
                            break;

                        case "Char":
                            // getting char value and Display it 
                            char charValueBack = BitConverter.ToChar(paramByteArray, 0);
                            holdvalue = charValueBack.ToString();
                            break;

                        case "BCD":
                            // getting char value and Display it 
                            string bcdValueBack = ConvertToBinaryCodedDecimal(true, paramByteArray);
                            holdvalue = bcdValueBack.ToString();
                            break;
                    }
                    switch (s)
                    {
                        case "HH":
                            packet.Hours = holdvalue;
                            break;
                        case "MM":
                            packet.Minutes = holdvalue;
                            break;
                        case "SS":
                            packet.Seconds = holdvalue;
                            break;
                        case "MS":
                            packet.MilliSeconds = holdvalue;
                            break;
                        case "ParamSize":
                            packet.ParameterListSize = holdvalue;
                            break;
                        case "LOCK":
                            packet.LockStatus = holdvalue;
                            break;
                        case "OriginalStatus":
                            packet.OriginalStatus = holdvalue;
                            break;
                        case "ID":
                            packet.PacketID = holdvalue;
                            break;
                    }
                }
                packet.PacketID = "004";
                packet.TransmissionDate = Convert.ToString(DateTime.Now);
                packetid = SavePacketData(packet); 
            }
            if (temp.Length > 0)
            {
                if (arr1.Length > 0)
                {
                   
                    for (int k = 0; k <= temp.GetUpperBound(0); k++)
                    {
                        int s = temp[k].ParameterID;
                        string s1 = temp[k].StartByte;
                        string s2 = temp[k].EndByte;
                        string s3 = temp[k].DataType;

                        if (s2 == "")
                        {
                            s2 = s1;
                        }

                        int startbyte = Convert.ToInt32(s1);
                        int endbyte = Convert.ToInt32(s2);
                        int j = 0;
                        int size = endbyte - (startbyte - 1);
                        byte[] paramByteArray = new byte[size];
                        for (int i = startbyte; i <= endbyte; i++)
                        {
                            paramByteArray[j] = arr1[i];
                            j++;
                        }
                        string Datatype = s3;
                        //if (BitConverter.IsLittleEndian)
                        //    Array.Reverse(paramByteArray);                             
                        string paramlength = paramByteArray.Length.ToString();
                        string DataType = s3;

                        det.ParameterID = s;
                        det.UDPPacketSequenceNo = "vvvv";
                        // int ID = GetPacketID();
                        det.UPDPacketID = "004";
                        det.PacketMasterID = 4;
                        //det.UPDPacketID = packet.PacketID;
                        //det.PacketMasterID = packetid;
                        det.FlightID = int.Parse(flightID); 
                        det.Date = DateTime.Now;

                        var equipmentparameterdetails = this.mySqlDBContext.EquipmentParameterDetailModels;                      
                        switch (DataType)
                        {
                            case "Unsigned char":
                                det.ParamData = paramByteArray[0].ToString();
                                SaveParametersData(det);
                                break;
                            case "Float":

                                float valueBack = BitConverter.ToSingle(paramByteArray, 0);
                                det.ParamData = valueBack.ToString();
                                SaveParametersData(det);
                                break;
                            case "Long":

                                long longValueBack = BitConverter.ToInt64(paramByteArray, 0);
                                det.ParamData = longValueBack.ToString();
                                SaveParametersData(det);
                                break;
                            case "Short":

                                short shortValueBack = BitConverter.ToInt16(paramByteArray, 0);
                                det.ParamData = shortValueBack.ToString();
                                SaveParametersData(det);
                                break;
                            case "Bool":

                                bool boolValueBack = BitConverter.ToBoolean(paramByteArray, 0);
                                det.ParamData = boolValueBack.ToString();
                                SaveParametersData(det);
                                break;
                            case "Char":
                                // getting char value and Display it 
                                char charValueBack = BitConverter.ToChar(paramByteArray, 0);
                                det.ParamData = charValueBack.ToString();
                                SaveParametersData(det);
                                break;
                            case "BCD":
                                // getting char value and Display it 
                                string bcdValueBack = ConvertToBinaryCodedDecimal(true, paramByteArray);
                                det.ParamData = bcdValueBack.ToString();
                                SaveParametersData(det);
                                break;
                        }

                    }

                }
                else
                {
                }
            }
            return true;
        }

        public static void SaveParametersData(EquipmentParameterDetailModel det)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            string parameterdata = "INSERT INTO equipmentparameterdata(ParamData,FlightID,Date,UDPPacketSequenceNo,UPDPacketID,ParameterID,PacketMasterID,Status) VALUES(@ParamData,@FlightID,@Date,@UDPPacketSequenceNo,@UPDPacketID,@ParameterID,@PacketMasterID,@Status)";
            try
            {
                using (MySqlCommand myCommand = new MySqlCommand(parameterdata, con))
                {
                    myCommand.Parameters.AddWithValue("@ParamData", det.ParamData);
                    myCommand.Parameters.AddWithValue("@FlightID", det.FlightID);
                    myCommand.Parameters.AddWithValue("@Date", det.Date);
                    myCommand.Parameters.AddWithValue("@UDPPacketSequenceNo", det.UDPPacketSequenceNo);
                    myCommand.Parameters.AddWithValue("@UPDPacketID", det.UPDPacketID);
                    myCommand.Parameters.AddWithValue("@ParameterID", det.ParameterID);
                    myCommand.Parameters.AddWithValue("@PacketMasterID", det.PacketMasterID);
                    myCommand.Parameters.AddWithValue("@Status", "");
                    con.Open();
                    myCommand.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);

            }

        }

        public static string ConvertToBinaryCodedDecimal(bool isLittleEndian, params byte[] bytes)
        {
            StringBuilder bcd = new StringBuilder(bytes.Length * 2);
            if (isLittleEndian)
            {
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    byte bcdByte = bytes[i];
                    int idHigh = bcdByte >> 4;
                    int idLow = bcdByte & 0x0F;
                    if (idHigh > 9 || idLow > 9)
                    {
                        throw new ArgumentException(
                            String.Format("One of the argument bytes was not in binary-coded decimal format: byte[{0}] = 0x{1:X2}.",
                            i, bcdByte));
                    }
                    bcd.Append(string.Format("{0}{1}", idHigh, idLow));
                }
            }
            else
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    byte bcdByte = bytes[i];
                    int idHigh = bcdByte >> 4;
                    int idLow = bcdByte & 0x0F;
                    if (idHigh > 9 || idLow > 9)
                    {
                        throw new ArgumentException(
                            String.Format("One of the argument bytes was not in binary-coded decimal format: byte[{0}] = 0x{1:X2}.",
                            i, bcdByte));
                    }
                    bcd.Append(string.Format("{0}{1}", idHigh, idLow));
                }
            }
            return bcd.ToString();
        }


        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }

        public static int SavePacketData(PacketModel det)
        {
            int id = 0;
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            string parameterdata = "INSERT INTO packetmaster(PacketID,EquipmentID,OriginalStatus,LockStatus,ParameterListSize,Hours,Minutes,Seconds,MilliSeconds,TransmissionDate) VALUES (@PacketID,@EquipmentID,@OriginalStatus,@LockStatus,@ParameterListSize,@Hours,@Minutes,@Seconds,@MilliSeconds,@TransmissionDate);SELECT LAST_INSERT_ID();";
            try
            {
                using (MySqlCommand myCommand = new MySqlCommand(parameterdata, con))
                {
                    myCommand.Parameters.AddWithValue("@PacketID", det.PacketID);
                    myCommand.Parameters.AddWithValue("@EquipmentID", det.EquipmentID);
                    myCommand.Parameters.AddWithValue("@OriginalStatus", det.OriginalStatus);
                    myCommand.Parameters.AddWithValue("@LockStatus", det.LockStatus);
                    myCommand.Parameters.AddWithValue("@ParameterListSize", det.ParameterListSize);
                    myCommand.Parameters.AddWithValue("@Hours", det.Hours);
                    myCommand.Parameters.AddWithValue("@Minutes", det.Minutes);
                    myCommand.Parameters.AddWithValue("@Seconds", det.Seconds);
                    myCommand.Parameters.AddWithValue("@MilliSeconds", det.MilliSeconds);
                    myCommand.Parameters.AddWithValue("@TransmissionDate", DateTime.Now);
                    con.Open();
                    id = Convert.ToInt32(myCommand.ExecuteScalar());
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);

            }
            finally
            {
            }
            return id;

        }
        public static IEnumerable<GetPacketDetails> GetPacketDetails(int EquipmentID)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select * from PacketDetailsMaster where EquipmentID=  '" + EquipmentID + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetPacketDetails>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetPacketDetails
                    {
                        PacketDetailsMasterID = Convert.ToInt32(dt.Rows[i]["PacketDetailsMasterID"].ToString()),
                        ParameterName = dt.Rows[i]["ParameterName"].ToString(),
                        EquipmentID = Convert.ToInt32(dt.Rows[i]["EquipmentID"].ToString()),
                        StartByte = Convert.ToInt32(dt.Rows[i]["StartByte"].ToString()),
                        EndByte = Convert.ToInt32(dt.Rows[i]["EndByte"].ToString()),
                        DataType = dt.Rows[i]["DataType"].ToString()
                    });
                }
            }
            return pdata;
        }
    }
}
