using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace DomainModel
{
    [Table("act_rule_regulatory")]

    public class Rulesandregulatorymodel
    {
        [Key]
        public int act_rule_regulatory_id { get; set; }



        public string act_rule_name { get; set; }
        public int actregulatoryid { get; set; }
        public string act_rule_appl_des { get; set; }

        public int category_of_law_ID { get; set; }
        public int law_type_id { get; set; }


        public int regulatory_authority_id { get; set; }

        public int jurisdiction_category_id { get; set; }
        public int? id { get; set; }
        public int? State_id { get; set; }
        public int? jurisdiction_location_id { get; set; }
        public string type_bussiness { get; set; }
        public string bussiness_operations { get; set; }
        public string no_of_employees { get; set; }
        public string bussiness_investment { get; set; }
        public string bussiness_turnover { get; set; }
        public string working_conditions { get; set; }
        public string bussiness_registration { get; set; }
        public string other_factor { get; set; }
        //public string global_act_id { get; set; }
        public string status { get; set; }
        public string created_date { get; set; }

        public string global_rule_id { get; set; }

        public int createdBy { get; set; }
        public string IsImportedData { get; set; }


        public string? updatedDate { get; set; }

        public int? updatedby { get; set; }



    }

    [Table("act_rule_regulatory_file")]

    public class ActRuleregulatoryfilemodel
    {

        [Key]
        public int act_rule_regulatory_file_id { get; set; }

        public int act_rule_regulatory_id { get; set; }
        public string global_rule_id { get; set; }
        public string filepath { get; set; }
        public string created_date { get; set; }
        public string filecategory { get; set; }
        public string status { get; set; }
        public string act_name { get; set; }
        public string file_name { get; set; }
        public string IsImportedData { get; set; }

        public string? updatedDate { get; set; }

        public int? updatedby { get; set; }
    }



}
