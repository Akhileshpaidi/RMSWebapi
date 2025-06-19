using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using Org.BouncyCastle.Bcpg;
using ITR_TelementaryAPI.Models;
using ITRTelemetry.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Ubiety.Dns.Core;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;


namespace ITRTelemetry.Controllers
{
    
        [Produces("application/json")]

        //[ApiController]
        public class RoleController : ControllerBase
        {
            private readonly MySqlDBContext mySqlDBContext;
        public IConfiguration Configuration { get; }

        public RoleController(MySqlDBContext mySqlDBContext, IConfiguration configuration)
            {
                this.mySqlDBContext = mySqlDBContext;
               Configuration = configuration;
        }

            //Getting Role Details

            [Route("api/RoleDetails/GetRoleDetails")]
            [HttpGet]

        public IEnumerable<object> GetRoleDetails()
        {
            try
            {
                var deatils1 = (from tblrolemaster in mySqlDBContext.RoleModels
                                join taskmaster in mySqlDBContext.TaskModels on tblrolemaster.task_id equals taskmaster.task_id
                                where (tblrolemaster.ROLE_STATUS == 0 || tblrolemaster.ROLE_STATUS == 1) && taskmaster.task_status == "Active"
                                select new
                                {
                                    tblrolemaster.ROLE_ID,
                                    tblrolemaster.ROLE_NAME,
                                    tblrolemaster.ROLE_DESC,
                                    tblrolemaster.ROLE_STATUS,
                                    taskmaster.task_id,
                                    taskmaster.task_name,
                                    tblrolemaster.roletype
                                }).ToList();

                return deatils1;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching role details", ex);
            }
        }

    [Route("api/RoleDetails/GetRolesListBy/{moduleID}")]
        [HttpGet]

        public IEnumerable<object> GetRolesListBy(int moduleID)
        {
            var role_list = (from role_types in mySqlDBContext.typeofrolemodels
                             join modules in mySqlDBContext.TaskModels on role_types.task_id equals modules.task_id
                                  where (role_types.task_id == moduleID) 
                                  select new
                                  {
                                      role_types.roletypeid,
                                      role_types.roletypename

                                  })
                                 .ToList();
            return role_list;
        }

        [Route("api/RoleDetails/GetComponentsList")]
        [HttpGet]
        public IEnumerable<object> GetComponentsList([FromQuery] int module_id)
        {
            try
            {
                var component_list = (from components in mySqlDBContext.Componentmodels
                                      where components.task_id == module_id && components.status == "0"
                                      group components by components.menu_item into groupedComponents
                                      select new
                                      {
                                          MenuItemName = groupedComponents.Key,
                                          Components = groupedComponents.Select(c => new
                                          {
                                              c.id,
                                              c.name,
                                              c.description,
                                              c.status,
                                              c.mandatory
                                          }).ToList()
                                      }).ToList();
                return component_list;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching Components List", ex);
            }
        }

        //public IEnumerable<object> GetComponentsList([FromQuery] int module_id)
        //{
        //    try
        //    {
        //        var component_list = (from components in mySqlDBContext.Componentmodels
        //                              where components.task_id == module_id
        //                              select new
        //                              {
        //                                  components.id,
        //                                  components.name,
        //                                  components.description,
        //                                  components.status,
        //                                  components.mandatory

        //                              })
        //                              .ToList();
        //        return component_list;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("An error occurred while fetching Components List", ex);
        //    }
        //}
        //public IEnumerable<object> GetComponentsList()
        //{
        //    try
        //    {
        //        var component_list = (from components in mySqlDBContext.Componentmodels
        //                              group components by components.menu_item into groupedComponents
        //                              select new
        //                              {
        //                                  MenuItemName = groupedComponents.Key,
        //                                  Components = groupedComponents.Select(c => new
        //                                  {
        //                                      c.id,
        //                                      c.name,
        //                                      c.description,
        //                                      c.status,
        //                                      c.mandatory
        //                                  }).ToList()
        //                              }).ToList();
        //        return component_list;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("An error occurred while fetching Components List", ex);
        //    }
        //}



        [Route("api/RoleDetails/GetcomplainceRoleDetails")]
        [HttpGet]

        public IEnumerable<object> GetcomplainceRoleDetails()
        {
            try
            {
                var deatils1 = (from tblrolemaster in mySqlDBContext.RoleModels
                                join taskmaster in mySqlDBContext.TaskModels on tblrolemaster.task_id equals taskmaster.task_id
                                where (tblrolemaster.ROLE_STATUS == 0 || tblrolemaster.ROLE_STATUS == 1) && taskmaster.task_status == "Active" && tblrolemaster.task_id == 3
                                select new
                                {
                                    tblrolemaster.ROLE_ID,
                                    tblrolemaster.ROLE_NAME,
                                    tblrolemaster.ROLE_DESC,
                                    tblrolemaster.ROLE_STATUS,
                                    taskmaster.task_id,
                                    taskmaster.task_name
                                })

                               .ToList();
                return deatils1;
            }
            catch(Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching role details", ex);
            }

        }

        [Route("api/RoleDetails/GetRoleDetailsbyid")]
        [HttpGet]

        public IEnumerable<object> GetRoleDetailsbyid(int taskid, string roletype)
        {
            try
            {

                var deatils1 = (from tblrolemaster in mySqlDBContext.RoleModels

                                join taskmaster in mySqlDBContext.TaskModels on tblrolemaster.task_id equals taskmaster.task_id
                                where tblrolemaster.ROLE_STATUS == 0 && taskmaster.task_status == "Active" && tblrolemaster.task_id == taskid && tblrolemaster.roletype == int.Parse(roletype)
                                select new
                                {
                                    tblrolemaster.ROLE_ID,
                                    tblrolemaster.ROLE_NAME,
                                    tblrolemaster.ROLE_DESC,
                                    taskmaster.task_id,
                                    taskmaster.task_name


                                })

                    .ToList();



                return deatils1;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching role details", ex);
            }
        }



        [Route("api/RoleDetails/DeleteRole")]
        [HttpDelete]

        public void DeleteRole(int Role_id)
        {
            try
            {
                var currentClass = new RoleModel { ROLE_ID = Role_id };
                currentClass.ROLE_STATUS = 1;
                this.mySqlDBContext.Entry(currentClass).Property("ROLE_STATUS").IsModified = true;
                this.mySqlDBContext.SaveChanges();

            }
            catch(Exception ex) 
            {
                throw new ApplicationException("An error occurred while Deleteing role", ex);
                    }
        }


        //get components based on role
        [Route("api/RoleDetails/GetRoleComponentsByID")]
        [HttpGet]
        public IActionResult GetRoleComponentsByID(int roleId)
                {
                    MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);

                    try
                    {
                            con.Open();

                            string selectRoleQuery = "SELECT ROLE_NAME, ROLE_DESC,tblrole.task_id,task_master.task_name FROM tblrole Left join task_master on task_master.task_id = tblrole.task_id WHERE ROLE_ID = @RoleId";

        
                            string selectComponentsQuery = "SELECT componentid,mandatory FROM map_role_component WHERE roleid = @RoleId";

                            RoleDetailsDto roleDetails = new RoleDetailsDto();

                            // Retrieve role data
                            using (MySqlCommand command = new MySqlCommand(selectRoleQuery, con))
                            {
                                command.Parameters.AddWithValue("@RoleId", roleId);

                                using (MySqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        roleDetails.RoleName = reader["ROLE_NAME"].ToString();
                                        roleDetails.RoleDesc = reader["ROLE_DESC"].ToString();
                                        roleDetails.typemodule = reader["task_name"].ToString();
                                        //roleDetails.roletypename = reader["roletypename"].ToString();
                                        roleDetails.task_id = (int) reader["task_id"];
                                        //roleDetails.roletypeid = (int)reader["roletypeid"];
                        }
                                }
                            }

        // Retrieve component IDs
        //using (MySqlCommand myCommand = new MySqlCommand(selectComponentsQuery, con)) 
        //{
        //    myCommand.Parameters.AddWithValue("@RoleId", roleId);

        //    List<int> componentIds = new List<int>();
        //    using (MySqlDataReader componentReader = myCommand.ExecuteReader())
        //    {
        //        while (componentReader.Read())
        //        {
        //            componentIds.Add((int)componentReader["componentid"]);
        //        }
        //    }

        //    roleDetails.ComponentIds = componentIds.ToArray();
        //}
           using (MySqlCommand myCommand = new MySqlCommand(selectComponentsQuery, con))
                {
                    myCommand.Parameters.AddWithValue("@RoleId", roleId);

                    List<ComponentDetail> componentDetailsList = new List<ComponentDetail>();

                    using (MySqlDataReader componentReader = myCommand.ExecuteReader())
                    {
                        while (componentReader.Read())
                        {
                            int componentId = (int)componentReader["componentid"];
                            string mandatoryStatus = (string)componentReader["mandatory"];

                            var componentDetail = new ComponentDetail
                            {
                                ComponentIds = componentId,
                                Mandatory = mandatoryStatus
                            };

                            componentDetailsList.Add(componentDetail);
                        }
                    }

                    roleDetails.ComponentDetails = componentDetailsList;
                }

                return Ok(roleDetails);
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







        //insert role

        [Route("api/RoleDetails/InsertRoleDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] RoleModel RoleModels)
        {
            var RoleModel = this.mySqlDBContext.RoleModels;
            RoleModel.Add(RoleModels);
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            RoleModels .CREATE_DATE = dt;
            RoleModels.ROLE_STATUS = 1;
            this.mySqlDBContext.SaveChanges();
            return Ok();
        }


        [Route("api/RoleDetails/CreateRole")]
        [HttpPost]
        public IActionResult CreateRole([FromBody] CreateRole RoleModels)
        {
            string ismandatory ="false"; 

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string insertQuery = "insert into tblrole(ROLE_NAME,ROLE_DESC,ROLE_STATUS,CREATE_DATE,task_id,created_by,roletype)values(@ROLE_NAME,@ROLE_DESC,@ROLE_STATUS,@CREATE_DATE,@task_id,@created_by,@roletype)";
            string selectQuery = "select ROLE_NAME from tblrole where ROLE_NAME= @roleName and ROLE_STATUS = 0";
            try
            {
                con.Open();

                using (MySqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    using (MySqlCommand mycommand = new MySqlCommand(selectQuery, con, transaction))
                    {
                        mycommand.Parameters.AddWithValue("@roleName", RoleModels.rolename);

                        using (MySqlDataReader reader = mycommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (RoleModels.rolename == reader["ROLE_NAME"].ToString())
                                {
                                        return Conflict("Role Already Exist");
                                    }
                            }
                        }
                    }
                    using (MySqlCommand myCommand1 = new MySqlCommand(insertQuery, con, transaction))
                    {

                        myCommand1.Parameters.AddWithValue("@ROLE_NAME", RoleModels.rolename);
                        myCommand1.Parameters.AddWithValue("@ROLE_DESC", RoleModels.description);
                        myCommand1.Parameters.AddWithValue("@task_id", RoleModels.task_id);
                        myCommand1.Parameters.AddWithValue("@created_by", RoleModels.created_by);
                        myCommand1.Parameters.AddWithValue("@ROLE_STATUS", "0");
                        myCommand1.Parameters.AddWithValue("@CREATE_DATE", DateTime.Now);
                        myCommand1.Parameters.AddWithValue("@roletype", RoleModels.roletype);


                            myCommand1.ExecuteNonQuery();

                        // Get the last inserted primary key value
                        int roleID = Convert.ToInt32(myCommand1.LastInsertedId.ToString());
                        int[] components = RoleModels.componentid;
                        int[] mandatorycomponents = RoleModels.mandatory;
                        for (int i = 0; i < components.Length; i++)
                        {

                            string insertQuery1 = "insert into map_role_component(roleid,componentid,mandatory)values(@roleid,@componentid,@mandatory)";


                            using (MySqlCommand myCommand2 = new MySqlCommand(insertQuery1, con, transaction))
                            {
                                    for (int j = 0; j < mandatorycomponents.Length; j++)
                                    {
                                        if (mandatorycomponents[j].ToString() == components[i].ToString())
                                        {
                                            myCommand2.Parameters.AddWithValue("@roleid", roleID);
                                            myCommand2.Parameters.AddWithValue("@componentid", components[i]);
                                            myCommand2.Parameters.AddWithValue("@mandatory", "yes");
                                            myCommand2.ExecuteNonQuery();
                                            ismandatory = "true";
                                        }

                                    }
                                    if (ismandatory != "true")
                                    {
                                        myCommand2.Parameters.AddWithValue("@roleid", roleID);
                                        myCommand2.Parameters.AddWithValue("@componentid", components[i]);
                                        myCommand2.Parameters.AddWithValue("@mandatory", "no");
                                        myCommand2.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        ismandatory = "false";
                                    }


                                }
                        }



                    }

                    transaction.Commit();
                        return Ok("0");
                    }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                    }
                finally
                {
                    con.Close();
                }
            }
        }
            catch (Exception ex)
            {

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            finally
            {
                con.Close();
            }
        }




        [Route("api/RoleDetails/UpdateRoleComponents")]
        [HttpPut]
        public IActionResult UpdateRoleComponents([FromBody] CreateRole RoleModels)
        {
            string ismandatory = "false";

            MySqlConnection con = new MySqlConnection(Configuration["ConnectionStrings:myDb1"]);
            string updateQuery = "UPDATE tblrole SET ROLE_NAME = @ROLE_NAME, ROLE_DESC = @ROLE_DESC, updated_by = @updated_by, updated_date = @updated_date WHERE ROLE_ID = @ROLE_ID";
            string insertMappingQuery = "insert into map_role_component(roleid,componentid,mandatory)values(@roleid,@componentid,@mandatory)";
            string deleteMappingQuery = "DELETE FROM map_role_component WHERE roleid = @roleid";
            string selectQuery = "select ROLE_NAME from tblrole where ROLE_NAME= @ROLE_NAME and ROLE_STATUS = 0 and ROLE_ID != @ROLE_ID";

            try
            {
                con.Open();

                // Start a transaction
                using (MySqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        using (MySqlCommand mycommand = new MySqlCommand(selectQuery, con, transaction))
                        {
                            mycommand.Parameters.AddWithValue("@ROLE_NAME", RoleModels.rolename);
                            mycommand.Parameters.AddWithValue("@ROLE_ID", RoleModels.roleid);

                            using (MySqlDataReader reader = mycommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (RoleModels.rolename == reader["ROLE_NAME"].ToString())
                                    {
                                        return Conflict("Role Already Exist");
                                    }
                                }
                            }
                        }
                        //update existing role name and description for the role
                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, con, transaction))
                        {

                            updateCommand.Parameters.AddWithValue("@ROLE_ID", RoleModels.roleid);
                            updateCommand.Parameters.AddWithValue("@ROLE_NAME", RoleModels.rolename);
                            updateCommand.Parameters.AddWithValue("@ROLE_DESC", RoleModels.description);
                            updateCommand.Parameters.AddWithValue("@updated_by", RoleModels.updated_by);
                            updateCommand.Parameters.AddWithValue("@updated_date", DateTime.Now);
                            updateCommand.ExecuteNonQuery();
                        }
                        //Delete existing component mappings for the role
                        using (MySqlCommand deleteCommand = new MySqlCommand(deleteMappingQuery, con, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@roleid", RoleModels.roleid);
                            //deleteCommand.Parameters.AddWithValue("@mandatory", "no");
                            deleteCommand.ExecuteNonQuery();
                        }

                        int[] components = RoleModels.componentid;
                        int[] mandatorycomponents = RoleModels.mandatory;
                        for (int i = 0; i < components.Length; i++)
                        {

                            string insertQuery1 = "insert into map_role_component(roleid,componentid,mandatory)values(@roleid,@componentid,@mandatory)";


                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery1, con, transaction))
                            {
                                for (int j = 0; j < mandatorycomponents.Length; j++)
                                {
                                    if (mandatorycomponents[j].ToString() == components[i].ToString())
                                    {
                                        insertCommand.Parameters.AddWithValue("@roleid", RoleModels.roleid);
                                        insertCommand.Parameters.AddWithValue("@componentid", components[i]);
                                        insertCommand.Parameters.AddWithValue("@mandatory", "yes");
                                        insertCommand.ExecuteNonQuery();
                                        ismandatory = "true";
                                    }

                                }
                                if (ismandatory != "true")
                                {
                                    insertCommand.Parameters.AddWithValue("@roleid", RoleModels.roleid);
                                    insertCommand.Parameters.AddWithValue("@componentid", components[i]);
                                    insertCommand.Parameters.AddWithValue("@mandatory", "no");
                                    insertCommand.ExecuteNonQuery();
                                }
                                else
                                {
                                    ismandatory = "false";
                                }


                            }
                        }

                        transaction.Commit();
                        return Ok("0");
                    }
                    
                    catch (Exception ex)
                    {
                        // An error occurred, rollback the transaction
                        transaction.Rollback();
                        return StatusCode(500, $"An error occurred: {ex.Message}");
                        // Re-throw the exception to signal that an error occurred
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            finally
            {
                // Ensure the connection is closed
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }

        }






        [Route("api/RoleDetails/UpdateRoleDetails")]
        [HttpPut]

        public void UpdateRoleModel([FromBody] RoleModel RoleModels)
        {
            if (RoleModels.ROLE_ID == 1)
            {
            }
            else
            {
                this.mySqlDBContext.Attach(RoleModels);
                this.mySqlDBContext.Entry(RoleModels).State = EntityState.Modified;

                var entry = this.mySqlDBContext.Entry(RoleModels);

                Type type = typeof(RoleModel);
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(RoleModels, null) == null || property.GetValue(RoleModels, null).Equals(1))
                    {
                        entry.Property(property.Name).IsModified = false;
                    }
                }

                this.mySqlDBContext.SaveChanges();
            }
        }
        //[Route("api/RoleDetails/RoleDetails")]
        //[HttpDelete]

        //public void DeleteRoleDetails(int id)
        //{
        //    var currentClass = new RoleModel { ROLE_ID = id };
        //    currentClass.ROLE_STATUS = 2 ;
        //    this.mySqlDBContext.Entry(currentClass).Property("ROLE_STATUS").IsModified = true;
        //    this.mySqlDBContext.SaveChanges();
        //}


        [Route("api/roleDetails/UpdateroleStatus")]
        [HttpDelete]

        public IActionResult UpdateroleStatus(int roleid)
        {
            try
            {


                var role = this.mySqlDBContext.RoleModels.FirstOrDefault(r => r.ROLE_ID == roleid);

                if (role != null)
                {
                    role.ROLE_STATUS = role.ROLE_STATUS == 0 ? 1 : 0;

                    this.mySqlDBContext.Entry(role).Property("ROLE_STATUS").IsModified = true;
                    this.mySqlDBContext.SaveChanges();

                }
                return Ok("Ok");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        public class RoleDetailsDto
        {
            public string RoleName { get; set; }
            public string RoleDesc { get; set; }
            public string typemodule { get; set; }
           // public string roletypename { get; set; }
            public int task_id { get; set; }
           // public int roletypeid { get; set; }

            public List<ComponentDetail> ComponentDetails { get; set; }
        }

        public class ComponentDetail
        {
            public int ComponentIds { get; set; }
            public string Mandatory { get; set; }
        }



        [Route("api/RoleDetails/GettypeRoleDetailsbyid/{module}")]
        [HttpGet]

        public IEnumerable<object> GettypeRoleDetailsbyid(int module)
        {
            // return this.mySqlDBContext.RoleModels.Where(x => x.ROLE_STATUS == 0).ToList();

            var deatils1 = (from tblrolemaster in mySqlDBContext.RoleModels
                            join roletype in mySqlDBContext.typeofrolemodels on tblrolemaster.roletypeid equals roletype.roletypeid
                            join taskmaster in mySqlDBContext.TaskModels on tblrolemaster.task_id equals taskmaster.task_id
                            where tblrolemaster.ROLE_STATUS == 0 && taskmaster.task_status == "Active" && tblrolemaster.task_id == module
                            select new
                            {
                                tblrolemaster.ROLE_ID,
                                tblrolemaster.ROLE_NAME,
                                tblrolemaster.ROLE_DESC,
                                taskmaster.task_id,
                                taskmaster.task_name,
                                roletype.roletypeid,
                                roletype.roletypename


                            })
                 .Distinct()
                .ToList();



            return deatils1;
        }

    }

}



    
