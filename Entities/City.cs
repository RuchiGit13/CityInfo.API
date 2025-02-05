using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    public class City
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = String.Empty;
        [MaxLength(200)]
        public string? Description { get; set; }

        public ICollection<PointOfInterest> pointOfInterests { get; set; } = new List<PointOfInterest>();

        public City (String name )
        {
            this.Name = name;
        }
    }
}
