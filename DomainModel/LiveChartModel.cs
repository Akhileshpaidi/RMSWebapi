using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DomainModel
{
    public class LiveChartModel
    {
        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParamData { get; set; }
        public int ParameterTypeID { get; set; }
        public string ParameterType { get; set; }
        public int FlightID { get; set; }

        public LiveChartModel(int ParameterIDVal, string ParameterNameVal, string ParamDataVal, int ParameterTypeIDVal, string ParameterTypeVal, int FlightIDVal)
        {
            ParameterID = ParameterIDVal;
            ParameterName = ParameterNameVal;
            ParamData = ParamDataVal;
            ParameterTypeID = ParameterTypeIDVal;
            ParameterType = ParameterTypeVal;
            FlightID = FlightIDVal;

        }
        public static void SaveParametersData(LiveChartModel det)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            string parameterdata = "INSERT INTO equipmentparameterdata(ParamData,FlightID,Date,UDPPacketSequenceNo,UPDPacketID,ParameterID,PacketMasterID,Status) VALUES(@ParamData,@FlightID,@Date,@UDPPacketSequenceNo,@UPDPacketID,@ParameterID,@PacketMasterID,@Status)";
            try
            {
                using (MySqlCommand myCommand = new MySqlCommand(parameterdata, con))
                {
                    myCommand.Parameters.AddWithValue("@ParamData", det.ParamData);
                    myCommand.Parameters.AddWithValue("@FlightID", det.FlightID);
                    myCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                    myCommand.Parameters.AddWithValue("@UDPPacketSequenceNo", "123");
                    myCommand.Parameters.AddWithValue("@UPDPacketID", "001");
                    myCommand.Parameters.AddWithValue("@ParameterID", det.ParameterID);
                    myCommand.Parameters.AddWithValue("@PacketMasterID", "1");
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

    }

    public class XYValues
    {
        public string X { get; set; }
        public string Y { get; set; }

        public XYValues(string ParameterIDXVal, string ParameterIDYVal)
        {
            X = ParameterIDXVal;
            Y = ParameterIDYVal;


        }
    }

    public class OfflineChartSignalRModel
    {
        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParamData { get; set; }
        public int ParameterTypeID { get; set; }
        public string ParameterType { get; set; }
        public int FlightID { get; set; }
        public DateTime Date { get; set; }

        public OfflineChartSignalRModel(int ParameterIDVal, string ParameterNameVal, string ParamDataVal, int ParameterTypeIDVal, string ParameterTypeVal, int FlightIDVal, DateTime date)
        {
            ParameterID = ParameterIDVal;
            ParameterName = ParameterNameVal;
            ParamData = ParamDataVal;
            ParameterTypeID = ParameterTypeIDVal;
            ParameterType = ParameterTypeVal;
            FlightID = FlightIDVal;
            Date = date;
        }


    }
}

