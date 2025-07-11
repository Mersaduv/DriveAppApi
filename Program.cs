using DriveApp.Data;
using DriveApp.Extensions;
using DriveApp.Endpoints.Auth;
using Microsoft.EntityFrameworkCore;
using DriveApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Swagger
builder.Services.ConfigureSwagger();

// Configure CORS
builder.Services.ConfigureCors();

// Configure database
builder.Services.ConfigureDatabase(builder.Configuration);

// Configure authentication with JWT
builder.Services.ConfigureAuthentication(builder.Configuration);

// Add SignalR services
builder.Services.AddSignalR();

// Register application services
builder.Services.ConfigureServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}

app.UseHttpsRedirection();
app.UseCorsConfiguration();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Enable WebSockets
app.UseWebSocketConfiguration();

// Map all API endpoints
app.MapAllApiEndpoints();

// Map custom auth endpoints
app.MapCustomAuthEndpoints();

// Map SignalR hubs
app.MapHub<WebSocketHub>("/hubs/websocket");

// Add API status endpoint
app.MapApiStatusEndpoint();

// Initialize the database with migrations and seed data if needed
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        
        try
        {
            await DatabaseMigrator.MigrateAndSeedAsync(services, loggerFactory);
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }
}

app.Run();
