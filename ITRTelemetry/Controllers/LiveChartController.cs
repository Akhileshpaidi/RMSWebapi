using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace ITRTelemetry.Controllers
{
    //[Produces("application/json")]
    [ApiController]
    public class LiveChartController : ControllerBase
    {
        private UdpClient receivingUdpClient;
        private IPEndPoint RemoteIpEndPoint;

        public LiveChartController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        [Route("api/LiveChart/GetParamDetails")]
        [HttpGet]
        public dynamic[] GenerateLiveData()
        
        {
            //myDb1ConnectionString = _configuration.GetConnectionString("myDb1");
            DataTable dtp = new DataTable();
            DataTable dtpvalues = new DataTable();
            dtpvalues.Columns.Add("ParameterID");
            dtpvalues.Columns.Add("ParameterName");
            dtpvalues.Columns.Add("ParamData");
            dtpvalues.Columns.Add("ParameterTypeID");
            dtpvalues.Columns.Add("ParameterType");
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterID, ParameterName, parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType,StartByte, EndByte, DataType from equipmentparametermaster inner join parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where EquipmentID=2 and equipmentparametermaster.ParameterTypeID IS  NOT NULL", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();
      
            var pdata = new LiveChartModel[dtp.Rows.Count];
            try
            {
                receivingUdpClient = new UdpClient(9000);
                //Creates an IPEndPoint to record the IP Address and port number of the sender.
                // The IPEndPoint will allow you to read datagrams sent from any source.
                // RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("12.123.4.2"), 0);
                RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                System.Diagnostics.Debug.WriteLine("Bytes Data " + receiveBytes);
                //if the data type is Byte Format User this code 
                if (receiveBytes.Length > 0)
                {
                    for (int k = 0; k < dtp.Rows.Count; k++)
                    {
                        int s = Convert.ToInt32(dtp.Rows[k]["ParameterID"].ToString());
                        int startbyte = Convert.ToInt32(dtp.Rows[k]["StartByte"].ToString());
                        int endbyte = Convert.ToInt32(dtp.Rows[k]["EndByte"].ToString());
                        string datatype = dtp.Rows[k]["DataType"].ToString();
                        string name = dtp.Rows[k]["ParameterName"].ToString();
                        string typeid = dtp.Rows[k]["ParameterTypeID"].ToString();
                        string typename = dtp.Rows[k]["ParameterType"].ToString();
                        int j = 0;
                        int size = endbyte - (startbyte - 1);
                        byte[] paramByteArray = new byte[size];
                        for (int i = startbyte; i <= endbyte; i++)
                        {
                            paramByteArray[j] = receiveBytes[i];
                            j++;
                        }
                        //if (BitConverter.IsLittleEndian)
                        //    Array.Reverse(paramByteArray);                             
                        string paramlength = paramByteArray.Length.ToString();
                        if(size == 1)
                        {
                            string valueBack = paramByteArray[0].ToString();
                            dtpvalues.Rows.Add(s,name,valueBack,typeid,typename);
                        }
                        if (size == 2)
                        {
                            short valueBack = BitConverter.ToInt16(paramByteArray, 0);
                            dtpvalues.Rows.Add(s, name, valueBack, typeid, typename);
                        }
                        if (size == 4)
                        {
                            float valueBack = BitConverter.ToSingle(paramByteArray, 0);
                            dtpvalues.Rows.Add(s, name, valueBack, typeid, typename);
                        }
                        if(size == 8)
                        {
                            long valueBack = BitConverter.ToInt64(paramByteArray, 0);
                            dtpvalues.Rows.Add(s, name, valueBack, typeid, typename);
                        }

                    }
                    for (int i = 0; i < dtpvalues.Rows.Count; i++)
                    {
                        pdata[i] = new LiveChartModel(
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterName"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParamData"].ToString()),
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterTypeID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterType"].ToString()),
                            1
                        );
                    }
                    System.Diagnostics.Debug.WriteLine(pdata);

                    

                }
            }
            catch (Exception ex)
            {

            }

            return pdata;
        }

        [Route("api/LiveChart/SetMissionID")]
        [HttpGet]
        public int SetMissionID(int sid,int mid)
        {
            //HttpContext.Session.SetString("MissionID", id);
            //string val = HttpContext.Session.GetString("MissionID");
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "insert into tbllogs(EquipmentID,MissionID,Datetime)values(@EquipmentID,@MissionID,@Datetime)";
            int result = 0;
            try
            {
                using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                {
                    myCommand1.Parameters.AddWithValue("@EquipmentID", sid);
                    myCommand1.Parameters.AddWithValue("@MissionID", mid);
                    myCommand1.Parameters.AddWithValue("@Datetime", DateTime.Now);
                    con.Open();
                    myCommand1.ExecuteNonQuery();
                    con.Close();
                    result = 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);

            }
            return result;

        }
    }
}
