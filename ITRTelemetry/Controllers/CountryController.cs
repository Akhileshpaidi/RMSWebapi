using DomainModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySQLProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITRTelemetry.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    [Produces("application/json")]
    public class CountryController : ControllerBase
    {
        private CommonDBContext commonDBContext;
        private readonly MySqlDBContext mySqlDBContext;
        public CountryController(MySqlDBContext mySqlDBContext, CommonDBContext commonDBContext)
        {
            this.mySqlDBContext = mySqlDBContext;
            this.commonDBContext = commonDBContext;
        }

        [Route("api/Country/GetCountries")]
        [HttpGet]

        public IEnumerable<object> GetCountries()
        {
            // return this.mySqlDBContext.CountryModels.ToList();
            var details1 = (from countries in mySqlDBContext.CountryModels
                            orderby countries.name == "India" ? 0 : 1, countries.name
                            select new
                            {
                                jurisdiction_country_id = countries.id, // Use a different alias here
                                countries.name,
                            })
                  .ToList();

            return details1;

        }

        [Route("api/States/GetStates")]
        [HttpGet]
        public IEnumerable<object> GetStates()
        {

            var deatils1 = (from states in mySqlDBContext.StateModels
                            join countries in mySqlDBContext.CountryModels on states.country_id equals countries.id
                            orderby states.name

                            select new
                            {
                                jurisdiction_country_id=countries.id,
                                jurisdiction_state_id = states.id, // Use a different alias here
                                states.name,
                            })

                          .ToList();

            return deatils1;

        }

        [Route("api/District/GetDistrictDetails")]
        [HttpGet]
        public IEnumerable<object> GetDistrictDetails()
        {

            var deatils1 = (from jurisdiction_location_list in mySqlDBContext.JurisdictionLocationModels
                            join states in mySqlDBContext.StateModels on jurisdiction_location_list.jurisdiction_location_id equals states.id

                            select new
                            {
                                jurisdiction_location_id = jurisdiction_location_list.jurisdiction_location_id,
                                jurisdiction_state_id = states.id, // Use a different alias here
                                jurisdiction_location_list.jurisdiction_district,
                            })

                          .ToList();

            return deatils1;

        }






        [Route("api/StateModels/GetStateDetails/{CountryId}")]
        [HttpGet]

        public IEnumerable<StateModel> GetStateDetails(int CountryId)
        {

            return this.mySqlDBContext.StateModels.Where(x => x.country_id == CountryId).OrderBy(x => x.name).ToList();

        }




        [Route("api/JurisdictionLocationModel/GetDistrictDetails/{stateId}")]
        [HttpGet]

        public IEnumerable<JurisdictionLocationModel> GetDistrictDetails(int stateId)
        {

            return this.mySqlDBContext.JurisdictionLocationModels.Where(x => x.status == "Active" && x.jurisdiction_state_id == stateId).OrderBy(x => x.jurisdiction_district).ToList();

        }

        [Route("api/JurisdictionLocationModel/superDistrictDetails/{stateId}")]
        [HttpGet]

        public IEnumerable<SupAdmin_JurisdictionLocationModel> superDistrictDetails(int stateId)
        {

            return this.commonDBContext.SupAdmin_JurisdictionLocationModels.Where(x => x.status == "Active" && x.jurisdiction_state_id == stateId).OrderBy(x => x.jurisdiction_district).ToList();

        }


    }
}
