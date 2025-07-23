using AutoMapper;
using ELTE.Cinema.Blazor.WebAssembly.ViewModels;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.Shared.SignalR.Models;

namespace ELTE.Cinema.Blazor.WebAssembly.Infrastructure
{
    public class BlazorMappingProfile : Profile
    {
        public BlazorMappingProfile()
        {
            CreateMap<MovieResponseDto, MovieViewModel>(MemberList.Source);
            CreateMap<MovieViewModel, MovieRequestDto>(MemberList.Destination);

            CreateMap<LoginViewModel, LoginRequestDto>(MemberList.Source);

            CreateMap<RoomResponseDto, RoomViewModel>(MemberList.Source);
            CreateMap<RoomViewModel, RoomRequestDto>(MemberList.Destination);

            CreateMap<ScreeningResponseDto, ScreeningViewModel>(MemberList.Source);
            CreateMap<ScreeningViewModel, ScreeningRequestDto>(MemberList.Destination)
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.Room!.Id))
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.Movie!.Id));

            CreateMap<ReservationResponseDto, ReservationViewModel>(MemberList.Destination);
            CreateMap<SeatResponseDto, SeatViewModel>(MemberList.Destination)
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore());

            CreateMap<SeatViewModel, SeatRequestDto>(MemberList.Destination);

            CreateMap<MovieNotificationDto, MovieViewModel>(MemberList.Source);

            CreateMap<SeatNotificationDto, SeatViewModel>(MemberList.Source)
                .ForMember(dest => dest.ReservationId,
                opt => opt.MapFrom(src => src.Reservation == null ? (int?)null : src.Reservation.Id))
                .ForSourceMember(src => src.Reservation, opt => opt.DoNotValidate());

            CreateMap<SeatViewModel, SeatNotificationDto>(MemberList.Destination)
                .ForMember(dest => dest.Reservation, opt => opt.Ignore());

            CreateMap<SeatClientStatusDto, SeatStatusViewModel>()
            .ConvertUsing<SeatStatusConverter>();

        }

    }

    public class SeatStatusConverter : ITypeConverter<SeatClientStatusDto, SeatStatusViewModel>
    {
        public SeatStatusViewModel Convert(SeatClientStatusDto source, SeatStatusViewModel destination, ResolutionContext context)
        {
            return source switch
            {
                SeatClientStatusDto.None => SeatStatusViewModel.Free,
                SeatClientStatusDto.Selected => SeatStatusViewModel.RemoteSelected,
                SeatClientStatusDto.Reserved => SeatStatusViewModel.Reserved,
                SeatClientStatusDto.Sold => SeatStatusViewModel.Sold,
                _ => throw new ArgumentOutOfRangeException(nameof(source), $"Unknown status value: {source}")
            };
        }
    }
}
