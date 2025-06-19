using DomainModel;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;



namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DocumentRepositoryController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public DocumentRepositoryController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }



        [Route("api/DocumentRepository/GetDocumentRepositoryList")]
        [HttpGet]

        public IEnumerable<DocumentFilesuplodModel> GetDocumentRepositoryList()
        {

            return this.mySqlDBContext.DocumentFilesuplodModels.Where(x => x.Status == "Active").ToList();

        }






    }
}
