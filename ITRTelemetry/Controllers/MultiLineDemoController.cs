using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using ITRTelemetry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySQLProvider;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class MultiLineDemoController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public MultiLineDemoController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }


        [Route("api/MultiLineDemo/AddValues")]
        [HttpGet]
        public void GetValues()
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itrdemo;sslmode=none;");
            try
                {
                 string fileName = @"E:\sharanya\ITR-DATA_API\itrtelemetrty-socketapi\ITRTelemetry\BinaryData\test_web_tx1y1x2y2x3y3x4y4x5y5.dat";                 
                 var temp = GetParameterData().ToArray();
                 if (temp.Length > 0)
                 {
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
                            DemoParameterMaster det = new DemoParameterMaster();
                            det.ParameterID= s;
                              var demoParameterMasters = this.mySqlDBContext.DemoParameterMasters;
                          //  DemoParameterMaster demoParameterMasters;
                            string paramlength = paramByteArray.Length.ToString();
                            string DataType = s3;                      

                            switch (DataType)
                            {
                                case "Float":
                                    //4 bytes size
                                    // getting float value and Display it 
                                    float valueBack = BitConverter.ToSingle(paramByteArray, 0);
                                    System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}, {3}", s1, s2, s3, valueBack);
                                    float[] value  = ConvertByteToFloat(paramByteArray);
                                    det.ParamValue = valueBack.ToString();
                                    // demoParameterMasters.Add(det);
                                    demoParameterMasters.Add(det);
                                    if (s== 0)
                                    {
                                    }
                                    else
                                    {

                                        string UpdateQuery = "update equipmentparameter set ParamValue=@ParamValue where ParameterID=@ParameterID";
                                        try
                                        {
                                            using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                                            {
                                                myCommand.Parameters.AddWithValue("@ParameterID",s );
                                                myCommand.Parameters.AddWithValue("@ParamValue", valueBack);
                                          
                                                con.Open();
                                                myCommand.ExecuteNonQuery();
                                                con.Close();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine("error " + ex.Message);

                                        }
                                        //this.mySqlDBContext.Attach(det);
                                        //this.mySqlDBContext.Entry(det).State = EntityState.Modified;
                                        //var entry = this.mySqlDBContext.Entry(det);

                                        //Type type = typeof(DemoParameterMaster);
                                        //PropertyInfo[] properties = type.GetProperties();
                                        //foreach (PropertyInfo property in properties)
                                        //{
                                        //    if (property.GetValue(det, null) == null || property.GetValue(det, null).Equals(0))
                                        //    {
                                        //        entry.Property(property.Name).IsModified = false;
                                        //    }
                                        //}

                                        //this.mySqlDBContext.SaveChanges();
                                    }
                                   
                                    break;                         
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static float[] ConvertByteToFloat(byte[] array)
        {
            float[] floatArr = new float[array.Length / 4];
            for (int i = 0; i < floatArr.Length; i++)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(array, i * 4, 4);
                }
                floatArr[i] = BitConverter.ToSingle(array, i * 4);
            }
            return floatArr;
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

        [Route("api/MultiLine/ParameterData")]
        [HttpGet]
        public IEnumerable<DemoParameterMaster> GetParameterData()
        {

            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itrdemo;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameter;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<DemoParameterMaster>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new DemoParameterMaster { ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"]), 
                        ParameterName = dt.Rows[i]["ParameterName"].ToString(),
                        DataType = dt.Rows[i]["DataType"].ToString(),
                        StartByte = dt.Rows[i]["StartByte"].ToString(),
                        EndByte = dt.Rows[i]["EndByte"].ToString(),
                        ParamType = dt.Rows[i]["ParamType"].ToString(),
                    });
                }
            }
            return pdata;

        }


        [Route("api/MultiLine/GetData")]
        [HttpGet]
        public IEnumerable<DemoParameters> GetData()
        {

            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itrdemo;sslmode=none;");
            con.Open();
            //MySqlCommand cmd = new MySqlCommand("SELECT equipmentparameterdata.ParameterDataID,equipmentparameterdata.ParameterID,equipmentparameter.ParameterName,equipmentparameter.ParamType,equipmentparameterdata.ParamValue FROM itrdemo.equipmentparameterdata inner join equipmentparameter on equipmentparameter.ParameterID=equipmentparameterdata.ParameterID;", con);
            MySqlCommand cmd = new MySqlCommand("select T.ParameterID,T.ParamValue from (select T.ParameterID,T.ParamValue,row_number() over(partition by T.ParameterDataID) as rn from itrdemo.equipmentparameterdata as T) as T where T.rn <= 100", con);
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

    //    [Route("api/MultiLine/GetDataPacket")]
    //    [HttpGet]
    //    public IEnumerable<PlotingData> GetDataPacket()
    //    {
    //        DataTable dt = new DataTable();
    //        DataSet dset = new DataSet();
    //        MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itrdemo;sslmode=none;");
    //        MySqlCommand cmd;
    //        MySqlDataAdapter da;
    //        con.Open();
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=1", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter1");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=2", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter2");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=3", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter3");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=4", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter4");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=5", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter5");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=6", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter6");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=7", con);
    //           cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter7");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=8", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter8");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=9", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter9"); 
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=10", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter10");
    //        cmd = new MySqlCommand("SELECT * FROM itrdemo.equipmentparameterdata where ParameterID=11", con);
    //        cmd.CommandType = CommandType.Text;
    //        da = new MySqlDataAdapter(cmd);
    //        da.Fill(dset, "Parameter11");
    //        con.Close();
    //        var pdata = new List<PlotingData>();
    //        if(dset.Tables.Count > 10)
    //        {
    //            for (int i = 0; i < dset.Tables[0].Rows.Count; i++)
    //            {
    //                string val = dset.Tables[0].Rows[i]["ParamValue"].ToString();

    //                pdata.Add(new PlotingData
    //                {
    //                    PacketID = i,
    //                    X1 = dset.Tables[0].Rows[i]["ParamValue"].ToString(),
    //                    Y1 = dset.Tables[1].Rows[i]["ParamValue"].ToString(),
    //                    X2 = dset.Tables[2].Rows[i]["ParamValue"].ToString(),
    //                    Y2 = dset.Tables[3].Rows[i]["ParamValue"].ToString(),
    //                    X3 = dset.Tables[4].Rows[i]["ParamValue"].ToString(),
    //                    Y3 = dset.Tables[5].Rows[i]["ParamValue"].ToString(),
    //                    X4 = dset.Tables[6].Rows[i]["ParamValue"].ToString(),
    //                    Y4 = dset.Tables[7].Rows[i]["ParamValue"].ToString(),
    //                    X5 = dset.Tables[8].Rows[i]["ParamValue"].ToString(),
    //                    Y5 = dset.Tables[9].Rows[i]["ParamValue"].ToString(),

    //                });;

    //            }
    //        }
    //        return pdata;
    //    }
    }
}
