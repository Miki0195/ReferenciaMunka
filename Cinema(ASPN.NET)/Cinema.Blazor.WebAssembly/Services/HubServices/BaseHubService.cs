using Blazored.LocalStorage;
using ELTE.Cinema.Blazor.WebAssembly.Config;
using ELTE.Cinema.Blazor.WebAssembly.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace ELTE.Cinema.Blazor.WebAssembly.Services.HubServices
{
    public abstract class BaseHubService : IBaseHubService
    {
        protected HubConnection? _hubConnection;
        private readonly AppConfig _appConfig;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILocalStorageService _localStorageService;
        private readonly IHttpRequestUtility _httpRequestUtility;

        protected BaseHubService(AppConfig appConfig, JsonSerializerOptions jsonOptions,
            ILocalStorageService localStorageService, IHttpRequestUtility httpRequestUtility)
        {
            _appConfig = appConfig;
            _jsonOptions = jsonOptions;
            _localStorageService = localStorageService;
            _httpRequestUtility = httpRequestUtility;
        }

        protected void InitHub(string hubName)
        {
            var fullUri = new Uri(new Uri(_appConfig.HubBaseUrl), hubName);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(fullUri, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        var token = await _localStorageService.GetItemAsStringAsync("AuthToken");

                        if (token == null || _httpRequestUtility.IsAccessTokenExpired(token))
                        {
                            token = (await _httpRequestUtility.RedeemTokenAsync()).AuthToken;
                        }

                        return token;
                    };
                })
                .AddJsonProtocol(config =>
                {
                    config.PayloadSerializerOptions = _jsonOptions;
                })
                .WithAutomaticReconnect()
                .Build();
        }

        protected async Task ConnectHubAsync()
        {
            if (_hubConnection!.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
        }

        public async Task DisconnectHubAsync()
        {
            if (_hubConnection!.State != HubConnectionState.Disconnected)
            {
                await _hubConnection.StopAsync();
            }
        }
    }
}
