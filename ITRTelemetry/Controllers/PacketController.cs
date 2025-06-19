using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class PacketController
    {
        private readonly MySqlDBContext mySqlDBContext;

        public PacketController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/Packet/GetPackets")]
        [HttpGet]
        public IEnumerable<object> Get()
        {
            return this.mySqlDBContext.PacketModels.Where(x => x.OriginalStatus == "Active").ToList();
          
        }

        [Route("api/Packet/GetPacketsDetails")]
        [HttpGet]
        public IEnumerable<object> GetPacketsDetails()
        {
            //return this.mySqlDBContext.PacketModels.Where(x => x.OriginalStatus == "Active").ToList();
            var ParamData = (from equipmentparametermaster in mySqlDBContext.EquipmentParameterModels
                             join equipmentmaster in mySqlDBContext.EquipmentsModels on equipmentparametermaster.EquipmentID equals equipmentmaster.EquipmentID
                             join equipmentparameterdata in mySqlDBContext.EquipmentParameterDetailModels on equipmentparametermaster.ParameterID equals equipmentparameterdata.ParameterID
                             join packetmaster in mySqlDBContext.PacketModels on equipmentparameterdata.PacketMasterID equals packetmaster.PacketMasterID
                             select new
                             {
                                 equipmentparametermaster.ParameterID,
                                 equipmentparametermaster.EquipmentID,
                                 equipmentmaster.EquipmentName,
                                 equipmentparametermaster.ParameterName,
                                 equipmentparameterdata.ParamData,
                                 equipmentparameterdata.Date,
                                 packetmaster.PacketMasterID,
                                 packetmaster.PacketID,
                                 packetmaster.ParameterListSize,
                                 packetmaster.LockStatus,
                                 packetmaster.Hours,
                                 packetmaster.Minutes,
                                 packetmaster.Seconds,
                                 packetmaster.MilliSeconds,
                                 packetmaster.TransmissionDate


                             });


            return ParamData;
        }

        //[Route("aapi/Packet/InsertParameter")]
        //[HttpPost]
        //public IActionResult InsertParameter([FromBody] PacketModel PacketModels)
        //{
        //    var packetModels = this.mySqlDBContext.PacketModels;
        //    packetModels.Add(PacketModels);
        //    DateTime dt = DateTime.Now;
        //    string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

        //    PacketModels.TransmissionDate = dt1;
        //    PacketModels.OriginalStatus = "Active";
        //    PacketModels.Hours = dt.ToString("HH");
        //    PacketModels.Minutes = dt.ToString("mm");
        //    PacketModels.Seconds = dt.ToString("ss");
        //    this.mySqlDBContext.SaveChanges();
        //    return Ok();
        //}
    }
}
