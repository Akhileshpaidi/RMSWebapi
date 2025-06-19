using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("penalty_category")]
    public class PenaltyCategoryModel
    {
        [Key]
        public int penalty_category_id { get; set; }
        public string penalty_category_name { get; set; }
        public string penalty_category_description { get; set; }
        public string penalty_category_status { get; set; } 
        public string penalty_category_date { get; set; }
        public string source { get; set; }

        public int createdby { get; set; }  


    }


    [Table("compliancepenatlymaster")]
    public class compliancepenatlymasterModel
    {
        [Key]
        public int compliancepenaltyid { get; set; }
        public int ruleid { get; set; }
        public int actid { get; set; }
        public string applicationselectionrule { get; set; }
        public string penaltydesc { get; set; }
        public string maxpenalty { get; set; }
        public string minpenalty { get; set; }
        public string additionalrefernce {  get; set; }
        public int createdBy { get; set; }
        public int penalty {  get; set; }
        public string createddate {  get; set; }
        public string status { get; set; }
        public string IsImportedData { get; set; }
        public string? updatedDate { get; set; }

        public int? updatedby { get; set; }
    }


    [Table("compliance_filepenalty")]

    public class PenaltyCategoryfileModel
    {
        [Key]
        public int compliance_filepenalty_id { get; set; }

        public int compliancepenaltyid { get; set; }

        public string filepath { get; set; }
        public string created_date { get; set; }
        public string filecategory { get; set; }
        public string status { get; set; }
        public string penalty_category_name { get; set; }
        public string file_name { get; set; }
        public string IsImportedData { get; set; }

        public string? updatedDate { get; set; }

        public int? updatedby { get; set; }



    }

}
