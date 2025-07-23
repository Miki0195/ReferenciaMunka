using AutoMapper;
using Blazored.LocalStorage;
using ELTE.Cinema.Blazor.WebAssembly.Config;
using ELTE.Cinema.Blazor.WebAssembly.Infrastructure;
using ELTE.Cinema.Blazor.WebAssembly.ViewModels;
using ELTE.Cinema.Shared.SignalR.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Text.Json;

namespace ELTE.Cinema.Blazor.WebAssembly.Services.HubServices
{
    public class ReservationHubService : BaseHubService, IReservationHubService
    {
        public event Action<SeatViewModel>? OnSeatStatusChanged;

        private readonly IMapper _mapper;
        private readonly IHttpRequestUtility _httpRequestUtility;
        private SeatViewModel? _lastNotifiedSeat;

        public ReservationHubService(AppConfig appConfig, JsonSerializerOptions jsonOptions, IMapper mapper,
            ILocalStorageService localStorageService, IHttpRequestUtility httpRequestUtility) :
            base(appConfig, jsonOptions, localStorageService, httpRequestUtility)
        {
            _mapper = mapper;
            _httpRequestUtility = httpRequestUtility;
        }

        public async Task StartHubConnectionAsync(int screeningId)
        {
            InitHub("ScreeningsHub");

            _hubConnection!.On<int, SeatNotificationDto>("SeatStatusChanged", (seatChangedScreeningId, dto) =>
            {
                var seatViewModel = _mapper.Map<SeatViewModel>(dto);
                OnSeatStatusChanged?.Invoke(seatViewModel);
            });

            await ConnectHubAsync();

            await _hubConnection!.InvokeAsync("JoinScreeningGroup", screeningId);
        }

        public async Task NotifySelectedSeat(int screeningId, SeatViewModel seat)
        {
            if (_lastNotifiedSeat != null)
            {
                var lastSeatNotificationDto = _mapper.Map<SeatNotificationDto>(_lastNotifiedSeat);
                lastSeatNotificationDto.Status = SeatClientStatusDto.None;
                await _hubConnection!.InvokeAsync("SeatStatusChanged", screeningId, lastSeatNotificationDto);
                _lastNotifiedSeat = null;
            }

            //if it's resevred or sold not necessary to notify the HUB
            if (seat.Status != SeatStatusViewModel.Free)
                return;

            var seatNotificationDto = _mapper.Map<SeatNotificationDto>(seat);
            seatNotificationDto.Status = SeatClientStatusDto.Selected;
            await _hubConnection!.InvokeAsync("SeatStatusChanged", screeningId, seatNotificationDto);
            _lastNotifiedSeat = seat;
        }

        public async Task DisconnectHubAsync(int screeningId)
        {
            await _hubConnection!.InvokeAsync("LeaveScreeningGroup", screeningId);
            await DisconnectHubAsync();
        }

    }
}
