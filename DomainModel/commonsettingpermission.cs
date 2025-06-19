using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("commonappsettings")]
    public class commonsettingpermission
    {
        [Key]
        public int commonappsettingsid {  get; set; }

        public string configuration {  get; set; }

        public Boolean permission {  get; set; }

        public int filesize {  get; set; }

        public int createdby {  get; set; }

        public string createddate { get; set; }

        public  string  status {  get; set; } 
    }
}
