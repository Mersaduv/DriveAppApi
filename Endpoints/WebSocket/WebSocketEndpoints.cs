using DriveApp.Services;
using global::System.Net.WebSockets;
using global::System.Text;
using global::System.Text.Json;

namespace DriveApp.Endpoints.WebSocket;

public static class WebSocketEndpoints
{
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(30);

    public static void MapWebSocketEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ws").WithTags("WebSocket");

        // WebSocket connection endpoint for passengers
        group.MapGet("/passenger/{userId}", async (
            HttpContext context,
            IWebSocketService webSocketService,
            Guid userId) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await webSocketService.HandlePassengerConnectionAsync(webSocket, userId);

            await HandleWebSocketCommunication(webSocket, webSocketService);
        });

        // WebSocket connection endpoint for drivers
        group.MapGet("/driver/{driverId}", async (
            HttpContext context,
            IWebSocketService webSocketService,
            Guid driverId) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await webSocketService.HandleDriverConnectionAsync(webSocket, driverId);

            await HandleWebSocketCommunication(webSocket, webSocketService);
        });
    }

    private static async Task HandleWebSocketCommunication(global::System.Net.WebSockets.WebSocket webSocket, IWebSocketService webSocketService)
    {
        var buffer = new byte[4096];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            try
            {
                var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                
                // Process the received message
                await webSocketService.ProcessMessageAsync(webSocket, message);

                // Prepare to receive the next message
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"WebSocket error: {ex.Message}");
                break;
            }
        }

        // Client requested close or exception occurred
        if (receiveResult.CloseStatus.HasValue)
        {
            try
            {
                await webSocketService.HandleDisconnectionAsync(webSocket);
                await webSocket.CloseAsync(
                    receiveResult.CloseStatus.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);
            }
            catch (Exception)
            {
                // Log but suppress exception during closure
            }
        }
    }
} 