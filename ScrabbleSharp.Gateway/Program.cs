using System.IO.Compression;
using System.Threading.RateLimiting;
using Grpc.Net.Compression;
using Microsoft.Net.Http.Headers;
using ScrabbleSharp.Engine.Core.Boards.Types;
using ScrabbleSharp.Engine.Core.Rules.Interfaces;
using ScrabbleSharp.Engine.GameModes;
using ScrabbleSharp.Engine.GameModes.Types;
using ScrabbleSharp.Engine.Services.MoveGeneration;
using ScrabbleSharp.Gateway.Configuration;
using ScrabbleSharp.Gateway.Interceptors;
using ScrabbleSharp.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure gRPC services
builder.Services.AddGrpc(opts =>
{
    opts.EnableDetailedErrors = builder.Environment.IsDevelopment();
    opts.MaxReceiveMessageSize = 10_240; // 10 KB
    opts.MaxSendMessageSize = 10_240; // 10 KB
    opts.ResponseCompressionAlgorithm = "gzip";
    opts.ResponseCompressionLevel = CompressionLevel.SmallestSize;
    opts.CompressionProviders = [new GzipCompressionProvider(CompressionLevel.SmallestSize)];

    // Add custom interceptor for request validation.
    opts.Interceptors.Add<ValidationInterceptor>();
});

// Remove server header for security.
builder.WebHost.ConfigureKestrel(k => k.AddServerHeader = false);

// Configure security headers (HSTS)
builder.Services.AddHsts(h =>
{
    h.Preload = true;
    h.IncludeSubDomains = true;
    h.MaxAge = TimeSpan.FromDays(365);
});
builder.Services.AddHttpsRedirection(r =>
{
    r.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    r.HttpsPort = 443;
});

// Configure CORS policies
builder.Services.AddCors(c =>
{
    var corsSettings = builder.Configuration
        .GetSection("Cors")
        .Get<Configuration.CorsSettings>() ?? new Configuration.CorsSettings();

    // Strict policy for production environments
    c.AddPolicy("StrictCors", policyBulder =>
    {
        policyBulder
            .WithOrigins(corsSettings.AllowedOrigins)
            .WithMethods("POST", "OPTIONS")
            .WithHeaders(
                HeaderNames.ContentType,
                "x-grpc-web", "x-user-agent", "grpc-timeout", "grpc-encoding",
                "grpc-accept-encoding", "connect-protocol-version", "connect-timeout-ms",
                // Custom headers for game state
                "x-mode", "x-up", "x-down", "x-right", "x-left", "x-expandable")
            .WithExposedHeaders(
                "grpc-status", "grpc-message", "grpc-encoding", "grpc-accept-encoding");
    });

    // Lenient policy for local development
    c.AddPolicy("DevCors", p => p
        .AllowAnyOrigin()
        .WithMethods("POST", "OPTIONS")
        .WithHeaders(
            HeaderNames.ContentType,
            "x-grpc-web", "x-user-agent", "grpc-timeout", "grpc-encoding",
            "grpc-accept-encoding", "connect-protocol-version", "connect-timeout-ms",
            // Custom headers for game state
            "x-mode", "x-up", "x-down", "x-right", "x-left", "x-expandable")
        .WithExposedHeaders(
            "grpc-status", "grpc-message", "grpc-encoding", "grpc-accept-encoding"));
});

// Configure rate limiting policy
builder.Services.AddRateLimiter(o =>
{
    o.AddPolicy("PerClient", ctx =>
    {
        // Identify client by IP, preferring proxy headers if available.
        var ip =
            ctx.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
            ?? ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? ctx.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // Reject requests immediately when limit is reached
            });
    });
});

// Register engine services for dependency injection
builder.Services.AddSingleton<IGameMode, LetterLeagueClassicMode>();
builder.Services.AddSingleton<IGameMode, ScrabbleClassicMode>();
builder.Services.AddSingleton<IGameMode, ScrabbleSuperMode>();
builder.Services.AddSingleton<IGameMode, ScrabbleDuelMode>();

builder.Services.AddSingleton<GameModeRegistry>();
builder.Services.AddTransient<MoveGenerator>();

// Register gateway services
builder.Services.AddTransient<IGameService, GameService>();

// Register board layouts
builder.Services.AddTransient<LetterLeagueLayout>();
builder.Services.AddTransient<ScrabbleSuperLayout>();
builder.Services.AddTransient<ScrabbleDuelLayout>();
builder.Services.AddTransient<ScrabbleLayout>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors(app.Environment.IsDevelopment() ? "DevCors" : "StrictCors");
app.UseRateLimiter();

// Add additional security headers middleware
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Permissions-Policy"] = "interest-cohort=()";
    await next();
});

// Map gRPC service endpoints
app.MapGrpcService<ScrabbleService>()
    .RequireCors(app.Environment.IsDevelopment() ? "DevCors" : "StrictCors")
    .RequireRateLimiting("PerClient");

app.Run();