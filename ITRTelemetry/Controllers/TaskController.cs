using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySQLProvider;
using MySqlConnector;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class TaskController : ControllerBase
    {

        private readonly MySqlDBContext mySqlDBContext;

        public TaskController(MySqlDBContext mySqlDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
        }

        //Getting TaskMaster Details by taskID

        [Route("api/TaskMaster/GetTaskMasterDetails")]
        [HttpGet]

        public IEnumerable<TaskModel> GetTaskMasterDetails()
        {

            return this.mySqlDBContext.TaskModels.Where(x => x.task_status == "Active").ToList();
        }


        //Insert TaskMaster  Details

        [Route("api/TaskMaster/InsertTaskMasterDetails")]
        [HttpPost]
        public IActionResult InsertParameter([FromBody] TaskModel TaskModels)
        {
            try
            {
                TaskModels.task_name = TaskModels.task_name?.Trim();

                var existingDepartment = this.mySqlDBContext.TaskModels
                    .FirstOrDefault(d => d.task_name == TaskModels.task_name && d.task_status == "Active");

                if (existingDepartment != null)
                {
                    // Department with the same name already exists, return an error message
                    return BadRequest("Error: Taskname with the same name already exists.");
                }
                // Proceed with the insertion
                var TaskModel = this.mySqlDBContext.TaskModels;
                TaskModel.Add(TaskModels);
                DateTime dt = DateTime.Now;
                string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");
                TaskModels.task_createdDate = dt1;
                TaskModels.task_status = "Active";
                this.mySqlDBContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Taskname with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
            [Route("api/TaskMaster/UpdateTaskMasterDetails")]
        [HttpPut]
        public IActionResult UpdateTaskMaster([FromBody] TaskModel TaskModels)
        {
            try
            {
                if (TaskModels.task_id == 0)
                {

                    // Logic for handling new department (insertion) goes here
                    // You may want to return some success response
                    return Ok("Insertion successful");
                }
                else
                {
                    TaskModels.task_name = TaskModels.task_name?.Trim();

                    var existingDepartment = this.mySqlDBContext.TaskModels
                        .FirstOrDefault(d => d.task_name == TaskModels.task_name && d.task_id != TaskModels.task_id && d.task_status == "Active");

                    if (existingDepartment != null)
                    {
                        // Department with the same name already exists, return an error message
                        return BadRequest("Error: Taskname with the same name already exists.");
                    }

                    // Existing department, update logic

                    this.mySqlDBContext.Attach(TaskModels);
                    this.mySqlDBContext.Entry(TaskModels).State = EntityState.Modified;

                    var entry = this.mySqlDBContext.Entry(TaskModels);

                    Type type = typeof(TaskModel);
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(TaskModels, null) == null || property.GetValue(TaskModels, null).Equals(0))
                        {
                            entry.Property(property.Name).IsModified = false;
                        }
                    }

                    this.mySqlDBContext.SaveChanges();
                    // You may want to return some success response
                    return Ok("Update successful");
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlException mysqlException && mysqlException.Number == 1062)
                {
                    // MySQL error number 1062 corresponds to a duplicate entry violation
                    // Handle the case where the department name already exists, e.g., return an error message
                    return BadRequest("Error: Taskname with the same name already exists.");
                }
                else
                {
                    // Handle other database update exceptions
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }


    



    [Route("api/TaskMaster/DeleteTaskMasterDetails")]
        [HttpDelete]
        public void DeleteTaskMaster(int id)
        {
            var currentClass = new TaskModel { task_id = id };
            currentClass.task_status = "Inactive";
            this.mySqlDBContext.Entry(currentClass).Property("task_status").IsModified = true;
            this.mySqlDBContext.SaveChanges();
        }

    }
}
