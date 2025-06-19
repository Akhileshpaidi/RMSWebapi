using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Data;
using ITRTelemetry.IOtpService;
using ITR_TelementaryAPI;
using NuGet.Versioning;
using Microsoft.EntityFrameworkCore;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    //[Route("api/Login")]
    //[ApiController]
    public class LoginController : ControllerBase
    {
        private ClsGlobal obj_ClsGlobal = new ClsGlobal();
        private readonly MySqlDBContext mySqlDBContext;

        private ClsEmail obj_Clsmail = new ClsEmail();
        private readonly OtpService _otpService;
        //private OtpService _otpService = new OtpService;

        public IConfiguration Configuration { get; }

        public LoginController(MySqlDBContext mySqlDBContext,IConfiguration configuration, OtpService otpService)
        {
            this.mySqlDBContext = mySqlDBContext;
            Configuration = configuration;
            _otpService = otpService;
        }

        // GET: api/<LoginController>
        // GET: api/Login
        [HttpGet]
        public IEnumerable<UserLogin> Get()
        {
            return this.mySqlDBContext.UserLogin.ToList();
        }

        //// GET api/<LoginController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // GET: api/Login/5
        [Route("api/Login/CheckUser")]
        [HttpPost]

        public object Get([FromBody] UserLogin loginModel)
        {
            //string result = "Invalid";
            string result = "0";
            var check = (from userlogin in this.mySqlDBContext.UserLogin select new { userlogin.UserName, userlogin.Password }).Where(x => x.UserName == loginModel.UserName && x.Password == loginModel.Password);

            if (check.FirstOrDefault() != null)
            {
                //result = "Exist";
                result = "1";
                return result;
            }

            else
            {
                return result;
            }

            //var check = (from userlogin in this.mySqlDBContext.UserLogin
            //             join roles in this.mySqlDBContext.Roles on userlogin.RoleId equals roles.ROLE_ID
            //             select new
            //             {
            //                 userlogin.LoginID,//vishnu
            //                 userlogin.UserName,
            //                 userlogin.Password,
            //                 userlogin.FirstName,
            //                 userlogin.RoleId,
            //                 roles.ROLE_NAME,
            //                 roles.ROLE_STATUS,
            //             }).Where(x => x.UserName == loginModel.UserName && x.Password == loginModel.Password ).ToList();
            //return check;
        }

        //public string Get([FromBody] UserLogin loginModel)
        //{
        //    string result = "Invalid";
        //    var check = (from userlogin in this.mySqlDBContext.Login select new { userlogin.UserName, userlogin.Password }).Where(x => x.UserName == loginModel.UserName && x.Password == loginModel.Password);

        //    if (check.FirstOrDefault() != null)
        //    {
        //        result = "Exist";
        //        return result;
        //    }

        //    else
        //    {
        //        return result;
        //    }
        //}

        // POST api/<LoginController>
        [HttpPost]
        public void Post([FromBody] string value)
        {



        }

        // PUT api/<LoginController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LoginController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

        }


        //[Route("api/Authenticate/Login")]
        //[HttpPost]

        [Route("api/ValidateLoginAndSendOtp")]
        [HttpPost]
        public async Task<IActionResult> ValidateLoginAndSendOtp([FromBody] usermodel loginModel)
        {
            var res = new Response();

            try
            {
                var encryptedPassword = obj_ClsGlobal.EncryptAES(loginModel.password);

                using (var transaction = await mySqlDBContext.Database.BeginTransactionAsync())
                {
                    var user = await mySqlDBContext.usermodels
                        .Where(u => u.USR_LOGIN == loginModel.USR_LOGIN && u.password == encryptedPassword && u.USR_STATUS == "Active")
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        var otp = new Random().Next(100000, 999999).ToString();

                        await _otpService.SaveOtpAsync(user.USR_ID, otp); // Save OTP
                        DateTime expiryTime = await _otpService.SaveOtpAsync(user.USR_ID, otp);
                        obj_Clsmail.SendAuthenticationEmail(user.emailid, otp); // Send email

                        await transaction.CommitAsync(); // Commit only after both steps

                        res.ResponseCode = "1";
                        res.ResponseTime = expiryTime;
                        res.ResponseData = obj_ClsGlobal.EncryptAES(JsonConvert.SerializeObject(new { userId = user.USR_ID }));
                    }
                    else
                    {
                        res.ResponseCode = "0";
                        res.ResponseDesc = "Invalid credentials";
                    }
                }
            }
            catch (Exception ex)
            {
                res.ResponseCode = "3";
                res.ResponseDesc = ex.Message;
            }

            return Ok(res);
        }

        [Route("api/VerifyOtpAndReturnUserData")]
        [HttpPost]
        public async Task<IActionResult> VerifyOtpAndReturnUserData([FromBody] UserOtpModel model)
        {
            var res = new Response();

            try
            {
                bool isValid = await _otpService.ValidateOtpAsync(model.userId, model.otp);

                if (!isValid)
                {
                    res.ResponseCode = "0";
                    res.ResponseDesc = "Invalid or expired OTP";
                    return Ok(res);
                }
                else
                {
                    res.ResponseCode = "1";
                    res.ResponseDesc = "verified";
                    return Ok(res);

                }
//                using (var obj_con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
//                {
//                    string userdata = @"SELECT
//                                    JSON_OBJECT(
//                                            'userid', tbluser.USR_ID,
//                                            'firstname', tbluser.firstname,
//                                            'lastname', tbluser.lastname,
//                                             'userlogin',tbluser.USR_LOGIN,
//                                            'useremailid', tbluser.emailid,
                                            
//                                            'EmployeeID', tbluser.EmployeeID,
//                                            'lastloginsuccess', tbluser.lastloginaccess,
//                                            'mobilenumber', tbluser.mobilenumber,
//                                             'tpauserid',tbluser.tpauserid,
//                                            'Entity_Master_id', tbluser.Entity_Master_id,
//                                                'country',entity_master.Entity_Master_Name,
//                                             'Unit_location_Master_id',tbluser .Unit_location_Master_id,
//                                              'state',unit_location_master .Unit_location_Master_name,
//											'Department_Master_id', tbluser.Department_Master_id,
//                                             'departmentName', department_master.Department_Master_name,

//'roles', (
//                             SELECT GROUP_CONCAT(tblrole.ROLE_NAME ORDER BY map_user_role.ROLE_ID SEPARATOR ', ')
//                             FROM tblrole
//                             INNER JOIN map_user_role ON map_user_role.ROLE_ID = tblrole.ROLE_ID
//                             WHERE map_user_role.USR_ID = tbluser.USR_ID
//                         ), 'taskid', GROUP_CONCAT( DISTINCT  map_user_role.taskid ORDER BY map_user_role.taskid SEPARATOR ', ')
//                                        ) AS userprofile,
//                                        JSON_ARRAYAGG(
//                                            JSON_OBJECT(
//                                                'roleid', tblrole.ROLE_ID,
//                                                'rolename', tblrole.ROLE_NAME,
//                                                'component', (
//                                                    SELECT JSON_ARRAYAGG(
//                                                        JSON_OBJECT(
//                                                            'componentid', m_appcomponent.id,
//                                                            'componentname', m_appcomponent.name,
                                                            
//                                                           'status', CASE WHEN componentid IS NOT NULL THEN 0 ELSE 1 END  
//                                                        )
//                                                    )
//                                                    FROM risk.map_role_component
//                                                    LEFT JOIN risk.m_appcomponent ON( map_role_component.componentid = m_appcomponent.id
//                                                    and  risk.map_role_component.roleid = tblrole.ROLE_ID)
//                                                      where    m_appcomponent.status = '0'
//                                                )
//                                            )
//                                        ) AS roles
//                                    FROM
//                                        risk.tbluser
//                                    INNER JOIN
//                                        risk.map_user_role ON tbluser.USR_ID = map_user_role.USR_ID
//                                    INNER JOIN
//                                        risk.tblrole ON map_user_role.ROLE_ID = tblrole.ROLE_ID
// LEFT JOIN
//                                       risk.entity_master on tbluser.Entity_Master_id = entity_master.Entity_Master_id
//                                    LEFT JOIN
//                                          risk.department_master on tbluser.Department_Master_id = department_master.Department_Master_id
//                                  LEFT JOIN
//                                      risk. unit_location_master on tbluser.Unit_location_Master_id = unit_location_master.Unit_location_Master_id
                                 
//                                    WHERE
//                                        risk.tbluser.USR_ID =@USR_ID
//                                    GROUP BY
//                                        risk.tbluser.USR_ID;";


//                    using (MySqlCommand obj_comm = new MySqlCommand(userdata, obj_con))
//                    {
//                        obj_comm.Parameters.AddWithValue("@USR_ID", model.userId);
//                        obj_con.Open();

//                        using (MySqlDataReader reader = obj_comm.ExecuteReader())
//                        {
//                            var userProfile = "";
//                            var rolesArray = "";

//                            if (reader.Read())
//                            {
//                                userProfile = reader.GetString(reader.GetOrdinal("userprofile"));
//                                rolesArray = reader.GetString(reader.GetOrdinal("roles"));
//                                var userResponseData = new
//                                {
//                                    profile = JsonConvert.DeserializeObject(userProfile),
//                                    role = new { roles = JsonConvert.DeserializeObject(rolesArray) }
//                                };
//                                //  res.ResponseData = JsonConvert.SerializeObject(responseData);
//                                // If you're encrypting:
//                                res.ResponseData = obj_ClsGlobal.EncryptAES(JsonConvert.SerializeObject(userResponseData));
//                                res.ResponseCode = "1";
//                                res.ResponseDesc = "OTP Verified, Login Success";
//                            }
//                            else
//                            {
//                                res.ResponseCode = "0";
//                                res.ResponseDesc = "User not Found";
//                            }
//                        }

//                        obj_comm.Dispose();

//                    }

//                    if (obj_con.State != ConnectionState.Closed)
//                        obj_con.Close();
//                    obj_con.Dispose();
//                }
            }
            catch (Exception ex)
            {
                res.ResponseCode = "3";
                res.ResponseDesc = ex.Message;
            }

            return Ok(res);
        }

        [Route("api/Authenticate/login")]
        [HttpPost]

        public IActionResult Login([FromBody] usermodel loginModel)
        {

            var res = new Response();
            try
            {


                using (MySqlConnection obj_con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]))
                {
                    string query = @"SELECT
                                    JSON_OBJECT(
                                            'userid', tbluser.USR_ID,
                                            'firstname', tbluser.firstname,
                                            'lastname', tbluser.lastname,
                                             'userlogin',tbluser.USR_LOGIN,
                                            'useremailid', tbluser.emailid,
                                            'isFirstLogin',tbluser.isFirstLogin,
                                            'EmployeeID', tbluser.EmployeeID,
                                            'lastloginsuccess', tbluser.lastloginaccess,
                                            'mobilenumber', tbluser.mobilenumber,
                                             'tpauserid',tbluser.tpauserid,
                                            'Entity_Master_id', tbluser.Entity_Master_id,
                                                'country',entity_master.Entity_Master_Name,
                                             'Unit_location_Master_id',tbluser .Unit_location_Master_id,
                                              'state',unit_location_master .Unit_location_Master_name,
											'Department_Master_id', tbluser.Department_Master_id,
                                             'departmentName', department_master.Department_Master_name,

'roles', (
                             SELECT GROUP_CONCAT(tblrole.ROLE_NAME ORDER BY map_user_role.ROLE_ID SEPARATOR ', ')
                             FROM tblrole
                             INNER JOIN map_user_role ON map_user_role.ROLE_ID = tblrole.ROLE_ID
                             WHERE map_user_role.USR_ID = tbluser.USR_ID
                         ), 'taskid', GROUP_CONCAT( DISTINCT  map_user_role.taskid ORDER BY map_user_role.taskid SEPARATOR ', ')
                                        ) AS userprofile,
                                        JSON_ARRAYAGG(
                                            JSON_OBJECT(
                                                'roleid', tblrole.ROLE_ID,
                                                'rolename', tblrole.ROLE_NAME,
                                                'component', (
                                                    SELECT JSON_ARRAYAGG(
                                                        JSON_OBJECT(
                                                            'componentid', m_appcomponent.id,
                                                            'componentname', m_appcomponent.name,
                                                            
                                                           'status', CASE WHEN componentid IS NOT NULL THEN 0 ELSE 1 END  
                                                        )
                                                    )
                                                    FROM risk.map_role_component
                                                    LEFT JOIN risk.m_appcomponent ON( map_role_component.componentid = m_appcomponent.id
                                                    and  risk.map_role_component.roleid = tblrole.ROLE_ID)
                                                      where    m_appcomponent.status = '0'
                                                )
                                            )
                                        ) AS roles
                                    FROM
                                        risk.tbluser
                                    INNER JOIN
                                        risk.map_user_role ON tbluser.USR_ID = map_user_role.USR_ID
                                    INNER JOIN
                                        risk.tblrole ON map_user_role.ROLE_ID = tblrole.ROLE_ID
 LEFT JOIN
                                       risk.entity_master on tbluser.Entity_Master_id = entity_master.Entity_Master_id
                                    LEFT JOIN
                                          risk.department_master on tbluser.Department_Master_id = department_master.Department_Master_id
                                  LEFT JOIN
                                      risk. unit_location_master on tbluser.Unit_location_Master_id = unit_location_master.Unit_location_Master_id
                                 
                                    WHERE
                                        risk.tbluser.USR_LOGIN =@userLogin and 
                                        risk.tbluser.password =@password  and 
                                        risk.tbluser.USR_STATUS =@USR_STATUS
                                    GROUP BY
                                        risk.tbluser.USR_ID;
                                    "
                                                        ;

                    using (MySqlCommand obj_comm = new MySqlCommand(query, obj_con))
                    {
                        obj_comm.Parameters.AddWithValue("@userLogin", loginModel.USR_LOGIN);
                        obj_comm.Parameters.AddWithValue("@password", obj_ClsGlobal.EncryptAES(loginModel.password));
                        obj_comm.Parameters.AddWithValue("@USR_STATUS", "Active");
                        obj_con.Open();

                        using (MySqlDataReader reader = obj_comm.ExecuteReader())
                        {
                            var userProfile = "";
                            var rolesArray = "";

                            if (reader.Read())
                            {
                                 userProfile = reader.GetString(reader.GetOrdinal("userprofile"));
                                 rolesArray = reader.GetString(reader.GetOrdinal("roles"));
                                var responseData = new
                                {
                                    profile = JsonConvert.DeserializeObject(userProfile),
                                    role = new { roles = JsonConvert.DeserializeObject(rolesArray) }
                                };
                              //  res.ResponseData = JsonConvert.SerializeObject(responseData);
                                // If you're encrypting:
                                 res.ResponseData = obj_ClsGlobal.EncryptAES(JsonConvert.SerializeObject(responseData));
                                res.ResponseCode = "1";
                                res.ResponseDesc = "Success";
                            }
                            else
                            {
                                res.ResponseCode = "0";
                                res.ResponseDesc = "User not found or invalid credentials";
                            }
                        }

                        obj_comm.Dispose();
                      
                    }

                    if (obj_con.State != ConnectionState.Closed)
                        obj_con.Close();
                    obj_con.Dispose();
                   
                }


                
            }




            catch (Exception ex)
            {
                res.ResponseCode = "3";
                res.ResponseDesc = ex.Message;
                res.ResponseData = "";
                //obj_ds.Dispose();
                //obj_comm.Dispose();
                //obj_adap.Dispose();
                //if (obj_con.State != ConnectionState.Closed)
                //    obj_con.Close();
                //obj_con.Dispose();
            }
            return Ok(res);
        }

        public class Response
        {
            public string ResponseCode { get; set; }
            public string ResponseDesc { get; set; } // Add this line if it's missing
            public string ResponseData { get; set; }
            public DateTime ResponseTime { get; set; }
        }
    }
}
