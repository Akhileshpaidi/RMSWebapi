using DomainModel;
using ITR_TelementaryAPI.Hubs;
using ITR_TelementaryAPI.Models.SignalRTickData;
using ITRTelemetry.Hubs;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;

namespace ITRTelemetry.Models.SignalRData
{
    public class OfflineSignalRDataService
    {
        private IHubContext<OfflineSignalRDataHub> hubContext { get; set; }
        private readonly object updateStockPricesLock = new object();
        private PlayBackParametersData[] tickData;
        private int lastTickIndex;
        private Timer timer;
        int count = 0;
        int SystemID, FlightID, MissionID, Frequency, Year,ParameterID,MINVal,MAXVal;
        string MissionStatus;
        MySqlConnection con;
        DataTable dt = new DataTable();
        DataTable dtp = new DataTable();
        // DataTable dtpvalues = new DataTable();

        private int dtcount = 0;
        public OfflineSignalRDataService(IHubContext<OfflineSignalRDataHub> hubCtx)
        {

            hubContext = hubCtx;
            dt.Columns.Add("ParameterID");
            dt.Columns.Add("ParamData");
            dt.Columns.Add("FlightID");
            dt.Columns.Add("Status");
            dt.Columns.Add("Count");
            dt.Columns.Add("Date");
            getOfflineDisplayModeDetails();
            tickData = GenerateTestData();
            lastTickIndex = tickData.Length - 1;
            timer = new Timer(Update, null, Frequency, Frequency);
        }
        public IEnumerable<PlayBackParametersData> GetAllData()
        {
            getOfflineDisplayModeDetails();
            tickData = GenerateTestData();
            lastTickIndex = tickData.Length - 1;
            var data = new List<PlayBackParametersData>();
           
                for (var i = lastTickIndex; data.Count < 10000; i--)
                {
                    data.Add(tickData[i]);
                    if (i == 0)
                    {
                        i = tickData.Length - 1;
                    }
                }
            

            return data;
        }

        private void Update(object state)
        {
           
                lastTickIndex = (lastTickIndex + 1) % tickData.Length;
                var tick = tickData[lastTickIndex];
                BroadcastStockPrice(tick);
           
        }

        private void BroadcastStockPrice(PlayBackParametersData item)
        {
            hubContext.Clients.All.SendAsync("updateParamData", item);
        }

        private PlayBackParametersData[] GenerateTestData()
        {
            DataTable dtp = new DataTable();
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select equipmentparameterdata.ParameterID,equipmentparametermaster.ParameterName,equipmentparameterdata.ParamData,equipmentparameterdata.Count,parametertypemaster.ParameterTypeID,parametertypemaster.ParameterType  from itr.equipmentparameterdata inner join itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID inner join itr.parametertypemaster on parametertypemaster.ParameterTypeID=equipmentparametermaster.ParameterTypeID where equipmentparametermaster.EquipmentID='" + SystemID + "' and equipmentparameterdata.FlightID='" + FlightID + "' and equipmentparameterdata.ParameterID='" + ParameterID + "'  and equipmentparameterdata.Count between '" + MINVal + "' and '" + MAXVal + "'", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();

            var pdata = new PlayBackParametersData[dtp.Rows.Count];
            try
            {
                for (int i = 0; i < dtp.Rows.Count; i++)
                {
                    pdata[i] = new PlayBackParametersData(
                        Convert.ToInt32(dtp.Rows[i]["ParameterID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterName"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParamData"].ToString()),
                        Convert.ToInt32(dtp.Rows[i]["ParameterTypeID"].ToString()),
                        Convert.ToString(dtp.Rows[i]["ParameterType"].ToString()),
                         Convert.ToInt32(dtp.Rows[i]["Count"].ToString())
                    );
                }
                
            }

            catch (Exception ex)
            {

            }
            return pdata;
        }
      public void StartMission()
        {
            count = 0;
            MissionStatus = "Started";
        }
        public void EndMission()
        {
            count = 0;
            MissionStatus = "Ended";
           
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
        public void getOfflineDisplayModeDetails()
        {
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=required");
            try
            {
                con.Open();
                MySqlCommand cmd1 = new MySqlCommand("SELECT * FROM itr.playbackmodeoffline ORDER BY PlaybackmodeofflineID DESC LIMIT 1", con);
                cmd1.CommandType = CommandType.Text;

                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);

                DataTable dt1 = new DataTable();
                da1.Fill(dt1);
                if (dt1.Rows.Count > 0)
                {
                    SystemID = Convert.ToInt32(dt1.Rows[0]["SystemID"].ToString());
                    MissionID = Convert.ToInt32(dt1.Rows[0]["MissionID"].ToString());
                    FlightID = Convert.ToInt32(dt1.Rows[0]["FlightID"].ToString());
                    Frequency = Convert.ToInt32(dt1.Rows[0]["Frequency"].ToString());
                    Year = Convert.ToInt32(dt1.Rows[0]["Year"].ToString());
                    ParameterID = Convert.ToInt32(dt1.Rows[0]["ParameterID"].ToString());
                    MINVal = Convert.ToInt32(dt1.Rows[0]["MinimumVal"].ToString());
                    MAXVal = Convert.ToInt32(dt1.Rows[0]["MaximumVal"].ToString());

                }
            }
            catch(Exception ex)
            {
                con.Close();
            }
            finally
            {
                con.Close();
            }
        }
    }
}
