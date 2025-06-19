using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("docsubcategory_master")]
    public class DocSubCategoryModel
    {
       [Key]
        public int Doc_SubCategoryID { get; set; }
        public string Doc_SubCategoryName { get; set; }
       public string Doc_SubCategoryDescription { get; set; }
       public int DocTypeID { get; set; }
       public int Doc_CategoryID { get; set; }
        public string Doc_Status { get; set; }
        public string Doc_createdDate { get; set; }
    }
}
