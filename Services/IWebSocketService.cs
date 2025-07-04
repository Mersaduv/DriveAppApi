using DriveApp.Models.WebSocket;
using System.Net.WebSockets;

namespace DriveApp.Services;

public interface IWebSocketService
{
    Task SendDriverLocationUpdateAsync(LocationUpdate locationUpdate);
    Task SendTripUpdateAsync(TripUpdate tripUpdate);
    Task SendDriverStatusUpdateAsync(DriverStatusUpdate statusUpdate);
    Task SendMessageToUserAsync(string userId, string messageType, object data);
    Task SendMessageToAllAsync(string messageType, object data);
    Task SendMessageToGroupAsync(string groupName, string messageType, object data);
    Task AddToGroupAsync(string connectionId, string groupName);
    Task RemoveFromGroupAsync(string connectionId, string groupName);
    
    // WebSocket handling methods
    Task HandlePassengerConnectionAsync(WebSocket webSocket, Guid userId);
    Task HandleDriverConnectionAsync(WebSocket webSocket, Guid driverId);
    Task ProcessMessageAsync(WebSocket webSocket, string message);
    Task HandleDisconnectionAsync(WebSocket webSocket);
} 