using DomainModel;
using ITRTelemetry.ExtensionMethods;
using ITRTelemetry.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ITRTelemetry.Models.SignalRData
{
    public class LiveDataVisualizationService
    {
       
        private IHubContext<LiveDataVisualizationHub> hubContext { get; set; }

        private readonly object updateStockPricesLock = new object();

        private StationParametersData[] tickData;
        private int lastTickIndex;
        private Timer timer;
        int count = 0;
        private UdpClient receivingUdpClient;
        private IPEndPoint RemoteIpEndPoint;
        int EquipmentID,FlightID,MissionID;
        int TSID, Validity, Priority;
        string IPAdd, Port, Name;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        string MissionStatus;
        MySqlConnection con;
        DataTable dt = new DataTable();
        DataTable dtp = new DataTable();
        // DataTable dtpvalues = new DataTable();
        private int arrsize = 0;
        private int dtcount = 0;
        private StationSelection[] data;
        public LiveDataVisualizationService(IHubContext<LiveDataVisualizationHub> hubCtx, IHttpContextAccessor httpContextAccessor)
        {

            hubContext = hubCtx;
            //MissionID = 5;
            _httpContextAccessor = httpContextAccessor;
            dt.Columns.Add("ParameterID");
            dt.Columns.Add("ParamData");
            dt.Columns.Add("FlightID");
            dt.Columns.Add("Status");
            dt.Columns.Add("Count");
            dt.Columns.Add("Date");
            PortConnection(MissionID);
            //receivingUdpClient = new UdpClient(9000);
            ////Creates an IPEndPoint to record the IP Address and port number of the sender.
            //// The IPEndPoint will allow you to read datagrams sent from any source.
            //// RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse("12.123.4.2"), 0);
            //RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            timer = new Timer(Update, null, 100, 100);
        }
        public IEnumerable<StationParametersData> GetAllData(int EquipID, int MID, int FID)
        {
            EquipmentID = EquipID;
            FlightID = FID;
            if (MissionStatus == "Started")
            {
                tickData = GenerateTestData();
            }
            return tickData;
        }
        private void Update(object state)
        {
            // lock (updateStockPricesLock)
            // {
            if (MissionStatus == "Started")
            {
                tickData = GenerateTestData();
                BroadcastParsedData(tickData);
            }

            // }
        }

        private void BroadcastParsedData(StationParametersData[] item)
        {
            count = count + 100;
            try
            {

                for (int k = 0; k < item.Length; k++)
                {
                    DataRow dr = dt.NewRow();
                    if (item[k] != null)
                    {
                        dr["ParameterID"] = item[k].ParameterID;
                        dr["ParamData"] = item[k].ParamData;
                        dr["FlightID"] = item[k].FlightID;
                        dr["Status"] = "Active";
                        dr["Count"] = count;
                        dr["Date"] = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        dt.Rows.Add(dr);
                    }
                }
                hubContext.Clients.All.SendAsync("updateParamData", item, count);
            }
            // HttpContext.Session.SetObjectAsJson("EmployeeDetails", dt);
            //}
            catch (Exception ex)
            {

            }
            finally
            {

            }
            
        }

        private StationParametersData[] GenerateTestData()
        {

            DataTable dtpvalues = new DataTable();
            dtpvalues.Columns.Add("ParameterID");
            dtpvalues.Columns.Add("ParameterName");
            dtpvalues.Columns.Add("ParamData");
            dtpvalues.Columns.Add("ParameterTypeID");
            dtpvalues.Columns.Add("ParameterType");
            //if (con == null)
            //{
            //    con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
            //    con.Open();
            //}
           
            //MySqlConnection con1 = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required;Connect Timeout=30");
            StationParametersData[] pdata= {};
            try
            {
           
             pdata = new StationParametersData[dtp.Rows.Count];
            Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);

                //if the data type is Byte Format User this code 
                if (receiveBytes.Length > 0)
                {
                    string param = receiveBytes[5].ToString();
                    int validity = checkingvalidity(param);
                    //if (validity == 0)
                    //{
                    //    PortConnection(MissionID);
                    //}
                    System.Diagnostics.Debug.WriteLine("Bytes Data " + receiveBytes);
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
                        pdata[i] = new StationParametersData(
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterName"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParamData"].ToString()),
                            Convert.ToInt32(dtpvalues.Rows[i]["ParameterTypeID"].ToString()),
                            Convert.ToString(dtpvalues.Rows[i]["ParameterType"].ToString()),
                            FlightID,
                            TSID,
                            Name,
                            IPAdd,
                            Port,
                            Priority,
                            validity
                        );

                    }
                    //for (int k = 0; k < pdata.Length; k++)
                    //{
                    //    LiveDataVisualizationModel.SaveParametersData(pdata[k]);
                    //}
                    //System.Diagnostics.Debug.WriteLine(pdata);
                }
            }
            catch (Exception ex)
            {
                //con1.Close();
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                //con1.Close();
            }
            return pdata;
        }



        private void PortConnection(int MissionID)
        {
            if (con == null)
            {
                con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
                con.Open();
            }
            // MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            if (MissionID == 0 && EquipmentID == 0)
            {

                MySqlCommand cmd1 = new MySqlCommand("SELECT * FROM tbllogs order by log_ID desc limit 1", con);
                cmd1.CommandType = CommandType.Text;

                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                DataTable dt1 = new DataTable();
                da1.Fill(dt1);
                if (dt1.Rows.Count > 0)
                {
                    MissionID = Convert.ToInt32(dt1.Rows[0]["MissionID"].ToString());
                    EquipmentID = Convert.ToInt32(dt1.Rows[0]["EquipmentID"].ToString());
                    
                    MySqlCommand command = new MySqlCommand("SELECT ParameterID, ParameterName, parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType,StartByte, EndByte, DataType from equipmentparametermaster inner join parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where EquipmentID='" + EquipmentID + "' and equipmentparametermaster.ParameterTypeID IS  NOT NULL", con);
                    command.CommandType = CommandType.Text;
                    MySqlDataAdapter data_adap = new MySqlDataAdapter(command);
                    data_adap.Fill(dtp);
                }
                // con.Close();
            }

            //con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT prioritymapping.PriorityMappingID,missiontable.EquipmentID,prioritymapping.MissionID,prioritymapping.TSID,telemetrystationtable.Name,prioritymapping.Priority,telemetrystationtable.IPAddress,telemetrystationtable.Port FROM prioritymapping inner join missiontable on missiontable.MissionID = prioritymapping.MissionID inner join telemetrystationtable on telemetrystationtable.TSID = prioritymapping.TSID where missiontable.MissionID = '" + MissionID + "' order by Priority ASC", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            arrsize = dt.Rows.Count;
            data = new StationSelection[arrsize];

            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    data[i] = new StationSelection
                    (
                      Convert.ToInt32(dt.Rows[i]["TSID"].ToString()),
                      dt.Rows[i]["Name"].ToString(),
                      dt.Rows[i]["IPAddress"].ToString(),
                      dt.Rows[i]["Port"].ToString(),
                      Convert.ToInt32(dt.Rows[i]["Priority"].ToString())
                    );
                }

            }

            // con.Close();
            for (int i = 0; i < data.Length; i++)
            {
                int validity = 0;

                receivingUdpClient = new UdpClient(Convert.ToInt32(data[i].Port));
                //Creates an IPEndPoint to record the IP Address and port number of the sender.
                // The IPEndPoint will allow you to read datagrams sent from any source.
                RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(data[i].IPAddress), 0);
                try
                {
                    int status = receivingUdpClient.Available;
                    if (status != 0)
                     {
                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    if (receiveBytes.Length > 0)
                    {
                        string param = receiveBytes[5].ToString();
                        validity = checkingvalidity(param);
                        System.Diagnostics.Debug.WriteLine("This is the message you received " +returnData.ToString());
                       // System.Diagnostics.Debug.WriteLine("This message was sent from " +RemoteIpEndPoint.Address.ToString() +" on their port number " +RemoteIpEndPoint.Port.ToString());
                        if (validity == 1)
                        {
                            TSID = Convert.ToInt32(dt.Rows[i]["TSID"].ToString());
                            Name = dt.Rows[i]["Name"].ToString();
                            IPAdd = dt.Rows[i]["IPAddress"].ToString();
                            Port = dt.Rows[i]["Port"].ToString();
                            Priority = Convert.ToInt32(dt.Rows[i]["Priority"].ToString());
                            Validity = validity;
                                // mdata[i] = new SelectedTStation
                                //(
                                //  Convert.ToInt32(dt.Rows[i]["TSID"].ToString()),
                                //  dt.Rows[i]["Name"].ToString(),
                                //  dt.Rows[i]["IPAddress"].ToString(),
                                //  dt.Rows[i]["Port"].ToString(),
                                //  Convert.ToInt32(dt.Rows[i]["Priority"].ToString()),
                                //  validity
                                //);
                                break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    }
                    //else
                    //{
                    //    break;
                    //}
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
          


        }
        public static int checkingvalidity(string num)
        {
            int number = Convert.ToInt32(num);
            //Convert.ToInt32(num);
            int i = 1, j = 2;
            int temp1 = number & (1 << (i - 1));
            int temp2 = number & (1 << (j - 1));
            if ((temp1 > 0) && (temp2 > 0))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        public void StartMission()
        {
            count = 0;
            MissionStatus = "Started";
        }
        public async void EndMission()
        {
            count = 0;
            MissionStatus = "Ended";
            await MySqlBulCopyAsync();
        }
        public async Task<bool> MySqlBulCopyAsync()
        {
            List<DataTable> tables = new List<DataTable>();
            bool result = false;
            if (MissionStatus == "Ended")
            {
                int dtcount = dt.Rows.Count / 5;
                //tables=SplitTable(dt,5);
                // var table = dt.AsEnumerable().ToChunks(225).Select(rows => rows.CopyToDataTable())
                var table = dt.AsEnumerable().ToChunks(1000)
                         .Select(rows => rows.ToList());
                //for (int i = 0; i < table.Count(); i++)
                //{

                // MySqlConnector.MySqlConnection con = new MySqlConnector.MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=Required;Connect Timeout=30;AllowLoadLocalInfile=true");
                MySqlConnector.MySqlConnection con = new MySqlConnector.MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=Required;Connect Timeout=30;AllowLoadLocalInfile=true");
                try
                {
                    await con.OpenAsync();
                    foreach (var data in table)
                    {
                        DataTable datatable = new DataTable();
                        datatable = data.CopyToDataTable<DataRow>();
                        result = true;
                        //using (var connection = new MySqlConnector.MySqlConnection(con))
                        //{

                        var bulkCopy = new MySqlConnector.MySqlBulkCopy(con);
                        bulkCopy.DestinationTableName = "equipmentparameterdata";
                        // the column mapping is required if you have a identity column in the table
                        bulkCopy.ColumnMappings.AddRange(GetMySqlColumnMapping(datatable));
                        await bulkCopy.WriteToServerAsync(datatable);
                        int temp = dt.Rows.Count;

                        //System.Diagnostics.Debug.WriteLine("BulkCopy Rows Inserted ");
                    }
                    dt.Rows.Clear();
                }
                catch (Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine("BulkCopy Exception " + ex.Message.ToString());
                    throw;
                }
                //}
            }
            return result;

        }
        private List<MySqlConnector.MySqlBulkCopyColumnMapping> GetMySqlColumnMapping(DataTable dataTable)
        {
            List<MySqlConnector.MySqlBulkCopyColumnMapping> colMappings = new List<MySqlConnector.MySqlBulkCopyColumnMapping>();
            int i = 0;
            foreach (DataColumn col in dataTable.Columns)
            {
                colMappings.Add(new MySqlConnector.MySqlBulkCopyColumnMapping(i, col.ColumnName));
                i++;
            }
            return colMappings;
        }
        public void MakeSQLConnection()
        {
            if (con == null)
            {
                con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
                con.Open();
            }
        }

        public void SQLDisConnect()
        {
            con.Close();
        }

        
    }
}
