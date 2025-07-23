using AutoMapper;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.Web.Models;

namespace ELTE.Cinema.Web.Infrastructure
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<Movie, MovieViewModel>(MemberList.Destination);
            CreateMap<Screening, ScreeningViewModel>(MemberList.Destination);
            CreateMap<Room, RoomViewModel>(MemberList.Destination);

            CreateMap<ReservationViewModel, Reservation>(MemberList.Source)
                .ForSourceMember(src => src.ScreeningId, opt => opt.DoNotValidate())
                .ForSourceMember(src => src.Seats, opt => opt.DoNotValidate())
                .ForMember(dest => dest.Seats, opt => opt.MapFrom<SeatReasolver>());
        }

        public class SeatReasolver : IValueResolver<ReservationViewModel, Reservation, ICollection<Seat>>
        {
            public ICollection<Seat> Resolve(ReservationViewModel source, Reservation destination, ICollection<Seat> members, ResolutionContext context)
            {
                return source.Seats.Select(s => new Seat
                {
                    ScreeningId = (int)source.ScreeningId,
                    Position = new SeatPosition(s.Row, s.Column),
                    Status = SeatStatus.Reserved
                }).ToList();
            }
        }
    }
}
