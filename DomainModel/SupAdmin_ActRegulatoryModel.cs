using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DomainModel
{
    [Table("act_regulatorymaster")]
    public class SupAdmin_ActRegulatoryModel
    {
        [Key]
        public int actregulatoryid { get; set; }

        public string actregulatoryname { get; set; }
        public string actrequlatorydescription { get; set; }
        public string global_actId { get; set; }
        public string createddate { get; set; }
        public int createdBy { get; set; }
        public string status { get; set; }

        public int? updatedby {  get; set; }

        public string updateddate { get; set; }
    }

    [Table("act_regulatory")]

    public class SupAdmin_Actregulatoryfilemodel
    {

        [Key]

        public int bare_act_id { get; set; }
        public int actregulatoryid { get; set; }


        public string global_act_id { get; set; }
        public string filepath { get; set; }
        public string created_date { get; set; }
        public string filecategory { get; set; }
        public string status { get; set; }
        //  public string act_name {  get; set; }
        public string file_name { get; set; }

        public int? updatedby { get; set; }

        public string updateddate { get; set; }

    }
}
