using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using DomainModel;
using System.Data;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Bibliography;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class DefaultNotifiresController : Controller
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultNotifiresController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("api/DefaultNotifiers/GetDefaultNotifiersDetails")]
        [HttpGet]

        public IEnumerable<object> GetDepartmentMasterDetails()
        {
            //return this.mySqlDBContext.DefaultNotifiresModels.Where(x => x.Status == "Active").ToList();
            var activeNotifiers = this.mySqlDBContext.DefaultNotifiresModels
                                         .Where(x => x.Status == "Active")
                                         .ToList();

            var modifiedNotifiers = activeNotifiers.Select(x => new
            {
                x.DefaultNotifiersID,
                x.DocTypeID,
                x.Doc_CategoryID,
                x.Doc_SubCategoryID,
                emailid = x.emailid.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                additional_emailid_notifiers =(x.additional_emailid_notifiers!=null) ?
                x.additional_emailid_notifiers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) :new string [0],
                x.Status
            }).ToList();

            return modifiedNotifiers;

        }
        [Route("api/DefaultNotifiers/InsertDefaultNotifiersDetails")]
        [HttpPost]

        public IActionResult InsertParameter([FromBody] DefaultNotifiresModel DefaultNotifiresModel)
        {
            var DefaultNotifiresModels = this.mySqlDBContext.DefaultNotifiresModels;
            DefaultNotifiresModels.Add(DefaultNotifiresModel);
            //DateTime dt = DateTime.Now;
            //string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            //DefaultNotifiresModels.CreatedDate = dt1;
            DefaultNotifiresModel.Status = "Active";
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }
        [Route("api/DefaultNotifiers/UpdateDefaultNotifiersDetails")]
        [HttpPut]

        public void UpdateDefaultNotifiersDetails([FromBody] DefaultNotifiresModel DefaultNotifiresModels)
        {
            if (DefaultNotifiresModels.DefaultNotifiersID == 0)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(DefaultNotifiresModels);
                this.mySqlDBContext.Entry(DefaultNotifiresModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(DefaultNotifiresModels);

                Type type = typeof(DefaultNotifiresModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(DefaultNotifiresModels, null) == null || property.GetValue(DefaultNotifiresModels, null).Equals(0))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }
        [Route("api/DefaultNotifiers/DeleteDefaultNotifiersDetails")]
        [HttpDelete]

        public void DeleteDefaultNotifiersDetails(int id)
        {
            var currentClass = new DefaultNotifiresModel { DefaultNotifiersID = id };
            currentClass.Status = "InActive";
            this.mySqlDBContext.Entry(currentClass).Property("Status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

        [Route("api/tblUsers/GettblUsersDetails")]
        [HttpGet]

        public IEnumerable<usermodel> GettblUsersDetails()
        {
            return this.mySqlDBContext.usermodels.Where(x => x.USR_STATUS == "Active").ToList();
        }

        [Route("api/DocSubCateg/GetDocSubCategDetails")]
        [HttpGet]

        public IEnumerable<DocSubCategoryModel> GetDocSubCategDetails()
        {

            return this.mySqlDBContext.DocSubCategoryModels.Where(x => x.Doc_Status == "Active").ToList();
        }


        [Route("api/DefaultNotifiers/SaveDefaultNotifiers")]
        [HttpPost]
        public IActionResult SaveDefaultNotifiers([FromBody] DefaultNotifiresModel DefaultNotifiresModels)
        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            try
            {
                MySqlCommand cmd;
                //string userids = DefaultNotifiresModels.emailid;
                //string[] b = DefaultNotifiresModels.emailid.Split(',');
                //string[] a = b.Select(value => $"'{value}'").ToArray();
                //string useridmailid = string.Join(",", a);
                //string userids = ExtractNumbers(useridmailid);
                //string emailIds = ExtractEmailIds(useridmailid);
                //if (userids == "") {
                //    userids = "0";
                //}
                //cmd = new MySqlCommand(@"SELECT * FROM risk.tbluser where USR_ID in(" + userids + ")  ", con);


                //cmd.CommandType = CommandType.Text;
                //MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                //DataTable dt = new DataTable();
                //da.Fill(dt);
                ////con.Close();


                
                //    string emailid = string.Empty;
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        if (i > 0)
                //        {
                //            emailid += ',';
                //        }
                //        emailid += dt.Rows[i]["emailid"].ToString();
                //    }
                //    if (emailIds != "")
                //    {
                //        emailid += ',';
                //        emailid += emailIds;
                //    }
                //    if (userids == "0")
                //    {
                //        emailid = emailIds;
                //    }
                    string insertQuery = (@"insert into default_notifiers(DocTypeID,Doc_CategoryID,Doc_SubCategoryID,Status,CreatedDate,emailid,additional_emailid_notifiers)values
                                     (@DocTypeID,@Doc_CategoryID,@Doc_SubCategoryID,@Status,@CreatedDate,@emailid,@additional_emailid_notifiers)");


                    con.Open();
                    using (MySqlCommand myCommand = new MySqlCommand(insertQuery, con))
                    {
                        myCommand.Parameters.AddWithValue("@DocTypeID", DefaultNotifiresModels.DocTypeID);
                        myCommand.Parameters.AddWithValue("@Doc_CategoryID", DefaultNotifiresModels.Doc_CategoryID);
                        myCommand.Parameters.AddWithValue("@Doc_SubCategoryID", DefaultNotifiresModels.Doc_SubCategoryID);
                        myCommand.Parameters.AddWithValue("@emailid", DefaultNotifiresModels.emailid);
                        myCommand.Parameters.AddWithValue("@additional_emailid_notifiers", DefaultNotifiresModels.additional_emailid_notifiers);
                        myCommand.Parameters.AddWithValue("@Status", "Active");
                        myCommand.Parameters.AddWithValue("@CreatedDate", System.DateTime.Now);
                        myCommand.ExecuteNonQuery();
                    }



                
                return Ok("added successfully");
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


        public static string ExtractNumbers(string input)
        {
            // Regular expression to match numeric values
            string pattern = @"'(\d+)'";
            var matches = Regex.Matches(input, pattern);

            // Extract and join the numeric values back into a single string
            var result = matches.Cast<Match>()
                                .Select(match => match.Value)
                                .ToArray();
            return string.Join(",", result);
        }
        public static string ExtractEmailIds(string input)
        {
            // Define a regular expression for matching email addresses
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            var regex = new Regex(emailPattern);

            // Split the input string by commas and trim each part of leading and trailing single quotes
            var parts = input.Split(',')
                             .Select(p => p.Trim('\''))
                             .Where(p => regex.IsMatch(p));  // Filter parts that match the email pattern

            // Surround each email with single quotes and join them with commas
            return string.Join(",", parts.Select(p => $"{p}"));
        }











        [Route("api/tblUsers/GettblUsersDetailswithentity")]
        [HttpGet]

        public IActionResult GettblUsersDetailswithentity()
        {
            //return this.mySqlDBContext.usermodels.Where(x => x.USR_STATUS == "Active").ToList();

            var users = (from tbluser in mySqlDBContext.usermodels
                         join unitlocation in mySqlDBContext.UnitLocationMasterModels on tbluser.Unit_location_Master_id equals unitlocation.Unit_location_Master_id
                         join entitymaster in mySqlDBContext.UnitMasterModels on tbluser.Entity_Master_id equals entitymaster.Entity_Master_id
                         select new
                         {
                             USR_ID=tbluser.USR_ID,
                            
                          entity =   entitymaster.Entity_Master_Name,
                             
                         location =    unitlocation.Unit_location_Master_name,
                           
                            name= tbluser.firstname,
                             firstname = $"{tbluser.firstname}-{entitymaster.Entity_Master_Name}-{unitlocation.Unit_location_Master_name}",
                             tbluser.emailid

                         }).ToList();

            return Ok( users);

        }







    }







}
