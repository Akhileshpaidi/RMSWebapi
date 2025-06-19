using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DomainModel
{
    [Table("risk_riskregistermaster")]
    public class Risk_AddRegisterModel
    {
        [Key]
        [JsonProperty("RiskRegisterMasterID")]
        public int? RiskRegisterMasterID { get; set; }

        [JsonProperty("Entity_Master_id")]
        public int? Entity_Master_id { get; set; }

        [JsonProperty("Unit_location_Master_id")]
        public int? Unit_location_Master_id { get; set; }

        [JsonProperty("department_Master_id")]
        public int? department_Master_id { get; set; }

        [JsonProperty("riskBusinessfunctionid")]
        public int? riskBusinessfunctionid { get; set; }

        [JsonProperty("businessprocessID")]
        public int? businessprocessID { get; set; }

        [JsonProperty("BusinessProcessL1ID")]
        public int? BusinessProcessL1ID { get; set; }

        [JsonProperty("BusinessProcessL2ID")]
        public int? BusinessProcessL2ID { get; set; }

        [JsonProperty("BusinessProcessL3ID")]
        public int? BusinessProcessL3ID { get; set; }

        [JsonProperty("BusinessSubProcessObjective")]
        public string BusinessSubProcessObjective { get; set; }

        [JsonProperty("NameofRiskDocumentRegister")]
        public string NameofRiskDocumentRegister { get; set; }

        [JsonProperty("ObjectiveofRiskDocument")]
        public string ObjectiveofRiskDocument { get; set; }

        [JsonProperty("RiskRootCause")]
        public string RiskRootCause { get; set; }

        [JsonProperty("Risk_Admin_typeOfRisk_id")]
        public int? Risk_Admin_typeOfRisk_id { get; set; }

        [JsonProperty("risk_admin_classification_id")]
        public int? risk_admin_classification_id { get; set; }

        [JsonProperty("risk_admin_risk_categorization_id")]
        public int? risk_admin_risk_categorization_id { get; set; }

        [JsonProperty("risk_admin_causeList_id")]
        public int? risk_admin_causeList_id { get; set; }

        [JsonProperty("AMLComplianceRisk")]
        public string AMLComplianceRisk { get; set; }

        [JsonProperty("ModelRisk")]
        public string ModelRisk { get; set; }

        [JsonProperty("ConductRisk")]
        public string ConductRisk { get; set; }

        [JsonProperty("ITCyberSecurity")]
        public string ITCyberSecurity { get; set; }

        [JsonProperty("ThirdPartyOutsourcing")]
        public string ThirdPartyOutsourcing { get; set; }

        [JsonProperty("FraudRisk")]
        public string FraudRisk { get; set; }

        [JsonProperty("LegalRisk")]
        public string LegalRisk { get; set; }

        [JsonProperty("OperationalRisk")]
        public string OperationalRisk { get; set; }

        [JsonProperty("ReputationalRisk")]
        public string ReputationalRisk { get; set; }

        [JsonProperty("FinancialRiskReporting")]
        public string FinancialRiskReporting { get; set; }

        [JsonProperty("RiskCostImpact")]
        public string RiskCostImpact { get; set; }

        [JsonProperty("risk_admin_riskImpactRating_id")]
        public int? risk_admin_riskImpactRating_id { get; set; }

        [JsonProperty("risk_admin_likeoccfact_id")]
        public int? risk_admin_likeoccfact_id { get; set; }

        [JsonProperty("InherentRiskRating")]
        public int? InherentRiskRating { get; set; }

        [JsonProperty("activityvalue")]
        public int? activityvalue { get; set; }

        [JsonProperty("RiskPriority")]
        public int? RiskPriority { get; set; }

        [JsonProperty("Slidervalue")]
        public int? Slidervalue { get; set; }

        [JsonProperty("RiskIntensity")]
        public int? RiskIntensity { get; set; }

        [JsonProperty("risk_admin_LETC_L1_id")]
        public int? risk_admin_LETC_L1_id { get; set; }

        [JsonProperty("CategoryL1Description")]
        public string CategoryL1Description { get; set; }

        [JsonProperty("risk_admin_letc_l2_id")]
        public int? risk_admin_letc_l2_id { get; set; }

        [JsonProperty("CategoryL2Description")]
        public string CategoryL2Description { get; set; }

        [JsonProperty("risk_admin_LETC_l3_id")]
        public int? risk_admin_LETC_l3_id { get; set; }

        [JsonProperty("CategoryL3Description")]
        public string CategoryL3Description { get; set; }

        [JsonProperty("risk_admin_potenBussImpactid")]
        public int? risk_admin_potenBussImpactid { get; set; }

        [JsonProperty("PotentialImpactDescription")]
        public string PotentialImpactDescription { get; set; }

        [JsonProperty("SuggestivePriventive")]
        public string SuggestivePriventive { get; set; }

        [JsonProperty("RepeatReviewFrequency")]
        public string RepeatReviewFrequency { get; set; }

        [JsonProperty("EnterValueforrepeat")]
        public int? EnterValueforrepeat { get; set; }

        [JsonProperty("Selectfrequencyperiod")]
        public string Selectfrequencyperiod { get; set; }

        [JsonProperty("StartDateNextReview")]
        public DateTime? StartDateNextReview { get; set; }

        [JsonProperty("NameOfRisRegister")]
        public string NameOfRisRegister { get; set; }

        [JsonProperty("NoOfRiskStatements")]
        public int? NoOfRiskStatements { get; set; }

        [JsonProperty("docTypeID")]
        public int? docTypeID { get; set; }

        [JsonProperty("doc_CategoryID")]
        public int? doc_CategoryID { get; set; }

        [JsonProperty("doc_SubCategoryID")]
        public int? doc_SubCategoryID { get; set; }

        [JsonProperty("DocumentEffectiveDate")]
        public DateTime? DocumentEffectiveDate { get; set; }

        [JsonProperty("DocumentConfidentiality")]
        public string DocumentConfidentiality { get; set; }

        [JsonProperty("natureOf_Doc_id")]
        public int? natureOf_Doc_id { get; set; }

        [JsonProperty("InternalReferenceNo")]
        public string InternalReferenceNo { get; set; }
        
        [JsonProperty("PhysicalVaultLocation")]
        public string PhysicalVaultLocation { get; set; }

        [JsonProperty("risk_admin_RiskAppetiteId")]
        public int? risk_admin_RiskAppetiteId { get; set; }

        [JsonProperty("AppetiteStatement")]
        public string AppetiteStatement { get; set; }

        [JsonProperty("PublishingRemarks")]
        public string PublishingRemarks { get; set; }

        [JsonProperty("FileAttachement")]
        public string FileAttachement { get; set; }

        [JsonProperty("OtpMethod")]
        public string OtpMethod { get; set; }

        [JsonProperty("Keywords")]
        public string Keywords { get; set; }

        [JsonProperty("ReviewfrequencyCheck")]
        public int? ReviewfrequencyCheck { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("UniqueDocumentID")]
 
        public string UniqueDocumentID { get; set; }

        [JsonProperty("InherentRatingColor")]
        public string InherentRatingColor { get; set; }

        [JsonProperty("RiskPirorityColor")]
        public string RiskPirorityColor { get; set; }

        [JsonProperty("RiskIntensityColor")]
        public string RiskIntensityColor { get; set; }

        [JsonProperty("BusinessFunctionHead")]
        public int? BusinessFunctionHead { get; set; }

        [JsonProperty("DocumentApprover")]
        public int? DocumentApprover { get; set; }

        [JsonProperty("RiskOwnership")]
        public int? RiskOwnership { get; set; }

        [JsonProperty("ProcessOwner")]
        public int? ProcessOwner { get; set; }

        [JsonProperty("BusinessProcessHead")]
        public int? BusinessProcessHead { get; set; }

        [JsonProperty("RiskStatementID")]
        public int? RiskStatementID { get; set; }

        [JsonProperty("UniqueRiskID")]
        public string UniqueRiskID { get; set; }

        [JsonProperty("activityid")]
        public int? activityid { get; set; }

        //[JsonProperty("Finish")]
        public string Finish { get; set; }

       public string RiskDefinition {  get; set; }
        public DateTime? LastEditedon { get; set; }

        //public int Entity_Master_id { get; set; }
        //public int Unit_location_Master_id { get; set; }
        //public int department_Master_id { get; set; }
        //public int riskBusinessfunctionid { get; set; }
        //public int businessprocessID { get; set; }
        //public int BusinessProcessL1ID { get; set; }
        //public int BusinessProcessL2ID { get; set; }
        //public int BusinessProcessL3ID { get; set; }
        //public string BusinessSubProcessObjective { get; set; }
        //public string NameofRiskDocumentRegister { get; set; }
        //public string ObjectiveofRiskDocument { get; set; }
        //public string RiskRootCause { get; set; }
        //public int risk_Admin_typeOfRisk_id { get; set; }
        //public int risk_admin_classification_id { get; set; }
        //public int risk_admin_risk_categorization_id { get; set; }
        //public int risk_admin_causeList_id { get; set; }
        //public string AMLComplianceRisk { get; set; }
        //public string ModelRisk { get; set; }
        //public string ConductRisk { get; set; }
        //public string ITCyberSecurity { get; set; }
        //public string ThirdPartyOutsourcing { get; set; }
        //public string FraudRisk { get; set; }
        //public string LegalRisk { get; set; }
        //public string OperationalRisk { get; set; }
        //public string ReputationalRisk { get; set; }
        //public string FinancialRiskReporting { get; set; }
        //public string RiskCostImpact { get; set; }
        //public int risk_admin_riskImpactRating_id { get; set; }
        //public int risk_admin_likeoccfact_id { get; set; }
        //public int InherentRiskRating { get; set; }
        //public int activityid { get; set; }
        //public int RiskPriority { get; set; }
        //public int Slidervalue { get; set; }
        //public int RiskIntensity { get; set; }
        //public string InherentRatingColor { get; set; }
        //public string RiskPirorityColor { get; set; }
        //public string RiskIntensityColor { get; set; }
        //// Consequences
        //public int risk_admin_LETC_L1_id { get; set; }
        //public string CategoryL1Description { get; set; }
        //public int risk_admin_letc_l2_id { get; set; }
        //public string CategoryL2Description { get; set; }
        //public int risk_admin_LETC_l3_id { get; set; }
        //public string CategoryL3Description { get; set; }
        //public int risk_admin_potenBussImpactid { get; set; }
        //public string PotentialImpactDescription { get; set; }
        //public string SuggestivePriventive {  get; set; }
        //public int ReviewfrequencyCheck {  get; set; }
        //public string UniqueRiskID {  get; set; }
        //public int RiskStatementID {  get; set; }
        //public string FileAttachement {  get; set; }
        //public string FileName {  get; set; }


        ////Risk Review
        //public string RepeatReviewFrequency {  get; set; }
        //public int EnterValueforrepeat {  get; set; }
        //public string Selectfrequencyperiod {  get; set; }
        //public DateTime? StartDateNextReview {  get; set; }
        //// final page 

        //public string NameOfRisRegister {  get; set; }
        //public int NoOfRiskStatements { get; set; }
        //public int docTypeID { get; set; }
        //public int doc_CategoryID { get; set; }
        //public int doc_SubCategoryID { get; set; }
        //public DateTime? DocumentEffectiveDate { get; set; }
        //public string DocumentConfidentiality { get; set; }
        //public string OtpMethod { get; set; }
        //public int natureOf_Doc_id { get; set; }
        //public int InternalReferenceNo { get; set; }
        //public string PhysicalVaultLocation { get; set; }
        //public int risk_admin_RiskAppetiteId { get; set; }
        //public string AppetiteStatement { get; set; }
        //public string PublishingRemarks { get; set; }
        //public string Keywords { get; set; }

    }
}
