using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel;
using ITR_TelementaryAPI;
using ITR_TelementaryAPI.Models;
using ITRTelemetry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using MySQLProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class AssessementProvideAccessController : ControllerBase
    {


        private ClsEmail obj_Clsmail = new ClsEmail();
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }


        public AssessementProvideAccessController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/UserRightsPermission/ass_user_permissionlist")]
        [HttpGet]

        public IEnumerable<ass_user_permissionlistModel> ass_user_permissionlist()
        {

            return this.mySqlDBContext.ass_user_permissionlists.ToList();
        }



        [Route("api/Assessment/ProvideAccess")]
        [HttpPost]
        public async Task<IActionResult> ProvideAccess([FromBody] assement_provideacess formData)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            var usrid = formData.UserloactionmappingID;

            string numbersString = formData.UserloactionmappingID;
            string[] numbersArray = numbersString.Split(',');
            try
            {
                con.Open();
                foreach (string numberString in numbersArray)
                {




                    int number = int.Parse(numberString);

                    // Query to get the UserID from the user_location_mapping table
                    string getUserIdQuery = @"SELECT USR_ID FROM user_location_mapping 
                                  WHERE user_location_mapping_id = @UserloactionmappingID";
                    string getVersionNumberQuery = @"
    SELECT verson_no 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @AssessementTemplateID and status = 'Active' ";

                    string verion= string.Empty;

                    using (var cmd = new MySqlCommand(getVersionNumberQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@AssessementTemplateID", formData.AssessementTemplateID);
                        object result = await cmd.ExecuteScalarAsync();

                        if (result != null)
                        {
                            verion = result.ToString();
                        }
                    }


                    int userId;
                    using (MySqlCommand getUserIdCommand = new MySqlCommand(getUserIdQuery, con))
                    {
                        getUserIdCommand.Parameters.AddWithValue("@UserloactionmappingID", number);

                        object result = getUserIdCommand.ExecuteScalar();
                        if (result != null)
                        {
                            userId = Convert.ToInt32(result);
                        }
                        else
                        {
                            throw new Exception("User ID not found for the given UserloactionmappingID");
                        }
                    }


                    string checkQuery = @"SELECT COUNT(*) FROM assement_provideacess 
                              WHERE AssessementTemplateID = @AssessementTemplateID 
                              AND EntityMasterID = @EntityMasterID 
                              AND UnitLocationMasterID = @UnitLocationMasterID 
                              AND UserloactionmappingID = @UserloactionmappingID 
                              AND Status = 'Active'";

                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, con))
                    {
                        checkCommand.Parameters.AddWithValue("@AssessementTemplateID", formData.AssessementTemplateID);
                        checkCommand.Parameters.AddWithValue("@EntityMasterID", formData.EntityMasterID);
                        checkCommand.Parameters.AddWithValue("@UnitLocationMasterID", formData.UnitLocationMasterID);
                        checkCommand.Parameters.AddWithValue("@UserloactionmappingID", number);
                        checkCommand.Parameters.AddWithValue("@verson_no", verion);
                        int existingRecordCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (existingRecordCount > 0)
                        {
                            // If a record exists, update the existing one to make its Status 'Delete'
                            string updateQuery = @"UPDATE assement_provideacess 
                                       SET Status = 'Inactive' 
                                       WHERE AssessementTemplateID = @AssessementTemplateID 
                                       AND EntityMasterID = @EntityMasterID 
                                       AND UnitLocationMasterID = @UnitLocationMasterID 
                                       AND UserloactionmappingID = @UserloactionmappingID 
                                       AND Status = 'Active'";

                            using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, con))
                            {
                                updateCommand.Parameters.AddWithValue("@AssessementTemplateID", formData.AssessementTemplateID);
                                updateCommand.Parameters.AddWithValue("@EntityMasterID", formData.EntityMasterID);
                                updateCommand.Parameters.AddWithValue("@UnitLocationMasterID", formData.UnitLocationMasterID);
                                updateCommand.Parameters.AddWithValue("@UserloactionmappingID", number);
                                updateCommand.Parameters.AddWithValue("@verson_no", verion);
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    // Now insert the new record as usual


                    string insertQuery = (@"insert into assement_provideacess(AssessementTemplateID,assessment_builder_id,EntityMasterID,UnitLocationMasterID,UserloactionmappingID,Access_Permissions,Status,CreatedDate,GrantUserPermission,UserID,createdby,verson_no)values
                    (@AssessementTemplateID,@assessment_builder_id,@EntityMasterID,@UnitLocationMasterID,@UserloactionmappingID,@Access_Permissions,@Status,@CreatedDate,@GrantUserPermission,@UserID,@createdby,@verson_no)");


                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {

                        myCommand1.Parameters.AddWithValue("@AssessementTemplateID", formData.AssessementTemplateID);
                        myCommand1.Parameters.AddWithValue("@assessment_builder_id", formData.assessment_builder_id);
                        myCommand1.Parameters.AddWithValue("@EntityMasterID", formData.EntityMasterID);
                        myCommand1.Parameters.AddWithValue("@UnitLocationMasterID", formData.UnitLocationMasterID);
                        myCommand1.Parameters.AddWithValue("@UserloactionmappingID", number);
                        myCommand1.Parameters.AddWithValue("@Access_Permissions", formData.Access_Permissions);

                        myCommand1.Parameters.AddWithValue("@Status", "Active");
                        myCommand1.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@GrantUserPermission", formData.GrantUserPermission);
                        myCommand1.Parameters.AddWithValue("@UserID", userId);
                        myCommand1.Parameters.AddWithValue("@createdby", formData.createdby);
                        myCommand1.Parameters.AddWithValue("@verson_no", verion);

                        myCommand1.ExecuteNonQuery();

                        //// Get the last inserted primary key value
                        //int questionID = Convert.ToInt32(myCommand1.LastInsertedId.ToString());
                        //foreach (Options option in assprovideaccess.options)
                        //{
                        //    // Access and work with each option
                        //    Console.WriteLine($"Index: {option.index}, Value: {option.value}");
                        //    string insertQuery1 = "insert into questionbank_options(question_id,options,created_date,status)values(@question_id,@options,@created_date,@status)";


                        //    using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery1, con))
                        //    {
                        //        myCommand2.Parameters.AddWithValue("@question_id", questionID);
                        //        myCommand2.Parameters.AddWithValue("@options", option.value);
                        //        myCommand2.Parameters.AddWithValue("@created_date", DateTime.Now);
                        //        myCommand2.Parameters.AddWithValue("@status", "Active");

                        //        myCommand2.ExecuteNonQuery();
                        //    }
                        //}


                        // ----- To get User Email

                        var userEmail = await mySqlDBContext.usermodels
                               .Where(x => x.USR_ID == userId)
                               .Select(x => x.emailid)
                               .FirstOrDefaultAsync();


                        string getTemplateNameQuery = @"
    SELECT assessment_name 
    FROM assessment_builder_versions 
    WHERE ass_template_id = @AssessementTemplateID";

                        string templateName = string.Empty;

                        using (var cmd = new MySqlCommand(getTemplateNameQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@AssessementTemplateID", formData.AssessementTemplateID);
                            object result = await cmd.ExecuteScalarAsync();

                            if (result != null)
                            {
                                templateName = result.ToString();
                            }
                        }


                        int senderid = formData.createdby;
                        var request = HttpContext.Request;
                        string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);


                        obj_Clsmail.AssessmentProvideAccessMail(userEmail, templateName, senderid, userId, baseUrl);

                    }


                }


                return Ok("successfully");

            }

            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            finally
            {
                con.Close();
            }
        }

        [Route("api/Assessmentprovide/Getassessmentprovideacess/{userid}")]
        [HttpGet]

        public IEnumerable<object> Getassessmentprovideacess(int userid)

        {


            var details = (from Assessmentprovide in mySqlDBContext.assement_provideacessmodel
                           join entity in mySqlDBContext.UnitMasterModels on Assessmentprovide.EntityMasterID equals entity.Entity_Master_id
                           join location in mySqlDBContext.UnitLocationMasterModels on Assessmentprovide.UnitLocationMasterID equals location.Unit_location_Master_id
                          
                           join assessmentbuilder in mySqlDBContext.AssessmentPublisherModels
                                on Assessmentprovide.AssessementTemplateID equals assessmentbuilder.ass_template_id
                           join tbluser in mySqlDBContext.usermodels on assessmentbuilder.user_id equals tbluser.USR_ID
                           join typemaster in mySqlDBContext.TypeModels on  assessmentbuilder.Type_id equals typemaster.Type_id
                           join subtype in mySqlDBContext.SubTypeModels on assessmentbuilder.SubType_id equals subtype.SubType_id
                           join cskilllevel in mySqlDBContext.CompetencySkillModels on assessmentbuilder.Competency_id equals cskilllevel.Competency_id
                           where Assessmentprovide.Status == "Active" && Assessmentprovide.createdby == userid


                           select new
                           {
                               Assessmentprovide.Assessement_ProvideAccessID,
                               Assessmentprovide.AssessementTemplateID,
                               assessmentbuilder.assessment_name,
                               entity.Entity_Master_Name,
                               location.Unit_location_Master_name,
                               assessmentbuilder.total_estimated_time,
                               assessmentbuilder.total_questions,
                               assessmentbuilder.verson_no,
                               tbluser.firstname,
                               typemaster.Type_Name,
                               subtype.SubType_Name,
                               cskilllevel.Competency_Name
                           })
                           .GroupBy(x => x.AssessementTemplateID)
      .Select(group => group.First());

            return details;

        }

        [Route("api/Assessment/GetAssessementProvideAccess")]
        [HttpGet]
        public IEnumerable<AssessmentProvideAccess> GetProvideAccess()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
    SELECT  
        t.Assessement_ProvideAccessID,
        tum.USR_ID,
        tum.firstname,
        t.AssessementTemplateID,
        t.EntityMasterID,
        t.UnitLocationMasterID,
        t.Access_Permissions,
        t.GrantUserPermission,
        t.assessment_builder_id,
        em.Entity_Master_Name,
        ulm.Unit_location_Master_name,
        ab.assessment_name 
    FROM risk.assement_provideacess AS t
    LEFT JOIN entity_master em 
        ON em.Entity_Master_id = t.EntityMasterID
    LEFT JOIN unit_location_master ulm 
        ON ulm.Unit_location_Master_id = t.UnitLocationMasterID
    LEFT JOIN tbluser tum 
        ON tum.USR_ID = t.UserID
    LEFT JOIN ass_user_permissionlist ap
        ON ap.Ass_User_PermissionListid = t.Access_Permissions
    LEFT JOIN assessment_builder ab 
        ON ab.assessment_builder_id = t.assessment_builder_id 
    WHERE t.Status = @Status", con);


            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@Status", "Active");

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<AssessmentProvideAccess>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    pdata.Add(new AssessmentProvideAccess
                    {
                        Assessement_ProvideAccessID = Convert.ToInt32(dt.Rows[i]["Assessement_ProvideAccessID"].ToString()),

                        user_id = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),
                        AssessementTemplateID = dt.Rows[i]["AssessementTemplateID"].ToString(),
                        EntityMasterID = Convert.ToInt32(dt.Rows[i]["EntityMasterID"].ToString()),
                        UnitLocationMasterID = Convert.ToInt32(dt.Rows[i]["UnitLocationMasterID"].ToString()),
                        // Access_Permissions = dt.Rows[i]["Access_Permissions"].ToString(),
                        // GrantUserPermission = dt.Rows[i]["GrantUserPermission"].ToString(),
                        Entity_Master_Name = dt.Rows[i]["Entity_Master_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"].ToString()),
                        Unit_location_Master_name = dt.Rows[i]["Unit_location_Master_name"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString(),



                    });
                }
            }
            return pdata;
        }

        [Route("api/Assessment/GetAssessementProvideAccessByID")]
        [HttpGet]
        public IEnumerable<AssessmentProvideAccess> GetProvideAccessByID(int ass_provideaccessid)
        {
            var pdata = new List<AssessmentProvideAccess>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
 
     SELECT  t.Assessement_ProvideAccessID,
tum.USR_ID,
t.AssessementTemplateID,
t.EntityMasterID,
t.UnitLocationMasterID,
t.Access_Permissions,
t.UserloactionmappingID,
t.GrantUserPermission,
t.assessment_builder_id,
em.Entity_Master_Name,
ulm.Unit_location_Master_name,
tum.firstname,
aspl.ass_User_PermissionListid,
ab.assessment_name FROM risk.assement_provideacess as t
LEFT JOIN entity_master em ON em.Entity_Master_id = t.EntityMasterID
LEFT JOIN unit_location_master ulm ON ulm.Unit_location_Master_id = t.UnitLocationMasterID
left join tbluser tum on tum.USR_ID=t.UserID
LEFT JOIN assessment_builder ab ON ab.assessment_builder_id = t.assessment_builder_id
LEFT JOIN ass_user_permissionlist aspl ON aspl.ass_User_PermissionListid = t.Access_Permissions
where t.Assessement_ProvideAccessID=@Assessement_ProvideAccessID and t.Status=@Status", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@Assessement_ProvideAccessID", ass_provideaccessid);
            cmd.Parameters.AddWithValue("@Status", "Active");
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {

                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new AssessmentProvideAccess
                    {
                        Assessement_ProvideAccessID = Convert.ToInt32(dt.Rows[i]["Assessement_ProvideAccessID"].ToString()),

                        user_id = Convert.ToInt32(dt.Rows[i]["USR_ID"].ToString()),
                        UserloactionmappingID = dt.Rows[i]["UserloactionmappingID"].ToString(),
                        AssessementTemplateID = dt.Rows[i]["AssessementTemplateID"].ToString(),
                        EntityMasterID = Convert.ToInt32(dt.Rows[i]["EntityMasterID"].ToString()),
                        UnitLocationMasterID = Convert.ToInt32(dt.Rows[i]["UnitLocationMasterID"].ToString()),
                        Access_Permissions = dt.Rows[i]["Access_Permissions"].ToString(),
                        GrantUserPermission = dt.Rows[i]["GrantUserPermission"].ToString(),
                        Entity_Master_Name = dt.Rows[i]["Entity_Master_Name"].ToString(),
                        assessment_builder_id = Convert.ToInt32(dt.Rows[i]["assessment_builder_id"].ToString()),
                        ass_User_PermissionListid = Convert.ToInt32(dt.Rows[i]["ass_User_PermissionListid"].ToString()),
                        Unit_location_Master_name = dt.Rows[i]["Unit_location_Master_name"].ToString(),
                        assessment_name = dt.Rows[i]["assessment_name"].ToString(),
                        firstname = dt.Rows[i]["firstname"].ToString()
                    });

                }

            }
            return pdata;
        }



        [Route("api/Assessment/GetAssUserRightsPermission")]
        [HttpGet]

        public IEnumerable<object> GetAssUserRightsPermission(int USR_ID)
        {

            var pdata = new List<ass_user_permissionlistModel>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"
 
   select * from ass_user_permissionlist where Ass_User_PermissionListid=@Ass_User_PermissionListid", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@Ass_User_PermissionListid", USR_ID);

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {


                pdata.Add(new ass_user_permissionlistModel
                {

                    AssUserPermissionName = dt.Rows[0]["AssUserPermissionName"].ToString(),

                });



            }
            return pdata;

        }



        [Route("api/Assessment/UpdateAssProvideAccess")]
        [HttpPost]
        public IActionResult UpdateAssProvideAccess([FromBody] assement_provideacess assprovideaccess)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            try
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand(@"Select * from assement_provideacess  where AssessementTemplateID=@AssessementTemplateID and UserloactionmappingID=@UserloactionmappingID and assement_provideacess.Status='Active'", con);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@AssessementTemplateID", assprovideaccess.AssessementTemplateID);
                cmd.Parameters.AddWithValue("@UserloactionmappingID", assprovideaccess.UserloactionmappingID);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                //con.Close();


                if (dt.Rows.Count > 0)
                {
                    int Assessement_ProvideAccessID = Convert.ToInt32(dt.Rows[0]["Assessement_ProvideAccessID"].ToString());

                    string insertQuery = "update assement_provideacess set Access_Permissions =@Access_Permissions where Assessement_ProvideAccessID=@Assessement_ProvideAccessID";

                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {
                        myCommand1.Parameters.AddWithValue("@Access_Permissions", assprovideaccess.Access_Permissions);
                        myCommand1.Parameters.AddWithValue("@Assessement_ProvideAccessID", Assessement_ProvideAccessID);


                        myCommand1.ExecuteNonQuery();





                    }


                }
                else
                {
                    return Ok();
                }

                return Ok("successfully");

            }

            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            finally
            {
                con.Close();
            }
        }

        [Route("api/Assessment/RemoveAssProvideAccess")]
        [HttpPost]
        public IActionResult RemoveAssProvideAccess([FromBody] assement_provideacess assprovideaccess)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);


            try
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand(@"Select * from assement_provideacess  where AssessementTemplateID=@AssessementTemplateID and UserloactionmappingID=@UserloactionmappingID and assement_provideacess.Status=@Status", con);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@AssessementTemplateID", assprovideaccess.AssessementTemplateID);
                cmd.Parameters.AddWithValue("@UserloactionmappingID", assprovideaccess.UserloactionmappingID);
                cmd.Parameters.AddWithValue("@Status", "Active");

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                da.Fill(dt);
                //con.Close();


                if (dt.Rows.Count > 0)
                {
                    int Assessement_ProvideAccessID = Convert.ToInt32(dt.Rows[0]["Assessement_ProvideAccessID"].ToString());
                    string insertQuery = "update assement_provideacess set Status =@Status where Assessement_ProvideAccessID=@Assessement_ProvideAccessID";

                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con))
                    {
                        myCommand1.Parameters.AddWithValue("@Status", "Removed");
                        myCommand1.Parameters.AddWithValue("@Assessement_ProvideAccessID", Assessement_ProvideAccessID);


                        myCommand1.ExecuteNonQuery();





                    }



                }

                return Ok("successfully");

            }

            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            finally
            {
                con.Close();
            }
        }



        [Route("api/Assessment/GetAssUsersByAssTemp")]
        [HttpGet]

        public IEnumerable<object> GetAssUsersByAssTemp(string AssessementTemplateID)
        {

            var pdata = new List<GetAssUsersByAssTempModel>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"Select tum.firstname,UserID,assement_provideacess.UserloactionmappingID,em.Entity_Master_Name,um.Unit_location_Master_name  from assement_provideacess left join tbluser tum on tum.USR_ID=assement_provideacess.UserID left join user_location_mapping ulm on ulm.user_location_mapping_id=assement_provideacess.UserloactionmappingID left join entity_master em on em.Entity_Master_id = ulm.Entity_Master_id left join  unit_location_master um on um.Unit_location_Master_id = ulm.Unit_location_Master_id where AssessementTemplateID=@AssessementTemplateID and assement_provideacess.Status=@Status", con);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            cmd.Parameters.AddWithValue("@Status", "Active");

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {

                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    pdata.Add(new GetAssUsersByAssTempModel
                    {

                        firstname = dt.Rows[i]["firstname"].ToString(),
                        UserID = Convert.ToInt32(dt.Rows[i]["UserID"].ToString()),

                        UserloactionmappingID = dt.Rows[i]["UserloactionmappingID"].ToString(),
                        Entity_Master_Name = dt.Rows[i]["Entity_Master_Name"].ToString(),
                        Unit_location_Master_name = dt.Rows[i]["Unit_location_Master_name"].ToString()


                    });

                }

            }
            return pdata;

        }



        [Route("api/Assessment/GetAssoneUserRightsPermission")]
        [HttpGet]

        public IEnumerable<object> GetAssoneUserRightsPermission(string USR_ID, string AssessementTemplateID)
        {

            var pdata = new List<AssessmentProvideAccess>();

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select * from assement_provideacess where UserloactionmappingID=@USR_ID and Status=@Status and AssessementTemplateID=@AssessementTemplateID", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@USR_ID", USR_ID);
            cmd.Parameters.AddWithValue("@Status", "Active");
            cmd.Parameters.AddWithValue("@AssessementTemplateID", AssessementTemplateID);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();


            if (dt.Rows.Count > 0)
            {


                pdata.Add(new AssessmentProvideAccess
                {
                    Assessement_ProvideAccessID = Convert.ToInt32(dt.Rows[0]["Assessement_ProvideAccessID"].ToString()),

                    //user_id = Convert.ToInt32(dt.Rows[0]["USR_ID"].ToString()),
                    AssessementTemplateID = dt.Rows[0]["AssessementTemplateID"].ToString(),
                    EntityMasterID = Convert.ToInt32(dt.Rows[0]["EntityMasterID"].ToString()),
                    UnitLocationMasterID = Convert.ToInt32(dt.Rows[0]["UnitLocationMasterID"].ToString()),
                    Access_Permissions = dt.Rows[0]["Access_Permissions"].ToString(),


                });



            }
            return pdata;

        }



        // get entity
        [Route("api/ProvideAccessassessment/Getenitityogassessment/{userid}")]
        [HttpGet]
        public IEnumerable<object> Getenitityogassessment (int userId)
        {
            
            var documents = mySqlDBContext.assement_provideacessmodel
             .Where(ad => ad.Status == "Active" && ad.createdby == userId)
             .ToList();



           
            var docs = documents.Select(ad => new
            {
                ad.UserID,
                Entity_Master_id = ad.EntityMasterID,
                ad.AssessementTemplateID
            });

            var result = (from doc in docs
                          join entityMaster in mySqlDBContext.UnitMasterModels
                          on doc.Entity_Master_id equals entityMaster.Entity_Master_id
                          select new
                          {
                              EntityId = doc.Entity_Master_id,
                              EntityName = entityMaster.Entity_Master_Name
                          })
                         .Distinct()
                         .ToList();

            return result;
        }


        // get location
        [Route("api/ProvideAccessassessment/Getlocationassessment/{entityid}")]
        [HttpGet]
        public IEnumerable<object> Getlocationassessment (int entityid)
        {
            var documents = mySqlDBContext.assement_provideacessmodel
                   .Where(ad => ad.Status == "Active" && ad.EntityMasterID == entityid)
                   .ToList();

            var docs = documents.Select(ad => new
            {
                ad.UserID,
                Unit_location_Master_id = ad.UnitLocationMasterID,
                ad.AssessementTemplateID
            });


            var result = (from doc in docs
                          join entityMaster in mySqlDBContext.UnitLocationMasterModels
                          on doc.Unit_location_Master_id equals entityMaster.Unit_location_Master_id
                          select new
                          {
                              unitId = doc.Unit_location_Master_id,
                              unitName = entityMaster.Unit_location_Master_name
                          })
                         .Distinct()
                         .ToList();

            return result;
        }


        // get user
        [Route("api/ProvideAccessassessment/GetassessuserDetailsbyid/{unit_location_Master_id}")]
        [HttpGet]
        public IEnumerable<object> GetassessuserDetailsbyid (int unit_location_Master_id)
        {
            var deatils = (from userpermission in mySqlDBContext.assement_provideacessmodel
                           join userlocation in mySqlDBContext.userlocationmappingModels on userpermission.UserloactionmappingID equals userlocation.user_location_mapping_id.ToString()
                           join tbluser in mySqlDBContext.usermodels on userlocation.USR_ID equals tbluser.USR_ID
                           join unitlocation in mySqlDBContext.UnitLocationMasterModels on userlocation.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                           join entitymaster in mySqlDBContext.UnitMasterModels on userlocation.Entity_Master_id equals entitymaster.Entity_Master_id
                           where userpermission.Status == "Active" && userlocation.Unit_location_Master_id == unit_location_Master_id
                           select new
                           {
                               userpermission.Assessement_ProvideAccessID,
                               userlocation.Entity_Master_id,
                               entitymaster.Entity_Master_Name,
                               userlocation.Unit_location_Master_id,
                               unitlocation.Unit_location_Master_name,
                               userlocation.USR_ID,
                               tbluser.firstname,
                               userpermission.UserloactionmappingID
                           }
                )
                .GroupBy(x => x.UserloactionmappingID)
                .Select(g => g.First())
                .ToList();
            return deatils;
        }




        [Route("api/ProvideAccessAssessment/GetAssessmentDetailsbyuser/{user_location_mapping_id}")]
          [HttpGet]

        public IEnumerable<object> GetAssessmentDetailsbyuser (int user_location_mapping_id)
        {

            var details = (from provideaccess in mySqlDBContext.assement_provideacessmodel
                           join entity in mySqlDBContext.UnitMasterModels on provideaccess.EntityMasterID equals entity.Entity_Master_id
                           join location in mySqlDBContext.UnitLocationMasterModels on provideaccess.UnitLocationMasterID equals location.Unit_location_Master_id
                           join assessmentbuilder in mySqlDBContext.AssessmentPublisherModels
                                on provideaccess.AssessementTemplateID equals assessmentbuilder.ass_template_id
                          
                          
                           where
                                 provideaccess.Status == "Active" && provideaccess.UserloactionmappingID == user_location_mapping_id.ToString() 


                           select new
                           {
                               provideaccess.Assessement_ProvideAccessID,
                               provideaccess.Access_Permissions,
                               provideaccess.AssessementTemplateID,
                               assessmentbuilder.assessment_name,
                               entity.Entity_Master_Name,
                               location.Unit_location_Master_name,
                               assessmentbuilder.total_estimated_time,
                               assessmentbuilder.total_questions,
                               assessmentbuilder.verson_no


                           })
                                 
                   .ToList();
            var permissions = mySqlDBContext.ass_user_permissionlists.ToList();

            var result = details.Select(detail => new
            {
                detail.Assessement_ProvideAccessID,
                detail.AssessementTemplateID,
                detail.assessment_name,
                detail.Entity_Master_Name,
                detail.total_estimated_time,
                detail.total_questions,
                detail.verson_no,
                detail.Unit_location_Master_name,
                Permissions = detail.Access_Permissions
                    .Split(',') // Split the comma-separated string
                    .Select(permissionId => permissions
                        .FirstOrDefault(p => p.Ass_User_PermissionListid.ToString() == permissionId)?.AssUserPermissionName)
                    .Where(permissionName => !string.IsNullOrEmpty(permissionName)) 
                    .ToList()
            });

            return result;
        }


    }


    }

