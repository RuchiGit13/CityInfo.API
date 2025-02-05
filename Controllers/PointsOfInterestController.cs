using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/cities/{cityid}/pointsofinterest")]
    [ApiController]
    [ApiVersion (2)]
    public class PointsOfInterestController : ControllerBase
    {
        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;
        private ICityInfoRepository _cityInfoRepository;
        private IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger = logger;
            _mailService = mailService;
            _cityInfoRepository = cityInfoRepository;
            _mapper = mapper;

        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointsOfInterestDto>>> GetPointsOfInterest(int cityid)
        {
            try
            {
                var city = await _cityInfoRepository.CityExistsAsync(cityid);
                if (city == false)
                {
                    _logger.LogInformation($"City with id {cityid} is not found");
                    return NotFound();
                }
                IEnumerable<PointOfInterest> POI = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityid);
                IEnumerable<PointsOfInterestDto> POIDto = _mapper.Map<IEnumerable<PointsOfInterestDto>>(POI);
                return Ok(POIDto);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception City with id {cityid} is not found", ex);
                return StatusCode(500, "A problem happened while executing your method");
            }
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointsOfInterestDto>> GetPointOfInterestAsync(int cityid, int pointofinterestid)
        {
            var city = await _cityInfoRepository.CityExistsAsync(cityid);
            if (city == false)
            {
                _logger.LogInformation($"City with id {cityid} is not found");
                return NotFound();
            }
            PointOfInterest poi = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityid, pointofinterestid);
            PointsOfInterestDto POI = _mapper.Map<PointsOfInterestDto>(poi);
            if (POI == null)
            {
                return NotFound();
            }
            else
                return Ok(POI);
        }

        [HttpPost]
        public async Task<ActionResult<PointsOfInterestDto>> CreatePointOfInterest(
               int cityId,
               PointOfInterestForCreationDto pointOfInterest)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(
                cityId, finalPointOfInterest);

            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn =
                _mapper.Map<Models.PointsOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                 new
                 {
                     cityId = cityId,
                     pointOfInterestId = createdPointOfInterestToReturn.Id
                 },
                 createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest (int cityid, int pointofinterestid, 
            pointOfInterestForUpdateDto pointOfInterestForUpdate)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityid))
            {
                return NotFound();
            }
            PointOfInterest poientity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityid, pointofinterestid);
            _mapper.Map(pointOfInterestForUpdate, poientity);
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest (int cityid, int pointofinterestid, JsonPatchDocument<pointOfInterestForUpdateDto> patchDocument)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityid))
            {
                return NotFound();
            }
            PointOfInterest poientity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityid, pointofinterestid);
            pointOfInterestForUpdateDto poiupdate = _mapper.Map<pointOfInterestForUpdateDto>(poientity);
            patchDocument.ApplyTo(poiupdate, ModelState);
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!TryValidateModel(poiupdate))
            {
                return BadRequest(ModelState);
            }
            _mapper.Map(poiupdate, poientity);
            await _cityInfoRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public async Task<ActionResult> DeletePointOfInterest ( int cityid, int pointofinterestid)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityid))
            {
                return NotFound();
            }
            PointOfInterest poi = await _cityInfoRepository.GetPointOfInterestForCityAsync (cityid, pointofinterestid);
            if (poi == null)
            {
                return NotFound();
            }
            _cityInfoRepository.DeletePointOfInterest(poi);
            _mailService.sendEmail("Deleteing POI", "POI has been deleted");
            return NoContent();
        }
    }
}
