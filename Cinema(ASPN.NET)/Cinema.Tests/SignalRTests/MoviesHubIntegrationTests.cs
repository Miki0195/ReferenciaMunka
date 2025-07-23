using System.Net;
using System.Net.Http.Json;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.Shared.SignalR.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace ELTE.Cinema.Tests.SignalRTests;

public class MoviesHubIntegrationTests: TestBase
{
    public MoviesHubIntegrationTests(): base("TestMoviesDatabase")
    {
        HubName = "MoviesHub";
    }
    
    [Fact]
    public async Task NewMovieAdded_SendMessage()
    {
        //Arrange
        var hubConnectionUser = CreateHubConnection();;
        await hubConnectionUser.StartAsync();

        var movieDto = new MovieRequestDto()
        {
           Title = "Teszt",
           Director = "Teszt",
           Synopsis = "Teszt",
           Length = 1000,
           Year = 2010
        };
        
        // Listen for NewMovieAdded messages
        MovieNotificationDto? receivedNotification = null;
        hubConnectionUser.On<MovieNotificationDto>("NewMovieAdded", notification =>
        {
            receivedNotification = notification;
        });
        
        await Login(AdminUser);
        var result = await Client.PostAsJsonAsync("movies", movieDto);
        
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        
        await Task.Delay(500); // Allow time for propagation

        //Verify
        Assert.NotNull(receivedNotification);
        Assert.Equal("Teszt", receivedNotification.Title);
        Assert.Equal("Teszt", receivedNotification.Director);
        Assert.Equal("Teszt", receivedNotification.Synopsis);
        Assert.Equal(1000, receivedNotification.Length);
        Assert.Equal(2010, receivedNotification.Year);
    }
}