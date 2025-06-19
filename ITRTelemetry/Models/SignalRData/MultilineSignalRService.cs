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
    public class MultilineSignalRService
    {
        private IHubContext<MultiLineSignalRData> hubContext { get; set; }

        private readonly object updateStockPricesLock = new object();

        private PlotingData[] tickData;
        private int lastTickIndex;
        private Timer timer;
        int count = 0;
        private UdpClient receivingUdpClient;
        private IPEndPoint RemoteIpEndPoint;
        public MultilineSignalRService(IHubContext<MultiLineSignalRData> hubCtx)
        {
            count++;
            hubContext = hubCtx;
            // receivingUdpClient = new UdpClient(5556);
            receivingUdpClient = new UdpClient(4501);
            //Creates an IPEndPoint to record the IP Address and port number of the sender.
            // The IPEndPoint will allow you to read datagrams sent from any source.
            // RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("12.123.4.2"), 0);
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            tickData = GenerateTestData1();
            lastTickIndex = tickData.Length - 1;
            timer = new Timer(Update, null, 100, 100);
           
        }
        public IEnumerable<PlotingData> GetAllData()
        {
            var data = new List<PlotingData>();
            lock (updateStockPricesLock)
            {
                for (var i = lastTickIndex; data.Count < 10000; i--)
                {
                    data.Add(tickData[i]);
                    if (i == 0)
                    {
                        i = tickData.Length - 1;
                    }
                }
            }

            return data;
        }


        private void Update(object state)
        {
            tickData = GenerateTestData1();
            lock (updateStockPricesLock)
            {
                lastTickIndex = (lastTickIndex + 1) % tickData.Length;
                var tick = tickData[lastTickIndex];
                // tick.Date = DateTime.Now;

                BroadcastStockPrice(tick);
            }
        }

        private void BroadcastStockPrice(PlotingData item)
        {
            hubContext.Clients.All.SendAsync("updateParamData", item);
        }



        //private PlotingData[] GenerateTestData()
        //{      
        //    DataTable dt = new DataTable();
        //    DataSet dset = new DataSet();
        //    MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itrdemo;sslmode=none;");
        //    MySqlCommand cmd;
        //    MySqlDataAdapter da;
        //    con.Open();
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=1", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter1");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=2", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter2");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=3", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter3");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=4", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter4");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=5", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter5");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=6", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter6");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=7", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter7");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=8", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter8");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=9", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter9");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=10", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter10");
        //    cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=11", con);
        //    cmd.CommandType = CommandType.Text;
        //    da = new MySqlDataAdapter(cmd);
        //    da.Fill(dset, "Parameter11");
        //    con.Close();
        //    var pdata = new PlotingData[dset.Tables[0].Rows.Count];
        //    if (dset.Tables.Count > 10)
        //    {
        //        for (int i = 0; i < dset.Tables[0].Rows.Count; i++)
        //        {
        //            string val = dset.Tables[0].Rows[i]["ParamValue"].ToString();
        //            pdata[i] = new PlotingData
        //            (
        //                  i,
        //                  dset.Tables[1].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[2].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[3].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[4].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[5].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[6].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[7].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[8].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[9].Rows[i]["ParamValue"].ToString(),
        //                  dset.Tables[10].Rows[i]["ParamValue"].ToString()

        //            );

        //        }
        //    }
        //    return pdata;
        //}
        private PlotingData[] GenerateTestData1()
        {
            DataTable dtp = new DataTable();
            dtp.Columns.Add("ParameterID", typeof(int));
            dtp.Columns.Add("ParameterName", typeof(string));
            dtp.Columns.Add("StartByte", typeof(int));
            dtp.Columns.Add("EndByte", typeof(int));
            dtp.Columns.Add("DataType", typeof(string));

            //dtp.Rows.Add(1, "X1", 0, 3, "Float");
            //dtp.Rows.Add(2, "Y1", 4, 7, "Float");
            dtp.Rows.Add(1, "Time", 8, 11, "Float");
            dtp.Rows.Add(3, "X2", 8, 11, "Float");
            dtp.Rows.Add(4, "Y2", 12, 15, "Float");
            dtp.Rows.Add(5, "X3", 16, 19, "Float");
            dtp.Rows.Add(6, "Y3", 20, 23, "Float");
            dtp.Rows.Add(7, "X4", 24, 27, "Float");
            dtp.Rows.Add(8, "Y4", 28, 31, "Float");
            dtp.Rows.Add(1, "X1", 24, 27, "Float");
            dtp.Rows.Add(2, "Y1", 28, 31, "Float");
            dtp.Rows.Add(10, "X5", 8, 11, "Float");
            dtp.Rows.Add(11, "Y5", 12, 15, "Float");

           
            var pdata = new PlotingData[11];
            try
            {

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                System.Diagnostics.Debug.WriteLine("Bytes Data "+ receiveBytes);
              //if the data type is Byte Format User this code 
                if (receiveBytes.Length > 0)
                {
                    PlotingDataParameters pval = new PlotingDataParameters();
                    for (int k = 0; k < dtp.Rows.Count; k++)
                    {
                        int s = Convert.ToInt32(dtp.Rows[k]["ParameterID"].ToString());
                        int startbyte = Convert.ToInt32(dtp.Rows[k]["StartByte"].ToString());
                        int endbyte = Convert.ToInt32(dtp.Rows[k]["EndByte"].ToString());
                        string datatype = dtp.Rows[k]["DataType"].ToString();
                        string name = dtp.Rows[k]["ParameterName"].ToString();
                        byte[] paramByteArray = new byte[4];
                        int j = 0;
                        for (int i = startbyte; i <= endbyte; i++)
                        {
                            paramByteArray[j] = receiveBytes[i];
                            j++;
                        }
                        float valueBack = BitConverter.ToSingle(paramByteArray);
                        if (name == "Time")
                        {
                            pval.Time = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("Time" + valueBack.ToString());
                        }
                        if (name == "X1")
                        {
                            pval.X1 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("X1 "+ valueBack.ToString());
                        }
                        if (name == "X2")
                        {
                            pval.X2 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("X2 " + valueBack.ToString());
                        }
                        if (name == "X3")
                        {
                            pval.X3 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("X3 " + valueBack.ToString());
                        }
                        if (name == "X4")
                        {
                            pval.X4 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("X4 " + valueBack.ToString());
                        }
                        if (name == "X5")
                        {
                            pval.X5 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("X5 " + valueBack.ToString());
                        }
                        if (name == "Y1")
                        {
                            pval.Y1 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("y1 " + valueBack.ToString());
                        }
                        if (name == "Y2")
                        {
                            pval.Y2 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("y2 " + valueBack.ToString());
                        }
                        if (name == "Y3")
                        {
                            pval.Y3 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("y3 " + valueBack.ToString());
                        }
                        if (name == "Y4")
                        {
                            pval.Y4 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("y4 " + valueBack.ToString());
                        }
                        if (name == "Y5")
                        {
                            pval.Y5 = valueBack.ToString();
                            System.Diagnostics.Debug.WriteLine("y5 " + valueBack.ToString());
                        }


                    }
                    
                    //for (int i = 0; i < dtp.Rows.Count; i++)
                    //{
                    //    pdata[i] = new PlotingData(
                    //       i,
                    //       pval.X1,
                    //       pval.X2,
                    //       pval.X3,
                    //       pval.X4,
                    //       pval.X5,
                    //       pval.Y1,
                    //       pval.Y2,
                    //       pval.Y3,
                    //       pval.Y4,
                    //       pval.Y5,
                    //       pval.Time
                    //    );
                    //    System.Diagnostics.Debug.WriteLine(pdata[i]);
                    //}
                    System.Diagnostics.Debug.WriteLine(pdata);


                }


            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            finally
            {

            }
            return pdata;

        }
    }

    }

