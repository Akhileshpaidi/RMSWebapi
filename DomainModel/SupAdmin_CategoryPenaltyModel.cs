using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("compliancepenatlymaster")]
    public class SupAdmin_CategoryPenaltyModel
    {              
            [Key]
            public int compliancepenaltyid { get; set; }
            public int ruleid { get; set; }
            public int actid { get; set; }
            public string applicationselectionrule { get; set; }
            public string penaltydesc { get; set; }
            public string maxpenalty { get; set; }
            public string minpenalty { get; set; }
            public string additionalrefernce { get; set; }
            public int createdBy { get; set; }
            public int penalty { get; set; }
            public string createddate { get; set; }
            public string status { get; set; }
        public int? updatedby { get; set; }

        public string updateddate { get; set; }
    }


    [Table("compliance_filepenalty")]

    public class SupAdmin_PenaltyCategoryfileModel

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
        public int? updatedby { get; set; }

        public string updateddate { get; set; }

    }
}

