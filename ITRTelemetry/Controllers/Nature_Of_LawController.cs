//using DomainModel;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ApplicationModels;
//using Microsoft.EntityFrameworkCore;
//using MySQLProvider;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;


//namespace ITRTelemetry.Controllers
//{
//    [Produces("application/json")]
//    public class Nature_Of_LawController : ControllerBase
//    {
//        private MySqlDBContext mySqlDBContext;

//        public Nature_Of_LawController(MySqlDBContext mySqlDBContext)
//        {
//            this.mySqlDBContext = mySqlDBContext;
//        }
//        [Route("api/Nature_Of_Law/GetNatureOfLawDetails")]
//        [HttpGet]
       
//        public IEnumerable<Nature_Of_LawModel> GetNatureOfLawDetails()
//        {
//            return this.mySqlDBContext.Nature_Of_LawModels.Where(x => x.NatureofLawStatus == "Active").ToList();
//        }


//        [Route("api/Nature_Of_Law/InsertNatureOfLawDetails")]
//        [HttpPost]

//        public IActionResult InsertParameter([FromBody] Nature_Of_LawModel Nature_Of_LawModel)
//        {
//            var Nature_Of_LawModels = this.mySqlDBContext.Nature_Of_LawModels;
//            Nature_Of_LawModels.Add(Nature_Of_LawModel);
//            //DateTime dt = DateTime.Now;
//            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
//            //SectorModels.Department_Master_CreatedDate = dt1;
//            Nature_Of_LawModel.NatureofLawStatus = "Active";
//            this.mySqlDBContext.SaveChanges();
//            return Ok();
//        }

//        [Route("api/Nature_Of_Law/UpdateNatureOfLawDetails")]
//        [HttpPut]

//        public void UpdateNatureOfLawDetails([FromBody] Nature_Of_LawModel Nature_Of_LawModels)
//        {
//            if (Nature_Of_LawModels.NatureofLawMasterID == 0)
//            {
//            }
//            else
//            {
//                this.mySqlDBContext.Attach(Nature_Of_LawModels);
//                this.mySqlDBContext.Entry(Nature_Of_LawModels).State = EntityState.Modified;

//                var entry = this.mySqlDBContext.Entry(Nature_Of_LawModels);

//                Type type = typeof(Nature_Of_LawModel);
//                PropertyInfo[] properties = type.GetProperties();
//                foreach (PropertyInfo property in properties)
//                {
//                    if (property.GetValue(Nature_Of_LawModels, null) == null || property.GetValue(Nature_Of_LawModels, null).Equals(0))
//                    {
//                        entry.Property(property.Name).IsModified = false;
//                    }
//                }

//                this.mySqlDBContext.SaveChanges();
//            }
//        }


//        [Route("api/Nature_Of_Law/DeleteNatureOfLawDetails")]
//        [HttpDelete]

//        public void DeleteNatureOfLawDetails(int id)
//        {
//            var currentClass = new Nature_Of_LawModel { NatureofLawMasterID = id };
//            currentClass.NatureofLawStatus = "Inactive";
//            this.mySqlDBContext.Entry(currentClass).Property("NatureofLawStatus").IsModified = true;
//            this.mySqlDBContext.SaveChanges();
//        }
//    }
//}
