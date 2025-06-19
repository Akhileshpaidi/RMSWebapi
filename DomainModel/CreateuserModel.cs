using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{

    [Table("m_user")]
    public class  CreateuserModel
    {
        [Key]
        public int pkid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string emailid { get; set; }
        public string employeeid { get; set; }
        public string dob { get; set; }
        public string company { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string Country { get; set; }


        public string password { get; set; }
        public int defaultrole { get; set; }
        public string status { get; set; }


        public DateTime lastpwdresetdate {  get; set; }

        public int failedcount {  get; set; }
      public DateTime lastloginattempt {  get; set; }
     public DateTime lastloginsuccess {  get; set; }
    public  int createdby {  get; set; }
    public string createddate {  get; set; }
     public    int modifiedby {  get; set; }
      public DateTime modifieddate {  get; set; }

    }
}
