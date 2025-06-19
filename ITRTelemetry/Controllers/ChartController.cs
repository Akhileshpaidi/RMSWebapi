using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DomainModel;
using ITRTelemetry.DataStorage;
using ITRTelemetry.HubConfig;
using ITRTelemetry.TimerFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;

namespace ITRTelemetry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private UdpClient receivingUdpClient;
        private IPEndPoint RemoteIpEndPoint;
        int EquipmentID, FlightID;

        private IHubContext<ChartHub> _hub;
        public ChartController(IHubContext<ChartHub> hub)
        {

            _hub = hub;
            int myport = 9000;
            bool alreadyinuse = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(p => p.Port == myport);
            if (alreadyinuse == false)
            {
                receivingUdpClient = new UdpClient(myport);
                //Creates an IPEndPoint to record the IP Address and port number of the sender.
                // The IPEndPoint will allow you to read datagrams sent from any source.
                // RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("12.123.4.2"), 0);
                RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }
            else
            {
               
            }
          
        }
        public IActionResult Get(int EID,int FID)
        {
            var timerManager = new TimerManager(() => _hub.Clients.All.SendAsync("transferchartdata", GenerateTestData()));
            EquipmentID = EID;
            FlightID=FID;
            return Ok(new { Message = "Request Completed" });
        }

        private LiveDataVisualizationModel[] GenerateTestData()
        {
            DataTable dtp = new DataTable();
            DataTable dtpvalues = new DataTable();
            dtpvalues.Columns.Add("ParameterID");
            dtpvalues.Columns.Add("ParameterName");
            dtpvalues.Columns.Add("ParamData");
            dtpvalues.Columns.Add("ParameterTypeID");
            dtpvalues.Columns.Add("ParameterType");
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterID, ParameterName, parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType,StartByte, EndByte, DataType from equipmentparametermaster inner join parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where EquipmentID='" + EquipmentID + "' and equipmentparametermaster.ParameterTypeID IS  NOT NULL", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();

            var pdata = new LiveDataVisualizationModel[dtp.Rows.Count];
            try
            {

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
                        if (size == 1)
                        {
                            string valueBack = paramByteArray[0].ToString();
                            dtpvalues.Rows.Add(s, name, valueBack, typeid, typename);
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
                        if (size == 8)
                        {
                            long valueBack = BitConverter.ToInt64(paramByteArray, 0);
                            dtpvalues.Rows.Add(s, name, valueBack, typeid, typename);
                        }

                    }
                    for (int i = 0; i < dtpvalues.Rows.Count; i++)
                    {
                        pdata[i] = new LiveDataVisualizationModel(
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterName"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParamData"].ToString()),
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterTypeID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterType"].ToString()),
                            FlightID
                        );

                    }
                    //for (int k = 0; k < pdata.Length; k++)
                    //{
                    //    LiveDataVisualizationModel.SaveParametersData(pdata[k]);
                    //}
                    System.Diagnostics.Debug.WriteLine(pdata);
                }
            }
            catch (Exception ex)
            {

            }

            return pdata;
        }
    }
}