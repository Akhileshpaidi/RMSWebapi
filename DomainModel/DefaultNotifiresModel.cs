using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("default_notifiers")]
    public class DefaultNotifiresModel
    {
        [Key]
        public int DefaultNotifiersID { get; set; }

        public int DocTypeID { get; set; }
        public  int Doc_CategoryID { get; set; }
        public int Doc_SubCategoryID { get; set; }
        public string emailid { get; set; }
        public string ? additional_emailid_notifiers { get; set; }
        public string Status { get; set; }
    
       // public DateTime? CreatedDate { get; set; }

    }
}
