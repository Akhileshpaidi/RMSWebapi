using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using DomainModel;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Linq;

namespace ITRTelemetry.Controllers
{
    [Produces("application/json")]

    public class DataImportController : ControllerBase
    {

        private readonly MySqlDBContext _dbContext;
        private readonly CommonDBContext _commondbContext;
        private readonly IHttpClientFactory _httpClientFactory;



        public DataImportController(MySqlDBContext dbContext, CommonDBContext commondbContext, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _commondbContext = commondbContext;
            _httpClientFactory = httpClientFactory;
           
        }
        [Route("api/DataImport/GetDataImport")]
        [HttpGet]
        //public async Task<IActionResult> ImportData()
        //{
        //    try
        //    {
        //        var httpClient = _httpClientFactory.CreateClient();
        //        var apiUrl = "http://localhost:18593/api/SuperAdminEntityType/GetEntityTypeMasterDetails";

        //        var response = await httpClient.GetAsync(apiUrl);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            return BadRequest("Failed to retrieve data from the API");
        //        }

        //        var jsonString = await response.Content.ReadAsStringAsync();
        //        var entities = JsonConvert.DeserializeObject<List<entityTypeModel>>(jsonString);

        //        var existingEntityNames = await _dbContext.entityTypeModels
        //          .Select(e => e.entitytypename)
        //          .ToListAsync();

        //        var entitiesToAdd = entities.Where(e => !existingEntityNames.Contains(e.entitytypename)).ToList();

        //        if (entitiesToAdd.Any())
        //        {
        //            foreach (var entity in entitiesToAdd)
        //            {
        //                entity.createddate = Convert.ToDateTime(entity.createddate).ToString("yyyy-MM-dd HH:mm:ss");
        //                entity.entitytypeid = 0;
        //            }



        //            // Add the imported entities to the database
        //            _dbContext.entityTypeModels.AddRange(entitiesToAdd);
        //            await _dbContext.SaveChangesAsync();

        //            return Ok("Data imported successfully");
        //        }

        //        return Ok("no new records were found to import");
        //    }
        //    //catch (Exception ex)
        //    //{
        //    //    return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        //    //}
        //    catch (DbUpdateException ex)
        //    {
        //        var mysqlException = ex.InnerException as MySqlException;
        //        if (mysqlException != null && mysqlException.Number == 1062) // MySQL error code for duplicate entry
        //        {
        //            // Handle duplicate key exception (e.g., log the error or skip duplicates)
        //            return BadRequest("Duplicate entry: " + mysqlException.Message);
        //        }
        //        else
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        //        }
        //    }
        //}

        //**Other common Settings**//
        [Route("api/DataImport/GetOtherCommonDataImportCommonDBToLocalDB")]
        [HttpPost]
        public async Task<IActionResult> GetOtherCommonDataImportCommonDBToLocalDB()
        {
            try
            {
                // Transfer data from SupAdmin_CompliancePeriodModel to CompliancePeriodModel
                await TransferDataFromSupAdminToCompliancPeriod();

                return Ok("All data imported successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }


        //** common entity attributes **//
        [Route("api/DataImport/GetDataImportCommonDBToLocalDB")]
        [HttpPost]
        public async Task<IActionResult> GetDataImportCommonDBToLocalDB()
        {
            try
            {
                // Transfer data from SupAdminEntityTypeModel to entityTypeModel
                await TransferDataFromSupAdminToEntity();

                // Transfer data from SupAdmin_UnitLocationTypeModel to UnitTypeModel
                await TransferDataFromSupAdminToUnitType();

                // Transfer data from SupAdmin_BusinessSectorListModel to Businesssectormodel
                await TransferDataFromSupAdminToBusinessSector();

                // Transfer data from SupAdmin_IndustryTypeListModel to industrytypemodel
                await TransferDataFromSupAdminToIndustryType();

                // Transfer data from SupAdmin_RegionModel to RegionModel
                await TransferDataFromSupAdminToRegion();

                // Transfer data from SupAdmin_SubRegionModel to SubRegionModel
                await TransferDataFromSupAdminToSub_Region();

                
                return Ok("All data imported successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
      


        //** frequency & holiday common entity attributes **//
        [Route("api/DataImport/GetFrequencyHolidayDataImportCommonDBToLocalDB")]
        [HttpPost]
        public async Task<IActionResult> GetFrequencyHolidayDataImportCommonDBToLocalDB()
        {
            try
            {
                // Transfer data from SupAdmin_FrequencyModel to Frequencymodel
                await TransferDataFromSupAdminToFrequency();

                // Transfer data from SupAdmin_HolidayModel to Holidaymaster
                await TransferDataFromSupAdminToHoliday();
                             
                return Ok("All data imported successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
        //** common compliance library attributes **//

        [Route("api/DataImport/GetComplianceDataImportCommonDBToLocalDB")]
        [HttpPost]
        public async Task<IActionResult> GetComplianceDataImportCommonDBToLocalDB()
        {
            try
            {
                // Transfer data from SupAdmin_CategoryOfLawModel to catageoryoflawmodel
                await TransferDataFromSupAdminToCategoryOfLaw();

                // Transfer data from SupAdmin_NatureOfLawModel to LawTypeModel
                await TransferDataFromSupAdminToNatureOfLaw();

                // Transfer data from SupAdmin_JurisdictionListModel to Jurisdictionmodel
                await TransferDataFromSupAdminToJurisdictionList();

                // Transfer data from SupAdmin_ComplianceRecordTypeModel to ComplianceRecordTypeModel
                await TransferDataFromSupAdminToComplianceRecordType();

                // Transfer data from SupAdmin_RegulatoryAuthorityModel to RegulatoryAuthorityModel
                await TransferDataFromSupAdminToRegulatoryAuthority();

                // Transfer data from ComplianceRiskClassificationModel to ComplianceRiskClassificationModel
                await TransferDataFromSupAdminToComplianceRiskClassification();

                // Transfer data from SupAdmin_PenaltyCategoryModel to PenaltyCategoryModel
                await TransferDataFromSupAdminToPenaltyCategory();

                // Transfer data from SupAdmin_ComplianceNotifiedStatusModel to ComplianceNotifiedStatusModel
                await TransferDataFromSupAdminToComplianceNotifiedStatus();

                // Transfer data from SupadminComplianceType to ComplianceModel
                await TransferDataFromSupAdminToCompliance();

                // Transfer data from SupAdmin_ComplianceGroupModel to ComplianceGroupModel
                await TransferDataFromSupAdminToComplianceGroup();

                // Transfer data from SupAdmin_JurisdictionLocationModel to JurisdictionLocationModel
                await TransferDataFromSupAdminToJurisdictionLocation();

                // Transfer data from SupAdmin_ComplianceRiskCriteriaModel to ComplianceRiskClassificationCriteriaModel
                await TransferDataFromSupAdminToCompliancRiskCriteria();

                return Ok("All data imported successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        //****  Act Regulatory Universe  **** //
        [Route("api/DataImportForAct/GetActDataImportCommonDBToLocalDB")]
        [HttpPost]

        public async Task<IActionResult> GetActDataImportCommonDBToLocalDB()
        {
            try
            {
                // Transfer data from SupAdmin_ActRegulatoryModel to Actregulatorymodel
                await TransferDataFromSupAdminToActRegulatory();

                // Transfer data from SupAdmin_Actregulatoryfilemodel to Actregulatoryfilemodel
                await TransferDataFromSupAdminToActRegulatoryfile();

                // Transfer data from SupAdmin_RulesandRegulatoryModel to Rulesandregulatorymodel
                await TransferDataFromSupAdminToRulesandRegulatory();

                // Transfer data from SupAdmin_ActRuleregulatoryfilemodel to ActRuleregulatoryfilemodel
                await TransferDataFromSupAdminToRulesandRegulatoryfiles();

                // Transfer data from SupAdmin_StatutoryformsModel to statutoryformsrecordsmodel
                await TransferDataFromSupAdminToStatutoryforms();

                // Transfer data from SupAdmin_statutoryformsrecordsfilemodel to statutoryformsrecordsfilemodel
                await TransferDataFromSupAdminToStatutoryfies();

                // Transfer data from SupAdmin_CategoryPenaltyModel to compliancepenatlymasterModel
                await TransferDataFromSupAdminToComplinacePenatly();

                // Transfer data from SupAdmin_PenaltyCategoryfileModel to PenaltyCategoryfileModel
                await TransferDataFromSupAdminToComplinacePenatlyfiles();


                return Ok("All data imported successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        //updated
        private async Task TransferDataFromSupAdminToEntity()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminEntities = await _commondbContext.SupAdminEntityTypeModels
                .Where(e => e.entitytypestatus == "Active")
                .ToListAsync();

            var existingEntities = await _dbContext.entityTypeModels.ToListAsync();
            var existingEntityMap = existingEntities.ToDictionary(e => e.entitytypeid);

            var newEntities = new List<entityTypeModel>();

            foreach (var entity in superAdminEntities)
            {
                if (existingEntityMap.TryGetValue(entity.entitytypeid, out var existingEntity))
                {
                    // Update existing record (excluding primary key)
                    existingEntity.entitytypename = entity.entitytypename;
                    existingEntity.entitytypeDesc = entity.entitytypeDesc;
                    existingEntity.entitytypestatus = entity.entitytypestatus;
                    existingEntity.createdBy = entity.createdBy;
                    existingEntity.createddate = dt1;
                    existingEntity.source = "Yes";
                    existingEntity.entitytypetablename = entity.entitytypetablename;
                }
                else
                {
                    // Add new record
                    newEntities.Add(new entityTypeModel
                    {
                        entitytypeid = entity.entitytypeid,
                        entitytypename = entity.entitytypename,
                        entitytypeDesc = entity.entitytypeDesc,
                        entitytypestatus = entity.entitytypestatus,
                        createdBy = entity.createdBy,
                        createddate = dt1,
                        source = "Yes",
                        entitytypetablename = entity.entitytypetablename
                    });
                }
            }

            if (newEntities.Any())
            {
                _dbContext.entityTypeModels.AddRange(newEntities);
            }

            await _dbContext.SaveChangesAsync();
        }
        //updated
        private async Task TransferDataFromSupAdminToUnitType()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminUnitTypes = await _commondbContext.SupAdmin_UnitLocationTypeModels
                .Where(e => e.UnitTypeStatus == "Active")
                .ToListAsync();

            var existingUnitTypes = await _dbContext.UnitTypeModels.ToListAsync();
            var existingUnitMap = existingUnitTypes.ToDictionary(e => e.UnitTypeID);

            var newUnitTypes = new List<UnitTypeModel>();

            foreach (var unitType in superAdminUnitTypes)
            {
                if (existingUnitMap.TryGetValue(unitType.UnitTypeID, out var existingUnit))
                {
                    // Update existing unit type (excluding primary key)
                    existingUnit.UnitTypeName = unitType.UnitTypeName;
                    existingUnit.UnitTypeStatus = unitType.UnitTypeStatus;
                    existingUnit.unittypeDesc = unitType.unittypeDesc;
                    existingUnit.createdBy = unitType.createdBy;
                    existingUnit.createddate = dt1;
                    existingUnit.source = "Yes";
                    existingUnit.unittypetablename = unitType.unittypetablename;
                }
                else
                {
                    // Add new unit type
                    newUnitTypes.Add(new UnitTypeModel
                    {
                        UnitTypeID = unitType.UnitTypeID,
                        UnitTypeName = unitType.UnitTypeName,
                        UnitTypeStatus = unitType.UnitTypeStatus,
                        unittypeDesc = unitType.unittypeDesc,
                        createdBy = unitType.createdBy,
                        createddate = dt1,
                        source = "Yes",
                        unittypetablename = unitType.unittypetablename
                    });
                }
            }

            if (newUnitTypes.Any())
            {
                _dbContext.UnitTypeModels.AddRange(newUnitTypes);
            }

            await _dbContext.SaveChangesAsync();
        }
        //updated
        private async Task TransferDataFromSupAdminToBusinessSector()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminBusinessSector = await _commondbContext.SupAdmin_BusinessSectorListModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            var existingSectors = await _dbContext.Businesssectormodels.ToListAsync();
            var existingSectorMap = existingSectors.ToDictionary(e => e.businesssectorid);

            var newSectors = new List<Businesssectormodel>();

            foreach (var entity in superAdminBusinessSector)
            {
                if (existingSectorMap.TryGetValue(entity.businesssectorid, out var existingEntity))
                {
                    // Update existing record
                    existingEntity.businesssectorname = entity.businesssectorname;
                    existingEntity.businesssectordescriptio = entity.businesssectordescriptio;
                    existingEntity.status = entity.status;
                    existingEntity.createdBy = entity.createdBy;
                    existingEntity.createddate = dt1;
                    existingEntity.source = "Yes";
                    existingEntity.businesssectortable = entity.businesssectortable;
                }
                else
                {
                    // Add new record
                    newSectors.Add(new Businesssectormodel
                    {
                        businesssectorid = entity.businesssectorid,
                        businesssectorname = entity.businesssectorname,
                        businesssectordescriptio = entity.businesssectordescriptio,
                        status = entity.status,
                        createdBy = entity.createdBy,
                        createddate = dt1,
                        source = "Yes",
                        businesssectortable = entity.businesssectortable
                    });
                }
            }

            if (newSectors.Any())
            {
                _dbContext.Businesssectormodels.AddRange(newSectors);
            }

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToIndustryType()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminIndustryType = await _commondbContext.SupAdmin_IndustryTypeListModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            var existingIndustryTypes = await _dbContext.industrytypemodels.ToListAsync();
            var existingIndustryMap = existingIndustryTypes.ToDictionary(e => e.industrytypeid);

            var newIndustryTypes = new List<industrytypemodel>();

            foreach (var entity in superAdminIndustryType)
            {
                if (existingIndustryMap.TryGetValue(entity.industrytypeid, out var existingEntity))
                {
                    // Update existing record (except primary key)
                    existingEntity.industrytypename = entity.industrytypename;
                    existingEntity.industrytypedescription = entity.industrytypedescription;
                    existingEntity.businesssectorid = entity.businesssectorid;
                    existingEntity.status = entity.status;
                    existingEntity.createdBy = entity.createdBy;
                    existingEntity.createddate = dt1;
                    existingEntity.source = "Yes";
                    existingEntity.industrytyptable = entity.industrytyptable;
                }
                else
                {
                    // Add new record
                    newIndustryTypes.Add(new industrytypemodel
                    {
                        industrytypeid = entity.industrytypeid,
                        industrytypename = entity.industrytypename,
                        industrytypedescription = entity.industrytypedescription,
                        businesssectorid = entity.businesssectorid,
                        status = entity.status,
                        createdBy = entity.createdBy,
                        createddate = dt1,
                        source = "Yes",
                        industrytyptable = entity.industrytyptable
                    });
                }
            }

            if (newIndustryTypes.Any())
            {
                _dbContext.industrytypemodels.AddRange(newIndustryTypes);
            }

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToFrequency()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminFrequency = await _commondbContext.SupAdmin_FrequencyModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            var existingFrequencies = await _dbContext.Frequencymodels.ToListAsync();
            var existingFrequencyMap = existingFrequencies.ToDictionary(e => e.frequencyid);

            var newFrequencies = new List<Frequencymodel>();

            foreach (var entity in superAdminFrequency)
            {
                if (existingFrequencyMap.TryGetValue(entity.frequencyid, out var existingEntity))
                {
                    // Update existing frequency
                    existingEntity.recurenceid = entity.recurenceid;
                    existingEntity.frequencyperiod = entity.frequencyperiod;
                    existingEntity.frequencyDescription = entity.frequencyDescription;
                    existingEntity.timeperiod = entity.timeperiod;
                    existingEntity.timeinterval = entity.timeinterval;
                    existingEntity.nooffrequencyintervals = entity.nooffrequencyintervals;
                    existingEntity.status = entity.status;
                    existingEntity.createdby = entity.createdby;
                    existingEntity.createddate = dt1;
                    existingEntity.source = "Yes";
                }
                else
                {
                    // Add new frequency
                    newFrequencies.Add(new Frequencymodel
                    {
                        frequencyid = entity.frequencyid,
                        recurenceid = entity.recurenceid,
                        frequencyperiod = entity.frequencyperiod,
                        frequencyDescription = entity.frequencyDescription,
                        timeperiod = entity.timeperiod,
                        timeinterval = entity.timeinterval,
                        nooffrequencyintervals = entity.nooffrequencyintervals,
                        status = entity.status,
                        createdby = entity.createdby,
                        createddate = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newFrequencies.Any())
            {
                _dbContext.Frequencymodels.AddRange(newFrequencies);
            }

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToHoliday()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminHoliday = await _commondbContext.SupAdmin_HolidayModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            var existingHolidayMap = await _dbContext.Holidaymasters
                .ToDictionaryAsync(e => e.holidayid);

            var newHolidays = new List<Holidaymaster>();

            foreach (var entity in superAdminHoliday)
            {
                if (existingHolidayMap.TryGetValue(entity.holidayid, out var existing))
                {
                    existing.holidayname = entity.holidayname;
                    existing.recurence = entity.recurence;
                    existing.holidaytimeperiod = entity.holidaytimeperiod;
                    existing.hoildaytimeinterval = entity.hoildaytimeinterval;
                    existing.holidaydescription = entity.holidaydescription;
                    existing.status = entity.status;
                    existing.createdby = entity.createdby;
                    existing.createddate = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newHolidays.Add(new Holidaymaster
                    {
                        holidayid = entity.holidayid,
                        holidayname = entity.holidayname,
                        recurence = entity.recurence,
                        holidaytimeperiod = entity.holidaytimeperiod,
                        hoildaytimeinterval = entity.hoildaytimeinterval,
                        holidaydescription = entity.holidaydescription,
                        status = entity.status,
                        createdby = entity.createdby,
                        createddate = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newHolidays.Any())
                _dbContext.Holidaymasters.AddRange(newHolidays);

            await _dbContext.SaveChangesAsync();
        }


        //update
        private async Task TransferDataFromSupAdminToRegion()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminRegion = await _commondbContext.SupAdmin_RegionModels
                .Where(e => e.RegionStatus == "Active")
                .ToListAsync();

            var existingRegionMap = await _dbContext.RegionModels
                .ToDictionaryAsync(e => e.RegionMasterID);

            var newRegions = new List<RegionModel>();

            foreach (var entity in superAdminRegion)
            {
                if (existingRegionMap.TryGetValue(entity.RegionMasterID, out var existing))
                {
                    existing.RegionName = entity.RegionName;
                    existing.regionDesc = entity.regionDesc;
                    existing.RegionStatus = entity.RegionStatus;
                    existing.createdBy = entity.createdBy;
                    existing.createddate = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newRegions.Add(new RegionModel
                    {
                        RegionMasterID = entity.RegionMasterID,
                        RegionName = entity.RegionName,
                        regionDesc = entity.regionDesc,
                        RegionStatus = entity.RegionStatus,
                        createdBy = entity.createdBy,
                        createddate = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newRegions.Any())
                _dbContext.RegionModels.AddRange(newRegions);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToSub_Region()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminSubRegion = await _commondbContext.SupAdmin_SubRegionModels
                .Where(e => e.SubRegionStatus == "Active")
                .ToListAsync();

            var existingSubRegionMap = await _dbContext.SubRegionModels
                .ToDictionaryAsync(e => e.Sub_RegionMasterID);

            var newSubRegions = new List<SubRegionModel>();

            foreach (var entity in superAdminSubRegion)
            {
                if (existingSubRegionMap.TryGetValue(entity.Sub_RegionMasterID, out var existing))
                {
                    existing.Sub_RegionName = entity.Sub_RegionName;
                    existing.Description = entity.Description;
                    existing.RegionMasterID = entity.RegionMasterID;
                    existing.SubRegionStatus = entity.SubRegionStatus;
                    existing.createdBy = entity.createdBy;
                    existing.createddate = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newSubRegions.Add(new SubRegionModel
                    {
                        Sub_RegionMasterID = entity.Sub_RegionMasterID,
                        Sub_RegionName = entity.Sub_RegionName,
                        Description = entity.Description,
                        RegionMasterID = entity.RegionMasterID,
                        SubRegionStatus = entity.SubRegionStatus,
                        createdBy = entity.createdBy,
                        createddate = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newSubRegions.Any())
                _dbContext.SubRegionModels.AddRange(newSubRegions);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToCategoryOfLaw()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            // Get active records from SupAdmin
            var superAdminCategoryLaw = await _commondbContext.SupAdmin_CategoryOfLawModels
                .Where(e => e.status == "Active")
                .ToListAsync();

          
            var existingCategoryLaws = await _dbContext.catageoryoflawmodels.ToListAsync();
            var existingLawMap = existingCategoryLaws.ToDictionary(e => e.category_of_law_ID);

            var newEntities = new List<catageoryoflawmodel>();

            foreach (var entity in superAdminCategoryLaw)
            {
                if (existingLawMap.TryGetValue(entity.category_of_law_ID, out var existingEntity))
                {
                    // Update existing record (except primary key)
                    existingEntity.law_Categoryname = entity.law_Categoryname;
                    existingEntity.category_of_Law_Description = entity.category_of_Law_Description;
                    existingEntity.status = entity.status;
                    existingEntity.createdby = entity.createdby;
                    existingEntity.category_of_Law_Create_Date = dt1;
                    existingEntity.source = "Yes";
                }
                else
                {
                    // New record to add
                    newEntities.Add(new catageoryoflawmodel
                    {
                        category_of_law_ID = entity.category_of_law_ID,
                        law_Categoryname = entity.law_Categoryname,
                        category_of_Law_Description = entity.category_of_Law_Description,
                        status = entity.status,
                        createdby = entity.createdby,
                        category_of_Law_Create_Date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
            {
                _dbContext.catageoryoflawmodels.AddRange(newEntities);
            }

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToNatureOfLaw()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminNatureLaw = await _commondbContext.SupAdmin_NatureOfLawModels
                .Where(e => e.law_status == "Active")
                .ToListAsync();

            var existingMap = await _dbContext.LawTypeModels
                .ToDictionaryAsync(e => e.law_type_id);

            var newEntities = new List<LawTypeModel>();

            foreach (var entity in superAdminNatureLaw)
            {
                if (existingMap.TryGetValue(entity.law_type_id, out var existing))
                {
                    existing.type_of_law = entity.type_of_law;
                    existing.law_description = entity.law_description;
                    existing.law_status = entity.law_status;
                    existing.createdby = entity.createdby;
                    existing.law_create_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new LawTypeModel
                    {
                        law_type_id = entity.law_type_id,
                        type_of_law = entity.type_of_law,
                        law_description = entity.law_description,
                        law_status = entity.law_status,
                        createdby = entity.createdby,
                        law_create_date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
                _dbContext.LawTypeModels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToJurisdictionList()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminJurisdiction = await _commondbContext.SupAdmin_JurisdictionListModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            var existingMap = await _dbContext.Jurisdictionmodels
                .ToDictionaryAsync(e => e.jurisdiction_category_id);

            var newEntities = new List<Jurisdictionmodel>();

            foreach (var entity in superAdminJurisdiction)
            {
                if (existingMap.TryGetValue(entity.jurisdiction_category_id, out var existing))
                {
                    existing.jurisdiction_categoryname = entity.jurisdiction_categoryname;
                    existing.jurisdiction_category_description = entity.jurisdiction_category_description;
                    existing.status = entity.status;
                    existing.createdby = entity.createdby;
                    existing.jurisdiction_category_create_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new Jurisdictionmodel
                    {
                        jurisdiction_category_id = entity.jurisdiction_category_id,
                        jurisdiction_categoryname = entity.jurisdiction_categoryname,
                        jurisdiction_category_description = entity.jurisdiction_category_description,
                        status = entity.status,
                        createdby = entity.createdby,
                        jurisdiction_category_create_date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
                _dbContext.Jurisdictionmodels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToComplianceRecordType()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminComplianceType = await _commondbContext.SupAdmin_ComplianceRecordTypeModels
                .Where(e => e.compliance_record_status == "Active")
                .ToListAsync();

            var existingMap = await _dbContext.ComplianceRecordTypeModels
                .ToDictionaryAsync(e => e.compliance_record_type_id);

            var newEntities = new List<ComplianceRecordTypeModel>();

            foreach (var entity in superAdminComplianceType)
            {
                if (existingMap.TryGetValue(entity.compliance_record_type_id, out var existing))
                {
                    existing.compliance_record_name = entity.compliance_record_name;
                    existing.compliance_record_description = entity.compliance_record_description;
                    existing.compliance_record_status = entity.compliance_record_status;
                    existing.compliance_createdby = entity.compliance_createdby;
                    existing.compliance_record_create_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new ComplianceRecordTypeModel
                    {
                        compliance_record_type_id = entity.compliance_record_type_id,
                        compliance_record_name = entity.compliance_record_name,
                        compliance_record_description = entity.compliance_record_description,
                        compliance_record_status = entity.compliance_record_status,
                        compliance_createdby = entity.compliance_createdby,
                        compliance_record_create_date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
                _dbContext.ComplianceRecordTypeModels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }

        //updated

        private async Task TransferDataFromSupAdminToRegulatoryAuthority()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminRegulatoryAuthority = await _commondbContext.SupAdmin_RegulatoryAuthorityModels
                .Where(e => e.regulatory_authority_status == "Active")
                .ToListAsync();

            var existingRegulatoryAuthorityMap = await _dbContext.RegulatoryAuthorityModels
                .ToDictionaryAsync(e => e.regulatory_authority_id);

            var newEntities = new List<RegulatoryAuthorityModel>();

            foreach (var entity in superAdminRegulatoryAuthority)
            {
                if (existingRegulatoryAuthorityMap.TryGetValue(entity.regulatory_authority_id, out var existing))
                {
                    existing.regulatory_authority_name = entity.regulatory_authority_name;
                    existing.regulatory_authority_description = entity.regulatory_authority_description;
                    existing.regulatory_authority_status = entity.regulatory_authority_status;
                    existing.createdby = entity.createdby;
                    existing.regulatory_authority_created_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new RegulatoryAuthorityModel
                    {
                        regulatory_authority_id = entity.regulatory_authority_id,
                        regulatory_authority_name = entity.regulatory_authority_name,
                        regulatory_authority_description = entity.regulatory_authority_description,
                        regulatory_authority_status = entity.regulatory_authority_status,
                        createdby = entity.createdby,
                        regulatory_authority_created_date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
                _dbContext.RegulatoryAuthorityModels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToComplianceRiskClassification()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminComplianceRisk = await _commondbContext.SupAdmin_ComplianceRiskClassificationModels
                .Where(e => e.compliance_risk_classification_status == "Active")
                .ToListAsync();

            var existingComplianceRiskMap = await _dbContext.ComplianceRiskClassificationModels
                .ToDictionaryAsync(e => e.compliance_risk_classification_id);

            var newEntities = new List<ComplianceRiskClassificationModel>();

            foreach (var entity in superAdminComplianceRisk)
            {
                if (existingComplianceRiskMap.TryGetValue(entity.compliance_risk_classification_id, out var existing))
                {
                    existing.compliance_risk_classification_name = entity.compliance_risk_classification_name;
                    existing.compliance_risk_classification_description = entity.compliance_risk_classification_description;
                    existing.compliance_risk_classification_status = entity.compliance_risk_classification_status;
                    existing.createdby = entity.createdby;
                    existing.compliance_risk_classification_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new ComplianceRiskClassificationModel
                    {
                        compliance_risk_classification_id = entity.compliance_risk_classification_id,
                        compliance_risk_classification_name = entity.compliance_risk_classification_name,
                        compliance_risk_classification_description = entity.compliance_risk_classification_description,
                        compliance_risk_classification_status = entity.compliance_risk_classification_status,
                        createdby = entity.createdby,
                        compliance_risk_classification_date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
                _dbContext.ComplianceRiskClassificationModels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToPenaltyCategory()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminPenaltyCategory = await _commondbContext.SupAdmin_PenaltyCategoryModels
                .Where(e => e.penalty_category_status == "Active")
                .ToListAsync();

            var existingPenaltyCategoryMap = await _dbContext.PenaltyCategoryModels
                .ToDictionaryAsync(e => e.penalty_category_id);

            var newEntities = new List<PenaltyCategoryModel>();

            foreach (var entity in superAdminPenaltyCategory)
            {
                if (existingPenaltyCategoryMap.TryGetValue(entity.penalty_category_id, out var existing))
                {
                    existing.penalty_category_name = entity.penalty_category_name;
                    existing.penalty_category_description = entity.penalty_category_description;
                    existing.penalty_category_status = entity.penalty_category_status;
                    existing.createdby = entity.createdby;
                    existing.penalty_category_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new PenaltyCategoryModel
                    {
                        penalty_category_id = entity.penalty_category_id,
                        penalty_category_name = entity.penalty_category_name,
                        penalty_category_description = entity.penalty_category_description,
                        penalty_category_status = entity.penalty_category_status,
                        createdby = entity.createdby,
                        penalty_category_date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
                _dbContext.PenaltyCategoryModels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToComplianceNotifiedStatus()
        {
            DateTime now = DateTime.Now;
            string dt1 = now.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminComplianceNotified = await _commondbContext.SupAdmin_ComplianceNotifiedStatusModels
                .Where(e => e.compliance_notified_status == "Active")
                .ToListAsync();

            var existingComplianceNotifiedMap = await _dbContext.ComplianceNotifiedStatusModels
                .ToDictionaryAsync(e => e.compliance_notified_id);

            var newEntities = new List<ComplianceNotifiedStatusModel>();

            foreach (var entity in superAdminComplianceNotified)
            {
                if (existingComplianceNotifiedMap.TryGetValue(entity.compliance_notified_id, out var existing))
                {
                    existing.compliance_notified_name = entity.compliance_notified_name;
                    existing.compliance_notified_description = entity.compliance_notified_description;
                    existing.compliance_notified_status = entity.compliance_notified_status;
                    existing.createdby = entity.createdby;
                    existing.compliance_notified_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new ComplianceNotifiedStatusModel
                    {
                        compliance_notified_id = entity.compliance_notified_id,
                        compliance_notified_name = entity.compliance_notified_name,
                        compliance_notified_description = entity.compliance_notified_description,
                        compliance_notified_status = entity.compliance_notified_status,
                        createdby = entity.createdby,
                        compliance_notified_date = dt1,
                        source = "Yes"
                    });
                }
            }

            if (newEntities.Any())
                _dbContext.ComplianceNotifiedStatusModels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToCompliance()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminCompliance = await _commondbContext.SupadminComplianceTypes
                .Where(e => e.compliance_type_status == "Active")
                .ToListAsync();

          
            var existingComplianceIds = await _dbContext.ComplianceModels
                .Select(e => e.compliance_type_id)
                .ToListAsync();

            var newEntities = new List<ComplianceModel>();

            foreach (var entity in superAdminCompliance)
            {
                if (existingComplianceIds.Contains(entity.compliance_type_id))
                {
                   
                    var existing = await _dbContext.ComplianceModels
                        .FirstOrDefaultAsync(e => e.compliance_type_id == entity.compliance_type_id);

                    if (existing != null)
                    {
                        existing.compliance_type_name = entity.compliance_type_name;
                        existing.compliance_type_description = entity.compliance_type_description;
                        existing.compliance_type_status = entity.compliance_type_status;
                        existing.createdby = entity.createdby;
                        existing.compliance_type_create_date = dt1;
                        existing.source = "Yes";
                    }
                }
                else
                {
                   
                    newEntities.Add(new ComplianceModel
                    {
                        compliance_type_id = entity.compliance_type_id,
                        compliance_type_name = entity.compliance_type_name,
                        compliance_type_description = entity.compliance_type_description,
                        compliance_type_status = entity.compliance_type_status,
                        createdby = entity.createdby,
                        compliance_type_create_date = dt1,
                        source = "Yes",
                    });
                }
            }

            // Add the new entities to the database if any
            if (newEntities.Any())
            {
                _dbContext.ComplianceModels.AddRange(newEntities);
            }

            // Save the changes to the database
            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToComplianceGroup()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");


            var superAdminComplianceGroup = await _commondbContext.SupAdmin_ComplianceGroupModels
                .Where(e => e.compliance_group_status == "Active")
                .ToListAsync();

            var existingComplianceGroupNames = await _dbContext.ComplianceGroupModels
                .ToDictionaryAsync(e => e.compliance_group_id);

            var newEntities = new List<ComplianceGroupModel>();

            foreach (var entity in superAdminComplianceGroup)
            {
                if (existingComplianceGroupNames.TryGetValue(entity.compliance_group_id, out var existing))
                {
                    existing.compliance_group_name = entity.compliance_group_name;
                    existing.compliance_group_description = entity.compliance_group_description;
                    existing.compliance_group_status = entity.compliance_group_status;
                    existing.createdby = entity.createdby;
                    existing.compliance_group_Create_date = dt1;
                    existing.source = "Yes";
                }
                else
                {
                    newEntities.Add(new ComplianceGroupModel
                    {
                        compliance_group_id = entity.compliance_group_id,
                        compliance_group_name = entity.compliance_group_name,
                        compliance_group_description = entity.compliance_group_description,
                        compliance_group_status = entity.compliance_group_status,
                        createdby = entity.createdby,
                        compliance_group_Create_date = dt1,
                        source = "Yes",

                    });
                }
            }

            if (newEntities.Any())
                _dbContext.ComplianceGroupModels.AddRange(newEntities);

            await _dbContext.SaveChangesAsync();
        }


        // updated
        private async Task TransferDataFromSupAdminToJurisdictionLocation()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            // Fetch active jurisdiction location data from SupAdmin
            var superAdminJurisdictionLocation = await _commondbContext.SupAdmin_JurisdictionLocationModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Fetch existing jurisdiction locations with relevant fields for matching
            var existingJurisdictionLocationNames = await _dbContext.JurisdictionLocationModels.
                ToDictionaryAsync(e => e.jurisdiction_location_id);

            // List to collect new records to be inserted
            var entitiesToAdd = new List<JurisdictionLocationModel>();

            foreach (var entity in superAdminJurisdictionLocation)
            {


                    if (existingJurisdictionLocationNames.TryGetValue(entity.jurisdiction_location_id, out var existing))
                {
                    existing. jurisdiction_district = entity.jurisdiction_district;
                    existing.jurisdiction_country_id = entity.jurisdiction_country_id;
                    existing.jurisdiction_state_id = entity.jurisdiction_state_id;
                    existing.status = entity.status;
                    existing.createdby = entity.createdby;
                    existing.jurisdiction_location_create_date = dt1;
                    existing.IsImportedData = "Yes"; // Update any other necessary fields
                    }
                
                else
                {
                    // If no existing record, add a new one
                    entitiesToAdd.Add(new JurisdictionLocationModel
                    {
                        jurisdiction_location_id = entity.jurisdiction_location_id,
                        jurisdiction_district = entity.jurisdiction_district,
                        jurisdiction_country_id = entity.jurisdiction_country_id,
                        jurisdiction_state_id = entity.jurisdiction_state_id,
                        status = entity.status,
                        createdby = entity.createdby,
                        jurisdiction_location_create_date = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Add new records to the database
            if (entitiesToAdd.Any())
            {
                _dbContext.JurisdictionLocationModels.AddRange(entitiesToAdd);
            }

            // Save changes to the database (both updates and inserts)
            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToCompliancRiskCriteria()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminRiskCriteria = await _commondbContext.SupAdmin_ComplianceRiskCriteriaModels
                .Where(e => e.compliance_risk_criteria_status == "Active")
                .ToListAsync();

            var existingRiskCriteria = await _dbContext.ComplianceRiskClassificationCriteriaModels
                .Select(e => e.compliance_risk_criteria_id)
                .ToListAsync();

            foreach (var entity in superAdminRiskCriteria)
            {
                // Check if the record already exists based on compliance_risk_criteria_id
                if (existingRiskCriteria.Contains(entity.compliance_risk_criteria_id))
                {
                    // Update the existing record
                    var existingEntity = await _dbContext.ComplianceRiskClassificationCriteriaModels
                        .FirstOrDefaultAsync(x => x.compliance_risk_criteria_id == entity.compliance_risk_criteria_id);

                    if (existingEntity != null)
                    {
                        existingEntity.compliance_risk_criteria_name = entity.compliance_risk_criteria_name;
                        existingEntity.compliance_risk_criteria_description = entity.compliance_risk_criteria_description;
                        existingEntity.compliance_risk_criteria_status = entity.compliance_risk_criteria_status;
                        existingEntity.createdby = entity.createdby;
                        existingEntity.compliance_risk_criteria_date = dt1;
                        existingEntity.IsImportedData = "Yes";
                    }
                }
                else
                {
                    // Insert new record if not found
                    _dbContext.ComplianceRiskClassificationCriteriaModels.Add(new ComplianceRiskClassificationCriteriaModel
                    {
                        compliance_risk_criteria_id = entity.compliance_risk_criteria_id,
                        compliance_risk_criteria_name = entity.compliance_risk_criteria_name,
                        compliance_risk_criteria_description = entity.compliance_risk_criteria_description,
                        compliance_risk_criteria_status = entity.compliance_risk_criteria_status,
                        createdby = entity.createdby,
                        compliance_risk_criteria_date = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToCompliancPeriod()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminCompliancePeriod = await _commondbContext.SupAdmin_CompliancePeriodModels
                .Where(e => e.compliance_period_status == "Active")
                .ToListAsync();

            var existingCompliancePeriods = await _dbContext.CompliancePeriodModels
                .Select(e => e.compliance_period_id)
                .ToListAsync();

            foreach (var entity in superAdminCompliancePeriod)
            {
                // Check if the record already exists based on compliance_period_id
                if (existingCompliancePeriods.Contains(entity.compliance_period_id))
                {
                    // Update the existing record
                    var existingEntity = await _dbContext.CompliancePeriodModels
                        .FirstOrDefaultAsync(x => x.compliance_period_id == entity.compliance_period_id);

                    if (existingEntity != null)
                    {
                        existingEntity.compliance_period_start = entity.compliance_period_start;
                        existingEntity.compliance_period_end = entity.compliance_period_end;
                        existingEntity.start_compliance_year_format = entity.start_compliance_year_format;
                        existingEntity.end_compliance_year_format = entity.end_compliance_year_format;
                        existingEntity.compliance_period_description = entity.compliance_period_description;
                        existingEntity.updated_date = entity.updated_date;
                        existingEntity.compliance_period_status = entity.compliance_period_status;
                        existingEntity.createdby = entity.createdby;
                        existingEntity.created_date = dt1;
                        existingEntity.check_box = entity.check_box;
                        existingEntity.IsImportedData = "Yes";
                    }
                }
                else
                {
                    // Insert new record if not found
                    _dbContext.CompliancePeriodModels.Add(new CompliancePeriodModel
                    {
                        compliance_period_id = entity.compliance_period_id,
                        compliance_period_start = entity.compliance_period_start,
                        compliance_period_end = entity.compliance_period_end,
                        start_compliance_year_format = entity.start_compliance_year_format,
                        end_compliance_year_format = entity.end_compliance_year_format,
                        compliance_period_description = entity.compliance_period_description,
                        updated_date = entity.updated_date,
                        compliance_period_status = entity.compliance_period_status,
                        createdby = entity.createdby,
                        created_date = dt1,
                        check_box = entity.check_box,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }


        private async Task TransferDataFromSupAdminToActRegulatory()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminActRegulatory = await _commondbContext.SupAdmin_ActRegulatoryModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            var existingActRegulatory = await _dbContext.Actregulatorymodels
                .Select(e => e.actregulatoryid)
                .ToListAsync();

            foreach (var entity in superAdminActRegulatory)
            {
                // Check if the record already exists based on actregulatoryid
                var existingEntity = await _dbContext.Actregulatorymodels
                    .FirstOrDefaultAsync(x => x.actregulatoryid == entity.actregulatoryid);

                if (existingEntity != null)
                {
                    // If the record exists, update all fields
                    existingEntity.actregulatoryname = entity.actregulatoryname;
                    existingEntity.actrequlatorydescription = entity.actrequlatorydescription;
                    existingEntity.global_actId = entity.global_actId;
                    existingEntity.createdBy = entity.createdBy;
                    existingEntity.status = entity.status;
                    existingEntity.createddate = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.Actregulatorymodels.Add(new Actregulatorymodel
                    {
                        actregulatoryid = entity.actregulatoryid,
                        actregulatoryname = entity.actregulatoryname,
                        actrequlatorydescription = entity.actrequlatorydescription,
                        global_actId = entity.global_actId,
                        createdBy = entity.createdBy,
                        status = entity.status,
                        createddate = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }

        private async Task TransferDataFromSupAdminToActRegulatoryfile()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminActRegulatoryFile = await _commondbContext.SupAdmin_Actregulatoryfilemodels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Get existing records based on id (e.g., bare_act_id or actregulatoryfile_id)
            var existingActRegulatoryFileIds = await _dbContext.Actregulatoryfilemodels
                .Select(e => e.bare_act_id) // Or you can use the appropriate ID field
                .ToListAsync();

            foreach (var entity in superAdminActRegulatoryFile)
            {
                // Check if the record already exists based on the id (bare_act_id or actregulatoryfile_id)
                var existingEntity = await _dbContext.Actregulatoryfilemodels
                    .FirstOrDefaultAsync(x => x.bare_act_id == entity.bare_act_id); // Check by the ID (bare_act_id)

                if (existingEntity != null)
                {
                    // If the record exists, update all fields (excluding the ID)
                    existingEntity.file_name = entity.file_name;
                    existingEntity.actregulatoryid = entity.actregulatoryid;
                    existingEntity.global_act_id = entity.global_act_id;
                    existingEntity.filepath = entity.filepath;
                    existingEntity.filecategory = entity.filecategory;
                    existingEntity.status = entity.status;
                    existingEntity.created_date = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.Actregulatoryfilemodels.Add(new Actregulatoryfilemodel
                    {
                        bare_act_id = entity.bare_act_id,
                        file_name = entity.file_name,
                        actregulatoryid = entity.actregulatoryid,
                        global_act_id = entity.global_act_id,
                        filepath = entity.filepath,
                        filecategory = entity.filecategory,
                        status = entity.status,
                        created_date = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }
       // update
        private async Task TransferDataFromSupAdminToRulesandRegulatory()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminRules = await _commondbContext.SupAdmin_RulesandRegulatoryModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Get existing records based on the id (e.g., act_rule_regulatory_id)
            var existingRulesIds = await _dbContext.Rulesandregulatorymodels
                .Select(e => e.act_rule_regulatory_id) // Check based on id
                .ToListAsync();

            foreach (var entity in superAdminRules)
            {
                // Check if the record already exists based on the id (act_rule_regulatory_id)
                var existingEntity = await _dbContext.Rulesandregulatorymodels
                    .FirstOrDefaultAsync(x => x.act_rule_regulatory_id == entity.act_rule_regulatory_id);

                if (existingEntity != null)
                {
                    // If the record exists, update all fields (excluding the ID)
                    existingEntity.act_rule_name = entity.act_rule_name;
                    existingEntity.actregulatoryid = entity.actregulatoryid;
                    existingEntity.act_rule_appl_des = entity.act_rule_appl_des;
                    existingEntity.category_of_law_ID = entity.category_of_law_ID;
                    existingEntity.law_type_id = entity.law_type_id;
                    existingEntity.regulatory_authority_id = entity.regulatory_authority_id;
                    existingEntity.jurisdiction_category_id = entity.jurisdiction_category_id;
                    existingEntity.State_id = entity.State_id ?? 0;
                    existingEntity.jurisdiction_location_id = entity.jurisdiction_location_id ?? 0;
                    existingEntity.type_bussiness = entity.type_bussiness;
                    existingEntity.bussiness_operations = entity.bussiness_operations;
                    existingEntity.no_of_employees = entity.no_of_employees;
                    existingEntity.bussiness_investment = entity.bussiness_investment;
                    existingEntity.bussiness_turnover = entity.bussiness_turnover;
                    existingEntity.working_conditions = entity.working_conditions;
                    existingEntity.bussiness_registration = entity.bussiness_registration;
                    existingEntity.other_factor = entity.other_factor;
                    existingEntity.status = entity.status;
                    existingEntity.global_rule_id = entity.global_rule_id;
                    existingEntity.createdBy = entity.createdBy;
                    existingEntity.created_date = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.Rulesandregulatorymodels.Add(new Rulesandregulatorymodel
                    {
                        act_rule_regulatory_id = entity.act_rule_regulatory_id,
                        act_rule_name = entity.act_rule_name,
                        actregulatoryid = entity.actregulatoryid,
                        act_rule_appl_des = entity.act_rule_appl_des,
                        category_of_law_ID = entity.category_of_law_ID,
                        law_type_id = entity.law_type_id,
                        regulatory_authority_id = entity.regulatory_authority_id,
                        jurisdiction_category_id = entity.jurisdiction_category_id,
                        id = entity.id,
                        State_id = entity.State_id ?? 0,
                        jurisdiction_location_id = entity.jurisdiction_location_id ?? 0,
                        type_bussiness = entity.type_bussiness,
                        bussiness_operations = entity.bussiness_operations,
                        no_of_employees = entity.no_of_employees,
                        bussiness_investment = entity.bussiness_investment,
                        bussiness_turnover = entity.bussiness_turnover,
                        working_conditions = entity.working_conditions,
                        bussiness_registration = entity.bussiness_registration,
                        other_factor = entity.other_factor,
                        status = entity.status,
                        global_rule_id = entity.global_rule_id,
                        createdBy = entity.createdBy,
                        created_date = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }
        

        //update
        private async Task TransferDataFromSupAdminToRulesandRegulatoryfiles()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminRulesfiles = await _commondbContext.SupAdmin_ActRuleregulatoryfilemodels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Get existing records based on the id (e.g., act_rule_regulatory_file_id)
            var existingRulesfilesIds = await _dbContext.ActRuleregulatoryfilemodels
                .Select(e => e.act_rule_regulatory_file_id) // Check based on id
                .ToListAsync();

            foreach (var entity in superAdminRulesfiles)
            {
                // Check if the record already exists based on the id (act_rule_regulatory_file_id)
                var existingEntity = await _dbContext.ActRuleregulatoryfilemodels
                    .FirstOrDefaultAsync(x => x.act_rule_regulatory_file_id == entity.act_rule_regulatory_file_id);

                if (existingEntity != null)
                {
                    // If the record exists, update all fields (excluding the ID)
                    existingEntity.act_name = entity.act_name;
                    existingEntity.act_rule_regulatory_id = entity.act_rule_regulatory_id;
                    existingEntity.global_rule_id = entity.global_rule_id;
                    existingEntity.filepath = entity.filepath;
                    existingEntity.filecategory = entity.filecategory;
                    existingEntity.file_name = entity.file_name;
                    existingEntity.status = entity.status;
                    existingEntity.created_date = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.ActRuleregulatoryfilemodels.Add(new ActRuleregulatoryfilemodel
                    {
                        act_rule_regulatory_file_id = entity.act_rule_regulatory_file_id,
                        act_name = entity.act_name,
                        act_rule_regulatory_id = entity.act_rule_regulatory_id,
                        global_rule_id = entity.global_rule_id,
                        filepath = entity.filepath,
                        filecategory = entity.filecategory,
                        file_name = entity.file_name,
                        status = entity.status,
                        created_date = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }



        //updated
        private async Task TransferDataFromSupAdminToStatutoryforms()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminStatutory = await _commondbContext.SupAdmin_StatutoryformsModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Get existing records based on statutoryformsid
            var existingStatutoryIds = await _dbContext.statutoryformsrecordsmodels
                .Select(e => e.statutoryformsid) // Checking by statutoryformsid
                .ToListAsync();

            foreach (var entity in superAdminStatutory)
            {
                // Check if the record already exists based on statutoryformsid
                var existingEntity = await _dbContext.statutoryformsrecordsmodels
                    .FirstOrDefaultAsync(x => x.statutoryformsid == entity.statutoryformsid);

                if (existingEntity != null)
                {
                    // If the record exists, update all fields (excluding the ID)
                    existingEntity.recordformsname = entity.recordformsname;
                    existingEntity.actregulatoryid = entity.actregulatoryid;
                    existingEntity.act_rule_regulatory_id = entity.act_rule_regulatory_id;
                    existingEntity.recordformsdesc = entity.recordformsdesc;
                    existingEntity.applicationrefernce = entity.applicationrefernce;
                    existingEntity.createdby = entity.createdby;
                    existingEntity.status = entity.status;
                    existingEntity.createddate = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.statutoryformsrecordsmodels.Add(new statutoryformsrecordsmodel
                    {
                        statutoryformsid = entity.statutoryformsid,
                        recordformsname = entity.recordformsname,
                        actregulatoryid = entity.actregulatoryid,
                        act_rule_regulatory_id = entity.act_rule_regulatory_id,
                        recordformsdesc = entity.recordformsdesc,
                        applicationrefernce = entity.applicationrefernce,
                        createdby = entity.createdby,
                        status = entity.status,
                        createddate = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }


        //updated
        private async Task TransferDataFromSupAdminToStatutoryfies()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminStatutoryfies = await _commondbContext.SupAdmin_statutoryformsrecordsfilemodels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Get existing records based on statutory_forms_filemaster_id
            var existingStatutoryfiesIds = await _dbContext.statutoryformsrecordsfilemodels
                .Select(e => e.statutory_forms_filemaster_id) // Checking by statutory_forms_filemaster_id
                .ToListAsync();

            foreach (var entity in superAdminStatutoryfies)
            {
                // Check if the record already exists based on statutory_forms_filemaster_id
                var existingEntity = await _dbContext.statutoryformsrecordsfilemodels
                    .FirstOrDefaultAsync(x => x.statutory_forms_filemaster_id == entity.statutory_forms_filemaster_id);

                if (existingEntity != null)
                {
                    // If the record exists, update all fields (excluding the ID)
                    existingEntity.file_name = entity.file_name;
                    existingEntity.statutoryformsid = entity.statutoryformsid;
                    existingEntity.filepath = entity.filepath;
                    existingEntity.filecategory = entity.filecategory;
                    existingEntity.recordformsname = entity.recordformsname;
                    existingEntity.status = entity.status;
                    existingEntity.created_date = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.statutoryformsrecordsfilemodels.Add(new statutoryformsrecordsfilemodel
                    {
                        statutory_forms_filemaster_id = entity.statutory_forms_filemaster_id,
                        file_name = entity.file_name,
                        statutoryformsid = entity.statutoryformsid,
                        filepath = entity.filepath,
                        filecategory = entity.filecategory,
                        recordformsname = entity.recordformsname,
                        status = entity.status,
                        created_date = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }

        // updated
        private async Task TransferDataFromSupAdminToComplinacePenatly()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminpenatly = await _commondbContext.SupAdmin_compliancepenatlymasterModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Get existing records based on compliancepenaltyid
            var existingpenatlyIds = await _dbContext.CompliancepenatlymasterModels
                .Select(e => e.compliancepenaltyid) // Checking by compliancepenaltyid
                .ToListAsync();

            foreach (var entity in superAdminpenatly)
            {
                // Check if the record already exists based on compliancepenaltyid
                var existingEntity = await _dbContext.CompliancepenatlymasterModels
                    .FirstOrDefaultAsync(x => x.compliancepenaltyid == entity.compliancepenaltyid);

                if (existingEntity != null)
                {
                    // If the record exists, update all fields (excluding the ID)
                    existingEntity.penaltydesc = entity.penaltydesc;
                    existingEntity.ruleid = entity.ruleid;
                    existingEntity.actid = entity.actid;
                    existingEntity.applicationselectionrule = entity.applicationselectionrule;
                    existingEntity.maxpenalty = entity.maxpenalty;
                    existingEntity.minpenalty = entity.minpenalty;
                    existingEntity.additionalrefernce = entity.additionalrefernce;
                    existingEntity.createdBy = entity.createdBy;
                    existingEntity.penalty = entity.penalty;
                    existingEntity.status = entity.status;
                    existingEntity.createddate = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.CompliancepenatlymasterModels.Add(new compliancepenatlymasterModel
                    {
                        compliancepenaltyid = entity.compliancepenaltyid,
                        penaltydesc = entity.penaltydesc,
                        ruleid = entity.ruleid,
                        actid = entity.actid,
                        applicationselectionrule = entity.applicationselectionrule,
                        maxpenalty = entity.maxpenalty,
                        minpenalty = entity.minpenalty,
                        additionalrefernce = entity.additionalrefernce,
                        createdBy = entity.createdBy,
                        penalty = entity.penalty,
                        status = entity.status,
                        createddate = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }

        //updated
        private async Task TransferDataFromSupAdminToComplinacePenatlyfiles()
        {
            DateTime dt = DateTime.Now;
            string dt1 = dt.ToString("yyyy-MM-dd HH:mm:ss");

            var superAdminpenatlyfiles = await _commondbContext.SupAdmin_PenaltyCategoryfileModels
                .Where(e => e.status == "Active")
                .ToListAsync();

            // Get existing records based on compliance_filepenalty_id
            var existingpenatlyfileIds = await _dbContext.PenaltyCategoryfileModels
                .Select(e => e.compliance_filepenalty_id) // Checking by compliance_filepenalty_id
                .ToListAsync();

            foreach (var entity in superAdminpenatlyfiles)
            {
                // Check if the record already exists based on compliance_filepenalty_id
                var existingEntity = await _dbContext.PenaltyCategoryfileModels
                    .FirstOrDefaultAsync(x => x.compliance_filepenalty_id == entity.compliance_filepenalty_id);

                if (existingEntity != null)
                {
                    // If the record exists, update all fields (excluding the ID)
                    existingEntity.penalty_category_name = entity.penalty_category_name;
                    existingEntity.compliancepenaltyid = entity.compliancepenaltyid;
                    existingEntity.filepath = entity.filepath;
                    existingEntity.filecategory = entity.filecategory;
                    existingEntity.file_name = entity.file_name;
                    existingEntity.status = entity.status;
                    existingEntity.created_date = dt1;
                    existingEntity.IsImportedData = "Yes";
                }
                else
                {
                    // If the record doesn't exist, insert a new record
                    _dbContext.PenaltyCategoryfileModels.Add(new PenaltyCategoryfileModel
                    {
                        compliance_filepenalty_id = entity.compliance_filepenalty_id,
                        penalty_category_name = entity.penalty_category_name,
                        compliancepenaltyid = entity.compliancepenaltyid,
                        filepath = entity.filepath,
                        filecategory = entity.filecategory,
                        file_name = entity.file_name,
                        status = entity.status,
                        created_date = dt1,
                        IsImportedData = "Yes",
                    });
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();
        }

    }
}
