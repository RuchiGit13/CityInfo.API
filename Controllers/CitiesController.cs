using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    //[Authorize(Policy = "MustBeFromHyd")]
    [Route("api/v{version:apiVersion}/cities")]
    [ApiVersion(1)]
    [ApiVersion(2)]

    public class CitiesController : ControllerBase
    {
        private ICityInfoRepository _cityInfoRepository;
        private IMapper _mapper;


        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityDtoWithoutPOI>>> GetCities(String? name, string? searchQuery, int pageNumber, int pageSize)
        {
            (IEnumerable<City> cities, PaginationMetadata pageData) = await _cityInfoRepository.GetCitiesAsync(name,searchQuery,pageNumber,pageSize);
            Response.Headers.Add("X-PaginationData",JsonSerializer.Serialize(pageData));
            return Ok(_mapper.Map< IEnumerable<CityDtoWithoutPOI>>(cities));
        }

        /// <summary>
        /// Get city by passing id
        /// </summary>
        /// <param name="id">Id of the city</param>
        /// <param name="includePointsOfInterest">Do you want to include POI</param>
        /// <response code="200">Returns the requested city</response>
        /// <returns>A city with or without points of interest</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCity(int id, Boolean includePointsOfInterest = false)
        {
            //CityDto city = _dataStore.Cities.FirstOrDefault(x => x.Id == id);
            //if (city == null)
            //{
            //    return NotFound();
            //}
            var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }

            return Ok(_mapper.Map<CityDtoWithoutPOI>(city));
        }


    }
}
