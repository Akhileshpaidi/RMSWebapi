using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel

{
    [Table("risk_categorization")]
    public class RiskMatrixModel
    {
        [Key]
        public int Risk_categorization_id { get; set; }
        public string Risk_categorization_name { get; set; }
        public string Risk_categorization_desc { get; set; }
        public string Risk_categorization_date { get; set; }
        public string Risk_categorization_status { get; set; }
    }


    [Table("risk_cause_list")]
    public class RiskMatrixcauseList
    {
        [Key]
        public int? Risk_cause_list_id { get; set; }
        public string? Risk_cause_list_name { get; set; }
        public string? Risk_cause_list_desc { get; set; }
        public string? Risk_cause_list_date { get; set; }
        public string? Risk_cause_list_status { get; set; }
    }


    [Table("risk_likelihood_occ_factor")]
    public class RiskLikelihood
    {
        [Key]
        public int? risk_likelihood_occ_factor_id { get; set; }
        public string? risk_likelihood_occ_factor_name { get; set; }
        public string? risk_likelihood_occ_factor_desc { get; set; }
        public int? risk_likelihood_occ_factor_value { get; set; }
        public string? colour_reference { get; set; }
        public string? risk_likelihood_occ_factor_date { get; set; }
        public string? risk_likelihood_occ_factor_status { get; set; }


    }




    [Table("risk_impactrating")]
    public class RiskImpactRating
    {
        [Key]
        public int ImpactRatingID { get; set; }
        public string RiskImpactRatingName { get; set; }
        public string RiskImpactRatingDescription { get; set; }
        public int RiskImpactRatingScale { get; set; }
        public string colour_reference { get; set; }
        public string risk_impactrating_date { get; set; }
        public string risk_impactrating_status { get; set; }
    }

    [Table("risk_initial_assessment_impact_factor")]
    public class risk_initial_assessment_impact_factor
    {
        [Key]
        public int risk_ini_ass_imp_id { get; set; }
        public string risk_ini_ass_imp_name { get; set; }
        public string risk_ini_ass_imp_desc { get; set; }
        public string risk_ini_ass_imp_date { get; set; }
        public string risk_ini_ass_imp_status { get; set; }
    }

    [Table("risk_mitigation_decision")]
    public class risk_mitigation_decision
    {
        [Key]
        public int Risk_mitigation_decision_id { get; set; }
        public string Risk_mitigation_decision_name { get; set; }
        public string Risk_mitigation_decision_desc { get; set; }
        public string Risk_mitigation_decision_date { get; set; }
        public string Risk_mitigation_decision_status { get; set; }
    }



    [Table("risk_asses_contr_accep_crit")]
    public class risk_asses_contr_accep_crit
    {
        [Key]
        public int risk_Asses_contr_accep_crit_id { get; set; }
        public string risk_Asses_contr_accep_crit_name { get; set; }
        public int? risk_Asses_contr_accep_crit_min_range { get; set; }
        public int? risk_Asses_contr_accep_crit_max_range {  get; set; }
        public string? risk_Asses_contr_accep_crit_desc { get; set; }
        public string? risk_Asses_contr_accep_crit_date { get; set; }
        public string? risk_Asses_contr_accep_crit_status { get; set; }

        [NotMapped]
        public List<int[]>? array { get; set; }

    }

    [Table("cont_test_cont_relevance")]
    public class cont_test_cont_relevance
    {
        [Key]
        public int cont_test_cont_relevance_id { get; set; }
        public string cont_test_cont_relevance_name { get; set; }
        public string cont_test_cont_relevance_desc { get; set; }
        public string cont_test_cont_relevance_date { get; set; }
        public string cont_test_cont_relevance_status { get; set; }
    }

}
