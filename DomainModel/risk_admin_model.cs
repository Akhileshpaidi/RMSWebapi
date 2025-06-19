


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainModel
{
    [Table("risk_admin_letc_l1")]
    public class risk_admin_letc_l1
    {
        [Key]
        public int? risk_admin_LETC_L1_id { get; set; }
        public string? risk_admin_LETC_L1_Name { get; set; }
        public string? risk_admin_LETC_L1_Desc { get; set; }
        public string? risk_admin_LETC_L1_show_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? isImported { get; set; }

        public int createdby {  get; set; }

    }

    [Table("risk_admin_letc_l2")]
    public class risk_admin_letc_l2
    {
        [Key]
        public int? risk_admin_letc_l2_id { get; set; }
        public string? risk_admin_letc_l2_name { get; set; }
        public string? risk_admin_letc_l2_desc { get; set; }
        public int? risk_admin_LETC_L1_id { get; set; }
        public string? risk_admin_letc_l2_show_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? isImported { get; set; }

        public int createdby { get; set; }
    }


    [Table("risk_admin_letc_l3")]
    public class risk_admin_letc_l3
    {
        [Key]
        public int? risk_admin_LETC_l3_id { get; set; }
        public string? risk_admin_LETC_l3_name { get; set; }
        public string? risk_admin_LETC_l3_desc { get; set; }
        public string? risk_admin_LETC_l3_show_desc { get; set; }
        public int? risk_admin_LETC_L1_id { get; set; }
        public int? risk_admin_letc_l2_id { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? isImported { get; set; }

        public int createdby { get; set; }
    }


    [Table("risk_admin_riskcontrefferating")]
    public class riskConEffRatingModel
    {
        [Key]
        public int risk_admin_RiskContrEffeRating_id { get; set; }
        public string? risk_admin_RiskContrEffeRatingName { get; set; }
        public string? risk_admin_RiskContrEffeRatingDesc { get; set; }
        public int? risk_admin_RiskContrEffeRatingRating { get; set; }
        public string? risk_admin_RiskContrEffeColor { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }
    }


    [Table("risk_admin_control_risk_of_assessment")]
    public class risk_admin_control_risk_of_assessment
    {
        [Key]
        public int? control_risk_of_assessment_id { get; set; }
        public string? control_risk_of_assessment_name { get; set; }
        public int? control_risk_of_assessment_range_min { get; set; }
        public int? control_risk_of_assessment_range_max { get; set; }
        public string? control_risk_of_assessment_desc { get; set; }
        public string? color_code { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? isImported { get; set; }

        public int createdby { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }

    }


    [Table("risk_admin_residual_risk_rating")]
    public class risk_admin_residual_risk_rating
    {
        [Key]
        public int? residual_risk_rating_id { get; set; }
        public string? residual_risk_rating_name { get; set; }
        public int? residual_risk_rating_min_rating { get; set; }
        public int? residual_risk_rating_max_rating { get; set; }
        public string? residual_risk_rating_desc { get; set; }
        public string? color_code { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }

    }

    [Table("risk_admin_control_measure")]
    public class risk_admin_control_measure
    {
        [Key]
        public int? control_measure_id { get; set; }
        public string? control_measure_name { get; set; }
        public string? control_measure_description { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }

    }

    [Table("risk_admin_inter_contr_comp")]
    public class risk_admin_inter_contr_comp
    {
        [Key]
        public int? risk_admin_inter_contr_comp_id { get; set; }
        public string? risk_admin_inter_contr_comp_name { get; set; }
        public string? risk_admin_inter_contr_comp_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public int? selected_control_measure { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }
    }

    [Table("risk_admin_inter_contr_principles")]
    public class risk_admin_inter_contr_principles
    {
        [Key]
        public int? risk_admin_inter_contr_Principles_id { get; set; }
        public string? risk_admin_inter_contr_Principles_name { get; set; }
        public string? risk_admin_inter_contr_Principles_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public int? control_measure_id { get; set; }
        public int? risk_admin_inter_contr_comp_id { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }

    }

    [Table("risk_admin_controlactivitytype")]
    public class risk_admin_controlactivitytype
    {
        [Key]
        public int? risk_admin_ControlActivityType_id { get; set; }
        public string? risk_admin_ControlActivityType_name { get; set; }
        public string? risk_admin_ControlActivityType_desc { get; set; }
        public string? created_date { get; set; }
        public string? status  { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }


    }


    [Table("risk_admin_control_activity_nature")]
    public class risk_admin_control_activity_nature
    {
        [Key]
        public int? risk_admin_control_activity_Nature_id { get; set; }
        public string? risk_admin_control_activity_Nature_name { get; set; }
        public string? short_code_for_activity_nature { get; set; }
        public string? risk_admin_control_activity_Nature_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public int? risk_admin_ControlActivityType_id { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }


    }

    [Table("risk_admin_control_activity_sub_nature")]
    public class risk_admin_control_activity_sub_nature
    {
        [Key]
        public int? risk_admin_Control_Activity_Sub_Nature_id { get; set; }
        public string? risk_admin_Control_Activity_Sub_Nature_name { get; set; }
        public string? risk_admin_Control_Activity_Sub_Nature_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public int? risk_admin_ControlActivityType_id { get; set; }
        public int? risk_admin_control_activity_Nature_id { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }


    }


    [Table("risk_db_control_assertion_check")]
    public class risk_db_control_assertion_check
    {
        [Key]
        public int? risk_db_Control_Assertion_Check_id { get; set; }
        public string? risk_db_Control_Assertion_Check_name { get; set; }
        public string? risk_db_Control_Assertion_Check_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public int? risk_admin_ControlActivityType_id { get; set; }

        public string? isImported { get; set; }
        public int createdby { get; set; }

    }

    [Table("risk_admin_control_reference_type")]
    public class risk_admin_control_reference_type
    {
        [Key]
        public int? risk_admin_Control_Reference_Type_id { get; set; }
        public string? risk_admin_Control_Reference_Type_name { get; set; }
        public string? risk_admin_Control_Reference_Type_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public string? selected_input_type { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }

    }
    [Table("risk_admin_con_accept_benchmark")]
    public class risk_admin_con_accept_benchmark
    {
        [Key]
        public int? risk_admin_con_accept_benchmark_id { get; set; }
        public string? risk_admin_con_accept_benchmark_name { get; set; }
        public string? risk_admin_con_accept_benchmark_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
        public int? risk_admin_con_accept_benchmark_min_level { get; set; }
        public int? risk_admin_con_accept_benchmark_max_level { get; set; }
        public string? risk_admin_con_accept_benchmark_color_code { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }

    }

}
