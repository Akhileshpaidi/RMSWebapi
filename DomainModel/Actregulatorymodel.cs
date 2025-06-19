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
    [Table("act_regulatorymaster")]

    public class Actregulatorymodel
    {

        [Key]
        public int actregulatoryid {  get; set; }

        public string actregulatoryname {  get; set; }
        public string actrequlatorydescription {  get; set; }
        public string global_actId { get; set; }
        public string createddate { get; set; }
        public int createdBy { get; set; }
        public string status { get; set; }
        public string IsImportedData { get; set; }

        public string updatedDate { get; set; }

        public  int? updatedby { get; set; }

        //  public Actregulatoryfilemodel[] references { get; set; }

    }



    [Table("act_regulatory")]

    public class Actregulatoryfilemodel
    {

        [Key]
        public int bare_act_id { get; set; }
        public int actregulatoryid { get; set; }

     
        public string global_act_id { get; set; }
        public string filepath {  get; set; }
        public string created_date { get; set; }
        public string filecategory { get; set; }
        public string status { get; set; }
      //  public string act_name {  get; set; }
        public string file_name { get; set; }
        public string IsImportedData { get; set; }

        public string updatedDate { get; set; }

        public int? updatedby { get; set; }



    }






    [Table("states")]


    public class statesModel
    {

        [Key]

      
        public int State_id { get; set; }
        public string name { get; set; }
       
        public int country_id { get; set; }
        public string country_code { get; set; }
        public int id { get; set; }
     


    }





}
