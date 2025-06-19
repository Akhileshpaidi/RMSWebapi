using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel
{
    public class LiveDataVisualizationModel
    {
        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParamData { get; set; }
        public int ParameterTypeID { get; set; }
        public string ParameterType { get; set; }
        public int FlightID { get; set; }

        public LiveDataVisualizationModel(int ParameterIDVal, string ParameterNameVal, string ParamDataVal, int ParameterTypeIDVal, string ParameterTypeVal, int FlightIDVal)
        {
            ParameterID = ParameterIDVal;
            ParameterName = ParameterNameVal;
            ParamData = ParamDataVal;
            ParameterTypeID = ParameterTypeIDVal;
            ParameterType = ParameterTypeVal;
            FlightID = FlightIDVal;

        }

        public static void SaveParametersData(LiveDataVisualizationModel det)
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

        //public static void SaveParametersData(StationParametersData stationParametersData)
        //{
        //    throw new NotImplementedException();
        //}

        public static void SaveParametersData(StationParametersData det, MySqlCommand myCommand)
        {
            try
            {
                //using ()
                //{
                myCommand.Parameters.AddWithValue("@ParamData", det.ParamData);
                myCommand.Parameters.AddWithValue("@FlightID", det.FlightID);
                myCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                myCommand.Parameters.AddWithValue("@UDPPacketSequenceNo", "123");
                myCommand.Parameters.AddWithValue("@UPDPacketID", "001");
                myCommand.Parameters.AddWithValue("@ParameterID", det.ParameterID);
                myCommand.Parameters.AddWithValue("@PacketMasterID", "1");
                myCommand.Parameters.AddWithValue("@Status", "");

                myCommand.ExecuteNonQuery();

                //  }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("error " + ex.Message);

            }

        }
    }

    public class StationParametersData
    {
        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParamData { get; set; }
        public int ParameterTypeID { get; set; }
        public string ParameterType { get; set; }
        public int FlightID { get; set; }
        public int TSID { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }

        public int Priority { get; set; }

        public int Validity { get; set; }

        public StationParametersData(int ParameterIDVal, string ParameterNameVal, string ParamDataVal, int ParameterTypeIDVal, string ParameterTypeVal, int FlightIDVal, int TSIDVal, string NameVal, string IPVal, string PortVal, int PriorityVal, int validbit)
        {
            ParameterID = ParameterIDVal;
            ParameterName = ParameterNameVal;
            ParamData = ParamDataVal;
            ParameterTypeID = ParameterTypeIDVal;
            ParameterType = ParameterTypeVal;
            FlightID = FlightIDVal;
            TSID = TSIDVal;
            Name = NameVal;
            IPAddress = IPVal;
            Port = PortVal;
            Priority = PriorityVal;
            Validity = validbit;

        }
    }

    public class PlayBackParametersData
    {
        public int ParameterID { get; set; }
        public string ParameterName { get; set; }
        public string ParamData { get; set; }
        public int ParameterTypeID { get; set; }
        public string ParameterType { get; set; }
       
        public int Count { get; set; }

        public PlayBackParametersData(int ParameterIDVal, string ParameterNameVal, string ParamDataVal, int ParameterTypeIDVal, string ParameterTypeVal, int count)
        {
            ParameterID = ParameterIDVal;
            ParameterName = ParameterNameVal;
            ParamData = ParamDataVal;
            ParameterTypeID = ParameterTypeIDVal;
            ParameterType = ParameterTypeVal;
            Count = count;

        }
    }
}
