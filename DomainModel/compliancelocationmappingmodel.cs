using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DomainModel
{
    [Table("compliance_location_mapping")]
    public class compliancelocationmappingmodel
    {
        [Key]
        public int compliance_location_Mapping_id { get; set; }
        public string companycomplianceid { get; set; }
        public string locationdepartmentmappingid { get; set; }
        public string createdDate { get; set; }
        public string status { get; set; }
        public int createdby { get; set; }

        public string companycompliancemappingid {  get; set; }
    }

    public class UpdateData7
    {
        public compliancelocationmappingmodel compliancelocationmappingmodels { get; set; }
        // Other properties may be defined here if needed
    }
}
