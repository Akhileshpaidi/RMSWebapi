
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("risk_natureof_control_perf")]
    public class RiskControlMatrixAttModel
    {
        [Key]
        public int natureOf_control_perf_id { get; set; }
        public string Risk_natureOf_control_perf_name { get; set; }
        public string Risk_natureOf_control_perf_desc { get; set; }
        public string Risk_natureOf_control_perf_date { get; set; }
        public string Risk_natureOf_control_perf_status { get; set; }
        public float? risk_natureOf_control_perf_rating { get; set; }


    }

    [Table("risk_natureof_cont_occu")]
    public class risk_natureof_cont_occu
    {
        [Key]
        public int risk_natureof_cont_occu_id { get; set; }
        public string risk_natureof_cont_occu_name { get; set; }
        public string risk_natureof_cont_occu_desc { get; set; }
        public int risk_natureof_cont_occu_rating {  get; set; }
        public string risk_natureof_cont_occu_date { get; set; }
        public string risk_natureof_cont_occu_status { get; set; }


    }


    [Table("risk_control_level")]
    public class RiskControlLevel
    {
        [Key]
        public int Risk_Control_level_id { get; set; }
        public string Risk_Control_level_name { get; set; }
        public string Risk_Control_level_desc { get; set; }
        public string Risk_Control_level_date { get; set; }
        public string Risk_Control_level_status { get; set; }


    }

    [Table("risk_control_dependencies")]
    public class RiskControldependency
    {
        [Key]
        public int risk_control_dependencies_id { get; set; }
        public string risk_control_dependencies_name { get; set; }
        public string risk_control_dependencies_desc { get; set; }
        public string risk_control_dependencies_date { get; set; }
        public string risk_control_dependencies_status { get; set; }


    }

    [Table("risk_frqof_contr_appl")]
    public class risk_frqof_contr_appl
    {
        [Key]
        public int risk_frqof_contr_appl_id { get; set; }
        public string risk_frqof_contr_appl_name { get; set; }
        public string risk_frqof_contr_appl_desc { get; set; }
        public int risk_frqof_contr_appl_rating {  get; set; }
        public string risk_frqof_contr_appl_date { get; set; }
        public string risk_frqof_contr_appl_status { get; set; }


    }


    [Table("risk_inherent_rating_level")]
    public class RiskInherentRatingLevel
    {
        [Key]
        public int Risk_inherent_rating_level_id { get; set; }
        public string Risk_inherent_rating_level_name { get; set; }
        public string Risk_inherent_rating_level_desc { get; set; }
        public int? Risk_inherent_rating_level_min { get; set; }
        public int? Risk_inherent_rating_level_max { get; set; }
        public string Risk_inherent_rating_level_date { get; set; }
        public string Risk_inherent_rating_level_status { get; set; }
        public string colour_reference { get; set; }
        //[NotMapped]
       // public List<int[]> array { get; set; }


    }


    [Table("risk_intensity")]
    public class risk_intensity
    {
        [Key]
        public int? risk_intensity_id { get; set; }
        public string? risk_intensity_name { get; set; }
        public string? risk_intensity_desc { get; set; }
        public int? risk_intensity_level_range_min { get; set; }
        public int? risk_intensity_level_range_max { get; set; }
        public string? risk_intensity_date { get; set; }
        public string? risk_intensity_status { get; set; }
        public string? colour_reference { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }


    }
}
