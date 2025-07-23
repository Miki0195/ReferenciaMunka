using AutoMapper;
using Blazored.LocalStorage;
using ELTE.Cinema.Blazor.WebAssembly.Config;
using ELTE.Cinema.Blazor.WebAssembly.Infrastructure;
using ELTE.Cinema.Blazor.WebAssembly.ViewModels;
using ELTE.Cinema.Shared.SignalR.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace ELTE.Cinema.Blazor.WebAssembly.Services.HubServices
{
    public class MovieHubService : BaseHubService, IMovieHubService
    {
        public event Action<MovieViewModel>? OnMovieReceived;

        private readonly IMapper _mapper;

        public MovieHubService(AppConfig appConfig, JsonSerializerOptions jsonOptions,
            IMapper mapper, ILocalStorageService localStorageService, IHttpRequestUtility httpRequestUtility)
            : base(appConfig, jsonOptions, localStorageService, httpRequestUtility)
        {
            _mapper = mapper;
        }

        public async Task StartMovieHubAsync()
        {
            InitHub("MoviesHub");

            _hubConnection!.On<MovieNotificationDto>("NewMovieAdded", movieNotificationDto =>
            {
                var movieViewModel = _mapper.Map<MovieViewModel>(movieNotificationDto);
                OnMovieReceived?.Invoke(movieViewModel);
            });

            await ConnectHubAsync();
        }
    }
}
