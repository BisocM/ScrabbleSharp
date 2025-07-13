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


// ========== gRPC (HTTP/2 + gRPC=Web) ==========
builder.Services.AddGrpc(opts =>
{
    opts.EnableDetailedErrors = builder.Environment.IsDevelopment();
    opts.MaxReceiveMessageSize = 10_240;
    opts.MaxSendMessageSize = 10_240;
    opts.ResponseCompressionAlgorithm = "gzip";
    opts.ResponseCompressionLevel = CompressionLevel.SmallestSize;
    opts.CompressionProviders = [new GzipCompressionProvider(CompressionLevel.SmallestSize)];

    opts.Interceptors.Add<ValidationInterceptor>();
});

// ========== Kestrel ==========
builder.WebHost.ConfigureKestrel(k => k.AddServerHeader = false);

// ========== HSTS / HTTPS ==========
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

// ========== CORS (needed for gRPC=Web from the browser) ==========
builder.Services.AddCors(c =>
{
#if RELEASE
    var corsSettings = builder.Configuration
        .GetSection("Cors")
        .Get<Configuration.CorsSettings>();

    c.AddPolicy("StrictCors", policyBulder =>
    {
        policyBulder
            .WithOrigins(corsSettings.AllowedOrigins)
            .WithMethods("POST", "OPTIONS")
            .WithHeaders(
                HeaderNames.ContentType,
                "x-grpc-web",
                "x-user-agent",
                "grpc-timeout",
                "grpc-encoding",
                "grpc-accept-encoding",
                "connect-protocol-version",
                "connect-timeout-ms",
                // === Application-specific ===
                "x-mode", 
                "x-up", 
                "x-down", 
                "x-right", 
                "x-left",
                "x-expandable")
            .WithExposedHeaders(
                "grpc-status",
                "grpc-message",
                "grpc-encoding",
                "grpc-accept-encoding");
    });
#endif

#if DEBUG
    c.AddPolicy("DevCors", p => p
        .AllowAnyOrigin()
        .WithMethods("POST", "OPTIONS")
        .WithHeaders(
            HeaderNames.ContentType,
            "x-grpc-web",
            "x-user-agent",
            "grpc-timeout",
            "grpc-encoding",
            "grpc-accept-encoding",
            "connect-protocol-version",
            "connect-timeout-ms",
            // === Application-specific ===
            "x-mode",
            "x-up",
            "x-down",
            "x-right",
            "x-left",
            "x-expandable")
        .WithExposedHeaders(
            "grpc-status",
            "grpc-message",
            "grpc-encoding",
            "grpc-accept-encoding"));
#endif
});

// ========== Rate limiting ==========
builder.Services.AddRateLimiter(o =>
{
    o.AddPolicy("PerClient", ctx =>
    {
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
                QueueLimit = 0
            });
    });
});

// ========== Start Engine Registration ==========

//Game-mode providers
builder.Services.AddSingleton<IGameMode, LetterLeagueClassicMode>();
builder.Services.AddSingleton<IGameMode, ScrabbleClassicMode>();
builder.Services.AddSingleton<IGameMode, ScrabbleSuperMode>();
builder.Services.AddSingleton<IGameMode, ScrabbleDuelMode>();

// Engine Services
builder.Services.AddSingleton<GameModeRegistry>();
builder.Services.AddTransient<MoveGenerator>();

// Gateway Services
builder.Services.AddTransient<IGameService, GameService>();

// Register all the possible layouts
builder.Services.AddTransient<LetterLeagueLayout>();
builder.Services.AddTransient<ScrabbleSuperLayout>();
builder.Services.AddTransient<ScrabbleDuelLayout>();
builder.Services.AddTransient<ScrabbleLayout>();

// ========== End Engine Registration ==========

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error"); // TODO add ErrorController
    app.UseHsts();
    // app.UseHttpsRedirection(); Kestrel doesn't need to terminate TLS, depending on setup. Ensure that if you are hosting this, you have an adequate environment.
}

app.UseRouting();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors(app.Environment.IsDevelopment() ? "DevCors" : "StrictCors");
app.UseRateLimiter();

// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Permissions-Policy"] = "interest-cohort-()";
    await next();
});

// ========== gRPC endpoint ==========
app.MapGrpcService<ScrabbleService>()
    .RequireCors(app.Environment.IsDevelopment() ? "DevCors" : "StrictCors")
    .RequireRateLimiting("PerClient");

app.Run();