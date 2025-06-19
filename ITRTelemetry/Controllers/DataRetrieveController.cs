using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ITR_TelementaryAPI.UDP;
using ITR_TelementaryAPI.Models;
using MySQLProvider;
using DomainModel;
using MySql.Data.MySqlClient;
using System.Data;
using ITRTelemetry.Models;
using ITRTelemetry.Controllers;
using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Collections;

namespace ITR_TelementaryAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class DataRetrieveController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public DataRetrieveController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }
        [Route("api/DataRetrieve/EquipmentParameterByID")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> Get(int EquipmentID)
        {
            return this.mySqlDBContext.EquipmentParameterModels.Where(x => x.EquipmentID == EquipmentID).ToList();
        }

        [Route("api/DataRetrieve/EquipmentParameterByIDGroup")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> GetData(int EquipmentID, int ParameterTypeID)
        {
            return this.mySqlDBContext.EquipmentParameterModels.Where(x => x.EquipmentID == EquipmentID && x.ParameterTypeID == ParameterTypeID).ToList();
        }

        [Route("api/DataRetrieve/EquipmentParameterByParameter")]
        [HttpGet]
        public object JoinParamData(int ParameterID)
        {
            var result = (from e in this.mySqlDBContext.EquipmentParameterDetailModels
                          join d
                          in this.mySqlDBContext.PacketModels on e.PacketMasterID equals d.PacketMasterID                        
                          select new
                          {
                              ParameterID = e.ParameterID,
                              ParamData = e.ParamData,
                              ParamDataID = e.ParamDataID,
                              Time = d.Hours.ToString() +":"+d.Minutes.ToString() + ":"+d.Seconds.ToString() + ":"+d.MilliSeconds.ToString(),
                              
                          }).Where(x => x.ParameterID == ParameterID).ToList();
            return result;
        }

        [Route("api/DataRetrieve/EquipmentParameterValues")]
        [HttpGet]
        public IEnumerable<EquipmentParameterDetailModel> GetDetails()
        {
            return this.mySqlDBContext.EquipmentParameterDetailModels.ToList();
        }
               
        [Route("api/DataRetrieve/GuageTestValues")]
        [HttpGet]
        public IEnumerable<GetParamData> GetTest()
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT equipmentparameterdata.ParameterID,ParamData,ParamDataID,ParameterName,ParameterTypeID,DataType,Date FROM itr.equipmentparameterdata inner join  itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamData>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetParamData { name = dt.Rows[i]["ParameterName"].ToString(), mean = dt.Rows[i]["ParamData"].ToString(), min = 0, max = 0 }); ;
                }
            }
            return pdata;       
        }

       
        [Route("api/DataRetrieve/AddValues")]
        [HttpGet]
        public IActionResult GetValues(int EquipmentID)
        {
            if (EquipmentID != 0)
            {
                try
                {
                    string fileName = @"E:\sharanya\ITR-DATA_API\itrtelemetrty-socketapi\ITRTelemetry\BinaryData\outputFile.bin";
                    var temp = Get(EquipmentID).ToArray();
                    if (temp.Length > 0)
                    {
                        byte[] arr1 = ReadFile(fileName);
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
                                EquipmentParameterDetailModel det = new EquipmentParameterDetailModel();
                                det.ParameterID
                                    = s;
                                det.UDPPacketSequenceNo = "hdhsh";
                               // int ID = GetPacketID();
                                det.UPDPacketID = "004";
                                det.PacketMasterID = 4;
                                det.FlightID = 1;
                                det.Date = DateTime.Now;

                                var equipmentparameterdetails = this.mySqlDBContext.EquipmentParameterDetailModels;

                                var AlreadyCheck = (from FDetail in mySqlDBContext.EquipmentParameterDetailModels
                                                    select new
                                                    {
                                                        FDetail.ParamDataID,
                                                        FDetail.ParameterID

                                                    }).Where(x => x.ParameterID == s).ToList();

                                switch (DataType)
                                {
                                    case "Unsigned char": 
                                        // The equivalent of unsigned char in C# is byte.
                                        //System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}, {3}", s1, s2, s3, paramByteArray[0]);
                                        det.ParamData = paramByteArray[0].ToString();
                                        equipmentparameterdetails.Add(det);
                                        //if (AlreadyCheck.Count > 0)
                                        //{
                                        //    det.ParamDataID = AlreadyCheck[0].ParamDataID;
                                        //    equipmentparameterdetails.Update(det);
                                        //}
                                        //else
                                        //{
                                        //    equipmentparameterdetails.Add(det);
                                        //}
                                        this.mySqlDBContext.SaveChanges();
                                        break;
                                    case "Float":
                                        //4 bytes size
                                        // getting float value and Display it 
                                        float valueBack = BitConverter.ToSingle(paramByteArray, 0);
                                        //System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}, {3}", s1, s2, s3, valueBack);
                                        det.ParamData = valueBack.ToString();
                                        equipmentparameterdetails.Add(det);
                                        //if (AlreadyCheck.Count > 0)
                                        //{
                                        //    det.ParamDataID = AlreadyCheck[0].ParamDataID;
                                        //    equipmentparameterdetails.Update(det);
                                        //}
                                        //else
                                        //{
                                        //    equipmentparameterdetails.Add(det);
                                        //}
                                        this.mySqlDBContext.SaveChanges();
                                        break;
                                    case "Long":
                                        //8 bytes size
                                        // getting long value and Display it 
                                        long longValueBack = BitConverter.ToInt64(paramByteArray, 0);
                                        // System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}, {3}", s1, s2, s3, longValueBack);
                                        det.ParamData = longValueBack.ToString();
                                        equipmentparameterdetails.Add(det);
                                        //if (AlreadyCheck.Count > 0)
                                        //{
                                        //    det.ParamDataID = AlreadyCheck[0].ParamDataID;
                                        //    equipmentparameterdetails.Update(det);
                                        //}
                                        //else
                                        //{
                                        //    equipmentparameterdetails.Add(det);
                                        //}
                                        this.mySqlDBContext.SaveChanges();
                                        break;
                                    case "Short":
                                        // getting short value and Display it 
                                        short shortValueBack = BitConverter.ToInt16(paramByteArray, 0);
                                        det.ParamData = shortValueBack.ToString();
                                        equipmentparameterdetails.Add(det);
                                        //if (AlreadyCheck.Count > 0)
                                        //{
                                        //    det.ParamDataID = AlreadyCheck[0].ParamDataID;
                                        //    equipmentparameterdetails.Update(det);
                                        //}
                                        //else
                                        //{
                                        //    equipmentparameterdetails.Add(det);
                                        //}
                                        this.mySqlDBContext.SaveChanges();
                                        break;
                                    case "Bool":
                                        // getting bool value and Display it 
                                        bool boolValueBack = BitConverter.ToBoolean(paramByteArray, 0);
                                        det.ParamData = boolValueBack.ToString();
                                        equipmentparameterdetails.Add(det);
                                        //if (AlreadyCheck.Count > 0)
                                        //{
                                        //    det.ParamDataID = AlreadyCheck[0].ParamDataID;
                                        //    equipmentparameterdetails.Update(det);
                                        //}
                                        //else
                                        //{
                                        //    equipmentparameterdetails.Add(det);
                                        //}
                                        this.mySqlDBContext.SaveChanges();
                                        break;
                                    case "Char":
                                        // getting char value and Display it 
                                        char charValueBack = BitConverter.ToChar(paramByteArray, 0);
                                        det.ParamData = charValueBack.ToString();
                                        equipmentparameterdetails.Add(det);
                                        //if (AlreadyCheck.Count > 0)
                                        //{
                                        //    det.ParamDataID = AlreadyCheck[0].ParamDataID;
                                        //    equipmentparameterdetails.Update(det);
                                        //}
                                        //else
                                        //{
                                        //    equipmentparameterdetails.Add(det);
                                        //}
                                        this.mySqlDBContext.SaveChanges();
                                        break;
                                }

                            }
                            return Ok(new { EquipmentID });
                        }
                        else
                        {
                            return Ok(new { EquipmentID = "No File Data for EquipmentID : " + EquipmentID + " " });
                        }
                    }
                    else
                    {
                        return Ok(new { EquipmentID = "No Parameter Data Exist for EquipmentID : " + EquipmentID + " " });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal Server Error:{ex}");
                }
            }
            return Ok(new { EquipmentID = "Enter Equipment ID" });
        }

        //public int GetPacketID()
        //{

        //    MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand("SELECT * FROM packetmaster ORDER BY PacketMasterID DESC LIMIT 1", con);
        //    cmd.CommandType = CommandType.Text;
        //    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
        //    DataTable dt = new DataTable();
        //    da.Fill(dt);
        //    con.Close();
        //    int packetid;
        //    if (dt.Rows.Count > 0)
        //    {
        //        packetid = Convert.ToInt32(dt.Rows[0]["PacketMasterID"].ToString());
        //        return packetid;
        //    }
        //    else
        //    {
        //        return 0;
        //    }

        //}


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
       

        [Route("api/DataRetrieve/UDP")]
        [HttpGet]
        public int UDPConection1(int EquipmentID, int TelemetryID)
        {         
            UDPSocket s = new UDPSocket();
            bool validity= s.Server("192.168.1.19", 9000, EquipmentID, TelemetryID);
            if(validity == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        [Route("api/DataRetrieve/GetParamsByTypeID")]
        [HttpGet]
        public IEnumerable<GetParamData> GetParamsByTypeID(int id)
        {
            var mdata = (from equipmentparameterdata in mySqlDBContext.EquipmentParameterDetailModels
                         join equipmentparametermaster in mySqlDBContext.EquipmentParameterModels on equipmentparameterdata.ParameterID equals equipmentparametermaster.ParameterID
                         select new
                         {
                             equipmentparameterdata.ParameterID,
                             equipmentparameterdata.ParamData,
                             equipmentparametermaster.ParameterTypeID


                         }).Where(x => x.ParameterTypeID == id).ToList();
            var pdata = new List<GetParamData>();
            for (int i = 0; i < mdata.Count; i++)
            {
                pdata.Add(new GetParamData { parameterid=mdata[i].ParameterID, mean = mdata[i].ParamData, name = Convert.ToString("Para " + mdata[i].ParameterID + "") });
            }
            return pdata;
        }

        //[Route("api/DataRetrieve/UDP1")]
        //[HttpGet]
        //public void UDP1()
        //{
        //    UdpClient receivingUdpClient = new UdpClient(8000);

        //    //Creates an IPEndPoint to record the IP Address and port number of the sender.
        //    // The IPEndPoint will allow you to read datagrams sent from any source.
        //    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        //    try
        //    {

        //        // Blocks until a message returns on this socket from a remote host.
        //        Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

        //        string returnData = Encoding.ASCII.GetString(receiveBytes);

        //        System.Diagnostics.Debug.WriteLine("This is the message you received " +
        //                               returnData.ToString());
        //        System.Diagnostics.Debug.WriteLine("This message was sent from " +
        //                                    RemoteIpEndPoint.Address.ToString() +
        //                                    " on their port number " +
        //                                    RemoteIpEndPoint.Port.ToString());
        //    }
        //    catch (Exception e)
        //    {
        //        System.Diagnostics.Debug.WriteLine(e.ToString());
        //    }

        //}


        [Route("api/DataRetrieve/GetPacketData")]
        [HttpGet]
        public object GetPacketData(int FlightID)
        {
            var result = (from e in this.mySqlDBContext.EquipmentParameterDetailModels
                          join d
                          in this.mySqlDBContext.EquipmentParameterModels on e.ParameterID equals d.ParameterID
                          join f
                          in this.mySqlDBContext.PacketModels on e.PacketMasterID equals f.PacketMasterID
                          select new
                          {
                              PacketID = f.PacketID,
                              ParameterID = d.ParameterID,
                              ParamData = e.ParamData,
                              ParameterName = d.ParameterName,
                              FlightID = e.FlightID,
                              Time = f.Hours.ToString() + ":" + f.Minutes.ToString() + ":" + f.Seconds.ToString() + ":" + f.MilliSeconds.ToString(),

                          }).Where(x => x.FlightID == FlightID).ToList();
            return result;
        }

        [Route("api/DataRetrieve/getDataByTypeID")]
        [HttpGet]
        public object getDataByTypeID(int ParametertypeID)
        {
            var result = (from e in this.mySqlDBContext.EquipmentParameterDetailModels
                          join d
                          in this.mySqlDBContext.PacketModels on e.PacketMasterID equals d.PacketMasterID
                          join f
                          in this.mySqlDBContext.EquipmentParameterModels on e.ParameterID equals f.ParameterID
                          select new
                          {
                              ParametertypeID=f.ParameterTypeID,
                              ParameterID = e.ParameterID,
                              ParameterName=f.ParameterName,
                              ParamData = e.ParamData,
                              ParamDataID = e.ParamDataID,
                              Time = d.Hours.ToString() + ":" + d.Minutes.ToString() + ":" + d.Seconds.ToString() + ":" + d.MilliSeconds.ToString(),

                          }).Where(x => x.ParametertypeID == ParametertypeID).ToList();
            return result;
        }
        [Route("api/DataRetrieve/GetAllParams")]
        [HttpGet]
        public object GetAllParams(int id)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterTypeID,ParameterID,ParameterName FROM itr.equipmentparametermaster where ParameterTypeID='" + id + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamData>();
            var add_list = new List<GetParamsModel>();
            if (dt.Rows.Count > 0)
            {
              //  string[] color = new string[] { "purple", "lightblue", "orange", "brown", "black", "green", "yellow", "red", "blue", "pink", "voilet", "lightyellow", };

                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    int ParameterTypeID = Convert.ToInt32(dt.Rows[i]["ParameterTypeID"].ToString());
                    int ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString());
                    string ParameterName = dt.Rows[i]["ParameterName"].ToString();
                    con.Open();
                    MySqlCommand cmd1 = new MySqlCommand("SELECT equipmentparametermaster.ParameterTypeID,equipmentparameterdata.ParamDataID,equipmentparametermaster.ParameterID,equipmentparametermaster.ParameterName,equipmentparameterdata.ParamData,CONCAT_WS(':',packetmaster.Hours,packetmaster.Minutes,packetmaster.Seconds,packetmaster.MilliSeconds)as Time FROM equipmentparameterdata inner join packetmaster on packetmaster.PacketMasterID=equipmentparameterdata.PacketMasterID inner join equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID where equipmentparametermaster.ParameterTypeID='" + ParameterTypeID + "' and equipmentparametermaster.ParameterID='" + ParameterID + "'", con);
                    cmd1.CommandType = CommandType.Text;
                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    con.Close();
                    //string Colors="";
                    string ParamData;
                    string Time;
                    string[] a = new string[] { };
                    string[] a1 = new string[] { };

                   
                        for (var j = 0; j < dt1.Rows.Count; j++)
                        {
                      
                        ParamData = dt1.Rows[j]["ParamData"].ToString();
                            Time = dt1.Rows[j]["Time"].ToString();
                            a = a.Append(ParamData).ToArray();
                            a1 = a1.Append(Time).ToArray();
                      
                    }

                        add_list.Add(new GetParamsModel { ParameterID = ParameterID, label = ParameterName, data = a, time = a1 , fill="false" });
                    
                }
            }
            return add_list;
        }

        [Route("api/DataRetrieve/GetAllParams1")]
        [HttpGet]
        public object GetAllParams1(int id)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterTypeID,ParameterID,ParameterName FROM itr.equipmentparametermaster where ParameterTypeID='" + id + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamData>();
            var add_list = new List<GetParamsModel>();
            if (dt.Rows.Count > 0)
            {
                string[] color = new string[] { "purple", "lightblue", "orange", "brown", "black", "green", "yellow", "red", "blue", "pink", "voilet", "lightyellow", };

                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    int ParameterTypeID = Convert.ToInt32(dt.Rows[i]["ParameterTypeID"].ToString());
                    int ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString());
                    string ParameterName = dt.Rows[i]["ParameterName"].ToString();
                    con.Open();
                    MySqlCommand cmd1 = new MySqlCommand("SELECT equipmentparametermaster.ParameterTypeID,equipmentparameterdata.ParamDataID,equipmentparametermaster.ParameterID,equipmentparametermaster.ParameterName,equipmentparameterdata.ParamData,CONCAT_WS(':',packetmaster.Hours,packetmaster.Minutes,packetmaster.Seconds,packetmaster.MilliSeconds)as Time FROM equipmentparameterdata inner join packetmaster on packetmaster.PacketMasterID=equipmentparameterdata.PacketMasterID inner join equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID where equipmentparametermaster.ParameterTypeID='" + ParameterTypeID + "' and equipmentparametermaster.ParameterID='" + ParameterID + "'", con);
                    cmd1.CommandType = CommandType.Text;
                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    MySqlCommand cmd2 = new MySqlCommand("SELECT DISTINCT CONCAT_WS(':',packetmaster.Hours,packetmaster.Minutes,packetmaster.Seconds,packetmaster.MilliSeconds)as Time FROM equipmentparameterdata inner join packetmaster on packetmaster.PacketMasterID=equipmentparameterdata.PacketMasterID inner join equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID where equipmentparametermaster.ParameterTypeID='" + ParameterTypeID + "'", con);
                    cmd2.CommandType = CommandType.Text;
                    MySqlDataAdapter da2 = new MySqlDataAdapter(cmd2);
                    DataTable dt2 = new DataTable();
                    da2.Fill(dt2);
                    con.Close();
                    //string Colors="";
                    //string ParamData;
                    string Time;
                   // var a = null ;
                    string[] a1 = new string[] { };
                    for (var j = 0; j < dt2.Rows.Count; j++)
                    {

                       // ParamData = dt1.Rows[j]["ParamData"].ToString();
                        Time = dt1.Rows[j]["Time"].ToString();
                        for (var k = 0; k < dt1.Rows.Count; k++)
                        {
                            string t1 = dt1.Rows[k]["Time"].ToString();
                                    if (Time.Equals(t1))
                                           {
                                var data = dt1.Rows[k]["data"].ToString();
                                             // a.Add({"param": data});
                                           }
                        }
                        //a = a.Append(ParamData).ToArray();
                        //a1 = a1.Append(Time).ToArray();

                    }

                   // add_list.Add(new GetParamsModel { ParameterID = ParameterID, label = ParameterName, data = a, time = a1, borderColor = color[i], fill = "false" });

                }
            }
            return add_list;
        }

        [Route("api/DataRetrieve/GetMaxParamData")]
        [HttpGet]
        public IEnumerable<GetParamData> GetMaxParamData()
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT(ParameterID),MAX(ParamData)as ParamData from  itr.equipmentparameterdata group by ParameterID", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamData>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetParamData { parameterid = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString()), mean = dt.Rows[i]["ParamData"].ToString() }); ;
                }
            }
            return pdata;
        }

        [Route("api/DataRetrieve/MaxParamDataByID")]
        [HttpGet]
        public IEnumerable<GetParamData> MaxParamDataByID(int id)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT(ParameterID),MAX(ParamData)as ParamData from  itr.equipmentparameterdata  where ParameterID='"+id+"' group by ParameterID", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamData>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetParamData { parameterid = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString()), mean = dt.Rows[i]["ParamData"].ToString() }); ;
                }
            }
            return pdata;
        }

        //            for (var j = 0; j < dt2.Rows.Count; j++)
        //            {

        //                //ParamData = dt1.Rows[j]["ParamData"].ToString();
        //                Time = dt2.Rows[j]["Time"].ToString();
        //                for (var k = 0; k < dt1.Rows.Count; k++)
        //                {
        //                    string t1 = dt1.Rows[k]["Time"].ToString();
        //                    if (Time.Equals(t1))
        //                    {
        //                        a = a.Add({}).ToArray();
        //                    }
        //                }




        //            }
        //          }

        //            add_list.Add(new GetParamsModel {  data = a  });

        //        }
        //    }
        //    return add_list;
        //}
        //[Route("api/DataRetrieve/GetPacketDataByID")]
        //[HttpGet]
        //public object GetPacketDataByID()
        //{
        //    var result = (from e in this.mySqlDBContext.EquipmentParameterDetailModels
        //                  join d
        //                  in this.mySqlDBContext.EquipmentParameterModels on e.ParameterID equals d.ParameterID
        //                  join f
        //                   in this.mySqlDBContext.PacketModels on e.PacketMasterID equals f.PacketMasterID
        //                  select new
        //                  {
        //                      PacketID = f.PacketID,
        //                      ParameterID = d.ParameterID,
        //                      ParamData = e.ParamData,
        //                      ParameterName = d.ParameterName,
        //                      FlightID = e.FlightID,
        //                      Time = f.Hours.ToString() + ":" + f.Minutes.ToString() + ":" + f.Seconds.ToString() + ":" + f.MilliSeconds.ToString(),
        //                  }).ToList();
        //    return result;

        //}


        [Route("api/DataRetrieve/GetAllParametersByFid")]
        [HttpGet]
        public IEnumerable<GetParamData> GetAllParametersByEid(int id)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            //MySqlCommand cmd = new MySqlCommand("SELECT distinct ParameterName,equipmentparametermaster.ParameterID FROM `equipmentparametermaster` INNER JOIN equipmentparameterdata ON equipmentparametermaster.ParameterID = equipmentparameterdata.ParameterID where FlightID='1' ORDER BY `ParameterID` ASC ", con);
            // MySqlCommand cmd = new MySqlCommand("SELECT distinct ParameterName,equipmentparametermaster.ParameterID FROM `equipmentparametermaster` INNER JOIN equipmentparameterdata ON equipmentparametermaster.ParameterID = equipmentparameterdata.ParameterID where FlightID='" + id + "' ORDER BY `ParameterID` ASC ", con);
            MySqlCommand cmd = new MySqlCommand("SELECT EquipmentID,ParameterID,ParameterName,StartByte,EndByte,DataType,Remark from equipmentparametermaster where ParameterTypeID IS NoT NULL and EquipmentID='" + id + "'", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamData>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetParamData { parameterid = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString()), name
                        = dt.Rows[i]["ParameterName"].ToString() }); 
                }
            }
            return pdata;
        }

        [Route("api/DataRetrieve/getParametersByFid")]
        [HttpGet]
        public object getParametersByFid(int id,int fid)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            //MySqlCommand cmd = new MySqlCommand("SELECT distinct ParameterName,equipmentparametermaster.ParameterID FROM `equipmentparametermaster` INNER JOIN equipmentparameterdata ON equipmentparametermaster.ParameterID = equipmentparameterdata.ParameterID where FlightID='1' ORDER BY `ParameterID` ASC ", con);
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT equipmentparametermaster.ParameterID,equipmentparametermaster.ParameterName,equipmentparametermaster.ParameterTypeID FROM equipmentparametermaster inner join parametertypemaster on equipmentparametermaster.ParameterTypeID = parametertypemaster.ParameterTypeID inner join equipmentparameterdata on equipmentparameterdata.ParameterID = equipmentparametermaster.ParameterID where equipmentparametermaster.ParameterTypeID ='"+id+"' and equipmentparameterdata.FlightID = '"+fid+"'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<EquipmentParameterModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new EquipmentParameterModel
                    {
                        ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString()),
                        ParameterTypeID = Convert.ToInt32(dt.Rows[i]["ParameterTypeID"].ToString()),
                        ParameterName= dt.Rows[i]["ParameterName"].ToString()
                    });
                }
            }
           
            return pdata;
        }

        [Route("api/DataRetrieve/GetStationsByMission")]
        [HttpGet]
        public IEnumerable<GetParamDataByMission> GetStationsByMission(int id)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select prioritymapping.PriorityMappingID,prioritymapping.TSID,telemetrystationtable.Name,prioritymapping.Priority,telemetrystationtable.Port  from prioritymapping inner join telemetrystationtable on telemetrystationtable.TSID=prioritymapping.TSID where prioritymapping.MissionID = '" + id + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamDataByMission>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    int tid = Convert.ToInt32(dt.Rows[i]["TSID"].ToString());
                    string Port = dt.Rows[i]["Port"].ToString();
                    con.Open();
                    MySqlCommand cmd1 = new MySqlCommand("select PriorityMappingID,MissionID,TSID,Priority from prioritymapping where MissionID='" + id + "' and TSID='" + tid + "'", con);

                    cmd1.CommandType = CommandType.Text;

                    MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);
                    con.Close();
                    int priority;
                    if(dt1.Rows.Count>0)
                    {
                        priority = Convert.ToInt32(dt1.Rows[0]["Priority"].ToString());
                    }
                    else
                    {
                        priority = 0;
                    }
                    pdata.Add(new GetParamDataByMission
                    {
                        tsid = Convert.ToInt32(dt.Rows[i]["TSID"].ToString()),
                        tsname = dt.Rows[i]["Name"].ToString(),
                        priority = priority,
                        port= Port
                    }) ;
                }
            }
            return pdata;
        }

        [Route("api/DataRetrieve/AssignPriorityToStations")]
        [HttpGet]
        public int AssignPriorityToStations(int priority, int mid,int tid)
        {
            int count = 0;
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select PriorityMappingID,MissionID,TSID,Priority from prioritymapping where MissionID='"+mid+ "' and TSID='"+ tid + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            if (dt.Rows.Count > 0)
            {
                con.Open();
                MySqlCommand cmd1 = new MySqlCommand("select PriorityMappingID,MissionID,TSID,Priority from prioritymapping where MissionID='" + mid + "'", con);

                cmd1.CommandType = CommandType.Text;  

                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                DataTable dt1 = new DataTable();
                da1.Fill(dt1);
                con.Close();
                for (var i = 0; i < dt1.Rows.Count; i++)
                {
                    int p = Convert.ToInt32(dt1.Rows[i]["Priority"].ToString());
                    if (p == priority)
                    {
                        count++;
                    }

                }
                if (count == 0)
                {
                   
                    string UpdateQuery = "update prioritymapping set Priority=@Priority where MissionID=@MissionID and TSID=@TSID";
                    try
                    {
                        using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                        {
                            myCommand.Parameters.AddWithValue("@Priority", priority);
                            myCommand.Parameters.AddWithValue("@MissionID", mid);
                            myCommand.Parameters.AddWithValue("@TSID", tid);
                            con.Open();
                            myCommand.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("error " + ex.Message);

                    }
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = "insert into prioritymapping(MissionID,TSID,Priority)values(@MissionID,@TSID,@Priority)";
                try
                {
                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {
                        myCommand1.Parameters.AddWithValue("@MissionID", mid);
                        myCommand1.Parameters.AddWithValue("@TSID", tid);
                        myCommand1.Parameters.AddWithValue("@Priority", priority);
                        con.Open();
                        myCommand1.ExecuteNonQuery();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("error " + ex.Message);

                }
                return 1;
            }

        }

        [Route("api/DataRetrieve/GetPriorityDetails")]
        [HttpGet]
        public IEnumerable<GetPriorityMapping> GetPriorityDetails(int MissionID)
        {
           
            var pdata=UDPSocket.GetPriorityMappingDetails(MissionID);
            return pdata;

        }

        [Route("api/DataRetrieve/UDP1")]
        [HttpGet]
        public void UDP1()
        {
            UdpClient receivingUdpClient = new UdpClient(8000);

            //Creates an IPEndPoint to record the IP Address and port number of the sender.
            // The IPEndPoint will allow you to read datagrams sent from any source.
            decimal myVal = Decimal.Parse("409252E-23", System.Globalization.NumberStyles.Float);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                string returnData = Encoding.ASCII.GetString(receiveBytes);

                System.Diagnostics.Debug.WriteLine("This is the message you received " +
                                       returnData.ToString());
                System.Diagnostics.Debug.WriteLine("This message was sent from " +
                                            RemoteIpEndPoint.Address.ToString() +
                                            " on their port number " +
                                            RemoteIpEndPoint.Port.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

        }
        static float ToFloat(byte[] input)
        {
            byte[] newArray = new[] { input[2], input[3], input[0], input[1] };
            return BitConverter.ToSingle(newArray, 0);
        }

        [Route("api/DataRetrieve/GetParamType1")]
        [HttpGet]
        public IEnumerable<ParameterType> GetParamType(int id)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            //MySqlCommand cmd = new MySqlCommand("SELECT distinct ParameterName,equipmentparametermaster.ParameterID FROM `equipmentparametermaster` INNER JOIN equipmentparameterdata ON equipmentparametermaster.ParameterID = equipmentparameterdata.ParameterID where FlightID='1' ORDER BY `ParameterID` ASC ", con);
            // MySqlCommand cmd = new MySqlCommand("SELECT distinct ParameterName,equipmentparametermaster.ParameterID FROM `equipmentparametermaster` INNER JOIN equipmentparameterdata ON equipmentparametermaster.ParameterID = equipmentparameterdata.ParameterID where FlightID='" + id + "' ORDER BY `ParameterID` ASC ", con);
            MySqlCommand cmd = new MySqlCommand("select DISTINCT parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType FROM equipmentparametermaster INNER JOIN parametertypemaster ON parametertypemaster.ParameterTypeID = equipmentparametermaster.ParameterTypeID where parametertypemaster.ParameterTypeID IS NoT NULL and EquipmentID='" + id + "'", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ParameterType>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new ParameterType
                    {
                        parameterTypeID = Convert.ToInt32(dt.Rows[i]["ParameterTypeID"].ToString()),
                        parameterType = dt.Rows[i]["ParameterType"].ToString(),


                    });
                }
            }
            return pdata;
        }

        [Route("api/DataRetrieve/StartMission")]
        [HttpGet]
        public int StartMission(int sid, int mid, int fid)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            string insertQuery = "insert into OperateMissionTable(EquipmentID,MissionID,FlightID,StartTime,EndTime,Status)values(@EquipmentID,@MissionID,@FlightID,@StartTime,@EndTime,@Status)";
            try
            {
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@EquipmentID", sid);
                    myCommand1.Parameters.AddWithValue("@MissionID", mid);
                    myCommand1.Parameters.AddWithValue("@FlightID", fid);
                    myCommand1.Parameters.AddWithValue("@StartTime", System.DateTime.Now);
                    myCommand1.Parameters.AddWithValue("@EndTime", null);
                    myCommand1.Parameters.AddWithValue("@Status", "Active");
                    con.Open();
                    myCommand1.ExecuteNonQuery();
                    con.Close();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);
                return 0;
            }

        }


        [Route("api/DataRetrieve/EndMission")]
        [HttpGet]
        public int EndMission(int sid, int mid, int fid)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            string insertQuery = "update OperateMissionTable set EndTime=@EndTime,Status=@Status where EquipmentID=@EquipmentID and MissionID=@MissionID and FlightID=@FlightID and Status='Active'";
            try
            {
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@EndTime", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                    myCommand1.Parameters.AddWithValue("@Status", "InActive");
                    myCommand1.Parameters.AddWithValue("@EquipmentID", sid);
                    myCommand1.Parameters.AddWithValue("@MissionID", mid);
                    myCommand1.Parameters.AddWithValue("@FlightID", fid);
                    con.Open();
                    myCommand1.ExecuteNonQuery();
                    con.Close();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);
                return 0;
            }

        }

        [Route("api/DataRetrieve/Playback")]
        [HttpGet]
        public PlayBackParametersData[] Playback(int sid,int fid,int pid,int min,int max)
        {
            DataTable dtp = new DataTable();
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select equipmentparameterdata.ParameterID,equipmentparametermaster.ParameterName,equipmentparameterdata.ParamData,equipmentparameterdata.Count,parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType  from itr.equipmentparameterdata inner join itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID inner join itr.parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where equipmentparametermaster.EquipmentID='" + sid + "' and equipmentparameterdata.FlightID='" + fid + "' and equipmentparameterdata.ParameterID='"+pid+"' and equipmentparameterdata.Count between '"+min+"' and '"+max+"'", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();

            var pdata = new PlayBackParametersData[dtp.Rows.Count];
            try
            {
                for (int i = 0; i < dtp.Rows.Count; i++)
                {
                    pdata[i] = new PlayBackParametersData(
                        Convert.ToInt32(dtp.Rows[i]["ParameterID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterName"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParamData"].ToString()),
                        Convert.ToInt32(dtp.Rows[i]["ParameterTypeID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterType"].ToString()),
                         Convert.ToInt32(dtp.Rows[i]["Count"].ToString())
                    );
                }
                System.Diagnostics.Debug.WriteLine(pdata);

            }

            catch (Exception ex)
            {

            }
            return pdata;
        }


        [Route("api/DataRetrieve/Playback1")]
        [HttpGet]
        public PlayBackParametersData[] Playback1(int sid, int fid, int pid)
        {
            DataTable dtp = new DataTable();
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select equipmentparameterdata.ParameterID,equipmentparametermaster.ParameterName,equipmentparameterdata.ParamData,equipmentparameterdata.Count,parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType  from itr.equipmentparameterdata inner join itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID inner join itr.parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where equipmentparametermaster.EquipmentID='" + sid + "' and equipmentparameterdata.FlightID='" + fid + "' and equipmentparameterdata.ParameterID='" + pid + "'", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();

            var pdata = new PlayBackParametersData[dtp.Rows.Count];
            try
            {
                for (int i = 0; i < dtp.Rows.Count; i++)
                {
                    pdata[i] = new PlayBackParametersData(
                        Convert.ToInt32(dtp.Rows[i]["ParameterID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterName"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParamData"].ToString()),
                        Convert.ToInt32(dtp.Rows[i]["ParameterTypeID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterType"].ToString()),
                         Convert.ToInt32(dtp.Rows[i]["Count"].ToString())
                    );
                }
                System.Diagnostics.Debug.WriteLine(pdata);

            }

            catch (Exception ex)
            {

            }
            return pdata;
        }

        [Route("api/DataRetrieve/PlaybackXY")]
        [HttpGet]
        public PlayBackParametersData[] PlaybackXY(int sid, int fid, int pidX,int pidY, int min, int max)
        {
            DataTable dtp = new DataTable();
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select equipmentparameterdata.ParameterID,equipmentparametermaster.ParameterName,equipmentparameterdata.ParamData,equipmentparameterdata.Count,parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType  from itr.equipmentparameterdata inner join itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID inner join itr.parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where equipmentparametermaster.EquipmentID='" + sid + "' and equipmentparameterdata.FlightID='" + fid + "' and equipmentparameterdata.ParameterID='" + pidX + "' and equipmentparameterdata.ParameterID='" + pidY + "' and equipmentparameterdata.Count between '" + min + "' and '" + max + "'", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();

            var pdata = new PlayBackParametersData[dtp.Rows.Count];
            try
            {
                for (int i = 0; i < dtp.Rows.Count; i++)
                {
                    pdata[i] = new PlayBackParametersData(
                        Convert.ToInt32(dtp.Rows[i]["ParameterID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterName"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParamData"].ToString()),
                        Convert.ToInt32(dtp.Rows[i]["ParameterTypeID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterType"].ToString()),
                         Convert.ToInt32(dtp.Rows[i]["Count"].ToString())
                    );
                }
                System.Diagnostics.Debug.WriteLine(pdata);

            }

            catch (Exception ex)
            {

            }
            return pdata;
        }

        [Route("api/DataRetrieve/InsertDisplayModeOffline")]
        [HttpGet]
        public int InsertDisplayModeOffline(int sid, int mid, int fid, int frequency, string year)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            string insertQuery = "insert into displaymodeoffline(SystemID,MissionID,FlightID,Frequency,Year,UserID,DateTime)values(@SystemID,@MissionID,@FlightID,@Frequency,@Year,@UserID,@DateTime)";
            try
            {
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@SystemID", sid);
                    myCommand1.Parameters.AddWithValue("@MissionID", mid);
                    myCommand1.Parameters.AddWithValue("@FlightID", fid);
                    myCommand1.Parameters.AddWithValue("@Frequency", frequency);
                    myCommand1.Parameters.AddWithValue("@Year", year);
                    myCommand1.Parameters.AddWithValue("@UserID", 0);
                    myCommand1.Parameters.AddWithValue("@DateTime", System.DateTime.Now);
                    con.Open();
                    myCommand1.ExecuteNonQuery();
                    con.Close();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);
                return 0;
            }

        }

        [Route("api/DataRetrieve/InsertIntoPlayback")]
        [HttpGet]
        public int InsertIntoPlayback(int sid, int mid, int fid,int year ,int frequency,int pid,int min,int max)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            string insertQuery = "INSERT INTO playbackmodeoffline(SystemID,MissionID,FlightID,Year,Frequency,Mode,ParameterID,MinimumVal,MaximumVal,UserID,Status,DateTime)VALUES(@SystemID,@MissionID,@FlightID,@Year,@Frequency,@Mode,@ParameterID,@MinimumVal,@MaximumVal,@UserID,@Status,@DateTime);";
            try
            {
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@SystemID", sid);
                    myCommand1.Parameters.AddWithValue("@MissionID", mid);
                    myCommand1.Parameters.AddWithValue("@FlightID", fid);
                    myCommand1.Parameters.AddWithValue("@Year", year);
                    myCommand1.Parameters.AddWithValue("@Frequency", frequency);
                    myCommand1.Parameters.AddWithValue("@Mode", "Playback");
                    myCommand1.Parameters.AddWithValue("@ParameterID", pid);
                    myCommand1.Parameters.AddWithValue("@MinimumVal", min);
                    myCommand1.Parameters.AddWithValue("@MaximumVal", max);
                    myCommand1.Parameters.AddWithValue("@UserID", 0);
                    myCommand1.Parameters.AddWithValue("@Status", "Active");
                    myCommand1.Parameters.AddWithValue("@DateTime", System.DateTime.Now);
                    con.Open();
                    myCommand1.ExecuteNonQuery();
                    con.Close();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);
                return 0;
            }

        }

        [Route("api/DataRetrieve/getMultiLineParams1")]
        [HttpGet]
        public List<MultiLineParameters> getMultiLineParams(int sid, int fid,int pid1)
        {
            int id=1;
           
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
            var add_list = new List<MultiLineParameters>();
            
            try
            {
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT count from equipmentparameterdata where FlightID='"+fid+"'", con);
            cmd.CommandType = CommandType.Text;
            DataTable dtp = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
           
                for (int i = 0; i < dtp.Rows.Count; i++)
                {
                string count = dtp.Rows[i]["count"].ToString();
                 
                    MySqlCommand cmd1 = new MySqlCommand("SELECT ParameterID,ParamData,count from equipmentparameterdata where FlightID='"+fid+"' and Count='"+count+"' and ParameterID in('70','76')", con);
                     cmd1.CommandType = CommandType.Text;
                     DataTable dtp1 = new DataTable();
                     MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                     da1.Fill(dtp1);
                    string[] a = new string[] { };
                    string[] a1 = new string[] { };
                    string pid;
                    string pdata;
                    List<string> list1 = new List<string>();
                    List<string> list2 = new List<string>();
                    for (int j=0;j< dtp1.Rows.Count; j++)
                    {
                        
                        pid = dtp1.Rows[j]["ParameterID"].ToString();
                        pdata = dtp1.Rows[j]["ParamData"].ToString();
                        list1.Add(pid);
                        list2.Add(pdata);
                       
                    }
                    a = list1.ToArray();
                    a1 = list2.ToArray();
                    add_list.Add(new MultiLineParameters {Time=count, ParameterID=a, ParamData=a1});
                    id++;
                }
               

            }

            catch (Exception ex)
            {
                con.Close();
            }
            finally
            {
                con.Close();
            }
            return add_list;
        }

       
    }

} 
