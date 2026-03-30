using System.Text.Json.Serialization;

namespace Catalogs.Domain.Entities
{
    public class ServicePricingOption
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string OptionName { get; set; } = default!;
        public decimal Price { get; set; }
        public string UoM { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public Service Service { get; set; } = default!;
    }
}
