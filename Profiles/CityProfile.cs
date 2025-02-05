using AutoMapper;

namespace CityInfo.API.Profiles
{
    public class CityProfile : Profile
    {
        public CityProfile() {
            CreateMap<Entities.City, Models.CityDtoWithoutPOI>();
            CreateMap<Entities.City, Models.CityDto>();


        }
    }
}
