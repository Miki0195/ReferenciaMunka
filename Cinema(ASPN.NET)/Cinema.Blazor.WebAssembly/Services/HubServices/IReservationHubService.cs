using ELTE.Cinema.Blazor.WebAssembly.ViewModels;

namespace ELTE.Cinema.Blazor.WebAssembly.Services.HubServices
{
    public interface IReservationHubService: IBaseHubService
    {
        public event Action<SeatViewModel>? OnSeatStatusChanged;

        public Task StartHubConnectionAsync(int screeningId);
        public Task NotifySelectedSeat(int screeningId, SeatViewModel seat);
        public Task DisconnectHubAsync(int screeningId);
    }
}
