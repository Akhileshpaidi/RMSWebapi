using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
namespace DomainModel
{
    [Table("tblrole")]
    public class RoleModel
    {
        [Key]
        public int ROLE_ID { get; set; }
        public string ROLE_NAME { get; set; }
        public string ROLE_DESC { get; set; }
        public string ROLE_MODULE { get; set; }
        public string ROLE_PRIVILEGES { get; set; }
        public int ROLE_STATUS { get; set; }

        public int? task_id {  get; set; }
        public int? roletypeid { get; set; }
        public DateTime CREATE_DATE { get; set; }
        public int? roletype { get; set; }
    }
    public class CreateRole
    {
        [Key]
        public int roleid { get; set; }
        public string authid { get; set; }
        public int[] componentid { get; set; }
        public string rolename { get; set; }
        public string description { get; set; }
        public string typemodule { get; set; }
        public int task_id { get; set; }
        public int roletypeid { get; set; }
        public int[] mandatory { get; set; }
        public string created_by { get; set; }
        public string updated_by { get; set; }
        public int? roletype { get; set; }
    }
}
