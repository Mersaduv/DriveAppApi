namespace DriveApp.Models.WebSocket;

public class WebSocketMessage
{
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
} 