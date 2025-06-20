using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel;
using ITR_TelementaryAPI.Models;
using ITRTelemetry.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using MySQLProvider;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;


namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class ComplainceUserMappingController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;

        private readonly IServiceProvider _serviceProvider;
        public IConfiguration Configuration { get; }


        public ComplainceUserMappingController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
        }

        [Route("api/ComplainceUserMappingController/Getcompanycompliance")]
        [HttpGet]
        public IActionResult Getcompanycompliance([FromQuery] CreateCompanyComplianceModel payload)
        {
            try
            {
                // Start with the base query
                var query = mySqlDBContext.CreateCompanyComplianceModels.AsQueryable();


                if (payload.category_of_law_id != null)
                    query = query.Where(e => e.category_of_law_id == payload.category_of_law_id);

                if (payload.law_type_id != null)
                    query = query.Where(e => e.law_type_id == payload.law_type_id);

                if (payload.rule_id != null)
                    query = query.Where(e => e.rule_id == payload.rule_id);

                if (payload.regulatory_authority_id != null)
                    query = query.Where(e => e.regulatory_authority_id == payload.regulatory_authority_id);

                if (payload.jursdiction_category_id != null)
                    query = query.Where(e => e.jursdiction_category_id == payload.jursdiction_category_id);

                if (payload.country_id != null)
                    query = query.Where(e => e.country_id == payload.country_id);

                if (payload.state_id != null)
                    query = query.Where(e => e.state_id == payload.state_id);//

                if (payload.jursdiction_Location_id != null)
                    query = query.Where(e => e.jursdiction_Location_id == payload.jursdiction_Location_id);
                if (payload.compliance_type_id != null)
                    query = query.Where(e => e.compliance_type_id == payload.compliance_type_id);
                if (payload.frequency_period_id != null)
                    query = query.Where(e => e.frequency_period_id == payload.frequency_period_id);

                var existingrecord = query.FirstOrDefault();

                bool allPayloadFieldsNull =
          payload.category_of_law_id == null &&
          payload.law_type_id == null &&
          payload.rule_id == null &&
          payload.regulatory_authority_id == null &&
          payload.jursdiction_category_id == null &&
          payload.country_id == null &&
          payload.state_id == null &&
          payload.jursdiction_Location_id == null && payload.compliance_type_id ==null && payload.frequency_period_id == null;

                IQueryable<CreateCompanyComplianceModel> complianceQuery;

                if (existingrecord != null)
                {

                    complianceQuery = query.Where(e => e.status == "Active");
                }
                else if (allPayloadFieldsNull)
                {

                    complianceQuery = mySqlDBContext.CreateCompanyComplianceModels
                                                    .Where(e => e.status == "Active");
                }
                else
                {
                    return Ok(new List<object>());
                }
                //
                var compliance = from companycompliance in complianceQuery
                                 join complaince_location in mySqlDBContext.compliancelocationmappingmodels on companycompliance.create_company_compliance_id.ToString() equals complaince_location.companycomplianceid
                                 join act_rule_regulatory in mySqlDBContext.Rulesandregulatorymodels on companycompliance.rule_id equals act_rule_regulatory.act_rule_regulatory_id
                                 join act_regulator in mySqlDBContext.Actregulatorymodels on act_rule_regulatory.actregulatoryid equals act_regulator.actregulatoryid
                                 join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on complaince_location.locationdepartmentmappingid equals (departmentlocation.locationdepartmentmappingid).ToString()
                                 join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
                                 join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
                                 join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
                                 join compliance_type in mySqlDBContext.ComplianceModels on companycompliance.compliance_type_id equals compliance_type.compliance_type_id
                                 join catgoryoflaw in mySqlDBContext.catageoryoflawmodels on companycompliance.category_of_law_id equals catgoryoflaw.category_of_law_ID
                                 join RegulatoryAuthority in mySqlDBContext.RegulatoryAuthorityModels on companycompliance.regulatory_authority_id equals RegulatoryAuthority.regulatory_authority_id
                               
                                 join jurisdictor in mySqlDBContext.Jurisdictionmodels on companycompliance.jursdiction_category_id equals jurisdictor.jurisdiction_category_id
                                 join country in mySqlDBContext.CountryModels on companycompliance.country_id equals country.id into countryGroup
                                 from countrynull in countryGroup.DefaultIfEmpty()
                                 join state in mySqlDBContext.StateModels on companycompliance.state_id equals state.id into stateGroup
                                 from statenull in stateGroup.DefaultIfEmpty()
                                 join district in mySqlDBContext.JurisdictionLocationModels on companycompliance.jursdiction_Location_id equals district.jurisdiction_location_id into districtGroup
                                 from districtnull in districtGroup.DefaultIfEmpty()
                                 join compliancrecordtype in mySqlDBContext.ComplianceRecordTypeModels on companycompliance.compliance_record_type_id equals compliancrecordtype.compliance_record_type_id
                                 join compliancgroup in mySqlDBContext.ComplianceGroupModels on companycompliance.compliance_group_id equals compliancgroup.compliance_group_id
                                 join compliancenotified in mySqlDBContext.ComplianceNotifiedStatusModels on companycompliance.compliance_notified_status_id equals compliancenotified.compliance_notified_id
                                 join compliancefreq in mySqlDBContext.Frequencymodels on companycompliance.frequency_period_id equals compliancefreq.frequencyid
                                 join classificationrisk in mySqlDBContext.ComplianceRiskClassificationCriteriaModels on companycompliance.risk_classification_criteria_id equals classificationrisk.compliance_risk_criteria_id
                                 where companycompliance.status == "Active" && complaince_location.status == "Mapped"
                                 orderby companycompliance.create_company_compliance_id ascending
                                 select new
                                 {
                                     companycompliance.create_company_compliance_id,
                                     companycompliance.company_compliance_id,
                                     companycompliance.compliance_name,
                                     act_rule_regulatory.act_rule_regulatory_id,
                                     act_rule_regulatory.act_rule_name,
                                     act_regulator.actregulatoryname,
                                     companycompliance.subscription_type,
                                     companycompliance.mapping_status,
                                     companycompliance.compliance_description,
                                     compliance_type = compliance_type.compliance_type_name,
                                     keywors_tags = companycompliance.key_words_tags,
                                     compliance_type_id = companycompliance.compliance_type_id,
                                     create_compliance_id = companycompliance.create_company_compliance_id,
                                     law_Categoryname = catgoryoflaw.law_Categoryname,
                                     regulatory_authority_name = RegulatoryAuthority.regulatory_authority_name,
                                     country = (countrynull != null) ? countrynull.name : "N/A",
                                     state = (statenull != null) ? statenull.name : "N/A",
                                     JurisdictionLocationDistrict = (districtnull != null) ? districtnull.jurisdiction_district : "N/A",
                                     compliance_record_name = compliancrecordtype.compliance_record_name,
                                     compliance_group_name = compliancgroup.compliance_group_name,
                                     compliance_notified_name = compliancenotified.compliance_notified_name,
                                     compliance_risk_criteria_name = classificationrisk.compliance_risk_criteria_name,
                                     complaince_location.compliance_location_Mapping_id,
                                     departmentmaster.Department_Master_name,
                                     entitymaster.Entity_Master_Name,
                                     unitlocation.Unit_location_Master_name,
                                     complaince_location.companycompliancemappingid,
                                     complaince_location.locationdepartmentmappingid,
                                     compliancefreq.frequencyperiod
                                 };

                var result = compliance.ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
            //try
            //{
            //    var existingrecord = mySqlDBContext.CreateCompanyComplianceModels
            //                  .FirstOrDefault(e => e.category_of_law_id == payload.category_of_law_id
            //                      || e.law_type_id == payload.law_type_id
            //                               || e.regulatory_authority_id == payload.regulatory_authority_id &&
            //                               e.jursdiction_category_id == payload.jursdiction_category_id
            //                               || e.country_id == payload.country_id &&
            //                               e.state_id == payload.state_id || e.jursdiction_Location_id == payload.jursdiction_Location_id);
            //    if (existingrecord != null)
            //    {
            //        var compliance = from companycompliance in mySqlDBContext.CreateCompanyComplianceModels
            //                         join act_rule_regulatory in mySqlDBContext.Rulesandregulatorymodels on companycompliance.rule_id equals act_rule_regulatory.act_rule_regulatory_id
            //                         join act_regulator in mySqlDBContext.Actregulatorymodels on act_rule_regulatory.actregulatoryid equals act_regulator.actregulatoryid
            //                         join complaince_location in mySqlDBContext.compliancelocationmappingmodels on companycompliance.create_company_compliance_id.ToString() equals complaince_location.companycomplianceid
            //                         join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on complaince_location.locationdepartmentmappingid equals (departmentlocation.locationdepartmentmappingid).ToString()
            //                         join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
            //                         join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
            //                         join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
            //                         where companycompliance.status == "Active"
            //                         select new
            //                         {
            //                             companycompliance.create_company_compliance_id,
            //                             companycompliance.company_compliance_id,
            //                             companycompliance.compliance_name,
            //                             act_rule_regulatory.act_rule_regulatory_id,
            //                             act_rule_regulatory.act_rule_name,
            //                             act_regulator.actregulatoryname,
            //                             companycompliance.subscription_type,
            //                             companycompliance.mapping_status,
            //                             complaince_location.compliance_location_Mapping_id,
            //                             departmentmaster.Department_Master_name,
            //                             entitymaster.Entity_Master_Name,
            //                             unitlocation.Unit_location_Master_name,
            //                             complaince_location.companycompliancemappingid
            //                         };
            //                         var result = compliance.ToList();

            //        return Ok(result); // Return the list as the content of OkObjectResult
            //    }
            //    else
            //    {
            //        //// Handle the case when the record does not exist
            //        //// You might want to return NotFound or BadRequest
            //        //return NotFound(); // Or any other appropriate action
            //        var compliance = from companycompliance in mySqlDBContext.CreateCompanyComplianceModels
            //                         join act_rule_regulatory in mySqlDBContext.Rulesandregulatorymodels on companycompliance.rule_id equals act_rule_regulatory.act_rule_regulatory_id
            //                         join act_regulator in mySqlDBContext.Actregulatorymodels on act_rule_regulatory.actregulatoryid equals act_regulator.actregulatoryid
            //                         join complaince_location in mySqlDBContext.compliancelocationmappingmodels on companycompliance.create_company_compliance_id.ToString() equals complaince_location.companycomplianceid
            //                         join departmentlocation in mySqlDBContext.Departmentlocationmappingmodels on complaince_location.locationdepartmentmappingid equals (departmentlocation.locationdepartmentmappingid).ToString()
            //                         join unitlocation in mySqlDBContext.UnitLocationMasterModels on departmentlocation.unitlocationid equals unitlocation.Unit_location_Master_id.ToString()
            //                         join departmentmaster in mySqlDBContext.DepartmentModels on departmentlocation.departmentid equals departmentmaster.Department_Master_id.ToString()
            //                         join entitymaster in mySqlDBContext.UnitMasterModels on departmentlocation.entityid equals entitymaster.Entity_Master_id
            //                         where companycompliance.status == "Active"
            //                         select new
            //                         {
            //                             companycompliance.create_company_compliance_id,
            //                             companycompliance.company_compliance_id,
            //                             companycompliance.compliance_name,
            //                             act_rule_regulatory.act_rule_regulatory_id,
            //                             act_rule_regulatory.act_rule_name,
            //                             act_regulator.actregulatoryname,
            //                             companycompliance.subscription_type,
            //                             companycompliance.mapping_status,
            //                             complaince_location.compliance_location_Mapping_id,
            //                             departmentmaster.Department_Master_name,
            //                             entitymaster.Entity_Master_Name,
            //                             unitlocation.Unit_location_Master_name,
            //                             complaince_location.companycompliancemappingid

            //                         };

            //        var result = compliance.ToList();
            //        return Ok(result);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    // Log or handle the exception here
            //    Console.WriteLine($"An error occurred: {ex.Message}");
            //    return null; // Or any other appropriate action
            //}
        }



        [Route("api/ComplainceUserMappingController/GetcomplainceUsers")]
        [HttpGet]

        public IEnumerable<GetUserModel> GetcomplainceUsers(string rolename, string locationdepartmentmappingids,string companycomplianceids, string userid)

        {

            //            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            //            con.Open();

            //            MySqlCommand cmd = new MySqlCommand(@"select ROLE_ID from tblrole where ROLE_NAME =@ROLE_NAME;", con);

            //            cmd.CommandType = CommandType.Text;

            //            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            //            cmd.Parameters.AddWithValue("@ROLE_NAME", rolename);

            //            DataTable dt = new DataTable();
            //            da.Fill(dt);
            //            con.Close();
            //            var pdata = new List<GetUserModel>();
            //            if (dt.Rows.Count > 0)
            //            {

            //                int ROLE_ID = Convert.ToInt32(dt.Rows[0]["ROLE_ID"].ToString());
            //                //converting locationids to in query ids(ex:1,2,3 to '1','2','3')
            //                string[] locationdepartmentmapid = locationdepartmentmappingids.Split(',');
            //                string[] a= locationdepartmentmapid.Select(value => $"'{value}'").ToArray();
            //                string locationdepartmentmappingid = string.Join(",", a);
            //                //converting complainceids to in query ids(ex:1,2,3 to '1','2','3')
            //                string[] companycomplceid = companycomplianceids.Split(',');
            //                string[] b = companycomplceid.Select(value => $"'{value}'").ToArray();
            //                string companycomplianceid = string.Join(",", b);
            //                MySqlCommand cmd1 = new MySqlCommand(@"select distinct USR_ID,firstname from user_workgroup_mapping uwg
            //join activityworkgroup awg on awg.activity_Workgroup_id=uwg.activityworkgroup_id
            //join compliance_location_mapping clm on clm.locationdepartmentmappingid=awg.locationdepartmentmappingid
            //join  tbluser on tbluser.USR_ID =uwg.userid
            //join tblrole on tblrole.ROLE_ID=awg.roles
            //where  locationdepartmentmappingid IN (" + locationdepartmentmappingid + ")  and companycomplianceid in (" + companycomplianceid + ") and awg.roles=@ROLE_ID and USR_ID not in(" + userid + ") order by tbluser.USR_ID ", con);

            //                cmd1.CommandType = CommandType.Text;

            //                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
            //                cmd1.Parameters.AddWithValue("@ROLE_ID", ROLE_ID);

            //                DataTable dt1 = new DataTable();
            //                da1.Fill(dt1);
            //                con.Close();
            //                if (dt1.Rows.Count > 0)
            //                {
            //                    for (int i = 0; i < dt1.Rows.Count; i++)
            //                    {
            //                        pdata.Add(new GetUserModel
            //                        {
            //                            firstname = dt1.Rows[i]["firstname"].ToString(),

            //                            USR_ID = Convert.ToInt32(dt1.Rows[i]["USR_ID"]),

            //                        });
            //                    }
            //                }
            //            }
            //            return pdata;
            var pdata = new List<GetUserModel>();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    // Step 1: Get ROLE_ID from role name
                    MySqlCommand cmd = new MySqlCommand(@"SELECT ROLE_ID FROM tblrole WHERE ROLE_NAME = @ROLE_NAME;", con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ROLE_NAME", rolename);

                    DataTable dt = new DataTable();
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        Console.WriteLine("No ROLE_ID found for rolename: " + rolename);
                        return pdata;
                    }

                    int ROLE_ID = Convert.ToInt32(dt.Rows[0]["ROLE_ID"]);

                    // Step 2: Format locationdepartmentmappingids
                    string[] locIdsRaw = locationdepartmentmappingids.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    string[] locIds = locIdsRaw.Select(id => $"'{MySqlHelper.EscapeString(id.Trim())}'").ToArray();
                    string locationIdsStr = string.Join(",", locIds);

                    // Step 3: Format companycomplianceids
                    string[] compIdsRaw = companycomplianceids.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    string[] compIds = compIdsRaw.Select(id => $"'{MySqlHelper.EscapeString(id.Trim())}'").ToArray();
                    string complianceIdsStr = string.Join(",", compIds);

                    // Step 4: Format user exclusion
                    string userCondition = "";
                    if (!string.IsNullOrWhiteSpace(userid))
                    {
                        string[] userIds = userid.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        string[] safeUserIds = userIds.Select(id => $"'{MySqlHelper.EscapeString(id.Trim())}'").ToArray();
                        userCondition = $"AND USR_ID NOT IN ({string.Join(",", safeUserIds)})";
                    }

                    // Step 5: Final Query
                    string finalQuery = $@"
                SELECT DISTINCT USR_ID, firstname
                FROM user_workgroup_mapping uwg
                JOIN activityworkgroup awg ON awg.activity_Workgroup_id = uwg.activityworkgroup_id
                JOIN compliance_location_mapping clm ON clm.locationdepartmentmappingid = awg.locationdepartmentmappingid
                JOIN tbluser ON tbluser.USR_ID = uwg.userid
                JOIN tblrole ON tblrole.ROLE_ID = awg.roles
                WHERE clm.locationdepartmentmappingid IN ({locationIdsStr})
                  AND companycomplianceid IN ({complianceIdsStr})
                  AND awg.roles = @ROLE_ID
                  {userCondition}
                ORDER BY tbluser.USR_ID";

                    Console.WriteLine("Executing SQL Query:");
                    Console.WriteLine(finalQuery);
                    Console.WriteLine("With ROLE_ID: " + ROLE_ID);

                    // Step 6: Execute final query
                    using (MySqlCommand cmd1 = new MySqlCommand(finalQuery, con))
                    {
                        cmd1.CommandType = CommandType.Text;
                        cmd1.Parameters.AddWithValue("@ROLE_ID", ROLE_ID);

                        using (MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1))
                        {
                            DataTable dt1 = new DataTable();
                            da1.Fill(dt1);

                            Console.WriteLine("Rows fetched: " + dt1.Rows.Count);

                            if (dt1.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt1.Rows.Count; i++)
                                {
                                    pdata.Add(new GetUserModel
                                    {
                                        firstname = dt1.Rows[i]["firstname"].ToString(),
                                        USR_ID = Convert.ToInt32(dt1.Rows[i]["USR_ID"])
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetcomplainceUsers API: " + ex.Message);
            }

            return pdata;
        }


        [Route("api/ComplainceUserMappingController/GetcomplainceRemidiationUsers")]
        [HttpGet]

        public IEnumerable<GetUserModel> GetcomplainceRemidiationUsers(string  userid, string locationdepartmentmappingids)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            if (userid != null && userid !="undefined")
            {
                var pdata = new List<GetUserModel>();
                //converting locationids to in query ids(ex:1,2,3 to '1','2','3')
                string[] locationdepartmentmapid = locationdepartmentmappingids.Split(',');
                string[] a = locationdepartmentmapid.Select(value => $"'{value}'").ToArray();
                string locationdepartmentmappingid = string.Join(",", a);

                MySqlCommand cmd1 = new MySqlCommand(@"select distinct USR_ID,firstname from user_workgroup_mapping uwg
join activityworkgroup awg on awg.activity_Workgroup_id=uwg.activityworkgroup_id
join compliance_location_mapping clm on clm.locationdepartmentmappingid=awg.locationdepartmentmappingid
join  tbluser on tbluser.USR_ID =uwg.userid
where compliance_location_Mapping_id IN (" + locationdepartmentmappingid + ") and USR_STATUS = \"Active\" and USR_ID not in(" + userid + ") order by tbluser.USR_ID ", con);

                cmd1.CommandType = CommandType.Text;

                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                cmd1.Parameters.AddWithValue("@USR_ID", userid);

                DataTable dt1 = new DataTable();
                da1.Fill(dt1);
                con.Close();
                if (dt1.Rows.Count > 0)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        pdata.Add(new GetUserModel
                        {
                            firstname = dt1.Rows[i]["firstname"].ToString(),

                            USR_ID = Convert.ToInt32(dt1.Rows[i]["USR_ID"]),

                        });
                    }
                }

                return pdata;
            }
            else
            {
                return null;
            }
        }



        [Route("api/ComplainceUserMappingController/GetNotificationMailAlerts")]
        [HttpGet]

        public IEnumerable<Notificationmailalert> GetNotificationMailAlerts()

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            
            MySqlCommand cmd = new MySqlCommand(@"select * from Notificationmailalert", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<Notificationmailalert>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                  

                    pdata.Add(new Notificationmailalert
                    {
                      
                        NotificationMailAlertid = Convert.ToInt32(dt.Rows[i]["NotificationMailAlertid"]),
                        NameofAlert = dt.Rows[i]["NameofAlert"].ToString(),
                        
                    });
                }
            }
            return pdata;

        }



        [Route("api/ComplainceUserMappingController/GetEscalationUsers")]
        [HttpGet]

        public IEnumerable<GetUserModel> GetEscalationUsers(string userid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            if (userid != null && userid != "undefined")
            {
                var pdata = new List<GetUserModel>();
                

                MySqlCommand cmd1 = new MySqlCommand(@"select distinct USR_ID,firstname from tbluser where  USR_STATUS ='Active' and USR_ID not in(" + userid + ") order by tbluser.USR_ID ", con);

                cmd1.CommandType = CommandType.Text;

                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                cmd1.Parameters.AddWithValue("@USR_ID", userid);

                DataTable dt1 = new DataTable();
                da1.Fill(dt1);
                con.Close();
                if (dt1.Rows.Count > 0)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        pdata.Add(new GetUserModel
                        {
                            firstname = dt1.Rows[i]["firstname"].ToString(),

                            USR_ID = Convert.ToInt32(dt1.Rows[i]["USR_ID"]),

                        });
                    }
                }

                return pdata;
            }
            else
            {
                return null;
            }
        }


        [Route("api/ComplainceUserMappingController/GetEmptyEscalationUsers")]
        [HttpGet]

        public IEnumerable<GetUserModel> GetEmptyEscalationUsers(string userid)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            if (userid != null && userid != "undefined")
            {
                var pdata = new List<GetUserModel>();


                MySqlCommand cmd1 = new MySqlCommand(@"select distinct USR_ID,firstname from tbluser where  USR_STATUS ='Acdchvjhmvhjmgvtive' and USR_ID not in(" + userid + ") order by tbluser.USR_ID ", con);

                cmd1.CommandType = CommandType.Text;

                MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                cmd1.Parameters.AddWithValue("@USR_ID", userid);

                DataTable dt1 = new DataTable();
                da1.Fill(dt1);
                con.Close();
                if (dt1.Rows.Count > 0)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        pdata.Add(new GetUserModel
                        {
                            firstname = dt1.Rows[i]["firstname"].ToString(),

                            USR_ID = Convert.ToInt32(dt1.Rows[i]["USR_ID"]),

                        });
                    }
                }

                return pdata;
            }
            else
            {
                return null;
            }
        }

        [Route("api/ComplainceUserMappingController/insertcompalinceusermapp")]
        [HttpPost]

        public IActionResult insertcompalinceusermapp([FromBody] object post)

        {
            var data = JsonConvert.DeserializeObject<dynamic>(post.ToString());
            if (data == null || data.ComplainceUserMapping == null || data.ComplainceUserActivityMapping == null)
            {
                return BadRequest("Invalid data.");
            }

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            // ===Transaction for rollback if any error occured in between in the insertion of data in the tables==
            using (var transaction = con.BeginTransaction()) 
            {
                try
                {
                    
                    var complainceusermappingModel = data.ComplainceUserMapping;
                    var complainceuseractivity = data.ComplainceUserActivityMapping;
                    var ComplainceEscalation = data.ComplainceEscalation;
                    //DateTime startdateTime = DateTime.Parse(complainceusermappingModel.StartDate);
                    //DateOnly startdateOnly = DateOnly.FromDateTime(startdateTime);
                    //DateTime enddateTime = DateTime.Parse(complainceusermappingModel.EndDate);
                    //DateOnly enddateOnly = DateOnly.FromDateTime(enddateTime);
                    //=========== Insertion of complaince_user_mapping Tabel =========== 
                    string insertQuerystatus = (@"insert into complaince_user_mapping(Include_Holiday_Factor,Make_Attachement_Mandatory,Attachement_Format_Allowed,PDF,doc_docx,xls_xlsx,ppt_pptx,compressed_zip,Include_Audit_Workflow,Include_Authorization_Workflow_required,Include_Review_Activity_Workflow,Include_Approve_Activity_Workflow,Include_Review_Approve_by_Same_User,No_of_Escalations,No_of_Attachements,No_of_Reminders,OverdueReminderDays,OverdueReminderPeriodicity,OverdueReminderNotification,Apply_Scheduler_On,Effective_StartDate,Effective_EndDate,company_compliance_ids,compliance_location_Mapping_ids,CreatedBy,Status,CreatedDate)values
                                                                                 (@Include_Holiday_Factor,@Make_Attachement_Mandatory,@Attachement_Format_Allowed,@PDF,@doc_docx,@xls_xlsx,@ppt_pptx,@compressed_zip,@Include_Audit_Workflow,@Include_Authorization_Workflow_required,@Include_Review_Activity_Workflow,@Include_Approve_Activity_Workflow,@Include_Review_Approve_by_Same_User,@No_of_Escalations,@No_of_Attachements,@No_of_Reminders,@OverdueReminderDays,@OverdueReminderPeriodicity,@OverdueReminderNotification,@Apply_Scheduler_On,@Effective_StartDate,@Effective_EndDate,@company_compliance_ids,@compliance_location_Mapping_ids,@CreatedBy,@Status,@CreatedDate);
            SELECT LAST_INSERT_ID();");
                    MySqlCommand myCommand11 = new MySqlCommand(insertQuerystatus, con);


                    myCommand11.Parameters.AddWithValue("@Include_Holiday_Factor", (bool)complainceusermappingModel.Include_Holiday_Factor ? 1 : 0);
                    myCommand11.Parameters.AddWithValue("@Make_Attachement_Mandatory", (bool)complainceusermappingModel.Make_Attachement_Mandatory ? 1 : 0);
                    myCommand11.Parameters.AddWithValue("@Attachement_Format_Allowed", (bool)complainceusermappingModel.Attachement_Format_Allowed ? 1 : 0);

                    if ((bool)complainceusermappingModel.Attachement_Format_Allowed)
                    {
                        myCommand11.Parameters.AddWithValue("@PDF", (bool)complainceusermappingModel.PDF ? 1 : 0);
                        myCommand11.Parameters.AddWithValue("@doc_docx", (bool)complainceusermappingModel.doc_docx ? 1 : 0);
                        myCommand11.Parameters.AddWithValue("@xls_xlsx", (bool)complainceusermappingModel.xls_xlsx ? 1 : 0);
                        myCommand11.Parameters.AddWithValue("@ppt_pptx", (bool)complainceusermappingModel.ppt_pptx ? 1 : 0);
                        myCommand11.Parameters.AddWithValue("@compressed_zip", (bool)complainceusermappingModel.compressed_zip ? 1 : 0);
                    }
                    else
                    {
                        myCommand11.Parameters.AddWithValue("@PDF", 0);
                        myCommand11.Parameters.AddWithValue("@doc_docx", 0);
                        myCommand11.Parameters.AddWithValue("@xls_xlsx", 0);
                        myCommand11.Parameters.AddWithValue("@ppt_pptx", 0);
                        myCommand11.Parameters.AddWithValue("@compressed_zip", 0);
                    }

                    myCommand11.Parameters.AddWithValue("@Include_Audit_Workflow", (bool)complainceusermappingModel.Include_Audit_Workflow ? 1 : 0);
                    myCommand11.Parameters.AddWithValue("@Include_Authorization_Workflow_required", (bool)complainceusermappingModel.Include_Authorization_Workflow_required ? 1 : 0);

                    if ((bool)complainceusermappingModel.Include_Authorization_Workflow_required)
                    {
                        myCommand11.Parameters.AddWithValue("@Include_Review_Activity_Workflow", 1);
                        myCommand11.Parameters.AddWithValue("@Include_Approve_Activity_Workflow", (bool)complainceusermappingModel.Include_Approve_Activity_Workflow ? 1 : 0);
                        myCommand11.Parameters.AddWithValue("@Include_Review_Approve_by_Same_User", (bool)complainceusermappingModel.Include_Review_Approve_by_Same_User ? 1 : 0);
                    }
                    else
                    {
                        myCommand11.Parameters.AddWithValue("@Include_Review_Activity_Workflow", (bool)complainceusermappingModel.Include_Review_Activity_Workflow ? 1 : 0);
                        myCommand11.Parameters.AddWithValue("@Include_Approve_Activity_Workflow", 0);
                        myCommand11.Parameters.AddWithValue("@Include_Review_Approve_by_Same_User", 0);
                    }

                    myCommand11.Parameters.AddWithValue("@No_of_Escalations", complainceusermappingModel.No_of_Escalations != null ? complainceusermappingModel.No_of_Escalations : 0);
                    myCommand11.Parameters.AddWithValue("@No_of_Attachements", complainceusermappingModel.No_of_Attachements != null ? complainceusermappingModel.No_of_Attachements : 0);
                    myCommand11.Parameters.AddWithValue("@No_of_Reminders", complainceusermappingModel.No_of_Reminders != null ? complainceusermappingModel.No_of_Reminders : 0);

                    myCommand11.Parameters.AddWithValue("@OverdueReminderDays", complainceusermappingModel.OverdueReminderDays != null ? complainceusermappingModel.OverdueReminderDays : 0);
                    myCommand11.Parameters.AddWithValue("@OverdueReminderPeriodicity", complainceusermappingModel.OverdueReminderPeriodicity);
                    myCommand11.Parameters.AddWithValue("@OverdueReminderNotification", complainceusermappingModel.OverdueReminderNotification);
                    myCommand11.Parameters.AddWithValue("@Apply_Scheduler_On", complainceusermappingModel.Apply_Scheduler_On);
                    myCommand11.Parameters.AddWithValue("@Effective_StartDate", complainceusermappingModel.Effective_StartDate);
                    myCommand11.Parameters.AddWithValue("@Effective_EndDate", complainceusermappingModel.Effective_EndDate != null ? complainceusermappingModel.Effective_EndDate : DBNull.Value);
                    myCommand11.Parameters.AddWithValue("@company_compliance_ids", complainceusermappingModel.company_compliance_ids);
                    myCommand11.Parameters.AddWithValue("@compliance_location_Mapping_ids", complainceusermappingModel.compliance_location_Mapping_ids);
                    myCommand11.Parameters.AddWithValue("@CreatedBy", complainceusermappingModel.CreatedBy);
                    myCommand11.Parameters.AddWithValue("@Status", "Active");
                    myCommand11.Parameters.AddWithValue("@CreatedDate", DateTime.Now);



                    int Complaince_User_Mapping_id = Convert.ToInt32(myCommand11.ExecuteScalar());

                    //=========== Insertion of complaince_user_activity_mapping Tabel =========== 


                    string insertQuery = (@"INSERT INTO complaince_user_activity_mapping (Complaince_User_Mapping_id, UpdateActivity, ReviewActivity,ApproveActivity, MonitorActivity,AuditActivity,ViewActivity,RemediationActivity,BackupUserActivity,Status, CreatedDate) 
                                                                             VALUES (@Complaince_User_Mapping_id, @UpdateActivity, @ReviewActivity,@ApproveActivity, @MonitorActivity,@AuditActivity,@ViewActivity,@RemediationActivity,@BackupUserActivity,@Status, @CreatedDate)");

                    MySqlCommand subTableCommand = new MySqlCommand(insertQuery, con);
                    subTableCommand.Parameters.AddWithValue("@Complaince_User_Mapping_id", Complaince_User_Mapping_id);
                    subTableCommand.Parameters.AddWithValue("@UpdateActivity", complainceuseractivity.UpdateActivity);
                    subTableCommand.Parameters.AddWithValue("@ReviewActivity", complainceuseractivity.ReviewActivity);
                    subTableCommand.Parameters.AddWithValue("@ApproveActivity", complainceuseractivity.ApproveActivity);
                    subTableCommand.Parameters.AddWithValue("@MonitorActivity", complainceuseractivity.MonitorActivity);
                    subTableCommand.Parameters.AddWithValue("@AuditActivity", complainceuseractivity.AuditActivity);
                    subTableCommand.Parameters.AddWithValue("@ViewActivity", complainceuseractivity.ViewActivity);
                    subTableCommand.Parameters.AddWithValue("@RemediationActivity", complainceuseractivity.RemediationActivity);
                    subTableCommand.Parameters.AddWithValue("@BackupUserActivity", complainceuseractivity.BackupUserActivity);
                    subTableCommand.Parameters.AddWithValue("@Status", "Active");
                    subTableCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    subTableCommand.ExecuteNonQuery();
                    if (ComplainceEscalation.Remindervalues != null && ComplainceEscalation.Remindervalues.Count > 0)
                    {
                        foreach (var reminder in ComplainceEscalation.Remindervalues)
                        {
                            int reminderAlertDays;
                            bool isValid = int.TryParse(reminder.reminderalertdays?.ToString(), out reminderAlertDays);
                            reminderAlertDays = isValid ? reminderAlertDays : 0;  // Default to 0 if parsing fails

                            var command = new MySqlCommand(@"INSERT INTO complaince_escalation_reminders (Complaince_User_Mapping_id,reminder_index, reminder_alert_days, periodicity_of_overdue_reminder, level_check_overdue_reminder, notification_overdue_reminder,Status,CreatedDate) VALUES 
                                                                                                 (@Complaince_User_Mapping_id,@reminder_index, @reminder_alert_days, @periodicity_of_overdue_reminder, @level_check_overdue_reminder, @notification_overdue_reminder,@Status,@CreatedDate)", con);

                            command.Parameters.AddWithValue("@Complaince_User_Mapping_id", Complaince_User_Mapping_id);
                            command.Parameters.AddWithValue("@reminder_index", reminder.remindersindex);
                           command.Parameters.AddWithValue("@reminder_alert_days", reminderAlertDays);
                           // command.Parameters.AddWithValue("@reminder_alert_days", reminder.reminderalertdays!="" || reminder.reminderalertdays!=null ? Convert.ToInt32(reminder.reminderalertdays):0);
                            command.Parameters.AddWithValue("@periodicity_of_overdue_reminder", reminder.Periodicityofoverduereminder);
                            command.Parameters.AddWithValue("@level_check_overdue_reminder", (bool)reminder.levelcheckOverduereminder ? 1 : 0);
                            command.Parameters.AddWithValue("@notification_overdue_reminder", reminder.Notification_overduereminder);
                            command.Parameters.AddWithValue("@Status", "Active");
                            command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                            command.ExecuteNonQuery();
                        }
                    }
                    if (ComplainceEscalation.LevelValues != null && ComplainceEscalation.LevelValues.Count > 0)
                    {
                        // Insert LevelValues
                        foreach (var level in ComplainceEscalation.LevelValues)
                        {
                            if (level.levelcheckusername != null && level.levelcheckusername.Count > 0)
                            {
                                bool containsOnlyNull = level.levelcheckusername.Count == 1 && level.levelcheckusername[0] == null;

                                if (!containsOnlyNull)
                                {
                                    foreach (var username in level.levelcheckusername)
                                    {
                                        var command = new MySqlCommand(@"INSERT INTO compliance_escalation_levels 
                            (Complaince_User_Mapping_id,level_index, level_check, level_check_username, level_check_overdue_reminder, CreatedDate,Status) 
                            VALUES (@Complaince_User_Mapping_id,@level_index, @level_check, @level_check_username, @level_check_overdue_reminder, @CreatedDate,@Status)", con);
                                        command.Parameters.AddWithValue("@Complaince_User_Mapping_id", Complaince_User_Mapping_id);

                                        command.Parameters.AddWithValue("@level_index", level.levelindex);
                                        command.Parameters.AddWithValue("@level_check", (bool)level.levelcheck ? 1 : 0);
                                        command.Parameters.AddWithValue("@level_check_username", username);
                                        command.Parameters.AddWithValue("@level_check_overdue_reminder", (bool)level.levelcheckOverduereminder ? 1 : 0);
                                        command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                        command.Parameters.AddWithValue("@Status", "Active");

                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                            if (level.reminders != null && level.reminders.Count > 0)
                            {
                                foreach (var reminder in level.reminders)
                                {
                                    var command = new MySqlCommand(@"INSERT INTO compliance_escalation_level_reminders 
                            (Complaince_User_Mapping_id,level_index, reminder_index, level_check_overdue_reminder, CreatedDate,Status) 
                            VALUES (@Complaince_User_Mapping_id,@level_index, @reminder_index, @level_check_overdue_reminder, @CreatedDate,@Status)", con);
                                    command.Parameters.AddWithValue("@Complaince_User_Mapping_id", Complaince_User_Mapping_id);

                                    command.Parameters.AddWithValue("@level_index", level.levelindex);
                                    command.Parameters.AddWithValue("@reminder_index", reminder.remindersindex);
                                    command.Parameters.AddWithValue("@level_check_overdue_reminder", (bool)reminder.levelcheckOverduereminder ? 1 : 0);
                                    command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                    command.Parameters.AddWithValue("@Status", "Active");

                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    
                    if (complainceusermappingModel.compliance_location_Mapping_ids != null)
                    {
                        string locationIds = complainceusermappingModel.compliance_location_Mapping_ids;
                        string[] locationIdArray = locationIds.Split(','); // Split "1,2,3" into ["1", "2", "3"]
                        foreach (var location in locationIdArray)
                        {
                            MySqlCommand cmd1 = new MySqlCommand(@"select companycomplianceid,companycompliancemappingid,locationdepartmentmappingid from compliance_location_mapping where compliance_location_Mapping_id=@compliance_location_Mapping_id ", con);

                            cmd1.CommandType = CommandType.Text;

                            MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1);
                            cmd1.Parameters.AddWithValue("@compliance_location_Mapping_id", location);

                            DataTable dt1 = new DataTable();
                            da1.Fill(dt1);

                            if (dt1.Rows.Count > 0)
                            {
                                var command = new MySqlCommand(@"INSERT INTO generate_current_batch_compliance (createdBy,createCompanyComplianceId, batchId, startDate, endDate, taskId,complaince_User_Mapping_id,locationDepartmentMappingId,updateUser,reviewUser,approveUser, monitorUser,auditUser,viewUser,remediationUser,backupUser,Status,createdDate) VALUES 
                                                     (@createdBy,@createCompanyComplianceId, @batchId, @startDate, @endDate, @taskId,@complaince_User_Mapping_id,@locationDepartmentMappingId,@updateUser, @reviewUser,@approveUser, @monitorUser,@auditUser,@viewUser,@remediationUser,@backupUser,@Status,@createdDate)", con);

                                command.Parameters.AddWithValue("@createdBy", complainceusermappingModel.CreatedBy);
                                command.Parameters.AddWithValue("@createCompanyComplianceId", Convert.ToInt32(dt1.Rows[0]["companycomplianceid"]));
                                command.Parameters.AddWithValue("@batchId", 1);
                                //command.Parameters.AddWithValue("@startDate", Convert.ToDateTime(complainceusermappingModel.Effective_StartDate).ToString("yyyy-MM-dd"));
                                //command.Parameters.AddWithValue("@endDate", complainceusermappingModel.Effective_EndDate!=null?Convert.ToDateTime(complainceusermappingModel.Effective_EndDate).ToString("yyyy-MM-dd"):null);
                                command.Parameters.AddWithValue("@startDate", complainceusermappingModel.Effective_StartDate);
                                command.Parameters.AddWithValue("@endDate", complainceusermappingModel.Effective_EndDate != null ? complainceusermappingModel.Effective_EndDate : DBNull.Value);
                                command.Parameters.AddWithValue("@Apply_Scheduler_On", complainceusermappingModel.Apply_Scheduler_On);
                                command.Parameters.AddWithValue("@taskId", dt1.Rows[0]["companycompliancemappingid"]);
                                command.Parameters.AddWithValue("@complaince_User_Mapping_id", Complaince_User_Mapping_id);
                                command.Parameters.AddWithValue("@locationDepartmentMappingId", dt1.Rows[0]["locationdepartmentmappingid"]);
                                command.Parameters.AddWithValue("@updateUser", complainceuseractivity.UpdateActivity);
                                command.Parameters.AddWithValue("@reviewUser", complainceuseractivity.ReviewActivity);
                                command.Parameters.AddWithValue("@approveUser", complainceuseractivity.ApproveActivity);
                                command.Parameters.AddWithValue("@monitorUser", complainceuseractivity.MonitorActivity);
                                command.Parameters.AddWithValue("@auditUser", complainceuseractivity.AuditActivity);
                                command.Parameters.AddWithValue("@viewUser", complainceuseractivity.ViewActivity);
                                command.Parameters.AddWithValue("@remediationUser", complainceuseractivity.RemediationActivity);
                                command.Parameters.AddWithValue("@backupUser", complainceuseractivity.BackupUserActivity);
                                command.Parameters.AddWithValue("@Status", "Active");
                                command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                                command.ExecuteNonQuery();
                            }

                        }
                    }
                    transaction.Commit();
                    //using (var scope = _serviceProvider.CreateScope())
                    //{
                    //    var controller = scope.ServiceProvider.GetRequiredService<BatchComplianceGeneration>();
                    //    controller.ProcessComplianceData();
                    //}
                    return Ok("successfully");

                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BadRequest($"Error: {ex.Message}");
                }
                finally
                {
                    con.Close();
                }
            }
        }



        [Route("api/frequencymasterforuser/GetfrequencyDetailsuser")]
        [HttpGet]

        public IEnumerable<object> GetfrequencyDetails()
        {
            var details = (from frequence in mySqlDBContext.Frequencymodels
                           where frequence.status == "Active"
                           select new
                           {
                               frequence.frequencyid,
                               frequence.frequencyperiod
                           })
                         .ToList();
            return details;
        }



        [Route("api/ComplainceUserMappingController/GetcomplainceendUsers")]
        [HttpGet]

        public IEnumerable<GetUserModel> GetcomplainceendUsers(string rolename, string locationdepartmentmappingids, string companycomplianceids)

        {

            var pdata = new List<GetUserModel>();

            try
            {
                using (MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    con.Open();

                    // Step 1: Get ROLE_ID from role name
                    MySqlCommand cmd = new MySqlCommand(@"SELECT ROLE_ID FROM tblrole WHERE ROLE_NAME = @ROLE_NAME;", con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ROLE_NAME", rolename);

                    DataTable dt = new DataTable();
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        Console.WriteLine("No ROLE_ID found for rolename: " + rolename);
                        return pdata;
                    }

                    int ROLE_ID = Convert.ToInt32(dt.Rows[0]["ROLE_ID"]);

                    // Step 2: Format locationdepartmentmappingids
                    string[] locIdsRaw = locationdepartmentmappingids.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    string[] locIds = locIdsRaw.Select(id => $"'{MySqlHelper.EscapeString(id.Trim())}'").ToArray();
                    string locationIdsStr = string.Join(",", locIds);

                    // Step 3: Format companycomplianceids
                    string[] compIdsRaw = companycomplianceids.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    string[] compIds = compIdsRaw.Select(id => $"'{MySqlHelper.EscapeString(id.Trim())}'").ToArray();
                    string complianceIdsStr = string.Join(",", compIds);

           
                    // Step 5: Final Query
                    string finalQuery = $@"
                SELECT DISTINCT USR_ID, firstname
                FROM user_workgroup_mapping uwg
                JOIN activityworkgroup awg ON awg.activity_Workgroup_id = uwg.activityworkgroup_id
                JOIN compliance_location_mapping clm ON clm.locationdepartmentmappingid = awg.locationdepartmentmappingid
                JOIN tbluser ON tbluser.USR_ID = uwg.userid
                JOIN tblrole ON tblrole.ROLE_ID = awg.roles
                WHERE clm.locationdepartmentmappingid IN ({locationIdsStr})
                  AND companycomplianceid IN ({complianceIdsStr})
                  AND awg.roles = @ROLE_ID
                
                ORDER BY tbluser.USR_ID";

                    Console.WriteLine("Executing SQL Query:");
                    Console.WriteLine(finalQuery);
                    Console.WriteLine("With ROLE_ID: " + ROLE_ID);

                    // Step 6: Execute final query
                    using (MySqlCommand cmd1 = new MySqlCommand(finalQuery, con))
                    {
                        cmd1.CommandType = CommandType.Text;
                        cmd1.Parameters.AddWithValue("@ROLE_ID", ROLE_ID);

                        using (MySqlDataAdapter da1 = new MySqlDataAdapter(cmd1))
                        {
                            DataTable dt1 = new DataTable();
                            da1.Fill(dt1);

                            Console.WriteLine("Rows fetched: " + dt1.Rows.Count);

                            if (dt1.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt1.Rows.Count; i++)
                                {
                                    pdata.Add(new GetUserModel
                                    {
                                        firstname = dt1.Rows[i]["firstname"].ToString(),
                                        USR_ID = Convert.ToInt32(dt1.Rows[i]["USR_ID"])
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetcomplainceUsers API: " + ex.Message);
            }

            return pdata;
        }

    }

}

