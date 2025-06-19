using DomainModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Permissions;
using System.Net.Mail;
using System.Net;
using ITR_TelementaryAPI;
using Microsoft.Extensions.Logging;
using System.Security;
using OpenXmlPowerTools;
using NuGet.Protocol.Plugins;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class provideAccessdocumentController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        private ClsEmail obj_Clsmail = new ClsEmail();
        private IEnumerable<object> doc_User_Access_mapping_id;
        public IConfiguration Configuration { get; }

        public provideAccessdocumentController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/ProvideAccessdocument/GetProvideAccessDetails")]
        [HttpGet]

        public IEnumerable<object> GetProvideAccessDetails()
        {

            var excludedDocumentIds = mySqlDBContext.AddDocumentModels
       .GroupBy(ad => ad.Document_Id)
       .Where(group => group.Count() > 1)
       .Select(group => group.Key)
       .ToList();

            var currentDate = DateTime.Now;
            string validationStatus;
            var details = (from documentmaster in mySqlDBContext.AddDocumentModels
                           join doctypemaster in mySqlDBContext.DocTypeMasterModels on documentmaster.DocTypeID equals doctypemaster.docTypeID
                           join doccategorymaster in mySqlDBContext.DocCategoryMasterModels on documentmaster.Doc_CategoryID equals doccategorymaster.Doc_CategoryID
                           join docsubcategorymaster in mySqlDBContext.DocSubCategoryModels on documentmaster.Doc_SubCategoryID equals docsubcategorymaster.Doc_SubCategoryID

                           join authoritypemaster in mySqlDBContext.AuthorityTypeMasters on documentmaster.AuthorityTypeID equals authoritypemaster.AuthorityTypeID

                           join authoritynamemaster in mySqlDBContext.AuthorityNameModels on documentmaster.AuthoritynameID equals authoritynamemaster.AuthoritynameID

                           //  join entitymaster in mySqlDBContext.UnitMasterModels on documentmaster.Entity_Master_id equals entitymaster.Entity_Master_id

                           //  join unitmaster in mySqlDBContext.UnitLocationMasterModels on documentmaster.Unit_location_Master_id equals unitmaster.Unit_location_Master_id

                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on documentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where
                                 documentmaster.addDoc_Status == "Active" &&
                                 documentmaster.Draft_Status == "Completed"

                           select new
                           {
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                               doctypemaster.docTypeName,
                               doccategorymaster.Doc_CategoryName,
                               docsubcategorymaster.Doc_SubCategoryName,
                               authoritynamemaster.AuthorityName,
                               authoritypemaster.AuthorityTypeName,
                               naturemaster.NatureOf_Doc_Name,
                               documentmaster.Doc_Confidentiality,
                               documentmaster.VersionControlNo,
                               // EntityName = entitymaster.Entity_Master_Name, // Replace with the actual property name
                               // UnitLocationName = unitmaster.Unit_location_Master_name, // Replace with the actual property name
                               ReviewStartDate = documentmaster.review_start_Date, // Include the review start date in the result

                           })
                           .Distinct()
                           .ToList()
            .Select(detail => new
            {
                detail.Title_Doc,
                detail.AddDoc_id,
                detail.Document_Id,
                detail.docTypeName,
                detail.Doc_CategoryName,
                detail.Doc_SubCategoryName,
                detail.AuthorityName,
                detail.AuthorityTypeName,
                detail.NatureOf_Doc_Name,
                detail.Doc_Confidentiality,
                detail.VersionControlNo,
                // detail.EntityName,
                // detail.UnitLocationName,
                detail.ReviewStartDate,
                Validation = CalculateValidationStatus(detail.ReviewStartDate, currentDate)
            })
               .ToList();



            // Function to calculate validation status
            string CalculateValidationStatus(DateTime? reviewStartDate, DateTime currentDate)
            {
                if (reviewStartDate.HasValue)
                {
                    TimeSpan span = reviewStartDate.Value - currentDate;
                    int NoofDays = span.Days;

                    if (NoofDays >= 0 && NoofDays <= 30)
                    {
                        return "Take Immediate Action";
                    }
                    else if (NoofDays < 0)
                    {
                        return "Expired";
                    }
                    else if (NoofDays >= 31 && NoofDays <= 60)
                    {
                        return "Expiring Soon";
                    }
                    else if (NoofDays >= 60)
                    {
                        return "Not Due";
                    }
                }

                return "No Date"; // Handle other cases as needed
            }

            return details;




        }

        [Route("api/ProvideAccessdocument/InsertProvideAccessdocumentDetails")]
        [HttpPost]

        public async Task<IActionResult> InsertParameter([FromForm] ProvideAccessdocument provideAccessdocumentModels,[FromForm] int[] Doc_id, [FromForm] int[] user_location_mapping_id, [FromForm] int[] doc_perm_rights_id, [FromForm] string ack_status,  [FromForm] string duedate, [FromForm] string validitydocument, [FromForm] string startDate, [FromForm] string endDate, [FromForm] string optionalreminder, [FromForm] int noofdays, [FromForm] string timeperiod, [FromForm] string trakstatus, [FromForm] int everyday, [FromForm] string reqtimeperiod)

        {
            try
            {

                var userDocumentInfo = new Dictionary<int, List<string>>();
                foreach (var docid in Doc_id)
                {
                    var documentId = await mySqlDBContext.AddDocumentModels
                            .Where(x => x.AddDoc_id == docid)
                            .Select(x => (x.Document_Id))
                            .FirstOrDefaultAsync();
                    var ackstatus = ack_status;
                    var dued = duedate;
                    var validity = validitydocument;
                    var start = startDate;
                    var end = endDate;
                    var optional = optionalreminder;
                    var noof = noofdays;
                    var time = timeperiod;
                    var tract = trakstatus;
                    int every = everyday;
                    var req = reqtimeperiod;
                  

                    var existingAccessDocuments = await mySqlDBContext.provideAccessdocumentModels
                    .Where(x => x.AddDoc_id == docid && x.Entity_Master_id == provideAccessdocumentModels.Entity_Master_id && x.Unit_location_Master_id == provideAccessdocumentModels.Unit_location_Master_id && x.Doc_User_Access_mapping_Status == "Active")
                     .Select(x => x.Doc_User_Access_mapping_id ).ToListAsync();

                    int docUserAccessMappingId = existingAccessDocuments.FirstOrDefault();
                    

                    if (!existingAccessDocuments.Any())

                    {
                        var ProvideAccessdocument = this.mySqlDBContext.provideAccessdocumentModels;
                        // Populate provideAccessdocumentModels properties as neededProvideAccessdocument
                        DateTime dt = DateTime.Now;
                        string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                        provideAccessdocumentModels.Doc_User_Access_mapping_createdDate = dt1;
                        provideAccessdocumentModels.Doc_User_Access_mapping_Status = "Active";
                        provideAccessdocumentModels.AddDoc_id = docid;
                        // provideAccessdocumentModels.Entity_Master_id = provideAccessdocumentModels.Entity_Master_id;
                        // provideAccessdocumentModels.Unit_location_Master_id = provideAccessdocumentModels.Unit_location_Master_id;
                        provideAccessdocumentModels.Document_Id = documentId;
                        provideAccessdocumentModels.Doc_User_Access_mapping_id = 0;

                        mySqlDBContext.provideAccessdocumentModels.Add(provideAccessdocumentModels);
                        await mySqlDBContext.SaveChangesAsync();



                        // Provoide Access first Table comleted//-----------------------------------------
                        // get Userid
                        var userIds = await mySqlDBContext.userlocationmappingModels
                            .Where(x => user_location_mapping_id.Contains(x.user_location_mapping_id))
                            .Select(x => x.USR_ID)
                            .ToListAsync();

                        // get peremissions and ackstatus

                        var doctaskuseracknowledmentstatusmodels = new List<doctaskuseracknowledmentstatusmodel>();
                        var userPermissionModels = new List<UserPermissionModel>();

                        foreach (var pkid in user_location_mapping_id)
                        {


                            var userId = await mySqlDBContext.userlocationmappingModels
                            .Where(x => x.user_location_mapping_id == pkid)
                            .Select(x => x.USR_ID)
                             .FirstOrDefaultAsync();



                            var existingPermissions = await mySqlDBContext.UserPermissionModels
                                .Where(x => x.AddDoc_id == docid && x.USR_ID == userId)
                                .ToListAsync();


                       
                         

                            if (userId != default(int))
                            {

                             
                         

                                var doctaskuseracknowledmentstatusmodel = new doctaskuseracknowledmentstatusmodel
                                {
                                    Doc_User_Access_mapping_id = provideAccessdocumentModels.Doc_User_Access_mapping_id,
                                    USR_ID = userId,
                                    user_location_mapping_id = pkid,
                                    AddDoc_id = provideAccessdocumentModels.AddDoc_id,
                                    Document_Id = provideAccessdocumentModels.Document_Id,
                                    ack_status = ackstatus,
                                    duedate = dued,
                                    validitydocument = validity,
                                    startDate = start,
                                    endDate = end,
                                    optionalreminder = optional,
                                    noofdays = noof,
                                    timeperiod = time,
                                    trakstatus = tract,
                                    everyday  = every,
                                    reqtimeperiod = req,
                                    createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    status = "Active",
                                    provideack_status = ackstatus
                                };

                                doctaskuseracknowledmentstatusmodels.Add(doctaskuseracknowledmentstatusmodel);

                                foreach (var permId in doc_perm_rights_id)
                                {
                                    var userPermissionModel = new UserPermissionModel
                                    {

                                        Doc_User_Access_mapping_id = provideAccessdocumentModels.Doc_User_Access_mapping_id,
                                        Doc_perm_rights_id = permId,
                                        USR_ID = userId,
                                        user_location_mapping_id = pkid,
                                        AddDoc_id = provideAccessdocumentModels.AddDoc_id,
                                        permissionstatus = "Active",
                                        permissioncreateddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        ack_status = ackstatus,
                                        permissioncreatedby =provideAccessdocumentModels.createdBy,

                                    };
                                    userPermissionModels.Add(userPermissionModel);
                                }
                            }

                        }

                        mySqlDBContext.doctaskuseracknowledmentstatusmodels.AddRange(doctaskuseracknowledmentstatusmodels);
                        mySqlDBContext.UserPermissionModels.AddRange(userPermissionModels);
                        await mySqlDBContext.SaveChangesAsync();




                        foreach (var userId in userIds)
                        {
                            var userEmail = await mySqlDBContext.usermodels
                                .Where(x => x.USR_ID == userId)
                                .Select(x => x.emailid)
                                .FirstOrDefaultAsync();

                            var DocumentNames = await mySqlDBContext.AddDocumentModels
                                .Where(x => x.AddDoc_id == docid)
                                .Select(x => x.Title_Doc)
                                .ToListAsync();

                            if (!userDocumentInfo.ContainsKey(userId))
                            {
                                userDocumentInfo[userId] = new List<string>();
                            }

                            // Add document titles for the current user
                            userDocumentInfo[userId].AddRange(DocumentNames);
                        }

                        var addDocument = await mySqlDBContext.AddDocumentModels.FirstOrDefaultAsync(x => x.AddDoc_id == docid);

                        if (addDocument != null)
                        {
                            addDocument.status_permission = "Permission";
                            await mySqlDBContext.SaveChangesAsync();
                        }


                        // Send emails with combined document information
                        //foreach (var (userId, documentNames) in userDocumentInfo)
                        //{
                        //    var userEmail = await mySqlDBContext.usermodels
                        //        .Where(x => x.USR_ID == userId)
                        //        .Select(x => x.emailid)
                        //        .FirstOrDefaultAsync();
                        //    // var senderemail = await mySqlDBContext.provideAccessdocumentModels.Where(x => x.Document_Id == documentId).Select(x => x.createdBy).FirstOrDefaultAsync();
                        //    var request = HttpContext.Request;
                        //    string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                        //    int senderid = provideAccessdocumentModels.createdBy;
                        //    if (documentNames.Any())
                        //    {
                        //        obj_Clsmail.ProvideAccessMail(userEmail, documentNames.ToArray(), senderid, userId, baseUrl);
                        //    }
                        //}

                        //return Ok();
                    }

                    else
                    {

                        // Provoide Access first Table comleted//-----------------------------------------
                        // get Userid
                        var userIds = await mySqlDBContext.userlocationmappingModels
                            .Where(x => user_location_mapping_id.Contains(x.user_location_mapping_id))
                            .Select(x => x.USR_ID)
                            .ToListAsync();

                        // get peremissions and ackstatus

                        var doctaskuseracknowledmentstatusmodels = new List<doctaskuseracknowledmentstatusmodel>();
                        var userPermissionModels = new List<UserPermissionModel>();

                        foreach (var pkid in user_location_mapping_id)
                        {
                            //var userId = userIds.FirstOrDefault(id => id == pkid);
                            //  var userId = await mySqlDBContext.userlocationmappingModels
                            //.Where(x => user_location_mapping_id.Contains(x.user_location_mapping_id))
                            //.Select(x => x.USR_ID)
                            //.ToListAsync();

                            var userId = await mySqlDBContext.userlocationmappingModels
                            .Where(x => x.user_location_mapping_id == pkid)
                            .Select(x => x.USR_ID)
                             .FirstOrDefaultAsync();



                            var existingPermissions = await mySqlDBContext.UserPermissionModels
                                .Where(x => x.AddDoc_id == docid && x.USR_ID == userId)
                                .ToListAsync();


                            if (existingPermissions.Any())
                            {
                                foreach (var permission in existingPermissions)
                                {
                                    permission.permissionstatus = "Inactive";
                                }

                                // Save changes to update the status of existing permissions
                                await mySqlDBContext.SaveChangesAsync();
                            }
                            var existingrecord = await mySqlDBContext.doctaskuseracknowledmentstatusmodels
                                  .Where(x => x.AddDoc_id == docid && x.USR_ID == userId && x.status == "Active")
                                  .ToListAsync();
                            if (existingrecord.Any())
                            {
                                foreach (var record in existingrecord)
                                {
                                    record.status = "Inactive";
                                }


                                // Save changes to update the status of existing records
                                await mySqlDBContext.SaveChangesAsync();
                            }

                            if (userId != default(int))
                            {



                                var doctaskuseracknowledmentstatusmodel = new doctaskuseracknowledmentstatusmodel
                                {
                                    Doc_User_Access_mapping_id = docUserAccessMappingId,
                                    USR_ID = userId,
                                    user_location_mapping_id = pkid,
                                    AddDoc_id = docid,
                                    Document_Id = documentId,
                                    ack_status = ackstatus,
                                    duedate = dued,
                                    validitydocument = validity,
                                    startDate = start,
                                    endDate = end,
                                    optionalreminder = optional,
                                    noofdays = noof,
                                    timeperiod = time,
                                    trakstatus = tract,
                                    everyday = every,
                                    reqtimeperiod = req,
                                    provideack_status = ackstatus,
                                    createddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    status = "Active"
                                };

                                doctaskuseracknowledmentstatusmodels.Add(doctaskuseracknowledmentstatusmodel);

                                foreach (var permId in doc_perm_rights_id)
                                {
                                    var userPermissionModel = new UserPermissionModel
                                    {

                                        Doc_User_Access_mapping_id = docUserAccessMappingId,
                                        Doc_perm_rights_id = permId,
                                        USR_ID = userId,
                                        user_location_mapping_id = pkid,
                                        AddDoc_id = docid,
                                        permissionstatus = "Active",
                                        permissioncreateddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        ack_status = ackstatus,
                                        permissioncreatedby = provideAccessdocumentModels.createdBy,
                                       

                                    };
                                    userPermissionModels.Add(userPermissionModel);
                                }
                            }

                        }

                        mySqlDBContext.doctaskuseracknowledmentstatusmodels.AddRange(doctaskuseracknowledmentstatusmodels);
                        mySqlDBContext.UserPermissionModels.AddRange(userPermissionModels);
                        await mySqlDBContext.SaveChangesAsync();
                        //    }


                        //    //use 
                        //
                        //var userDocumentInfo = new Dictionary<int, List<string>>();

                        //foreach (var docid in Doc_id)
                        //{



                        //var userIds = await mySqlDBContext.userlocationmappingModels
                        //    .Where(x => user_location_mapping_id.Contains(x.user_location_mapping_id))
                        //    .Select(x => x.USR_ID)
                        //    .ToListAsync();

                        foreach (var userId in userIds)
                        {
                            var userEmail = await mySqlDBContext.usermodels
                                .Where(x => x.USR_ID == userId)
                                .Select(x => x.emailid)
                                .FirstOrDefaultAsync();

                            var DocumentNames = await mySqlDBContext.AddDocumentModels
                                .Where(x => x.AddDoc_id == docid)
                                .Select(x => x.Title_Doc)
                                .ToListAsync();

                            if (!userDocumentInfo.ContainsKey(userId))
                            {
                                userDocumentInfo[userId] = new List<string>();
                            }

                            // Add document titles for the current user
                            userDocumentInfo[userId].AddRange(DocumentNames);
                        }

                        var addDocument = await mySqlDBContext.AddDocumentModels.FirstOrDefaultAsync(x => x.AddDoc_id == docid);

                        if (addDocument != null)
                        {
                            addDocument.status_permission = "Permission";
                            await mySqlDBContext.SaveChangesAsync();
                        }

                        // return Ok();
                    }
                }

                // Send emails with combined document information
                foreach (var (userId, documentNames) in userDocumentInfo)
                        {
                            var userEmail = await mySqlDBContext.usermodels
                                .Where(x => x.USR_ID == userId)
                                .Select(x => x.emailid)
                                .FirstOrDefaultAsync();
                            //  var senderemail = await mySqlDBContext.provideAccessdocumentModels.Where(x => x.Document_Id == documentId).Select(x => x.createdBy).FirstOrDefaultAsync();

                            int senderid = provideAccessdocumentModels.createdBy;
                            var request = HttpContext.Request;
                            string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);

                            if (documentNames.Any())
                            {
                                obj_Clsmail.ProvideAccessMail(userEmail, documentNames.ToArray(), senderid, userId, baseUrl);
                            }
                        }

                 
                return Ok();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                // Log the error if needed
                return StatusCode(500, "Internal Server Error");
            }
        }

     


        [Route("api/ProvideAccess/GetProvideAccessDetails/{Select_Opt}")]
        [HttpGet]

        public IEnumerable<ProvideAccessModel> GetProvideAccessDetails(string Select_Opt)
        {


            if (this.mySqlDBContext == null)
            {
                // Handle the case when mySqlDBContext is null
                return new List<ProvideAccessModel>(); // Or return an appropriate result
            }

            if (string.IsNullOrEmpty(Select_Opt))
            {
                // Handle the case when Select_Opt is null or empty
                return new List<ProvideAccessModel>(); // Or return an appropriate result
            }

            //MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=risk;sslmode=none;");
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT " +
                "e.Select_Opt, " +
    "e.AddDoc_id," +
    "e.Title_Doc," +
    "t.DocTypeName," +
    "C.Doc_CategoryName," +
    "sc.Doc_SubCategoryName," +
    "a.AuthorityName," +
   "at.AuthorityTypeName," +
    "p.NatureOf_Doc_Name " +
   "FROM " +
    "risk.add_doc e" +
" JOIN " +
    "risk.doctype_master t ON t.DocTypeID = e.DocTypeID" +
" JOIN " +
    "risk.doccategory_master C ON C.Doc_CategoryID = e.Doc_CategoryID" +
" JOIN " +
    "risk.docsubcategory_master sc ON sc.Doc_SubCategoryID = e.Doc_SubCategoryID" +
" JOIN " +
    "risk.authorityname_master a ON a.AuthoritynameID = e.AuthoritynameID" +
" JOIN " +
    "risk.authoritytype_master at ON at.AuthorityTypeID = e.AuthorityTypeID" +
" JOIN " +
    "risk.natureof_doc p ON p.NatureOf_Doc_id = e.NatureOf_Doc_id" +

     " WHERE Select_Opt ='" + Select_Opt + "';", con);

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
                    pdata.Add(new ProvideAccessModel
                    {
                        AuthorityName = dt.Rows[i]["AuthorityName"].ToString(),
                        AddDoc_id = Convert.ToInt32(dt.Rows[i]["AddDoc_id"].ToString()),

                        AuthorityTypeName = dt.Rows[i]["AuthorityTypeName"].ToString(),
                        Title_Doc = dt.Rows[i]["Title_Doc"].ToString(),
                        NatureOf_Doc_Name = dt.Rows[i]["NatureOf_Doc_Name"].ToString(),
                        DocTypeName = dt.Rows[i]["DocTypeName"].ToString(),
                        Doc_CategoryName = dt.Rows[i]["Doc_CategoryName"].ToString(),
                        Doc_SubCategoryName = dt.Rows[i]["Doc_SubCategoryName"].ToString(),
                        Select_Opt = dt.Rows[i]["Select_Opt"].ToString()
                    });
                }
            }
            return pdata;

        }


        [Route("api/UserRights/GetUserRights")]
        [HttpGet]

        public IEnumerable<object> UserRigths(string Select_Opt, int AddDoc_id)
        {


            //if (this.mySqlDBContext == null)
            //{
            //    // Handle the case when mySqlDBContext is null
            //    return new List<UserRightsPermissionModel>(); // Or return an appropriate result
            //}

            //if (string.IsNullOrEmpty(Select_Opt))
            //{
            //    // Handle the case when Select_Opt is null or empty
            //    return new List<UserRightsPermissionModel>(); // Or return an appropriate result
            //}

            //MySqlConnection con = new MySqlConnection(@"server=localhost;userid=root;pwd=N33mu$123;port=3306;database=risk;sslmode=none;");
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select add_doc.AddDoc_id, doc_perm_rights.Publish_type,doc_perm_rights.publish_Name,add_doc.Select_Opt from doc_perm_rights inner join add_doc on doc_perm_rights.Publish_type =add_doc.Select_Opt where add_doc.Select_Opt='" + Select_Opt + "' and add_doc.AddDoc_id='" + AddDoc_id + "'", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<UserRightsPermissionModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new UserRightsPermissionModel
                    {
                        Publish_type = dt.Rows[i]["Publish_type"].ToString(),
                        publish_Name = dt.Rows[i]["publish_Name"].ToString(),
                    });
                }
            }
            return pdata;

            //var deatils = (from permissionmaster in mySqlDBContext.UserrightsModels
            //               join adddocumentmaster in mySqlDBContext.ProvideAccessModels
            //               on permissionmaster.Publish_type equals adddocumentmaster.Select_Opt
            //               where adddocumentmaster.Select_Opt == Select_Opt &&  adddocumentmaster.AddDoc_id == AddDoc_id
            //               select new
            //               {
            //                   permissionmaster.publish_Name,
            //               });

            //return deatils;




        }

        [Route("api/UserIds/GetUsersIds")]
        [HttpGet]
        public async Task<IActionResult> GetUsersIds(string addDocIDstr)
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            string[] addDocIDs1 = addDocIDstr.Split(",");
            int[] addDocIDs = new int[addDocIDs1.Length];
            for (int i = 0; i < addDocIDs1.Length; i++)
            {
                addDocIDs[i] = Convert.ToInt32(addDocIDs1[i]);
            }

            List<int> UserIDs = new List<int>();
            List<provideusermodel> userlist = new List<provideusermodel>();

            for (int i = 0; i < addDocIDs.Length; i++)
            {
                MySqlDataAdapter da = new MySqlDataAdapter("SELECT Unit_location_Master_id from add_doc where AddDoc_id='" + addDocIDs[i] + "'", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    string[] Unit_location_Master_ids = dt.Rows[0][0].ToString().Split(",");
                    for (int j = 0; j < Unit_location_Master_ids.Length; j++)
                    {

                        MySqlDataAdapter daa = new MySqlDataAdapter("SELECT user_location_mapping_id from user_location_mapping where Unit_location_Master_id='" + Unit_location_Master_ids[j] + "'", con);
                        DataTable dtt = new DataTable();
                        daa.Fill(dtt);
                        for (int k = 0; k < dtt.Rows.Count; k++)
                        {
                            //if (!UserIDs.Contains(Convert.ToInt32(dtt.Rows[k][0]))){


                            MySqlDataAdapter daaa = new MySqlDataAdapter("SELECT user_location_mapping_id,user_location_mapping.USR_ID,department_master.Department_Master_name,firstname,entity_master.Entity_Master_Name,unit_location_master.Unit_location_Master_name from user_location_mapping inner join tbluser on tbluser.USR_ID = user_location_mapping.USR_ID left join department_master on department_master.Department_Master_id = tbluser.USR_ID  inner join unit_location_master on unit_location_master.Unit_location_Master_id = user_location_mapping.Unit_location_Master_id inner join entity_master on entity_master.Entity_Master_id = user_location_mapping.Entity_Master_id where user_location_mapping. user_location_mapping_id='" + dtt.Rows[k][0] + "'", con);
                            DataTable dttt = new DataTable();
                            daaa.Fill(dttt);
                            if (dttt.Rows.Count > 0)
                            {
                                provideusermodel userdata = new provideusermodel();
                                userdata.firstname = Convert.ToString(dttt.Rows[0]["firstname"]);
                                userdata.USR_ID = Convert.ToInt32(dttt.Rows[0]["USR_ID"]);
                                userdata.user_location_mapping_id = Convert.ToInt32(dttt.Rows[0]["user_location_mapping_id"]);
                                userdata.Unit_location_Master_name = Convert.ToString(dttt.Rows[0]["Unit_location_Master_name"]);
                                userdata.Entity_Master_Name = Convert.ToString(dttt.Rows[0]["Entity_Master_Name"]);
                                userdata.Department_Master_name = Convert.ToString(dttt.Rows[0]["Department_Master_name"]);
                                userlist.Add(userdata);

                            }
                            //    UserIDs.Add(Convert.ToInt32(dtt.Rows[k][0]));

                            //}
                        }

                    }

                }
            }





            //        var usersIds = await mySqlDBContext.AddDocumentModels
            //             .Where(d => addDocIDs.Contains(d.AddDoc_id))
            //.Select(d => new {
            //    AddDoc_ID = d.AddDoc_id,
            //    UnitLocationIds = mySqlDBContext.userlocationmappingModels
            //        .Where(u => d.Unit_location_Master_id.Contains(u.Unit_location_Master_id.ToString()))
            //        .Select(u => u.USR_ID)
            //        .ToList()
            //})
            //.ToListAsync();


            return Ok(userlist);
        }





        [Route("api/ProvideAccessdocument/GetProvideAccessDetailsbyunitlocation/{unit_location_Master_id}")]
        [HttpGet]

        public IEnumerable<object> GetProvideAccessDetailsbyunitlocation(int unit_location_Master_id)
        {


            var excludedDocumentIds = mySqlDBContext.AddDocumentModels
       .GroupBy(ad => ad.Document_Id)
       .Where(group => group.Count() > 1)
       .Select(group => group.Key)
       .ToList();



            var currentDate = DateTime.Now;

            string validationStatus;
            var details = (from documentmaster in mySqlDBContext.AddDocumentModels
                           join doctypemaster in mySqlDBContext.DocTypeMasterModels on documentmaster.DocTypeID equals doctypemaster.docTypeID
                           join doccategorymaster in mySqlDBContext.DocCategoryMasterModels on documentmaster.Doc_CategoryID equals doccategorymaster.Doc_CategoryID
                           join docsubcategorymaster in mySqlDBContext.DocSubCategoryModels on documentmaster.Doc_SubCategoryID equals docsubcategorymaster.Doc_SubCategoryID

                           join authoritypemaster in mySqlDBContext.AuthorityTypeMasters on documentmaster.AuthorityTypeID equals authoritypemaster.AuthorityTypeID

                           join authoritynamemaster in mySqlDBContext.AuthorityNameModels on documentmaster.AuthoritynameID equals authoritynamemaster.AuthoritynameID

                           //  join entitymaster in mySqlDBContext.UnitMasterModels on documentmaster.Entity_Master_id equals entitymaster.Entity_Master_id

                           //  join unitmaster in mySqlDBContext.UnitLocationMasterModels on documentmaster.Unit_location_Master_id equals unitmaster.Unit_location_Master_id

                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on documentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where
                                 documentmaster.addDoc_Status == "Active" &&
                                 documentmaster.Draft_Status == "Completed"

                            && documentmaster.Unit_location_Master_id.Contains (unit_location_Master_id.ToString())


                           select new
                           {
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                               doctypemaster.docTypeName,
                               doccategorymaster.Doc_CategoryName,
                               docsubcategorymaster.Doc_SubCategoryName,
                               authoritynamemaster.AuthorityName,
                               authoritypemaster.AuthorityTypeName,
                               naturemaster.NatureOf_Doc_Name,
                               documentmaster.Doc_Confidentiality,
                               documentmaster.VersionControlNo,
                               // EntityName = entitymaster.Entity_Master_Name, // Replace with the actual property name
                               // UnitLocationName = unitmaster.Unit_location_Master_name, // Replace with the actual property name
                               ReviewStartDate = documentmaster.review_start_Date, // Include the review start date in the result

                           })
                           .Distinct()
                           .ToList()
            .Select(detail => new
            {
                detail.Title_Doc,
                detail.AddDoc_id,
                detail.Document_Id,
                detail.docTypeName,
                detail.Doc_CategoryName,
                detail.Doc_SubCategoryName,
                detail.AuthorityName,
                detail.AuthorityTypeName,
                detail.NatureOf_Doc_Name,
                detail.Doc_Confidentiality,
                detail.VersionControlNo,
                // detail.EntityName,
                // detail.UnitLocationName,
                detail.ReviewStartDate,
                Validation = CalculateValidationStatus(detail.ReviewStartDate, currentDate)
            })
               .ToList();



            // Function to calculate validation status
            string CalculateValidationStatus(DateTime? reviewStartDate, DateTime currentDate)
            {
                if (reviewStartDate.HasValue)
                {
                    TimeSpan span = reviewStartDate.Value - currentDate;
                    int NoofDays = span.Days;

                    if (NoofDays >= 0 && NoofDays <= 30)
                    {
                        return "Take Immediate Action";
                    }
                    else if (NoofDays < 0)
                    {
                        return "Expired";
                    }
                    else if (NoofDays >= 31 && NoofDays <= 60)
                    {
                        return "Expiring Soon";
                    }
                    else if (NoofDays >= 60)
                    {
                        return "Not Due";
                    }
                }

                return "No Date"; // Handle other cases as needed
            }

            return details;




        }







        [Route("api/ProvideAccessdocument/GetProvideAccessDetailsbyuser/{user_location_mapping_id}")]
        [HttpGet]

        public IEnumerable<object> GetProvideAccessDetailsbyuser(int user_location_mapping_id)
        {

            var details = (from provideaccess in mySqlDBContext.UserPermissionModels
                           join documentmaster in mySqlDBContext.AddDocumentModels on provideaccess.AddDoc_id equals documentmaster.AddDoc_id
                           join doctypemaster in mySqlDBContext.DocTypeMasterModels on documentmaster.DocTypeID equals doctypemaster.docTypeID
                           join doccategorymaster in mySqlDBContext.DocCategoryMasterModels on documentmaster.Doc_CategoryID equals doccategorymaster.Doc_CategoryID
                           join docsubcategorymaster in mySqlDBContext.DocSubCategoryModels on documentmaster.Doc_SubCategoryID equals docsubcategorymaster.Doc_SubCategoryID

                           join authoritypemaster in mySqlDBContext.AuthorityTypeMasters on documentmaster.AuthorityTypeID equals authoritypemaster.AuthorityTypeID

                           join authoritynamemaster in mySqlDBContext.AuthorityNameModels on documentmaster.AuthoritynameID equals authoritynamemaster.AuthoritynameID

                           join permission in mySqlDBContext.UserrightsModels on  provideaccess.Doc_perm_rights_id equals permission.Doc_perm_rights_id
                           join naturemaster in mySqlDBContext.NatureOf_DocumentMasterModels on documentmaster.NatureOf_Doc_id equals naturemaster.NatureOf_Doc_id
                           where
                                 documentmaster.addDoc_Status == "Active" && provideaccess.permissionstatus == "Active" && documentmaster.Draft_Status == "Completed" && provideaccess.user_location_mapping_id == user_location_mapping_id


                           select new
                           {
                               provideaccess.doc_user_permission_mapping_pkid,
                               documentmaster.Title_Doc,
                               documentmaster.AddDoc_id,
                               documentmaster.Document_Id,
                               doctypemaster.docTypeName,
                               doccategorymaster.Doc_CategoryName,
                               docsubcategorymaster.Doc_SubCategoryName,
                               authoritynamemaster.AuthorityName,
                               authoritypemaster.AuthorityTypeName,
                               naturemaster.NatureOf_Doc_Name,
                               documentmaster.Doc_Confidentiality,
                               documentmaster.VersionControlNo,
                               provideaccess.Doc_perm_rights_id,
                               permission.publish_Name

                           })
                                  .GroupBy(x => x.AddDoc_id)
                         .Select(g => new
                         {
                             Doc_perm_rights_ids = string.Join(",", g.Select(x => x.Doc_perm_rights_id)),
                             publish_Name = string.Join(",", g.Select(x => x.publish_Name)),
                             VersionControlNo = g.Select(x => x.VersionControlNo).FirstOrDefault(),
                             NatureOf_Doc_Name = g.Select(x => x.NatureOf_Doc_Name).FirstOrDefault(),
                             doc_user_permission_mapping_pkid = g.Select(x => x.doc_user_permission_mapping_pkid).FirstOrDefault(),
                             Title_Doc = g.Select(x => x.Title_Doc).FirstOrDefault(),
                             AddDoc_id = g.Select(x => x.AddDoc_id).FirstOrDefault(),
                             Document_Id = g.Select(x => x.Document_Id).FirstOrDefault(),
                             docTypeName = g.Select(x => x.docTypeName).FirstOrDefault(),
                             Doc_CategoryName = g.Select(x => x.Doc_CategoryName).FirstOrDefault(),
                             Doc_SubCategoryName = g.Select(x => x.Doc_SubCategoryName).FirstOrDefault(),
                             AuthorityName = g.Select(x => x.AuthorityName).FirstOrDefault(),
                             AuthorityTypeName = g.Select(x => x.AuthorityTypeName).FirstOrDefault()
                         })
                   .ToList();

            return details;




        }






        [Route("api/ProvideAccessdocument/Getenitityogdocument/{userid}")]
        [HttpGet]
        public IEnumerable<object> Getenitityogdocument(int userid)
        {
            var documents = mySqlDBContext.AddDocumentModels
                   .Where(ad => ad.addDoc_Status == "Active" && ad.Draft_Status== "Completed" && ad.USR_ID == userid)
                   .ToList();

            var docs = documents
                        .SelectMany(ad => ad.Entity_Master_id.Split(',')
                                        .Select(entityId => new
                                        {
                                            ad.USR_ID,
                                            Entity_Master_id = entityId,
                                            ad.AddDoc_id
                                        }));

         
            var result = (from doc in docs
                         join entitymaster in mySqlDBContext.UnitMasterModels
                         on doc.Entity_Master_id equals entitymaster.Entity_Master_id.ToString()
                         select new
                         {
                           entityid =  doc.Entity_Master_id,
                            entityname = entitymaster.Entity_Master_Name
                         })
                         .Distinct()
                          .ToList();
            

                  return result;
        }




        [Route("api/ProvideAccessdocument/Getlocationdocument/{entityid}")]
        [HttpGet]
        public IEnumerable<object> Getlocationdocument(string entityid)
        {
            var documents = mySqlDBContext.AddDocumentModels
                   .Where(ad => ad.addDoc_Status == "Active" && ad.Draft_Status == "Completed" && ad.Entity_Master_id == entityid)
                   .ToList();

            var docs = documents
                        .SelectMany(ad => ad.Unit_location_Master_id.Split(',')
                                        .Select(entityId => new
                                        {
                                            ad.USR_ID,
                                            Unit_location_Master_id = entityId,
                                            ad.AddDoc_id
                                        }));


            var result = (from doc in docs
                          join unitmaster in mySqlDBContext.UnitLocationMasterModels
                          on doc.Unit_location_Master_id equals unitmaster.Unit_location_Master_id.ToString()
                          select new
                          {
                              unitid = doc.Unit_location_Master_id,
                             unitname = unitmaster.Unit_location_Master_name
                          })
                         .Distinct()
                          .ToList();


            return result;
        }
    }

}


    



