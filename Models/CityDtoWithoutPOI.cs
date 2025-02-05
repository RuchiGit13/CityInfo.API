namespace CityInfo.API.Models
{
    /// <summary>
    /// Model to hold city details without POI
    /// </summary>
    public class CityDtoWithoutPOI
    {
        /// <summary>
        /// Id of the city
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the city
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

    }
}
