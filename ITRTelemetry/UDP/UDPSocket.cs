using DomainModel;
using ITR_TelementaryAPI.Models;
using ITR_TelementaryAPI.Models.SignalRTickData;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ITR_TelementaryAPI.UDP
{
    public class UDPSocket
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 1 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public bool Server(string address, int port,int EquipmentID, int TID)
        
        {
            bool validity = false;          
            try {
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
                validity = Receive(EquipmentID, TID);
               
            }
            catch(Exception e)
            {
                //validity = Receive(EquipmentID, TID);
            }
            finally
            {
              
            }
            return validity;

        }

       
      
        public bool Client(string address, int port,int EquipmentID, int TID)
        {
            bool validity = false;
            _socket.Connect(IPAddress.Parse(address), port);
            validity = Receive(EquipmentID, TID);
            return validity;
        }

        public void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
                System.Diagnostics.Debug.WriteLine("SEND: {0}, {1}", bytes, text);
            }, state);
        }

        public bool Receive(int EquipmentID, int Tid)
        {
            bool packetvalidity = false;
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
              //  System.Diagnostics.Debug.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));
                byte[] bytesdata = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(so.buffer, 0, bytes));
                packetvalidity = DataParse(bytesdata, EquipmentID);    
                int station = Tid;
            }, state);
            return packetvalidity;
        }

       
        public static List<EquipmentParameterModel> Get(int EquipmentID)
        {
            //int EquipmentID = 1;
            EquipmentParameterDetailModel det = new EquipmentParameterDetailModel();
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterID, ParameterName, ParameterTypeID, StartByte, EndByte, DataType FROM equipmentparametermaster where EquipmentID = '"+EquipmentID+"'", con);
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
        

        public bool DataParse(byte[] arr1,int Equip)
        {
            var packetid = 0;
            int EquipmentID = Equip;
            PacketModel packet = new PacketModel();
            packet.EquipmentID = 2;
            EquipmentParameterDetailModel det = new EquipmentParameterDetailModel();
            var temp = Get(EquipmentID).ToArray();
            var packettemp = GetPacketDetails(EquipmentID).ToArray();
            if(packettemp.Length > 0)
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
                    int packetvalidity=0;
                    if (s == "ID")
                    {
                      //  string validbit=ByteArrayToString(paramByteArray);
                        string validbit = paramByteArray[0].ToString();
                        //packetvalidity = checked1(validbit);
                        System.Diagnostics.Debug.WriteLine("ID: " + validbit);
                        packetvalidity = checkingvalidity(validbit);
                        System.Diagnostics.Debug.WriteLine("Validity bit: "+ packetvalidity);

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

        //public static string HexStringToBinary(string hex)
        //{
        //    StringBuilder result = new StringBuilder();
        //    string num;
        //    foreach (char c in hex)
        //    {
        //        // This will crash for non-hex characters. You might want to handle that differently.
        //        result.Append(hexCharacterToBinary[char.ToLower(c)]);
        //    }
        //    num = result.ToString();
        //    if (num.Contains("[01]+") && !num.StartsWith("0"))
        //    {
        //        System.Diagnostics.Debug.WriteLine("Valid number :" + num);
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine("InValid number :" + num);
        //    }
        //    return result.ToString();
        //}
        //public static readonly Dictionary<char, string> hexCharacterToBinary = new Dictionary<char, string> {
        //                    { '0', "0000" },
        //                    { '1', "0001" },
        //                    { '2', "0010" },
        //                    { '3', "0011" },
        //                    { '4', "0100" },
        //                    { '5', "0101" },
        //                    { '6', "0110" },
        //                    { '7', "0111" },
        //                    { '8', "1000" },
        //                    { '9', "1001" },
        //                    { 'a', "1010" },
        //                    { 'b', "1011" },
        //                    { 'c', "1100" },
        //                    { 'd', "1101" },
        //                    { 'e', "1110" },
        //                    { 'f', "1111" }
        // };

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
       // public static string ByteArrayToString(byte[] ba)
       // {
       //     StringBuilder hex = new StringBuilder(ba.Length * 2);
       //     foreach (byte b in ba)
       //         hex.AppendFormat("{0:x2}", b);            
       //     return hex.ToString();
       // }

       // public static string hex2binary(string hexvalue)
       // {
       //     string binaryval = "";
       //     binaryval = Convert.ToString(Convert.ToInt32(hexvalue, 16), 2);
           
       //     return binaryval;
       // }

       // public  static int checked1(string number){
       //     int num = Convert.ToInt32(number);
       //     int j = 0, k;
       //     int count = 0;
       //         while(num>0) 
       //         { 
       //             j=num%10; 
 
       //             if((j!=0)||(j!=1)) 
       //             {
             
       //             System.Diagnostics.Debug.WriteLine("num is not binary");
       //             break;

       //             }
       //             num=num/10; 
 
       //             if(num==0) 
       //             {
                   
       //             System.Diagnostics.Debug.WriteLine("num is binary");
       //             count++;
       //             } 
       //        }
       //     return count;
       //}

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
        public static IEnumerable<GetPriorityMapping> GetPriorityMappingDetails(int MissionID)
        {
            
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT prioritymapping.PriorityMappingID,missiontable.EquipmentID,prioritymapping.MissionID,prioritymapping.TSID,telemetrystationtable.Name,prioritymapping.Priority FROM prioritymapping inner join missiontable on missiontable.MissionID = prioritymapping.MissionID inner join telemetrystationtable on telemetrystationtable.TSID = prioritymapping.TSID where missiontable.MissionID = '"+ MissionID + "' order by Priority ASC", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetPriorityMapping>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetPriorityMapping
                    {
                        PriorityMappingID = Convert.ToInt32(dt.Rows[i]["PriorityMappingID"].ToString()),
                        EquipmentID = Convert.ToInt32(dt.Rows[i]["EquipmentID"].ToString()),
                        MissionID = Convert.ToInt32(dt.Rows[i]["MissionID"].ToString()),
                        TSID = Convert.ToInt32(dt.Rows[i]["TSID"].ToString()),
                        Priority = Convert.ToInt32(dt.Rows[i]["Priority"].ToString()),
                        TelemetryName =dt.Rows[i]["Name"].ToString()

                    });
                }
            }
            return pdata;
        }
    }
}