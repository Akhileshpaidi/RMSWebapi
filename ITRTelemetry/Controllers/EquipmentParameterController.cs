using DomainModel;
using ITR_TelementaryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySQLProvider;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ITR_TelementaryAPI.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class EquipmentParameterController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public EquipmentParameterController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/EquipmentParameter/GetParamDetails")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> Get()
        {
            return this.mySqlDBContext.EquipmentParameterModels.Where(x => x.Status == "Active").ToList();
            //var ParamData = (from equipmentparametermaster in mySqlDBContext.EquipmentParameterModels
            //                 join equipmentmaster in mySqlDBContext.EquipmentsModels on equipmentparametermaster.EquipmentID equals equipmentmaster.EquipmentID
            //                 select new
            //               {
            //                 equipmentparametermaster.ParameterID,
            //                 equipmentparametermaster.EquipmentID,
            //                 equipmentmaster.EquipmentName,
            //                 equipmentparametermaster.FrameRate,
            //                 equipmentparametermaster.FrameLength,
            //                 equipmentparametermaster.FrameDescription,
            //                 equipmentparametermaster.ParameterName,
            //                 equipmentparametermaster.StartByte,
            //                 equipmentparametermaster.EndByte,
            //                 equipmentparametermaster.DataType,
            //                 equipmentparametermaster.Remark,
            //                 equipmentparametermaster.DateTime,
            //                 equipmentparametermaster.Status

            //             }).Where(x => x.Status == "Active").ToList();


            //return ParamData;
        }

        [Route("api/EquipmentParameter/InsertParameter")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] EquipmentParameterModel EquipmentParameterModels)
        {
            var equipmentParameterModels = this.mySqlDBContext.EquipmentParameterModels;
            equipmentParameterModels.Add(EquipmentParameterModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            EquipmentParameterModels.DateTime = dt1;
            EquipmentParameterModels.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }



        [Route("api/EquipmentParameter/UpdateParameter")]
        [HttpPut]
        public void UpdateParameter([FromBody] EquipmentParameterModel EquipmentParameterModels)
        {
            if (EquipmentParameterModels.ParameterID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(EquipmentParameterModels);
                this.mySqlDBContext.Entry(EquipmentParameterModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(EquipmentParameterModels);

                Type type = typeof(EquipmentParameterModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(EquipmentParameterModels, null) == null || property.GetValue(EquipmentParameterModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }

        }



        [Route("api/EquipmentParameter/DeleteParameter")]
        [HttpDelete]
        public void DeleteParameter(int id)
        {
            var currentClass = new EquipmentParameterModel { ParameterID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        [Route("api/EquipmentParameter/DynamicParameter")]
        [HttpPost]
        public IActionResult DynamicParameter([FromBody] Object objData)
        {

            string source = objData.ToString();
            dynamic data = JObject.Parse(source);
            //Console.WriteLine(data.numberOfParams);

            string paramno = data.numberOfParams;
            string equipmentid = data.numberOfEquipments;
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject((data.paramlist).ToString());
            foreach (JObject config in data["paramlist"])
            {

                string name = (string)config["name"];
                string paramtype = (string)config["paramtype"];
                string startbyte = (string)config["startbyte"];
                string endbyte = (string)config["endbyte"];
                string datatype = (string)config["datatype"];
                string framerate = (string)config["framerate"];
                string framelen = (string)config["framelen"];
                string framedesc = (string)config["framedesc"];
                EquipmentParameterModel equip = new EquipmentParameterModel();
                equip.ParameterName = name;
                equip.ParameterTypeID = Convert.ToInt32(paramtype);
                equip.StartByte = startbyte;
                equip.EndByte = endbyte;
                equip.DataType = datatype;
                equip.FrameRate = framerate;
                equip.FrameLength = framelen;
                equip.FrameDescription = framedesc;
                equip.EquipmentID = Convert.ToInt32(equipmentid);

                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                equip.DateTime = dt1;
                equip.Status = "Active";
                var equipmentParameterModels = this.mySqlDBContext.EquipmentParameterModels;
                equipmentParameterModels.Add(equip);
                this.mySqlDBContext.SaveChanges();
            }
            return Ok();


        }

        [Route("api/EquipmentParameter/GetAllParameters")]
        [HttpGet]
        public IEnumerable<object> GetAllParameters()
        {

            var ParamData = (from equipmentparametermaster in mySqlDBContext.EquipmentParameterModels
                             join equipmentmaster in mySqlDBContext.EquipmentsModels on equipmentparametermaster.EquipmentID equals equipmentmaster.EquipmentID
                             join equipmentparameterdata in mySqlDBContext.EquipmentParameterDetailModels on equipmentparametermaster.ParameterID equals equipmentparameterdata.ParameterID
                             select new
                             {
                                 equipmentparametermaster.ParameterID,
                                 equipmentparametermaster.EquipmentID,
                                 equipmentmaster.EquipmentName,
                                 equipmentparametermaster.ParameterName,
                                 equipmentparameterdata.ParamData,
                                 equipmentparameterdata.Date,
                                 //equipmentparametermaster.FrameRate,
                                 //equipmentparametermaster.FrameLength,
                                 //equipmentparametermaster.FrameDescription,
                                 //equipmentparametermaster.StartByte,
                                 //equipmentparametermaster.EndByte,
                                 //equipmentparametermaster.DataType,
                                 //equipmentparametermaster.Remark,
                                 ////equipmentparametermaster.DateTime,
                                 //equipmentparametermaster.Status,
                                 //equipmentparameterdata.ParamDataID,
                                 //equipmentparameterdata.FlightID,
                                 //equipmentparameterdata.UDPPacketSequenceNo,
                                 //equipmentparameterdata.UPDPacketID,

                             });


            return ParamData;
        }




        [Route("api/EquipmentParameter/GetParamsByTypeID")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> GetParamsByTypeID(int id)
        {
            return this.mySqlDBContext.EquipmentParameterModels.Where(x => x.ParameterTypeID == id).ToList();
        }



        [Route("api/EquipmentParameter/GetDistinctParameters")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> GetParameters(int ParameterTypeID)
        {

            //MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            //con.Open();
            //MySqlCommand cmd = new MySqlCommand("SELECT  ParameterID,ParameterName from  itr.equipmentparametermaster where ParameterTypeID='"+ ParameterTypeID + "'", con);

            //cmd.CommandType = CommandType.Text;

            //MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            //DataTable dt = new DataTable();
            //da.Fill(dt);
            //con.Close();
            //var pdata = new List<GetParamData>();
            //if (dt.Rows.Count > 0)
            //{
            //    for (var i = 0; i < dt.Rows.Count; i++)
            //    {
            //        pdata.Add(new GetParamData { parameterid = (Convert.ToInt32(dt.Rows[i]["ParameterID"].ToString())) });
            //        pdata.Add(new GetParamData { name = dt.Rows[i]["ParameterName"].ToString() });
            //    }
            //}
            //return pdata;
            return this.mySqlDBContext.EquipmentParameterModels.Where(x => x.ParameterTypeID == ParameterTypeID).ToList();

        }

        [Route("api/EquipmentParameter/GetParameterData")]
        [HttpGet]
        public IEnumerable<GetParamData> GetParameterData(string paramdata)
        {

            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT equipmentparametermaster.ParameterName,ParamData from equipmentparameterdata inner join equipmentparametermaster on equipmentparametermaster.ParameterID=equipmentparameterdata.ParameterID where ParameterName='" + paramdata + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<GetParamData>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new GetParamData { name = dt.Rows[i]["ParameterName"].ToString(), mean = dt.Rows[i]["ParamData"].ToString() });
                }
            }
            return pdata;

        }
        [Route("api/EquipmentParameter/InsertParameterArray")]
        [HttpPost]
        public IActionResult InsertEquipmentParameterFromExcel([FromBody] Object objData)
        {
            string source = objData.ToString();
            dynamic data = JObject.Parse(source);
            //Console.WriteLine(data.numberOfParams);
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject((data.EquipmentParameterExcelModel).ToString());
            foreach (JObject config in data["EquipmentParameterExcelModel"])
            {

                string ParameterName = (string)config["ParameterName"];
                string StartByte = (string)config["StartByte"];
                string EndByte = (string)config["EndByte"];
                string Remark = (string)config["Remark"];
                string DataType = (string)config["DataType"];
                string EquipmentID = (string)config["EquipmentID"];
                string Minimum = (string)config["Minimum"];
                string Maximum = (string)config["Maximum"];
                string ParameterGroupId = (string)config["ParameterGroupId"];
                EquipmentParameterModel equip = new EquipmentParameterModel();
                equip.ParameterName = ParameterName;
                equip.StartByte = StartByte;
                equip.EndByte = EndByte;
                equip.Remark = Remark;
                equip.DataType = DataType;
                equip.EquipmentID = Convert.ToInt32(EquipmentID);
                equip.Minimum = Minimum;
                equip.Maximum = Maximum;
                equip.ParameterTypeID = Convert.ToInt32(ParameterGroupId);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                equip.DateTime = dt1;
                equip.Status = "Active";
                var equipmentParameterModels = this.mySqlDBContext.EquipmentParameterModels;
                equipmentParameterModels.Add(equip);
                this.mySqlDBContext.SaveChanges();

            }
            //int id = 0;
            //MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            //string parameterdata = "INSERT INTO equipmentparametermaster(ParameterName,StartByte,EndByte,DataType,Remark,EquipmentID ,DateTime) VALUES (@ParameterName,@StartByte,@EndByte,@DataType,@Remark,@EquipmentID ,@DateTime);SELECT LAST_INSERT_ID();";
            //try
            //{
            //    //EquipmentParameterExcelModel[] EquipmentParameterExcelModel = EquipmentParameterModels.EquipmentParameterExcelModel;
            //    con.Open();
            //    for (int i = 0; i < EquipmentParameterExcelModel.Length; i++)
            //    {
            //        using (MySqlCommand myCommand = new MySqlCommand(parameterdata, con))
            //        {
            //            myCommand.Parameters.AddWithValue("@ParameterName", EquipmentParameterExcelModel[i].ParameterName);
            //            myCommand.Parameters.AddWithValue("@StartByte", EquipmentParameterExcelModel[i].StartByte);
            //            myCommand.Parameters.AddWithValue("@EndByte", EquipmentParameterExcelModel[i].EndByte);
            //            myCommand.Parameters.AddWithValue("@DataType", EquipmentParameterExcelModel[i].DataType);
            //            myCommand.Parameters.AddWithValue("@Remark", EquipmentParameterExcelModel[i].Remark);
            //            myCommand.Parameters.AddWithValue("@EquipmentID", EquipmentParameterExcelModel[i].EquipmentID);
            //            myCommand.Parameters.AddWithValue("@DateTime", DateTime.Now);
            //            id = Convert.ToInt32(myCommand.ExecuteScalar());
            //        }
                    
            //    }
            //    con.Close();
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest();
            //    System.Diagnostics.Debug.WriteLine("error " + ex.Message);

            //}
            //finally
            //{
            //}
            return Ok();
        }
        [Route("api/EquipmentParameter/getParameters")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> getParameters(int EquipmentID)
        {

            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT EquipmentID,ParameterID,ParameterName,StartByte,EndByte,DataType,Remark from equipmentparametermaster where ParameterTypeID IS NULL and EquipmentID='"+ EquipmentID + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<EquipmentParameterModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new EquipmentParameterModel {
                                                            EquipmentID = Convert.ToInt32(dt.Rows[i]["EquipmentID"]), 
                                                            ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"]), 
                                                            ParameterName = dt.Rows[i]["ParameterName"].ToString(),
                                                            StartByte = dt.Rows[i]["StartByte"].ToString(),
                                                            EndByte = dt.Rows[i]["EndByte"].ToString(),
                                                            DataType = dt.Rows[i]["DataType"].ToString(),
                                                            Remark = dt.Rows[i]["Remark"].ToString(),
                                                           });
                }
            }
            return pdata;

        }

        [Route("api/EquipmentParameter/UpdateParamsGroupIDS")]
        [HttpPut]
        public void UpdateParamsGroupIDS(int pid,int paramgroupid)
        {
            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            string UpdateQuery = "update equipmentparametermaster set ParameterTypeID=@ParameterTypeID where ParameterID=@ParameterID";
            try
            {
                using (MySqlCommand myCommand = new MySqlCommand(UpdateQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@ParameterID", pid);
                    myCommand.Parameters.AddWithValue("@ParameterTypeID", paramgroupid);
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
        [Route("api/EquipmentParameter/getParamsByEquipmentID")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> getParamsByEquipmentID(int EquipmentID)
        {

            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT EquipmentID,ParameterID,ParameterName,StartByte,EndByte,DataType,Remark from equipmentparametermaster where ParameterTypeID IS NoT NULL and EquipmentID='" + EquipmentID + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<EquipmentParameterModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new EquipmentParameterModel
                    {
                        EquipmentID = Convert.ToInt32(dt.Rows[i]["EquipmentID"]),
                        ParameterID = Convert.ToInt32(dt.Rows[i]["ParameterID"]),
                        ParameterName = dt.Rows[i]["ParameterName"].ToString(),
                        StartByte = dt.Rows[i]["StartByte"].ToString(),
                        EndByte = dt.Rows[i]["EndByte"].ToString(),
                        DataType = dt.Rows[i]["DataType"].ToString(),
                        Remark = dt.Rows[i]["Remark"].ToString(),
                    });
                }
            }
            return pdata;

        }
        [Route("api/EquipmentParameter/GetParametersByID")]
        [HttpGet]
        public IEnumerable<EquipmentParameterModel> GetParametersByID(int ParameterTypeID,int EquipmentID)
        {

     
            return this.mySqlDBContext.EquipmentParameterModels.Where(x => x.ParameterTypeID == ParameterTypeID && x.EquipmentID== EquipmentID).ToList();

        }

    }

}



