using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
// using Microsoft.AspNetCore.Authentication.Cookies;
using Scalar.AspNetCore;
using Website.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

// Configure logging
builder.Logging.AddConsole();

// Add authentication services
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.LoginPath = "/login";
//         options.AccessDeniedPath = "/login";
//         options.Cookie.Name = "Website.Auth";
//         options.Cookie.HttpOnly = true;
//         options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//         options.Cookie.SameSite = SameSiteMode.Strict;
//         options.ExpireTimeSpan = TimeSpan.FromHours(24);
//         options.SlidingExpiration = true;
//     });
// builder.Services.AddAuthorization();

// Rate limiting - fixed window policy used by the notes API group
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 10;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Response compression (Brotli + Gzip, including HTTPS)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddControllers();
builder.Services.AddRazorComponents();
builder.Services.AddOpenApi();

var app = builder.Build();

// -----------------------------------------------------------------------
// Middleware pipeline – order matters
// -----------------------------------------------------------------------

// 1. Exception handling (outermost – catches everything below)
if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts(); // default 30 days; adjust in production as needed
}

// 2. Transport security & compression (before any response-generating middleware)
app.UseHttpsRedirection();

if (app.Environment.IsProduction())
{
    app.UseResponseCompression();
}

// 3. Static assets (served directly; compression handled by pre-compressed variants)
app.MapStaticAssets();

// 4. Status code pages (re-executes after downstream middleware sets a non-success code)
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// 5. Rate limiting (applied per-endpoint via .RequireRateLimiting)
app.UseRateLimiter();

// 6. Authentication → Authorization → Antiforgery
// app.UseAuthentication();
// app.UseAuthorization();
app.UseAntiforgery();

// -----------------------------------------------------------------------
// Endpoint mapping
// -----------------------------------------------------------------------

// var notesGroup = app.MapGroup("/auth")
//     .MapAuthEndpoints()
//     .WithTags("Authentication");
//
// var authGroup = app.MapGroup("/notes")
//     .MapNotesEndpoints()
//     .RequireAuthorization()
//     .WithTags("Notes")
//     .RequireRateLimiting("fixed");
//
// var chatGroup = app.MapGroup("/chat")
//     .MapChatEndpoints()
//     .RequireAuthorization()
//     .WithTags("Chat")
//     .RequireRateLimiting("fixed");

// app.MapEndpoints(notesGroup);
// app.MapEndpoints(authGroup);
// app.MapEndpoints(chatGroup);

app.MapRazorComponents<App>();

app.Run();