using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{

    [Table("department-locationmapping")]
    public  class Departmentlocationmappingmodel
    {
        [Key]
     public int   locationdepartmentmappingid {  get; set; }
        public string departmentid { get; set; }
        public string unitlocationid { get; set; }
        public int entityid { get; set; }
        public int createdby { get; set; }
        public string createddate { get; set; }

        public string status { get; set; }
    }
    public class UpdateData2
    {
        public Departmentlocationmappingmodel Departmentlocationmappingmodels { get; set; }
        // Other properties may be defined here if needed
    }
}
