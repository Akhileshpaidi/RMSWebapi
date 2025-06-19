using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("doccategory_master")]

    public class DocCategoryMasterModel
    {
     
        
            [Key]
            public int Doc_CategoryID { get; set; }
            public string Doc_CategoryName { get; set; }
            public string Doc_CategoryDescription { get; set; }
            public int DocTypeID { get; set; }
            public string doccategory_status { get; set; }

          public string doccategory_createdDate { get; set; }
       
    }
}
