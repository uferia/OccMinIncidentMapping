using AutoMapper;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data.Dtos;

namespace Infrastructure.Data.MappingProfiles
{
    public class FirestoreMappingProfile : Profile
    {
        public FirestoreMappingProfile()
        {
            // Incident -> Firestore DTO (simple conversion)
            CreateMap<Incident, IncidentFirestoreDto>()
                .ForMember(dest => dest.Hazard, opt => opt.MapFrom(src => src.Hazard.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Firestore DTO -> Incident (with safe parsing)
            CreateMap<IncidentFirestoreDto, Incident>()
                .ForMember(dest => dest.Hazard, opt => opt.MapFrom(src => ParseHazard(src.Hazard)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseStatus(src.Status)));
        }

        private static HazardType ParseHazard(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return HazardType.Flood;

            return Enum.TryParse<HazardType>(value, ignoreCase: true, out var result)
                ? result
                : HazardType.Flood;
        }

        private static StatusType ParseStatus(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return StatusType.Ongoing;

            return Enum.TryParse<StatusType>(value, ignoreCase: true, out var result)
                ? result
                : StatusType.Ongoing;
        }
    }
}
