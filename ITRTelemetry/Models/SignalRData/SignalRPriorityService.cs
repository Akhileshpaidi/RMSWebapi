using DomainModel;
using ITR_TelementaryAPI.Models;
using ITR_TelementaryAPI.UDP;
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
    public class SignalRPriorityService
    {
        private IHubContext<SignalRPriorityHub> hubContext { get; set; }

        private readonly object updateStockPricesLock = new object();

        private GetPriorityMapping[] tickData;
        private int lastTickIndex;
        private Timer timer;
        int MissionID ;
        bool validitycheck;
        PriorityQueue<TelemetryStation> pq = new PriorityQueue<TelemetryStation>();
        public SignalRPriorityService(IHubContext<SignalRPriorityHub> hubCtx)
        {
            hubContext = hubCtx;
        }
        public bool GetAllData(int EID,int MID)
        {
           
            MissionID = MID;
           // var data = new List<GetPriorityMapping>();
            Byte[] receiveBytes;
            try
                {
                    pq = new PriorityQueue<TelemetryStation>();
                   TestPriorityQueues();
                   if(pq.Count()>0)
                   {
                    // Blocks until a message returns on this socket from a remote host.
                    UdpClient receivingUdpClient = new UdpClient(8000);
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    System.Diagnostics.Debug.WriteLine("This is the message you received " +
                                           returnData.ToString());
                    System.Diagnostics.Debug.WriteLine("This message was sent from " +
                                                RemoteIpEndPoint.Address.ToString() +
                                                " on their port number " +
                                                RemoteIpEndPoint.Port.ToString());
                    validitycheck = DataParse(receiveBytes, 2);
                   
                    timer = new Timer(Update, null, 100, 100);
                }
               
            }
            catch (Exception e)
            {
               System.Diagnostics.Debug.WriteLine(e.ToString());
            }
             return validitycheck;
        }
        public void TestPriorityQueues()
        {
            //System.Diagnostics.Debug.WriteLine("\nBegin Priority Queue demo");      
            //System.Diagnostics.Debug.WriteLine("\nCreating priority queue of Telemetry Stations\n");
           
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT prioritymapping.PriorityMappingID,missiontable.EquipmentID,prioritymapping.MissionID,prioritymapping.TSID,telemetrystationtable.Name,prioritymapping.Priority FROM prioritymapping inner join missiontable on missiontable.MissionID = prioritymapping.MissionID inner join telemetrystationtable on telemetrystationtable.TSID = prioritymapping.TSID where missiontable.MissionID = '" + MissionID + "' order by Priority ASC", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            int arrsize = dt.Rows.Count;
            var data = new TelemetryStation[arrsize];
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    int tsid = Convert.ToInt32(dt.Rows[i]["TSID"].ToString());
                    var name = dt.Rows[i]["Name"].ToString();
                    data[i] = new TelemetryStation
                    (
                       tsid,
                       name
                    );
                }

            }
            for (int i = 1; i < data.Length; i++)
            {
                // System.Diagnostics.Debug.WriteLine("Adding " + data[i].Name.ToString() + " to priority queue");
                pq.Enqueue(data[i]);
            }

            System.Diagnostics.Debug.WriteLine("\nPriory queue is: ");
            System.Diagnostics.Debug.WriteLine(pq.ToString());

        }
        public static void TestPriorityQueue(int numOperations)
        {
            Random rand = new Random(0);
            PriorityQueue<TelemetryStation> pq = new PriorityQueue<TelemetryStation>();
            for (int op = 0; op < numOperations; ++op)
            {
                int opType = rand.Next(0, 2);

                if (opType == 0) // enqueue
                {
                    string Name = op + "man";
                    int TSID = (100 - 1) * rand.Next() + 1;
                    pq.Enqueue(new TelemetryStation(TSID, Name));
                    if (pq.IsConsistent() == false)
                    {
                        System.Diagnostics.Debug.WriteLine("Test fails after enqueue operation # " + op);
                    }
                }
                else // dequeue
                {
                    if (pq.Count() > 0)
                    {
                        TelemetryStation e = pq.Dequeue();
                        if (pq.IsConsistent() == false)
                        {
                            System.Diagnostics.Debug.WriteLine("Test fails after dequeue operation # " + op);
                        }
                    }
                }
            } // for
            System.Diagnostics.Debug.WriteLine("\nAll tests passed");
        } // TestPriorityQueue

    
    private void Update(object state)
        {
            int validitybit;
            lock (updateStockPricesLock)
            {
                if (validitycheck == true)
                {
                    validitybit = 1;
                }
                else
                {
                    validitybit = 0;
                    if (pq.Count() > 0)
                    {
                        TelemetryStation e = pq.Dequeue();
                         System.Diagnostics.Debug.WriteLine("Deque" + e);
                        
                    }
                }
                BroadcastStockPrice(validitybit);
            }
        }

        private void BroadcastStockPrice(int validity)
        {
            hubContext.Clients.All.SendAsync("updateParamData", validity,pq);
        }

        public bool DataParse(byte[] arr1, int Equip)
        {
            var packetid = 0;
            int EquipmentID = Equip;
            PacketModel packet = new PacketModel();
            packet.EquipmentID = 2;
            EquipmentParameterDetailModel det = new EquipmentParameterDetailModel();
            var temp = Get(EquipmentID).ToArray();
            var packettemp = GetPacketDetails(EquipmentID).ToArray();
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
                    int packetvalidity = 0;
                    if (s == "ID")
                    {
                        //  string validbit=ByteArrayToString(paramByteArray);
                        string validbit = paramByteArray[0].ToString();
                        //packetvalidity = checked1(validbit);
                        System.Diagnostics.Debug.WriteLine("ID: " + validbit);
                        packetvalidity = checkingvalidity(validbit);
                        System.Diagnostics.Debug.WriteLine("Validity bit: " + packetvalidity);

                    }
                    if (packetvalidity <= 0)
                    {
                        return false;
                    }
                    string DataType = s3;
                    var holdvalue = "";
                    switch (DataType)
                    {
                        case "Unsigned char":
                            holdvalue = paramByteArray[0].ToString();
                            System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);

                            //System.Diagnostics.Debug.WriteLine("Row Inserted");
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
                            // System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            // System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);

                            break;

                        case "Short":
                            short shortValueBack = BitConverter.ToInt16(paramByteArray, 0);
                            holdvalue = shortValueBack.ToString();
                            // System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            //System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);

                            break;

                        case "Bool":
                            bool boolValueBack = BitConverter.ToBoolean(paramByteArray, 0);
                            holdvalue = boolValueBack.ToString();
                            // System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            // System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);

                            break;

                        case "Char":
                            // getting char value and Display it 
                            char charValueBack = BitConverter.ToChar(paramByteArray, 0);
                            holdvalue = charValueBack.ToString();
                            //System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            // System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);

                            break;

                        case "BCD":
                            // getting char value and Display it 
                            string bcdValueBack = ConvertToBinaryCodedDecimal(true, paramByteArray);
                            holdvalue = bcdValueBack.ToString();
                            // System.Diagnostics.Debug.WriteLine("ParameterID: " + s);
                            // System.Diagnostics.Debug.WriteLine("ParamData: " + holdvalue);

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

                packet.TransmissionDate = Convert.ToString(DateTime.Now);
                //  packetid = SavePacketData(packet); 
            }
            if (temp.Length > 0)
            {
                if (arr1.Length > 0)
                {
                    //..............First given format...............................................
                    //string hhbcd = ConvertToBinaryCodedDecimal(true, arr1[2]);
                    //string mmbcd = ConvertToBinaryCodedDecimal(true, arr1[3]);
                    //string ssbcd = ConvertToBinaryCodedDecimal(true, arr1[4]);
                    //string msbcd = ConvertToBinaryCodedDecimal(true, arr1[5]);
                    //string lockstatus = arr1[6].ToString();
                    //// string dont =arr1[7].ToString();
                    //byte[] plist = new byte[2];
                    //int p = 0;
                    //for (int i = 8; i <= 9; i++)
                    //{
                    //    plist[p] = arr1[i];
                    //    p++;
                    //}
                    //short paramlistsize = BitConverter.ToInt16(plist, 0);
                    //string timeval = hhbcd + ":" + mmbcd + ":" + ssbcd + ":" + msbcd;
                    //System.Diagnostics.Debug.WriteLine("Time: " + timeval);
                    //System.Diagnostics.Debug.WriteLine("LockStatus: " + lockstatus);
                    //System.Diagnostics.Debug.WriteLine("ParameterListSize: " + paramlistsize);

                    //................Second given format............................................
                    //string hhbcd = arr1[0].ToString();
                    //string mmbcd = arr1[1].ToString();
                    //string ssbcd = arr1[2].ToString();
                    //string msbcd = arr1[3].ToString();
                    //string lockstatus = arr1[4].ToString();
                    //string id = arr1[5].ToString();
                    //byte[] plist = new byte[2];
                    //int p = 0;
                    //for (int i = 6; i <= 7; i++)
                    //{
                    //    plist[p] = arr1[i];
                    //    p++;
                    //}
                    //short paramlistsize = BitConverter.ToInt16(plist, 0);
                    //string timeval = hhbcd + ":" + mmbcd + ":" + ssbcd + ":" + msbcd;
                    //System.Diagnostics.Debug.WriteLine("Time: " + timeval);
                    //System.Diagnostics.Debug.WriteLine("LockStatus: " + lockstatus);
                    //System.Diagnostics.Debug.WriteLine("Id: " + id);
                    //System.Diagnostics.Debug.WriteLine("ParameterListSize: " + paramlistsize);                   

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
                        det.UDPPacketSequenceNo = "hdhsh";
                        // int ID = GetPacketID();
                        det.UPDPacketID = packet.PacketID;
                        det.PacketMasterID = packetid;
                        det.FlightID = 1;
                        det.Date = DateTime.Now;

                        // var equipmentparameterdetails = this.mySqlDBContext.EquipmentParameterDetailModels;                      
                        switch (DataType)
                        {
                            case "Unsigned char":
                                det.ParamData = paramByteArray[0].ToString();
                                // System.Diagnostics.Debug.WriteLine("ParameterID: " + det.ParameterID);
                                // System.Diagnostics.Debug.WriteLine("ParamData: " + det.ParamData);
                                //SaveParametersData(det);
                                //System.Diagnostics.Debug.WriteLine("Row Inserted");
                                break;
                            case "Float":

                                float valueBack = BitConverter.ToSingle(paramByteArray, 0);
                                det.ParamData = valueBack.ToString();
                                // System.Diagnostics.Debug.WriteLine("ParameterID: " + det.ParameterID);
                                // System.Diagnostics.Debug.WriteLine("ParamData: " + det.ParamData);
                                // SaveParametersData(det);
                                // System.Diagnostics.Debug.WriteLine("Row Inserted");
                                break;
                            case "Long":

                                long longValueBack = BitConverter.ToInt64(paramByteArray, 0);
                                det.ParamData = longValueBack.ToString();
                                // System.Diagnostics.Debug.WriteLine("ParameterID: " + det.ParameterID);
                                // System.Diagnostics.Debug.WriteLine("ParamData: " + det.ParamData);
                                // SaveParametersData(det);
                                // System.Diagnostics.Debug.WriteLine("Row Inserted");
                                break;
                            case "Short":

                                short shortValueBack = BitConverter.ToInt16(paramByteArray, 0);
                                det.ParamData = shortValueBack.ToString();
                                //System.Diagnostics.Debug.WriteLine("ParameterID: " + det.ParameterID);
                                // System.Diagnostics.Debug.WriteLine("ParamData: " + det.ParamData);
                                //SaveParametersData(det);
                                // System.Diagnostics.Debug.WriteLine("Row Inserted");
                                break;
                            case "Bool":

                                bool boolValueBack = BitConverter.ToBoolean(paramByteArray, 0);
                                det.ParamData = boolValueBack.ToString();
                                // System.Diagnostics.Debug.WriteLine("ParameterID: " + det.ParameterID);
                                //System.Diagnostics.Debug.WriteLine("ParamData: " + det.ParamData);
                                //  SaveParametersData(det);
                                //System.Diagnostics.Debug.WriteLine("Row Inserted");
                                break;
                            case "Char":
                                // getting char value and Display it 
                                char charValueBack = BitConverter.ToChar(paramByteArray, 0);
                                det.ParamData = charValueBack.ToString();
                                //System.Diagnostics.Debug.WriteLine("ParameterID: " + det.ParameterID);
                                // System.Diagnostics.Debug.WriteLine("ParamData: " + det.ParamData);
                                //SaveParametersData(det);
                                // System.Diagnostics.Debug.WriteLine("Row Inserted");
                                break;
                            case "BCD":
                                // getting char value and Display it 
                                string bcdValueBack = ConvertToBinaryCodedDecimal(true, paramByteArray);
                                det.ParamData = bcdValueBack.ToString();
                                // System.Diagnostics.Debug.WriteLine("ParameterID: " + det.ParameterID);
                                // System.Diagnostics.Debug.WriteLine("ParamData: " + det.ParamData);
                                // SaveParametersData(det);
                                //System.Diagnostics.Debug.WriteLine("Row Inserted");
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
        public static List<EquipmentParameterModel> Get(int EquipmentID)
        {
            //int EquipmentID = 1;
            EquipmentParameterDetailModel det = new EquipmentParameterDetailModel();
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterID, ParameterName, ParameterTypeID, StartByte, EndByte, DataType FROM equipmentparametermaster where EquipmentID = '" + EquipmentID + "'", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<EquipmentParameterModel>();
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var ParameterID = dt.Rows[i]["ParameterID"].ToString();
                var ParameterName = dt.Rows[i]["ParameterName"].ToString();
                var ParameterTypeID = dt.Rows[i]["ParameterTypeID"].ToString();
                var StartByte = dt.Rows[i]["StartByte"].ToString();
                var EndByte = dt.Rows[i]["EndByte"].ToString();
                var DataType = dt.Rows[i]["DataType"].ToString();
                pdata.Add(new EquipmentParameterModel { ParameterID = Convert.ToInt32(ParameterID), ParameterName = ParameterName, ParameterTypeID = Convert.ToInt32(ParameterTypeID), StartByte = StartByte, EndByte = EndByte, DataType = DataType });
            }
            return pdata;
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

    }
}
