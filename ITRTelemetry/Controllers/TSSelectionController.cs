using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DomainModel;
using ITRTelemetry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class TSSelectionController : ControllerBase
    {
        string TStation;
        [Route("api/TSSelection/Get")]
        public IEnumerable<SelectedTStation> Get(int MissionID)
        {

            MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none;");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT prioritymapping.PriorityMappingID,missiontable.EquipmentID,prioritymapping.MissionID,prioritymapping.TSID,telemetrystationtable.Name,prioritymapping.Priority,telemetrystationtable.IPAddress,telemetrystationtable.Port FROM prioritymapping inner join missiontable on missiontable.MissionID = prioritymapping.MissionID inner join telemetrystationtable on telemetrystationtable.TSID = prioritymapping.TSID where missiontable.MissionID = '" + MissionID + "' order by Priority ASC", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            int arrsize = dt.Rows.Count;
            var data = new StationSelection[arrsize];
            var pdata = new SelectedTStation[1];
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    data[i] = new StationSelection
                    (
                      Convert.ToInt32(dt.Rows[i]["TSID"].ToString()),
                      dt.Rows[i]["Name"].ToString(),
                      dt.Rows[i]["IPAddress"].ToString(),
                      dt.Rows[i]["Port"].ToString(),
                      Convert.ToInt32(dt.Rows[i]["Priority"].ToString())
                    );
                }

            }

            con.Close();
            for(int i=0; i<data.Length; i++)
            {
                int validity = TelemetryStationUDP.UDP1(data[i].IPAddress, data[i].Port);
                if (validity == 1)
                {
                    pdata[i] = new SelectedTStation
                   (
                     Convert.ToInt32(dt.Rows[i]["TSID"].ToString()),
                     dt.Rows[i]["Name"].ToString(),
                     dt.Rows[i]["IPAddress"].ToString(),
                     dt.Rows[i]["Port"].ToString(),
                     Convert.ToInt32(dt.Rows[i]["Priority"].ToString()),
                     validity
                   );
                  break;
                }
                else
                {
                    continue;
                }
            }
            return pdata;
        }

       
    }

  
}
