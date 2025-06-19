using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel;
using ITR_TelementaryAPI;
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
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Web;
using System.IO;
using DocumentFormat.OpenXml.Wordprocessing;
namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    public class CustomerRegController : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        ClsGlobal obj_ClsGlobal = new ClsGlobal();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerRegController(MySqlDBContext mySqlDBContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

        }



        [Route("api/CustomerRegController/insertCustomerReg")]
        [HttpPost]

        public IActionResult insertCustomerReg()

        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;
            // Read the form data
            var form = httpRequest.Form;
            var encryptedRequestJson = form["encryptedRequest"];
            EncryptedRequestModel encryptedRequest = JsonConvert.DeserializeObject<EncryptedRequestModel>(encryptedRequestJson);
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            var mainFilePath="";
            if (form.Files .Count>0)
            {
                // Read the file
                var file = form.Files[0];
                
                //var mainFilesFolder = Directory.CreateDirectory(Path.Combine(VersionFolder, "Main"));

                mainFilePath = Path.Combine("GST-Certificate", file.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "GST-Certificate");

                // Create the folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (var stream = new FileStream(mainFilePath, FileMode.Create))
                {
                    file.CopyToAsync(stream);
                }

            }
            else
                mainFilePath = "NA";
            try
            {
                //[FromBody] EncryptedRequestModel encryptedRequest, [FromForm] IFormFile file
                //var encryptedRequest =new EncryptedRequestModel();
                string decryptedPayload = obj_ClsGlobal.DecryptAES(encryptedRequest.RequestData);
                var CustomerReg = JsonConvert.DeserializeObject<CustomerRegModel>(decryptedPayload);
                con.Open();
                MySqlCommand cmd = new MySqlCommand(@"select * from customer_registration where Status=@Status and EmailID=@EmailID ;", con);

                cmd.CommandType = CommandType.Text;

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                cmd.Parameters.AddWithValue("@Status", "Active");
                cmd.Parameters.AddWithValue("@EmailID", CustomerReg.EmailID);

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Emaild with the same name already exists, return an error message
                    return BadRequest("Email Id with the same name already exists.");
                }



                else
                {
                    string insertcmd = (@"insert into customer_registration(FirstName,LastName,MobileNumber,EmailID,OrganizationName,GSTRegistered,GSTNumber,GST_Org_name,GST_Address,Supply_Country,Supply_State,GST_Certificate,
                            Same_as_Registered_Entity,billing_org_name,billing_gst_number,billing_Address,billing_country,billing_state,Same_as_Registered_User,Grp_Admin_firstname,Grp_Admin_lastname,Grp_Admin_Mobilenumber,Grp_Admin_EmailID,Status,CreatedDate)values
                                                                     (@FirstName,@LastName,@MobileNumber,@EmailID,@OrganizationName,@GSTRegistered,@GSTNumber,@GST_Org_name,@GST_Address,@Supply_Country,@Supply_State,@GST_Certificate,
                             @Same_as_Registered_Entity,@billing_org_name,@billing_gst_number,@billing_Address,@billing_country,@billing_state,@Same_as_Registered_User,@Grp_Admin_firstname,@Grp_Admin_lastname,@Grp_Admin_Mobilenumber,@Grp_Admin_EmailID,@Status,@CreatedDate) ");
                    using (MySqlCommand myCommand12 = new MySqlCommand(insertcmd, con))
                    {
                        myCommand12.Parameters.AddWithValue("@FirstName", CustomerReg.FirstName);
                        myCommand12.Parameters.AddWithValue("@LastName", CustomerReg.LastName);
                        myCommand12.Parameters.AddWithValue("@MobileNumber", CustomerReg.MobileNumber);
                        myCommand12.Parameters.AddWithValue("@EmailID", CustomerReg.EmailID);
                        myCommand12.Parameters.AddWithValue("@OrganizationName", CustomerReg.OrganizationName);
                        myCommand12.Parameters.AddWithValue("@GSTRegistered", CustomerReg.GSTRegistered);
                        myCommand12.Parameters.AddWithValue("@GSTNumber", CustomerReg.GSTNumber !="" ?CustomerReg.GSTNumber:"NA");
                        myCommand12.Parameters.AddWithValue("@GST_Org_name", CustomerReg.GST_Org_name!=""? CustomerReg.GST_Org_name:"NA");
                        myCommand12.Parameters.AddWithValue("@GST_Address", CustomerReg.GST_Address != "" ? CustomerReg.GST_Address : "NA");
                        myCommand12.Parameters.AddWithValue("@Supply_Country", CustomerReg.Supply_Country);
                        myCommand12.Parameters.AddWithValue("@Supply_State", CustomerReg.Supply_State);
                        myCommand12.Parameters.AddWithValue("@GST_Certificate", mainFilePath);
                        myCommand12.Parameters.AddWithValue("@Same_as_Registered_Entity", CustomerReg.Same_as_Registered_Entity);
                        myCommand12.Parameters.AddWithValue("@billing_org_name", CustomerReg.billing_org_name);
                        myCommand12.Parameters.AddWithValue("@billing_gst_number", CustomerReg.billing_gst_number);
                        myCommand12.Parameters.AddWithValue("@billing_Address", CustomerReg.billing_Address);
                        myCommand12.Parameters.AddWithValue("@billing_country", CustomerReg.billing_country);
                        myCommand12.Parameters.AddWithValue("@billing_state", CustomerReg.billing_state);
                        myCommand12.Parameters.AddWithValue("@Same_as_Registered_User", CustomerReg.Same_as_Registered_User);
                        myCommand12.Parameters.AddWithValue("@Grp_Admin_firstname", CustomerReg.Grp_Admin_firstname);
                        myCommand12.Parameters.AddWithValue("@Grp_Admin_lastname", CustomerReg.Grp_Admin_lastname);
                        myCommand12.Parameters.AddWithValue("@Grp_Admin_Mobilenumber", CustomerReg.Grp_Admin_Mobilenumber);
                        myCommand12.Parameters.AddWithValue("@Grp_Admin_EmailID", CustomerReg.Grp_Admin_EmailID);
                        myCommand12.Parameters.AddWithValue("@Status", "Active");
                        myCommand12.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        //myCommand12.Parameters.AddWithValue("@Scheduled_Ass_StatusID", CustomerReg.FirstName);
                        myCommand12.ExecuteNonQuery();

                    }








                    return Ok("successfully");
                }
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




        [Route("api/CustomerRegController/GetCustomerReg")]
        [HttpGet]

        public IEnumerable<CustomerRegModel> GetCustomerReg()

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select * from customer_registration where Status=@Status;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@Status", "Active");
            
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<CustomerRegModel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    string registeredentitysamevalue = dt.Rows[i]["Same_as_Registered_Entity"].ToString().ToLower();
                    bool registeredentitysame = registeredentitysamevalue == "true" || registeredentitysamevalue == "1";
                    string registeredusersamevalue = dt.Rows[i]["Same_as_Registered_User"].ToString().ToLower();
                    bool registeredusersame = registeredusersamevalue == "true" || registeredusersamevalue == "1";

                    pdata.Add(new CustomerRegModel
                    {
                        CustomerRegID = Convert.ToInt32(dt.Rows[i]["CustomerRegID"]),

                       // FirstName = dt.Rows[i]["FirstName"].ToString() + " " + dt.Rows[i]["LastName"].ToString(),
                        FirstName = dt.Rows[i]["FirstName"].ToString(),
                        LastName = dt.Rows[i]["LastName"].ToString(),
                        MobileNumber = dt.Rows[i]["MobileNumber"].ToString(),
                        EmailID = dt.Rows[i]["EmailID"].ToString(),
                        OrganizationName = dt.Rows[i]["OrganizationName"].ToString(),

                        GSTRegistered = Convert.ToInt32(dt.Rows[i]["GSTRegistered"]),
                        GSTNumber = dt.Rows[i]["GSTNumber"].ToString(),
                        GST_Org_name = dt.Rows[i]["GST_Org_name"].ToString(),
                        GST_Address = dt.Rows[i]["GST_Address"].ToString(),
                        Supply_Country = Convert.ToInt32(dt.Rows[i]["Supply_Country"]),
                        Supply_State = Convert.ToInt32(dt.Rows[i]["Supply_State"]),
                        Same_as_Registered_Entity = registeredentitysame,
                        billing_org_name = dt.Rows[i]["billing_org_name"].ToString(),
                        billing_gst_number = dt.Rows[i]["billing_gst_number"].ToString(),
                        billing_Address = dt.Rows[i]["billing_Address"].ToString(),
                        billing_country = Convert.ToInt32(dt.Rows[i]["billing_country"]),
                        billing_state = Convert.ToInt32(dt.Rows[i]["billing_state"]),
                        Same_as_Registered_User = registeredusersame,
                        Grp_Admin_firstname = dt.Rows[i]["Grp_Admin_firstname"].ToString(),
                        Grp_Admin_lastname = dt.Rows[i]["Grp_Admin_lastname"].ToString(),
                        Grp_Admin_Mobilenumber = dt.Rows[i]["Grp_Admin_Mobilenumber"].ToString(),
                        Grp_Admin_EmailID = dt.Rows[i]["Grp_Admin_EmailID"].ToString(),
                        CreatedDate = Convert.ToDateTime(dt.Rows[i]["CreatedDate"]),

                    });




                }
            }
            return pdata;

        }


        [Route("api/CustomerRegController/GetCustomerRegByID")]
        [HttpGet]

        public IEnumerable<CustomerRegModel> GetCustomerRegByID(int CustomerRegID)

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT CustomerRegID,FirstName,LastName,MobileNumber,EmailID,OrganizationName,GSTRegistered,GSTNumber,GST_Org_name,
GST_Address,Supply_Country,Supply_State,
 Same_as_Registered_Entity,billing_org_name,billing_gst_number,billing_Address,billing_country,billing_state,Same_as_Registered_User,Grp_Admin_firstname,Grp_Admin_lastname,
 Grp_Admin_Mobilenumber,Grp_Admin_EmailID,CreatedDate,(select name from states where id=Supply_State ) as SupplyStateName,(select name from states where id=billing_state ) as billingStateName,(select name from countries where id=billing_country ) as billingcountryName,(select name from countries where id=Supply_Country ) as supplycountryName FROM risk.customer_registration where CustomerRegID=@CustomerRegID;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@CustomerRegID", CustomerRegID);

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<CustomerRegModel>();
            if (dt.Rows.Count > 0)
            {
                

                    string registeredentitysamevalue = dt.Rows[0]["Same_as_Registered_Entity"].ToString().ToLower();
                    bool registeredentitysame = registeredentitysamevalue == "true" || registeredentitysamevalue == "1";
                    string registeredusersamevalue = dt.Rows[0]["Same_as_Registered_User"].ToString().ToLower();
                    bool registeredusersame = registeredusersamevalue == "true" || registeredusersamevalue == "1";
                   string GSTRegisteredmaind="";
                if (Convert.ToInt32(dt.Rows[0]["GSTRegistered"]) == 0)
                {
                    GSTRegisteredmaind = "No";
                }
                else
                {
                    GSTRegisteredmaind = "Yes";
                }
                pdata.Add(new CustomerRegModel
                    {
                        CustomerRegID = Convert.ToInt32(dt.Rows[0]["CustomerRegID"]),

                        // FirstName = dt.Rows[0]["FirstName"].ToString() + " " + dt.Rows[0]["LastName"].ToString(),
                        FirstName = dt.Rows[0]["FirstName"].ToString(),
                        LastName = dt.Rows[0]["LastName"].ToString(),
                        MobileNumber = dt.Rows[0]["MobileNumber"].ToString(),
                        EmailID = dt.Rows[0]["EmailID"].ToString(),
                        OrganizationName = dt.Rows[0]["OrganizationName"].ToString(),

                        GSTRegistered = Convert.ToInt32(dt.Rows[0]["GSTRegistered"]),

                    GSTRegisteredmain= GSTRegisteredmaind,
                        GSTNumber = dt.Rows[0]["GSTNumber"].ToString(),
                        GST_Org_name = dt.Rows[0]["GST_Org_name"].ToString(),
                        GST_Address = dt.Rows[0]["GST_Address"].ToString(),
                        Supply_Country = Convert.ToInt32(dt.Rows[0]["Supply_Country"]),
                        Supply_State = Convert.ToInt32(dt.Rows[0]["Supply_State"]),
                        Same_as_Registered_Entity = registeredentitysame,
                        billing_org_name = dt.Rows[0]["billing_org_name"].ToString(),
                        billing_gst_number = dt.Rows[0]["billing_gst_number"].ToString(),
                        billing_Address = dt.Rows[0]["billing_Address"].ToString(),
                        billing_country = Convert.ToInt32(dt.Rows[0]["billing_country"]),
                        billing_state = Convert.ToInt32(dt.Rows[0]["billing_state"]),
                        Same_as_Registered_User = registeredusersame,
                        Grp_Admin_firstname = dt.Rows[0]["Grp_Admin_firstname"].ToString(),
                        Grp_Admin_lastname = dt.Rows[0]["Grp_Admin_lastname"].ToString(),
                        Grp_Admin_Mobilenumber = dt.Rows[0]["Grp_Admin_Mobilenumber"].ToString(),
                        Grp_Admin_EmailID = dt.Rows[0]["Grp_Admin_EmailID"].ToString(),
                    SupplyStateName = dt.Rows[0]["SupplyStateName"].ToString(),
                    billingStateName = dt.Rows[0]["billingStateName"].ToString(),
                    billingcountryName = dt.Rows[0]["billingcountryName"].ToString(),
                    supplycountryName = dt.Rows[0]["supplycountryName"].ToString(),
                        CreatedDate = Convert.ToDateTime(dt.Rows[0]["CreatedDate"]),

                    });




                }
            
            return pdata;

        }


        [Route("api/CustomerRegController/GetModules")]
        [HttpGet]

        public IEnumerable<taskmodulesmodel> GetModules()

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"SELECT * FROM risk.task_master where task_status=@task_status;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@task_status", "Active");

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<taskmodulesmodel>();
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    

                    pdata.Add(new taskmodulesmodel
                    {
                        task_id = Convert.ToInt32(dt.Rows[i]["task_id"]),

                        task_name = dt.Rows[i]["task_name"].ToString(),
                        task_desc = dt.Rows[i]["task_desc"].ToString(),
                       

                    });




                }
            }
            return pdata;

        }


        [Route("api/CustomerRegController/insertSubscribtiondetails")]
        [HttpPost]

        public IActionResult insertSubscribtiondetails()

        
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;
            // Read the form data
            var form = httpRequest.Form;
            var encryptedRequestJson = form["encryptedRequest"];
            EncryptedRequestModel encryptedRequest = JsonConvert.DeserializeObject<EncryptedRequestModel>(encryptedRequestJson);
            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

            try
            {

                string decryptedPayload = obj_ClsGlobal.DecryptAES(encryptedRequest.RequestData);
                var CustomerReg = JsonConvert.DeserializeObject<customer_subscriptionmodel>(decryptedPayload);
                con.Open();
                string insertcmd = (@"insert into customer_subscription(CustomerRegID,task_ids,No_of_Users,No_of_Companys,No_of_Locations,BankName,IFSC_Code,BankAccountNo,Amt_Collection_Date,Unique_Sub_ID,Status,CreatedDate,Subscriptiontype,FromDate,ToDate)values
                           (@CustomerRegID,@task_ids,@No_of_Users,@No_of_Companys,@No_of_Locations,@BankName,@IFSC_Code,@BankAccountNo,@Amt_Collection_Date,@Unique_Sub_ID,@Status,@CreatedDate,@Subscriptiontype,@FromDate,@ToDate) ");

                using (MySqlCommand myCommand12 = new MySqlCommand(insertcmd, con))
                {
                    myCommand12.Parameters.AddWithValue("@CustomerRegID", CustomerReg.CustomerRegID);
                    myCommand12.Parameters.AddWithValue("@task_ids", CustomerReg.task_ids);
                    myCommand12.Parameters.AddWithValue("@No_of_Users", CustomerReg.No_of_Users);
                    myCommand12.Parameters.AddWithValue("@No_of_Companys", CustomerReg.No_of_Companys);
                    myCommand12.Parameters.AddWithValue("@No_of_Locations", CustomerReg.No_of_Locations);
                    myCommand12.Parameters.AddWithValue("@BankName", CustomerReg.BankName);
                    myCommand12.Parameters.AddWithValue("@IFSC_Code", CustomerReg.IFSC_Code);
                    myCommand12.Parameters.AddWithValue("@BankAccountNo", CustomerReg.BankAccountNo);
                    myCommand12.Parameters.AddWithValue("@Amt_Collection_Date", CustomerReg.Amt_Collection_Date);
                    myCommand12.Parameters.AddWithValue("@Unique_Sub_ID", CustomerReg.Unique_Sub_ID);
                    myCommand12.Parameters.AddWithValue("@Status", "Active");
                    myCommand12.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    myCommand12.Parameters.AddWithValue("@Subscriptiontype", CustomerReg.Subscriptiontype);
                    myCommand12.Parameters.AddWithValue("@FromDate", CustomerReg.FromDate);
                    myCommand12.Parameters.AddWithValue("@ToDate", CustomerReg.ToDate);
                    myCommand12.ExecuteNonQuery();

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


        [Route("api/CustomerRegController/Getuniquesequence")]
        [HttpGet]

        public IEnumerable<customer_subscriptionmodel> Getuniquesequence()

        {

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(@"select Unique_Sub_ID from customer_subscription where Status=@Status order by Unique_Sub_ID  desc limit 1;", con);

            cmd.CommandType = CommandType.Text;

            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            cmd.Parameters.AddWithValue("@Status", "Active");

            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            var pdata = new List<customer_subscriptionmodel>();
            if (dt.Rows.Count > 0)
            {
                  pdata.Add(new customer_subscriptionmodel
                    {
                        Unique_Sub_ID = dt.Rows[0]["Unique_Sub_ID"].ToString(),
                    });

            }
            else
            {
                return new List<customer_subscriptionmodel>();
            }
            return pdata;
            }
        



    }
}
