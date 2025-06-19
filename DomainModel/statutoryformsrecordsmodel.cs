using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("statutoryforms_recordsmaster")]

    public class statutoryformsrecordsmodel
    {
        [Key]
        public int statutoryformsid { get; set; }
        public int actregulatoryid { get; set; }
        public int act_rule_regulatory_id { get; set; }

        public string recordformsname { get; set;}
        public string recordformsdesc { get; set; }
        public string applicationrefernce { get; set; }
        public string createddate { get; set; }
        public  int? createdby { get; set; }
        public string status { get; set; }
        public string IsImportedData { get; set; }
        public string? updatedDate { get; set; }

        public int? updatedby { get; set; }


    }


    [Table("statutory_forms_filemaster")]

    public class statutoryformsrecordsfilemodel
    {
        [Key]
        public int statutory_forms_filemaster_id { get; set; }

        public int statutoryformsid { get; set; }
        
        public string filepath { get; set; }
        public string created_date { get; set; }
        public string filecategory { get; set; }
        public string status { get; set; }
        public string recordformsname { get; set; }
        public string file_name { get; set; }
        public string IsImportedData { get; set; }
        public string? updatedDate { get; set; }

        public int? updatedby { get; set; }



    }




}
