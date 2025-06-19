using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DomainModel
{
    [Table("risk_admin_typeofrisk")]
    public class RiskAdminModel
    {
        [Key]

        public int Risk_Admin_typeOfRisk_id { get; set; }
        public string Risk_Admin_typeOfRisk_name { get; set; }
        public string Risk_Admin_typeOfRisk_desc { get; set; }
        public string Risk_Admin_typeOfRisk_date { get; set; }
        public string Risk_Admin_typeOfRisk_status { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }
    }

    [Table("risk_admin_classification")]
    public class risk_admin_classification
    {
        [Key]

        public int risk_admin_classification_id { get; set; }
        public string risk_admin_classification_name { get; set; }
        public string risk_admin_classification_desc { get; set; }
        public string risk_admin_classification_date { get; set; }
        public string risk_admin_classification_status { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }
        public int Risk_Admin_typeOfRisk_id { get; set; }
    }

    [Table("risk_admin_riskimpactrating")]
    public class risk_admin_riskimpactrating
    {
        [Key]

        public int risk_admin_riskImpactRating_id { get; set; }
        public string risk_admin_riskImpactRating_name { get; set; }
        public string risk_admin_riskImpactRating_desc { get; set; }
        public int risk_admin_riskImpactRating_value { get; set; }
        public string risk_admin_riskImpactRating_date { get; set; }
        public string risk_admin_riskImpactRating_status { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }
        public string color_reference { get; set; }
    }


    [Table("risk_admin_likeoccfact")]
    public class risk_admin_likeoccfact
    {
        [Key]

        public int risk_admin_likeoccfact_id { get; set; }
        public string risk_admin_likeoccfact_name { get; set; }
        public string risk_admin_likeoccfact_desc { get; set; }
        public int risk_admin_likeoccfact_value { get; set; }
        public string risk_admin_likeoccfact_date { get; set; }
        public string risk_admin_likeoccfact_status { get; set; }
        public string isImported { get; set; }
        public string color_reference { get; set; }

        public int createdby { get; set; }
    }

    [Table("risk_admin_risk_categorization")]
    public class risk_admin_risk_categorization
    {
        [Key]

        public int risk_admin_risk_categorization_id { get; set; }
        public string risk_admin_risk_categorizationName { get; set; }
        public string risk_admin_risk_categorizationDesc { get; set; }
        public string risk_admin_risk_categorizationDate { get; set; }
        public string risk_admin_risk_categorizationStatus { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }
    }


    [Table("risk_admin_causelist")]
    public class risk_admin_causelist
    {
        [Key]

        public int risk_admin_causeList_id { get; set; }
        public string risk_admin_causeListName { get; set; }
        public string risk_admin_causeListDesc { get; set; }
        public string risk_admin_causeListdate { get; set; }
        public string risk_admin_causeListStatus { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }
    }


    [Table("risk_admin_riskpriority")]
    public class risk_admin_riskpriority
    {
        [Key]
        public int? risk_admin_riskPriorityId { get; set; }
        public string? risk_admin_riskPriorityName { get; set; }
        public int? rating_level_min { get; set; }
        public int? rating_level_max { get; set; }
        public string? risk_priority_description { get; set; }
        public string? color_code { get; set; }
        public string? risk_admin_riskPriorityDate { get; set; }
        public string? risk_admin_riskPriorityStatus { get; set; }
        public string? isImported { get; set; }
        public int createdby { get; set; }
        //  [NotMapped]
        //public List<int[]> array { get; set; }


    }

    [Table("risk_admin_potenbussimpact")]
    public class risk_admin_potenbussimpact
    {
        [Key]
        public int? risk_admin_potenBussImpactid { get; set; }
        public string? risk_admin_potenBussImpactname { get; set; }
        public string? risk_admin_potenBussImpactdesc { get; set; }
        public string? showDescription { get; set; }
        public string? risk_admin_potenBussImpactdate { get; set; }
        public string? risk_admin_potenBussImpactstatus { get; set; }

        public string? isImported { get; set; }

        public int createdby { get; set; }

    }


    [Table("risk_admin_riskappetite")]
    public class risk_admin_riskappetite
    {
        [Key]
        public int? risk_admin_RiskAppetiteId { get; set; }
        public string? risk_admin_RiskAppetiteName { get; set; }
        public string? risk_admin_RiskAppetiteDesc { get; set; }
        public int? risk_level_range_min { get; set; }
        public int? risk_level_range_max { get; set; }
        public string? risk_admin_RiskAppetiteDate { get; set; }
        public string? risk_admin_RiskAppetiteStatus { get; set; }
        public string? colour_reference { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }
      //  [NotMapped]
    //    public List<int[]> array { get; set; }

    }

    [Table("risk_admin_risktolerance")]
    public class risk_admin_risktolerance
    {
        [Key]
        public int? risk_admin_riskToleranceid { get; set; }
        public string? risk_admin_riskToleranceName { get; set; }
        public string? risk_admin_riskToleranceDesc { get; set; }
        public int? risk_level_range_min { get; set; }
        public int? risk_level_range_max { get; set; }
        public string? risk_admin_riskToleranceDate { get; set; }
        public string? risk_admin_riskTolerancestatus { get; set; }
        public string? colour_reference { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }

    }


    [Table("risk_admin_inherriskratinglevl")]
    public class risk_admin_inherriskratinglevl
    {
        [Key]
        public int? risk_admin_inherRiskRatingLevlid { get; set; }
        public string? risk_admin_inherRiskRatingLevlname { get; set; }
        public string? risk_admin_inherRiskRatingLevlDesc { get; set; }
        public int? risk_level_range_min { get; set; }
        public int? risk_level_range_max { get; set; }
        public string? risk_admin_inherRiskRatingdate { get; set; }
        public string? risk_admin_inherRiskRatingstatus { get; set; }
        public string? colour_reference { get; set; }
      

        public string isImported { get; set; }

        public int createdby { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }
    }


    [Table("risk_admin_riskintensity")]
    public class risk_admin_riskintensity
    {
        [Key]
        public int? risk_admin_riskIntensityid { get; set; }
        public string? risk_admin_riskIntensityname { get; set; }
        public string? risk_admin_riskIntensitydesc { get; set; }
        public int? risk_level_range_min { get; set; }
        public int? risk_level_range_max { get; set; }
        public string? risk_admin_riskIntensitydate { get; set; }
        public string? risk_admin_riskIntensityStatus { get; set; }
        public string? colour_reference { get; set; }

        public string isImported { get; set; }

        public int createdby { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }

        //public string Isimported { get; set; }
    }

    [Table("risk_admin_naturecontrperf")]
    public class risk_admin_naturecontrperf
    {
        [Key]
        public int risk_admin_NatureContrPerfid { get; set; }
        public string risk_admin_NatureContrPerfname { get; set; }
        public string risk_admin_NatureContrPerfdesc { get; set; }
        public string risk_admin_NatureContrPerfdate { get; set; }
        public string risk_admin_NatureContrPerfStatus { get; set; }
        public float? risk_natureOf_control_perf_rating { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }

    }


    [Table("risk_admin_natucontroccur")]
    public class risk_admin_natucontroccur
    {
        [Key]
        public int risk_admin_natucontroccurid { get; set; }
        public string risk_admin_natucontroccurName { get; set; }
        public string risk_admin_natucontroccurDesc { get; set; }
        public int risk_natureof_cont_occu_rating { get; set; }
        public string risk_admin_natucontroccurdate { get; set; }
        public string risk_admin_natucontroccurstatus { get; set; }

        public string isImported { get; set; }

        public int createdby { get; set; }
    }


    [Table("risk_admin_contrlevel")]
    public class risk_admin_contrlevel
    {
        [Key]
        public int risk_admin_contrLevelid { get; set; }
        public string risk_admin_contrLevelName { get; set; }
        public string risk_admin_contrLeveldesc { get; set; }
        public string risk_admin_contrLeveldate { get; set; }
        public string risk_admin_contrLevelstatus { get; set; }

        public string isImported { get; set; }

        public int createdby { get; set; }
    }

    [Table("risk_admin_contrdepen")]
    public class risk_admin_contrdepen
    {
        [Key]
        public int risk_admin_contrDepenid { get; set; }
        public string risk_admin_contrDepenname { get; set; }
        public string risk_admin_contrDependesc { get; set; }
        public string risk_admin_contrDependate { get; set; }
        public string risk_admin_contrDepenstatus { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }

    }

    [Table("risk_admin_frqcontrapplid")]
    public class risk_admin_frqcontrapplid
    {
        [Key]
        public int risk_admin_frqcontrapplidid { get; set; }
        public string risk_admin_frqcontrapplidname { get; set; }
        public string risk_admin_frqcontrappliddesc { get; set; }
        public int risk_frqof_contr_appl_rating { get; set; }
        public string risk_admin_frqcontrappliddate { get; set; }
        public string risk_admin_frqcontrapplidStatus { get; set; }

        public string isImported { get; set; }

        public int createdby { get; set; }
    }

    [Table("risk_admin_bpmatratscaleindicator")]
    public class risk_admin_bpmatratscaleindicator
    {
        [Key]
        public int? risk_admin_bpmatratscaleindicatorid { get; set; }
        public string? risk_admin_bpmatratscaleindicatorname { get; set; }
        public int? BPMaturityRatingScaleIndicators_rating_min { get; set; }
        public int? BPMaturityRatingScaleIndicators_rating_max { get; set; }
        public string? risk_admin_bpmatratscaleindicatordesc { get; set; }

        public string? risk_admin_bpmatratscaleindicatordate { get; set; }
        public string? risk_admin_bpmatratscaleindicatorstatus { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }
        //[NotMapped]
        //public List<int[]> array { get; set; }
        //public string isImported { get; set; }

    }

    [Table("risk_admin_contrasstestatt")]
    public class risk_admin_contrasstestatt
    {
        [Key]
        public int? risk_admin_contrAssTestAttid { get; set; }
        public string? risk_admin_contrAssTestAttname { get; set; }

        public string? risk_admin_contrAssTestAttdesc { get; set; }

        public string? risk_admin_contrAssTestAttdate { get; set; }
        public string? risk_admin_contrAssTestAttstatus { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }

    }

    [Table("risk_admin_iniassimpfact")]
    public class risk_admin_iniassimpfact
    {
        [Key]
        public int risk_admin_Iniassimpfactid { get; set; }
        public string risk_admin_Iniassimpfactname { get; set; }
        public string risk_admin_Iniassimpfactdesc { get; set; }
        public string risk_admin_Iniassimpfactdate { get; set; }
        public string risk_admin_Iniassimpfactstatus { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }
    }

    [Table("risk_admin_mitdecilist")]
    public class risk_admin_mitdecilist
    {
        [Key]
        public int risk_admin_MitdeciListid { get; set; }
        public string risk_admin_MitdeciListname { get; set; }
        public string risk_admin_MitdeciListdesc { get; set; }
        public string risk_admin_MitdeciListdate { get; set; }
        public string risk_admin_MitdeciListstatus { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }
    }

    [Table("risk_admin_asscontracptcrit")]
    public class risk_admin_asscontracptcrit
    {
        [Key]
        public int? risk_admin_asscontracptCritid { get; set; }
        public string? risk_admin_asscontracptCritname { get; set; }
        public int? risk_Asses_contr_accep_crit_min_range { get; set; }
        public int? risk_Asses_contr_accep_crit_max_range { get; set; }
        public string? risk_admin_asscontracptCritdesc { get; set; }
        public string? risk_admin_asscontracptCritdate { get; set; }
        public string? risk_admin_asscontracptCritstatus { get; set; }
        public int createdby { get; set; }
        public string isImported { get; set; }

        //[NotMapped]
        //public List<int[]>? array { get; set; }


    }

    [Table("risk_admin_risktredecilist")]
    public class risk_admin_risktredecilist
    {
        [Key]
        public int? risk_admin_risktredecilistid { get; set; }
        public string? risk_admin_risktredecilistname { get; set; }
        public string? risk_admin_risktredecilistdesc { get; set; }
        public string? risk_admin_risktredecilistdate { get; set; }
        public string? risk_admin_risktredeciliststatus { get; set; }
        public string isImported { get; set; }

        public int createdby { get; set; }
    

    }
    [Table("risk_admin_risktrdecimatrix")]
    public class risk_admin_risktrdecimatrix
    {
        [Key]
        public int? risk_admin_risktrdecimatrixid { get; set; }
        public int? risk_admin_inherRiskRatingLevlid { get; set; }
        public string? risk_admin_risktrdecimatrixdesc { get; set; }
        public int? RiskTreatmentDecision { get; set; }
        public string? risk_admin_risktrdecimatrixdate { get; set; }
        public string? risk_admin_risktrdecimatrixstatus { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }


    }

    [Table("risk_admin_mitactireq")]
    public class risk_admin_mitactireq
    {
        [Key]
        public int risk_admin_MitActiReqid { get; set; }
        public string? risk_admin_MitActiReqname { get; set; }
        public string? risk_admin_MitActiReqdesc { get; set; }
        public string? risk_admin_MitActiReqdate { get; set; }
        public string? risk_admin_MitActiReqstatus { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }
    }


    [Table("risk_admin_actiprilist")]
    public class risk_admin_actiprilist
    {
        [Key]
        public int risk_admin_actiPriListid { get; set; }
        public string? risk_admin_actiPriListname { get; set; }
        public string? risk_admin_actiPriListdesc { get; set; }
        public string? risk_admin_actiPriListdate { get; set; }
        public string? risk_admin_actiPriListstats { get; set; }
        public string isImported { get; set; }
        public int createdby { get; set; }
    }

}
