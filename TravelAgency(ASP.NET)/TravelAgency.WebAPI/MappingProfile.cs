using AutoMapper;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.Shared.Models;

namespace ELTE.TravelAgency.WebAPI;

/// <summary>
/// Leképezési szabályok
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<City, CityDto>()
            .ReverseMap();

        CreateMap<Building, BuildingDto>()
            .ForMember(dest => dest.Features, opt => opt.MapFrom<FeatureResolver>());
        CreateMap<BuildingDto, Building>()
            .ForMember(dest => dest.City, opt => opt.Ignore())
            .ForMember(dest => dest.Features, opt => opt.MapFrom<FeatureDtoResolver>());

        CreateMap<BuildingImage, ImageDto>()
            .ReverseMap();

        CreateMap<Apartment, ApartmentDto>()
            .ReverseMap();

        CreateMap<Rent, RentDto>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom<TotalPriceResolver>());
        CreateMap<RentDto, Rent>();

        CreateMap<UserDto, User>(MemberList.Source)
            .ForSourceMember(dest => dest.Id, opt => opt.DoNotValidate())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForSourceMember(src => src.Password, opt => opt.DoNotValidate())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        CreateMap<User, UserDto>(MemberList.Destination)
            .ForMember(src => src.Password, opt => opt.Ignore());
    }

    /// <summary>
    /// Foglalás teljes árának kiszámítása
    /// </summary>
    public class TotalPriceResolver : IValueResolver<Rent, RentDto, int>
    {
        private readonly IApartmentService _apartmentService;

        public TotalPriceResolver(IApartmentService apartmentService)
        {
            _apartmentService = apartmentService;
        }

        public int Resolve(Rent source, RentDto destination, int destMember, ResolutionContext context)
        {
            return _apartmentService.GetPrice(
                source.StartDate, source.EndDate,
                source.ApartmentId).Result;
        }
    }

    public class FeatureResolver : IValueResolver<Building, BuildingDto, FeatureDto[]>
    {
        public FeatureDto[] Resolve(Building source, BuildingDto destination, FeatureDto[] destMember,
            ResolutionContext context)
        {
            var result = new List<FeatureDto>();

            var featureId = 0;
            foreach (Feature feature in Enum.GetValues(typeof(Feature)))
            {
                if (feature > 0)
                {
                    result.Add(new FeatureDto
                    {
                        Id = featureId++,
                        IsAvailable = source.Features.HasFlag(feature)
                    });
                }
            }

            return result.ToArray();
        }
    }
    public class FeatureDtoResolver : IValueResolver<BuildingDto, Building, Feature>
    {
        public Feature Resolve(BuildingDto source, Building destination, Feature destMember, ResolutionContext context)
        {
            if (!source.Features.Any())
                return Feature.None;

            var result = Feature.None;
            foreach (var feature in source.Features)
            {
                if (feature.IsAvailable)
                {
                    result += 1 << feature.Id;
                }
            }

            return result;
        }
    }
}
