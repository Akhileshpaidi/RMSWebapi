
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using ITR_TelementaryAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;

namespace ITR_TelementaryAPI.Models.SignalR {
    public class ParameterDataService {
        private readonly IEnumerable<ParametersModel> _params;
        private IHubContext<LiveUpdateSignalRHub> _hubContext { get; set; }

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(100);
        private readonly Random _updateOrNotRandom = new Random();

        private readonly Timer _timer;

        private readonly object _updatePricesLock = new object();

        static readonly Random random = new Random();

        public ParameterDataService(IHubContext<LiveUpdateSignalRHub> hubContext) {
            _hubContext = hubContext;

            _params = GenerateParams();

            _timer = new Timer(UpdateParamData, null, _updateInterval, _updateInterval);

        }

        public IEnumerable<ParametersModel> GetParamData() {
            return _params;
        }

        static IEnumerable<ParametersModel> GenerateParams() {
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParamDataID,ParamData,FlightID,equipmentparameterdata.Date,UDPPacketSequenceNo,UPDPacketID,equipmentparameterdata.ParameterID,ParameterName,StartByte,EndByte,Remark,DataType,equipmentparametermaster.EquipmentID,EquipmentName,FrameRate,FrameLength,FrameDescription,equipmentparametermaster.DateTime,equipmentparametermaster.Status FROM itr.equipmentparameterdata inner join  itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID inner join itr.equipmentmaster on equipmentmaster.EquipmentID=equipmentparametermaster.EquipmentID", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            int arrsize = dt.Rows.Count;
            var _params = new ParametersModel[arrsize];
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    _params[i] = new ParametersModel(63.70M)
                    {
                        ParamDataID = Convert.ToInt32(dt.Rows[i]["ParamDataID"].ToString()),
                        ParamData = dt.Rows[i]["ParamData"].ToString(),
                        Date = Convert.ToDateTime(dt.Rows[i]["Date"].ToString()),
                        ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString()),
                        ParameterName = dt.Rows[i]["ParameterName"].ToString(),
                        EquipmentName = dt.Rows[i]["EquipmentName"].ToString(),
                        LastUpdate = DateTime.Now,

                        //Status = dt.Rows[i]["Status"].ToString(),
                        //Symbol = "GOOG",
                        //DayOpen = 55.9M,
                        //FlightID = Convert.ToInt32(dt.Rows[i]["FlightID"].ToString()),
                        //UDPPacketSequenceNo = dt.Rows[i]["UDPPacketSequenceNo"].ToString(),
                        //UPDPacketID = dt.Rows[i]["UPDPacketID"].ToString(),
                        //StartByte = dt.Rows[i]["StartByte"].ToString(),
                        //EndByte = dt.Rows[i]["EndByte"].ToString(),
                        //Remark = dt.Rows[i]["Remark"].ToString(),
                        //DataType = dt.Rows[i]["DataType"].ToString(),
                        //EquipmentID = Convert.ToInt32(dt.Rows[i]["EquipmentID"].ToString()),
                       
                        //FrameRate = dt.Rows[i]["FrameRate"].ToString(),
                        //FrameLength = dt.Rows[i]["FrameLength"].ToString(),
                        //FrameDescription = dt.Rows[i]["FrameDescription"].ToString(),
                    };
                }
            }
            return _params;
        }

        static IEnumerable<ParametersModel> GenerateParamMaster()
        {
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ParameterID,ParameterName,ParameterTypeID FROM equipmentparametermaster", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            int arrsize = dt.Rows.Count;
            var _paramsmaster = new ParametersModel[arrsize];
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    _paramsmaster[i] = new ParametersModel(63.70M)
                    {
                       // ParamDataID = Convert.ToInt32(dt.Rows[i]["ParamDataID"].ToString()),
                      //  ParamData = dt.Rows[i]["ParamData"].ToString(),
                       // Date = Convert.ToDateTime(        dt.Rows[i]["Date"].ToString()),
                        ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString()),
                        ParameterName = dt.Rows[i]["ParameterName"].ToString(),
                        EquipmentName = dt.Rows[i]["ParameterTypeID"].ToString(),
                        LastUpdate = DateTime.Now,
                    };
                }
            }
            return _paramsmaster;
        }


        private void UpdateParamData(object state) {
            lock(_updatePricesLock) {
                foreach(var param in _params) {
                    if(TryUpdatePrice(param)) {
                        BroadcastPrice(param);
                    }
                }
            }
        }

        private bool TryUpdatePrice(ParametersModel paramdata) {
            var r = _updateOrNotRandom.NextDouble();
            if (r > .1)
            {
                return false;
            }
            paramdata.Update();
            return true;
        }

        private void BroadcastPrice(ParametersModel paramdata) {
            _hubContext.Clients.All.SendAsync("updateParamdata", paramdata);
        }
    }
}
