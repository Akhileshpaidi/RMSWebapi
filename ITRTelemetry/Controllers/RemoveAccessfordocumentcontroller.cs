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
  
    public class RemoveAccessfordocumentcontroller : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public RemoveAccessfordocumentcontroller(MySqlDBContext mySqlDBContext)

        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/removeProvideAccessdocument/GetremoveProvideAccessdocumentDetails")]
        [HttpGet]


        public IEnumerable<object> GetremoveProvideAccessdocumentDetails()

        {


            var deatils = (from providepermissionsmaster in mySqlDBContext.provideAccessdocumentModels
                           join adddocumentmaster in mySqlDBContext.AddDocumentModels on providepermissionsmaster.AddDoc_id equals adddocumentmaster.AddDoc_id
                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on adddocumentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active" && adddocumentmaster.status_permission == "Permission"
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
        //[Route("api/RemoveAccessdocument/UpdateRemoveAccessdocument")]
        //[HttpPut]
        //public void UpdateRemoveAccessdocument([FromBody] UpdateData updateData)
        //{
        //    try
        //    {
        //        ProvideAccessdocument provideAccessdocumentModels = updateData?.ProvideAccessdocumentModels;
        //        int UserId = updateData?.UserId ?? 0; // Assign a default value if UserId is null
        //        int[] DocPermId = updateData?.DocPermId ?? Array.Empty<int>(); // Assign an empty array if DocPermId is null

        //        if (provideAccessdocumentModels == null || UserId == 0)
        //        {
        //            // Log or handle the case where provideAccessdocumentModels or UserId is null
        //            return;
        //        }

        //        if (provideAccessdocumentModels.Doc_User_Access_mapping_id == 0)
        //        {
        //            // Handle the case where Doc_User_Access_mapping_id is 0 (e.g., insert logic).
        //            // Log or handle this case as needed
        //        }
        //        else
        //        {
        //            provideAccessdocumentModels.duedate = Convert.ToDateTime(provideAccessdocumentModels.duedate).ToString("yyyy-MM-dd");

        //            // Attach the entity
        //            this.mySqlDBContext.Attach(provideAccessdocumentModels);
        //            this.mySqlDBContext.Entry(provideAccessdocumentModels).State = EntityState.Modified;

        //            var entry = this.mySqlDBContext.Entry(provideAccessdocumentModels);

        //            Type type = typeof(ProvideAccessdocument);
        //            PropertyInfo[] properties = type.GetProperties();
        //            foreach (PropertyInfo property in properties)
        //            {
        //                // Check for null before accessing property values
        //                if (property.GetValue(provideAccessdocumentModels, null) == null || property.GetValue(provideAccessdocumentModels, null).Equals(0))
        //                {
        //                    entry.Property(property.Name).IsModified = false;
        //                }
        //            }

        //            var existingpermEntries = mySqlDBContext.UserPermissionModels.Where(x => x.USR_ID == UserId);

        //            // Set existing UserPermissionModels to inactive
        //            foreach (var existingpermEntry in existingpermEntries)
        //            {
        //                // Check for null before accessing property values
        //                if (existingpermEntry != null)
        //                {
        //                    existingpermEntry.permissionstatus = "inactive";
        //                }
        //            }

        //            // Update status for each Doc_perm_rights_id of the selected UserId
        //            foreach (var permId in DocPermId)
        //            {
        //                var existingPermission = mySqlDBContext.UserPermissionModels
        //                    .FirstOrDefault(x => x.USR_ID == UserId && x.Doc_perm_rights_id == permId);

        //                // Check for null before accessing property values
        //                if (existingPermission != null)
        //                {
        //                    existingPermission.permissionstatus = "inactive";
        //                }
        //                else
        //                {
        //                    // If the record doesn't exist, you may choose to handle it or ignore it
        //                    // Log or handle this case as needed
        //                }
        //            }

        //            mySqlDBContext.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception details
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        // Optionally rethrow the exception if further handling is needed
        //        // throw;
        //    }
        //}

    }

}

