using ITR_TelementaryAPI.Models.SignalRTickData;
using ITRTelemetry.Hubs;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ITRTelemetry.Models.SignalRData
{
    public class SignalRParamsGroupService
    {
        private IHubContext<SignalRParamsGroupHub> hubContext { get; set; }

        private readonly object updateStockPricesLock = new object();

        private ParameterData[] tickData;
        private int lastTickIndex;
        private Timer timer;
        private int ParameterTypeID;
        public SignalRParamsGroupService(IHubContext<SignalRParamsGroupHub> hubCtx)
        {
            hubContext = hubCtx;
        }

        public IEnumerable<ParameterData> GetAllData(int ParamTypeID)
        {
            ParameterTypeID = ParamTypeID;
            tickData = GenerateTestData();
            lastTickIndex = tickData.Length - 1;
            timer = new Timer(Update, null, 100, 100);
            var data = new List<ParameterData>();

            lock (updateStockPricesLock)
            {
                for (var i = lastTickIndex; data.Count < 100; i--)
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
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT equipmentmaster.EquipmentName,equipmentparameterdata.ParameterID,ParamData,ParamDataID,ParameterName,ParameterTypeID,DataType,Date FROM itr.equipmentparameterdata inner join  itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID inner join itr.equipmentmaster on equipmentmaster.EquipmentID=equipmentparametermaster.EquipmentID where equipmentparametermaster.ParameterTypeID='" + ParameterTypeID + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var lastPrice = 140m;
            int arrsize = dt.Rows.Count;
            var data = new ParameterData[arrsize];
            var curTime = DateTime.Now;
            var random = new Random();
            var slowRandomValue = random.NextDouble();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    lastPrice = Convert.ToDecimal(dt.Rows[i]["ParamData"].ToString());
                    var volume = (int)(100 + 1900 * random.NextDouble() * slowRandomValue);
                    var date = Convert.ToDateTime(dt.Rows[i]["Date"].ToString());

                    // var curTime = Convert.ToDateTime(dt.Rows[i]["Date"].ToString()); ;
                    //data[data.Length - 1 - i] = new TickItem(lastPrice, volume, curTime.AddSeconds(-1 * i));
                    // data[i] = new TickItem(lastPrice, volume, curTime.AddSeconds(-1 * (data.Length - 1 - i)));
                    data[i] = new ParameterData
                    (
                       lastPrice,
                       volume,
                       curTime.AddSeconds(-1 * (data.Length - 1 - i)),
                       dt.Rows[i]["ParameterName"].ToString(),
                       dt.Rows[i]["ParamData"].ToString(),
                       Convert.ToDateTime(dt.Rows[i]["Date"].ToString()),
                       dt.Rows[i]["ParameterTypeID"].ToString(),
                       dt.Rows[i]["ParameterID"].ToString(),
                       dt.Rows[i]["EquipmentName"].ToString()
                    );
                }

            }
            return data;
        }
    }
}