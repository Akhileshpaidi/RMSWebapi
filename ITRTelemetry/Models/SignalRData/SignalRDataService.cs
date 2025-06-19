using DomainModel;
using ITR_TelementaryAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;

namespace ITR_TelementaryAPI.Models.SignalRTickData
{
    public class SignalRDataService
    {
        private IHubContext<SignalRDataHub> hubContext { get; set; }

        private readonly object updateStockPricesLock = new object();

        private ParameterData[] tickData;
        private int lastTickIndex;
        private Timer timer;

        public SignalRDataService(IHubContext<SignalRDataHub> hubCtx)
        {
            hubContext = hubCtx;
            tickData = GenerateTestData();
            lastTickIndex = tickData.Length - 1;
            timer = new Timer(Update, null, 10, 10);
        }

        public IEnumerable<ParameterData> GetAllData()
        {
            var data = new List<ParameterData>();
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
            lock (updateStockPricesLock)
            {
                lastTickIndex = (lastTickIndex + 1) % tickData.Length;
                var tick = tickData[lastTickIndex];
                tick.Date = DateTime.Now;

                BroadcastStockPrice(tick);
            }
        }

        private void BroadcastStockPrice(ParameterData item)
        {
            hubContext.Clients.All.SendAsync("updateParamData", item);
        }

        private ParameterData[] GenerateTestData()
        {
            EquipmentParameterDetailModel det = new EquipmentParameterDetailModel();
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterID, ParameterName, ParameterTypeID, StartByte, EndByte, DataType FROM equipmentparametermaster where EquipmentID = 1 and ParameterTypeID IS  NOT NULL", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
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
            int arrsize = dt.Rows.Count;
            var data = new ParameterData[arrsize];

            var temp = pdata.ToArray();
            if (temp.Length > 0)
            {
                string fileName = @"E:\sharanya\ITR-DATA_API\itrtelemetrty-socketapi\ITRTelemetry\BinaryData\outputFile.bin";
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
                        var lastPrice = 140m;
                        var curTime = DateTime.Now;
                        var random = new Random();
                        var slowRandomValue = random.NextDouble();

                        switch (DataType)
                        {
                            case "Unsigned char":
                                // The equivalent of unsigned char in C# is byte.
                                //System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}, {3}", s1, s2, s3, paramByteArray[0]);
                                det.ParamData = paramByteArray[0].ToString();

                                break;
                            case "Float":
                                //4 bytes size
                                // getting float value and Display it 
                                float valueBack = BitConverter.ToSingle(paramByteArray, 0);
                                //System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}, {3}", s1, s2, s3, valueBack);
                                det.ParamData = valueBack.ToString();

                                break;
                            case "Long":
                                //8 bytes size
                                // getting long value and Display it 
                                long longValueBack = BitConverter.ToInt64(paramByteArray, 0);
                                // System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}, {3}", s1, s2, s3, longValueBack);
                                det.ParamData = longValueBack.ToString();

                                break;
                            case "Short":
                                // getting short value and Display it 
                                short shortValueBack = BitConverter.ToInt16(paramByteArray, 0);
                                det.ParamData = shortValueBack.ToString();

                                break;
                            case "Bool":
                                // getting bool value and Display it 
                                bool boolValueBack = BitConverter.ToBoolean(paramByteArray, 0);
                                det.ParamData = boolValueBack.ToString();

                                break;
                            case "Char":
                                // getting char value and Display it 
                                char charValueBack = BitConverter.ToChar(paramByteArray, 0);
                                det.ParamData = charValueBack.ToString();

                                break;
                        }
                        lastPrice = Convert.ToDecimal(det.ParamData);
                        var volume = (int)(100 + 1900 * random.NextDouble() * slowRandomValue);
                        var date = DateTime.Now;
                        var Equipment = "1";
                        data[k] = new ParameterData
                        (
                           lastPrice,
                           volume,
                           DateTime.Now,
                           temp[k].ParameterName,
                           det.ParamData,
                           DateTime.Now,
                           Convert.ToString(temp[k].ParameterTypeID),
                           Convert.ToString(temp[k].ParameterID),
                           Equipment
                        );

                    }
                }
            }
            return data;
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
    }
}




