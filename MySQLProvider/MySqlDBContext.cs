using System;
using System.Collections.Generic;
using System.Text;
using DomainModel;
using Microsoft.EntityFrameworkCore;
using static DomainModel.SupAdmin_StatutoryformsModel;

namespace MySQLProvider
{
    public class MySqlDBContext : DbContext
    {
      

        public MySqlDBContext(DbContextOptions<MySqlDBContext> options) : base(options)
        { }
        public DbSet<UserOtpModel> UserOtpModels { get; set; }
        public DbSet<UserLogin> UserLogin { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<UserRegistration> UserRegistrations { get; set; }

        public DbSet<raisequeryModel> raisequery { get; set; }
        //public DbSet<raisequeryModel> raisequery { get; set; }
        public DbSet<raisequeryfilesModel> raisequeryfiles { get; set; }
        public DbSet<reviewqueryfilesModel> reviewqueryfiles { get; set; }
        public DbSet<NotificationModel> notification { get; set; }

        public DbSet<FlightModel> FlightModels { get; set; }

        public DbSet<TypeofData> TypeofDatas { get; set; }

        public DbSet<MissionModel>  MissionModels{ get; set; }
        public DbSet<doctaskuseracknowledmentstatusmodel> doctaskuseracknowledmentstatusmodels {  get; set; }
        public DbSet<TelemetryStationModel> TelemetryStationModels { get; set; }

        public DbSet<EquipmentParameterDetailModel> EquipmentParameterDetailModels { get; set; }

        public DbSet<EquipmentParameterModel> EquipmentParameterModels { get; set; }

        public DbSet<EquipmentsModel> EquipmentsModels { get; set; }
        public DbSet<FolderUpload> FolderUpload { get; set; }

        public DbSet<ParameterTypeModel> ParameterTypeModels { get; set; }

        public DbSet<PacketModel> PacketModels { get; set; }

        public DbSet<DemoParameterMaster> DemoParameterMasters { get; set; }
        public DbSet<NotificationCenterModel> NotificationCenterModels { get; set; }


        //Risk Project
        public DbSet<RoleModel> RoleModels { get; set; }
        public DbSet<ActivitylogModel> ActivitylogModels { get; set; }
        public DbSet<LogModel> LogModels { get; set; }
        public DbSet<TaskModel> TaskModels { get; set; }
        public DbSet<forgetpasswordtoken> forgetpasswordtokens { get; set; }
        public DbSet<DocumentmasterModel> DocumentmasterModels { get; set; }
        public DbSet<DocTypeMasterModel> DocTypeMasterModels { get; set; }
        public DbSet<DocCategoryMasterModel> DocCategoryMasterModels { get; set; }
        public DbSet<DocSubCategoryModel> DocSubCategoryModels { get; set; }
        public DbSet<AuthorityTypeMaster> AuthorityTypeMasters { get; set; }
        public DbSet<AuthorityNameModel> AuthorityNameModels { get; set; }
        public DbSet<NatureOf_DocumentMasterModel> NatureOf_DocumentMasterModels { get; set; }

        public DbSet<ScoreIndicatorModel> ScoreIndicatorModels { get; set; }
        public DbSet<CompetencySkillModel> CompetencySkillModels { get; set; }

        public DbSet<TypeModel> TypeModels { get; set; }

        public DbSet<SubTypeModel> SubTypeModels { get; set; }

        public DbSet<SubjectModel> SubjectModels { get; set; }

        public DbSet<TopicModel> TopicModels { get; set; }

        public DbSet<KeyImprovementIndicatorsModel> KeyImprovementIndicatorsModels { get; set; }

      
        public DbSet<CheckLevelModel> CheckLevelModels { get; set; }
        public DbSet<DepartmentModel> DepartmentModels { get; set; }

        public DbSet<usermodel> usermodels { get; set; }

        public DbSet<UnitMasterModel> UnitMasterModels {  get; set; }
        
        public DbSet<UnitLocationMasterModel> UnitLocationMasterModels {  get; set; }

        public DbSet<AddDocumentModel> AddDocumentModels { get; set; }

        public DbSet<ProvideAccessModel> ProvideAccessModels { get; set; }
        public DbSet<userlocationmappingModel> userlocationmappingModels { get; set; }

        public DbSet<DirectuploadModel> DirectuploadModels { get; set; }
        public DbSet<PublishedDoc> PublishedDocs { get; set; }
        public DbSet<AckReqModel> AckReqModels { get; set; }
        
        public DbSet<DocumentFilesuplodModel> DocumentFilesuplodModels { get; set; }
        public DbSet<UserRightsPermissionModel> UserrightsModels { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProvideAccessModel>()
                .HasKey(p => p.AddDoc_id); // Adjust based on your actual primary key property

            // Optionally configure other entity relationships or constraints
        }

        public DbSet<QuestionBankModel> QuestionBankModels { get; set; }
        public DbSet<ProvideAccessdocument> provideAccessdocumentModels { get; set; }
        public DbSet<UserPermissionModel> UserPermissionModels { get; set; }
        public DbSet<CreateuserModel> CreateuserModels { get; set; }

   
        public DbSet<mapuserrolemodel> mapuserrolemodels { get; set; }

        public DbSet<Componentmodel> Componentmodels {  get; set; }
        public DbSet<PeriodicityFactorModel> PeriodicityFactorModels { get; set; }
        public DbSet<MonthModel> MonthModels { get; set; }

        public DbSet<AcknowledgementModel> AcknowledgementModels { get; set; }
        public DbSet<SectorModel> SectorModels { get; set; }
        public DbSet<Sub_SectorModel> Sub_SectorModels { get; set; }
        public DbSet<UnitModel> UnitModels { get; set; }
        public DbSet<UnitTypeModel> UnitTypeModels { get; set; }

        public DbSet<ComplianceModel> ComplianceModels { get; set; }
        public DbSet<RegionModel> RegionModels { get; set; }
        public DbSet<SubRegionModel> SubRegionModels { get; set; }
        public DbSet<LawTypeModel> LawTypeModels { get; set; }
        public DbSet<ComplianceGroupModel> ComplianceGroupModels { get; set; }
        public DbSet<ComplianceRecordTypeModel> ComplianceRecordTypeModels { get; set; }
        public DbSet<RegulatoryAuthorityModel> RegulatoryAuthorityModels { get; set; }
        public DbSet<CountryModel> CountryModels {  get; set; }
        public DbSet<StateModel> StateModels {  get; set; }
        public DbSet<Actregulatoryfilemodel> Actregulatoryfilemodels { get; set; }
        public DbSet<ActRuleregulatoryfilemodel> ActRuleregulatoryfilemodels { get; set; }
        public DbSet<JurisdictionLocationModel> JurisdictionLocationModels { get; set; }
        public DbSet<ComplianceRiskClassificationModel> ComplianceRiskClassificationModels { get; set; }
        public DbSet<ComplianceRiskClassificationCriteriaModel> ComplianceRiskClassificationCriteriaModels { get; set; }
        public DbSet<ComplianceNotifiedStatusModel> ComplianceNotifiedStatusModels { get; set; }
        public DbSet<CompliancePeriodModel> CompliancePeriodModels { get; set; }
        public DbSet<PenaltyCategoryModel> PenaltyCategoryModels { get; set; }
        public DbSet<compliancepenatlymasterModel> CompliancepenatlymasterModels { get; set; }
        public DbSet<PenaltyCategoryfileModel> PenaltyCategoryfileModels { get; set; }
        public DbSet<CreateCompanyComplianceModel> CreateCompanyComplianceModels { get; set; }
        public DbSet<CompanyComplianceScheduler> CompanyComplianceSchedulerModels { get; set; }
        public DbSet<CurrentBatchCompliance> CurrentBatchComplianceModels { get; set; }
        public DbSet<BatchCompliance> BatchComplianceModels { get; set; }
        public DbSet<ViewMitigationModel> ViewMitigationModels { get; set; }
        public DbSet<RiskCategoryModel> RiskCategoryModels { get; set; }
        public DbSet<AssessGenDetailsModel> AssessGenDetailsModels { get; set; }
        public DbSet<AssessmentPublisherModel> AssessmentPublisherModels { get; set;}
        public DbSet<ass_user_permissionlistModel> ass_user_permissionlists { get; set; }

        public DbSet<user_ass_ans_details> user_ass_ans_details { get; set; }


        public DbSet<entityTypeModel> entityTypeModels { get; set; }
        public DbSet<defualtmodulerole> defualtmodulerolemodels { get; set; }
        public DbSet<assement_provideacess> assement_provideacessmodel { get; set; }

        public DbSet< typeofusermodel> typeofusermodels { get; set; }
        public DbSet<tpamodel> tpausermodels { get; set; }

        public DbSet<TPAEntitymodel> TPAEntitymodels {  get; set; }


        public DbSet<sheduledAssessmentmappeduser> sheduledAssessmentmappedusermodels {  get; set; }

        public DbSet<Sheduledexcluededusersmodel> Sheduledexcluededusersmodels {  get; set; }


        public DbSet<industrytypemodel> industrytypemodels {  get; set; }
        
             public DbSet<Businesssectormodel> Businesssectormodels { get; set; }

        public DbSet<Frequencymodel> Frequencymodels { get; set; }

        public DbSet<Holidaymaster> Holidaymasters { get; set; }

        public DbSet<catageoryoflawmodel> catageoryoflawmodels {  get; set; }


        public DbSet<Jurisdictionmodel> Jurisdictionmodels {  get; set; }

        public DbSet<suggestionsModel> suggestionsModels { get; set; }
        public DbSet<AcknowledgeSuggestionssModel> AcknowledgeSuggestionssModels { get; set; }
        //    public DbSet<tpaexternalmappedusersmodel> tpaexternalmappedusersmodels { get; set; }
        
               public DbSet<MitigationModel> MitigationModels { get; set; }



        public DbSet<Actregulatorymodel> Actregulatorymodels { get; set; }

        public DbSet<Rulesandregulatorymodel> Rulesandregulatorymodels { get; set; }
        public DbSet<statutoryformsrecordsmodel> statutoryformsrecordsmodels { get; set; }
        public DbSet<statutoryformsrecordsfilemodel> statutoryformsrecordsfilemodels { get; set; }

        public DbSet<ActivityWorkgroupModel> ActivityWorkgroupModels {  get; set; }

        public DbSet<useractivitymapping> useractivitymappingmodels {  get; set; }

        public DbSet<compliancelocationmappingmodel> compliancelocationmappingmodels {  get; set; }

        public DbSet<Departmentlocationmappingmodel> Departmentlocationmappingmodels { get; set; }

        public DbSet<typeofrolemodel> typeofrolemodels { get; set; }
        public DbSet<ReviewStatusSettingsModel> ReviewStatusSettingsModels { get; set; }
        public DbSet<DefaultNotifiresModel> DefaultNotifiresModels { get; set; }

        public DbSet<EntityHeirarchyLevelModel> EntityHeirarchyLevelModels { get; set; }
        public DbSet<NotificationSetUpModel> NotificationSetUpModels { get; set; }
        public DbSet<Alertsandremindersmodel> Alertsandremindersmodels { get; set; }

        public DbSet<Notificationreminder> Notificationmailalerts { get; set; }

        public DbSet<Appspecifyconfigurtion> Appspecifyconfigurtions { get; set; }

            public DbSet<commonsettingpermission> commonsettingpermissions {  get; set; }
        //Risk Part3 Document 
        public DbSet<Risk_BusinessProcess> Risk_BusinessProcesss { get; set; }
        public DbSet<Risk_Sub_ProcessL1> Risk_Sub_ProcessL1s { get; set; }
        public DbSet<Risk_Sub_ProcessL2> Risk_Sub_ProcessL2s { get; set; }
        public DbSet<Risk_Sub_ProcessL3> Risk_Sub_ProcessL3s { get; set; }
        public DbSet<RiskDefaultNotifiers> RiskDefaultNotifierss { get; set; }

        public DbSet<Risk_AddRegisterModel> Risk_AddRegisterModels { get; set; }
        public DbSet<Risk_NotificationSetUp> Risk_NotificationSetUps { get; set; }

       // public DbSet<EmailNotification> EmailNotifications { get; set; }

        // Risk part 2

        public DbSet<RiskBusinessFunctionModel> RiskBusinessFunctionModels {  get; set; }
        public DbSet<assessmentAttributesmodel> assessmentAttributesmodels { get; set; }

        public DbSet<RiskAssessmentTemplateModel> RiskAssessmentTemplateModels {  get; set; }
        public DbSet<riskAssessmenttemplatesubtypemodel> riskAssessmenttemplatesubtypemodels { get; set; }

        public DbSet<RIskKeyfocusAreaModel> RIskKeyfocusAreaModels { get; set; }
        public DbSet<riskoverallappititeModel> riskoverallappititeModels { get; set; }

        public DbSet<risklosseventtrackermodel> risklosseventtrackermodels {  get; set; }

        public DbSet<riskquestionbankattributekeyarea> riskquestionbankattributekeyareas {  get; set; }

        public DbSet<riskquestionbankattributeSubkeyarea> riskquestionbankattributeSubkeyareas { get; set; }

        public DbSet<Risk_RiskStatement> Risk_RiskStatements { get; set; }
        public DbSet<Risk_StatementFileMaster> Risk_StatementFileMasters { get; set; }
        public DbSet<RiskAdminModel> riskAdminModels { get; set; }
        public DbSet<risk_admin_classification> riskAdminClassifications { get; set; }
        public DbSet<risk_admin_riskimpactrating> risk_admin_riskimpactrating { get; set; }
        public DbSet<risk_admin_likeoccfact> risk_admin_likeoccfact { get; set; }
        public DbSet<risk_admin_risk_categorization> risk_admin_risk_categorization { get; set; }
        public DbSet<risk_admin_causelist> risk_admin_causelist { get; set; }
        public DbSet<risk_admin_riskpriority> risk_admin_riskpriority { get; set; }
        public DbSet<risk_admin_potenbussimpact> risk_admin_potenbussimpact { get; set; }
        public DbSet<risk_admin_riskappetite> risk_admin_riskappetite { get; set; }
        public DbSet<risk_admin_risktolerance> risk_admin_risktolerance { get; set; }
        public DbSet<risk_admin_inherriskratinglevl> risk_admin_inherriskratinglevl { get; set; }
        public DbSet<risk_admin_riskintensity> risk_admin_riskintensity { get; set; }
        public DbSet<risk_admin_letc_l1> risk_admin_letc_l1 { get; set; }
        public DbSet<risk_admin_letc_l2> risk_Admin_Letc_L2s { get; set; }
        public DbSet<risk_admin_letc_l3> risk_admin_letc_l3 { get; set; }
        public DbSet<risk_admin_naturecontrperf> risk_admin_naturecontrperf { get; set; }
        public DbSet<risk_admin_natucontroccur> risk_admin_natucontroccur { get; set; }
        public DbSet<risk_admin_contrlevel> risk_admin_contrlevel { get; set; }
        public DbSet<risk_admin_contrdepen> risk_admin_contrdepen { get; set; }
        public DbSet<risk_admin_frqcontrapplid> risk_Admin_Frqcontrapplids { get; set; }
        public DbSet<risk_admin_bpmatratscaleindicator> risk_admin_bpmatratscaleindicator { get; set; }
        public DbSet<risk_admin_contrasstestatt> risk_admin_contrasstestatt {  get; set; }

        public DbSet<risk_admin_iniassimpfact> risk_admin_iniassimpfact { get; set; }
        public DbSet<risk_admin_mitdecilist> risk_admin_mitdecilist { get; set; }
        public DbSet<risk_admin_asscontracptcrit> risk_admin_asscontracptcrit {  get; set; }


        public DbSet<riskConEffRatingModel> riskConEffRatingModels { get; set; }
        public DbSet<risk_admin_control_risk_of_assessment> risk_admin_control_risk_of_assessments { get; set; }
        public DbSet<risk_admin_residual_risk_rating> risk_admin_residual_risk_ratings { get; set; }
        public DbSet<risk_admin_control_measure> risk_admin_control_measures { get; set; }
        public DbSet<risk_admin_risktredecilist> risk_admin_risktredecilist { get; set; }
        public DbSet<risk_admin_risktrdecimatrix> risk_admin_risktrdecimatrix { get; set; }
        public DbSet<risk_admin_mitactireq> risk_admin_mitactireq { get; set; }
        public DbSet<risk_admin_actiprilist> risk_admin_actiprilist {  get; set; }

        public DbSet<risk_admin_inter_contr_comp> risk_admin_inter_contr_comps { get; set; }
        public DbSet<risk_admin_inter_contr_principles> risk_admin_inter_contr_principless { get; set; }
        public DbSet<risk_admin_controlactivitytype> risk_admin_controlactivitytypes { get; set; }
        public DbSet<risk_admin_control_activity_nature> risk_admin_control_activity_natures { get; set; }
        public DbSet<risk_admin_control_activity_sub_nature> risk_admin_control_activity_sub_natures { get; set; }
        public  DbSet<risk_db_control_assertion_check> risk_db_control_assertion_checks { get; set; }
        public DbSet<risk_admin_control_reference_type> risk_admin_control_reference_types { get; set; }
        public DbSet<risk_admin_con_accept_benchmark> risk_admin_con_accept_benchmarks { get; set; }
        public DbSet<DocumentConfidentiality> DocumentConfidentialitys { get; set; }

        public DbSet<riskadmineventmodel> riskadmineventmodels { get; set; }

        public DbSet<riskadminactivityfrequency> riskadminactivityfrequencymodels { get; set; }

        public DbSet<riskadmincontrolcontrolermodel> riskadmincontrolcontrolermodels { get; set; }

        public DbSet<riskadmincontrolmonitoringmechmodel> riskadmincontrolmonitoringmechmodels { get; set; }

        public DbSet<risksamplingstandards> risksamplingstandardsmodel { get; set; }
        public DbSet<keyControls> keyControls { get; set; }

        public DbSet<nonKeyControls> nonKeyControls { get; set; }

        public DbSet<generalControls> generalControls { get; set; }
        public DbSet<controlstatementmodel> controlstatementmodels { get; set; }

        public DbSet<controlstatementfilemodel> controlstatementfilemodels { get; set; }


        public DbSet<controlsubstatementmodel> controlsubstatementmodels { get; set; }


        // FTU
        public DbSet<RemediationPlanModel> RemediationPlanModels { get; set; }
        public DbSet<UpdateComplianceModel> UpdateComplianceModels { get; set; }
        public DbSet<complainceusermapping> ComplainceUserMappingModels { get; set; }
        public DbSet<UpdateComplianceFilesModel> UpdateComplianceFilesModels { get; set; }
        public DbSet<HelpModel> HelpModels { get; set; }

    }

    public class CommonDBContext : DbContext
    {
        public CommonDBContext(DbContextOptions<CommonDBContext> options) : base(options)
        { }

        public DbSet<SupAdminEntityTypeModel> SupAdminEntityTypeModels { get; set; }
        public DbSet<SupAdmin_UnitLocationTypeModel> SupAdmin_UnitLocationTypeModels { get; set; }
        public DbSet<SupAdmin_BusinessSectorListModel> SupAdmin_BusinessSectorListModels { get; set; }
        public DbSet<SupAdmin_IndustryTypeListModel> SupAdmin_IndustryTypeListModels { get; set; }
        public DbSet<SupAdmin_RegionModel> SupAdmin_RegionModels { get; set; }
        public DbSet<SupAdmin_SubRegionModel> SupAdmin_SubRegionModels { get; set; }
        public DbSet<SupAdmin_FrequencyModel> SupAdmin_FrequencyModels { get; set; }
        public DbSet<SupAdmin_HolidayModel> SupAdmin_HolidayModels { get; set; }

        //second Common Entity Attributes
        public DbSet<SupadminComplianceType> SupadminComplianceTypes { get; set; }
        public DbSet<SupAdmin_CategoryOfLawModel> SupAdmin_CategoryOfLawModels { get; set; }
        public DbSet<SupAdmin_NatureOfLawModel> SupAdmin_NatureOfLawModels { get; set; }
        public DbSet<SupAdmin_JurisdictionListModel> SupAdmin_JurisdictionListModels { get; set; }
        public DbSet<SupAdmin_ComplianceRecordTypeModel> SupAdmin_ComplianceRecordTypeModels { get; set; }
        public DbSet<SupAdmin_RegulatoryAuthorityModel> SupAdmin_RegulatoryAuthorityModels { get; set; }
        public DbSet<SupAdmin_ComplianceRiskClassificationModel> SupAdmin_ComplianceRiskClassificationModels { get; set; }
        public DbSet<SupAdmin_PenaltyCategoryModel> SupAdmin_PenaltyCategoryModels { get; set; }
        public DbSet<SupAdmin_ComplianceNotifiedStatusModel> SupAdmin_ComplianceNotifiedStatusModels { get; set; }
        public DbSet<SupAdmin_ComplianceGroupModel> SupAdmin_ComplianceGroupModels { get; set; }

        public DbSet<SupAdmin_JurisdictionLocationModel> SupAdmin_JurisdictionLocationModels { get; set; }
        public DbSet<SupAdmin_ComplianceRiskCriteriaModel> SupAdmin_ComplianceRiskCriteriaModels { get; set; }
        public DbSet<SupAdmin_CompliancePeriodModel> SupAdmin_CompliancePeriodModels { get; set; }

        public DbSet<SupAdmin_ActRegulatoryModel> SupAdmin_ActRegulatoryModels { get; set; }
        public DbSet<SupAdmin_Actregulatoryfilemodel> SupAdmin_Actregulatoryfilemodels { get; set; }
        public DbSet<SupAdmin_RulesandRegulatoryModel> SupAdmin_RulesandRegulatoryModels { get; set; }
        public DbSet<SupAdmin_ActRuleregulatoryfilemodel> SupAdmin_ActRuleregulatoryfilemodels { get; set; }
        public DbSet<SupAdmin_StatutoryformsModel> SupAdmin_StatutoryformsModels { get; set; }
        public DbSet<SupAdmin_statutoryformsrecordsfilemodel> SupAdmin_statutoryformsrecordsfilemodels { get; set; }
        public DbSet<SupAdmin_CategoryPenaltyModel> SupAdmin_compliancepenatlymasterModels { get; set; }
        public DbSet<SupAdmin_PenaltyCategoryfileModel> SupAdmin_PenaltyCategoryfileModels { get; set; }
        public DbSet<CreateGlobalComplianceModel> CreateGlobalComplianceModels { get; set; }

        public DbSet<GlobalComplianceScheduler> GlobalComplianceSchedulers { get; set; }
        public DbSet<CountryModel> SupAdmin_CountryModelModels { get; set; }
        public DbSet<StateModel> SupAdmin_StateModelModels { get; set; }
        public DbSet<usermodel> usermodels { get; set; }

        // Risk Matrix Attributes
        public DbSet<RiskSup_TypeOfRisk> RiskSup_TypeOfRisks { get; set; }
        public DbSet<RiskSup_RiskClassification> RiskSup_RiskClassifications { get; set; }
        public DbSet<RiskSup_ImpactRating> RiskSup_ImpactRatings { get; set; }
        public DbSet<RiskMatrixModel> RiskMatrixModels { get; set; }
        public DbSet<RiskMatrixcauseList> riskMatrixcauseLists { get; set; }

        public DbSet<RiskSupAdminModel> RiskSupAdminModels { get; set; }

        public DbSet<potential_business_impact> potential_business_impacts { get; set; }
        public DbSet<loss_event_threat_category> loss_event_threat_categorys { get; set; }
        public DbSet<losseventthreacategory_l2> losseventthreacategory_l2s { get; set; }
        public DbSet<losseventthreacategory_l3> losseventthreacategory_l3s { get; set; }


        public DbSet<RiskLikelihood> riskLikelihoods { get; set; }
        public DbSet<RiskControlMatrixAttModel> riskControlMatrixAttModels { get; set; }
        public DbSet<RiskControlLevel> riskControlLevels { get; set; }
        public DbSet<RiskControldependency> riskControldependencies { get; set; }
        public DbSet<getcontrol_measure> getcontrol_measures { get; set; }
        public DbSet<control_activity_type> control_activity_types { get; set; }
        public DbSet<control_reference_type> control_reference_types { get; set; }
        public DbSet<mitigation_action> mitigation_actions { get; set; }
        public DbSet<action_priority_list> action_priority_lists { get; set; }

        public DbSet<RiskImpactRating> riskImpactRatings { get; set; }
        public DbSet<residual_risk_rating> residual_risk_ratings { get; set; }

        public DbSet<control_risk_of_assessment> control_risk_of_assessments { get; set; }
        public DbSet<RiskInherentRatingLevel> riskInherentRatingLevels { get; set; }

        public DbSet<risk_intensity> risk_Intensities { get; set; }

        public DbSet<risk_natureof_cont_occu> risk_Natureof_Cont_Occus { get; set; }
        public DbSet<risk_frqof_contr_appl> risk_frqof_contr_appl { get; set; }
        public DbSet<riskcontroleffectivenessrating> riskcontroleffectivenessratings { get; set; }


        public DbSet<cont_test_cont_relevance> cont_Test_Cont_Relevances { get; set; }
        public DbSet<risk_asses_contr_accep_crit> risk_Asses_Contr_Accep_Crits { get; set; }
        public DbSet<risk_mitigation_decision> risk_Mitigation_Decisions { get; set; }
        public DbSet<risk_initial_assessment_impact_factor> risk_Initial_Assessment_Impact_Factors { get; set; }
        public DbSet<bpmaturityratingscaleindicators> bpmaturityratingscaleindicatorss { get; set; }
        public DbSet<controlassesstestattributes> controlassesstestattributess { get; set; }
        public DbSet<RiskTreatmentDecisionList> RiskTreatmentDecisionLists { get; set; }
        public DbSet<risk_treatmetdecisionmatrix> risk_treatmetdecisionmatrixs { get; set; }
        public DbSet<risk_controltestdecisionlist> risk_controltestdecisionlists { get; set; }


        public DbSet<risksuperadmincontrolcompmodel> risksuperadmincontrolcompmodels { get; set; }

        public DbSet<risksuperadminactivityfrequencymodel> risksuperadminactivityfrequencymodels { get; set; }

        public DbSet<risksuperadmineventfrequencymodel> risksuperadmineventfrequencymodels { get; set; }

        public DbSet<risksuperadmincontrolmonitoringmodel> risksuperadmincontrolmonitoringmodels {  get; set; }

    }
}
