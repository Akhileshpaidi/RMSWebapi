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
    public class removeaccesscontroller : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public removeaccesscontroller(MySqlDBContext mySqlDBContext)

        {
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/removeAccessdocument/GetremoveAccessdocumentDetails")]
        [HttpGet]
        public IEnumerable<object> GetremoveAccessdocumentDetails()

        {


            var deatils = (from providepermissionsmaster in mySqlDBContext.provideAccessdocumentModels
                           join adddocumentmaster in mySqlDBContext.AddDocumentModels on providepermissionsmaster.AddDoc_id equals adddocumentmaster.AddDoc_id
                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on adddocumentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active" && adddocumentmaster.status_permission == "Permission"
                           select new
                           {
                               providepermissionsmaster.Doc_User_Access_mapping_id,
                               providepermissionsmaster.AddDoc_id,
                               adddocumentmaster.Title_Doc,
                               naturemaster.NatureOf_Doc_Name,
                               adddocumentmaster.AuthorityTypeName,
                               adddocumentmaster.Keywords_tags,

                           })
                                 .Distinct();
            return deatils;

        }




        [Route("api/removeAccessdocument/GetremoveAccessdocumentDetailsbyid/{unit_location_Master_id}")]
        [HttpGet]
        public IEnumerable<object> GetremoveAccessdocumentDetailsbyid(int unit_location_Master_id)

        {



            var deatils = (from providepermissionsmaster in mySqlDBContext.provideAccessdocumentModels
                           join permissionsmapping in mySqlDBContext.UserPermissionModels on providepermissionsmaster.Doc_User_Access_mapping_id equals permissionsmapping.Doc_User_Access_mapping_id
                           join userlocationmapping in mySqlDBContext.userlocationmappingModels on permissionsmapping.user_location_mapping_id equals userlocationmapping.user_location_mapping_id
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on userlocationmapping.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                           join enitymaster in mySqlDBContext.UnitMasterModels on userlocationmapping.Entity_Master_id equals enitymaster.Entity_Master_id
                           join tbluser in mySqlDBContext.usermodels on permissionsmapping.USR_ID equals tbluser.USR_ID
                           join adddocumentmaster in mySqlDBContext.AddDocumentModels on providepermissionsmaster.AddDoc_id equals adddocumentmaster.AddDoc_id
                           join documenttypemaster in mySqlDBContext.DocTypeMasterModels on adddocumentmaster.DocTypeID equals documenttypemaster.docTypeID
                           join Documentcatmaster in mySqlDBContext.DocCategoryMasterModels on adddocumentmaster.Doc_CategoryID equals Documentcatmaster.Doc_CategoryID
                           join Documentsubcatmaster in mySqlDBContext.DocSubCategoryModels on adddocumentmaster.Doc_SubCategoryID equals Documentsubcatmaster.Doc_SubCategoryID
                           join Authority in mySqlDBContext.AuthorityTypeMasters on adddocumentmaster.AuthorityTypeID equals Authority.AuthorityTypeID
                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on adddocumentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active" && adddocumentmaster.Draft_Status == "Completed" && adddocumentmaster.addDoc_Status == "Active" && permissionsmapping.permissionstatus == "Active" && providepermissionsmaster.Unit_location_Master_id == unit_location_Master_id
                           select new
                           {
                               providepermissionsmaster.Doc_User_Access_mapping_id,
                               providepermissionsmaster.AddDoc_id,
                               adddocumentmaster.Document_Id,
                               documenttypemaster.docTypeName,
                               tbluser.firstname,
                               Documentcatmaster.Doc_CategoryName,
                               Documentsubcatmaster.Doc_SubCategoryName,
                               adddocumentmaster.Title_Doc,
                               userlocationmapping.Unit_location_Master_id,
                               userlocationmapping.Entity_Master_id,
                               enitymaster.Entity_Master_Name,
                               unitlocation.Unit_location_Master_name,
                               naturemaster.NatureOf_Doc_Name,
                               Authority.AuthorityTypeName,
                               adddocumentmaster.Nature_Confidentiality,
                               adddocumentmaster.Keywords_tags,
                               adddocumentmaster.VersionControlNo

                           })
                                                 .GroupBy(x => x.Doc_User_Access_mapping_id)
  .Select(group => group.First());
            return deatils;
        }







        [Route("api/RemoveAccessdocument/UpdateRemoveAccessdocument")]
        [HttpPut]
        public void UpdateRemoveAccessdocument([FromBody] UpdateData updateData)
        {
            try
            {
                doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels = updateData?.doctaskuseracknowledmentstatusmodels;
                int userlocationmappingid = updateData?.userlocationmappingid ?? 0;
                int[] DocPermId = updateData?.DocPermId ?? Array.Empty<int>();

                if (doctaskuseracknowledmentstatusmodels == null || userlocationmappingid == 0)
                {

                    return;
                }

                if (doctaskuseracknowledmentstatusmodels.Doc_User_Access_mapping_id == 0)
                {

                }
                else
                {
                

                    var exisrtingrecord = this.mySqlDBContext.doctaskuseracknowledmentstatusmodels.Where(x => x.user_location_mapping_id == userlocationmappingid && x.Doc_User_Access_mapping_id == doctaskuseracknowledmentstatusmodels.Doc_User_Access_mapping_id && x.status == "Active");
                    foreach (var exisrtingrec in exisrtingrecord)
                    {
                        exisrtingrec.status = "Inactive";
                    }


                    // provideAccessdocumentModels.duedate = Convert.ToDateTime(provideAccessdocumentModels.duedate).ToString("yyyy-MM-dd");
                    var existingpermEntries = mySqlDBContext.UserPermissionModels.Where(x => x.user_location_mapping_id == userlocationmappingid && x.Doc_User_Access_mapping_id == doctaskuseracknowledmentstatusmodels.Doc_User_Access_mapping_id && x.permissionstatus == "Active");
                    foreach (var existingpermEntry in existingpermEntries)
                    {
                        // Check for null before accessing property values
                        if (existingpermEntry != null)
                        {
                            existingpermEntry.permissionstatus = "Inactive";
                        }
                    }

                    foreach (var permId in DocPermId)
                    {
                        var existingPermission = mySqlDBContext.UserPermissionModels
                            .FirstOrDefault(x => x.user_location_mapping_id == userlocationmappingid && x.Doc_perm_rights_id == permId);

            
                        if (existingPermission != null)
                        {
                            existingPermission.permissionstatus = "inactive";
                        }
                        else
                        {
                           
                        }
                    }

                    mySqlDBContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Optionally rethrow the exception if further handling is needed
                // throw;
            }
        }


        [Route("api/removeAccessdocument/getUserdeatilsbyID/{Doc_User_Access_mapping_id}")]
        [HttpGet]

        public IEnumerable<object> getUserdeatilsbyAddDocid(int Doc_User_Access_mapping_id)
        {

            var result = (from docAccess in mySqlDBContext.provideAccessdocumentModels
                          join docPermission in mySqlDBContext.UserPermissionModels on docAccess.Doc_User_Access_mapping_id equals docPermission.Doc_User_Access_mapping_id
                          join userlocationmap in mySqlDBContext.userlocationmappingModels on docPermission.user_location_mapping_id equals userlocationmap.user_location_mapping_id
                          join entitymaster in mySqlDBContext.UnitMasterModels on userlocationmap.Entity_Master_id equals entitymaster.Entity_Master_id
                          join unitmaster in mySqlDBContext.UnitLocationMasterModels on userlocationmap.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                          join tbluser in mySqlDBContext.usermodels on docPermission.USR_ID equals tbluser.USR_ID
                          join Department in mySqlDBContext.DepartmentModels on tbluser.Department_Master_id equals Department.Department_Master_id

                          where  docAccess.Doc_User_Access_mapping_id == Doc_User_Access_mapping_id &&
                                               docPermission.permissionstatus != "inactive"

                          select new
                          {
                              docPermission.user_location_mapping_id,
                              docPermission.USR_ID,
                              entitymaster.Entity_Master_Name,
                              unitmaster.Unit_location_Master_name,
                              tbluser.firstname,
                              Department.Department_Master_name
                          })
                 .GroupBy(x => x.user_location_mapping_id)
                   .Select(g => g.First())
                   .ToList();

            return result;

        }


    }
}

