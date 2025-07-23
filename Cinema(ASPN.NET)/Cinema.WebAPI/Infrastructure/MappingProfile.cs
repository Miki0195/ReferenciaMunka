using AutoMapper;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.Shared.SignalR.Models;

namespace ELTE.Cinema.WebAPI.Infrastructure;

/// <summary>
/// Automapper mappingProfile
/// </summary>
public class MappingProfile: Profile
{
    /// <summary>
    /// Constructor - Define mapping rules here
    /// </summary>
    public MappingProfile()
    {
        CreateMap<MovieRequestDto, Movie>(MemberList.Source);
        CreateMap<Movie, MovieResponseDto>(MemberList.Destination);

        CreateMap<RoomRequestDto, Room>(MemberList.Source);
        CreateMap<Room, RoomResponseDto>(MemberList.Destination);

        CreateMap<ScreeningRequestDto, Screening>(MemberList.Source);
        CreateMap<Screening, ScreeningResponseDto>(MemberList.Destination);

        CreateMap<SeatRequestDto, Seat>(MemberList.Source)
            .ForSourceMember(s => s.Column, opt => opt.DoNotValidate())
            .ForSourceMember(s => s.Row, opt => opt.DoNotValidate())
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => new SeatPosition(src.Row, src.Column)));

        CreateMap<Seat, SeatResponseDto>(MemberList.Destination)
            .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.Position.Row))
            .ForMember(dest => dest.Column, opt => opt.MapFrom(src => src.Position.Column))
            .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.ReservationId));

        CreateMap<ReservationRequestDto, Reservation>(MemberList.Source)
             .ForSourceMember(src => src.ScreeningId, opt => opt.DoNotValidate())
             .ForSourceMember(dest => dest.Seats, opt => opt.DoNotValidate())
             .ForMember(dest => dest.Seats, opt => opt.MapFrom<SeatResolver>());
        CreateMap<Reservation, ReservationResponseDto>(MemberList.Destination)
            .ForMember(dest => dest.Screening, opt => opt.MapFrom(src => src.Seats.First().Screening));
        CreateMap<SeatStatus, SeatStatusDto>(MemberList.Source);
        
        CreateMap<UserRequestDto, User>(MemberList.Source)
            .ForSourceMember(src => src.Password, opt => opt.DoNotValidate())
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

        CreateMap<User, UserResponseDto>(MemberList.Destination);
        
        //SignalR
        CreateMap<Movie, MovieNotificationDto>(MemberList.Destination);

        CreateMap<Seat, SeatNotificationDto>(MemberList.Destination)
            .ForMember(dest => dest.Row, opt => opt.MapFrom(src => src.Position.Row))
            .ForMember(dest => dest.Column, opt => opt.MapFrom(src => src.Position.Column))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<SeatClientStatusDto>(src.Status.ToString())))
            .ForMember(dest => dest.Reservation, opt => opt.MapFrom(src => src.Reservation));
        
        CreateMap<Reservation, ReservationNotificationDto>(MemberList.Destination);
        CreateMap<Reservation, List<SeatNotificationDto>>(MemberList.Destination)
            .ConvertUsing((src, dest, ctx) => src.Seats.Select(s => ctx.Mapper.Map<SeatNotificationDto>(s)).ToList());
    }
}

/// <summary>
/// SeatResolver
/// </summary>
public class SeatResolver : IValueResolver<ReservationRequestDto, Reservation, ICollection<Seat>>
{
    /// <summary>
    /// Resolve
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="members"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public ICollection<Seat> Resolve(ReservationRequestDto source, Reservation destination, ICollection<Seat> members, ResolutionContext context)
    {
        return source.Seats.Select(s => new Seat
        {
            ScreeningId = (int)source.ScreeningId,
            Position = new SeatPosition(s.Row, s.Column),
            Status = SeatStatus.Reserved
        }).ToList();
    }
}
