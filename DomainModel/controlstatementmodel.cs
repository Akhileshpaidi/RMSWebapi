using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DomainModel
{
    [Table("risk_control_statement")]

    public class controlstatementmodel
    {

        [Key]
        public int control_statement_id { get; set; }

        public string control_objective_heading { get; set; }
        public string control_brief_description { get; set; }
        public string control_detailed_description { get; set; }

        public string addsubcontrolcheckboxField { get; set; }

        public int controlmeasureid { get; set; }
        public string add_sub_control { get; set; }
        public string created_date { get; set; }
        public int createdBy { get; set; }
        public string status { get; set; }


       public string globalcontrolId { get; set; }

        //public int? updatedby { get; set; }


    }



    [Table("risk_control_statement_attach_doc")]

    public class controlstatementfilemodel
    {

        [Key]
        public int control_statement_attach_doc_id { get; set; }
        public int control_statement_id { get; set; }

        public string filepath { get; set; }
        //public string created_date { get; set; }
        public string filecategory { get; set; }
        public string status { get; set; }
         public string globalcontrolId {  get; set; }
        public string file_name { get; set; }

        public string created_date { get; set; }
         
        //public int? updatedby { get; set; }



    }



    [Table("risk_control_statement_sub")]

    public class controlsubstatementmodel
    {

        [Key]
        public int control_statement_sub_id { get; set; }
        public int control_statement_id { get; set; }

        public string checkboxField2 { get; set; }
        public string sub_control_name { get; set; }
        public string confirm_dependency { get; set; }

        public string dependent_sub_control_id { get; set; }
        public string created_date { get; set; }
        
        public string status { get; set; }
        //  public string act_name {  get; set; }
        //public string file_name { get; set; }
        //public string IsImportedData { get; set; }

        //public string updatedDate { get; set; }

        //public int? updatedby { get; set; }



    }

}
