using AutoMapper;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.Web.Models;
using System;
using System.Collections.Generic;

namespace ELTE.TravelAgency.Web
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<City, CityViewModel>();
            CreateMap<CityViewModel, City>();

            CreateMap<Building, BuildingViewModel>()
                .ForMember(dest => dest.Features, opt => opt.MapFrom<FeatureResolver>());
            CreateMap<BuildingViewModel, Building>()
                .ForMember(dest => dest.Features, opt => opt.MapFrom<FeatureViewModelResolver>());

            CreateMap<BuildingViewModel, BuildingDetailsViewModel>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => new List<int>()));

            CreateMap<Apartment, ApartmentViewModel>();
            CreateMap<ApartmentViewModel, Apartment>();

            CreateMap<Rent, RentViewModel>();
            CreateMap<RentViewModel, Rent>();
        }

        public class FeatureResolver : IValueResolver<Building, BuildingViewModel, ICollection<FeatureViewModel>>
        {
            public ICollection<FeatureViewModel> Resolve(Building source, BuildingViewModel destination, ICollection<FeatureViewModel> destMember,
                ResolutionContext context)
            {
                List<FeatureViewModel> result = new List<FeatureViewModel>();

                int featureId = 0;
                foreach (Feature feature in Enum.GetValues(typeof(Feature)))
                {
                    if (feature > 0)
                    {
                        result.Add(new FeatureViewModel
                        {
                            Id = featureId++,
                            IsAvailable = source.Features.HasFlag(feature)
                        });
                    }
                }

                return result;
            }
        }

        public class FeatureViewModelResolver : IValueResolver<BuildingViewModel, Building, Feature>
        {
            public Feature Resolve(BuildingViewModel source, Building destination, Feature destMember, ResolutionContext context)
            {
                if (source.Features.Count == 0)
                    return Feature.None;

                Feature result = Feature.None;
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
}
