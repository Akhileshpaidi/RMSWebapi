using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel
{
    [Table("risk_priority")]
    public class RiskSupAdminModel
    {
        [Key]
        public int? risk_priority_id { get; set; }
        public string? risk_priority_name { get; set; }
        public int? rating_level_min { get; set; }
        public int? rating_level_max { get; set; }
        public string? risk_priority_description { get; set; }
        public string? color_code { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
       // [NotMapped]
        //public List<int[]> array { get; set; }

    }



    [Table("potential_business_impact")]
    public class potential_business_impact
    {
        [Key]
        public int? potential_business_impact_id { get; set; }
        public string? potential_business_impact_name { get; set; }
        public string? potential_business_impact_des { get; set; }
        public string? potential_business_impact_show_des { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }



    }

    [Table("loss_event_threat_category")]
    public class loss_event_threat_category
    {
        [Key]
        public int? Loss_Event_Threat_Category_id { get; set; }
        public string? Loss_Event_Threat_Category_Name { get; set; }
        public string? Loss_Event_Threat_Category_desc { get; set; }
        public string? Loss_Event_Threat_Category_show_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }



    }



    [Table("losseventthreacategory_l2")]
    public class losseventthreacategory_l2
    {
        [Key]
        public int? lossEventThreaCategory_L2_id { get; set; }
        public int? Loss_Event_Threat_Category_id { get; set; }
        public string? lossEventThreaCategory_L2_Name { get; set; }
        public string? lossEventThreaCategory_L2_Des { get; set; }
        public string? lossEventThreaCategory_L2_show_des { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }



    }

    [Table("losseventthreacategory_l3")]
    public class losseventthreacategory_l3
    {
        [Key]
        public int? lossEventThreaCategory_L3_id { get; set; }
        public int? Loss_Event_Threat_Category_id { get; set; }
        public int? lossEventThreaCategory_L2_id { get; set; }
        public string? lossEventThreaCategory_L3_Name { get; set; }
        public string? lossEventThreaCategory_L3_Des { get; set; }
        public string? lossEventThreaCategory_L3_show_des { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }



    }

    [Table("control_measure")]
    public class getcontrol_measure
    {
        [Key]
        public int? control_measure_id { get; set; }
        public string? control_measure_name { get; set; }
        public string? control_measure_description { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }

    }

    [Table("control_activity_type")]
    public class control_activity_type
    {
        [Key]
        public int control_activity_type_id { get; set; }
        public string? control_activity_type_name { get; set; }
        public string? control_activity_type_description { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
    }

    [Table("control_reference_type")]
    public class control_reference_type
    {
        [Key]
        public int? control_reference_type_id { get; set; }
        public string? control_reference_type_name { get; set; }
        public string? control_reference_type_desc { get; set; }
        public string input_type { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }

    }



    [Table("mitigation_action")]
    public class mitigation_action
    {
        [Key]
        public int mitigation_action_id { get; set; }
        public string? mitigation_action_name { get; set; }
        public string? mitigation_action_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
    }


    [Table("action_priority_list")]
    public class action_priority_list
    {
        [Key]
        public int action_priority_list_id { get; set; }
        public string? action_priority_list_name { get; set; }
        public string? action_priority_list_desc { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }
    }



    [Table("residual_risk_rating")]
    public class residual_risk_rating
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
        //[NotMapped]
        //public List<int[]> array { get; set; }

    }

    [Table("control_risk_of_assessment")]
    public class control_risk_of_assessment
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
        //[NotMapped]
        //public List<int[]> array { get; set; }

    }


    [Table("risk_contr_eff_rating")]
    public class riskcontroleffectivenessrating
    {
        [Key]
        public int? risk_contr_eff_rating_id { get; set; }
        public string? risk_contr_eff_rating_name { get; set; }
       
        public int? risk_contr_eff_rating_rating { get; set; }
        public string? risk_contr_eff_rating_desc { get; set; }
        public string? risk_contr_eff_rating_color { get; set; }
        public string? risk_contr_eff_rating_date { get; set; }
        public string? risk_contr_eff_rating_status { get; set; }

    }

    

    [Table("bpmaturityratingscaleindicators")]
    public class bpmaturityratingscaleindicators
    {
        [Key]
        public int? BPMaturityRatingScaleIndicators_id { get; set; }
        public string? BPMaturityRatingScaleIndicators_name { get; set; }
        public int? BPMaturityRatingScaleIndicators_rating_min { get; set; }
        public int? BPMaturityRatingScaleIndicators_rating_max { get; set; }
        public string? BPMaturityRatingScaleIndicators_desc { get; set; }

        public string? created_date { get; set; }
        public string? status { get; set; }
     


    }


    [Table("controlassesstestattributes")]
    public class controlassesstestattributes
    {
        [Key]
        public int? ControlAssessTestAttributes_id { get; set; }
        public string? ControlAssessTestAttributes_name { get; set; }
     
        public string? ControlAssessTestAttributes_desc { get; set; }

        public string? created_date { get; set; }
        public string? status { get; set; }

    }

    [Table("risk_treatmentdecisionlist")]
    public class RiskTreatmentDecisionList
    {
        [Key]
        public int? Risk_treatmentDecisionList_id {get;set;}
        public string? Risk_treatmentDecisionList_Name { get; set; }
        public string? Risk_treatmentDecisionList_Des { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }


    }
    [Table("risk_treatmetdecisionmatrix")]
    public class risk_treatmetdecisionmatrix
    {
        [Key]
        public int? Risk_TreatmetDecisionMatrix_id { get; set; }
        public int? InherentRiskRatingLevel { get;set;}
        public string? Risk_TreatmetDecisionMatrix_des { get; set; }
        public int? RiskTreatmentDecision { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }


    }

    [Table("risk_controltestdecisionlist")]
    public class risk_controltestdecisionlist
    {
        [Key]
        public int? Risk_controlTestDecisionList_id { get; set; }
        public int? ControlTestingParametersName { get; set; }
        public string? ControlTestDecisionName { get; set; }
        public string ? ControlTesDecisionDescription { get; set; }
        public int? ControlTestDecisionRatingScore { get; set; }
        public string ? colorReference { get; set; }
        public string? created_date { get; set; }
        public string? status { get; set; }


    }

}
