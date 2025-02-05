using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Models
{
    public class pointOfInterestForUpdateDto
    {
        [Required(ErrorMessage = "You should provide a name value")]
        [MaxLength(50)]
        public string Name { get; set; } = String.Empty;
        [MaxLength(50)]
        public string? Description { get; set; }
    }
}
