using DriveApp.Data;
using DriveApp.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Swagger
builder.Services.ConfigureSwagger();

// Configure CORS
builder.Services.ConfigureCors();

// Configure database
builder.Services.ConfigureDatabase(builder.Configuration);

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

// Enable WebSockets
app.UseWebSocketConfiguration();

// Map all API endpoints
app.MapAllApiEndpoints();

// Add API status endpoint
app.MapApiStatusEndpoint();

// Initialize the database with seed data if needed
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        
        try
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
            DbInitializer.Initialize(services, loggerFactory);
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

app.Run();
