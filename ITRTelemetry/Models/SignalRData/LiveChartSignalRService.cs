using DomainModel;
using ITRTelemetry.Hubs;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ITRTelemetry.Models.SignalRData
{
    public class LiveChartSignalRService
    {
        private IHubContext<LiveChartSignalRHub> hubContext { get; set; }

        private readonly object updateStockPricesLock = new object();

        private XYValues[] tickData;
        private int lastTickIndex;
        private Timer timer;
        int count = 0;
        private UdpClient receivingUdpClient;
        private IPEndPoint RemoteIpEndPoint;
        int EquipmentID, XParamID, YParamID,FlightID;
        string XVal = "";
        string YVal = "";
        public LiveChartSignalRService(IHubContext<LiveChartSignalRHub> hubCtx)
        {
          
            hubContext = hubCtx;
            receivingUdpClient = new UdpClient(4501);
            //Creates an IPEndPoint to record the IP Address and port number of the sender.
            // The IPEndPoint will allow you to read datagrams sent from any source.
            // RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("12.123.4.2"), 0);
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
          
            timer = new Timer(Update, null, 10, 10);
        }
        public IEnumerable<XYValues> GetAllData(int EquipID, int XParameterID, int YParameterID,int FID)
        {
            EquipmentID = EquipID;
            XParamID = XParameterID;
            YParamID = YParameterID;
            FlightID = FID;
            tickData = GenerateTestData();
            return tickData;
        }
        private void Update(object state)
        {
            lock (updateStockPricesLock)
            {
                tickData = GenerateTestData();
                BroadcastStockPrice(tickData);
            }
        }

        private void BroadcastStockPrice(XYValues[] item)
        {
            hubContext.Clients.All.SendAsync("updateParamData", item);
        }

        private XYValues[] GenerateTestData()
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
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterID, ParameterName, parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType,StartByte, EndByte, DataType from equipmentparametermaster inner join parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where EquipmentID='"+ EquipmentID + "' and equipmentparametermaster.ParameterTypeID IS  NOT NULL", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();

            var pdata = new LiveChartModel[dtp.Rows.Count];
            var xydata = new XYValues[1];
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
                        if (size == 1)//unsigned data
                        {
                            string valueBack = paramByteArray[0].ToString();
                            dtpvalues.Rows.Add(s, name, valueBack, typeid, typename);
                        }
                        if (size == 2)//
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
                        pdata[i] = new LiveChartModel(
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterName"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParamData"].ToString()),
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterTypeID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterType"].ToString()),
                            FlightID
                        );
                      

                    }
                    for (int k = 0; k < pdata.Length; k++)
                    {
                        //LiveChartModel.SaveParametersData(pdata[k]);
                        if (pdata[k].ParameterID == XParamID)
                        {
                            XVal = pdata[k].ParamData;
                        }
                        if (pdata[k].ParameterID == YParamID)
                        {
                            YVal = pdata[k].ParamData;
                        }

                    }

                    xydata[0] = new XYValues(
                       XVal, YVal
                    );
                    System.Diagnostics.Debug.WriteLine(xydata);
                }
            }
            catch (Exception ex)
            {

            }

            return xydata;
        }
    }
}
