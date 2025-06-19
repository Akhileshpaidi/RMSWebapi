using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("app_specific_settings")]
    public class Appspecifyconfigurtion
    {
        [Key]
        public int appspecificsettingsid { get; set;} 
        public string configuration {  get; set; }
    
         public Boolean grantpermission { get; set; }
         public  int? parameters {  get; set; }
         public string timeperiod {  get; set; }
           public int createdby { get; set; }
        public string createddate {  get; set; }

        public string status { get; set; }
    }
}
