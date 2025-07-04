using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DriveApp.Models.WebSocket;

namespace DriveApp.Services;

public class WebSocketHub : Hub
{
    // Connection management
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
    
    // User can join groups (e.g., driver group, specific trip group)
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
    
    // User can leave groups
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}

public class WebSocketService : IWebSocketService
{
    private readonly IHubContext<WebSocketHub> _hubContext;
    private readonly ILogger<WebSocketService> _logger;
    private readonly ConcurrentDictionary<Guid, WebSocket> _userConnections = new();
    private readonly ConcurrentDictionary<Guid, WebSocket> _driverConnections = new();

    public WebSocketService(IHubContext<WebSocketHub> hubContext, ILogger<WebSocketService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task HandlePassengerConnectionAsync(WebSocket webSocket, Guid userId)
    {
        _userConnections.TryAdd(userId, webSocket);
        _logger.LogInformation($"Passenger connected: {userId}");
        
        // Send welcome message
        var welcomeMessage = new WebSocketMessage
        {
            Type = "connection_status",
            Data = new { connected = true, userId = userId, role = "passenger" },
            Timestamp = DateTime.UtcNow
        };
        
        await SendWebSocketMessageAsync(webSocket, welcomeMessage);
    }

    public async Task HandleDriverConnectionAsync(WebSocket webSocket, Guid driverId)
    {
        _driverConnections.TryAdd(driverId, webSocket);
        _logger.LogInformation($"Driver connected: {driverId}");
        
        // Send welcome message
        var welcomeMessage = new WebSocketMessage
        {
            Type = "connection_status",
            Data = new { connected = true, driverId = driverId, role = "driver" },
            Timestamp = DateTime.UtcNow
        };
        
        await SendWebSocketMessageAsync(webSocket, welcomeMessage);
    }

    public async Task ProcessMessageAsync(WebSocket webSocket, string message)
    {
        try
        {
            var messageObj = JsonSerializer.Deserialize<WebSocketMessage>(message);
            if (messageObj == null)
            {
                _logger.LogWarning("Received invalid message format");
                return;
            }
            
            _logger.LogInformation($"Received message of type: {messageObj.Type}");
            
            // Process different message types
            switch (messageObj.Type)
            {
                case "driver_location":
                    if (messageObj.Data is JsonElement jsonElement)
                    {
                        var locationUpdate = jsonElement.Deserialize<LocationUpdate>();
                        if (locationUpdate != null)
                        {
                            await SendDriverLocationUpdateAsync(locationUpdate);
                        }
                    }
                    break;
                
                case "trip_update":
                    if (messageObj.Data is JsonElement jsonElement2)
                    {
                        var tripUpdate = jsonElement2.Deserialize<TripUpdate>();
                        if (tripUpdate != null)
                        {
                            await SendTripUpdateAsync(tripUpdate);
                        }
                    }
                    break;
                
                case "driver_status":
                    if (messageObj.Data is JsonElement jsonElement3)
                    {
                        var statusUpdate = jsonElement3.Deserialize<DriverStatusUpdate>();
                        if (statusUpdate != null)
                        {
                            await SendDriverStatusUpdateAsync(statusUpdate);
                        }
                    }
                    break;
                
                default:
                    _logger.LogWarning($"Unknown message type: {messageObj.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket message");
        }
    }

    public async Task HandleDisconnectionAsync(WebSocket webSocket)
    {
        // Find and remove the connection
        var userEntry = _userConnections.FirstOrDefault(x => x.Value == webSocket);
        if (userEntry.Key != Guid.Empty)
        {
            _userConnections.TryRemove(userEntry.Key, out _);
            _logger.LogInformation($"Passenger disconnected: {userEntry.Key}");
            return;
        }
        
        var driverEntry = _driverConnections.FirstOrDefault(x => x.Value == webSocket);
        if (driverEntry.Key != Guid.Empty)
        {
            _driverConnections.TryRemove(driverEntry.Key, out _);
            _logger.LogInformation($"Driver disconnected: {driverEntry.Key}");
        }
    }

    private async Task SendWebSocketMessageAsync(WebSocket webSocket, WebSocketMessage message)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            return;
        }
        
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async Task SendDriverLocationUpdateAsync(LocationUpdate locationUpdate)
    {
        var message = new WebSocketMessage
        {
            Type = "driver_location_update",
            Data = locationUpdate,
            RequestId = Guid.NewGuid().ToString()
        };

        await SendMessageToGroupAsync($"trip_{locationUpdate.UserId}", "driver_location_update", message);
    }

    public async Task SendTripUpdateAsync(TripUpdate tripUpdate)
    {
        var message = new WebSocketMessage
        {
            Type = "trip_update",
            Data = tripUpdate,
            RequestId = Guid.NewGuid().ToString()
        };

        await SendMessageToGroupAsync($"trip_{tripUpdate.TripId}", "trip_update", message);
    }

    public async Task SendDriverStatusUpdateAsync(DriverStatusUpdate statusUpdate)
    {
        var message = new WebSocketMessage
        {
            Type = "driver_status_update",
            Data = statusUpdate,
            RequestId = Guid.NewGuid().ToString()
        };

        await SendMessageToGroupAsync("admins", "driver_status_update", message);
    }

    public async Task SendMessageToUserAsync(string userId, string messageType, object data)
    {
        var message = new WebSocketMessage
        {
            Type = messageType,
            Data = data,
            RequestId = Guid.NewGuid().ToString()
        };

        await _hubContext.Clients.User(userId).SendAsync("ReceiveMessage", JsonSerializer.Serialize(message));
    }

    public async Task SendMessageToAllAsync(string messageType, object data)
    {
        var message = new WebSocketMessage
        {
            Type = messageType,
            Data = data,
            RequestId = Guid.NewGuid().ToString()
        };

        await _hubContext.Clients.All.SendAsync("ReceiveMessage", JsonSerializer.Serialize(message));
    }

    public async Task SendMessageToGroupAsync(string groupName, string messageType, object data)
    {
        var message = new WebSocketMessage
        {
            Type = messageType,
            Data = data,
            RequestId = Guid.NewGuid().ToString()
        };

        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", JsonSerializer.Serialize(message));
    }

    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
        _logger.LogInformation($"Added connection {connectionId} to group {groupName}");
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
        _logger.LogInformation($"Removed connection {connectionId} from group {groupName}");
    }
} 