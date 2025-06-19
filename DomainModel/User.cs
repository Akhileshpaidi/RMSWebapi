using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DomainModel
{
    [Table("tbluser")]
   
    public class usermodel
    {
        [Key]
        public int USR_ID { get; set; }
        public string firstname { get; set;}
        public string lastname { get; set; }
        public string USR_LOGIN { get; set; }
        public string password { get; set; }


        public int? EmployeeID {  get; set; }
        public int ROLE_ID { get; set; }
        public string USR_STATUS { get; set; }
        public string CREATED_DATE { get; set; }
        public string mobilenumber { get; set; }
        public string emailid { get; set; }
        public string roles { get; set; }
        public string dob { get; set; }
        public string company { get; set; }
        public string city { get; set; }
        public string state { get; set; }


        public string country { get; set; }
        public int defaultrole { get; set; }
        public string lastpwdresetdate { get; set; }
        public int? failedcount { get; set; }

        public string lastloginattempt { get; set; }

        public string lastloginaccess { get; set; }

        public int? createdby { get; set; }
        public int? modifiedby { get; set; }
        public string modifieddate { get; set; }

        public int Department_Master_id { get; set; }    
 

        public int Entity_Master_id { get; set; }
        public int Unit_location_Master_id { get; set; }

        public string Designation { get; set; }

        public string taskids {  get; set; }

        public int typeofuser { get; set; }

        public int? tpaenityid {  get; set; }

     //  public string? tpadescription {  get; set; }

        public int? tpauserid { get; set; }
        public int isFirstLogin { get; set; }

    }

     public class provideusermodel
    {
        public int USR_ID { get; set; }
        public string firstname { get; set; }

        public int user_location_mapping_id { get; set; }    
        public int Entity_Master_id { get; set; }
  public int Unit_location_Master_id  { get; set; }
        public string Unit_location_Master_name { get; set; }
        public string Entity_Master_Name { get; set; }

        public int Department_Master_id { get; set; }
       public string Department_Master_name {  get; set; }


    }
    public class EncryptedRequestModel
    {
        public string RequestData { get; set; }
    }

    public class ChangePasswordModel
    {
        public int userid { get; set; } 

            public int? tpauserid { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class raisequeryModel
    {
        [Key]
        public int raisequeryid { get; set; }
        public string queryImportance { get; set; }
        public int taskid { get; set; }
        public int menuItem { get; set; }
        public int componentid { get; set; }
        public int reportingpersonid { get; set; }
        public int userid { get; set; }
        public string subjectTitle { get; set; }
        public string issueDetails { get; set; }
        public string filesdata { get; set; }
        public string trackingNo { get; set; }
        public string createdDate { get; set; }
        public string status { get; set; }
        public string resolutionCategory { get; set; }
        public string resolutionDetails { get; set; }
        public string resolutionfiles { get; set; }
        public string resolutionQueryDate { get; set; }
        public string reportingPersonEmail { get; set; }
        public string userEmail { get; set; }
    }
    public class raisequeryfilesModel
    {
        [Key]
        public int raisequeryfilesid { get; set; }
        public string fileName { get; set; }
        public string fileType { get; set; }
        public byte[] fileData { get; set; }
        public string uploadedBy { get; set; }
        public string uploadedDate { get; set; }
        public string trackingNo { get; set; }
        public string userid { get; set; }
    }
    public class reviewqueryfilesModel
    {
        [Key]
        public int reviewqueryfilesid { get; set; }
        public string fileName { get; set; }
        public string fileType { get; set; }
        public byte[] fileData { get; set; }
        public string uploadedBy { get; set; }
        public string uploadedDate { get; set; }
        public string trackingNo { get; set; }
        public string reviewuserid { get; set; }
    }
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Recipient { get; set; } // Email, User ID, or Phone Number
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    public class mailnotification
    {
        public string ReportingPersonEmail { get; set; }
        public string IssueTitle { get; set; }
        public string IssueDescription { get; set; }
        public int SenderId { get; set; }
        public int ReportingPersonId { get; set; }
        public string BaseUrl { get; set; }
        public string[] DocumentNames { get; set; }
    }
}
