using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{ 
    [Table("risksamplingstandards")]
    public  class risksamplingstandards
    {


        [Key]
        public int risksamplingstandardsId { get; set; }  // Primary key
        public string samplingAuthority { get; set; }
        public int frequencyControl { get; set; }

        public keyControls KeyControls { get; set; }

        public  nonKeyControls nonKeyControls { get; set; }

        public generalControls generalControls { get; set; }


        //public int createdby {  get; set; }

        public string createddate { get; set; } 

        public string status { get; set; }
    }



    [Table("risksamplingkeyControls")]
    public class keyControls

    {
        [Key]
        public int keycontrolsid {  get; set; }

        public Boolean? keyControlscheckedstatus {  get; set; }
        public string controltype {  get; set; }
        public int? keyControlsstartValue { get; set; }
        public int? keyControlsendValue { get; set; }
        public string keyControlstransactionDescription { get; set; }
        public int? keyControlsrangeStartValue { get; set; }
        public string keyControlspercentageDescription { get; set; }
        public string keyControlstextDescription { get; set; }

        public int risksamplingstandardsId { get; set; }
    }



    [Table("risksamplingnonKeyControls")]
    public class nonKeyControls
    {
        [Key]
        public int nonkeyControlsid {  get; set; }

        public Boolean? nonKeyControlscheckedstatus { get; set; }
        public string noncontroltype {  get; set; }
        public int? nonKeyControlsstartValue { get; set; }
        public int? nonKeyControlsendValue { get; set; }
        public string nonKeyControlstransactionDescription { get; set; }
        public int? nonKeyControlsrangeStartValue { get; set; }
        public string nonKeyControlspercentageDescription { get; set; }
        public string nonKeyControlstextDescription { get; set; }

        public int risksamplingstandardsId { get; set; }
    }


    [Table("risksamplinggeneralControls")]
    public class generalControls
    {
        [Key]
        public int generalcontrolssid { get; set; }
        public Boolean? generalControlscheckedstatus { get; set; }
        public string gencontroltype {  get; set; }

        public int? generalControlsstartValue { get; set; }
        public int? generalControlsendValue { get; set; }
        public string generalControlstransactionDescription { get; set; }
        public int? generalControlsrangeStartValue { get; set; }
        public string generalControlspercentageDescription { get; set; }
        public string generalControlstextDescription { get; set; }

        public int risksamplingstandardsId { get; set; }
    }

}
