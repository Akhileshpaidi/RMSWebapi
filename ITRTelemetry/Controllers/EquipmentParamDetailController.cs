using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;

namespace ITR_TelementaryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentParamDetailController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public EquipmentParamDetailController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        // GET: api/<EquipmentParamDetailController>
        // GET: api/EquipmentParamDetail
        [HttpGet]
        public IEnumerable<EquipmentParameterDetailModel> Get()
        {
            return this.mySqlDBContext.EquipmentParameterDetailModels.ToList();
        }
    }
}
