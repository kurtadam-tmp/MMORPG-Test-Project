using MMORPG.Domain.Interfaces;
using MMORPG.Infrastructure.Cache;
using MMORPG.Infrastructure.Data;
using MMORPG.Infrastructure.Repositories;
using MMORPG.Infrastructure.Services;
using MMORPG.Infrastructure.Services.Economy;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register Database & Cache Factories
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
    new DbConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=mmorpg_db;Username=postgres;Password=postgres"));

builder.Services.AddSingleton<IRedisConnectionFactory>(_ => 
    new RedisConnectionFactory(builder.Configuration.GetConnectionString("Redis") 
    ?? "localhost:6379"));

builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Register Repositories
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IStatRepository, StatRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

// Register Gateway Application Services
builder.Services.AddSingleton<IPlayerSessionService, PlayerSessionService>();
builder.Services.AddSingleton<IZoneStateService, ZoneStateService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddSingleton<IGameDataEditorService, GameDataEditorService>();
builder.Services.AddSingleton<IGatewayHandshakeService, GatewayHandshakeService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("=================================================================");
Console.WriteLine("     MMORPG Auth & Gateway REST API Service Initialized           ");
Console.WriteLine("=================================================================");

app.Run();
