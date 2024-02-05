using Blazored.Toast;
using Blazorise;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;
using Server.Authentication;
using Server.Components;
using Server.Middleware;
using Server.Services;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

var builder = WebApplication.CreateBuilder(args);

// Add localization services
builder.Services.AddLocalization();

builder.Services.AddControllers();

// Set up configuration
builder.Configuration.AddJsonFile("appsettings.json");

//Setup Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddTransient<TokenExpiryHandler>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<StateContainer>();
builder.Services.AddSingleton<CacheService>();
builder.Services.AddScoped<AuthenticationProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Add blazorise services   
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();
//Add empty providers
builder.Services.AddEmptyProviders();
// End Blazorise services

var app = builder.Build();

// Configure supported cultures and localization options
string[] supportedCultures = ["en-US", "ar-SA"];
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.UseMiddleware<RateLimitingMiddleware>();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
