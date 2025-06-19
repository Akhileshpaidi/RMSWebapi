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
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using System.IO;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    [ApiController]

    public class SupAdmin_RulesandRegulationsController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        private MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public SupAdmin_RulesandRegulationsController(CommonDBContext commonDBContext, IConfiguration configuration)
        {
            this.commonDBContext = commonDBContext;
            Configuration = configuration;
            this.mySqlDBContext = mySqlDBContext;
        }

        [Route("api/SupAdmin_ActRegulatory/GetrulesandregulatoryDetails")]
        [HttpGet]

        public IEnumerable<object> GetrulesandregulatoryDetails()
        {
            //return this.commonDBContext.Actregulatorymodels.Where(x => x.status == "Active").ToList();

            var act = from act_rule_regulatory in commonDBContext.SupAdmin_RulesandRegulatoryModels
                      join actregulatory in commonDBContext.SupAdmin_ActRegulatoryModels on act_rule_regulatory.actregulatoryid equals actregulatory.actregulatoryid
                      where act_rule_regulatory.status == "Active"
                      select new
                      {
                          act_rule_regulatory.global_rule_id,
                          act_rule_regulatory.act_rule_regulatory_id,
                          act_rule_regulatory.act_rule_name,
                          actregulatory.actregulatoryname,
                          actrulename = $"{actregulatory.actregulatoryname}-{act_rule_regulatory.act_rule_name}",
                          rulename = $"{act_rule_regulatory.global_rule_id}-{act_rule_regulatory.act_rule_name}"
                      };
            var result = act.ToList();


            return result;
        }

        [Route("api/SupAdmin_ActRegulatory/superGetrulesandregulatoryByID/{actid}")]
        [HttpGet]
        public IEnumerable<object> superGetrulesandregulatoryByID(int actid)
        {
            try
            {
                //return this.commonDBContext.Actregulatorymodels.Where(x => x.status == "Active").ToList();

                var act = from act_rule_regulatory in commonDBContext.SupAdmin_RulesandRegulatoryModels
                          where act_rule_regulatory.status == "Active" && act_rule_regulatory.actregulatoryid == actid
                          join act_regulator in commonDBContext.SupAdmin_ActRegulatoryModels on act_rule_regulatory.actregulatoryid equals act_regulator.actregulatoryid
                          select new
                          {
                              act_rule_regulatory.global_rule_id,
                              act_rule_regulatory.act_rule_regulatory_id,
                              act_rule_regulatory.act_rule_name,
                              act_regulator.actregulatoryname,
                              rulename = $"{act_rule_regulatory.global_rule_id}-{act_rule_regulatory.act_rule_name}"

                          };
                var result = act.ToList();

                return result;
            }
            catch (Exception ex)
            {
                // Log or handle the exception here
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; // Or any other appropriate action
            }
        }

        [Route("api/SupAdmin_ActRegulatory/InsertRuleActRegulatory")]
        [HttpPost]
        public async Task<IActionResult> InsertRuleActRegulatory()
        {
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:commondb"]);
            con.Open();

            try
            {
                // Helper function to parse integers safely
                int? ParseIntOrNull(string value)
                {
                    if (string.IsNullOrEmpty(value) || value.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        return null; // Return null for empty or "null" string
                    }
                    if (int.TryParse(value, out int result))
                    {
                        return result; // Return parsed integer
                    }
                    return null; // Return null if parsing fails
                }

                var Maxruleid = this.commonDBContext.SupAdmin_RulesandRegulatoryModels.Max(d => (int?)d.act_rule_regulatory_id) ?? 50000;

                var newruleId = Maxruleid + 1;

                var form = HttpContext.Request.Form;
                var actregulatoryid = form["actregulatoryid"].FirstOrDefault();
                var act_rule_name = form["act_rule_name"].FirstOrDefault();

                var category_of_law_ID = form["category_of_law_ID"].FirstOrDefault();
                var act_rule_appl_des = form["act_rule_appl_des"].FirstOrDefault();
                var law_type_id = form["law_type_id"].FirstOrDefault();
                var regulatory_authority_id = form["regulatory_authority_id"].FirstOrDefault();
                var jurisdiction_category_id = form["jurisdiction_category_id"].FirstOrDefault();
                var id = form["id"].FirstOrDefault();

            

                var State_id = form["State_id"].FirstOrDefault();
                var jurisdiction_location_id = form["jurisdiction_location_id"].FirstOrDefault();
                var type_bussiness = form["type_bussiness"].FirstOrDefault();
                var bussiness_operations = form["bussiness_operations"].FirstOrDefault();
                var no_of_employees = form["no_of_employees"].FirstOrDefault();
                var bussiness_investment = form["bussiness_investment"].FirstOrDefault();
                var bussiness_turnover = form["bussiness_turnover"].FirstOrDefault();
                var working_conditions = form["working_conditions"].FirstOrDefault();
                var bussiness_registration = form["bussiness_registration"].FirstOrDefault();
                var other_factor = form["other_factor"].FirstOrDefault();
                var createdBy = form["userId"].FirstOrDefault();
                var files = form.Files; // This should contain the files
                                        //var weblink = form["weblink"].FirstOrDefault();
                var weblinks = form["weblink"].ToString();

                // Get the current HTTP request
                var request = HttpContext.Request;
                string baseUrl = string.Format("{0}://{1}", request.Scheme, request.Host);
                // Generate global_act_id
                string globalactId = GenerateGlobalActId(con, int.Parse(actregulatoryid));

                var globalactIdFloder = Path.Combine("Reports", "GlobalRuleActID", globalactId);

                DirectoryInfo GlobalActIdFolderPath = Directory.CreateDirectory(globalactIdFloder);



                // Insert data into act-regulatorymaster table using insertMasterQuery
                var insertMasterQuery = "INSERT INTO act_rule_regulatory (act_rule_regulatory_id,actregulatoryid, act_rule_name,category_of_law_ID,act_rule_appl_des,law_type_id,regulatory_authority_id,id,State_id,jurisdiction_location_id,jurisdiction_category_id,type_bussiness,bussiness_operations,no_of_employees,bussiness_investment,bussiness_turnover,working_conditions,bussiness_registration,other_factor, created_date, status, global_rule_id,createdBy) " +
                                                                     "VALUES (@act_rule_regulatory_id,@actregulatoryid, @act_rule_name,@category_of_law_ID,@act_rule_appl_des,@law_type_id,@regulatory_authority_id,@id,@State_id,@jurisdiction_location_id,@jurisdiction_category_id,@type_bussiness,@bussiness_operations,@no_of_employees,@bussiness_investment,@bussiness_turnover,@working_conditions,@bussiness_registration,@other_factor, @created_date,@status, @global_rule_id,@createdBy); " +
                                        "SELECT LAST_INSERT_ID();";

                MySqlCommand masterCommand = new MySqlCommand(insertMasterQuery, con);
                masterCommand.Parameters.AddWithValue("@act_rule_regulatory_id", newruleId);
                masterCommand.Parameters.AddWithValue("@actregulatoryid", actregulatoryid);
                masterCommand.Parameters.AddWithValue("@act_rule_name", act_rule_name);
                masterCommand.Parameters.AddWithValue("@category_of_law_ID", category_of_law_ID);
                masterCommand.Parameters.AddWithValue("@act_rule_appl_des", act_rule_appl_des);
                masterCommand.Parameters.AddWithValue("@law_type_id", law_type_id);
                masterCommand.Parameters.AddWithValue("@regulatory_authority_id", regulatory_authority_id);
                masterCommand.Parameters.AddWithValue("@jurisdiction_category_id", jurisdiction_category_id);
                masterCommand.Parameters.AddWithValue("@id", ParseIntOrNull( id));
                masterCommand.Parameters.AddWithValue("@State_id", State_id);
                masterCommand.Parameters.AddWithValue("@jurisdiction_location_id", jurisdiction_location_id);
                masterCommand.Parameters.AddWithValue("@type_bussiness", type_bussiness);
                masterCommand.Parameters.AddWithValue("@bussiness_operations", bussiness_operations);
                masterCommand.Parameters.AddWithValue("@no_of_employees", no_of_employees);
                masterCommand.Parameters.AddWithValue("@bussiness_investment", bussiness_investment);
                masterCommand.Parameters.AddWithValue("@bussiness_turnover", bussiness_turnover);
                masterCommand.Parameters.AddWithValue("@working_conditions", working_conditions);
                masterCommand.Parameters.AddWithValue("@bussiness_registration", bussiness_registration);
                masterCommand.Parameters.AddWithValue("@other_factor", other_factor);
                masterCommand.Parameters.AddWithValue("@created_date", DateTime.Now);
                masterCommand.Parameters.AddWithValue("@global_rule_id", globalactId);
                masterCommand.Parameters.AddWithValue("@createdBy", createdBy);
                 masterCommand.Parameters.AddWithValue("@status", "Active");
                int insertedact_rule_regulatory_id = Convert.ToInt32(masterCommand.ExecuteScalar());

                List<string> fileList = new List<string>();


                foreach (var file in files)
                {

                    // Save the file to the directory
                    var filePath = Path.Combine(globalactIdFloder, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add the file path to the list
                    fileList.Add(filePath);


                    // Insert file attachment for each record
                    InsertFile("FileAttach", $"{baseUrl}/Reports/GlobalRuleActID/{globalactId}/{file.FileName}", file, newruleId, globalactId, file.FileName);
                }


                if (!string.IsNullOrEmpty(weblinks))
                {

                    string[] webLinksArray = weblinks.Split(';');

                    foreach (var weblink in webLinksArray)
                    {
                        // Insert each web link as a separate record
                        InsertFile("Weblink", weblink.Trim(), null, newruleId, globalactId, null);
                    }
                }

                // Function to insert file or web link
                void InsertFile(string filecategory, string filepath, IFormFile file, int newruleId, string globalactId, string fileName)
                {
                    var fileUploadModel = new SupAdmin_ActRuleregulatoryfilemodel
                    {
                        act_name = filecategory == "FileAttach" ? file?.Name : null, // Check if file is null before accessing its properties
                        filepath = filepath,
                        filecategory = filecategory,
                        status = "Active",
                        created_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        act_rule_regulatory_id = newruleId,
                        global_rule_id = globalactId,
                    };
                    var Maxrulefileid = this.commonDBContext.SupAdmin_ActRuleregulatoryfilemodels.Max(d => (int?)d.act_rule_regulatory_file_id) ?? 50000;

                    var newActfileRegulatoryId = Maxrulefileid + 1;

                    string insertSubTableQuery = "INSERT INTO commondb.act_rule_regulatory_file (act_rule_regulatory_file_id,act_rule_regulatory_id, global_rule_id, filecategory, filepath, status, created_date,file_name) " +
                                          "VALUES (@act_rule_regulatory_file_id,@act_rule_regulatory_id, @global_rule_id, @fileCategory, @filePath, @status, @createddate,@file_name)";

                    MySqlCommand subTableCommand = new MySqlCommand(insertSubTableQuery, con);
                    subTableCommand.Parameters.AddWithValue("@act_rule_regulatory_file_id", newActfileRegulatoryId);
                    subTableCommand.Parameters.AddWithValue("@act_rule_regulatory_id", newruleId);
                    subTableCommand.Parameters.AddWithValue("@global_rule_id", globalactId);
                    subTableCommand.Parameters.AddWithValue("@fileCategory", fileUploadModel.filecategory);
                    subTableCommand.Parameters.AddWithValue("@filePath", fileUploadModel.filepath);
                    subTableCommand.Parameters.AddWithValue("@status", fileUploadModel.status);
                    subTableCommand.Parameters.AddWithValue("@createddate", fileUploadModel.created_date);
                    subTableCommand.Parameters.AddWithValue("@file_name", fileName);
                    subTableCommand.ExecuteNonQuery();
                }






                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex}");
            }
        }
        private string GenerateGlobalActId(MySqlConnection con, int actregulatoryid)
        {
            string gActId;

            // Retrieve the highest existing gActId from the database
            string getMaxIdQuery = "SELECT MAX(CAST(SUBSTRING(global_actId, LENGTH(global_actId) - 3) AS UNSIGNED)) FROM commondb.act_regulatorymaster WHERE actregulatoryid = @actregulatoryid";

            using (MySqlCommand getMaxIdCommand = new MySqlCommand(getMaxIdQuery, con))
            {
                getMaxIdCommand.Parameters.AddWithValue("@actregulatoryid", actregulatoryid);
                object maxSerialNoObj = getMaxIdCommand.ExecuteScalar();
                int maxSerialNo;

                if (maxSerialNoObj != DBNull.Value && maxSerialNoObj != null)
                {
                    maxSerialNo = Convert.ToInt32(maxSerialNoObj);
                    //maxSerialNo++; // Increment the main number by 1
                }
                else
                {
                    maxSerialNo = 1; // If no existing records, set the main number to 1
                }

                // Retrieve the highest existing sub-number for the current main number
                string getSubNumberQuery = $"SELECT MAX(CAST(SUBSTRING_INDEX(SUBSTRING_INDEX(global_rule_id, '.', -1), '.', 1) AS UNSIGNED)) AS max_gc_subnumber " +
                                            $"FROM commondb.act_rule_regulatory WHERE global_rule_id LIKE 'GC{maxSerialNo:D4}.%'";

                using (MySqlCommand getSubNumberCommand = new MySqlCommand(getSubNumberQuery, con))
                {
                    object maxGCSubNumberObj = getSubNumberCommand.ExecuteScalar();
                    int maxGCSubNumber = 0;

                    if (maxGCSubNumberObj != DBNull.Value && maxGCSubNumberObj != null)
                    {
                        maxGCSubNumber = Convert.ToInt32(maxGCSubNumberObj);
                        maxGCSubNumber++; // Increment the sub-number by 1
                    }
                    else
                    {
                        maxGCSubNumber = 1; // If no existing records, set the sub-number to 1
                    }

                    gActId = $"GC{maxSerialNo:D4}.{maxGCSubNumber:D3}"; // Ensure it's formatted correctly
                }
            }

            return gActId;
        }

        [Route("api/SupAdmin_ActRegulatory/superadminGetactruleandprocudreDetails/{ruleid}")]
        [HttpGet]

        public IEnumerable<object> superadminGetactruleandprocudreDetails(int ruleid)
         {
            // Fetch initial data using LINQ query
            var result = (from act_rule_regulatory in commonDBContext.SupAdmin_RulesandRegulatoryModels
                          join actrulefilerepositiory in commonDBContext.SupAdmin_ActRuleregulatoryfilemodels
                              on act_rule_regulatory.act_rule_regulatory_id equals actrulefilerepositiory.act_rule_regulatory_id into act_rule_regulatory_join
                          from actrulefilerepositiory in act_rule_regulatory_join.DefaultIfEmpty()
                          join actregulatory in commonDBContext.SupAdmin_ActRegulatoryModels
                              on act_rule_regulatory.actregulatoryid equals actregulatory.actregulatoryid into actregulatory_join
                          from actregulatory in actregulatory_join.DefaultIfEmpty()
                          join category in commonDBContext.SupAdmin_CategoryOfLawModels
                              on act_rule_regulatory.category_of_law_ID equals category.category_of_law_ID into category_join
                          from category in category_join.DefaultIfEmpty()
                          join Jurisdiction in commonDBContext.SupAdmin_JurisdictionListModels
                              on act_rule_regulatory.jurisdiction_category_id equals Jurisdiction.jurisdiction_category_id into Jurisdiction_join
                          from Jurisdiction in Jurisdiction_join.DefaultIfEmpty()
                          join jurisdiction_location in commonDBContext.SupAdmin_JurisdictionLocationModels
                              on act_rule_regulatory.jurisdiction_location_id equals jurisdiction_location.jurisdiction_location_id into jurisdiction_location_join
                          from jurisdiction_location in jurisdiction_location_join.DefaultIfEmpty()
                          join regulator in commonDBContext.SupAdmin_RegulatoryAuthorityModels
                              on act_rule_regulatory.regulatory_authority_id equals regulator.regulatory_authority_id into regulator_join
                          from regulator in regulator_join.DefaultIfEmpty()
                          join country in commonDBContext.SupAdmin_CountryModelModels
                              on act_rule_regulatory.id equals country.id into country_join
                          from country in country_join.DefaultIfEmpty()
                          join state in commonDBContext.SupAdmin_StateModelModels
                              on act_rule_regulatory.State_id equals state.id into state_join
                          from state in state_join.DefaultIfEmpty()
                          join law_type in commonDBContext.SupAdmin_NatureOfLawModels
                              on act_rule_regulatory.law_type_id equals law_type.law_type_id into law_type_join
                          from law_type in law_type_join.DefaultIfEmpty()
                              //join tbluser in mySqlDBContext.usermodels on actregulatory.createdBy equals tbluser.USR_ID
                         // join law_type in commonDBContext.SupAdmin_NatureOfLawModels on act_rule_regulatory.law_type_id equals law_type.law_type_id
                          //join user in mySqlDBContext.usermodels on act_rule_regulatory.updatedby equals user.USR_ID into userJoin
                          //from user in userJoin.DefaultIfEmpty()
                          where act_rule_regulatory.status == "Active" && act_rule_regulatory.act_rule_regulatory_id == ruleid
                          select new
                          {
                              actruleid = act_rule_regulatory.act_rule_regulatory_id,
                              actrulename = act_rule_regulatory.act_rule_name,
                              actruledesc = act_rule_regulatory.act_rule_appl_des,
                              actname = actregulatory.actregulatoryname,
                              actdesc = actregulatory.actrequlatorydescription,
                              actid = actregulatory.actregulatoryid,
                              rulename = $"{act_rule_regulatory.global_rule_id}-{act_rule_regulatory.act_rule_name}",
                              actname1 = $"{actregulatory.global_actId}-{actregulatory.actregulatoryname}",
                              countryName = country.name ?? "",
                              stateName = state.name?? "",
                              lawType = law_type.type_of_law,
                              category = category.law_Categoryname,
                           //   countryId = jurisdiction_location.jurisdiction_country_id,
                           //   stateId = jurisdiction_location.jurisdiction_state_id,
                              jurisdiction_district = jurisdiction_location.jurisdiction_district ??"",
                              jurisdiction = Jurisdiction.jurisdiction_categoryname,
                              regulator = regulator.regulatory_authority_name,
                           
                              investment = act_rule_regulatory.bussiness_investment,
                              operations = act_rule_regulatory.bussiness_operations,
                              businesstype = act_rule_regulatory.type_bussiness,
                              turnover = act_rule_regulatory.bussiness_turnover,
                              registration = act_rule_regulatory.bussiness_registration,
                              noofemployees = act_rule_regulatory.no_of_employees,
                              other = act_rule_regulatory.other_factor,
                              workconditions = act_rule_regulatory.working_conditions,
                              sid = (int?)act_rule_regulatory.State_id ?? 0,
                              Did = (int?)act_rule_regulatory.jurisdiction_location_id ?? 0,
                              // create = $"{tbluser.firstname}-{act_rule_regulatory.created_date}",
                              //update = $"{user.firstname}-{act_rule_regulatory.updateddate}"

                          })
                         .Distinct()
                         .ToList(); // Materialize the query result

            // Execute additional queries separately
            //var ruleFiles = mySqlDBContext.ActRuleregulatoryfilemodels
            //                .Where(rf => result.Select(r => r.actid).Contains(rf.act_rule_regulatory_id))
            //                .Select(rf => new { rf.act_rule_regulatory_id, rf.filecategory, rf.filepath })
            //                .ToList();

            //var actFiles = mySqlDBContext.Actregulatoryfilemodels
            //               .Where(af => result.Select(r => r.actid).Contains(af.actregulatoryid))
            //               .Select(af => new { af.actregulatoryid, af.filecategory, af.filepath })
            //               .ToList();

            var ruleFiles = commonDBContext.SupAdmin_ActRuleregulatoryfilemodels
                     .Where(af => af.act_rule_regulatory_id == ruleid && af.status == "Active")
                    .Select(af => new
                    {
                        af.act_rule_regulatory_id,
                        af.act_rule_regulatory_file_id,
                        af.filepath,
                        af.status,
                        filecategory = af.filecategory
                    })
                                     .ToList();

            var actFiles = commonDBContext.SupAdmin_Actregulatoryfilemodels
              .Where(af => result.Select(r => r.actid).Contains(af.actregulatoryid) && af.status == "Active")
              .Select(af => new
              {
                  af.actregulatoryid,
                  af.bare_act_id,
                  af.filepath,
                  af.status,
                  filecategory = af.filecategory
              })
              .ToList();

            // Merge additional data into the original result
            var modifiedResult = result.Select(item => new
            {
                // Original properties
                item.actruleid,
                item.actid,
                item.actrulename,
                item.actruledesc,
                item.actname,
                item.actdesc,
                item.actname1,
                item.rulename,
                item.countryName,
                item.stateName,
                item.lawType,
                item.category,
              //  item.countryId,
              //  item.stateId,
                item.jurisdiction_district,
                item.jurisdiction,
                item.regulator,
                item.investment,
                item.operations,
                item.businesstype,
                item.turnover,
                item.registration,
                item.noofemployees,
                item.other,
                item.workconditions,
                //item.create,
                //item.update,
               
                // Additional properties from separate queries
                //rulefiles = ruleFiles.Where(rf => rf.act_rule_regulatory_id == item.actid)
                //                     .Select(rf => new { rf.filecategory, rf.filepath })
                //                     .ToList(),
                //actfiles = actFiles.Where(af => af.actregulatoryid == item.actid)
                //                   .Select(af => new { af.filecategory, af.filepath })
                //                   .ToList()

                rulefiles = ruleFiles.Where(af => af.act_rule_regulatory_id == item.actruleid && af.status == "Active")
                                   .Select(af => new
                                   {
                                       af.filecategory,
                                       af.act_rule_regulatory_file_id,
                                       af.filepath,
                                       filename = af.filepath != null ?
                                                  af.filepath.Substring(af.filepath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList(),

                actfiles = actFiles.Where(af => af.actregulatoryid == item.actid && af.status == "Active")
                                   .Select(af => new
                                   {
                                       af.filecategory,
                                       af.bare_act_id,
                                       af.filepath,
                                       filename = af.filepath != null ?
                                                  af.filepath.Substring(af.filepath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList()
            })
            .ToList();

            return modifiedResult;
        }


        [Route("api/SupAdmin_ActRegulatory/superadminGetactruleregulationsDetailsnyid/{ruleid}")]
        [HttpGet]

        public IEnumerable<object> superadminGetactruleregulationsDetailsnyid(int ruleid)
        {
            var result = (from act_rule_regulatory in commonDBContext.SupAdmin_RulesandRegulatoryModels
                          join actRuleFileRepository in commonDBContext.SupAdmin_ActRuleregulatoryfilemodels
                       on act_rule_regulatory.act_rule_regulatory_id equals actRuleFileRepository.act_rule_regulatory_id into actRuleRegulatoryJoin
                          from actRuleFileRepository in actRuleRegulatoryJoin.DefaultIfEmpty()
                          join actregulatory in commonDBContext.SupAdmin_ActRegulatoryModels
                              on act_rule_regulatory.actregulatoryid equals actregulatory.actregulatoryid into actregulatory_join
                          from actregulatory in actregulatory_join.DefaultIfEmpty()
                          join category in commonDBContext.SupAdmin_CategoryOfLawModels
                              on act_rule_regulatory.category_of_law_ID equals category.category_of_law_ID into category_join
                          from category in category_join.DefaultIfEmpty()
                          join Jurisdiction in commonDBContext.SupAdmin_JurisdictionListModels
                              on act_rule_regulatory.jurisdiction_category_id equals Jurisdiction.jurisdiction_category_id into Jurisdiction_join
                          from Jurisdiction in Jurisdiction_join.DefaultIfEmpty()
                          join jurisdiction_location in commonDBContext.SupAdmin_JurisdictionLocationModels
                              on act_rule_regulatory.jurisdiction_location_id equals jurisdiction_location.jurisdiction_location_id into jurisdiction_location_join
                          from jurisdiction_location in jurisdiction_location_join.DefaultIfEmpty()
                          join regulator in commonDBContext.SupAdmin_RegulatoryAuthorityModels
                              on act_rule_regulatory.regulatory_authority_id equals regulator.regulatory_authority_id into regulator_join
                          from regulator in regulator_join.DefaultIfEmpty()
                          join country in commonDBContext.SupAdmin_CountryModelModels
                              on act_rule_regulatory.id equals country.id into country_join
                          from country in country_join.DefaultIfEmpty()
                          
                          join state in commonDBContext.SupAdmin_StateModelModels
                              on act_rule_regulatory.State_id equals state.id into state_join
                          from state in state_join.DefaultIfEmpty()

                         
                          join law_type in commonDBContext.SupAdmin_NatureOfLawModels
                              on act_rule_regulatory.law_type_id equals law_type.law_type_id into law_type_join
                          from law_type in law_type_join.DefaultIfEmpty()

                              //join tbluser in mySqlDBContext.usermodels on actregulatory.createdBy equals tbluser.USR_ID
                              //  join law_type in commonDBContext.SupAdmin_NatureOfLawModels on act_rule_regulatory.law_type_id equals law_type.law_type_id
                              //join user in mySqlDBContext.usermodels on act_rule_regulatory.updatedby equals user.USR_ID into userJoin
                              //from user in userJoin.DefaultIfEmpty()
                          where act_rule_regulatory.status == "Active" && act_rule_regulatory.act_rule_regulatory_id == ruleid
                          select new
                          {
                              actruleid = act_rule_regulatory.act_rule_regulatory_id,
                              actrulename = act_rule_regulatory.act_rule_name,
                              actruledesc = act_rule_regulatory.act_rule_appl_des,
                              actname = actregulatory.actregulatoryname,
                              actdesc = actregulatory.actrequlatorydescription,
                              actid = actregulatory.actregulatoryid,
                              rulename = $"{act_rule_regulatory.global_rule_id}-{act_rule_regulatory.act_rule_name}",
                              actname1 = $"{actregulatory.global_actId}-{actregulatory.actregulatoryname}",
                              countryName = country.name ?? "",
                              stateName = state.name ?? "",
                              lawType = law_type.type_of_law,
                              category = category.law_Categoryname,
                          //  countryId = jurisdiction_location.jurisdiction_country_id,
                            //stateId = jurisdiction_location.jurisdiction_state_id,
                              jurisdiction_district = jurisdiction_location.jurisdiction_district?? "",
                              jurisdiction = Jurisdiction.jurisdiction_categoryname,
                              regulator = regulator.regulatory_authority_name,
                              investment = act_rule_regulatory.bussiness_investment,
                              operations = act_rule_regulatory.bussiness_operations,
                              businesstype = act_rule_regulatory.type_bussiness,
                              turnover = act_rule_regulatory.bussiness_turnover,
                              registration = act_rule_regulatory.bussiness_registration,
                              noofemployees = act_rule_regulatory.no_of_employees,
                              other = act_rule_regulatory.other_factor,
                              workconditions = act_rule_regulatory.working_conditions,
                              //create = $"{tbluser.firstname}-{act_rule_regulatory.created_date}",
                              global_rule_id = act_rule_regulatory.global_rule_id,
                             cntryid = act_rule_regulatory.id ?? 0,
                              sid = (int?) act_rule_regulatory.State_id ?? 0,
                              Did = (int?)act_rule_regulatory.jurisdiction_location_id ??  0,
                              catid = act_rule_regulatory.category_of_law_ID,
                              lawid = act_rule_regulatory.law_type_id,
                              authid = act_rule_regulatory.regulatory_authority_id,
                              regid = act_rule_regulatory.actregulatoryid,
                              judcatid = act_rule_regulatory.jurisdiction_category_id,
                           //  update = $"{user.firstname}-{act_rule_regulatory.updateddate}"
                          })
                      .Distinct()
                      .ToList();


            // Execute additional queries separately
            //var ruleFiles = mySqlDBContext.ActRuleregulatoryfilemodels
            //                .Where(rf => result.Select(r => r.actid).Contains(rf.act_rule_regulatory_id))
            //                .Select(rf => new { rf.act_rule_regulatory_id, rf.filecategory, rf.filepath })
            //                .ToList();

            var ruleFiles = commonDBContext.SupAdmin_ActRuleregulatoryfilemodels
                     .Where(af => af.status == "Active" && af.act_rule_regulatory_id == ruleid )
                    .Select(af => new
                    {
                        af.act_rule_regulatory_id,
                        af.act_rule_regulatory_file_id,
                        af.filepath,
                        af.status,
                        filecategory = af.filecategory,
                    })
                    .ToList(); 



            //// Merge additional data into the original result
            var modifiedResult = result.Select(item => new
            {
                // Original properties
                item.actruleid,
                item.actid,
                item.actrulename,
                item.actruledesc,
                item.actname,
                item.actdesc,
                item.actname1,
                item.rulename,
                item.countryName,
                item.stateName,
                item.lawType,
                item.category,
                //item.countryId,
                //item.stateId,
                item.jurisdiction_district,
                item.jurisdiction,
                item.regulator,
                item.investment,
                item.operations,
                item.businesstype,
                item.turnover,
                item.registration,
                item.noofemployees,
                item.other,
                item.workconditions,
                //item.create,
                item.global_rule_id,
                item.cntryid,
item.sid,
item.Did,
                item.catid,
               item.lawid,
                item.authid,
                item.regid,
                item.judcatid,
                // item.update,

                // Additional properties from separate queries
                //rulefiles = ruleFiles.Where(rf => rf.act_rule_regulatory_id == item.actid)
                //                     .Select(rf => new { rf.filecategory, rf.filepath })
                //                     .ToList(),


                rulefiles = ruleFiles.Where(af => af.act_rule_regulatory_id == item.actruleid && af.status == "Active")
                                   .Select(af => new
                                   {
                                       af.filecategory,
                                       af.act_rule_regulatory_file_id,
                                       af.filepath,
                                       filename = af.filepath != null ?
                                                  af.filepath.Substring(af.filepath.LastIndexOf('/') + 1) :
                                                  null
                                   })
                                   .ToList(),
            })
            .ToList();

            return modifiedResult;
        }


        [Route("api/SupAdmin_ActRegulatory/superadminactruleDownLoadFiles")]
        [HttpGet]
        public async Task<IActionResult> superadminactruleDownLoadFiles(string filePath)
        {
            try
            {
                // Extract the file name from the URL
                //string[] segments = filePath.Split('/');
                //string extractedFileName = segments.LastOrDefault();

                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("Invalid file name provided.");
                    return BadRequest("Invalid file name provided.");
                }

                // var filePath = Path.Combine("Reports", "GlobalAct", extractedFileName);

                // Debugging: Print file path
                Console.WriteLine($"File Path: {filePath}");

                Uri uri = new Uri(filePath);
                string relativePath = uri.LocalPath.TrimStart('/');

                // Assuming filePath is the local file path on the server
                // string localFilePath = Path.Combine("YourLocalFolderPath", relativePath);

                if (System.IO.File.Exists(relativePath))
                {
                    // Read the file content
                    byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(relativePath);

                    // Determine the file name from the local file path
                    string extractedFileName = Path.GetFileName(relativePath);

                    // Determine the file type based on the file extension or fileType parameter
                    string contentType = GetContentType(extractedFileName);

                    // Return the file content as a FileResult
                    return File(fileBytes, contentType, extractedFileName);
                }
                else
                {
                    Console.WriteLine("File does not exist at the specified path.");
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Internal Server Error: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        private string GetContentType(string fileName)
        {
            // Implement logic to determine the content type based on the file extension or fileType parameter
            // For simplicity, assume the content type based on the file extension
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                case ".docx":
                    return "application/msword";
                case ".xls":
                case ".xlsx":
                    return "application/vnd.ms-excel";
                default:
                    return "application/octet-stream"; // Default content type for binary data
            }
        }



        [Route("Api/SupAdmin_ActRegulatory/superadminUpdateActRuleRegulatory")]
        [HttpPost]

        public async Task<IActionResult> superadminUpdateActRuleRegulatory([FromQuery] int act_rule_regulatory_id)
        {
            try
            {
                var Maxrulefileid = this.commonDBContext.SupAdmin_ActRuleregulatoryfilemodels.Max(d => (int?)d.act_rule_regulatory_file_id) ?? 50000;

                var newActfileRegulatoryId = Maxrulefileid + 1;

                var rule = await commonDBContext.SupAdmin_RulesandRegulatoryModels.FirstOrDefaultAsync(a => a.act_rule_regulatory_id == act_rule_regulatory_id);

                if (rule == null)
                {
                    return NotFound();
                }

                var formCollection = await Request.ReadFormAsync();

                // Update actregulatory details
                rule.actregulatoryid = int.Parse(formCollection["actregulatoryid"]);
                rule.act_rule_name = formCollection["act_rule_name"];
                rule.act_rule_appl_des = formCollection["act_rule_appl_des"];
                rule.category_of_law_ID = int.Parse(formCollection["category_of_law_ID"]);
                rule.law_type_id = int.Parse(formCollection["law_type_id"]);
                rule.regulatory_authority_id = int.Parse(formCollection["regulatory_authority_id"]);
                rule.jurisdiction_category_id = int.Parse(formCollection["jurisdiction_category_id"]);
               // rule.id = int.Parse(formCollection["id"]);
                if (int.TryParse(formCollection["id"], out var id))
                {
                    rule.id = id;
                }

                if (int.TryParse(formCollection["State_id"], out var stateId))
                {
                    rule.State_id = stateId;
                }

                if (int.TryParse(formCollection["jurisdiction_location_id"], out var jurisdictionLocationId))
                {
                    rule.jurisdiction_location_id = jurisdictionLocationId;
                }
                rule.type_bussiness = formCollection["type_bussiness"];
                rule.bussiness_operations = formCollection["bussiness_operations"];
                rule.no_of_employees = formCollection["bussiness_operations"];
                rule.bussiness_investment = formCollection["bussiness_operations"];
                rule.bussiness_registration = formCollection["bussiness_registration"];
                rule.bussiness_turnover = formCollection["bussiness_turnover"];
                rule.working_conditions = formCollection["working_conditions"];
                rule.other_factor = formCollection["other_factor"];
                rule.updatedby = int.Parse(formCollection["updatedby"]);
                rule.updateddate = DateTime.Now.ToString("yyyy-MM-dd");


                // Handle Weblinks
                var existingWeblinks = await commonDBContext.SupAdmin_ActRuleregulatoryfilemodels
                    .Where(f => f.act_rule_regulatory_id == act_rule_regulatory_id && f.filecategory == "Weblink")
                    .ToListAsync();

                var newWeblinks = formCollection["Weblink"].ToString().Split(';', StringSplitOptions.RemoveEmptyEntries);
                var newWeblinkSet = new HashSet<string>(newWeblinks);

                // Remove existing weblinks that are not in the new weblinks
                foreach (var existingLink in existingWeblinks)
                {
                    if (!newWeblinkSet.Contains(existingLink.file_name))
                    {
                        existingLink.status = "Inactive";
                        existingLink.updateddate = DateTime.Now.ToString("yyyy-MM-dd");
                        existingLink.updatedby = int.Parse(formCollection["updatedby"]);
                        // mySqlDBContext.ActRuleregulatoryfilemodels.Remove(existingLink);
                    }
                }

                // Add new weblinks that are not in the existing weblinks
                foreach (var link in newWeblinks)
                {
                    
                    if (!existingWeblinks.Any(f => f.file_name == link))
                    {
                        commonDBContext.SupAdmin_ActRuleregulatoryfilemodels.Add(new SupAdmin_ActRuleregulatoryfilemodel
                        {
                            act_rule_regulatory_file_id = newActfileRegulatoryId++,
                            act_rule_regulatory_id = act_rule_regulatory_id,
                            file_name = link,
                            filecategory = "Weblink",
                            filepath = link,
                            global_rule_id = formCollection["global_rule_id"],
                            created_date = DateTime.Now.ToString("yyyy-MM-dd"),
                            updateddate = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedby = int.Parse(formCollection["updatedby"]),
                            status = "Active",
                       
                        });
                    }
                }

                // Handle File Attachments
                var existingFiles = await commonDBContext.SupAdmin_ActRuleregulatoryfilemodels
                    .Where(f => f.act_rule_regulatory_id == act_rule_regulatory_id && f.filecategory == "FileAttach")
                    .ToListAsync();

                var files = formCollection.Files;
                var global_rule_id = formCollection["global_rule_id"].ToString(); // Retrieve global_rule_id from the form data

                // Ensure the directory exists
                var globalactruleIdFolder = Path.Combine("Reports", "GlobalRuleActID", global_rule_id);
                Directory.CreateDirectory(globalactruleIdFolder);

                //// Remove existing file records that are not in the new files
                //foreach (var existingFile in existingFiles)
                //{
                //    if (!files.Any(f => f.FileName == existingFile.file_name))
                //    {
                //        existingFile.status = "Inactive";
                //        existingFile.updateddate = DateTime.Now.ToString("yyyy-MM-dd");
                //        existingFile.updatedby = int.Parse(formCollection["updatedby"]);
                //        //  mySqlDBContext.ActRuleregulatoryfilemodels.Remove(existingFile);
                //    }
                //}

                // Add new files and update existing files
                foreach (var file in files)
                {
                    var fileName = file.FileName;
                    var filePath = Path.Combine(globalactruleIdFolder, fileName);
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    var fileUrl = new Uri(new Uri(baseUrl), $"Reports/GlobalRuleActID/{global_rule_id}/{fileName}").ToString();

                    var existingFile = existingFiles.FirstOrDefault(f => f.file_name == fileName);
                    if (existingFile != null)
                    {
                        // Update existing file properties
                        existingFile.filecategory = "FileAttach";
                        existingFile.filepath = fileUrl;
                        existingFile.created_date = DateTime.Now.ToString("yyyy-MM-dd");

                        // Overwrite the existing file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        // Save the new file to the directory
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
               

                        // Add new file entry to the database
                        commonDBContext.SupAdmin_ActRuleregulatoryfilemodels.Add(new SupAdmin_ActRuleregulatoryfilemodel
                        {
                            act_rule_regulatory_file_id = newActfileRegulatoryId++,
                            act_rule_regulatory_id = act_rule_regulatory_id,
                            file_name = fileName,
                            filecategory = "FileAttach",
                            filepath = fileUrl,
                            global_rule_id = global_rule_id,
                            created_date = DateTime.Now.ToString("yyyy-MM-dd"),
                            updateddate = DateTime.Now.ToString("yyyy-MM-dd"),
                            updatedby = int.Parse(formCollection["updatedby"]),
                            status = "Active",
                         
                        });
                    }
                }

                await commonDBContext.SaveChangesAsync(); // Save changes for new files and weblinks

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while updating the Act Regulatory record.");
            }
        }

        [Route("api/SupAdmin_ActRegulatory/removesuprulesandregulatory/{act_rule_regulatory_file_id}")]
        [HttpPost]
        public void removesuprulesandregulatory(int act_rule_regulatory_file_id)
        {
            try
            {
                var currentClass = new SupAdmin_ActRuleregulatoryfilemodel { act_rule_regulatory_file_id = act_rule_regulatory_file_id };
                currentClass.status = "Inactive";
                currentClass.updateddate = DateTime.Now.ToString("yyyy-MM-dd");
                this.commonDBContext.Entry(currentClass).Property("status").IsModified = true;
                this.commonDBContext.SaveChanges();
            }
            catch
            {
                return;
            }
        }

    }

}
