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
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using ITR_TelementaryAPI.Models;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Diagnostics;
using static ITRTelemetry.Controllers.RoleController;
using static ITRTelemetry.Controllers.Componentcontroller;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AcknowlwdgementDataController:ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        public IConfiguration Configuration { get; }
        public AcknowlwdgementDataController(MySqlDBContext mySqlDBContext, IConfiguration configuration)

        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }
        [Route("api/Acknowlwdgedata/GetAcknowlwdgedataDetails")]
        [HttpGet]
        public IEnumerable<object> GetAcknowlwdgedataDetails(int USR_ID)

        {



            //      var details = ( from  providePermissionsMaster in mySqlDBContext.provideAccessdocumentModels
            //                      join userPermissionsMaster in mySqlDBContext.UserPermissionModels on providePermissionsMaster.Doc_User_Access_mapping_id equals userPermissionsMaster.Doc_User_Access_mapping_id
            //join addDocumentMaster in mySqlDBContext.AddDocumentModels on providePermissionsMaster.AddDoc_id equals addDocumentMaster.AddDoc_id
            //join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
            //join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
            //join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
            //join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
            //join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
            //join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
            ////join doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels on userPermissionsMaster.USR_ID equals doc_task.USR_ID
            //                      where doc_task.status == "Active" && (doc_task.ack_status == "true" && doc_task.USR_ID == USR_ID)

            var details = (from doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels
                          join userPermissionsMaster in mySqlDBContext.UserPermissionModels on doc_task.USR_ID equals userPermissionsMaster.USR_ID
                          join  providePermissionsMaster in mySqlDBContext.provideAccessdocumentModels on userPermissionsMaster.Doc_User_Access_mapping_id equals providePermissionsMaster.Doc_User_Access_mapping_id
                          join addDocumentMaster in mySqlDBContext.AddDocumentModels on doc_task.AddDoc_id equals addDocumentMaster.AddDoc_id
                          join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
                          join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
                          join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
                          join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
                          join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
                          join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
                          where doc_task.status == "Active" && doc_task.ack_status == "true" && doc_task.USR_ID == USR_ID && addDocumentMaster.addDoc_Status =="Active"

                           select new
                          {
                              providePermissionsMaster.Doc_User_Access_mapping_id,
                              doc_task.AddDoc_id,
                              addDocumentMaster.Title_Doc,
                              natureMaster.NatureOf_Doc_Name,
                              authorityTypeMaster.AuthorityTypeName,
                              authorityMasters.AuthorityName,
                              addDocumentMaster.Keywords_tags,
                              docTypeMaster.docTypeName,
                              docSubcatMaster.Doc_SubCategoryName,
                              docCatMaster.Doc_CategoryName
                          })
                          .Distinct();

                      return details;


        }


        [Route("api/Acknowlwdgedata/GetUserAccessability")]
    [HttpGet]
    public IEnumerable<object> GetUserAccessability(int USR_ID )

    {


            //var details = (from providepermissionsmaster in mySqlDBContext.provideAccessdocumentModels
            //               join userpermissionsmaster in mySqlDBContext.UserPermissionModels on providepermissionsmaster.Doc_User_Access_mapping_id equals userpermissionsmaster.Doc_User_Access_mapping_id
            //               join adddocumentmaster in mySqlDBContext.AddDocumentModels on providepermissionsmaster.AddDoc_id equals adddocumentmaster.AddDoc_id
            //               join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on adddocumentmaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
            //               join authorityMasters in mySqlDBContext.AuthorityNameModels on adddocumentmaster.AuthoritynameID equals authorityMasters.AuthoritynameID
            //               join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on adddocumentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
            //               join Doctypemaster in mySqlDBContext.DocTypeMasterModels on adddocumentmaster.DocTypeID equals Doctypemaster.docTypeID
            //               join Docsubcatmaster in mySqlDBContext.DocSubCategoryModels on adddocumentmaster.Doc_SubCategoryID equals Docsubcatmaster.Doc_SubCategoryID
            //               join Doccatmaster in mySqlDBContext.DocCategoryMasterModels on adddocumentmaster.Doc_CategoryID equals Doccatmaster.Doc_CategoryID
            //               where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active" && providepermissionsmaster.ack_status == "false" || providepermissionsmaster.ack_status == "" || providepermissionsmaster.ack_status == "Acknowledged" && userpermissionsmaster.USR_ID == USR_ID

            var details = (from doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels
                           join userPermissionsMaster in mySqlDBContext.UserPermissionModels on doc_task.USR_ID equals userPermissionsMaster.USR_ID
                           join providePermissionsMaster in mySqlDBContext.provideAccessdocumentModels on userPermissionsMaster.Doc_User_Access_mapping_id equals providePermissionsMaster.Doc_User_Access_mapping_id
                           join addDocumentMaster in mySqlDBContext.AddDocumentModels on doc_task.AddDoc_id equals addDocumentMaster.AddDoc_id
                           join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
                           join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
                           join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
                           join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
                           join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
                           join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
                           where doc_task.status == "Active" && doc_task.ack_status == "false" || doc_task.ack_status == "" || doc_task.ack_status == "Acknowledged" || doc_task.ack_status == "Read Later" && doc_task.USR_ID == USR_ID && addDocumentMaster.addDoc_Status == "Active"

                           select new
                       {
                           providePermissionsMaster.Doc_User_Access_mapping_id,
                           providePermissionsMaster.AddDoc_id,
                           addDocumentMaster.Title_Doc,
                           natureMaster.NatureOf_Doc_Name,
                           authorityTypeMaster.AuthorityTypeName,
                           authorityMasters.AuthorityName,
                           addDocumentMaster.Keywords_tags,
                           docTypeMaster.docTypeName,
                           docSubcatMaster.Doc_SubCategoryName,
                           docCatMaster.Doc_CategoryName,
                           })
                        .Distinct();
                        return details;

    }

        [Route("api/Acknowlwdgedata/GetReadCompletedUserAccessability")]
        [HttpGet]
        public IEnumerable<object> GetReadCompletedUserAccessability(int USR_ID)

        {


            //var details = (from providepermissionsmaster in mySqlDBContext.provideAccessdocumentModels
            //               join userpermissionsmaster in mySqlDBContext.UserPermissionModels on providepermissionsmaster.Doc_User_Access_mapping_id equals userpermissionsmaster.Doc_User_Access_mapping_id
            //               join adddocumentmaster in mySqlDBContext.AddDocumentModels on providepermissionsmaster.AddDoc_id equals adddocumentmaster.AddDoc_id
            //               join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on adddocumentmaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
            //               join authorityMasters in mySqlDBContext.AuthorityNameModels on adddocumentmaster.AuthoritynameID equals authorityMasters.AuthoritynameID
            //               join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on adddocumentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
            //               join Doctypemaster in mySqlDBContext.DocTypeMasterModels on adddocumentmaster.DocTypeID equals Doctypemaster.docTypeID
            //               join Docsubcatmaster in mySqlDBContext.DocSubCategoryModels on adddocumentmaster.Doc_SubCategoryID equals Docsubcatmaster.Doc_SubCategoryID
            //               join Doccatmaster in mySqlDBContext.DocCategoryMasterModels on adddocumentmaster.Doc_CategoryID equals Doccatmaster.Doc_CategoryID
            //               where providepermissionsmaster.Doc_User_Access_mapping_Status == "Active" && providepermissionsmaster.ack_status == "Reading Completed" && userpermissionsmaster.USR_ID == USR_ID
           var details = (from doc_task in mySqlDBContext.doctaskuseracknowledmentstatusmodels
                           join userPermissionsMaster in mySqlDBContext.UserPermissionModels on doc_task.USR_ID equals userPermissionsMaster.USR_ID
                           join providePermissionsMaster in mySqlDBContext.provideAccessdocumentModels on userPermissionsMaster.Doc_User_Access_mapping_id equals providePermissionsMaster.Doc_User_Access_mapping_id
                           join addDocumentMaster in mySqlDBContext.AddDocumentModels on doc_task.AddDoc_id equals addDocumentMaster.AddDoc_id
                           join authorityTypeMaster in mySqlDBContext.AuthorityTypeMasters on addDocumentMaster.AuthorityTypeID equals authorityTypeMaster.AuthorityTypeID
                           join authorityMasters in mySqlDBContext.AuthorityNameModels on addDocumentMaster.AuthoritynameID equals authorityMasters.AuthoritynameID
                           join natureMaster in mySqlDBContext.NatureOf_DocumentMasterModels on addDocumentMaster.NatureOf_Doc_id equals natureMaster.NatureOf_Doc_id
                           join docTypeMaster in mySqlDBContext.DocTypeMasterModels on addDocumentMaster.DocTypeID equals docTypeMaster.docTypeID
                           join docSubcatMaster in mySqlDBContext.DocSubCategoryModels on addDocumentMaster.Doc_SubCategoryID equals docSubcatMaster.Doc_SubCategoryID
                           join docCatMaster in mySqlDBContext.DocCategoryMasterModels on addDocumentMaster.Doc_CategoryID equals docCatMaster.Doc_CategoryID
                           where doc_task.status == "Active" && doc_task.ack_status == "Reading Completed" && doc_task.USR_ID == USR_ID && addDocumentMaster.addDoc_Status == "Active"
                           select new
                           {
                               providePermissionsMaster.Doc_User_Access_mapping_id,
                               providePermissionsMaster.AddDoc_id,
                               addDocumentMaster.Title_Doc,
                               natureMaster.NatureOf_Doc_Name,
                               authorityTypeMaster.AuthorityTypeName,
                               authorityMasters.AuthorityName,
                               addDocumentMaster.Keywords_tags,
                               docTypeMaster.docTypeName,
                               docSubcatMaster.Doc_SubCategoryName,
                               docCatMaster.Doc_CategoryName,

                           })
                            .Distinct();
                           return details;

        }


        //[Route("api/Acknowlwdgedata/UpdateDocStatus/{adddoc_id}")]
        //[HttpPut]
        //public void UpdateDocStatus(int adddoc_id)

        //{
        //        var currentClass = new ProvideAccessdocument { AddDoc_id = adddoc_id };
        //        currentClass.ack_status = "Acknowledged";
        //        this.mySqlDBContext.Entry(currentClass).Property("ack_status").IsModified = true;
        //        this.mySqlDBContext.SaveChanges();

        //}
        //[Route("api/Acknowledgedata/UpdateDocStatus/{adddoc_id}")]
        //[HttpPut]
        //public IActionResult UpdateDocStatus(int adddoc_id, int user_id)
        //{
        //    // Load the existing entity from the database
        //    var existingEntity = this.mySqlDBContext.doctaskuseracknowledmentstatusmodels
        //        .FirstOrDefault(e => e.AddDoc_id == adddoc_id && e.USR_ID == user_id);

        //    try
        //    {
        //        if (existingEntity == null)
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            existingEntity.ack_status = "Acknowledged";
        //            this.mySqlDBContext.SaveChanges();
        //            return Ok();
        //        }
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return StatusCode(409, "Concurrency conflict");
        //    }
        //}



        [Route("api/Acknowlwdgedata/GetAcknowlwdgedataDatabyid")]
        [HttpPost]
        public IEnumerable<ProvideAccessModel> GetAcknowlwdgedataDatabyid([FromBody] doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels)

        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(
    @"SELECT
                e.AuthoritynameID,
                e.AuthorityTypeID,
                e.NatureOf_Doc_id,
                e.DocTypeID,
                e.Doc_SubCategoryID,
                e.Doc_CategoryID,
                e.Title_Doc,
                e.Doc_Confidentiality,
                e.Eff_Date,
                e.Initial_creation_doc_date,
                e.Doc_internal_num,
                e.Doc_Inter_ver_num,
                e.Doc_Phy_Valut_Loc,
                e.Doc_process_Owner,
                e.Doc_Approver,
                e.Date_Doc_Revision,
                e.Date_Doc_Approver,
                e.freq_period,
                e.Keywords_tags,
                e.pub_doc,
                e.publisher_comments,
                e.document_name,
                e.Linking_Doc_names,
                e.indicative_reading_time,
                e.Time_period,
                e.review_start_Date,
                e.freq_period_type,
                e.Sub_title_doc,
                e.AddDoc_id,
                e.OtpMethod,
                e.Obj_Doc,
                e.Review_Frequency_Status,
                e.Doc_Linking_Status,
                e.Document_Id,
                e.VersionControlNo,
                b.firstname,
                e.addDoc_createdDate,
                t.DocTypeName,
                C.Doc_CategoryName,
                sc.Doc_SubCategoryName,
                a.AuthorityName,
                at.AuthorityTypeName,
                p.NatureOf_Doc_Name,
                 ack.Favorite,
                ack.startDate,
                ack.endDate
            FROM
                risk.add_doc e
            LEFT OUTER JOIN
                risk.doctype_master t ON t.DocTypeID = e.DocTypeID
            LEFT OUTER JOIN
                risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID
            LEFT OUTER JOIN
                risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID
            LEFT OUTER JOIN
                risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID
            LEFT OUTER JOIN
                risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID
            LEFT OUTER JOIN
                risk.tblUser b ON b.USR_ID = e.USR_ID
            LEFT OUTER JOIN
                risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id
            Left Outer JOIN 
                risk.doc_taskuseracknowledment_status ack ON ack.AddDoc_id = e.AddDoc_id
            WHERE
                e.AddDoc_id = @AddDoc_id && ack.USR_ID = @USR_ID", con);

            // Assuming AddDoc_id is a parameter, add it to the MySqlCommand.Parameters collection doc_user_permission_mapping
            cmd.Parameters.AddWithValue("@AddDoc_id", doctaskuseracknowledmentstatusmodels.AddDoc_id);
            cmd.Parameters.AddWithValue("@USR_ID", doctaskuseracknowledmentstatusmodels.USR_ID);

            //" Left Outer JOIN " +
            // "risk.doc_taskuseracknowledment_status ack ON ack.USR_ID = e.USR_ID" +
            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<ProvideAccessModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    int Review_Frequency_Status;
                    int Doc_Linking_Status;
                    if (dt.Rows[i]["Review_Frequency_Status"] != null && !string.IsNullOrEmpty(dt.Rows[i]["Review_Frequency_Status"].ToString()))
                    {
                        int.TryParse(dt.Rows[i]["Review_Frequency_Status"].ToString(), out Review_Frequency_Status);
                    }
                    else
                    {
                        Review_Frequency_Status = 0;
                    }

                    // Check and convert Doc_Linking_Status
                    if (dt.Rows[i]["Doc_Linking_Status"] != null && !string.IsNullOrEmpty(dt.Rows[i]["Doc_Linking_Status"].ToString()))
                    {
                        int.TryParse(dt.Rows[i]["Doc_Linking_Status"].ToString(), out Doc_Linking_Status);
                    }
                    else
                    {
                        Doc_Linking_Status = 0;
                    }
                    pdata.Add(new ProvideAccessModel
                    {
                        // AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),
                        // AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        // Document_Id = dt.Rows[i]["Document_Id"].ToString(),
                        // VersionControlNo = dt.Rows[i]["VersionControlNo"].ToString(),
                        // DocTypeID = Convert.ToInt32(dt.Rows[i]["DocTypeID"].ToString()),
                        // AuthoritynameID = Convert.ToInt32(dt.Rows[i]["AuthoritynameID"].ToString()),
                        // AuthorityTypeID = Convert.ToInt32(dt.Rows[i]["AuthorityTypeID"].ToString()),
                        // NatureOf_Doc_id = Convert.ToInt32(dt.Rows[i]["NatureOf_Doc_id"].ToString()),
                        // Doc_CategoryID = Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"].ToString()),
                        // Doc_SubCategoryID = Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"].ToString()),
                        // AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
                        // Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        // Sub_title_doc = dt.Rows[i]["Sub_title_doc"].ToString(),
                        // Obj_Doc = dt.Rows[i]["Obj_Doc"].ToString(),
                        // // document_name = dt.Rows[i]["document_name"].ToString(),
                        // Linking_Doc_names = dt.Rows[i]["Linking_Doc_names"].ToString(),

                        // NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        // DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        // Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        // Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        // Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"].ToString(),
                        // OtpMethod = dt.Rows[i]["OtpMethod"].ToString(),
                        // Eff_Date = Convert.ToDateTime(dt.Rows[i]["Eff_Date"]).ToString("yyyy-MM-dd"),
                        // Initial_creation_doc_date = Convert.ToDateTime(dt.Rows[i]["Initial_creation_doc_date"]).ToString("yyyy-MM-dd"),
                        // Date_Doc_Revision = Convert.ToDateTime(dt.Rows[i]["Date_Doc_Revision"]).ToString("yyyy-MM-dd"),
                        // //Eff_Date = dt.Rows[i]["Eff_Date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Eff_Date"]).ToString("yyyy-MM-dd") : null,
                        // //Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Initial_creation_doc_date"]).ToString("yyyy-MM-dd") : null,
                        // //Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Date_Doc_Revision"]).ToString("yyyy-MM-dd") : null,

                        // Date_Doc_Approver = Convert.ToDateTime(dt.Rows[i]["Date_Doc_Approver"]).ToString("yyyy-MM-dd"),
                        // review_start_Date = Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("yyyy-MM-dd"),
                        // //review_start_Date = dt.Rows[i]["review_start_Date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("yyyy-MM-dd") : "",

                        // // Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"].ToString(),
                        // Doc_internal_num = dt.Rows[i]["Doc_internal_num"].ToString(),
                        // Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"].ToString(),
                        // Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString(),
                        // Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"].ToString(),
                        // Doc_Approver = dt.Rows[i]["Doc_Approver"].ToString(),
                        // // Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"].ToString(),

                        // // Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"].ToString(),
                        // // review_start_Date = dt.Rows[i]["review_start_Date"].ToString(),
                        // freq_period_type = dt.Rows[i]["freq_period_type"].ToString(),
                        // freq_period = dt.Rows[i]["freq_period"].ToString(),
                        // Keywords_tags = dt.Rows[i]["Keywords_tags"].ToString(),
                        // pub_doc = dt.Rows[i]["pub_doc"].ToString(),
                        // firstname = dt.Rows[i]["firstname"].ToString(),
                        // //CREATED_DATE = dt.Rows[i]["addDoc_createdDate"].ToString(),
                        // CREATED_DATE = Convert.ToDateTime(dt.Rows[i]["addDoc_createdDate"]).ToString("yyyy-MM-dd"),
                        // publisher_comments = dt.Rows[i]["publisher_comments"].ToString(),
                        // indicative_reading_time = dt.Rows[i]["indicative_reading_time"].ToString(),
                        // Time_period = dt.Rows[i]["Time_period"].ToString(),
                        // //publish_Name = dt.Rows[i]["publish_Name"].ToString(),
                        // Review_Frequency_Status = Review_Frequency_Status,
                        // Doc_Linking_Status = Doc_Linking_Status,
                        // favorite = Convert.ToBoolean(dt.Rows[i]["Favorite"]),
                        // //startdate = dt.Rows[i]["startDate"].ToString(),
                        // //enddate = dt.Rows[i]["endDate"].ToString(),
                        // startdate = dt.Rows[i]["startDate"] != DBNull.Value ? dt.Rows[i]["startDate"].ToString() : null,
                        //enddate = dt.Rows[i]["endDate"] != DBNull.Value ? dt.Rows[i]["endDate"].ToString() : null,
                        AddDoc_id = dt.Rows[i]["AddDoc_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["AddDoc_id"]) : default(int),
                        AuthorityName = dt.Rows[i]["AuthorityName"] != DBNull.Value ? dt.Rows[i]["AuthorityName"].ToString() : null,
                        Document_Id = dt.Rows[i]["Document_Id"] != DBNull.Value ? dt.Rows[i]["Document_Id"].ToString() : null,
                        VersionControlNo = dt.Rows[i]["VersionControlNo"] != DBNull.Value ? dt.Rows[i]["VersionControlNo"].ToString() : null,
                        DocTypeID = dt.Rows[i]["DocTypeID"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["DocTypeID"]) : default(int),
                        AuthoritynameID = dt.Rows[i]["AuthoritynameID"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["AuthoritynameID"]) : default(int),
                        AuthorityTypeID = dt.Rows[i]["AuthorityTypeID"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["AuthorityTypeID"]) : default(int),
                        NatureOf_Doc_id = dt.Rows[i]["NatureOf_Doc_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["NatureOf_Doc_id"]) : default(int),
                        Doc_CategoryID = dt.Rows[i]["Doc_CategoryID"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["Doc_CategoryID"]) : default(int),
                        Doc_SubCategoryID = dt.Rows[i]["Doc_SubCategoryID"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["Doc_SubCategoryID"]) : default(int),
                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"] != DBNull.Value ? dt.Rows[i]["AuthorityTypeName"].ToString() : null,
                        Title_Doc = dt.Rows[i]["Title_Doc"] != DBNull.Value ? dt.Rows[i]["Title_Doc"].ToString() : null,
                        Sub_title_doc = dt.Rows[i]["Sub_title_doc"] != DBNull.Value ? dt.Rows[i]["Sub_title_doc"].ToString() : null,
                        Obj_Doc = dt.Rows[i]["Obj_Doc"] != DBNull.Value ? dt.Rows[i]["Obj_Doc"].ToString() : null,
                        // document_name = dt.Rows[i]["document_name"] != DBNull.Value ? dt.Rows[i]["document_name"].ToString() : null,
                        Linking_Doc_names = dt.Rows[i]["Linking_Doc_names"] != DBNull.Value ? dt.Rows[i]["Linking_Doc_names"].ToString() : null,

                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"] != DBNull.Value ? dt.Rows[i]["NatureOf_Doc_Name"].ToString() : null,
                        DocTypeName = dt.Rows[i]["DocTypeName"] != DBNull.Value ? dt.Rows[i]["DocTypeName"].ToString() : null,
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"] != DBNull.Value ? dt.Rows[i]["Doc_CategoryName"].ToString() : null,
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"] != DBNull.Value ? dt.Rows[i]["Doc_SubCategoryName"].ToString() : null,
                        Doc_Confidentiality = dt.Rows[i]["Doc_Confidentiality"] != DBNull.Value ? dt.Rows[i]["Doc_Confidentiality"].ToString() : null,
                        OtpMethod = dt.Rows[i]["OtpMethod"] != DBNull.Value ? dt.Rows[i]["OtpMethod"].ToString() : null,
                        Eff_Date = dt.Rows[i]["Eff_Date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Eff_Date"]).ToString("yyyy-MM-dd") : null,
                        Initial_creation_doc_date = dt.Rows[i]["Initial_creation_doc_date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Initial_creation_doc_date"]).ToString("yyyy-MM-dd") : null,
                        Date_Doc_Revision = dt.Rows[i]["Date_Doc_Revision"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Date_Doc_Revision"]).ToString("yyyy-MM-dd") : null,

                        Date_Doc_Approver = dt.Rows[i]["Date_Doc_Approver"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["Date_Doc_Approver"]).ToString("yyyy-MM-dd") : null,
                        review_start_Date = dt.Rows[i]["review_start_Date"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["review_start_Date"]).ToString("yyyy-MM-dd") : null,

                        Doc_internal_num = dt.Rows[i]["Doc_internal_num"] != DBNull.Value ? dt.Rows[i]["Doc_internal_num"].ToString() : null,
                        Doc_Inter_ver_num = dt.Rows[i]["Doc_Inter_ver_num"] != DBNull.Value ? dt.Rows[i]["Doc_Inter_ver_num"].ToString() : null,
                        Doc_Phy_Valut_Loc = dt.Rows[i]["Doc_Phy_Valut_Loc"] != DBNull.Value ? dt.Rows[i]["Doc_Phy_Valut_Loc"].ToString() : null,
                        Doc_process_Owner = dt.Rows[i]["Doc_process_Owner"] != DBNull.Value ? dt.Rows[i]["Doc_process_Owner"].ToString() : null,
                        Doc_Approver = dt.Rows[i]["Doc_Approver"] != DBNull.Value ? dt.Rows[i]["Doc_Approver"].ToString() : null,

                        freq_period_type = dt.Rows[i]["freq_period_type"] != DBNull.Value ? dt.Rows[i]["freq_period_type"].ToString() : null,
                        freq_period = dt.Rows[i]["freq_period"] != DBNull.Value ? dt.Rows[i]["freq_period"].ToString() : null,
                        Keywords_tags = dt.Rows[i]["Keywords_tags"] != DBNull.Value ? dt.Rows[i]["Keywords_tags"].ToString() : null,
                        pub_doc = dt.Rows[i]["pub_doc"] != DBNull.Value ? dt.Rows[i]["pub_doc"].ToString() : null,
                        firstname = dt.Rows[i]["firstname"] != DBNull.Value ? dt.Rows[i]["firstname"].ToString() : null,
                        // CREATED_DATE = dt.Rows[i]["addDoc_createdDate"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["addDoc_createdDate"]).ToString("yyyy-MM-dd") : null,
                        CREATED_DATE = dt.Rows[i]["addDoc_createdDate"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[i]["addDoc_createdDate"]).ToString("yyyy-MM-dd") : null,
                        publisher_comments = dt.Rows[i]["publisher_comments"] != DBNull.Value ? dt.Rows[i]["publisher_comments"].ToString() : null,
                        indicative_reading_time = dt.Rows[i]["indicative_reading_time"] != DBNull.Value ? dt.Rows[i]["indicative_reading_time"].ToString() : null,
                        Time_period = dt.Rows[i]["Time_period"] != DBNull.Value ? dt.Rows[i]["Time_period"].ToString() : null,
                        // publish_Name = dt.Rows[i]["publish_Name"] != DBNull.Value ? dt.Rows[i]["publish_Name"].ToString() : null,
                        Review_Frequency_Status = Review_Frequency_Status,
                        Doc_Linking_Status = Doc_Linking_Status,
                        favorite = dt.Rows[i]["Favorite"] != DBNull.Value ? Convert.ToBoolean(dt.Rows[i]["Favorite"]) : default(bool),
                        startdate = dt.Rows[i]["startDate"] != DBNull.Value ? dt.Rows[i]["startDate"].ToString() : null,
                        enddate = dt.Rows[i]["endDate"] != DBNull.Value ? dt.Rows[i]["endDate"].ToString() : null,




                    });
                }
            }
            return pdata;

        }

        [Route("api/Acknowlwdgedata/GetDocVersions")]
        [HttpGet]
        public IEnumerable<object> GetDocVersions([FromQuery] int AddDocId, string document_Id)
        {
            List<VersionDetail> versions = new List<VersionDetail>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
                {
                    con.Open();

                    string selectVersionsQuery = @"
                SELECT DISTINCT VersionControlNo, Doc_rev_map_createdDate
                FROM document_revision_mapping
                WHERE AddDoc_id = @AddDocId AND Document_Id = @DocumentId";

                    using (MySqlCommand command = new MySqlCommand(selectVersionsQuery, con))
                    {
                        command.Parameters.AddWithValue("@AddDocId", AddDocId);
                        command.Parameters.AddWithValue("@DocumentId", document_Id);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                VersionDetail versionDetail = new VersionDetail
                                {
                                    VersionControlNo = reader["VersionControlNo"].ToString(),
                                    DocRevMapCreatedDate = Convert.ToDateTime(reader["Doc_rev_map_createdDate"])
                                };

                                versions.Add(versionDetail);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception (logging, rethrowing, etc.)
                    Console.WriteLine("An error occurred: " + ex.Message);
                }


            return versions;
        }

        [Route("api/Acknowlwdgedata/GetPermissionRights")]
        [HttpGet]
        //public IEnumerable<ProvideAccessModel> GetPermissionRights([FromBody] doctaskuseracknowledmentstatusmodel doctaskuseracknowledmentstatusmodels)
        //public IEnumerable<object> GetPermissionRights([FromQuery] int AddDocId, int userID, string document_Id)
        //{
        //    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

        //    try
        //    {
        //        con.Open();

        //        string selectMappingIDsQuery = "SELECT Doc_User_Access_mapping_id FROM doc_user_access_mapping WHERE AddDoc_id = @AddDocId AND Document_Id = @DocumentId";
        //        string selectPermissionRightsIDsQuery = "SELECT Doc_perm_rights_id FROM doc_user_permission_mapping WHERE AddDoc_id = @AddDocId AND USR_ID = @UserId AND Doc_User_Access_mapping_id = @MappingId";
        //        string selectPermissionNamesQuery = "SELECT publish_Name FROM doc_perm_rights WHERE Doc_perm_rights_id = @PermissionRightsId"; 

        //        PermissionsMapping mappingDetails = new PermissionsMapping();

        //        // Step 1: Retrieve the mapping IDs
        //        List<string> mappingIds = new List<string>();

        //        using (MySqlCommand command = new MySqlCommand(selectMappingIDsQuery, con))
        //        {
        //            command.Parameters.AddWithValue("@AddDocId", AddDocId);
        //            command.Parameters.AddWithValue("@DocumentId", document_Id);

        //            using (MySqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    mappingIds.Add(reader["Doc_User_Access_mapping_id"].ToString());
        //                }
        //            }
        //        }

        //        // Step 2: Retrieve the permission rights IDs using the mapping IDs
        //        List<string> permissionRightsIds = new List<string>();

        //        foreach (var mappingId in mappingIds)
        //        {
        //            using (MySqlCommand command = new MySqlCommand(selectPermissionRightsIDsQuery, con))
        //            {
        //                command.Parameters.AddWithValue("@AddDocId", AddDocId);
        //                command.Parameters.AddWithValue("@UserId", userID);
        //                command.Parameters.AddWithValue("@MappingId", mappingId);

        //                using (MySqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        permissionRightsIds.Add(reader["Doc_perm_rights_id"].ToString());
        //                    }
        //                }
        //            }
        //        }

        //        // Step 3: Retrieve the permission names using the permission rights IDs
        //        List<string> permissionNames = new List<string>();

        //        foreach (var permissionRightsId in permissionRightsIds)
        //        {
        //            using (MySqlCommand command = new MySqlCommand(selectPermissionNamesQuery, con))
        //            {
        //                command.Parameters.AddWithValue("@PermissionRightsId", permissionRightsId);

        //                using (MySqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        permissionNames.Add(reader["publish_Name"].ToString());
        //                    }
        //                }
        //            }
        //        }

        //        // Store the results in mappingDetails
        //        mappingDetails.ids = string.Join(",", mappingIds);
        //        mappingDetails.permissionRightsIds = permissionRightsIds;
        //        mappingDetails.permissionNames = permissionNames;

        //        // Process or return mappingDetails as needed
        //        return mappingDetails;
        //    }

        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("An error occurred while fetching Components List", ex);
        //    }
        //}
        public PermissionsMapping GetPermissionRights([FromQuery] int AddDocId, int userID, string document_Id)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {
                con.Open();

                string selectMappingIDsQuery = "SELECT Doc_User_Access_mapping_id FROM doc_user_access_mapping WHERE AddDoc_id = @AddDocId AND Document_Id = @DocumentId";
                string selectPermissionRightsIDsQuery = "SELECT Doc_perm_rights_id FROM doc_user_permission_mapping WHERE AddDoc_id = @AddDocId AND USR_ID = @UserId AND Doc_User_Access_mapping_id = @MappingId AND permissionstatus = 'Active' ";
                string selectPermissionNamesQuery = "SELECT publish_Name FROM doc_perm_rights WHERE Doc_perm_rights_id = @PermissionRightsId";

                PermissionsMapping mappingDetails = new PermissionsMapping
                {
                    permissionRightsIds = new List<string>(),
                    permissionNames = new List<string>()
                };

                // Step 1: Retrieve the mapping IDs
                List<string> mappingIds = new List<string>();

                using (MySqlCommand command = new MySqlCommand(selectMappingIDsQuery, con))
                {
                    command.Parameters.AddWithValue("@AddDocId", AddDocId);
                    command.Parameters.AddWithValue("@DocumentId", document_Id);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            mappingIds.Add(reader["Doc_User_Access_mapping_id"].ToString());
                        }
                    }
                }

                // Step 2: Retrieve the permission rights IDs using the mapping IDs
                foreach (var mappingId in mappingIds)
                {
                    using (MySqlCommand command = new MySqlCommand(selectPermissionRightsIDsQuery, con))
                    {
                        command.Parameters.AddWithValue("@AddDocId", AddDocId);
                        command.Parameters.AddWithValue("@UserId", userID);
                        command.Parameters.AddWithValue("@MappingId", mappingId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mappingDetails.permissionRightsIds.Add(reader["Doc_perm_rights_id"].ToString());
                            }
                        }
                    }
                }

                // Step 3: Retrieve the permission names using the permission rights IDs
                foreach (var permissionRightsId in mappingDetails.permissionRightsIds)
                {
                    using (MySqlCommand command = new MySqlCommand(selectPermissionNamesQuery, con))
                    {
                        command.Parameters.AddWithValue("@PermissionRightsId", permissionRightsId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mappingDetails.permissionNames.Add(reader["publish_Name"].ToString());
                            }
                        }
                    }
                }

                // Store the results in mappingDetails
                mappingDetails.ids = string.Join(",", mappingIds);

                // Return the mappingDetails object
                return mappingDetails;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching Permission Rights", ex);
            }
        }

        public class ProvideAccessdocument
        {
            public int AddDoc_id { get; set; }
            public string ack_status { get; set; }

            // Add other properties if any

            [Timestamp]
            public byte[] RowVersion { get; set; }
        }
        public class PermissionsMapping
        {
            public string ids { get; set; }
            public List<string> permissionRightsIds { get; set; }
            public List<string> permissionNames { get; set; } // Add property for permission names
        }

        public class VersionDetail
        {
            public string VersionControlNo { get; set; }
            public DateTime DocRevMapCreatedDate { get; set; }
        }
        public class VersionsDetails
        {
            public List<VersionDetail> Versions { get; set; } = new List<VersionDetail>();
        }
    }
}
