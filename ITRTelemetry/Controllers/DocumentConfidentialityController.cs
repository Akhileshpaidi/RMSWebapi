using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using MySqlConnector;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DocumentConfidentialityController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public DocumentConfidentialityController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/Confidentiality/GetDocumentConfidentiality")]
        [HttpGet]

        public IEnumerable<DocumentConfidentiality> GetDocumentConfidentiality()
        {
            return this.mySqlDBContext.DocumentConfidentialitys.Where(x => x.Status == "Active").ToList();
        }
        [Route("api/Confidentiality/InsertDocumentConfidentiality")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] DocumentConfidentiality DocumentConfidentialitys)
        {
            try
            {
                // Proceed with the insertion
                var DocumentConfidentiality = this.mySqlDBContext.DocumentConfidentialitys;
                DocumentConfidentiality.Add(DocumentConfidentialitys);
                DocumentConfidentialitys.Status = "Active";

                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
               return BadRequest($"Error: {ex.Message}");
               
            }
        }
        [Route("api/Confidentiality/UpdateDocumentConfidentiality")]
        [HttpPut]
        public IActionResult UpdateDocumentConfidentiality([FromBody] DocumentConfidentiality DocumentConfidentialitys)
        {
            try
            {
                if (DocumentConfidentialitys.DocumentConfidentialityID == 0)
                {
                     return Ok("Insertion successful");
                }
                else
                {
                    // Existing department, update logic

                    this.mySqlDBContext.Attach(DocumentConfidentialitys);
                    this.mySqlDBContext.Entry(DocumentConfidentialitys).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(DocumentConfidentialitys);

                    Type type = typeof(DocumentConfidentiality);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(DocumentConfidentialitys, null) == null || property.GetValue(DocumentConfidentialitys, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
              return BadRequest($"Error: {ex.Message}");
              
            }

        }


        [Route("api/Confidentiality/DeleteDocumentConfidentiality")]
        [HttpDelete]
        public void DeleteDocumentConfidentiality(int id)
        {
            var currentClass = new DocumentConfidentiality { DocumentConfidentialityID = id };
            currentClass.Status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
