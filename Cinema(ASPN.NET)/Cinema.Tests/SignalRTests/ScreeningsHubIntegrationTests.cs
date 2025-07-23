using System.Net;
using System.Net.Http.Json;
using ELTE.Cinema.Cache;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.Shared.SignalR.HubInterfaces;
using ELTE.Cinema.Shared.SignalR.Models;
using ELTE.Cinema.SignalR.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ELTE.Cinema.Tests.SignalRTests;

public class ScreeningsHubIntegrationTests : TestBase
{
    public ScreeningsHubIntegrationTests(): base(("TestScreeningDatabase"))
    {
        HubName = "ScreeningsHub";
    }
    
    [Fact]
    public async Task AuthorizedUser_CanConnectToHub()
    {
        //Arrange
        var token = await Login(AdminUser);
        var hubConnection = CreateHubConnection(token);
        
        //Act
        await hubConnection.StartAsync();

        //Verify
        Assert.Equal(HubConnectionState.Connected, hubConnection.State);
    }
    
    [Fact]
    public async Task NotAuthorizedUser_CannotConnectToHub()
    {
        //Arrange
        var hubConnection = CreateHubConnection();
        
        //Act & Verify
        await Assert.ThrowsAsync<HttpRequestException>(() => hubConnection.StartAsync());
    }
    
     [Fact]
    public async Task AuthorizedUser_CanJoinGroup()
    {
        //Arrange
        const int screeningId = 1;
        
        var token = await Login(User);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();
        var service = Factory.Services.GetRequiredService<IScreeningsNotificationService>();
        
        var seatNotification = new SeatNotificationDto
        {
            Row = 1,
            Column = 2,
            Status = SeatClientStatusDto.Selected
        };

        
        //Act
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        await service.NotifySeatStatusChangedAsync(screeningId, seatNotification);
        
        await Task.Delay(500); // Allow time for propagation
        
        //Verify
        Assert.Single(receivedNotifications);
        Assert.Equal(seatNotification.Row, receivedNotifications[0].Row);
        Assert.Equal(seatNotification.Column, receivedNotifications[0].Column);
        Assert.Equal(seatNotification.Status, receivedNotifications[0].Status);
    }
    
    [Fact]
    public async Task AuthorizedUser_CanLeaveGroup()
    {
        //Arrange
        const int screeningId = 1;
        
        var token = await Login(User);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();
        var service = Factory.Services.GetRequiredService<IScreeningsNotificationService>();
        
        var seatNotification = new SeatNotificationDto
        {
            Row = 1,
            Column = 2,
            Status = SeatClientStatusDto.Selected
        };

        
        //Act
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.LeaveScreeningGroup), screeningId);
        
        await service.NotifySeatStatusChangedAsync(screeningId, seatNotification);
        
        await Task.Delay(500); // Allow time for propagation
        
        //Verify
        Assert.Empty(receivedNotifications);
    }
    
    [Fact]
    public async Task SeatStatusChanged_SendsMessageToGroup()
    {
        // Arrange
        const int screeningId = 1;
        var seatNotification = new SeatNotificationDto
        {
            Row = 1,
            Column = 2,
            Status = SeatClientStatusDto.Selected
        };

        var receivedNotifications = new List<SeatNotificationDto>();
        
        var token = await Login(User);
        var hubConnection1 = CreateHubConnection(token);
        await hubConnection1.StartAsync();
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        var hubConnection2 = CreateHubConnection(token);;
        await hubConnection2.StartAsync();
        await hubConnection2.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        // Listen for SeatStatusChanged messages
        hubConnection2.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });

        
        //Act
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), screeningId, seatNotification);
        
        // Allow time for message propagation
        await Task.Delay(500);

        // Assert
        Assert.Single(receivedNotifications);
        Assert.Equal(seatNotification.Row, receivedNotifications[0].Row);
        Assert.Equal(seatNotification.Column, receivedNotifications[0].Column);
    }
    
    [Fact]
    public async Task SeatStatusChanged_SendsMessageToGroup_AdminReceives()
    {
        // Arrange
        const int screeningId = 1;
        var seatNotification = new SeatNotificationDto
        {
            Row = 1,
            Column = 2,
            Status = SeatClientStatusDto.Selected
        };

        var receivedNotifications = new List<SeatNotificationDto>();
        
        var userToken = await Login(User);
        var hubConnection1 = CreateHubConnection(userToken);
        await hubConnection1.StartAsync();
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        var adminToken = await Login(AdminUser);
        var hubConnection2 = CreateHubConnection(adminToken);
        await hubConnection2.StartAsync();
        await hubConnection2.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        // Listen for SeatStatusChanged messages
        hubConnection2.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });

        //Act
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), screeningId, seatNotification);
        
        // Allow time for message propagation
        await Task.Delay(500);

        // Assert
        Assert.Single(receivedNotifications);
        Assert.Equal(seatNotification.Row, receivedNotifications[0].Row);
        Assert.Equal(seatNotification.Column, receivedNotifications[0].Column);
    }
    
    [Fact]
    public async Task SeatStatusChanged_SendsMessageToGroup_WhenNotJoined()
    {
        // Arrange
        const int screeningId = 1;
        var seatNotification = new SeatNotificationDto
        {
            Row = 1,
            Column = 2,
            Status = SeatClientStatusDto.Selected
        };

        var receivedNotifications = new List<SeatNotificationDto>();
        
        var token = await Login(User);
        var hubConnection1 = CreateHubConnection(token);
        await hubConnection1.StartAsync();
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        var hubConnection2 = CreateHubConnection(token);
        await hubConnection2.StartAsync();
        
        // Listen for SeatStatusChanged messages
        hubConnection2.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        //Act
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), screeningId, seatNotification);
        
        // Allow time for message propagation
        await Task.Delay(500);

        // Assert
        Assert.Empty(receivedNotifications);
    }
    
    [Fact]
    public async Task SeatStatusChanged_SendMessage_WhenNewReservation()
    {
        //Arrange
        const int screeningId = 1;

        var token = await Login(AdminUser);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();

        var reservationDto = new ReservationRequestDto
        {
            ScreeningId = screeningId,
            Name = User.Name,
            Email = User.Email,
            Phone = User.PhoneNumber,
            Seats =
            [
                new SeatRequestDto
                {
                    Row = 1,
                    Column = 3,
                },

                new SeatRequestDto
                {
                    Row = 1,
                    Column = 2,
                }
            ]
        };
        
        //Join to group
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        // Listen for SeatStatusChanged messages
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });

        await Login(User);
        var result = await Client.PostAsJsonAsync("reservations", reservationDto);
        
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        
        await Task.Delay(500); // Allow time for propagation

        //Verify
        Assert.Equal(2, receivedNotifications.Count);
        
        Assert.Equal(SeatClientStatusDto.Reserved, receivedNotifications.First().Status);
        Assert.Equal(SeatClientStatusDto.Reserved, receivedNotifications.Last().Status);
        
        Assert.Equal(SeatClientStatusDto.Reserved, receivedNotifications.First().Status);
        Assert.Equal(SeatClientStatusDto.Reserved, receivedNotifications.Last().Status);
        
        Assert.NotNull(receivedNotifications.First().Reservation);
        Assert.NotNull(receivedNotifications.Last().Reservation);
    }
    
     [Fact]
    public async Task SeatStatusChanged_SendMessage_WhenCancelReservation()
    {
        //Arrange
        const int screeningId = 1;
        
        var token = await Login(AdminUser);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();
        
        var reservationDto = new ReservationRequestDto
        {
            ScreeningId = screeningId,
            Name = User.Name,
            Email = User.Email,
            Phone = User.PhoneNumber,
            Seats =
            [
                new SeatRequestDto
                {
                    Row = 1,
                    Column = 3,
                },

                new SeatRequestDto
                {
                    Row = 1,
                    Column = 2,
                }
            ]
        };

        await Login(User);
        var result = await Client.PostAsJsonAsync("reservations", reservationDto);
        
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        
        //Join to group
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        // Listen for SeatStatusChanged messages
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });

        result = await Client.DeleteAsync("reservations/1");
        
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        
        await Task.Delay(500); // Allow time for propagation

        //Verify
        Assert.Equal(2, receivedNotifications.Count);
        
        Assert.Equal(SeatClientStatusDto.None, receivedNotifications.First().Status);
        Assert.Equal(SeatClientStatusDto.None, receivedNotifications.Last().Status);
        
        Assert.Equal(SeatClientStatusDto.None, receivedNotifications.First().Status);
        Assert.Equal(SeatClientStatusDto.None, receivedNotifications.Last().Status);

    }
    
    [Fact]
    public async Task SeatStatusChanged_ReceivesReservation_WhenAdmin()
    {
        //Arrange
        const int screeningId = 1;
        
        var token = await Login(AdminUser);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();
        var service = Factory.Services.GetRequiredService<IScreeningsNotificationService>();
        
        var seatNotification = new SeatNotificationDto
        {
            Row = 1,
            Column = 2,
            Status = SeatClientStatusDto.Selected,
            Reservation = new ReservationNotificationDto
            {
                Id = 1,
                Name = "Test user",
                CreatedAt = DateTime.Now,
                Email = "test@test.com",
                Phone = "06201111111"
            }
        };

        
        //Act
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        await service.NotifySeatStatusChangedAsync(screeningId, seatNotification);
        
        await Task.Delay(500); // Allow time for propagation
        
        //Verify
        Assert.Single(receivedNotifications);
        Assert.Equal(seatNotification.Row, receivedNotifications[0].Row);
        Assert.Equal(seatNotification.Column, receivedNotifications[0].Column);
        Assert.Equal(seatNotification.Status, receivedNotifications[0].Status);
        Assert.NotNull(receivedNotifications[0].Reservation);
    }
    
    [Fact]
    public async Task SeatStatusChanged_NotReceivesReservation_WhenNotAdmin()
    {
        //Arrange
        const int screeningId = 1;
        
        var token = await Login(User);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();
        var service = Factory.Services.GetRequiredService<IScreeningsNotificationService>();
        
        var seatNotification = new SeatNotificationDto
        {
            Row = 1,
            Column = 2,
            Status = SeatClientStatusDto.Selected,
            Reservation = new ReservationNotificationDto
            {
                Id = 1,
                Name = "Test user",
                CreatedAt = DateTime.Now,
                Email = "test@test.com",
                Phone = "06201111111"
            }
        };

        
        //Act
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        await service.NotifySeatStatusChangedAsync(screeningId, seatNotification);
        
        await Task.Delay(500); // Allow time for propagation
        
        //Verify
        Assert.Single(receivedNotifications);
        Assert.Equal(seatNotification.Row, receivedNotifications[0].Row);
        Assert.Equal(seatNotification.Column, receivedNotifications[0].Column);
        Assert.Equal(seatNotification.Status, receivedNotifications[0].Status);
        Assert.Null(receivedNotifications[0].Reservation);

    }
    
     [Fact]
    public async Task SeatStatusChanged_SendMessage_WhenAdminSellSeats()
    {
        //Arrange
        const int screeningId = 1;
        
        var token = await Login(AdminUser);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();
        
        var seatDto = new SeatRequestDto
        {
            Row = 1,
            Column = 3,
        };
        
        //Join to group
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        // Listen for SeatStatusChanged messages
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        await Login(AdminUser);
        var result = await Client.PutAsJsonAsync($"screenings/{screeningId}/seats/sell", seatDto);
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        
        await Task.Delay(500); // Allow time for propagation

        //Verify
        Assert.Single(receivedNotifications);
        Assert.Equal(SeatClientStatusDto.Sold, receivedNotifications.First().Status);
    }   
    
     [Fact]
    public async Task JoinGroup_ReceiveAllCachedSeatsForScreening()
    {
        //Arrange
        const int screeningId = 1;
        
        var token = await Login(User);
        var hubConnection = CreateHubConnection(token);
        await hubConnection.StartAsync();
        var cache = Factory.Services.GetRequiredService<ICacheManager>();

        var seats = new List<SeatNotificationDto>
        {
            new()
            {
                Row = 1,
                Column = 2,
                Status = SeatClientStatusDto.Selected
            },
            new()
            {
                Row = 3,
                Column = 2,
                Status = SeatClientStatusDto.Selected
            }
        };

        await cache.SetAsync($"{hubConnection.ConnectionId}_{screeningId}", seats);
        await cache.SetAsync($"{hubConnection.ConnectionId}_2", seats);
        
        // Listen for SeatStatusChanged messages
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        //Act
        await hubConnection.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        await Task.Delay(500); // Allow time for propagation
        
        //Verify
        Assert.Equal(seats.Count, receivedNotifications.Count);
    }
    
    [Fact]
    public async Task LeaveGroup_ReceiveCachedSeatsDeletionForScreening_WhenLeavesScreening()
    {
        //Arrange
        const int screeningId = 1;
        var token = await Login(User);
        var hubConnection1 = CreateHubConnection(token);
        await hubConnection1.StartAsync();
        
        var hubConnection2 = CreateHubConnection(token);
        await hubConnection2.StartAsync();

        
        var seats = new List<SeatNotificationDto>
        {
            new()
            {
                Row = 1,
                Column = 2,
                Status = SeatClientStatusDto.Selected
            },
            new()
            {
                Row = 3,
                Column = 2,
                Status = SeatClientStatusDto.Selected
            }
        };
        
        //Act
        await hubConnection2.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), screeningId);
        
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), screeningId, seats.First());
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), screeningId, seats.Last());
        
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), 2, seats.First());
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), 2, seats.Last());
        
        // Listen for SeatStatusChanged messages
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection2.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            Assert.Equal(screeningId, receivedScreening);
            receivedNotifications.Add(notification);
        });
        
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.LeaveScreeningGroup), screeningId);
        
        await Task.Delay(500); // Allow time for propagation
        
        //Verify
        Assert.Equal(seats.Count, receivedNotifications.Count);
        Assert.Equal(SeatClientStatusDto.None, receivedNotifications.First().Status);
    }
    
    [Fact]
    public async Task LeaveGroup_ReceiveCachedSeatsDeletionForScreening_WhenDisconnect()
    {
        //Arrange
        var token = await Login(User);
        var hubConnection1 = CreateHubConnection(token);
        await hubConnection1.StartAsync();
        
        var hubConnection2 = CreateHubConnection(token);
        await hubConnection2.StartAsync();
        
        var seats = new List<SeatNotificationDto>
        {
            new()
            {
                Row = 1,
                Column = 2,
                Status = SeatClientStatusDto.Selected
            },
            new()
            {
                Row = 3,
                Column = 2,
                Status = SeatClientStatusDto.Selected
            }
        };
        
        //Act
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), 1, seats.First());
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), 1, seats.Last());
        
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), 2, seats.First());
        await hubConnection1.InvokeAsync(nameof(IScreeningsHub.SeatStatusChanged), 2, seats.Last());
        
        await hubConnection2.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), 1);
        await hubConnection2.InvokeAsync(nameof(IScreeningsHub.JoinScreeningGroup), 2);
        
        // Listen for SeatStatusChanged messages
        var receivedNotifications = new List<SeatNotificationDto>();
        hubConnection2.On<int, SeatNotificationDto>(nameof(IScreeningsHub.SeatStatusChanged), (receivedScreening, notification) =>
        {
            receivedNotifications.Add(notification);
        });
        
        await hubConnection1.DisposeAsync();
        
        await Task.Delay(1000); // Allow time for propagation
        
        //Verify
        Assert.Equal(seats.Count * 2, receivedNotifications.Count);
        Assert.Equal(SeatClientStatusDto.None, receivedNotifications.First().Status);
    }
    
}