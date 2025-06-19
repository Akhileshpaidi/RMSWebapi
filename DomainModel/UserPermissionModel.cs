using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Cms;
using System.ComponentModel.DataAnnotations;


namespace DomainModel
{
    [Table("doc_user_permission_mapping")]
    public class UserPermissionModel
    {
        [Key]
        public int doc_user_permission_mapping_pkid { get; set; }
        public int Doc_User_Access_mapping_id { get; set; }
        public int Doc_perm_rights_id { get; set; }


        public int AddDoc_id { get; set; }

        public string ack_status { get; set; }
        public string permissioncreateddate { get; set; }
        public string permissionstatus { get; set; }
        public int USR_ID { get; set; }
        public int user_location_mapping_id { get; set; }

        public int? permissioncreatedby { get; set; }

        public int? permissionupdatedby { get; set; }
    }


    public class UserInfo
    {
        public int USR_ID { get; set; }
        public string firstname { get; set; }
    }

    public class UserRightsResult
    {
        public int user_location_mapping_id { get; set; }
        public string Doc_perm_rights_id { get; set; }

        // Add the missing properties
        public int Doc_User_Access_mapping_id { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }

        public string ack_status { get; set; }
        public string duedate { get; set; }
        public string trakstatus { get; set; }
        public string optionalreminder { get; set; }
        public int noofdays { get; set; }
        public int everyday { get; set; }
        public string timeperiod { get; set; }
        public string reqtimeperiod { get; set; }
        public string validitydocument { get; set; }


        public string provideack_status { get; set; }
    }
}
