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
using DocumentFormat.OpenXml.Bibliography;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlPowerTools.HtmlToWml.CSS;
using DocumentFormat.OpenXml.Spreadsheet;
using ITR_TelementaryAPI;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class editAccesscontroller : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        private ClsEmail obj_Clsmail = new ClsEmail();
        public editAccesscontroller(MySqlDBContext mySqlDBContext)

        {
            this.mySqlDBContext = mySqlDBContext;
        }
        [Route("api/editAccessdocument/GeteditAccessdocumentDetails/{unit_location_Master_id}")]
        [HttpGet]

        public IEnumerable<object> GeteditAccessdocumentDetails( int unit_location_Master_id)

        {
            try
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
                               where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active" &&adddocumentmaster. Draft_Status == "Completed" && adddocumentmaster.addDoc_Status == "Active" && permissionsmapping.permissionstatus =="Active" && providepermissionsmaster.Unit_location_Master_id == unit_location_Master_id
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
            catch(Exception ex) 
            {
                string errorMessage = ex.Message;
                throw new Exception(errorMessage);
            }
        }

        [Route("api/editAccessdocument/GeteditAccessdocumentDetailsbyid/{Doc_User_Access_mapping_id}")]
        [HttpGet]

        public IEnumerable<object> GeteditAccessdocumentDetailsbyid(int Doc_User_Access_mapping_id)

        {


            var deatils = (from providepermissionsmaster in mySqlDBContext.provideAccessdocumentModels
                           join adddocumentmaster in mySqlDBContext.AddDocumentModels on providepermissionsmaster.AddDoc_id equals adddocumentmaster.AddDoc_id
                         join acktable in mySqlDBContext.doctaskuseracknowledmentstatusmodels on providepermissionsmaster.Doc_User_Access_mapping_id equals acktable.Doc_User_Access_mapping_id
                           //join unitmaster in mySqlDBContext.UnitLocationMasterModels on providepermissionsmaster.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                           join userpermissionmaster in mySqlDBContext.UserPermissionModels on providepermissionsmaster.Doc_User_Access_mapping_id equals userpermissionmaster.Doc_User_Access_mapping_id
                           join usermaster in mySqlDBContext.usermodels on userpermissionmaster.USR_ID equals usermaster.USR_ID
                           join permissionmaster in mySqlDBContext.UserrightsModels on userpermissionmaster.Doc_perm_rights_id equals permissionmaster.Doc_perm_rights_id
                           //join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on adddocumentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active" && adddocumentmaster.Draft_Status == "Completed" && adddocumentmaster.addDoc_Status == "Active"
                             && providepermissionsmaster.Doc_User_Access_mapping_id == Doc_User_Access_mapping_id
                           select new
                           {
                               providepermissionsmaster.Doc_User_Access_mapping_id,
                               providepermissionsmaster.AddDoc_id,
                              // providepermissionsmaster.Entity_Master_id,
                              // entitymaster.Entity_Master_Name,
                              // providepermissionsmaster.Unit_location_Master_id,
                              // unitmaster.Unit_location_Master_name,
                               userpermissionmaster.USR_ID,
                               userpermissionmaster.user_location_mapping_id,
                               usermaster.firstname,
                               userpermissionmaster.Doc_perm_rights_id,

                               //permissionmaster.publish_Name,
                               //adddocumentmaster.Title_Doc,
                               //naturemaster.NatureOf_Doc_Name,
                               // adddocumentmaster.AuthorityTypeName,
                               //  adddocumentmaster.Keywords_tags,
                              // acktable.everyday,
                              // acktable.ack_status,
                              // duedate = Convert.ToDateTime(acktable.duedate).ToString("dd-MM-yyyy"),
                              // acktable.noofdays,
                              // acktable.trakstatus,
                              // acktable.optionalreminder,
                              // acktable.reqtimeperiod,
                              // acktable.timeperiod,
                              // acktable.validitydocument,
                              //startDate = Convert.ToDateTime(acktable.startDate).ToString("yyyy-MM-dd"),
                              // endDate = Convert.ToDateTime(acktable.endDate).ToString("yyyy-MM-dd") 


                           })
                                .Distinct();
            return deatils;

        }

        [Route("api/editAccessdocument/getUserdeatilsbyID/{Doc_User_Access_mapping_id}")]
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

                          where docAccess.Doc_User_Access_mapping_id == Doc_User_Access_mapping_id
                          && docPermission.permissionstatus != "inactive"
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


        [Route("api/editAccessdocument/getUserdeatils")]
        [HttpGet]


        public IEnumerable<object> getUserdeatils()
        {
            var result = (from docAccess in mySqlDBContext.provideAccessdocumentModels
                          join docPermission in mySqlDBContext.UserPermissionModels on docAccess.Doc_User_Access_mapping_id equals docPermission.Doc_User_Access_mapping_id
                          join userlocationmap in mySqlDBContext.userlocationmappingModels on docPermission.user_location_mapping_id equals userlocationmap.user_location_mapping_id
                          join entitymaster in mySqlDBContext.UnitMasterModels on userlocationmap.Entity_Master_id equals entitymaster.Entity_Master_id
                          join unitmaster in mySqlDBContext.UnitLocationMasterModels on userlocationmap.Unit_location_Master_id equals unitmaster.Unit_location_Master_id
                          join tbluser in mySqlDBContext.usermodels on docPermission.USR_ID equals tbluser.USR_ID
                          join Department in mySqlDBContext.DepartmentModels on tbluser.Department_Master_id equals Department.Department_Master_id
                          where docPermission.permissionstatus=="Active"
     
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


        [Route("api/editAccessdocument/getUserdeatilsbyuserid/{user_location_mapping_id}")]
        [HttpGet]


        public IEnumerable<object> getUserdeatilsbyuserid(int user_location_mapping_id)
        
        {
            var result = (from userperimissions in mySqlDBContext.UserPermissionModels
                         join docpermission in mySqlDBContext.provideAccessdocumentModels on userperimissions.Doc_User_Access_mapping_id equals docpermission.Doc_User_Access_mapping_id
                         join documentmaster in mySqlDBContext.AddDocumentModels on docpermission.Document_Id equals documentmaster.Document_Id
                         join tbluser in mySqlDBContext.usermodels on userperimissions.USR_ID equals tbluser.USR_ID
                          where userperimissions.user_location_mapping_id == user_location_mapping_id

                         select new
                         {
                             documentmaster.Keywords_tags,
                             documentmaster. Title_Doc,
                             docpermission.Document_Id,
                             userperimissions .user_location_mapping_id,
                             userperimissions.Doc_User_Access_mapping_id,
                             userperimissions.USR_ID,
                             tbluser

                         })
                       .GroupBy(x => x.Doc_User_Access_mapping_id)
                    .Select(g => g.First())
                    .ToList();




            return result;
        }





            [Route("api/editAccessdocument/UpdateEeditAccessdocument")]
        [HttpPut]
        public void UpdateEeditAccessdocument([FromBody] UpdateData updateData)
        {
            doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels = updateData.doctaskuseracknowledmentstatusmodels;
           // ProvideAccessdocument provideAccessdocumentModels = updateData.ProvideAccessdocumentModels;
            int userlocationmapid = updateData.userlocationmappingid;
            int[] DocPermId = updateData.DocPermId;
            int updatedby = updateData.permissionupdatedby;
            if (doctaskuseracknowledmentstatusmodels.Doc_User_Access_mapping_id == 0)
            {
                //Handle the case where Doc_User_Access_mapping_id is 0 (e.g., insert logic).
            }
            else
            {
                string inputDateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ"; // Input format of the date string
                string outputDateFormat = "yyyy-MM-dd"; // Desired output format for MySQL

                string ParseAndFormatDate(string dateString)
                {
                    if (!string.IsNullOrEmpty(dateString))  // Check if date string is not null or empty
                    {
                        // Attempt to parse the date, return formatted date if successful
                        if (DateTime.TryParseExact(dateString, inputDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime parsedDate))
                        {
                            return parsedDate.ToString(outputDateFormat);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid date format: " + dateString);
                        }
                    }
                    return null;
                }
                var userLocationMapping = mySqlDBContext.userlocationmappingModels
                .Where(x => x.user_location_mapping_id == userlocationmapid)
                 .Select(x => x.USR_ID)
                .FirstOrDefault();

                var existingModel = mySqlDBContext.doctaskuseracknowledmentstatusmodels.FirstOrDefault(model =>
                      model.Doc_User_Access_mapping_id == doctaskuseracknowledmentstatusmodels.Doc_User_Access_mapping_id &&
                      model.user_location_mapping_id == userlocationmapid &&
                        model.AddDoc_id == doctaskuseracknowledmentstatusmodels.AddDoc_id && model.status == "Active");


                // Parse and format all relevant date fields
                existingModel.duedate = ParseAndFormatDate(doctaskuseracknowledmentstatusmodels.duedate);
                //    existingModel.startDate = doctaskuseracknowledmentstatusmodels.startDate;
                existingModel.endDate = ParseAndFormatDate(doctaskuseracknowledmentstatusmodels.endDate);
                existingModel.ack_status = doctaskuseracknowledmentstatusmodels.ack_status;
                existingModel.provideack_status = doctaskuseracknowledmentstatusmodels.ack_status;
                existingModel.validitydocument = doctaskuseracknowledmentstatusmodels.validitydocument;
                existingModel.optionalreminder = doctaskuseracknowledmentstatusmodels.ack_status;
                existingModel.trakstatus = doctaskuseracknowledmentstatusmodels.trakstatus;
                existingModel.everyday = doctaskuseracknowledmentstatusmodels.everyday;
                existingModel.timeperiod = doctaskuseracknowledmentstatusmodels.timeperiod;
                existingModel.reqtimeperiod = doctaskuseracknowledmentstatusmodels.reqtimeperiod;
                existingModel.noofdays = doctaskuseracknowledmentstatusmodels.noofdays;
                existingModel.USR_ID = userLocationMapping;
                existingModel.user_location_mapping_id = userlocationmapid;






                if (userLocationMapping != null)
                {
                    int USR_ID = userLocationMapping;


                    var existingpermEntries = mySqlDBContext.UserPermissionModels.Where(x => x.user_location_mapping_id == userlocationmapid && x.Doc_User_Access_mapping_id == doctaskuseracknowledmentstatusmodels.Doc_User_Access_mapping_id && x.permissionstatus == "Active");
                    mySqlDBContext.UserPermissionModels.RemoveRange(existingpermEntries);

                    mySqlDBContext.SaveChanges();


                    if (existingModel != null)
                    {
                        // Update the existing model's properties
                        existingModel.ack_status = doctaskuseracknowledmentstatusmodels.ack_status;
                        existingModel.createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        existingModel.status = "Active";
                        // existingModel.startDate = provideAccessdocumentModels.startDate;
                        // existingModel.endDate = provideAccessdocumentModels.endDate;
                    }
                    else
                    {

                    }

                    foreach (var permId in DocPermId)
                    {

                        var userPermissionModel = new UserPermissionModel
                        {
                            Doc_User_Access_mapping_id = doctaskuseracknowledmentstatusmodels.Doc_User_Access_mapping_id,
                            Doc_perm_rights_id = permId,
                            user_location_mapping_id = userlocationmapid,
                            USR_ID = USR_ID,
                            ack_status = doctaskuseracknowledmentstatusmodels.ack_status,
                            AddDoc_id = doctaskuseracknowledmentstatusmodels.AddDoc_id,
                            permissionstatus = "Active",
                            permissioncreateddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            permissionupdatedby  = updatedby

                        };
                        mySqlDBContext.UserPermissionModels.Add(userPermissionModel);
                    }


                    var userEmail = mySqlDBContext.usermodels
         .Where(x => x.USR_ID == userLocationMapping)
         .Select(x => x.emailid)
         .FirstOrDefault();

                    var DocumentNames = mySqlDBContext.AddDocumentModels
                        .Where(x => x.AddDoc_id == doctaskuseracknowledmentstatusmodels.AddDoc_id)
                        .Select(x => x.Title_Doc)
                        .ToArray();

                    //  var senderemail = await mySqlDBContext.provideAccessdocumentModels.Where(x => x.Document_Id == documentId).Select(x => x.createdBy).FirstOrDefaultAsync();

                    int senderid = updatedby;
                    var request = HttpContext.Request;
                    string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);


                    obj_Clsmail.editAccessMail(userEmail, DocumentNames, senderid, userLocationMapping, baseUrl);

                    mySqlDBContext.SaveChanges();
                }
            }

          
            
        }

    }
}