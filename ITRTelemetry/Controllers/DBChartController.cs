using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;


namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class DBChartController : ControllerBase
    {
        public DBChartController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        [Route("api/DBChart/GetParamDetails")]
        [HttpGet]
        public dynamic[] GenerateLiveData()
        {
            //myDb1ConnectionString = _configuration.GetConnectionString("myDb1");
            DataTable dtp = new DataTable();
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT equipmentparameterdata.ParamDataID,equipmentparametermaster.ParameterID, equipmentparametermaster.ParameterName,equipmentparameterdata.ParamData, parametertypemaster.ParameterTypeID, parametertypemaster.ParameterType from equipmentparameterdata inner join equipmentparametermaster on equipmentparametermaster.ParameterID = equipmentparameterdata.ParameterID inner join parametertypemaster on parametertypemaster.ParameterTypeID = equipmentparametermaster.ParameterTypeID where EquipmentID = 1 and equipmentparametermaster.ParameterTypeID IS  NOT NULL ORDER BY equipmentparameterdata.ParamDataID DESC LIMIT 500", con);
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dtp);
            con.Close();

            var pdata = new LiveChartModel[dtp.Rows.Count];
            try
            {
                    for (int i = 0; i < dtp.Rows.Count; i++)
                    {
                        pdata[i] = new LiveChartModel(
                            Convert.ToInt32(dtp.Rows[i]["ParameterID"].ToString()),
                            Convert.ToString(dtp.Rows[i]["ParameterName"].ToString()),
                            Convert.ToString(dtp.Rows[i]["ParamData"].ToString()),
                            Convert.ToInt32(dtp.Rows[i]["ParameterTypeID"].ToString()),
                            Convert.ToString(dtp.Rows[i]["ParameterType"].ToString()),
                            1
                        );
                    }
                    System.Diagnostics.Debug.WriteLine(pdata);
                
            }
            catch (Exception ex)
            {

            }
            return pdata;
        }
    }
}
