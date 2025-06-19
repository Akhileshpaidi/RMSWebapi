using DomainModel;
using ITRTelemetry.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ITRTelemetry.DataStorage
{
    public class  DataManager
    {
        public static List<ChartModel> GetData()
        {

            ////    var r = new Random();
            ////    MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            ////    con.Open();
            ////    MySqlCommand cmd = new MySqlCommand("SELECT equipmentparameterdata.ParameterID,ParamData,ParamDataID,ParameterName,ParameterTypeID,DataType,Date FROM itr.equipmentparameterdata inner join  itr.equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID", con);

            ////    cmd.CommandType = CommandType.Text;

            ////    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            ////    DataTable dt = new DataTable();
            ////    da.Fill(dt);

            ////    var pdata = new List<ChartModel>();
            ////    if (dt.Rows.Count > 0)
            ////    {
            ////        for (var i = 0; i < dt.Rows.Count; i++)
            ////        {
            ////            var label = dt.Rows[i]["ParameterTypeID"].ToString();
            ////            var data = dt.Rows[i]["ParamData"].ToString();

            ////            // new ChartModel { Data = new List<decimal> { Convert.ToDecimal(data) }, Label = label }; //here we are getting outofbound exception
            ////            pdata.Add(new ChartModel { Data = new List<decimal> { Convert.ToDecimal(data) }, Label = label });
            ////        }
            ////    }
            ////    con.Close();
            ////    return pdata;
            ////}
            var r = new Random();
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select PacketID,equipmentparameterdata.ParameterID,ParamData,ParameterName,FlightID,Hours,Minutes,Seconds,MilliSeconds from equipmentparameterdata inner join equipmentparametermaster on equipmentparametermaster.ParameterID = equipmentparameterdata.ParameterID inner join packetmaster on packetmaster.PacketMasterID = equipmentparameterdata.PacketMasterID", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ChartModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < 10; i++)
                {
                    var label = dt.Rows[i]["Hours"].ToString() + ":" + dt.Rows[i]["Minutes"].ToString() + ":" + dt.Rows[i]["Seconds"].ToString();
                    var data = dt.Rows[i]["ParamData"].ToString();
                    var parameterid = dt.Rows[i]["ParameterID"].ToString();

                    // new ChartModel { Data = new List<decimal> { Convert.ToDecimal(data) }, Label = label }; //here we are getting outofbound exception
                    pdata.Add(new ChartModel { Data = new List<decimal> { Convert.ToDecimal(data) }, Label = label, ParameterID = parameterid });
                }
            }
            return pdata;
        }

        public static List<DemoParameters> GetDemoData()
        {

            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itrdemo;sslmode=none;");
            con.Open();
           // MySqlCommand cmd = new MySqlCommand("SELECT equipmentparameterdata.ParameterDataID,equipmentparameterdata.ParameterID,equipmentparameter.ParameterName,equipmentparameter.ParamType,equipmentparameterdata.ParamValue FROM itrdemo.equipmentparameterdata inner join equipmentparameter on equipmentparameter.ParameterID=equipmentparameterdata.ParameterID;", con);
            MySqlCommand cmd = new MySqlCommand("select T.ParameterID,T.ParamValue from (select T.ParameterID,T.ParamValue,row_number() over(partition by T.ParameterDataID) as rn from itrdemo.equipmentparameterdata as T) as T where T.rn <= 1000", con);
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DemoParameters>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DemoParameters
                    {
                        //ParameterDataID = Convert.ToInt32(dt.Rows[i]["ParameterDataID"]),
                        ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"]),
                        // ParameterName = dt.Rows[i]["ParameterName"].ToString(),
                        ParamValue = dt.Rows[i]["ParamValue"].ToString(),
                        // ParamType = dt.Rows[i]["ParamType"].ToString(),
                    });
                }
            }
            return pdata;

        }

        //public static List<PlotingData> GetDemoPacketData()
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
        //    var pdata = new List<PlotingData>();
        //    if (dset.Tables.Count > 10)
        //    {
        //        for (int i = 0; i < dset.Tables[0].Rows.Count; i++)
        //        {
        //            string val = dset.Tables[0].Rows[i]["ParamValue"].ToString();

        //            pdata.Add(new PlotingData
        //            {
        //                PacketID = i,
        //                X1 = dset.Tables[0].Rows[i]["ParamValue"].ToString(),
        //                Y1 = dset.Tables[1].Rows[i]["ParamValue"].ToString(),
        //                X2 = dset.Tables[2].Rows[i]["ParamValue"].ToString(),
        //                Y2 = dset.Tables[3].Rows[i]["ParamValue"].ToString(),
        //                X3 = dset.Tables[4].Rows[i]["ParamValue"].ToString(),
        //                Y3 = dset.Tables[5].Rows[i]["ParamValue"].ToString(),
        //                X4 = dset.Tables[6].Rows[i]["ParamValue"].ToString(),
        //                Y4 = dset.Tables[7].Rows[i]["ParamValue"].ToString(),
        //                X5 = dset.Tables[8].Rows[i]["ParamValue"].ToString(),
        //                Y5 = dset.Tables[9].Rows[i]["ParamValue"].ToString(),

        //            }); ;

        //        }
        //    }
        //    return pdata;

        //}



    }

}
