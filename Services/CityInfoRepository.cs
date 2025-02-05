using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<(IEnumerable<City>,PaginationMetadata)> GetCitiesAsync(string? name, string? searchQ, int pageNumber, int pageSize)
        {
            var collection = _context.Cities as IQueryable<City>;
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection.Where(c => c.Name == name);
            }

            if (!string.IsNullOrWhiteSpace(searchQ))
            {
                searchQ = searchQ.Trim();
                collection = collection.Where(a => a.Name.Contains(searchQ)
                    || (a.Description != null && a.Description.Contains(searchQ)));
            }
            int count = await collection.CountAsync();
            PaginationMetadata pageData = new PaginationMetadata(count, pageSize, pageNumber);
            var collectionToReturn = await collection.OrderBy(c => c.Name)
               .Skip(pageSize * (pageNumber - 1))
               .Take(pageSize)
               .ToListAsync();

            return (collectionToReturn, pageData);
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
           return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
                return await _context.Cities.Include(c => c.pointOfInterests).Where(c => c.Id == cityId).FirstOrDefaultAsync();
            else
                return await _context.Cities.FirstOrDefaultAsync(c => c.Id == cityId);
        }

        public async Task<City?> GetCityAsync(int cityid)
        {
            return await _context.Cities.Where(c => c.Id == cityid).FirstOrDefaultAsync();
        }

        public Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return _context.PointOfInterest.Where(p => p.Id == pointOfInterestId).Where(p=> p.CityId == cityId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await _context.PointOfInterest.Where(p => p.CityId == cityId).ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId,
            PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId,false);
            if (city != null)
            {
                city.pointOfInterests.Add(pointOfInterest);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        void ICityInfoRepository.DeletePointOfInterest(PointOfInterest poi)
        {
            _context.PointOfInterest.Remove(poi);
        }
    }
}
