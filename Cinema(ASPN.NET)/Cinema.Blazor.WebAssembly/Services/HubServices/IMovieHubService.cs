using ELTE.Cinema.Blazor.WebAssembly.ViewModels;

namespace ELTE.Cinema.Blazor.WebAssembly.Services.HubServices
{
    public interface IMovieHubService: IBaseHubService
    {
        public event Action<MovieViewModel>? OnMovieReceived;
        Task StartMovieHubAsync();
    }
}
