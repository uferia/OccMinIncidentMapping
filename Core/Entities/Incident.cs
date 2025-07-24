using Core.Enums;

namespace Core.Entities
{
    public class Incident : BaseEntity
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public HazardType Hazard { get; set; }
        public StatusType Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
