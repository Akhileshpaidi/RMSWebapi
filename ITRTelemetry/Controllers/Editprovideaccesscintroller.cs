using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;

namespace ITRTelemetry.Controllers
{

    [Produces("application/json")]
    public class Editprovideaccesscintroller : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Editprovideaccesscintroller(MySqlDBContext mySqlDBContext)

        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/editProvideAccessdocument/GeteditProvideAccessdocumentDetails")]
        [HttpGet]

        public IEnumerable<object> GeteditProvideAccessdocumentDetails()

        {


            var deatils = (from providepermissionsmaster in mySqlDBContext.provideAccessdocumentModels
                           join adddocumentmaster in mySqlDBContext.AddDocumentModels on providepermissionsmaster.AddDoc_id equals adddocumentmaster.AddDoc_id
                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on adddocumentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active"
                           select new
                           {
                               providepermissionsmaster.AddDoc_id,
                               adddocumentmaster.Title_Doc,
                               naturemaster.NatureOf_Doc_Name,
                               adddocumentmaster.AuthorityTypeName,
                               adddocumentmaster.Keywords_tags,

                           })
                                .Distinct();
            return deatils;

        }
    }
}
