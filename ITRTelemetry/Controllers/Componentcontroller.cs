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

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class Componentcontroller : ControllerBase
    {
        private readonly MySqlDBContext mySqlDBContext;
        public Componentcontroller(MySqlDBContext mySqlDBContext)

        {
            this.mySqlDBContext = mySqlDBContext;
        }
        private UserData GetUserData(int userId)
        {
            // Your database query logic here
            // This is a placeholder, and you should replace it with actual database queries
            // For example, using Entity Framework for database queries in a real application
            UserData userData = new UserData
            {
                Profile = new UserProfile
                {
                    UserId = 23,
                    FirstName = "Vishnu",
                    // ... other profile data
                },
                Role = new UserRole
                {
                    Roles = new List<Role>
                {
                    new Role
                    {
                        RoleId = 1,
                        RoleName = "Admin",
                        // ... other role data
                    }
                }
                }
            };
            return userData;
        }

        // Define an API endpoint to retrieve user data
        //[HttpGet]
        //[Route("api/user/{userId}")]
        //public IHttpActionResult GetUser(int userId)
        //{
        //    UserData userData = GetUserData(userId);

        //    if (userData == null)
        //    {
        //        return NotFound(); // Or any other appropriate HTTP status code
        //    }

        //    return Ok(new { ResponseData = userData });
        //}

    public class UserData
    {
        public UserProfile Profile { get; set; }
        public UserRole Role { get; set; }
    }

    public class UserProfile
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        // ... other profile properties
    }

    public class UserRole
    {
        public List<Role> Roles { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        // ... other role properties
    }
    [Route("api/componentlist/GetcomponentlistDetails")]
        [HttpGet]
        public IEnumerable<Componentmodel> GetcomponentlistDetails()
        {
            //return this.mySqlDBContext.Componentmodels.ToList();
            return this.mySqlDBContext.Componentmodels.Where(x => x.status == "1").ToList();
        }
    }
}
