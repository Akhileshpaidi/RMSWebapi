using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("activitylog")]
    public class ActivitylogModel
    {

        [Key]
        public int ActivityLogID { get; set; }
        public string ActivityType { get; set; }
        public int UserID { get; set; }
        public string Status { get; set; }
        public string DocumentFileName { get; set; }
        public string DocumentType { get; set; }
        public DateTime CreatedDate { get; set; }
        public int VendorID { get; set; }
    }
}
